using Apha.Common.Contracts;

namespace Apha.FPSApps.Infrastructure.Integrations.HttpExecutor
{
    public interface IHttpExecutor
    {
        Task<ApiResponse<T>> GetAsync<T>(string url);        

        Task<byte[]> GetFileAsync(string url);

        Task<ApiResponse<T>> PostAsync<TRequest, T>(
            string url,
            TRequest body);

        Task<ApiResponse<T>> PutAsync<TRequest, T>(
            string url,
            TRequest body);

        Task<ApiResponse<T>> PatchAsync<TRequest, T>(
            string url,
            TRequest body);

        Task<ApiResponse<T>> DeleteAsync<T>(string url);

        Task<ApiResponse<T>> DeleteAsync<TRequest, T>(string url, TRequest body);

        Task<ApiResponse<T>> PostMultipartAsync<T>(string url, MultipartFormDataContent content);
    }
}
