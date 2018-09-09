using Docms.Domain.Documents;
using Docms.Domain.Events;
using Docms.Infrastructure;
using Docms.Infrastructure.Files;
using Docms.Infrastructure.MediatR;
using Docms.Queries.DocumentHistories;
using Docms.Web.Application.DomainEventHandlers;
using Docms.Web.Tests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Tests
{
    [TestClass]
    public class UpdateDocumentHistoriesQueriesEventHandlerTests
    {
        private DocmsContext ctx;
        private UpdateDocumentHistoriesQueriesEventHandler sut;

        [TestInitialize]
        public void Setup()
        {
            ctx = new DocmsContext(new DbContextOptionsBuilder<DocmsContext>()
                .UseInMemoryDatabase("UpdateDocumentHistoriesQueriesEventHandlerTests")
                .Options, new MockMediator());
            sut = new UpdateDocumentHistoriesQueriesEventHandler(ctx);
        }

        [TestCleanup]
        public async Task Teardown()
        {
            ctx.DocumentHistories.RemoveRange(ctx.DocumentHistories);
            await ctx.SaveChangesAsync();
        }

        [TestMethod]
        public async Task ドキュメント作成のイベントが登録される()
        {
            var document = new Domain.Documents.Document(new DocumentPath("path1/content1.txt"), "text/plain", 5, Hash.CalculateHash(new MemoryStream(new byte[] { 72, 101, 108, 108, 111 })));
            var ev = document.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev as DocumentCreatedEvent));
            Assert.AreEqual(1, await ctx.DocumentHistories.Where(f => f.Path == "path1/content1.txt" && f is DocumentCreated).CountAsync());
        }

        [TestMethod]
        public async Task ドキュメント削除のイベントが登録される()
        {
            var document1 = new Domain.Documents.Document(new DocumentPath("path1/subpath1/document1.txt"), "text/plain", 5, Hash.CalculateHash(new MemoryStream(new byte[] { 72, 101, 108, 108, 111 })));
            var ev1 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev1 as DocumentCreatedEvent));
            document1.ClearDomainEvents();
            document1.Delete();
            var ev2 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentDeletedEvent>(ev2 as DocumentDeletedEvent));
            Assert.AreEqual(1, await ctx.DocumentHistories.Where(f => f.Path == "path1/subpath1/document1.txt" && f is DocumentCreated).CountAsync());
            Assert.AreEqual(1, await ctx.DocumentHistories.Where(f => f.Path == "path1/subpath1/document1.txt" && f is DocumentDeleted).CountAsync());
        }

        [TestMethod]
        public async Task ドキュメント移動のイベントが登録される()
        {
            var document1 = new Domain.Documents.Document(new DocumentPath("path1/subpath1/content1.txt"), "text/plain", 5, Hash.CalculateHash(new MemoryStream(new byte[] { 72, 101, 108, 108, 111 })));
            var ev1 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev1 as DocumentCreatedEvent));

            document1.ClearDomainEvents();
            document1.MoveTo(new DocumentPath("path2/subpath1/document1.txt"));
            var ev2 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentMovedEvent>(ev2 as DocumentMovedEvent));
            Assert.AreEqual(1, await ctx.DocumentHistories.Where(f => f.Path == "path1/subpath1/content1.txt" && f is DocumentCreated).CountAsync());
            Assert.AreEqual(1, await ctx.DocumentHistories.Where(f => f.Path == "path1/subpath1/content1.txt" && f is DocumentMovedToNewPath).CountAsync());
            Assert.AreEqual(1, await ctx.DocumentHistories.Where(f => f.Path == "path2/subpath1/document1.txt" && f is DocumentMovedFromOldPath).CountAsync());
        }

        [TestMethod]
        public async Task ドキュメント更新のイベントが登録される()
        {
            var document1 = new Domain.Documents.Document(new DocumentPath("path1/subpath1/content1.txt"), "text/plain", 5, Hash.CalculateHash(new MemoryStream(new byte[] { 72, 101, 108, 108, 111 })));
            var ev1 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev1 as DocumentCreatedEvent));

            document1.ClearDomainEvents();
            document1.Update("application/json", 2, Hash.CalculateHash(new MemoryStream(new byte[] { 123, 125 })));
            var ev2 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentUpdatedEvent>(ev2 as DocumentUpdatedEvent));
            Assert.AreEqual(1, await ctx.DocumentHistories.Where(f => f.Path == "path1/subpath1/content1.txt" && f is DocumentCreated).CountAsync());
            Assert.AreEqual(1, await ctx.DocumentHistories.Where(f => f.Path == "path1/subpath1/content1.txt" && f is DocumentUpdated).CountAsync());
        }
    }
}
