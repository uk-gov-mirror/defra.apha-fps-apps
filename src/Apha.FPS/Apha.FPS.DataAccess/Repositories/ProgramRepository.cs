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

        public async Task<PagedData<ProgramPlanCostView>> GetProgramTimeSnapshotAsync(PaginationParameters<string> query)
        {
            var version = "Plan - " + DateTime.Now.ToString("dd/MM/yyyy");
            var excludedPrograms = new[] { "ZT_prog", "ZT_leave", "Pend_work" };

            var planQuery =
                (from prg in _dbContext.Programs
                 join prj in _dbContext.Projects on prg.ProgramNo equals prj.Program
                 join sj in _dbContext.StaffJobs on prj.ParentProject equals sj.JobCode
                 join stf in _dbContext.StaffGeneralViews on sj.StaffId equals stf.StaffId
                 join wgg in _dbContext.WorkgroupGradeGeneralViews on stf.WorkGroupGrade equals wgg.WgGrade
                 join pcg in _dbContext.ProfitCentreGradeViews on wgg.ProfitCentreGrade equals pcg.PcGrade
                 select new ProgramPlanCostView
                 {
                     Version = version,
                     Directorate = prg.Directorate,
                     Program = prj.Program,
                     Customer = prj.Customer,
                     Contract = prj.Contract,
                     Project = prj.ParentProject,
                     Status = prj.ProjectStatus,
                     ResourceCentre = pcg.ProfitCentre,
                     WorkGroup = wgg.WorkGroup,
                     GradeCode = wgg.GradeCode,
                     Name = stf.Name,
                     Hours = sj.PlannedHours,
                     HoursCost = excludedPrograms.Contains(prj.Program)
                         ? 0m
                         : (decimal)sj.PlannedHours * (pcg.ChargeRate ?? 0)
                 }).Distinct();

            planQuery = ApplyProgramPlanCostFilter(planQuery, query.Filter);
            planQuery = (IQueryable<ProgramPlanCostView>)ApplyProgramPlanCostSorting(planQuery, query.SortBy, query.Descending);

            return await base.ApplyPagingAsync(planQuery, query.Page, query.PageSize);
        }

        private static IQueryable ApplyProgramPlanCostSorting(IQueryable<ProgramPlanCostView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return descending ? query.OrderByDescending(x => x.HoursCost) : query.OrderBy(x => x.HoursCost);
            }

            return ApplyProgramPlanCostSortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplyProgramPlanCostSortingByProperty(IQueryable<ProgramPlanCostView> query, string property, bool descending)
        {
            return property switch
            {
                "version" => ApplyPlanCostOrder(query, i => i.Version, descending),
                "directorate" => ApplyPlanCostOrder(query, i => i.Directorate, descending),
                "program" => ApplyPlanCostOrder(query, i => i.Program, descending),
                "customer" => ApplyPlanCostOrder(query, i => i.Customer, descending),
                "contract" => ApplyPlanCostOrder(query, i => i.Contract, descending),
                "project" => ApplyPlanCostOrder(query, i => i.Project, descending),
                "status" => ApplyPlanCostOrder(query, i => i.Status, descending),
                "resourcecentre" => ApplyPlanCostOrder(query, i => i.ResourceCentre, descending),
                "workgroup" => ApplyPlanCostOrder(query, i => i.WorkGroup, descending),
                "gradecode" => ApplyPlanCostOrder(query, i => i.GradeCode, descending),
                "name" => ApplyPlanCostOrder(query, i => i.Name, descending),
                "hours" => ApplyPlanCostOrder(query, i => i.Hours, descending),
                "hourscost" => ApplyPlanCostOrder(query, i => i.HoursCost, descending),
                _ => query
            };
        }

        private static IQueryable ApplyPlanCostOrder<T>(IQueryable<ProgramPlanCostView> query, Expression<Func<ProgramPlanCostView, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private static IQueryable<ProgramPlanCostView> ApplyProgramPlanCostFilter(IQueryable<ProgramPlanCostView> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            query = ApplyILikeFilter(dict, "Version", query, (q, v) => q.Where(x => EF.Functions.ILike(x.Version!, v)));
            query = ApplyILikeFilter(dict, "Directorate", query, (q, v) => q.Where(x => EF.Functions.ILike(x.Directorate!, v)));
            query = ApplyILikeFilter(dict, "Program", query, (q, v) => q.Where(x => EF.Functions.ILike(x.Program!, v)));
            query = ApplyILikeFilter(dict, "Customer", query, (q, v) => q.Where(x => EF.Functions.ILike(x.Customer!, v)));
            query = ApplyILikeFilter(dict, "Contract", query, (q, v) => q.Where(x => EF.Functions.ILike(x.Contract!, v)));
            query = ApplyILikeFilter(dict, "Project", query, (q, v) => q.Where(x => EF.Functions.ILike(x.Project!, v)));
            query = ApplyILikeFilter(dict, "Status", query, (q, v) => q.Where(x => EF.Functions.ILike(x.Status!, v)));
            query = ApplyILikeFilter(dict, "ResourceCentre", query, (q, v) => q.Where(x => EF.Functions.ILike(x.ResourceCentre!, v)));
            query = ApplyILikeFilter(dict, "WorkGroup", query, (q, v) => q.Where(x => EF.Functions.ILike(x.WorkGroup!, v)));
            query = ApplyILikeFilter(dict, "GradeCode", query, (q, v) => q.Where(x => EF.Functions.ILike(x.GradeCode!, v)));
            query = ApplyILikeFilter(dict, "Name", query, (q, v) => q.Where(x => EF.Functions.ILike(x.Name!, v)));

            return query;
        }

        private static IQueryable<ProgramPlanCostView> ApplyILikeFilter(
            IDictionary<string, object> dict,
            string key,
            IQueryable<ProgramPlanCostView> query,
            Func<IQueryable<ProgramPlanCostView>, string, IQueryable<ProgramPlanCostView>> applyWhere)
        {
            if (dict.TryGetValue(key, out var value) && value != null)
                return applyWhere(query, $"%{value}%");

            return query;
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
