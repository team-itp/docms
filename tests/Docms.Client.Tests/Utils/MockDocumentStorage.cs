using Docms.Client.DocumentStores;
using Docms.Client.Starter;
using System;
using System.Threading;
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
