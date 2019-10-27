using Docms.Application.DomainEventHandlers;
using Docms.Application.Tests.Utils;
using Docms.Domain.Identity;
using Docms.Domain.Identity.Events;
using Docms.Infrastructure.MediatR;
using Docms.Queries.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Application.Tests
{
    [TestClass]
    public class DeviceGrantsQueriesEventHandlerTests
    {
        private MockDocmsContext ctx;
        private MockUsersQueries usersQueries;
        private DeviceGrantsQueriesEventHandler sut;

        [TestInitialize]
        public void Setup()
        {
            ctx = new MockDocmsContext("DeviceGrantsQueriesEventHandlerTests");
            usersQueries = new MockUsersQueries();
            usersQueries.Create(new User()
            {
                Id = "USERID",
                AccountName = "USERACCOUNTNAME",
                Name = "USER NAME",
            });
            sut = new DeviceGrantsQueriesEventHandler(ctx, usersQueries);
        }

        [TestCleanup]
        public async Task Teardown()
        {
            ctx.DeviceGrants.RemoveRange(ctx.DeviceGrants);
            await ctx.SaveChangesAsync();
        }

        [TestMethod]
        public async Task デバイスの登録イベントが作成される()
        {
            var device = new Device("DEVID1", "USERAGENT", "USERID");
            var ev = device.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DeviceNewlyAccessedEvent>(ev as DeviceNewlyAccessedEvent));
            Assert.AreEqual(1, await ctx.DeviceGrants.Where(f => f.DeviceId == "DEVID1").CountAsync());
        }
    }
}
