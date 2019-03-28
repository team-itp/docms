using Docms.Client.Documents;
using Docms.Client.DocumentStores;
using Docms.Client.FileSystem;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class LocalDocumentStorageTests
    {
        private string tempDir;
        private LocalFileSystem localFileSystem;
        private MockLocalDbContext localDb;
        private LocalDocumentStorage sut;

        [TestInitialize]
        public void Setup()
        {
            tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            localFileSystem = new LocalFileSystem(tempDir);
            localDb = new MockLocalDbContext();
            sut = new LocalDocumentStorage(localFileSystem, localDb);
        }

        [TestCleanup]
        public void Teardown()
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public async Task ローカルのファイルとNodeの構造を同期する_ファイルがRootに一件の場合()
        {
            await FileSystemUtils.Create(localFileSystem, "file1.txt");
            await sut.Sync();
            var nodes = (sut.GetNode(PathString.Root) as ContainerNode).Children;
            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(new PathString("file1.txt"), nodes.First().Path);
        }

        [TestMethod]
        public async Task ローカルのファイルとNodeの構造を同期する_ファイルがRootに二件の場合()
        {
            await FileSystemUtils.Create(localFileSystem, "file1.txt");
            await FileSystemUtils.Create(localFileSystem, "file2.txt");
            await sut.Sync();
            var nodes = (sut.GetNode(PathString.Root) as ContainerNode).Children;
            Assert.AreEqual(2, nodes.Count());
            Assert.AreEqual(new PathString("file1.txt"), nodes.First().Path);
            Assert.AreEqual(new PathString("file2.txt"), nodes.Last().Path);
        }

        [TestMethod]
        public async Task ローカルのファイルとNodeの構造を同期する_ファイルがルートに1件とサブフォルダに2件の場合()
        {
            await FileSystemUtils.Create(localFileSystem, "file1.txt");
            await FileSystemUtils.Create(localFileSystem, "dir/file1.txt");
            await FileSystemUtils.Create(localFileSystem, "dir/file2.txt");
            await sut.Sync();
            var nodes = (sut.GetNode(PathString.Root) as ContainerNode).Children;
            Assert.AreEqual(2, nodes.Count());
            var dir = nodes.First() as ContainerNode;
            Assert.AreEqual(new PathString("dir"), dir.Path);
            Assert.AreEqual(new PathString("dir/file1.txt"), dir.Children.First().Path);
            Assert.AreEqual(new PathString("dir/file2.txt"), dir.Children.Last().Path);
            Assert.AreEqual(new PathString("file1.txt"), nodes.Last().Path);
        }

        [TestMethod]
        public async Task ローカルのファイルとNodeの構造を同期する_更新()
        {
            await FileSystemUtils.Create(localFileSystem, "file1.txt");
            await FileSystemUtils.Create(localFileSystem, "dir1/file2.txt");
            await FileSystemUtils.Update(localFileSystem, "file1.txt");
            await FileSystemUtils.Update(localFileSystem, "dir1/file2.txt");
            await sut.Sync();
            var rootNodes = (sut.GetNode(PathString.Root) as ContainerNode).Children;
            Assert.AreEqual(2, rootNodes.Count());

            var file1 = rootNodes.Last() as DocumentNode;
            Assert.AreEqual("file1.txt", file1.Name);
            Assert.AreEqual(new PathString("file1.txt"), file1.Path);
            Assert.AreEqual("file1.txt updated".Length, file1.FileSize);

            var dir1 = rootNodes.First() as ContainerNode;
            Assert.AreEqual("dir1", dir1.Name);
            Assert.AreEqual(new PathString("dir1"), dir1.Path);

            var subNodes = (sut.GetNode(new PathString("dir1")) as ContainerNode).Children;
            Assert.AreEqual(1, subNodes.Count());

            var file2 = subNodes.First() as DocumentNode;
            Assert.AreEqual("file2.txt", file2.Name);
            Assert.AreEqual(new PathString("dir1/file2.txt"), file2.Path);
            Assert.AreEqual("dir1/file2.txt updated".Length, file2.FileSize);
        }

        [TestMethod]
        public async Task ローカルのファイルとNodeの構造を同期する_移動()
        {
            await FileSystemUtils.Create(localFileSystem, "file1.txt");
            await FileSystemUtils.Create(localFileSystem, "dir1/file2.txt");
            await FileSystemUtils.Create(localFileSystem, "file3.txt");
            await FileSystemUtils.Update(localFileSystem, "file1.txt");
            await FileSystemUtils.Update(localFileSystem, "dir1/file2.txt");
            await FileSystemUtils.Move(localFileSystem, "file1.txt", "file1_moved.txt");
            await FileSystemUtils.Move(localFileSystem, "dir1/file2.txt", "file2_moved.txt");
            await FileSystemUtils.Move(localFileSystem, "file3.txt", "dir1/file3.txt");
            await FileSystemUtils.Move(localFileSystem, "dir1/file3.txt", "file3_moved.txt");
            await sut.Sync();
            var rootNodes = (sut.GetNode(PathString.Root) as ContainerNode).Children;
            Assert.AreEqual(3, rootNodes.Count());

            var file1 = rootNodes.First() as DocumentNode;
            Assert.AreEqual("file1_moved.txt", file1.Name);
            Assert.AreEqual(new PathString("file1_moved.txt"), file1.Path);
            Assert.AreEqual("file1.txt updated".Length, file1.FileSize);

            var file2 = rootNodes.Skip(1).First() as DocumentNode;
            Assert.AreEqual("file2_moved.txt", file2.Name);
            Assert.AreEqual(new PathString("file2_moved.txt"), file2.Path);
            Assert.AreEqual("dir1/file2.txt updated".Length, file2.FileSize);

            var file3 = rootNodes.Skip(2).First() as DocumentNode;
            Assert.AreEqual("file3_moved.txt", file3.Name);
            Assert.AreEqual(new PathString("file3_moved.txt"), file3.Path);
            Assert.AreEqual("file3.txt".Length, file3.FileSize);
        }

        [TestMethod]
        public async Task ローカルのファイルとNodeの構造を同期する_削除()
        {
            await FileSystemUtils.Create(localFileSystem, "file1.txt");
            await FileSystemUtils.Create(localFileSystem, "dir1/file2.txt");
            await sut.Sync();

            await FileSystemUtils.Delete(localFileSystem, "dir1");
            await sut.Sync();

            var rootNodes = (sut.GetNode(PathString.Root) as ContainerNode).Children;
            Assert.AreEqual(1, rootNodes.Count());

            var file1 = rootNodes.First() as DocumentNode;
            Assert.AreEqual("file1.txt", file1.Name);
        }

        [TestMethod]
        public async Task データベースにデータを保存し再構築する()
        {
            await FileSystemUtils.Create(localFileSystem, "file1.txt");
            await FileSystemUtils.Create(localFileSystem, "dir1/file2.txt");
            await FileSystemUtils.Create(localFileSystem, "file3.txt");
            await FileSystemUtils.Update(localFileSystem, "file1.txt");
            await FileSystemUtils.Update(localFileSystem, "dir1/file2.txt");
            await FileSystemUtils.Move(localFileSystem, "file1.txt", "file1_moved.txt");
            await FileSystemUtils.Move(localFileSystem, "dir1/file2.txt", "file2_moved.txt");
            await FileSystemUtils.Move(localFileSystem, "file3.txt", "dir1/file3.txt");
            await FileSystemUtils.Move(localFileSystem, "dir1/file3.txt", "file3_moved.txt");
            await sut.Sync();
            var rootNodes = (sut.GetNode(PathString.Root) as ContainerNode).Children;
            Assert.AreEqual(3, rootNodes.Count());

            var file1 = rootNodes.First() as DocumentNode;
            Assert.AreEqual("file1_moved.txt", file1.Name);
            Assert.AreEqual(new PathString("file1_moved.txt"), file1.Path);
            Assert.AreEqual("file1.txt updated".Length, file1.FileSize);

            var file2 = rootNodes.Skip(1).First() as DocumentNode;
            Assert.AreEqual("file2_moved.txt", file2.Name);
            Assert.AreEqual(new PathString("file2_moved.txt"), file2.Path);
            Assert.AreEqual("dir1/file2.txt updated".Length, file2.FileSize);

            var file3 = rootNodes.Skip(2).First() as DocumentNode;
            Assert.AreEqual("file3_moved.txt", file3.Name);
            Assert.AreEqual(new PathString("file3_moved.txt"), file3.Path);
            Assert.AreEqual("file3.txt".Length, file3.FileSize);
        }
    }
}
