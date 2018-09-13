using Docms.Domain.Identity;
using Docms.Infrastructure.Repositories;
using Docms.Infrastructure.Tests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Tests
{
    [TestClass]
    public class DeviceRepositoryTests
    {
        [TestMethod]
        public async Task デバイスがDeviceIdで取得できること()
        {
            var mediator = new MockMediator();
            var ctx = new DocmsContext(new DbContextOptionsBuilder<DocmsContext>()
                .UseInMemoryDatabase("DeviceRepositoryTests")
                .Options, mediator);
            var sut = new DeviceRepository(ctx);
            var d1 = await sut.AddAsync(new Device("ABC", "USER1"));
            await sut.UnitOfWork.SaveEntitiesAsync();
            var d2 = await sut.GetAsync("ABC");
            Assert.AreEqual("ABC", d2.DeviceId);
            Assert.AreEqual(false, d2.Granted);
        }
    }
}