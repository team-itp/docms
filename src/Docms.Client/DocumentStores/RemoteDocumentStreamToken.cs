using Docms.Client.Api;
using Docms.Client.Types;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.DocumentStores
{
    class RemoteDocumentStreamToken : IDocumentStreamToken
    {
        private readonly PathString path;
        private readonly IDocmsApiClient api;
        private readonly List<Stream> streams;

        public RemoteDocumentStreamToken(PathString path, IDocmsApiClient api)
        {
            this.path = path;
            this.api = api;
            streams = new List<Stream>();
        }

        public async Task<Stream> GetStreamAsync()
        {
            var stream = await api.DownloadAsync(path.ToString()).ConfigureAwait(false);
            streams.Add(stream);
            return stream;
        }

        public void Dispose()
        {
            foreach (var stream in streams)
            {
                stream.Dispose();
            }
        }
    }
}