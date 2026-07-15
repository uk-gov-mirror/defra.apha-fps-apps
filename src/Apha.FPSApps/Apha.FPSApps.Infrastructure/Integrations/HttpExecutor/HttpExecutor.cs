using Apha.Common.Contracts;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Apha.FPSApps.Infrastructure.Integrations.HttpExecutor
{
    public class HttpExecutor : IHttpExecutor
    {
        private readonly HttpClient _http;
        public HttpExecutor(HttpClient http)
        {
            _http = http;
        }

        public async Task<ApiResponse<T>> GetAsync<T>(string url)
        {
            var response = await _http.GetAsync(url);
            return await response.ToApiResponse<T>();
        }        

        public async Task<byte[]> GetFileAsync(string url)
        {
            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<ApiResponse<T>> PostAsync<TRequest, T>(
            string url,
            TRequest body)
        {
            var response = await _http.PostAsJsonAsync(url, body);
            return await response.ToApiResponse<T>();
        }

        public async Task<ApiResponse<T>> PutAsync<TRequest, T>(
            string url,
            TRequest body)
        {
            var response = await _http.PutAsJsonAsync(url, body);
            return await response.ToApiResponse<T>();
        }

        public async Task<ApiResponse<T>> PatchAsync<TRequest, T>(
            string url,
            TRequest body)
        {
            var response = await _http.PatchAsJsonAsync(url, body);
            return await response.ToApiResponse<T>();
        }

        public async Task<ApiResponse<T>> DeleteAsync<T>(string url)
        {
            var response = await _http.DeleteAsync(url);
            return await response.ToApiResponse<T>();
        }

        public async Task<ApiResponse<T>> DeleteAsync<TRequest, T>(string url, TRequest body)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json")
            };
            var response = await _http.SendAsync(request);
            return await response.ToApiResponse<T>();
        }

        public async Task<ApiResponse<T>> PostMultipartAsync<T>(string url, MultipartFormDataContent content)
        {
            var response = await _http.PostAsync(url, content);
            return await response.ToApiResponse<T>();
        }
    }
}
