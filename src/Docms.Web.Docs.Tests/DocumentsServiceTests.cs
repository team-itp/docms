using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Web.Docs.Mocks
{
    [TestClass]
    public class DocumentsServiceTests
    {
        private InMemoryFileStorage _storage;
        private InMemoryDocumentsRepository _docRepo;
        private InMemoryTagsRepository _tagRepo;
        private DocumentsService sut;

        [TestInitialize]
        public void Setup()
        {
            _storage = new InMemoryFileStorage();
            _docRepo = new InMemoryDocumentsRepository();
            _tagRepo = new InMemoryTagsRepository();
            sut = new DocumentsService(_storage, _docRepo, _tagRepo);
        }

        [TestMethod]
        public async Task ファイルを新たに作成するとDocumentStorageとDocumentRepositoryにドキュメントが作成される()
        {
            await sut.CreateAsync("Test/File1.txt", new MemoryStream(Encoding.UTF8.GetBytes("Hello, World")), Array.Empty<Tag>(), new UserInfo()
            {
                Id = 1,
                Name = "UserName"
            });

            var document = await _docRepo.FindAsync(1);
            Assert.AreEqual(1, document.Id);
            Assert.AreEqual("File1.txt", document.Name);
        }
    }
}
