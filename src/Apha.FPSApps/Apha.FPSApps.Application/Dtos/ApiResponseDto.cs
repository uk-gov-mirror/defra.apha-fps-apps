namespace Apha.FPSApps.Application.Dtos
{
    public class ApiResponseDto<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public PaginationDto? Pagination { get; set; }
        public decimal Total { get; set; } = 0;
        public List<ApiErrorDto>? Errors { get; set; } = new();
        public ApiMetaDto Meta { get; set; } = new();

        public static ApiResponseDto<T> SuccessResponse(T data, PaginationDto? pagination = null, decimal total = 0)
        {
            return new ApiResponseDto<T>
            {
                Success = true,
                Data = data,
                Pagination = pagination,
                Total = total,
                Meta = new ApiMetaDto
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    TimestampUtc = DateTime.UtcNow
                }
            };
        }

        public static ApiResponseDto<T> FailureResponse(List<ApiErrorDto>? errors, ApiMetaDto meta)
        {
            return new ApiResponseDto<T>
            {
                Success = false,
                Errors = errors,
                Meta = new ApiMetaDto
                {
                    CorrelationId = meta.CorrelationId,
                    TimestampUtc = DateTime.UtcNow
                }
            };
        }

        public static ApiResponseDto<T> ValidationFailure(
        string message,
        Dictionary<string, string[]> validationErrors)
        {
            return new ApiResponseDto<T>
            {
                Success = false,
                Errors = new List<ApiErrorDto> {
                    new ApiErrorDto
                    {
                        Code = "VALIDATION_ERROR",
                        Message = message,
                        Details = validationErrors
                    } 
                },
                Meta = new ApiMetaDto
                {
                    CorrelationId = Guid.NewGuid().ToString(),
                    TimestampUtc = DateTime.UtcNow
                }
            };
        }
    }

    public class ApiErrorDto
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Details { get; set; }
    }

    public class ApiMetaDto
    {
        public string CorrelationId { get; set; } = string.Empty;
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }
}
