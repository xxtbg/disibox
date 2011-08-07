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
using Microsoft.Win32;

namespace Disibox.Gui
{
    /// <summary>
    /// Interaction logic for ProcessWindow.xaml
    /// </summary>
    public partial class ProcessWindow : Window
    {
        private StreamReader _reader;
        private StreamWriter _writer;
        public ProcessWindow(StreamReader reader, StreamWriter writer)
        {
            InitializeComponent();
            _reader = reader;
            _writer = writer;
            FillListView();
        }

        private void FillListView()
        {
            var numberOfProcessingTools = Int32.Parse(_reader.ReadLine());

            for(int i=0; i<numberOfProcessingTools; ++i)
            {
                var listItem = _reader.ReadLine();
                listView.Items.Add(listItem);
            }
        }



        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            if (listView.SelectedIndex == -1) return;

            _writer.WriteLine(listView.SelectedIndex);

            //leggo l'uri del file processato
            var processedFile = _reader.ReadLine();


            var saveDialog = new SaveFileDialog();

            var result = saveDialog.ShowDialog();
            if (result == true && saveDialog.CheckPathExists)
            {
                var path = System.IO.Path.GetDirectoryName(saveDialog.FileName);
                    
                //downloading file to the path

                MessageBox.Show("file successfuly downloaded to: " + path);

            } else
            {
                //delete the temporary file from the cloud - processedFile
                MessageBox.Show("processed file deleted from the cloud because you didn't want to save it or the specified path does not exist");

            }

            this.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            //_server.Close();
            this.Close();
        }
    }
}
