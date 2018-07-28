using Docms.Uploader.Common;
using System;
using System.IO;

namespace Docms.Uploader.FileWatch
{

    public class WatchingFileViewModel : ViewModelBase
    {
        public WatchingFileViewModel(string fullPath, string relativePath)
        {
            FullPath = fullPath;
            FileName = Path.GetFileName(relativePath);
            Extension = Path.GetExtension(relativePath).ToLowerInvariant();
            DisplayPath = relativePath;
        }
        public string FullPath { get; }
        public string FileName { get; }
        public string Extension { get; }
        public string DisplayPath { get; set; }

        public static WatchingFileViewModel Create(string fullPath, string watchingRoot)
        {
            var relativePath = fullPath.Remove(0, watchingRoot.Length);
            if (relativePath.StartsWith("\\"))
            {
                relativePath = relativePath.Substring(1);
            }
            var file = new WatchingFileViewModel(fullPath, relativePath.ToString());
            return file;
        }
    }
}
