using Docms.Client.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class DocmsApiClinetTests
    {
        private static bool noConnection;

        private DocmsApiClinet sut;

        [TestInitialize]
        public async Task Setup()
        {
            if (noConnection) return;

            try
            {
                sut = new DocmsApiClinet("http://localhost:51693");
                await sut.LoginAsync("testuser", "Passw0rd").ConfigureAwait(false);
            }
            catch (Exception)
            {
                noConnection = true;
            }
        }

        [TestCleanup]
        public async Task Teardown()
        {
            if (noConnection) return;
            await sut.LogoutAsync().ConfigureAwait(false);
        }

        [TestMethod]
        public async Task サーバーよりルートディレクトリ内のファイルの一覧を取得する()
        {
            if (noConnection) Assert.Fail("接続不良のため失敗");
            await sut.VerifyTokenAsync().ConfigureAwait(false);
            var entries = await sut.GetEntriesAsync("").ConfigureAwait(false);
        }

        [TestMethod]
        public async Task サーバーよりサブディレクトリ内のファイルの一覧を取得する()
        {
            if (noConnection) Assert.Fail("接続不良のため失敗");
            await sut.CreateOrUpdateDocumentAsync("test1/test1.txt", new MemoryStream(Encoding.UTF8.GetBytes("test1"))).ConfigureAwait(false);
            await sut.DeleteDocumentAsync("test1/test1.txt").ConfigureAwait(false);
            var time = DateTime.UtcNow;
            await sut.CreateOrUpdateDocumentAsync("test1/test1.txt",
                new MemoryStream(Encoding.UTF8.GetBytes("test1")),
                new DateTime(2018, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2018, 10, 2, 0, 0, 0, DateTimeKind.Utc)).ConfigureAwait(false);
            await sut.VerifyTokenAsync().ConfigureAwait(false);
            var entries = await sut.GetEntriesAsync("test1").ConfigureAwait(false);
            Assert.IsTrue(entries.Any(e => e.Path == "test1/test1.txt"));
            var histories = await sut.GetHistoriesAsync("test1/test1.txt", time);
            var history = histories.LastOrDefault(e => e.Path == "test1/test1.txt") as DocumentCreatedHistory;
            Assert.AreEqual(new DateTime(2018, 10, 1, 0, 0, 0, DateTimeKind.Utc), history.Created);
            Assert.AreEqual(new DateTime(2018, 10, 2, 0, 0, 0, DateTimeKind.Utc), history.LastModified);
        }

        [TestMethod]
        public async Task サーバーにファイルをアップロードする()
        {
            if (noConnection) Assert.Fail("接続不良のため失敗");
            await sut.VerifyTokenAsync().ConfigureAwait(false);
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test.txt", new MemoryStream(Encoding.UTF8.GetBytes("test1"))).ConfigureAwait(false);
            var entries = await sut.GetEntriesAsync("test1/subtest1").ConfigureAwait(false);
            Assert.IsTrue(entries.Any(e => e.Path == "test1/subtest1/test.txt"));
        }

        [TestMethod]
        public async Task サーバーのファイルを移動する()
        {
            if (noConnection) Assert.Fail("接続不良のため失敗");
            await sut.VerifyTokenAsync().ConfigureAwait(false);
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test1.txt", new MemoryStream(Encoding.UTF8.GetBytes("test1"))).ConfigureAwait(false);
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test2.txt", new MemoryStream(Encoding.UTF8.GetBytes("test2"))).ConfigureAwait(false);
            await sut.DeleteDocumentAsync("test1/subtest1/test2.txt").ConfigureAwait(false);
            await sut.MoveDocumentAsync("test1/subtest1/test1.txt", "test1/subtest1/test2.txt").ConfigureAwait(false);
            Assert.IsNull(await sut.GetDocumentAsync("test1/subtest1/test1.txt").ConfigureAwait(false));
            Assert.IsNotNull(await sut.GetDocumentAsync("test1/subtest1/test2.txt").ConfigureAwait(false));
        }

        [TestMethod]
        public async Task 存在しないファイルを取得する()
        {
            if (noConnection) Assert.Fail("接続不良のため失敗");
            await sut.VerifyTokenAsync().ConfigureAwait(false);
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test1.txt", new MemoryStream(Encoding.UTF8.GetBytes("test1"))).ConfigureAwait(false);
            await sut.DeleteDocumentAsync("test1/subtest1/test1.txt").ConfigureAwait(false);
            Assert.IsNull(await sut.GetDocumentAsync("test1/subtest1/test1.txt").ConfigureAwait(false));
            await Assert.ThrowsExceptionAsync<ServerException>(async () => await sut.DownloadAsync("test1/subtest1/test1.txt").ConfigureAwait(false));
        }

        [TestMethod]
        public async Task 存在しないファイルを削除してもエラーにはならない()
        {
            if (noConnection) Assert.Fail("接続不良のため失敗");
            await sut.VerifyTokenAsync().ConfigureAwait(false);
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test1.txt", new MemoryStream(Encoding.UTF8.GetBytes("test1"))).ConfigureAwait(false);
            await sut.DeleteDocumentAsync("test1/subtest1/test1.txt").ConfigureAwait(false);
            await sut.DeleteDocumentAsync("test1/subtest1/test1.txt").ConfigureAwait(false);
        }
    }
}
