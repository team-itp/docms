using Docms.Infrastructure.Files;
using Docms.Infrastructure.Queries;
using Docms.Infrastructure.Tests.Utils;
using Docms.Queries.DeviceAuthorization;
using Docms.Queries.DocumentHistories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Tests
{
    [TestClass]
    public class DeviceGrantsQueriesTests
    {
        private DocmsContext ctx;
        private IDeviceGrantsQueries sut;

        [TestInitialize]
        public void Setup()
        {
            ctx = new DocmsContext(new DbContextOptionsBuilder<DocmsContext>()
                .UseInMemoryDatabase("DeviceGrantsQueriesTests")
                .Options, new MockMediator());
            sut = new DeviceGrantsQueries(ctx);
        }

        [TestCleanup]
        public async Task Teardown()
        {
            ctx.DeviceGrants.RemoveRange(ctx.DeviceGrants);
            await ctx.SaveChangesAsync();
        }

        [TestMethod]
        public async Task デバイスが存在しない場合は未承認であること()
        {
            var isGranted = await sut.IsGrantedAsync("ABC");
            Assert.IsFalse(isGranted);
        }

        [TestMethod]
        public async Task デバイスが拒否状態の場合はfalseを返す()
        {
            ctx.DeviceGrants.Add(new DeviceGrant()
            {
                DeviceId = "ABC",
                IsGranted = false
            });
            var isGranted = await sut.IsGrantedAsync("ABC");
            Assert.IsFalse(isGranted);
        }

        [TestMethod]
        public async Task デバイスが許可状態の場合はtrueを返す()
        {
            ctx.DeviceGrants.Add(new DeviceGrant()
            {
                DeviceId = "ABC",
                IsGranted = true,
                GrantedBy = "user1",
                GrantedAt = DateTime.UtcNow
            });
            var isGranted = await sut.IsGrantedAsync("ABC");
            Assert.IsTrue(isGranted);
        }
    }
}
