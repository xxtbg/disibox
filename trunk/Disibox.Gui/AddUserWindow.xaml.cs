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
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            var username = textBox.Text;
            var password1 = passwordBox.Password;
            var password2 = passwordBox2.Password;

            if (password1.Equals(password2))
            {
                _ds.AddUser(username, password1, (bool)checkBoxAdmin.IsChecked);
                this.Close();
            } else
            {
                MessageBox.Show("The two passwords must be the same!", "Error inserting a new user");
            }

        }


    }
}
