using Docms.Client.Documents;
using Docms.Client.DocumentStores;
using Docms.Client.Synchronization;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class DocumentNodeStorageTests
    {
        private MockDocmsApiClient apiClient;
        private SynchronizationContext synchronizationContext;
        private RemoteDocumentStorage sut;
        private MockDocumentDbContext localDb;

        [TestInitialize]
        public void Setup()
        {
            apiClient = new MockDocmsApiClient();
            synchronizationContext = new SynchronizationContext();
            localDb = new MockDocumentDbContext();
            sut = new RemoteDocumentStorage(apiClient, synchronizationContext, localDb);
        }

        [TestMethod]
        public async Task サーバーから履歴を読み込んでディレクトリ構造を構築する_ファイルがRootに一件の場合()
        {
            await DocmsApiUtils.Create(apiClient, "file1.txt").ConfigureAwait(false);
            await sut.Sync().ConfigureAwait(false);
            var nodes = (sut.GetNode(PathString.Root) as ContainerNode).Children;
            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(new PathString("file1.txt"), nodes.First().Path);
        }

        [TestMethod]
        public async Task サーバーから履歴を読み込んでディレクトリ構造を構築する_ファイルがRootに二件の場合()
        {
            await DocmsApiUtils.Create(apiClient, "file1.txt").ConfigureAwait(false);
            await DocmsApiUtils.Create(apiClient, "file2.txt").ConfigureAwait(false);
            await sut.Sync().ConfigureAwait(false);
            var nodes = (sut.GetNode(PathString.Root) as ContainerNode).Children;
            Assert.AreEqual(2, nodes.Count());
            Assert.AreEqual(new PathString("file1.txt"), nodes.First().Path);
            Assert.AreEqual(new PathString("file2.txt"), nodes.Last().Path);
        }

        [TestMethod]
        public async Task サーバーから履歴を読み込んでディレクトリ構造を構築する_ファイルがルートに1件とサブフォルダに2件の場合()
        {
            await DocmsApiUtils.Create(apiClient, "file1.txt").ConfigureAwait(false);
            await DocmsApiUtils.Create(apiClient, "dir/file1.txt").ConfigureAwait(false);
            await DocmsApiUtils.Create(apiClient, "dir/file2.txt").ConfigureAwait(false);
            await sut.Sync().ConfigureAwait(false);
            var nodes = (sut.GetNode(PathString.Root) as ContainerNode).Children;
            Assert.AreEqual(2, nodes.Count());
            var dir = nodes.First() as ContainerNode;
            Assert.AreEqual(new PathString("dir"), dir.Path);
            Assert.AreEqual(new PathString("dir/file1.txt"), dir.Children.First().Path);
            Assert.AreEqual(new PathString("dir/file2.txt"), dir.Children.Last().Path);
            Assert.AreEqual(new PathString("file1.txt"), nodes.Last().Path);
        }

        [TestMethod]
        public async Task サーバーから履歴を読み込んでディレクトリ構造を構築する_更新()
        {
            await DocmsApiUtils.Create(apiClient, "file1.txt").ConfigureAwait(false);
            await DocmsApiUtils.Create(apiClient, "dir1/file2.txt").ConfigureAwait(false);
            await DocmsApiUtils.Update(apiClient, "file1.txt").ConfigureAwait(false);
            await DocmsApiUtils.Update(apiClient, "dir1/file2.txt").ConfigureAwait(false);
            await sut.Sync().ConfigureAwait(false);
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
        public async Task サーバーから履歴を読み込んでディレクトリ構造を構築する_移動()
        {
            await DocmsApiUtils.Create(apiClient, "file1.txt").ConfigureAwait(false);
            await DocmsApiUtils.Create(apiClient, "dir1/file2.txt").ConfigureAwait(false);
            await DocmsApiUtils.Create(apiClient, "file3.txt").ConfigureAwait(false);
            await DocmsApiUtils.Update(apiClient, "file1.txt").ConfigureAwait(false);
            await DocmsApiUtils.Update(apiClient, "dir1/file2.txt").ConfigureAwait(false);
            await DocmsApiUtils.Move(apiClient, "file1.txt", "file1_moved.txt").ConfigureAwait(false);
            await DocmsApiUtils.Move(apiClient, "dir1/file2.txt", "file2_moved.txt").ConfigureAwait(false);
            await DocmsApiUtils.Move(apiClient, "file3.txt", "dir1/file3.txt").ConfigureAwait(false);
            await DocmsApiUtils.Move(apiClient, "dir1/file3.txt", "file3_moved.txt").ConfigureAwait(false);
            await sut.Sync().ConfigureAwait(false);
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
            await DocmsApiUtils.Create(apiClient, "file1.txt").ConfigureAwait(false);
            await DocmsApiUtils.Create(apiClient, "dir1/file2.txt").ConfigureAwait(false);
            await DocmsApiUtils.Update(apiClient, "file1.txt").ConfigureAwait(false);
            await DocmsApiUtils.Update(apiClient, "dir1/file2.txt").ConfigureAwait(false);
            await sut.Sync().ConfigureAwait(false);
            await sut.Save().ConfigureAwait(false);

            sut = new RemoteDocumentStorage(apiClient, synchronizationContext, localDb);
            await sut.Initialize().ConfigureAwait(false);
            await sut.Save().ConfigureAwait(false);

            sut = new RemoteDocumentStorage(apiClient, synchronizationContext, localDb);
            await sut.Initialize().ConfigureAwait(false);

            var rootNodes = (sut.GetNode(PathString.Root) as ContainerNode).Children;
            Assert.AreEqual(2, rootNodes.Count());

            var file1 = rootNodes.Last() as DocumentNode;
            Assert.AreEqual("file1.txt", file1.Name);
            Assert.AreEqual(new PathString("file1.txt"), file1.Path);
            Assert.AreEqual("file1.txt updated".Length, file1.FileSize);
            Assert.AreEqual(FileSystemUtils.DEFAULT_CREATE_TIME, file1.Created);
            Assert.AreEqual(FileSystemUtils.DEFAULT_CREATE_TIME.AddHours(1), file1.LastModified);

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
    }
}
