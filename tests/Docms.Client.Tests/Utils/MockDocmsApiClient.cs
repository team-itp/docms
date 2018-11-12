using Docms.Client.Api;
using Docms.Client.Api.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Utils
{
    class MockDocmsApiClient : IDocmsApiClient
    {
        public Dictionary<string, Document> entries = new Dictionary<string, Document>();
        public Dictionary<string, byte[]> streams = new Dictionary<string, byte[]>();
        public Dictionary<string, List<History>> histories = new Dictionary<string, List<History>>();

        private void AddFile(string path, string contentType, byte[] data, DateTime created, DateTime lastModified)
        {
            var hash = CalculateHash(data);
            var parentPath = Path.GetDirectoryName(path);
            var document = new Document(
                new DocumentResponse()
                {
                    Path = path,
                    ParentPath = parentPath,
                    ContentType = contentType,
                    FileSize = data.Length,
                    Hash = hash,
                    Created = created,
                    LastModified = lastModified
                }, this);

            if (!entries.TryAdd(path, document))
            {
                throw new ServerException(400, "Bad Request");
            }
            streams.Add(path, data);
        }

        private void RemoveFile(string path)
        {
            if (!entries.TryGetValue(path, out var entry))
            {
                throw new ServerException(400, "Bad Request");
            }
            entries.Remove(path);
            streams.Remove(path);
        }

        private string CalculateHash(byte[] data)
        {
            using (var sha1 = SHA1.Create())
            {
                var hashBin = sha1.ComputeHash(data);
                return BitConverter.ToString(hashBin).Replace("-", "");
            }
        }

        private ContainerResponse GetContainerFromPath(string path)
        {
            return new ContainerResponse()
            {
                Path = path,
                ParentPath = string.IsNullOrEmpty(path)
                    ? null
                    : Path.GetDirectoryName(path),
                Name = Path.GetFileName(path),
                Entries = new List<EntryResponse>()
            };
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

        private void AddCreated(string path, string contentType, byte[] data, DateTime created, DateTime lastModified)
        {
            AddHisotry(new DocumentCreatedHistory()
            {
                Path = path,
                ContentType = contentType,
                Hash = CalculateHash(data),
                FileSize = data.Length,
                Created = created,
                LastModified = lastModified
            });
        }

        private void AddUpdated(string path, string contentType, byte[] data, DateTime created, DateTime lastModified)
        {
            AddHisotry(new DocumentUpdatedHistory()
            {
                Path = path,
                ContentType = contentType,
                Hash = CalculateHash(data),
                FileSize = data.Length,
                Created = created,
                LastModified = lastModified
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

        public Task CreateOrUpdateDocumentAsync(string path, Stream stream, DateTime? created = null, DateTime? lastModified = null)
        {
            if (entries.TryGetValue(path, out var entry))
            {
                RemoveFile(path);
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var now = DateTime.UtcNow;
                    AddFile(path, "application/octet-stream", ms.ToArray(), created ?? now, lastModified ?? now);
                    AddUpdated(path, "application/octet-stream", ms.ToArray(), created ?? now, lastModified ?? now);
                }
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var now = DateTime.UtcNow;
                    AddFile(path, "application/octet-stream", ms.ToArray(), created ?? now, lastModified ?? now);
                    AddCreated(path, "application/octet-stream", ms.ToArray(), created ?? now, lastModified ?? now);
                }
            }
            return Task.CompletedTask;
        }

        public Task MoveDocumentAsync(string originalPath, string destinationPath)
        {
            if (!entries.TryGetValue(originalPath, out var entry))
            {
                throw new ServerException(400, "Bad Request");
            }
            var data = streams[originalPath];
            RemoveFile(originalPath);
            AddFile(destinationPath, entry.ContentType, data, entry.Created, entry.LastModified);
            AddMove(originalPath, destinationPath, entry.ContentType, data);
            return Task.CompletedTask;
        }

        public Task DeleteDocumentAsync(string path)
        {
            if (!entries.TryGetValue(path, out var entry))
            {
                throw new ServerException(400, "Bad Request");
            }
            RemoveFile(path);
            AddDelete(path);
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
            entries.TryGetValue(path, out var entry);
            return Task.FromResult(entry);
        }

        public Task<IEnumerable<Entry>> GetEntriesAsync(string path)
        {
            var entriesUnderPath = entries.Values
                .Where(e => Regex.IsMatch(e.ParentPath, string.IsNullOrEmpty(path) ? "^[^/]+" : $"^{path}(/[^/]+)?$"))
                .ToArray();
            var documentsInPath = entriesUnderPath
                .Where(e => e.ParentPath == path)
                .OrderBy(e => e.Path);
            var entriesInPath = entriesUnderPath
                .Where(e => e.ParentPath != path)
                .Select(e => e.ParentPath)
                .Distinct()
                .OrderBy(e => e)
                .Select(GetContainerFromPath)
                .Select(e => new Container(e, this))
                .ToList<Entry>();
            entriesInPath.AddRange(documentsInPath);
            return Task.FromResult(entriesInPath.AsEnumerable());
        }

        public Task<IEnumerable<History>> GetHistoriesAsync(string path, DateTime? lastSynced)
        {
            var historiesKvs = histories.Where(key => string.IsNullOrEmpty(path) || key.Key.StartsWith(path));
            var historyValues = historiesKvs.SelectMany(kv => kv.Value);
            if (lastSynced != null)
            {
                historyValues = historyValues.Where(h => h.Timestamp > lastSynced.Value);
            }
            historyValues = historyValues.OrderBy(e => e.Timestamp);
            return Task.FromResult(historyValues.ToArray() as IEnumerable<History>);
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
