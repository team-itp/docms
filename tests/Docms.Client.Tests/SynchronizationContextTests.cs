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
            sut.AddLocalFile(new PathString("test.txt"), "HASH", 10);
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is RequestForUploadState));
        }

        [TestMethod]
        public void 同期コンテキストのアップロード待ち状態にアップロードを実行するとアップロード完了待ちになる()
        {
            sut.AddLocalFile(new PathString("test.txt"), "HASH", 10);
            sut.LocalFileUploaded(new PathString("test.txt"));
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is UploadingState));
        }

        [TestMethod]
        public void 同期コンテキストのアップロード済み状態のファイルがある状態でアップロード済みのファイルと同じファイルが見つかると状態がクリアされる()
        {
            sut.AddLocalFile(new PathString("test.txt"), "HASH", 10);
            sut.LocalFileUploaded(new PathString("test.txt"));
            sut.AddRemoteFile(new PathString("test.txt"), "HASH", 10);
            Assert.AreEqual(0, sut.States.Count());
        }

        [TestMethod]
        public void 同期コンテキストにリモートのファイルが追加されるとダウンロード要求状態になる()
        {
            sut.AddRemoteFile(new PathString("test.txt"), "HASH", 10);
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is RequestForDownloadState));
        }

        [TestMethod]
        public void 同期コンテキストがダウンロード要求状態でダウンロードが実行されるとダウンロード完了待ち状態になる()
        {
            sut.AddRemoteFile(new PathString("test.txt"), "HASH", 10);
            sut.RemoteFileDownloaded(new PathString("test.txt"));
            Assert.AreEqual(1, sut.States.Count());
            Assert.IsTrue(sut.States.Any(q => q is DownloadingState));
        }
    }
}
