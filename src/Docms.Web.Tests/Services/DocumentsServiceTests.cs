using Docms.Web.Data;
using Docms.Web.Services;
using Docms.Web.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Tests.Services
{
    [TestClass]
    public class DocumentsServiceTests
    {
        private DocmsDbContext db;
        private DocumentsService sut;

        [TestInitialize]
        public void Setup()
        {
            db = new DocmsDbContext(TestUtils.CreateContext());
            sut = new DocumentsService(db);
        }

        [TestCleanup]
        public void Teardown()
        {
            db.Dispose();
        }

        [TestMethod]
        public async Task 新たにドキュメント情報を作成する()
        {
            var newfile = TestUtils.DocumentUrlForPath("Test1.txt");
            await sut.CreateAsync(newfile);

            Assert.AreEqual(1, db.Documents.Count());
        }

        [TestMethod]
        public async Task 新たにドキュメント情報をタグ付きで作成する()
        {
            var newfile = TestUtils.DocumentUrlForPath("Test1.txt");
            await sut.CreateAsync(newfile, new List<string>()
            {
                "Tag1",
                "Tag2",
            });

            Assert.AreEqual(1, db.Documents.Count());
            Assert.AreEqual(2, db.Tags.Count());
            Assert.AreEqual(2, db.DocumentTags.Count());
        }

        [TestMethod]
        public async Task ドキュメント情報に既存のタグを追加できる()
        {
            await db.Tags.AddRangeAsync(new[] { new Tag() { Name = "Tag1" }, new Tag() { Name = "Tag2" } });
            var newfile = TestUtils.DocumentUrlForPath("Test1.txt");
            await sut.CreateAsync(newfile);
            await sut.AddTagsAsync(newfile, new List<string>()
            {
                "Tag1",
                "Tag2",
            });

            Assert.AreEqual(1, db.Documents.Count());
            Assert.AreEqual(2, db.Tags.Count());
            Assert.AreEqual(2, db.DocumentTags.Count());
        }

        [TestMethod]
        public async Task ドキュメント情報に新規にタグを追加できる()
        {
            var newfile = TestUtils.DocumentUrlForPath("Test1.txt");
            await sut.CreateAsync(newfile);
            await sut.AddTagsAsync(newfile, new List<string>()
            {
                "Tag1",
                "Tag2",
            });

            Assert.AreEqual(1, db.Documents.Count());
            Assert.AreEqual(2, db.Tags.Count());
            Assert.AreEqual(2, db.DocumentTags.Count());
        }

        [TestMethod]
        public async Task ドキュメント情報からタグを削除できる()
        {
            var newfile = TestUtils.DocumentUrlForPath("Test1.txt");
            await sut.CreateAsync(newfile, new List<string>()
            {
                "Tag1",
                "Tag2",
                "Tag3",
            });
            await sut.RemoveTagsAsync(newfile, new List<string>()
            {
                "Tag2"
            });

            Assert.AreEqual(1, db.Documents.Count());
            Assert.AreEqual(3, db.Tags.Count());
            Assert.AreEqual(2, db.DocumentTags.Count());
        }
    }
}
