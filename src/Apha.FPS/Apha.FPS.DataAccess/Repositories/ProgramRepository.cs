using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;


namespace Apha.FPS.DataAccess.Repositories
{
    public class ProgramRepository : BaseRepository, IProgramRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public ProgramRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }
              
        public async Task<IEnumerable<Program>> GetAllProgramsAsync()
        {
            return await _dbContext.ProgramViews
                .Where(p => p.UserEmail != null && p.UserEmail.ToLower() == _requestContext.UserEmailId)
                .Select(p => new Program
                {
                    ProgramNo = p.ProgramNo ?? "",
                    ProgramName = p.ProgramName,
                    Directorate = p.Directorate,
                    Target = p.Target,
                    Manager = p.Manager
                }).OrderBy(p => p.ProgramNo).ToListAsync();
        }

        public async Task<IEnumerable<Program>> GetAllProgramsForAllUsers()
        {
            return await _dbContext.Programs
                .Select(p => new Program
                {
                    ProgramNo = p.ProgramNo,
                    ProgramName = p.ProgramName,
                    Directorate = p.Directorate,
                    Target = p.Target,
                    Manager = p.Manager
                }).OrderBy(p => p.ProgramNo).ToListAsync();
        }

        public async Task<PagedData<Program>> GetAllProgramsAsync(PaginationParameters<string> query)
        {

            var programQuery = _dbContext.ProgramViews
                .Where(p => p.UserEmail != null && p.UserEmail.ToLower() == _requestContext.UserEmailId)
                                .Select(p => new Program
                                {
                                    ProgramNo = p.ProgramNo ?? "",
                                    ProgramName = p.ProgramName,
                                    Directorate = p.Directorate,
                                    Target = p.Target,
                                    Manager = p.Manager
                                }).AsQueryable();
        

            programQuery = ApplyProgramFilter(programQuery, query.Filter);
            programQuery = (IQueryable<Program>)ApplySorting(programQuery, query.SortBy, query.Descending);

            var result = await programQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<Program?> GetProgramByIdAsync(string id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);

            return await _dbContext.Programs
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProgramNo == id);
        }

        public async Task<Program> AddProgramAsync(Program entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            entity.FpsYear = _requestContext.FpsYear;

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    _dbContext.Programs.Add(entity);

                    // ITrig: Add UserProgram for the requesting user
                    var requestingUser = await _dbContext.Users
                        .FirstOrDefaultAsync(u => u.UserEmail != null &&
                                                  u.UserEmail.ToLower() == _requestContext.UserEmailId);
                    if (requestingUser != null)
                    {
                        _dbContext.UserPrograms.Add(new UserProgram
                        {
                            ProgramNo = entity.ProgramNo,
                            UserID = requestingUser.UserId,
                            FpsYear = _requestContext.FpsYear
                        });
                    }

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return entity;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<Program> UpdateProgramAsync(Program entity, string originalProgramNo)
        {
            ArgumentNullException.ThrowIfNull(entity);
            entity.FpsYear = _requestContext.FpsYear;

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    // UTrig: Cascade ProgramNo PK change to related tables
                    if (entity.ProgramNo != originalProgramNo)
                    {
                        await _dbContext.Database.ExecuteSqlInterpolatedAsync(
                            $"UPDATE fps.tlkpprogram SET programno = {entity.ProgramNo} WHERE programno = {originalProgramNo}");

                        await _dbContext.UserPrograms
                            .IgnoreQueryFilters()
                            .Where(up => up.ProgramNo == originalProgramNo)
                            .ExecuteUpdateAsync(s => s.SetProperty(up => up.ProgramNo, entity.ProgramNo));

                        await _dbContext.Projects
                            .Where(p => p.Program == originalProgramNo)
                            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Program, entity.ProgramNo));
                    }

                    _dbContext.Programs.Update(entity);

                    // UTrig: Ensure UserProgram exists for the requesting user
                    var requestingUser = await _dbContext.Users
                        .FirstOrDefaultAsync(u => u.UserEmail != null &&
                                                  u.UserEmail.ToLower() == _requestContext.UserEmailId);
                    if (requestingUser != null)
                    {
                        var userProgramExists = await _dbContext.UserPrograms
                            .AnyAsync(up => up.ProgramNo == entity.ProgramNo && up.UserID == requestingUser.UserId);

                        if (!userProgramExists)
                        {
                            _dbContext.UserPrograms.Add(new UserProgram
                            {
                                ProgramNo = entity.ProgramNo,
                                UserID = requestingUser.UserId,
                                FpsYear = _requestContext.FpsYear
                            });
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return entity;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<bool> HasLinkedProjectsAsync(string programNo)
        {
            return await _dbContext.Projects
                .AnyAsync(p => p.Program == programNo);
        }

        public async Task<bool> DeleteProgramAsync(string id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    await _dbContext.UserPrograms
                        .Where(up => up.ProgramNo == id)
                        .ExecuteDeleteAsync();

                    var rowsAffected = await _dbContext.Programs
                        .Where(p => p.ProgramNo == id)
                        .ExecuteDeleteAsync();

                    await transaction.CommitAsync();
                    return rowsAffected > 0;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private static IQueryable ApplySorting(IQueryable<Program> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query;
            }

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<Program> query, string property, bool descending)
        {
            return property switch
            {
                "programno" => ApplyOrder(query, i => i.ProgramNo, descending),
                "programname" => ApplyOrder(query, i => i.ProgramName, descending),
                "directorate" => ApplyOrder(query, i => i.Directorate, descending),
                "target" => ApplyOrder(query, i => i.Target, descending),
                "manager" => ApplyOrder(query, i => i.Manager, descending),
                _ => query
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<Program> query, Expression<Func<Program, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private static IQueryable<Program> ApplyProgramFilter(IQueryable<Program> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("ProgramNo", out var programNo) && programNo != null)
                query = query.Where(x => EF.Functions.ILike(x.ProgramNo, $"%{programNo}%"));

            if (dict.TryGetValue("ProgramName", out var programName) && programName != null)
                query = query.Where(x => EF.Functions.ILike(x.ProgramName!, $"%{programName}%"));

            if (dict.TryGetValue("Directorate", out var directorate) && directorate != null)
                query = query.Where(x => EF.Functions.ILike(x.Directorate!, $"%{directorate}%"));

            if (dict.TryGetValue("Manager", out var manager) && manager != null)
                query = query.Where(x => EF.Functions.ILike(x.Manager!, $"%{manager}%"));

            return query;
        }

    }
}
