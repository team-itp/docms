using Docms.Domain.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Docms.Domain.Tests
{
    [TestClass]
    public class TreeEntryTests
    {
        [TestMethod]
        public void ファイルのエントリーを作成する()
        {
            var sut = TreeEntry.CreateFileEntry("README", new Hash("a906cb2a4a904a152e80877d4088654daad0c859"));
            Assert.AreEqual("100644 blob a906cb2a4a904a152e80877d4088654daad0c859      README", sut.ToString());
        }
    }
}
