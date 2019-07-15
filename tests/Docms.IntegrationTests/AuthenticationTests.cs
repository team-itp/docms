using Docms.IntegrationTests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System.Linq;

namespace Docms.IntegrationTests
{
    [TestClass]
    public class AuthenticationTests
    {
        private IWebDriver _driver;

        [TestInitialize]
        public void Initialize()
        {
            _driver = WebDriverUtils.InitializeChromeDriver();
        }

        [TestMethod]
        public void RedirectToLoginPageIfNotLoggedIn()
        {
            WebDriverUtils.GoTo(_driver, "");
            Assert.AreEqual(Constants.UrlFor("/account/login?returnUrl=%2F"), _driver.Url);
        }

        [TestMethod]
        public void LoginFailWhenInvalidCredentials()
        {
            WebDriverUtils.Login(_driver, "admin", "invalidpassword");
            Assert.AreEqual(Constants.UrlFor("/account/login?returnUrl=%2F"), _driver.Url);
            Assert.IsTrue(_driver.FindElements(By.CssSelector(".text-danger")).Any(e => e.Text == "ユーザー名またはパスワードが不正です。"));
        }

        [TestMethod]
        public void RedirectToFileListWhenLoginSucceeded()
        {
            WebDriverUtils.Login(_driver, "admin", "05Jgx6Uh");
            Assert.AreEqual(Constants.UrlFor("/files/view"), _driver.Url);
            Assert.IsNotNull(_driver.Manage().Cookies.GetCookieNamed(".AspNetCore.Identity.Application"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            _driver.Quit();
        }
    }
}
