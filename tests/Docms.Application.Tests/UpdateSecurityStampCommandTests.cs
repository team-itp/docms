using Docms.Application.Commands;
using Docms.Application.Tests.Utils;
using Docms.Infrastructure.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Docms.Application.Tests
{
    [TestClass]
    public class UpdateSecurityStampCommandTests
    {
        private MockDocmsContext context;
        private UpdateSecurityStampCommandHandler sut;

        [TestInitialize]
        public void Initialize()
        {
            context = new MockDocmsContext(Guid.NewGuid().ToString());
            sut = new UpdateSecurityStampCommandHandler(context);
        }

        [TestMethod]
        public async Task ユーザーが存在しない場合にセキュリティスタンプの更新コマンドが実行されるとDOMSコンテキストのユーザーが作成される()
        {
            await sut.Handle(new UpdateSecurityStampCommand()
            {
                UserId = "unknown user id",
                SecurityStamp = "SECURITY_STAMP"
            });
            var user = await context.Users.FindAsync("unknown user id");
            Assert.IsNotNull(user);
            Assert.AreEqual("SECURITY_STAMP", user.SecurityStamp);
        }

        [TestMethod]
        public async Task ユーザーが存在する場合にセキュリティスタンプの更新コマンドが実行されるとDOMSコンテキストのユーザーが作成される()
        {
            await context.Users.AddAsync(new DocmsUser()
            {
                Id = "known_user",
                SecurityStamp = "SECURITY_STAMP1"
            });
            await context.SaveChangesAsync();

            await sut.Handle(new UpdateSecurityStampCommand()
            {
                UserId = "known_user",
                SecurityStamp = "SECURITY_STAMP2"
            });
            var user = await context.Users.FindAsync("known_user");
            Assert.AreEqual("SECURITY_STAMP2", user.SecurityStamp);
        }
    }
}
