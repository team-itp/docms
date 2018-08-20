using Docms.Client.FileTrees;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Docms.Client.Tests
{
    [TestClass]
    public class FileSystemTreeTests
    {
        [TestMethod]
        public void ファイルを追加する()
        {
            var sut = new FileSystemTree();
            sut.AddFile("test1/content.txt");
            Assert.IsTrue(sut.Exists("test1/content.txt"));
            var delta = sut.GetDelta();
            Assert.AreEqual(1, delta.Count());
            Assert.AreEqual("test1/content.txt", delta.First().Path);
        }
        [TestMethod]
        public void ファイルを更新する()
        {
            var sut = new FileSystemTree();
            sut.AddFile("test1/content.txt");
            sut.ClearDelta();
            var delta = sut.GetDelta();
            Assert.AreEqual(0, delta.Count());

            sut.Update("test1/content.txt");
            Assert.IsTrue(sut.Exists("test1/content.txt"));
            delta = sut.GetDelta();
            Assert.AreEqual(1, delta.Count());
            Assert.AreEqual("test1/content.txt", delta.First().Path);
        }
    }
}
