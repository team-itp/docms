using Docms.Client.Data;
using Docms.Client.RemoteDocuments;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class RemoteDocumentStorageTests
    {
        private MockDocmsApiClient apiClient;
        private RemoteDocumentStorage sut;
        private LocalDbContext localDb;

        [TestInitialize]
        public void Setup()
        {
            apiClient = new MockDocmsApiClient();
            localDb = new LocalDbContext(new DbContextOptionsBuilder<LocalDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
            sut = new RemoteDocumentStorage(apiClient, localDb);
        }

        [TestMethod]
        public async Task サーバーから履歴を読み込んでディレクトリ構造を構築する_ファイルがRootに一件の場合()
        {
            await DocmsApiUtils.Create(apiClient, "file1.txt");
            await sut.UpdateAsync();
            var nodes = (sut.GetNode(PathString.Root) as RemoteContainer).Children;
            Assert.AreEqual(1, nodes.Count());
            Assert.AreEqual(new PathString("file1.txt"), nodes.First().Path);
        }

        [TestMethod]
        public async Task サーバーから履歴を読み込んでディレクトリ構造を構築する_ファイルがRootに二件の場合()
        {
            await DocmsApiUtils.Create(apiClient, "file1.txt");
            await DocmsApiUtils.Create(apiClient, "file2.txt");
            await sut.UpdateAsync();
            var nodes = (sut.GetNode(PathString.Root) as RemoteContainer).Children;
            Assert.AreEqual(2, nodes.Count());
            Assert.AreEqual(new PathString("file1.txt"), nodes.First().Path);
            Assert.AreEqual(new PathString("file2.txt"), nodes.Last().Path);
        }

        [TestMethod]
        public async Task サーバーから履歴を読み込んでディレクトリ構造を構築する_ファイルがルートに1件とサブフォルダに2件の場合()
        {
            await DocmsApiUtils.Create(apiClient, "file1.txt");
            await DocmsApiUtils.Create(apiClient, "dir/file1.txt");
            await DocmsApiUtils.Create(apiClient, "dir/file2.txt");
            await sut.UpdateAsync();
            var nodes = (sut.GetNode(PathString.Root) as RemoteContainer).Children;
            Assert.AreEqual(2, nodes.Count());
            var dir = nodes.First() as RemoteContainer;
            Assert.AreEqual(new PathString("dir"), dir.Path);
            Assert.AreEqual(new PathString("dir/file1.txt"), dir.Children.First().Path);
            Assert.AreEqual(new PathString("dir/file2.txt"), dir.Children.Last().Path);
            Assert.AreEqual(new PathString("file1.txt"), nodes.Last().Path);
        }

        [TestMethod]
        public async Task サーバーから履歴を読み込んでディレクトリ構造を構築する_更新()
        {
            await DocmsApiUtils.Create(apiClient, "file1.txt");
            await DocmsApiUtils.Create(apiClient, "dir1/file2.txt");
            await DocmsApiUtils.Update(apiClient, "file1.txt");
            await DocmsApiUtils.Update(apiClient, "dir1/file2.txt");
            await sut.UpdateAsync();
            var rootNodes = (sut.GetNode(PathString.Root) as RemoteContainer).Children;
            Assert.AreEqual(2, rootNodes.Count());

            var file1 = rootNodes.Last() as RemoteDocument;
            Assert.AreEqual("file1.txt", file1.Name);
            Assert.AreEqual(new PathString("file1.txt"), file1.Path);
            Assert.AreEqual("file1.txt updated".Length, file1.FileSize);

            var dir1 = rootNodes.First() as RemoteContainer;
            Assert.AreEqual("dir1", dir1.Name);
            Assert.AreEqual(new PathString("dir1"), dir1.Path);

            var subNodes = (sut.GetNode(new PathString("dir1")) as RemoteContainer).Children;
            Assert.AreEqual(1, subNodes.Count());

            var file2 = subNodes.First() as RemoteDocument;
            Assert.AreEqual("file2.txt", file2.Name);
            Assert.AreEqual(new PathString("dir1/file2.txt"), file2.Path);
            Assert.AreEqual("dir1/file2.txt updated".Length, file2.FileSize);
        }
    }
}
