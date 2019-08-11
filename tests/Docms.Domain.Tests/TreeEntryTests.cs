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

        [TestMethod]
        public void ディレクトリのエントリーを作成する()
        {
            var sut = TreeEntry.CreateTreeEntry("lib", new Hash("99f1a6d12cb4b6f19c8655fca46c3ecf317074e0"));
            Assert.AreEqual("040000 tree 99f1a6d12cb4b6f19c8655fca46c3ecf317074e0      lib", sut.ToString());
        }
    }
}
