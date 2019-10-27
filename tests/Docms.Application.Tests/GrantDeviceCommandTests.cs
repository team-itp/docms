using Docms.Domain.Identity;
using Docms.Application.Commands;
using Docms.Application.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Application.Tests
{
    [TestClass]
    public class GrantDeviceCommandTests
    {
        private MockDeviceRepository repository;
        private GrantDeviceCommandHandler sut;

        [TestInitialize]
        public void Setup()
        {
            repository = new MockDeviceRepository();
            sut = new GrantDeviceCommandHandler(repository);
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
    }
}
