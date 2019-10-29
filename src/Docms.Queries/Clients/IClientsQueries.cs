using System.Linq;
using System.Threading.Tasks;

namespace Docms.Queries.Clients
{
    public interface IClientsQueries
    {
        Task<ClientInfo> FindByIdAsync(string id);
        IQueryable<ClientInfo> GetClients();
    }
}
