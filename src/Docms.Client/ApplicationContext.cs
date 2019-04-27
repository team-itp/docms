using Docms.Client.Api;
using Docms.Client.Data;
using Docms.Client.DocumentStores;
using Docms.Client.FileSystem;
using Docms.Client.Operations;
using Docms.Client.Tasks;
using System;

namespace Docms.Client
{
    public class ApplicationContext : IDisposable
    {
        public IApplication App { get; set; }
        public IDocmsApiClient Api { get; set; }
        public DocumentDbContext DocumentDb { get; set; }
        public IFileSystem FileSystem { get; set; }
        public IDocumentStorage LocalStorage { get; set; }
        public IDocumentStorage RemoteStorage { get; set; }
        public ITask CurrentTask { get; set; }
        public IResourceOperationDispatcher<SyncHistoryDbContext> SyncHistoryDbDispatcher { get; set; }

        public void Dispose()
        {
            DocumentDb.Dispose();
            SyncHistoryDbDispatcher.Dispose();
        }
    }
}
