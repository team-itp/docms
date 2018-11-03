using Docms.Domain.Identity;
using Docms.Domain.Identity.Events;
using Docms.Infrastructure;
using Docms.Infrastructure.MediatR;
using Docms.Web.Application.DomainEventHandlers;
using Docms.Web.Application.Identity;
using Docms.Web.Tests.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Web.Tests
{
    [TestClass]
    public class UpdateDeviceGrantsQueriesEventHandlerTests
    {
        private DocmsContext ctx;
        private IUserStore<ApplicationUser> userStore;
        private UpdateDeviceGrantsQueriesEventHandler sut;

        [TestInitialize]
        public void Setup()
        {
            ctx = new DocmsContext(new DbContextOptionsBuilder<DocmsContext>()
                .UseInMemoryDatabase("UpdateDeviceGrantsQueriesEventHandlerTests")
                .Options, new MockMediator());
            userStore = new MockUserStore();
            userStore.CreateAsync(new ApplicationUser()
            {
                Id = "USERID",
                AccountName = "USERACCOUNTNAME",
                Name = "USER NAME",
            }, default(CancellationToken));
            sut = new UpdateDeviceGrantsQueriesEventHandler(ctx, userStore);
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
