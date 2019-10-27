using Docms.Domain.Clients;
using Docms.Infrastructure.Repositories;
using Docms.Infrastructure.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Tests
{
    [TestClass]
    public class ClientRepositoryTests
    {
        [TestMethod]
        public async Task クライアントがClientIdで取得できること()
        {
            var ctx = new MockDocmsContext("MockDocmsContext");
            var sut = new ClientRepository(ctx);
            var d1 = new Client("ABC", "UPLOADER", "192.168.0.1");
            d1.Request(ClientRequestType.Start);
            await sut.AddAsync(d1);
            await sut.UnitOfWork.SaveEntitiesAsync();

            var d2 = await sut.GetAsync("ABC");
            Assert.AreEqual("ABC", d2.ClientId);
            Assert.AreEqual("UPLOADER", d2.Type);
            Assert.AreEqual("192.168.0.1", d2.IpAddress);
            Assert.AreEqual(ClientStatus.Stopped, d2.Status);
            Assert.AreEqual(ClientRequestType.Start, d2.RequestType);
        }
    }
}
