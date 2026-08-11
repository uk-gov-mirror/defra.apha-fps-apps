namespace Apha.PACT.Application.Pagination
{
    public class PaginatedResult<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
        public PaginationDto PaginationData { get; set; } = new PaginationDto(); // Initialize to avoid nullability issue
        public decimal Total { get; set; } = 0; // Initialize to avoid nullability issue

        public PaginatedResult() { }       

        public PaginatedResult(IEnumerable<T> items, PaginationDto paginationData, decimal total = 0)
        {
            Data = items;
            PaginationData = paginationData;
            Total = total;
        }
    }

    public class PaginationDto
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}
