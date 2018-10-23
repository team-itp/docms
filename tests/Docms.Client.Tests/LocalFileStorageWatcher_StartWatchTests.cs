using Docms.Client.FileStorage;
using Docms.Client.FileTrees;
using Docms.Client.SeedWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class LocalFileStorageWatcher_StartWatchTests
    {
        private string _watchingPath;
        private InternalFileTree fileTree;
        private LocalFileStorageWatcher sut;

        [TestInitialize]
        public void Setup()
        {
            _watchingPath = Path.GetFullPath("tmp" + Guid.NewGuid().ToString());
            fileTree = new InternalFileTree();
            sut = new LocalFileStorageWatcher(_watchingPath, fileTree);
        }

        [TestCleanup]
        public async Task Teardown()
        {
            await sut.StopWatch();
            if (Directory.Exists(_watchingPath))
            {
                Directory.Delete(_watchingPath, true);
            }
        }

        private void CreateDirectory(string path)
        {
            var fullpath = Path.Combine(_watchingPath, path);
            if (!Directory.Exists(fullpath))
            {
                Directory.CreateDirectory(fullpath);
            }
        }

        private void CreateFile(string path, string content)
        {
            var fullpath = Path.Combine(_watchingPath, path);
            CreateDirectory(Path.GetDirectoryName(fullpath));
            File.WriteAllBytes(fullpath, Encoding.UTF8.GetBytes(content));
        }

        private void CreateFile(string path, string content, FileAttributes attr)
        {
            var fullpath = Path.Combine(_watchingPath, path);
            CreateDirectory(Path.GetDirectoryName(fullpath));
            File.WriteAllBytes(fullpath, Encoding.UTF8.GetBytes(content));
            File.SetAttributes(fullpath, attr);
        }

        [TestMethod]
        public async Task ルートディレクトリにディレクトリのみが存在する場合ディレクトリは登録されない()
        {
            CreateDirectory("dir1");
            await sut.StartWatch();
            Assert.IsNull(fileTree.GetDirectory(new PathString("dir1")));
        }

        [TestMethod]
        public async Task ルートディレクトリにファイルが存在する場合正しくファイルが登録される()
        {
            CreateFile("content1.txt", "content1.txt");
            await sut.StartWatch();
            Assert.IsNotNull(fileTree.GetFile(new PathString("content1.txt")));
        }

        [TestMethod]
        public async Task サブディレクトリにファイルが存在する場合正しくファイルが登録される()
        {
            CreateFile("dir1/content1.txt", "dir1/content1.txt");
            await sut.StartWatch();
            Assert.IsNotNull(fileTree.GetDirectory(new PathString("dir1")));
            Assert.IsNotNull(fileTree.GetFile(new PathString("dir1/content1.txt")));
        }

        [TestMethod]
        public async Task 隠しファイルとシステムファイルは登録されない()
        {
            CreateFile("dir1/content1.txt", "dir1/content1.txt", FileAttributes.Hidden);
            CreateFile("dir1/content2.txt", "dir1/content2.txt", FileAttributes.System);
            await sut.StartWatch();
            Assert.IsNull(fileTree.GetDirectory(new PathString("dir1")));
            Assert.IsNull(fileTree.GetFile(new PathString("dir1/content1.txt")));
            Assert.IsNull(fileTree.GetFile(new PathString("dir1/content2.txt")));
        }
    }
}