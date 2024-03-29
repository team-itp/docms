﻿using Docms.Queries.Clients;
using System.Linq;
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

        public async Task<ClientInfo> FindByIdAsync(string id)
        {
            return await _context.ClientInfo.FindAsync(id);
        }

        public IQueryable<ClientInfo> GetClients()
        {
            return _context.ClientInfo;
        }
    }
}
