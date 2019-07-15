using Docms.Client.Api;
using Docms.IntegrationTests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading.Tasks;

namespace Docms.IntegrationTests
{
    [TestClass]
    public class DocumentApiTests
    {
        [TestMethod]
        public async Task ThrowsConnectionExceptionWhenServerIsNotUp()
        {
            using (var server = new MockHttpServer(8000, context => { }))
            {
            }
            using (var client = new DocmsApiClient("docms-client-1", "http://localhost:8000"))
            {
                var docClient = client.CreateDocumentClient();
                await Assert.ThrowsExceptionAsync<ConnectionException>(() => docClient.GetEntriesAsync());
            }
        }

        [TestMethod]
        public async Task ThrowsUnauthorizedExceptionWhenClientIsNotAuthenticated()
        {
            using (var server = new MockHttpServer(8000, context => {
                context.Response.StatusCode = 401;
                context.Response.Close();
            }))
            using (var client = new DocmsApiClient("docms-client-1", "http://localhost:8000"))
            {
                var docClient = client.CreateDocumentClient();
                await Assert.ThrowsExceptionAsync<UnauthorizedException>(() => docClient.GetEntriesAsync());
            }
        }
    }
}
