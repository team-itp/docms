using Docms.Client.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Utils
{
    class MockDocmsApiClient : IDocmsApiClient
    {
        public Dictionary<string, List<Entry>> entries = new Dictionary<string, List<Entry>>();
        public Dictionary<string, byte[]> streams = new Dictionary<string, byte[]>();
        public Dictionary<string, List<History>> histories = new Dictionary<string, List<History>>();

        internal void AddFile(string path, string contentType, byte[] data)
        {
            var dirPath = Path.GetDirectoryName(path);
            var tmpPathSb = new StringBuilder();
            foreach (var pathComponent in dirPath.Split('/', '\\'))
            {
                var parentPath = tmpPathSb.ToString();
                var entryPath = tmpPathSb.Append(pathComponent).ToString();
                if (entries.TryGetValue(parentPath, out var es))
                {
                    if (!es.Any(e => e.Path == entryPath))
                    {
                        es.Add(new Container(entryPath, this));
                    }
                }
                else
                {
                    entries.Add(parentPath, new List<Entry>() { new Container(entryPath, this) });
                }
                tmpPathSb.Append('/');
            }
            var hash = CalculateHash(data);
            entries.TryAdd(dirPath, new List<Entry>());
            var now = DateTime.UtcNow;
            entries[dirPath].Add(new Document(path, contentType, hash, now, now, this));
            streams.Add(path, data);
            histories.Add(path, new List<History>() { new DocumentCreated() { Id = Guid.NewGuid(), Timestamp = DateTime.UtcNow, Path = path, ContentType = contentType, FileSize = data.Length, Hash = hash, Created = now, LastModified = now } });
        }

        private string CalculateHash(byte[] data)
        {
            using (var sha1 = SHA1.Create())
            {
                var hashBin = sha1.ComputeHash(data);
                return BitConverter.ToString(hashBin).Replace("-", "");
            }
        }

        public async Task CreateDocumentAsync(string path, Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);
                AddFile(path, "application/octet-stream", ms.ToArray());
            }
        }

        public async Task UpdateDocumentAsync(string path, Stream stream)
        {
            await DeleteDocumentAsync(path);
            await CreateDocumentAsync(path, stream);
        }

        public Task MoveDocumentAsync(string originalPath, string destinationPath)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDocumentAsync(string path)
        {
            var es = entries.FirstOrDefault(kv => kv.Value.Any(e => e.Path == path));
            if (es.Key != null)
            {
                es.Value.Remove(es.Value.FirstOrDefault(e => e.Path == path));
                streams.Remove(path);
                histories.Remove(path);
            }
            return Task.CompletedTask;
        }

        public Task<Stream> DownloadAsync(string path)
        {
            if (streams.TryGetValue(path, out var data))
            {
                return Task.FromResult(new MemoryStream(data) as Stream);
            }
            return Task.FromResult(default(Stream));
        }

        public Task<Document> GetDocumentAsync(string path)
        {
            var es = entries.FirstOrDefault(kv => kv.Value.Any(e => e.Path == path));
            if (es.Key != null)
            {
                return Task.FromResult(es.Value.FirstOrDefault(e => e.Path == path) as Document);
            }
            return Task.FromResult(default(Document));
        }

        public Task<IEnumerable<Entry>> GetEntriesAsync(string path)
        {
            return Task.FromResult(entries.TryGetValue(path, out var es) ? es : Array.Empty<Entry>() as IEnumerable<Entry>);
        }

        public Task<IEnumerable<History>> GetHistoriesAsync(string path, DateTime? lastSynced)
        {
            return Task.FromResult(histories[path] as IEnumerable<History>);
        }

        public Task LoginAsync(string username, string password)
        {
            throw new NotImplementedException();
        }

        public Task LogoutAsync()
        {
            throw new NotImplementedException();
        }

        public Task VerifyTokenAsync()
        {
            throw new NotImplementedException();
        }
    }
}
