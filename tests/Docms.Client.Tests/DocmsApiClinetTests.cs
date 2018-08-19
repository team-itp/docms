using Docms.Client.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class DocmsApiClinetTests
    {
        private DocmsApiClinet sut;

        [TestInitialize]
        public async Task Setup()
        {
            sut = new DocmsApiClinet("http://localhost:51693");
            await sut.LoginAsync("testuser", "Passw0rd").ConfigureAwait(false);
        }

        [TestCleanup]
        public async Task Teardown()
        {
            await sut.LogoutAsync().ConfigureAwait(false);
        }

        [TestMethod]
        public async Task サーバーよりルートディレクトリ内のファイルの一覧を取得する()
        {
            await sut.VerifyTokenAsync().ConfigureAwait(false);
            var entries = await sut.GetEntriesAsync("").ConfigureAwait(false);
        }

        [TestMethod]
        public async Task サーバーにファイルをアップロードする()
        {
            await sut.VerifyTokenAsync().ConfigureAwait(false);
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test.txt", new MemoryStream(Encoding.UTF8.GetBytes("test1"))).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task サーバーのファイルを移動する()
        {
            await sut.VerifyTokenAsync().ConfigureAwait(false);
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test1.txt", new MemoryStream(Encoding.UTF8.GetBytes("test1"))).ConfigureAwait(false);
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test2.txt", new MemoryStream(Encoding.UTF8.GetBytes("test2"))).ConfigureAwait(false);
            await sut.DeleteDocumentAsync("test1/subtest1/test2.txt").ConfigureAwait(false);
            await sut.MoveDocumentAsync("test1/subtest1/test1.txt", "test1/subtest1/test2.txt").ConfigureAwait(false);
        }

        [TestMethod]
        public async Task 存在しないファイルを取得する()
        {
            await sut.VerifyTokenAsync().ConfigureAwait(false);
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test1.txt", new MemoryStream(Encoding.UTF8.GetBytes("test1"))).ConfigureAwait(false);
            await sut.DeleteDocumentAsync("test1/subtest1/test1.txt").ConfigureAwait(false);
            Assert.IsNull(await sut.GetDocumentAsync("test1/subtest1/test1.txt").ConfigureAwait(false));
        }
    }
}
