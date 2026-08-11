using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class TestsRequiredByWgRepository : BaseRepository, ITestsRequiredByWgRepository
    {
        public TestsRequiredByWgRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<List<TestsRequiredByWgView>> GetTestsRequiredByWgAsync(string? profitCentre)
        {
            var query = _context.TestsRequiredByWgViews
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(profitCentre))
            {
                var normalised = profitCentre.ToLower();
                query = query.Where(x => x.ProfitCentre != null && x.ProfitCentre.ToLower() == normalised);
            }

            return await query
                .OrderBy(x => x.ProfitCentre)
                .ThenBy(x => x.WorkGroup)
                .ThenBy(x => x.TestCode)
                .ToListAsync();
        }
    }
}
