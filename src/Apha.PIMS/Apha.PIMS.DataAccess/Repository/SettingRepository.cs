using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PIMS.DataAccess.Repository
{
    public class SettingRepository : BaseRepository, ISettingRepository
    {
        private readonly PimsDbContext _dbContext;

        public SettingRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<Settings>> GetAllSettingsAsync()
        {
            return await _dbContext.DatabaseSettings
                .AsNoTracking()
                .OrderBy(s => s.Id)
                .ToListAsync();
        }
        public async Task<List<Settings>> GetAllUserUpdateableSettingsAsync()
        {
            return await _dbContext.DatabaseSettings
                .AsNoTracking()
                .Where(s => s.Userupdateable == true)
                .OrderBy(s => s.Id)
                .ToListAsync();
        }
        public async Task<Settings?> GetSettingByIdAsync(string id)
        {
            return await _dbContext.DatabaseSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task<Settings> UpdateSettingAsync(Settings entity)
        {
            _dbContext.DatabaseSettings.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }
        public async Task<bool> SettingExistsAsync(string id)
        {
            return await _dbContext.DatabaseSettings
                .AnyAsync(s => s.Id == id);
        }
    }
}
