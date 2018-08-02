using Docms.Infrastructure.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Tests
{
    [TestClass]
    public class LocalFileStorageTests
    {
        private string basepath;
        private LocalFileStorage sut;

        [TestInitialize]
        public void Setup()
        {
            basepath = Path.GetFullPath("tmp");
            sut = new LocalFileStorage(basepath);
        }

        [TestCleanup]
        public void Teardown()
        {
            if (Directory.Exists(basepath))
            {
                Directory.Delete(basepath, true);
            }
        }

        [TestMethod]
        public async Task 存在しないディレクトリより0件のファイルが返ること()
        {
            Assert.AreEqual(0, (await sut.GetFilesAsync("test")).Count());
        }

        [TestMethod]
        public async Task 存在するディレクトリより存在する件数のファイルが返ること()
        {
            var dir1 = Path.Combine(basepath, "dir1");
            Directory.CreateDirectory(dir1);
            var dir2 = Path.Combine(basepath, "dir2");
            var subdir2 = Path.Combine(basepath, "dir2\\subdir2");
            Directory.CreateDirectory(subdir2);
            System.IO.File.WriteAllText(Path.Combine(dir2, "content1.txt"), "dir2content1");
            System.IO.File.WriteAllText(Path.Combine(subdir2, "content1.txt"), "subdir2content1");
            System.IO.File.WriteAllText(Path.Combine(subdir2, "content2.txt"), "subdir2content2");
            Assert.AreEqual(0, (await sut.GetFilesAsync("dir1")).Count());
            Assert.AreEqual(2, (await sut.GetFilesAsync("dir2")).Count());
            Assert.AreEqual(2, (await sut.GetFilesAsync("dir2\\subdir2")).Count());
        }
    }
}
