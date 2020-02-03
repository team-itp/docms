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
        private Device device;

        [TestInitialize]
        public async Task Setup()
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
            device = new Device("DEVID1", "USERAGENT", "USERID");
            var ev = device.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DeviceNewlyAccessedEvent>(ev as DeviceNewlyAccessedEvent));
            device.ClearDomainEvents();
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
            Assert.AreEqual(1, await ctx.DeviceGrants.Where(f => f.DeviceId == "DEVID1").CountAsync());
        }

        [TestMethod]
        public async Task デバイスの承認イベントが作成される()
        {
            device.Grant("USERID");
            var ev = device.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DeviceGrantedEvent>(ev as DeviceGrantedEvent));
            Assert.AreEqual(1, await ctx.DeviceGrants.Where(f => f.DeviceId == "DEVID1" && f.IsGranted && !f.IsDeleted).CountAsync());
        }

        [TestMethod]
        public async Task デバイスの拒否イベントが作成される()
        {
            device.Grant("USERID");
            device.ClearDomainEvents();
            device.Revoke("USERID");
            var ev = device.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DeviceRevokedEvent>(ev as DeviceRevokedEvent));
            Assert.AreEqual(1, await ctx.DeviceGrants.Where(f => f.DeviceId == "DEVID1" && !f.IsGranted && f.IsDeleted).CountAsync());
        }

        [TestMethod]
        public async Task デバイスの再登録イベントが作成される()
        {
            device.Grant("USERID");
            device.Revoke("USERID");
            device.ClearDomainEvents();
            device.Reregister("USERID");
            var ev = device.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DeviceReregisteredEvent>(ev as DeviceReregisteredEvent));
            Assert.AreEqual(1, await ctx.DeviceGrants.Where(f => f.DeviceId == "DEVID1" && !f.IsGranted && !f.IsDeleted).CountAsync());
        }
    }
}
