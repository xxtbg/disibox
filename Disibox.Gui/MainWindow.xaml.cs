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
using Disibox.Gui.Util;
using Microsoft.Win32;
using Disibox.Data;
using Path = System.IO.Path;

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
            var ofd = new OpenFileDialog();
            var result = ofd.ShowDialog();

            if (result == true) 
                textBoxFileToUpload.Text = ofd.FileName;
        }

        private void buttonUpload_Click(object sender, RoutedEventArgs e)
        {
            if (textBoxFileToUpload.Text != "")
            {
                var filePath = textBoxFileToUpload.Text;
                var fileName = Path.GetFileName(filePath);
                FileStream fileStream;
                try
                {
                    fileStream = new FileStream(filePath, FileMode.Open);
                } catch (Exception ex)
                {
                    MessageBox.Show("The file to upload cannot be opened: " + ex, "Uploading a file");
                    textBoxFileToUpload.Text = "";
                    return;
                }

                try
                {
                    _dataSource.AddFile(fileName, fileStream);
                }
                catch (Exception)
                {
                    MessageBox.Show("Cannot upload the file to the cloud", "Uploading a file");
                    textBoxFileToUpload.Text = "";
                    return;
                }

                MessageBox.Show("The file has been uploaded successfully!", "Uploading a file");
                textBoxFileToUpload.Text = "";
            } else {
                MessageBox.Show("No file to upload");
            }

        }

        private void buttonRefreshFiles_Click(object sender, RoutedEventArgs e)
        {
            IList<FileAndMime> names = null;

            try
            {
                names = _dataSource.GetFilesMetadata();
            } catch (Exception)
            {
                MessageBox.Show("Cannot refresh the file list", "Refreshing file list");
            }

            if (names == null)
                return;

            listView_Files.Items.Clear();

            foreach (var name in names) 
                listView_Files.Items.Add(name);
        }

        private void ExitProgram(object sender, RoutedEventArgs e) {
            _dataSource.Logout();
            Application.Current.Shutdown();
        }

        private void Logout(object sender, RoutedEventArgs e) {
            _dataSource.Logout();
            new LoginWindow().Show();
            Close();
        }

        private void buttonDeleteFile_Click(object sender, RoutedEventArgs e) {
            var selectedItem = (FileAndMime)listView_Files.SelectedItem;

            if (selectedItem == null) return;
            
            Console.WriteLine(selectedItem.Filename);
//            to delete the file with this name = selectedItem.Filename
        }

        private void buttonDownloadFile_Click(object sender, RoutedEventArgs e) {
            var selectedItem = (FileAndMime)listView_Files.SelectedItem;
            var saveDialog = new SaveFileDialog();

            if (selectedItem == null || saveDialog.ShowDialog() != true || !saveDialog.CheckPathExists) return;
            var fileToDownload = _dataSource.GetFile(selectedItem.Uri);

            //catch exception if any
            try
            {
                fileToDownload.CopyTo(File.Create(saveDialog.FileName));
            } catch (Exception ex)
            {
                MessageBox.Show("Error during the download of the file: " + ex, "Downloading file");
                return;
            }

            MessageBox.Show("File successfuly downloaded to: " + saveDialog.FileName, "Downloading file");
        }

        private void processFile(object sender, RoutedEventArgs e) {
            var selectedItem = (FileAndMime)listView_Files.SelectedItem;

            if (selectedItem == null)
            {
                MessageBox.Show("You must select a file in order to process it!", "Processing file");
                return;
            }
            
            // estabilishing connection with the server
            var server = new TcpClient();
            StreamReader reader;
            StreamWriter writer;
            string answer;

            try
            {
                server.Connect(IPAddress.Parse(_serverString), _serverPort);
                reader = new StreamReader(server.GetStream());
                writer = new StreamWriter(server.GetStream()) {AutoFlush = true};
            } catch(Exception ex)
            {
                MessageBox.Show("Error during the connection to the processing Server: " + ex, "Processing file");
                return;
            }

            try
            {
                writer.WriteLine(_user);
                writer.WriteLine(_password);

                writer.WriteLine(selectedItem.Mime);
                writer.WriteLine(selectedItem.Uri);

                answer = reader.ReadLine();
            } catch (Exception ex)
            {
                MessageBox.Show("An error occured while sending credentials and item to process: " + ex, "Processing file");
                return;
            }

            if (answer == null || answer.Equals("KO"))
            {
                MessageBox.Show("Error during the authentication to the Dispatcher Role", "Processing File");
                server.Close();
                return;
            }

            new ProcessWindow(reader, writer, _dataSource);

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
            IList<string> adminUsers;
            IList<string> commonUsers;

            try
            {
                adminUsers = _dataSource.GetAdminUsersEmails();
                commonUsers = _dataSource.GetCommonUsersEmails();
            } catch(Exception)
            {
                MessageBox.Show("Only a logged user (only administrator) can see the list of users", "User Listing");
                return;
            }

            listView_Users.Items.Clear();

            foreach (var adminUser in adminUsers)
                listView_Users.Items.Add(new UserAndType {User = adminUser, Type = "Administrator"});

            foreach (var commonUser in commonUsers)
                listView_Users.Items.Add(new UserAndType {User = commonUser, Type = "Common"});

        }

        
    }
}
