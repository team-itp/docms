using Docms.Uploader.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Docms.Uploader.Common
{
    [TestClass]
    public class LoginViewModelTests
    {
        private MockDocmsClient mockClient;
        private LoginViewModel sut;

        [TestInitialize]
        public void Setup()
        {
            mockClient = new MockDocmsClient();
            sut = new LoginViewModel(mockClient);
        }

        [TestMethod]
        public void インスタンス化直後にはエラーが存在しない()
        {
            Assert.IsFalse(sut.HasErrors);
        }

        [TestMethod]
        public void インスタンス化直後にログインボタンを押下するとエラーとなる()
        {
            sut.Login();
            Assert.IsTrue(sut.HasErrors);
        }

        [TestMethod]
        public void リセットボタンでエラーがない状態に戻る()
        {
            sut.Login();
            sut.Reset();
            Assert.IsFalse(sut.HasErrors);
        }

        [TestMethod]
        public void ユーザー名が空欄の場合エラーとなる()
        {
            sut.Username = "";
            Assert.IsTrue(sut.HasErrors);
        }

        [TestMethod]
        public void ユーザー名が空欄ではない場合エラーとならない()
        {
            sut.Username = "invalidusername";
            Assert.IsFalse(sut.HasErrors);
        }

        [TestMethod]
        public void パスワードが空欄の場合エラーとなる()
        {
            sut.Password = "";
            Assert.IsTrue(sut.HasErrors);
        }

        [TestMethod]
        public void パスワードが空欄ではない場合エラーとはならない()
        {
            sut.Password = "invalidpassword";
            Assert.IsFalse(sut.HasErrors);
        }

        [TestMethod]
        public void 誤ったログイン情報でログインボタンを押下するとモデルのエラーとはならないがエラーメッセージが表示される()
        {
            sut.Username = "invalidusername";
            sut.Password = "invalidpassword";
            sut.Login();
            Assert.IsFalse(sut.HasErrors);
            Assert.IsNotNull(sut.ErrorMessage);
        }

        [TestMethod]
        public void 誤ったログイン情報でログインボタンを押下したあとユーザー名を変更するとエラーメッセージが消える()
        {
            sut.Username = "invalidusername";
            sut.Password = "invalidpassword";
            sut.Login();
            sut.Username = "invalidusername2";
            Assert.IsNull(sut.ErrorMessage);
        }

        [TestMethod]
        public void 誤ったログイン情報でログインボタンを押下したあとパスワードを変更するとエラーメッセージが消える()
        {
            sut.Username = "invalidusername";
            sut.Password = "invalidpassword";
            sut.Login();
            sut.Password = "invalidpassword2";
            Assert.IsNull(sut.ErrorMessage);
        }

        [TestMethod]
        public void 正常なログイン情報でログインボタンを押下すると正常のイベントが発生する()
        {
            var isLoginSucceeded = false;
            sut.LoginSucceeded += (s, e) => isLoginSucceeded = true;
            sut.Username = "validusername";
            sut.Password = "validpassword";
            sut.Login();
            Assert.IsNull(sut.ErrorMessage);
            Assert.IsFalse(sut.HasErrors);
            Assert.IsTrue(isLoginSucceeded);
        }
    }
}
