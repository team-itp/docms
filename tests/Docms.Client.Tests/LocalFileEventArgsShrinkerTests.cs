using Docms.Client.FileWatching;
using Docms.Client.SeedWork;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Docms.Client.Tests
{
    [TestClass]
    public class LocalFileEventArgsShrinkerTests
    {
        [TestMethod]
        public void ドキュメントが新規作成された後に更新された場合更新は無視される()
        {
            var sut = new LocalFileEventArgsShrinker();
            sut.Apply(new FileCreatedEventArgs(new PathString("test/content1.txt")));
            sut.Apply(new FileModifiedEventArgs(new PathString("test/content1.txt")));
            var ev = sut.Events.Single();
            Assert.IsTrue(ev is FileCreatedEventArgs);
        }

        [TestMethod]
        public void ドキュメントが新規作成された後に削除された場合イベントは存在しない()
        {
            var sut = new LocalFileEventArgsShrinker();
            sut.Apply(new FileCreatedEventArgs(new PathString("test/content1.txt")));
            sut.Apply(new FileDeletedEventArgs(new PathString("test/content1.txt")));
            Assert.IsFalse(sut.Events.Any());
        }

        [TestMethod]
        public void ドキュメントが新規作成された後に移動した場合移動先で新規作成されたこととなる()
        {
            var sut = new LocalFileEventArgsShrinker();
            sut.Apply(new FileCreatedEventArgs(new PathString("test/content1.txt")));
            sut.Apply(new FileMovedEventArgs(new PathString("test/content2.txt"), new PathString("test/content1.txt")));
            var ev = sut.Events.Single();
            Assert.IsTrue(ev is FileCreatedEventArgs);
            Assert.AreEqual("test/content2.txt", ev.Path.ToString());
        }

        [TestMethod]
        public void ドキュメントが更新された後に削除された場合単に削除されたことになる()
        {
            var sut = new LocalFileEventArgsShrinker();
            sut.Apply(new FileModifiedEventArgs(new PathString("test/content1.txt")));
            sut.Apply(new FileDeletedEventArgs(new PathString("test/content1.txt")));
            var ev = sut.Events.Single();
            Assert.IsTrue(ev is FileDeletedEventArgs);
        }

        [TestMethod]
        public void ドキュメントが更新された後に再度更新された場合一度更新されたことになる()
        {
            var sut = new LocalFileEventArgsShrinker();
            sut.Apply(new FileModifiedEventArgs(new PathString("test/content1.txt")));
            sut.Apply(new FileModifiedEventArgs(new PathString("test/content1.txt")));
            var ev = sut.Events.Single();
            Assert.IsTrue(ev is FileModifiedEventArgs);
        }

        [TestMethod]
        public void ドキュメントが更新された後に移動した場合元の場所で削除されて新しい場所で新規に作成されたことになる()
        {
            var sut = new LocalFileEventArgsShrinker();
            sut.Apply(new FileModifiedEventArgs(new PathString("test/content1.txt")));
            sut.Apply(new FileMovedEventArgs(new PathString("test/content2.txt"), new PathString("test/content1.txt")));
            Assert.AreEqual(2, sut.Events.Count());
            var ev1 = sut.Events.First();
            Assert.IsTrue(ev1 is FileDeletedEventArgs);
            Assert.AreEqual("test/content1.txt", ev1.Path.ToString());
            var ev2 = sut.Events.Last();
            Assert.IsTrue(ev2 is FileCreatedEventArgs);
            Assert.AreEqual("test/content2.txt", ev2.Path.ToString());
        }

        [TestMethod]
        public void ドキュメントが削除された後に再度作成された場合更新されたことになる()
        {
            var sut = new LocalFileEventArgsShrinker();
            sut.Apply(new FileDeletedEventArgs(new PathString("test/content1.txt")));
            sut.Apply(new FileCreatedEventArgs(new PathString("test/content1.txt")));
            var ev = sut.Events.Single();
            Assert.IsTrue(ev is FileModifiedEventArgs);
        }

        [TestMethod]
        public void ドキュメントが削除された後に他の場所から移動してきた場合移動元は削除されたこととなり移動先は更新されたこととなる()
        {
            var sut = new LocalFileEventArgsShrinker();
            sut.Apply(new FileDeletedEventArgs(new PathString("test/content1.txt")));
            sut.Apply(new FileMovedEventArgs(new PathString("test/content1.txt"), new PathString("test/content2.txt")));
            Assert.AreEqual(2, sut.Events.Count());
            var ev1 = sut.Events.First();
            Assert.IsTrue(ev1 is FileDeletedEventArgs);
            Assert.AreEqual("test/content1.txt", ev1.Path.ToString());
            var ev2 = sut.Events.Last();
            Assert.IsTrue(ev2 is FileMovedEventArgs);
            Assert.AreEqual("test/content1.txt", ev2.Path.ToString());
        }

        [TestMethod]
        public void ドキュメントが移動した後で移動元に生成された場合そのままの履歴になる()
        {
            var sut = new LocalFileEventArgsShrinker();
            sut.Apply(new FileModifiedEventArgs(new PathString("test/content2.txt")));
            sut.Apply(new FileMovedEventArgs(new PathString("test/content1.txt"), new PathString("test/content2.txt")));
            sut.Apply(new FileCreatedEventArgs(new PathString("test/content2.txt")));
            Assert.AreEqual(2, sut.Events.Count());
            var ev1 = sut.Events.First();
            Assert.IsTrue(ev1 is FileCreatedEventArgs);
            Assert.AreEqual("test/content1.txt", ev1.Path.ToString());
            var ev2 = sut.Events.Last();
            Assert.IsTrue(ev2 is FileModifiedEventArgs);
            Assert.AreEqual("test/content2.txt", ev2.Path.ToString());
        }

        [TestMethod]
        public void 更新されたドキュメントが移動して移動元に新規に生成された場合更新されたことになる()
        {
            var sut = new LocalFileEventArgsShrinker();
            sut.Apply(new FileModifiedEventArgs(new PathString("test/content2.txt")));
            sut.Apply(new FileMovedEventArgs(new PathString("test/content1.txt"), new PathString("test/content2.txt")));
            sut.Apply(new FileCreatedEventArgs(new PathString("test/content2.txt")));
            Assert.AreEqual(2, sut.Events.Count());
            var ev1 = sut.Events.First();
            Assert.IsTrue(ev1 is FileCreatedEventArgs);
            Assert.AreEqual("test/content1.txt", ev1.Path.ToString());
            var ev2 = sut.Events.Last();
            Assert.IsTrue(ev2 is FileModifiedEventArgs);
            Assert.AreEqual("test/content2.txt", ev2.Path.ToString());
        }

        [TestMethod]
        public void ドキュメントが移動した後で移動先のドキュメントが削除された場合移動元が削除されたことになる()
        {
            var sut = new LocalFileEventArgsShrinker();
            sut.Apply(new FileMovedEventArgs(new PathString("test/content1.txt"), new PathString("test/content2.txt")));
            sut.Apply(new FileDeletedEventArgs(new PathString("test/content1.txt")));
            var ev = sut.Events.Single();
            Assert.IsTrue(ev is FileDeletedEventArgs);
            Assert.AreEqual("test/content2.txt", ev.Path.ToString());
        }

        [TestMethod]
        public void ドキュメントが移動した後で移動先が更新された場合移動元が削除されて移動先に新規に作成されたことになる()
        {
            var sut = new LocalFileEventArgsShrinker();
            sut.Apply(new FileMovedEventArgs(new PathString("test/content1.txt"), new PathString("test/content2.txt")));
            sut.Apply(new FileModifiedEventArgs(new PathString("test/content1.txt")));
            Assert.AreEqual(2, sut.Events.Count());
            var ev1 = sut.Events.First();
            Assert.IsTrue(ev1 is FileDeletedEventArgs);
            Assert.AreEqual("test/content2.txt", ev1.Path.ToString());
            var ev2 = sut.Events.Last();
            Assert.IsTrue(ev2 is FileCreatedEventArgs);
            Assert.AreEqual("test/content1.txt", ev2.Path.ToString());
        }

        [TestMethod]
        public void ドキュメントが移動した後でさらに移動した場合1つの移動に集約される()
        {
            var sut = new LocalFileEventArgsShrinker();
            sut.Apply(new FileMovedEventArgs(new PathString("test/content2.txt"), new PathString("test/content1.txt")));
            sut.Apply(new FileMovedEventArgs(new PathString("test/content3.txt"), new PathString("test/content2.txt")));
            var ev = sut.Events.Single() as FileMovedEventArgs;
            Assert.IsNotNull(ev);
            Assert.AreEqual("test/content3.txt", ev.Path.ToString());
            Assert.AreEqual("test/content1.txt", ev.FromPath.ToString());
        }

        [TestMethod]
        public void ドキュメントが移動した後でさらに移動して元の位置に戻った場合移動はなかったことになる()
        {
            var sut = new LocalFileEventArgsShrinker();
            sut.Apply(new FileMovedEventArgs(new PathString("test/content2.txt"), new PathString("test/content1.txt")));
            sut.Apply(new FileMovedEventArgs(new PathString("test/content1.txt"), new PathString("test/content2.txt")));
            Assert.IsFalse(sut.Events.Any());
        }
    }
}
