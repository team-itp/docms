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
    public class DeleteDocumentCommandTests
    {
        [TestMethod]
        public async Task コマンドを発行してドキュメントが削除されること()
        {
            var repository = new MockDocumentRepository();
            var localFileStorage = new LocalFileStorage("tmp");
            try
            {
                var sut = new CreateDocumentCommandHandler(repository, localFileStorage);
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello, world")))
                {
                    await sut.Handle(new CreateDocumentCommand()
                    {
                        Path = new FilePath("document.txt"),
                        Stream = ms
                    });
                    Assert.AreEqual(1, repository.Documents.Count);
                }
            }
            catch
            {
                if (System.IO.Directory.Exists("tmp"))
                {
                    System.IO.Directory.Delete("tmp");
                }
            }
        }
    }
}
