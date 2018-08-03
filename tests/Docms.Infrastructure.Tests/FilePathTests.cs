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
            Assert.AreEqual("test.txt", sut.ToString());
            Assert.AreEqual(null, sut.DirectoryPath);
            Assert.AreEqual("test.txt", sut.FileName);
            Assert.AreEqual("test", sut.FileNameWithoutExtension);
            Assert.AreEqual(".txt", sut.Extension);
        }

        [TestMethod]
        public void 階層のあるパスの文字列からインスタンス化して拡張子とファイル名とディレクトリ名が取得できること()
        {
            var sut = new FilePath("path1\\path2\\test.txt");
            Assert.AreEqual("path1/path2/test.txt", sut.ToString());
            Assert.AreEqual(new FilePath("path1\\path2"), sut.DirectoryPath);
            Assert.AreEqual(new FilePath("path1"), sut.DirectoryPath.DirectoryPath);
            Assert.AreEqual("test.txt", sut.FileName);
            Assert.AreEqual("test", sut.FileNameWithoutExtension);
            Assert.AreEqual(".txt", sut.Extension);
        }

        [TestMethod]
        public void 拡張子のないパスの文字列からインスタンス化してファイル名とディレクトリ名が取得できること()
        {
            var sut = new FilePath("path1\\test");
            Assert.AreEqual("path1/test", sut.ToString());
            Assert.AreEqual(new FilePath("path1"), sut.DirectoryPath);
            Assert.AreEqual(null, sut.DirectoryPath.DirectoryPath);
            Assert.AreEqual("test", sut.FileName);
            Assert.AreEqual("test", sut.FileNameWithoutExtension);
            Assert.AreEqual(null, sut.Extension);
        }
    }
}
