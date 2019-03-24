using Docms.Client.Documents;
using Docms.Client.DocumentStores;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class LocalDocumentStorageTests
    {
        private string tempDir;
        private MockLocalDbContext localDb;
        private LocalDocumentStorage sut;

        [TestInitialize]
        public void Setup()
        {
            tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            localDb = new MockLocalDbContext();

            sut = new LocalDocumentStorage(tempDir, localDb);
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
            await LocalFileUtils.Create(tempDir, "file1.txt");
            await sut.Sync();
            var nodes = (sut.GetNode(PathString.Root) as ContainerNode).Children;
            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(new PathString("file1.txt"), nodes.First().Path);
        }

        [TestMethod]
        public async Task ローカルのファイルとNodeの構造を同期する_ファイルがRootに二件の場合()
        {
            await LocalFileUtils.Create(tempDir, "file1.txt");
            await LocalFileUtils.Create(tempDir, "file2.txt");
            await sut.Sync();
            var nodes = (sut.GetNode(PathString.Root) as ContainerNode).Children;
            Assert.AreEqual(2, nodes.Count());
            Assert.AreEqual(new PathString("file1.txt"), nodes.First().Path);
            Assert.AreEqual(new PathString("file2.txt"), nodes.Last().Path);
        }

        [TestMethod]
        public async Task ローカルのファイルとNodeの構造を同期する_ファイルがルートに1件とサブフォルダに2件の場合()
        {
            await LocalFileUtils.Create(tempDir, "file1.txt");
            await LocalFileUtils.Create(tempDir, "dir/file1.txt");
            await LocalFileUtils.Create(tempDir, "dir/file2.txt");
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
            await LocalFileUtils.Create(tempDir, "file1.txt");
            await LocalFileUtils.Create(tempDir, "dir1/file2.txt");
            await LocalFileUtils.Update(tempDir, "file1.txt");
            await LocalFileUtils.Update(tempDir, "dir1/file2.txt");
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
            await LocalFileUtils.Create(tempDir, "file1.txt");
            await LocalFileUtils.Create(tempDir, "dir1/file2.txt");
            await LocalFileUtils.Create(tempDir, "file3.txt");
            await LocalFileUtils.Update(tempDir, "file1.txt");
            await LocalFileUtils.Update(tempDir, "dir1/file2.txt");
            await LocalFileUtils.Move(tempDir, "file1.txt", "file1_moved.txt");
            await LocalFileUtils.Move(tempDir, "dir1/file2.txt", "file2_moved.txt");
            await LocalFileUtils.Move(tempDir, "file3.txt", "dir1/file3.txt");
            await LocalFileUtils.Move(tempDir, "dir1/file3.txt", "file3_moved.txt");
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
        public async Task データベースにデータを保存し再構築する()
        {
            await LocalFileUtils.Create(tempDir, "file1.txt");
            await LocalFileUtils.Create(tempDir, "dir1/file2.txt");
            await LocalFileUtils.Update(tempDir, "file1.txt");
            await LocalFileUtils.Update(tempDir, "dir1/file2.txt");
            await sut.Sync();
            await sut.Save();

            sut = new LocalDocumentStorage(tempDir, localDb);
            await sut.Initialize();
            await sut.Save();

            sut = new LocalDocumentStorage(tempDir, localDb);
            await sut.Initialize();

            var rootNodes = (sut.GetNode(PathString.Root) as ContainerNode).Children;
            Assert.AreEqual(2, rootNodes.Count());

            var file1 = rootNodes.Last() as DocumentNode;
            Assert.AreEqual("file1.txt", file1.Name);
            Assert.AreEqual(new PathString("file1.txt"), file1.Path);
            Assert.AreEqual("file1.txt updated".Length, file1.FileSize);
            Assert.AreEqual(LocalFileUtils.DEFAULT_CREATE_TIME, file1.Created);
            Assert.AreEqual(LocalFileUtils.DEFAULT_CREATE_TIME.AddHours(1), file1.LastModified);

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
        public async Task ロックされていないファイルのストリームを開く()
        {
            await LocalFileUtils.Create(tempDir, "file1.txt");
            await sut.Sync();
            using (var token = sut.GetDocumentStreamToken(new PathString("file1.txt")))
            {
                var ms = new MemoryStream();
                using (var stream = await token.GetStreamAsync())
                {
                    await stream.CopyToAsync(ms);
                    Assert.AreEqual("file1.txt", Encoding.UTF8.GetString(ms.ToArray()));
                }
            }
        }
    }
}
