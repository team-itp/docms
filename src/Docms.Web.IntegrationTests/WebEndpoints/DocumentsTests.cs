using Docms.Web.Tests.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docms.Web.IntegrationTests.MvcEndpoints
{
    [TestClass]
    public class DocumentsTests
    {
        private WebApplicationFactory<Startup> _factory;

        [TestInitialize]
        public void Setup()
        {
            _factory = TestUtils.CreateWebApplicationFactory();
        }

        [TestMethod]
        public async Task documents_エンドポイントのリクエストが200番台のステータスを返却する()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/documents");
            response.EnsureSuccessStatusCode();
            Assert.AreEqual("text/html; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }
    }
}
