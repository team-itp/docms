using Docms.Web.Application.Commands;
using Docms.Web.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docms.Web.Tests
{
    [TestClass]
    public class AddNewDeviceCommandTests
    {
        private MockDeviceRepository repository;
        private AddNewDeviceCommandHandler sut;

        [TestInitialize]
        public void Setup()
        {
            repository = new MockDeviceRepository();
            sut = new AddNewDeviceCommandHandler(repository);
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
    }
}
