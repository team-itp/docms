using Docms.Client.DocumentStores;
using Docms.Client.Synchronization;

namespace Docms.Client.Tests.Utils
{
    class MockApplicationContext : ApplicationContext
    {
        public MockApplication MockApp { get; set; }
        public MockDocmsApiClient MockApi { get; set; }
        public MockDocumentDbContext MockDocumentDb { get; set; }
        public MockFileSystem MockFileSystem { get; set; }
        public SynchronizationContext MockSynchronizationContext { get; set; }
        public LocalDocumentStorage MockLocalStorage { get; set; }
        public RemoteDocumentStorage MockRemoteStorage { get; set; }

        public MockApplicationContext()
        {
            MockApp = new MockApplication();
            MockApi = new MockDocmsApiClient();
            MockDocumentDb = new MockDocumentDbContext();
            MockFileSystem = new MockFileSystem();
            MockSynchronizationContext = new SynchronizationContext();
            MockLocalStorage = new LocalDocumentStorage(MockFileSystem, MockSynchronizationContext, MockDocumentDb);
            MockRemoteStorage = new RemoteDocumentStorage(MockApi, MockSynchronizationContext, MockDocumentDb);
            App = MockApp;
            Api = MockApi;
            DocumentDb = MockDocumentDb;
            FileSystem = MockFileSystem;
            SynchronizationContext = MockSynchronizationContext;
            LocalStorage = MockLocalStorage;
            RemoteStorage = MockRemoteStorage;
        }
    }
}
