using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docms.Web.Infrastructure.Docs
{
    [TestClass]
    public class DocmsContextDocumentsRepositoryTests
    {
        [TestMethod]
        public async Task DocmsContextDocumentsRepository‚Éƒ^ƒOî•ñ‚ª0Œ‚Ì‘—Şî•ñ‚ª“o˜^‚Å‚«‚é‚±‚Æ()
        {
            var db = new DocmsContext();
            var sut = new DocmsContextDocumentsRepository(db);
            db.Database.EnsureCreated();

            var document = new Docms.Web.Docs.Document()
            {
                Name = "Test1.pdf",
                Path = "Test1.pdf",
                MediaType = "application/pdf",
                Size = 238,
            };

            await sut.CreateAsync(document);

            var dbDoc = db.Documents.Find(document.Id);
            Assert.AreEqual(document.Name, dbDoc.Name);
            Assert.AreEqual(document.Path, dbDoc.Path);
            Assert.AreEqual(document.MediaType, dbDoc.MediaType);
            Assert.AreEqual(document.Size, dbDoc.Size);
        }
    }
}
