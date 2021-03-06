﻿using Docms.Application.Commands;
using Docms.Application.Tests.Utils;
using Docms.Infrastructure.Files;
using Docms.Infrastructure.Storage.InMemory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Application.Tests
{
    [TestClass]
    public class DocumentCommandTests
    {
        private MockDocumentRepository repository;
        private InMemoryDataStore dataStore;
        private DocumentCommandHandler sut;

        [TestInitialize]
        public void Setup()
        {
            repository = new MockDocumentRepository();
            dataStore = new InMemoryDataStore();
            sut = new DocumentCommandHandler(repository, dataStore);
        }

        [TestMethod]
        public async Task コマンドを発行してドキュメントが作成されること()
        {
            
            await sut.Handle(new CreateOrUpdateDocumentCommand()
            {
                Path = new FilePath("document.txt"),
                Data = await dataStore.CreateAsync(Encoding.UTF8.GetBytes("Hello, world")).ConfigureAwait(false)
            });
            var document = repository.Entities.Single();
            Assert.AreEqual("document.txt", document.Path.ToString());
            Assert.AreEqual("Hello, world", await ReadTextAsync(document.StorageKey));
        }

        [TestMethod]
        public async Task コマンドを発行してドキュメントが更新されること()
        {
            await repository.AddAsync(DocumentUtils.Create("test1/document1.txt", "Hello, world"));
            await sut.Handle(new CreateOrUpdateDocumentCommand()
            {
                Path = new FilePath("test1/document1.txt"),
                Data = await dataStore.CreateAsync(Encoding.UTF8.GetBytes("Hello, new world")).ConfigureAwait(false)
            });
            var document = repository.Entities.Single();
            Assert.AreEqual("test1/document1.txt", document.Path.ToString());
            Assert.AreEqual("Hello, new world", await ReadTextAsync(document.StorageKey));
        }

        [TestMethod]
        public async Task ファイルが存在する場合でもコマンドを発行してドキュメントが作成されること()
        {
            await sut.Handle(new CreateOrUpdateDocumentCommand()
            {
                Path = new FilePath("test1/document1.txt"),
                Data = await dataStore.CreateAsync(Encoding.UTF8.GetBytes("Hello, world")).ConfigureAwait(false),
                ForceCreate = true
            });
            await sut.Handle(new CreateOrUpdateDocumentCommand()
            {
                Path = new FilePath("test1/document1.txt"),
                Data = await dataStore.CreateAsync(Encoding.UTF8.GetBytes("Hello, new world")).ConfigureAwait(false),
                ForceCreate = true
            });
            var doc1 = repository.Entities.FirstOrDefault(e => e.Path == "test1/document1.txt");
            var doc2 = repository.Entities.FirstOrDefault(e => e.Path == "test1/document1(1).txt");
            Assert.IsNotNull(doc1);
            Assert.IsNotNull(doc2);
            Assert.AreEqual("Hello, world", await ReadTextAsync(doc1.StorageKey));
            Assert.AreEqual("Hello, new world", await ReadTextAsync(doc2.StorageKey));
        }

        [TestMethod]
        public async Task 削除済みのファイルが存在する場合でもコマンドを発行してドキュメントが作成されること()
        {
            var document = DocumentUtils.Create("test1/document1.txt", "Hello, world");
            document.Delete();
            await repository.AddAsync(document);
            await sut.Handle(new CreateOrUpdateDocumentCommand()
            {
                Path = new FilePath("test1/document1.txt"),
                Data = await dataStore.CreateAsync(Encoding.UTF8.GetBytes("Hello, new world")).ConfigureAwait(false),
            });
            var doc1 = repository.Entities.FirstOrDefault(e => e.Path == null);
            var doc2 = repository.Entities.FirstOrDefault(e => e.Path == "test1/document1.txt");
            Assert.IsNotNull(doc1);
            Assert.IsNotNull(doc2);
            Assert.AreEqual("Hello, new world", await ReadTextAsync(doc2.StorageKey));
        }

        [TestMethod]
        public async Task コマンドを発行してドキュメントが削除されること()
        {
            await repository.AddAsync(DocumentUtils.Create("test1/document1.txt", "Hello, World"));
            await sut.Handle(new DeleteDocumentCommand()
            {
                Path = new FilePath("test1/document1.txt"),
            });
            Assert.IsNull(await repository.GetAsync("test1/document1.txt"));
        }

        [TestMethod]
        public async Task コマンドを発行してファイルが移動すること()
        {
            await repository.AddAsync(DocumentUtils.Create("test1/document1.txt", "Hello, world"));
            await sut.Handle(new MoveDocumentCommand()
            {
                OriginalPath = new FilePath("test1/document1.txt"),
                DestinationPath = new FilePath("test2/document2.txt"),
            });
            Assert.AreEqual("test2/document2.txt", repository.Entities.First().Path);
        }

        private async Task<string> ReadTextAsync(string key)
        {
            var data = await dataStore.FindAsync(key).ConfigureAwait(false);
            using (var fs = await data.OpenStreamAsync().ConfigureAwait(false))
            using (var sr = new StreamReader(fs))
            {
                return await sr.ReadToEndAsync();
            }
        }
    }
}
