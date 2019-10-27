using Docms.Queries.Clients;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Queries
{
    public class ClientsQueries : IClientsQueries
    {
        private readonly DocmsContext _context;

        public ClientsQueries(DocmsContext context)
        {
            _context = context;
        }

        public Task<ClientInfo> FindByIdAsync(string id)
        {
            return _context.ClientInfo.FindAsync(id);
        }
    }
}
