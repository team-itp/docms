using Microsoft.WindowsAzure.Storage;
using RestSharp;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client
{
    public class DocmsClient
    {
        private string _serverUri;
        private RestClient _client;
        private CloudStorageAccount _account;

        public DocmsClient(string uri)
        {
            _serverUri = uri.EndsWith("/") ? uri : uri + "/";
            _client = new RestClient(uri);
            _account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["storageAccount"]);
        }

        public async Task CreateDocumentAsync(string localFilePath, string name, string[] tags)
        {
            var storageClient = _account.CreateCloudBlobClient();
            var container = storageClient.GetContainerReference("files");
            await container.CreateIfNotExistsAsync();
            var blobName = Guid.NewGuid();
            var blob = container.GetBlockBlobReference(blobName.ToString());
            await blob.UploadFromFileAsync(localFilePath);
            var uri = blob.Uri.ToString();
            var request = new RestRequest(_serverUri + "api/documents", Method.POST);
            request.AddJsonBody(new
            {
                Uri = blob.Uri,
                Name = Path.GetFileName(localFilePath),
                Tags = tags
            });
            await _client.PostTaskAsync<dynamic>(request);
        }
    }
}
