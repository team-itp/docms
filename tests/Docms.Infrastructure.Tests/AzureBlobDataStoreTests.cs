using Docms.Infrastructure.Storage;
using Docms.Infrastructure.Storage.AzureBlobStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Infrastructure.Tests
{
    [TestClass]
    public class AzureBlobDataStoreTests
    {
        private string baseContainerName;
        private CloudStorageAccount account;
        private CloudBlobClient client;
        private CloudBlobContainer container;
        private AzureBlobDataStore sut;
        private static bool failed;

        [TestInitialize]
        public async Task Setup()
        {
            if (failed) return;
            baseContainerName = "files";
            account = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
            client = account.CreateCloudBlobClient();
            sut = new AzureBlobDataStore("UseDevelopmentStorage=true", "files");
            container = client.GetContainerReference(baseContainerName);
            try
            {
                await container.CreateIfNotExistsAsync();
            }
            catch (StorageException)
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

        private MemoryStream Ms(string content)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(content));
        }

        private string ExtractStream(Stream sr)
        {
            var ms = new MemoryStream();
            sr.CopyTo(ms);
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        [TestMethod]
        public async Task ファイルをアップロードできること()
        {
            var key = sut.CreateKey();
            var ms = Ms("Hello, world");
            var data = await sut.CreateAsync(key, ms);
            Assert.AreEqual(ms.Length, data.Length);
            Assert.AreEqual(Hash.CalculateHash(ms.ToArray()), data.Hash);
            using (var sr = await data.OpenStreamAsync())
            {
                Assert.AreEqual("Hello, world", ExtractStream(sr));
            }
        }

        [TestMethod]
        public async Task ファイルをダウンロードできること()
        {
            var key = sut.CreateKey();
            var ms = Ms("Hello, world");
            await sut.CreateAsync(key, ms);
            var data = await sut.FindAsync(key);
            Assert.AreEqual(ms.Length, data.Length);
            Assert.AreEqual(Hash.CalculateHash(ms.ToArray()), data.Hash);
            using (var sr = await data.OpenStreamAsync())
            {
                Assert.AreEqual("Hello, world", ExtractStream(sr));
            }
        }

        [TestMethod]
        public async Task ハッシュが未計算のBlobを取得できること()
        {
            var key = sut.CreateKey();

            var blob = container.GetBlockBlobReference(key);
            var ms = Ms("Hello, world");
            await blob.UploadFromStreamAsync(ms);

            var data = await sut.FindAsync(key);
            Assert.IsNull(data.Hash);
            Assert.AreEqual(ms.Length, data.Length);
            using (var sr = await data.OpenStreamAsync())
            {
                Assert.AreEqual("Hello, world", ExtractStream(sr));
            }
        }

    }
}
