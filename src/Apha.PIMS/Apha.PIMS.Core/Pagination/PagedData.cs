namespace Apha.PIMS.Core.Pagination
{
    
    public class PagedData<T>
    {
        public IReadOnlyCollection<T> Data { get; }       
        public PaginationData PaginationData { get; }

        public PagedData(IReadOnlyCollection<T> items, PaginationData paginationData)
        {
            Data = items;           
            PaginationData = paginationData;
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
