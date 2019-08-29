using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var builder = new WebHostBuilder().Configure(app => app.UseWebDav());
            var server = new TestServer(builder);
            var client = server.CreateClient();
            var request = new HttpRequestMessage(new HttpMethod("PROPFIND"), server.BaseAddress);
            request.Content = new StringContent(@"<?xml version=""1.0"" encoding=""utf-8"" ?><D:propfind xmlns:D=""DAV:""><D:prop xmlns:R=""http://ns.example.com/boxschema/""><R:bigbox/></D:prop></D:propfind>", Encoding.UTF8, "application/xml");
            var response = await client.SendAsync(request).ConfigureAwait(false);
            var xml = XDocument.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            Assert.AreEqual("prop", xml.Root.Name.LocalName);
        }
    }
}
