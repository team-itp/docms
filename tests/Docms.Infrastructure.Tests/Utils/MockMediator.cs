using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Tests.Utils
{
    class MockMediator : IMediator
    {
        public List<INotification> Notifications { get; } = new List<INotification>();
        public List<object> Requests { get; } = new List<object>();

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            Notifications.Add(notification as INotification);
            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            Notifications.Add(notification);
            return Task.CompletedTask;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult(default(TResponse));
        }

        public Task<object> Send(object request, CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            return Task.FromResult<object>(default);
        }
    }
}
