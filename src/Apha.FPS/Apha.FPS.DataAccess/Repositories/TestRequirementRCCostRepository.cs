using System.Dynamic;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.FPS.DataAccess.Repositories
{
    public class TestRequirementRCCostRepository : BaseRepository, ITestRequirementRCCostRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public TestRequirementRCCostRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }
         
        public async Task<PagedData<TestRequirementRCCost>> GetPagedByTestCodeAsync(
            PaginationParameters<string> query, string testCode)
        {
            var q = _dbContext.TestRequirementRCCosts
                .AsNoTracking()
                .Where(e => e.TestCode == testCode);

            q = ApplyFilter(q, query.Filter);
            q = ApplySort(q, query.SortBy, query.Descending);

            var page = Math.Max(query.Page, 1);
            var pageSize = Math.Max(query.PageSize, 10);

            var result = await q.ToListAsync();
            return base.ApplyPaging(result, page, pageSize);
        }

        public async Task<IEnumerable<TestRequirementRCCost>> GetByTestCodeAsync(string testCode)
        {
            return await _dbContext.TestRequirementRCCosts
                .AsNoTracking()
                .Where(e => e.TestCode == testCode)
                .OrderBy(e => e.Buyer)
                .ThenBy(e => e.ProfitCentre)
                .ToListAsync();
        }

        public async Task<TestRequirementRCCost?> GetByKeyAsync(
            string testCode, string buyer, string profitCentre)
        {
            return await _dbContext.TestRequirementRCCosts
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.TestCode == testCode
                                       && e.Buyer == buyer
                                       && e.ProfitCentre == profitCentre);
        }

        public async Task<bool> ExistsAsync(string testCode, string buyer, string profitCentre)
        {
            return await _dbContext.TestRequirementRCCosts
                .AnyAsync(e => e.TestCode == testCode
                            && e.Buyer == buyer
                            && e.ProfitCentre == profitCentre);
        }

        public async Task<TestRequirementRCCost> AddAsync(TestRequirementRCCost testRequirementRCCost)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    _dbContext.TestRequirementRCCosts.Add(testRequirementRCCost);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return testRequirementRCCost;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<TestRequirementRCCost> UpdateAsync(TestRequirementRCCost testRequirementRCCost)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _dbContext.TestRequirementRCCosts
                        .FirstOrDefaultAsync(e => e.TestCode == testRequirementRCCost.TestCode
                                               && e.Buyer == testRequirementRCCost.Buyer
                                               && e.ProfitCentre == testRequirementRCCost.ProfitCentre
                                               && e.FpsYear == testRequirementRCCost.FpsYear);

                    if (existing == null)
                        throw new KeyNotFoundException(
                            $"TestRequirementRCCost not found: TestCode='{testRequirementRCCost.TestCode}', " +
                            $"Buyer='{testRequirementRCCost.Buyer}', " +
                            $"ProfitCentre='{testRequirementRCCost.ProfitCentre}', " +
                            $"FpsYear={testRequirementRCCost.FpsYear}");

                    existing.Price = testRequirementRCCost.Price;

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return existing;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<bool> DeleteAsync(string testCode, string buyer, string profitCentre)
        {
            var entity = await _dbContext.TestRequirementRCCosts
                .FirstOrDefaultAsync(e => e.TestCode == testCode
                                       && e.Buyer == buyer
                                       && e.ProfitCentre == profitCentre);

            if (entity == null)
                return false;

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    _dbContext.TestRequirementRCCosts.Remove(entity);
                    await _dbContext.SaveChangesAsync();
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

        private static IQueryable<TestRequirementRCCost> ApplyFilter(IQueryable<TestRequirementRCCost> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("TestCode", out var testCode) && testCode != null)
                query = query.Where(x => EF.Functions.ILike(x.TestCode, $"%{testCode}%"));

            if (dict.TryGetValue("Buyer", out var buyer) && buyer != null)
                query = query.Where(x => x.Buyer != null && EF.Functions.ILike(x.Buyer, $"%{buyer}%"));

            if (dict.TryGetValue("ProfitCentre", out var profitCentre) && profitCentre != null)
                query = query.Where(x => x.ProfitCentre != null && EF.Functions.ILike(x.ProfitCentre, $"%{profitCentre}%"));

            if (dict.TryGetValue("Price", out var priceValue) && priceValue != null && decimal.TryParse(priceValue.ToString(), out var price))
                query = query.Where(x => x.Price == price);

            return query;
        }

        private static IQueryable<TestRequirementRCCost> ApplySort(IQueryable<TestRequirementRCCost> query, string? sortBy, bool descending)
        {
            return sortBy?.ToLowerInvariant() switch
            {
                "testcode" => descending ? query.OrderByDescending(x => x.TestCode) : query.OrderBy(x => x.TestCode),
                "buyer" => descending ? query.OrderByDescending(x => x.Buyer) : query.OrderBy(x => x.Buyer),
                "profitcentre" => descending ? query.OrderByDescending(x => x.ProfitCentre) : query.OrderBy(x => x.ProfitCentre),
                "price" => descending ? query.OrderByDescending(x => x.Price) : query.OrderBy(x => x.Price),
                _ => descending
                    ? query.OrderByDescending(x => x.Buyer).ThenByDescending(x => x.ProfitCentre)
                    : query.OrderBy(x => x.Buyer).ThenBy(x => x.ProfitCentre)
            };
        }
    }
}
