using Docms.Client.Data;
using Docms.Client.DocumentStores;
using Docms.Client.Operations;
using Docms.Client.Syncing;

namespace Docms.Client.Tests.Utils
{
    class MockApplicationContext : ApplicationContext
    {
        public MockApplication MockApp { get; set; }
        public MockDocmsApiClient MockApi { get; set; }
        public MockFileSystem MockFileSystem { get; set; }
        public MockDocumentDbContext MockDocumentDb { get; set; }
        public MockSyncHistoryDbContext MockSyncHistoryDb { get; set; }
        public ResourceOperationDispatcher<SyncHistoryDbContext> MockSyncHistoryDbDispatcher { get; set; }
        public SyncManager MockSyncManager { get; set; }
        public LocalDocumentStorage MockLocalStorage { get; set; }
        public RemoteDocumentStorage MockRemoteStorage { get; set; }
        public MockTask MockCurrentTask { get; set; }

        public MockApplicationContext()
        {
            MockApp = new MockApplication();
            MockApi = new MockDocmsApiClient();
            MockFileSystem = new MockFileSystem();
            MockDocumentDb = new MockDocumentDbContext();
            MockSyncHistoryDb = new MockSyncHistoryDbContext();
            MockSyncHistoryDbDispatcher = new ResourceOperationDispatcher<SyncHistoryDbContext>(MockSyncHistoryDb);
            MockSyncManager = new SyncManager(MockSyncHistoryDbDispatcher);
            MockLocalStorage = new LocalDocumentStorage(MockFileSystem, MockDocumentDb);
            MockRemoteStorage = new RemoteDocumentStorage(MockApi, MockDocumentDb);
            MockCurrentTask = new MockTask();
            App = MockApp;
            Api = MockApi;
            FileSystem = MockFileSystem;
            DocumentDb = MockDocumentDb;
            SyncHistoryDbDispatcher = MockSyncHistoryDbDispatcher;
            SyncManager = MockSyncManager;
            LocalStorage = MockLocalStorage;
            RemoteStorage = MockRemoteStorage;
            CurrentTask = MockCurrentTask;
        }
    }
}
