using Docms.Uploader.FileWatch;
using System.Linq;
using System.Windows.Controls;

namespace Docms.Uploader.Views
{
    /// <summary>
    /// WatchingFileListView.xaml の相互作用ロジック
    /// </summary>
    public partial class WatchingFileListView : UserControl
    {
        public WatchingFileListView()
        {
            InitializeComponent();
        }

        private void flow_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = DataContext as WatchingFileListViewModel;
            if (vm != null)
            {
                e.RemovedItems
                    .Cast<WatchingFileViewModel>()
                    .ToList()
                    .ForEach(item => vm.SelectedFiles.Remove(item));
                e.AddedItems
                    .Cast<WatchingFileViewModel>()
                    .ToList()
                    .ForEach(item => vm.SelectedFiles.Add(item));
            }
        }
    }
}
