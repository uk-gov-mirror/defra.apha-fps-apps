namespace Apha.Common.Contracts
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public decimal Total { get; set; } = 0;
        public Pagination? Pagination { get; set; } = null;
        public List<ApiError>? Errors { get; set; } = new();
        public ApiMeta Meta { get; set; } = new();     

    }

    public class ApiError
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public object? Details { get; set; }
    }

    public class ApiMeta
    {
        public string CorrelationId { get; set; } = string.Empty;
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    }

    public class Pagination
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}

    
