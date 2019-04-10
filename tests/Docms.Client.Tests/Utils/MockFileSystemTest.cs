using Docms.Client.FileSystem;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Utils
{
    [TestClass]
    public class MockFileSystemTest
    {
        private MockFileSystem sut;

        [TestInitialize]
        public void Setup()
        {
            sut = new MockFileSystem();
        }

        [TestMethod]
        public async Task ファイルを作成する()
        {
            await FileSystemUtils.Create(sut, "test1.txt").ConfigureAwait(false);
            Assert.IsTrue(FileSystemUtils.FileExists(sut, "test1.txt"));

            await FileSystemUtils.Create(sut, "dir1/test2.txt").ConfigureAwait(false);
            Assert.IsTrue(FileSystemUtils.DirectoryExists(sut, "dir1"));
            Assert.IsTrue(FileSystemUtils.FileExists(sut, "dir1/test2.txt"));

            await FileSystemUtils.Create(sut, "dir1/test3.txt").ConfigureAwait(false);
            Assert.IsTrue(FileSystemUtils.FileExists(sut, "dir1/test3.txt"));
        }

        [TestMethod]
        public async Task ファイルを削除する()
        {
            await FileSystemUtils.Create(sut, "test1.txt").ConfigureAwait(false);
            await FileSystemUtils.Create(sut, "dir1/test2.txt").ConfigureAwait(false);
            await FileSystemUtils.Create(sut, "dir1/test3.txt").ConfigureAwait(false);

            await sut.Delete(new PathString("test1.txt")).ConfigureAwait(false);
            await sut.Delete(new PathString("dir1/test2.txt")).ConfigureAwait(false);

            Assert.IsFalse(FileSystemUtils.FileExists(sut, "test1.txt"));
            Assert.IsTrue(FileSystemUtils.DirectoryExists(sut, "dir1"));
            Assert.IsFalse(FileSystemUtils.FileExists(sut, "dir1/test2.txt"));
            Assert.IsTrue(FileSystemUtils.FileExists(sut, "dir1/test3.txt"));
        }

        [TestMethod]
        public async Task 空のフォルダにファイルを作成する()
        {
            await FileSystemUtils.Create(sut, "dir1/test2.txt").ConfigureAwait(false);
            await sut.Delete(new PathString("dir1/test2.txt")).ConfigureAwait(false);

            await FileSystemUtils.Create(sut, "dir1").ConfigureAwait(false);

            Assert.IsFalse(FileSystemUtils.DirectoryExists(sut, "dir1"));
            Assert.IsTrue(FileSystemUtils.FileExists(sut, "dir1"));
        }

        [TestMethod]
        public async Task ファイルとディレクトリの一覧を取得する()
        {
            await FileSystemUtils.Create(sut, "test1.txt").ConfigureAwait(false);
            await FileSystemUtils.Create(sut, "test2.txt").ConfigureAwait(false);
            await FileSystemUtils.Create(sut, "dir1/test3.txt").ConfigureAwait(false);
            await FileSystemUtils.Create(sut, "dir1/test4.txt").ConfigureAwait(false);
            await FileSystemUtils.Create(sut, "dir1/subDir2/test5.txt").ConfigureAwait(false);
            using (var files = sut.GetFiles(PathString.Root).GetEnumerator())
            {
                Assert.IsTrue(files.MoveNext());
                Assert.AreEqual(new PathString("test1.txt"), files.Current);
                Assert.IsTrue(files.MoveNext());
                Assert.AreEqual(new PathString("test2.txt"), files.Current);
                Assert.IsFalse(files.MoveNext());
            }
            using (var files = sut.GetDirectories(PathString.Root).GetEnumerator())
            {
                Assert.IsTrue(files.MoveNext());
                Assert.AreEqual(new PathString("dir1"), files.Current);
                Assert.IsFalse(files.MoveNext());
            }
            using (var files = sut.GetFiles(new PathString("dir1")).GetEnumerator())
            {
                Assert.IsTrue(files.MoveNext());
                Assert.AreEqual(new PathString("dir1/test3.txt"), files.Current);
                Assert.IsTrue(files.MoveNext());
                Assert.AreEqual(new PathString("dir1/test4.txt"), files.Current);
                Assert.IsFalse(files.MoveNext());
            }
            using (var files = sut.GetDirectories(new PathString("dir1")).GetEnumerator())
            {
                Assert.IsTrue(files.MoveNext());
                Assert.AreEqual(new PathString("dir1/subDir2"), files.Current);
                Assert.IsFalse(files.MoveNext());
            }
            // 存在しないファイルとディレクトリーでは空のリストが生成される
            using (var files = sut.GetFiles(new PathString("dir2")).GetEnumerator())
            {
                Assert.IsFalse(files.MoveNext());
            }
            using (var files = sut.GetDirectories(new PathString("dir2")).GetEnumerator())
            {
                Assert.IsFalse(files.MoveNext());
            }
        }

        [TestMethod]
        public async Task ファイルを移動する()
        {
            await FileSystemUtils.Create(sut, "test1.txt").ConfigureAwait(false);
            var hash1 = sut.GetFileInfo(new PathString("test1.txt")).CalculateHash();

            await sut.Move(new PathString("test1.txt"), new PathString("test2.txt")).ConfigureAwait(false);

            Assert.IsTrue(FileSystemUtils.FileExists(sut, "test2.txt"));
            Assert.IsFalse(FileSystemUtils.FileExists(sut, "test1.txt"));

            await sut.Move(new PathString("test2.txt"), new PathString("dir1/test3.txt")).ConfigureAwait(false);
            Assert.IsTrue(FileSystemUtils.FileExists(sut, "dir1/test3.txt"));
            Assert.IsFalse(FileSystemUtils.FileExists(sut, "test2.txt"));

            await sut.Move(new PathString("dir1/test3.txt"), new PathString("dir1/test4.txt")).ConfigureAwait(false);
            Assert.IsTrue(FileSystemUtils.FileExists(sut, "dir1/test4.txt"));
            Assert.IsFalse(FileSystemUtils.FileExists(sut, "dir1/test3.txt"));

            await sut.Move(new PathString("dir1/test4.txt"), new PathString("test5.txt")).ConfigureAwait(false);
            Assert.IsTrue(FileSystemUtils.FileExists(sut, "test5.txt"));
            Assert.IsFalse(FileSystemUtils.FileExists(sut, "dir1/test4.txt"));

            Assert.AreEqual(hash1, sut.GetFileInfo(new PathString("test5.txt")).CalculateHash());
            Assert.IsFalse(FileSystemUtils.FileExists(sut, "dir1"));
            Assert.IsTrue(FileSystemUtils.DirectoryExists(sut, "dir1"));
        }
    }
}
