using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public abstract class BaseRepository
    {
        protected readonly FpsDbContext _context;

        protected BaseRepository(FpsDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected static async Task<PagedData<T>> ApplyPaging<T>(IQueryable<T> source, int page, int pageSize)
        {
            var totalRecords = await source.CountAsync();
            var result = await source
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagination = new PaginationData
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                TotalRecords = totalRecords
            };

            return new PagedData<T>(result.AsReadOnly(), pagination);
        }

        protected static PagedData<T> ApplyPagingInMemory<T>(List<T> data, int page, int pageSize)
        {
            int totalRecords = data.Count;
            List<T> paged = data
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagination = new PaginationData
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                TotalRecords = totalRecords
            };

            return new PagedData<T>(paged.AsReadOnly(), pagination);
        }
    }
}
