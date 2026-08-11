using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class TestsRequiredByRcRepository : BaseRepository, ITestsRequiredByRcRepository
    {
        public TestsRequiredByRcRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<List<TestsRequiredByRcView>> GetTestsRequiredByRcAsync(string? profitCentre)
        {
            var query = _context.TestsRequiredByRcViews
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(profitCentre))
            {
                var normalised = profitCentre.ToLower();
                query = query.Where(x => x.ProfitCentre != null && x.ProfitCentre.ToLower() == normalised);
            }

            return await query
                .OrderBy(x => x.ProfitCentre)
                .ThenBy(x => x.TestCode)
                .ToListAsync();
        }
    }
}
