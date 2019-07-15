using Docms.Client.Api.Documents;
using System.Threading.Tasks;

namespace Docms.IntegrationTests.Utils
{
    static class DocumentUtils
    {
        internal static async Task ClearAllAsync(DocumentClient client)
        {
            async Task deleteDir(string path)
            {
                var entries = await client.GetEntriesAsync(path).ConfigureAwait(false);
                foreach (var entry in entries)
                {
                    if (entry is Container)
                    {
                        await deleteDir(entry.Path);
                    }
                    else
                    {
                        await client.DeleteAsync(entry.Path).ConfigureAwait(false);
                    }
                }
            };

            await deleteDir("");
        }
    }
}
