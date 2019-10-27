using Docms.Domain.Clients;
using Docms.Domain.Clients.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Docms.Domain.Tests
{
    [TestClass]
    public class ClientTests
    {
        Client sut;

        [TestInitialize]
        public void Setup()
        {
            sut = new Client(Guid.NewGuid().ToString(), "UPLOADER", "192.168.10.1");
            Assert.AreEqual("UPLOADER", sut.Type);
            Assert.AreEqual("192.168.10.1", sut.IpAddress);
            Assert.AreEqual(ClientStatus.Stopped, sut.Status);
        }

        [TestMethod]
        public void 新しいクライアントを生成した場合クライアントの作成イベントが発生する()
        {
            Assert.AreEqual(1, sut.DomainEvents.Count);
            Assert.IsTrue(sut.DomainEvents.Last() is ClientCreatedEvent);
        }

        [TestMethod]
        public void クライアントに要求を出した場合に要求イベントが発生する()
        {
            sut.Request(ClientRequestType.Start);
            Assert.AreEqual(2, sut.DomainEvents.Count);
            Assert.IsTrue(sut.DomainEvents.Last() is ClientRequestCreatedEvent);
        }

        [TestMethod]
        public void クライアントに現在と同じ要求を出した場合に要求イベントが発生しない()
        {
            sut.Request(ClientRequestType.Start);
            sut.Request(ClientRequestType.Start);
            Assert.AreEqual(2, sut.DomainEvents.Count);
        }

        [TestMethod]
        public void クライアントに要求を出さずに応答を返すとエラーが発生する()
        {
            Assert.ThrowsException<InvalidOperationException>(() => sut.Accept("invalid_request_id"));
        }

        [TestMethod]
        public void クライアントに要求を出してからその応答をすると応答イベントが発生する()
        {
            sut.Request(ClientRequestType.Start);
            sut.Accept(sut.RequestId);
            Assert.AreEqual(3, sut.DomainEvents.Count);
            Assert.IsTrue(sut.DomainEvents.Last() is ClientRequestAcceptedEvent);
        }

        [TestMethod]
        public void クライアントのステータスを更新するとステータス更新イベントが発生する()
        {
            sut.UpdateStatus(ClientStatus.Running);
            Assert.AreEqual(ClientStatus.Running, sut.Status);
            Assert.AreEqual(2, sut.DomainEvents.Count);
            Assert.IsTrue(sut.DomainEvents.Last() is ClientStatusUpdatedEvent);
        }
    }
}
