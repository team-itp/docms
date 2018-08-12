using Docms.Client.Api;
using Docms.Client.FileStorage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Docms.Client.FileSyncing
{
    public class Initializer
    {
        private IDocmsApiClient _client;
        private ILocalFileStorage _storage;
        private List<Document> _errors = new List<Document>();

        public Initializer(IDocmsApiClient client, ILocalFileStorage storage)
        {
            _client = client;
            _storage = storage;
        }

        public async Task InitializeAsync()
        {
            await DownloadFiles("");
        }

        private async Task DownloadFiles(string path)
        {
            foreach (var item in await _client.GetEntriesAsync(path))
            {
                var doc = item as Document;
                if (doc != null)
                {
                    try
                    {
                        await _storage.Create(doc.Path, await doc.OpenStreamAsync(), doc.Created, doc.LastModified);
                    }
                    catch (Exception)
                    {
                        _errors.Add(doc);
                    }
                }

                var con = item as Container;
                if (con != null)
                {
                    await DownloadFiles(con.Path);
                }
            }
        }
    }
}
