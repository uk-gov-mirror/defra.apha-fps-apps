using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public class MonthRepository : BaseRepository, IMonthRepository
    {
        public MonthRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Month>> GetAllMonthsAsync()
        {
            return await _context.Months
                .AsNoTracking()
                .OrderBy(m => m.MonthNumber)
                .ToListAsync();
        }        
    }
}
