using Docms.Client.FileWatching;
using Docms.Client.SeedWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Docms.Client.Tests
{
    [TestClass]
    public class LocalFileEventShrinkerTests
    {
        [TestMethod]
        public void ドキュメントが新規作成された後に更新された場合更新は無視される()
        {
            var sut = new LocalFileEventShrinker();
            sut.Apply(new DocumentCreated(new PathString("test/content1.txt")));
            sut.Apply(new DocumentUpdated(new PathString("test/content1.txt")));
            var ev = sut.Events.Single();
            Assert.IsTrue(ev is DocumentCreated);
        }

        [TestMethod]
        public void ドキュメントが新規作成された後に削除された場合イベントは存在しない()
        {
            var sut = new LocalFileEventShrinker();
            sut.Apply(new DocumentCreated(new PathString("test/content1.txt")));
            sut.Apply(new DocumentDeleted(new PathString("test/content1.txt")));
            Assert.IsFalse(sut.Events.Any());
        }

        [TestMethod]
        public void ドキュメントが新規作成された後に移動した場合移動先で新規作成されたこととなる()
        {
            var sut = new LocalFileEventShrinker();
            sut.Apply(new DocumentCreated(new PathString("test/content1.txt")));
            sut.Apply(new DocumentMoved(new PathString("test/content2.txt"), new PathString("test/content1.txt")));
            var ev = sut.Events.Single();
            Assert.IsTrue(ev is DocumentCreated);
            Assert.AreEqual("test/content2.txt", ev.Path.ToString());
        }

        [TestMethod]
        public void ドキュメントが更新された後に削除された場合単に削除されたことになる()
        {
            var sut = new LocalFileEventShrinker();
            sut.Apply(new DocumentUpdated(new PathString("test/content1.txt")));
            sut.Apply(new DocumentDeleted(new PathString("test/content1.txt")));
            var ev = sut.Events.Single();
            Assert.IsTrue(ev is DocumentDeleted);
        }

        [TestMethod]
        public void ドキュメントが更新された後に再度更新された場合一度更新されたことになる()
        {
            var sut = new LocalFileEventShrinker();
            sut.Apply(new DocumentUpdated(new PathString("test/content1.txt")));
            sut.Apply(new DocumentUpdated(new PathString("test/content1.txt")));
            var ev = sut.Events.Single();
            Assert.IsTrue(ev is DocumentUpdated);
        }

        [TestMethod]
        public void ドキュメントが更新された後に移動した場合元の場所で削除されて新しい場所で新規に作成されたことになる()
        {
            var sut = new LocalFileEventShrinker();
            sut.Apply(new DocumentUpdated(new PathString("test/content1.txt")));
            sut.Apply(new DocumentMoved(new PathString("test/content2.txt"), new PathString("test/content1.txt")));
            Assert.AreEqual(2, sut.Events.Count());
            var ev1 = sut.Events.First();
            Assert.IsTrue(ev1 is DocumentDeleted);
            Assert.AreEqual("test/content1.txt", ev1.Path.ToString());
            var ev2 = sut.Events.Last();
            Assert.IsTrue(ev2 is DocumentCreated);
            Assert.AreEqual("test/content2.txt", ev2.Path.ToString());
        }

        [TestMethod]
        public void ドキュメントが削除された後に再度作成された場合更新されたことになる()
        {
            var sut = new LocalFileEventShrinker();
            sut.Apply(new DocumentDeleted(new PathString("test/content1.txt")));
            sut.Apply(new DocumentCreated(new PathString("test/content1.txt")));
            var ev = sut.Events.Single();
            Assert.IsTrue(ev is DocumentUpdated);
        }

        [TestMethod]
        public void ドキュメントが削除された後に他の場所から移動してきた場合移動元は削除されたこととなり移動先は更新されたこととなる()
        {
            var sut = new LocalFileEventShrinker();
            sut.Apply(new DocumentDeleted(new PathString("test/content1.txt")));
            sut.Apply(new DocumentMoved(new PathString("test/content1.txt"), new PathString("test/content2.txt")));
            Assert.AreEqual(2, sut.Events.Count());
            var ev1 = sut.Events.First();
            Assert.IsTrue(ev1 is DocumentDeleted);
            Assert.AreEqual("test/content1.txt", ev1.Path.ToString());
            var ev2 = sut.Events.Last();
            Assert.IsTrue(ev2 is DocumentMoved);
            Assert.AreEqual("test/content1.txt", ev2.Path.ToString());
        }

        [TestMethod]
        public void ドキュメントが移動した後で移動元に生成された場合そのままの履歴になる()
        {
            var sut = new LocalFileEventShrinker();
            sut.Apply(new DocumentUpdated(new PathString("test/content2.txt")));
            sut.Apply(new DocumentMoved(new PathString("test/content1.txt"), new PathString("test/content2.txt")));
            sut.Apply(new DocumentCreated(new PathString("test/content2.txt")));
            Assert.AreEqual(2, sut.Events.Count());
            var ev1 = sut.Events.First();
            Assert.IsTrue(ev1 is DocumentCreated);
            Assert.AreEqual("test/content1.txt", ev1.Path.ToString());
            var ev2 = sut.Events.Last();
            Assert.IsTrue(ev2 is DocumentUpdated);
            Assert.AreEqual("test/content2.txt", ev2.Path.ToString());
        }

        [TestMethod]
        public void 更新されたドキュメントが移動して移動元に新規に生成された場合更新されたことになる()
        {
            var sut = new LocalFileEventShrinker();
            sut.Apply(new DocumentUpdated(new PathString("test/content2.txt")));
            sut.Apply(new DocumentMoved(new PathString("test/content1.txt"), new PathString("test/content2.txt")));
            sut.Apply(new DocumentCreated(new PathString("test/content2.txt")));
            Assert.AreEqual(2, sut.Events.Count());
            var ev1 = sut.Events.First();
            Assert.IsTrue(ev1 is DocumentCreated);
            Assert.AreEqual("test/content1.txt", ev1.Path.ToString());
            var ev2 = sut.Events.Last();
            Assert.IsTrue(ev2 is DocumentUpdated);
            Assert.AreEqual("test/content2.txt", ev2.Path.ToString());
        }

        [TestMethod]
        public void ドキュメントが移動した後で移動先のドキュメントが削除された場合移動元が削除されたことになる()
        {
            var sut = new LocalFileEventShrinker();
            sut.Apply(new DocumentMoved(new PathString("test/content1.txt"), new PathString("test/content2.txt")));
            sut.Apply(new DocumentDeleted(new PathString("test/content1.txt")));
            var ev = sut.Events.Single();
            Assert.IsTrue(ev is DocumentDeleted);
            Assert.AreEqual("test/content2.txt", ev.Path.ToString());
        }

        [TestMethod]
        public void ドキュメントが移動した後で移動先が更新された場合移動元が削除されて移動先に新規に作成されたことになる()
        {
            var sut = new LocalFileEventShrinker();
            sut.Apply(new DocumentMoved(new PathString("test/content1.txt"), new PathString("test/content2.txt")));
            sut.Apply(new DocumentUpdated(new PathString("test/content1.txt")));
            Assert.AreEqual(2, sut.Events.Count());
            var ev1 = sut.Events.First();
            Assert.IsTrue(ev1 is DocumentDeleted);
            Assert.AreEqual("test/content2.txt", ev1.Path.ToString());
            var ev2 = sut.Events.Last();
            Assert.IsTrue(ev2 is DocumentCreated);
            Assert.AreEqual("test/content1.txt", ev2.Path.ToString());
        }

        [TestMethod]
        public void ドキュメントが移動した後でさらに移動した場合1つの移動に集約される()
        {
            var sut = new LocalFileEventShrinker();
            sut.Apply(new DocumentMoved(new PathString("test/content2.txt"), new PathString("test/content1.txt")));
            sut.Apply(new DocumentMoved(new PathString("test/content3.txt"), new PathString("test/content2.txt")));
            var ev = sut.Events.Single() as DocumentMoved;
            Assert.IsNotNull(ev);
            Assert.AreEqual("test/content3.txt", ev.Path.ToString());
            Assert.AreEqual("test/content1.txt", ev.OldPath.ToString());
        }

        [TestMethod]
        public void ドキュメントが移動した後でさらに移動して元の位置に戻った場合移動はなかったことになる()
        {
            var sut = new LocalFileEventShrinker();
            sut.Apply(new DocumentMoved(new PathString("test/content2.txt"), new PathString("test/content1.txt")));
            sut.Apply(new DocumentMoved(new PathString("test/content1.txt"), new PathString("test/content2.txt")));
            Assert.IsFalse(sut.Events.Any());
        }
    }
}
