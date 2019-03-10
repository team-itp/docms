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
    }
}