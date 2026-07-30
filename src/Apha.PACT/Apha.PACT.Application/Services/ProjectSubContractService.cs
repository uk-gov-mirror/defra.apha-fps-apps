using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.Common.Utilities.ExcelImport;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class ProjectSubContractService : IProjectSubContractService
    {
        private readonly IProjectSubContractRepository _repository;
        private readonly IMapper _mapper;

        public ProjectSubContractService(IProjectSubContractRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<ProjectSubContractDto>> GetPagedProjectSubContractsAsync(QueryParameters<string> query, string? project)
        {
            PaginationParameters<string> parameters = _mapper.Map<PaginationParameters<string>>(query);
            PagedData<ProjectSubContract> pagedData = await _repository.GetPagedProjectSubContractsAsync(parameters, project);
            return _mapper.Map<PaginatedResult<ProjectSubContractDto>>(pagedData);
        }

        public async Task<decimal> GetTotalAmountAsync(string? project)
            => await _repository.GetTotalAmountAsync(project);

        public async Task<PaginatedResult<ProjectSubContractDto>> GetFpsProjectSubContractsAsync(QueryParameters<string> query, string? project, bool filterByAnimalAcctCodes = false)
        {
            PaginationParameters<string> parameters = _mapper.Map<PaginationParameters<string>>(query);
            PagedData<ProjectSubContract> pagedData = await _repository.GetFpsProjectSubContractsAsync(parameters, project, filterByAnimalAcctCodes);
            return _mapper.Map<PaginatedResult<ProjectSubContractDto>>(pagedData);
        }

        public async Task<decimal> GetFpsProjectSubContractTotalAmountAsync(string? project, bool filterByAnimalAcctCodes = false)
            => await _repository.GetFpsProjectSubContractTotalAmountAsync(project, filterByAnimalAcctCodes);

        public async Task<ProjectSubContractDto?> GetByIdAsync(int subContCounter)
        {
            ProjectSubContract? entity = await _repository.GetByIdAsync(subContCounter);
            return entity == null ? null : _mapper.Map<ProjectSubContractDto>(entity);
        }

        public async Task<ProjectSubContractDto> CreateAsync(ProjectSubContractDto dto)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required", "PROJECT_REQUIRED"));
            if (dto.Month is null)
                errors.Add(new BusinessValidationError("Month is required", "MONTH_REQUIRED"));
            if (dto.Amount is null)
                errors.Add(new BusinessValidationError("Amount is required", "AMOUNT_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            ProjectSubContract entity = _mapper.Map<ProjectSubContract>(dto);
            ProjectSubContract created = await _repository.CreateAsync(entity);
            return _mapper.Map<ProjectSubContractDto>(created);
        }

        public async Task<ProjectSubContractDto> UpdateAsync(ProjectSubContractDto dto)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(dto.Project))
                errors.Add(new BusinessValidationError("Project is required", "PROJECT_REQUIRED"));
            if (dto.Month is null)
                errors.Add(new BusinessValidationError("Month is required", "MONTH_REQUIRED"));
            if (dto.Amount is null)
                errors.Add(new BusinessValidationError("Amount is required", "AMOUNT_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            ProjectSubContract entity = _mapper.Map<ProjectSubContract>(dto);
            ProjectSubContract updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<ProjectSubContractDto>(updated);
        }

        public async Task<bool> DeleteAsync(int subContCounter)
        {
            return await _repository.DeleteAsync(subContCounter);
        }

        public async Task<MonthlySubContractsPivotDto> GetMonthlySubContractsSummaryAsync(QueryParameters<string> query)
        {
            // Push filter to the repository so the DB query is already filtered
            PaginationParameters<string> parameters = _mapper.Map<PaginationParameters<string>>(query);

          var data = await _repository.GetMonthlySubContractsSummaryAsync(parameters);

            // Discover all months present in filtered data (used to build columns)
            List<int> months = data
                .Select(x => (int)x.Month)
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            // Group flat rows into pivot rows (must be done in-memory: dict per row)
            IEnumerable<MonthlySubContractsSummaryDto> rows = data
                .GroupBy(x => new { x.Program, x.ParentProject })
                .Select(g => new MonthlySubContractsSummaryDto
                {
                    Program = g.Key.Program,
                    ParentProject = g.Key.ParentProject,
                    MonthlyAmounts = g.ToDictionary(x => (int)x.Month, x => x.MonthlyAmount ?? 0m)
                });

            // Sort grouped pivot rows (including dynamic month columns M1..M12)
            rows = SortPivotRows(rows, query.SortBy, query.Descending);

            // Paginate grouped rows in-memory
            var allRows = rows.ToList();
            int totalRecords = allRows.Count;
            int page = query.Page < 1 ? 1 : query.Page;
            int pageSize = query.PageSize < 1 ? 10 : query.PageSize;
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            List<MonthlySubContractsSummaryDto> pagedRows = allRows
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new MonthlySubContractsPivotDto
            {
                Months = months,
                Rows = pagedRows,
                Pagination = new PaginationDto
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    TotalRecords = totalRecords
                }
            };
        }

        public async Task<PaginatedResult<SubContractRmsImportRowDto>> GetFailedSubContractRmsAsync(QueryParameters<string> query, string importedBy)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetFailedSubContractRmsAsync(parameters, importedBy);
            return _mapper.Map<PaginatedResult<SubContractRmsImportRowDto>>(pagedData);
        }

        public async Task<int> DeleteFailedSubContractRmsByUserAsync(string importedBy)
        {
            return await _repository.DeleteFailedSubContractRmsByUserAsync(importedBy);
        }

        public async Task<SubContractRmsImportResultDto> ImportSubContractRmsAsync(SubContractRmsImportDto request, string importedBy)
        {
            var validProjects = await _repository.GetValidProjectsAsync();
            var fpsYear = _repository.GetCurrentFpsYear();
            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var fileName = request.FileName ?? "SubContractRMS-Import.xlsx";

            var passedRows = new List<ProjectSubContract>(request.Rows.Count);
            var failedRows = new List<ProjectSubcontractStaging>(request.Rows.Count);

            foreach (var source in request.Rows)
            {
                var failures = ValidateImportRow(source, validProjects);

                var parsedMonth = ExcelParseHelper.TryParseDouble(source.Month);
                var parsedAmount = ExcelParseHelper.TryParseDecimal(source.Amount);
                var parsedSupplierNumber = ExcelParseHelper.TryParseInt(source.SupplierNumber);
                var parsedDailyRate = ExcelParseHelper.TryParseDecimal(source.DailyRate);
                var parsedAnimalDays = ExcelParseHelper.TryParseInt(source.AnimalDays);

                if (failures.Count == 0)
                {
                    passedRows.Add(new ProjectSubContract
                    {
                        Project = source.Project,
                        TestJob = source.TestJob,
                        Month = parsedMonth,
                        Amount = parsedAmount,
                        WorkGroup = source.WorkGroup,
                        AcctCode = source.AcctCode,
                        Supplier = source.Supplier,
                        Description = source.Description,
                        SupplierNumber = parsedSupplierNumber,
                        DailyRate = parsedDailyRate,
                        AnimalDays = parsedAnimalDays,
                        FpsYear = fpsYear
                    });
                }
                else
                {
                    failedRows.Add(new ProjectSubcontractStaging
                    {
                        Project = source.Project,
                        TestJob = source.TestJob,
                        Month = source.Month,
                        Amount = source.Amount,
                        WorkGroup = source.WorkGroup,
                        AcctCode = source.AcctCode,
                        Supplier = source.Supplier,
                        Description = source.Description,
                        SupplierNumber = source.SupplierNumber,
                        DailyRate = source.DailyRate,
                        AnimalDays = source.AnimalDays,
                        Filename = fileName,
                        ImportedBy = importedBy,
                        ImportedDate = now,
                        IsPassed = false,
                        IsExported = false,
                        ValidationFailure = string.Join("\n", failures)
                    });
                }
            }

            var result = await _repository.ImportSubContractRmsAsync(passedRows, failedRows);
            var totalCount = result.PassedCount + result.FailedCount;

            return new SubContractRmsImportResultDto
            {
                PassedCount = result.PassedCount,
                FailedCount = result.FailedCount,                
                Message = $"Import completed successfully. {result.PassedCount} out of {totalCount} records successfully validated and is now live."
            };
        }

        private static List<string> ValidateImportRow(SubContractRmsImportRowDto row, HashSet<string> validProjects)
        {
            var failures = new List<string>();

            ExcelValidationHelper.ValidateStringInSet(row.Project, validProjects, "Project", failures);
            ExcelValidationHelper.ValidateRequiredDecimal(row.Amount, "Amount", failures);
            ExcelValidationHelper.ValidateMonth(row.Month, failures);
            ExcelValidationHelper.ValidateNonNegativeInteger(row.SupplierNumber, "Supplier Number", failures, required: false);
            ExcelValidationHelper.ValidateDecimal(row.DailyRate, "Daily Rate", failures, required: false);
            ExcelValidationHelper.ValidateNonNegativeInteger(row.AnimalDays, "Animal Days", failures, required: false);

            return failures;
        }

        public async Task<SubContractRmsImportRowDto?> GetFailedSubContractRmsByIdAsync(int id, string importedBy)
        {
            var entity = await _repository.GetFailedSubContractRmsByIdAsync(id, importedBy);
            return entity == null ? null : _mapper.Map<SubContractRmsImportRowDto>(entity);
        }

        public async Task<bool> SaveFailedSubContractRmsAsync(int id, SubContractRmsImportRowDto dto, string importedBy)
        {
            var validProjects = await _repository.GetValidProjectsAsync();
            var fpsYear = _repository.GetCurrentFpsYear();

            var failures = ValidateImportRow(dto, validProjects);

            if (failures.Count > 0)
            {
                // Map display field names to model property names for inline validation
                var fieldNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Project", "Project" },
                    { "Amount", "Amount" },
                    { "Month", "Month" },
                    { "Supplier Number", "SupplierNumber" },
                    { "Daily Rate", "DailyRate" },
                    { "Animal Days", "AnimalDays" }
                };

                // Convert validation failures to BusinessValidationError format
                var validationErrors = failures.Select(failure =>
                {
                    // Extract field name from error message (format: "FieldName message")
                    // Try to match multi-word field names first (longest match wins)
                    var displayFieldName = string.Empty;
                    var message = failure;

                    // Sort by length descending to match longest field names first
                    foreach (var fieldKey in fieldNameMap.Keys.OrderByDescending(k => k.Length))
                    {
                        if (failure.StartsWith(fieldKey + " ", StringComparison.OrdinalIgnoreCase))
                        {
                            displayFieldName = fieldKey;
                            message = failure.Substring(fieldKey.Length + 1).Trim();
                            break;
                        }
                    }

                    // Check if this is a "does not exist" error - show in summary only
                    if (message.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
                    {
                        // For summary-only errors, use the full message and empty code
                        return new BusinessValidationError(failure, string.Empty);
                    }

                    // Map display name to model property name for inline field validation
                    var modelFieldName = displayFieldName;
                    if (!string.IsNullOrEmpty(displayFieldName) && fieldNameMap.TryGetValue(displayFieldName, out var mappedFieldName))
                    {
                        modelFieldName = mappedFieldName;
                    }

                    // BusinessValidationError(message, code) - code is the field name.
                    var finalMessage = string.IsNullOrEmpty(displayFieldName) ? message : $"{displayFieldName} {message}";
                    return new BusinessValidationError(finalMessage, modelFieldName);
                }).ToList();

                throw new BusinessValidationErrorException(validationErrors);
            }

            var parsedMonth = ExcelParseHelper.TryParseDouble(dto.Month);
            var parsedAmount = ExcelParseHelper.TryParseDecimal(dto.Amount);
            var parsedSupplierNumber = ExcelParseHelper.TryParseInt(dto.SupplierNumber);
            var parsedDailyRate = ExcelParseHelper.TryParseDecimal(dto.DailyRate);
            var parsedAnimalDays = ExcelParseHelper.TryParseInt(dto.AnimalDays);

            // Record is valid - move to ProjectSubContract
            var subContract = new ProjectSubContract
            {
                Project = dto.Project,
                TestJob = dto.TestJob,
                Month = parsedMonth,
                Amount = parsedAmount,
                WorkGroup = dto.WorkGroup,
                AcctCode = dto.AcctCode,
                Supplier = dto.Supplier,
                Description = dto.Description,
                SupplierNumber = parsedSupplierNumber,
                DailyRate = parsedDailyRate,
                AnimalDays = parsedAnimalDays,
                FpsYear = fpsYear
            };

            await _repository.CreateAsync(subContract);
            await _repository.DeleteFailedSubContractRmsByIdAsync(id, importedBy);

            return true; // Successfully moved to SubContract
        }

        public async Task<bool> DeleteFailedSubContractRmsByIdAsync(int id, string importedBy)
        {
            return await _repository.DeleteFailedSubContractRmsByIdAsync(id, importedBy);
        }

        private static IEnumerable<MonthlySubContractsSummaryDto> SortPivotRows(
            IEnumerable<MonthlySubContractsSummaryDto> rows, string? sortBy, bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return rows.OrderBy(r => r.Program).ThenBy(r => r.ParentProject);

            // Dynamic month column: PropertyName is "M1" … "M12"
            // Parse the month number and sort by the corresponding amount value
            if (sortBy.StartsWith("M", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(sortBy[1..], out int month)
                && month is >= 1 and <= 12)
            {
                return descending
                    ? rows.OrderByDescending(r => r.MonthlyAmounts.GetValueOrDefault(month))
                          .ThenBy(r => r.Program)
                    : rows.OrderBy(r => r.MonthlyAmounts.GetValueOrDefault(month))
                          .ThenBy(r => r.Program);
            }

            return sortBy.ToLower() switch
            {
                "program" when descending => rows.OrderByDescending(r => r.Program).ThenByDescending(r => r.ParentProject),
                "program" => rows.OrderBy(r => r.Program).ThenBy(r => r.ParentProject),
                "parentproject" when descending => rows.OrderByDescending(r => r.ParentProject).ThenByDescending(r => r.Program),
                "parentproject" => rows.OrderBy(r => r.ParentProject).ThenBy(r => r.Program),
                _ => rows.OrderBy(r => r.Program).ThenBy(r => r.ParentProject)
            };
        }
    }
}
