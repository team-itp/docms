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
            sut.Invoke(token =>
            {
                executed = true;
                are.Set();
            });

            are.WaitOne();
            sut.Shutdown();
            Assert.IsTrue(executed);
        }

        [TestMethod]
        public void アプリケーションでオペレーションを2回実行し同時に1つしか実行されないことを確認する()
        {
            var executed1 = false;
            var executed2 = false;
            var are = new AutoResetEvent(false);
            var sut = new Application();
            var task = Task.Run(() => sut.Run());
            sut.Invoke(token =>
            {
                executed1 = true;
                are.Set();
                are.Reset();
                are.WaitOne();
            });
            sut.Invoke(token =>
            {
                executed2 = true;
                are.Set();
            });

            are.WaitOne();
            Assert.IsTrue(executed1);
            Assert.IsFalse(executed2);
            are.Set();
            are.Reset();
            are.WaitOne();
            Assert.IsTrue(executed2);
            sut.Shutdown();
        }

        [TestMethod]
        public void アプリケーションで2個目のキューが残っている場合に終了して2回目のキューが実行されないこと()
        {
            var executed1 = false;
            var executed2 = false;
            var are = new AutoResetEvent(false);
            var sut = new Application();
            var task = Task.Run(() => sut.Run());
            sut.Invoke(token =>
            {
                executed1 = true;
                are.Set();
                are.Reset();
                are.WaitOne();
            });
            sut.Invoke(token =>
            {
                executed2 = true;
                are.Set();
            });

            are.WaitOne();
            Assert.IsTrue(executed1);
            Assert.IsFalse(executed2);

            sut.Shutdown();
            are.Set();
            task.Wait();
            Assert.IsFalse(executed2);
        }

        [TestMethod]
        public void アプリケーションで実行時にCancellationTokenを指定してcancelした場合に当該のオペレーションが実行されない()
        {
            var executed2 = false;
            var are = new AutoResetEvent(false);
            var cts = new CancellationTokenSource();
            var sut = new Application();
            var task = Task.Run(() => sut.Run());
            sut.Invoke(token =>
            {
                are.Set();
                are.Reset();
                are.WaitOne();
            });
            sut.Invoke(token =>
            {
                executed2 = true;
                are.Set();
            }, cts.Token);

            are.WaitOne();
            cts.Cancel();
            are.Set();

            Assert.IsFalse(executed2);
            sut.Shutdown();
            Task.WaitAny(new[] { task }, 20);
        }
    }
}
