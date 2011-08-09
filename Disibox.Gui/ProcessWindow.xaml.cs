using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Disibox.Data;
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
        private readonly DataSource _ds;

        private bool _erroFillingList = false;
        public ProcessWindow(StreamReader reader, StreamWriter writer, DataSource ds)
        {
            InitializeComponent();
            _reader = reader;
            _writer = writer;
            _ds = ds;

            FillListView();
            
            if (!_erroFillingList)
                ShowDialog();
        }

        private void FillListView()
        {
            string temp = null;
            var numberOfProcessingTools = 0;
            try
            {
                temp = _reader.ReadLine();
            }
            catch (Exception)
            {
                MessageBox.Show("The server is not responding, try later!", "Information");
                _erroFillingList = true;
                return;
            }

            if (temp == null)
            {
                MessageBox.Show("Error occured during comminication with the server, try later!", "Information");
                _erroFillingList = true;
                return;
            }

            try
            {
                numberOfProcessingTools = Int32.Parse(temp);
            } catch (Exception)
            {
                MessageBox.Show("Mesage returned from server is wrong format or null, try later!", "Information");
                _erroFillingList = true;
                return;
            }

            for(var i=0; i<numberOfProcessingTools; ++i)
            {
                string listItem = null;
                try
                {
                    listItem = _reader.ReadLine();   
                } catch (Exception)
                {
                    MessageBox.Show("Error occured during comminication with the server, try later!", "Information");
                    _erroFillingList = true;
                    return;
                }
                listView.Items.Add(listItem);
            }
        }



        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            var operationToApply = (string)listView.SelectedItem;
            if (operationToApply == null) return;
            string processedFile = null;

            try
            {
                _writer.WriteLine(operationToApply);

                //leggo l'uri del file processato
                processedFile = _reader.ReadLine();
            } catch (Exception)
            {
                MessageBox.Show("Error occured during comminication with the server, try later!", "Information");
                Close();
                return;
            }

            if (processedFile == null)
            {
                MessageBox.Show("Error occured during comminication with the server, try later!", "Information");
                Close();
                return;
            }

            var saveDialog = new SaveFileDialog();

            if (saveDialog.ShowDialog() == true && saveDialog.CheckPathExists)
            {
                var path = saveDialog.FileName;
                var fileblob = _ds.GetFile(processedFile);

                
                //downloading file to the path
                try
                {
                    fileblob.CopyTo(File.Create(path));
                } catch (Exception)
                {
                    MessageBox.Show("Error during the download of the file", "Downloading file");

                    //delete the file fileblob

                    Close();
                    return;
                }
                MessageBox.Show("File successfuly downloaded to: " + path, "Downloading file");
            } else
            {
                MessageBox.Show("processed file deleted from the cloud because you didn't " +
                                "want to save it or the specified path does not exist", "Downloading file");

            }

            //delete the file fileblob
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
