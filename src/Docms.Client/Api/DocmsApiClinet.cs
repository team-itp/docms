using Docms.Client.Api.Responses;
using Docms.Client.Api.Serialization;
using IdentityModel.Client;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Docms.Client.Api
{
    public class DocmsApiClinet : IDocmsApiClient
    {
        private readonly string _serverUri;
        private readonly string _defaultPath;
        private string _tokenEndpoint;
        private string _introspectionEndpoint;
        private string _revocationEndpoint;
        private string _username;
        private string _password;
        private string _accessToken;
        private RestClient _client;

        public JsonSerializerSettings DefaultJsonSerializerSettings { get; set; }

        public DocmsApiClinet(string uri, string defaultPath = "api/v1")
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new ArgumentNullException(nameof(uri));
            }
            _serverUri = uri.EndsWith("/") ? uri : uri + "/";
            _defaultPath = (defaultPath ?? "").EndsWith("/") ? defaultPath : defaultPath + "/";
            _client = new RestClient(_serverUri);
            DefaultJsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = new DocmsJsonTypeBinder(),
                NullValueHandling = NullValueHandling.Ignore,
            };
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
                if (result.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new NotFoundException(result.ResponseUri.ToString());
                }
                throw new ServerException((int)result.StatusCode, result.Content);
            }
        }

        public async Task<IEnumerable<Entry>> GetEntriesAsync(string path)
        {
            var request = new RestRequest(_defaultPath + "files", Method.GET);
            if (!string.IsNullOrEmpty(path))
            {
                request.AddQueryParameter("path", path);
            }
            request.AddHeader("Authorization", "Bearer " + _accessToken);
            var result = await _client.ExecuteGetTaskAsync(request).ConfigureAwait(false);
            ThrowIfNotSuccessfulStatus(result);
            var container = JsonConvert.DeserializeObject<ContainerResponse>(result.Content, DefaultJsonSerializerSettings);
            return container.Entries
                .Select(e => e is ContainerResponse
                    ? new Container(e as ContainerResponse, this) as Entry
                    : new Document(e as DocumentResponse, this) as Entry);
        }

        public async Task<Document> GetDocumentAsync(string path)
        {
            var request = new RestRequest(_defaultPath + "files", Method.GET);
            request.AddQueryParameter("path", path ?? throw new ArgumentNullException(nameof(path)));
            request.AddHeader("Authorization", "Bearer " + _accessToken);
            var result = await _client.ExecuteGetTaskAsync(request).ConfigureAwait(false);
            ThrowIfNotSuccessfulStatus(result);
            var document = JsonConvert.DeserializeObject<DocumentResponse>(result.Content, DefaultJsonSerializerSettings);
            return new Document(document, this);
        }

        public async Task<Stream> DownloadAsync(string path)
        {
            var request = new RestRequest(_defaultPath + "files", Method.GET);
            if (!string.IsNullOrEmpty(path))
            {
                request.AddQueryParameter("path", path);
            }
            request.AddHeader("Authorization", "Bearer " + _accessToken);
            var result = await _client.ExecuteGetTaskAsync(request).ConfigureAwait(false);
            ThrowIfNotSuccessfulStatus(result);
            return new MemoryStream(result.RawBytes);
        }

        public async Task CreateOrUpdateDocumentAsync(string path, Stream stream, DateTime? created = null, DateTime? lastModified = null)
        {
            using (var ms = new MemoryStream())
            {
                var request = new RestRequest(_defaultPath + "files", Method.POST);
                request.AddParameter("path", path ?? throw new ArgumentNullException(nameof(path)));
                await stream.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);
                request.AddFile("file", ms.ToArray(), path.Substring(path.LastIndexOf('/') > -1 ? path.LastIndexOf('/') : 0));
                if (created != null)
                {
                    request.AddParameter("created", created.Value);
                }
                if (lastModified != null)
                {
                    request.AddParameter("lastModified", lastModified.Value);
                }
                request.AddHeader("Authorization", "Bearer " + _accessToken);
                var result = await _client.ExecutePostTaskAsync(request).ConfigureAwait(false);
                ThrowIfNotSuccessfulStatus(result);
            }
        }

        public async Task MoveDocumentAsync(string originalPath, string destinationPath)
        {
            var request = new RestRequest(_defaultPath + "files/move", Method.POST);
            request.AddParameter("destinationPath", destinationPath);
            request.AddParameter("originalPath", originalPath);
            request.AddHeader("Authorization", "Bearer " + _accessToken);
            var result = await _client.ExecutePostTaskAsync(request).ConfigureAwait(false);
            ThrowIfNotSuccessfulStatus(result);
        }

        public async Task DeleteDocumentAsync(string path)
        {
            var request = new RestRequest(_defaultPath + "files", Method.DELETE);
            request.AddQueryParameter("path", path);
            request.AddHeader("Authorization", "Bearer " + _accessToken);
            var result = await _client.ExecuteTaskAsync(request).ConfigureAwait(false);
            ThrowIfNotSuccessfulStatus(result);
        }

        public async Task<IEnumerable<History>> GetHistoriesAsync(string path, DateTime? since = null)
        {
            var request = new RestRequest(_defaultPath + "histories", Method.GET);
            if (!string.IsNullOrEmpty(path))
            {
                request.AddQueryParameter("path", path);
            }
            request.AddHeader("Authorization", "Bearer " + _accessToken);
            var result = await _client.ExecuteGetTaskAsync(request).ConfigureAwait(false);
            ThrowIfNotSuccessfulStatus(result);
            return JsonConvert.DeserializeObject<List<History>>(result.Content, DefaultJsonSerializerSettings);
        }
    }
}
