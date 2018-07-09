using Docms.Client;
using Docms.Uploader.Common;
using Docms.Uploader.Properties;
using Docms.Uploader.Views;
using System.Threading.Tasks;
using System.Windows;

namespace Docms.Uploader
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = new MainWindow();

            var client = new DocmsClient(Settings.Default.DocmsWebEndpoint);

            var userid = Settings.Default.UserId;
            var password = Settings.Default.GetPassword();

            var needCredential = string.IsNullOrEmpty(userid);
            if (!needCredential)
            {
                try
                {
                    Task.Run(async () => await client.LoginAsync(userid, password)).Wait();
                }
                catch
                {
                    needCredential = true;
                }
            }

            if (needCredential)
            {
                var loginWindow = new LoginWindow();
                var loginVM = new LoginViewModel(client)
                {
                    Username = userid,
                };
                loginWindow.DataContext = loginVM;

                var loginSuccess = false;
                loginVM.LoginSucceeded += (s, ev) =>
                {
                    loginWindow.Close();
                    loginSuccess = true;
                };
                loginWindow.ShowDialog();
                if (!loginSuccess)
                {
                    Shutdown();
                    return;
                }
            }

            var mainVM = new MainWindowViewModel(client);
            mainVM.SessionEnded += (s, ev) =>
            {
                mainWindow.Close();
            };
            mainWindow.DataContext = mainVM;
            mainWindow.Show();
        }
    }
}
