using System.Dynamic;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.FPS.DataAccess.Repositories
{
    public class TestRCCostRepository : BaseRepository, ITestRCCostRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public TestRCCostRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        public async Task<PagedData<TestRCCost>> GetPagedByTestCodeAsync(
            PaginationParameters<string> query, string testCode)
        {
            var q = _dbContext.TestRCCosts
                .AsNoTracking()
                .Where(e => e.TestCode == testCode);

            q = ApplyFilter(q, query.Filter);
            q = ApplySort(q, query.SortBy, query.Descending);

            var page = Math.Max(query.Page, 1);
            var pageSize = Math.Max(query.PageSize, 10);

            var result = await q.ToListAsync();
            return base.ApplyPaging(result, page, pageSize);
        }

    

        public async Task<IEnumerable<TestRCCost>> GetByTestCodeAsync(string testCode)
        {
            return await _dbContext.TestRCCosts
                .AsNoTracking()
                .Where(e => e.TestCode == testCode)
                .OrderBy(e => e.ProfitCentre)
                .ToListAsync();
        }

        public async Task<TestRCCost?> GetByKeyAsync(string testCode, string profitCentre)
        {
            return await _dbContext.TestRCCosts
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.TestCode == testCode
                                       && e.ProfitCentre == profitCentre);
        }

        public async Task<bool> ExistsAsync(string testCode, string profitCentre)
        {
            return await _dbContext.TestRCCosts
                .AnyAsync(e => e.TestCode == testCode
                            && e.ProfitCentre == profitCentre);
        }

        public async Task<TestRCCost> AddAsync(TestRCCost testRCCost)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    _dbContext.TestRCCosts.Add(testRCCost);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return testRCCost;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<TestRCCost> UpdateAsync(TestRCCost testRCCost)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _dbContext.TestRCCosts
                        .FirstOrDefaultAsync(e => e.TestCode == testRCCost.TestCode
                                               && e.ProfitCentre == testRCCost.ProfitCentre
                                               && e.FpsYear == testRCCost.FpsYear);

                    if (existing == null)
                        throw new KeyNotFoundException(
                            $"TestRCCost not found: TestCode='{testRCCost.TestCode}', " +
                            $"ProfitCentre='{testRCCost.ProfitCentre}', FpsYear={testRCCost.FpsYear}");

                    existing.Price = testRCCost.Price;

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

        public async Task<bool> DeleteAsync(string testCode, string profitCentre)
        {
            var entity = await _dbContext.TestRCCosts
                .FirstOrDefaultAsync(e => e.TestCode == testCode
                                       && e.ProfitCentre == profitCentre);

            if (entity == null)
                return false;

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    _dbContext.TestRCCosts.Remove(entity);
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

        private static IQueryable<TestRCCost> ApplyFilter(IQueryable<TestRCCost> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("TestCode", out var testCode) && testCode != null)
                query = query.Where(x => EF.Functions.ILike(x.TestCode, $"%{testCode}%"));

            if (dict.TryGetValue("ProfitCentre", out var profitCentre) && profitCentre != null)
                query = query.Where(x => x.ProfitCentre != null && EF.Functions.ILike(x.ProfitCentre, $"%{profitCentre}%"));

            if (dict.TryGetValue("Price", out var priceValue) && priceValue != null && decimal.TryParse(priceValue.ToString(), out var price))
                query = query.Where(x => x.Price == price);

            return query;
        }

        private static IQueryable<TestRCCost> ApplySort(IQueryable<TestRCCost> query, string? sortBy, bool descending)
        {
            return sortBy?.ToLowerInvariant() switch
            {
                "testcode" => descending ? query.OrderByDescending(x => x.TestCode) : query.OrderBy(x => x.TestCode),
                "profitcentre" => descending ? query.OrderByDescending(x => x.ProfitCentre) : query.OrderBy(x => x.ProfitCentre),
                "price" => descending ? query.OrderByDescending(x => x.Price) : query.OrderBy(x => x.Price),
                _ => descending ? query.OrderByDescending(x => x.ProfitCentre) : query.OrderBy(x => x.ProfitCentre)
            };
        }
    }
}
