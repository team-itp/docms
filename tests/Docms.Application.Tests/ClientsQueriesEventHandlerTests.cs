using Docms.Application.DomainEventHandlers;
using Docms.Application.Tests.Utils;
using Docms.Domain.Clients;
using Docms.Domain.Clients.Events;
using Docms.Infrastructure.MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Application.Tests
{
    [TestClass]
    public class ClientsQueriesEventHandlerTests
    {
        private MockDocmsContext ctx;
        private ClientsQueriesEventHandler sut;
        private Client client;

        [TestInitialize]
        public async Task Setup()
        {
            ctx = new MockDocmsContext("ClientsQueriesEventHandlerTests");
            sut = new ClientsQueriesEventHandler(ctx);
            client = new Client("CLI1", "UPLOADER", "192.168.0.1");
            await PublishEvents(client);
        }

        [TestCleanup]
        public async Task Teardown()
        {
            ctx.ClientInfo.RemoveRange(ctx.ClientInfo);
            await ctx.SaveChangesAsync();
        }

        private async Task PublishEvents(Client client)
        {
            foreach (var ev in client.DomainEvents)
            {
                var dev = DomainEventNotification.Create(ev);
                var method = sut.GetType().GetMethod("Handle", new[] { dev.GetType(), typeof(CancellationToken) });
                var task = method?.Invoke(sut, new object[] { dev, default(CancellationToken) }) as Task;
                await task;
            }
            client.ClearDomainEvents();
        }

        [TestMethod]
        public async Task クライアントの登録イベントでクライアント情報を作成する()
        {
            var info = await ctx.ClientInfo.FirstOrDefaultAsync(f => f.ClientId == "CLI1");
            Assert.AreEqual("UPLOADER", info.Type);
            Assert.AreEqual("192.168.0.1", info.IpAddress);
        }

        [TestMethod]
        public async Task クライアントの要求イベントでクライアント情報を更新する()
        {
            client.Request(ClientRequestType.Start);
            await PublishEvents(client);

            var info = await ctx.ClientInfo.FirstOrDefaultAsync(f => f.ClientId == "CLI1");
            Assert.AreEqual(client.RequestId, info.RequestId);
            Assert.AreEqual("Start", info.RequestType);
        }

        [TestMethod]
        public async Task クライアントの受領イベントでクライアント情報を更新する()
        {
            client.Request(ClientRequestType.Start);
            client.Accept(client.RequestId);
            await PublishEvents(client);

            var info = await ctx.ClientInfo.FirstOrDefaultAsync(f => f.ClientId == "CLI1");
            Assert.AreEqual(client.RequestId, info.AcceptedRequestId);
            Assert.AreEqual("Start", info.AcceptedRequestType);
        }

        [TestMethod]
        public async Task クライアントのステータス更新イベントでクライアント情報を更新する()
        {
            client.UpdateStatus(ClientStatus.Running);
            await PublishEvents(client);

            var info = await ctx.ClientInfo.FirstOrDefaultAsync(f => f.ClientId == "CLI1");
            Assert.AreEqual("Running", info.Status);
        }

        [TestMethod]
        public async Task クライアントの最終アクセス時間更新イベントでクライアント情報を更新する()
        {
            var utcNow = DateTime.UtcNow;
            await sut.Handle(
                DomainEventNotification.Create(
                    new ClientLastAccessedTimeUpdatedEvent(client, client.ClientId, utcNow)
                    ) as DomainEventNotification<ClientLastAccessedTimeUpdatedEvent>);

            var info = await ctx.ClientInfo.FirstOrDefaultAsync(f => f.ClientId == "CLI1");
            Assert.AreEqual(utcNow, info.LastAccessedTime);
        }
    }
}
