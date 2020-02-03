using Docms.Domain.Identity.Events;
using Docms.Domain.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Docms.Domain.Tests
{
    [TestClass]
    public class DeviceTests
    {
        [TestMethod]
        public void 新しいデバイスを生成した場合にデバイスの初期化イベントが発生する()
        {
            var sut = new Device(Guid.NewGuid().ToString(), "USERAGENT", "USER1");
            Assert.IsFalse(sut.Deleted);
            Assert.AreEqual(1, sut.DomainEvents.Count);
            Assert.IsTrue(sut.DomainEvents.First() is DeviceNewlyAccessedEvent);
        }

        [TestMethod]
        public void デバイスを許可したらデバイスの許可イベントが発生する()
        {
            var sut = new Device(Guid.NewGuid().ToString(), "USERAGENT", "USER1");
            sut.ClearDomainEvents();
            sut.Grant("userId1");
            Assert.IsFalse(sut.Deleted);
            Assert.AreEqual(1, sut.DomainEvents.Count);
            Assert.IsTrue(sut.DomainEvents.First() is DeviceGrantedEvent);
        }

        [TestMethod]
        public void デバイスを拒否したらデバイスの拒否イベントが発生する()
        {
            var sut = new Device(Guid.NewGuid().ToString(), "USERAGENT", "USER1");
            sut.ClearDomainEvents();
            sut.Revoke("userId1");
            Assert.IsTrue(sut.Deleted);
            Assert.AreEqual(1, sut.DomainEvents.Count);
            Assert.IsTrue(sut.DomainEvents.First() is DeviceRevokedEvent);
        }

        [TestMethod]
        public void デバイスを再登録したらデバイスの再登録イベントが発生する()
        {
            var sut = new Device(Guid.NewGuid().ToString(), "USERAGENT", "USER1");
            sut.Revoke("userId1");
            sut.ClearDomainEvents();
            sut.Reregister("user2");
            Assert.IsFalse(sut.Deleted);
            Assert.AreEqual(1, sut.DomainEvents.Count);
            Assert.IsTrue(sut.DomainEvents.First() is DeviceReregisteredEvent);
        }
    }
}
