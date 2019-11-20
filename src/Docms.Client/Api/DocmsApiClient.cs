using Docms.Client.Api.Responses;
using Docms.Client.Api.Serialization;
using Docms.Client.Exceptions;
using IdentityModel.Client;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;

namespace Docms.Client.Api
{
    public class DocmsApiClient : IDocmsApiClient
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly string _serverUri;
        private readonly string _defaultPath;
        private readonly string _uploadClientId;
        private string _tokenEndpoint;
        private string _introspectionEndpoint;
        private string _revocationEndpoint;
        private string _accessToken;
        private readonly HttpClient _client;

        public JsonSerializerSettings DefaultJsonSerializerSettings { get; set; }

        public DocmsApiClient(string uploadClientId, string uri, string defaultPath = "api/v1")
        {
            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new ArgumentNullException(nameof(uri));
            }
            _serverUri = uri.EndsWith("/") ? uri : uri + "/";
            _defaultPath = (defaultPath ?? "").EndsWith("/") ? defaultPath : defaultPath + "/";
            _uploadClientId = uploadClientId;
            _client = new HttpClient()
            {
                Timeout = TimeSpan.FromMinutes(30),
            };
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
        /// <returns></returns>
        public async Task LoginAsync()
        {
            using (var discoveryClient = new DiscoveryClient(new Uri(_serverUri).GetLeftPart(UriPartial.Authority).ToString()))
            {
                discoveryClient.Policy.RequireHttps = false;
                var doc = await discoveryClient.GetAsync().ConfigureAwait(false);

                _tokenEndpoint = doc.TokenEndpoint;
                _introspectionEndpoint = doc.IntrospectionEndpoint;
                _revocationEndpoint = doc.RevocationEndpoint;

                if (_tokenEndpoint == null)
                {
                    throw new ServiceUnavailableException();
                }

                var client = new TokenClient(
                    _tokenEndpoint,
                    "docms-client",
                    "docms-client-secret");

                var response = await client.RequestClientCredentialsAsync("docmsapi").ConfigureAwait(false);
                if (response.IsError)
                {
                    throw new InvalidLoginException();
                }
                _logger.Debug("login success.");
                _accessToken = response.AccessToken;
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
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
            _client.DefaultRequestHeaders.Authorization = null;
            _logger.Debug("logout success.");
        }

        /// <summary>
        /// 現在保持中のアクセストークンを確認する
        /// </summary>
        /// <returns></returns>
        private async Task<bool> CheckAccessTokenIsActiveAsync()
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
                return response.IsActive;
            }
        }

        /// <summary>
        /// トークンの検証を行い、エラーがあれば再度トークンを取得する
        /// </summary>
        /// <returns></returns>
        private async Task<bool> VerifyTokenAsync()
        {
            if (await CheckAccessTokenIsActiveAsync().ConfigureAwait(false))
            {
                return true;
            }

            // 正常にログインできない場合はエラーが投げられる
            await LoginAsync().ConfigureAwait(false);
            return false;
        }

        private async Task<HttpResponseMessage> ExecuteAsync(Func<HttpRequestMessage> requestFactory)
        {
            try
            {
                var response = await _client.SendAsync(requestFactory.Invoke()).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.NotFound)
                {
                    if (await VerifyTokenAsync().ConfigureAwait(false))
                    {
                        return response;
                    }
                    response = await _client.SendAsync(requestFactory.Invoke()).ConfigureAwait(false);
                }
                return response;
            }
            catch (Exception ex)
            {
                throw new ServerException("", ex);
            }
        }

        private async Task<T> ParseJson<T>(HttpResponseMessage result)
        {
            return JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync().ConfigureAwait(false), DefaultJsonSerializerSettings);
        }

        private string ToQueryString(Dictionary<string, string> query)
        {
            return string.Join("&", query.Select(kv => Uri.EscapeDataString(kv.Key) + "=" + Uri.EscapeDataString(kv.Value)));
        }

        private void ThrowIfNotSuccessfulStatus(HttpResponseMessage result)
        {
            if (!result.IsSuccessStatusCode)
            {
                throw new ServerException(
                    result.RequestMessage.RequestUri.ToString(),
                    result.RequestMessage.Method.ToString(),
                    result.RequestMessage.Content?.ToString(),
                    (int)result.StatusCode,
                    result.Content.ToString());
            }
        }

        public async Task PostRegisterClient(string type)
        {
            _logger.Debug("requesting post client: " + _uploadClientId);
            var result = await ExecuteAsync(() =>
            {
                var requestUri = new UriBuilder(_serverUri)
                {
                    Path = $"{_defaultPath}clients"
                };

                var keyValue = new Dictionary<string, string>
                {
                    { "clientId", _uploadClientId },
                    { "type", type }
                };

                var request = new HttpRequestMessage(HttpMethod.Post, requestUri.Uri)
                {
                    Content = new FormUrlEncodedContent(keyValue)
                };
                return request;
            }).ConfigureAwait(false);
            ThrowIfNotSuccessfulStatus(result);
        }

        public async Task<ClientInfoResponse> GetClientInfoAsync()
        {
            _logger.Debug("requesting get client info: " + _uploadClientId);
            var result = await ExecuteAsync(() =>
            {
                var requestUri = new UriBuilder(_serverUri)
                {
                    Path = $"{_defaultPath}clients/{_uploadClientId}"
                };
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri.Uri);
                return request;
            }).ConfigureAwait(false);
            ThrowIfNotSuccessfulStatus(result);
            return await ParseJson<ClientInfoResponse>(result).ConfigureAwait(false);
        }

        public async Task<ClientInfoRequstResponse> GetLatestRequest()
        {
            _logger.Debug("requesting get request for client: " + _uploadClientId);
            var result = await ExecuteAsync(() =>
            {
                var requestUri = new UriBuilder(_serverUri)
                {
                    Path = $"{_defaultPath}clients/{_uploadClientId}"
                };
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri.Uri);
                return request;
            }).ConfigureAwait(false);
            ThrowIfNotSuccessfulStatus(result);
            return await ParseJson<ClientInfoRequstResponse>(result).ConfigureAwait(false);
        }

        public async Task PutAccepted(string requestId)
        {
            _logger.Debug("requesting put accept client: " + _uploadClientId + ", request: " + requestId);
            var result = await ExecuteAsync(() =>
            {
                var requestUri = new UriBuilder(_serverUri)
                {
                    Path = $"{_defaultPath}clients/{_uploadClientId}/requests/{requestId}/accept"
                };
                var request = new HttpRequestMessage(HttpMethod.Put, requestUri.Uri);
                return request;
            }).ConfigureAwait(false);
            ThrowIfNotSuccessfulStatus(result);
        }

        public async Task PutStatus(string status)
        {
            _logger.Debug("requesting put status for client: " + _uploadClientId);
            var result = await ExecuteAsync(() =>
            {
                var requestUri = new UriBuilder(_serverUri)
                {
                    Path = $"{_defaultPath}clients/{_uploadClientId}/status"
                };

                var keyValuePair = new Dictionary<string, string>
                {
                    { "status",  status }
                };

                var request = new HttpRequestMessage(HttpMethod.Put, requestUri.Uri)
                {
                    Content = new FormUrlEncodedContent(keyValuePair)
                };
                return request;
            }).ConfigureAwait(false);
            ThrowIfNotSuccessfulStatus(result);
        }

        public async Task<IEnumerable<Entry>> GetEntriesAsync(string path)
        {
            _logger.Debug("requesting get entries for path: " + path);
            var result = await ExecuteAsync(() =>
            {
                var query = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(path))
                {
                    query["path"] = path;
                }

                var requestUri = new UriBuilder(_serverUri)
                {
                    Path = $"{_defaultPath}files",
                    Query = ToQueryString(query)
                };
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri.Uri);
                return request;
            }).ConfigureAwait(false);
            ThrowIfNotSuccessfulStatus(result);
            var container = await ParseJson<ContainerResponse>(result).ConfigureAwait(false);
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
                var query = new Dictionary<string, string>
                {
                    { "path", path ?? throw new ArgumentNullException(nameof(path)) }
                };

                var requestUri = new UriBuilder(_serverUri)
                {
                    Path = $"{_defaultPath}files",
                    Query = ToQueryString(query)
                };
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri.Uri);
                return request;
            }).ConfigureAwait(false);
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
            else
            {
                ThrowIfNotSuccessfulStatus(result);
                return new Document(await ParseJson<DocumentResponse>(result).ConfigureAwait(false), this);
            }
        }

        public async Task<Stream> DownloadAsync(string path)
        {
            _logger.Debug("requesting downloading for path: " + path);
            var result = await ExecuteAsync(() =>
            {
                var query = new Dictionary<string, string>
                {
                    { "path" , path ?? throw new ArgumentNullException(nameof(path)) },
                    { "download", true.ToString() }
                };

                var requestUri = new UriBuilder(_serverUri)
                {
                    Path = $"{_defaultPath}files",
                    Query = ToQueryString(query)
                };
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri.Uri);
                return request;
            }).ConfigureAwait(false);
            ThrowIfNotSuccessfulStatus(result);
            var downloadResponse = new DownloadResponse();
            downloadResponse.WriteResponse(await result.Content.ReadAsStreamAsync().ConfigureAwait(false));
            return downloadResponse;
        }

        public async Task CreateOrUpdateDocumentAsync(string path, Func<Stream> streamFactory, DateTime? created = null, DateTime? lastModified = null)
        {
            _logger.Debug("requesting uploading for path: " + path);
            using (var stream = streamFactory.Invoke())
            {
                var result = await ExecuteAsync(() =>
                {
                    var requestUri = new UriBuilder(_serverUri)
                    {
                        Path = $"{_defaultPath}files"
                    };

                    var content = new MultipartFormDataContent
                    {
                        { new StringContent(path ?? throw new ArgumentNullException(nameof(path))), "path" }
                    };
                    stream.Seek(0, SeekOrigin.Begin);
                    content.Add(new StreamContent(stream), "file", path.Substring(path.LastIndexOf('/') > -1 ? path.LastIndexOf('/') : 0));
                    if (created != null)
                    {
                        content.Add(new StringContent(XmlConvert.ToString(created.Value, XmlDateTimeSerializationMode.Utc)), "created");
                    }
                    if (lastModified != null)
                    {
                        content.Add(new StringContent(XmlConvert.ToString(lastModified.Value, XmlDateTimeSerializationMode.Utc)), "lastModified");
                    }

                    var request = new HttpRequestMessage(HttpMethod.Post, requestUri.Uri)
                    {
                        Content = content
                    };
                    return request;
                }).ConfigureAwait(false);
                ThrowIfNotSuccessfulStatus(result);
            }
        }

        public async Task MoveDocumentAsync(string originalPath, string destinationPath)
        {
            _logger.Debug("requesting move for original path: " + originalPath + " to destination path: " + destinationPath);
            var result = await ExecuteAsync(() =>
            {
                var requestUri = new UriBuilder(_serverUri)
                {
                    Path = $"{_defaultPath}files/move"
                };

                var keyValue = new Dictionary<string, string>
                {
                    { "destinationPath", destinationPath },
                    { "originalPath", originalPath }
                };

                var request = new HttpRequestMessage(HttpMethod.Post, requestUri.Uri)
                {
                    Content = new FormUrlEncodedContent(keyValue)
                };
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
                    var query = new Dictionary<string, string>
                    {
                        { "path", path ?? throw new ArgumentNullException(nameof(path)) }
                    };

                    var requestUri = new UriBuilder(_serverUri)
                    {
                        Path = $"{_defaultPath}files",
                        Query = ToQueryString(query)
                    };

                    var request = new HttpRequestMessage(HttpMethod.Delete, requestUri.Uri);
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
                var query = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(path))
                {
                    query["path"] = path;
                }
                if (lastHistoryId != null)
                {
                    query["last_history_id"] = lastHistoryId.Value.ToString();
                }
                query["per_page"] = "1000";

                var requestUri = new UriBuilder(_serverUri)
                {
                    Path = $"{_defaultPath}histories",
                    Query = ToQueryString(query)
                };
                var request = new HttpRequestMessage(HttpMethod.Get, requestUri.Uri);
                return request;
            }).ConfigureAwait(false);
            ThrowIfNotSuccessfulStatus(result);

            var resultList = await ParseJson<List<History>>(result).ConfigureAwait(false);
            var pagination = PaginationHeader.Parse(result.Headers.FirstOrDefault(h => string.Equals(h.Key, "Link", StringComparison.InvariantCultureIgnoreCase)).Value?.FirstOrDefault());
            while (!string.IsNullOrEmpty(pagination?.Next))
            {
                result = await ExecuteAsync(() =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, pagination.Next);
                    return request;
                }).ConfigureAwait(false);
                ThrowIfNotSuccessfulStatus(result);
                resultList.AddRange(await ParseJson<List<History>>(result).ConfigureAwait(false));
                pagination = PaginationHeader.Parse(result.Headers.FirstOrDefault(h => string.Equals(h.Key, "Link", StringComparison.InvariantCultureIgnoreCase)).Value?.FirstOrDefault());
            }
            return resultList;
        }
    }
}
