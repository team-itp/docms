using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Docms.Client.Api.Documents
{
    public class DocumentClient
    {
        private DocmsApiClient _client;

        public DocumentClient(DocmsApiClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<Entry>> GetEntriesAsync(string path = null)
        {
            var jObject = await _client.GetAsync<JObject>("files", new[]
            {
                new KeyValuePair<string, string>("path", path)
            }).ConfigureAwait(false);
            var result = new List<Entry>();
            foreach (var entry in jObject["entries"])
            {
                if (entry["$type"].ToString() == "container")
                {
                    result.Add(new Container(entry["path"].ToString(), entry["name"].ToString()));
                }
                else
                {
                    result.Add(new Document(entry["path"].ToString(), entry["name"].ToString()));
                }
            }
            return result;
        }

        public Task UploadAsync(string path, Stream stream)
        {
            var content = new MultipartFormDataContent();
            var file = new StreamContent(stream);
            file.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = path
            };
            content.Add(file, "file");
            content.Add(new StringContent(path), "path");
            return _client.PostAsync("files", content);
        }

        public Task DeleteAsync(string path)
        {
            return _client.DeleteAsync("files", new[]
            {
                new KeyValuePair<string, string>("path", path)
            });
        }
    }
}
