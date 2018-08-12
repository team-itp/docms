﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.Api
{
    public interface IDocmsApiClient
    {
        Task CreateDocumentAsync(string path, Stream stream);
        Task UpdateDocumentAsync(string path, Stream stream);
        Task MoveDocumentAsync(string originalPath, string destinationPath);
        Task DeleteDocumentAsync(string path);
        Task<IEnumerable<Entry>> GetEntriesAsync(string path);
        Task<Document> GetDocumentAsync(string path);
        Task<Stream> DownloadAsync(string path);
        Task<IEnumerable<History>> GetHistoriesAsync(DateTimeOffset? lastSynced);
        Task LoginAsync(string username, string password);
        Task LogoutAsync();
        Task VerifyTokenAsync();
    }
}