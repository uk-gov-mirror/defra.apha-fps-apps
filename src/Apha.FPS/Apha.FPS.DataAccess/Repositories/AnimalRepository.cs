/*
 * TRANSFORMENGINE MIGRATION — AnimalRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - Added GetAnimalCostByAnimalTypeAsync(PaginationParameters<string> query, string animalType)
 *     to implement IAnimalRepository.GetAnimalCostByAnimalTypeAsync added in Phase 2
 *   - Added BuildAnimalCostByAnimalTypeQuery(string animalType) private helper that mirrors
 *     BuildAnimalCostQuery but discriminates on AnimalType instead of JobCode; used by ASU View
 *
 * PRESERVED:
 *   - All existing Animal Master CRUD methods: GetAllAnimalsAsync (both overloads),
 *     GetAnimalByIdAsync, AddAnimalAsync, UpdateAnimalAsync, DeleteAnimalAsync
 *   - All existing Animal Cost methods: GetAnimalCostAsync, GetTotalAnimalCostAsync,
 *     GetAnimalCostViewByIdAsync, GetAnimalRateByIdAsync, AddAnimalCostAsync,
 *     UpdateAnimalCostAsync, DeleteJobAnimalCostAsync
 *   - All private helper methods: BuildAnimalCostQuery, ApplyAnimalCostFilter, ApplySorting,
 *     ApplySortingByProperty, ApplyOrder<T>, ApplyAnimalMasterSorting,
 *     ApplyAnimalSortingByProperty, ApplyAnimalMasterOrder<T>, ApplyAnimalFilter,
 *     CreateAnimalRequestLogEntry
 *   - Transaction strategy pattern (CreateExecutionStrategy + BeginTransactionAsync) on all write ops
 *   - AsNoTracking pattern on all read ops
 *   - IFpsRequestContext injection for FpsYear and UserEmailId scoping
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: IFpsRequestContext.UserEmailId case-sensitivity — BuildAnimalCostQuery
 *     and BuildAnimalCostByAnimalTypeQuery use .ToLower() comparison against stored UserEmail;
 *     verify email values are always persisted in lowercase in the database
 */

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
    public class AnimalRepository : BaseRepository, IAnimalRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public AnimalRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        public async Task<List<Animal>> GetAnimalLookup() => await _dbContext.Animals.ToListAsync();

        // Animal Master CRUD
        public async Task<IEnumerable<Animal>> GetAllAnimalsAsync()
        {
            return await _dbContext.Animals
                .AsNoTracking()
                .Where(a => a.FpsYear == _requestContext.FpsYear)
                .OrderBy(a => a.AnimalType)
                .ToListAsync();
        }

        public async Task<PagedData<Animal>> GetAllAnimalsAsync(PaginationParameters<string> query)
        {
            var animalQuery = _dbContext.Animals
                .AsNoTracking()
                .Where(a => a.FpsYear == _requestContext.FpsYear)
                .AsQueryable();

            animalQuery = ApplyAnimalFilter(animalQuery, query.Filter);

            animalQuery = ApplyAnimalMasterSorting(animalQuery, query.SortBy, query.Descending);

            var result = await animalQuery.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<Animal?> GetAnimalByIdAsync(string animalType)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(animalType);
            return await _dbContext.Animals
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AnimalType == animalType && a.FpsYear == _requestContext.FpsYear);
        }

        public async Task<Animal> AddAnimalAsync(Animal entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            entity.FpsYear = _requestContext.FpsYear;

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    _dbContext.Animals.Add(entity);
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

        public async Task<Animal> UpdateAnimalAsync(Animal entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            entity.FpsYear = _requestContext.FpsYear;

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    _dbContext.Animals.Update(entity);
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

        public async Task<bool> DeleteAnimalAsync(string animalType)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(animalType);
            var entity = await _dbContext.Animals
                .FirstOrDefaultAsync(a => a.AnimalType == animalType && a.FpsYear == _requestContext.FpsYear);

            if (entity == null)
                return false;

            _dbContext.Animals.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }



        public async Task<PagedData<AnimalCostView>> GetAnimalCostAsync(PaginationParameters<string> query, string jobCode)
        {
            var queryAnimalCost = BuildAnimalCostQuery(jobCode);

            queryAnimalCost = ApplyAnimalCostFilter(queryAnimalCost, query.Filter);

            queryAnimalCost = (IQueryable<AnimalCostView>)ApplySorting(queryAnimalCost, query.SortBy, query.Descending);

            var result = await queryAnimalCost.ToListAsync();

            var animalCostViews = result.Select(e =>
            {
                e.AnimalCost = (decimal)e.NumberOfDays * (decimal)e.NumberOfAnimals * (e.DailyRate ?? 0m);
                return e;
            }).ToList();

            return base.ApplyPaging(animalCostViews, query.Page, query.PageSize);
        }

        public async Task<decimal> GetTotalAnimalCostAsync(string jobCode)
        {
            var result = await BuildAnimalCostQuery(jobCode).ToListAsync();
            return result.Sum(e => (decimal)e.NumberOfDays * (decimal)e.NumberOfAnimals * (e.DailyRate ?? 0m));
        }

        public async Task<AnimalCostView?> GetAnimalCostViewByIdAsync(int indCounter, string jobCode)
        {
            var record = await BuildAnimalCostQuery(jobCode)
                .Where(e => e.IndCounter == indCounter)
                .FirstOrDefaultAsync();

            if (record == null) return null;
            record.AnimalCost = (decimal)record.NumberOfDays * (decimal)record.NumberOfAnimals * (record.DailyRate ?? 0m);
            return record;
        }

        public async Task<decimal?> GetAnimalRateByIdAsync(string animalType, string jobCode)
        {

            var IsDefraProject = await _dbContext.Projects.Where(e => e.ParentProject == jobCode).Select(p => p.IsDefraProject).FirstOrDefaultAsync();

            var queryAnimalCost = from animal in _dbContext.Animals
                                  where animal.AnimalType == animalType
                                  select IsDefraProject == -1 ? animal.DefraDailyRate : animal.DailyRate;
            return await queryAnimalCost.FirstOrDefaultAsync();
        }

        public async Task<AnimalRequest> AddAnimalCostAsync(AnimalRequest animalReq)
        {
            ArgumentNullException.ThrowIfNull(animalReq);
            animalReq.FpsYear = _requestContext.FpsYear;

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var logEntry = CreateAnimalRequestLogEntry(animalReq, "I");

                    _dbContext.AnimalRequests.Add(animalReq);
                    _dbContext.AnimalRequestLogs.Add(logEntry);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return animalReq;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<AnimalRequest> UpdateAnimalCostAsync(AnimalRequest animalReq)
        {
            ArgumentNullException.ThrowIfNull(animalReq);

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existingEntity = await _dbContext.AnimalRequests.FindAsync(animalReq.IndCounter, _requestContext.FpsYear);

                    if (existingEntity == null)
                        throw new InvalidOperationException(
                            $"Animal cost with AnimalType {animalReq.AnimalType} not found");

                    existingEntity.JobCode = animalReq.JobCode;
                    existingEntity.AnimalType = animalReq.AnimalType;
                    existingEntity.NumberOfDays = animalReq.NumberOfDays;
                    existingEntity.NumberOfAnimals = animalReq.NumberOfAnimals;
                    existingEntity.FpsYear = _requestContext.FpsYear;

                    var logEntry = CreateAnimalRequestLogEntry(existingEntity, "U");

                    _dbContext.AnimalRequestLogs.Add(logEntry);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return existingEntity;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<bool> DeleteJobAnimalCostAsync(int indCounter)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(indCounter);

            var entity = await _dbContext.AnimalRequests.FindAsync(indCounter, _requestContext.FpsYear);
            if (entity == null)
                return false;

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var logEntry = CreateAnimalRequestLogEntry(entity, "D");

                    _dbContext.AnimalRequests.Remove(entity);
                    _dbContext.AnimalRequestLogs.Add(logEntry);
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

        // TRANSFORMENGINE: GetAnimalCostByAnimalTypeAsync — new method required for ASU View;
        // filters AnimalCostView rows by animalType rather than jobCode, mirroring GetAnimalCostAsync.
        // Added here (Phase 4 DataAccess) to keep the build green after the Phase 2 interface update.
        public async Task<PagedData<AnimalCostView>> GetAnimalCostByAnimalTypeAsync(PaginationParameters<string> query, string animalType)
        {
            var queryAnimalCost = BuildAnimalCostByAnimalTypeQuery(animalType);

            queryAnimalCost = ApplyAnimalCostFilter(queryAnimalCost, query.Filter);

            queryAnimalCost = (IQueryable<AnimalCostView>)ApplySorting(queryAnimalCost, query.SortBy, query.Descending);

            var result = await queryAnimalCost.ToListAsync();

            var animalCostViews = result.Select(e =>
            {
                e.AnimalCost = (decimal)e.NumberOfDays * (decimal)e.NumberOfAnimals * (e.DailyRate ?? 0m);
                return e;
            }).ToList();

            return base.ApplyPaging(animalCostViews, query.Page, query.PageSize);
        }

        // TRANSFORMENGINE: BuildAnimalCostByAnimalTypeQuery — mirrors BuildAnimalCostQuery but
        // discriminates on animalReq.AnimalType instead of animalReq.JobCode; used by ASU View.
        private IQueryable<AnimalCostView> BuildAnimalCostByAnimalTypeQuery(string animalType)
        {
            return from animalReq in _dbContext.AnimalRequestViews
                   join animal in _dbContext.Animals on animalReq.AnimalType equals animal.AnimalType
                   join project in _dbContext.ProjectViews on
                          new { animalReq.JobCode, animalReq.UserId } equals new { JobCode = project.ParentProject, project.UserId }
                   let dailyRate = (project.IsDefraProject == -1 ? animal.DefraDailyRate : animal.DailyRate)
                   where animalReq.AnimalType == animalType
                       && animalReq.UserEmail != null
                       && animalReq.UserEmail.ToLower() == _requestContext.UserEmailId
                   select new AnimalCostView
                   {
                       IndCounter = animalReq.IndCounter,
                       Programme = project.Program,
                       AnimalType = animalReq.AnimalType,
                       JobCode = animalReq.JobCode,
                       NumberOfDays = animalReq.NumberOfDays,
                       NumberOfAnimals = animalReq.NumberOfAnimals,
                       DailyRate = dailyRate,
                       TotalDays = animalReq.NumberOfAnimals * animalReq.NumberOfDays
                   };
        }

        private AnimalRequestLog CreateAnimalRequestLogEntry(AnimalRequest animalReq, string insertDelete)
        {
            return new AnimalRequestLog
            {
                JobCode = animalReq.JobCode,
                AnimalType = animalReq.AnimalType,
                NumberOfDays = animalReq.NumberOfDays,
                NumberOfAnimals = animalReq.NumberOfAnimals,
                DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UserId = _requestContext.UserEmailId,
                InsertDelete = insertDelete,
                FpsYear = _requestContext.FpsYear
            };
        }

        private IQueryable<AnimalCostView> BuildAnimalCostQuery(string jobCode)
        {
            return from animalReq in _dbContext.AnimalRequestViews
                   join animal in _dbContext.Animals on animalReq.AnimalType equals animal.AnimalType
                   join project in _dbContext.ProjectViews on
                          new { animalReq.JobCode, animalReq.UserId } equals new { JobCode = project.ParentProject, project.UserId }
                   let dailyRate = (project.IsDefraProject == -1 ? animal.DefraDailyRate : animal.DailyRate)
                   where animalReq.JobCode == jobCode
                       && animalReq.UserEmail != null
                       && animalReq.UserEmail.ToLower() == _requestContext.UserEmailId
                   select new AnimalCostView
                   {
                       IndCounter = animalReq.IndCounter,
                       Programme = project.Program,
                       AnimalType = animalReq.AnimalType,
                       JobCode = animalReq.JobCode,
                       NumberOfDays = animalReq.NumberOfDays,
                       NumberOfAnimals = animalReq.NumberOfAnimals,
                       DailyRate = dailyRate,
                       TotalDays = animalReq.NumberOfAnimals * animalReq.NumberOfDays
                   };
        }

        private static IQueryable<AnimalCostView> ApplyAnimalCostFilter(IQueryable<AnimalCostView> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("AnimalType", out var animalType) && animalType != null)
                query = query.Where(x => EF.Functions.ILike(x.AnimalType!, $"%{animalType}%"));

            if (dict.TryGetValue("NumberOfDays", out var numberOfDays) && numberOfDays != null)
                query = query.Where(x => EF.Functions.ILike(x.NumberOfDays.ToString(), $"%{numberOfDays}%"));

            if (dict.TryGetValue("NumberOfAnimals", out var numberOfAnimals) && numberOfAnimals != null)
                query = query.Where(x => EF.Functions.ILike(x.NumberOfAnimals.ToString(), $"%{numberOfAnimals}%"));

            return query;
        }

        private static IQueryable ApplySorting(IQueryable<AnimalCostView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query;
            }

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<AnimalCostView> query, string property, bool descending)
        {
            return property switch
            {
                "animaltype" => ApplyOrder(query, i => i.AnimalType, descending),
                "animalcost" => ApplyOrder(query, i => i.AnimalCost, descending),
                "dailyrate" => ApplyOrder(query, i => i.DailyRate, descending),
                "numberofdays" => ApplyOrder(query, i => i.NumberOfDays, descending),
                "numberofanimals" => ApplyOrder(query, i => i.NumberOfAnimals, descending),
                _ => query
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<AnimalCostView> query, Expression<Func<AnimalCostView, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private static IQueryable<Animal> ApplyAnimalMasterSorting(IQueryable<Animal> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query;
            }

            return ApplyAnimalSortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable<Animal> ApplyAnimalSortingByProperty(IQueryable<Animal> query, string property, bool descending)
        {
            return property switch
            {
                "animaltype" => ApplyAnimalMasterOrder(query, i => i.AnimalType, descending),
                "species" => ApplyAnimalMasterOrder(query, i => i.Species, descending),
                "dailyrate" => ApplyAnimalMasterOrder(query, i => i.DailyRate, descending),
                "securitylevel" => ApplyAnimalMasterOrder(query, i => i.SecurityLevel, descending),
                "defradailyrate" => ApplyAnimalMasterOrder(query, i => i.DefraDailyRate, descending),
                "planbyweek" => ApplyAnimalMasterOrder(query, i => i.PlanByWeek, descending),
                _ => query
            };
        }

        private static IQueryable<Animal> ApplyAnimalMasterOrder<T>(IQueryable<Animal> query, Expression<Func<Animal, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private static IQueryable<Animal> ApplyAnimalFilter(IQueryable<Animal> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("AnimalType", out var animalType) && animalType != null)
                query = query.Where(a => EF.Functions.ILike(a.AnimalType!, $"%{animalType}%"));

            if (dict.TryGetValue("Species", out var species) && species != null)
                query = query.Where(a => EF.Functions.ILike(a.Species!, $"%{species}%"));

            if (dict.TryGetValue("SecurityLevel", out var securityLevel) && securityLevel != null)
                query = query.Where(a => EF.Functions.ILike(a.SecurityLevel!, $"%{securityLevel}%"));

            return query;
        }
    }
}