using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Uploader.FileWatch
{
    [TestClass]
    public class WatchingFileListViewModelTests
    {
        private WatchingFileListViewModel sut;

        private string _pathToWatch = Path.GetFullPath("temp");

        [TestInitialize]
        public void Setup()
        {
            if (Directory.Exists(_pathToWatch))
            {
                Directory.Delete(_pathToWatch, true);
            }
            Directory.CreateDirectory(_pathToWatch);
            sut = new WatchingFileListViewModel(_pathToWatch);
        }

        [TestCleanup]
        public void Teardown()
        {
            if (sut.IsWatching)
            {
                sut.Stopwatch();
            }

            if (Directory.Exists(_pathToWatch))
            {
                Directory.Delete(_pathToWatch, true);
            }
        }

        [TestMethod]
        public void 監視が始まっていない場合はファイルが追加されても何も起きない()
        {
            File.WriteAllText(Path.Combine(_pathToWatch, "testfile1"), "testdata");
            Assert.AreEqual(0, sut.Files.Count);
        }

        [TestMethod]
        public void 監視が始まったときすでにファイルが存在する場合はすべて追加される()
        {
            File.WriteAllText(Path.Combine(_pathToWatch, "testfile1"), "testdata1");
            File.WriteAllText(Path.Combine(_pathToWatch, "testfile2"), "testdata2");

            sut.Startwatch();
            Assert.AreEqual(2, sut.Files.Count);
        }

        [TestMethod]
        public async Task 監視後ファイルが追加されるとFileListにファイルが追加される()
        {
            var task = sut.Startwatch();
            File.WriteAllText(Path.Combine(_pathToWatch, "testfile1"), "testdata1");

            await Task.Delay(300); // ファイルの変更が反映されるまで 200ms ～ 250ms かかるため待つ
            Assert.AreEqual(1, sut.Files.Count);
        }

        [TestMethod]
        public async Task 監視後ファイルが削除されるとFileListからファイルが削除される()
        {
            File.WriteAllText(Path.Combine(_pathToWatch, "testfile1"), "testdata1");
            var task = sut.Startwatch();

            File.Delete(Path.Combine(_pathToWatch, "testfile1"));
            await Task.Delay(300); // ファイルの変更が反映されるまで 200ms ～ 250ms かかるため待つ
            Assert.AreEqual(0, sut.Files.Count);
        }

        [TestMethod]
        public async Task 監視後ファイルが変更されてもファイルは存在するまま()
        {
            File.WriteAllText(Path.Combine(_pathToWatch, "testfile1"), "testdata1");
            var task = sut.Startwatch();
            var file = sut.Files.First();
            File.WriteAllText(Path.Combine(_pathToWatch, "testfile1"), "testdata1");
            await Task.Delay(300); // ファイルの変更が反映されるまで 200ms ～ 250ms かかるため待つ
            Assert.AreEqual(1, sut.Files.Count);
            Assert.AreEqual(file, sut.Files.First());
        }
    }
}
