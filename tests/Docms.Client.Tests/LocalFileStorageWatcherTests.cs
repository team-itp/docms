using Docms.Client.FileStorage;
using Docms.Client.FileTrees;
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
    public class LocalFileStorageWatcherTests
    {
        private string _watchingPath;
        private LocalFileStorageWatcher sut;

        [TestInitialize]
        public async Task Setup()
        {
            _watchingPath = Path.GetFullPath("tmp");
            sut = new LocalFileStorageWatcher(_watchingPath);
            await sut.StartWatch();
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

        private void CreateFile(string path, string content)
        {
            var fullpath = Path.Combine(_watchingPath, path);
            if (!Directory.Exists(Path.GetDirectoryName(fullpath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullpath));
            }
            File.WriteAllBytes(fullpath, Encoding.UTF8.GetBytes(content));
            Thread.Sleep(5);
        }

        private void UpdateFile(string path, string content)
        {
            var fullpath = Path.Combine(_watchingPath, path);
            File.WriteAllBytes(fullpath, Encoding.UTF8.GetBytes(content));
            Thread.Sleep(5);
        }

        private void MoveFile(string fromPath, string toPath)
        {
            var fullpathFrom = Path.Combine(_watchingPath, fromPath);
            var fullpathTo = Path.Combine(_watchingPath, toPath);
            if (!Directory.Exists(Path.GetDirectoryName(fullpathTo)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullpathTo));
            }
            File.Move(fullpathFrom, fullpathTo);
            Thread.Sleep(5);
        }

        private void MoveDirectory(string fromPath, string toPath)
        {
            var fullpathFrom = Path.Combine(_watchingPath, fromPath);
            var fullpathTo = Path.Combine(_watchingPath, toPath);
            if (!Directory.Exists(Path.GetDirectoryName(fullpathTo)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullpathTo));
            }
            Directory.Move(fullpathFrom, fullpathTo);
            Thread.Sleep(5);
        }

        private void DeleteFile(string path)
        {
            var fullpath = Path.Combine(_watchingPath, path);
            File.Delete(fullpath);
            Thread.Sleep(5);
        }

        [TestMethod]
        public async Task ルートディレクトリへのファイルの保存を検知した場合にイベントが発生する()
        {
            var ev = default(FileCreatedEventArgs);
            sut.FileCreated += new EventHandler<FileCreatedEventArgs>((s, e) =>
            {
                ev = e;
            });
            CreateFile("content1.txt", "content1.txt");
            await sut.StopWatch();
            Assert.AreEqual("content1.txt", ev.Path.ToString());
        }

        [TestMethod]
        public async Task サブディレクトリへのファイルの保存を検知した場合にイベントが発生する()
        {
            var ev = default(FileCreatedEventArgs);
            sut.FileCreated += new EventHandler<FileCreatedEventArgs>((s, e) =>
            {
                ev = e;
            });
            CreateFile("dir/content1.txt", "dir/content1.txt");
            await sut.StopWatch();
            Assert.AreEqual("dir/content1.txt", ev.Path.ToString());
        }

        [TestMethod]
        public async Task ルートディレクトリへのファイルの更新を検知した場合にイベントが発生する()
        {
            CreateFile("content1.txt", "content1.txt");
            var ev = default(FileModifiedEventArgs);
            sut.FileModified += new EventHandler<FileModifiedEventArgs>((s, e) =>
            {
                ev = e;
            });
            UpdateFile("content1.txt", "content1.txt modified");
            await sut.StopWatch();
            Assert.AreEqual("content1.txt", ev.Path.ToString());
        }

        [TestMethod]
        public async Task サブディレクトリへのファイルの更新を検知した場合にイベントが発生する()
        {
            CreateFile("dir/content1.txt", "dir/content1.txt");
            var ev = default(FileModifiedEventArgs);
            sut.FileModified += new EventHandler<FileModifiedEventArgs>((s, e) =>
            {
                ev = e;
            });
            UpdateFile("dir/content1.txt", "dir/content1.txt modified");
            await sut.StopWatch();
            Assert.AreEqual("dir/content1.txt", ev.Path.ToString());
        }

        [TestMethod]
        public async Task ルートディレクトリのファイルの名前変更を検知した場合にイベントが発生する()
        {
            CreateFile("content1.txt", "content1.txt");
            var ev = default(FileMovedEventArgs);
            sut.FileMoved += new EventHandler<FileMovedEventArgs>((s, e) =>
            {
                ev = e;
            });
            MoveFile("content1.txt", "content2.txt");
            await sut.StopWatch();
            Assert.AreEqual("content2.txt", ev.Path.ToString());
        }

        [TestMethod]
        public async Task サブディレクトリのファイルの名前変更を検知した場合にイベントが発生する()
        {
            CreateFile("dir/content1.txt", "dir/content1.txt");
            var ev = default(FileMovedEventArgs);
            sut.FileMoved += new EventHandler<FileMovedEventArgs>((s, e) =>
            {
                ev = e;
            });
            MoveFile("dir/content1.txt", "dir/content2.txt");
            await sut.StopWatch();
            Assert.AreEqual("dir/content2.txt", ev.Path.ToString());
        }

        [TestMethod]
        public async Task サブディレクトリをまたいでのファイルの移動を検知した場合にイベントが発生する()
        {
            CreateFile("dir1/content1.txt", "dir1/content1.txt");
            var ev1 = default(FileDeletedEventArgs);
            sut.FileDeleted += new EventHandler<FileDeletedEventArgs>((s, e) =>
            {
                ev1 = e;
            });
            var ev2 = default(FileCreatedEventArgs);
            sut.FileCreated += new EventHandler<FileCreatedEventArgs>((s, e) =>
            {
                ev2 = e;
            });
            MoveFile("dir1/content1.txt", "dir2/content2.txt");
            await sut.StopWatch();
            Assert.AreEqual("dir1/content1.txt", ev1.Path.ToString());
            Assert.AreEqual("dir2/content2.txt", ev2.Path.ToString());
        }

        [TestMethod]
        public async Task ルートディレクトリのファイルの削除を検知した場合にイベントが発生する()
        {
            var ev = default(FileDeletedEventArgs);
            sut.FileDeleted += new EventHandler<FileDeletedEventArgs>((s, e) =>
            {
                ev = e;
            });
            CreateFile("content1.txt", "content1.txt");
            DeleteFile("content1.txt");
            await sut.StopWatch();
            Assert.AreEqual("content1.txt", ev.Path.ToString());
        }

        [TestMethod]
        public async Task サブディレクトリのファイルの削除を検知した場合にイベントが発生する()
        {
            var ev = default(FileDeletedEventArgs);
            sut.FileDeleted += new EventHandler<FileDeletedEventArgs>((s, e) =>
            {
                ev = e;
            });
            CreateFile("dir/content1.txt", "dir/content1.txt");
            DeleteFile("dir/content1.txt");
            await sut.StopWatch();
            Assert.AreEqual("dir/content1.txt", ev.Path.ToString());
        }

        [TestMethod]
        public async Task ルートディレクトリのディレクトリの移動を検知した場合にイベントが発生する()
        {
            CreateFile("dir1/content1.txt", "dir1/content1.txt");
            CreateFile("dir1/content2.txt", "dir1/content2.txt");
            var ev1 = new List<FileDeletedEventArgs>();
            sut.FileDeleted += new EventHandler<FileDeletedEventArgs>((s, e) =>
            {
                ev1.Add(e);
            });
            var ev2 = new List<FileCreatedEventArgs>();
            sut.FileCreated += new EventHandler<FileCreatedEventArgs>((s, e) =>
            {
                ev2.Add(e);
            });
            MoveDirectory("dir1", "dir2");
            await sut.StopWatch();
            Assert.AreEqual("dir1/content1.txt", ev1.First().Path.ToString());
            Assert.AreEqual("dir1/content1.txt", ev1.Last().Path.ToString());
            Assert.AreEqual("dir2/content1.txt", ev2.First().Path.ToString());
            Assert.AreEqual("dir2/content1.txt", ev2.Last().Path.ToString());
        }
    }
}
