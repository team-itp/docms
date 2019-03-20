using Docms.Client.Data;
using Docms.Client.Starter;
using Docms.Client.Tests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class ApplicationStarterTests
    {
        const string SERVER_URL = "http://localhost:51693";
        const string CLIENT_ID = "53140a5b-4b21-48d8-a38f-a0c880e23b93";
        const string USER_NAME = "testuser";
        const string PASSWORD = "Passw0rd";
        private string watchPath;
        private MockApplicationEngine engine;
        private ApplicationStarter sut;

        [TestInitialize]
        public void Setup()
        {
            engine = new MockApplicationEngine();
            watchPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            if (!Directory.Exists(watchPath))
            {
                Directory.CreateDirectory(watchPath);
            }
        }

        [TestCleanup]
        public void Teardown()
        {
            if (Directory.Exists(watchPath))
            {
                Directory.Delete(watchPath, true);
            }
        }

        [TestMethod]
        public void 初期化処理でログインに失敗した場合Applicationを終了する()
        {
            sut = new ApplicationStarter(watchPath, SERVER_URL, CLIENT_ID, USER_NAME, "invalid_password");
            sut.Start(engine);
            engine.WatiUntilStateChanges();
            Assert.IsTrue(engine.IsFailed);
        }

        [TestMethod]
        public void 初期化処理でフォルダが存在しない場合Applicationを終了する()
        {
            Directory.Delete(watchPath, true);
            sut = new ApplicationStarter(watchPath, SERVER_URL, CLIENT_ID, USER_NAME, PASSWORD);
            sut.Start(engine);
            engine.WatiUntilStateChanges();
            Assert.IsTrue(engine.IsFailed);
        }

        [TestMethod]
        public void 初期化処理でDbContextの初期化に失敗した場合エラーとなる()
        {
            Directory.CreateDirectory(Path.Combine(watchPath, ".docms"));
            using (var stream = File.Open(Path.Combine(watchPath, ".docms", "data.db"), FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                sut = new ApplicationStarter(watchPath, SERVER_URL, CLIENT_ID, USER_NAME, PASSWORD);
                sut.Start(engine);
                engine.WatiUntilStateChanges();
            }
            Assert.IsTrue(engine.IsFailed);
        }

        [TestMethod]
        public void 初期化処理が完了しサーバーのファイル履歴がすべて読み込まれていること()
        {
            sut = new ApplicationStarter(watchPath, SERVER_URL, CLIENT_ID, USER_NAME, PASSWORD);
            sut.Start(engine);
            engine.WatiUntilStateChanges();
            Assert.IsTrue(engine.IsStarted);
            Assert.IsTrue(engine.Context.Db.Histories.Any());
        }
    }
}
