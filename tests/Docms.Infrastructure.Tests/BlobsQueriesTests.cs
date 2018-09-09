using Docms.Infrastructure.Queries;
using Docms.Infrastructure.Tests.Utils;
using Docms.Queries.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Tests
{
    [TestClass]
    public class BlobsQueriesTests
    {
        private DocmsContext ctx;
        private IBlobsQueries sut;

        [TestInitialize]
        public async Task Setup()
        {
            ctx = new DocmsContext(new DbContextOptionsBuilder<DocmsContext>()
                .UseInMemoryDatabase("BlobsQueriesTests")
                .Options, new MockMediator());
            sut = new BlobsQueries(ctx);

            ctx.BlobContainers.Add(new BlobContainer() { Path = "path1", Name = "path1", ParentPath = null });
            ctx.BlobContainers.Add(new BlobContainer() { Path = "path1/subpath1", Name = "subpath1", ParentPath = "path1" });
            ctx.BlobContainers.Add(new BlobContainer() { Path = "path2", Name = "path2", ParentPath = null });
            ctx.Blobs.Add(new Blob() { Path = "path1/Blob1.txt", Name = "Blob1.txt", ParentPath = "path1" });
            ctx.Blobs.Add(new Blob() { Path = "path1/Blob2.txt", Name = "Blob2.txt", ParentPath = "path1" });
            ctx.Blobs.Add(new Blob() { Path = "path2/Blob1.txt", Name = "Blob1.txt", ParentPath = "path2" });
            await ctx.SaveChangesAsync();
        }

        [TestCleanup]
        public async Task Teardown()
        {
            ctx.BlobContainers.RemoveRange(ctx.BlobContainers);
            ctx.Blobs.RemoveRange(ctx.Blobs);
            await ctx.SaveChangesAsync();
        }

        [TestMethod]
        public async Task 指定されたパスがファイルの場合ファイルが取得できること()
        {
            var entry = await sut.GetEntryAsync("path1/Blob1.txt");
            Assert.IsNotNull(entry);
            Assert.IsTrue(entry is Blob);
        }

        [TestMethod]
        public async Task 指定されたパスがコンテナの場合コンテナが取得できること()
        {
            var entry = await sut.GetEntryAsync("path2");
            var blobContainer = entry as BlobContainer;
            Assert.IsNotNull(blobContainer);
            Assert.AreEqual(1, blobContainer.Entries.Count);
        }

        [TestMethod]
        public async Task 指定されたパスがコンテナで複数のエントリーを含んでいる場合すべて取得できること()
        {
            var entry = await sut.GetEntryAsync("path1");
            var blobContainer = entry as BlobContainer;
            Assert.IsNotNull(blobContainer);
            Assert.AreEqual(3, blobContainer.Entries.Count);
        }

        [TestMethod]
        public async Task ルートディレクトリの配下のエントリーがすべて取得できること()
        {
            var entry = await sut.GetEntryAsync(null);
            var blobContainer = entry as BlobContainer;
            Assert.IsNotNull(blobContainer);
            Assert.AreEqual(2, blobContainer.Entries.Count);
        }
    }
}
