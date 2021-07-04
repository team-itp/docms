using Docms.Domain.Clients;
using Docms.Domain.SeedWork;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly DocmsContext _context;

        public IUnitOfWork UnitOfWork => _context;

        public ClientRepository(DocmsContext context)
        {
            _context = context;
        }

        public async Task<Client> GetAsync(string clientId)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(e => e.ClientId == clientId);
            return client;
        }

        public async Task AddAsync(Client client)
        {
            await _context.Clients.AddAsync(client);
        }

        public Task UpdateAsync(Client client)
        {
            _context.Clients.Update(client);
            return Task.CompletedTask;
        }
    }
}
