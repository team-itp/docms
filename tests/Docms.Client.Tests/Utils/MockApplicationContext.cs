using Docms.Client.DocumentStores;

namespace Docms.Client.Tests.Utils
{
    class MockApplicationContext : ApplicationContext
    {
        public MockApplication MockApp { get; set; }
        public MockDocmsApiClient MockApi { get; set; }
        public MockFileSystem MockFileSystem { get; set; }
        public MockLocalDbContext MockDb { get; set; }
        public LocalDocumentStorage MockLocalStorage { get; set; }
        public RemoteDocumentStorage MockRemoteStorage { get; set; }
        public MockTask MockCurrentTask { get; set; }

        public MockApplicationContext()
        {
            MockApp = new MockApplication();
            MockApi = new MockDocmsApiClient();
            MockFileSystem = new MockFileSystem();
            MockDb = new MockLocalDbContext();
            MockLocalStorage = new LocalDocumentStorage(MockFileSystem, MockDb);
            MockRemoteStorage = new RemoteDocumentStorage(MockApi, MockDb);
            MockCurrentTask = new MockTask();
            App = MockApp;
            Api = MockApi;
            FileSystem = MockFileSystem;
            Db = MockDb;
            LocalStorage = MockLocalStorage;
            RemoteStorage = MockRemoteStorage;
            CurrentTask = MockCurrentTask;
        }
    }
}
