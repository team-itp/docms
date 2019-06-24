using Docms.Client.Data;
using Docms.Client.DocumentStores;
using Docms.Client.Tests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class DocumentRepositoryTests
    {
        private DocumentDbContext db;
        private DocumentRepository<LocalDocument> sut;

        [TestInitialize]
        public void Setup()
        {
            db = new MockDocumentDbContext();
            db.Database.EnsureCreated();
            db.LocalDocuments.AddRange(new[] {
                new LocalDocument()
                {
                    Path = "test0.txt",
                    Created = new DateTime(2019, 10, 1),
                    LastModified = new DateTime(2019, 10, 1),
                    FileSize = 10,
                    Hash = "HASH1"
                }
            });
            db.Dispose();

            db = new MockDocumentDbContext();

            sut = new DocumentRepository<LocalDocument>(db, db.LocalDocuments);
        }

        [TestCleanup]
        public void Teardown()
        {
            if (File.Exists("data.db"))
            {
                File.Delete("data.db");
            }
        }

        [TestMethod]
        public async Task データが存在しない場合にデータが登録される()
        {
            await sut.MergeAsync(new[] {
                new LocalDocument()
                {
                    Path = "test0.txt",
                    Created = new DateTime(2019, 10, 1),
                    LastModified = new DateTime(2019, 10, 1),
                    FileSize = 10,
                    Hash = "HASH1"
                },
                new LocalDocument()
                {
                    Path = "test1.txt",
                    Created = new DateTime(2019, 10, 1),
                    LastModified = new DateTime(2019, 10, 1),
                    FileSize = 10,
                    Hash = "HASH1"
                }
            });

            Assert.IsTrue(db.LocalDocuments.Any(d => d.Path == "test0.txt"));
            Assert.IsTrue(db.LocalDocuments.Any(d => d.Path == "test1.txt"));
        }

        [TestMethod]
        public async Task データが存在する場合にデータが更新される()
        {
            await sut.MergeAsync(new[] {
                new LocalDocument()
                {
                    Path = "test0.txt",
                    Created = new DateTime(2019, 10, 1),
                    LastModified = new DateTime(2019, 10, 1),
                    FileSize = 10,
                    Hash = "HASH1"
                },
                new LocalDocument()
                {
                    Path = "test1.txt",
                    Created = new DateTime(2019, 10, 1),
                    LastModified = new DateTime(2019, 10, 2),
                    FileSize = 10,
                    Hash = "HASH1"
                }
            });

            await sut.MergeAsync(new[] {
                new LocalDocument()
                {
                    Path = "test0.txt",
                    Created = new DateTime(2019, 10, 1),
                    LastModified = new DateTime(2019, 10, 2),
                    FileSize = 10,
                    Hash = "HASH1"
                },
                new LocalDocument()
                {
                    Path = "test1.txt",
                    Created = new DateTime(2019, 10, 1),
                    LastModified = new DateTime(2019, 10, 2),
                    FileSize = 10,
                    Hash = "HASH1"
                }
            });

            Assert.IsTrue(db.LocalDocuments.Any(d => d.Path == "test0.txt" && d.LastModified == new DateTime(2019, 10, 2)));
            Assert.IsTrue(db.LocalDocuments.Any(d => d.Path == "test1.txt" && d.LastModified == new DateTime(2019, 10, 2)));
        }

        [TestMethod]
        public async Task データが存在しない場合にデータが削除される()
        {
            await sut.MergeAsync(new[] {
                new LocalDocument()
                {
                    Path = "test1.txt",
                    Created = new DateTime(2019, 10, 1),
                    LastModified = new DateTime(2019, 10, 1),
                    FileSize = 10,
                    Hash = "HASH1"
                }
            });

            await sut.MergeAsync(new[] {
                new LocalDocument()
                {
                    Path = "test2.txt",
                    Created = new DateTime(2019, 10, 1),
                    LastModified = new DateTime(2019, 10, 1),
                    FileSize = 10,
                    Hash = "HASH1"
                },
            });

            Assert.IsFalse(db.LocalDocuments.Any(d => d.Path == "test0.txt"));
            Assert.IsFalse(db.LocalDocuments.Any(d => d.Path == "test1.txt"));
            Assert.IsTrue(db.LocalDocuments.Any(d => d.Path == "test2.txt"));
        }

        [TestMethod]
        public async Task ファイルを単体で追加更新できる()
        {
            await sut.UpdateAsync(
                new LocalDocument()
                {
                    Path = "test1.txt",
                    Created = new DateTime(2019, 10, 1),
                    LastModified = new DateTime(2019, 10, 2),
                    FileSize = 10,
                    Hash = "HASH1"
                });

            await sut.UpdateAsync(
                new LocalDocument()
                {
                    Path = "test0.txt",
                    Created = new DateTime(2019, 10, 1),
                    LastModified = new DateTime(2019, 10, 2),
                    FileSize = 10,
                    Hash = "HASH1"
                });

            Assert.IsTrue(db.LocalDocuments.Any(d => d.Path == "test0.txt" && d.LastModified == new DateTime(2019, 10, 2)));
            Assert.IsTrue(db.LocalDocuments.Any(d => d.Path == "test1.txt" && d.LastModified == new DateTime(2019, 10, 2)));
        }
    }
}
