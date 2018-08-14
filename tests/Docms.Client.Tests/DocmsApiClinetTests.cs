using Docms.Client.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class DocmsApiClinetTests
    {
        private DocmsApiClinet sut;

        [TestInitialize]
        public void Setup()
        {
            sut = new DocmsApiClinet("http://localhost:51693");
        }

        [TestMethod]
        public async Task �T�[�o�[��胋�[�g�f�B���N�g�����̃t�@�C���̈ꗗ���擾����()
        {
            var entries = await sut.GetEntriesAsync("");
        }

        [TestMethod]
        public async Task �T�[�o�[�Ƀt�@�C�����A�b�v���[�h����()
        {
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test.txt", new MemoryStream(Encoding.UTF8.GetBytes("test1")));
        }

        [TestMethod]
        public async Task �T�[�o�[�̃t�@�C�����ړ�����()
        {
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test1.txt", new MemoryStream(Encoding.UTF8.GetBytes("test1")));
            await sut.CreateOrUpdateDocumentAsync("test1/subtest1/test2.txt", new MemoryStream(Encoding.UTF8.GetBytes("test2")));
            await sut.DeleteDocumentAsync("test1/subtest1/test2.txt");
            await sut.MoveDocumentAsync("test1/subtest1/test1.txt", "test1/subtest1/test2.txt");
        }
    }
}
