using Docms.Domain.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Docms.Domain.Tests
{
    [TestClass]
    public class StModeTests
    {
        [TestMethod]
        public void _170000はファイル種別を示すビット領域を表すビットマスク()
        {
            Assert.AreEqual("170000", StMode.S_IFMT.ToHexString());
        }

        [TestMethod]
        public void _140000はソケット()
        {
            Assert.AreEqual("140000", StMode.S_IFSOCK.ToHexString());
        }

        [TestMethod]
        public void _120000はシンボリックリンク()
        {
            Assert.AreEqual("120000", StMode.S_IFLNK.ToHexString());
        }

        [TestMethod]
        public void _100000は通常のファイル()
        {
            Assert.AreEqual("100000", StMode.S_IFREG.ToHexString());
        }

        [TestMethod]
        public void _060000はブロックデバイス()
        {
            Assert.AreEqual("060000", StMode.S_IFBLK.ToHexString());
        }

        [TestMethod]
        public void _040000はディレクトリ()
        {
            Assert.AreEqual("040000", StMode.S_IFDIR.ToHexString());
        }

        [TestMethod]
        public void _020000はキャラクターデバイス()
        {
            Assert.AreEqual("020000", StMode.S_IFCHR.ToHexString());
        }

        [TestMethod]
        public void _010000はFIFO()
        {
            Assert.AreEqual("010000", StMode.S_IFIFO.ToHexString());
        }

        [TestMethod]
        public void _004000はset_user_ID_bit()
        {
            Assert.AreEqual("004000", StMode.S_ISUID.ToHexString());
        }

        [TestMethod]
        public void _002000はset_group_ID_bit()
        {
            Assert.AreEqual("002000", StMode.S_ISGID.ToHexString());
        }

        [TestMethod]
        public void _001000はスティッキービット()
        {
            Assert.AreEqual("001000", StMode.S_ISVTX.ToHexString());
        }

        [TestMethod]
        public void _000700ファイル所有者のアクセス許可用のビットマスク()
        {
            Assert.AreEqual("000700", StMode.S_IRWXU.ToHexString());
        }

        [TestMethod]
        public void _000400所有者の読み込み許可()
        {
            Assert.AreEqual("000400", StMode.S_IRUSR.ToHexString());
        }

        [TestMethod]
        public void _000200所有者の書き込み許可()
        {
            Assert.AreEqual("000200", StMode.S_IWUSR.ToHexString());
        }

        [TestMethod]
        public void _000100所有者の実行許可()
        {
            Assert.AreEqual("000100", StMode.S_IXUSR.ToHexString());
        }

        [TestMethod]
        public void _000070グループのアクセス許可用のビットマスク()
        {
            Assert.AreEqual("000070", StMode.S_IRWXG.ToHexString());
        }

        [TestMethod]
        public void _000040グループの読み込み許可()
        {
            Assert.AreEqual("000040", StMode.S_IRGRP.ToHexString());
        }

        [TestMethod]
        public void _000020グループの書き込み許可()
        {
            Assert.AreEqual("000020", StMode.S_IWGRP.ToHexString());
        }

        [TestMethod]
        public void _000010グループの実行許可()
        {
            Assert.AreEqual("000010", StMode.S_IXGRP.ToHexString());
        }

        [TestMethod]
        public void _000007他人のアクセス許可用のビットマスク()
        {
            Assert.AreEqual("000007", StMode.S_IRWXO.ToHexString());
        }

        [TestMethod]
        public void _000004他人の読み込み許可()
        {
            Assert.AreEqual("000004", StMode.S_IROTH.ToHexString());
        }

        [TestMethod]
        public void _000002他人の書き込み許可()
        {
            Assert.AreEqual("000002", StMode.S_IWOTH.ToHexString());
        }

        [TestMethod]
        public void _000001他人の実行許可()
        {
            Assert.AreEqual("000001", StMode.S_IXOTH.ToHexString());
        }

    }
}
