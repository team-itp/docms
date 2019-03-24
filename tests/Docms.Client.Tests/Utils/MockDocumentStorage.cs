using Docms.Client.DocumentStores;
using Docms.Client.Starter;
using Docms.Client.Types;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Utils
{
    class MockDocumentStorage : DocumentStorageBase
    {
        public override IDocumentStreamToken GetDocumentStreamToken(PathString path)
        {
            return new MockDocmentStreamToken(Encoding.UTF8.GetBytes(path.ToString()));
        }

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
    }
}
