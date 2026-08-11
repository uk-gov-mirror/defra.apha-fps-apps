namespace Apha.PACT.Core.Pagination
{
    public class PagedData<T>
    {
        public IReadOnlyCollection<T> Data { get; }       
        public PaginationData PaginationData { get; }
        public decimal Total { get; set; } = 0;

        public PagedData(IReadOnlyCollection<T> items, PaginationData paginationData, decimal total = 0)
        {
            Data = items;           
            PaginationData = paginationData;
            Total = total;
        }
    }

    public class PaginationData
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; } 
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}
