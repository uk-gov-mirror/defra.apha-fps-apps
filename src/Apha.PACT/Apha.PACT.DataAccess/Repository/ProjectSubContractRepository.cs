using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;

namespace Apha.PACT.DataAccess.Repository
{
    public class ProjectSubContractRepository : BaseRepository, IProjectSubContractRepository
    {
        private readonly IFpsRequestContext _fpsRequestContext;

        public ProjectSubContractRepository(FpsDbContext context, IFpsRequestContext fpsRequestContext) : base(context)
        {
            _fpsRequestContext = fpsRequestContext;
        }       

        public async Task<PagedData<ProjectSubContract>> GetPagedProjectSubContractsAsync(PaginationParameters<string> query, string? project)
        {
            IQueryable<ProjectSubContract> querySubContracts = _context.ProjectSubContracts.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(project))
            {
                querySubContracts = querySubContracts.Where(s => s.Project.ToLower() == project.ToLower());
            }

            querySubContracts = ApplySubContractFilter(querySubContracts, query.Filter);
            querySubContracts = (IQueryable<ProjectSubContract>)ApplySorting(querySubContracts, query.SortBy, query.Descending);

            return await ApplyPaging(querySubContracts, query.Page, query.PageSize);
        }

        public async Task<decimal> GetTotalAmountAsync(string? project)
        {
            IQueryable<ProjectSubContract> query = _context.ProjectSubContracts.AsNoTracking();
            if (!string.IsNullOrEmpty(project))
                query = query.Where(s => s.Project == project);
            return (await query.SumAsync(s => s.Amount)) ?? 0m;
        }

        private static readonly IReadOnlyList<string> AnimalAcctCodes =
            new[] { "LargeAnimals", "SmallAnimals", "Mice" };

        public async Task<PagedData<ProjectSubContract>> GetFpsProjectSubContractsAsync(PaginationParameters<string> query, string? project, bool filterByAnimalAcctCodes = false)
        {
            IQueryable<ProjectSubContract> q = _context.ProjectSubContracts.AsNoTracking();

            if (filterByAnimalAcctCodes)
                q = q.Where(s => AnimalAcctCodes.Contains(s.AcctCode));
            else
                q = q.Where(s => !AnimalAcctCodes.Contains(s.AcctCode));

            if (!string.IsNullOrEmpty(project))
                q = q.Where(s => s.Project == project);

            q = ApplySubContractFilter(q, query.Filter);
            q = (IQueryable<ProjectSubContract>)ApplySorting(q, query.SortBy, query.Descending);

            return await ApplyPaging(q, query.Page, query.PageSize);
        }

        public async Task<decimal> GetFpsProjectSubContractTotalAmountAsync(string? project, bool filterByAnimalAcctCodes = false)
        {
            IQueryable<ProjectSubContract> q = _context.ProjectSubContracts.AsNoTracking();

            if (filterByAnimalAcctCodes)
                q = q.Where(s => AnimalAcctCodes.Contains(s.AcctCode));
            else
                q = q.Where(s => !AnimalAcctCodes.Contains(s.AcctCode));

            if (!string.IsNullOrEmpty(project))
                q = q.Where(s => s.Project == project);

            return (await q.SumAsync(s => s.Amount)) ?? 0m;
        }

        public async Task<ProjectSubContract?> GetByIdAsync(int subContCounter)
        {
            return await _context.ProjectSubContracts
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SubContCounter == subContCounter);
        }

        public async Task<ProjectSubContract> CreateAsync(ProjectSubContract entity)
        {
            entity.FpsYear = _fpsRequestContext.FpsYear;
            await _context.ProjectSubContracts.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<ProjectSubContract> UpdateAsync(ProjectSubContract entity)
        {
            entity.FpsYear = _fpsRequestContext.FpsYear;
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int subContCounter)
        {
            ProjectSubContract? entity = await _context.ProjectSubContracts
                .FirstOrDefaultAsync(s => s.SubContCounter == subContCounter && s.FpsYear == _fpsRequestContext.FpsYear);
            if (entity == null) return false;
            _context.ProjectSubContracts.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<MonthlySubContractsSummary>> GetMonthlySubContractsSummaryAsync(PaginationParameters<string> parameters)
        {
            IQueryable<MonthlySubContractsSummary> query = _context.MonthlySubContractsSummary.AsNoTracking();

            // Parse filter JSON from DataGrid: {"Program":"ADMIN","ParentProject":"AH"}
            if (!string.IsNullOrWhiteSpace(parameters.Filter))
            {
                dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(parameters.Filter);
                if (filterModel != null)
                {
                    IDictionary<string, object> dict = (IDictionary<string, object>)filterModel;

                    if (dict.TryGetValue("Program", out object? program) && program != null)
                        query = query.Where(x => EF.Functions.ILike(x.Program, $"%{program}%"));

                    if (dict.TryGetValue("ParentProject", out object? parentProject) && parentProject != null)
                        query = query.Where(x => EF.Functions.ILike(x.ParentProject, $"%{parentProject}%"));
                }
            }

            // Always order raw rows by Program, Project, Month so grouping is stable
            return await query
                .OrderBy(x => x.Program)
                .ThenBy(x => x.ParentProject)
                .ThenBy(x => x.Month)
                .ToListAsync();
        }

        public async Task<HashSet<string>> GetValidProjectsAsync()
        {
            var fpsYear = _fpsRequestContext.FpsYear;
            return await _context.Projects
                .AsNoTracking()
                .Where(p => p.FpsYear == fpsYear)
                .Select(p => p.ParentProject)
                .ToHashSetAsync(StringComparer.OrdinalIgnoreCase);
        }

        public int GetCurrentFpsYear() => _fpsRequestContext.FpsYear;

        public async Task<PagedData<SubContractRmsImportRow>> GetFailedSubContractRmsAsync(PaginationParameters<string> query, string importedBy)
        {
            var latestImportedDate = await _context.ProjectSubcontractStagings
                .AsNoTracking()
                .Where(x => x.ImportedBy == importedBy && x.IsPassed == false)
                .MaxAsync(x => x.ImportedDate);

            if (!latestImportedDate.HasValue)
            {
                return new PagedData<SubContractRmsImportRow>(
                    Array.Empty<SubContractRmsImportRow>(),                   
                    new PaginationData
                    {
                        PageNumber = query.Page,
                        PageSize = query.PageSize,
                        TotalRecords = 0,
                        TotalPages = 0
                    });
            }

            IQueryable<ProjectSubcontractStaging> failedQuery = _context.ProjectSubcontractStagings
                .AsNoTracking()
                .Where(x => x.ImportedBy == importedBy && x.IsPassed == false && x.ImportedDate == latestImportedDate.Value);

            failedQuery = ApplyFailedSubContractFilter(failedQuery, query.Filter);
            failedQuery = (IQueryable<ProjectSubcontractStaging>)ApplyFailedSubContractSorting(failedQuery, query.SortBy, query.Descending);

            IQueryable<SubContractRmsImportRow> rows = failedQuery
                .Select(x => new SubContractRmsImportRow
                {
                    Id = x.Id,
                    Project = x.Project,
                    TestJob = x.TestJob,
                    Month = x.Month,
                    Amount = x.Amount,
                    WorkGroup = x.WorkGroup,
                    AcctCode = x.AcctCode,
                    Supplier = x.Supplier,
                    Description = x.Description,
                    SupplierNumber = x.SupplierNumber,
                    DailyRate = x.DailyRate,
                    AnimalDays = x.AnimalDays,
                    ValidationFailure = x.ValidationFailure,
                    ImportedDate = x.ImportedDate
                });

            return await ApplyPaging(rows, query.Page, query.PageSize);
        }

        public async Task<int> DeleteFailedSubContractRmsByUserAsync(string importedBy)
        {
            var rows = await _context.ProjectSubcontractStagings
                .Where(x => x.ImportedBy == importedBy)
                .ToListAsync();

            if (rows.Count == 0)
            {
                return 0;
            }

            _context.ProjectSubcontractStagings.RemoveRange(rows);
            return await _context.SaveChangesAsync();
        }

        public async Task<SubContractRmsImportResult> ImportSubContractRmsAsync(List<ProjectSubContract> passedRows, List<ProjectSubcontractStaging> failedRows)
        {
            if (passedRows.Count == 0 && failedRows.Count == 0)
            {
                return new SubContractRmsImportResult { PassedCount = 0, FailedCount = 0 };
            }

            if (passedRows.Count > 0)
            {
                await _context.ProjectSubContracts.AddRangeAsync(passedRows);
            }

            if (failedRows.Count > 0)
            {
                await _context.ProjectSubcontractStagings.AddRangeAsync(failedRows);
            }

            await _context.SaveChangesAsync();

            return new SubContractRmsImportResult
            {
                PassedCount = passedRows.Count,
                FailedCount = failedRows.Count
            };
        }

        public async Task<ProjectSubcontractStaging?> GetFailedSubContractRmsByIdAsync(int id, string importedBy)
        {
            return await _context.ProjectSubcontractStagings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.ImportedBy == importedBy);
        }

        public async Task<bool> DeleteFailedSubContractRmsByIdAsync(int id, string importedBy)
        {
            var entity = await _context.ProjectSubcontractStagings
                .FirstOrDefaultAsync(s => s.Id == id && s.ImportedBy == importedBy);
            if (entity == null) return false;
            _context.ProjectSubcontractStagings.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static IQueryable<ProjectSubContract> ApplySubContractFilter(IQueryable<ProjectSubContract> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter)) return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null) return query;

            IDictionary<string, object> dict = (IDictionary<string, object>)filterModel;

            query = ApplyStringFilter(dict, "Project", query, (q, v) => q.Where(x => x.Project != null && EF.Functions.ILike(x.Project, v)));
            query = ApplyStringFilter(dict, "AcctCode", query, (q, v) => q.Where(x => x.AcctCode != null && EF.Functions.ILike(x.AcctCode, v)));
            query = ApplyStringFilter(dict, "TestJob", query, (q, v) => q.Where(x => x.TestJob != null && EF.Functions.ILike(x.TestJob, v)));
            query = ApplyStringFilter(dict, "WorkGroup", query, (q, v) => q.Where(x => x.WorkGroup != null && EF.Functions.ILike(x.WorkGroup, v)));
            query = ApplyStringFilter(dict, "Description", query, (q, v) => q.Where(x => x.Description != null && EF.Functions.ILike(x.Description, v)));
            query = ApplyStringFilter(dict, "Supplier", query, (q, v) => q.Where(x => x.Supplier != null && EF.Functions.ILike(x.Supplier, v)));
            query = ApplyMonthFilter(dict, query);
            query = ApplySupplierNumberFilter(dict, query);

            return query;
        }

        private static IQueryable<ProjectSubContract> ApplyStringFilter(
            IDictionary<string, object> dict,
            string key,
            IQueryable<ProjectSubContract> query,
            Func<IQueryable<ProjectSubContract>, string, IQueryable<ProjectSubContract>> applyWhere)
        {
            if (dict.TryGetValue(key, out object? value) && value != null)
                query = applyWhere(query, $"%{value}%");
            return query;
        }

        private static IQueryable<ProjectSubcontractStaging> ApplyFailedSubContractFilter(IQueryable<ProjectSubcontractStaging> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter)) return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null) return query;

            IDictionary<string, object> dict = (IDictionary<string, object>)filterModel;

            query = ApplyFailedStringFilter(dict, "Project", query, (q, v) => q.Where(x => x.Project != null && EF.Functions.ILike(x.Project, v)));
            query = ApplyFailedStringFilter(dict, "AcctCode", query, (q, v) => q.Where(x => x.AcctCode != null && EF.Functions.ILike(x.AcctCode, v)));
            query = ApplyFailedStringFilter(dict, "TestJob", query, (q, v) => q.Where(x => x.TestJob != null && EF.Functions.ILike(x.TestJob, v)));
            query = ApplyFailedStringFilter(dict, "WorkGroup", query, (q, v) => q.Where(x => x.WorkGroup != null && EF.Functions.ILike(x.WorkGroup, v)));
            query = ApplyFailedStringFilter(dict, "Description", query, (q, v) => q.Where(x => x.Description != null && EF.Functions.ILike(x.Description, v)));
            query = ApplyFailedStringFilter(dict, "Supplier", query, (q, v) => q.Where(x => x.Supplier != null && EF.Functions.ILike(x.Supplier, v)));
            query = ApplyFailedStringFilter(dict, "ValidationFailure", query, (q, v) => q.Where(x => x.ValidationFailure != null && EF.Functions.ILike(x.ValidationFailure, v)));
            query = ApplyFailedMonthFilter(dict, query);
            query = ApplyFailedSupplierNumberFilter(dict, query);

            return query;
        }

        private static IQueryable<ProjectSubcontractStaging> ApplyFailedStringFilter(
            IDictionary<string, object> dict,
            string key,
            IQueryable<ProjectSubcontractStaging> query,
            Func<IQueryable<ProjectSubcontractStaging>, string, IQueryable<ProjectSubcontractStaging>> applyWhere)
        {
            if (dict.TryGetValue(key, out object? value) && value != null)
                query = applyWhere(query, $"%{value}%");
            return query;
        }

        private static IQueryable<ProjectSubcontractStaging> ApplyFailedMonthFilter(IDictionary<string, object> dict, IQueryable<ProjectSubcontractStaging> query)
        {
            if (dict.TryGetValue("Month", out object? month) && month != null)
            {
                var monthValue = month.ToString();
                if (!string.IsNullOrWhiteSpace(monthValue))
                    query = query.Where(x => x.Month == monthValue);
            }
            return query;
        }

        private static IQueryable<ProjectSubcontractStaging> ApplyFailedSupplierNumberFilter(IDictionary<string, object> dict, IQueryable<ProjectSubcontractStaging> query)
        {
            if (dict.TryGetValue("SupplierNumber", out object? supplierNumber) && supplierNumber != null)
            {
                var supplierNumberValue = supplierNumber.ToString();
                if (!string.IsNullOrWhiteSpace(supplierNumberValue))
                    query = query.Where(x => x.SupplierNumber == supplierNumberValue);
            }
            return query;
        }

        private static IQueryable<ProjectSubContract> ApplyMonthFilter(IDictionary<string, object> dict, IQueryable<ProjectSubContract> query)
        {
            if (dict.TryGetValue("Month", out object? month) && month != null && int.TryParse(month.ToString(), out int monthValue))
                query = query.Where(x => (int?)x.Month == monthValue);
            return query;
        }

        private static IQueryable<ProjectSubContract> ApplySupplierNumberFilter(IDictionary<string, object> dict, IQueryable<ProjectSubContract> query)
        {
            if (dict.TryGetValue("SupplierNumber", out object? supplierNumber) && supplierNumber != null && int.TryParse(supplierNumber.ToString(), out int supplierNumberValue))
                query = query.Where(x => x.SupplierNumber == supplierNumberValue);
            return query;
        }

        private static IQueryable ApplySorting(IQueryable<ProjectSubContract> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query.OrderBy(e => e.SubContCounter);

            return ApplySortingByProperty(query, NormalizeSortProperty(sortBy), descending);
        }

        private static string NormalizeSortProperty(string property)
        {
            return new string(property.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
        }

        private static IQueryable ApplySortingByProperty(IQueryable<ProjectSubContract> query, string property, bool descending)
        {
            return property switch
            {
                "project" => ApplyOrder(query, s => s.Project, descending),
                "month" => ApplyOrder(query, s => s.Month, descending),
                "amount" => ApplyOrder(query, s => s.Amount, descending),
                "acctcode" => ApplyOrder(query, s => s.AcctCode, descending),
                "testjob" => ApplyOrder(query, s => s.TestJob, descending),
                "subcontcounter" or "counter" => ApplyOrder(query, s => s.SubContCounter, descending),
                "description" => ApplyOrder(query, s => s.Description, descending),
                "supplier" => ApplyOrder(query, s => s.Supplier, descending),
                "suppliernumber" => ApplyOrder(query, s => s.SupplierNumber, descending),
                "dailyrate" => ApplyOrder(query, s => s.DailyRate, descending),
                "animaldays" => ApplyOrder(query, s => s.AnimalDays, descending),
                _ => query.OrderBy(e => e.SubContCounter)
            };
        }

        private static IQueryable ApplyFailedSubContractSorting(IQueryable<ProjectSubcontractStaging> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query.OrderBy(x => x.Id);

            return sortBy.ToLower() switch
            {
                "id" => ApplyFailedOrder(query, s => s.Id, descending),
                "project" => ApplyFailedOrder(query, s => s.Project, descending),
                "testjob" => ApplyFailedOrder(query, s => s.TestJob, descending),
                "month" => ApplyFailedOrder(query, s => s.Month, descending),
                "amount" => ApplyFailedOrder(query, s => s.Amount, descending),
                "workgroup" => ApplyFailedOrder(query, s => s.WorkGroup, descending),
                "acctcode" => ApplyFailedOrder(query, s => s.AcctCode, descending),
                "supplier" => ApplyFailedOrder(query, s => s.Supplier, descending),
                "description" => ApplyFailedOrder(query, s => s.Description, descending),
                "suppliernumber" => ApplyFailedOrder(query, s => s.SupplierNumber, descending),
                "dailyrate" => ApplyFailedOrder(query, s => s.DailyRate, descending),
                "animaldays" => ApplyFailedOrder(query, s => s.AnimalDays, descending),
                "validationfailure" => ApplyFailedOrder(query, s => s.ValidationFailure, descending),
                "importeddate" => ApplyFailedOrder(query, s => s.ImportedDate, descending),
                _ => query.OrderBy(x => x.Id)
            };
        }        

        private static IQueryable ApplyOrder<T>(IQueryable<ProjectSubContract> query, Expression<Func<ProjectSubContract, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private static IQueryable ApplyFailedOrder<T>(IQueryable<ProjectSubcontractStaging> query, Expression<Func<ProjectSubcontractStaging, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }
    }
}
