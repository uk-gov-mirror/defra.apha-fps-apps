using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PIMS.DataAccess.Repository
{
    public class ReportGroupLinkRepository : BaseRepository, IReportGroupLinkRepository
    {
        private readonly PimsDbContext _dbContext;

        public ReportGroupLinkRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ReportGroupLink>> GetAllReportGroupLinksAsync()
        {
            return await _dbContext.ReportGroupLinks
                .AsNoTracking()
                .OrderBy(l => l.ReportId)
                .ThenBy(l => l.GroupId)
                .ToListAsync();
        }
        public async Task<List<ReportGroupLink>> GetReportGroupLinksByReportIdAsync(int reportId)
        {
            return await _dbContext.ReportGroupLinks
                .AsNoTracking()
                .Where(l => l.ReportId == reportId)
                .OrderBy(l => l.GroupId)
                .ToListAsync();
        }
        public async Task<ReportGroupLink?> GetReportGroupLinkByIdAsync(int reportId, int groupId)
        {
            return await _dbContext.ReportGroupLinks
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.ReportId == reportId && l.GroupId == groupId);
        }
        public async Task<ReportGroupLink> AddReportGroupLinkAsync(ReportGroupLink entity)
        {
            _dbContext.ReportGroupLinks.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> DeleteReportGroupLinkAsync(int reportId, int groupId)
        {
            int rowsAffected = await _dbContext.ReportGroupLinks
                .Where(l => l.ReportId == reportId && l.GroupId == groupId)
                .ExecuteDeleteAsync();

            return rowsAffected > 0;
        }
        public async Task<bool> ReportGroupLinkExistsAsync(int reportId, int groupId)
        {
            return await _dbContext.ReportGroupLinks
                .AnyAsync(l => l.ReportId == reportId && l.GroupId == groupId);
        }
    }
}
