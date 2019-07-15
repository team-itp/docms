using Docms.Client.Api;
using Docms.IntegrationTests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Docms.IntegrationTests
{
    [TestClass]
    public class DocumentTests
    {
        private IWebDriver _driver;
        private DocmsApiClient _client;

        [TestInitialize]
        public async Task Initialize()
        {
            _driver = WebDriverUtils.InitializeChromeDriver();
            _client = new DocmsApiClient("docms-client", Constants.TestUrlBase);
            await DocumentUtils.ClearAllAsync(_client.CreateDocumentClient()).ConfigureAwait(false);
            WebDriverUtils.LoginAsAdmin(_driver);
        }

        [TestMethod]
        public async Task ListRootDocuments()
        {
            await _client.CreateDocumentClient().UploadAsync("test.txt", new MemoryStream(Encoding.UTF8.GetBytes("test.txt"))).ConfigureAwait(false);
            _driver.Navigate().Refresh();
            var container = _driver.FindElement(By.CssSelector("body > div > div"));
            var children = container.FindElements(By.ClassName(""));
        }

        [TestCleanup]
        public void Cleanup()
        {
            _driver.Quit();
            _client.Dispose();
        }
    }
}
