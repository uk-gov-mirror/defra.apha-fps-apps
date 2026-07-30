using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public class ProjectRepository : BaseRepository, IProjectRepository
    {
        public ProjectRepository(FpsDbContext context) : base(context) { }

        public async Task<bool> ExistsAsync(string parentProject)
        {
            return await _context.Projects
                .AsNoTracking()
                .AnyAsync(p => p.ParentProject.ToLower() == parentProject.ToLower());
        }
    }
}
