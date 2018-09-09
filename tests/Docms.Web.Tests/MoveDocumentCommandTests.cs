using Docms.Domain.Documents;
using Docms.Infrastructure.Files;
using Docms.Web.Application.Commands;
using Docms.Web.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Web.Tests
{
    [TestClass]
    public class MoveDocumentCommandTests
    {
        private MockDocumentRepository repository;
        private LocalFileStorage localFileStorage;
        private MoveDocumentCommandHandler sut;

        [TestInitialize]
        public void Setup()
        {
            repository = new MockDocumentRepository();
            localFileStorage = new LocalFileStorage("tmp");
            sut = new MoveDocumentCommandHandler(repository, localFileStorage);
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
        public async Task コマンドを発行してファイルが移動すること()
        {
            var dir = await localFileStorage.GetDirectoryAsync("test1");

            var bytes = Encoding.UTF8.GetBytes("Hello, world");
            await dir.SaveAsync("document1.txt", "text/plain", new MemoryStream());
            await repository.AddAsync(new Document(new DocumentPath("test1/document1.txt"), "text/plain", bytes.Length, Hash.CalculateHash(bytes)));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello, world")))
            {
                await sut.Handle(new MoveDocumentCommand()
                {
                    OriginalPath = new FilePath("test1/document1.txt"),
                    DestinationPath = new FilePath("test2/document2.txt"),
                });
            }
            Assert.AreEqual("test2/document2.txt", repository.Entities.First().Path.Value);
            Assert.IsNull(await localFileStorage.GetEntryAsync("test1/document1.txt"));
            Assert.IsNotNull(await localFileStorage.GetEntryAsync("test2/document2.txt"));
        }
    }
}
