using Docms.Domain.Documents;
using Docms.Infrastructure.Files;
using Docms.Infrastructure.Repositories;
using Docms.Infrastructure.Tests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Tests
{
    [TestClass]
    public class DocumentRepositoryTests
    {
        private Document BuildDocument(string path)
        {
            return new Document(new DocumentPath(path), "text/plain", 10, "abcde");
        }

        [TestMethod]
        public async Task DocumentがPathで取得できること()
        {
            var mediator = new MockMediator();
            var ctx = new DocmsContext(new DbContextOptionsBuilder<DocmsContext>()
                .UseInMemoryDatabase("DocumentsRepositoryTests")
                .Options, mediator);
            var sut = new DocumentRepository(ctx);
            var d1 = await sut.AddAsync(BuildDocument("dir1/test.txt"));
            await sut.UnitOfWork.SaveEntitiesAsync();
            var d2 = await sut.GetAsync("dir1/test.txt");
            Assert.AreEqual("dir1/test.txt", d2.Path.Value);
        }
    }
}