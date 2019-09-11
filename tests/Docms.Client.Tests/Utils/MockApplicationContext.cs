using Docms.Client.DocumentStores;
using Docms.Client.Synchronization;

namespace Docms.Client.Tests.Utils
{
    class MockApplicationContext : ApplicationContext
    {
        public MockDocmsApiClient MockApi { get; set; }
        public MockDocumentDbContext MockDocumentDb { get; set; }
        public MockFileSystem MockFileSystem { get; set; }
        public SynchronizationContext MockSynchronizationContext { get; set; }
        public LocalDocumentStorage MockLocalStorage { get; set; }
        public RemoteDocumentStorage MockRemoteStorage { get; set; }

        public MockApplicationContext()
        {
            MockApi = new MockDocmsApiClient();
            MockDocumentDb = new MockDocumentDbContext();
            MockFileSystem = new MockFileSystem();
            MockSynchronizationContext = new SynchronizationContext();
            MockLocalStorage = new LocalDocumentStorage(MockFileSystem, MockSynchronizationContext);
            MockRemoteStorage = new RemoteDocumentStorage(MockApi, MockSynchronizationContext, MockDocumentDb);
            Api = MockApi;
            DocumentDb = MockDocumentDb;
            FileSystem = MockFileSystem;
            SynchronizationContext = MockSynchronizationContext;
            LocalStorage = MockLocalStorage;
            RemoteStorage = MockRemoteStorage;
        }
    }
}
