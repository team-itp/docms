using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Docms.Web.Controllers
{
    public static class HttpContextExtensions
    {
        public static RangeHeaderValue GetRanges(this HttpContext context, long contentSize)
        {
            RangeHeaderValue rangesResult = null;

            string rangeHeader = context.Request.Headers["Range"];

            if (!string.IsNullOrEmpty(rangeHeader))
            {
                // rangeHeader contains the value of the Range HTTP Header and can have values like:
                //      Range: bytes=0-1            * Get bytes 0 and 1, inclusive
                //      Range: bytes=0-500          * Get bytes 0 to 500 (the first 501 bytes), inclusive
                //      Range: bytes=400-1000       * Get bytes 500 to 1000 (501 bytes in total), inclusive
                //      Range: bytes=-200           * Get the last 200 bytes
                //      Range: bytes=500-           * Get all bytes from byte 500 to the end
                //
                // Can also have multiple ranges delimited by commas, as in:
                //      Range: bytes=0-500,600-1000 * Get bytes 0-500 (the first 501 bytes), inclusive plus bytes 600-1000 (401 bytes) inclusive

                // Remove "Ranges" and break up the ranges
                string[] ranges = rangeHeader.Replace("bytes=", string.Empty).Split(",".ToCharArray());

                rangesResult = new RangeHeaderValue();

                for (int i = 0; i < ranges.Length; i++)
                {
                    const int START = 0, END = 1;

                    long endByte, startByte;

                    long parsedValue;

                    string[] currentRange = ranges[i].Split("-".ToCharArray());

                    if (long.TryParse(currentRange[END], out parsedValue))
                        endByte = parsedValue;
                    else
                        endByte = contentSize - 1;


                    if (long.TryParse(currentRange[START], out parsedValue))
                        startByte = parsedValue;
                    else
                    {
                        // No beginning specified, get last n bytes of file
                        // We already parsed end, so subtract from total and
                        // make end the actual size of the file
                        startByte = contentSize - endByte;
                        endByte = contentSize - 1;
                    }

                    rangesResult.Ranges.Add(new RangeItemHeaderValue(startByte, endByte));
                }
            }

            return rangesResult;
        }
    }

    public class VideoStreamResult : FileStreamResult
    {
        // default buffer size as defined in BufferedStream type
        private const int BufferSize = 0x1000;
        private string MultipartBoundary = "<qwe123>";

        public VideoStreamResult(Stream fileStream, string contentType)
            : base(fileStream, contentType)
        {

        }

        public VideoStreamResult(Stream fileStream, MediaTypeHeaderValue contentType)
            : base(fileStream, contentType.MediaType)
        {

        }

        private bool IsMultipartRequest(RangeHeaderValue range)
        {
            return range != null && range.Ranges != null && range.Ranges.Count > 1;
        }

        private bool IsRangeRequest(RangeHeaderValue range)
        {
            return range != null && range.Ranges != null && range.Ranges.Count > 0;
        }

        protected async Task WriteVideoAsync(HttpResponse response)
        {
            var bufferingFeature = response.HttpContext.Features.Get<IHttpBufferingFeature>();
            bufferingFeature?.DisableResponseBuffering();

            var length = FileStream.Length;

            var range = response.HttpContext.GetRanges(length);

            if (IsMultipartRequest(range))
            {
                response.ContentType = $"multipart/byteranges; boundary={MultipartBoundary}";
            }
            else
            {
                response.ContentType = ContentType.ToString();
            }

            response.Headers.Add("Accept-Ranges", "bytes");

            if (IsRangeRequest(range))
            {
                response.StatusCode = (int)HttpStatusCode.PartialContent;

                if (!IsMultipartRequest(range))
                {
                    response.Headers.Add("Content-Range", $"bytes {range.Ranges.First().From}-{range.Ranges.First().To}/{length}");
                }

                foreach (var rangeValue in range.Ranges)
                {
                    if (IsMultipartRequest(range)) // dunno if multipart works
                    {
                        await response.WriteAsync($"--{MultipartBoundary}");
                        await response.WriteAsync(Environment.NewLine);
                        await response.WriteAsync($"Content-type: {ContentType}");
                        await response.WriteAsync(Environment.NewLine);
                        await response.WriteAsync($"Content-Range: bytes {range.Ranges.First().From}-{range.Ranges.First().To}/{length}");
                        await response.WriteAsync(Environment.NewLine);
                    }

                    await WriteDataToResponseBody(rangeValue, response);

                    if (IsMultipartRequest(range))
                    {
                        await response.WriteAsync(Environment.NewLine);
                    }
                }

                if (IsMultipartRequest(range))
                {
                    await response.WriteAsync($"--{MultipartBoundary}--");
                    await response.WriteAsync(Environment.NewLine);
                }
            }
            else
            {
                await FileStream.CopyToAsync(response.Body);
            }
        }

        private async Task WriteDataToResponseBody(RangeItemHeaderValue rangeValue, HttpResponse response)
        {
            var startIndex = rangeValue.From ?? 0;
            var endIndex = rangeValue.To ?? 0;

            byte[] buffer = new byte[BufferSize];
            long totalToSend = endIndex - startIndex;
            int count = 0;

            long bytesRemaining = totalToSend + 1;
            response.ContentLength = bytesRemaining;

            FileStream.Seek(startIndex, SeekOrigin.Begin);

            while (bytesRemaining > 0)
            {
                try
                {
                    if (bytesRemaining <= buffer.Length)
                        count = FileStream.Read(buffer, 0, (int)bytesRemaining);
                    else
                        count = FileStream.Read(buffer, 0, buffer.Length);

                    if (count == 0)
                        return;

                    await response.Body.WriteAsync(buffer, 0, count);

                    bytesRemaining -= count;
                }
                catch (IndexOutOfRangeException)
                {
                    await response.Body.FlushAsync();
                    return;
                }
                finally
                {
                    await response.Body.FlushAsync();
                }
            }
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            await WriteVideoAsync(context.HttpContext.Response);
        }
    }
}
