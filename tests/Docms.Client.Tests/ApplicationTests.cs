using Docms.Client.Uploading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    class MockLocalFileUploader : ILocalFileUploader
    {
        private List<Func<CancellationToken, Task>> callbacks;

        public MockLocalFileUploader(List<Func<CancellationToken, Task>> callbacks)
        {
            this.callbacks = callbacks;
        }

        public async Task UploadAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var cb in callbacks)
            {
                await cb.Invoke(cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }

    class MockApplicationContext : IApplicationContext
    {
        public TaskCompletionSource<bool> InitializedCompletionSource = new TaskCompletionSource<bool>();
        public TaskCompletionSource<bool> DisposedCompletionSource = new TaskCompletionSource<bool>();

        public MockApplicationContext(ILocalFileUploader uploader)
        {
            Uploader = uploader;
        }

        public ILocalFileUploader Uploader { get; set; }

        public Task InitializeAsync()
        {
            InitializedCompletionSource.SetResult(true);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            DisposedCompletionSource.SetResult(true);
        }
    }

    [TestClass]
    public class ApplicationTests
    {
        ILogger logger = LogManager.GetCurrentClassLogger();

        [TestMethod]
        public void アプリケーションを起動して終了する()
        {
            List<Func<CancellationToken, Task>> handlers = new List<Func<CancellationToken, Task>>();
            var mockUploader = new MockLocalFileUploader(handlers);
            var mockContext = new MockApplicationContext(mockUploader);
            var sut = new Application(mockContext);
            Assert.IsFalse(mockContext.InitializedCompletionSource.Task.IsCompleted);
            var task = Task.Run(() => sut.Run());
            Task.WaitAny(new[] { mockContext.InitializedCompletionSource.Task }, 100);
            Assert.IsTrue(mockContext.InitializedCompletionSource.Task.IsCompleted);
            Assert.IsFalse(task.IsCompleted);
            sut.Quit();
            Task.WaitAny(new[] { mockContext.DisposedCompletionSource.Task }, 100);
            Assert.IsTrue(mockContext.DisposedCompletionSource.Task.IsCompleted);
            Task.WaitAny(new[] { task }, 100);
            Assert.IsTrue(task.IsCompleted);
        }

        [TestMethod]
        public void 正常なキューを載せて正常に処理が実行される()
        {
            var are = new AutoResetEvent(false);
            List<Func<CancellationToken, Task>> handlers = new List<Func<CancellationToken, Task>>();
            handlers.Add(token =>
            {
                var tcs = new TaskCompletionSource<bool>();
                Task.Run(() =>
                {
                    are.WaitOne();
                    tcs.SetResult(true);
                });
                return tcs.Task;
            });
            var mockUploader = new MockLocalFileUploader(handlers);
            var mockContext = new MockApplicationContext(mockUploader);
            var sut = new Application(mockContext);
            var task = Task.Run(() => sut.Run());
            Task.WaitAny(new[] { task }, 100);

            var uploadTask = sut.UploadAllFilesAsync();
            Task.WaitAny(new[] { uploadTask }, 100);
            Assert.IsFalse(uploadTask.IsCompleted);

            are.Set();
            Task.WaitAny(new[] { uploadTask }, 100);
            Assert.IsTrue(uploadTask.IsCompleted);

            var quitTask = Task.Run(() => sut.Quit());
            Task.WaitAny(new[] { quitTask }, 100);

            Assert.IsTrue(uploadTask.IsCompletedSuccessfully);
            Assert.IsTrue(task.IsCompletedSuccessfully);
            Assert.IsTrue(quitTask.IsCompletedSuccessfully);
        }

        [TestMethod]
        public void キューをキャンセルできる()
        {
            var are = new AutoResetEvent(false);
            var secondFlg = false;
            List<Func<CancellationToken, Task>> handlers = new List<Func<CancellationToken, Task>>();
            handlers.Add(token =>
            {
                var tcs = new TaskCompletionSource<bool>();
                Task.Run(() =>
                {
                    are.WaitOne();
                    tcs.SetResult(true);
                });
                return tcs.Task;
            });
            handlers.Add(token =>
            {
                secondFlg = true;
                return Task.CompletedTask;
            });
            var mockUploader = new MockLocalFileUploader(handlers);
            var mockContext = new MockApplicationContext(mockUploader);
            var sut = new Application(mockContext);
            var task = Task.Run(() => sut.Run());
            Task.WaitAny(new[] { task }, 100);

            var uploadTask = sut.UploadAllFilesAsync();
            Task.WaitAny(new[] { uploadTask }, 100);
            var quitTask = Task.Run(() => sut.Quit());
            Task.WaitAny(new[] { quitTask }, 100);

            Assert.IsFalse(uploadTask.IsCompleted);

            are.Set();

            while (!quitTask.IsCompleted && !task.IsCompleted)
            {
                Thread.Sleep(10);
            }

            Assert.IsTrue(uploadTask.IsCanceled);
            Assert.IsTrue(task.IsCompletedSuccessfully);

            Assert.IsFalse(secondFlg);
        }

        [TestMethod]
        public void キューでエラーが発生しても処理が継続する()
        {
            var are = new AutoResetEvent(false);
            List<Func<CancellationToken, Task>> handlers = new List<Func<CancellationToken, Task>>();
            handlers.Add(token =>
            {
                var tcs = new TaskCompletionSource<bool>();
                Task.Run(() =>
                {
                    are.WaitOne();
                    tcs.SetException(new Exception());
                });
                return tcs.Task;
            });
            var mockUploader = new MockLocalFileUploader(handlers);
            var mockContext = new MockApplicationContext(mockUploader);
            var sut = new Application(mockContext);
            var task = Task.Run(() => sut.Run());
            Task.WaitAny(new[] { task }, 100);

            var uploadTask1 = sut.UploadAllFilesAsync();
            Task.WaitAny(new[] { uploadTask1 }, 100);
            are.Set();

            handlers.Clear();
            var execFlg = false;
            handlers.Add(token =>
            {
                execFlg = true;
                return Task.CompletedTask;
            });

            var uploadTask2 = sut.UploadAllFilesAsync();
            Task.WaitAny(new[] { uploadTask2 }, 100);

            var quitTask = Task.Run(() => sut.Quit());
            Task.WaitAny(new[] { quitTask }, 100);

            Assert.IsFalse(uploadTask1.IsCompleted && uploadTask1.IsFaulted);
            Assert.IsTrue(uploadTask2.IsCompletedSuccessfully);
            Assert.IsTrue(task.IsCompletedSuccessfully);

            Assert.IsTrue(execFlg);
        }
    }

}
