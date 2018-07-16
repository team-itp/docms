using System;
using System.IO;
using PdfiumViewer;

namespace Docms.Uploader.FileWatch
{

    public abstract class MediaFile
    {
        public MediaFile(string name, string fullPath)
        {
            Name = name;
            FullPath = fullPath;
        }

        public string Name { get; }
        public string FullPath { get; }

        public static bool IsMediaFile(string path)
        {
            var ext = Path.GetExtension(path).ToUpperInvariant();
            return ext.EndsWith(".PDF") || ext.EndsWith(".PNG") || ext.EndsWith(".JPG");
        }

        public static MediaFile Create(string fullPath)
        {
            var name = Path.GetFileName(fullPath);
            var ext = Path.GetExtension(fullPath).ToUpperInvariant();
            if (ext.EndsWith(".PDF"))
            {
                return new PdfFile(name, fullPath);
            }
            return new ImageFile(name, fullPath);
        }
    }

    public class PdfFile : MediaFile
    {
        public PdfFile(string name, string fullPath) : base(name, fullPath)
        {
            try
            {
                using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
                {
                    PdfDocument.Load(fs);
                }
            }
            catch
            {
                GC.Collect();
                throw;
            }
        }
    }

    public class ImageFile : MediaFile
    {
        public ImageFile(string name, string fullPath) : base(name, fullPath)
        {
        }
    }
}
