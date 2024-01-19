using Newtonsoft.Json;
using SecureBank.Common;
using System;
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

        public async Task<APIResponse<TResponse>> SendAsync<TResponse>(APIMethodType type, string url)
        {
            return await SendRequestAsync<APIResponse<TResponse>>(type, url, null);
        }

        public async Task<APIResponse> SendAsync(APIMethodType type, string url)
        {
            return await SendRequestAsync<APIResponse>(type, url, null);
        }

        public async Task<APIResponse<TResponse>> SendAsync<TResponse, TBody>(APIMethodType type, string url, TBody body)
        {
            HttpContent content = PrepareBody(body);

            return await SendRequestAsync<APIResponse<TResponse>>(type, url, content);
        }

        public async Task<APIResponse> SendAsync<TBody>(APIMethodType type, string url, TBody body)
        {
            HttpContent content = PrepareBody(body);

            return await SendRequestAsync<APIResponse>(type, url, content);
        }

        #endregion



        #region PRIVATE METHODS

        private HttpContent PrepareBody<T>(T body)
        {
            string json = JsonConvert.SerializeObject(body);

            HttpContent content = new StringContent(json);
            content.Headers.ContentType.MediaType = "application/json";

            return content;
        }

        private async Task<T> SendRequestAsync<T>(APIMethodType type, string url, HttpContent? content)
        {
            try
            {
                HttpResponseMessage response = type switch
                {
                    APIMethodType.GET => await _httpClient.GetAsync(url),
                    APIMethodType.POST => await _httpClient.PostAsync(url, content),
                    APIMethodType.PUT => await _httpClient.PutAsync(url, content),
                    APIMethodType.DELETE => await _httpClient.DeleteAsync(url),
                    _ => throw new NotImplementedException()
                };

                string responseBodyString = await response.Content.ReadAsStringAsync();

                T? responseBodyObject = JsonConvert.DeserializeObject<T>(responseBodyString);

                if (responseBodyObject is null)
                {
                    throw new Exception($"Wrong response type. Response: {responseBodyString}; {response.StatusCode}");
                }

                return responseBodyObject;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        #endregion
    }
}
