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

        [TestMethod]
        public void 同期コンテキストのアップロード要求待ち状態でアップロードを実行するとアップロード完了待ちになる()
        {
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.UploadRequested(new PathString("test.txt"));
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is UploadingState));
        }

        [TestMethod]
        public void 同期コンテキストのアップロード完了待ち状態でダウンロードしたファイルと同じファイルがリモートで見つかると状態がクリアされる()
        {
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.UploadRequested(new PathString("test.txt"));
            sut.RemoteFileAdded(new PathString("test.txt"), "HASH", 10);
            Assert.AreEqual(0, sut.States.Count());
        }

        [TestMethod]
        public void 同期コンテキストのアップロード要求待ちでローカルファイルが削除された場合状態がクリアされる()
        {
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.LocalFileDeleted(new PathString("test.txt"), "HASH", 10);
            Assert.AreEqual(0, sut.States.Count());
        }

        [TestMethod]
        public void 同期コンテキストのアップロード完了待ちでローカルファイルが削除された場合削除要求状態になる()
        {
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.UploadRequested(new PathString("test.txt"));
            sut.LocalFileDeleted(new PathString("test.txt"), "HASH", 10);
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is RequestForDeleteState));
        }

        [TestMethod]
        public void 同期コンテキストのアップロード完了待ちでローカルのファイルが上書きされた場合再度アップロード要求となる()
        {
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.UploadRequested(new PathString("test.txt"));
            sut.LocalFileAdded(new PathString("test.txt"), "HASH2", 11);
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is RequestForUploadState));
            Assert.AreEqual("HASH2", sut.States.First().Hash);
            Assert.AreEqual(11, sut.States.First().Length);
        }

        [TestMethod]
        public void 同期コンテキストでアップロード完了待ちの場合に待ち状態と異なるリモートファイルが追加された場合ダウンロード要求となる()
        {
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.UploadRequested(new PathString("test.txt"));
            sut.RemoteFileAdded(new PathString("test.txt"), "HASH2", 11);
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is RequestForDownloadState));
            Assert.AreEqual("HASH2", sut.States.First().Hash);
            Assert.AreEqual(11, sut.States.First().Length);
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
            sut.DownloadRequested(new PathString("test.txt"));
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is DownloadingState));
        }

        [TestMethod]
        public void 同期コンテキストのダウンロード完了待ち状態でダウンロードしたファイルと同じファイルがローカルで見つかると状態がクリアされる()
        {
            sut.RemoteFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.DownloadRequested(new PathString("test.txt"));
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 10);
            Assert.AreEqual(0, sut.States.Count());
        }

        [TestMethod]
        public void 同期コンテキストにダウンロード要求がある場合にローカルファイルが変更されるとアップロード要求となる()
        {
            sut.RemoteFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.LocalFileAdded(new PathString("test.txt"), "HASH2", 11);
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is RequestForUploadState));
            Assert.AreEqual("HASH2", sut.States.First().Hash);
            Assert.AreEqual(11, sut.States.First().Length);
        }

        [TestMethod]
        public void 同期コンテキストにダウンロード要求がある場合にリモートファイルが更新されるとダウンロード要求が更新される()
        {
            sut.RemoteFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.RemoteFileAdded(new PathString("test.txt"), "HASH2", 11);
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is RequestForDownloadState));
            Assert.AreEqual("HASH2", sut.States.First().Hash);
            Assert.AreEqual(11, sut.States.First().Length);
        }

        [TestMethod]
        public void 同期コンテキストにダウンロード要求がある場合にリモートファイルが削除されると状態がクリアされる()
        {
            sut.RemoteFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.RemoteFileDeleted(new PathString("test.txt"), "HASH", 10);
            Assert.AreEqual(0, sut.States.Count());
        }

        [TestMethod]
        public void 同期コンテキストにダウンロード完了待ちでローカルファイルが変更されるとアップロード要求となる()
        {
            sut.RemoteFileAdded(new PathString("test.txt"), "HASH", 10);
            sut.DownloadRequested(new PathString("test.txt"));
            sut.LocalFileAdded(new PathString("test.txt"), "HASH2", 11);
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is RequestForUploadState));
            Assert.AreEqual("HASH2", sut.States.First().Hash);
            Assert.AreEqual(11, sut.States.First().Length);
        }

        [TestMethod]
        public void 同期コンテキストでファイルを削除された場合に削除要求が追加される()
        {
            sut.LocalFileDeleted(new PathString("test.txt"), "HASH", 10);
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is RequestForDeleteState));
        }

        [TestMethod]
        public void 同期コンテキストで削除要求が実行された場合削除完了待ちとなる()
        {
            sut.LocalFileDeleted(new PathString("test.txt"), "HASH", 10);
            sut.DeleteRequested(new PathString("test.txt"));
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is DeletingState));
        }

        [TestMethod]
        public void 同期コンテキストで削除待ちの場合にリモートファイルが削除されると状態がクリアされる()
        {
            sut.LocalFileDeleted(new PathString("test.txt"), "HASH", 10);
            sut.DeleteRequested(new PathString("test.txt"));
            sut.RemoteFileDeleted(new PathString("test.txt"), "HASH", 10);
            Assert.AreEqual(0, sut.States.Count());
        }

        [TestMethod]
        public void 同期コンテキストで削除要求状態で同一のファイルがローカルに追加された場合状態がクリアされる()
        {
            sut.LocalFileDeleted(new PathString("test.txt"), "HASH", 10);
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 10);
            Assert.AreEqual(0, sut.States.Count());
        }

        [TestMethod]
        public void 同期コンテキストで削除要求状態で異なるファイルがローカルに追加された場合状態がクリアされる()
        {
            sut.LocalFileDeleted(new PathString("test.txt"), "HASH", 10);
            sut.LocalFileAdded(new PathString("test.txt"), "HASH2", 11);
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is RequestForUploadState));
            Assert.AreEqual("HASH2", sut.States.First().Hash);
            Assert.AreEqual(11, sut.States.First().Length);
        }


        [TestMethod]
        public void 同期コンテキストで削除要求完了状態で同一のファイルがローカルに追加された場合アップロード要求待ちになる()
        {
            sut.LocalFileDeleted(new PathString("test.txt"), "HASH", 10);
            sut.DeleteRequested(new PathString("test.txt"));
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 10);
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is RequestForUploadState));
        }

        [TestMethod]
        public void 同期コンテキストでリモートファイルが削除された場合リモートファイルの削除状態となる()
        {
            sut.RemoteFileDeleted(new PathString("test.txt"), "HASH", 10);
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is RemoteFileDeletedState));
        }

        [TestMethod]
        public void 同期コンテキストでリモートファイルの削除状態でローカルに同じファイルが作成された場合状態がクリアされる()
        {
            sut.RemoteFileDeleted(new PathString("test.txt"), "HASH", 10);
            sut.LocalFileAdded(new PathString("test.txt"), "HASH", 10);
            Assert.AreEqual(0, sut.States.Count());
        }

        [TestMethod]
        public void 同期コンテキストでリモートファイルの削除状態でローカルに違うファイルが作成された場合アップロード要求待ち状態になる()
        {
            sut.RemoteFileDeleted(new PathString("test.txt"), "HASH", 10);
            sut.LocalFileAdded(new PathString("test.txt"), "HASH1", 11);
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is RequestForUploadState));
        }
    }
}
