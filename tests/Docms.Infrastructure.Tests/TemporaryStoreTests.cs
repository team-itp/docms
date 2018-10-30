using Docms.Infrastructure.DataStores;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Tests
{
    [TestClass]
    public class TemporaryStoreTests
    {
        [TestMethod]
        public async Task 小さなデータを保存できること()
        {
            var sut = new TemporaryStore();
            var smallData = Encoding.UTF8.GetBytes("Hello, world1");
            var data = await sut.CreateAsync(new MemoryStream(smallData), smallData.Length);
            using (var stream = await data.OpenStreamAsync())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                Assert.AreEqual("Hello, world1", await reader.ReadToEndAsync());
            }
            await sut.DisposeAsync(data);
        }

        [TestMethod]
        public async Task 大きなデータを保存できること()
        {
            var sut = new TemporaryStore();
            var largeData = Enumerable.Range(0, 30_000_000).Select(v => (byte)((v % 64) + 64)).ToArray();
            var data = await sut.CreateAsync(new MemoryStream(largeData), largeData.Length);
            using (var stream = await data.OpenStreamAsync())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                Assert.AreEqual(Encoding.UTF8.GetString(largeData), await reader.ReadToEndAsync());
            }
            await sut.DisposeAsync(data);
        }
    }
}
