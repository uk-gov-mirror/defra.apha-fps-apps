using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
namespace Apha.FPS.DataAccess.Repositories
{
    public class BudgetBidsRepository : BaseRepository, IBudgetBidsRepository
    {
        private readonly IFpsRequestContext _requestContext;

        public BudgetBidsRepository(FpsDbContext context, IFpsRequestContext requestContext) : base(context)
        {
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        private async Task ThrowIfNotOwnerAsync(string WorkGroupName)
        {
            var isOwner = await (
                from u in _context.Users
                join up in _context.UserProfitcentres on u.UserId equals up.UserId
                join w in _context.Workgroups on up.ProfitCentre equals w.ProfitCentre
                where w.WorkGroupName == WorkGroupName
                   && u.UserEmail != null
                   && u.UserEmail.ToLower() == _requestContext.UserEmailId.ToLower()
                select u
            ).AnyAsync();

            if (!isOwner)
                throw new UnauthorizedAccessException(
                    $"User does not have access to workgroup '{WorkGroupName}'.");
        }

        public async Task<List<BidView>> GetBidViewAsync(string workgroup)
        {
            var rows = await _context.BidViews
                .AsNoTracking()
                .Where(b => b.WorkGroupName == workgroup && b.UserEmail != null && b.UserEmail.ToLower() == _requestContext.UserEmailId.ToLower())
                .OrderBy(b => b.Account)
                .ToListAsync();

            return rows.DistinctBy(b => b.Account).ToList();
        }

        public async Task<PagedData<BidView>> GetBidViewPagedAsync(PaginationParameters<string> query, string workgroup)
        {
            var q = _context.BidViews
                .AsNoTracking()
                .Where(b => b.WorkGroupName == workgroup && b.UserEmail != null && b.UserEmail.ToLower() == _requestContext.UserEmailId.ToLower())
                .AsQueryable();

            q = ApplyBidViewFilter(q, query.Filter);
            q = ApplyBidViewSort(q, query.SortBy, query.Descending);

            var result = (await q.ToListAsync()).DistinctBy(b => b.Account).ToList();
            return base.ApplyPaging(result, query.Page > 0 ? query.Page : 1, query.PageSize > 0 ? query.PageSize : 10);
        }

        public async Task<Bid?> GetBidByIdAsync(string WorkGroupName, string account)
        {
            return await _context.Bids
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.WorkGroupName == WorkGroupName && b.Account == account);
        }

        public async Task<bool> HasRelatedPurchasesAsync(string WorkGroupName, string account)
        {
            return await _context.Purchases
                .AnyAsync(p => p.WorkGroupName == WorkGroupName && p.Account == account);
        }

        public async Task<Bid> AddBidAsync(Bid bid)
        {
            await ThrowIfNotOwnerAsync(bid.WorkGroupName);
            bid.FpsYear = _requestContext.FpsYear;

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Bids.Add(bid);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return bid;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<Bid> UpdateBidAsync(Bid bid)
        {
            await ThrowIfNotOwnerAsync(bid.WorkGroupName);

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var existing = await _context.Bids
                        .FirstOrDefaultAsync(b => b.WorkGroupName == bid.WorkGroupName && b.Account == bid.Account);

                    existing!.GenBid = bid.GenBid;

                    await _context.SaveChangesAsync();
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

        public async Task<bool> DeleteBidAsync(string WorkGroupName, string account)
        {
            await ThrowIfNotOwnerAsync(WorkGroupName);

            var entity = await _context.Bids
                .FirstOrDefaultAsync(b => b.WorkGroupName == WorkGroupName && b.Account == account);

            if (entity == null)
                return false;

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Bids.Remove(entity);
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

        public async Task<List<AccountCategory>> GetAccountCategoriesAsync()
        {
            return await _context.AccountCategories
                .AsNoTracking()
                .Where(a => a.RcSpecific == -1)
                .OrderBy(a => a.AccShortName)
                .ToListAsync();
        }

        public async Task<PagedData<GenericBidView>> GetGenericBidsPagedAsync(PaginationParameters<string> query)
        {
            var result = BuildGenericBidQuery();

            result = ApplyGenericBidFilter(result, query.Filter);
            result = ApplyGenericBidSort(result, query.SortBy, query.Descending);

            return base.ApplyPaging(result, query.Page > 0 ? query.Page : 1, query.PageSize > 0 ? query.PageSize : 10);
        }

        private IQueryable<GenericBidView> BuildGenericBidQuery()
        {
            return (from b in _context.Bids
                    join wg in _context.Workgroups on b.WorkGroupName equals wg.WorkGroupName
                    join ac in _context.AccountCategories on b.Account equals ac.AccShortName
                    select new GenericBidView
                    {
                        ProfitCentre = wg.ProfitCentre,
                        WorkGroupName = b.WorkGroupName,
                        Account = b.Account,
                        GenBid = b.GenBid,
                        SysTimeStamp = wg.SysTimestamp,
                        AccountType = ac.AccountType
                    }).Distinct();
        }

        private static IQueryable<GenericBidView> ApplyGenericBidFilter(IQueryable<GenericBidView> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            if (filter.TrimStart().StartsWith('{'))
            {
                try
                {
                    var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(filter,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dict != null)
                    {
                        if (dict.TryGetValue("ProfitCentre", out var pc) && !string.IsNullOrWhiteSpace(pc))
                            query = query.Where(b => EF.Functions.ILike(b.ProfitCentre, $"%{pc}%"));
                        if (dict.TryGetValue("WorkGroupName", out var wg) && !string.IsNullOrWhiteSpace(wg))
                            query = query.Where(b => EF.Functions.ILike(b.WorkGroupName, $"%{wg}%"));
                        if (dict.TryGetValue("Account", out var acc) && !string.IsNullOrWhiteSpace(acc))
                            query = query.Where(b => EF.Functions.ILike(b.Account, $"%{acc}%"));
                        if (dict.TryGetValue("AccountType", out var at) && !string.IsNullOrWhiteSpace(at))
                            query = query.Where(b => b.AccountType != null && EF.Functions.ILike(b.AccountType, $"%{at}%"));
                    }
                }
                catch { }
            }
            return query;
        }

        private static IQueryable<GenericBidView> ApplyGenericBidSort(IQueryable<GenericBidView> query, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "profitcentre"  => descending ? query.OrderByDescending(b => b.ProfitCentre)  : query.OrderBy(b => b.ProfitCentre),
                "workgroupname" => descending ? query.OrderByDescending(b => b.WorkGroupName) : query.OrderBy(b => b.WorkGroupName),
                "account"       => descending ? query.OrderByDescending(b => b.Account)       : query.OrderBy(b => b.Account),
                "genbid"        => descending ? query.OrderByDescending(b => b.GenBid)        : query.OrderBy(b => b.GenBid),
                "accounttype"   => descending ? query.OrderByDescending(b => b.AccountType)   : query.OrderBy(b => b.AccountType),
                _               => query.OrderBy(b => b.Account)
            };
        }

        private static IQueryable<BidView> ApplyBidViewFilter(IQueryable<BidView> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            if (!filter.TrimStart().StartsWith('{'))
                return query;

            var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(filter,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? [];

            if (dict.TryGetValue("WorkGroupName", out var wg) && !string.IsNullOrWhiteSpace(wg))
                query = query.Where(b => EF.Functions.ILike(b.WorkGroupName, $"%{wg}%"));

            if (dict.TryGetValue("Account", out var acc) && !string.IsNullOrWhiteSpace(acc))
                query = query.Where(b => EF.Functions.ILike(b.Account, $"%{acc}%"));

            return query;
        }

        private static IQueryable<BidView> ApplyBidViewSort(IQueryable<BidView> query, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "workgroupname" => descending ? query.OrderByDescending(b => b.WorkGroupName) : query.OrderBy(b => b.WorkGroupName),
                "account" => descending ? query.OrderByDescending(b => b.Account) : query.OrderBy(b => b.Account),
                "genbid" => descending ? query.OrderByDescending(b => b.GenBid) : query.OrderBy(b => b.GenBid),
                _ => query.OrderBy(b => b.Account)
            };
        }
    }
}
