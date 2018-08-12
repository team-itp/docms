using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.Api
{
    public abstract class Entry
    {
        public Entry(string path, IDocmsApiClient client)
        {
            Client = client;
            Path = path;
        }

        protected IDocmsApiClient Client { get; }

        public string Path { get; }
    }

    public class Container : Entry
    {
        public Container(string path, IDocmsApiClient client) : base(path, client)
        {
        }

        public async Task<IEnumerable<Entry>> GetEntriesAsync()
        {
            return await Client.GetEntriesAsync(Path);
        }
    }

    public class Document : Entry
    {
        public Document(string path, string contentType, string hash, DateTimeOffset created, DateTimeOffset lastModified, IDocmsApiClient client) : base(path, client)
        {
            ContentType = contentType;
            Hash = hash;
            Created = created;
            LastModified = lastModified;
        }

        public string ContentType { get; }
        public string Hash { get; }
        public DateTimeOffset Created { get; }
        public DateTimeOffset LastModified { get; }

        public async Task<Stream> OpenStreamAsync()
        {
            return await Client.DownloadAsync(Path);
        }
    }
}
