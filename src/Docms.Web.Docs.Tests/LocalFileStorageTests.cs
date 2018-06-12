using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Web.Docs
{
    [TestClass]
    public class LocalFileStorageTests
    {
        [TestInitialize]
        public void Setup()
        {
            if (Directory.Exists("Data"))
            {
                foreach (var file in Directory.GetFiles("Data", "*", SearchOption.AllDirectories))
                {
                    File.Delete(file);
                }
                Directory.Delete("Data", true);
            }
        }

        [TestMethod]
        public async Task LocalFileStorageにファイルを登録すると所定のディレクトリにファイルが作成される()
        {
            var sut = new LocalFileStorage(Path.GetFullPath("Data"));

            var before = DateTime.Now;
            var fileInfo = await sut.SaveAsync("Test\\File1.txt", new MemoryStream(Encoding.UTF8.GetBytes("Hello, World")));
            var after = DateTime.Now;

            Assert.AreEqual("text/plain", fileInfo.MediaType);
            Assert.AreEqual("Test/File1.txt", fileInfo.Path);
            Assert.AreEqual("File1.txt", fileInfo.Name);
            Assert.AreEqual(Encoding.UTF8.GetByteCount("Hello, World"), fileInfo.Size);
            Assert.IsTrue(before < fileInfo.CreatedAt && fileInfo.CreatedAt < after);
            Assert.IsTrue(before < fileInfo.ModifiedAt && fileInfo.ModifiedAt < after);

            Assert.IsTrue(File.Exists("Data\\Test\\File1.txt"));
            Assert.AreEqual("Hello, World", File.ReadAllText("Data\\Test\\File1.txt"));
        }

        [TestMethod]
        public async Task LocalFileStorageにファイルが登録済みの場合メタ情報が取得できる()
        {
            var path = "Test\\File1.txt";
            var sut = new LocalFileStorage(Path.GetFullPath("Data"));
            Directory.CreateDirectory("Data\\Test");

            var before = DateTime.Now;
            File.WriteAllBytes("Data\\Test\\File1.txt", Encoding.UTF8.GetBytes("Hello, World"));
            var after = DateTime.Now;

            var fileInfo = await sut.GetFileInfoAsync(path).ConfigureAwait(false);

            Assert.AreEqual("text/plain", fileInfo.MediaType);
            Assert.AreEqual("Test/File1.txt", fileInfo.Path);
            Assert.AreEqual("File1.txt", fileInfo.Name);
            Assert.AreEqual(Encoding.UTF8.GetByteCount("Hello, World"), fileInfo.Size);
        }
    }
}
