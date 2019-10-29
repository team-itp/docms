using Docms.Client.Api.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.Api
{
    public interface IDocmsApiClient
    {
        Task PostRegisterClient(string clientId, string type);
        Task<ClientInfoResponse> GetClientInfoAsync(string clientId);
        Task<ClientInfoRequstResponse> GetLatestRequest(string clientId);
        Task PutAccepted(string clientId, string requestId);
        Task PutStatus(string clientId, string status);

        Task CreateOrUpdateDocumentAsync(string path, Func<Stream> streamFactory, DateTime? created, DateTime? lastModified);
        Task MoveDocumentAsync(string originalPath, string destinationPath);
        Task DeleteDocumentAsync(string path);
        Task<IEnumerable<Entry>> GetEntriesAsync(string path);
        Task<Document> GetDocumentAsync(string path);
        Task<Stream> DownloadAsync(string path);
        Task<IEnumerable<History>> GetHistoriesAsync(string path, Guid? latestHistoryId = default);
        Task LoginAsync(string username, string password);
        Task LogoutAsync();
    }
}
