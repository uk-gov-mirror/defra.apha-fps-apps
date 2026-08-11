using Apha.Common.Contracts;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Api.Filters
{
    public class ApiResponseActionFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(
            ResultExecutingContext context,
            ResultExecutionDelegate next)
        {
            if (context.Result is not ObjectResult objectResult ||
                objectResult.Value is null)
            {
                await next();
                return;
            }

            var correlationId = GetCorrelationId(context);

            object wrappedResponse = IsPaginatedResult(objectResult.Value)
                ? CreatePaginatedResponse(objectResult.Value, correlationId)
                : CreateStandardResponse(objectResult.Value, correlationId);

            context.Result = new ObjectResult(wrappedResponse)
            {
                StatusCode = objectResult.StatusCode ?? StatusCodes.Status200OK
            };

            await next();
        }

        private static bool IsPaginatedResult(object value)
        {
            var type = value.GetType();

            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(PaginationRes<>);
        }

        private static object CreateStandardResponse(object value, string correlationId)
        {
            return new ApiResponse<object>
            {
                Success = true,
                Data = value,
                Errors = null,
                Meta = CreateMeta(correlationId)
            };
        }

        private static object CreatePaginatedResponse(object value, string correlationId)
        {
            dynamic paginated = value;

            return new ApiResponse<object>
            {
                Success = true,
                Data = paginated.Data,
                Total = paginated.Total,
                Pagination = paginated.PaginationData,
                Errors = null,
                Meta = CreateMeta(correlationId)
            };
        }

        private static ApiMeta CreateMeta(string correlationId)
        {
            return new ApiMeta
            {
                CorrelationId = correlationId,
                TimestampUtc = DateTime.UtcNow
            };
        }

        private static string GetCorrelationId(ResultExecutingContext context)
        {
            return context.HttpContext.Request.Headers["X-Correlation-ID"]
                   .ToString();
        }
    }
}
