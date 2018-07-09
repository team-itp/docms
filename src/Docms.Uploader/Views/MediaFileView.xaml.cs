using Docms.Uploader.FileWatch;
using Docms.Uploader.Views.Utils;
using PdfiumViewer;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Docms.Uploader.Views
{
    /// <summary>
    /// MediaFileView.xaml の相互作用ロジック
    /// </summary>
    public partial class MediaFileView : UserControl
    {
        private CancellationTokenSource _tokenSource;

        public MediaFileView()
        {
            InitializeComponent();
            this.DataContextChanged += MediaFileView_DataContextChanged;
            this.Unloaded += MediaFileView_Unloaded;
        }

        private void MediaFileView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
            }

            _tokenSource = new CancellationTokenSource();
            var token = _tokenSource.Token;
            var waitingTimeInMs = 1000;
            Task.Run(async () =>
            {
                await Task.Delay(100);
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await RefleshMedia();
                        _tokenSource = null;
                        return;
                    }
                    catch (Exception)
                    {
                        await Task.Delay(waitingTimeInMs *= 2);
                    }
                }
            });
        }

        private void MediaFileView_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            _tokenSource?.Cancel();
        }

        public async Task RefleshMedia()
        {
            var mediaFile = default(MediaFile);
            await this.Dispatcher.InvokeAsync(() =>
            {
                mediaFile = DataContext as MediaFile;
            });

            if (mediaFile == null)
            {
                return;
            }

            var path = mediaFile.FullPath;
            if (!File.Exists(path))
                return;

            if (path.ToUpper().EndsWith(".PDF"))
            {
                using (var document = PdfDocument.Load(path))
                {
                    var bitmap = document.Render(0, 96, 96, false);
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        image.Source = BitmapHelper.ToBitmapSource(bitmap);
                    });
                }
            }
            else
            {
                await this.Dispatcher.InvokeAsync(() =>
                {
                    image.Source = new BitmapImage(new Uri(path));
                });
            }
        }
    }
}
