using Docms.Domain.SeedWork;
using System.Threading.Tasks;

namespace Docms.Domain.Clients
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<Client> FindAsync(string clientId);
        Task SaveAsync(Client device);
    }
}
