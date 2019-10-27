using Docms.Domain.Clients;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Application.Commands
{
    public class RegisterClientCommandHandler : IRequestHandler<RegisterClientCommand, bool>
    {
        private readonly IClientRepository _clientRepository;

        public RegisterClientCommandHandler(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public async Task<bool> Handle(RegisterClientCommand request, CancellationToken cancellationToken = default)
        {
            var client = await _clientRepository.GetAsync(request.ClientId);
            if (client != null)
            {
                throw new InvalidOperationException();
            }

            client = new Client(request.ClientId, request.Type, request.IpAddress);
            await _clientRepository.AddAsync(client);
            await _clientRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            return true;
        }
    }
}
