using Apha.Common.Utilities.ExcelImport;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Validation;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class MonthlyOutputService : IMonthlyOutputService
    {
        private readonly IMonthlyOutputRepository _repository;
        private readonly ICalenderMonthRepository _calenderMonthRepository;
        private readonly IWorkGroupRepository _workGroupRepository;
        private readonly ITestCapabilityRepository _testCapabilityRepository;
        private readonly ITestRequirementRepository _testRequirementRepository;
        private readonly IMapper _mapper;

        public MonthlyOutputService(
            IMonthlyOutputRepository repository,
            IMapper mapper,
            ICalenderMonthRepository calenderMonthRepository,
            IWorkGroupRepository workGroupRepository,
            ITestCapabilityRepository testCapabilityRepository,
            ITestRequirementRepository testRequirementRepository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _calenderMonthRepository = calenderMonthRepository ?? throw new ArgumentNullException(nameof(calenderMonthRepository));
            _workGroupRepository = workGroupRepository ?? throw new ArgumentNullException(nameof(workGroupRepository));
            _testCapabilityRepository = testCapabilityRepository ?? throw new ArgumentNullException(nameof(testCapabilityRepository));
            _testRequirementRepository = testRequirementRepository ?? throw new ArgumentNullException(nameof(testRequirementRepository));
        }

        public async Task<PaginatedResult<MonthlyOutputLogDto>> GetMonthlyOutputLogAsync(
            QueryParameters<string> query,
            string? workGroup,
            string? testCode,
            string? buyer,
            DateTime? dateImported,
            double? month,
            string? userId,
            string? insertDelete)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _repository.GetMonthlyOutputLogAsync(filter, workGroup, testCode, buyer, dateImported, month, userId, insertDelete);
            return _mapper.Map<PaginatedResult<MonthlyOutputLogDto>>(result);
        }

        public async Task<PaginatedResult<MonthlyOutputDto>> SearchLiveAsync(
            QueryParameters<string> query,
            string? workGroup,
            string? testCode,
            string? buyer,
            double? month)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _repository.SearchLiveAsync(filter, workGroup, testCode, buyer, month);
            return _mapper.Map<PaginatedResult<MonthlyOutputDto>>(result);
        }

        public async Task<MonthlyOutputDto?> GetLiveByKeyAsync(string testCode, string buyer, double month, string workGroup)
        {
            var entity = await _repository.GetLiveByKeyAsync(testCode, buyer, month, workGroup);
            return entity == null ? null : _mapper.Map<MonthlyOutputDto>(entity);
        }

        public async Task<MonthlyOutputDto> UpdateLiveAsync(MonthlyOutputDto monthlyOutput)
        {
            var entity = _mapper.Map<MonthlyOutput>(monthlyOutput);

            var originalTestCode = string.IsNullOrWhiteSpace(monthlyOutput.OriginalTestCode) ? monthlyOutput.TestCode : monthlyOutput.OriginalTestCode;
            var originalBuyer = string.IsNullOrWhiteSpace(monthlyOutput.OriginalBuyer) ? monthlyOutput.Buyer : monthlyOutput.OriginalBuyer;
            var originalMonth = monthlyOutput.OriginalMonth ?? monthlyOutput.Month;
            var originalWorkGroup = string.IsNullOrWhiteSpace(monthlyOutput.OriginalWorkGroup) ? monthlyOutput.WorkGroup : monthlyOutput.OriginalWorkGroup;

            var updated = await _repository.UpdateLiveAsync(entity, originalTestCode, originalBuyer, originalMonth, originalWorkGroup);
            return _mapper.Map<MonthlyOutputDto>(updated);
        }

        public async Task<bool> DeleteLiveAsync(string testCode, string buyer, double month, string workGroup)
        {
            return await _repository.DeleteLiveAsync(testCode, buyer, month, workGroup);
        }

        public async Task<PaginatedResult<StagingMonthlyOutputDto>> SearchStagingAsync(
            QueryParameters<string> query,
            string importedBy,
            bool? passed)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _repository.SearchStagingAsync(filter, importedBy, passed);
            return _mapper.Map<PaginatedResult<StagingMonthlyOutputDto>>(result);
        }

        public async Task<StagingMonthlyOutputDto?> GetStagingByIdAsync(int id, string importedBy)
        {
            var entity = await _repository.GetStagingByIdAsync(id, importedBy);
            return entity == null ? null : _mapper.Map<StagingMonthlyOutputDto>(entity);
        }

        public async Task<StagingMonthlyOutputDto> CreateStagingAsync(StagingMonthlyOutputDto stagingMonthlyOutput, string importedBy)
        {
            var entity = _mapper.Map<StagingMonthlyOutput>(stagingMonthlyOutput);
            entity.ImportedBy = importedBy;
            entity.ImportedDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            await ValidateSingleRecordAsync(entity);
            var created = await _repository.CreateStagingAsync(entity);
            return _mapper.Map<StagingMonthlyOutputDto>(created);
        }

        public async Task<StagingMonthlyOutputDto> UpdateStagingAsync(StagingMonthlyOutputDto stagingMonthlyOutput, string importedBy)
        {
            var entity = _mapper.Map<StagingMonthlyOutput>(stagingMonthlyOutput);
            entity.ImportedDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            await ValidateSingleRecordAsync(entity);
            var updated = await _repository.UpdateStagingAsync(entity, importedBy);
            return _mapper.Map<StagingMonthlyOutputDto>(updated);
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
            return await _repository.DeleteFailedStagingByUserAsync(importedBy);
        }

        public async Task<MonthlyOutputImportResultDto> ImportStagingAsync(MonthlyOutputImportDto request, string importedBy)
        {
            var importedDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            if (request.ImportType == 4)
            {
                var rowsToUpdate = new List<StagingMonthlyOutput>();
                var rowsToInsert = new List<StagingMonthlyOutput>();

                foreach (var row in request.Rows)
                {
                    if (row.Id > 0)
                    {
                        var existing = await _repository.GetStagingByIdAsync(row.Id, importedBy);
                        if (existing != null)
                        {
                            existing.TestCode = row.TestCode ?? string.Empty;
                            existing.Buyer = row.Buyer ?? string.Empty;
                            existing.Month = ExcelParseHelper.TryParseDouble(row.Month) ?? 0;
                            existing.WorkGroup = row.WorkGroup ?? string.Empty;
                            existing.Volume = ExcelParseHelper.TryParseDouble(row.Volume);
                            existing.Passed = false;
                            existing.FailureComments = string.Empty;
                            rowsToUpdate.Add(existing);
                            continue;
                        }
                    }

                    rowsToInsert.Add(new StagingMonthlyOutput
                    {
                        TestCode = row.TestCode ?? string.Empty,
                        Buyer = row.Buyer ?? string.Empty,
                        Month = ExcelParseHelper.TryParseDouble(row.Month) ?? 0,
                        WorkGroup = row.WorkGroup ?? string.Empty,
                        Volume = ExcelParseHelper.TryParseDouble(row.Volume),
                        FailureComments = string.Empty,
                        Passed = false,
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

                return new MonthlyOutputImportResultDto
                {
                    ImportedCount = processedCount,
                    PassedCount = 0,
                    FailedCount = 0,
                    Message = $"Import completed. {processedCount} rows processed in staging."
                };
            }

            var rows = request.Rows.Select(row => new StagingMonthlyOutput
            {
                TestCode = row.TestCode ?? string.Empty,
                Buyer = row.Buyer ?? string.Empty,
                Month = ExcelParseHelper.TryParseDouble(row.Month) ?? 0,
                WorkGroup = row.WorkGroup ?? string.Empty,
                Volume = ExcelParseHelper.TryParseDouble(row.Volume),
                FailureComments = string.Empty,
                Passed = false,
                Filename = request.FileName,
                ImportedBy = importedBy,
                ImportedDate = importedDate
            }).ToList();

            var importedCount = await _repository.ImportStagingAsync(rows);
            return new MonthlyOutputImportResultDto
            {
                ImportedCount = importedCount,
                PassedCount = 0,
                FailedCount = 0,
                Message = $"Import completed. {importedCount} rows added to staging."
            };
        }

        public async Task<MonthlyOutputValidateResultDto> ValidateStagingAsync(string importedBy)
        {
            await _repository.RemoveZeroAndNullVolumeRecordsAsync(importedBy);

            var records = await _repository.GetStagingRecordsForValidationAsync(importedBy);

            if (records.Count == 0)
            {
                return new MonthlyOutputValidateResultDto
                {
                    PassedCount = 0,
                    FailedCount = 0,
                    Message = "Validation completed. No records to validate."
                };
            }

            var context = await LoadValidationContextAsync();
            var result = ValidateRecords(records, context);

            await _repository.UpdateStagingRecordsAsync(records);
            return result;
        }

        public async Task<MonthlyOutputMakeLiveResultDto> MakeLiveAsync(string importedBy)
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
            return new MonthlyOutputMakeLiveResultDto
            {
                ProcessedCount = result.ProcessedCount,
                ImportedCount = result.ImportedCount,
                FailedCount = result.FailedCount,
                Message = $"Make live completed. {result.ImportedCount} records moved into MonthlyOutput."
            };
        }


        private async Task ValidateSingleRecordAsync(StagingMonthlyOutput entity)
        {
            var context = await LoadValidationContextAsync();
            var stagingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var failures = ValidateRecordSync(entity, context, stagingKeys);

            if (failures.Count == 0
                && !string.IsNullOrWhiteSpace(entity.TestCode)
                && !string.IsNullOrWhiteSpace(entity.Buyer)
                && !string.IsNullOrWhiteSpace(entity.WorkGroup))
            {
                if (await _repository.LiveRecordExistsAsync(entity.TestCode, entity.Buyer, entity.Month, entity.WorkGroup))
                    failures.Add($"A similar record already imported. WG = {entity.WorkGroup}, Buyer = {entity.Buyer}, TestCode = {entity.TestCode} and Month = {entity.Month}.");
            }

            entity.Passed = failures.Count == 0;
            entity.FailureComments = failures.Count == 0 ? string.Empty : string.Join(Environment.NewLine, failures);
        }

        private async Task<OutputValidationContext> LoadValidationContextAsync()
        {
            var calenderMonths = await _calenderMonthRepository.GetCalenderMonthsAsync();
            var allTestCapabilities = await _testCapabilityRepository.GetAllAsync();
            var allActiveTestRequirements = await _testRequirementRepository.GetAllActiveAsync();

            return new OutputValidationContext
            {
                ValidWorkGroups = new HashSet<string>(
                    await _workGroupRepository.GetAllWorkGroupNamesAsync(),
                    StringComparer.OrdinalIgnoreCase),
                ValidMonths = new HashSet<double>(calenderMonths.Select(c => (double)(c.MonthNumber ?? 0))),
                TestCapabilityKeys = new HashSet<string>(
                    allTestCapabilities.Select(tc => $"{tc.TestCode}|{tc.WorkGroup}"),
                    StringComparer.OrdinalIgnoreCase),
                ActiveBuyerKeys = new HashSet<string>(
                    allActiveTestRequirements.Select(tr => $"{tr.TestCode}|{tr.Buyer}"),
                    StringComparer.OrdinalIgnoreCase)
            };
        }

        private MonthlyOutputValidateResultDto ValidateRecords(
            List<StagingMonthlyOutput> records,
            OutputValidationContext context)
        {
            var stagingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var passedCount = 0;
            var failedCount = 0;

            foreach (var record in records)
            {
                var failures = ValidateRecordSync(record, context, stagingKeys);

                // Duplicate check against live is done during import for single records;
                // for bulk validation we rely on the staging-key de-dup above
                record.Passed = failures.Count == 0;
                record.FailureComments = failures.Count == 0 ? string.Empty : string.Join(Environment.NewLine, failures);

                if (record.Passed == true) passedCount++;
                else failedCount++;
            }

            return new MonthlyOutputValidateResultDto
            {
                PassedCount = passedCount,
                FailedCount = failedCount,
                Message = $"Validation completed. {passedCount} records passed and {failedCount} records failed."
            };
        }

        private static List<string> ValidateRecordSync(
            StagingMonthlyOutput record,
            OutputValidationContext context,
            HashSet<string> stagingKeys)
        {
            var failures = new List<string>();
            var workGroup = record.WorkGroup?.Trim();
            var testCode = record.TestCode?.Trim();
            var buyer = record.Buyer?.Trim();
            var month = record.Month;
            var volume = record.Volume;

            // Volume must be numeric and > 0
            if (volume == null || volume <= 0)
            {
                failures.Add($"The volume is not a number. \"{volume}\"");
                return failures;
            }

            // WorkGroup
            if (string.IsNullOrWhiteSpace(workGroup))
            {
                failures.Add("The work group name is blank.");
                return failures;
            }
            if (!context.ValidWorkGroups.Contains(workGroup))
            {
                failures.Add($"The work group name not an actual WG: {workGroup}");
                return failures;
            }

            // TestCode, WG and Buyer are all required together
            if (string.IsNullOrWhiteSpace(testCode) || string.IsNullOrWhiteSpace(buyer))
            {
                failures.Add("No Testcode, WG or Project (or buying test).");
                return failures;
            }

            // TestCode + WorkGroup must exist in tlkpTestCapability
            var capKey = $"{testCode}|{workGroup}";
            if (!context.TestCapabilityKeys.Contains(capKey))
            {
                failures.Add($"The WG not set up to do this test, or invalid test: {testCode}, {workGroup}");
                return failures;
            }

            // TestCode + Buyer must exist in tlkpTestReqmt (active)
            var reqKey = $"{testCode}|{buyer}";
            if (!context.ActiveBuyerKeys.Contains(reqKey))
            {
                failures.Add($"The test or Project (or buying test) is invalid, or this project not buying this test(anymore): {testCode}, {buyer}");
                return failures;
            }

            // Month
            if (month == 0)
            {
                failures.Add("The month No. is blank.");
                return failures;
            }
            if (!context.ValidMonths.Contains(month))
            {
                failures.Add($"The month No. is invalid: {month}");
                return failures;
            }

            // Duplicate check within this staging batch
            var key = $"{testCode}|{buyer}|{(int)month}|{workGroup}";
            if (!stagingKeys.Add(key))
            {
                failures.Add($"Similar record in sheet being imported, WG = {workGroup}, Buyer = {buyer}, TestCode = {testCode} and Month = {month}.");
            }

            return failures;
        }
    }

    internal class OutputValidationContext
    {
        public required HashSet<string> ValidWorkGroups { get; init; }
        public required HashSet<double> ValidMonths { get; init; }
        public required HashSet<string> TestCapabilityKeys { get; init; }
        public required HashSet<string> ActiveBuyerKeys { get; init; }
    }
}
