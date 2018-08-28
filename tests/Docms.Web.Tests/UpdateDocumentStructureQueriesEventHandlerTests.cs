﻿using Docms.Domain.Documents;
using Docms.Domain.Events;
using Docms.Infrastructure.Files;
using Docms.Infrastructure.MediatR;
using Docms.Web.Application.DomainEventHandlers;
using Docms.Web.Application.Queries;
using Docms.Web.Application.Queries.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Tests
{
    [TestClass]
    public class UpdateDocumentStructureQueriesEventHandlerTests
    {
        private DocmsQueriesContext ctx;
        private UpdateDocumentStructureQueriesEventHandler sut;

        [TestInitialize]
        public void Setup()
        {
            ctx = new DocmsQueriesContext(new DbContextOptionsBuilder<DocmsQueriesContext>()
                .UseInMemoryDatabase("UpdateDocumentStructureQueriesEventHandlerTests")
                .Options);
            sut = new UpdateDocumentStructureQueriesEventHandler(ctx);
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
            var document = new Domain.Documents.Document(new DocumentPath("path1/content1.txt"), "text/plain", 5, Hash.CalculateHash(new MemoryStream(new byte[] { 72, 101, 108, 108, 111 })));
            var ev = document.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev as DocumentCreatedEvent));
            Assert.IsTrue(await ctx.Documents.AnyAsync(f => f.Path == "path1/content1.txt"));
            Assert.IsTrue(await ctx.Containers.AnyAsync(f => f.Path == "path1"));
        }

        [TestMethod]
        public async Task すでに存在するディレクトリのパスに対して読み取りモデルのファイルを追加する()
        {
            var document1 = new Domain.Documents.Document(new DocumentPath("path1/subpath1/content1.txt"), "text/plain", 5, Hash.CalculateHash(new MemoryStream(new byte[] { 72, 101, 108, 108, 111 })));
            var ev1 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev1 as DocumentCreatedEvent));
            var document2 = new Domain.Documents.Document(new DocumentPath("path1/subpath1/content2.txt"), "text/plain", 6, Hash.CalculateHash(new MemoryStream(new byte[] { 72, 101, 108, 108, 111, 50 })));
            var ev2 = document2.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev2 as DocumentCreatedEvent));
            Assert.IsTrue(await ctx.Documents.AnyAsync(f => f.Path == "path1/subpath1/content2.txt"));
        }

        [TestMethod]
        public async Task 親Containerに1件のみファイルが存在するパスを削除してDocumentとContainerが削除される()
        {
            var document1 = new Domain.Documents.Document(new DocumentPath("path1/subpath1/document1.txt"), "text/plain", 5, Hash.CalculateHash(new MemoryStream(new byte[] { 72, 101, 108, 108, 111 })));
            var ev1 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev1 as DocumentCreatedEvent));
            document1.ClearDomainEvents();
            document1.Delete();
            var ev2 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentDeletedEvent>(ev2 as DocumentDeletedEvent));
            Assert.IsFalse(await ctx.Documents.AnyAsync(f => f.Path == "path1/subpath1/document1.txt"));
            Assert.IsFalse(await ctx.Containers.AnyAsync(f => f.Path == "path1/subpath1"));
        }

        [TestMethod]
        public async Task 親Containerに2件以上のファイルが存在するパスを削除してDocumentは削除されるがContainerは削除されない()
        {
            var document1 = new Domain.Documents.Document(new DocumentPath("path1/subpath1/content1.txt"), "text/plain", 5, Hash.CalculateHash(new MemoryStream(new byte[] { 72, 101, 108, 108, 111 })));
            var ev1 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev1 as DocumentCreatedEvent));
            var document2 = new Domain.Documents.Document(new DocumentPath("path1/subpath1/content2.txt"), "text/plain", 6, Hash.CalculateHash(new MemoryStream(new byte[] { 72, 101, 108, 108, 111, 50 })));
            var ev2 = document2.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev2 as DocumentCreatedEvent));

            document1.ClearDomainEvents();
            document1.Delete();
            var ev3 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentDeletedEvent>(ev3 as DocumentDeletedEvent));
            Assert.IsFalse(await ctx.Documents.AnyAsync(f => f.Path == "path1/subpath1/content1.txt"));
            Assert.IsTrue(await ctx.Containers.AnyAsync(f => f.Path == "path1/subpath1"));
        }

        [TestMethod]
        public async Task ドキュメントが移動した場合移動元のパスにドキュメントはなくなり移動先のパスにドキュメントが生成される()
        {
            var document1 = new Domain.Documents.Document(new DocumentPath("path1/subpath1/content1.txt"), "text/plain", 5, Hash.CalculateHash(new MemoryStream(new byte[] { 72, 101, 108, 108, 111 })));
            var ev1 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev1 as DocumentCreatedEvent));

            document1.ClearDomainEvents();
            document1.MoveTo(new DocumentPath("path2/subpath1/document1.txt"));
            var ev2 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentMovedEvent>(ev2 as DocumentMovedEvent));
            Assert.IsFalse(await ctx.Documents.AnyAsync(f => f.Path == "path1/subpath1/content1.txt"));
            Assert.IsFalse(await ctx.Containers.AnyAsync(f => f.Path == "path1/subpath1"));
            Assert.IsFalse(await ctx.Containers.AnyAsync(f => f.Path == "path1"));
            Assert.IsTrue(await ctx.Documents.AnyAsync(f => f.Path == "path2/subpath1/document1.txt"));
            Assert.IsTrue(await ctx.Containers.AnyAsync(f => f.Path == "path2/subpath1"));
            Assert.IsTrue(await ctx.Containers.AnyAsync(f => f.Path == "path2"));
        }

        [TestMethod]
        public async Task _2件以上のドキュメントが存在するパスでドキュメントが移動した場合ほかのパスには影響がない()
        {
            var document1 = new Domain.Documents.Document(new DocumentPath("path1/subpath1/content1.txt"), "text/plain", 5, Hash.CalculateHash(new MemoryStream(new byte[] { 72, 101, 108, 108, 111 })));
            var ev1 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev1 as DocumentCreatedEvent));
            var document2 = new Domain.Documents.Document(new DocumentPath("path1/subpath1/content2.txt"), "text/plain", 6, Hash.CalculateHash(new MemoryStream(new byte[] { 72, 101, 108, 108, 111, 50 })));
            var ev2 = document2.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev2 as DocumentCreatedEvent));

            document1.ClearDomainEvents();
            document1.MoveTo(new DocumentPath("path2/subpath1/document1.txt"));
            var ev3 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentMovedEvent>(ev3 as DocumentMovedEvent));
            Assert.IsFalse(await ctx.Documents.AnyAsync(f => f.Path == "path1/subpath1/content1.txt"));
            Assert.IsTrue(await ctx.Documents.AnyAsync(f => f.Path == "path1/subpath1/content2.txt"));
            Assert.IsTrue(await ctx.Containers.AnyAsync(f => f.Path == "path1/subpath1"));
            Assert.IsTrue(await ctx.Containers.AnyAsync(f => f.Path == "path1"));
            Assert.IsTrue(await ctx.Documents.AnyAsync(f => f.Path == "path2/subpath1/document1.txt"));
            Assert.IsTrue(await ctx.Containers.AnyAsync(f => f.Path == "path2/subpath1"));
            Assert.IsTrue(await ctx.Containers.AnyAsync(f => f.Path == "path2"));
        }

        [TestMethod]
        public async Task ファイルが更新された場合正しく更新されること()
        {
            var document1 = new Domain.Documents.Document(new DocumentPath("path1/subpath1/content1.txt"), "text/plain", 5, Hash.CalculateHash(new MemoryStream(new byte[] { 72, 101, 108, 108, 111 })));
            var ev1 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev1 as DocumentCreatedEvent));

            document1.ClearDomainEvents();
            document1.Update("application/json", 2, Hash.CalculateHash(new MemoryStream(new byte[] { 123, 125 })));
            var ev2 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentUpdatedEvent>(ev2 as DocumentUpdatedEvent));
            Assert.AreEqual("application/json", (await ctx.Documents.FirstAsync(f => f.Path == "path1/subpath1/content1.txt")).ContentType);
        }
    }
}