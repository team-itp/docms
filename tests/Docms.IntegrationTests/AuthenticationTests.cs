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
            WebDriverUtils.LoginAsAdmin(_driver);
            Assert.AreEqual(Constants.UrlFor("/files/view"), _driver.Url);
            Assert.IsNotNull(_driver.Manage().Cookies.GetCookieNamed(".AspNetCore.Identity.Application"));
        }

        [TestMethod]
        public void RedirectToLoginPageWhenLogout()
        {
            WebDriverUtils.LoginAsAdmin(_driver);
            var navbarSettingsDropdownLink = _driver.FindElement(By.CssSelector("#navbarSettingsDropdown"));
            navbarSettingsDropdownLink.Click();
            var logoutButton = _driver.FindElement(By.CssSelector("form[action='/account/logout'] button[type=submit]"));
            logoutButton.Click();
            Assert.AreEqual(Constants.UrlFor("/account/login?returnUrl=%2F"), _driver.Url);
            Assert.IsNull(_driver.Manage().Cookies.GetCookieNamed(".AspNetCore.Identity.Application"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            _driver.Quit();
        }
    }
}
