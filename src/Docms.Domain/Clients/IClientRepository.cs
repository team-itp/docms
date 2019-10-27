using System.Threading.Tasks;

namespace Docms.Domain.Clients
{
    public interface IClientRepository
    {
        Task<Client> FindAsync(string clientId);
        Task SaveAsync(Client device);
    }
}
