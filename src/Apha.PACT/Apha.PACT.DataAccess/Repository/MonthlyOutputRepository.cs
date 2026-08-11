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
    public class MonthlyOutputRepository : BaseRepository, IMonthlyOutputRepository
    {
        private readonly IFpsRequestContext _fpsRequestContext;

        public MonthlyOutputRepository(FpsDbContext context, IFpsRequestContext fpsRequestContext) : base(context)
        {
            _fpsRequestContext = fpsRequestContext;
        }

        // ── Log search ──────────────────────────────────────────────────────────

        public async Task<PagedData<MonthlyOutputLog>> GetMonthlyOutputLogAsync(
            PaginationParameters<string> query,
            string? workGroup,
            string? testCode,
            string? buyer,
            DateTime? dateImported,
            double? month,
            string? userId,
            string? insertDelete)
        {
            IQueryable<MonthlyOutputLog> baseQuery = _context.MonthlyOutputLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(workGroup))
                baseQuery = baseQuery.Where(x => x.WorkGroup == workGroup);

            if (!string.IsNullOrWhiteSpace(testCode))
                baseQuery = baseQuery.Where(x => x.TestCode == testCode);

            if (!string.IsNullOrWhiteSpace(buyer))
                baseQuery = baseQuery.Where(x => x.Buyer == buyer);

            if (dateImported.HasValue)
            {
                var dateOnly = dateImported.Value.Date;
                baseQuery = baseQuery.Where(x => x.DateTime.HasValue && x.DateTime.Value.Date == dateOnly);
            }

            if (month.HasValue)
                baseQuery = baseQuery.Where(x => x.Month.HasValue && (int)x.Month.Value == (int)month.Value);

            if (!string.IsNullOrWhiteSpace(userId))
                baseQuery = baseQuery.Where(x => x.UserId != null && x.UserId.Contains(userId));

            if (!string.IsNullOrWhiteSpace(insertDelete))
                baseQuery = baseQuery.Where(x => x.InsertDelete != null && x.InsertDelete.StartsWith(insertDelete));

            baseQuery = ApplyMonthlyOutputFilter(baseQuery, query.Filter);
            baseQuery = (IQueryable<MonthlyOutputLog>)ApplySorting(baseQuery, query.SortBy, query.Descending);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        private static IQueryable<MonthlyOutputLog> ApplyMonthlyOutputFilter(IQueryable<MonthlyOutputLog> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            IDictionary<string, object> dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("SequenceNo", out object? sequenceNo)
                && sequenceNo != null
                && int.TryParse(sequenceNo.ToString(), out int sequenceNoValue))
            {
                query = query.Where(x => x.SequenceNo == sequenceNoValue);
            }

            if (dict.TryGetValue("TestCode", out object? testCode) && testCode != null)
                query = query.Where(x => x.TestCode != null && EF.Functions.ILike(x.TestCode, $"%{testCode}%"));

            if (dict.TryGetValue("Buyer", out object? buyer) && buyer != null)
                query = query.Where(x => x.Buyer != null && EF.Functions.ILike(x.Buyer, $"%{buyer}%"));

            if (dict.TryGetValue("WorkGroup", out object? workGroup) && workGroup != null)
                query = query.Where(x => x.WorkGroup != null && EF.Functions.ILike(x.WorkGroup, $"%{workGroup}%"));

            if (dict.TryGetValue("Month", out object? month) && month != null && double.TryParse(month.ToString(), out double monthValue))
                query = query.Where(x => x.Month.HasValue && (int)x.Month.Value == (int)monthValue);

            if (dict.TryGetValue("Volume", out object? volume) && volume != null && double.TryParse(volume.ToString(), out double volumeValue))
                query = query.Where(x => x.Volume.HasValue && x.Volume.Value == volumeValue);

            if (dict.TryGetValue("DateTime", out object? dateImported) && dateImported != null && DateTime.TryParse(dateImported.ToString(), out DateTime importedDate))
            {
                var dateOnly = importedDate.Date;
                query = query.Where(x => x.DateTime.HasValue && x.DateTime.Value.Date == dateOnly);
            }

            if (dict.TryGetValue("UserId", out object? userId) && userId != null)
                query = query.Where(x => x.UserId != null && EF.Functions.ILike(x.UserId, $"%{userId}%"));

            if (dict.TryGetValue("InsertDelete", out object? insertDelete) && insertDelete != null)
                query = query.Where(x => x.InsertDelete != null && EF.Functions.ILike(x.InsertDelete, $"%{insertDelete}%"));

            return query;
        }

        private static IQueryable ApplySorting(IQueryable<MonthlyOutputLog> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return query.OrderByDescending(x => x.DateTime).ThenBy(x => x.SequenceNo);

            return sortBy.ToLower() switch
            {
                "sequenceno" or "id" => ApplyOrder(query, x => x.SequenceNo, descending),
                "testcode" => ApplyOrder(query, x => x.TestCode, descending),
                "buyer" => ApplyOrder(query, x => x.Buyer, descending),
                "month" => ApplyOrder(query, x => x.Month, descending),
                "workgroup" => ApplyOrder(query, x => x.WorkGroup, descending),
                "volume" => ApplyOrder(query, x => x.Volume, descending),
                "datetime" or "dateimported" => ApplyOrder(query, x => x.DateTime, descending),
                "userid" => ApplyOrder(query, x => x.UserId, descending),
                "insertdelete" or "action" => ApplyOrder(query, x => x.InsertDelete, descending),
                _ => query.OrderByDescending(x => x.DateTime).ThenBy(x => x.SequenceNo)
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<MonthlyOutputLog> query, Expression<Func<MonthlyOutputLog, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        public async Task<bool> ExistsByTestCodeAndWorkGroupAsync(string testCode, string workGroup)
        {
            return await _context.MonthlyOutputs
                .AsNoTracking()
                .AnyAsync(m => m.TestCode == testCode && m.WorkGroup == workGroup);
        }

        public async Task<bool> LiveRecordExistsAsync(string testCode, string buyer, double month, string workGroup)
        {
            return await _context.MonthlyOutputs
                .AsNoTracking()
                .AnyAsync(m => m.TestCode == testCode && m.Buyer == buyer
                            && (int)m.Month == (int)month && m.WorkGroup == workGroup);
        }


        public async Task<PagedData<MonthlyOutput>> SearchLiveAsync(
            PaginationParameters<string> query,
            string? workGroup,
            string? testCode,
            string? buyer,
            double? month)
        {
            var monthlyOutputs = _context.MonthlyOutputs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(workGroup))
                monthlyOutputs = monthlyOutputs.Where(x => x.WorkGroup == workGroup);
            if (!string.IsNullOrWhiteSpace(testCode))
                monthlyOutputs = monthlyOutputs.Where(x => x.TestCode == testCode);
            if (!string.IsNullOrWhiteSpace(buyer))
                monthlyOutputs = monthlyOutputs.Where(x => x.Buyer == buyer);
            if (month.HasValue)
                monthlyOutputs = monthlyOutputs.Where(x => (int)x.Month == (int)month.Value);

            monthlyOutputs = (IQueryable<MonthlyOutput>)ApplyLiveSorting(monthlyOutputs, query.SortBy, query.Descending);

            var pagedLiveData = await ApplyPaging(monthlyOutputs, query.Page, query.PageSize);

            pagedLiveData.Total = await monthlyOutputs.SumAsync(x => (decimal)(x.Volume ?? 0));
            return pagedLiveData;
        }

        public async Task<MonthlyOutput?> GetLiveByKeyAsync(string testCode, string buyer, double month, string workGroup)
        {
            return await _context.MonthlyOutputs
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.TestCode == testCode && m.Buyer == buyer
                                       && (int)m.Month == (int)month && m.WorkGroup == workGroup);
        }

        public async Task<MonthlyOutput> UpdateLiveAsync(
            MonthlyOutput monthlyOutput,
            string originalTestCode,
            string originalBuyer,
            double originalMonth,
            string originalWorkGroup)
        {
            var existing = await _context.MonthlyOutputs
                .FirstOrDefaultAsync(m => m.TestCode == originalTestCode && m.Buyer == originalBuyer
                                       && (int)m.Month == (int)originalMonth && m.WorkGroup == originalWorkGroup)
                ?? throw new KeyNotFoundException("Monthly Output live record not found.");

            existing.TestCode = monthlyOutput.TestCode;
            existing.Buyer = monthlyOutput.Buyer;
            existing.Month = monthlyOutput.Month;
            existing.WorkGroup = monthlyOutput.WorkGroup;
            existing.Volume = monthlyOutput.Volume;

            var logEntry = BuildLogEntry(existing, "U");
            await _context.MonthlyOutputLogs.AddAsync(logEntry);

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteLiveAsync(string testCode, string buyer, double month, string workGroup)
        {
            var existing = await _context.MonthlyOutputs
                .FirstOrDefaultAsync(m => m.TestCode == testCode && m.Buyer == buyer
                                       && (int)m.Month == (int)month && m.WorkGroup == workGroup);

            if (existing is null)
                return false;

            var logEntry = BuildLogEntry(existing, "D");
            await _context.MonthlyOutputLogs.AddAsync(logEntry);

            _context.MonthlyOutputs.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<PagedData<StagingMonthlyOutput>> SearchStagingAsync(
            PaginationParameters<string> query,
            string importedBy,
            bool? passed)
        {
            var stagingQuery = _context.StagingMonthlyOutputs
                .AsNoTracking()
                .Where(x => x.ImportedBy == importedBy);

            if (passed.HasValue)
            {
                if (passed.Value)
                {
                    stagingQuery = stagingQuery.Where(x => x.Passed == true);
                }
                else
                {
                    stagingQuery = stagingQuery.Where(x => x.Passed == false);
                }
            }

            stagingQuery = ApplyStagingFilter(stagingQuery, query.Filter);
            stagingQuery = (IQueryable<StagingMonthlyOutput>)ApplyStagingSorting(stagingQuery, query.SortBy, query.Descending);

            var pagedStagingData = await ApplyPaging(stagingQuery, query.Page, query.PageSize);
            pagedStagingData.Total = await stagingQuery.SumAsync(x => (decimal)(x.Volume ?? 0));
            return pagedStagingData;
        }

        public async Task<StagingMonthlyOutput?> GetStagingByIdAsync(int id, string importedBy)
        {
            return await _context.StagingMonthlyOutputs
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.ImportedBy == importedBy);
        }

        public async Task<StagingMonthlyOutput> CreateStagingAsync(StagingMonthlyOutput stagingMonthlyOutput)
        {
            await _context.StagingMonthlyOutputs.AddAsync(stagingMonthlyOutput);
            await _context.SaveChangesAsync();
            return stagingMonthlyOutput;
        }

        public async Task<StagingMonthlyOutput> UpdateStagingAsync(StagingMonthlyOutput stagingMonthlyOutput, string importedBy)
        {
            var existing = await _context.StagingMonthlyOutputs
                .FirstOrDefaultAsync(x => x.Id == stagingMonthlyOutput.Id && x.ImportedBy == importedBy)
                ?? throw new KeyNotFoundException($"Staging Monthly Output record {stagingMonthlyOutput.Id} not found.");

            existing.TestCode = stagingMonthlyOutput.TestCode;
            existing.Buyer = stagingMonthlyOutput.Buyer;
            existing.Month = stagingMonthlyOutput.Month;
            existing.WorkGroup = stagingMonthlyOutput.WorkGroup;
            existing.Volume = stagingMonthlyOutput.Volume;
            existing.Passed = false;
            existing.FailureComments = "This record has been edited since being validated. Needs re-validating.";

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteStagingAsync(int id, string importedBy)
        {
            var row = await _context.StagingMonthlyOutputs
                .FirstOrDefaultAsync(x => x.Id == id && x.ImportedBy == importedBy);

            if (row is null) return false;
            _context.StagingMonthlyOutputs.Remove(row);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteAllStagingByUserAsync(string importedBy)
        {
            var rows = await _context.StagingMonthlyOutputs
                .Where(x => x.ImportedBy == importedBy)
                .ToListAsync();

            _context.StagingMonthlyOutputs.RemoveRange(rows);
            await _context.SaveChangesAsync();
            return rows.Count;
        }

        public async Task<int> DeleteFailedStagingByUserAsync(string importedBy)
        {
            var rows = await _context.StagingMonthlyOutputs
                .Where(x => x.ImportedBy == importedBy && x.Passed == false)
                .ToListAsync();

            _context.StagingMonthlyOutputs.RemoveRange(rows);
            await _context.SaveChangesAsync();
            return rows.Count;
        }

        public async Task<int> ImportStagingAsync(IEnumerable<StagingMonthlyOutput> stagingRows)
        {
            var list = stagingRows.ToList();
            await _context.StagingMonthlyOutputs.AddRangeAsync(list);
            await _context.SaveChangesAsync();
            return list.Count;
        }

        public async Task<int> RemoveZeroAndNullVolumeRecordsAsync(string importedBy)
        {
            var rows = await _context.StagingMonthlyOutputs
                .Where(x => x.ImportedBy == importedBy
                         && (x.Volume == null || x.Volume == 0))
                .ToListAsync();

            _context.StagingMonthlyOutputs.RemoveRange(rows);
            await _context.SaveChangesAsync();
            return rows.Count;
        }

        public async Task<List<StagingMonthlyOutput>> GetStagingRecordsForValidationAsync(string importedBy)
        {
            return await _context.StagingMonthlyOutputs
                .Where(x => x.ImportedBy == importedBy && x.Passed == false)
                .OrderBy(x => x.Id)
                .ToListAsync();
        }

        public async Task UpdateStagingRecordsAsync(IEnumerable<StagingMonthlyOutput> records)
        {
            foreach (var record in records)
            {
                _context.Entry(record).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasFailedStagingAsync(string importedBy)
        {
            return await _context.StagingMonthlyOutputs
                .AsNoTracking()
                .AnyAsync(x => x.ImportedBy == importedBy && x.Passed == false);
        }


        public async Task<(int ProcessedCount, int ImportedCount, int FailedCount)> MakeLiveAsync(string importedBy)
        {
            const string noLongerValidMessage = "This record is no longer valid. Needs re-validating";

            var passedRows = await _context.StagingMonthlyOutputs
                .Where(x => x.ImportedBy == importedBy && x.Passed == true)
                .OrderBy(x => x.Id)
                .ToListAsync();

            if (passedRows.Count == 0)
                return (0, 0, 0);

            var failedCount = await _context.StagingMonthlyOutputs
                .AsNoTracking()
                .CountAsync(x => x.ImportedBy == importedBy && x.Passed == false);

            var importedCount = 0;

            foreach (var row in passedRows)
            {
                if (!IsValidForMakeLive(row))
                {
                    await MarkRowAsInvalidAsync(row, noLongerValidMessage);
                    failedCount++;
                    continue;
                }

                var imported = await TryImportRowToLiveAsync(row, noLongerValidMessage);
                if (imported)
                    importedCount++;
                else
                    failedCount++;
            }

            return (passedRows.Count, importedCount, failedCount);
        }

        private static bool IsValidForMakeLive(StagingMonthlyOutput row)
        {
            return !string.IsNullOrWhiteSpace(row.TestCode)
                && !string.IsNullOrWhiteSpace(row.Buyer)
                && row.Month != 0
                && !string.IsNullOrWhiteSpace(row.WorkGroup);
        }

        private async Task MarkRowAsInvalidAsync(StagingMonthlyOutput row, string failureMessage)
        {
            row.Passed = false;
            row.FailureComments = failureMessage;
            await _context.SaveChangesAsync();
        }

        private async Task<bool> TryImportRowToLiveAsync(StagingMonthlyOutput row, string failureMessage)
        {
            MonthlyOutput? liveRow = null;
            MonthlyOutputLog? logEntry = null;

            try
            {
                liveRow = CreateLiveRow(row);
                logEntry = BuildLogEntry(liveRow, "I");

                await _context.MonthlyOutputs.AddAsync(liveRow);
                await _context.MonthlyOutputLogs.AddAsync(logEntry);
                _context.StagingMonthlyOutputs.Remove(row);

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

        private MonthlyOutput CreateLiveRow(StagingMonthlyOutput row)
        {
            return new MonthlyOutput
            {
                TestCode = row.TestCode,
                Buyer = row.Buyer,
                Month = row.Month,
                WorkGroup = row.WorkGroup,
                Volume = row.Volume,
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


        private static IQueryable<StagingMonthlyOutput> ApplyStagingFilter(IQueryable<StagingMonthlyOutput> stagingQuery, string? filter)
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

            if (dict.TryGetValue("TestCode", out var testCode) && testCode != null)
                stagingQuery = stagingQuery.Where(x => EF.Functions.ILike(x.TestCode!, $"%{testCode}%"));

            if (dict.TryGetValue("Buyer", out var buyer) && buyer != null)
                stagingQuery = stagingQuery.Where(x => EF.Functions.ILike(x.Buyer!, $"%{buyer}%"));

            if (dict.TryGetValue("FailureComments", out var failureComments) && failureComments != null)
                stagingQuery = stagingQuery.Where(x => EF.Functions.ILike(x.FailureComments!, $"%{failureComments}%"));

            return stagingQuery;
        }

        private static IQueryable ApplyLiveSorting(IQueryable<MonthlyOutput> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderBy(x => x.WorkGroup)
                    .ThenBy(x => x.TestCode)
                    .ThenBy(x => x.Buyer)
                    .ThenBy(x => x.Month);
            }

            return ApplyLiveSortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplyLiveSortingByProperty(IQueryable<MonthlyOutput> query, string property, bool descending)
        {
            return property switch
            {
                "workgroup" => ApplyLiveOrder(query, s => s.WorkGroup, descending),
                "testcode" => ApplyLiveOrder(query, s => s.TestCode, descending),
                "buyer" => ApplyLiveOrder(query, s => s.Buyer, descending),
                "month" => ApplyLiveOrder(query, s => s.Month, descending),
                "volume" => ApplyLiveOrder(query, s => s.Volume, descending),
                _ => query.OrderBy(x => x.WorkGroup).ThenBy(x => x.TestCode).ThenBy(x => x.Buyer).ThenBy(x => x.Month)
            };
        }

        private static IQueryable ApplyLiveOrder<T>(IQueryable<MonthlyOutput> query, Expression<Func<MonthlyOutput, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private static IQueryable ApplyStagingSorting(IQueryable<StagingMonthlyOutput> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderBy(x => x.WorkGroup)
                    .ThenBy(x => x.TestCode)
                    .ThenBy(x => x.Buyer)
                    .ThenBy(x => x.Month)
                    .ThenBy(x => x.Id);
            }

            return ApplyStagingSortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplyStagingSortingByProperty(IQueryable<StagingMonthlyOutput> query, string property, bool descending)
        {
            return property switch
            {
                "workgroup" => ApplyStagingOrder(query, s => s.WorkGroup, descending),
                "testcode" => ApplyStagingOrder(query, s => s.TestCode, descending),
                "buyer" => ApplyStagingOrder(query, s => s.Buyer, descending),
                "month" => ApplyStagingOrder(query, s => s.Month, descending),
                "volume" => ApplyStagingOrder(query, s => s.Volume, descending),
                "pass" => ApplyStagingOrder(query, s => s.Passed, descending),
                "failurecomments" => ApplyStagingOrder(query, s => s.FailureComments, descending),
                _ => query.OrderBy(x => x.WorkGroup).ThenBy(x => x.TestCode).ThenBy(x => x.Buyer).ThenBy(x => x.Month)
            };
        }

        private static IQueryable ApplyStagingOrder<T>(IQueryable<StagingMonthlyOutput> query, Expression<Func<StagingMonthlyOutput, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private MonthlyOutputLog BuildLogEntry(MonthlyOutput row, string insertDelete)
        {
            return new MonthlyOutputLog
            {
                TestCode = row.TestCode,
                Buyer = row.Buyer,
                Month = row.Month,
                WorkGroup = row.WorkGroup,
                Volume = row.Volume,
                WgBuyer = row.WgBuyer,
                DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UserId = _fpsRequestContext.UserEmailId,
                InsertDelete = insertDelete,
                FpsYear = _fpsRequestContext.FpsYear
            };
        }
    }
}
