using Docms.Uploader.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Docms.Uploader.Upload
{
    [TestClass]
    public class UploaderViewModelTests
    {
        private MockDocmsClient mockClient;
        private UploaderViewModel sut;

        [TestInitialize]
        public void Setup()
        {
            mockClient = new MockDocmsClient();
            sut = new UploaderViewModel(mockClient);
        }

        [TestMethod]
        public void インスタンス化直後ファイルが未選択状態でアップロードボタンが押せない()
        {
            Assert.AreEqual(0, sut.SelectedFiles.Count);
            Assert.IsFalse(sut.CanUpload());
        }
    }
}
