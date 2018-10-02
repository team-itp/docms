﻿using Docms.Domain.Documents;
using Docms.Infrastructure.DataStores;
using Docms.Infrastructure.Files;
using Docms.Web.Application.Commands;
using Docms.Web.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Web.Tests
{
    [TestClass]
    public class CreateOrUpdateDocumentCommandTests
    {
        private MockDocumentRepository repository;
        private LocalFileStorage localFileStorage;
        private InMemoryTemporaryStore temporaryStore;
        private CreateOrUpdateDocumentCommandHandler sut;

        [TestInitialize]
        public void Setup()
        {
            repository = new MockDocumentRepository();
            localFileStorage = new LocalFileStorage("tmp");
            temporaryStore = new InMemoryTemporaryStore();
            sut = new CreateOrUpdateDocumentCommandHandler(repository, localFileStorage, temporaryStore);
        }

        [TestCleanup]
        public void Teardown()
        {
            if (System.IO.Directory.Exists("tmp"))
            {
                System.IO.Directory.Delete("tmp", true);
            }
        }

        [TestMethod]
        public async Task コマンドを発行してドキュメントが作成されること()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello, world")))
            {
                await sut.Handle(new CreateOrUpdateDocumentCommand()
                {
                    Path = new FilePath("document.txt"),
                    Stream = ms,
                });
            }
            Assert.AreEqual(1, repository.Entities.Count);
            Assert.AreEqual("Hello, world", await ReadTextAsync("document.txt"));
        }

        [TestMethod]
        public async Task コマンドを発行してドキュメントが更新されること()
        {
            var dir = await localFileStorage.GetDirectoryAsync("test1");
            var bytes = Encoding.UTF8.GetBytes("Hello, world");
            await dir.SaveAsync("document1.txt", "text/plain", new MemoryStream(bytes));
            await repository.AddAsync(new Document(new DocumentPath("test1/document1.txt"), "text/plain", bytes.Length, Hash.CalculateHash(bytes)));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello, new world")))
            {
                await sut.Handle(new CreateOrUpdateDocumentCommand()
                {
                    Path = new FilePath("test1/document1.txt"),
                    Stream = ms,
                });
            }
            Assert.AreEqual("test1/document1.txt", repository.Entities.First().Path.Value);
            Assert.AreEqual("Hello, new world", await ReadTextAsync("test1/document1.txt"));
        }

        [TestMethod]
        public async Task ファイルが存在する場合でもコマンドを発行してドキュメントが作成されること()
        {
            var dir = await localFileStorage.GetDirectoryAsync("test1");
            var bytes = Encoding.UTF8.GetBytes("Hello, world");
            await dir.SaveAsync("document1.txt", "text/plain", new MemoryStream(bytes));
            await repository.AddAsync(new Document(new DocumentPath("test1/document1.txt"), "text/plain", bytes.Length, Hash.CalculateHash(bytes)));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello, new world")))
            {
                await sut.Handle(new CreateOrUpdateDocumentCommand()
                {
                    Path = new FilePath("test1/document1.txt"),
                    Stream = ms,
                    ForceCreate = true
                });
            }
            Assert.IsNotNull(repository.Entities.FirstOrDefault(e => e.Path.Value == "test1/document1.txt"));
            Assert.IsNotNull(repository.Entities.FirstOrDefault(e => e.Path.Value == "test1/document1(1).txt"));
            Assert.AreEqual("Hello, world", await ReadTextAsync("test1/document1.txt"));
            Assert.AreEqual("Hello, new world", await ReadTextAsync("test1/document1(1).txt"));
        }

        [TestMethod]
        public async Task 削除済みのファイルが存在する場合でもコマンドを発行してドキュメントが作成されること()
        {
            var bytes = Encoding.UTF8.GetBytes("Hello, world");
            var document = new Document(new DocumentPath("test1/document1.txt"), "text/plain", bytes.Length, Hash.CalculateHash(bytes));
            document.Delete();
            await repository.AddAsync(document);
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello, new world")))
            {
                await sut.Handle(new CreateOrUpdateDocumentCommand()
                {
                    Path = new FilePath("test1/document1.txt"),
                    Stream = ms
                });
            }
            Assert.IsNotNull(repository.Entities.FirstOrDefault(e => e.Path == null));
            Assert.IsNotNull(repository.Entities.Skip(1).FirstOrDefault(e => e.Path.Value == "test1/document1.txt"));
            Assert.AreEqual("Hello, new world", await ReadTextAsync("test1/document1.txt"));
        }

        private async Task<string> ReadTextAsync(string filePath)
        {
            var file = (await localFileStorage.GetEntryAsync(filePath).ConfigureAwait(false)) as Infrastructure.Files.File;
            using (var fs = await file.OpenAsync().ConfigureAwait(false))
            using (var sr = new StreamReader(fs))
            {
                return await sr.ReadToEndAsync();
            }
        }
    }
}
