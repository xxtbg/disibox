using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        private string _user;
        private string _password;
        private DataSource _dataSource;

        //for accessing the server
        private string _serverString = "127.0.0.1";
        private int _serverPort = 2345;

        public MainWindow() {
            InitializeComponent();
        }

        public string User {
            get { return _user; }
            set { _user = value;
                this.Title = "Disibox - " + _user;
            }
        }

        public string Password
        {
            set { _password = value; }
        }

        public DataSource Datasource {
            get { return _dataSource; }
            set { _dataSource = value; }
        }

        private void buttonBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            Nullable<bool> result = ofd.ShowDialog();

            if (result == true) {
                string filename = ofd.FileName;
                textBoxFileToUpload.Text = filename;
            }
        }

        private void buttonUpload_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxFileToUpload.Text != "") {
                var retn = _dataSource.AddFile(textBoxFileToUpload.Text);
                MessageBox.Show("The file has been uploaded successfully: " + retn);
            } else {
                MessageBox.Show("No file to upload");
            }

        }

        private void buttonRefreshFiles_Click(object sender, RoutedEventArgs e)
        {
            var names = _dataSource.GetFileNames();

            listView_Files.Items.Clear();

            foreach (var name in names) 
                listView_Files.Items.Add(name);
        }

        private void ExitProgram(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void Logout(object sender, RoutedEventArgs e) {
            _dataSource.Logout();
            new LoginWindow().Show();
            this.Close();
        }

        private void buttonDeleteFile_Click(object sender, RoutedEventArgs e) {
            var selectedItem = (FileAndMime)listView_Files.SelectedItem;

            if (selectedItem == null) return;
            
            Console.WriteLine(selectedItem.Filename);
//            to delete the file with this name = selectedItem.Filename
        }

        private void buttonDownloadFile_Click(object sender, RoutedEventArgs e) {
            var selectedItem = (FileAndMime)listView_Files.SelectedItem;

            if (selectedItem == null) return;
//            to download the file with this name = selectedItem.Filename
        }

        private void processFile(object sender, RoutedEventArgs e) {
            var selectedItem = (FileAndMime)listView_Files.SelectedItem;

//            if (selectedItem == null) return;
            //            to download the file with this name = selectedItem.Filename
            
            // estabilishing connection with the server
            var server = new TcpClient();
            server.Connect(IPAddress.Parse(_serverString), _serverPort);

            var reader = new StreamReader(server.GetStream());
            var writer = new StreamWriter(server.GetStream()) {AutoFlush = true};

            
            writer.WriteLine(_user);
            writer.WriteLine(_password);

            writer.WriteLine("mime");
            writer.WriteLine("file to process");


            var processWindow = new ProcessWindow(reader, writer);
            processWindow.ShowDialog();

        }

        private void buttonAddUser_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddUserWindow(_dataSource);
            addWindow.ShowDialog();
        }

        private void buttonDeleteUser_Click(object sender, RoutedEventArgs e)
        {

        }

        private void buttonRefreshUsers_Click(object sender, RoutedEventArgs e)
        {
            var adminUsers = _dataSource.GetAdminUsersEmails();
            var commonUsers = _dataSource.GetCommonUsersEmails();

            listView_Files.Items.Clear();

            foreach (var adminUser in adminUsers)
                listView_Users.Items.Add(adminUser);

            foreach (var commonUser in commonUsers)
                listView_Users.Items.Add(commonUser);

        }

        
    }
}
