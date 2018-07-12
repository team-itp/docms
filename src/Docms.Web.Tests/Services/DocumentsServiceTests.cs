using Docms.Web.Data;
using Docms.Web.Services;
using Docms.Web.Tests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Tests.Services
{
    [TestClass]
    public class DocumentsServiceTests
    {
        private DocmsDbContext db;
        private TagsService ts;
        private DocumentsService sut;

        [TestInitialize]
        public void Setup()
        {
            db = new DocmsDbContext(TestUtils.CreateContext());
            db.Tags.RemoveRange(db.Tags);
            db.TagMeta.RemoveRange(db.TagMeta);
            db.SaveChanges();
            ts = new TagsService(db);
            sut = new DocumentsService(db, ts);
        }

        [TestCleanup]
        public void Teardown()
        {
            db.Dispose();
        }

        [TestMethod]
        public async Task 新たにドキュメント情報を作成する()
        {
            var blobUri = TestUtils.DocumentUrlForPath(Guid.NewGuid().ToString());
            await sut.CreateAsync(blobUri, "Test1.txt", "USERACCOUNTNAME");

            Assert.AreEqual(1, db.Documents.Count());
        }

        [TestMethod]
        public async Task 新たにドキュメント情報をタグ付きで作成する()
        {
            var blobUri = TestUtils.DocumentUrlForPath(Guid.NewGuid().ToString());
            await sut.CreateAsync(blobUri, "Test1.txt", "USERACCOUNTNAME", new List<string>()
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
            var blobUri = TestUtils.DocumentUrlForPath(Guid.NewGuid().ToString());
            var docId = await sut.CreateAsync(blobUri, "Test1.txt", "USERACCOUNTNAME");
            await sut.AddTagsAsync(docId, new List<string>()
            {
                "Tag1",
                "Tag2",
            }, "USERACCOUNTNAME");

            Assert.AreEqual(1, db.Documents.Count());
            Assert.AreEqual(2, db.Tags.Count());
            Assert.AreEqual(2, db.DocumentTags.Count());
        }

        [TestMethod]
        public async Task ドキュメント情報の名前を変更できる()
        {
            var blobUri = TestUtils.DocumentUrlForPath(Guid.NewGuid().ToString());
            var docId = await sut.CreateAsync(blobUri, "Test1.txt", "USERACCOUNTNAME");
            await sut.UpdateFileNameAsync(docId, "Test2.txt", "USERACCOUNTNAME");

            Assert.AreEqual("Test2.txt", (await db.Documents.FindAsync(docId)).FileName);
        }

        [TestMethod]
        public async Task ドキュメント情報に新規にタグを追加できる()
        {
            var blobUri = TestUtils.DocumentUrlForPath(Guid.NewGuid().ToString());
            var docId = await sut.CreateAsync(blobUri, "Test1.txt", "USERACCOUNTNAME");
            await sut.AddTagsAsync(docId, new List<string>()
            {
                "Tag1",
                "Tag2",
            }, "USERACCOUNTNAME");

            Assert.AreEqual(1, db.Documents.Count());
            Assert.AreEqual(2, db.Tags.Count());
            Assert.AreEqual(2, db.DocumentTags.Count());
        }

        [TestMethod]
        public async Task ドキュメント情報からタグを削除できる()
        {
            var blobUri = TestUtils.DocumentUrlForPath(Guid.NewGuid().ToString());
            var docId = await sut.CreateAsync(blobUri, "Test1.txt", "USERACCOUNTNAME", new List<string>()
            {
                "Tag1",
                "Tag2",
                "Tag3",
            });

            var tagId = db.Tags.FirstOrDefault(e => e.Name == "Tag2").Id;
            await sut.RemoveTagsAsync(docId, new List<int>()
            {
                tagId
            }, "USERACCOUNTNAME");

            Assert.AreEqual(1, db.Documents.Count());
            Assert.AreEqual(3, db.Tags.Count());
            Assert.AreEqual(2, db.DocumentTags.Count());
        }

        [TestMethod]
        public async Task ドキュメント情報にカテゴリーつきで新規にタグを追加できる()
        {
            var blobUri = TestUtils.DocumentUrlForPath(Guid.NewGuid().ToString());
            var docId = await sut.CreateAsync(blobUri, "Test1.txt", "USERACCOUNTNAME");
            await sut.AddTagWithCategoryAsync(docId, "Tag1", "Category1", "USERACCOUNTNAME");

            Assert.AreEqual(1, db.Documents.Count());
            Assert.AreEqual(1, db.Tags.Count());
            Assert.AreEqual(1, db.Tags.Include(e=> e.Metadata).Where(e => e.Metadata.Any(m => m.MetaKey == "category")).Count());
            Assert.AreEqual(1, db.DocumentTags.Count());
        }
    }
}
