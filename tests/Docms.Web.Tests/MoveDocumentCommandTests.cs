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
        private MoveDocumentCommandHandler sut;

        [TestInitialize]
        public void Setup()
        {
            repository = new MockDocumentRepository();
            sut = new MoveDocumentCommandHandler(repository);
        }

        [TestMethod]
        public async Task コマンドを発行してファイルが移動すること()
        {
            await repository.AddAsync(DocumentUtils.Create("test1/document1.txt", "Hello, world"));
            await sut.Handle(new MoveDocumentCommand()
            {
                OriginalPath = new FilePath("test1/document1.txt"),
                DestinationPath = new FilePath("test2/document2.txt"),
            });
            Assert.AreEqual("test2/document2.txt", repository.Entities.First().Path.Value);
        }
    }
}
