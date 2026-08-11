namespace Apha.FPSApps.Application.Pagination
{
    public class PaginatedResult<T>
    {
        public IEnumerable<T> data { get; set; } = Enumerable.Empty<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public decimal Total { get; set; } = 0;

        public PaginatedResult() { }

        public PaginatedResult(IEnumerable<T> items, int totalCount, decimal total =0)
        {
            data = items;
            TotalCount = totalCount;
            Total = total;
        }

        public PaginatedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        {
            data = items;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
        }
    }
}
