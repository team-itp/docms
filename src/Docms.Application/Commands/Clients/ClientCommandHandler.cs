using Docms.Domain.Clients;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Application.Commands
{
    public class ClientCommandHandler : 
        IRequestHandler<RegisterClientCommand, bool>,
        IRequestHandler<RequestToClientCommand, bool>,
        IRequestHandler<AcceptRequestFromUserCommand, bool>,
        IRequestHandler<UpdateClientStatusCommand, bool>
    {
        private readonly IClientRepository _clientRepository;

        public ClientCommandHandler(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public async Task<bool> Handle(RegisterClientCommand request, CancellationToken cancellationToken = default)
        {
            var client = await _clientRepository.GetAsync(request.ClientId).ConfigureAwait(false);
            if (client != null)
            {
                return false;
            }

            client = new Client(request.ClientId, request.Type, request.IpAddress);
            client.UpdateLastAccessTime();

            await _clientRepository.AddAsync(client).ConfigureAwait(false);
            await _clientRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        public async Task<bool> Handle(RequestToClientCommand request, CancellationToken cancellationToken = default)
        {
            var client = await _clientRepository.GetAsync(request.ClientId).ConfigureAwait(false);
            if (client == null)
            {
                throw new InvalidOperationException();
            }

            client.Request(request.RequestType);

            await _clientRepository.UpdateAsync(client).ConfigureAwait(false);
            await _clientRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        public async Task<bool> Handle(AcceptRequestFromUserCommand request, CancellationToken cancellationToken = default)
        {
            var client = await _clientRepository.GetAsync(request.ClientId).ConfigureAwait(false);
            if (client == null)
            {
                throw new InvalidOperationException();
            }

            client.Accept(request.RequestId);
            client.UpdateLastAccessTime();

            await _clientRepository.UpdateAsync(client).ConfigureAwait(false);
            await _clientRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }

        public async Task<bool> Handle(UpdateClientStatusCommand request, CancellationToken cancellationToken = default)
        {
            var client = await _clientRepository.GetAsync(request.ClientId).ConfigureAwait(false);
            if (client == null)
            {
                throw new InvalidOperationException();
            }

            client.UpdateStatus(request.ClientStatus);
            client.UpdateLastAccessTime();

            await _clientRepository.UpdateAsync(client).ConfigureAwait(false);
            await _clientRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken).ConfigureAwait(false);
            return true;
        }
    }
}
