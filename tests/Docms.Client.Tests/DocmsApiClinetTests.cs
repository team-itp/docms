using Docms.Client.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class DocmsApiClinetTests
    {
        [TestMethod]
        public async Task サーバーよりルートディレクトリ内のファイルの一覧を取得する()
        {
            var sut = new DocmsApiClinet("http://localhost:51693", "api/v1");
            var rootContainer = await sut.GetEntriesAsync("/");
        }
    }
}
