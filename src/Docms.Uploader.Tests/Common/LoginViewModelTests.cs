using Docms.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Docms.Uploader.Common
{
    [TestClass]
    public class LoginViewModelTests
    {
        [TestMethod]
        public void インスタンス化直後にはエラーが存在しない()
        {
            var mockClient = new MockDocmsClient();
            var sut = new LoginViewModel(mockClient);
            Assert.IsFalse(sut.HasErrors);
        }
    }
}
