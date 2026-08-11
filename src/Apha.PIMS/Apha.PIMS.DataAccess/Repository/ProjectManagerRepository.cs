using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PIMS.DataAccess.Repository
{
    public class ProjectManagerRepository : BaseRepository, IProjectManagerRepository
    {
        private readonly PimsDbContext _dbContext;

        public ProjectManagerRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ProjectManager>> GetAllProjectManagersAsync()
        {
            return await _dbContext.ProjectManagers
                .AsNoTracking()
                .OrderBy(m => m.Projectmanager)
                .ToListAsync();
        }

        public async Task<PagedData<ProjectManager>> GetPagedProjectManagersAsync(PaginationParameters<string>? query = null)
        {
            query ??= new PaginationParameters<string>();

            var baseQuery = _dbContext.ProjectManagers.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(query.Filter)
                    ?? new Dictionary<string, string>();

                if (filters.TryGetValue("Projectmanager", out var managerFilter)
                    && !string.IsNullOrWhiteSpace(managerFilter))
                {
                    var value = managerFilter.Trim();
                    baseQuery = baseQuery.Where(m => EF.Functions.ILike(m.Projectmanager, $"%{value}%"));
                }

                if (filters.TryGetValue("Email", out var emailFilter)
                    && !string.IsNullOrWhiteSpace(emailFilter))
                {
                    var value = emailFilter.Trim();
                    baseQuery = baseQuery.Where(m => m.Email != null && EF.Functions.ILike(m.Email, $"%{value}%"));
                }

                if (filters.TryGetValue("Mnumber", out var mnumberFilter)
                    && !string.IsNullOrWhiteSpace(mnumberFilter))
                {
                    var value = mnumberFilter.Trim();
                    baseQuery = baseQuery.Where(m => m.Mnumber != null && EF.Functions.ILike(m.Mnumber, $"%{value}%"));
                }

                if (filters.TryGetValue("LoginEmail", out var loginEmailFilter)
                    && !string.IsNullOrWhiteSpace(loginEmailFilter))
                {
                    var value = loginEmailFilter.Trim();
                    baseQuery = baseQuery.Where(m => m.LoginEmail != null && EF.Functions.ILike(m.LoginEmail, $"%{value}%"));
                }
            }

            baseQuery = (query.SortBy, query.Descending) switch
            {
                ("Projectmanager", true)  => baseQuery.OrderByDescending(m => m.Projectmanager),
                ("Projectmanager", false) => baseQuery.OrderBy(m => m.Projectmanager),
                ("Email", true)  => baseQuery.OrderByDescending(m => m.Email).ThenBy(m => m.Projectmanager),
                ("Email", false) => baseQuery.OrderBy(m => m.Email).ThenBy(m => m.Projectmanager),
                ("Mnumber", true)  => baseQuery.OrderByDescending(m => m.Mnumber).ThenBy(m => m.Projectmanager),
                ("Mnumber", false) => baseQuery.OrderBy(m => m.Mnumber).ThenBy(m => m.Projectmanager),
                ("LoginEmail", true)  => baseQuery.OrderByDescending(m => m.LoginEmail).ThenBy(m => m.Projectmanager),
                ("LoginEmail", false) => baseQuery.OrderBy(m => m.LoginEmail).ThenBy(m => m.Projectmanager),
                (_, true)                  => baseQuery.OrderByDescending(m => m.Projectmanager),
                _                          => baseQuery.OrderBy(m => m.Projectmanager)
            };

            var page = query.Page > 0 ? query.Page : 1;
            var pageSize = query.PageSize > 0 ? query.PageSize : 10;
            return await ApplyPaging(baseQuery, page, pageSize);
        }

        public async Task<List<string>> GetManagerNamesAsync()
        {
            var query =
                from p in _dbContext.MyTlkpProjects.AsNoTracking()
                join r in _dbContext.RadtrackProgs.AsNoTracking()
                    on p.Program equals r.Program
                where p.Manager != null && p.Manager != ""
                orderby p.Manager
                select p.Manager!;

            return await query
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();
        }

        public async Task<ProjectManager?> GetProjectManagerByNameAsync(string projectManagerName)
        {
            return await _dbContext.ProjectManagers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Projectmanager == projectManagerName);
        }

        public async Task<ProjectManager> AddProjectManagerAsync(ProjectManager entity)
        {
            _dbContext.ProjectManagers.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<ProjectManager> UpdateProjectManagerAsync(ProjectManager entity)
        {
            _dbContext.ProjectManagers.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteProjectManagerAsync(string projectManagerName)
        {
            var rows = await _dbContext.ProjectManagers
                .Where(m => m.Projectmanager == projectManagerName)
                .ExecuteDeleteAsync();

            return rows > 0;
        }

        public async Task<bool> ProjectManagerExistsAsync(string projectManagerName)
        {
            return await _dbContext.ProjectManagers
                .AnyAsync(m => m.Projectmanager == projectManagerName);
        }
    }
}
