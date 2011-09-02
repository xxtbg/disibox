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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Disibox.Data.Client;
using Disibox.Data.Client.Exceptions;
using Disibox.Gui.Util;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace Disibox.Gui {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private string _user;
        private string _password;
        private ClientDataSource _dataSource;

        //for accessing the server
        private readonly string _serverString;
        private readonly int _serverPort;

        public MainWindow() {
            InitializeComponent();
            _serverString = Properties.Settings.Default.DefaultProcessingServer;
            _serverPort = Properties.Settings.Default.DefaultProcessingServerPort;
        }

        #region getters&setters

        public string User {
            get { return _user; }
            set {
                _user = value;
                Title = "Disibox - " + _user;
            }
        }

        public string Password {
            set { _password = value; }
        }

        public ClientDataSource Datasource {
            get { return _dataSource; }
            set { _dataSource = value; }
        }

        #endregion

        #region uploading_file

        /*=============================================================================
            Uploading a file callbacks
        =============================================================================*/

        private void buttonBrowse_Click(object sender, RoutedEventArgs e) {
            var ofd = new OpenFileDialog();
            var result = ofd.ShowDialog();

            if (result == true)
                textBoxFileToUpload.Text = ofd.FileName;
        }

        private void buttonUpload_Click(object sender, RoutedEventArgs e) {
            if (textBoxFileToUpload.Text != "") {
                var filePath = textBoxFileToUpload.Text;
                var fileName = Path.GetFileName(filePath);
                FileStream fileStream;

                try {
                    fileStream = new FileStream(filePath, FileMode.Open);
                } catch (Exception ex) {
                    MessageBox.Show("The file to upload cannot be opened: " + ex, "Uploading a file");
                    textBoxFileToUpload.Text = "";
                    return;
                }

                var result = MessageBoxResult.Cancel;
                try {
                    _dataSource.AddFile(fileName, fileStream, false);
                } catch (FileExistingException) {
                    result = MessageBox.Show("This file already exists on the cloud, " +
                                             "do you want to overwrite it?",
                                             "Uploading a file", MessageBoxButton.YesNo);
                } catch (Exception) {
                    MessageBox.Show("Cannot upload the file to the cloud", "Uploading a file");
                    textBoxFileToUpload.Text = "";
                    fileStream.Close();
                    return;
                }

                switch (result) {
                    case MessageBoxResult.Cancel:
                        MessageBox.Show("The file has been uploaded successfully!", "Uploading a file");
                        break;

                    case MessageBoxResult.No:
                        MessageBox.Show("The file on the cloud has been NOT overwritten by the local file!",
                                        "Uploading a file");
                        break;

                    case MessageBoxResult.Yes:
                        try {
                            _dataSource.AddFile(fileName, fileStream, true);
                        } catch (Exception) {
                            MessageBox.Show("Cannot upload the file to the cloud (overwritting)", "Uploading a file");
                            textBoxFileToUpload.Text = "";
                            fileStream.Close();
                            return;
                        }

                        MessageBox.Show("The file has been uploaded successfully!", "Uploading a file");
                        break;
                }

                textBoxFileToUpload.Text = "";
                fileStream.Close();
            } else {
                MessageBox.Show("No file to upload", "Uploading a File");
            }
        }

        #endregion

        #region file_listing

        /*=============================================================================
            Listing, downloading and deleting files callbacks
        =============================================================================*/

        private void buttonRefreshFiles_Click(object sender, RoutedEventArgs e) {
            IList<FileMetadata> names = null;

            try {
                names = _dataSource.GetFileMetadata();
            } catch (Exception) {
                MessageBox.Show("Cannot refresh the file list", "Refreshing file list");
            }

            if (names == null)
                return;

            listView_Files.Items.Clear();

            foreach (var name in names)
                listView_Files.Items.Add(name);
        }

        private void buttonDeleteFile_Click(object sender, RoutedEventArgs e) {
            var selectedItem = (FileMetadata) listView_Files.SelectedItem;

            if (selectedItem == null) return;

            var ok = false;
            try {
                ok = _dataSource.DeleteFile(selectedItem.Uri);
            } catch (UserNotLoggedInException) {
                MessageBox.Show("Only a logged user can delete files that owns", "Deleting file");
            } catch (FileNotOwnedException) {
                MessageBox.Show("Error deleting not owned file", "Deleting file");
                return;
            }

            MessageBox.Show(ok ? "Error deleting the file" : "The file was deleted successfully", "Deleting file");

            PerformClick(buttonRefreshFiles);
        }

        private void buttonDownloadFile_Click(object sender, RoutedEventArgs e) {
            var selectedItem = (FileMetadata) listView_Files.SelectedItem;
            var saveDialog = new SaveFileDialog();
            FileStream destinationFile;

            if (selectedItem == null || saveDialog.ShowDialog() != true || !saveDialog.CheckPathExists) return;
            var fileToDownload = _dataSource.GetFile(selectedItem.Uri);

            //catch exception if any
            try {
                destinationFile = File.Create(saveDialog.FileName);
            } catch (Exception) {
                MessageBox.Show("Error during the download of the file (creating destination file) ", "Downloading file");
                return;
            }

            try {
                fileToDownload.CopyTo(destinationFile);
            } catch (Exception ex) {
                MessageBox.Show("Error during the download of the file: " + ex, "Downloading file");
                destinationFile.Close();
                return;
            }

            destinationFile.Close();
            MessageBox.Show("File successfuly downloaded to: " + saveDialog.FileName, "Downloading file");
        }

        #endregion

        #region user_listing

        /*=============================================================================
            Listing, adding and deleting users callbacks
        =============================================================================*/

        private void buttonAddUser_Click(object sender, RoutedEventArgs e) {
            var addWindow = new AddUserWindow(_dataSource);
            addWindow.ShowDialog();
            PerformClick(buttonRefreshUsers);
        }

        private void buttonDeleteUser_Click(object sender, RoutedEventArgs e) {
            var selectedUser = (UserAndType) listView_Users.SelectedItem;
            if (selectedUser == null) return;

            var result = MessageBox.Show("Are you sure you want to delete \"" + selectedUser.User + "\"?",
                                         "Deleting User", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
                return;

            try {
                _dataSource.DeleteUser(selectedUser.User);
            } catch (UserNotLoggedInException) {
                MessageBox.Show("Only a logged user (only administrator) can see the list of users", "Deleting User");
            } catch (UserNotAdminException) {
                MessageBox.Show("Only a logged user (only administrator) can see the list of users", "Deleting User");
            } catch (CannotDeleteLastAdminException) {
                MessageBox.Show("The default administrator user cannot be deleted!", "Deleting User");
            } catch (UserNotExistingException) {
                MessageBox.Show("The user you are trying to delete does not exists!", "Deleting User");
            }

            PerformClick(buttonRefreshUsers);
        }

        private void buttonRefreshUsers_Click(object sender, RoutedEventArgs e) {
            IList<string> adminUsers;
            IList<string> commonUsers;

            try {
                adminUsers = _dataSource.GetAdminUsersEmails();
                commonUsers = _dataSource.GetCommonUsersEmails();
            } catch (Exception) {
                MessageBox.Show("Only a logged user (only administrator) can see the list of users", "User Listing");
                return;
            }

            listView_Users.Items.Clear();

            foreach (var adminUser in adminUsers)
                listView_Users.Items.Add(new UserAndType {User = adminUser, Type = "Administrator"});

            foreach (var commonUser in commonUsers)
                listView_Users.Items.Add(new UserAndType {User = commonUser, Type = "Common"});
        }

        #endregion

        #region process_file

        /*=============================================================================
            Processing files callback
        =============================================================================*/

        private void processFile(object sender, RoutedEventArgs e) {
            var selectedItem = (FileMetadata) listView_Files.SelectedItem;

            if (selectedItem == null) {
                MessageBox.Show("You must select a file in order to process it!", "Processing file");
                return;
            }

            // estabilishing connection with the server
            var server = new TcpClient();
            StreamReader reader;
            StreamWriter writer;
            string answer;

            try {
                server.Connect(IPAddress.Parse(_serverString), _serverPort);
                reader = new StreamReader(server.GetStream());
                writer = new StreamWriter(server.GetStream()) {AutoFlush = true};
            } catch (Exception ex) {
                MessageBox.Show("Error during the connection to the processing Server: " + ex, "Processing file");
                return;
            }

            try {
                writer.WriteLine(_user);
                writer.WriteLine(_password);

                writer.WriteLine(selectedItem.ContentType);
                writer.WriteLine(selectedItem.Uri);

                answer = reader.ReadLine();
            } catch (Exception ex) {
                MessageBox.Show("An error occured while sending credentials and item to process: " + ex,
                                "Processing file");
                return;
            }

            if (answer == null || answer.Equals("KO")) {
                MessageBox.Show("Error during the authentication to the Dispatcher Role", "Processing File");
                reader.Close();
                writer.Close();
                server.Close();
                return;
            }

            new ProcessWindow(reader, writer, _dataSource);
        }

        #endregion

        #region exit_logout

        private void ExitProgram(object sender, RoutedEventArgs e) {
            _dataSource.Logout();
            Application.Current.Shutdown();
        }

        private void Logout(object sender, RoutedEventArgs e) {
            _dataSource.Logout();
            new LoginWindow().Show();
            Close();
        }

        #endregion

        #region utils

        private static void PerformClick(Button button) {
            var peer = new ButtonAutomationPeer(button);
            var invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            if (invokeProv != null)
                invokeProv.Invoke();
        }

        #endregion

        private void tabControl1_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.Source is TabControl) {
                if (tabItemFiles.IsSelected)
                    PerformClick(buttonRefreshFiles);
                else if (tabItemUsers.IsSelected)
                    PerformClick(buttonRefreshUsers);
            }
        }

    }
}
