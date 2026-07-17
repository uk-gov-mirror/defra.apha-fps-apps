using Apha.Common.Contracts;
using System.Net.Http.Json;
using System.Text.Json;

namespace Apha.FPSApps.Infrastructure.Integrations.HttpExecutor
{
    public static class HttpResponseExtensions
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static async Task<ApiResponse<T>> ToApiResponse<T>(
         this HttpResponseMessage response)
        {
            // Handle 404 Not Found - throw exception to let middleware handle it
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new KeyNotFoundException("The requested resource was not found.");
            }

            // ASP.NET Core's Ok(null) is auto-converted to 204 No Content by MVC's default
            // output formatter — the body is empty, so there is nothing to deserialize.
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
            {
                return new ApiResponse<T>
                {
                    Success = true,
                    Data = default,
                    Meta = new ApiMeta
                    {
                        CorrelationId = Guid.NewGuid().ToString(),
                        TimestampUtc = DateTime.UtcNow
                    }
                };
            }

            try
            {
                var apiResponse =
                    await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);

                if (apiResponse != null)
                    return apiResponse;
            }           
            catch (Exception ex)
            {                
                throw new InvalidOperationException("An unexpected error occurred while processing the response.", ex);
            }

            return new ApiResponse<T>
            {
                Success = false,
                Errors = new List<ApiError> { new ApiError { Code = response.StatusCode.ToString(), Message = response.ReasonPhrase! } },
                Meta = new ApiMeta
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    TimestampUtc = DateTime.UtcNow
                }
            };
        }
    }
}
