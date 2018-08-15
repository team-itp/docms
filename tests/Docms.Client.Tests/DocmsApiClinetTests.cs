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
        public void Setup()
        {
            sut = new DocmsApiClinet("http://localhost:51693");
        }

        [TestMethod]
        public async Task サーバーよりルートディレクトリ内のファイルの一覧を取得する()
        {
            var entries = await sut.GetEntriesAsync("");
        }

        [TestMethod]
        public async Task サーバーにファイルをアップロードする()
        {
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test.txt", new MemoryStream(Encoding.UTF8.GetBytes("test1")));
        }

        [TestMethod]
        public async Task サーバーのファイルを移動する()
        {
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test1.txt", new MemoryStream(Encoding.UTF8.GetBytes("test1")));
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test2.txt", new MemoryStream(Encoding.UTF8.GetBytes("test2")));
            await sut.DeleteDocumentAsync("test1/subtest1/test2.txt");
            await sut.MoveDocumentAsync("test1/subtest1/test1.txt", "test1/subtest1/test2.txt");
        }
    }
}
