﻿using RestSharp;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client
{
    public class DocmsClient
    {
        private string _serverUri;
        private RestClient _client;

        public DocmsClient(string uri)
        {
            _serverUri = uri.EndsWith("/") ? uri : uri + "/";
            _client = new RestClient(uri);
        }

        public async Task CreateDocumentAsync(string localFilePath, string name, string[] tags)
        {
            var fileUploadRequest = new RestRequest(_serverUri + "blobs");
            fileUploadRequest.AddFile(Guid.NewGuid().ToString() + Path.GetExtension(name), File.ReadAllBytes(localFilePath), Path.GetFileName(localFilePath));
            var fileUploadResponse = await _client.ExecutePostTaskAsync(fileUploadRequest);
            if (!fileUploadResponse.IsSuccessful)
            {
                // TODO: message
                throw new Exception();
            }

            var request = new RestRequest(_serverUri + "api/documents", Method.POST);
            request.AddJsonBody(new
            {
                Uri = fileUploadResponse.Headers.First(h => h.Name == "Location").Value,
                Name = Path.GetFileName(localFilePath),
                Tags = tags
            });
            await _client.PostTaskAsync<dynamic>(request);
        }
    }
}