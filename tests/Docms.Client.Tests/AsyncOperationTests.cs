using Docms.Client.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class AsyncOperationTest
    {
        [TestMethod]
        public void 処理が完了した場合にTaskが完了となる()
        {
            var executed = false;
            var sut = new GenericAsyncOperation(token => { executed = true; return Task.CompletedTask; }, CancellationToken.None);
            Assert.IsFalse(sut.IsAborted);
            Assert.AreEqual(TaskStatus.WaitingForActivation, sut.Task.Status);
            sut.Start();
            Assert.IsTrue(executed);
            Assert.AreEqual(TaskStatus.RanToCompletion, sut.Task.Status);
        }

        [TestMethod]
        public void 当初からCancellationTokenがCancel状態の場合IsAbortがtrueに設定される()
        {
            var executed = false;
            var sut = new GenericAsyncOperation(token => { executed = true; return Task.CompletedTask; }, new CancellationToken(true));
            Assert.IsTrue(sut.IsAborted);
            Assert.AreEqual(TaskStatus.Canceled, sut.Task.Status);
            sut.Start();
            Assert.IsFalse(executed);
        }

        [TestMethod]
        public void インスタンス生成後にAbortが呼ばれた場合()
        {
            var executed = false;
            var sut = new GenericAsyncOperation(token => { executed = true; return Task.CompletedTask; }, CancellationToken.None);
            sut.Abort();
            Assert.IsTrue(sut.IsAborted);
            Assert.AreEqual(TaskStatus.Canceled, sut.Task.Status);
            sut.Start();
            Assert.IsFalse(executed);
        }

        [TestMethod]
        public void インスタンス生成後にCancellationTokenのCancel状態が変更されたとき()
        {
            var executed = false;
            var cts = new CancellationTokenSource();
            var sut = new GenericAsyncOperation(token => { executed = true; return Task.CompletedTask; }, cts.Token);
            cts.Cancel();
            Assert.IsTrue(sut.IsAborted);
            Assert.AreEqual(TaskStatus.Canceled, sut.Task.Status);
            sut.Start();
            Assert.IsFalse(executed);
        }

        [TestMethod]
        public void 処理開始後にキャンセルした場合処理が正常終了の場合にタスクも正常となる()
        {
            var are1 = new AutoResetEvent(false);
            var are2 = new AutoResetEvent(false);
            var cts = new CancellationTokenSource();
            var sut = new GenericAsyncOperation(token =>
            {
                are1.Set();
                are2.WaitOne();
                return Task.CompletedTask;
            }, cts.Token);
            var task = Task.Run(() => sut.Start());
            are1.WaitOne();
            cts.Cancel();

            Assert.IsFalse(sut.IsAborted);
            Assert.AreEqual(TaskStatus.WaitingForActivation, sut.Task.Status);

            are2.Set();
            task.Wait();

            Assert.IsFalse(sut.IsAborted);
            Assert.AreEqual(TaskStatus.RanToCompletion, sut.Task.Status);
        }

        [TestMethod]
        public void 処理開始後にキャンセルした場合処理がキャンセルの場合にオペレーションもキャンセルになる()
        {
            var are1 = new AutoResetEvent(false);
            var are2 = new AutoResetEvent(false);
            var cts = new CancellationTokenSource();
            var sut = new GenericAsyncOperation(token =>
            {
                are1.Set();
                are2.WaitOne();
                cts.Token.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }, cts.Token);
            var task = Task.Run(() => sut.Start());
            are1.WaitOne();
            cts.Cancel();

            Assert.IsFalse(sut.IsAborted);
            Assert.AreEqual(TaskStatus.WaitingForActivation, sut.Task.Status);

            are2.Set();
            task.Wait();

            Assert.IsFalse(sut.IsAborted);
            Assert.AreEqual(TaskStatus.Canceled, sut.Task.Status);
        }

        [TestMethod]
        public void 処理が完了した場合にTaskが完了となる_非同期の場合()
        {
            var executed = false;
            var sut = new GenericAsyncOperation(async token => { await Task.Yield(); executed = true; }, CancellationToken.None);
            Assert.IsFalse(sut.IsAborted);
            Assert.AreEqual(TaskStatus.WaitingForActivation, sut.Task.Status);
            sut.Start();
            Assert.IsTrue(executed);
            Assert.AreEqual(TaskStatus.RanToCompletion, sut.Task.Status);
        }

        [TestMethod]
        public void 当初からCancellationTokenがCancel状態の場合IsAbortがtrueに設定される_非同期の場合()
        {
            var executed = false;
            var sut = new GenericAsyncOperation(async token => { await Task.Yield(); executed = true; }, new CancellationToken(true));
            Assert.IsTrue(sut.IsAborted);
            Assert.AreEqual(TaskStatus.Canceled, sut.Task.Status);
            sut.Start();
            Assert.IsFalse(executed);
        }

        [TestMethod]
        public void インスタンス生成後にAbortが呼ばれた場合_非同期の場合()
        {
            var executed = false;
            var sut = new GenericAsyncOperation(async token => { await Task.Yield(); executed = true; }, CancellationToken.None);
            sut.Abort();
            Assert.IsTrue(sut.IsAborted);
            Assert.AreEqual(TaskStatus.Canceled, sut.Task.Status);
            sut.Start();
            Assert.IsFalse(executed);
        }

        [TestMethod]
        public void インスタンス生成後にCancellationTokenのCancel状態が変更されたとき_非同期の場合()
        {
            var executed = false;
            var cts = new CancellationTokenSource();
            var sut = new GenericAsyncOperation(async token => { await Task.Yield(); executed = true; }, cts.Token);
            cts.Cancel();
            Assert.IsTrue(sut.IsAborted);
            Assert.AreEqual(TaskStatus.Canceled, sut.Task.Status);
            sut.Start();
            Assert.IsFalse(executed);
        }

        [TestMethod]
        public void 処理開始後にキャンセルした場合処理が正常終了の場合にタスクも正常となる_非同期の場合()
        {
            var are1 = new AutoResetEvent(false);
            var are2 = new AutoResetEvent(false);
            var cts = new CancellationTokenSource();
            var sut = new GenericAsyncOperation(async token =>
            {
                await Task.Yield();
                are1.Set();
                are2.WaitOne();
            }, cts.Token);
            var task = Task.Run(() => sut.Start());
            are1.WaitOne();
            cts.Cancel();

            Assert.IsFalse(sut.IsAborted);
            Assert.AreEqual(TaskStatus.WaitingForActivation, sut.Task.Status);

            are2.Set();
            task.Wait();

            Assert.IsFalse(sut.IsAborted);
            Assert.AreEqual(TaskStatus.RanToCompletion, sut.Task.Status);
        }

        [TestMethod]
        public void 処理開始後にキャンセルした場合処理がキャンセルの場合にオペレーションもキャンセルになる_非同期の場合()
        {
            var are1 = new AutoResetEvent(false);
            var are2 = new AutoResetEvent(false);
            var cts = new CancellationTokenSource();
            var sut = new GenericAsyncOperation(async token =>
            {
                await Task.Yield();
                are1.Set();
                are2.WaitOne();
                cts.Token.ThrowIfCancellationRequested();
            }, cts.Token);
            var task = Task.Run(() => sut.Start());
            are1.WaitOne();
            cts.Cancel();

            Assert.IsFalse(sut.IsAborted);
            Assert.AreEqual(TaskStatus.WaitingForActivation, sut.Task.Status);

            are2.Set();
            task.Wait();

            Assert.IsFalse(sut.IsAborted);
            Assert.AreEqual(TaskStatus.Canceled, sut.Task.Status);
        }

        [TestMethod]
        public void 処理中に例外が発生した場合にタスクがエラーになる_同期の場合()
        {
            var sut = new GenericAsyncOperation(token =>
            {
                throw new Exception();
            });
            sut.Start();

            Assert.IsFalse(sut.IsAborted);
            Assert.AreEqual(TaskStatus.Faulted, sut.Task.Status);
        }

        [TestMethod]
        public void 処理中に例外が発生した場合にタスクがエラーになる_非同期の場合()
        {
            var sut = new GenericAsyncOperation(async token =>
            {
                await Task.Yield();
                throw new Exception();
            });
            sut.Start();

            Assert.IsFalse(sut.IsAborted);
            Assert.AreEqual(TaskStatus.Faulted, sut.Task.Status);
        }
    }
}