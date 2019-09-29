using Docms.Client.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class DocmsApiClientTests
    {
        private static bool noConnection;

        private DocmsApiClient sut;

        [TestInitialize]
        public async Task Setup()
        {
            if (noConnection) return;

            try
            {
                sut = new DocmsApiClient("http://localhost:51693");
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
            var entries = await sut.GetEntriesAsync("").ConfigureAwait(false);
            Assert.IsNotNull(entries);
        }

        [TestMethod]
        public async Task サーバーよりサブディレクトリ内のファイルの一覧を取得する()
        {
            if (noConnection) Assert.Fail("接続不良のため失敗");
            await sut.CreateOrUpdateDocumentAsync(
                "test1/test1.txt", 
                () => new MemoryStream(Encoding.UTF8.GetBytes("test1"))).ConfigureAwait(false);
            await sut.DeleteDocumentAsync("test1/test1.txt").ConfigureAwait(false);
            await sut.CreateOrUpdateDocumentAsync("test1/test1.txt",
                () => new MemoryStream(Encoding.UTF8.GetBytes("test1")),
                new DateTime(2018, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                new DateTime(2018, 10, 2, 0, 0, 0, DateTimeKind.Utc)).ConfigureAwait(false);
            var entries = await sut.GetEntriesAsync("test1").ConfigureAwait(false);
            Assert.IsTrue(entries.Any(e => e.Path == "test1/test1.txt"));
            using(var sr = await sut.DownloadAsync("test1/test1.txt"))
            {
                var ms = new MemoryStream();
                await sr.CopyToAsync(ms);
                Assert.AreEqual("test1", Encoding.UTF8.GetString(ms.ToArray()));
            }
            var histories = await sut.GetHistoriesAsync("test1/test1.txt").ConfigureAwait(false);
            var history = histories.LastOrDefault(e => e.Path == "test1/test1.txt") as DocumentCreatedHistory;
            Assert.AreEqual(new DateTime(2018, 10, 1, 0, 0, 0, DateTimeKind.Utc), history.Created);
            Assert.AreEqual(new DateTime(2018, 10, 2, 0, 0, 0, DateTimeKind.Utc), history.LastModified);
        }

        [TestMethod]
        public async Task サーバーにファイルをアップロードする()
        {
            if (noConnection) Assert.Fail("接続不良のため失敗");
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test.txt", () => new MemoryStream(Encoding.UTF8.GetBytes("test1"))).ConfigureAwait(false);
            var entries = await sut.GetEntriesAsync("test1/subtest1").ConfigureAwait(false);
            Assert.IsTrue(entries.Any(e => e.Path == "test1/subtest1/test.txt"));
        }

        [TestMethod]
        public async Task ファイルのアップロードでトークンがエラーとなる場合に自動的に再ログインしてリクエストを送信する()
        {
            if (noConnection) Assert.Fail("接続不良のため失敗");
            var restSharp = sut.GetType().GetField("_client", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sut) as RestSharp.IRestClient;
            sut.GetType().GetField("_accessToken", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(sut, "invalid_access_token");
            restSharp.Authenticator = new RestSharp.Authenticators.JwtAuthenticator("invalid_access_token");
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test.txt", () => new MemoryStream(Encoding.UTF8.GetBytes("test1"))).ConfigureAwait(false);
            var entries = await sut.GetEntriesAsync("test1/subtest1").ConfigureAwait(false);
            Assert.IsTrue(entries.Any(e => e.Path == "test1/subtest1/test.txt"));
        }

        [TestMethod]
        public async Task ファイルの読み込みでエラーになった場合に読み込みエラーの例外が発生する()
        {
            if (noConnection) Assert.Fail("接続不良のため失敗");
            var filename = Path.GetTempFileName();
            File.Delete(filename);
            await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () =>
            {
                await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test.txt", () => File.OpenRead(filename)).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task 大きいファイルをサーバーにアップロードする()
        {
            if (noConnection) Assert.Fail("接続不良のため失敗");
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test.txt", () => new MemoryStream(
                Enumerable.Range(0, 300_000_000).Select(v => (byte)v).ToArray())
            ).ConfigureAwait(false);
            var entries = await sut.GetEntriesAsync("test1/subtest1").ConfigureAwait(false);
            Assert.IsTrue(entries.Any(e => e.Path == "test1/subtest1/test.txt"));
        }

        [TestMethod]
        public async Task サーバーのファイルを移動する()
        {
            if (noConnection) Assert.Fail("接続不良のため失敗");
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test1.txt", () => new MemoryStream(Encoding.UTF8.GetBytes("test1"))).ConfigureAwait(false);
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test2.txt", () => new MemoryStream(Encoding.UTF8.GetBytes("test2"))).ConfigureAwait(false);
            await sut.DeleteDocumentAsync("test1/subtest1/test2.txt").ConfigureAwait(false);
            await sut.MoveDocumentAsync("test1/subtest1/test1.txt", "test1/subtest1/test2.txt").ConfigureAwait(false);
            Assert.IsNull(await sut.GetDocumentAsync("test1/subtest1/test1.txt").ConfigureAwait(false));
            Assert.IsNotNull(await sut.GetDocumentAsync("test1/subtest1/test2.txt").ConfigureAwait(false));
        }

        [TestMethod]
        public async Task 存在しないファイルを取得する()
        {
            if (noConnection) Assert.Fail("接続不良のため失敗");
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test1.txt", () => new MemoryStream(Encoding.UTF8.GetBytes("test1"))).ConfigureAwait(false);
            await sut.DeleteDocumentAsync("test1/subtest1/test1.txt").ConfigureAwait(false);
            Assert.IsNull(await sut.GetDocumentAsync("test1/subtest1/test1.txt").ConfigureAwait(false));
            await Assert.ThrowsExceptionAsync<ServerException>(async () => await sut.DownloadAsync("test1/subtest1/test1.txt").ConfigureAwait(false));
        }

        [TestMethod]
        public async Task 存在しないファイルを削除してもエラーにはならない()
        {
            if (noConnection) Assert.Fail("接続不良のため失敗");
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test1.txt", () => new MemoryStream(Encoding.UTF8.GetBytes("test1"))).ConfigureAwait(false);
            await sut.DeleteDocumentAsync("test1/subtest1/test1.txt").ConfigureAwait(false);
            await sut.DeleteDocumentAsync("test1/subtest1/test1.txt").ConfigureAwait(false);
        }

        [TestMethod]
        public async Task 履歴が取得できること()
        {
            if (noConnection) Assert.Fail("接続不良のため失敗");
            var histories = await sut.GetHistoriesAsync("").ConfigureAwait(false);
            var first = histories.First();
            var second = histories.Skip(1).First();
            var third = histories.Skip(2).First();
            var historiesExcludeFirst = await sut.GetHistoriesAsync("", first.Id).ConfigureAwait(false);
            Assert.AreEqual(second.Id, historiesExcludeFirst.First().Id);
            var historiesExcludeFirstAndSecond = await sut.GetHistoriesAsync("", second.Id).ConfigureAwait(false);
            Assert.AreEqual(third.Id, historiesExcludeFirstAndSecond.First().Id);
        }
    }
}
