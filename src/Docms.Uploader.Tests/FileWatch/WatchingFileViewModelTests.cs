using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Docms.Uploader.FileWatch
{
    [TestClass]
    public class WatchingFileViewModelTests
    {
        private WatchingFileViewModel sut;

        [TestInitialize]
        public void Setup()
        {
            sut = WatchingFileViewModel.Create("C:\\testdir\\testdir2\\test.PNG", "C:\\testdir");
        }

        [TestMethod]
        public void 相対パスが表示名として設定されること()
        {
            Assert.AreEqual("testdir2\\test.PNG", sut.DisplayPath);
        }

        [TestMethod]
        public void ファイル名が設定されること()
        {
            Assert.AreEqual("test.PNG", sut.FileName);
        }

        [TestMethod]
        public void 拡張子が小文字で設定されること()
        {
            Assert.AreEqual(".png", sut.Extension);
        }


        [TestMethod]
        public void 絶対パスが設定されること()
        {
            Assert.AreEqual("C:\\testdir\\testdir2\\test.PNG", sut.FullPath);
        }
    }
}
