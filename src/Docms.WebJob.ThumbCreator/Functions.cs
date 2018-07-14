using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Docms.WebJob.ThumbCreator
{
    public class Functions
    {
        public static void ProcessQueueMessage(
            [BlobTrigger("files/{name}.{ext}")] Stream input, string name, string ext,
            [Blob("thumbnails/{name}_large.{ext}", FileAccess.Write)] CloudBlockBlob largeThumbOutput,
            [Blob("thumbnails/{name}_small.{ext}", FileAccess.Write)] CloudBlockBlob smallThumbOutput,
            TextWriter log)
        {
            log.WriteLine("Processing: " + name);
            var largeThumbMs = default(MemoryStream);
            if (ext.ToUpperInvariant() == "PDF")
            {
                var temp = new MemoryStream();
                input.CopyTo(temp);

                double pageWidth;
                double pageHeight;
                temp.Seek(0, SeekOrigin.Begin);
                using (var pdfDoc = PdfSharp.Pdf.IO.PdfReader.Open(temp))
                {
                    pageWidth = pdfDoc.Pages[0].Width.Inch * 175.1426;
                    pageHeight = pdfDoc.Pages[0].Height.Inch * 175.1426;
                }
                temp.Seek(0, SeekOrigin.Begin);
                using (var pdfDoc = PdfiumViewer.PdfDocument.Load(temp))
                {
                    var image = pdfDoc.Render(0, (int)pageWidth, (int)pageHeight, 96, 96, true);

                    largeThumbMs = new MemoryStream();
                    image.Save(largeThumbMs, ImageFormat.Png);
                }
            }
            else if (ext.ToUpperInvariant() == "PNG" 
                || ext.ToUpperInvariant() == "JPG"
                || ext.ToUpperInvariant() == "JPEG")
            {
                largeThumbMs = new MemoryStream();
                input.CopyTo(largeThumbMs);
            }

            if (largeThumbMs == null)
            {
                return;
            }


            var factory = new ImageProcessor.ImageFactory();
            var smallThumbMs = new MemoryStream();
            largeThumbMs.Seek(0, SeekOrigin.Begin);
            factory.Load(largeThumbMs);
            factory.Resize(new Size(280, 280))
                .Save(smallThumbMs);
            largeThumbOutput.UploadFromStream(largeThumbMs);
            largeThumbOutput.Properties.ContentType = "image/png";
            largeThumbOutput.SetProperties();
            smallThumbOutput.UploadFromStream(smallThumbMs);
            smallThumbOutput.Properties.ContentType = "image/png";
            smallThumbOutput.SetProperties();
        }
    }
}
