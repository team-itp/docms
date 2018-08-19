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
            await sut.LoginAsync("testuser", "Passw0rd");
        }

        [TestCleanup]
        public async Task Teardown()
        {
            await sut.LogoutAsync();
        }

        [TestMethod]
        public async Task サーバーよりルートディレクトリ内のファイルの一覧を取得する()
        {
            await sut.VerifyTokenAsync();
            var entries = await sut.GetEntriesAsync("");
        }

        [TestMethod]
        public async Task サーバーにファイルをアップロードする()
        {
            await sut.VerifyTokenAsync();
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test.txt", new MemoryStream(Encoding.UTF8.GetBytes("test1")));
        }

        [TestMethod]
        public async Task サーバーのファイルを移動する()
        {
            await sut.VerifyTokenAsync();
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test1.txt", new MemoryStream(Encoding.UTF8.GetBytes("test1")));
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test2.txt", new MemoryStream(Encoding.UTF8.GetBytes("test2")));
            await sut.DeleteDocumentAsync("test1/subtest1/test2.txt");
            await sut.MoveDocumentAsync("test1/subtest1/test1.txt", "test1/subtest1/test2.txt");
        }
    }
}
