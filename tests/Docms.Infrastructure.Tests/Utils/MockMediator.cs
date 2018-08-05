using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Docms.Infrastructure.Tests.Utils
{
    public class MockMediator : IMediator
    {
        public List<INotification> Notifications { get; } = new List<INotification>();
        public List<IBaseRequest> Requests { get; } = new List<IBaseRequest>();

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default(CancellationToken)) where TNotification : INotification
        {
            Notifications.Add(notification);
            return Task.CompletedTask;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default(CancellationToken))
        {
            Requests.Add(request);
            return Task.FromResult(default(TResponse));
        }
    }
}
