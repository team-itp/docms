using Docms.Client.Api;
using Docms.Client.RemoteStorage;
using Docms.Client.SeedWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class CacheableRemoteFileRepositoryTests
    {
        private RemoteFileContext db;
        private CacheableRemoteFileRepository sut;

        [TestInitialize]
        public void Setup()
        {
            db = new RemoteFileContext(new DbContextOptionsBuilder<RemoteFileContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
            sut = new CacheableRemoteFileRepository(db);
        }

        [TestMethod]
        public async Task ファイルを追加して変更を確定しないうちはDBに反映されないが確定すると反映される()
        {
            var remoteFile1 = new RemoteFile(new PathString("content1.txt"));
            var remoteFile2 = new RemoteFile(new PathString("dir1/content2.txt"));
            var historyId1 = Guid.NewGuid();
            remoteFile1.Apply(new DocumentCreatedHistory()
            {
                Id = historyId1,
                Timestamp = new DateTime(2018, 1, 1, 1, 1, 1, DateTimeKind.Utc),
                Path = "content1.txt",
                ContentType = "text/html",
                FileSize = 10,
                Hash = "1010",
                Created = new DateTime(2018, 1, 1, 1, 1, 1, DateTimeKind.Utc),
                LastModified = new DateTime(2018, 1, 1, 1, 1, 1, DateTimeKind.Utc),
            });
            await sut.AddAsync(remoteFile1);
            await sut.AddAsync(remoteFile2);
            Assert.IsFalse(await db.RemoteNodes.AnyAsync());
            Assert.IsFalse(await db.RemoteFiles.AnyAsync());
            Assert.IsFalse(await db.RemoteContainers.AnyAsync());
            Assert.IsFalse(await db.RemoteFileHistories.AnyAsync());

            var rootFiles = await sut.GetFilesAsync(PathString.Root);
            var rootDirs = await sut.GetDirectoriesAsync(PathString.Root);
            var dir1Files = await sut.GetFilesAsync(new PathString("dir1"));
            var dir1Dirs = await sut.GetDirectoriesAsync(new PathString("dir1"));
            Assert.AreEqual("content1.txt", rootFiles.Single().ToString());
            Assert.AreEqual("dir1", rootDirs.Single().ToString());
            Assert.AreEqual("dir1/content2.txt", dir1Files.Single().ToString());
            Assert.IsFalse(dir1Dirs.Any());

            await sut.SaveAsync();
            Assert.IsTrue(await db.RemoteFiles.AnyAsync(f => f.Path == "content1.txt"));
            Assert.IsTrue(await db.RemoteFiles.AnyAsync(f => f.Path == "dir1/content2.txt"));
            Assert.IsTrue(await db.RemoteContainers.AnyAsync(f => f.Path == "dir1"));
            Assert.IsTrue(await db.RemoteFileHistories.AnyAsync(f => f.HistoryId == historyId1));
        }

        [TestMethod]
        public async Task DBに存在するデータを元にファイルとディレクトリの一覧を正しく取得できること()
        {
            var remoteFile1 = new RemoteFile(new PathString("content1.txt"));
            var remoteFile2 = new RemoteFile(new PathString("dir1/content2.txt"));
            var historyId1 = Guid.NewGuid();
            remoteFile1.Apply(new DocumentCreatedHistory()
            {
                Id = historyId1,
                Timestamp = new DateTime(2018, 1, 1, 1, 1, 1, DateTimeKind.Utc),
                Path = "content1.txt",
                ContentType = "text/html",
                FileSize = 10,
                Hash = "1010",
                Created = new DateTime(2018, 1, 1, 1, 1, 1, DateTimeKind.Utc),
                LastModified = new DateTime(2018, 1, 1, 1, 1, 1, DateTimeKind.Utc),
            });
            db.RemoteContainers.Add(new RemoteContainer(PathString.Root));
            db.RemoteContainers.Add(new RemoteContainer(new PathString("dir1")));
            db.RemoteFiles.Add(remoteFile1);
            db.RemoteFiles.Add(remoteFile2);
            await db.SaveChangesAsync();

            var rootFiles = await sut.GetFilesAsync(PathString.Root);
            var rootDirs = await sut.GetDirectoriesAsync(PathString.Root);
            var dir1Files = await sut.GetFilesAsync(new PathString("dir1"));
            var dir1Dirs = await sut.GetDirectoriesAsync(new PathString("dir1"));
            Assert.AreEqual("content1.txt", rootFiles.Single().ToString());
            Assert.AreEqual("dir1", rootDirs.Single().ToString());
            Assert.AreEqual("dir1/content2.txt", dir1Files.Single().ToString());
            Assert.IsFalse(dir1Dirs.Any());
        }

        [TestMethod]
        public async Task DBに登録されているデータを更新しキャッシュが正しく更新されてDBにも変更が反映されること()
        {
            var remoteFile1 = new RemoteFile(new PathString("content1.txt"));
            var remoteFile2 = new RemoteFile(new PathString("dir1/content2.txt"));
            var historyId1 = Guid.NewGuid();
            remoteFile1.Apply(new DocumentCreatedHistory()
            {
                Id = historyId1,
                Timestamp = new DateTime(2018, 1, 1, 1, 1, 1, DateTimeKind.Utc),
                Path = "content1.txt",
                ContentType = "text/html",
                FileSize = 10,
                Hash = "1010",
                Created = new DateTime(2018, 1, 1, 1, 1, 1, DateTimeKind.Utc),
                LastModified = new DateTime(2018, 1, 1, 1, 1, 1, DateTimeKind.Utc),
            });
            db.RemoteContainers.Add(new RemoteContainer(PathString.Root));
            db.RemoteContainers.Add(new RemoteContainer(new PathString("dir1")));
            db.RemoteFiles.Add(remoteFile1);
            db.RemoteFiles.Add(remoteFile2);
            await db.SaveChangesAsync();

            var rootFiles = await sut.GetFilesAsync(PathString.Root);
            var rootDirs = await sut.GetDirectoriesAsync(PathString.Root);
            var dir1Files = await sut.GetFilesAsync(new PathString("dir1"));
            var dir1Dirs = await sut.GetDirectoriesAsync(new PathString("dir1"));

            var file1 = await sut.FindAsync(new PathString("content1.txt"));
            Assert.AreEqual(1, file1.RemoteFileHistories.Count);
            var file2 = await sut.FindAsync(new PathString("dir1/content1.txt"));
            var historyId2 = Guid.NewGuid();
            file1.Apply(new DocumentUpdatedHistory()
            {
                Id = historyId2,
                Timestamp = new DateTime(2018, 1, 1, 1, 1, 1, DateTimeKind.Utc),
                Path = "content1.txt",
                ContentType = "text/html",
                FileSize = 11,
                Hash = "101010",
                Created = new DateTime(2018, 1, 1, 1, 1, 1, DateTimeKind.Utc),
                LastModified = new DateTime(2018, 1, 1, 1, 1, 1, DateTimeKind.Utc),
            });
            await sut.UpdateAsync(file1);
            await sut.SaveAsync();

            var dbFile1 = await db.RemoteFiles.FirstOrDefaultAsync(f => f.Path == "content1.txt");
            Assert.AreEqual(2, dbFile1.RemoteFileHistories.Count);
        }

    }
}
