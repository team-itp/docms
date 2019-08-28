using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Docms.Infrastructure.WebDav.Tests
{
    [TestClass]
    public class PropfindTests
    {
        [TestMethod]
        public async Task PROPFINDに一つのプロパティ名をリクエストする()
        {
            var builder = new WebHostBuilder()
                .UseEnvironment("Development")
                .UseStartup<Startup>();

            var testServer = new TestServer(builder);
            var client = testServer.CreateClient();

            var request = new HttpRequestMessage(new HttpMethod("PROPFIND"), "https://localhost:44375/");
            request.Content = new StringContent(@"<?xml version=""1.0"" encoding=""utf-8"" ?><D:propfind xmlns:D=""DAV:""><D:prop xmlns:R=""http://ns.example.com/boxschema/""><R:bigbox/></D:prop></D:propfind>", Encoding.UTF8, "application/xml");
            var response = await client.SendAsync(request).ConfigureAwait(false);
            var xml = XDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            Assert.AreEqual("prop", xml.Root.Name.LocalName);
        }
    }
}
