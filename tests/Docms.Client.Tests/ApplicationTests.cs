using Docms.Client.Uploading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public Task UploadAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.WhenAll(callbacks.Select(c => c.Invoke(cancellationToken)));
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

            var uploadTask = sut.UploadAllFilesAsync();
            Task.WaitAny(new[] { uploadTask }, 100);
            Assert.IsFalse(uploadTask.IsCompleted);

            are.Set();
            Task.WaitAny(new[] { uploadTask }, 100);
            Assert.IsTrue(uploadTask.IsCompleted);

            sut.Quit();
            Task.WaitAny(new[] { task }, 100);
            Assert.IsTrue(task.IsCompleted);
        }

        [TestMethod]
        public void キューをキャンセルできる()
        {
            var are = new AutoResetEvent(false);
            List<Func<CancellationToken, Task>> handlers = new List<Func<CancellationToken, Task>>();
            handlers.Add(token =>
            {
                var tcs = new TaskCompletionSource<bool>();
                Task.Run(() =>
                {
                    are.WaitOne();
                    tcs.SetCanceled();
                });
                return tcs.Task;
            });
            var mockUploader = new MockLocalFileUploader(handlers);
            var mockContext = new MockApplicationContext(mockUploader);
            var sut = new Application(mockContext);
            var task = Task.Run(() => sut.Run());

            var uploadTask = sut.UploadAllFilesAsync();
            Task.WaitAny(new[] { uploadTask }, 100);
            sut.Quit();

            Task.WaitAny(new[] { uploadTask }, 100);
            Assert.IsFalse(uploadTask.IsCompleted);

            are.Set();
            Task.WaitAny(new[] { uploadTask }, 100);
            Assert.IsTrue(uploadTask.IsCanceled);
            Assert.IsTrue(task.IsCompleted);
        }

    }

}
