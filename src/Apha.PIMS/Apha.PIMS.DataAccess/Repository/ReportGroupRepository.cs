using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PIMS.DataAccess.Repository
{
    public class ReportGroupRepository : BaseRepository, IReportGroupRepository
    {
        private readonly PimsDbContext _dbContext;

        public ReportGroupRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<ReportGroup>> GetAllReportGroupsAsync()
        {
            return await _dbContext.ReportGroups
                .AsNoTracking()
                .OrderBy(g => g.Description)
                .ToListAsync();
        }
        public async Task<PagedData<ReportGroup>> GetPagedReportGroupsAsync(PaginationParameters<string> query, int? reportId = null)
        {
            var baseQuery = _dbContext.ReportGroups.AsNoTracking();

            if (reportId.HasValue)
            {
                var linkedGroupIds = _dbContext.ReportGroupLinks
                    .Where(l => l.ReportId == reportId.Value)
                    .Select(l => l.GroupId);
                baseQuery = baseQuery.Where(g => linkedGroupIds.Contains(g.GroupId));
            }

            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(query.Filter)
                    ?? new Dictionary<string, string>();

                if (filters.TryGetValue("GroupId", out var groupIdFilter)
                    && int.TryParse(groupIdFilter, out var groupId))
                {
                    baseQuery = baseQuery.Where(g => g.GroupId == groupId);
                }

                if (filters.TryGetValue("Description", out var descriptionFilter)
                    && !string.IsNullOrWhiteSpace(descriptionFilter))
                {
                    var value = descriptionFilter.Trim();
                    baseQuery = baseQuery.Where(g =>
                        EF.Functions.ILike(g.Description, $"%{value}%"));
                }
            }

            baseQuery = (query.SortBy, query.Descending) switch
            {
                ("GroupId", true) => baseQuery.OrderByDescending(g => g.GroupId),
                ("GroupId", false) => baseQuery.OrderBy(g => g.GroupId),
                ("Description", true) => baseQuery.OrderByDescending(g => g.Description),
                ("Description", false) => baseQuery.OrderBy(g => g.Description),
                (_, true) => baseQuery.OrderByDescending(g => g.Description),
                _ => baseQuery.OrderBy(g => g.Description)
            };

            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            return await ApplyPaging(baseQuery, page, pageSize);
        }
        // via tblreportgroup_link (inner join on groupid); ordered by description
        public async Task<List<ReportGroup>> GetReportGroupsByReportIdAsync(int reportId)
        {
            var linkedGroupIds = _dbContext.ReportGroupLinks
                .Where(l => l.ReportId == reportId)
                .Select(l => l.GroupId);

            return await _dbContext.ReportGroups
                .AsNoTracking()
                .Where(g => linkedGroupIds.Contains(g.GroupId))
                .OrderBy(g => g.Description)
                .ToListAsync();
        }

        public async Task<ReportGroup?> GetReportGroupByIdAsync(int groupId)
        {
            return await _dbContext.ReportGroups
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.GroupId == groupId);
        }
        public async Task<ReportGroup> AddReportGroupAsync(ReportGroup entity)
        {
            _dbContext.ReportGroups.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<ReportGroup> UpdateReportGroupAsync(ReportGroup entity)
        {
            _dbContext.ReportGroups.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> DeleteReportGroupAsync(int groupId)
        {
            int rowsAffected = await _dbContext.ReportGroups
                .Where(g => g.GroupId == groupId)
                .ExecuteDeleteAsync();

            return rowsAffected > 0;
        }
        public async Task<bool> ReportGroupExistsAsync(int groupId)
        {
            return await _dbContext.ReportGroups
                .AnyAsync(g => g.GroupId == groupId);
        }

        public async Task<bool> HasLinkedReportsAsync(int groupId)
        {
            return await _dbContext.ReportGroupLinks
                .AnyAsync(l => l.GroupId == groupId);
        }
    }
}
