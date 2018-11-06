using Docms.Domain.Documents;
using Docms.Infrastructure.Storage;
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
    public class DeleteDocumentCommandTests
    {
        private MockDocumentRepository repository;
        private DeleteDocumentCommandHandler sut;

        [TestInitialize]
        public void Setup()
        {
            repository = new MockDocumentRepository();
            sut = new DeleteDocumentCommandHandler(repository);
        }

        [TestMethod]
        public async Task コマンドを発行してドキュメントが削除されること()
        {
            await repository.AddAsync(DocumentUtils.Create("test1/document1.txt", "Hello, World"));
            await sut.Handle(new DeleteDocumentCommand()
            {
                Path = new FilePath("test1/document1.txt"),
            });
            Assert.IsNull(await repository.GetAsync("test1/document1.txt"));
        }
    }
}
