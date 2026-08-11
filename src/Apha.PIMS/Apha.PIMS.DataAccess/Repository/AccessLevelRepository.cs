using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PIMS.DataAccess.Repository
{
    public class AccessLevelRepository : BaseRepository, IAccessLevelRepository
    {
        private readonly PimsDbContext _dbContext;

        public AccessLevelRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<AccessLevel>> GetAllAsync()
        {
            return await _dbContext.AccessLevels
                .AsNoTracking()
                .OrderBy(l => l.SystemId)
                .ThenBy(l => l.AccessLevelId)
                .ToListAsync();
        }

        public async Task<List<AccessLevel>> GetBySystemIdAsync(int systemid)
        {
            return await _dbContext.AccessLevels
                .AsNoTracking()
                .Where(l => l.SystemId == systemid)
                .OrderBy(l => l.AccessLevelId)
                .ToListAsync();
        }
        public async Task<AccessLevel?> GetByIdAsync(int systemid, int accesslevelid)
        {
            return await _dbContext.AccessLevels
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.SystemId == systemid && l.AccessLevelId == accesslevelid);
        }
        public async Task<AccessLevel> AddAsync(AccessLevel entity)
        {
            _dbContext.AccessLevels.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<AccessLevel> UpdateAsync(AccessLevel entity)
        {
            _dbContext.AccessLevels.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task DeleteAsync(int systemid, int accesslevelid)
        {
            await _dbContext.AccessLevels
                .Where(l => l.SystemId == systemid && l.AccessLevelId == accesslevelid)
                .ExecuteDeleteAsync();
        }
        public async Task<bool> ExistsAsync(int systemid, int accesslevelid)
        {
            return await _dbContext.AccessLevels
                .AnyAsync(l => l.SystemId == systemid && l.AccessLevelId == accesslevelid);
        }
    }
}
