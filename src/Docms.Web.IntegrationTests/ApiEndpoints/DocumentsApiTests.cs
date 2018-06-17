using Docms.Web.Tests.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docms.Web.IntegrationTests.ApiEndpoints
{
    [TestClass]
    public class DocumentsApiTests
    {
        private WebApplicationFactory<Startup> _factory;

        [TestInitialize]
        public void Setup()
        {
            _factory = TestUtils.CreateWebApplicationFactory();
        }

        [TestMethod]
        public async Task api_documents_エンドポイントのリクエストが200番台のステータスを返却する()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/documents");
            response.EnsureSuccessStatusCode();
            Assert.AreEqual("application/json; charset=utf-8",
                response.Content.Headers.ContentType.ToString());
        }
    }
}
