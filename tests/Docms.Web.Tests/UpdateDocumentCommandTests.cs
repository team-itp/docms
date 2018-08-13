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
    public class UpdateDocumentCommandTests
    {
        private MockDocumentRepository repository;
        private LocalFileStorage localFileStorage;
        private UpdateDocumentCommandHandler sut;

        [TestInitialize]
        public void Setup()
        {
            repository = new MockDocumentRepository();
            localFileStorage = new LocalFileStorage("tmp");
            sut = new UpdateDocumentCommandHandler(repository, localFileStorage);
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
        public async Task コマンドを発行してドキュメントが更新されること()
        {
            var dir = await localFileStorage.GetDirectoryAsync("test1");
            var prop = await dir.SaveAsync("document1.txt", new MemoryStream(Encoding.UTF8.GetBytes("Hello, world")));
            await repository.AddAsync(new Document(new DocumentPath("test1/document1.txt"), prop.ContentType, prop.Size, prop.Hash));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello, new world")))
            {
                await sut.Handle(new UpdateDocumentCommand()
                {
                    Path = new FilePath("test1/document1.txt"),
                    Stream = ms,
                });
            }
            Assert.AreEqual("test1/document1.txt", repository.Documents.First().Path.Value);
            var file = (await localFileStorage.GetEntryAsync("test1/document1.txt")) as Infrastructure.Files.File;
            using (var fs = await file.OpenAsync())
            using (var ms = new MemoryStream())
            {
                await fs.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);
                using(var sr = new StreamReader(ms))
                {
                    Assert.AreEqual("Hello, new world", await sr.ReadToEndAsync());
                }
            }
        }
    }
}
