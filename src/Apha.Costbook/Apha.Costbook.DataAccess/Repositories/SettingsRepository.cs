using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apha.Costbook.DataAccess.Repositories
{
    public class SettingsRepository : ISettingsRepository
    {
        private readonly CostbookDbContext _context;

        public SettingsRepository(CostbookDbContext context)
        {
            _context = context;
        }
        public async Task<string?> GetSettingValueByIdAsync(string id)
        {
            var result = await _context.DatabaseSettings
                .Where(s => s.Id == id)
                .Select(s => s.Setting)
                .FirstOrDefaultAsync();

            return result;
        }
        public async Task<List<Settings>> GetAllUserUpdatableAsync()
        {
            return await _context.DatabaseSettings
                .AsNoTracking()
                .OrderBy(s => s.Id)
                .ToListAsync();
        }
        
        public async Task<bool> UpdateMultipleAsync(Dictionary<string, string> settingsById)
        {
            if (settingsById == null || settingsById.Count == 0)
                return false;

            foreach (var kvp in settingsById)
            {
                var id = kvp.Key;
                var value = kvp.Value;
                
                await _context.DatabaseSettings
                    .Where(s => s.Id == id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.Setting, value));
            }

            return true;
        }
    }
}
