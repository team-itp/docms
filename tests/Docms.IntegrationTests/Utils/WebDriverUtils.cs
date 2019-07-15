using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Docms.IntegrationTests.Utils
{
    static class WebDriverUtils
    {
        internal static IWebDriver InitializeChromeDriver()
        {
            var options = new ChromeOptions
            {
                PageLoadStrategy = PageLoadStrategy.Normal,
            };
            return new ChromeDriver(options);
        }

        internal static void GoTo(IWebDriver driver, string path)
        {
            driver.Navigate().GoToUrl(Constants.UrlFor(path));
        }

        internal static void Login(IWebDriver driver, string accountName, string password)
        {
            if (!driver.Url.StartsWith(Constants.UrlFor("/account/login")))
            {
                driver.Navigate().GoToUrl(Constants.UrlFor(""));
                Assert.IsTrue(driver.Url.StartsWith(Constants.UrlFor("/account/login")));
            }
            var accountNameEl = driver.FindElement(By.Name("AccountName"));
            var passwordEl = driver.FindElement(By.Name("Password"));
            var sendButton = driver.FindElement(By.CssSelector("form[action='/account/login?returnUrl=%2F'] button[type=submit]"));
            accountNameEl.SendKeys(accountName);
            passwordEl.SendKeys(password);
            sendButton.Click();
        }

        internal static void LoginAsAdmin(IWebDriver driver)
        {
            Login(driver, "admin", "05Jgx6Uh");
        }
    }
}
