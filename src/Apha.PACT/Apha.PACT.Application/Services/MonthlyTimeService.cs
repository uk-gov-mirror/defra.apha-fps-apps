using Apha.Common.Utilities.ExcelImport;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class MonthlyTimeService : IMonthlyTimeService
    {
        private readonly IMonthlyTimeRepository _repository;
        private readonly ICalenderMonthRepository _calenderMonthRepository;
        private readonly IWorkGroupRepository _workGroupRepository;
        private readonly ITimeCodeValidRepository _timeCodeValidRepository;
        private readonly IMapper _mapper;

        public MonthlyTimeService(
            IMonthlyTimeRepository repository,
            IMapper mapper,
            ICalenderMonthRepository calenderMonthRepository,
            IWorkGroupRepository workGroupRepository,
            ITimeCodeValidRepository timeCodeValidRepository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _calenderMonthRepository = calenderMonthRepository ?? throw new ArgumentNullException(nameof(calenderMonthRepository));
            _workGroupRepository = workGroupRepository ?? throw new ArgumentNullException(nameof(workGroupRepository));
            _timeCodeValidRepository = timeCodeValidRepository ?? throw new ArgumentNullException(nameof(timeCodeValidRepository));
        }

        public async Task<PaginatedResult<MonthlyTimeDto>> SearchLiveAsync(
            QueryParameters<string> query,
            string? workGroup,
            string? timeCode,
            string? pactStaffId,
            string? parentProject,
            double? month)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _repository.SearchLiveAsync(filter, workGroup, timeCode, pactStaffId, parentProject, month);
            return _mapper.Map<PaginatedResult<MonthlyTimeDto>>(result);
        }

        public async Task<MonthlyTimeDto?> GetLiveByKeyAsync(string pactStaffId, string timeCode, double month, string parentProject)
        {
            var entity = await _repository.GetLiveByKeyAsync(pactStaffId, timeCode, month, parentProject);
            return entity == null ? null : _mapper.Map<MonthlyTimeDto>(entity);
        }

        public async Task<MonthlyTimeDto> UpdateLiveAsync(MonthlyTimeDto monthlyTime)
        {
            var entity = _mapper.Map<MonthlyTime>(monthlyTime);

            var originalPactStaffId = string.IsNullOrWhiteSpace(monthlyTime.OriginalPactStaffId)
                ? monthlyTime.PactStaffId
                : monthlyTime.OriginalPactStaffId;

            var updated = await _repository.UpdateLiveAsync(entity, originalPactStaffId);
            return _mapper.Map<MonthlyTimeDto>(updated);
        }

        public async Task<bool> DeleteLiveAsync(string pactStaffId, string timeCode, double month, string parentProject)
        {
            return await _repository.DeleteLiveAsync(pactStaffId, timeCode, month, parentProject);
        }

        public async Task<PaginatedResult<StagingMonthlyTimeDto>> SearchStagingAsync(
            QueryParameters<string> query,
            string importedBy,
            bool? passed)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _repository.SearchStagingAsync(filter, importedBy, passed);
            return _mapper.Map<PaginatedResult<StagingMonthlyTimeDto>>(result);
        }

        public async Task<StagingMonthlyTimeDto?> GetStagingByIdAsync(int id, string importedBy)
        {
            var entity = await _repository.GetStagingByIdAsync(id, importedBy);
            return entity == null ? null : _mapper.Map<StagingMonthlyTimeDto>(entity);
        }

        public async Task<StagingMonthlyTimeDto> CreateStagingAsync(StagingMonthlyTimeDto stagingMonthlyTime, string importedBy)
        {
            var entity = _mapper.Map<StagingMonthlyTime>(stagingMonthlyTime);
            entity.ImportedBy = importedBy;
            entity.ImportedDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            await ValidateSingleRecordAsync(entity);
            var created = await _repository.CreateStagingAsync(entity);
            return _mapper.Map<StagingMonthlyTimeDto>(created);
        }

        public async Task<StagingMonthlyTimeDto> UpdateStagingAsync(StagingMonthlyTimeDto stagingMonthlyTime, string importedBy)
        {
            var entity = _mapper.Map<StagingMonthlyTime>(stagingMonthlyTime);
            entity.ImportedDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            await ValidateSingleRecordAsync(entity);
            var updated = await _repository.UpdateStagingAsync(entity, importedBy);
            return _mapper.Map<StagingMonthlyTimeDto>(updated);
        }

        public async Task<BulkUpdateStagingMonthlyTimeNamesResultDto> BulkUpdateStagingNamesAsync(BulkUpdateStagingMonthlyTimeNamesDto request, string importedBy)
        {
            if (string.IsNullOrWhiteSpace(request.OriginalWorkGroup) || string.IsNullOrWhiteSpace(request.OriginalPactStaffId))
            {
                return new BulkUpdateStagingMonthlyTimeNamesResultDto { UpdatedCount = 0 };
            }

            var updatedCount = await _repository.BulkUpdateStagingNamesAsync(
                importedBy,
                request.OriginalWorkGroup,
                request.OriginalPactStaffId,
                request.NewName,
                request.NewPactStaffId,
                request.NewPactId,
                request.ExcludeId);

            return new BulkUpdateStagingMonthlyTimeNamesResultDto { UpdatedCount = updatedCount };
        }

        private async Task ValidateSingleRecordAsync(StagingMonthlyTime entity)
        {
            var context = await LoadValidationContextAsync();
            var stagingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var failures = ValidateRecord(entity, context, stagingKeys);
            entity.Passed = failures.Count == 0;
            entity.FailureComments = failures.Count == 0
                ? string.Empty
                : string.Join(Environment.NewLine, failures);
        }

        public async Task<bool> DeleteStagingAsync(int id, string importedBy)
        {
            return await _repository.DeleteStagingAsync(id, importedBy);
        }

        public async Task<int> DeleteAllStagingByUserAsync(string importedBy)
        {
            return await _repository.DeleteAllStagingByUserAsync(importedBy);
        }

        public async Task<int> DeleteFailedStagingByUserAsync(string importedBy)
        {
            var deletedCount = await _repository.DeleteFailedStagingByUserAsync(importedBy);
            return deletedCount;
        }

        public async Task<MonthlyTimeImportResultDto> ImportStagingAsync(MonthlyTimeImportDto request, string importedBy)
        {
            var importedDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            if (request.ImportType == 4)
            {
                var rowsToUpdate = new List<StagingMonthlyTime>();
                var rowsToInsert = new List<StagingMonthlyTime>();

                foreach (var row in request.Rows)
                {
                    if (row.Id > 0)
                    {
                        var existing = await _repository.GetStagingByIdAsync(row.Id, importedBy);
                        if (existing != null)
                        {
                            existing.PactStaffId = row.PactStaffId;
                            existing.TimeCode = row.TimeCode;
                            existing.ParentProject = row.ParentProject;
                            existing.Month = ExcelParseHelper.TryParseDouble(row.Month);
                            existing.WorkGroup = row.WorkGroup;
                            existing.Hours = ExcelParseHelper.TryParseDouble(row.Hours);
                            existing.Name = row.Name;
                            existing.PactId = null;
                            existing.Passed = false;
                            existing.FailureComments = string.Empty;
                            rowsToUpdate.Add(existing);
                            continue;
                        }
                    }

                    rowsToInsert.Add(new StagingMonthlyTime
                    {
                        PactStaffId = row.PactStaffId,
                        TimeCode = row.TimeCode,
                        ParentProject = row.ParentProject,
                        Month = ExcelParseHelper.TryParseDouble(row.Month),
                        WorkGroup = row.WorkGroup,
                        Hours = ExcelParseHelper.TryParseDouble(row.Hours),
                        FailureComments = string.Empty,
                        Passed = false,
                        PactId = null,
                        Name = row.Name,
                        Filename = request.FileName,
                        ImportedBy = importedBy,
                        ImportedDate = importedDate
                    });
                }

                if (rowsToUpdate.Count > 0)
                {
                    await _repository.UpdateStagingRecordsAsync(rowsToUpdate);
                }

                var insertedCount = await _repository.ImportStagingAsync(rowsToInsert);
                var processedCount = rowsToUpdate.Count + insertedCount;

                return new MonthlyTimeImportResultDto
                {
                    ImportedCount = processedCount,
                    PassedCount = 0,
                    FailedCount = 0,
                    Message = $"Import completed. {processedCount} rows processed in staging."
                };
            }

            var rows = request.Rows.Select(row => new StagingMonthlyTime
            {
                PactStaffId = row.PactStaffId,
                TimeCode = row.TimeCode,
                ParentProject = row.ParentProject,
                Month = ExcelParseHelper.TryParseDouble(row.Month),
                WorkGroup = row.WorkGroup,
                Hours = ExcelParseHelper.TryParseDouble(row.Hours),
                FailureComments = string.Empty,
                Passed = false,
                PactId = row.PactId,
                Name = row.Name,
                Filename = request.FileName,
                ImportedBy = importedBy,
                ImportedDate = importedDate
            }).ToList();

            var importedCount = await _repository.ImportStagingAsync(rows);
            return new MonthlyTimeImportResultDto
            {
                ImportedCount = importedCount,
                PassedCount = 0,
                FailedCount = 0,
                Message = $"Import completed. {importedCount} rows added to staging."
            };
        }

        public async Task<MonthlyTimeValidateResultDto> ValidateStagingAsync(string importedBy)
        {
            // Remove records with zero or null hours
            await _repository.RemoveZeroAndNullHourRecordsAsync(importedBy);

            var records = await _repository.GetStagingRecordsForValidationAsync(importedBy);

            if (records.Count == 0)
            {
                return new MonthlyTimeValidateResultDto
                {
                    PassedCount = 0,
                    FailedCount = 0,
                    Message = "Validation completed. No records to validate."
                };
            }

            var validationContext = await LoadValidationContextAsync();

            var validationResult = await ValidateRecordsAsync(records, validationContext);

            await _repository.UpdateStagingRecordsAsync(records);

            return validationResult;
        }

        private async Task<ValidationContext> LoadValidationContextAsync()
        {
            var calenderMonths = await _calenderMonthRepository.GetCalenderMonthsAsync();
            return new ValidationContext
            {
                ValidWorkGroups = new HashSet<string>(
                    await _workGroupRepository.GetAllWorkGroupNamesAsync(),
                    StringComparer.OrdinalIgnoreCase),
                ValidMonths = new HashSet<double>(calenderMonths.Select(c => (double)(c.MonthNumber ?? 0))),
                StaffByWorkGroup = await _workGroupRepository.GetStaffByWorkGroupAsync(),
                TimeCodeRows = await _timeCodeValidRepository.GetTimeCodeValidsAsync(),
                ExistingLiveKeys = await _repository.GetExistingLiveKeysAsync()
            };
        }

        private async Task<MonthlyTimeValidateResultDto> ValidateRecordsAsync(
            List<StagingMonthlyTime> records,
            ValidationContext context)
        {
            var stagingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var passedCount = 0;
            var failedCount = 0;

            foreach (var record in records)
            {
                var failures = ValidateRecord(record, context, stagingKeys);

                record.Passed = failures.Count == 0;
                record.FailureComments = failures.Count == 0
                    ? string.Empty
                    : string.Join(Environment.NewLine, failures);

                if (record.Passed == true)
                    passedCount++;
                else
                    failedCount++;
            }

            return new MonthlyTimeValidateResultDto
            {
                PassedCount = passedCount,
                FailedCount = failedCount,
                Message = $"Validation completed. {passedCount} records passed and {failedCount} records failed."
            };
        }

        private static List<string> ValidateRecord(
            StagingMonthlyTime record,
            ValidationContext context,
            HashSet<string> stagingKeys)
        {
            var failures = new List<string>();

            var workGroup = record.WorkGroup?.Trim();
            var staffId = record.PactStaffId?.Trim();
            var name = record.Name?.Trim();
            var timeCode = record.TimeCode?.Trim();
            var parentProject = record.ParentProject?.Trim();
            var month = record.Month;
            var hours = record.Hours;

            ValidateHours(hours, failures);
            ValidateWorkGroup(workGroup, context.ValidWorkGroups, failures);
            ValidateStaff(staffId, name, workGroup, record, context.StaffByWorkGroup, failures);
            ValidateTimeCode(timeCode, workGroup, context.TimeCodeRows, failures);
            ValidateParentProject(parentProject, workGroup, timeCode, context.TimeCodeRows, failures);
            ValidateMonth(month, context.ValidMonths, failures);
            ValidateDuplicates(record, failures, context, stagingKeys);

            return failures;
        }

        private static void ValidateHours(double? hours, List<string> failures)
        {
            if (!hours.HasValue || hours.Value <= 0)
                failures.Add($"The hours field is not a number -\"{hours}\"");
        }

        private static void ValidateWorkGroup(string? workGroup, HashSet<string> validWorkGroups, List<string> failures)
        {
            if (string.IsNullOrWhiteSpace(workGroup))
            {
                failures.Add("The work group name is blank.");
            }
            else if (!validWorkGroups.Contains(workGroup))
            {
                failures.Add($"The work group name is invalid: {workGroup}");
            }
        }

        private static void ValidateStaff(
            string? staffId,
            string? name,
            string? workGroup,
            StagingMonthlyTime record,
            List<WorkGroupStaffItem> staffByWorkGroup,
            List<string> failures)
        {
            if (string.IsNullOrWhiteSpace(staffId))
            {
                failures.Add("Staff ID blank.");
                return;
            }

            if (char.IsDigit(staffId[0]))
            {
                ValidateNumericStaff(staffId, workGroup, record, staffByWorkGroup, failures);
                return;
            }

            ValidateNamedStaff(staffId, name, workGroup, record, staffByWorkGroup, failures);
        }

        private static void ValidateNumericStaff(
            string staffId,
            string? workGroup,
            StagingMonthlyTime record,
            List<WorkGroupStaffItem> staffByWorkGroup,
            List<string> failures)
        {
            var matchBySpNumber = staffByWorkGroup
                .Where(x => x.WorkGroup == workGroup && x.SpNumber == staffId)
                .ToList();

            ProcessStaffMatches(matchBySpNumber, staffId, workGroup, record, failures);
        }

        private static void ValidateNamedStaff(
            string staffId,
            string? name,
            string? workGroup,
            StagingMonthlyTime record,
            List<WorkGroupStaffItem> staffByWorkGroup,
            List<string> failures)
        {
            var matchByName = staffByWorkGroup
                .Where(x => x.WorkGroup == workGroup &&
                (!string.IsNullOrWhiteSpace(staffId) && x.Name == staffId) ||
                (!string.IsNullOrWhiteSpace(name) && x.Name == name))
                .ToList();

            ProcessStaffMatches(matchByName, staffId, workGroup, record, failures);
        }

        private static void ProcessStaffMatches(
            List<WorkGroupStaffItem> matches,
            string staffId,
            string? workGroup,
            StagingMonthlyTime record,
            List<string> failures)
        {
            if (matches.Count == 0)
            {
                failures.Add($"This staff ID not in this WG: {staffId}");
                return;
            }

            if (matches.Count == 1)
            {
                PopulateUniqueStaffMatch(record, matches[0]);
                return;
            }

            if (string.IsNullOrWhiteSpace(record.PactId) || record.PactId == "0")
            {
                failures.Add($"There is more than one person with this name or SP number in {workGroup}, you will need to manually identify which is correct for this record.");
                return;
            }

            record.PactStaffId = matches[0].SpNumber;
            record.Name = matches[0].Name;
        }

        private static void PopulateUniqueStaffMatch(StagingMonthlyTime record, WorkGroupStaffItem match)
        {
            record.PactStaffId = match.SpNumber;
            record.PactId = match.PactId;
            record.Name = match.Name;
        }

        private static void ValidateTimeCode(
            string? timeCode,
            string? workGroup,
            List<TimeCodeValid> timeCodeRows,
            List<string> failures)
        {
            if (string.IsNullOrWhiteSpace(timeCode))
            {
                failures.Add("The Timecode is blank.");
                return;
            }

            var validTimeCodes = timeCodeRows
                .Where(x => x.WorkGroup == workGroup && x.TimeCode == timeCode)
                .ToList();

            if (validTimeCodes.Count == 0)
            {
                failures.Add($"Timecode not valid for this WG or invalid timecode: {timeCode}, {workGroup}");
            }
            else if (!validTimeCodes.Any(x => x.Active))
            {
                failures.Add("Timecode not valid for this WG.");
            }
        }

        private static void ValidateParentProject(
            string? parentProject,
            string? workGroup,
            string? timeCode,
            List<TimeCodeValid> timeCodeRows,
            List<string> failures)
        {
            if (string.IsNullOrWhiteSpace(parentProject))
            {
                failures.Add("The Project is blank.");
            }
            else if (!timeCodeRows.Any(x => x.WorkGroup == workGroup && x.TimeCode == timeCode && x.ParentProject == parentProject))
            {
                failures.Add($"Not valid timecode/Project/WG combination: {timeCode}, {parentProject}, {workGroup}");
            }
        }

        private static void ValidateMonth(double? month, HashSet<double> validMonths, List<string> failures)
        {
            if (!month.HasValue)
            {
                failures.Add("The month No. is blank.");
            }
            else if (!validMonths.Contains(month.Value))
            {
                failures.Add($"The month No. invalid: {month}");
            }
        }

        private static void ValidateDuplicates(
            StagingMonthlyTime record,
            List<string> failures,
            ValidationContext context,
            HashSet<string> stagingKeys)
        {
            if (failures.Count > 0 || string.IsNullOrWhiteSpace(record.PactId))
                return;

            var key = BuildRecordKey(record);

            if (!stagingKeys.Add(key))
            {
                failures.Add($"Similar record in sheet being imported, WG = {record.WorkGroup}, PACTID = {record.PactId}, TimeCode = {record.TimeCode}, ParentProject = {record.ParentProject} and Month = {record.Month}.");
            }
            else if (context.ExistingLiveKeys.Contains(key))
            {
                failures.Add($"Similar record already imported, WG = {record.WorkGroup}, PACTID = {record.PactId}, TimeCode = {record.TimeCode}, ParentProject = {record.ParentProject} and Month = {record.Month} already exists in the MonthlyTime table.");
            }
        }

        private static string BuildRecordKey(StagingMonthlyTime record)
        {
            return $"{record.PactStaffId}|{record.TimeCode}|{record.ParentProject}|{record.WorkGroup}|{record.Month}";
        }

        public async Task<MonthlyTimeMakeLiveResultDto> MakeLiveAsync(string importedBy)
        {
            if (await _repository.HasFailedStagingAsync(importedBy))
            {
                throw new BusinessValidationErrorException([
                    new BusinessValidationError(
                        "All records have to have passed before you can run the import. Either delete or correct those that have failed first.",
                        "FAILED_STAGING_EXISTS")
                ]);
            }

            var result = await _repository.MakeLiveAsync(importedBy);
            return new MonthlyTimeMakeLiveResultDto
            {
                ProcessedCount = result.ProcessedCount,
                ImportedCount = result.ImportedCount,
                FailedCount = result.FailedCount,
                Message = $"Make live completed. {result.ImportedCount} records moved into MonthlyTime."
            };
        }

        public async Task<PaginatedResult<MonthlyTimeLogDto>> SearchAsync(
            QueryParameters<string> query,
            MonthlyTimeLogFilterDto monthlyTimeLogFilter)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var logFilter = _mapper.Map<MonthlyTimeLogFilter>(monthlyTimeLogFilter);

            var result = await _repository.SearchAsync(filter, logFilter);
            return _mapper.Map<PaginatedResult<MonthlyTimeLogDto>>(result);
        }        
    }

    /// <summary>
    /// Helper class to encapsulate validation lookup data for MonthlyTime staging validation.
    /// </summary>
    internal class ValidationContext
    {
        public required HashSet<string> ValidWorkGroups { get; init; }
        public required HashSet<double> ValidMonths { get; init; }
        public required List<WorkGroupStaffItem> StaffByWorkGroup { get; init; }
        public required List<TimeCodeValid> TimeCodeRows { get; init; }
        public required HashSet<string> ExistingLiveKeys { get; init; }
    }
}

