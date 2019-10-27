using Docms.Application.Commands;
using Docms.Application.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Docms.Application.Tests
{
    [TestClass]
    public class ClientCommandTests
    {
        private MockClientRepository repository;
        private RegisterClientCommandHandler sut;

        [TestInitialize]
        public void Setup()
        {
            repository = new MockClientRepository();
            sut = new RegisterClientCommandHandler(repository);
        }

        [TestMethod]
        public async Task クライアントを作成する()
        {
            var clientId = Guid.NewGuid().ToString();
            await sut.Handle(new RegisterClientCommand()
            {
                ClientId = clientId,
                Type = "UPLOADER",
                IpAddress = "192.168.10.1"
            });

            var client = await repository.GetAsync(clientId);
            Assert.IsNotNull(client);
        }
    }
}
