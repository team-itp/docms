using Docms.Infrastructure.Repositories;
using Docms.Infrastructure.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Tests
{
    [TestClass]
    public class DocumentRepositoryTests
    {
        [TestMethod]
        public async Task DocumentがPathで取得できること()
        {
            var ctx = new MockDocmsContext("DocumentsRepositoryTests");
            var sut = new DocumentRepository(ctx);
            await sut.AddAsync(DocumentUtils.Create("dir1/test.txt", "Hello, world"));
            await sut.UnitOfWork.SaveEntitiesAsync();
            var document = await sut.GetAsync("dir1/test.txt");
            Assert.AreEqual("dir1/test.txt", document.Path);
        }

        [TestMethod]
        public async Task ルート直下のサブディレクトリをディレクトリとして認識すること()
        {
            var ctx = new MockDocmsContext("DocumentsRepositoryTests");
            var sut = new DocumentRepository(ctx);
            await sut.AddAsync(DocumentUtils.Create("dir1/test.txt", "Hello, world"));
            await sut.UnitOfWork.SaveEntitiesAsync();
            Assert.IsTrue(await sut.IsContainerPath("dir1"));
        }
    }
}