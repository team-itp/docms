namespace Docms.Client.Tests.Utils
{
    class MockApplicationContext : ApplicationContext
    {
        public MockApplication MockApp { get; set; }
        public MockDocmsApiClient MockApi { get; set; }
        public MockLocalDbContext MockDb { get; set; }
        public MockDocumentStorage MockLocalStorage { get; set; }
        public MockDocumentStorage MockRemoteStorage { get; set; }
        public MockTask MockCurrentTask { get; set; }

        public MockApplicationContext()
        {
            MockApp = new MockApplication();
            MockApi = new MockDocmsApiClient();
            MockDb = new MockLocalDbContext();
            MockLocalStorage = new MockDocumentStorage();
            MockRemoteStorage = new MockDocumentStorage();
            MockCurrentTask = new MockTask();
            App = MockApp;
            Api = MockApi;
            Db = MockDb;
            LocalStorage = MockLocalStorage;
            RemoteStorage = MockRemoteStorage;
            CurrentTask = MockCurrentTask;
        }
    }
}
