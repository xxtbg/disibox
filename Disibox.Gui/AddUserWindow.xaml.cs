﻿//
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

using System;
using System.Windows;
using Disibox.Data.Client;
using Disibox.Data.Client.Exceptions;
using Disibox.Data.Entities;
using Disibox.Data.Exceptions;

namespace Disibox.Gui {
    /// <summary>
    /// Interaction logic for AddWindow.xaml
    /// </summary>
    public partial class AddUserWindow : Window {
        private readonly ClientDataSource _ds;

        public AddUserWindow(ClientDataSource ds)
        {
            InitializeComponent();
            _ds = ds;
            textBox.Focus();
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e) {
            var username = textBox.Text;
            var password1 = passwordBox.Password;
            var password2 = passwordBox2.Password;

            if (password1.Equals(password2)) {
                var userType = UserType.CommonUser;
                if (checkBoxAdmin.IsChecked == true)
                    userType = UserType.AdminUser;

                try {
                    _ds.AddUser(username, password1, userType);
                } catch (UserNotAdminException) {
                    MessageBox.Show("Only a user with administrator priviledges can add a user",
                                    "Error inserting a new user");
            } catch (ArgumentNullException) {
                    MessageBox.Show("User and/or Passowrd are blank, please retry!", "Error inserting new user");
                    textBox.Focus();
                    return;
                } catch (InvalidEmailException) {
                    MessageBox.Show("User email is invalid, please retry!", "Error inserting new user");
                    textBox.Focus();
                    return;
                } catch (InvalidPasswordException) {
                    MessageBox.Show("User password is invalid, please retry!", "Error inserting new user");
                    textBox.Focus();
                    return;
                } catch (UserNotLoggedInException) {
                    MessageBox.Show("Trying to insert new user without being logged in, log in " +
                                    "first and then please retry !", "Error inserting new user");
                    textBox.Focus();
                    return;
                } catch (UserExistingException) {
                    MessageBox.Show("The user you trying to insert already exists, please retry with another email!",
                                    "Error inserting new user");
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