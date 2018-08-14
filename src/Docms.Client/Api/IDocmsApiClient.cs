using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.Api
{
    public interface IDocmsApiClient
    {
        Task CreateOrUpdateDocumentAsync(string path, Stream stream, DateTime? created, DateTime? lastModified);
        Task MoveDocumentAsync(string originalPath, string destinationPath);
        Task DeleteDocumentAsync(string path);
        Task<IEnumerable<Entry>> GetEntriesAsync(string path);
        Task<Document> GetDocumentAsync(string path);
        Task<Stream> DownloadAsync(string path);
        Task<IEnumerable<History>> GetHistoriesAsync(string path, DateTime? lastSynced = default(DateTime?));
        Task LoginAsync(string username, string password);
        Task LogoutAsync();
        Task VerifyTokenAsync();
    }
}
