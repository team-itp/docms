using Docms.Domain.Identity;
using Docms.Application.Commands;
using Docms.Application.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Application.Tests
{
    [TestClass]
    public class RevokeDeviceCommandTests
    {
        private MockDeviceRepository repository;
        private RevokeDeviceCommandHandler sut;

        [TestInitialize]
        public void Setup()
        {
            repository = new MockDeviceRepository();
            sut = new RevokeDeviceCommandHandler(repository);
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
