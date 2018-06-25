using Docms.Client.Models;
using RestSharp;
using System;
using System.Collections.Generic;
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

        /// <summary>
        /// ユーザー情報取得
        /// </summary>
        /// <returns>ユーザー情報</returns>
        public async Task<IEnumerable<UserResponse>> GetUsersAsync()
        {
            var request = new RestRequest(_serverUri + "api/vs/users");
            var result = await _client.GetTaskAsync<List<UserResponse>>(request);
            return result;
        }

        /// <summary>
        /// 顧客情報取得
        /// </summary>
        /// <returns>顧客情報</returns>
        public async Task<IEnumerable<CustomerResponse>> GetCustomerAsync()
        {
            var request = new RestRequest(_serverUri + "api/vs/customers");
            var result = await _client.GetTaskAsync<List<CustomerResponse>>(request);
            return result;
        }

        /// <summary>
        /// ドキュメント情報作成
        /// </summary>
        /// <param name="localFilePath">アップロードするファイルパス</param>
        /// <param name="name">ファイル名</param>
        /// <param name="tags">タグ名</param>
        /// <returns></returns>
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
            var registResponse = await _client.ExecutePostTaskAsync(request);
            if (!registResponse.IsSuccessful)
            {
                throw new Exception();
            }
        }
    }
}
