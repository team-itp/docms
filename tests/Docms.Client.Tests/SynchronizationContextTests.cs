using Docms.Client.Synchronization;
using Docms.Client.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Docms.Client.Tests
{
    [TestClass]
    public class SynchronizationContextTests
    {
        private SynchronizationContext sut;

        [TestInitialize]
        public void Setup()
        {
            sut = new SynchronizationContext();
        }

        [TestMethod]
        public void 同期コンテキストにローカルファイルを追加するとアップロード要求待ちになる()
        {
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 10);
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is RequestForUploadState));
        }

        [TestMethod]
        public void 同期コンテキストのアップロード待ち状態にアップロードを実行するとアップロード完了待ちになる()
        {
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.LocalFileUploaded(new PathString("test.txt"));
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is UploadingState));
        }

        [TestMethod]
        public void 同期コンテキストのアップロード完了待ち状態でダウンロードしたファイルと同じファイルがリモートで見つかると状態がクリアされる()
        {
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.LocalFileUploaded(new PathString("test.txt"));
            sut.RemoteFileAdded(new PathString("test.txt"), "HASH", 10);
            Assert.AreEqual(0, sut.States.Count());
        }

        [TestMethod]
        public void 同期コンテキストにリモートのファイルが追加されるとダウンロード要求状態になる()
        {
            sut.RemoteFileAdded(new PathString("test.txt"), "HASH", 10);
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is RequestForDownloadState));
        }

        [TestMethod]
        public void 同期コンテキストがダウンロード要求状態でダウンロードが実行されるとダウンロード完了待ち状態になる()
        {
            sut.RemoteFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.RemoteFileDownloaded(new PathString("test.txt"));
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is DownloadingState));
        }

        [TestMethod]
        public void 同期コンテキストのダウンロード完了待ち状態でダウンロードしたファイルと同じファイルがローカルで見つかると状態がクリアされる()
        {
            sut.RemoteFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.RemoteFileDownloaded(new PathString("test.txt"));
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 10);
            Assert.AreEqual(0, sut.States.Count());
        }

        [TestMethod]
        public void 同期コンテキストのアップロード要求待ちでローカルファイルが削除された場合状態がクリアされる()
        {
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.LocalFileRemoved(new PathString("test.txt"));
            Assert.AreEqual(0, sut.States.Count());
        }

        [TestMethod]
        public void 同期コンテキストのアップロード要求待ちでローカルにハッシュが違うファイルが追加された場合アップロード要求待ちが変更される()
        {
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.LocalFileAdded(new PathString("test.txt"), "HASH2", 10);
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is RequestForUploadState));
            Assert.AreEqual("HASH2", sut.States.First().Hash);
        }

        [TestMethod]
        public void 同期コンテキストのアップロード要求待ちでローカルにサイズが違うファイルが追加された場合アップロード要求待ちが変更される()
        {
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 11);
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is RequestForUploadState));
            Assert.AreEqual(11, sut.States.First().Length);
        }

        [TestMethod]
        public void 同期コンテキストのアップロード要求待ちでリモートにファイルが追加された場合アップロード要求はそのままとなる()
        {
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.RemoteFileAdded(new PathString("test.txt"), "HASH2", 11);
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is RequestForUploadState));
            Assert.AreEqual("HASH", sut.States.First().Hash);
            Assert.AreEqual(10, sut.States.First().Length);
        }
    }
}
