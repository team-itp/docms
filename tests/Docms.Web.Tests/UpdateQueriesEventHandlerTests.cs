using Docms.Domain.Documents;
using Docms.Domain.Events;
using Docms.Infrastructure.MediatR;
using Docms.Web.Application.DomainEventHandlers;
using Docms.Web.Application.Queries;
using Docms.Web.Application.Queries.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Tests
{
    [TestClass]
    public class UpdateQueriesEventHandlerTests
    {
        private DocmsQueriesContext ctx;
        private UpdateQueriesEventHandler sut;

        [TestInitialize]
        public async Task Setup()
        {
            ctx = new DocmsQueriesContext(new DbContextOptionsBuilder<DocmsQueriesContext>()
                .UseInMemoryDatabase("UpdateQueriesEventHandlerTests")
                .Options);
            sut = new UpdateQueriesEventHandler(ctx);

            ctx.Containers.Add(new Container() { Path = "path1", Name = "path1", ParentPath = null });
            ctx.Containers.Add(new Container() { Path = "path1/subpath1", Name = "subpath1", ParentPath = "path1" });
            ctx.Containers.Add(new Container() { Path = "path2", Name = "path2", ParentPath = null });
            ctx.Documents.Add(new Application.Queries.Documents.Document() { Path = "path1/document1.txt", Name = "document1.txt", ParentPath = "path1" });
            ctx.Documents.Add(new Application.Queries.Documents.Document() { Path = "path1/document2.txt", Name = "document2.txt", ParentPath = "path1" });
            ctx.Documents.Add(new Application.Queries.Documents.Document() { Path = "path1/subpath1/document1.txt", Name = "document1.txt", ParentPath = "path1/subpath1" });
            ctx.Documents.Add(new Application.Queries.Documents.Document() { Path = "path2/document1.txt", Name = "document1.txt", ParentPath = "path2" });
            await ctx.SaveChangesAsync();
        }

        [TestCleanup]
        public async Task Teardown()
        {
            ctx.Containers.RemoveRange(ctx.Containers);
            ctx.Documents.RemoveRange(ctx.Documents);
            await ctx.SaveChangesAsync();
        }

        [TestMethod]
        public async Task ドメインイベントから読み取りモデルのファイルを追加する()
        {
            var document = new Domain.Documents.Document(new DocumentPath("path3/content1.txt"), "text/plain", 10, new byte[] { 1, 2, 3, 4 });
            var ev = document.DomainEvents.First() as DocumentCreatedEvent;
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev));
            Assert.IsTrue(await ctx.Documents.AnyAsync(f => f.Path == "path3/content1.txt"));
            Assert.IsTrue(await ctx.Containers.AnyAsync(f => f.Path == "path3"));
        }

        [TestMethod]
        public async Task すでに存在するディレクトリのパスに対して読み取りモデルのファイルを追加する()
        {
            var document = new Domain.Documents.Document(new DocumentPath("path1/subpath1/content1.txt"), "text/plain", 10, new byte[] { 1, 2, 3, 4 });
            var ev = document.DomainEvents.First() as DocumentCreatedEvent;
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev));
            Assert.IsTrue(await ctx.Documents.AnyAsync(f => f.Path == "path1/subpath1/content1.txt"));
        }

        [TestMethod]
        public async Task 親Containerに1件のみファイルが存在するパスを削除してDocumentとContainerが削除される()
        {
            var document1 = new Domain.Documents.Document(new DocumentPath("path1/subpath1/document1.txt"), "text/plain", 10, new byte[] { 1, 2, 3, 4 });
            document1.ClearDomainEvents();
            document1.Delete();
            var ev = document1.DomainEvents.First() as DocumentDeletedEvent;
            await sut.Handle(new DomainEventNotification<DocumentDeletedEvent>(ev));
            Assert.IsFalse(await ctx.Documents.AnyAsync(f => f.Path == "path1/subpath1/document1.txt"));
            Assert.IsFalse(await ctx.Containers.AnyAsync(f => f.Path == "path1/subpath1"));
        }

        [TestMethod]
        public async Task 親Containerに2件以上のファイルが存在するパスを削除してDocumentは削除されるがContainerは削除されない()
        {
            var document1 = new Domain.Documents.Document(new DocumentPath("path1/document1.txt"), "text/plain", 10, new byte[] { 1, 2, 3, 4 });
            document1.ClearDomainEvents();
            document1.Delete();
            var ev = document1.DomainEvents.First() as DocumentDeletedEvent;
            await sut.Handle(new DomainEventNotification<DocumentDeletedEvent>(ev));
            Assert.IsFalse(await ctx.Documents.AnyAsync(f => f.Path == "path1/document1.txt"));
            Assert.IsTrue(await ctx.Containers.AnyAsync(f => f.Path == "path1"));
        }
    }
}
