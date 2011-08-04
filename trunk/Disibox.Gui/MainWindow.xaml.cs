using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Disibox.Data;

namespace Disibox.Gui {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }


        private void button1_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();

            Nullable<bool> result = ofd.ShowDialog();

            if (result == true) {
                string filename = ofd.FileName;
                textBox1.Text = filename;
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e) {
            var ds = new DataSource();

            if (textBox1.Text != "") {
                var retn = ds.AddFile(textBox1.Text);
                MessageBox.Show("The file has been uploaded successfully: " + retn);
            } else {
                MessageBox.Show("No file to upload");
            }

        }

        private void button3_Click(object sender, RoutedEventArgs e) {
            var ds = new DataSource();

            var names = ds.GetFileNames();

            foreach (var name in names) 
                listView_Files.Items.Add(name);
        }
    }
}
