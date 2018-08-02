using Docms.Infrastructure.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Docms.Infrastructure.Tests
{
    [TestClass]
    public class FilePathTests
    {
        [TestMethod]
        public void 階層のないパスの文字列からインスタンス化して拡張子とファイル名が取得できること()
        {
            var sut = new FilePath("test.txt");
            Assert.AreEqual(null, sut.DirectoryPath);
            Assert.AreEqual("test.txt", sut.FileName);
            Assert.AreEqual("test", sut.FileNameWithoutExtension);
            Assert.AreEqual(".txt", sut.Extension);
        }
    }
}
