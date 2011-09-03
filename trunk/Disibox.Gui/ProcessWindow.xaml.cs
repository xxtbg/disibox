//
// Copyright (c) 2011, University of Genoa
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the University of Genoa nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL UNIVERSITY OF GENOA BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

using System;
using System.IO;
using System.Windows;
using Disibox.Data.Client;
using Disibox.Data.Client.Exceptions;
using Disibox.Data.Exceptions;
using Disibox.Gui.Util;
using Microsoft.Win32;

namespace Disibox.Gui
{
    /// <summary>
    /// Interaction logic for ProcessWindow.xaml
    /// </summary>
    public partial class ProcessWindow : Window
    {
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private readonly ClientDataSource _ds;
        private bool _erroFillingList = false;

        public ProcessWindow(StreamReader reader, StreamWriter writer, ClientDataSource ds) {
            InitializeComponent();
            _reader = reader;
            _writer = writer;
            _ds = ds;

            FillListView();

            if (!_erroFillingList)
                ShowDialog();
        }

        private void FillListView() {
            string temp = null;
            var numberOfProcessingTools = 0;

            const string titleMessageBox = "Processing tools";
            const string messageMessageBox = "Error while retriving the list of processing lists: ";

            try {
                temp = _reader.ReadLine();
            } catch (Exception) {
                MessageBox.Show(messageMessageBox+"the server is not responding! (retriving the number of tools)", titleMessageBox);
                _erroFillingList = true;
                return;
            }

            if (temp == null) {
                MessageBox.Show(messageMessageBox + "number of tools is not valid format or null", titleMessageBox);
                _erroFillingList = true;
                return;
            }

            try {
                numberOfProcessingTools = Int32.Parse(temp);
            } catch (Exception) {
                MessageBox.Show(messageMessageBox + "number of tools is not valid format or null", titleMessageBox);
                _erroFillingList = true;
                return;
            }

            for (var i = 0; i < numberOfProcessingTools; ++i) {
                string[] info;

                try {
                    info = _reader.ReadLine().Split(',');
                    if (info.Length != 3)
                        throw new Exception();
                } catch (Exception) {
                    MessageBox.Show(messageMessageBox + "cannot retrive tool information", titleMessageBox);
                    _erroFillingList = true;
                    return;
                }

                listView.Items.Add(new ProcessingToolInformation(info[0].Trim(), info[1].Trim(), info[2].Trim()));
            }
        }


        private void buttonApply_Click(object sender, RoutedEventArgs e) {
            var operationToApply = (ProcessingToolInformation) listView.SelectedItem;
            if (operationToApply == null) return;
            string processedFile = null;
            const string titleMessageBox = "Processing the file";
            const string messageMessageBox = "Error applying the processing tool: ";

            try {
                _writer.WriteLine(operationToApply.Name);

                //leggo l'uri del file processato
                processedFile = _reader.ReadLine();
            } catch (Exception) {
                MessageBox.Show(messageMessageBox+"error occured retriving the uri of the output file!", titleMessageBox);
                Close();
                return;
            }

            if (processedFile == null) {
                MessageBox.Show(messageMessageBox+"uri of the output file is null!", titleMessageBox);
                Close();
                return;
            }

            var saveDialog = new SaveFileDialog();

            if (saveDialog.ShowDialog() == true && saveDialog.CheckPathExists) {
                var path = saveDialog.FileName;
                Stream sourceFileBlob;
                FileStream destinationFile;

                try {
                    destinationFile = File.Create(path);
                } catch (Exception) {
                    MessageBox.Show(messageMessageBox + "error during the creation of the destination file",
                                    titleMessageBox);
                    Close();
                    return;
                }

                try {
                    sourceFileBlob = _ds.GetOutput(processedFile);
                } catch (InvalidOutputUriException) {
                    MessageBox.Show(messageMessageBox + "invalid output uri!", titleMessageBox);
                    destinationFile.Close();
                    Close();
                    return;
                } catch (ArgumentNullException) {
                    MessageBox.Show(messageMessageBox + "argument is null!", titleMessageBox);
                    destinationFile.Close();
                    Close();
                    return;
                } catch (UserNotLoggedInException) {
                    MessageBox.Show(messageMessageBox + "you are not logged in!", titleMessageBox);
                    destinationFile.Close();
                    Close();
                    return;
                } catch (Exception) {
                    MessageBox.Show(messageMessageBox + "unknown error!", titleMessageBox);
                    destinationFile.Close();
                    Close();
                    return;
                }

                //downloading file to the path
                try {
                    sourceFileBlob.CopyTo(destinationFile);
                } catch (Exception) {
                    MessageBox.Show(messageMessageBox+"error during the download of the file to the destination", titleMessageBox);
                   
                    destinationFile.Close();
                    sourceFileBlob.Close();
                    try {
                        _ds.DeleteOutput(processedFile);
                    } catch {
                        MessageBox.Show("An error occured while deleting the output file on the cloud! " +
                                        "But the file is successfuly saved locally!", titleMessageBox);
                    }

                    Close();
                    return;
                }
                sourceFileBlob.Close();
                destinationFile.Close();
                MessageBox.Show("File successfuly downloaded to: " + path, titleMessageBox);
            } else {
                try {
                    _ds.DeleteOutput(processedFile);
                } catch {
                    MessageBox.Show("An error occured while deleting the output file on the cloud! " +
                                    "Nothing is saved locally!", titleMessageBox);
                }
                MessageBox.Show("Processed file deleted from the cloud because you didn't " +
                                "want to save it or the specified path does not exists!", titleMessageBox);
            }

            Close();

        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
