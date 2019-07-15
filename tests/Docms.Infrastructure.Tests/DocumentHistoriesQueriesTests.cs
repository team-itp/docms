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
            Created(ctx, "path1/subpath1/subsubpath1/document1.txt", 2);
            Created(ctx, "path1/subpath1/subsubpath1/document2.txt", 3);
            Created(ctx, "path1/subpath1/document2.txt", 4);
            Created(ctx, "path1/subpath1document2.txt", 5);
            Moved(ctx, "path1/subpath1/document2.txt", "path2/subpath1/document1.txt", 4);
            Updated(ctx, "path1/subpath1/document1.txt", 6);
            Deleted(ctx, "path1/subpath1/document1.txt", 6);
            Created(ctx, "path2/document1.txt", 7);
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
            ctx.DocumentHistories.Add(DocumentHistory.DocumentCreated(
                DateTime.UtcNow,
                contentId,
                path,
                "stragekey",
                "text/plain",
                5 + contentId.ToString().Length,
                Hash.CalculateHash(Encoding.UTF8.GetBytes("Hello" + contentId)),
                now,
                now));
        }

        private void Moved(DocmsContext ctx, string from, string to, int contentId)
        {
            var now = DateTime.UtcNow;
            ctx.DocumentHistories.Add(DocumentHistory.DocumentUpdated(
                DateTime.UtcNow,
                contentId,
                to,
                "stragekey",
                "text/plain",
                5 + contentId.ToString().Length,
                Hash.CalculateHash(Encoding.UTF8.GetBytes("Hello" + contentId)),
                now,
                now));
            ctx.DocumentHistories.Add(DocumentHistory.DocumentDeleted(
                DateTime.UtcNow,
                contentId,
                from));
        }

        private void Updated(DocmsContext ctx, string path, int contentId)
        {
            var now = DateTime.UtcNow;
            ctx.DocumentHistories.Add(DocumentHistory.DocumentUpdated(
                DateTime.UtcNow,
                contentId,
                path,
                "stragekey",
                "text/plain",
                5 + contentId.ToString().Length,
                Hash.CalculateHash(Encoding.UTF8.GetBytes("Hello" + contentId)),
                now,
                now));
        }

        private void Deleted(DocmsContext ctx, string path, int contentId)
        {
            ctx.DocumentHistories.Add(DocumentHistory.DocumentDeleted(
                DateTime.UtcNow,
                contentId,
                path));
        }

        [TestMethod]
        public async Task 条件なしでルートを指定した場合ドキュメントの履歴がすべて取得できること()
        {
            var histories = sut.GetHistories("");
            Assert.AreEqual(await ctx.DocumentHistories.CountAsync(), await histories.CountAsync());
        }

        [TestMethod]
        public async Task ディレクトリを指定した場合当該のドキュメント配下の履歴がすべて取得できること()
        {
            var histories = sut.GetHistories("path1/subpath1");
            Assert.AreEqual(7, await histories.CountAsync());
        }

        [TestMethod]
        public async Task 期間を指定してデータが取得できること()
        {
            var since = DateTime.UtcNow;
            await Task.Delay(1);
            Created(ctx, "path1/newdocument.txt", 1);
            await ctx.SaveChangesAsync();
            var histories = sut.GetHistories("", since);
            Assert.AreEqual(1, await histories.CountAsync());
        }

        [TestMethod]
        public async Task HistoryIdを指定して履歴を検索出来ること()
        {
            var historyId = (await ctx.DocumentHistories.OrderByDescending(h => h.Timestamp).FirstAsync()).Id;
            Created(ctx, "path1/newdocument.txt", 1);
            await ctx.SaveChangesAsync();
            var histories = sut.GetHistories("", null, historyId);
            Assert.AreEqual(1, await histories.CountAsync());
        }
    }
}
