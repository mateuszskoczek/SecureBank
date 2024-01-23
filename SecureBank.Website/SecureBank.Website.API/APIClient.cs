using Newtonsoft.Json;
using SecureBank.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecureBank.Website.API
{
    public class APIClient
    {
        #region FIELDS

        private readonly HttpClient _httpClient;

        #endregion



        #region CONSTRUCTORS

        public APIClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #endregion



        #region PUBLIC METHODS

        public async Task<APIResponse<TResponse>> SendAsync<TResponse>(APIMethodType type, string url, Dictionary<string, string>? query = null)
        {
            url = AddQuery(url, query);
            return await SendRequestAndParseBodyAsync<TResponse>(type, url, null);
        }

        public async Task<APIResponse> SendAsync(APIMethodType type, string url, Dictionary<string, string>? query = null)
        {
            url = AddQuery(url, query);
            return await SendRequestAndParseBodyAsync(type, url, null);
        }

        public async Task<APIResponse<TResponse>> SendAsync<TResponse, TBody>(APIMethodType type, string url, TBody body, Dictionary<string, string>? query = null)
        {
            url = AddQuery(url, query);
            HttpContent content = PrepareBody(body);

            return await SendRequestAndParseBodyAsync<TResponse>(type, url, content);
        }

        public async Task<APIResponse> SendAsync<TBody>(APIMethodType type, string url, TBody body, Dictionary<string, string>? query = null)
        {
            url = AddQuery(url, query);
            HttpContent content = PrepareBody(body);

            return await SendRequestAndParseBodyAsync(type, url, content);
        }

        #endregion



        #region PRIVATE METHODS

        private string AddQuery(string url, Dictionary<string, string>? query)
        {
            if (query is not null && query.Count > 0)
            {
                Dictionary<string, string> queryNew = query.ToDictionary();
                StringBuilder sb = new StringBuilder(url);
                KeyValuePair<string, string> item = queryNew.ElementAt(0);
                queryNew.Remove(item.Key);
                sb.Append($"?{item.Key}={item.Value}");

                foreach (KeyValuePair<string, string> item2 in queryNew)
                {
                    sb.Append($"&{item2.Key}={item2.Value}");
                }
                return sb.ToString();
            }
            return url;
        }

        private HttpContent PrepareBody<T>(T body)
        {
            string json = JsonConvert.SerializeObject(body);

            HttpContent content = new StringContent(json);
            content.Headers.ContentType.MediaType = "application/json";

            return content;
        }

        private async Task<APIResponse> SendRequestAndParseBodyAsync(APIMethodType type, string url, HttpContent? content)
        {
            try
            {
                HttpResponseMessage response = await SendRequestAsync(type, url, content);
                
                string stringResponse = await response.Content.ReadAsStringAsync();

                APIResponse? responseBodyObject = JsonConvert.DeserializeObject<APIResponse>(stringResponse);

                if (responseBodyObject is null)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return new APIResponse
                        {
                            Status = ResponseStatus.Unauthorized,
                            Message = $"You do not have permission"
                        };
                    }
                    else
                    {
                        return new APIResponse
                        {
                            Status = ResponseStatus.BadRequest,
                            Message = $"Wrong response type. Response: {stringResponse}; {response.StatusCode}"
                        };
                    }
                }

                return responseBodyObject;
            }
            catch (Exception ex)
            {
                return new APIResponse
                {
                    Status = ResponseStatus.BadRequest,
                    Message = ex.Message
                };
            }
        }

        private async Task<APIResponse<T>> SendRequestAndParseBodyAsync<T>(APIMethodType type, string url, HttpContent? content)
        {
            try
            {
                HttpResponseMessage response = await SendRequestAsync(type, url, content);

                string stringResponse = await response.Content.ReadAsStringAsync();

                APIResponse<T>? responseBodyObject = JsonConvert.DeserializeObject<APIResponse<T>>(stringResponse);

                if (responseBodyObject is null)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        return new APIResponse<T>
                        {
                            Status = ResponseStatus.Unauthorized,
                            Message = $"You do not have permission"
                        };
                    }
                    else
                    {
                        return new APIResponse<T>
                        {
                            Status = ResponseStatus.BadRequest,
                            Message = $"Wrong response type. Response: {stringResponse}; {response.StatusCode}"
                        };
                    }
                }

                return responseBodyObject;
            }
            catch (Exception ex)
            {
                return new APIResponse<T>
                {
                    Status = ResponseStatus.BadRequest,
                    Message = ex.Message
                };
            }
        }

        private async Task<HttpResponseMessage> SendRequestAsync(APIMethodType type, string url, HttpContent? content)
        {
            return type switch
            {
                APIMethodType.GET => await _httpClient.GetAsync(url),
                APIMethodType.POST => await _httpClient.PostAsync(url, content),
                APIMethodType.PUT => await _httpClient.PutAsync(url, content),
                APIMethodType.PATCH => await _httpClient.PatchAsync(url, content),
                APIMethodType.DELETE => await _httpClient.DeleteAsync(url),
                _ => throw new NotImplementedException()
            };         
        }

        #endregion
    }
}
