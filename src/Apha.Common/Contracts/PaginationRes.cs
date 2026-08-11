using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Common.Contracts
{
    public class PaginationRes<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
        public Pagination PaginationData { get; set; } = new Pagination(); // Initialize to avoid nullability issue
        public decimal Total { get; set; } = 0;

        public PaginationRes() { }

        public PaginationRes(IEnumerable<T> items, Pagination paginationData, decimal total = 0)
        {
            Data = items;
            PaginationData = paginationData;
            Total = total;
        }
    }
}
