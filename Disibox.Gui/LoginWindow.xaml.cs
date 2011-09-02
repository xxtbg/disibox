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

using System.Windows;
using Disibox.Data.Client;
using Disibox.Data.Client.Exceptions;

namespace Disibox.Gui {
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window {
        private readonly MainWindow _mainWindow;
        private readonly ClientDataSource _dataSource;

        public LoginWindow() {
            InitializeComponent();
            _mainWindow = new MainWindow();
            _dataSource = new ClientDataSource();
            textBox.Focus();
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
                    Close();
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
