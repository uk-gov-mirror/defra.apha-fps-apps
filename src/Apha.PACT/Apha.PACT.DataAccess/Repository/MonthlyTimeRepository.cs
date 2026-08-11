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
    public class MonthlyTimeRepository : BaseRepository, IMonthlyTimeRepository
    {
        private readonly IFpsRequestContext _fpsRequestContext;       

        public MonthlyTimeRepository(FpsDbContext context, IFpsRequestContext fpsRequestContext) : base(context)
        {
            _fpsRequestContext = fpsRequestContext;
        }       

        public async Task<bool> HasMonthlyTimeEntriesAsync(string workGroup, string timeCode, string parentProject)
        {
            return await _context.MonthlyTimes
                .AsNoTracking()
                .AnyAsync(m => m.WorkGroup == workGroup && m.TimeCode == timeCode && m.ParentProject == parentProject);
        }

        public async Task<PagedData<MonthlyTimeStaff>> SearchLiveAsync(
            PaginationParameters<string> query,
            string? workGroup,
            string? timeCode,
            string? pactStaffId,
            string? parentProject,              
            double? month)
        {
            var monthlyTimes = from mt in _context.MonthlyTimes.AsNoTracking()
                               join wgs in _context.WorkGroupStaffViews.AsNoTracking()
                               on new { StaffId = (string?)mt.PactStaffId, Year = (int?)mt.FpsYear }
                               equals new { StaffId = wgs.PactId, Year = wgs.FpsYear }
                               where mt.FpsYear == _fpsRequestContext.FpsYear
                               select new MonthlyTimeStaff
                               {
                                   PactStaffId = mt.PactStaffId,
                                   Name = wgs.Name,
                                   TimeCode = mt.TimeCode,
                                   Month = mt.Month,
                                   ParentProject = mt.ParentProject,
                                   WorkGroup = mt.WorkGroup,
                                   Hours = mt.Hours,
                                   FpsYear = mt.FpsYear
                               };

            if (!string.IsNullOrWhiteSpace(workGroup))
                monthlyTimes = monthlyTimes.Where(x => x.WorkGroup == workGroup);

            if (!string.IsNullOrWhiteSpace(timeCode))
                monthlyTimes = monthlyTimes.Where(x => x.TimeCode == timeCode);

            if (!string.IsNullOrWhiteSpace(pactStaffId))
                monthlyTimes = monthlyTimes.Where(x => x.PactStaffId == pactStaffId);

            if (!string.IsNullOrWhiteSpace(parentProject))
                monthlyTimes = monthlyTimes.Where(x => x.ParentProject == parentProject);

            if (month.HasValue)
                monthlyTimes = monthlyTimes.Where(x => (int)x.Month == (int)month.Value);
            
            monthlyTimes = (IQueryable<MonthlyTimeStaff>)ApplyLiveSorting(monthlyTimes, query.SortBy, query.Descending);            

            var pagedLiveData = await ApplyPaging(monthlyTimes, query.Page, query.PageSize);         
            

            pagedLiveData.Total = await monthlyTimes.SumAsync(x => (decimal)(x.Hours ?? 0));   
            return pagedLiveData;
        }

        public async Task<MonthlyTime?> GetLiveByKeyAsync(string pactStaffId, string timeCode, double month, string parentProject)
        {
            return await _context.MonthlyTimes
                .FirstOrDefaultAsync(x => x.PactStaffId == pactStaffId
                    && x.TimeCode == timeCode
                    && x.Month == month
                    && x.ParentProject == parentProject
                    && x.FpsYear == _fpsRequestContext.FpsYear);
        }

        public async Task<MonthlyTime> UpdateLiveAsync(MonthlyTime monthlyTime, string originalPactStaffId)
        {
            var fpsYear = _fpsRequestContext.FpsYear;
            monthlyTime.FpsYear = fpsYear;

            var targetKeyExists = await _context.MonthlyTimes
                .AsNoTracking()
                .AnyAsync(x => x.PactStaffId == monthlyTime.PactStaffId
                    && x.TimeCode == monthlyTime.TimeCode
                    && x.Month == monthlyTime.Month
                    && x.ParentProject == monthlyTime.ParentProject
                    && x.FpsYear == fpsYear
                    && x.PactStaffId != originalPactStaffId);

            if (targetKeyExists)
                throw new InvalidOperationException("A record with the target key already exists in MonthlyTime.");

            // Capture original (DELETED) values for MT_LOG 'UD' entry - MT_LOG Update Trigger
            var original = await _context.MonthlyTimes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PactStaffId == originalPactStaffId
                    && x.TimeCode == monthlyTime.TimeCode
                    && x.Month == monthlyTime.Month
                    && x.ParentProject == monthlyTime.ParentProject
                    && x.FpsYear == fpsYear);

            var updatedCount = await _context.MonthlyTimes
                .Where(x => x.PactStaffId == originalPactStaffId
                    && x.TimeCode == monthlyTime.TimeCode
                    && x.Month == monthlyTime.Month
                    && x.ParentProject == monthlyTime.ParentProject
                    && x.FpsYear == fpsYear)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.PactStaffId, monthlyTime.PactStaffId)
                    .SetProperty(x => x.TimeCode, monthlyTime.TimeCode)
                    .SetProperty(x => x.Month, monthlyTime.Month)
                    .SetProperty(x => x.ParentProject, monthlyTime.ParentProject)
                    .SetProperty(x => x.WorkGroup, monthlyTime.WorkGroup)
                    .SetProperty(x => x.Hours, monthlyTime.Hours));

            if (updatedCount == 0)
                throw new InvalidOperationException("MonthlyTime record not found for update.");

            var updated = await _context.MonthlyTimes
                .FirstOrDefaultAsync(x => x.PactStaffId == monthlyTime.PactStaffId
                    && x.TimeCode == monthlyTime.TimeCode
                    && x.Month == monthlyTime.Month
                    && x.ParentProject == monthlyTime.ParentProject
                    && x.FpsYear == fpsYear);

            if (updated == null)
                throw new InvalidOperationException("MonthlyTime record updated but could not be reloaded.");

            // Log 'UD' (old values) and 'UI' (new values) - MT_LOG Update Trigger
            if (original != null)
                await _context.MonthlyTimeLogs.AddAsync(BuildLogEntry(original, "UD"));
            await _context.MonthlyTimeLogs.AddAsync(BuildLogEntry(updated, "UI"));
            await _context.SaveChangesAsync();

            return updated;
        }

        public async Task<bool> DeleteLiveAsync(string pactStaffId, string timeCode, double month, string parentProject)
        {
            var entity = await GetLiveByKeyAsync(pactStaffId, timeCode, month, parentProject);
            if (entity == null)
                return false;

            // Log 'D' for the deleted row - MT_LOG Delete Trigger
            await _context.MonthlyTimeLogs.AddAsync(BuildLogEntry(entity, "D"));
            _context.MonthlyTimes.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string pactStaffId, string timeCode, double month, string parentProject)
        {
            return await _context.MonthlyTimes
                .AsNoTracking()
                .AnyAsync(x => x.PactStaffId == pactStaffId
                    && x.TimeCode == timeCode
                    && x.Month == month
                    && x.ParentProject == parentProject
                    && x.FpsYear == _fpsRequestContext.FpsYear);
        }

        public async Task<PagedData<StagingMonthlyTime>> SearchStagingAsync(
            PaginationParameters<string> query,
            string importedBy,
            bool? passed)
        {
            var stagingQuery = _context.StagingMonthlyTimes
                .AsNoTracking()
                .Where(x => x.ImportedBy == importedBy);

            if (passed.HasValue)
            {
                if (passed.Value)
                {
                    // If passed is true, get all passed records
                    stagingQuery = stagingQuery.Where(x => x.Passed == true);
                }
                else
                {  
                    stagingQuery = stagingQuery.Where(x => x.Passed == false);
                }
            }

            // Apply filtering
            stagingQuery = ApplyStagingFilter(stagingQuery, query.Filter);

            stagingQuery = (IQueryable<StagingMonthlyTime>)ApplyStagingSorting(stagingQuery, query.SortBy, query.Descending);            

            var pagedStagingData = await ApplyPaging(stagingQuery, query.Page, query.PageSize);
            pagedStagingData.Total = await stagingQuery.SumAsync(x => (decimal)(x.Hours ?? 0));
            return pagedStagingData;
        }

        public async Task<StagingMonthlyTime?> GetStagingByIdAsync(int id, string importedBy)
        {
            return await _context.StagingMonthlyTimes
                .FirstOrDefaultAsync(x => x.Id == id && x.ImportedBy == importedBy);
        }

        public async Task<StagingMonthlyTime> CreateStagingAsync(StagingMonthlyTime stagingMonthlyTime)
        {            
            await _context.StagingMonthlyTimes.AddAsync(stagingMonthlyTime);
            await _context.SaveChangesAsync();
            return stagingMonthlyTime;
        }

        public async Task<StagingMonthlyTime> UpdateStagingAsync(StagingMonthlyTime stagingMonthlyTime, string importedBy)
        {
            var existing = await GetStagingByIdAsync(stagingMonthlyTime.Id, importedBy)
                ?? throw new InvalidOperationException("Staging monthly time record not found.");

            existing.PactStaffId = stagingMonthlyTime.PactStaffId;
            existing.TimeCode = stagingMonthlyTime.TimeCode;
            existing.ParentProject = stagingMonthlyTime.ParentProject;
            existing.Month = stagingMonthlyTime.Month;
            existing.WorkGroup = stagingMonthlyTime.WorkGroup;
            existing.Hours = stagingMonthlyTime.Hours;
            existing.PactId = stagingMonthlyTime.PactId;
            existing.Name = stagingMonthlyTime.Name;
            existing.Passed = false;
            existing.FailureComments = "This record has been edited since being validated. Needs re-validating.";

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<int> BulkUpdateStagingNamesAsync(
            string importedBy,
            string originalWorkGroup,
            string originalPactStaffId,
            string? newName,
            string? newPactStaffId,
            string? newPactId,
            int? excludeId)
        {
            var query = _context.StagingMonthlyTimes
                .Where(x => x.ImportedBy == importedBy
                    && x.WorkGroup == originalWorkGroup
                    && x.PactStaffId == originalPactStaffId);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Name, newName)
                .SetProperty(x => x.PactStaffId, newPactStaffId)
                .SetProperty(x => x.PactId, newPactId)
                .SetProperty(x => x.Passed, false)
                .SetProperty(x => x.FailureComments, "This record has been edited since being validated. Needs re-validating."));
        }

        public async Task<bool> DeleteStagingAsync(int id, string importedBy)
        {
            var existing = await GetStagingByIdAsync(id, importedBy);
            if (existing == null)
                return false;

            _context.StagingMonthlyTimes.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteAllStagingByUserAsync(string importedBy)
        {
            return await _context.StagingMonthlyTimes
                .Where(x => x.ImportedBy == importedBy)
                .ExecuteDeleteAsync();
        }

        public async Task<int> DeleteFailedStagingByUserAsync(string importedBy)
        {
            return await _context.StagingMonthlyTimes
                .Where(x => x.ImportedBy == importedBy && x.Passed == false)
                .ExecuteDeleteAsync();
        }

        public async Task<int> ImportStagingAsync(IEnumerable<StagingMonthlyTime> stagingRows)
        {
            var rows = stagingRows.ToList();

            if (rows.Count == 0)
                return 0;

            await _context.StagingMonthlyTimes.AddRangeAsync(rows);
            await _context.SaveChangesAsync();
            return rows.Count;
        }

        public async Task<int> RemoveZeroAndNullHourRecordsAsync(string importedBy)
        {
            return await _context.StagingMonthlyTimes
                .Where(x => x.ImportedBy == importedBy && (x.Hours == null || x.Hours == 0))
                .ExecuteDeleteAsync();
        }

        public async Task<List<StagingMonthlyTime>> GetStagingRecordsForValidationAsync(string importedBy)
        {
            return await _context.StagingMonthlyTimes
                .Where(x => x.ImportedBy == importedBy && x.Passed == false)
                .OrderBy(x => x.Id)
                .Distinct()
                .ToListAsync();
        }

        public async Task UpdateStagingRecordsAsync(IEnumerable<StagingMonthlyTime> records)
        {
            foreach (var record in records)
            {
                _context.Entry(record).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();
        }

        public async Task<HashSet<string>> GetExistingLiveKeysAsync()
        {
            var keys = await _context.MonthlyTimes
                .AsNoTracking()
                .Select(x => x.PactStaffId + "|" + x.TimeCode + "|" + x.ParentProject + "|" + x.WorkGroup + "|" + x.Month)
                .ToListAsync();
            return new HashSet<string>(keys);
        }

        public async Task<bool> HasFailedStagingAsync(string importedBy)
        {
            return await _context.StagingMonthlyTimes
                .AsNoTracking()
                .AnyAsync(x => x.ImportedBy == importedBy && x.Passed == false);
        }

        public async Task<(int ProcessedCount, int ImportedCount, int FailedCount)> MakeLiveAsync(string importedBy)
        {
            const string noLongerValidMessage = "This record is no longer valid. Needs re-validating";

            var passedRows = await _context.StagingMonthlyTimes
                .Where(x => x.ImportedBy == importedBy && x.Passed == true)
                .OrderBy(x => x.Id)
                .ToListAsync();

            if (passedRows.Count == 0)
                return (0, 0, 0);

            var failedCount = await _context.StagingMonthlyTimes
                .AsNoTracking()
                .CountAsync(x => x.ImportedBy == importedBy && x.Passed == false);

            var importedCount = 0;
            foreach (var row in passedRows)
            {
                if (!IsValidForMakeLive(row))
                {
                    MarkRowAsInvalid(row, noLongerValidMessage);
                    failedCount++;
                    continue;
                }

                var imported = await TryImportRowToLiveAsync(row, noLongerValidMessage);
                if (imported)
                    importedCount++;
                else
                    failedCount++;
            }

            await _context.SaveChangesAsync();

            if (failedCount == 0)
                await DeleteAllStagingByUserAsync(importedBy);

            return (passedRows.Count, importedCount, failedCount);
        }

        private static bool IsValidForMakeLive(StagingMonthlyTime row)
        {
            return !string.IsNullOrWhiteSpace(row.PactId)
                && !string.IsNullOrWhiteSpace(row.TimeCode)
                && row.Month.HasValue
                && !string.IsNullOrWhiteSpace(row.ParentProject);
        }

        private static void MarkRowAsInvalid(StagingMonthlyTime row, string failureMessage)
        {
            row.Passed = false;
            row.FailureComments = failureMessage;
        }

        private async Task<bool> TryImportRowToLiveAsync(StagingMonthlyTime row, string failureMessage)
        {
            MonthlyTime? liveRow = null;
            MonthlyTimeLog? logEntry = null;

            try
            {
                liveRow = CreateLiveRow(row);
                logEntry = BuildLogEntry(liveRow, "I");

                await _context.MonthlyTimes.AddAsync(liveRow);
                await _context.MonthlyTimeLogs.AddAsync(logEntry);
                _context.StagingMonthlyTimes.Remove(row);

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                DetachIfTracked(liveRow);
                DetachIfTracked(logEntry);

                var rowEntry = _context.Entry(row);
                if (rowEntry.State == EntityState.Deleted)
                    rowEntry.State = EntityState.Unchanged;

                row.Passed = false;
                row.FailureComments = failureMessage;
                rowEntry.State = EntityState.Modified;

                await _context.SaveChangesAsync();
                return false;
            }
        }

        private MonthlyTime CreateLiveRow(StagingMonthlyTime row)
        {
            return new MonthlyTime
            {
                PactStaffId = row.PactId ?? string.Empty,
                TimeCode = row.TimeCode ?? string.Empty,
                Month = row.Month!.Value,
                ParentProject = row.ParentProject ?? string.Empty,
                WorkGroup = row.WorkGroup ?? string.Empty,
                Hours = row.Hours,
                FpsYear = _fpsRequestContext.FpsYear
            };
        }

        private void DetachIfTracked(object? entity)
        {
            if (entity == null)
                return;

            var entry = _context.Entry(entity);
            if (entry.State != EntityState.Detached)
                entry.State = EntityState.Detached;
        }

        public async Task<PagedData<MonthlyTimeLog>> SearchAsync(
    PaginationParameters<string> query,
    MonthlyTimeLogFilter monthlyTimeLogFilter)
        {
            string? workGroup = monthlyTimeLogFilter.WorkGroup;
            string? timeCode = monthlyTimeLogFilter.TimeCode;
            string? pactStaffId = monthlyTimeLogFilter.PactStaffId;
            string? parentProject = monthlyTimeLogFilter.ParentProject;
            DateTime? dateImported = monthlyTimeLogFilter.DateImported;
            double? month = monthlyTimeLogFilter.Month;
            string? userId = monthlyTimeLogFilter.UserId;
            string? insertDelete = monthlyTimeLogFilter.InsertDelete;

            IQueryable<MonthlyTimeLog> baseQuery = _context.MonthlyTimeLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(workGroup))
                baseQuery = baseQuery.Where(x => x.WorkGroup == workGroup);

            if (!string.IsNullOrWhiteSpace(timeCode))
                baseQuery = baseQuery.Where(x => x.TimeCode == timeCode);

            if (!string.IsNullOrWhiteSpace(pactStaffId))
                baseQuery = baseQuery.Where(x => x.PactStaffId == pactStaffId);

            if (!string.IsNullOrWhiteSpace(parentProject))
                baseQuery = baseQuery.Where(x => x.ParentProject == parentProject);

            if (dateImported.HasValue)
            {
                var dateOnly = dateImported.Value.Date;
                baseQuery = baseQuery.Where(x => x.DateTime.HasValue
                    && x.DateTime.Value.Date == dateOnly);
            }

            if (month.HasValue)
                baseQuery = baseQuery.Where(x => (int)x.Month == (int)month.Value);

            if (!string.IsNullOrWhiteSpace(userId))
                baseQuery = baseQuery.Where(x => x.UserId != null && x.UserId.Contains(userId));

            if (!string.IsNullOrWhiteSpace(insertDelete))
                baseQuery = baseQuery.Where(x => x.InsertDelete != null
                    && x.InsertDelete.StartsWith(insertDelete));

            baseQuery = ApplyMonthlyTimeFilter(baseQuery, query.Filter);
            baseQuery = (IQueryable<MonthlyTimeLog>)ApplySorting(baseQuery, query.SortBy, query.Descending);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        private static IQueryable<MonthlyTimeLog> ApplyMonthlyTimeFilter(IQueryable<MonthlyTimeLog> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            var dict = DeserializeFilterToDictionary(filter);
            if (dict == null)
                return query;

            query = ApplyWhen(query, TryGetIntValue(dict, "SequenceNo", out var sequenceNoValue), x => x.SequenceNo == sequenceNoValue);
            query = ApplyWhen(query, TryGetStringValue(dict, "TimeCode", out var timeCode), x => x.TimeCode != null && EF.Functions.ILike(x.TimeCode, $"%{timeCode}%"));
            query = ApplyWhen(query, TryGetStringValue(dict, "ParentProject", out var parentProject), x => x.ParentProject != null && EF.Functions.ILike(x.ParentProject, $"%{parentProject}%"));
            query = ApplyWhen(query, TryGetDoubleValue(dict, "Month", out var monthValue), x => (int)x.Month == (int)monthValue);
            query = ApplyWhen(query, TryGetStringValue(dict, "PactStaffId", out var pactStaffId), x => x.PactStaffId != null && EF.Functions.ILike(x.PactStaffId, $"%{pactStaffId}%"));
            query = ApplyWhen(query, TryGetStringValue(dict, "WorkGroup", out var workGroup), x => x.WorkGroup != null && EF.Functions.ILike(x.WorkGroup, $"%{workGroup}%"));
            query = ApplyWhen(query, TryGetDoubleValue(dict, "Hours", out var hoursValue), x => x.Hours.HasValue && x.Hours.Value == hoursValue);
            query = ApplyWhen(query, TryGetDateValue(dict, "DateTime", out var importedDate), x => x.DateTime.HasValue && x.DateTime.Value.Date == importedDate.Date);
            query = ApplyWhen(query, TryGetStringValue(dict, "UserId", out var userId), x => x.UserId != null && EF.Functions.ILike(x.UserId, $"%{userId}%"));
            query = ApplyWhen(query, TryGetStringValue(dict, "InsertDelete", out var insertDelete), x => x.InsertDelete != null && EF.Functions.ILike(x.InsertDelete, $"%{insertDelete}%"));

            return query;
        }

        private static IQueryable<MonthlyTimeLog> ApplyWhen(
            IQueryable<MonthlyTimeLog> query,
            bool condition,
            Expression<Func<MonthlyTimeLog, bool>> predicate)
            => condition ? query.Where(predicate) : query;

        private static IDictionary<string, object>? DeserializeFilterToDictionary(string filter)
        {
            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            return filterModel as IDictionary<string, object>;
        }

        private static bool TryGetStringValue(IDictionary<string, object> dict, string key, out string value)
        {
            value = string.Empty;
            if (!dict.TryGetValue(key, out var rawValue) || rawValue == null)
                return false;

            value = rawValue.ToString() ?? string.Empty;
            return true;
        }

        private static bool TryGetIntValue(IDictionary<string, object> dict, string key, out int value)
        {
            value = default;
            return dict.TryGetValue(key, out var rawValue)
                && rawValue != null
                && int.TryParse(rawValue.ToString(), out value);
        }

        private static bool TryGetDoubleValue(IDictionary<string, object> dict, string key, out double value)
        {
            value = default;
            return dict.TryGetValue(key, out var rawValue)
                && rawValue != null
                && double.TryParse(rawValue.ToString(), out value);
        }

        private static bool TryGetDateValue(IDictionary<string, object> dict, string key, out DateTime value)
        {
            value = default;
            return dict.TryGetValue(key, out var rawValue)
                && rawValue != null
                && DateTime.TryParse(rawValue.ToString(), out value);
        }

        private static IQueryable ApplySorting(IQueryable<MonthlyTimeLog> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query.OrderByDescending(x => x.DateTime).ThenBy(x => x.SequenceNo);

            return sortBy.ToLower() switch
            {
                "sequenceno" or "id" => ApplyOrder(query, x => x.SequenceNo, descending),
                "timecode" => ApplyOrder(query, x => x.TimeCode, descending),
                "parentproject" or "project" => ApplyOrder(query, x => x.ParentProject, descending),
                "month" => ApplyOrder(query, x => x.Month, descending),
                "pactstaffid" or "staffid" => ApplyOrder(query, x => x.PactStaffId, descending),
                "workgroup" => ApplyOrder(query, x => x.WorkGroup, descending),
                "hours" => ApplyOrder(query, x => x.Hours, descending),
                "datetime" or "dateimported" => ApplyOrder(query, x => x.DateTime, descending),
                "userid" => ApplyOrder(query, x => x.UserId, descending),
                "insertdelete" or "action" => ApplyOrder(query, x => x.InsertDelete, descending),
                _ => query.OrderByDescending(x => x.DateTime).ThenBy(x => x.SequenceNo)
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<MonthlyTimeLog> query, Expression<Func<MonthlyTimeLog, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private static IQueryable<StagingMonthlyTime> ApplyStagingFilter(IQueryable<StagingMonthlyTime> stagingQuery, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return stagingQuery;
            }

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
            {
                return stagingQuery;
            }

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("WorkGroup", out var workGroup) && workGroup != null)
                stagingQuery = stagingQuery.Where(x => EF.Functions.ILike(x.WorkGroup!, $"%{workGroup}%"));

            if (dict.TryGetValue("PactStaffId", out var pactStaffId) && pactStaffId != null)
                stagingQuery = stagingQuery.Where(x => EF.Functions.ILike(x.PactStaffId!, $"%{pactStaffId}%"));

            if (dict.TryGetValue("Name", out var name) && name != null)
                stagingQuery = stagingQuery.Where(x => EF.Functions.ILike(x.Name!, $"%{name}%"));

            if (dict.TryGetValue("TimeCode", out var timeCode) && timeCode != null)
                stagingQuery = stagingQuery.Where(x => EF.Functions.ILike(x.TimeCode!, $"%{timeCode}%"));

            if (dict.TryGetValue("ParentProject", out var parentProject) && parentProject != null)
                stagingQuery = stagingQuery.Where(x => EF.Functions.ILike(x.ParentProject!, $"%{parentProject}%"));            

            return stagingQuery;
        }

        private static IQueryable ApplyLiveSorting(IQueryable<MonthlyTimeStaff> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderBy(x => x.WorkGroup)
                .ThenBy(x => x.PactStaffId)
                .ThenBy(x => x.TimeCode)
                .ThenBy(x => x.ParentProject)
                .ThenBy(x => x.Month);
            }               

            return ApplyLiveSortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplyLiveSortingByProperty(IQueryable<MonthlyTimeStaff> query, string property, bool descending)
        {
            return property switch
            {
                "workgroup" => ApplyLiveOrder(query, s => s.WorkGroup, descending),
                "name" => ApplyLiveOrder(query, s => s.Name, descending),
                "timecode" => ApplyLiveOrder(query, s => s.TimeCode, descending),
                "parentproject" => ApplyLiveOrder(query, s => s.ParentProject, descending),
                "month" => ApplyLiveOrder(query, s => s.Month, descending),
                "hours" => ApplyLiveOrder(query, s => s.Hours, descending),
                "pactstaffid" => ApplyLiveOrder(query, s => s.PactStaffId, descending),
                _ => query.OrderBy(x => x.WorkGroup).ThenBy(x => x.PactStaffId).ThenBy(x => x.TimeCode).ThenBy(x => x.ParentProject).ThenBy(x => x.Month)
            };
        }

        private static IQueryable ApplyLiveOrder<T>(IQueryable<MonthlyTimeStaff> query, Expression<Func<MonthlyTimeStaff, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private static IQueryable ApplyStagingSorting(IQueryable<StagingMonthlyTime> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderBy(x => x.WorkGroup)
                .ThenBy(x => x.PactStaffId)
                .ThenBy(x => x.TimeCode)
                .ThenBy(x => x.ParentProject)
                .ThenBy(x => x.Month)
                .ThenBy(x => x.Id);
            }

            return ApplyStagingSortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplyStagingSortingByProperty(IQueryable<StagingMonthlyTime> query, string property, bool descending)
        {
            return property switch
            {
                "workgroup" => ApplyStagingOrder(query, s => s.WorkGroup, descending),
                "pactstaffid" => ApplyStagingOrder(query, s => s.PactStaffId, descending),
                "name" => ApplyStagingOrder(query, s => s.Name, descending),
                "timecode" => ApplyStagingOrder(query, s => s.TimeCode, descending),
                "parentproject" => ApplyStagingOrder(query, s => s.ParentProject, descending),
                "period" => ApplyStagingOrder(query, s => s.Month, descending),
                "hours" => ApplyStagingOrder(query, s => s.Hours, descending),
                "passed" => ApplyStagingOrder(query, s => s.Passed, descending),
                "pactid" => ApplyStagingOrder(query, s => s.PactId, descending),
                "failurecomments" => ApplyStagingOrder(query, s => s.FailureComments, descending),
                _ => query.OrderBy(x => x.WorkGroup).ThenBy(x => x.PactStaffId).ThenBy(x => x.TimeCode).ThenBy(x => x.ParentProject).ThenBy(x => x.Month)
            };
        }

        private static IQueryable ApplyStagingOrder<T>(IQueryable<StagingMonthlyTime> query, Expression<Func<StagingMonthlyTime, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private MonthlyTimeLog BuildLogEntry(MonthlyTime entity, string insertDelete)
        {
            return new MonthlyTimeLog
            {
                PactStaffId = entity.PactStaffId,
                TimeCode = entity.TimeCode,
                Month = entity.Month,
                ParentProject = entity.ParentProject,
                WorkGroup = entity.WorkGroup,
                Hours = entity.Hours,
                DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UserId = _fpsRequestContext.UserEmailId,
                InsertDelete = insertDelete,
                FpsYear = entity.FpsYear
            };
        }
    }
}
