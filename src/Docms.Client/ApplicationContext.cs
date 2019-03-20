using Docms.Client.Api;
using Docms.Client.Data;
using Docms.Client.DocumentStores;

namespace Docms.Client
{
    public class ApplicationContext
    {
        public IDocmsApiClient Api { get; set; }
        public LocalDbContext Db { get; set; }
        public LocalDocumentStorage LocalStorage { get; set; }
        public RemoteDocumentStorage RemoteStorage { get; set; }
    }
}
