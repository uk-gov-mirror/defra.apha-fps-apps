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
    public class ProjectRepository : BaseRepository, IProjectRepository
    {
        private const string FilterKeyParentProject = "ParentProject";

        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public ProjectRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

        public async Task<IEnumerable<ProjectView>> GetAllProjectsAsync()
        {
            return await _dbContext.ProjectViews
                .Where(p => EF.Functions.ILike(p.UserEmail!, _requestContext.UserEmailId)).ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetAllProjectsForAllUsersAsync()
        {
            return await _dbContext.Projects
                .ToListAsync();
        }

        public async Task<IEnumerable<PactProjectView>> GetAllPactProjectsAsync()
        {
            return await _dbContext.PactProjectViews
                .AsNoTracking()
                .OrderBy(p => p.ParentProject)
                .ToListAsync();
        }

        public async Task<PagedData<Project>> GetProjectsByProjectGroupAsync(PaginationParameters<string> query, string projectGroup)
        {
            var projectQuery = (from pg in _dbContext.ProjectGroupViews
                                join pv in _dbContext.Projects on
                                new { pg.ProjectGroupName } equals new { ProjectGroupName = pv.ProjectGroup }
                                where EF.Functions.ILike(pg.UserEmail!, _requestContext.UserEmailId) && pg.ProjectGroupName == projectGroup
                                select (new Project
                                {
                                    ParentProject = pv.ParentProject ?? string.Empty,
                                    ProjectTitle = pv.ProjectTitle ?? string.Empty,
                                    Program = pv.Program ?? string.Empty,
                                    Manager = pv.Manager,
                                    Customer = pv.Customer ?? string.Empty,
                                    Contract = pv.Contract ?? string.Empty,
                                    Disease = pv.Disease ?? string.Empty,
                                    ProjectStatus = pv.ProjectStatus ?? string.Empty,
                                    ProjectGroup = pv.ProjectGroup,
                                    BudgetCvl = pv.BudgetCvl,
                                    CustIncome = pv.CustIncome,
                                    TransferIncome = pv.TransferIncome,
                                    PlanCaseWorkDebit = pv.PlanCaseWorkDebit,
                                    IsDefraProject = pv.IsDefraProject,
                                    IncomeAccountCode = pv.IncomeAccountCode ?? string.Empty
                                })).AsQueryable();

            projectQuery = ApplyProjectFilter(projectQuery, query.Filter);
            projectQuery = (IQueryable<Project>)ApplySorting(projectQuery, query.SortBy, query.Descending);

            var result = await projectQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<PagedData<Project>> GetProjectsByProjectGroupProjectProfitabilityVLAAsync(PaginationParameters<string> query, string projectGroup)
        {
            var projectQuery = (from pg in _dbContext.ProjectGroupViews
                               join pv in _dbContext.Projects on
                               new { pg.ProjectGroupName } equals new { ProjectGroupName = pv.ProjectGroup }
                               where EF.Functions.ILike(pg.UserEmail!, _requestContext.UserEmailId) && pg.ProjectGroupName == projectGroup
                               select pv).AsQueryable();

            projectQuery = ApplyProjectFilter(projectQuery, query.Filter);
            projectQuery = (IQueryable<Project>)ApplySorting(projectQuery, query.SortBy, query.Descending);

            var result = await projectQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<PagedData<Project>> GetProjectsByProgramAsync(PaginationParameters<string> query, string programNo)
        {
            var projectQuery = _dbContext.ProjectViews
                .AsNoTracking()
                .Where(p => EF.Functions.ILike(p.UserEmail!, _requestContext.UserEmailId) && p.Program == programNo)
                .Select(pv => new Project
                {
                    ParentProject = pv.ParentProject ?? string.Empty,
                    ProjectTitle = pv.ProjectTitle ?? string.Empty,
                    Program = pv.Program ?? string.Empty,
                    Manager = pv.Manager,
                    Customer = pv.Customer ?? string.Empty,
                    Contract = pv.Contract ?? string.Empty,
                    Disease = pv.Disease ?? string.Empty,
                    ProjectStatus = pv.ProjectStatus ?? string.Empty,
                    ProjectGroup = pv.ProjectGroup,
                    BudgetCvl = pv.BudgetCvl,
                    CustIncome = pv.CustIncome ?? 0,
                    TransferIncome = pv.TransferIncome ?? 0,
                    PlanCaseWorkDebit = pv.PlanCaseWorkDebit,
                    IsDefraProject = pv.IsDefraProject ?? 0,
                    IncomeAccountCode = pv.IncomeAccountCode ?? string.Empty
                }).AsQueryable();

            projectQuery = ApplyProjectFilter(projectQuery, query.Filter);
            projectQuery = (IQueryable<Project>)ApplySorting(projectQuery, query.SortBy, query.Descending);

            var result = await projectQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<PagedData<Project>> GetProjectsByProgramProjectProfitabilityVLAAsync(PaginationParameters<string> query, string programNo)
        {
            var projectQuery = _dbContext.ProjectViews
                .AsNoTracking()
                .Where(p => EF.Functions.ILike(p.UserEmail!, _requestContext.UserEmailId) && p.Program == programNo)
                .Select(pv => MapToProject(pv)).AsQueryable();

            projectQuery = ApplyProjectFilter(projectQuery, query.Filter);
            projectQuery = (IQueryable<Project>)ApplySorting(projectQuery, query.SortBy, query.Descending);

            var result = await projectQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<Project?> GetProjectByIdAsync(string parentProject)
        {
            return await _dbContext.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ParentProject.ToLower() == parentProject.ToLower());
        }

        public async Task<PagedData<Project>> GetPagedProjectsAsync(PaginationParameters<string> query)
        {
            var projectQuery = _dbContext.ProjectViews
                .Where(p => EF.Functions.ILike(p.UserEmail!, _requestContext.UserEmailId))
                .Select(pv => new Project
                {
                    ParentProject = pv.ParentProject ?? string.Empty,
                    ProjectTitle = pv.ProjectTitle ?? string.Empty,
                    Program = pv.Program ?? string.Empty,
                    Customer = pv.Customer ?? string.Empty,
                    Contract = pv.Contract ?? string.Empty,
                    Disease = pv.Disease ?? string.Empty,
                    ProjectStatus = pv.ProjectStatus ?? string.Empty,
                    CostCentre = pv.CostCentre,
                    OracleProjectCode = pv.OracleProjectCode,
                    SubAccountCode = pv.SubAccountCode,
                    IsDefraProject = pv.IsDefraProject ?? 0,
                    IncomeAccountCode = pv.IncomeAccountCode ?? string.Empty
                }).AsQueryable();

            projectQuery = ApplyProjectFilter(projectQuery, query.Filter);
            projectQuery = (IQueryable<Project>)ApplySorting(projectQuery, query.SortBy, query.Descending);

            var result = await projectQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }
        
        public async Task<PagedData<ProjectView>> GetPagedProjectsByUserAsync(PaginationParameters<string> query)
        {
            var queryable = _dbContext.ProjectViews
                .AsNoTracking()
                .Where(p => EF.Functions.ILike(p.UserEmail!, _requestContext.UserEmailId))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search;
                queryable = queryable.Where(p =>
                    EF.Functions.ILike(p.ParentProject!, $"%{search}%") ||
                    EF.Functions.ILike(p.ProjectTitle!, $"%{search}%"));
            }

            queryable = query.SortBy switch
            {
                string s when s.Equals("parentproject", StringComparison.OrdinalIgnoreCase) => query.Descending
                    ? queryable.OrderByDescending(p => p.ParentProject)
                    : queryable.OrderBy(p => p.ParentProject),
                string s when s.Equals("projecttitle", StringComparison.OrdinalIgnoreCase) => query.Descending
                    ? queryable.OrderByDescending(p => p.ProjectTitle)
                    : queryable.OrderBy(p => p.ProjectTitle),
                _ => queryable.OrderBy(p => p.ParentProject)
            };

            var result = await queryable.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<PagedData<PactProjectView>> GetPagedPactProjectsAsync(PaginationParameters<string> query)
        {
            var querProjects = _dbContext.PactProjectViews.AsNoTracking().AsQueryable();

            // Apply filtering
            querProjects = ApplyPactProjectFilter(querProjects, query.Filter);

            // Apply sorting
            querProjects = (IQueryable<PactProjectView>)ApplyPactProjectSorting(querProjects, query.SortBy, query.Descending);

            // Execute query
            var result = await querProjects.ToListAsync();

            // Apply paging
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<PagedData<PactProjectView>> GetPagedPactProjectsByProgramAsync(PaginationParameters<string> query, string programNo)
        {
            var projectQuery = _dbContext.PactProjectViews
                .AsNoTracking()
                .Where(p => p.Program == programNo).AsQueryable();

            projectQuery = ApplyPactProjectFilter(projectQuery, query.Filter);

            projectQuery = (IQueryable<PactProjectView>)ApplyPactProjectSorting(projectQuery, query.SortBy, query.Descending);

            var result = await projectQuery.ToListAsync();

            return ApplyPaging(result, query.Page, query.PageSize);
        }

        //Create Project with trigger code
        public async Task<Project> CreateProjectAsync(Project project)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    project.FpsYear = _requestContext.FpsYear;
                    project.DateCreated = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
                    NormalizeDateTimesToUnspecified(project);
                    await _dbContext.Projects.AddAsync(project);
                    // Converted trigger logic — UITrig_tlkpProject FOR INSERT: stage audit log in same unit of work
                    _dbContext.ProjectLogs.Add(MapProjectToLog(project, "I", _requestContext.UserEmailId));
                    await _dbContext.SaveChangesAsync();

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
            return project;
        }

        //Update Project with trigger code
        public async Task<Project> UpdateProjectAsync(Project project)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    project.FpsYear = _requestContext.FpsYear;
                    NormalizeDateTimesToUnspecified(project);
                    _dbContext.Entry(project).State = EntityState.Modified;
                    _dbContext.Entry(project).Property(p => p.IncomeAccountCode).IsModified = false;
                    // Converted trigger logic — UITrig_tlkpProject FOR UPDATE: stage audit log in same unit of work
                    _dbContext.ProjectLogs.Add(MapProjectToLog(project, "I", _requestContext.UserEmailId));
                    await _dbContext.SaveChangesAsync();

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
            return project;
        }

        private static Project MapToProject(ProjectView pv) => new()
        {
            ParentProject     = pv.ParentProject     ?? string.Empty,
            ProjectTitle      = pv.ProjectTitle      ?? string.Empty,
            Program           = pv.Program           ?? string.Empty,
            Manager           = pv.Manager,
            Customer          = pv.Customer          ?? string.Empty,
            Contract          = pv.Contract          ?? string.Empty,
            Disease           = pv.Disease           ?? string.Empty,
            ProjectStatus     = pv.ProjectStatus     ?? string.Empty,
            ProjectGroup      = pv.ProjectGroup,
            BudgetCvl         = pv.BudgetCvl,
            CustIncome        = pv.CustIncome        ?? 0,
            TransferIncome    = pv.TransferIncome    ?? 0,
            PlanCaseWorkDebit = pv.PlanCaseWorkDebit,
            IsDefraProject    = pv.IsDefraProject    ?? 0,
            IncomeAccountCode = pv.IncomeAccountCode ?? string.Empty
        };

        private static List<T> SortList<T, TKey>(List<T> list, Func<T, TKey> keySelector, bool descending)
            => descending ? list.OrderByDescending(keySelector).ToList()
                          : list.OrderBy(keySelector).ToList();

        private static void NormalizeDateTimesToUnspecified(Project p)
        {
            if (p.DateCreated.HasValue && p.DateCreated.Value.Kind != DateTimeKind.Unspecified)
                p.DateCreated = DateTime.SpecifyKind(p.DateCreated.Value, DateTimeKind.Unspecified);

            if (p.DateCosted.HasValue && p.DateCosted.Value.Kind != DateTimeKind.Unspecified)
                p.DateCosted = DateTime.SpecifyKind(p.DateCosted.Value, DateTimeKind.Unspecified);
        }

        public async Task<Project?> UpdatePactProjectDetailsAsync(Project project)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            Project? entity = null;

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    entity = await _dbContext.Projects
                        .FirstOrDefaultAsync(p => p.ParentProject == project.ParentProject
                            && p.FpsYear == _requestContext.FpsYear);

                    if (entity == null) return;

                    entity.ProjectTitle = project.ProjectTitle;
                    entity.Program = project.Program;
                    entity.Customer = project.Customer;
                    entity.Manager = project.Manager;
                    entity.Contract = project.Contract;
                    entity.ProjectStatus = project.ProjectStatus;
                    entity.Disease = project.Disease;
                    entity.IsDefraProject = project.IsDefraProject;
                    entity.Finished = project.Finished;
                    entity.Comments = project.Comments;
                    entity.BudgetCvl = project.BudgetCvl;
                    entity.TransferIncome = project.TransferIncome;
                    entity.PvsIncome = project.PvsIncome;
                    entity.WipEoy = project.WipEoy;
                    entity.WipLimit = project.WipLimit;
                    entity.WipCurrent = project.WipCurrent;
                    entity.FecCost = project.FecCost;

                    NormalizeDateTimesToUnspecified(entity);
                    // Converted trigger logic — UITrig_tlkpProject FOR UPDATE: stage audit log in same unit of work
                    _dbContext.ProjectLogs.Add(MapProjectToLog(entity, "U", _requestContext.UserEmailId));
                    await _dbContext.SaveChangesAsync();

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
            return entity;
        }

        public async Task<Project?> UpdatePactPortfolioDetailsAsync(Project project)
        {
            var entity = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.ParentProject == project.ParentProject
                    && p.FpsYear == _requestContext.FpsYear);

            if (entity == null) return null;

            entity.ProjectTitle = project.ProjectTitle;
            entity.Program = project.Program;
            entity.Manager = project.Manager;
            entity.Finished = project.Finished;
            entity.Comments = project.Comments;
            entity.BudgetCvl = project.BudgetCvl;
            entity.TransferIncome = project.TransferIncome;

            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Project?> UpdateFpsPortfolioDetailsAsync(Project project)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            Project? entity = null;

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    entity = await _dbContext.Projects
                        .FirstOrDefaultAsync(p => p.ParentProject == project.ParentProject
                            && p.FpsYear == _requestContext.FpsYear);

                    if (entity == null) return;

                    entity.ProjectTitle = project.ProjectTitle;
                    entity.Program = project.Program;
                    entity.Manager = project.Manager;
                    entity.Disease = project.Disease;
                    entity.ProjectStatus = project.ProjectStatus;
                    entity.TransferIncome = project.TransferIncome;
                    entity.CustIncome = project.CustIncome;
                    entity.Profit = project.Profit;
                    entity.Contract = project.Contract;
                    entity.Customer = project.Customer;

                    NormalizeDateTimesToUnspecified(entity);
                    _dbContext.ProjectLogs.Add(MapProjectToLog(entity, "U", _requestContext.UserEmailId));
                    await _dbContext.SaveChangesAsync();

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
            return entity;
        }

        public async Task<bool> DeleteProjectAsync(string parentProject)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            var deleted = false;

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var project = await _dbContext.Projects
                        .FirstOrDefaultAsync(p => p.ParentProject == parentProject
                            && p.FpsYear == _requestContext.FpsYear);
                    if (project == null) return;
                    NormalizeDateTimesToUnspecified(project);
                    // Converted trigger logic — DTrig_tlkpProject FOR DELETE: stage audit log before delete in same unit of work
                    _dbContext.ProjectLogs.Add(MapProjectToLog(project, "D", _requestContext.UserEmailId));
                    _dbContext.Projects.Remove(project);
                    await _dbContext.SaveChangesAsync();

                    await tx.CommitAsync();
                    deleted = true;
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
            return deleted;
        }

        public async Task<bool> HasAssociatedJobCodesAsync(string parentProject)
        {
            return await _dbContext.JobCodes
                .AnyAsync(j => j.ParentProject == parentProject
                    && j.FpsYear == _requestContext.FpsYear);
        }

        public async Task<bool> CheckProgramExistsAsync(string programNo)
        {
            if (string.IsNullOrWhiteSpace(programNo))
                return true; // null/empty is allowed (nullable FK)
            return await _dbContext.Programs
                .AsNoTracking()
                .AnyAsync(p => p.ProgramNo == programNo);
        }

        private static IQueryable ApplyOrder<T>(IQueryable<Project> query, Expression<Func<Project, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private static IQueryable<Project> ApplyProjectFilter(IQueryable<Project> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue(FilterKeyParentProject, out var parentProject) && parentProject != null)
                query = query.Where(x => EF.Functions.ILike(x.ParentProject, $"%{parentProject}%"));

            if (dict.TryGetValue("ProjectTitle", out var projectTitle) && projectTitle != null)
                query = query.Where(x => EF.Functions.ILike(x.ProjectTitle, $"%{projectTitle}%"));

            if (dict.TryGetValue("Manager", out var manager) && manager != null)
                query = query.Where(x => EF.Functions.ILike(x.Manager!, $"%{manager}%"));

            if (dict.TryGetValue("OracleProjectCode", out var oracleProjectCode) && oracleProjectCode != null)
                query = query.Where(x => EF.Functions.ILike(x.OracleProjectCode!, $"%{oracleProjectCode}%"));

            if (dict.TryGetValue("SubAccountCode", out var subAccountCode) && subAccountCode != null)
                query = query.Where(x => EF.Functions.ILike(x.SubAccountCode!, $"%{subAccountCode}%"));

            if (dict.TryGetValue("CostCentre", out var costCentre) && costCentre != null
                && double.TryParse(costCentre.ToString(), out var costCentreValue))
                query = query.Where(x => x.CostCentre.HasValue && Math.Abs(x.CostCentre.Value - costCentreValue) < 1e-9);

            return query;
        }

        private static IQueryable<ProjectView> ApplyProfitabilityFilter(IQueryable<ProjectView> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("JobCode", out var jobCode) && jobCode != null)
                query = query.Where(x => EF.Functions.ILike(x.ParentProject!, $"%{jobCode}%"));

            if (dict.TryGetValue("ProjectStatus", out var projectStatus) && projectStatus != null)
                query = query.Where(x => EF.Functions.ILike(x.ProjectStatus!, $"%{projectStatus}%"));

            return query;
        }

        private static IQueryable ApplySorting(IQueryable<Project> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(p => p.ParentProject);

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }


        private static IQueryable ApplySortingByProperty(IQueryable<Project> query, string property, bool descending)
        {
            return property switch
            {
                "parentproject"    => ApplyOrder(query, p => p.ParentProject, descending),
                "projecttitle"     => ApplyOrder(query, p => p.ProjectTitle, descending),
                "program"          => ApplyOrder(query, p => p.Program, descending),
                "manager"          => ApplyOrder(query, p => p.Manager, descending),
                "projectgroup"     => ApplyOrder(query, p => p.ProjectGroup, descending),
                "customer"         => ApplyOrder(query, p => p.Customer, descending),
                "contract"         => ApplyOrder(query, p => p.Contract, descending),
                "disease"          => ApplyOrder(query, p => p.Disease, descending),
                "projectstatus"    => ApplyOrder(query, p => p.ProjectStatus, descending),
                "budgetcvl"        => ApplyOrder(query, p => p.BudgetCvl, descending),
                "budgetext"        => ApplyOrder(query, p => p.CustIncome, descending),
                "transferincome"   => ApplyOrder(query, p => p.TransferIncome, descending),
                "plancaseworkdebit"=> ApplyOrder(query, p => p.PlanCaseWorkDebit, descending),
                "costcentre"       => ApplyOrder(query, p => p.CostCentre, descending),
                "oracleprojectcode"=> ApplyOrder(query, p => p.OracleProjectCode, descending),
                "subaccountcode"   => ApplyOrder(query, p => p.SubAccountCode, descending),
                _ => query.OrderBy(p => p.ParentProject)
            };
        }


        private static IQueryable<PactProjectView> ApplyPactProjectFilter(IQueryable<PactProjectView> queryProjects, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return queryProjects;
            }

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
            {
                return queryProjects;
            }

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue(FilterKeyParentProject, out var parentProject) && parentProject != null)
            {
                queryProjects = queryProjects.Where(x => EF.Functions.ILike(x.ParentProject, $"%{parentProject}%"));
            }

            if (dict.TryGetValue("ProjectTitle", out var projectTitle) && projectTitle != null)
            {
                queryProjects = queryProjects.Where(x => EF.Functions.ILike(x.ProjectTitle, $"%{projectTitle}%"));
            }

            if(dict.TryGetValue("Manager", out var manager) && manager != null)
            {
                queryProjects = queryProjects.Where(x => x.Manager != null && EF.Functions.ILike(x.Manager, $"%{manager}%"));
            }

            return queryProjects;
        }

        private static IQueryable ApplyPactProjectSorting(IQueryable<PactProjectView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderBy(e => e.ParentProject);
            }

            return ApplyPactSortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplyPactSortingByProperty(IQueryable<PactProjectView> query, string property, bool descending)
        {
            return property switch
            {
                "parentproject" => ApplyPactProjectOrder(query, i => i.ParentProject, descending),
                "projecttitle"  => ApplyPactProjectOrder(query, i => i.ProjectTitle, descending),
                "manager"       => ApplyPactProjectOrder(query, i => i.Manager, descending),
                "projectstatus" => ApplyPactProjectOrder(query, i => i.ProjectStatus, descending),
                _ => query.OrderBy(e => e.ParentProject)
            };
        }

        private static IQueryable ApplyPactProjectOrder<T>(IQueryable<PactProjectView> query, Expression<Func<PactProjectView, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        // -- ProgrammeNewProject operations ----------------------------------

        /// <summary>
        /// Checks whether a project code already exists — derived from qryProjectCheck.
        /// </summary>
        public async Task<bool> CheckProjectExistsAsync(string newProject)
        {
            return await _dbContext.Projects
                .AsNoTracking()
                .AnyAsync(p => p.ParentProject == newProject);
        }

        /// <summary>
        /// Checks whether an old project code has Farm File submission data — derived from qryProjectCheckFF.
        /// </summary>
        public async Task<bool> CheckProjectExistsInFarmFileAsync(string oldProject)
        {
            return await _dbContext.SurvFFSubmissions
                .AsNoTracking()
                .AnyAsync(s => s.Contract == oldProject);
        }

        // -- Delete pre-condition checks (moved to service layer) ------------

        public async Task<bool> HasPlannedTestsAsync(string parentProject)
        {
            return await _dbContext.TestRequirements
                .AsNoTracking()
                .AnyAsync(tr => tr.ProjectBuyerCode == parentProject);
        }

        public async Task<bool> HasMonthlyOutputAsync(string parentProject)
        {
            return await _dbContext.MonthlyOutputs
                .AsNoTracking()
                .AnyAsync(mo => mo.Buyer == parentProject);
        }

        public async Task<bool> HasMonthlyTimeAsync(string parentProject)
        {
            return await _dbContext.MonthlyTimes
                .AsNoTracking()
                .AnyAsync(mt => mt.ParentProject == parentProject);
        }

        public async Task<bool> HasProjectInvoicesAsync(string parentProject)
        {
            return await _dbContext.ProjectInvoices
                .AsNoTracking()
                .AnyAsync(pi => pi.ProjectParent == parentProject);
        }

        public async Task<bool> HasProjectSubcontractsAsync(string parentProject)
        {
            return await _dbContext.ProjectSubContracts
                .AsNoTracking()
                .AnyAsync(ps => ps.Project == parentProject);
        }

        /// <summary>
        /// Renames a project code and updates all child table references — derived from usp_ChangeProjectCode.
        /// UITrig_tlkpProject FOR INSERT appended: stages audit log entry in same unit of work.
        /// </summary>
        public async Task ChangeProjectCodeAsync(string oldCode, string newCode)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    await CopyProjectRowAsync(oldCode, newCode);
                    await CopyJobCodesAsync(oldCode, newCode);
                    await CopyTimeCodeValidsAsync(oldCode, newCode);
                    await CopyTestRequirementsAsync(oldCode, newCode);
                    await UpdateMonthlyTimesAsync(oldCode, newCode);
                    await UpdateMonthlyOutputsAsync(oldCode, newCode);
                    await UpdateAdditionalCostsAsync(oldCode, newCode);
                    await UpdateSimpleChildTablesAsync(oldCode, newCode);
                    await UpdateAnimalRequestsAsync(oldCode, newCode);
                    await UpdateStaffJobsAsync(oldCode, newCode);
                    await DeleteOldCodeRowsAsync(oldCode);

                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
        }

        private async Task CopyProjectRowAsync(string oldCode, string newCode)
        {
            var oldProject = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.ParentProject == oldCode)
                ?? throw new InvalidOperationException($"Project '{oldCode}' not found.");

            var newProject = new Project
            {
                ParentProject     = newCode,
                ProjectTitle      = oldProject.ProjectTitle,
                Program           = oldProject.Program,
                Customer          = oldProject.Customer,
                Manager           = oldProject.Manager,
                TransferIncome    = oldProject.TransferIncome,
                CustIncome        = oldProject.CustIncome,
                WipEoy            = oldProject.WipEoy,
                WipLimit          = oldProject.WipLimit,
                WipCurrent        = oldProject.WipCurrent,
                ProjectStatus     = oldProject.ProjectStatus,
                CostBookNo        = oldProject.CostBookNo,
                FecCost           = oldProject.FecCost,
                Profit            = oldProject.Profit,
                BudgetCvl         = oldProject.BudgetCvl,
                DateCreated       = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                DateCosted        = oldProject.DateCosted,
                Disease           = oldProject.Disease,
                Contract          = oldProject.Contract,
                ProjectParent     = oldProject.ProjectParent,
                ShortTitle        = oldProject.ShortTitle,
                CaseWorkSub       = oldProject.CaseWorkSub,
                PvsIncome         = oldProject.PvsIncome,
                PlanCaseWorkDebit = oldProject.PlanCaseWorkDebit,
                Finished          = oldProject.Finished,
                OwningRc          = oldProject.OwningRc,
                Comments          = oldProject.Comments,
                CarryOver         = oldProject.CarryOver,
                CarryOverSeed     = oldProject.CarryOverSeed,
                IsDefraProject    = oldProject.IsDefraProject,
                CostCentre        = oldProject.CostCentre,
                OracleProjectCode = oldProject.OracleProjectCode,
                SubAccountCode    = oldProject.SubAccountCode,
                ProjectGroup      = oldProject.ProjectGroup,
                IncomeAccountCode = oldProject.IncomeAccountCode,
                FpsYear           = oldProject.FpsYear
            };

            NormalizeDateTimesToUnspecified(newProject);
            await _dbContext.Projects.AddAsync(newProject);
            _dbContext.ProjectLogs.Add(MapProjectToLog(newProject, "I", _requestContext.UserEmailId));
            await _dbContext.SaveChangesAsync();
        }

        private async Task CopyJobCodesAsync(string oldCode, string newCode)
        {
            var jobCodesToCopy = await _dbContext.JobCodes
                .Where(jc => jc.ParentProject == oldCode)
                .AsNoTracking()
                .ToListAsync();

            if (jobCodesToCopy.Count == 0) return;

            var newJobCodes = jobCodesToCopy.Select(jc => new JobCode
            {
                JobCodeId       = jc.JobCodeId == oldCode ? newCode : jc.JobCodeId,
                ParentProject   = newCode,
                JobCodeWorkGroup = jc.JobCodeWorkGroup,
                NewProg         = jc.NewProg,
                Type            = jc.Type,
                JobCodeName     = jc.JobCodeName,
                FpsYear         = jc.FpsYear
            }).ToList();

            await _dbContext.JobCodes.AddRangeAsync(newJobCodes);
            await _dbContext.SaveChangesAsync();
        }

        private async Task CopyTimeCodeValidsAsync(string oldCode, string newCode)
        {
            await _dbContext.TestCapabilities
                .Where(tc => tc.PlanPortfolio == oldCode)
                .ExecuteUpdateAsync(s => s.SetProperty<string>(x => x.PlanPortfolio, newCode));

            var tcvToCopy = await _dbContext.TimeCodeValids
                .Where(tcv => tcv.ParentProject == oldCode || tcv.Portfolio == oldCode)
                .AsNoTracking()
                .ToListAsync();

            if (tcvToCopy.Count == 0) return;

            var newTcvs = tcvToCopy
                .Select(tcv => new TimeCodeValid
                {
                    WorkGroup     = tcv.WorkGroup,
                    TimeCode      = tcv.TimeCode      == oldCode ? newCode : tcv.TimeCode,
                    ParentProject = tcv.ParentProject == oldCode ? newCode : tcv.ParentProject,
                    TestCode      = tcv.TestCode,
                    JobCode       = tcv.JobCode       == oldCode ? newCode : tcv.JobCode,
                    Portfolio     = tcv.Portfolio     == oldCode ? newCode : tcv.Portfolio,
                    Active        = tcv.Active,
                    FpsYear       = tcv.FpsYear
                })
                .DistinctBy(tcv => new { tcv.WorkGroup, tcv.TimeCode, tcv.ParentProject })
                .ToList();

            await _dbContext.TimeCodeValids.AddRangeAsync(newTcvs);
            await _dbContext.SaveChangesAsync();
        }

        private async Task CopyTestRequirementsAsync(string oldCode, string newCode)
        {
            var testReqsToCopy = await _dbContext.TestRequirements
                .Where(tr => tr.ProjectBuyerCode == oldCode)
                .AsNoTracking()
                .ToListAsync();

            if (testReqsToCopy.Count == 0) return;

            var newTestReqs = testReqsToCopy.Select(tr => new TestRequirement
            {
                TestCode        = tr.TestCode,
                Buyer           = newCode,
                UnitPrice       = tr.UnitPrice,
                NoRequired      = tr.NoRequired,
                ProjectBuyerCode = newCode,
                TestBuyerCode   = tr.TestBuyerCode,
                DateCreated     = tr.DateCreated,
                Active          = tr.Active,
                FpsYear         = tr.FpsYear
            }).ToList();

            await _dbContext.TestRequirements.AddRangeAsync(newTestReqs);
            await _dbContext.SaveChangesAsync();

            // Derived from UITrig_tlkpTestReqmt: log inserted rows to TestReq_LOG
            _dbContext.TestRequirementLogs.AddRange(newTestReqs.Select(tr => new TestRequirementLog
            {
                TestCode        = tr.TestCode,
                Buyer           = tr.Buyer,
                UnitPrice       = (double?)tr.UnitPrice,
                NoRequired      = tr.NoRequired,
                ProjectBuyerCode = tr.ProjectBuyerCode,
                TestBuyerCode   = tr.TestBuyerCode,
                Active          = tr.Active,
                DateTime        = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UserId          = _requestContext.UserEmailId,
                InsertDelete    = "I",
                FpsYear         = tr.FpsYear
            }));
            await _dbContext.SaveChangesAsync();
        }

        private async Task UpdateMonthlyTimesAsync(string oldCode, string newCode)
        {
            var mtToLog = await _dbContext.MonthlyTimes
                .Where(mt => mt.ParentProject == oldCode)
                .AsNoTracking()
                .ToListAsync();

            if (mtToLog.Count > 0)
            {
                // Derived from MT_LOG_UTrig: log old state (UD) then new state (UI) before update
                _dbContext.MonthlyTimeLogs.AddRange(mtToLog.Select(mt => new MonthlyTimeLog
                {
                    PactStaffId   = mt.PactStaffId,
                    TimeCode      = mt.TimeCode,
                    Month         = mt.Month,
                    ParentProject = mt.ParentProject,
                    WorkGroup     = mt.WorkGroup,
                    Hours         = mt.Hours,
                    DateTime      = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UserId        = _requestContext.UserEmailId,
                    InsertDelete  = "UD",
                    FpsYear       = mt.FpsYear ?? _requestContext.FpsYear
                }));
                _dbContext.MonthlyTimeLogs.AddRange(mtToLog.Select(mt => new MonthlyTimeLog
                {
                    PactStaffId   = mt.PactStaffId,
                    TimeCode      = mt.TimeCode == oldCode ? newCode : mt.TimeCode,
                    Month         = mt.Month,
                    ParentProject = newCode,
                    WorkGroup     = mt.WorkGroup,
                    Hours         = mt.Hours,
                    DateTime      = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UserId        = _requestContext.UserEmailId,
                    InsertDelete  = "UI",
                    FpsYear       = mt.FpsYear ?? _requestContext.FpsYear
                }));
                await _dbContext.SaveChangesAsync();
            }

            await _dbContext.MonthlyTimes
                .Where(mt => mt.ParentProject == oldCode)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.ParentProject, newCode)
                    .SetProperty(x => x.TimeCode, x => x.TimeCode == oldCode ? newCode : x.TimeCode));
        }

        private async Task UpdateMonthlyOutputsAsync(string oldCode, string newCode)
        {
            var moToLog = await _dbContext.MonthlyOutputs
                .Where(mo => mo.Buyer == oldCode)
                .AsNoTracking()
                .ToListAsync();

            if (moToLog.Count > 0)
            {
                // Derived from MO_LOG_UTrig: log old state (UD) then new state (UI) before update
                _dbContext.MonthlyOutputLogs.AddRange(moToLog.Select(mo => new MonthlyOutputLog
                {
                    TestCode     = mo.TestCode,
                    Buyer        = mo.Buyer,
                    Month        = mo.Month,
                    WorkGroup    = mo.WorkGroup,
                    Volume       = mo.Volume,
                    WgBuyer      = mo.WgBuyer,
                    DateTime     = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UserId       = _requestContext.UserEmailId,
                    InsertDelete = "UD",
                    FpsYear      = mo.FpsYear
                }));
                _dbContext.MonthlyOutputLogs.AddRange(moToLog.Select(mo => new MonthlyOutputLog
                {
                    TestCode     = mo.TestCode,
                    Buyer        = newCode,
                    Month        = mo.Month,
                    WorkGroup    = mo.WorkGroup,
                    Volume       = mo.Volume,
                    WgBuyer      = mo.WgBuyer,
                    DateTime     = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UserId       = _requestContext.UserEmailId,
                    InsertDelete = "UI",
                    FpsYear      = mo.FpsYear
                }));
                await _dbContext.SaveChangesAsync();
            }

            await _dbContext.MonthlyOutputs
                .Where(mo => mo.Buyer == oldCode)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Buyer, newCode));
        }

        private async Task UpdateAdditionalCostsAsync(string oldCode, string newCode)
        {
            var acToLog = await _dbContext.AdditionalCosts
                .Where(ac => ac.JobCode == oldCode)
                .AsNoTracking()
                .ToListAsync();

            if (acToLog.Count > 0)
            {
                // Derived from UITrig_tblAdditionalCosts: log new state ('I') before update
                _dbContext.AdditionalCostLogs.AddRange(acToLog.Select(ac => new AdditionalCostLog
                {
                    JobCode      = newCode,
                    Account      = ac.Account,
                    Description  = ac.Description,
                    ItemCost     = ac.ItemCost,
                    Freq         = ac.Freq,
                    Supplier     = ac.Supplier,
                    DateTime     = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UserId       = _requestContext.UserEmailId,
                    InsertDelete = "I",
                    FpsYear      = ac.FpsYear ?? _requestContext.FpsYear
                }));
                await _dbContext.SaveChangesAsync();
            }

            await _dbContext.AdditionalCosts
                .Where(ac => ac.JobCode == oldCode)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.JobCode, newCode));
        }

        private async Task UpdateSimpleChildTablesAsync(string oldCode, string newCode)
        {
            await _dbContext.ProjectInvoices
                .Where(pi => pi.ProjectParent == oldCode)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.ProjectParent, newCode));
            await _dbContext.ProjectSubContracts
                .Where(ps => ps.Project == oldCode)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Project, newCode));
            await _dbContext.TimeCostCalcs
                .Where(tc => tc.Project == oldCode)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Project, newCode)
                    .SetProperty(x => x.JobCode, x => x.JobCode == oldCode ? newCode : x.JobCode));
            await _dbContext.ProjectMonths
                .Where(pm => pm.Project == oldCode)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Project, newCode));
        }

        private async Task UpdateAnimalRequestsAsync(string oldCode, string newCode)
        {
            var arToLog = await _dbContext.AnimalRequests
                .Where(ar => ar.JobCode == oldCode)
                .AsNoTracking()
                .ToListAsync();

            if (arToLog.Count > 0)
            {
                // Derived from UITrig_tblAnimalReq: log new state ('I') before update
                _dbContext.AnimalRequestLogs.AddRange(arToLog.Select(ar => new AnimalRequestLog
                {
                    JobCode         = newCode,
                    AnimalType      = ar.AnimalType,
                    NumberOfDays    = ar.NumberOfDays,
                    NumberOfAnimals = ar.NumberOfAnimals,
                    DateTime        = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UserId          = _requestContext.UserEmailId,
                    InsertDelete    = "I",
                    FpsYear         = ar.FpsYear ?? _requestContext.FpsYear
                }));
                await _dbContext.SaveChangesAsync();
            }

            await _dbContext.AnimalRequests
                .Where(ar => ar.JobCode == oldCode)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.JobCode, newCode));

            await _dbContext.Milestones
                .Where(m => m.Project == oldCode)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Project, newCode));
        }

        private async Task UpdateStaffJobsAsync(string oldCode, string newCode)
        {
            var sjToLog = await _dbContext.StaffJobs
                .Where(sj => sj.JobCode == oldCode)
                .AsNoTracking()
                .ToListAsync();

            if (sjToLog.Count > 0)
            {
                // Derived from UITrig_tblStaffJob: log new state ('I') before update
                _dbContext.StaffJobLogs.AddRange(sjToLog.Select(sj => new StaffJobLog
                {
                    StaffId      = sj.StaffId,
                    JobCode      = newCode,
                    PlannedHours = sj.PlannedHours,
                    DateTime     = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UserId       = _requestContext.UserEmailId,
                    InsertDelete = "I",
                    FpsYear      = sj.FpsYear ?? _requestContext.FpsYear
                }));
                await _dbContext.SaveChangesAsync();
            }

            await _dbContext.StaffJobs
                .Where(sj => sj.JobCode == oldCode)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.JobCode, newCode));

            await _dbContext.ProjectMonthFinals
                .Where(pmf => pmf.Project == oldCode)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.Project, newCode));
        }

        private async Task DeleteOldCodeRowsAsync(string oldCode)
        {
            // sp_Delete_tr, sp_Delete_tcv, sp_Delete_jc, sp_Delete_pp

            // Derived from DTrig_tlkpTestReqmt: log deleted rows before delete
            var trToDelete = await _dbContext.TestRequirements
                .Where(tr => tr.ProjectBuyerCode == oldCode)
                .AsNoTracking()
                .ToListAsync();

            if (trToDelete.Count > 0)
            {
                _dbContext.TestRequirementLogs.AddRange(trToDelete.Select(tr => new TestRequirementLog
                {
                    TestCode        = tr.TestCode,
                    Buyer           = tr.Buyer,
                    UnitPrice       = (double?)tr.UnitPrice,
                    NoRequired      = tr.NoRequired,
                    ProjectBuyerCode = tr.ProjectBuyerCode,
                    TestBuyerCode   = tr.TestBuyerCode,
                    Active          = tr.Active,
                    DateTime        = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UserId          = _requestContext.UserEmailId,
                    InsertDelete    = "D",
                    FpsYear         = tr.FpsYear
                }));
                await _dbContext.SaveChangesAsync();
            }

            await _dbContext.TestRequirements
                .Where(tr => tr.ProjectBuyerCode == oldCode)
                .ExecuteDeleteAsync();
            await _dbContext.TimeCodeValids
                .Where(tcv => tcv.ParentProject == oldCode || tcv.Portfolio == oldCode)
                .ExecuteDeleteAsync();
            await _dbContext.JobCodes
                .Where(jc => jc.ParentProject == oldCode)
                .ExecuteDeleteAsync();

            // Stage "D" audit log for the old project before deleting
            var projectToDelete = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.ParentProject == oldCode);

            if (projectToDelete != null)
            {
                NormalizeDateTimesToUnspecified(projectToDelete);
                _dbContext.ProjectLogs.Add(MapProjectToLog(projectToDelete, "D", _requestContext.UserEmailId));
                _dbContext.Projects.Remove(projectToDelete);
                await _dbContext.SaveChangesAsync();
            }
        }


        /// <summary>
        /// Deletes a project and all dependent child records — derived from usp_Delete_Project.
        /// DTrig_tlkpProject (DELETE) appended: stages audit log entry.
        /// </summary>
        public async Task DeleteProjectAndChildrenAsync(string parentProject)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    await DeleteProjectCoreAsync(parentProject);
                    await tx.CommitAsync();
                }
                catch
                {
                    await tx.RollbackAsync();
                    throw;
                }
            });
        }

        private async Task DeleteProjectCoreAsync(string parentProject)
        {
            // Converted trigger logic — DTrig_tlkpProject FOR DELETE: stage audit log before delete
            var project = await _dbContext.Projects
                .FirstOrDefaultAsync(p => p.ParentProject == parentProject);

            if (project == null) return;

            NormalizeDateTimesToUnspecified(project);
            _dbContext.ProjectLogs.Add(MapProjectToLog(project, "D", _requestContext.UserEmailId));
            await _dbContext.SaveChangesAsync();

            // sp_Delete_tcv
            await _dbContext.TimeCodeValids
                .Where(tcv => tcv.ParentProject == parentProject || tcv.Portfolio == parentProject)
                .ExecuteDeleteAsync();

            // sp_Delete_JC
            await _dbContext.JobCodes
                .Where(jc => jc.ParentProject == parentProject)
                .ExecuteDeleteAsync();

            // sp_delete_tr — Derived from DTrig_tlkpTestReqmt: log before delete
            await LogAndDeleteTestRequirementsAsync(parentProject);

            // sp_Delete_ar — Derived from DTrig_tblAnimalReq: log before delete
            await LogAndDeleteAnimalRequestsAsync(parentProject);

            // sp_Delete_sj — Derived from DTrig_tblStaffJob: log before delete
            await LogAndDeleteStaffJobsAsync(parentProject);

            // sp_Delete_ac — Derived from DTrig_tblAdditionalCosts: log before delete
            await LogAndDeleteAdditionalCostsAsync(parentProject);

            // sp_Delete_pp
            await _dbContext.Projects
                .Where(p => p.ParentProject == parentProject)
                .ExecuteDeleteAsync();
        }

        private async Task LogAndDeleteTestRequirementsAsync(string parentProject)
        {
            var trToDelete = await _dbContext.TestRequirements
                .Where(tr => tr.ProjectBuyerCode == parentProject)
                .AsNoTracking()
                .ToListAsync();

            if (trToDelete.Count > 0)
            {
                _dbContext.TestRequirementLogs.AddRange(trToDelete.Select(tr => new TestRequirementLog
                {
                    TestCode         = tr.TestCode,
                    Buyer            = tr.Buyer,
                    UnitPrice        = (double?)tr.UnitPrice,
                    NoRequired       = tr.NoRequired,
                    ProjectBuyerCode = tr.ProjectBuyerCode,
                    TestBuyerCode    = tr.TestBuyerCode,
                    Active           = tr.Active,
                    DateTime         = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UserId           = _requestContext.UserEmailId,
                    InsertDelete     = "D",
                    FpsYear          = tr.FpsYear
                }));
                await _dbContext.SaveChangesAsync();
            }

            await _dbContext.TestRequirements
                .Where(tr => tr.ProjectBuyerCode == parentProject)
                .ExecuteDeleteAsync();
        }

        private async Task LogAndDeleteAnimalRequestsAsync(string parentProject)
        {
            var arToDelete = await _dbContext.AnimalRequests
                .Where(ar => ar.JobCode == parentProject)
                .AsNoTracking()
                .ToListAsync();

            if (arToDelete.Count > 0)
            {
                _dbContext.AnimalRequestLogs.AddRange(arToDelete.Select(ar => new AnimalRequestLog
                {
                    JobCode         = ar.JobCode,
                    AnimalType      = ar.AnimalType,
                    NumberOfDays    = ar.NumberOfDays,
                    NumberOfAnimals = ar.NumberOfAnimals,
                    DateTime        = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UserId          = _requestContext.UserEmailId,
                    InsertDelete    = "D",
                    FpsYear         = ar.FpsYear ?? _requestContext.FpsYear
                }));
                await _dbContext.SaveChangesAsync();
            }

            await _dbContext.AnimalRequests
                .Where(ar => ar.JobCode == parentProject)
                .ExecuteDeleteAsync();
        }

        private async Task LogAndDeleteStaffJobsAsync(string parentProject)
        {
            var sjToDelete = await _dbContext.StaffJobs
                .Where(sj => sj.JobCode == parentProject)
                .AsNoTracking()
                .ToListAsync();

            if (sjToDelete.Count > 0)
            {
                _dbContext.StaffJobLogs.AddRange(sjToDelete.Select(sj => new StaffJobLog
                {
                    StaffId      = sj.StaffId,
                    JobCode      = sj.JobCode,
                    PlannedHours = sj.PlannedHours,
                    DateTime     = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UserId       = _requestContext.UserEmailId,
                    InsertDelete = "D",
                    FpsYear      = sj.FpsYear ?? _requestContext.FpsYear
                }));
                await _dbContext.SaveChangesAsync();
            }

            await _dbContext.StaffJobs
                .Where(sj => sj.JobCode == parentProject)
                .ExecuteDeleteAsync();
        }

        private async Task LogAndDeleteAdditionalCostsAsync(string parentProject)
        {
            var acToDelete = await _dbContext.AdditionalCosts
                .Where(ac => ac.JobCode == parentProject)
                .AsNoTracking()
                .ToListAsync();

            if (acToDelete.Count > 0)
            {
                _dbContext.AdditionalCostLogs.AddRange(acToDelete.Select(ac => new AdditionalCostLog
                {
                    JobCode      = ac.JobCode,
                    Account      = ac.Account,
                    Description  = ac.Description,
                    ItemCost     = ac.ItemCost,
                    Freq         = ac.Freq,
                    Supplier     = ac.Supplier,
                    DateTime     = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    UserId       = _requestContext.UserEmailId,
                    InsertDelete = "D",
                    FpsYear      = ac.FpsYear ?? _requestContext.FpsYear
                }));
                await _dbContext.SaveChangesAsync();
            }

            await _dbContext.AdditionalCosts
                .Where(ac => ac.JobCode == parentProject)
                .ExecuteDeleteAsync();
        }


        // -- Private helpers ------------------------------------------------

        private static ProjectLog MapProjectToLog(Project p, string operation, string userId) => new()
        {
            ParentProject = p.ParentProject,
            ProjectTitle = p.ProjectTitle,
            Program = p.Program,
            Customer = p.Customer,
            Manager = p.Manager,
            TransferIncome = p.TransferIncome,
            CustIncome = p.CustIncome,
            WipEoy = p.WipEoy,
            WipLimit = p.WipLimit,
            WipCurrent = p.WipCurrent,
            ProjectStatus = p.ProjectStatus,
            CostBookNo = p.CostBookNo,
            DateCreated = p.DateCreated,
            FecCost = p.FecCost,
            Profit = p.Profit,
            BudgetCvl = p.BudgetCvl,
            DateCosted = p.DateCosted,
            Disease = p.Disease,
            Contract = p.Contract,
            ProjectParent = p.ProjectParent,
            ShortTitle = p.ShortTitle,
            CaseWorkSub = p.CaseWorkSub,
            PvsIncome = p.PvsIncome,
            PlanCaseWorkDebit = p.PlanCaseWorkDebit,
            Finished = p.Finished,
            OwningRc = p.OwningRc,
            Comments = p.Comments,
            CarryOver = p.CarryOver,
            CarryOverSeed = p.CarryOverSeed,
            DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
            InsertDelete = operation,
            JobCode = p.ParentProject,
            IsDefraProject = p.IsDefraProject,
            CostCentre = p.CostCentre,
            OracleProjectCode = p.OracleProjectCode,
            SubAccountCode = p.SubAccountCode,
            ProjectGroup = p.ProjectGroup,
            IncomeAccountCode = p.IncomeAccountCode,
            FpsYear = p.FpsYear,
            UserId = userId
        };

        private sealed record ProjectProfitabilityEntry(
            string? ParentProject,
            decimal? BudgetCvl,
            decimal? Profit,
            string? ProjectStatus,
            string? Program);

        private sealed record VlaProjectEntry(
            string? ParentProject,
            decimal? BudgetCvl,
            string? ProjectStatus,
            string? Program,
            string? Customer,
            string? Manager,
            decimal? ProgrammeTarget);

        /// <summary>
        /// Returns paginated project profitability data for a given programme.
        /// Translates qryProjectProfitability3: Projects + Programs + aggregate cost sub-queries.
        /// Staff costs sourced from TimeCostCalcsViews (vtimecostcalcs, grouped by Project).
        /// Animal costs from AnimalRequests joined to Animals for daily rate.
        /// Test costs from TestRequirements (NoRequired × UnitPrice per vtbltestrequ).
        /// Additional costs from AdditionalCosts (sum of ItemCost per JobCode).
        /// workTypeFilter: "all" | "approved" | "not-approved"
        /// </summary>
        public async Task<PagedData<ProjectProfitabilityView>> GetProjectProfitabilityAsync(
            PaginationParameters<string> query, string programNo, string workTypeFilter)
        {
            var projectQuery = _dbContext.ProjectViews
                .AsNoTracking()
                .Where(p => EF.Functions.ILike(p.UserEmail!, _requestContext.UserEmailId) && p.Program == programNo);

            if (workTypeFilter == "approved")
                projectQuery = projectQuery.Where(p => p.ProjectStatus == "Approved");
            else if (workTypeFilter == "not-approved")
                projectQuery = projectQuery.Where(p => p.ProjectStatus == "Not Approved");

            projectQuery = ApplyProfitabilityFilter(projectQuery, query.Filter);

            var projects = await projectQuery
                .Select(p => new ProjectProfitabilityEntry(p.ParentProject, p.BudgetCvl, p.Profit, p.ProjectStatus, p.Program))
                .ToListAsync();

            if (projects.Count == 0)
                return ApplyPaging(new List<ProjectProfitabilityView>(), query.Page, query.PageSize);

            var programme = await _dbContext.Programs
                .AsNoTracking()
                .Where(pg => pg.ProgramNo == programNo)
                .Select(pg => new { pg.ProgramNo, pg.Target })
                .FirstOrDefaultAsync();

            var programmeTargetMap = programme != null
                ? new Dictionary<string, decimal?> { { programme.ProgramNo!, programme.Target } }
                : new Dictionary<string, decimal?>();

            return await ComputeProfitabilityAsync(query, projects, programmeTargetMap);
        }

        /// <summary>
        /// Returns paginated project profitability data for a given project group.
        /// Same cost logic as GetProjectProfitabilityAsync but filtered by ProjectGroup instead of ProgramNo.
        /// ProgrammeTarget is resolved per-project from each project's attached Programme.
        /// workTypeFilter: "all" | "approved" | "not-approved"
        /// </summary>
        public async Task<PagedData<ProjectProfitabilityView>> GetProjectGroupProfitabilityAsync(
            PaginationParameters<string> query, string projectGroup, string workTypeFilter)
        {
            var projectQuery = (from pg in _dbContext.ProjectGroupViews
                                join pv in _dbContext.Projects on
                                new { pg.ProjectGroupName } equals new { ProjectGroupName = pv.ProjectGroup }
                                where EF.Functions.ILike(pg.UserEmail!, _requestContext.UserEmailId)
                                      && pg.ProjectGroupName == projectGroup
                                select pv).AsQueryable();

            if (workTypeFilter == "approved")
                projectQuery = projectQuery.Where(p => p.ProjectStatus == "Approved");
            else if (workTypeFilter == "not-approved")
                projectQuery = projectQuery.Where(p => p.ProjectStatus == "Not Approved");

            projectQuery = ApplyProjectFilter(projectQuery, query.Filter);

            var projects = await projectQuery
                .Select(p => new ProjectProfitabilityEntry(p.ParentProject, p.BudgetCvl, p.Profit, p.ProjectStatus, p.Program))
                .ToListAsync();

            if (projects.Count == 0)
                return ApplyPaging(new List<ProjectProfitabilityView>(), query.Page, query.PageSize);

            var distinctProgramNos = projects
                .Select(p => p.Program)
                .Where(p => p != null)
                .Distinct()
                .ToList();

            var programmeTargetMap = await _dbContext.Programs
                .AsNoTracking()
                .Where(pg => distinctProgramNos.Contains(pg.ProgramNo))
                .ToDictionaryAsync(pg => pg.ProgramNo!, pg => pg.Target);

            return await ComputeProfitabilityAsync(query, projects, programmeTargetMap);
        }

        private static ProjectProfitabilityView BuildProfitabilityRow(
            ProjectProfitabilityEntry p,
            Dictionary<string, decimal> staffMap,
            Dictionary<string, decimal> additionalMap,
            Dictionary<string, decimal> testMap,
            Dictionary<string, decimal> animalCostByJob,
            Dictionary<string, decimal?> programmeTargetMap)
        {
            var staff      = staffMap.TryGetValue(p.ParentProject!, out var s)  ? s  : 0m;
            var additional = additionalMap.TryGetValue(p.ParentProject!, out var a) ? a : 0m;
            var test       = testMap.TryGetValue(p.ParentProject!, out var t)   ? t  : 0m;
            var animal     = animalCostByJob.TryGetValue(p.ParentProject!, out var an) ? an : 0m;
            var total      = staff + additional + test + animal;
            var profit     = p.Profit ?? 0m;
            var jcProfit   = (p.BudgetCvl ?? 0m) - total;

            return new ProjectProfitabilityView
            {
                JobCode                = p.ParentProject!,
                JcTotalStaffCosts      = staff,
                JcTotalTestCosts       = test,
                JcTotalAnimalCosts     = animal,
                JcTotalAdditionalCosts = additional,
                TotalCosts             = total,
                BudgetCvl              = p.BudgetCvl,
                JcProfit               = jcProfit,
                TargetProfit           = profit,
                OffTarget              = jcProfit - profit,
                ProgramNo              = p.Program,
                ProgrammeTarget        = p.Program != null && programmeTargetMap.TryGetValue(p.Program, out var tgt) ? tgt : null,
                ProjectStatus          = p.ProjectStatus
            };
        }

        private async Task<PagedData<ProjectProfitabilityView>> ComputeProfitabilityAsync(
            PaginationParameters<string> query,
            List<ProjectProfitabilityEntry> projects,
            Dictionary<string, decimal?> programmeTargetMap)
        {
            var projectCodes = projects.Select(p => p.ParentProject).ToList();

            // Calculate staff costs by summing Cost from TimeCostCalcsViews per Project (JobCode)
            var staffCosts = await (
                from sj in _dbContext.StaffJobs
                join wge in _dbContext.WorkGroupEmployees
                    on sj.StaffId equals wge.PactId                   
                join wgg in _dbContext.WorkgroupGrades
                    on wge.WorkGroupGrade equals wgg.WgGrade 
                join pcg in _dbContext.ProfitCentreGrades
                    on wgg.ProfitCentreGrade equals pcg.PcGrade                   
                join p in _dbContext.Projects
                    on sj.JobCode equals p.ParentProject
                join pg in _dbContext.Programs
                    on p.Program equals pg.ProgramNo                                      
                where projectCodes.Contains(sj.JobCode)
                    && pg != null
                    && EF.Functions.ILike(pg.SectorName!, "%charge%")
                select new
                {                    
                    sectorCharge = string.Equals((pg.SectorName ?? "").Trim(), "charge", StringComparison.OrdinalIgnoreCase) ? 1m : 0m,
                    JobCode = sj.JobCode,                    
                    PlannedHours = sj.PlannedHours,
                    ChargeRate = p.IsDefraProject == 0 ? pcg.ChargeRate : pcg.DefraChargeRate
                })
                .ToListAsync();            

            // Additional costs by summing ItemCost per JobCode from AdditionalCosts
            var additionalCosts = await _dbContext.AdditionalCosts
                .AsNoTracking()
                .Where(ac => projectCodes.Contains(ac.JobCode))
                .GroupBy(ac => ac.JobCode)
                .Select(g => new { JobCode = g.Key, TotalAdditional = g.Sum(x => x.ItemCost) })
                .ToListAsync();

            //Calculate test costs by multiplying NoRequired by UnitPrice for each TestRequirement, then summing per JobCode
            var testCostsRaw = await _dbContext.TestRequirements
                .AsNoTracking()
                .Where(tr => projectCodes.Contains(tr.Buyer))
                .Select(tr => new
                {
                    tr.Buyer,
                    NoRequired = Convert.ToDecimal(tr.NoRequired ?? 0d),
                    UnitPrice = Convert.ToDecimal(tr.UnitPrice ?? 0m)
                })
                .ToListAsync();

            var testCosts = testCostsRaw
                .GroupBy(tr => tr.Buyer)
                .Select(g => new { JobCode = g.Key, TotalTest = g.Sum(x => x.NoRequired * x.UnitPrice) })
                .ToList();

            // Calculate animal costs: NumberOfAnimals × NumberOfDays × (IsDefraProject=0 ? DailyRate : DefraDailyRate)           
            var animalCostsRaw = await (
                from ar in _dbContext.AnimalRequests
                join p in _dbContext.Projects
                    on ar.JobCode equals p.ParentProject                    
                join a in _dbContext.Animals
                    on ar.AnimalType equals a.AnimalType                  
                where projectCodes.Contains(ar.JobCode)
                select new
                {
                    ar.JobCode,
                    ar.NumberOfAnimals,
                    ar.NumberOfDays,
                    Cost = p.IsDefraProject == 0 ? a.DailyRate : a.DefraDailyRate
                })
                .ToListAsync();

            var animalCostByJob = animalCostsRaw
                .GroupBy(x => x.JobCode)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => (decimal)(x.NumberOfAnimals * x.NumberOfDays) * (x.Cost ?? 0m)));

            var staffMap = staffCosts
                .Where(e => e.sectorCharge == 1m)
                .GroupBy(x => x.JobCode)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => (decimal)x.PlannedHours * (x.ChargeRate ?? 0m) * x.sectorCharge));
            var additionalMap = additionalCosts.ToDictionary(x => x.JobCode, x => x.TotalAdditional);
            var testMap = testCosts.ToDictionary(x => x.JobCode, x => x.TotalTest);

            var results = projects
                .Select(p => BuildProfitabilityRow(p, staffMap, additionalMap, testMap, animalCostByJob, programmeTargetMap))
                .ToList();

            // Apply sorting
            results = query.SortBy?.ToLower() switch
            {
                "jobcode"               => SortList(results, r => r.JobCode,                 query.Descending),
                "totalcosts"            => SortList(results, r => r.TotalCosts,              query.Descending),
                "budgetcvl"             => SortList(results, r => r.BudgetCvl,               query.Descending),
                "jcprofit"              => SortList(results, r => r.JcProfit,                query.Descending),
                "offtarget"             => SortList(results, r => r.OffTarget,               query.Descending),
                "projectstatus"         => SortList(results, r => r.ProjectStatus,           query.Descending),
                "jctotalstaffcosts"     => SortList(results, r => r.JcTotalStaffCosts,       query.Descending),
                "jctotaltestcosts"      => SortList(results, r => r.JcTotalTestCosts,        query.Descending),
                "jctotalanimalcosts"    => SortList(results, r => r.JcTotalAnimalCosts,      query.Descending),
                "jctotaladditionalcosts"=> SortList(results, r => r.JcTotalAdditionalCosts,  query.Descending),
                "targetprofit"          => SortList(results, r => r.TargetProfit,            query.Descending),
                _                       => results.OrderBy(r => r.JobCode).ToList()
            };

            return ApplyPaging(results, query.Page, query.PageSize);
        }

        // ── VLA Project Profitability ──────────────────────────────────────────

        /// <summary>
        /// Returns paginated project profitability data for the VLA view.
        /// Filter dimensions (all optional, case-insensitive):
        ///   projectStatus, programNo, manager, customer.
        /// </summary>
        public async Task<PagedData<ProjectProfitabilityVlaView>> GetProjectProfitabilityVlaAsync(
            PaginationParameters<string> query,
            string? projectStatus = null,
            string? programNo = null,
            string? manager = null,
            string? customer = null)
        {
            // Use an anonymous projection so EF Core can translate all Where predicates.
            // Projecting directly to a named record type (VlaProjectEntry) and then composing
            // Where clauses on it produces untranslatable expressions like new VlaProjectEntry(...).Program.
            var rawQuery = (from p in _dbContext.Projects.AsNoTracking()
                            join pg in _dbContext.Programs on p.Program equals pg.ProgramNo into pgJoin
                            from pg in pgJoin.DefaultIfEmpty()
                            select new { p, pg }).AsQueryable();

            if (!string.IsNullOrWhiteSpace(projectStatus))
                rawQuery = rawQuery.Where(x => EF.Functions.ILike(x.p.ProjectStatus!, $"%{projectStatus}%"));

            if (!string.IsNullOrWhiteSpace(programNo))
                rawQuery = rawQuery.Where(x => EF.Functions.ILike(x.p.Program!, $"%{programNo}%"));

            if (!string.IsNullOrWhiteSpace(manager))
                rawQuery = rawQuery.Where(x => x.pg != null && EF.Functions.ILike(x.pg.Manager!, $"%{manager}%"));

            if (!string.IsNullOrWhiteSpace(customer))
                rawQuery = rawQuery.Where(x => EF.Functions.ILike(x.p.Customer!, $"%{customer}%"));

            if (!string.IsNullOrWhiteSpace(query.Search))
                rawQuery = rawQuery.Where(x => EF.Functions.ILike(x.p.ParentProject!, $"%{query.Search}%"));

            var filterDict = ParseFilterDict(query.Filter);
            if (filterDict.TryGetValue("JobCode", out var jobCode))
                rawQuery = rawQuery.Where(x => EF.Functions.ILike(x.p.ParentProject!, $"%{jobCode}%"));
            if (filterDict.TryGetValue(FilterKeyParentProject, out var parentProject))
                rawQuery = rawQuery.Where(x => EF.Functions.ILike(x.p.ParentProject!, $"%{parentProject}%"));

            var projects = await rawQuery
                .Select(x => new VlaProjectEntry(
                    x.p.ParentProject,
                    x.p.BudgetCvl,
                    x.p.ProjectStatus,
                    x.p.Program,
                    x.p.Customer,
                    x.pg == null ? null : x.pg.Manager,
                    x.pg == null ? (decimal?)null : x.pg.Target))
                .ToListAsync();

            if (projects.Count == 0)
                return ApplyPaging(new List<ProjectProfitabilityVlaView>(), query.Page, query.PageSize);

            return await ComputeProfitabilityForVlaAsync(query, projects);
        }

        private static IDictionary<string, object> ParseFilterDict(string? filter)
        {
            if (string.IsNullOrEmpty(filter)) return new Dictionary<string, object>();
            dynamic? model = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            return model is IDictionary<string, object> dict ? dict : new Dictionary<string, object>();
        }

        private static List<ProjectProfitabilityVlaView> ApplyVlaSorting(
            List<ProjectProfitabilityVlaView> results, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "jobcode"         => SortList(results, v => v.JobCode,          descending),
                "program"         => SortList(results, v => v.Program,          descending),
                "customer"        => SortList(results, v => v.Customer,         descending),
                "manager"         => SortList(results, v => v.Manager,          descending),
                "status"          => SortList(results, v => v.Status,           descending),
                "staffcosts"      => SortList(results, v => v.StaffCosts,       descending),
                "testcost"        => SortList(results, v => v.TestCost,         descending),
                "animalcosts"     => SortList(results, v => v.AnimalCosts,      descending),
                "additionalcosts" => SortList(results, v => v.AdditionalCosts,  descending),
                "totalcosts"      => SortList(results, v => v.TotalCosts,       descending),
                "budget"          => SortList(results, v => v.Budget,           descending),
                "profit"          => SortList(results, v => v.Profit,           descending),
                "targetprofit"    => SortList(results, v => v.TargetProfit,     descending),
                "offtarget"       => SortList(results, v => v.OffTarget,        descending),
                _                 => results.OrderBy(v => v.JobCode).ToList()
            };
        }

        private async Task<PagedData<ProjectProfitabilityVlaView>> ComputeProfitabilityForVlaAsync(
            PaginationParameters<string> query,
            List<VlaProjectEntry> projects)
        {
            var projectCodes = projects.Select(p => p.ParentProject).ToList();

            var staffCosts = await (
                from sj in _dbContext.StaffJobs
                join wge in _dbContext.WorkGroupEmployees on sj.StaffId equals wge.PactId
                join wgg in _dbContext.WorkgroupGrades on wge.WorkGroupGrade equals wgg.WgGrade
                join pcg in _dbContext.ProfitCentreGrades on wgg.ProfitCentreGrade equals pcg.PcGrade
                join p in _dbContext.Projects on sj.JobCode equals p.ParentProject
                join pg in _dbContext.Programs on p.Program equals pg.ProgramNo
                where projectCodes.Contains(sj.JobCode)
                    && pg != null
                    && EF.Functions.ILike(pg.SectorName!, "%charge%")
                select new
                {
                    sectorCharge = string.Equals((pg.SectorName ?? "").Trim(), "charge", StringComparison.OrdinalIgnoreCase) ? 1m : 0m,
                    JobCode = sj.JobCode,
                    PlannedHours = sj.PlannedHours,
                    ChargeRate = p.IsDefraProject == 0 ? pcg.ChargeRate : pcg.DefraChargeRate
                })
                .ToListAsync();

            var additionalCosts = await _dbContext.AdditionalCosts
                .AsNoTracking()
                .Where(ac => projectCodes.Contains(ac.JobCode))
                .GroupBy(ac => ac.JobCode)
                .Select(g => new { JobCode = g.Key, TotalAdditional = g.Sum(x => x.ItemCost) })
                .ToListAsync();

            var testCostsRaw = await _dbContext.TestRequirements
                .AsNoTracking()
                .Where(tr => projectCodes.Contains(tr.Buyer))
                .Select(tr => new
                {
                    tr.Buyer,
                    NoRequired = Convert.ToDecimal(tr.NoRequired ?? 0d),
                    UnitPrice = Convert.ToDecimal(tr.UnitPrice ?? 0m)
                })
                .ToListAsync();

            var testCosts = testCostsRaw
                .GroupBy(tr => tr.Buyer)
                .Select(g => new { JobCode = g.Key, TotalTest = g.Sum(x => x.NoRequired * x.UnitPrice) })
                .ToList();

            var animalCostsRaw = await (
                from ar in _dbContext.AnimalRequests
                join p in _dbContext.Projects on ar.JobCode equals p.ParentProject
                join a in _dbContext.Animals on ar.AnimalType equals a.AnimalType
                where projectCodes.Contains(ar.JobCode)
                select new
                {
                    ar.JobCode,
                    ar.NumberOfAnimals,
                    ar.NumberOfDays,
                    Cost = p.IsDefraProject == 0 ? a.DailyRate : a.DefraDailyRate
                })
                .ToListAsync();

            var animalCostByJob = animalCostsRaw
                .GroupBy(x => x.JobCode)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => (decimal)(x.NumberOfAnimals * x.NumberOfDays) * (x.Cost ?? 0m)));

            var staffMap = staffCosts
                .Where(e => e.sectorCharge == 1m)
                .GroupBy(x => x.JobCode)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => (decimal)x.PlannedHours * (x.ChargeRate ?? 0m) * x.sectorCharge));
            var additionalMap = additionalCosts.ToDictionary(x => x.JobCode, x => x.TotalAdditional);
            var testMap = testCosts.ToDictionary(x => x.JobCode, x => x.TotalTest);

            var results = projects.Select(p =>
                BuildVlaProfitabilityRow(p, staffMap, additionalMap, testMap, animalCostByJob)).ToList();

            results = ApplyVlaSorting(results, query.SortBy, query.Descending);

            return ApplyPaging(results, query.Page, query.PageSize);
        }

        private static ProjectProfitabilityVlaView BuildVlaProfitabilityRow(
            VlaProjectEntry p,
            Dictionary<string, decimal> staffMap,
            Dictionary<string, decimal> additionalMap,
            Dictionary<string, decimal> testMap,
            Dictionary<string, decimal> animalCostByJob)
        {
            var staff      = staffMap.TryGetValue(p.ParentProject!, out var s)  ? s  : 0m;
            var additional = additionalMap.TryGetValue(p.ParentProject!, out var a) ? a : 0m;
            var test       = testMap.TryGetValue(p.ParentProject!, out var t)   ? t  : 0m;
            var animal     = animalCostByJob.TryGetValue(p.ParentProject!, out var an) ? an : 0m;
            var total      = staff + additional + test + animal;
            var budget     = p.BudgetCvl ?? 0m;
            var profit     = budget - total;
            var targetProfit = p.ProgrammeTarget ?? 0m;
            return new ProjectProfitabilityVlaView
            {
                JobCode       = p.ParentProject!,
                Program       = p.Program,
                Customer      = p.Customer,
                Manager       = p.Manager,
                Status        = p.ProjectStatus,
                StaffCosts    = staff,
                TestCost      = test,
                AnimalCosts   = animal,
                AdditionalCosts = additional,
                TotalCosts    = total,
                Budget        = p.BudgetCvl,
                Profit        = profit,
                TargetProfit  = targetProfit,
                OffTarget     = profit - targetProfit
            };
        }

        public async Task<PagedData<ProjectStaffReplanView>> GetProjectStaffReplanAsync(PaginationParameters<string> query, string workgroup)
        {
            var baseQuery = (from proj in _dbContext.Projects
                             join sj in _dbContext.StaffJobs on proj.ParentProject equals sj.JobCode
                             join wge in _dbContext.WorkGroupEmployees on sj.StaffId equals wge.PactId
                             join emp in _dbContext.Employees on wge.SpNumber equals emp.SPNumber
                             join wgg in _dbContext.WorkgroupGrades on wge.WorkGroupGrade equals wgg.WgGrade
                             join wg in _dbContext.Workgroups on wgg.Workgroup equals wg.WorkGroupName
                             join pc in _dbContext.ProfitCentres on wg.ProfitCentre equals pc.ProfitCentreId
                             join upc in _dbContext.UserProfitcentres on pc.ProfitCentreId equals upc.ProfitCentre
                             join u in _dbContext.Users on upc.UserId equals u.UserId
                             where wgg.Workgroup == workgroup
                                && EF.Functions.ILike(u.UserEmail!, _requestContext.UserEmailId)
                             select new ProjectStaffReplanView
                             {
                                 WorkGroup = wgg.Workgroup,
                                 GradeCode = wgg.GradeCode,
                                 Name = (emp.LastName ?? string.Empty) + ", " +
                                                 (emp.FirstName ?? string.Empty),
                                 PlannedHours = sj.PlannedHours,
                                 ParentProject = proj.ParentProject,
                                 Program = proj.Program,
                                 WgGrade = wgg.WgGrade
                             }).Distinct();

            baseQuery = ApplyStaffReplanFilter(baseQuery, query.Filter);
            baseQuery = ApplyStaffReplanSorting(baseQuery, query.SortBy, query.Descending);

            var result = await baseQuery.AsNoTracking().ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        private static IQueryable<ProjectStaffReplanView> ApplyStaffReplanFilter(
            IQueryable<ProjectStaffReplanView> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("WorkGroup", out var workGroup) && workGroup != null)
                query = query.Where(x => EF.Functions.ILike(x.WorkGroup!, $"%{workGroup}%"));

            if (dict.TryGetValue("GradeCode", out var gradeCode) && gradeCode != null)
                query = query.Where(x => EF.Functions.ILike(x.GradeCode!, $"%{gradeCode}%"));

            if (dict.TryGetValue("Name", out var name) && name != null)
                query = query.Where(x => EF.Functions.ILike(x.Name!, $"%{name}%"));

            if (dict.TryGetValue("ParentProject", out var parentProject) && parentProject != null)
                query = query.Where(x => EF.Functions.ILike(x.ParentProject!, $"%{parentProject}%"));

            if (dict.TryGetValue("Program", out var program) && program != null)
                query = query.Where(x => EF.Functions.ILike(x.Program!, $"%{program}%"));

            return query;
        }

        private static IQueryable<ProjectStaffReplanView> ApplyStaffReplanSorting(
            IQueryable<ProjectStaffReplanView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(x => x.WorkGroup).ThenBy(x => x.Name);

            return sortBy.ToLower() switch
            {
                "workgroup" => descending ? query.OrderByDescending(x => x.WorkGroup) : query.OrderBy(x => x.WorkGroup),
                "gradecode" => descending ? query.OrderByDescending(x => x.GradeCode) : query.OrderBy(x => x.GradeCode),
                "name" => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                "plannedhours" => descending ? query.OrderByDescending(x => x.PlannedHours) : query.OrderBy(x => x.PlannedHours),
                "parentproject" => descending ? query.OrderByDescending(x => x.ParentProject) : query.OrderBy(x => x.ParentProject),
                "program" => descending ? query.OrderByDescending(x => x.Program) : query.OrderBy(x => x.Program),
                _ => query.OrderBy(x => x.WorkGroup).ThenBy(x => x.Name)
            };
        }
    }
}