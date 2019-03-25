using Docms.Client.Data;
using Docms.Client.Operations;
using Docms.Client.Tasks;
using Docms.Client.Tests.Utils;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class ApplicationEngineTests
    {
        private MockApplicationContext context;
        private ApplicationEngine sut;

        [TestMethod]
        public void アプリケーションが終了するまでタスクを実行し続ける()
        {
            var operation = default(IOperation);
            Task.Run(() => sut.Start());

            // 一周目
            // LocalDocumentStorageSyncOperation
            operation = context.MockApp.GetNextOperation();
            Assert.IsTrue(operation is LocalDocumentStorageSyncOperation);
            operation.Start();
            // RemoteDocumentStorageSyncOperation
            operation = context.MockApp.GetNextOperation();
            Assert.IsTrue(operation is RemoteDocumentStorageSyncOperation);
            operation.Start();
            // DetermineDiffOperation
            operation = context.MockApp.GetNextOperation();
            Assert.IsTrue(operation is DetermineDiffOperation);
            operation.Start();

            context.MockApp.Shutdown();
            // 二周目
            // LocalDocumentStorageSyncOperation
            operation = context.MockApp.GetNextOperation();
            Assert.IsTrue(operation is LocalDocumentStorageSyncOperation);
            operation.Start();
            // RemoteDocumentStorageSyncOperation
            operation = context.MockApp.GetNextOperation();
            Assert.IsTrue(operation is RemoteDocumentStorageSyncOperation);
            operation.Start();
            // DetermineDiffOperation
            operation = context.MockApp.GetNextOperation();
            Assert.IsTrue(operation is DetermineDiffOperation);
            operation.Start();

            
        }
    }
}
