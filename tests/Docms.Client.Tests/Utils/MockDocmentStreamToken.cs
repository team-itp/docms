using Docms.Client.DocumentStores;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Utils
{
    class MockDocmentStreamToken : IDocumentStreamToken
    {
        private readonly byte[] data;

        public MockDocmentStreamToken(byte[] data)
        {
            this.data = data;
        }

        public Task<Stream> GetStreamAsync()
        {
            return Task.FromResult(new MemoryStream(data) as Stream);
        }

        public void Dispose()
        {
        }
    }
}