using Docms.Client.Api.Responses;
using Docms.Client.Api.Serialization;
using IdentityModel.Client;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;

namespace Docms.Client.Api
{
    public class DocmsApiClient : IDocmsApiClient
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

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

        public DocmsApiClient(string uri, string defaultPath = "api/v1")
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new ArgumentNullException(nameof(uri));
            }
            _serverUri = uri.EndsWith("/") ? uri : uri + "/";
            _defaultPath = (defaultPath ?? "").EndsWith("/") ? defaultPath : defaultPath + "/";
            DefaultJsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = new DocmsJsonTypeBinder(),
                NullValueHandling = NullValueHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
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

            using (var discoveryClient = new DiscoveryClient(new Uri(_serverUri).GetLeftPart(UriPartial.Authority).ToString()))
            {
                discoveryClient.Policy.RequireHttps = false;
                var doc = await discoveryClient.GetAsync().ConfigureAwait(false);

                _tokenEndpoint = doc.TokenEndpoint;
                _introspectionEndpoint = doc.IntrospectionEndpoint;
                _revocationEndpoint = doc.RevocationEndpoint;

                if (_tokenEndpoint == null)
                {
                    throw new InvalidLoginException();
                }

                var client = new TokenClient(
                    _tokenEndpoint,
                    "docms-client",
                    "docms-client-secret");

                var response = await client.RequestResourceOwnerPasswordAsync(username, password, "docmsapi").ConfigureAwait(false);
                if (response.IsError)
                {
                    throw new InvalidLoginException();
                }
                _logger.Debug("login success.");
                _username = username;
                _password = password;
                _accessToken = response.AccessToken;
                _client = new RestClient(_serverUri)
                {
                    FollowRedirects = false,
                    Timeout = 30 * 60 * 1000,
                };
                _client.ConfigureWebRequest(x => x.AllowWriteStreamBuffering = false);
                _client.Authenticator = new JwtAuthenticator(_accessToken);
            }
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
            _client.Authenticator = null;
            _logger.Debug("logout success.");
        }

        /// <summary>
        /// トークンの検証を行う
        /// </summary>
        /// <returns></returns>
        public async Task VerifyTokenAsync()
        {
            try
            {
                using (var client = new IntrospectionClient(
                    _introspectionEndpoint,
                    "docmsapi",
                    "docmsapi-secret"))
                {
                    var response = await client.SendAsync(
                        new IntrospectionRequest
                        {
                            Token = _accessToken,
                            ClientId = "docms-client",
                            ClientSecret = "docms-client-secret",
                        }).ConfigureAwait(false);

                    _logger.Debug("token verified.");
                    if (response.IsActive)
                    {
                        return;
                    }
                }

            }
            catch { }
            await LoginAsync(_username, _password).ConfigureAwait(false);
        }

        private async Task<IRestResponse> ExecuteAsync(Func<RestRequest> requestFactory)
        {
            if (_client == null)
            {
                throw new InvalidLoginException();
            }
            var response = await _client.ExecuteTaskAsync(requestFactory.Invoke()).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                try
                {
                    await VerifyTokenAsync().ConfigureAwait(false);
                    response = await _client.ExecuteTaskAsync(requestFactory.Invoke()).ConfigureAwait(false);
                }
                catch { }
            }
            return response;
        }

        private void ThrowIfNotSuccessfulStatus(IRestResponse result)
        {
            if (!result.IsSuccessful)
            {
                if (result.ErrorException != null)
                {
                    throw new ServerException(result.Content, result.ErrorException);
                }
                else
                {
                    throw new ServerException(result.Request.Resource, result.Request.Method.ToString(), result.Request.ToString(), (int)result.StatusCode, result.Content);
                }
            }
        }

        public async Task<IEnumerable<Entry>> GetEntriesAsync(string path)
        {
            _logger.Debug("requesting get entries for path: " + path);
            var result = await ExecuteAsync(() =>
            {
                var request = new RestRequest(_defaultPath + "files", Method.GET);
                if (!string.IsNullOrEmpty(path))
                {
                    request.AddQueryParameter("path", path);
                }
                return request;
            }).ConfigureAwait(false);
            ThrowIfNotSuccessfulStatus(result);
            var container = JsonConvert.DeserializeObject<ContainerResponse>(result.Content, DefaultJsonSerializerSettings);
            return container.Entries
                .Select(e => e is ContainerResponse
                    ? new Container(e as ContainerResponse, this) as Entry
                    : new Document(e as DocumentResponse, this) as Entry);
        }

        public async Task<Document> GetDocumentAsync(string path)
        {
            _logger.Debug("Requesting get document for path: " + path);
            var result = await ExecuteAsync(() =>
            {
                var request = new RestRequest(_defaultPath + "files", Method.GET);
                request.AddQueryParameter("path", path ?? throw new ArgumentNullException(nameof(path)));
                return request;
            }).ConfigureAwait(false);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            else
            {
                ThrowIfNotSuccessfulStatus(result);
                var document = JsonConvert.DeserializeObject<DocumentResponse>(result.Content, DefaultJsonSerializerSettings);
                return new Document(document, this);
            }
        }

        public async Task<Stream> DownloadAsync(string path)
        {
            _logger.Debug("requesting downloading for path: " + path);
            var downloadResponse = new DownloadResponse();
            var result = await ExecuteAsync(() =>
            {
                var request = new RestRequest(_defaultPath + "files", Method.GET);
                request.ResponseWriter = stream => downloadResponse.WriteResponse(stream);
                if (!string.IsNullOrEmpty(path))
                {
                    request.AddQueryParameter("path", path);
                    request.AddQueryParameter("download", true.ToString());
                }
                return request;
            }).ConfigureAwait(false);
            try
            {
                ThrowIfNotSuccessfulStatus(result);
            }
            catch
            {
                downloadResponse.Dispose();
                throw;
            }
            return downloadResponse;
        }

        public async Task CreateOrUpdateDocumentAsync(string path, Func<Stream> streamFactory, DateTime? created = null, DateTime? lastModified = null)
        {
            _logger.Debug("requesting uploading for path: " + path);
            var result = await ExecuteAsync(() =>
            {
                var stream = streamFactory.Invoke();
                var request = new RestRequest(_defaultPath + "files", Method.POST);
                request.AddParameter("path", path ?? throw new ArgumentNullException(nameof(path)));
                request.AddFile("file", sr =>
                {
                    try
                    {
                        stream.CopyTo(sr);
                    }
                    finally
                    {
                        stream?.Dispose();
                    }
                }, path.Substring(path.LastIndexOf('/') > -1 ? path.LastIndexOf('/') : 0), stream.Length);
                if (created != null)
                {
                    request.AddParameter("created", XmlConvert.ToString(created.Value, XmlDateTimeSerializationMode.Utc));
                }
                if (lastModified != null)
                {
                    request.AddParameter("lastModified", XmlConvert.ToString(lastModified.Value, XmlDateTimeSerializationMode.Utc));
                }
                return request;
            }).ConfigureAwait(false);
            ThrowIfNotSuccessfulStatus(result);
        }

        public async Task MoveDocumentAsync(string originalPath, string destinationPath)
        {
            _logger.Debug("requesting move for original path: " + originalPath + " to destination path: " + destinationPath);
            var result = await ExecuteAsync(() =>
            {
                var request = new RestRequest(_defaultPath + "files/move", Method.POST);
                request.AddParameter("destinationPath", destinationPath);
                request.AddParameter("originalPath", originalPath);
                return request;
            }).ConfigureAwait(false);
            ThrowIfNotSuccessfulStatus(result);
        }

        public async Task DeleteDocumentAsync(string path)
        {
            _logger.Debug("requesting deletion for path: " + path);
            try
            {
                var result = await ExecuteAsync(() =>
                {
                    var request = new RestRequest(_defaultPath + "files", Method.DELETE);
                    request.AddQueryParameter("path", path);
                    return request;
                }).ConfigureAwait(false);
                ThrowIfNotSuccessfulStatus(result);
            }
            catch (ServerException ex)
            {
                if (ex.StatusCode == (int)HttpStatusCode.BadRequest)
                {
                    return;
                }
                throw;
            }
        }

        public async Task<IEnumerable<History>> GetHistoriesAsync(string path, Guid? lastHistoryId = null)
        {
            _logger.Debug("requesting histories for path: " + path + " lastHistoryId: " + lastHistoryId?.ToString() ?? "");
            var result = await ExecuteAsync(() =>
            {
                var request = new RestRequest(_defaultPath + "histories", Method.GET);
                if (!string.IsNullOrEmpty(path))
                {
                    request.AddQueryParameter("path", path);
                }
                if (lastHistoryId != null)
                {
                    request.AddQueryParameter("last_history_id", lastHistoryId.Value.ToString());
                }
                request.AddQueryParameter("per_page", "1000");
                return request;
            }).ConfigureAwait(false);
            ThrowIfNotSuccessfulStatus(result);

            var resultList = JsonConvert.DeserializeObject<List<History>>(result.Content, DefaultJsonSerializerSettings);
            var pagination = PaginationHeader.Parse(result.Headers.FirstOrDefault(h => h.Name == "Link")?.Value?.ToString());
            while (!string.IsNullOrEmpty(pagination?.Next))
            {
                result = await ExecuteAsync(() =>
                {
                    var request = new RestRequest(pagination.Next, Method.GET);
                    return request;
                }).ConfigureAwait(false);
                ThrowIfNotSuccessfulStatus(result);
                resultList.AddRange(JsonConvert.DeserializeObject<List<History>>(result.Content, DefaultJsonSerializerSettings));
                pagination = PaginationHeader.Parse(result.Headers.FirstOrDefault(h => h.Name == "Link")?.Value?.ToString());
            }
            return resultList;
        }
    }
}
