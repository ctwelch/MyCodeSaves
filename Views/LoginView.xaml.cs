using BasicBridge.Models;
using System;
using System.Text;
using System.Windows;

namespace BasicBridge.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
            DataModel = new DataModel();
            PpUserId.Focus();
        }

        DataModel DataModel { get; set; }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.PpUserId != null && this.PpPassword != null)
            {
                string userId = this.PpUserId.Text;
                string password = this.PpPassword.Password;

                var encrypted = ApplyPpEncryption(password);
                //#if DEBUG
                //var user = new User() { UserId = "ndaniels", EntityNum = 4514826 };
                //var view = new MainWindow(user);
                //view.Show();
                //this.Close();
                //#else
                var user = DataModel.Login(userId, encrypted);

                if (!string.IsNullOrEmpty(user.UserId))
                {
                    user.UserId = user.UserId.ToLower();
                    var view = new MainWindow(user);
                    view.Show();
                    this.Close();
                }
                else
                {
                    // produce a warning that login has failed
                    LoginErrorMessage.Visibility = Visibility.Visible;
                }
                //#endif
            }
        }

        private string ApplyPpEncryption(string password)
        {
            string result = "";

            foreach(char character in password)
            {
                result += Encoding.Default.GetString(new byte[] { Convert.ToByte(character * 2) });
            }

            return result;
        }
    }
}
