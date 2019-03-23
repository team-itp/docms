using Docms.Client.Api;
using Docms.Client.Data;
using Docms.Client.DocumentStores;
using Docms.Client.Tasks;

namespace Docms.Client
{
    public class ApplicationContext
    {
        public IApplication App { get; set; }
        public IDocmsApiClient Api { get; set; }
        public LocalDbContext Db { get; set; }
        public IDocumentStorage LocalStorage { get; set; }
        public IDocumentStorage RemoteStorage { get; set; }
        public ITask CurrentTask { get; set; }
    }
}
