using Docms.Client.Api;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Docms.Client.Tests.Utils
{
    public static class DocmsApiUtils
    {
        public static Task Create(IDocmsApiClient apiClient, string path)
        {
            var time = new DateTime(2019, 1, 1, 10, 11, 12, DateTimeKind.Utc);
            return apiClient.CreateOrUpdateDocumentAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(path)), time, time);
        }

        public static async Task Update(IDocmsApiClient apiClient, string path)
        {
            var file = await apiClient.GetDocumentAsync(path).ConfigureAwait(false);
            var stream = await apiClient.DownloadAsync(path).ConfigureAwait(false);
            var ms = new MemoryStream();
            await stream.CopyToAsync(ms).ConfigureAwait(false);
            var str = Encoding.UTF8.GetString(ms.ToArray()) + " updated";
            await apiClient.CreateOrUpdateDocumentAsync(path, new MemoryStream(Encoding.UTF8.GetBytes(str)), file.Created, file.LastModified.AddHours(1)).ConfigureAwait(false);
        }

        public static Task Move(IDocmsApiClient apiClient, string fromPath, string toPath)
        {
            return apiClient.MoveDocumentAsync(fromPath, toPath);
        }
    }
}