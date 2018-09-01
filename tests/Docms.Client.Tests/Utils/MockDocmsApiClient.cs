using Docms.Client.Api;
using Docms.Client.Api.Responses;
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

        private void AddFile(string path, string contentType, byte[] data)
        {
            var dirPath = Path.GetDirectoryName(path);
            AddParentDirecotryEntries(dirPath);
            var hash = CalculateHash(data);
            entries.TryAdd(dirPath, new List<Entry>());
            var now = DateTime.UtcNow;
            entries[dirPath].Add(new Document(new DocumentResponse() { Path = path, ParentPath = Path.GetDirectoryName(path), ContentType = contentType, FileSize = data.Length, Hash = hash, LastModified = now }, this));
            streams.Add(path, data);
        }

        private void RemoveFile(string path)
        {
            var dirPath = Path.GetDirectoryName(path);
            var es = entries[dirPath];
            es.Remove(es.FirstOrDefault(e => e.Path == path));
            RemoveParentDirecotryEntriesIfEmpty(dirPath);
            streams.Remove(path);
        }

        private void AddParentDirecotryEntries(string dirPath)
        {
            var tmpPathSb = new StringBuilder();
            foreach (var pathComponent in dirPath.Split('/', '\\'))
            {
                var parentPath = tmpPathSb.ToString();
                var entryPath = tmpPathSb.Append(pathComponent).ToString();
                if (entries.TryGetValue(parentPath, out var es))
                {
                    if (!es.Any(e => e.Path == entryPath))
                    {
                        es.Add(new Container(new ContainerResponse() { Path = entryPath }, this));
                    }
                }
                else
                {
                    entries.Add(parentPath, new List<Entry>() { new Container(new ContainerResponse() { Path = entryPath, ParentPath = parentPath }, this) });
                }
                tmpPathSb.Append('/');
            }
        }

        private void RemoveParentDirecotryEntriesIfEmpty(string dirPath)
        {
            var tmpPathSb = new StringBuilder();
            foreach (var pathComponent in dirPath.Split('/', '\\'))
            {
                var parentPath = tmpPathSb.ToString();
                var entryPath = tmpPathSb.Append(pathComponent).ToString();
                if (entries.TryGetValue(parentPath, out var es))
                {
                    if (!es.Any())
                    {
                        entries.Remove(parentPath);
                    }
                }
                tmpPathSb.Append('/');
            }
        }

        private string CalculateHash(byte[] data)
        {
            using (var sha1 = SHA1.Create())
            {
                var hashBin = sha1.ComputeHash(data);
                return BitConverter.ToString(hashBin).Replace("-", "");
            }
        }

        private void AddHisotry(History history)
        {
            history.Id = Guid.NewGuid();
            history.Timestamp = DateTime.UtcNow;

            if (!histories.TryGetValue(history.Path, out var historiesOfPath))
            {
                historiesOfPath = new List<History>();
                histories.Add(history.Path, historiesOfPath);
            }
            historiesOfPath.Add(history);
        }

        private void AddCreated(string path, string contentType, byte[] data)
        {
            var now = DateTime.UtcNow;
            AddHisotry(new DocumentCreatedHistory()
            {
                Path = path,
                ContentType = contentType,
                Hash = CalculateHash(data),
                FileSize = data.Length,
                Created = now,
                LastModified = now
            });
        }

        private void AddUpdated(string path, string contentType, byte[] data)
        {
            var now = DateTime.UtcNow;
            AddHisotry(new DocumentUpdatedHistory()
            {
                Path = path,
                ContentType = contentType,
                Hash = CalculateHash(data),
                FileSize = data.Length,
                Created = now,
                LastModified = now
            });
        }

        private void AddMove(string originalPath, string destinationPath, string contentType, byte[] data)
        {
            var now = DateTime.UtcNow;
            AddHisotry(new DocumentMovedFromHistory()
            {
                Path = destinationPath,
                OldPath = originalPath,
                ContentType = contentType,
                Hash = CalculateHash(data),
                FileSize = data.Length,
                Created = now,
                LastModified = now
            });
            AddHisotry(new DocumentMovedToHistory()
            {
                Path = originalPath,
                NewPath = destinationPath,
            });
        }

        private void AddDelete(string path)
        {
            var now = DateTime.UtcNow;
            AddHisotry(new DocumentDeletedHistory()
            {
                Path = path,
            });
        }

        public async Task CreateOrUpdateDocumentAsync(string path, Stream stream, DateTime? created = null, DateTime? lastModified = null)
        {
            var es = entries.FirstOrDefault(kv => kv.Value.Any(e => e.Path == path));
            if (es.Key != null)
            {
                RemoveFile(path);
                using (var ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms).ConfigureAwait(false);
                    ms.Seek(0, SeekOrigin.Begin);
                    AddFile(path, "application/octet-stream", ms.ToArray());
                    AddUpdated(path, "application/octet-stream", ms.ToArray());
                }
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    await stream.CopyToAsync(ms).ConfigureAwait(false);
                    ms.Seek(0, SeekOrigin.Begin);
                    AddFile(path, "application/octet-stream", ms.ToArray());
                    AddCreated(path, "application/octet-stream", ms.ToArray());
                }
            }
        }

        public Task MoveDocumentAsync(string originalPath, string destinationPath)
        {
            var data = streams[originalPath];
            RemoveFile(originalPath);
            AddFile(destinationPath, "appliction/octet-stream", data);
            AddMove(originalPath, destinationPath, "appliction/octet-stream", data);
            return Task.CompletedTask;
        }

        public Task DeleteDocumentAsync(string path)
        {
            var es = entries.FirstOrDefault(kv => kv.Value.Any(e => e.Path == path));
            if (es.Key != null)
            {
                es.Value.Remove(es.Value.FirstOrDefault(e => e.Path == path));
                streams.Remove(path);
                RemoveParentDirecotryEntriesIfEmpty(es.Key);
                AddDelete(path);
            }
            return Task.CompletedTask;
        }

        public Task<Stream> DownloadAsync(string path)
        {
            if (streams.TryGetValue(path, out var data))
            {
                return Task.FromResult(new MemoryStream(data) as Stream);
            }
            throw new InvalidOperationException();
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
            var historiesKvs = histories.Where(key => string.IsNullOrEmpty(path) || key.Key.StartsWith(path));
            var historyValues = historiesKvs.SelectMany(kv => kv.Value);
            if (lastSynced != null)
            {
                historyValues = historyValues.Where(h => h.Timestamp > lastSynced.Value);
            }
            return Task.FromResult(historyValues);
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
