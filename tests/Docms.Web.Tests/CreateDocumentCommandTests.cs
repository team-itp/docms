using Docms.Infrastructure.Files;
using Docms.Web.Application.Commands;
using Docms.Web.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Web.Tests
{
    [TestClass]
    public class CreateDocumentCommandTests
    {
        private MockDocumentRepository repository;
        private LocalFileStorage localFileStorage;
        private CreateDocumentCommandHandler sut;

        [TestInitialize]
        public void Setup()
        {
            repository = new MockDocumentRepository();
            localFileStorage = new LocalFileStorage("tmp");
            sut = new CreateDocumentCommandHandler(repository, localFileStorage);
        }

        [TestCleanup]
        public void Teardown()
        {
            if (System.IO.Directory.Exists("tmp"))
            {
                System.IO.Directory.Delete("tmp", true);
            }
        }

        [TestMethod]
        public async Task コマンドを発行してドキュメントが作成されること()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello, world")))
            {
                await sut.Handle(new CreateDocumentCommand()
                {
                    Path = new FilePath("document.txt"),
                    Stream = ms
                });
            }
            Assert.AreEqual(1, repository.Documents.Count);
            Assert.IsTrue((await localFileStorage.GetEntryAsync("document.txt")) is Docms.Infrastructure.Files.File);
        }
    }
}
