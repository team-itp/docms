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
            if (!System.IO.Directory.Exists(basepath))
            {
                System.IO.Directory.CreateDirectory(basepath);
            }
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
            var fullpath = Path.Combine(basepath, path);
            if (!System.IO.Directory.Exists(fullpath))
            {
                System.IO.Directory.CreateDirectory(fullpath);
            }
        }

        private void DeleteDirectory(string path)
        {
            var fullpath = Path.Combine(basepath, path);
            if (System.IO.Directory.Exists(fullpath))
            {
                System.IO.Directory.Delete(fullpath, true);
            }
        }

        private void CreateFile(string path, string content)
        {
            var fullpath = Path.Combine(basepath, path);
            MakeDirectory(Path.GetDirectoryName(path));
            System.IO.File.WriteAllText(fullpath, content);
        }

        private void DeleteFile(string path)
        {
            var fullpath = Path.Combine(basepath, path);
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
            var root = await sut.GetEntryAsync("") as Files.Directory;
            var dir1 = await sut.GetEntryAsync("dir1") as Files.Directory;
            var dir2 = await sut.GetEntryAsync("dir2") as Files.Directory;
            var subdir2 = await sut.GetEntryAsync("dir2\\subdir2") as Files.Directory;
            Assert.AreEqual(2, (await root.GetEntriesAsync()).Count());
            Assert.AreEqual(0, (await dir1.GetEntriesAsync()).Count());
            Assert.AreEqual(2, (await dir2.GetEntriesAsync()).Count());
            Assert.AreEqual(2, (await subdir2.GetEntriesAsync()).Count());
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
            var file = await dir.SaveAsync("content1.txt", "text/plain", ms);
            Assert.AreEqual("dir2content1", System.IO.File.ReadAllText(Path.Combine(basepath, "dir2\\content1.txt")));
            Assert.AreEqual("dir2/content1.txt", file.Path.ToString());
        }

        [TestMethod]
        public async Task すでに存在するパスに対してストリームからファイルに保存できること()
        {
            var ms1 = new MemoryStream(Encoding.UTF8.GetBytes("dir2content1"));
            var dir = await sut.GetDirectoryAsync("dir2") as Files.Directory;
            await dir.SaveAsync("content1.txt", "text/plain", ms1);
            Assert.AreEqual("dir2content1", System.IO.File.ReadAllText(Path.Combine(basepath, "dir2\\content1.txt")));
            var ms2 = new MemoryStream(Encoding.UTF8.GetBytes("dir2content1new"));
            var file = await dir.SaveAsync("content1.txt", "text/plain", ms2);
            Assert.AreEqual("dir2/content1.txt", file.Path.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ディレクトリに対して保存しようとして失敗する()
        {
            MakeDirectory("test");
            var dir = await sut.GetDirectoryAsync(".");
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("testcontent"));
            ms.Seek(0, SeekOrigin.Begin);
            await dir.SaveAsync("test", "text/plain", ms);
        }

        [TestMethod]
        public async Task ファイルが移動できること()
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("dir2content1"));
            ms.Seek(0, SeekOrigin.Begin);
            var dir = await sut.GetDirectoryAsync("dir2");
            var file = await dir.SaveAsync("content1.txt", "text/plain", ms);
            Assert.IsTrue(System.IO.File.Exists(Path.Combine(basepath, "dir2\\content1.txt")));
            await sut.MoveAsync(file.Path, new FilePath("content1.txt"));
            Assert.IsFalse(System.IO.File.Exists(Path.Combine(basepath, "dir2\\content1.txt")));
            Assert.IsTrue(System.IO.File.Exists(Path.Combine(basepath, "content1.txt")));
        }

        [TestMethod]
        public async Task 移動左記にディレクトリがない場合でもファイルの移動ができること()
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("dir2content1"));
            ms.Seek(0, SeekOrigin.Begin);
            var dir = await sut.GetDirectoryAsync("dir2");
            var file = await dir.SaveAsync("content1.txt", "text/plain", ms);
            Assert.IsTrue(System.IO.File.Exists(Path.Combine(basepath, "dir2\\content1.txt")));
            await sut.MoveAsync(file.Path, new FilePath("dir1/subdir1/content1.txt"));
            Assert.IsFalse(System.IO.File.Exists(Path.Combine(basepath, "dir2\\content1.txt")));
            Assert.IsTrue(System.IO.File.Exists(Path.Combine(basepath, "dir1/subdir1/content1.txt")));
        }

        [TestMethod]
        public async Task ファイルを削除できること()
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("dir2content1"));
            ms.Seek(0, SeekOrigin.Begin);
            var dir = await sut.GetDirectoryAsync("dir2");
            var file = await dir.SaveAsync("content1.txt", "text/plain", ms);
            Assert.IsTrue(System.IO.File.Exists(Path.Combine(basepath, "dir2\\content1.txt")));
            await sut.DeleteAsync(file);
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
