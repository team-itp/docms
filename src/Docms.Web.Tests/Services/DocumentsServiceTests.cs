using Docms.Web.Data;
using Docms.Web.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Web.Services
{
    [TestClass]
    public class DocumentsServiceTests
    {
        [TestMethod]
        public async Task ファイルが追加された場合新たにドキュメント情報が作成される()
        {
            var db = new DocmsDbContext(TestUtils.CreateContext());
            var sut = new DocumentsService(db);

            var newfile = TestUtils.DocumentUrlForPath("Test1.txt");
            await sut.CreateAsync(newfile);

            Assert.AreEqual(1, db.Documents.Count());
        }

        [TestMethod]
        public async Task ファイルにタグを追加できる()
        {
            var db = new DocmsDbContext(TestUtils.CreateContext());
            var sut = new DocumentsService(db);

            var newfile = TestUtils.DocumentUrlForPath("Test1.txt");
            await sut.CreateAsync(newfile);
            await sut.AddTagsAsync(newfile, new List<string>() 
            {
                "Tag1",
                "Tag2",
            });

            Assert.AreEqual(1, db.Documents.Count());
            Assert.AreEqual(2, db.DocumentTags.Count());
        }
    }
}
