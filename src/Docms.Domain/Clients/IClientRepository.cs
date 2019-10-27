using Docms.Domain.SeedWork;
using System.Threading.Tasks;

namespace Docms.Domain.Clients
{
    public interface IClientRepository : IRepository<Client>
    {
        Task<Client> GetAsync(string clientId);
        Task AddAsync(Client device);
        Task UpdateAsync(Client device);
    }
}
