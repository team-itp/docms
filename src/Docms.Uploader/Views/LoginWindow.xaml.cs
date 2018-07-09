using Docms.Uploader.Common;
using System.Windows;
using System.Windows.Controls;

namespace Docms.Uploader.Views
{
    /// <summary>
    /// LoginWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LoginViewModel;
            var pb = sender as PasswordBox;
            if (vm != null && pb != null)
            {
                vm.Password = pb.Password;
            }
        }
    }
}
