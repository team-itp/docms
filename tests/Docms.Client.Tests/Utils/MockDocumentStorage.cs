using Docms.Client.DocumentStores;
using Docms.Client.Types;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Utils
{
    class MockDocumentStorage : DocumentStorageBase
    {
        public override Task Initialize()
        {
            return Task.CompletedTask;
        }

        public override Task Save()
        {
            return Task.CompletedTask;
        }

        public override Task Sync()
        {
            return Task.CompletedTask;
        }

        public override Task<IDocumentStreamToken> ReadDocument(PathString path)
        {
            return Task.FromResult<IDocumentStreamToken>(new DefaultStreamToken(new MemoryStream(Encoding.UTF8.GetBytes(path.ToString()))));
        }

        public override Task WriteDocument(PathString path, Stream stream, DateTime created, DateTime lastModified)
        {
            return Task.CompletedTask;
        }
    }
}
