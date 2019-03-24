using Docms.Client.Types;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.DocumentStores
{
    class LocalDocumentStreamToken : IDocumentStreamToken
    {
        private readonly PathString path;
        private readonly LocalDocumentStorage storage;
        private readonly List<Stream> streams;

        public LocalDocumentStreamToken(PathString path, LocalDocumentStorage storage)
        {
            this.path = path;
            this.storage = storage;
            this.streams = new List<Stream>();
        }

        public void Dispose()
        {
            foreach (var stream in streams)
            {
                stream.Dispose();
            }
        }

        public Task<Stream> GetStreamAsync()
        {
            var fullpath = storage.GetFullPath(path);
            var stream = File.Open(fullpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            streams.Add(stream);
            return Task.FromResult(stream as Stream);
        }
    }
}
