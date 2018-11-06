using Docms.Infrastructure.Storage;
using Docms.Infrastructure.Queries;
using Docms.Infrastructure.Tests.Utils;
using Docms.Queries.DocumentHistories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Tests
{
    [TestClass]
    public class DocumentHistoriesQueriesTests
    {
        private DocmsContext ctx;
        private IDocumentHistoriesQueries sut;

        [TestInitialize]
        public async Task Setup()
        {
            ctx = new DocmsContext(new DbContextOptionsBuilder<DocmsContext>()
                .UseInMemoryDatabase("DocumentHistoriesQueriesTests")
                .Options, new MockMediator());
            sut = new DocumentHistoriesQueries(ctx);

            Created(ctx, "path1/subpath1/document1.txt", 1);
            Created(ctx, "path1/subpath1/document2.txt", 2);
            Created(ctx, "path1/subpath1document2.txt", 3);
            Moved(ctx, "path1/subpath1/document2.txt", "path2/subpath1/document1.txt", 2);
            Updated(ctx, "path1/subpath1/document1.txt", 4);
            Deleted(ctx, "path1/subpath1/document1.txt");
            Created(ctx, "path2/document1.txt", 4);
            await ctx.SaveChangesAsync();
        }

        [TestCleanup]
        public async Task Teardown()
        {
            ctx.DocumentHistories.RemoveRange(ctx.DocumentHistories);
            await ctx.SaveChangesAsync();
        }

        private void Created(DocmsContext ctx, string path, int contentId)
        {
            var now = DateTime.UtcNow;
            ctx.DocumentCreated.Add(new DocumentCreated()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Path = path,
                ContentType = "text/plain",
                FileSize = 5 + contentId.ToString().Length,
                Hash = Hash.CalculateHash(Encoding.UTF8.GetBytes("Hello" + contentId)),
                Created = now,
                LastModified = now
            });
        }

        private void Moved(DocmsContext ctx, string from, string to, int contentId)
        {
            var now = DateTime.UtcNow;
            ctx.DocumentMovedFromOldPath.Add(new DocumentMovedFromOldPath()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Path = to,
                OldPath = from,
                ContentType = "text/plain",
                FileSize = 5 + contentId.ToString().Length,
                Hash = Hash.CalculateHash(Encoding.UTF8.GetBytes("Hello" + contentId)),
                Created = now,
                LastModified = now
            });
            ctx.DocumentMovedToNewPath.Add(new DocumentMovedToNewPath()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Path = from,
                NewPath = to
            });
        }

        private void Updated(DocmsContext ctx, string path, int contentId)
        {
            var now = DateTime.UtcNow;
            ctx.DocumentUpdated.Add(new DocumentUpdated()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Path = path,
                ContentType = "text/plain",
                FileSize = 5 + contentId.ToString().Length,
                Hash = Hash.CalculateHash(Encoding.UTF8.GetBytes("Hello" + contentId)),
                Created = now,
                LastModified = now
            });
        }

        private void Deleted(DocmsContext ctx, string path)
        {
            ctx.DocumentDeleted.Add(new DocumentDeleted()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Path = path
            });
        }

        [TestMethod]
        public async Task 条件なしでルートを指定した場合ドキュメントの履歴がすべて取得できること()
        {
            var histories = await sut.GetHistoriesAsync("");
            Assert.AreEqual(await ctx.DocumentHistories.CountAsync(), histories.Count());
        }

        [TestMethod]
        public async Task ディレクトリを指定した場合当該のドキュメント配下の履歴がすべて取得できること()
        {
            var histories = await sut.GetHistoriesAsync("path1/subpath1");
            Assert.AreEqual(5, histories.Count());
        }

        [TestMethod]
        public async Task 期間を指定してデータが取得できること()
        {
            var since = DateTime.UtcNow;
            await Task.Delay(1);
            Created(ctx, "path1/newdocument.txt", 1);
            await ctx.SaveChangesAsync();
            var histories = await sut.GetHistoriesAsync("", since);
            Assert.AreEqual(1, histories.Count());
        }
    }
}
