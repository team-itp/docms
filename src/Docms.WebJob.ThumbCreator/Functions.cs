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
            [BlobTrigger("files/{name}")] CloudBlockBlob input,
            [Blob("thumbnails/{name}_large.png")] CloudBlockBlob largeThumbOutput,
            [Blob("thumbnails/{name}_small.png")] CloudBlockBlob smallThumbOutput,
            TextWriter log)
        {
            log.WriteLine("Processing: " + input.Name);
            var largeThumbMs = default(MemoryStream);
            if (input.Properties.ContentType.EndsWith("pdf")) // application/pdf
            {
                double pageWidth;
                double pageHeight;
                using (var pdfDoc = PdfSharp.Pdf.IO.PdfReader.Open(input.OpenRead()))
                {
                    pageWidth = pdfDoc.Pages[0].Width.Inch * 175.1426;
                    pageHeight = pdfDoc.Pages[0].Height.Inch * 175.1426;
                }
                using (var pdfDoc = PdfiumViewer.PdfDocument.Load(input.OpenRead()))
                {
                    var image = pdfDoc.Render(0, (int)pageWidth, (int)pageHeight, 96, 96, true);

                    largeThumbMs = new MemoryStream();
                    image.Save(largeThumbMs, ImageFormat.Png);
                }
            }
            else if (input.Properties.ContentType.StartsWith("image"))
            {
                largeThumbMs = new MemoryStream();
                input.DownloadToStream(largeThumbMs);
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
