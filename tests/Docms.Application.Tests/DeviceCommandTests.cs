using Docms.Application.Commands;
using Docms.Application.Tests.Utils;
using Docms.Domain.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Application.Tests
{
    [TestClass]
    public class DeviceCommandTests
    {
        private MockDeviceRepository repository;
        private DeviceCommandHandler sut;

        [TestInitialize]
        public void Setup()
        {
            repository = new MockDeviceRepository();
            sut = new DeviceCommandHandler(repository);
        }

        [TestMethod]
        public async Task コマンドを発行してデバイスが作成されること()
        {
            await sut.Handle(new AddNewDeviceCommand()
            {
                DeviceId = "123",
            });
            Assert.AreEqual(1, repository.Entities.Count);
        }

        [TestMethod]
        public async Task コマンドを発行してデバイスが許可されること()
        {
            await repository.AddAsync(new Device("123", "USERAGENT", "USER1"));
            await sut.Handle(new GrantDeviceCommand()
            {
                DeviceId = "123",
                ByUserId = "USER2"
            });
            Assert.AreEqual(1, repository.Entities.Count);
            Assert.IsTrue(repository.Entities.First().Granted);
        }

        [TestMethod]
        public async Task コマンドを発行してデバイスが拒否されること()
        {
            await repository.AddAsync(new Device("123", "USERAGENT", "USER1") { Granted = true });
            await sut.Handle(new RevokeDeviceCommand()
            {
                DeviceId = "123",
                ByUserId = "USER2"
            });
            Assert.AreEqual(1, repository.Entities.Count);
            Assert.IsFalse(repository.Entities.First().Granted);
        }
    }
}
