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
    public class AdditionalCostRepository : BaseRepository, IAdditionalCostRepository
    {
        private readonly IFpsRequestContext _requestContext;

        public AdditionalCostRepository(FpsDbContext context, IFpsRequestContext requestContext) : base(context)
        {
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        public async Task<PagedData<AdditionalCost>> GetByJobCodeAsync(PaginationParameters<string> query, string jobCode)
        {
            var queryable = BuildAdditionalCostQuery(jobCode);

            queryable = (IQueryable<AdditionalCost>)ApplySorting(queryable, query.SortBy, query.Descending);
            queryable = ApplyAdditionalCostFilter(queryable, query.Filter);

            var records = await queryable.ToListAsync();

            return ApplyPaging(records, query.Page, query.PageSize);
        }

        public async Task<decimal> GetTotalItemCostAsync(string jobCode)
        {
            return await _context.AdditionalCosts
                .AsNoTracking()
                .Where(a => a.JobCode == jobCode)
                .SumAsync(a => a.ItemCost);
        }

        public async Task<List<AccountCategory>> GetAccountCategoriesAsync()
        {
            return await _context.AccountCategories
                .AsNoTracking()
                .Where(a => a.ProjectSpecific == -1)
                .Distinct()
                .OrderBy(a => a.AccShortName)
                .ToListAsync();
        }

        public async Task<AdditionalCost?> GetByIdAsync(string jobCode, string account, string description)
        {
            var jobCodeValue = (jobCode ?? string.Empty).Trim();
            var accountValue = (account ?? string.Empty).Trim();
            var descriptionValue = (description ?? string.Empty).Trim();

            return await _context.AdditionalCosts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.JobCode.Trim() == jobCodeValue
                                       && a.Account.Trim() == accountValue
                                       && a.Description.Trim() == descriptionValue);
        }

        public async Task<AdditionalCost> AddAsync(AdditionalCost additionalCost)
        {
            ArgumentNullException.ThrowIfNull(additionalCost);
            additionalCost.FpsYear = _requestContext.FpsYear;

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var logEntry = CreateAdditionalCostLogEntry(additionalCost, "I");

                    _context.AdditionalCosts.Add(additionalCost);
                    _context.AdditionalCostLogs.Add(logEntry);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return additionalCost;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<AdditionalCost> UpdateAsync(AdditionalCost additionalCost, string originalAccount, string originalDescription)
        {
            ArgumentNullException.ThrowIfNull(additionalCost);

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var jobCodeValue = (additionalCost.JobCode ?? string.Empty).Trim();
                    var originalAccountValue = (originalAccount ?? string.Empty).Trim();
                    var originalDescriptionValue = (originalDescription ?? string.Empty).Trim();

                    var existing = await _context.AdditionalCosts
                        .FirstOrDefaultAsync(a => a.JobCode.Trim() == jobCodeValue
                                               && a.Account.Trim() == originalAccountValue
                                               && a.Description.Trim() == originalDescriptionValue);

                    if (existing == null)
                        throw new InvalidOperationException(
                            $"Additional cost with JobCode {additionalCost.JobCode}, Account {originalAccount}, Description {originalDescription} not found");

                    var descriptionChanged = !string.Equals(
                        existing.Description, additionalCost.Description, StringComparison.OrdinalIgnoreCase);

                    var accountChanged = !string.Equals(
                        existing.Account, additionalCost.Account, StringComparison.OrdinalIgnoreCase);

                    AdditionalCost result;

                    if (descriptionChanged || accountChanged)
                    {
                        // Account and Description are part of the primary key, so the row
                        // must be recreated rather than updated in place.
                        var replacement = new AdditionalCost
                        {
                            JobCode = additionalCost.JobCode,
                            Account = additionalCost.Account,
                            Description = additionalCost.Description,
                            ItemCost = additionalCost.ItemCost,
                            Freq = additionalCost.Freq,
                            Supplier = additionalCost.Supplier,
                            FpsYear = _requestContext.FpsYear
                        };

                        var deleteLog = CreateAdditionalCostLogEntry(existing, "D");
                        var insertLog = CreateAdditionalCostLogEntry(replacement, "I");

                        _context.AdditionalCosts.Remove(existing);
                        _context.AdditionalCosts.Add(replacement);
                        _context.AdditionalCostLogs.Add(deleteLog);
                        _context.AdditionalCostLogs.Add(insertLog);

                        result = replacement;
                    }
                    else
                    {
                        existing.ItemCost = additionalCost.ItemCost;
                        existing.Freq = additionalCost.Freq;
                        existing.Supplier = additionalCost.Supplier;
                        existing.FpsYear = _requestContext.FpsYear;

                        var logEntry = CreateAdditionalCostLogEntry(existing, "U");
                        _context.AdditionalCostLogs.Add(logEntry);

                        result = existing;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<bool> DeleteAsync(string jobCode, string account, string description)
        {
            var entity = await _context.AdditionalCosts
                .FirstOrDefaultAsync(a => a.JobCode == jobCode && a.Account == account && a.Description == description);

            if (entity == null)
                return false;

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var logEntry = CreateAdditionalCostLogEntry(entity, "D");

                    _context.AdditionalCosts.Remove(entity);
                    _context.AdditionalCostLogs.Add(logEntry);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private AdditionalCostLog CreateAdditionalCostLogEntry(AdditionalCost additionalCost, string insertDelete)
        {
            return new AdditionalCostLog
            {
                JobCode = additionalCost.JobCode,
                Account = additionalCost.Account,
                Description = additionalCost.Description,
                ItemCost = additionalCost.ItemCost,
                Freq = additionalCost.Freq,
                Supplier = additionalCost.Supplier,
                DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UserId = _requestContext.UserEmailId,
                InsertDelete = insertDelete,
                FpsYear = _requestContext.FpsYear
            };
        }

        private IQueryable<AdditionalCost> BuildAdditionalCostQuery(string jobCode)
        {
           return _context.AdditionalCostViews
                .AsNoTracking()
                .Where(a => a.JobCode == jobCode 
                    && a.UserEmail != null 
                    && a.UserEmail.ToLower() == _requestContext.UserEmailId)
                .Select(a => new AdditionalCost
                {
                    JobCode = a.JobCode,
                    Account = a.Account,
                    Description = a.Description,
                    ItemCost = a.ItemCost,
                    Freq = a.Freq,
                    Supplier = a.Supplier,
                    FpsYear = a.FpsYear
                })
                .OrderBy(a => a.Description)
                .AsQueryable();           
        }

        private static IQueryable ApplySorting(IQueryable<AdditionalCost> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query;
            }

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<AdditionalCost> query, string property, bool descending)
        {
            return property switch
            {
                "description" => ApplyOrder(query, i => i.Description, descending),
                "account"     => ApplyOrder(query, i => i.Account, descending),
                "itemcost"    => ApplyOrder(query, i => i.ItemCost, descending),
                "freq"        => ApplyOrder(query, i => i.Freq, descending),
                "supplier"    => ApplyOrder(query, i => i.Supplier, descending),
                _             => query
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<AdditionalCost> query, Expression<Func<AdditionalCost, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private static IQueryable<AdditionalCost> ApplyAdditionalCostFilter(IQueryable<AdditionalCost> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return query;
            }

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
            {
                return query;
            }

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("Description", out var description) && description != null)
                query = query.Where(x => EF.Functions.ILike(x.Description, $"%{description}%"));

            if (dict.TryGetValue("Account", out var account) && account != null)
                query = query.Where(x => EF.Functions.ILike(x.Account, $"%{account}%"));

            if (dict.TryGetValue("Supplier", out var supplier) && supplier != null)
                query = query.Where(x => EF.Functions.ILike(x.Supplier!, $"%{supplier}%"));

            return query;
        }
    }
}
