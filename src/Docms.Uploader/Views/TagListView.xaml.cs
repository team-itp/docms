using Docms.Uploader.Upload;
using MaterialDesignThemes.Wpf;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Docms.Uploader.Views
{
    /// <summary>
    /// TagListView.xaml の相互作用ロジック
    /// </summary>
    public partial class TagListView : UserControl
    {
        public TagListView()
        {
            InitializeComponent();
        }

        private void Chip_DeleteClick(object sender, System.Windows.RoutedEventArgs e)
        {
            var chip = sender as Chip;
            var tag = chip.DataContext as Tag;
            var collection = DataContext as ICollection<Tag>;
            collection.Remove(tag);
        }
    }
}
