using Docms.Domain.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Docms.Domain.Tests
{
    [TestClass]
    public class TreeEntryTests
    {
        [TestMethod]
        public void ファイルのエントリーを作成する()
        {
            var sut = TreeEntry.CreateEntry("README", new BlobObject(new Hash("a906cb2a4a904a152e80877d4088654daad0c859"), null));
            Assert.AreEqual("100644 blob a906cb2a4a904a152e80877d4088654daad0c859      README", sut.ToString());
        }

        [TestMethod]
        public void ディレクトリのエントリーを作成する()
        {
            var sut = TreeEntry.CreateEntry("lib", new TreeObject(new Hash("99f1a6d12cb4b6f19c8655fca46c3ecf317074e0"), Array.Empty<TreeEntry>()));
            Assert.AreEqual("040000 tree 99f1a6d12cb4b6f19c8655fca46c3ecf317074e0      lib", sut.ToString());
        }
    }
}
