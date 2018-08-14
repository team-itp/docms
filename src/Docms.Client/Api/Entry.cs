using Docms.Client.Api.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.Api
{
    public abstract class Entry
    {
        public Entry(EntryResponse entry, IDocmsApiClient client)
        {
            Client = client;
            Path = entry.Path;
            ParentPath = entry.ParentPath;
        }

        protected IDocmsApiClient Client { get; }

        public string Path { get; }
        public string ParentPath { get; }
    }

    public class Container : Entry
    {
        public Container(ContainerResponse entry, IDocmsApiClient client) : base(entry, client)
        {
        }

        public Task<IEnumerable<Entry>> GetEntriesAsync()
        {
            return Client.GetEntriesAsync(Path);
        }
    }

    public class Document : Entry
    {
        public Document(DocumentResponse entry, IDocmsApiClient client) : base(entry, client)
        {
            ContentType = entry.ContentType;
            Hash = entry.Hash;
            LastModified = entry.LastModified;
        }

        public string ContentType { get; }
        public string Hash { get; }
        public DateTime LastModified { get; }

        public async Task<Stream> OpenStreamAsync()
        {
            return await Client.DownloadAsync(Path);
        }
    }
}
