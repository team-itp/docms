using Docms.Domain.Documents;
using Docms.Domain.Events;
using Docms.Infrastructure.MediatR;
using Docms.Web.Application.DomainEventHandlers.DocumentCreated;
using Docms.Web.Application.Queries;
using Docms.Web.Application.Queries.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Tests
{
    [TestClass]
    public class DocumentCreated_UpdateQueriesEventHandler
    {
        private DocmsQueriesContext ctx;
        private UpdateQueriesEventHandler sut;

        [TestInitialize]
        public async Task Setup()
        {
            ctx = new DocmsQueriesContext(new DbContextOptionsBuilder<DocmsQueriesContext>()
                .UseInMemoryDatabase("DocumentCreated_UpdateQueriesEventHandler")
                .Options);
            sut = new UpdateQueriesEventHandler(ctx);

            ctx.Containers.Add(new Container() { Path = "path1", Name = "path1", ParentPath = null });
            ctx.Containers.Add(new Container() { Path = "path1/subpath1", Name = "subpath1", ParentPath = "path1" });
            ctx.Containers.Add(new Container() { Path = "path2", Name = "path2", ParentPath = null });
            ctx.Files.Add(new File() { Path = "path1/document1.txt", Name = "document1.txt", ParentPath = "path1" });
            ctx.Files.Add(new File() { Path = "path1/document2.txt", Name = "document2.txt", ParentPath = "path1" });
            ctx.Files.Add(new File() { Path = "path2/document1.txt", Name = "document1.txt", ParentPath = "path2" });
            await ctx.SaveChangesAsync();
        }

        [TestCleanup]
        public async Task Teardown()
        {
            ctx.Containers.RemoveRange(ctx.Containers);
            ctx.Files.RemoveRange(ctx.Files);
            await ctx.SaveChangesAsync();
        }

        [TestMethod]
        public async Task ドメインイベントから読み取りモデルのファイルを追加する()
        {
            var document = new Document(new DocumentPath("path3/content1.txt"), "text/plain", 10, new byte[] { 1, 2, 3, 4 });
            var ev = document.DomainEvents.First() as DocumentCreatedEvent;
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev));
            Assert.IsTrue(await ctx.Files.AnyAsync(f => f.Path == "path3/content1.txt"));
            Assert.IsTrue(await ctx.Containers.AnyAsync(f => f.Path == "path3"));
        }
    }
}
