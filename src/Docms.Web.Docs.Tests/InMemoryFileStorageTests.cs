using Docms.Web.Docs.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Web.Docs
{
    [TestClass]
    public class InMemoryFileStorageTests
    {
        [TestMethod]
        public async Task InMemoryFileStorageにファイルを登録できる()
        {
            const string path = "Test/File1.txt";

            var sut = new InMemoryFileStorage();
            var before = DateTime.Now;
            var fileInfo = await sut.SaveAsync("Test/File1.txt", new MemoryStream(Encoding.UTF8.GetBytes("Hello, World")));
            var after = DateTime.Now;

            Assert.AreEqual("text/plain", fileInfo.MediaType);
            Assert.AreEqual(path, fileInfo.Path);
            Assert.AreEqual("File1.txt", fileInfo.Name);
            Assert.AreEqual(Encoding.UTF8.GetByteCount("Hello, World"), fileInfo.Size);
            Assert.IsTrue(before < fileInfo.Created && fileInfo.Created < after);
            Assert.IsTrue(before < fileInfo.Modified && fileInfo.Modified < after);
        }

        [TestMethod]
        public async Task InMemoryFileStorageにファイルが登録済みの場合メタ情報が取得できる()
        {
            const string path = "Test/File1.txt";

            var sut = new InMemoryFileStorage();
            var before = DateTime.Now;
            await sut.SaveAsync(path, new MemoryStream(Encoding.UTF8.GetBytes("Hello, World")));
            var after = DateTime.Now;

            var fileInfo = await sut.GetFileInfoAsync(path);

            Assert.AreEqual("text/plain", fileInfo.MediaType);
            Assert.AreEqual("Test/File1.txt", fileInfo.Path);
            Assert.AreEqual("File1.txt", fileInfo.Name);
            Assert.AreEqual(Encoding.UTF8.GetByteCount("Hello, World"), fileInfo.Size);
        }
    }
}
