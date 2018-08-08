using Docms.Web.Application.Queries;
using Docms.Web.Application.Queries.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docms.Web.Tests
{
    [TestClass]
    public class DocumentsQueriesTests
    {
        private DocmsQueriesContext ctx;
        private DocumentsQueries sut;

        [TestInitialize]
        public async Task Setup()
        {
            ctx = new DocmsQueriesContext(new DbContextOptionsBuilder<DocmsQueriesContext>()
                .UseInMemoryDatabase("DocumentsQueriesTests")
                .Options);
            sut = new DocumentsQueries(ctx);

            ctx.Containers.Add(new Container() { Path = "path1", Name = "path1", ParentPath = null });
            ctx.Containers.Add(new Container() { Path = "path1/subpath1", Name = "subpath1", ParentPath = "path1" });
            ctx.Containers.Add(new Container() { Path = "path2", Name = "path2", ParentPath = null });
            ctx.Files.Add(new File() { Path = "path1/document1.txt", Name = "document1.txt", ParentPath = "path1" });
            ctx.Files.Add(new File() { Path = "path1/document2.txt", Name = "document2.txt", ParentPath = "path1" });
            ctx.Files.Add(new File() { Path = "path2/document1.txt", Name = "document1.txt", ParentPath = "path2" });
            await ctx.SaveChangesAsync();
        }

        [TestCleanup]
        public async Task Teardonw()
        {
            ctx.Containers.RemoveRange(ctx.Containers);
            ctx.Files.RemoveRange(ctx.Files);
            await ctx.SaveChangesAsync();
        }

        [TestMethod]
        public async Task 指定されたパスがファイルの場合ファイルが取得できること()
        {
            var entry = await sut.GetEntryAsync("path1/document1.txt");
            Assert.IsNotNull(entry);
            Assert.IsTrue(entry is File);
        }

        [TestMethod]
        public async Task 指定されたパスがコンテナの場合コンテナが取得できること()
        {
            var entry = await sut.GetEntryAsync("path2");
            var container = entry as Container;
            Assert.IsNotNull(container);
            Assert.AreEqual(1, container.Entries.Count);
        }

        [TestMethod]
        public async Task 指定されたパスがコンテナで複数のエントリーを含んでいる場合すべて取得できること()
        {
            var entry = await sut.GetEntryAsync("path1");
            var container = entry as Container;
            Assert.IsNotNull(container);
            Assert.AreEqual(3, container.Entries.Count);
        }

        [TestMethod]
        public async Task ルートディレクトリの配下のエントリーがすべて取得できること()
        {
            var entry = await sut.GetEntryAsync(null);
            var container = entry as Container;
            Assert.IsNotNull(container);
            Assert.AreEqual(2, container.Entries.Count);
        }
    }
}
