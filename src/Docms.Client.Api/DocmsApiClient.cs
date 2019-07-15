using Docms.Client.Api.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Docms.Client.Api
{
    public class DocmsApiClient : IDisposable
    {
        private string _clientId;
        private string _defaultPath;
        private HttpClient _httpClient;
        private string _accessToken;
        private JsonSerializerSettings _serializerSettings;

        public DocmsApiClient(string clientId, string uri, string defaultPath = "api/v1")
        {
            _clientId = clientId;
            _defaultPath = defaultPath.EndsWith("/") ? defaultPath : defaultPath + "/";
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(uri);
            _serializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                }
            };
        }

        public async Task LoginAsync()
        {
            var tokenResponse = await _httpClient.PostAsync("/connect/token", new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", "docms-client"),
                new KeyValuePair<string, string>("client_secret", "docms-client-secret"),
            })).ConfigureAwait(false);
            if (!tokenResponse.IsSuccessStatusCode)
            {
                throw new UnauthorizedException();
            }
            var tokenResponseObj = JObject.Parse(await tokenResponse.Content.ReadAsStringAsync().ConfigureAwait(false));
            _accessToken = tokenResponseObj["access_token"].ToString();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        internal async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> func)
        {
            try
            {
                var response = await func.Invoke().ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await LoginAsync().ConfigureAwait(false);
                    response = await func.Invoke().ConfigureAwait(false);
                }
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    throw new ForbiddenException();
                }
                return response;
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException is SocketException)
                {
                    throw new ConnectionException(_httpClient.BaseAddress.ToString());
                }
                throw new DocmsApiException(ex);
            }
        }

        internal async Task<T> GetAsync<T>(string path, IEnumerable<KeyValuePair<string, string>> param)
        {
            var url = _defaultPath + path;
            if (url.Contains("?"))
            {
                url = url + "&" + string.Join("&", param.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            }
            else
            {
                url = url + "?" + string.Join("&", param.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            }
            var response = await SendAsync(() =>
            {
                return _httpClient.GetAsync(_defaultPath + path);
            }).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(responseString);
        }

        internal async Task PostAsync(string path, HttpContent content)
        {
            var response = await SendAsync(() =>
            {
                return _httpClient.PostAsync(_defaultPath + path, content);
            }).ConfigureAwait(false);
        }

        internal async Task DeleteAsync(string path, KeyValuePair<string, string>[] param)
        {
            var url = _defaultPath + path;
            if (url.Contains("?"))
            {
                url = url + "&" + string.Join("&", param.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            }
            else
            {
                url = url + "?" + string.Join("&", param.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
            }
            var response = await SendAsync(() =>
            {
                return _httpClient.DeleteAsync(_defaultPath + path);
            }).ConfigureAwait(false);
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public DocumentClient CreateDocumentClient()
        {
            return new DocumentClient(this);
        }

        public void Dispose()
        {
            if (_httpClient != null)
            {
                _httpClient.Dispose();
            }
        }
    }
}
