﻿using IdentityModel;
using IdentityModel.Client;
using Docms.Client.Models;
using Newtonsoft.Json;
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
        private string _accessToken;
        private RestClient _client;

        public DocmsClient(string uri)
        {
            _serverUri = uri.EndsWith("/") ? uri : uri + "/";
            _client = new RestClient(uri);
        }

        public async Task LoginAsync(string username, string password)
        {
            var discoveryClient = new DiscoveryClient(new Uri(_serverUri).GetLeftPart(UriPartial.Authority).ToString());
            discoveryClient.Policy.RequireHttps = false;
            var doc = await discoveryClient.GetAsync();

            var tokenEndpoint = doc.TokenEndpoint;
            var client = new TokenClient(
                tokenEndpoint,
                "docms-client",
                "docms-client-secret");

            var response = await client.RequestResourceOwnerPasswordAsync(username, password, "docmsapi");
            _accessToken = response.AccessToken;
        }

        /// <summary>
        /// ユーザー情報取得
        /// </summary>
        /// <returns>ユーザー情報</returns>
        public async Task<IEnumerable<UserResponse>> GetUsersAsync()
        {
            var request = new RestRequest(_serverUri + "api/vs/users");
            request.AddHeader("Authorization", "Bearer " + _accessToken);
            var result = await _client.GetTaskAsync<List<UserResponse>>(request);
            return result;
        }

        /// <summary>
        /// タグ取得
        /// </summary>
        /// <returns>タグ</returns>
        public async Task<IEnumerable<TagResponse>> GetTagsAsync()
        {
            var request = new RestRequest(_serverUri + "api/tags");
            request.AddHeader("Authorization", "Bearer " + _accessToken);
            var result = await _client.GetTaskAsync<List<TagResponse>>(request);
            return result;
        }

        /// <summary>
        /// 顧客情報取得
        /// </summary>
        /// <returns>顧客情報</returns>
        public async Task<IEnumerable<CustomerResponse>> GetCustomersAsync()
        {
            var request = new RestRequest(_serverUri + "api/vs/customers");
            request.AddHeader("Authorization", "Bearer " + _accessToken);
            var result = await _client.ExecuteGetTaskAsync(request);
            return JsonConvert.DeserializeObject<List<CustomerResponse>>(result.Content);
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
            fileUploadRequest.AddHeader("Authorization", "Bearer " + _accessToken);
            fileUploadRequest.AddFile("file", File.ReadAllBytes(localFilePath), Guid.NewGuid().ToString() + Path.GetExtension(name));
            var fileUploadResponse = await _client.ExecutePostTaskAsync(fileUploadRequest);
            if (!fileUploadResponse.IsSuccessful)
            {
                // TODO: message
                throw new Exception();
            }
            var blobName = JsonConvert.DeserializeObject<FileCreatedResponse>(fileUploadResponse.Content).BlobName;

            var request = new RestRequest(_serverUri + "api/documents", Method.POST);
            request.AddHeader("Authorization", "Bearer " + _accessToken);
            request.AddJsonBody(new UploadDocumentRequest()
            {
                BlobName = blobName,
                Name = name,
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
