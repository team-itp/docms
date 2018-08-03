using Docms.Infrastructure.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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

        private void MakeDirectory(string path)
        {
            var fullpath = Path.IsPathFullyQualified(path)
                ? path
                : Path.Combine(basepath, path);
            if (!Directory.Exists(fullpath))
            {
                Directory.CreateDirectory(fullpath);
            }
        }

        private void CreateFile(string path, string content)
        {
            var fullpath = Path.IsPathFullyQualified(path)
                ? path
                : Path.Combine(basepath, path);
            MakeDirectory(Path.GetDirectoryName(fullpath));
            System.IO.File.WriteAllText(fullpath, content);
        }

        [TestMethod]
        public async Task 存在しないディレクトリより0件のファイルが返ること()
        {
            Assert.AreEqual(0, (await sut.GetFilesAsync("test")).Count());
        }

        [TestMethod]
        public async Task 存在するディレクトリより存在する件数のファイルが返ること()
        {
            MakeDirectory("dir1");
            CreateFile("dir2\\content1.txt", "dir2content1");
            CreateFile("dir2/subdir2/content1.txt", "dir2content1");
            CreateFile("dir2/subdir2\\content2.txt", "dir2content2");
            Assert.AreEqual(0, (await sut.GetFilesAsync("dir1")).Count());
            Assert.AreEqual(2, (await sut.GetFilesAsync("dir2")).Count());
            Assert.AreEqual(2, (await sut.GetFilesAsync("dir2\\subdir2")).Count());
        }

        [TestMethod]
        public async Task 存在するファイル情報を取得することができる()
        {
            CreateFile("dir2\\content1.txt", "dir2content1");
            var file = await sut.GetFileAsync("dir2\\content1.txt");
            Assert.AreEqual("dir2/content1.txt", file.Path.ToString());
            Assert.AreEqual(12, file.Size);
            Assert.IsNotNull(file.Sha1Hash);
        }
    }
}
