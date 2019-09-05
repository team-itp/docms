using Docms.Client.Operations;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Operations
{
    [TestClass]
    public class UploadLocalDocumentOperationTests
    {
        private MockApplicationContext context;

        [TestInitialize]
        public async Task Setup()
        {
            context = new MockApplicationContext();
            await FileSystemUtils.Create(context.FileSystem, "test1.txt").ConfigureAwait(false);
            await FileSystemUtils.Create(context.FileSystem, "dir1/test2.txt").ConfigureAwait(false);
            await context.MockLocalStorage.SyncAsync().ConfigureAwait(false);
        }

        [TestCleanup]
        public void Teardown()
        {
            context.Dispose();
        }

        [TestMethod]
        public async Task ファイルが存在しない場合アップロードされずに終了する()
        {
            var file = context.FileSystem.GetFileInfo(new PathString("dir1/test2.txt"));
            await context.FileSystem.Delete(new PathString("dir1/test2.txt")).ConfigureAwait(false);
            var sut = new UploadLocalDocumentOperation(context, new PathString("dir1/test2.txt"), file.CalculateHash(), file.FileSize);
            await sut.ExecuteAsync().ConfigureAwait(false);
            Assert.AreEqual(0, context.MockApi.histories.Count);
        }

        [TestMethod]
        public async Task ファイルが存在する場合アップロードされること()
        {
            var file = context.FileSystem.GetFileInfo(new PathString("test1.txt"));
            var sut = new UploadLocalDocumentOperation(context, new PathString("test1.txt"), file.CalculateHash(), file.FileSize);
            await sut.ExecuteAsync().ConfigureAwait(false);
            Assert.AreEqual(1, context.MockApi.histories.Count);
        }
    }
}
