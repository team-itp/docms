using Docms.Domain.Documents;
using Docms.Infrastructure.Files;
using Docms.Infrastructure.Repositories;
using Docms.Infrastructure.Storage;
using Docms.Infrastructure.Storage.InMemory;
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
        private InMemoryDataStore dataStore;
        private CreateOrUpdateDocumentCommandHandler sut;

        [TestInitialize]
        public void Setup()
        {
            repository = new MockDocumentRepository();
            dataStore = new InMemoryDataStore();
            sut = new CreateOrUpdateDocumentCommandHandler(repository, dataStore);
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
                    SizeOfStream = ms.Length,
                });
            }
            var document = repository.Entities.Single();
            Assert.AreEqual("document.txt", document.Path.ToString());
            Assert.AreEqual("Hello, world", await ReadTextAsync(document.StorageKey));
        }

        [TestMethod]
        public async Task コマンドを発行してドキュメントが更新されること()
        {
            await repository.AddAsync(DocumentUtils.Create("test1/document1.txt", "Hello, world"));
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello, new world")))
            {
                await sut.Handle(new CreateOrUpdateDocumentCommand()
                {
                    Path = new FilePath("test1/document1.txt"),
                    Stream = ms,
                    SizeOfStream = ms.Length,
                });
            }
            var document = repository.Entities.Single();
            Assert.AreEqual("test1/document1.txt", document.Path.ToString());
            Assert.AreEqual("Hello, new world", await ReadTextAsync(document.StorageKey));
        }

        [TestMethod]
        public async Task ファイルが存在する場合でもコマンドを発行してドキュメントが作成されること()
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello, world")))
            {
                await sut.Handle(new CreateOrUpdateDocumentCommand()
                {
                    Path = new FilePath("test1/document1.txt"),
                    Stream = ms,
                    SizeOfStream = ms.Length,
                    ForceCreate = true
                });
            }
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello, new world")))
            {
                await sut.Handle(new CreateOrUpdateDocumentCommand()
                {
                    Path = new FilePath("test1/document1.txt"),
                    Stream = ms,
                    SizeOfStream = ms.Length,
                    ForceCreate = true
                });
            }
            var doc1 = repository.Entities.FirstOrDefault(e => e.Path.Value == "test1/document1.txt");
            var doc2 = repository.Entities.FirstOrDefault(e => e.Path.Value == "test1/document1(1).txt");
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
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes("Hello, new world")))
            {
                await sut.Handle(new CreateOrUpdateDocumentCommand()
                {
                    Path = new FilePath("test1/document1.txt"),
                    Stream = ms,
                    SizeOfStream = ms.Length,
                });
            }
            var doc1 = repository.Entities.FirstOrDefault(e => e.Path == null);
            var doc2 = repository.Entities.FirstOrDefault(e => e.Path?.Value == "test1/document1.txt");
            Assert.IsNotNull(doc1);
            Assert.IsNotNull(doc2);
            Assert.AreEqual("Hello, new world", await ReadTextAsync(doc2.StorageKey));
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
