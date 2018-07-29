using Docms.Uploader.Common;
using Docms.Uploader.Views.Utils;
using System.Windows;
using System.Windows.Controls;

namespace Docms.Uploader.Views
{
    /// <summary>
    /// LoginWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class LoginWindow
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void PasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                PasswordBoxHelper.SetPassword(PasswordBox, PasswordBox.SecurePassword);
                LoginButton.Command?.Execute(LoginButton.CommandParameter);
            }
        }
    }
}
