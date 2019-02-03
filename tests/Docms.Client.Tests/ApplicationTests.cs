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
            var mockContext = new MockApplicationContext();
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
    }

}
