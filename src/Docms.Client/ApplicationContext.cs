using Docms.Client.Api;
using Docms.Client.FileWatching;
using Docms.Client.LocalStorage;
using Docms.Client.RemoteStorage;
using Docms.Client.Uploading;
using Microsoft.EntityFrameworkCore;
using NLog;
using System.Threading.Tasks;

namespace Docms.Client
{
    public class ApplicationContext : IApplicationContext
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly string _watchPath;
        private readonly string _localDbPath;
        private readonly string _serverUrl;
        private readonly string _uploadUserName;
        private readonly string _uploadUserPassword;
        private RemoteFileContext _db;

        public ApplicationContext(string watchPath, string localDbPath, string serverUrl, string uploadUserName, string uploadUserPassword)
        {
            _watchPath = watchPath;
            _localDbPath = localDbPath;
            _serverUrl = serverUrl;
            _uploadUserName = uploadUserName;
            _uploadUserPassword = uploadUserPassword;

            ApiClient = new DocmsApiClient(_serverUrl, "api/v1");
            LocalFileStorage = new LocalFileStorage(_watchPath);
            _db = new RemoteFileContext(new DbContextOptionsBuilder<RemoteFileContext>()
                .UseSqlite(string.Format("Data Source={0}", _localDbPath))
                .Options);
            RemoteFileStorage = new RemoteFileStorage(ApiClient, _db);
            Uploader = new LocalFileUploader(LocalFileStorage, RemoteFileStorage);
        }

        public IDocmsApiClient ApiClient { get; }
        public ILocalFileStorage LocalFileStorage { get; }
        public IRemoteFileStorage RemoteFileStorage { get; }
        public ILocalFileUploader Uploader { get; set; }

        public async Task InitializeAsync()
        {
            await _db.Database.EnsureCreatedAsync();
            await ApiClient.LoginAsync(_uploadUserName, _uploadUserPassword);
        }

        public void Dispose()
        {
            ApiClient.LogoutAsync().Wait();
        }
    }
}
