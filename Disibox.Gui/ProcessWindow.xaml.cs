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
            if (listView.SelectedIndex != -1)
            {
                Console.WriteLine(listView.SelectedIndex);
                _writer.WriteLine("index: " + listView.SelectedIndex);

                Console.WriteLine("trasmesso l'index");
                //leggo l'uri del file processato
                var processedFile = _reader.ReadLine();

                MessageBox.Show("uri of processed file (want to save or not?): " + processedFile);
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            //_server.Close();
            this.Close();
        }
    }
}
