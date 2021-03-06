﻿using Docms.Application.DomainEventHandlers;
using Docms.Application.Tests.Utils;
using Docms.Domain.Documents;
using Docms.Domain.Documents.Events;
using Docms.Infrastructure;
using Docms.Infrastructure.MediatR;
using Docms.Infrastructure.Storage.InMemory;
using Docms.Queries.DocumentHistories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Application.Tests
{
    [TestClass]
    public class DocumentHistoriesQueriesEventHandlerTests
    {
        private DocmsContext ctx;
        private DocumentHistoriesQueriesEventHandler sut;

        [TestInitialize]
        public void Setup()
        {
            ctx = new DocmsContext(new DbContextOptionsBuilder<DocmsContext>()
                .UseInMemoryDatabase("DocumentHistoriesQueriesEventHandlerTests")
                .Options, new MockMediator());
            sut = new DocumentHistoriesQueriesEventHandler(ctx);
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
            var document = DocumentUtils.Create("path1/content1.txt", "testdata");
            var ev = document.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev as DocumentCreatedEvent));
            Assert.AreEqual(1, await ctx.DocumentHistories.Where(f => f.Path == "path1/content1.txt" && f.Discriminator == DocumentHistoryDiscriminator.DocumentCreated).CountAsync());
        }

        [TestMethod]
        public async Task ドキュメント削除のイベントが登録される()
        {
            var document1 = DocumentUtils.Create("path1/subpath1/document1.txt", "Hello, World");
            var ev1 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev1 as DocumentCreatedEvent));
            document1.ClearDomainEvents();
            document1.Delete();
            var ev2 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentDeletedEvent>(ev2 as DocumentDeletedEvent));
            Assert.AreEqual(1, await ctx.DocumentHistories.Where(f => f.Path == "path1/subpath1/document1.txt" && f.Discriminator == DocumentHistoryDiscriminator.DocumentCreated).CountAsync());
            Assert.AreEqual(1, await ctx.DocumentHistories.Where(f => f.Path == "path1/subpath1/document1.txt" && f.Discriminator == DocumentHistoryDiscriminator.DocumentDeleted).CountAsync());
        }

        [TestMethod]
        public async Task ドキュメント移動のイベントが登録される()
        {
            var document1 = DocumentUtils.Create("path1/subpath1/content1.txt", "Hello, World");
            var ev1 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev1 as DocumentCreatedEvent));

            document1.ClearDomainEvents();
            document1.MoveTo(new DocumentPath("path2/subpath1/document1.txt"));
            var ev2 = document1.DomainEvents.First();

            await sut.Handle(new DomainEventNotification<DocumentMovedEvent>(ev2 as DocumentMovedEvent));
            Assert.AreEqual(1, await ctx.DocumentHistories.Where(f => f.Path == "path1/subpath1/content1.txt" && f.Discriminator == DocumentHistoryDiscriminator.DocumentCreated).CountAsync());
            Assert.AreEqual(1, await ctx.DocumentHistories.Where(f => f.Path == "path1/subpath1/content1.txt" && f.Discriminator == DocumentHistoryDiscriminator.DocumentDeleted).CountAsync());
            Assert.AreEqual(1, await ctx.DocumentHistories.Where(f => f.Path == "path2/subpath1/document1.txt" && f.Discriminator == DocumentHistoryDiscriminator.DocumentCreated).CountAsync());
        }

        [TestMethod]
        public async Task ドキュメント更新のイベントが登録される()
        {
            var document1 = DocumentUtils.Create("path1/subpath1/content1.txt", "Hello, World");
            var ev1 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentCreatedEvent>(ev1 as DocumentCreatedEvent));

            document1.ClearDomainEvents();
            document1.Update("application/json", InMemoryData.Create("storagekey2", Encoding.UTF8.GetBytes("Hello, New World")));
            var ev2 = document1.DomainEvents.First();
            await sut.Handle(new DomainEventNotification<DocumentUpdatedEvent>(ev2 as DocumentUpdatedEvent));
            Assert.AreEqual(1, await ctx.DocumentHistories.Where(f => f.Path == "path1/subpath1/content1.txt" && f.Discriminator == DocumentHistoryDiscriminator.DocumentCreated).CountAsync());
            Assert.AreEqual(1, await ctx.DocumentHistories.Where(f => f.Path == "path1/subpath1/content1.txt" && f.Discriminator == DocumentHistoryDiscriminator.DocumentUpdated).CountAsync());
        }
    }
}
