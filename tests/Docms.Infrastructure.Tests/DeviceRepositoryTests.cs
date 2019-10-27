using Docms.Domain.Identity;
using Docms.Infrastructure.Repositories;
using Docms.Infrastructure.Tests.Utils;
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
            var ctx = new MockDocmsContext("MockDocmsContext");
            var sut = new DeviceRepository(ctx);
            await sut.AddAsync(new Device("ABC", "USERAGENT", "USER1"));
            await sut.UnitOfWork.SaveEntitiesAsync();
            var d2 = await sut.GetAsync("ABC");
            Assert.AreEqual("ABC", d2.DeviceId);
            Assert.AreEqual("USERAGENT", d2.DeviceUserAgent);
            Assert.AreEqual(false, d2.Granted);
        }
    }
}