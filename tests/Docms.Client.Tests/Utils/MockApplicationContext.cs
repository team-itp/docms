using Docms.Client.DocumentStores;
using Docms.Client.Synchronization;

namespace Docms.Client.Tests.Utils
{
    class MockApplicationContext : ApplicationContext
    {
        public MockDocmsApiClient MockApi { get; set; }
        public MockDocumentDbContextFactory MockDocumentDbFactory { get; set; }
        public MockFileSystem MockFileSystem { get; set; }
        public SynchronizationContext MockSynchronizationContext { get; set; }
        public LocalDocumentStorage MockLocalStorage { get; set; }
        public RemoteDocumentStorage MockRemoteStorage { get; set; }

        public MockApplicationContext()
        {
            MockApi = new MockDocmsApiClient();
            MockDocumentDbFactory = new MockDocumentDbContextFactory();
            MockFileSystem = new MockFileSystem();
            MockSynchronizationContext = new SynchronizationContext();
            MockLocalStorage = new LocalDocumentStorage(MockFileSystem, MockSynchronizationContext);
            MockRemoteStorage = new RemoteDocumentStorage(MockApi, MockSynchronizationContext, MockDocumentDbFactory);
            Api = MockApi;
            DbFactory = MockDocumentDbFactory;
            FileSystem = MockFileSystem;
            SynchronizationContext = MockSynchronizationContext;
            LocalStorage = MockLocalStorage;
            RemoteStorage = MockRemoteStorage;
        }
    }
}
