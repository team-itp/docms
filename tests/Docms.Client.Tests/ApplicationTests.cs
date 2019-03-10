using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class ApplicationTests
    {
        [TestMethod]
        public void アプリケーションを起動して終了する()
        {
            var sut = new Application();
            var task = Task.Run(() => sut.Run());
            Task.WaitAny(new[] { task }, 100);
            Assert.IsFalse(task.IsCompleted);
            sut.Shutdown();
            Task.WaitAny(new[] { task }, 100);
            Assert.IsTrue(task.IsCompleted);
        }
    }
}
