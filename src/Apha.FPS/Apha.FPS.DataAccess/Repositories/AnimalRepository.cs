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

        public async Task<PagedData<AnimalSnapshotView>> GetAnimalSnapshotAsync(PaginationParameters<string> query)
        {
            var snapshotQuery = BuildAnimalSnapshotQuery();

            snapshotQuery = ApplyAnimalSnapshotFilter(snapshotQuery, query.Filter);

            snapshotQuery = (IQueryable<AnimalSnapshotView>)ApplyAnimalSnapshotSorting(snapshotQuery, query.SortBy, query.Descending);

            return base.ApplyPaging(snapshotQuery, query.Page, query.PageSize);
        }

        public async Task<AnimalCostView?> GetAnimalCostViewByIdAsync(int indCounter, string jobCode)
        {
            var record = await BuildAnimalCostQuery(jobCode)
                .Where(e => e.IndCounter == indCounter)
                .FirstOrDefaultAsync();

            if (record == null) return null;
            record.AnimalCost = Math.Round((decimal)record.NumberOfDays * (decimal)record.NumberOfAnimals * (record.DailyRate ?? 0m), 4);
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

        private IQueryable<AnimalSnapshotView> BuildAnimalSnapshotQuery()
        {
            return from prg in _dbContext.Programs
                   join prj in _dbContext.ProjectViews on prg.ProgramNo equals prj.Program
                   join ar in _dbContext.AnimalRequests on prj.ParentProject equals ar.JobCode
                   join an in _dbContext.Animals on ar.AnimalType equals an.AnimalType
                   where EF.Functions.ILike(prj.UserEmail!, _requestContext.UserEmailId)
                   select new AnimalSnapshotView
                   {
                       Directorate = prg.Directorate,
                       Program = prj.Program,
                       Contract = prj.Contract,
                       Project = prj.ParentProject,
                       ProjectStatus = prj.ProjectStatus,
                       Species = an.Species,
                       SecurityLevel = an.SecurityLevel,
                       AnimalType = an.AnimalType,
                       DailyRate = an.DailyRate,
                       JobCode = ar.JobCode,
                       NumberOfDays = ar.NumberOfDays,
                       NumberOfAnimals = ar.NumberOfAnimals,
                       Cost = 0//(decimal)((an.DailyRate ?? 0m) * (decimal)ar.NumberOfAnimals * (decimal)ar.NumberOfDays) 
                   };
        }

        private static IQueryable<AnimalSnapshotView> ApplyAnimalSnapshotFilter(IQueryable<AnimalSnapshotView> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("Directorate", out var directorate) && directorate != null)
                query = query.Where(x => EF.Functions.ILike(x.Directorate!, $"%{directorate}%"));

            if (dict.TryGetValue("Program", out var program) && program != null)
                query = query.Where(x => EF.Functions.ILike(x.Program!, $"%{program}%"));

            if (dict.TryGetValue("Contract", out var contract) && contract != null)
                query = query.Where(x => EF.Functions.ILike(x.Contract!, $"%{contract}%"));

            if (dict.TryGetValue("Project", out var project) && project != null)
                query = query.Where(x => EF.Functions.ILike(x.Project!, $"%{project}%"));

            if (dict.TryGetValue("ProjectStatus", out var projectStatus) && projectStatus != null)
                query = query.Where(x => EF.Functions.ILike(x.ProjectStatus!, $"%{projectStatus}%"));

            if (dict.TryGetValue("Species", out var species) && species != null)
                query = query.Where(x => EF.Functions.ILike(x.Species!, $"%{species}%"));

            if (dict.TryGetValue("SecurityLevel", out var securityLevel) && securityLevel != null)
                query = query.Where(x => EF.Functions.ILike(x.SecurityLevel!, $"%{securityLevel}%"));

            if (dict.TryGetValue("AnimalType", out var animalType) && animalType != null)
                query = query.Where(x => EF.Functions.ILike(x.AnimalType!, $"%{animalType}%"));

            if (dict.TryGetValue("JobCode", out var jobCode) && jobCode != null)
                query = query.Where(x => EF.Functions.ILike(x.JobCode!, $"%{jobCode}%"));

            if (dict.TryGetValue("Cost", out var cost) && cost != null)
                query = query.Where(x => EF.Functions.ILike((x.Cost ?? 0m).ToString(), $"%{cost}%"));

            return query;
        }

        private static IQueryable ApplyAnimalSnapshotSorting(IQueryable<AnimalSnapshotView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query;
            }

            return ApplyAnimalSnapshotSortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplyAnimalSnapshotSortingByProperty(IQueryable<AnimalSnapshotView> query, string property, bool descending)
        {
            return property switch
            {
                "directorate" => ApplyAnimalSnapshotOrder(query, i => i.Directorate, descending),
                "program" => ApplyAnimalSnapshotOrder(query, i => i.Program, descending),
                "contract" => ApplyAnimalSnapshotOrder(query, i => i.Contract, descending),
                "project" => ApplyAnimalSnapshotOrder(query, i => i.Project, descending),
                "projectstatus" => ApplyAnimalSnapshotOrder(query, i => i.ProjectStatus, descending),
                "species" => ApplyAnimalSnapshotOrder(query, i => i.Species, descending),
                "securitylevel" => ApplyAnimalSnapshotOrder(query, i => i.SecurityLevel, descending),
                "animaltype" => ApplyAnimalSnapshotOrder(query, i => i.AnimalType, descending),
                "dailyrate" => ApplyAnimalSnapshotOrder(query, i => i.DailyRate, descending),
                "jobcode" => ApplyAnimalSnapshotOrder(query, i => i.JobCode, descending),
                "numberofdays" => ApplyAnimalSnapshotOrder(query, i => i.NumberOfDays, descending),
                "numberofanimals" => ApplyAnimalSnapshotOrder(query, i => i.NumberOfAnimals, descending),
                "cost" => ApplyAnimalSnapshotOrder(query, i => i.Cost, descending),
                _ => query
            };
        }

        private static IQueryable ApplyAnimalSnapshotOrder<T>(IQueryable<AnimalSnapshotView> query, Expression<Func<AnimalSnapshotView, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
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

            if (dict.TryGetValue("JobCode", out var jobCode) && jobCode != null)
                query = query.Where(x => EF.Functions.ILike(x.JobCode!, $"%{jobCode}%"));

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

        /// <summary>
        /// Global total animal cost across all animal requests for the current FPS year.
        /// Converted from the MS Access Form_Activate query used when SellingPC = "ASU":
        /// SELECT Sum(tblAnimalReq.NumberOfDays * tblAnimalReq.NumberOfAnimals * tblAnimals.DailyRate)
        /// FROM tblAnimals INNER JOIN tblAnimalReq ON tblAnimals.AnimalType = tblAnimalReq.AnimalType
        ///
        /// Two-step pattern: NumberOfDays and NumberOfAnimals are double precision; DailyRate is money
        /// (decimal). Mixed-type arithmetic is performed in LINQ-to-Objects after materialisation to
        /// avoid a double x decimal compile error inside the IQueryable projection.
        /// </summary>
        public async Task<decimal> GetGlobalAnimalCostAsync()
        {
            // Step 1 — IQueryable: fetch raw typed columns, no arithmetic
            var raw = await (from req in _dbContext.AnimalRequests
                             join animal in _dbContext.Animals
                                 on req.AnimalType equals animal.AnimalType
                             select new
                             {
                                 req.NumberOfDays,
                                 req.NumberOfAnimals,
                                 animal.DailyRate
                             })
                .AsNoTracking()
                .ToListAsync();

            // Step 2 — LINQ-to-Objects: safe mixed-type arithmetic after materialisation
            return raw.Sum(x => (decimal)x.NumberOfDays * (decimal)x.NumberOfAnimals * (x.DailyRate ?? 0m));
        }

        // Animal Costs ASU View (AnimalCosts — frmAnimalCosts)

        /// <summary>
        /// Converted from qryJobAnimalCost (RecordSource of fsubAnimalCosts).
        /// Returns a paged list of all animal cost records for the current FPS year,
        /// optionally filtered by animal type. No user-email guard — this is the ASU admin view.
        ///
        /// Two-step pattern: NumberOfDays and NumberOfAnimals are double precision (C# double);
        /// DailyRate/DefraDailyRate are money (C# decimal). Mixed-type arithmetic is performed
        /// in LINQ-to-Objects after materialisation to avoid a double × decimal compile error
        /// inside the IQueryable projection.
        /// </summary>
        public async Task<PagedData<AnimalCostView>> GetAnimalCostByAnimalTypeAsync(
            PaginationParameters<string> query, string animalType)
        {
            // Step 1 — IQueryable: apply provider-side filter, then materialise
            var animalCostQuery = BuildProgrammeAnimalCostQuery(animalType);

            animalCostQuery = ApplyAnimalCostFilter(animalCostQuery, query.Filter);

            var raw = await animalCostQuery.ToListAsync();

            // Step 2 — LINQ-to-Objects: compute AnimalCost (double × decimal) before sorting
            foreach (var e in raw)
            {
                e.AnimalCost = (decimal)e.NumberOfDays * (decimal)e.NumberOfAnimals * (e.DailyRate ?? 0m);
            }

            // Step 3 — sort in memory so AnimalCost ordering reflects the computed value
            var result = ApplyAnimalCostInMemorySorting(raw, query.SortBy, query.Descending);

            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        private static List<AnimalCostView> ApplyAnimalCostInMemorySorting(
            List<AnimalCostView> source, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return source;

            Func<AnimalCostView, object?> keySelector = sortBy.ToLower() switch
            {
                "jobcode" => i => i.JobCode,
                "animalcost" => i => i.AnimalCost,
                "dailyrate" => i => i.DailyRate,
                "totaldays" => i => i.TotalDays,
                "numberofanimals" => i => i.NumberOfAnimals,
                _ => null!
            };

            if (keySelector == null)
                return source;

            return descending
                ? source.OrderByDescending(keySelector).ToList()
                : source.OrderBy(keySelector).ToList();
        }


        private IQueryable<AnimalCostView> BuildProgrammeAnimalCostQuery(string animalType)
        {
            return from project in _dbContext.Projects
                   join animalReq in _dbContext.AnimalRequests
                       on project.ParentProject equals animalReq.JobCode
                   join animal in _dbContext.Animals
                       on animalReq.AnimalType equals animal.AnimalType
                   where animalReq.AnimalType.ToLower().Trim().Equals(animalType.ToLower().Trim())
                   let dailyRate = project.IsDefraProject == 0 ? animal.DailyRate : animal.DefraDailyRate
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
    }
}