﻿using ImageProcessor.Imaging;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Docms.WebJob.ThumbCreator
{
    public class Functions
    {
        public static void ProcessQueueMessage(
            [QueueTrigger("create-thumbnail")] CreateThumbnailQueue queue,
            TextWriter log)
        {
            log.WriteLine("Processing: " + queue.BlobName);

            var account = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);
            var blobClient = account.CreateCloudBlobClient();
            var filesContainer = blobClient.GetContainerReference("files");
            CloudBlockBlob input = filesContainer.GetBlockBlobReference(queue.BlobName);
            var thumbnailsContainer = blobClient.GetContainerReference("thumbnails");
            thumbnailsContainer.CreateIfNotExists();
            CloudBlockBlob largeThumbOutput = thumbnailsContainer.GetBlockBlobReference(string.Format("{0}_large.png", queue.BlobName));
            CloudBlockBlob smallThumbOutput = thumbnailsContainer.GetBlockBlobReference(string.Format("{0}_small.png", queue.BlobName));

           var inputMs = new MemoryStream();
            input.DownloadToStream(inputMs);

            if (queue.ContentType.EndsWith("pdf")) // application/pdf
            {
                var pdfImageStream = CreatePdfImage(inputMs);
                var smallThumbMs = CreateSmallThumb(pdfImageStream);
                UploadAsPngImages(pdfImageStream, largeThumbOutput);
                UploadAsPngImages(smallThumbMs, smallThumbOutput);
            }
            else if (queue.ContentType.StartsWith("image"))
            {
                inputMs.Seek(0, SeekOrigin.Begin);
                var largeThumbMs = new MemoryStream();
                new ImageProcessor.ImageFactory()
                    .Load(inputMs)
                    .Resize(new ResizeLayer(new Size(2048, 2048), ResizeMode.Max, AnchorPosition.Center, upscale: false))
                    .Save(largeThumbMs);

                var smallThumbMs = CreateSmallThumb(inputMs);
                UploadAsPngImages(largeThumbMs, largeThumbOutput);
                UploadAsPngImages(smallThumbMs, smallThumbOutput);
            }
        }

        private static MemoryStream CreatePdfImage(MemoryStream pdfStream)
        {
            var output = new MemoryStream();
            double pageWidth;
            double pageHeight;
            pdfStream.Seek(0, SeekOrigin.Begin);
            using (var pdfDoc = PdfSharp.Pdf.IO.PdfReader.Open(pdfStream))
            {
                pageWidth = pdfDoc.Pages[0].Width.Inch * 175.1426;
                pageHeight = pdfDoc.Pages[0].Height.Inch * 175.1426;
            }
            pdfStream.Seek(0, SeekOrigin.Begin);

            using (var pdfDoc = PdfiumViewer.PdfDocument.Load(pdfStream))
            {
                var image = pdfDoc.Render(0, (int)pageWidth, (int)pageHeight, 96, 96, true);
                image.Save(output, ImageFormat.Png);
            }
            return output;
        }

        private static MemoryStream CreateSmallThumb(MemoryStream inputStream)
        {
            inputStream.Seek(0, SeekOrigin.Begin);

            var smallThumbMs = new MemoryStream();
            new ImageProcessor.ImageFactory()
                .Load(inputStream)
                .Resize(new ResizeLayer(new Size(480, 480), ResizeMode.Crop, AnchorPosition.Center, upscale: false))
                .Save(smallThumbMs);
            return smallThumbMs;
        }

        private static void UploadAsPngImages(Stream input, CloudBlockBlob output)
        {
            output.UploadFromStream(input);
            output.Properties.ContentType = "image/png";
            output.SetProperties();
        }
    }
}
