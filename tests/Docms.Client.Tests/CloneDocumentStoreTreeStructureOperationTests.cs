using Docms.Client.Documents;
using Docms.Client.Operations;
using Docms.Client.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class CloneDocumentStoreTreeStructureOperationTests
    {
        [TestMethod]
        public async Task ファイルが0件のリモートファイルをコピーしてローカルのストレージは0件になる()
        {
            var context = new MockApplicationContext();
            var sut = new CloneDocumentStoreTreeStructureOperation(context);
            await sut.ExecuteAsync(CancellationToken.None);
            Assert.IsFalse(context.RemoteStorage.Root.Children.Any());
        }

        [TestMethod]
        public async Task ファイルが1件のリモートファイルをコピーしてローカルのストレージは1件になる()
        {
            var context = new MockApplicationContext();
            await DocmsApiUtils.Create(context.Api, "file1.txt");
            await context.RemoteStorage.SyncAsync(CancellationToken.None);
            var sut = new CloneDocumentStoreTreeStructureOperation(context);
            await sut.ExecuteAsync(CancellationToken.None);
            Assert.AreEqual(1, context.RemoteStorage.Root.Children.Count());
            Assert.AreEqual("file1.txt", context.RemoteStorage.Root.Children.First().Name);
        }

        [TestMethod]
        public async Task ファイルが2件フォルダが1件のリモートファイルをコピーしてローカルのストレージはファイルが2件フォルダが1件になる()
        {
            var context = new MockApplicationContext();
            await DocmsApiUtils.Create(context.Api, "file1.txt");
            await DocmsApiUtils.Create(context.Api, "dir1/file2.txt");
            await context.RemoteStorage.SyncAsync(CancellationToken.None);
            var sut = new CloneDocumentStoreTreeStructureOperation(context);
            await sut.ExecuteAsync(CancellationToken.None);
            Assert.AreEqual(2, context.RemoteStorage.Root.Children.Count());
            var file = context.RemoteStorage.Root.Children.OfType<DocumentNode>().First();
            Assert.AreEqual("file1.txt", file.Name);
            var dir = context.RemoteStorage.Root.Children.OfType<ContainerNode>().First();
            Assert.AreEqual("dir1", dir.Name);
            Assert.AreEqual(1, dir.Children.Count());
            Assert.AreEqual("file2.txt", dir.Children.First().Name);
        }
    }
}
