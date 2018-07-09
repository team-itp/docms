using Docms.Uploader.ApplicationSettings;
using System.Windows;
using System.Windows.Controls;

namespace Docms.Uploader.Views
{
    /// <summary>
    /// WatchDirectorySettingsView.xaml の相互作用ロジック
    /// </summary>
    public partial class WatchDirectorySettingsView : UserControl
    {
        public WatchDirectorySettingsView()
        {
            InitializeComponent();
        }

        private void Reference_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = DirectoryToWatch.Text;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    var vm = DataContext as WatchDirecotrySettingsViewModel;
                    if (vm != null)
                    {
                        vm.WatchDirectory = dialog.SelectedPath;
                    }
                }
            }
        }
    }
}
