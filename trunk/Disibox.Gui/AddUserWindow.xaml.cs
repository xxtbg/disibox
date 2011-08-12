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
// DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//

using System;
using System.Windows;
using Disibox.Data;

namespace Disibox.Gui {
    /// <summary>
    /// Interaction logic for AddWindow.xaml
    /// </summary>
    public partial class AddUserWindow : Window {
        private readonly DataSource _ds;

        public AddUserWindow(DataSource ds) {
            InitializeComponent();
            _ds = ds;
            textBox.Focus();
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e) {
            var username = textBox.Text;
            var password1 = passwordBox.Password;
            var password2 = passwordBox2.Password;

            if (password1.Equals(password2)) {
                var isAdmin = false;
                if (checkBoxAdmin.IsChecked == true)
                    isAdmin = true;

                try {
                    _ds.AddUser(username, password1, isAdmin);
                } catch (Exception) {
                    MessageBox.Show("Only a user with administrator priviledges can add a user",
                                    "Error inserting a new user");
                    textBox.Focus();
                    return;
                }
                Close();
            } else {
                MessageBox.Show("The two passwords must be the same!", "Error inserting a new user");
                passwordBox.Focus();
            }
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}