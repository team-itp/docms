using Docms.Application.Commands;
using Docms.Application.Tests.Utils;
using Docms.Domain.Clients;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Docms.Application.Tests
{
    [TestClass]
    public class ClientCommandTests
    {
        private MockClientRepository repository;
        private ClientCommandHandler sut;
        private string clientId;
        private DateTime lastAccessedTime;

        [TestInitialize]
        public async Task Setup()
        {
            repository = new MockClientRepository();
            sut = new ClientCommandHandler(repository);

            clientId = Guid.NewGuid().ToString();
            await sut.Handle(new RegisterClientCommand()
            {
                ClientId = clientId,
                Type = "UPLOADER",
                IpAddress = "192.168.10.1"
            });

            var client = await repository.GetAsync(clientId);
            lastAccessedTime = client.LastAccessedTime.Value;
        }

        [TestMethod]
        public async Task クライアントを作成する()
        {
            var client = await repository.GetAsync(clientId);
            Assert.IsNotNull(client);
            Assert.IsNull(client.RequestId);
            Assert.IsNotNull(client.LastAccessedTime);
        }

        [TestMethod]
        public async Task クライアントに要求を出す()
        {
            await sut.Handle(new RequestToClientCommand()
            {
                ClientId = clientId,
                RequestType = ClientRequestType.Start
            });

            var client = await repository.GetAsync(clientId);
            Assert.IsNotNull(client.RequestId);
            Assert.AreEqual(ClientRequestType.Start, client.RequestType);
            Assert.IsTrue(client.LastAccessedTime == lastAccessedTime);
        }

        [TestMethod]
        public async Task クライアントが要求を承認する()
        {
            await sut.Handle(new RequestToClientCommand()
            {
                ClientId = clientId,
                RequestType = ClientRequestType.Start
            });

            var client = await repository.GetAsync(clientId);

            await sut.Handle(new AcceptRequestFromUserCommand()
            {
                ClientId = clientId,
                RequestId = client.RequestId
            });

            Assert.IsTrue(client.IsAccepted);
            Assert.IsTrue(client.LastAccessedTime > lastAccessedTime);
        }

        [TestMethod]
        public async Task クライアントがステータスを送信する()
        {
            await sut.Handle(new UpdateClientStatusCommand()
            {
                ClientId = clientId,
                ClientStatus = ClientStatus.Running
            });

            var client = await repository.GetAsync(clientId);
            Assert.AreEqual(ClientStatus.Running, client.Status);
            Assert.IsTrue(client.LastAccessedTime > lastAccessedTime);
        }
    }
}
