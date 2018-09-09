using Docms.Domain.Documents;
using Docms.Domain.Events.Documents;
using Docms.Domain.Events.Identity;
using Docms.Domain.Identity;
using Docms.Infrastructure;
using Docms.Infrastructure.Files;
using Docms.Infrastructure.MediatR;
using Docms.Queries.DocumentHistories;
using Docms.Web.Application.DomainEventHandlers;
using Docms.Web.Tests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Tests
{
    [TestClass]
    public class UpdateDeviceGrantsQueriesEventHandlerTests
    {
        private DocmsContext ctx;
        private UpdateDeviceGrantsQueriesEventHandler sut;

        [TestInitialize]
        public void Setup()
        {
            ctx = new DocmsContext(new DbContextOptionsBuilder<DocmsContext>()
                .UseInMemoryDatabase("UpdateDeviceGrantsQueriesEventHandlerTests")
                .Options, new MockMediator());
            sut = new UpdateDeviceGrantsQueriesEventHandler(ctx);
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
            var device = new Device("DEVID1");
            var ev = device.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DeviceNewlyAccessedEvent>(ev as DeviceNewlyAccessedEvent));
            Assert.AreEqual(1, await ctx.DeviceGrants.Where(f => f.DeviceId == "DEVID1").CountAsync());
        }
    }
}
