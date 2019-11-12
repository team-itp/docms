using Docms.Infrastructure.Queries;
using Docms.Infrastructure.Tests.Utils;
using Docms.Queries.Clients;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Tests
{
    [TestClass]
    public class ClientsQueriesTests
    {
        [TestMethod]
        public async Task クライアントの一覧を取得できる()
        {
            var context = new MockDocmsContext("ClientsQueriesTests");
            var sut = new ClientsQueries(context);
            context.ClientInfo.Add(new ClientInfo()
            {
                ClientId = "client1",
                Type = "UPLOADER",
                IpAddress = "192.168.10.1",
            });
            context.ClientInfo.Add(new ClientInfo()
            {
                ClientId = "client2",
                Type = "UPLOADER",
                IpAddress = "192.168.10.2",
            });
            await context.SaveChangesAsync();
            var clients = await sut.GetClients().ToListAsync();
            Assert.AreEqual(2, clients.Count());
        }
    }
}
