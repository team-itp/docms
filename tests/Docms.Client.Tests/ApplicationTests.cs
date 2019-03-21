using Docms.Client.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
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
            Task.WaitAny(new[] { task }, 20);
            Assert.IsFalse(task.IsCompleted);
            sut.Shutdown();
            Task.WaitAny(new[] { task }, 20);
            Assert.IsTrue(task.IsCompleted);
        }

        [TestMethod]
        public void アプリケーションでオペレーションを実行する()
        {
            var executed = false;
            var are = new AutoResetEvent(false);
            var sut = new Application();
            var task = Task.Run(() => sut.Run());
            sut.Invoke(new GenericSyncOperation(token =>
            {
                executed = true;
                are.Set();
            }));

            are.WaitOne();
            sut.Shutdown();
            Assert.IsTrue(executed);
        }

        [TestMethod]
        public void アプリケーションでオペレーションを2回実行し同時に1つしか実行されないことを確認する()
        {
            var executed1 = false;
            var executed2 = false;
            var are1 = new AutoResetEvent(false);
            var are2 = new AutoResetEvent(false);
            var are3 = new AutoResetEvent(false);
            var sut = new Application();
            var task = Task.Run(() => sut.Run());
            sut.Invoke(new GenericSyncOperation(token =>
            {
                executed1 = true;
                are1.Set();
                are2.WaitOne();
            }));
            sut.Invoke(new GenericSyncOperation(token =>
            {
                executed2 = true;
                are3.Set();
            }));

            are1.WaitOne();
            Assert.IsTrue(executed1);
            Assert.IsFalse(executed2);

            are2.Set();
            are3.WaitOne();
            Assert.IsTrue(executed2);
            sut.Shutdown();
        }

        [TestMethod]
        public void アプリケーションで2個目のキューが残っている場合に終了して2回目のキューが実行されないこと()
        {
            var executed1 = false;
            var executed2 = false;
            var are1 = new AutoResetEvent(false);
            var are2 = new AutoResetEvent(false);
            var sut = new Application();
            var task = Task.Run(() => sut.Run());
            sut.Invoke(new GenericSyncOperation(token =>
            {
                executed1 = true;
                are1.Set();
                are2.WaitOne();
            }));
            sut.Invoke(new GenericSyncOperation(token =>
            {
                executed2 = true;
            }));

            are1.WaitOne();
            Assert.IsTrue(executed1);
            Assert.IsFalse(executed2);

            sut.Shutdown();
            are2.Set();
            task.Wait();
            Assert.IsFalse(executed2);
        }

        [TestMethod]
        public void アプリケーションで実行時にCancellationTokenを指定してcancelした場合に当該のオペレーションが実行されない()
        {
            var executed2 = false;
            var are1 = new AutoResetEvent(false);
            var are2 = new AutoResetEvent(false);
            var are3 = new AutoResetEvent(false);
            var cts = new CancellationTokenSource();
            var sut = new Application();
            var task = Task.Run(() => sut.Run());
            sut.Invoke(new GenericSyncOperation(token =>
            {
                are1.Set();
                are2.WaitOne();
            }));
            sut.Invoke(new GenericSyncOperation(token =>
            {
                executed2 = true;
            }, cts.Token));

            are1.WaitOne();
            cts.Cancel();
            are2.Set();

            Assert.IsFalse(executed2);
            sut.Shutdown();
            Task.WaitAny(new[] { task }, 20);
        }
    }
}
