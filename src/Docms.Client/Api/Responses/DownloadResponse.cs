using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.Api.Responses
{
    class DownloadResponse : Stream
    {
        private readonly string tempFilePath;
        private FileStream fileStream;

        public DownloadResponse()
        {
            tempFilePath = Path.GetTempFileName();
        }

        public async Task WriteResponse(Stream stream)
        {
            using (var fs = new FileStream(tempFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 4096, true))
            {
                await stream.CopyToAsync(fs);
            }
            fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, true);
        }

        public override bool CanRead => fileStream.CanRead;

        public override bool CanSeek => fileStream.CanSeek;

        public override bool CanWrite => fileStream.CanWrite;

        public override long Length => fileStream.Length;

        public override long Position { get => fileStream.Position; set => fileStream.Position = value; }

        public override void Flush()
        {
            fileStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return fileStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return fileStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            fileStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            fileStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                fileStream.Dispose();
                File.Delete(tempFilePath);
            }
        }
    }
}
