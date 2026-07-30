using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;


namespace Apha.PIMS.DataAccess.Repository
{
    public class MilestoneRepository : BaseRepository, IMilestoneRepository
    {
        private readonly PimsDbContext _dbContext;
        private const string ExistingMilestoneNote = "This Project Milestone already exists";

        public MilestoneRepository(PimsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<PagedData<Milestone>> GetAllMilestonesAsync(PaginationParameters<string> parameters, string project)
        {
            IQueryable<Milestone> query =
                from milestone in _dbContext.Milestones.AsNoTracking()
                join milestoneType in _dbContext.MilestoneTypes.AsNoTracking()
                    on milestone.IdType equals milestoneType.IdType.ToString() into milestoneTypeGroup
                from milestoneType in milestoneTypeGroup.DefaultIfEmpty()
                where milestone.Project == project
                select new Milestone
                {
                    Project = milestone.Project,
                    Number = milestone.Number,
                    Description = milestone.Description,
                    DateDue = milestone.DateDue,
                    DateCompleted = milestone.DateCompleted,
                    DateFormReceived = milestone.DateFormReceived,
                    UnderSdReview = milestone.UnderSdReview,
                    OnTarget = milestone.OnTarget,
                    ProjectLeaderComment = milestone.ProjectLeaderComment,
                    CapsComment = milestone.CapsComment,
                    IdType = milestoneType != null ? milestoneType.Type : milestone.IdType
                };

            query = ApplyFilter(query, parameters.Filter);
            query = ApplySorting(query, parameters.SortBy, parameters.Descending);

            return await ApplyPaging(query, parameters.Page, parameters.PageSize);
        }               

        public async Task<Milestone?> GetMilestoneAsync(string project, string number)
            => await _dbContext.Milestones
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Project == project && m.Number == number);

        public async Task<string> GetProgramByProjectAsync(string project)
        {
            string? program = await _dbContext.ProjectRadTrackData
                .AsNoTracking()
                .Where(g => g.Parentproject == project)
                .Join(_dbContext.ProjectLatestDetails,
                      g => g.Parentproject,
                      v => v.ParentProject,
                      (g, v) => v.Program)
                .FirstOrDefaultAsync();

            return program ?? string.Empty;
        }

        public async Task<Milestone> AddMilestoneAsync(Milestone entity, string? changedBy)
        {
            _dbContext.Milestones.Add(entity);
            await _dbContext.SaveChangesAsync();

            try
            {
                _dbContext.LogMilestones.Add(BuildLogEntry(entity, 'I', changedBy));
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Log entry creation failure should not affect milestone addition
            }

            return entity;
        }

        public async Task<Milestone> UpdateMilestoneAsync(Milestone entity, string? changedBy)
        {
            _dbContext.Milestones.Update(entity);
            await _dbContext.SaveChangesAsync();

            try
            {
                _dbContext.LogMilestones.Add(BuildLogEntry(entity, 'U', changedBy));
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Log entry creation failure should not affect milestone update
            }

            return entity;
        }
        public async Task<bool> DeleteMilestoneAsync(string project, string number)
        {
            int rows = await _dbContext.Milestones
                .Where(m => m.Project == project && m.Number == number)
                .ExecuteDeleteAsync();
            return rows > 0;
        }
        public async Task<List<MilestoneType>> GetMilestoneTypesAsync(string? milestoneDeliverable = null)
        {
            IQueryable<MilestoneType> query = _dbContext.MilestoneTypes.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(milestoneDeliverable))
            {
                char filter = milestoneDeliverable[0];
                query = query.Where(t => t.MilestoneDeliverable == filter);
            }
            return await query.OrderBy(t => t.Type).ToListAsync();
        }

        public async Task<PagedData<MilestoneFormDates>> GetAllMilestoneFormDatesAsync(PaginationParameters<string> parameters, string parentProject)
        {
            IQueryable<MilestoneFormDates> query = _dbContext.MilestoneFormDates
                .AsNoTracking()
                .Where(f => f.ParentProject == parentProject)
                .OrderByDescending(f => f.Year);

            return await ApplyPaging(query, parameters.Page, parameters.PageSize);
        }

        public async Task<MilestoneFormDates?> GetMilestoneFormDatesAsync(short year, string parentProject)
            => await _dbContext.MilestoneFormDates
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Year == year && f.ParentProject == parentProject);

        public async Task<MilestoneFormDates> AddMilestoneFormDatesAsync(MilestoneFormDates entity)
        {
            _dbContext.MilestoneFormDates.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<MilestoneFormDates> UpdateMilestoneFormDatesAsync(MilestoneFormDates entity)
        {
            _dbContext.MilestoneFormDates.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteMilestoneFormDatesAsync(short year, string parentProject)
        {
            int rows = await _dbContext.MilestoneFormDates
                .Where(f => f.Year == year && f.ParentProject == parentProject)
                .ExecuteDeleteAsync();
            return rows > 0;
        }

        private static IQueryable<Milestone> ApplyFilter(IQueryable<Milestone> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter) || filter == "{}")
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("Number", out var number) && number != null)
            {
                string val = number.ToString()!;
                query = query.Where(x => EF.Functions.ILike(x.Number, $"%{val}%"));
            }

            return query;
        }

        private static IQueryable<Milestone> ApplySorting(IQueryable<Milestone> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy) || string.Equals(sortBy, "number", StringComparison.OrdinalIgnoreCase))
                    return ApplyOrder(query, m => m.Number, descending);

            return query.OrderBy(m => m.Number);
        }

        private static IQueryable<Milestone> ApplyOrder<T>(
            IQueryable<Milestone> query,
            Expression<Func<Milestone, T>> keySelector,
            bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        private static IQueryable<StagingMilestone> ApplyStagingSorting(IQueryable<StagingMilestone> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy) || string.Equals(sortBy, "number", StringComparison.OrdinalIgnoreCase))
                return ApplyStagingOrder(query, m => m.Number, descending);

            return query.OrderBy(m => m.Number);
        }
        private static IQueryable<StagingMilestone> ApplyStagingOrder<T>(
            IQueryable<StagingMilestone> query,
            Expression<Func<StagingMilestone, T>> keySelector,
            bool descending)
            => descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        public async Task<bool> UpdateFormRequiredAsync(string parentproject, bool formRequired)
        {
            int rows = await _dbContext.ProjectRadTrackData
                .Where(p => p.Parentproject == parentproject)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Formrequired, formRequired));
            return rows > 0;
        }

        public async Task<PagedData<LogMilestone>> GetLogMilestonesAsync(PaginationParameters<string> parameters,string? project,string? numberPart1,string? numberPart2)
        {
            string numberPattern;
            if (string.IsNullOrWhiteSpace(numberPart1) && string.IsNullOrWhiteSpace(numberPart2))
            {
                numberPattern = string.Empty;
            }
            else
            {
                string left  = string.IsNullOrWhiteSpace(numberPart1) ? "%" : numberPart1;
                string right = string.IsNullOrWhiteSpace(numberPart2) ? "%" : numberPart2;
                numberPattern = $"{left}/{right}";
            }

            IQueryable<LogMilestone> logQuery = _dbContext.LogMilestones.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(project))
                logQuery = logQuery.Where(l => l.Project == project);

            if (!string.IsNullOrWhiteSpace(numberPattern))
                logQuery = logQuery.Where(l => EF.Functions.Like(l.Number!, numberPattern));

            IQueryable<LogMilestone> query =
                from l in logQuery
                join pm in _dbContext.ProjectManagers.AsNoTracking()
                    on l.ChangedBy equals pm.Mnumber into pmGroup
                from pm in pmGroup.DefaultIfEmpty()
                orderby l.DateChanged descending
                select new LogMilestone
                {
                    Id                   = l.Id,
                    Project              = l.Project,
                    Number               = l.Number,
                    Description          = l.Description,
                    DateDue              = l.DateDue,
                    DateCompleted        = l.DateCompleted,
                    DateFormReceived     = l.DateFormReceived,
                    UnderSdReview        = l.UnderSdReview,
                    OnTarget             = l.OnTarget,
                    ProjectLeaderComment = l.ProjectLeaderComment,
                    CapsComment          = l.CapsComment,
                    IdType               = l.IdType,
                    DateChanged          = l.DateChanged,
                    ChangedBy            = pm != null
                                               ? pm.Projectmanager
                                               : l.ChangedBy != null ? "(" + l.ChangedBy + ")" : null,
                    UpdateType           = l.UpdateType
                };

            return await ApplyPaging(query, parameters.Page, parameters.PageSize);
        }

        private static LogMilestone BuildLogEntry(Milestone m, char updateType, string? changedBy)
            => new()
            {
                Project              = m.Project,
                Number               = m.Number,
                Description          = m.Description,
                DateDue              = DateTime.SpecifyKind(m.DateDue, DateTimeKind.Unspecified),
                DateCompleted        = m.DateCompleted.HasValue
                                           ? DateTime.SpecifyKind(m.DateCompleted.Value, DateTimeKind.Unspecified)
                                           : null,
                DateFormReceived     = m.DateFormReceived.HasValue
                                           ? DateTime.SpecifyKind(m.DateFormReceived.Value, DateTimeKind.Unspecified)
                                           : null,
                UnderSdReview        = m.UnderSdReview,
                OnTarget             = m.OnTarget,
                ProjectLeaderComment = m.ProjectLeaderComment,
                CapsComment          = m.CapsComment,
                IdType               = string.IsNullOrEmpty(m.IdType) ? null : m.IdType[0],
                DateChanged          = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                ChangedBy            = changedBy,
                UpdateType           = updateType
            };

        // ── Staging / Import ─────────────────────────────────────────────────
        public async Task<PagedData<StagingMilestone>> GetAllStagingRowsAsync(PaginationParameters<string> parameters, string? createdBy = null)
        {

            IQueryable<StagingMilestone> query = _dbContext.StagingMilestones
               .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(createdBy))
                query = query.Where(s => s.CreatedBy == createdBy);

            query = ApplyStagingFilter(query, parameters.Filter);
            query = ApplyStagingSorting(query, parameters.SortBy, parameters.Descending);


            return await ApplyPaging(query, parameters.Page, parameters.PageSize);


        }

        private static IQueryable<StagingMilestone> ApplyStagingFilter(IQueryable<StagingMilestone> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter) || filter == "{}")
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("Number", out var number) && number != null)
            {
                string val = number.ToString()!;
                query = query.Where(x => EF.Functions.ILike(x.Number!, $"%{val}%"));
            }

            return query;
        }

        public async Task<List<StagingMilestone>> GetStagingRowsAsync(int id)
        {
            return await _dbContext.StagingMilestones
                .AsNoTracking()
                .Where(s => s.Id == id)
                .ToListAsync();
        }

        public async Task<StagingMilestone> AddStagingRowAsync(StagingMilestone entity, string? createdBy = null)
        {
            if (!string.IsNullOrWhiteSpace(createdBy))
                entity.CreatedBy = createdBy;

            _dbContext.StagingMilestones.Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<StagingMilestone> UpdateStagingRowAsync(StagingMilestone entity, string? createdBy = null)
        {
            string? existingCreatedBy = await _dbContext.StagingMilestones
                .AsNoTracking()
                .Where(s => s.Id == entity.Id)
                .Select(s => s.CreatedBy)
                .FirstOrDefaultAsync();

            entity.CreatedBy = !string.IsNullOrWhiteSpace(createdBy)
                ? createdBy
                : existingCreatedBy;

            _dbContext.StagingMilestones.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteStagingRowAsync(int id, string? createdBy = null)
        {
            IQueryable<StagingMilestone> query = _dbContext.StagingMilestones
                .Where(s => s.Id == id);

            if (!string.IsNullOrWhiteSpace(createdBy))
                query = query.Where(s => s.CreatedBy == createdBy);

            int rows = await query.ExecuteDeleteAsync();
            return rows > 0;
        }

        public async Task<int> ClearStagingAsync(string project, string? createdBy = null)
        {
            IQueryable<StagingMilestone> query = _dbContext.StagingMilestones;
              

            if (!string.IsNullOrWhiteSpace(createdBy))
                query = query.Where(s => s.CreatedBy == createdBy);

            return await query.ExecuteDeleteAsync();
        }

        public async Task ValidateStagingAsync(string project, string? typeId, bool isDeliverableMode, string? createdBy = null)
        {
            IQueryable<StagingMilestone> query = _dbContext.StagingMilestones;
               

            if (!string.IsNullOrWhiteSpace(createdBy))
                query = query.Where(s => s.CreatedBy == createdBy);

            List<StagingMilestone> rows = await query.ToListAsync();

            foreach (StagingMilestone row in rows)
            {
                // Skip fully empty rows
                if (string.IsNullOrWhiteSpace(row.Description) &&
                    string.IsNullOrWhiteSpace(row.Number)
                    )
                {
                    _dbContext.StagingMilestones.Remove(row);
                    continue;
                }

                row.TypeId = typeId;
                row.Note = null;

                // Validate date
                if (row.DateDue == default)
                    row.Note = (row.Note ?? string.Empty) + "(*Please check this date*)";

                // Validate number format: must match YY/NN
                if (!string.IsNullOrWhiteSpace(row.Number) &&
                    !System.Text.RegularExpressions.Regex.IsMatch(row.Number.Trim(), @"^\d{2}/\d{2}$"))
                {
                    row.Note = (row.Note ?? string.Empty) + " Please check this number format.";
                }
                else if (!string.IsNullOrWhiteSpace(row.Number))
                {
                    string trimmed = row.Number.Trim();
                    bool exists = await _dbContext.Milestones
                        .AnyAsync(m => m.Project == project && m.Number == trimmed);
                    if (exists)
                        row.Note = (row.Note ?? string.Empty) + $" {ExistingMilestoneNote}.";
                    else
                        row.Number = trimmed;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> ImportStagingAsync(string project, string? changedBy, string? createdBy = null)
        {
            IQueryable<StagingMilestone> validQuery = _dbContext.StagingMilestones
                .AsNoTracking()
                .Where(s => s.Note == null && !string.IsNullOrWhiteSpace(s.Number));

            if (!string.IsNullOrWhiteSpace(createdBy))
                validQuery = validQuery.Where(s => s.CreatedBy == createdBy);

            List<StagingMilestone> validRows = await validQuery.ToListAsync();

            if (validRows.Count == 0)
                return 0;

            HashSet<string> existingNumbers = await _dbContext.Milestones
                .AsNoTracking()
                .Where(m => m.Project == project)
                .Select(m => m.Number)
                .ToHashSetAsync();

            List<StagingMilestone> rowsToInsert = validRows
                .Where(s => !existingNumbers.Contains(s.Number!))
                .ToList();

            List<Milestone> newMilestones = rowsToInsert
                .Select(s => new Milestone
                {
                    Project = project,
                    Number = s.Number!,
                    Description = s.Description,
                    DateDue = DateTime.SpecifyKind(s.DateDue, DateTimeKind.Unspecified),
                    IdType = s.TypeId
                })
                .ToList();

            if (newMilestones.Count == 0)
                return 0;

            await _dbContext.Milestones.AddRangeAsync(newMilestones);
            await _dbContext.SaveChangesAsync();

            foreach (Milestone m in newMilestones)
            {
                try
                {
                    _dbContext.LogMilestones.Add(BuildLogEntry(m, 'I', changedBy));
                }
                catch (Exception)
                {
                    // Log entry creation failure should not affect import
                }
            }

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Log save failure must not affect the import operation
            }

            List<int> insertedStagingIds = rowsToInsert.Select(r => r.Id).ToList();
            if (insertedStagingIds.Count > 0)
            {
                await _dbContext.StagingMilestones
                    .Where(s => insertedStagingIds.Contains(s.Id))
                    .ExecuteDeleteAsync();
            }

            return newMilestones.Count;
        }

        public async Task<int> ImportWithOverwriteAsync(string project, string? changedBy, string? createdBy = null)
        {
            IQueryable<StagingMilestone> updateQuery = _dbContext.StagingMilestones;

            if (!string.IsNullOrWhiteSpace(createdBy))
                updateQuery = updateQuery.Where(s => s.CreatedBy == createdBy);
            else
                updateQuery = updateQuery.Where(_ => false);

            await updateQuery.ExecuteUpdateAsync(s => s.SetProperty(x => x.Project, project));

            IQueryable<StagingMilestone> stagingRowsQuery = _dbContext.StagingMilestones
                .AsNoTracking()
                .Where(s => s.Project == project
                            && (s.Note == null || EF.Functions.Like(s.Note, $"%{ExistingMilestoneNote}%"))
                            && _dbContext.Milestones.Any(m => m.Project == project && m.Number == s.Number));

            if (!string.IsNullOrWhiteSpace(createdBy))
                stagingRowsQuery = stagingRowsQuery.Where(s => s.CreatedBy == createdBy);

            List<StagingMilestone> stagingRows = await stagingRowsQuery.ToListAsync();

            int updated = 0;
            foreach (StagingMilestone stagingRow in stagingRows)
            {
                Milestone? existing = await _dbContext.Milestones
                    .FirstOrDefaultAsync(m => m.Project == project && m.Number == stagingRow.Number);

                if (existing is null)
                    continue;

                existing.DateDue = DateTime.SpecifyKind(stagingRow.DateDue, DateTimeKind.Unspecified);
                existing.Description = stagingRow.Description;
                _dbContext.Milestones.Update(existing);
                await _dbContext.SaveChangesAsync();

                try
                {
                    _dbContext.LogMilestones.Add(BuildLogEntry(existing, 'U', changedBy));
                    await _dbContext.SaveChangesAsync();
                }
                catch (Exception)
                {
                    // Log write failure must not affect the overwrite operation
                }

                await _dbContext.StagingMilestones
                    .Where(s => s.Id == stagingRow.Id)
                    .ExecuteDeleteAsync();

                updated++;
            }

            return updated;
        }

        public async Task<string> GetNextMilestoneNumberAsync(string project, int year)
        {
            string yr2d = year.ToString()[^2..];

            string? latestInMilestone = await _dbContext.Milestones
                .AsNoTracking()
                .Where(m => m.Project == project && m.Number != null && m.Number.StartsWith(yr2d))
                .MaxAsync(m => m.Number);

            string? latestInStaging = await _dbContext.StagingMilestones
                .AsNoTracking()
                .Where(s => s.Number != null && s.Number.StartsWith(yr2d))
                .MaxAsync(s => s.Number);

            if (string.IsNullOrEmpty(latestInMilestone) && string.IsNullOrEmpty(latestInStaging))
                return $"{yr2d}/01";

            int milestoneSeq = ParseSeq(latestInMilestone);
            int stagingSeq = ParseSeq(latestInStaging);
            int next = Math.Max(milestoneSeq, stagingSeq) + 1;
            return $"{yr2d}/{next:D2}";
        }

        private static int ParseSeq(string? number)
        {
            if (string.IsNullOrWhiteSpace(number)) return 0;
            int slash = number.IndexOf('/');
            if (slash < 0 || slash >= number.Length - 1) return 0;
            return int.TryParse(number[(slash + 1)..], out int seq) ? seq : 0;
        }

        // ── Project Year Manager ─────────────────────────────────────────────────

        /// <summary>
        /// Gets project year manager details by filtering projects by year and joining with manager information
        /// </summary>
        /// <param name="year">The year to filter projects</param>
        /// <returns>List of project year manager details</returns>
        public async Task<List<ProjectYearManager>> GetProjectYearManagersAsync(int year)
        {
            var query = from project in _dbContext.MyTlkpProjects.AsNoTracking()
                        join manager in _dbContext.ProjectManagers.AsNoTracking()
                            on project.Manager equals manager.Projectmanager into managerGroup
                        from manager in managerGroup.DefaultIfEmpty()
                        where project.Year == year
                        select new ProjectYearManager
                        {
                            ProjectYear = project.Year,
                            ParentProject = project.Parentproject,
                            Manager = project.Manager,
                            ManagerNumber = manager != null ? manager.Mnumber : null
                        };

            return await query.ToListAsync();
        }
        public async Task<PagedData<Milestone>> GetPMDMilestonesAsync(PaginationParameters<string> parameters, string project)
        {
            DateTime fyStart = GetFYStart();
            DateTime fyEnd = fyStart.AddYears(1).AddDays(-1);

            IQueryable<Milestone> query =
                from milestone in _dbContext.Milestones.AsNoTracking()
                join milestoneType in _dbContext.MilestoneTypes.AsNoTracking()
                    on milestone.IdType equals milestoneType.IdType.ToString()
                where milestone.Project == project
                      && milestone.DateDue >= fyStart
                      && milestone.DateDue <= fyEnd
                select new Milestone
                {
                    Project = milestone.Project,
                    Number = milestone.Number,
                    Description = milestone.Description,
                    DateDue = milestone.DateDue,
                    DateCompleted = milestone.DateCompleted,
                    UnderSdReview = milestone.UnderSdReview,
                    OnTarget = milestone.OnTarget,
                    ProjectLeaderComment = milestone.ProjectLeaderComment,
                    IdType = milestoneType.MilestoneDeliverable.HasValue
                        ? milestoneType.MilestoneDeliverable.Value.ToString()
                        : null
                };

            query = ApplyFilter(query, parameters.Filter);
            query = ApplySorting(query, parameters.SortBy, parameters.Descending);

            return await ApplyPaging(query, parameters.Page, parameters.PageSize);
        }

        private static DateTime GetFYStart()
        {
            int currentYear = DateTime.Today.Year;
            int currentMonth = DateTime.Today.Month;

            int fyYear = currentMonth < 6 ? currentYear - 1 : currentYear;
            return new DateTime(fyYear, 4, 1);
        }
    }
}
