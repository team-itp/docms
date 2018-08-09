using Docms.Infrastructure.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
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
            MakeDirectory(basepath);
            sut = new LocalFileStorage(basepath);
        }

        [TestCleanup]
        public void Teardown()
        {
            if (System.IO.Directory.Exists(basepath))
            {
                System.IO.Directory.Delete(basepath, true);
            }
        }

        private void MakeDirectory(string path)
        {
            var fullpath = Path.IsPathFullyQualified(path)
                ? path
                : Path.Combine(basepath, path);
            if (!System.IO.Directory.Exists(fullpath))
            {
                System.IO.Directory.CreateDirectory(fullpath);
            }
        }

        private void DeleteDirectory(string path)
        {
            var fullpath = Path.IsPathFullyQualified(path)
                ? path
                : Path.Combine(basepath, path);
            if (System.IO.Directory.Exists(fullpath))
            {
                System.IO.Directory.Delete(fullpath, true);
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

        private void DeleteFile(string path)
        {
            var fullpath = Path.IsPathFullyQualified(path)
                ? path
                : Path.Combine(basepath, path);
            if (System.IO.File.Exists(fullpath))
            {
                System.IO.File.Delete(fullpath);
            }
        }

        [TestMethod]
        public async Task 存在しないパスに対してGetEntryを実行しnullが戻る()
        {
            Assert.IsNull(await sut.GetEntryAsync("test"));
        }

        [TestMethod]
        public async Task ファイルのパスに対してGetEntryを呼び出してファイルが取得される()
        {
            CreateFile("test", "dir2content1");
            var entry = await sut.GetEntryAsync("test");
            Assert.IsTrue(entry is Files.File);
        }

        [TestMethod]
        public async Task 存在するディレクトリから存在する件数のファイルが返ること()
        {
            MakeDirectory("dir1");
            CreateFile("dir2\\content1.txt", "dir2content1");
            CreateFile("dir2/subdir2/content1.txt", "dir2content1");
            CreateFile("dir2/subdir2\\content2.txt", "dir2content2");
            var dir1 = await sut.GetEntryAsync("dir1") as Files.Directory;
            var dir2 = await sut.GetEntryAsync("dir2") as Files.Directory;
            var subdir2 = await sut.GetEntryAsync("dir2\\subdir2") as Files.Directory;
            Assert.AreEqual(0, (await dir1.GetFilesAsync()).Count());
            Assert.AreEqual(2, (await dir2.GetFilesAsync()).Count());
            Assert.AreEqual(2, (await subdir2.GetFilesAsync()).Count());
        }

        [TestMethod]
        public async Task 存在するファイル情報を取得することができる()
        {
            CreateFile("dir2\\content1.txt", "dir2content1");
            var file = await sut.GetEntryAsync("dir2\\content1.txt") as Files.File;
            var fileProps = await file.GetPropertiesAsync();
            Assert.AreEqual(12, fileProps.Size);
            Assert.IsNotNull(fileProps.Hash);
        }

        [TestMethod]
        public async Task ファイルのストリームがオープン出来ること()
        {
            CreateFile("dir2\\content1.txt", "dir2content1");
            var file = await sut.GetEntryAsync("dir2\\content1.txt") as Files.File;
            using (var fs = await file.OpenAsync())
            using (var reader = new StreamReader(fs, Encoding.UTF8))
            {
                var text = await reader.ReadLineAsync();
                Assert.AreEqual("dir2content1", text);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ディレクトリに対してストリームを開こうとして失敗する()
        {
            CreateFile("test", "test1content");
            var file = (await sut.GetEntryAsync("test")) as Files.File;
            DeleteFile("test");
            await file.OpenAsync();
        }

        [TestMethod]
        public async Task ストリームからファイルに保存できること()
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("dir2content1"));
            ms.Seek(0, SeekOrigin.Begin);
            var dir = await sut.GetDirectoryAsync("dir2") as Files.Directory;
            var fileProps = await dir.SaveAsync("content1.txt", ms);
            Assert.AreEqual("dir2content1", System.IO.File.ReadAllText(Path.Combine(basepath, "dir2\\content1.txt")));
            Assert.AreEqual(12, fileProps.Size);
            Assert.IsNotNull(fileProps.Hash);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ディレクトリに対して保存しようとして失敗する()
        {
            MakeDirectory("test");
            var dir = await sut.GetDirectoryAsync(".");
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("testcontent"));
            ms.Seek(0, SeekOrigin.Begin);
            await dir.SaveAsync("test", ms);
        }

        [TestMethod]
        public async Task ファイルを削除できること()
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("dir2content1"));
            ms.Seek(0, SeekOrigin.Begin);
            var dir = await sut.GetDirectoryAsync("dir2");
            var fileProps = await dir.SaveAsync("content1.txt", ms);
            Assert.IsTrue(System.IO.File.Exists(Path.Combine(basepath, "dir2\\content1.txt")));
            await sut.DeleteAsync(fileProps.File);
            Assert.IsFalse(System.IO.File.Exists(Path.Combine(basepath, "dir2\\content1.txt")));
        }

        [TestMethod]
        public async Task 空のディレクトリを削除できること()
        {
            MakeDirectory("dir2\\subdir2");
            Assert.IsTrue(System.IO.Directory.Exists(Path.Combine(basepath, "dir2\\subdir2")));
            var subdir2 = await sut.GetDirectoryAsync("dir2\\subdir2");
            await sut.DeleteAsync(subdir2);
            Assert.IsFalse(System.IO.Directory.Exists(Path.Combine(basepath, "dir2\\subdir2")));
            Assert.IsTrue(System.IO.Directory.Exists(Path.Combine(basepath, "dir2")));
        }

        [TestMethod]
        public async Task 空ではないディレクトリを削除できること()
        {
            CreateFile("dir2\\subdir2\\content1.txt", "dir2subdir2content1");
            Assert.IsTrue(System.IO.Directory.Exists(Path.Combine(basepath, "dir2\\subdir2")));
            var subdir2 = await sut.GetDirectoryAsync("dir2\\subdir2");
            await sut.DeleteAsync(subdir2);
            Assert.IsFalse(System.IO.Directory.Exists(Path.Combine(basepath, "dir2\\subdir2")));
            Assert.IsTrue(System.IO.Directory.Exists(Path.Combine(basepath, "dir2")));
        }
    }
}
