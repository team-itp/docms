﻿using Docms.Infrastructure.Files;
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
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ファイルに対してファイルの取得を実行しエラーとなる()
        {
            CreateFile("test", "dir2content1");
            await sut.GetFilesAsync("test");
        }

        [TestMethod]
        public async Task 存在するディレクトリから存在する件数のファイルが返ること()
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
            var file = await sut.GetPropertiesAsync("dir2\\content1.txt");
            Assert.AreEqual(12, file.Size);
            Assert.IsNotNull(file.Sha1Hash);
        }

        [TestMethod]
        public async Task ファイルのストリームがオープン出来ること()
        {
            CreateFile("dir2\\content1.txt", "dir2content1");
            using (var fs = await sut.OpenAsync("dir2\\content1.txt"))
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
            MakeDirectory("test");
            await sut.OpenAsync("test");
        }

        [TestMethod]
        public async Task ストリームからファイルに保存できること()
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("dir2content1"));
            ms.Seek(0, SeekOrigin.Begin);
            var file = await sut.SaveAsync("dir2\\content1.txt", ms);
            Assert.AreEqual("dir2content1", System.IO.File.ReadAllText(Path.Combine(basepath, "dir2\\content1.txt")));
            Assert.AreEqual(12, file.Size);
            Assert.IsNotNull(file.Sha1Hash);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task ディレクトリに対して保存しようとして失敗する()
        {
            MakeDirectory("test");
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("testcontent"));
            ms.Seek(0, SeekOrigin.Begin);
            await sut.SaveAsync("test", ms);
        }

        [TestMethod]
        public async Task ファイルを削除できること()
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("dir2content1"));
            ms.Seek(0, SeekOrigin.Begin);
            await sut.SaveAsync("dir2\\content1.txt", ms);
            Assert.IsTrue(System.IO.File.Exists(Path.Combine(basepath, "dir2\\content1.txt")));
            await sut.DeleteAsync("dir2\\content1.txt");
            Assert.IsFalse(System.IO.File.Exists(Path.Combine(basepath, "dir2\\content1.txt")));
        }

        [TestMethod]
        public async Task ディレクトリを削除できること()
        {
            MakeDirectory("dir2\\subdir2");
            Assert.IsTrue(System.IO.Directory.Exists(Path.Combine(basepath, "dir2\\subdir2")));
            await sut.DeleteAsync("dir2\\subdir2");
            Assert.IsFalse(System.IO.Directory.Exists(Path.Combine(basepath, "dir2\\subdir2")));
            Assert.IsTrue(System.IO.Directory.Exists(Path.Combine(basepath, "dir2")));
        }
    }
}
