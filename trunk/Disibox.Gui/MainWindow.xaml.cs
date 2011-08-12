using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
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
using Disibox.Data.Exceptions;

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

                var result = MessageBoxResult.Cancel;
                try
                {
                    _dataSource.AddFile(fileName, fileStream, false);
                }
                catch (FileAlreadyExistingException)
                {
                    result = MessageBox.Show("This file already exists on the cloud, do you want to overwrite it?", "Uploading a file", MessageBoxButton.YesNo);
                }
                catch (Exception)
                {
                    MessageBox.Show("Cannot upload the file to the cloud", "Uploading a file");
                    textBoxFileToUpload.Text = "";
                    fileStream.Close();
                    return;
                }

                switch (result)
                {
                    case MessageBoxResult.Cancel:
                        MessageBox.Show("The file has been uploaded successfully!", "Uploading a file");
                        break;

                    case MessageBoxResult.No:
                        MessageBox.Show("The file on the cloud has been NOT overwritten by the local file!", "Uploading a file");
                        break;

                    case MessageBoxResult.Yes:
                        try
                        {
                            _dataSource.AddFile(fileName, fileStream, true);
                        }
                        catch (Exception)
                        {
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

        private void buttonRefreshFiles_Click(object sender, RoutedEventArgs e)
        {
            IList<FileAndMime> names = null;

            try
            {
                names = _dataSource.GetFileMetadata();
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

            var ok = false;
            try
            {
                ok = _dataSource.DeleteFile(selectedItem.Uri);
            }
            catch (LoggedInUserRequiredException)
            {
                MessageBox.Show("Only a logged user can delete files that owns", "Deleting file");
            }
            catch (DeletingNotOwnedFileException)
            {
                MessageBox.Show("Error deleting not owned file", "Deleting file");
                return;
            }

            MessageBox.Show(ok ? "Error deleting the file" : "The file was deleted successfully", "Deleting file");

            PerformClick(buttonRefreshFiles);
        }

        private void buttonDownloadFile_Click(object sender, RoutedEventArgs e) {
            var selectedItem = (FileAndMime)listView_Files.SelectedItem;
            var saveDialog = new SaveFileDialog();
            FileStream destinationFile;

            if (selectedItem == null || saveDialog.ShowDialog() != true || !saveDialog.CheckPathExists) return;
            var fileToDownload = _dataSource.GetFile(selectedItem.Uri);

            //catch exception if any
            try
            {
                destinationFile = File.Create(saveDialog.FileName);
            } catch(Exception)
            {
                MessageBox.Show("Error during the download of the file (creating destination file) ", "Downloading file");
                return; 
            }

            try
            {
                fileToDownload.CopyTo(destinationFile);
            } catch (Exception ex)
            {
                MessageBox.Show("Error during the download of the file: " + ex, "Downloading file");
                destinationFile.Close();
                return;
            }

            destinationFile.Close();
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
                reader.Close();
                writer.Close();
                server.Close();
                return;
            }

            new ProcessWindow(reader, writer, _dataSource);

        }

        private void buttonAddUser_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddUserWindow(_dataSource);
            addWindow.ShowDialog();
            PerformClick(buttonRefreshUsers);
        }

        private void buttonDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = (UserAndType)listView_Users.SelectedItem;
            if (selectedUser == null) return;

            var result = MessageBox.Show("Are you sure you want to delete \"" + selectedUser.User + "\"?",
                                         "Deleting User", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.No)
                return;

            try
            {
                _dataSource.DeleteUser(selectedUser.User);
            } 
            catch (LoggedInUserRequiredException)
            {
                MessageBox.Show("Only a logged user (only administrator) can see the list of users", "Deleting User");                
            }
            catch(AdminUserRequiredException)
            {
                MessageBox.Show("Only a logged user (only administrator) can see the list of users", "Deleting User");
            }
            catch(CannotDeleteUserException)
            {
                MessageBox.Show("The default administrator user cannot be deleted!", "Deleting User");                
            }
            catch(UserNotExistingException)
            {
                MessageBox.Show("The user you are trying to delete does not exists!", "Deleting User");                
            }

            PerformClick(buttonRefreshUsers);
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


         /* utils */
        private static void PerformClick(Button button)
        {
            var peer = new ButtonAutomationPeer(button);
            var invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            if (invokeProv != null)
                invokeProv.Invoke();
        }

    }
}
