using Docms.Domain.Clients;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Application.Tests.Utils
{
    class MockClientRepository : MockRepository<Client>, IClientRepository
    {
        public Task<Client> GetAsync(string clientId)
        {
            return Task.FromResult(Entities.Where(e => e.ClientId == clientId).FirstOrDefault());
        }
    }
}
