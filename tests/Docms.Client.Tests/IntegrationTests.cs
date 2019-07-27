using Docms.Client.Tasks;
using Docms.Client.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace Docms.Client.Tests
{
    [TestClass]
    [Ignore]
   public class IntegrationTests
    {
        [TestMethod]
        public async Task 大量のファイルを登録_更新しても問題が発生しないこと()
        {
            var context = new MockApplicationContext();
            context.App = new Application();

            for(var i = 0; i < 1000; i++)
            {
                for (var j = 0; j < 1000; j++)
                {
                    await FileSystemUtils.Create(context.FileSystem, $"dir{i}/test{j}.txt");
                }
            }

            var sut = new SyncTask(context, context.App.CancellationToken);
            await sut.ExecuteAsync();

            for (var i = 0; i < 1000; i++)
            {
                for (var j = 0; j < 1000; j++)
                {
                    await FileSystemUtils.Update(context.FileSystem, $"dir{i}/test{j}.txt");
                }
            }

            var sut2 = new SyncTask(context, context.App.CancellationToken);
            await sut2.ExecuteAsync();
        }
    }
}
