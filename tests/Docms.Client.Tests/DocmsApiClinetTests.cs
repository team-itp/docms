using Docms.Client.Api;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    public class DocmsApiClinetTests
    {
        [TestMethod]
        public async Task �T�[�o�[��胋�[�g�f�B���N�g�����̃t�@�C���̈ꗗ���擾����()
        {
            var sut = new DocmsApiClinet("http://localhost:51693", "api/v1");
            var rootContainer = await sut.GetEntriesAsync("/");
        }
    }
}
