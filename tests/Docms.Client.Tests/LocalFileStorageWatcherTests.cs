using Docms.Client.FileStorage;
using Docms.Client.FileTracking;
using Docms.Client.SeedWork;
using Docms.Client.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class LocalFileStorageWatcherTests
    {
        private LocalFileStorageWatcher sut;
        private string _watchingPath;

        [TestInitialize]
        public void Setup()
        {
            _watchingPath = Path.GetFullPath("tmp");
            sut = new LocalFileStorageWatcher(_watchingPath);
            sut.StartWatch();
        }

        [TestCleanup]
        public void Teardown()
        {
            sut.StopWatch();
            if (Directory.Exists(_watchingPath))
            {
                Directory.Delete(_watchingPath, true);
            }
        }

        [TestMethod]
        public void ファイルの保存を検知した場合にイベントが発生する()
        {
            var ev = default(FileCreatedEventArgs);
            sut.FileCreated += new EventHandler<FileCreatedEventArgs>((s, e) =>
            {
                ev = e;
            });
            File.WriteAllBytes("tmp\\content1.txt", Encoding.UTF8.GetBytes("content1.txt"));
            Thread.Sleep(20);
            Assert.AreEqual("content1.txt", ev.Path);
        }
    }
}
