using Docms.Infrastructure.Files;
using Docms.Web.Application.Queries;
using Docms.Web.Application.Queries.DocumentHistories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Web.Tests
{
    [TestClass]
    public class DocumentHistoriesQueriesTests
    {
        private DocmsQueriesContext ctx;
        private DocumentHistoriesQueries sut;

        [TestInitialize]
        public async Task Setup()
        {
            ctx = new DocmsQueriesContext(new DbContextOptionsBuilder<DocmsQueriesContext>()
                .UseInMemoryDatabase("DocumentHistoriesQueriesTests")
                .Options);
            sut = new DocumentHistoriesQueries(ctx);

            Created(ctx, "path1/subpath1/document1.txt", 1);
            Created(ctx, "path1/subpath1/document2.txt", 2);
            Created(ctx, "path1/subpath1document2.txt", 3);
            Moved(ctx, "path1/subpath1/document2.txt", "path2/subpath1/document1.txt");
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

        private void Created(DocmsQueriesContext ctx, string path, int contentId)
        {
            var now = DateTime.UtcNow;
            ctx.DocumentCreated.Add(new DocumentCreated()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Path = path,
                ContentType = "text/plain",
                FileSize = 5 + contentId.ToString().Length,
                Hash = Hash.CalculateHashString(Encoding.UTF8.GetBytes("Hello" + contentId)),
                Created = now,
                LastModified = now
            });
        }

        private void Moved(DocmsQueriesContext ctx, string from, string to)
        {
            ctx.DocumentMovedFromOldPath.Add(new DocumentMovedFromOldPath()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Path = to,
                OldPath = from
            });
            ctx.DocumentMovedToNewPath.Add(new DocumentMovedToNewPath()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Path = from,
                NewPath = to
            });
        }

        private void Updated(DocmsQueriesContext ctx, string path, int contentId)
        {
            var now = DateTime.UtcNow;
            ctx.DocumentUpdated.Add(new DocumentUpdated()
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Path = path,
                ContentType = "text/plain",
                FileSize = 5 + contentId.ToString().Length,
                Hash = Hash.CalculateHashString(Encoding.UTF8.GetBytes("Hello" + contentId)),
                Created = now,
                LastModified = now
            });
        }

        private void Deleted(DocmsQueriesContext ctx, string path)
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
            Created(ctx, "path1/newdocument.txt", 1);
            await ctx.SaveChangesAsync();
            var histories = await sut.GetHistoriesAsync("", since);
            Assert.AreEqual(1, histories.Count());
        }
    }
}
