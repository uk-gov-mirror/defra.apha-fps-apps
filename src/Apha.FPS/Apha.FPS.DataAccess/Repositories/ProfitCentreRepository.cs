using Apha.FPS.Core.Enums;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;

namespace Apha.FPS.DataAccess.Repositories
{
    public class ProfitCentreRepository : BaseRepository, IProfitCentreRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public ProfitCentreRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

        public async Task<List<ProfitCentreView>> GetProfitCentresAsync()
        {
            var rows = await _dbContext.ProfitCentreViews
                .AsNoTracking()
                .Where(x => x.UserEmail != null
                         && x.UserEmail.ToLower() == _requestContext.UserEmailId)
                .OrderBy(x => x.ProfitCentreId)
                .ToListAsync();

            // The underlying view joins profit centres with user-permissions rows, which can
            // produce duplicate ProfitCentreId entries when a user holds multiple permission
            // assignments for the same centre.  Deduplicate in memory after the ordered fetch
            // so the dropdown only shows each Resource Center once.
            return rows
                .GroupBy(x => x.ProfitCentreId)
                .Select(g => g.First())
                .ToList();
        }

        public async Task<IEnumerable<ProfitCentre>> GetAllProfitCentresAsync()
        {
            return await _context.ProfitCentres
                .AsNoTracking()
                .OrderBy(p => p.ProfitCentreId)
                .ToListAsync();
        }

        public async Task<PagedData<ProfitCentre>> GetAllProfitCentresPagedAsync(PaginationParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var profitCentresQuery = _dbContext.ProfitCentres
                .AsNoTracking()
                .AsQueryable()
                .Distinct();

            profitCentresQuery = ApplyProfitCentreFilter(profitCentresQuery, query.Filter);
            profitCentresQuery = ApplyProfitCentreSorting(profitCentresQuery, query.SortBy, query.Descending);

            var profitCentres = await profitCentresQuery.ToListAsync();
            return ApplyPaging(profitCentres, query.Page, query.PageSize);
        }

        public async Task<ProfitCentre?> GetProfitCentreByIdAsync(string profitCentreId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentreId);

            var normalised = profitCentreId.ToLower();
            return await _dbContext.ProfitCentres
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProfitCentreId.ToLower() == normalised);
        }

        public async Task<ProfitCentre> CreateProfitCentreAsync(ProfitCentre profitCentre)
        {
            ArgumentNullException.ThrowIfNull(profitCentre);

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    _dbContext.ProfitCentres.Add(profitCentre);
                    await _dbContext.SaveChangesAsync();

                    var currentUser = await _dbContext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UserEmail != null && u.UserEmail.ToLower() == _requestContext.UserEmailId);

                    var currentUserId = currentUser?.UserId ?? (int)SuperUser.SuperUserId;

                    var systemUserAlreadyExists = await _dbContext.UserProfitcentres
                        .IgnoreQueryFilters()
                        .AnyAsync(upc => upc.ProfitCentre == profitCentre.ProfitCentreId && upc.UserId == currentUserId);

                    if (!systemUserAlreadyExists)
                    {
                        _dbContext.UserProfitcentres.Add(new UserProfitcentre
                        {
                            ProfitCentre = profitCentre.ProfitCentreId,
                            UserId = currentUserId,
                            FpsYear = _requestContext.FpsYear
                        });
                    }

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return profitCentre;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<ProfitCentre> UpdateProfitCentreAsync(string originalProfitCentreId, ProfitCentre profitCentre)
        {
            ArgumentNullException.ThrowIfNull(profitCentre);
            ArgumentException.ThrowIfNullOrWhiteSpace(originalProfitCentreId);

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existingProfitCentre = await _dbContext.ProfitCentres
                        .FirstOrDefaultAsync(p => p.ProfitCentreId == originalProfitCentreId);

                    if (existingProfitCentre == null)
                        return profitCentre;

                    existingProfitCentre.ProfitCentreName = profitCentre.ProfitCentreName;
                    existingProfitCentre.Division = profitCentre.Division;
                    existingProfitCentre.ContTarget = profitCentre.ContTarget;
                    existingProfitCentre.ProfitCentreHead = profitCentre.ProfitCentreHead;
                    existingProfitCentre.DivisionId = profitCentre.DivisionId;
                    existingProfitCentre.EmailRecipient = profitCentre.EmailRecipient;

                    var currentUser = await _dbContext.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UserEmail != null && u.UserEmail.ToLower() == _requestContext.UserEmailId);

                    var currentUserId = currentUser?.UserId ?? (int)SuperUser.SuperUserId;

                    var userAlreadyLinked = await _dbContext.UserProfitcentres
                        .IgnoreQueryFilters()
                        .AnyAsync(upc => upc.ProfitCentre == originalProfitCentreId && upc.UserId == currentUserId);

                    if (!userAlreadyLinked)
                    {
                        _dbContext.UserProfitcentres.Add(new UserProfitcentre
                        {
                            ProfitCentre = originalProfitCentreId,
                            UserId = currentUserId,
                            FpsYear = _requestContext.FpsYear
                        });
                    }

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return existingProfitCentre;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<bool> DeleteProfitCentreAsync(string profitCentreId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentreId);

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var profitCentre = await _dbContext.ProfitCentres
                        .FirstOrDefaultAsync(p => p.ProfitCentreId == profitCentreId);

                    if (profitCentre == null)
                        return false;

                    // CASCADE: delete from tblUser_ProfitCentre
                    var userProfitCentres = await _dbContext.UserProfitcentres
                        .IgnoreQueryFilters()
                        .Where(upc => upc.ProfitCentre == profitCentreId)
                        .ToListAsync();

                    _dbContext.UserProfitcentres.RemoveRange(userProfitCentres);
                    _dbContext.ProfitCentres.Remove(profitCentre);
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

        public async Task<bool> HasLinkedGradesAsync(string profitCentreId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentreId);

            return await _dbContext.ProfitCentreGrades
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(pcg => pcg.ProfitCentre == profitCentreId);
        }

        public async Task<bool> HasLinkedWorkgroupsAsync(string profitCentreId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentreId);

            return await _dbContext.Workgroups
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(wg => wg.ProfitCentre == profitCentreId);
        }

        public async Task<bool> ProfitCentreExistsAsync(string profitCentreId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentreId);

            var normalised = profitCentreId.ToLower();
            return await _dbContext.ProfitCentres
                .AsNoTracking()
                .AnyAsync(p => p.ProfitCentreId.ToLower() == normalised);
        }

        public async Task<bool> UpdateProfitCentreSettingsAsync(string profitCentre, int timesheet, int outputsheet, short timesheetlayout)
        {
            var entities = await _context.ProfitCentres
                .Where(p => p.ProfitCentreId == profitCentre)
                .ToListAsync();

            foreach (var p in entities)
            {
                p.Timesheet = timesheet;
                p.OutputSheet = outputsheet;
                p.TimesheetLayout = timesheetlayout;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<IEnumerable<ProfitCentreCostSummary>> GetProfitCenterCostSummaryAsync(double monthNumber)
        {
            // Fetch data with basic filtering, then perform calculation in memory
            var query = from tcc in _dbContext.TimeCostCalcs
                        join wg in _dbContext.Workgroups on tcc.WorkGroup equals wg.WorkGroupName
                        where tcc.Class == "Charge" && tcc.Month <= monthNumber
                        select new
                        {
                            wg.ProfitCentre,
                            tcc.ChargeRate,
                            tcc.Time
                        };

            var data = await query.ToListAsync();

            // Group and calculate in memory to avoid PostgreSQL type casting issues
            var result = data
                .GroupBy(x => x.ProfitCentre)
                .Select(g => new ProfitCentreCostSummary
                {
                    ProfitCentre = g.Key,
                    Cost = g.Sum(x => (x.ChargeRate ?? 0m) * (decimal)(x.Time ?? 0))
                })
                .ToList();

            return result;
        }

        public async Task<PagedData<ProfitCentreCostSummary>> GetPagedProfitCenterCostSummaryAsync(
            PaginationParameters<string> parameters, double monthNumber)
        {
            ArgumentNullException.ThrowIfNull(parameters);

            // Get all data first
            var allData = (await GetProfitCenterCostSummaryAsync(monthNumber)).ToList();

            // Apply sorting
            var sortedData = parameters.SortBy?.ToLower() switch
            {
                "profitcentre" => parameters.Descending
                    ? allData.OrderByDescending(x => x.ProfitCentre)
                    : allData.OrderBy(x => x.ProfitCentre),
                "cost" => parameters.Descending
                    ? allData.OrderByDescending(x => x.Cost)
                    : allData.OrderBy(x => x.Cost),
                _ => allData.OrderBy(x => x.ProfitCentre)
            };

            // Use base repository ApplyPaging helper to create PagedData<T>
            return ApplyPaging(sortedData, parameters.Page, parameters.PageSize);
        }

        private static IQueryable<ProfitCentre> ApplyProfitCentreFilter(IQueryable<ProfitCentre> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("ProfitCentreId", out var profitCentreId) && profitCentreId != null)
            {
                var filterValue = profitCentreId.ToString();
                if (!string.IsNullOrWhiteSpace(filterValue))
                    query = query.Where(p => p.ProfitCentreId.Contains(filterValue));
            }

            if (dict.TryGetValue("ProfitCentreName", out var profitCentreName) && profitCentreName != null)
            {
                var filterValue = profitCentreName.ToString();
                if (!string.IsNullOrWhiteSpace(filterValue))
                    query = query.Where(p => p.ProfitCentreName.Contains(filterValue));
            }

            if (dict.TryGetValue("Division", out var division) && division != null)
            {
                var filterValue = division.ToString();
                if (!string.IsNullOrWhiteSpace(filterValue))
                    query = query.Where(p => p.Division.Contains(filterValue));
            }

            return query;
        }

        private static IQueryable<ProfitCentre> ApplyProfitCentreSorting(IQueryable<ProfitCentre> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query.OrderBy(p => p.ProfitCentreId);

            return sortBy switch
            {
                "ProfitCentreName" => descending ? query.OrderByDescending(p => p.ProfitCentreName) : query.OrderBy(p => p.ProfitCentreName),
                "Division" => descending ? query.OrderByDescending(p => p.Division) : query.OrderBy(p => p.Division),
                "ContTarget" => descending ? query.OrderByDescending(p => p.ContTarget) : query.OrderBy(p => p.ContTarget),
                "ProfitCentreHead" => descending ? query.OrderByDescending(p => p.ProfitCentreHead) : query.OrderBy(p => p.ProfitCentreHead),
                _ => descending ? query.OrderByDescending(p => p.ProfitCentreId) : query.OrderBy(p => p.ProfitCentreId),
            };
        }
    }
}
