using IdentityModel.Client;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Docms.Client.Api
{
    public class DocmsApiClinet : IDocmsApiClient
    {
        private string _serverUri;
        private string _tokenEndpoint;
        private string _introspectionEndpoint;
        private string _revocationEndpoint;
        private string _username;
        private string _password;
        private string _accessToken;
        private RestClient _client;

        public DocmsApiClinet(string uri, string defaultPath)
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new ArgumentNullException(nameof(uri));
            }
            if (!uri.EndsWith("/"))
            {
                uri += "/";
            }
            _serverUri = uri.EndsWith("/") ? uri : uri + "/";
            _client = new RestClient(uri);
        }

        /// <summary>
        /// ログイン
        /// </summary>
        /// <param name="username">ユーザー名</param>
        /// <param name="password">パスワード</param>
        /// <returns></returns>
        public async Task LoginAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            var discoveryClient = new DiscoveryClient(new Uri(_serverUri).GetLeftPart(UriPartial.Authority).ToString());
            discoveryClient.Policy.RequireHttps = false;
            var doc = await discoveryClient.GetAsync().ConfigureAwait(false);

            _tokenEndpoint = doc.TokenEndpoint;
            _introspectionEndpoint = doc.IntrospectionEndpoint;
            _revocationEndpoint = doc.RevocationEndpoint;

            var client = new TokenClient(
                _tokenEndpoint,
                "docms-client",
                "docms-client-secret");

            var response = await client.RequestResourceOwnerPasswordAsync(username, password, "docmsapi").ConfigureAwait(false);
            if (response.IsError)
            {
                throw new InvalidLoginException();
            }
            _username = username;
            _password = password;
            _accessToken = response.AccessToken;
        }

        /// <summary>
        /// ログアウト
        /// </summary>
        /// <returns></returns>
        public async Task LogoutAsync()
        {
            var client = new TokenRevocationClient(
                _revocationEndpoint,
                "docms-client",
                "docms-client-secret");

            await client.RevokeAccessTokenAsync(_accessToken).ConfigureAwait(false);
        }

        /// <summary>
        /// トークンの検証を行う
        /// </summary>
        /// <returns></returns>
        public async Task VerifyTokenAsync()
        {
            var client = new IntrospectionClient(
                _introspectionEndpoint,
                "docmsapi",
                "docmsapi-secret");

            var response = await client.SendAsync(
                new IntrospectionRequest
                {
                    Token = _accessToken,
                    ClientId = "docms-client",
                    ClientSecret = "docms-client-secret",
                }).ConfigureAwait(false);

            if (!response.IsActive)
            {
                await LoginAsync(_username, _password);
            }
        }

        private void ThrowIfNotSuccessfulStatus(IRestResponse result)
        {
            if (!result.IsSuccessful)
            {
                throw new ServerException((int)result.StatusCode, result.Content);
            }
        }

        public Task CreateDocumentAsync(string path, Stream stream)
        {
            throw new NotImplementedException();
        }

        public Task DeleteDocumentAsync(string path)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> DownloadAsync(string path)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Entry>> GetEntriesAsync(string path)
        {
            throw new NotImplementedException();
        }

        public Task<Document> GetDocumentAsync(string path)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<History>> GetHistoriesAsync(string path, DateTime? lastSynced = null)
        {
            throw new NotImplementedException();
        }

        public Task MoveDocumentAsync(string originalPath, string destinationPath)
        {
            throw new NotImplementedException();
        }

        public Task UpdateDocumentAsync(string path, Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
