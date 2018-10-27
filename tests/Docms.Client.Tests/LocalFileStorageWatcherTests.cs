using Docms.Client.FileStorage;
using Docms.Client.FileTrees;
using Docms.Client.SeedWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class LocalFileStorageWatcherTests
    {
        private string _watchingPath;
        private InternalFileTree fileTree;
        private LocalFileStorageWatcher sut;

        [TestInitialize]
        public async Task Setup()
        {
            _watchingPath = Path.GetFullPath("tmp" + Guid.NewGuid().ToString());
            fileTree = new InternalFileTree();
            sut = new LocalFileStorageWatcher(_watchingPath, fileTree);
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

        private async Task CreateDirectory(string path)
        {
            var fullpath = Path.Combine(_watchingPath, path);
            if (!Directory.Exists(fullpath))
            {
                Directory.CreateDirectory(fullpath);
            }
            await Task.Delay(5);
        }

        private async Task CreateFile(string path, string content)
        {
            var fullpath = Path.Combine(_watchingPath, path);
            if (!Directory.Exists(Path.GetDirectoryName(fullpath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullpath));
            }
            File.WriteAllBytes(fullpath, Encoding.UTF8.GetBytes(content));
            await Task.Delay(5);
        }

        private async Task UpdateFile(string path, string content)
        {
            var fullpath = Path.Combine(_watchingPath, path);
            File.WriteAllBytes(fullpath, Encoding.UTF8.GetBytes(content));
            await Task.Delay(5);
        }

        private async Task MoveFile(string fromPath, string toPath)
        {
            var fullpathFrom = Path.Combine(_watchingPath, fromPath);
            var fullpathTo = Path.Combine(_watchingPath, toPath);
            if (!Directory.Exists(Path.GetDirectoryName(fullpathTo)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullpathTo));
            }
            File.Move(fullpathFrom, fullpathTo);
            await Task.Delay(5);
        }

        private async Task MoveDirectory(string fromPath, string toPath)
        {
            var fullpathFrom = Path.Combine(_watchingPath, fromPath);
            var fullpathTo = Path.Combine(_watchingPath, toPath);
            if (!Directory.Exists(Path.GetDirectoryName(fullpathTo)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullpathTo));
            }
            Directory.Move(fullpathFrom, fullpathTo);
            await Task.Delay(5);
        }

        private async Task DeleteFile(string path)
        {
            var fullpath = Path.Combine(_watchingPath, path);
            File.Delete(fullpath);
            await Task.Delay(5);
        }

        private async Task DeleteDirectory(string path)
        {
            var fullpath = Path.Combine(_watchingPath, path);
            Directory.Delete(fullpath, true);
            await Task.Delay(5);
        }

        [TestMethod]
        public async Task ルートディレクトリへのファイルの保存を検知した場合にイベントが発生する()
        {
            var ev = default(FileCreatedEventArgs);
            sut.FileCreated += new EventHandler<FileCreatedEventArgs>((s, e) =>
            {
                ev = e;
            });
            await CreateFile("content1.txt", "content1.txt");
            Assert.AreEqual("content1.txt", ev.Path.ToString());
            Assert.IsNotNull(fileTree.GetFile(new PathString("content1.txt")));
        }

        [TestMethod]
        public async Task サブディレクトリへのファイルの保存を検知した場合にイベントが発生する()
        {
            var ev = default(FileCreatedEventArgs);
            sut.FileCreated += new EventHandler<FileCreatedEventArgs>((s, e) =>
            {
                ev = e;
            });
            await CreateFile("dir/content1.txt", "dir/content1.txt");
            Assert.AreEqual("dir/content1.txt", ev.Path.ToString());
            Assert.IsNotNull(fileTree.GetFile(new PathString("dir/content1.txt")));
        }

        [TestMethod]
        public async Task ルートディレクトリへのファイルの更新を検知した場合にイベントが発生する()
        {
            await CreateFile("content1.txt", "content1.txt");
            var ev = default(FileModifiedEventArgs);
            sut.FileModified += new EventHandler<FileModifiedEventArgs>((s, e) =>
            {
                ev = e;
            });
            await UpdateFile("content1.txt", "content1.txt modified");
            Assert.AreEqual("content1.txt", ev.Path.ToString());
            Assert.IsNotNull(fileTree.GetFile(new PathString("content1.txt")));
        }

        [TestMethod]
        public async Task サブディレクトリへのファイルの更新を検知した場合にイベントが発生する()
        {
            await CreateFile("dir/content1.txt", "dir/content1.txt");
            var ev = default(FileModifiedEventArgs);
            sut.FileModified += new EventHandler<FileModifiedEventArgs>((s, e) =>
            {
                ev = e;
            });
            await UpdateFile("dir/content1.txt", "dir/content1.txt modified");
            Assert.AreEqual("dir/content1.txt", ev.Path.ToString());
            Assert.IsNotNull(fileTree.GetFile(new PathString("dir/content1.txt")));
        }

        [TestMethod]
        public async Task ルートディレクトリのファイルの名前変更を検知した場合にイベントが発生する()
        {
            await CreateFile("content1.txt", "content1.txt");
            var ev = default(FileMovedEventArgs);
            sut.FileMoved += new EventHandler<FileMovedEventArgs>((s, e) =>
            {
                ev = e;
            });
            await MoveFile("content1.txt", "content2.txt");
            Assert.AreEqual("content2.txt", ev.Path.ToString());
            Assert.IsNull(fileTree.GetFile(new PathString("content1.txt")));
            Assert.IsNotNull(fileTree.GetFile(new PathString("content2.txt")));
        }

        [TestMethod]
        public async Task サブディレクトリのファイルの名前変更を検知した場合にイベントが発生する()
        {
            await CreateFile("dir/content1.txt", "dir/content1.txt");
            var ev = default(FileMovedEventArgs);
            sut.FileMoved += new EventHandler<FileMovedEventArgs>((s, e) =>
            {
                ev = e;
            });
            await MoveFile("dir/content1.txt", "dir/content2.txt");
            Assert.AreEqual("dir/content2.txt", ev.Path.ToString());
            Assert.IsNull(fileTree.GetFile(new PathString("dir/content1.txt")));
            Assert.IsNotNull(fileTree.GetFile(new PathString("dir/content2.txt")));
        }

        [TestMethod]
        public async Task サブディレクトリをまたいでのファイルの移動を検知した場合にイベントが発生する()
        {
            await CreateFile("dir1/content1.txt", "dir1/content1.txt");
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
            await MoveFile("dir1/content1.txt", "dir2/content2.txt");
            Assert.AreEqual("dir1/content1.txt", ev1.Path.ToString());
            Assert.AreEqual("dir2/content2.txt", ev2.Path.ToString());
            Assert.IsNull(fileTree.GetFile(new PathString("dir1")));
            Assert.IsNull(fileTree.GetFile(new PathString("dir1/content1.txt")));
            Assert.IsNotNull(fileTree.GetFile(new PathString("dir2/content2.txt")));
        }

        [TestMethod]
        public async Task ルートディレクトリのファイルの削除を検知した場合にイベントが発生する()
        {
            await CreateFile("content1.txt", "content1.txt");
            var ev = default(FileDeletedEventArgs);
            sut.FileDeleted += new EventHandler<FileDeletedEventArgs>((s, e) =>
            {
                ev = e;
            });
            await DeleteFile("content1.txt");
            Assert.AreEqual("content1.txt", ev.Path.ToString());
            Assert.IsNull(fileTree.GetFile(new PathString("content1.txt")));
        }

        [TestMethod]
        public async Task サブディレクトリのファイルの削除を検知した場合にイベントが発生する()
        {
            await CreateFile("dir/content1.txt", "dir/content1.txt");
            var ev = default(FileDeletedEventArgs);
            sut.FileDeleted += new EventHandler<FileDeletedEventArgs>((s, e) =>
            {
                ev = e;
            });
            await DeleteFile("dir/content1.txt");
            Assert.AreEqual("dir/content1.txt", ev.Path.ToString());
            Assert.IsNull(fileTree.GetFile(new PathString("dir/content1.txt")));
        }

        [TestMethod]
        public async Task ルートディレクトリのディレクトリの名前変更を検知した場合にイベントが発生する()
        {
            await CreateFile("dir1/content1.txt", "dir1/content1.txt");
            await CreateFile("dir1/content2.txt", "dir1/content2.txt");
            var ev = new List<FileMovedEventArgs>();
            sut.FileMoved += new EventHandler<FileMovedEventArgs>((s, e) =>
            {
                ev.Add(e);
            });
            await MoveDirectory("dir1", "dir2");
            Assert.AreEqual("dir2/content1.txt", ev.First().Path.ToString());
            Assert.AreEqual("dir1/content1.txt", ev.First().FromPath.ToString());
            Assert.AreEqual("dir2/content2.txt", ev.Last().Path.ToString());
            Assert.AreEqual("dir1/content2.txt", ev.Last().FromPath.ToString());
            Assert.IsNull(fileTree.GetFile(new PathString("dir1/content1.txt")));
            Assert.IsNull(fileTree.GetFile(new PathString("dir1/content2.txt")));
            Assert.IsNotNull(fileTree.GetFile(new PathString("dir2/content1.txt")));
            Assert.IsNotNull(fileTree.GetFile(new PathString("dir2/content2.txt")));
        }

        [TestMethod]
        public async Task ルートディレクトリのディレクトリがサブディレクトリへ移動した場合にイベントが発生する()
        {
            await CreateFile("dir1/content1.txt", "dir1/content1.txt");
            await CreateFile("dir1/content2.txt", "dir1/content2.txt");
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
            await MoveDirectory("dir1", "dir2/subdir2");
            Assert.AreEqual("dir1/content1.txt", ev1.First().Path.ToString());
            Assert.AreEqual("dir1/content2.txt", ev1.Last().Path.ToString());
            Assert.AreEqual("dir2/subdir2/content1.txt", ev2.First().Path.ToString());
            Assert.AreEqual("dir2/subdir2/content2.txt", ev2.Last().Path.ToString());
            Assert.IsNull(fileTree.GetFile(new PathString("dir1/content1.txt")));
            Assert.IsNull(fileTree.GetFile(new PathString("dir1/content2.txt")));
            Assert.IsNotNull(fileTree.GetFile(new PathString("dir2/subdir2/content1.txt")));
            Assert.IsNotNull(fileTree.GetFile(new PathString("dir2/subdir2/content2.txt")));
        }

        [TestMethod]
        public async Task サブディレクトリのディレクトリがルートディレクトリへ移動した場合にイベントが発生する()
        {
            await CreateFile("dir1/subdir1/content1.txt", "dir1/subdir1/content1.txt");
            await CreateFile("dir1/subdir1/content2.txt", "dir1/subdir1/content2.txt");
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
            await MoveDirectory("dir1/subdir1", "dir2");
            Assert.AreEqual("dir1/subdir1/content1.txt", ev1.First().Path.ToString());
            Assert.AreEqual("dir1/subdir1/content2.txt", ev1.Last().Path.ToString());
            Assert.AreEqual("dir2/content1.txt", ev2.First().Path.ToString());
            Assert.AreEqual("dir2/content2.txt", ev2.Last().Path.ToString());
            Assert.IsNull(fileTree.GetFile(new PathString("dir1/subdir1/content1.txt")));
            Assert.IsNull(fileTree.GetFile(new PathString("dir1/subdir1/content2.txt")));
            Assert.IsNotNull(fileTree.GetFile(new PathString("dir2/content1.txt")));
            Assert.IsNotNull(fileTree.GetFile(new PathString("dir2/content2.txt")));
        }

        [TestMethod]
        public async Task ルートディレクトリのディレクトリの削除を検知した場合にイベントが発生する()
        {
            await CreateFile("dir1/content1.txt", "dir1/content1.txt");
            await CreateFile("dir1/content2.txt", "dir1/content2.txt");
            var ev = new List<FileDeletedEventArgs>();
            sut.FileDeleted += new EventHandler<FileDeletedEventArgs>((s, e) =>
            {
                ev.Add(e);
            });
            await DeleteDirectory("dir1");
            Assert.AreEqual("dir1/content1.txt", ev.First().Path.ToString());
            Assert.AreEqual("dir1/content2.txt", ev.Last().Path.ToString());
            Assert.IsNull(fileTree.GetDirectory(new PathString("dir1")));
            Assert.IsNull(fileTree.GetFile(new PathString("dir1/content1.txt")));
            Assert.IsNull(fileTree.GetFile(new PathString("dir1/content2.txt")));
        }

        [TestMethod]
        public async Task ルートディレクトリのディレクトリが削除された場合にイベントが発生する()
        {
            await CreateFile("dir1/content1.txt", "dir1/content1.txt");
            await CreateFile("dir1/content2.txt", "dir1/content2.txt");
            var ev = new List<FileDeletedEventArgs>();
            sut.FileDeleted += new EventHandler<FileDeletedEventArgs>((s, e) =>
            {
                ev.Add(e);
            });
            await DeleteDirectory("dir1");
            Assert.AreEqual("dir1/content1.txt", ev.First().Path.ToString());
            Assert.AreEqual("dir1/content2.txt", ev.Last().Path.ToString());
            Assert.IsNull(fileTree.GetFile(new PathString("dir1/content1.txt")));
            Assert.IsNull(fileTree.GetFile(new PathString("dir1/content2.txt")));
        }

        [TestMethod]
        public async Task サブディレクトリのディレクトリが削除された場合にイベントが発生する()
        {
            await CreateFile("dir1/subdir1/content1.txt", "dir1/subdir1/content1.txt");
            await CreateFile("dir1/subdir1/content2.txt", "dir1/subdir1/content2.txt");
            var ev = new List<FileDeletedEventArgs>();
            sut.FileDeleted += new EventHandler<FileDeletedEventArgs>((s, e) =>
            {
                ev.Add(e);
            });
            await DeleteDirectory("dir1/subdir1");
            Assert.AreEqual("dir1/subdir1/content1.txt", ev.First().Path.ToString());
            Assert.AreEqual("dir1/subdir1/content2.txt", ev.Last().Path.ToString());
            Assert.IsNull(fileTree.GetFile(new PathString("dir1/subdir1/content1.txt")));
            Assert.IsNull(fileTree.GetFile(new PathString("dir1/subdir1/content2.txt")));
        }

        [TestMethod]
        public async Task ファイルの書き込みが大量に発生した場合に正しく追随する()
        {
            const int DIRECTRY_COUNT = 10;
            const int FILE_COUNT = 10;
            var ev = new List<LocalFileEvenArgs>();
            sut.FileCreated += new EventHandler<FileCreatedEventArgs>((s, e) => ev.Add(e));
            sut.FileModified += new EventHandler<FileModifiedEventArgs>((s, e) => ev.Add(e));
            sut.FileMoved += new EventHandler<FileMovedEventArgs>((s, e) => ev.Add(e));
            sut.FileDeleted += new EventHandler<FileDeletedEventArgs>((s, e) => ev.Add(e));

            for (var i = 0; i < DIRECTRY_COUNT; i++)
            {
                await CreateDirectory($"dir{i}");
            }
            await sut.StopWatch(false);
            await sut.StartWatch();

            for (var i = 0; i < DIRECTRY_COUNT; i++)
            {
                for (var j = 0; j < FILE_COUNT; j++)
                {
                    await CreateFile($"dir{i}/content{j}.txt", $"dir{i}/content{j}.txt");
                }
            }

            for (var i = 0; i < DIRECTRY_COUNT; i++)
            {
                for (var j = 0; j < FILE_COUNT; j++)
                {
                    await UpdateFile($"dir{i}/content{j}.txt", $"dir{i}/content{j}.txt update");
                }
            }

            for (var i = 0; i < DIRECTRY_COUNT; i++)
            {
                for (var j = 0; j < FILE_COUNT; j++)
                {
                    await MoveFile($"dir{i}/content{j}.txt", $"dir{i}/moved_content{j}.txt");
                }
            }

            for (var i = 0; i < DIRECTRY_COUNT; i++)
            {
                for (var j = 0; j < FILE_COUNT; j++)
                {
                    await DeleteFile($"dir{i}/moved_content{j}.txt");
                }
            }

            await sut.StopWatch(false);

            using (var iter = ev.GetEnumerator())
            {
                for (var i = 0; i < DIRECTRY_COUNT; i++)
                {
                    for (var j = 0; j < FILE_COUNT; j++)
                    {
                        Assert.IsTrue(iter.MoveNext());
                        if (iter.Current is FileCreatedEventArgs ce)
                        {
                            Assert.AreEqual($"dir{i}/content{j}.txt", ce.Path.ToString());
                        }
                        else
                        {
                            Assert.Fail($"i:{i}, j:{j}, iter.Current:{iter.Current} {iter.Current.Path}");
                        }
                    }
                }

                for (var i = 0; i < DIRECTRY_COUNT; i++)
                {
                    for (var j = 0; j < FILE_COUNT; j++)
                    {
                        Assert.IsTrue(iter.MoveNext());
                        if (iter.Current is FileModifiedEventArgs me)
                        {
                            Assert.AreEqual($"dir{i}/content{j}.txt", me.Path.ToString());
                        }
                        else
                        {
                            Assert.Fail($"i:{i}, j:{j}, iter.Current:{iter.Current} {iter.Current.Path}");
                        }
                    }
                }

                for (var i = 0; i < DIRECTRY_COUNT; i++)
                {
                    for (var j = 0; j < FILE_COUNT; j++)
                    {
                        Assert.IsTrue(iter.MoveNext());
                        if (iter.Current is FileMovedEventArgs me)
                        {
                            Assert.AreEqual($"dir{i}/moved_content{j}.txt", me.Path.ToString());
                            Assert.AreEqual($"dir{i}/content{j}.txt", me.FromPath.ToString());
                        }
                        else
                        {
                            Assert.Fail($"i:{i}, j:{j}, iter.Current:{iter.Current} {iter.Current.Path}");
                        }
                    }
                }

                for (var i = 0; i < DIRECTRY_COUNT; i++)
                {
                    for (var j = 0; j < FILE_COUNT; j++)
                    {
                        Assert.IsTrue(iter.MoveNext());
                        if (iter.Current is FileDeletedEventArgs de)
                        {
                            Assert.AreEqual($"dir{i}/moved_content{j}.txt", de.Path.ToString());
                        }
                        else
                        {
                            Assert.Fail($"i:{i}, j:{j}, iter.Current:{iter.Current} {iter.Current.Path}");
                        }
                    }
                }
                Assert.IsFalse(iter.MoveNext());
            }
        }
    }
}