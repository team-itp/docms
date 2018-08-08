using Docms.Web.Application.Commands;
using Docms.Web.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docms.Web.Tests
{
    [TestClass]
    public class CreateDocumentCommandTests
    {
        [TestMethod]
        public async Task コマンドを発行してドキュメントが作成されること()
        {
            var repository = new MockDocumentRepository();
            var sut = new CreateDocumentCommandHandler(repository);
            await sut.Handle(new CreateDocumentCommand()
            {
                Path = "document.txt",
                ContentType = "text/plain",
                FileSize = 10L,
                Hash = new byte[] { 1, 2, 3, 4 }
            });
            Assert.AreEqual(1, repository.Documents.Count);
        }
    }
}
