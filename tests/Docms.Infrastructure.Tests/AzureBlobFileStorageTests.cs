using Docms.Infrastructure.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Tests
{
    [TestClass]
    [Ignore]
    public class AzureBlobFileStorageTests
    {
        private CloudStorageAccount account;
        private CloudBlobClient client;
        private string baseContainerName;
        private AzureBlobFileStorage sut;
        private static bool failed;

        [TestInitialize]
        public async Task Setup()
        {
            if (failed) return;
            baseContainerName = "files";
            account = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            client = account.CreateCloudBlobClient();
            sut = new AzureBlobFileStorage("UseDevelopmentStorage=true", "files");
            var container = client.GetContainerReference(baseContainerName);
            try
            {
                await container.CreateIfNotExistsAsync();
            }
            catch
            {
                failed = true;
            }
        }

        [TestCleanup]
        public async Task Teardown()
        {
            if (failed) return;
            var container = client.GetContainerReference(baseContainerName);
            await container.DeleteIfExistsAsync();
        }

        private async Task CreateFileAsync(string path, string content)
        {
            var container = client.GetContainerReference(baseContainerName);
            await container.CreateIfNotExistsAsync();
            var blockBlob = container.GetBlockBlobReference(path);
            await blockBlob.UploadFromStreamAsync(new MemoryStream(Encoding.UTF8.GetBytes(content)));
            blockBlob.Properties.ContentType = "text/plain";
            await blockBlob.SetPropertiesAsync();
        }

        private async Task<bool> ExistsAsync(string path)
        {
            var container = client.GetContainerReference(baseContainerName);
            await container.CreateIfNotExistsAsync().ConfigureAwait(false);
            var blockBlob = container.GetBlockBlobReference(path);
            if (await blockBlob.ExistsAsync().ConfigureAwait(false))
            {
                return true;
            }

            var blobDir = container.GetDirectoryReference(path);
            var result = await blobDir.ListBlobsSegmentedAsync(null).ConfigureAwait(false);
            return result.Results.Any();
        }

        private async Task<string> ReadAllTextAsync(string path)
        {
            var container = client.GetContainerReference(baseContainerName);
            await container.CreateIfNotExistsAsync();
            var blockBlob = container.GetBlockBlobReference(path);
            return await blockBlob.DownloadTextAsync();
        }

        private async Task DeleteFileAsync(string path)
        {
            var container = client.GetContainerReference(baseContainerName);
            await container.CreateIfNotExistsAsync();
            var blockBlob = container.GetBlockBlobReference(path);
            await blockBlob.DeleteIfExistsAsync();
        }

        [TestMethod]
        public async Task 存在しないパスに対してGetEntryを実行しnullが戻る()
        {
            if (failed) Assert.Fail();
            Assert.IsNull(await sut.GetEntryAsync("test"));
        }

        [TestMethod]
        public async Task ファイルのパスに対してGetEntryを呼び出してファイルが取得される()
        {
            if (failed) Assert.Fail();
            await CreateFileAsync("test", "dir2content1");
            var entry = await sut.GetEntryAsync("test");
            Assert.IsTrue(entry is Files.File);
        }

        [TestMethod]
        public async Task 存在するディレクトリから存在する件数のファイルが返ること()
        {
            if (failed) Assert.Fail();
            await CreateFileAsync("dir2/content1.txt", "dir2content1");
            await CreateFileAsync("dir2/subdir2/content1.txt", "dir2content1");
            await CreateFileAsync("dir2/subdir2/content2.txt", "dir2content2");
            var root = await sut.GetEntryAsync("") as Files.Directory;
            var dir1 = await sut.GetEntryAsync("dir1") as Files.Directory;
            var dir2 = await sut.GetEntryAsync("dir2") as Files.Directory;
            var subdir2 = await sut.GetEntryAsync("dir2/subdir2") as Files.Directory;
            Assert.AreEqual(1, (await root.GetEntriesAsync()).Count());
            Assert.IsNull(dir1);
            Assert.AreEqual(2, (await dir2.GetEntriesAsync()).Count());
            Assert.AreEqual(2, (await subdir2.GetEntriesAsync()).Count());
        }

        [TestMethod]
        public async Task ファイルのストリームがオープン出来ること()
        {
            if (failed) Assert.Fail();
            await CreateFileAsync("dir2/content1.txt", "dir2content1");
            var file = await sut.GetEntryAsync("dir2/content1.txt") as Files.File;
            using (var fs = await file.OpenAsync())
            using (var reader = new StreamReader(fs, Encoding.UTF8))
            {
                var text = await reader.ReadLineAsync();
                Assert.AreEqual("dir2content1", text);
            }
        }

        [TestMethod]
        public async Task ストリームからファイルに保存できること()
        {
            if (failed) Assert.Fail();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("dir2content1"));
            ms.Seek(0, SeekOrigin.Begin);
            var dir = await sut.GetDirectoryAsync("dir2") as Files.Directory;
            var file = await dir.SaveAsync("content1.txt", "text/plain", ms);
            Assert.AreEqual("dir2content1", await ReadAllTextAsync("dir2/content1.txt"));
            Assert.AreEqual("dir2/content1.txt", file.Path.ToString());
        }

        [TestMethod]
        public async Task すでに存在するパスに対してストリームからファイルに保存できること()
        {
            if (failed) Assert.Fail();
            var ms1 = new MemoryStream(Encoding.UTF8.GetBytes("dir2content1"));
            var dir = await sut.GetDirectoryAsync("dir2") as Files.Directory;
            await dir.SaveAsync("content1.txt", "text/plain", ms1);
            Assert.AreEqual("dir2content1", await ReadAllTextAsync("dir2/content1.txt"));
            var ms2 = new MemoryStream(Encoding.UTF8.GetBytes("dir2content1new"));
            var file = await dir.SaveAsync("content1.txt", "text/plain", ms2);
            Assert.AreEqual("dir2/content1.txt", file.Path.ToString());
        }

        [TestMethod]
        public async Task ファイルが移動できること()
        {
            if (failed) Assert.Fail();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("dir2content1"));
            ms.Seek(0, SeekOrigin.Begin);
            var dir = await sut.GetDirectoryAsync("dir2");
            var file = await dir.SaveAsync("content1.txt", "text/plain", ms);
            Assert.IsTrue(await ExistsAsync("dir2/content1.txt"));
            await sut.MoveAsync(file.Path, new FilePath("content1.txt"));
            Assert.IsFalse(await ExistsAsync("dir2/content1.txt"));
            Assert.IsTrue(await ExistsAsync("content1.txt"));
        }

        [TestMethod]
        public async Task 移動左記にディレクトリがない場合でもファイルの移動ができること()
        {
            if (failed) Assert.Fail();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("dir2content1"));
            ms.Seek(0, SeekOrigin.Begin);
            var dir = await sut.GetDirectoryAsync("dir2");
            var file = await dir.SaveAsync("content1.txt", "text/plain", ms);
            Assert.IsTrue(await ExistsAsync("dir2/content1.txt"));
            await sut.MoveAsync(file.Path, new FilePath("dir1/subdir1/content1.txt"));
            Assert.IsFalse(await ExistsAsync("dir2/content1.txt"));
            Assert.IsTrue(await ExistsAsync("dir1/subdir1/content1.txt"));
        }

        [TestMethod]
        public async Task ファイルを削除できること()
        {
            if (failed) Assert.Fail();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes("dir2content1"));
            ms.Seek(0, SeekOrigin.Begin);
            var dir = await sut.GetDirectoryAsync("dir2");
            var file = await dir.SaveAsync("content1.txt", "text/plain", ms);
            Assert.IsTrue(await ExistsAsync("dir2/content1.txt"));
            await sut.DeleteAsync(file);
            Assert.IsFalse(await ExistsAsync("dir2/content1.txt"));
        }

        [TestMethod]
        public async Task 空ではないディレクトリを削除できること()
        {
            if (failed) Assert.Fail();
            await CreateFileAsync("dir2/subdir2/content1.txt", "dir2subdir2content1");
            Assert.IsTrue(await ExistsAsync("dir2/subdir2"));
            var subdir2 = await sut.GetDirectoryAsync("dir2/subdir2");
            await sut.DeleteAsync(subdir2);
            Assert.IsFalse(await ExistsAsync("dir2/subdir2"));
        }
    }
}
