using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PIMS.DataAccess.Repository
{
    public class ReportRepository : BaseRepository, IReportRepository
    {
        private readonly PimsDbContext _dbContext;

        public ReportRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Report>> GetAllReportsAsync()
        {
            return await _dbContext.Reports
                .AsNoTracking()
                .OrderBy(r => r.SortOrder)
                .ThenBy(r => r.ReportName)
                .ToListAsync();
        }

        public async Task<PagedData<Report>> GetPagedReportsAsync(PaginationParameters<string> query)
        {
            var baseQuery = _dbContext.Reports.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(query.Filter)
                    ?? new Dictionary<string, string>();

                if (filters.TryGetValue("ReportName", out var reportnameFilter)
                    && !string.IsNullOrWhiteSpace(reportnameFilter))
                {
                    var value = reportnameFilter.Trim();
                    baseQuery = baseQuery.Where(r => EF.Functions.ILike(r.ReportName, $"%{value}%"));
                }

                if (filters.TryGetValue("ReportDescription", out var descFilter)
                    && !string.IsNullOrWhiteSpace(descFilter))
                {
                    var value = descFilter.Trim();
                    baseQuery = baseQuery.Where(r => EF.Functions.ILike(r.ReportDescription ?? string.Empty, $"%{value}%"));
                }

                if (filters.TryGetValue("SortOrder", out var sortOrderFilter)
                    && int.TryParse(sortOrderFilter, out var sortOrderVal))
                {
                    baseQuery = baseQuery.Where(r => r.SortOrder == sortOrderVal);
                }

                if (filters.TryGetValue("Emailable", out var emailableFilter)
                    && bool.TryParse(emailableFilter, out var emailableVal))
                {
                    baseQuery = baseQuery.Where(r => r.Emailable == emailableVal);
                }
            }

            baseQuery = (query.SortBy, query.Descending) switch
            {
                ("ReportName", true)         => baseQuery.OrderByDescending(r => r.ReportName),
                ("ReportName", false)        => baseQuery.OrderBy(r => r.ReportName),
                ("ReportDescription", true)  => baseQuery.OrderByDescending(r => r.ReportDescription),
                ("ReportDescription", false) => baseQuery.OrderBy(r => r.ReportDescription),
                ("SortOrder", true)          => baseQuery.OrderByDescending(r => r.SortOrder),
                ("SortOrder", false)         => baseQuery.OrderBy(r => r.SortOrder),
                ("Emailable", true)          => baseQuery.OrderByDescending(r => r.Emailable),
                ("Emailable", false)         => baseQuery.OrderBy(r => r.Emailable),
                (_, true)                    => baseQuery.OrderByDescending(r => r.SortOrder).ThenByDescending(r => r.ReportName),
                _                            => baseQuery.OrderBy(r => r.SortOrder).ThenBy(r => r.ReportName)
            };

            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            return await ApplyPaging(baseQuery, page, pageSize);
        }

        public async Task<Report?> GetReportByIdAsync(int id)
        {
            return await _dbContext.Reports
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Report> AddReportAsync(Report entity)
        {
            var maxId = await _dbContext.Reports.MaxAsync(x => (int?)x.Id) ?? 0;
            entity.Id = maxId + 1;    

            _dbContext.Reports.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Report> UpdateReportAsync(Report entity)
        {
            _dbContext.Reports.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteReportAsync(int id)
        {
            int rowsAffected = await _dbContext.Reports
                .Where(r => r.Id == id)
                .ExecuteDeleteAsync();

            return rowsAffected > 0;
        }

        public async Task<bool> ReportExistsAsync(int id)
        {
            return await _dbContext.Reports
                .AnyAsync(r => r.Id == id);
        }
    }
}
