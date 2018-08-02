using Docms.Infrastructure.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Docms.Infrastructure.Tests
{
    [TestClass]
    public class FilePathTests
    {
        [TestMethod]
        public void �K�w�̂Ȃ��p�X�̕����񂩂�C���X�^���X�����Ċg���q�ƃt�@�C�������擾�ł��邱��()
        {
            var sut = new FilePath("test.txt");
            Assert.AreEqual(null, sut.DirectoryPath);
            Assert.AreEqual("test.txt", sut.FileName);
            Assert.AreEqual("test", sut.FileNameWithoutExtension);
            Assert.AreEqual(".txt", sut.Extension);
        }

        [TestMethod]
        public void �K�w�̂���p�X�̕����񂩂�C���X�^���X�����Ċg���q�ƃt�@�C�����ƃf�B���N�g�������擾�ł��邱��()
        {
            var sut = new FilePath("path1\\path2\\test.txt");
            Assert.AreEqual(new FilePath("path1\\path2"), sut.DirectoryPath);
            Assert.AreEqual(new FilePath("path1"), sut.DirectoryPath.DirectoryPath);
            Assert.AreEqual("test.txt", sut.FileName);
            Assert.AreEqual("test", sut.FileNameWithoutExtension);
            Assert.AreEqual(".txt", sut.Extension);
        }

        [TestMethod]
        public void �g���q�̂Ȃ��p�X�̕����񂩂�C���X�^���X�����ăt�@�C�����ƃf�B���N�g�������擾�ł��邱��()
        {
            var sut = new FilePath("path1\\test");
            Assert.AreEqual(new FilePath("path1"), sut.DirectoryPath);
            Assert.AreEqual(null, sut.DirectoryPath.DirectoryPath);
            Assert.AreEqual("test", sut.FileName);
            Assert.AreEqual("test", sut.FileNameWithoutExtension);
            Assert.AreEqual(null, sut.Extension);
        }
    }
}
