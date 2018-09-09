using Docms.Domain.Identity;
using Docms.Web.Application.Commands;
using Docms.Web.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Tests
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
            await repository.AddAsync(new Device("123"));
            await sut.Handle(new GrantDeviceCommand()
            {
                DeviceId = "123",
                ByUserId = "user1"
            });
            Assert.AreEqual(1, repository.Entities.Count);
            Assert.IsTrue(repository.Entities.First().Granted);
        }
    }
}
