using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PIMS.DataAccess.Repository
{
    public class AccessSystemRepository : BaseRepository, IAccessSystemRepository
    {
        private readonly PimsDbContext _dbContext;

        public AccessSystemRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<AccessSystem>> GetAllAsync()
        {
            return await _dbContext.AccessSystems
                .AsNoTracking()
                .OrderBy(s => s.SystemId)
                .ToListAsync();
        }
        public async Task<AccessSystem?> GetByIdAsync(int systemid)
        {
            return await _dbContext.AccessSystems
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SystemId == systemid);
        }
        public async Task<bool> ExistsAsync(int systemid)
        {
            return await _dbContext.AccessSystems
                .AnyAsync(s => s.SystemId == systemid);
        }
    }
}
