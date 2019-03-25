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
    }
}
