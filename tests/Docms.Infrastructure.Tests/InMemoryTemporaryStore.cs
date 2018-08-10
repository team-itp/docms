using Docms.Infrastructure.DataStores;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Tests
{
    [TestClass]
    public class InMemoryDataStoreTests
    {
        [TestMethod]
        public async Task GuidをキーとしてStreamを保存できること()
        {
            var sut = new InMemoryTemporaryStore();
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var time1 = await sut.SaveAsync(id1, new MemoryStream(Encoding.UTF8.GetBytes("Hello, world1")));
            var time2 = await sut.SaveAsync(id2, new MemoryStream(Encoding.UTF8.GetBytes("Hello, world2")));
            using (var stream = await sut.OpenStreamAsync(id1))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                Assert.AreEqual("Hello, world1", await reader.ReadToEndAsync());
            }
            using (var stream = await sut.OpenStreamAsync(id1))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                Assert.AreEqual("Hello, world1", await reader.ReadToEndAsync());
            }
            await sut.DeleteBeforeAsync(time1);
            Assert.IsNull(await sut.OpenStreamAsync(id1));
            Assert.IsNotNull(await sut.OpenStreamAsync(id2));
            await sut.DeleteAsync(id2);
            Assert.IsNull(await sut.OpenStreamAsync(id2));
        }
    }
}
