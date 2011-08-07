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
using System.Windows.Shapes;
using Disibox.Data;
using Disibox.Data.Exceptions;

namespace Disibox.Gui {
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window {
        private MainWindow _mainWindow;
        private DataSource _dataSource;
        public LoginWindow() {
            InitializeComponent();
            _mainWindow = new MainWindow();
            _dataSource = new DataSource();
        }

        private void buttonLogin_Click(object sender, RoutedEventArgs e) {
            var username = textBox.Text;
            var password = passwordBox.Password;

            try {
                _dataSource.Login(username, password);
                if (!_mainWindow.IsVisible) {
                    _mainWindow.User = username;
                    _mainWindow.Password = password;
                    _mainWindow.Datasource = _dataSource;
                    _mainWindow.Show();
                    this.Close();
                }
            } catch (UserNotExistingException) {
                MessageBox.Show("User and/or Passowrd are not correct, please retry!", "Error when Log in");
            }

        }

        private void buttonExit_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

    }
}
