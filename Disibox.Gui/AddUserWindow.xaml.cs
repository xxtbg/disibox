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

namespace Disibox.Gui
{
    /// <summary>
    /// Interaction logic for AddWindow.xaml
    /// </summary>
    public partial class AddUserWindow : Window
    {
        private DataSource _ds;
        public AddUserWindow(DataSource ds)
        {
            InitializeComponent();
            _ds = ds;
            textBox.Focus();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            var username = textBox.Text;
            var password1 = passwordBox.Password;
            var password2 = passwordBox2.Password;

            if (password1.Equals(password2))
            {
                var isAdmin = false;
                if (checkBoxAdmin.IsChecked == true)
                    isAdmin = true;

                try
                {
                    _ds.AddUser(username, password1, isAdmin);
                } catch(Exception)
                {
                    MessageBox.Show("Only a user with administrator priviledges can add a user", "Error inserting a new user");
                    textBox.Focus();
                    return;
                }
                Close();
            } else
            {
                MessageBox.Show("The two passwords must be the same!", "Error inserting a new user");
                passwordBox.Focus();
            }

        }


    }
}
