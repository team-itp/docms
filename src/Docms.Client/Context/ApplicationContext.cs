using System.Threading;

namespace Docms.Client
{
    public class ApplicationContext
    {
        public CancellationToken ApplicationShuttingDownToken { get; set; }
    }
}
