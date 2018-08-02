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
    }
}
