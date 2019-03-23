using Docms.Client.Operations;
using Docms.Client.Tasks;
using Docms.Client.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Docms.Client.Tests
{
    [TestClass]
    public class SyncTaskTest
    {
        private MockApplicationContext context;
        private SyncTask sut;

        [TestInitialize]
        public void Setup()
        {
            context = new MockApplicationContext();
            sut = new SyncTask(context);
        }

        [TestMethod]
        public void ローカルファイルの同期処理を実行する()
        {
            sut.Start();
            var operation = default(IOperation);
            context.App.BeforeInvoke += new EventHandler<InvokeEventArgs>((s, e) => operation = e.Operation);
            context.MockApp.Process();
            Assert.IsTrue(operation is LocalDocumentStorageSyncOperation);
        }
    }
}
