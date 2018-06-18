using Docms.Web.Tests.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;

namespace Docms.Web.IntegrationTests.ApiEndpoints
{
    [TestClass]
    public class BlobsTests
    {
        private WebApplicationFactory<Startup> _factory;

        [TestInitialize]
        public void Setup()
        {
            _factory = TestUtils.CreateWebApplicationFactory();
        }

        [TestMethod]
        public async Task ファイルをアップロードすることができる()
        {
            var client = _factory.CreateClient();
            var multipart = new MultipartFormDataContent();
            multipart.Add(new StringContent("Test"), "file", "filename.txt");
            var response = await client.PostAsync("/blobs", multipart);
            response.EnsureSuccessStatusCode();
        }
    }
}
