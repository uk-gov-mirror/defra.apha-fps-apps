using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;
using Npgsql;

namespace Apha.PACT.Application.Services
{
    public class WorkGroupService : IWorkGroupService
    {
        private readonly IWorkGroupRepository _repository;
        private readonly IMapper _mapper;

        public WorkGroupService(IWorkGroupRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WorkGroupDto>> GetAllWorkGroupsAsync()
        {
            var items = await _repository.GetAllWorkGroupsAsync();
            return _mapper.Map<IEnumerable<WorkGroupDto>>(items);
        }

        public async Task<List<string>> GetAllWorkGroupNamesAsync()
            => await _repository.GetAllWorkGroupNamesAsync();


        public async Task<List<WorkGroupViewDto>> GetWorkGroupsByProfitCentreForBudgetAsync(string profitCentre)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            var views = await _repository.GetWorkGroupsByProfitCentreForBudgetAsync(profitCentre);
            return _mapper.Map<List<WorkGroupViewDto>>(views);
        }

        public async Task<PaginatedResult<WorkGroupViewDto>> GetWorkGroupsByProfitCentreForBudgetPagedAsync(
            QueryParameters<string> query, string profitCentre)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupsByProfitCentreForBudgetPagedAsync(parameters, profitCentre);
            return _mapper.Map<PaginatedResult<WorkGroupViewDto>>(pagedData);
        }

        public async Task<PaginatedResult<WorkGroupTimeCodeDto>> GetWorkGroupTimeCodeAsync(QueryParameters<string> query, string workGroup, int monthNumber)
        {
            ValidateWorkGroup(workGroup);
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupTimeCodeAsync(parameters, workGroup, monthNumber);
            return _mapper.Map<PaginatedResult<WorkGroupTimeCodeDto>>(pagedData);
        }

        public async Task<PaginatedResult<WorkGroupValidTimeCodeDto>> GetWorkGroupValidTimeCodeAsync(
            QueryParameters<string> query, string workGroup)
        {
            ValidateWorkGroup(workGroup);
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupValidTimeCodeAsync(parameters, workGroup);
            return _mapper.Map<PaginatedResult<WorkGroupValidTimeCodeDto>>(pagedData);
        }

        public async Task<WgSummarisedStaffTimeUsageDto> GetWgSummarisedStaffTimeUsageAsync(
            QueryParameters<string> query, string staffName)
        {
            ValidateStaffName(staffName);

            var rawEntries = await _repository.GetWgSummarisedStaffTimeUsageAsync(staffName);
            var entries = _mapper.Map<IEnumerable<WgSummarisedStaffTimeUsageEntryDto>>(rawEntries);

            // Derive HrsPaid: sum across all distinct people in the work group
            var hrsPaid = entries
                .GroupBy(e => e.Name)
                .Select(g => g.First())
                .Sum(e => e.HrsPaid ?? 0);

            var standardHoursPerMonth = hrsPaid > 0 ? hrsPaid / 12.0 : 0;

            // Build ALL rows first — summary must reflect the full dataset, not just the current page
            var allRows = BuildRows(entries);
            var summary = BuildSummary(allRows, standardHoursPerMonth);

            // Apply sort before paging so the requested order is respected
            allRows = ApplySortToWgStaffTimeRows(allRows, query.SortBy, query.Descending);

            // Paginate rows after summary is computed
            var page = Math.Max(1, query.Page);
            var pageSize = Math.Max(1, query.PageSize);
            var totalRecords = allRows.Count;
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            var pagedRows = allRows
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new WgSummarisedStaffTimeUsageDto
            {
                Rows = pagedRows,
                Summary = summary,
                HrsPaid = hrsPaid,
                JobTitleLookup = entries
                    .Where(r => !string.IsNullOrWhiteSpace(r.JobCode))
                    .DistinctBy(r => r.JobCode)
                    .Select(r => new JobTitleLookupItem
                    {
                        JobCode = r.JobCode!,
                        JobTitle = string.IsNullOrWhiteSpace(r.JobTitle) ? string.Empty : r.JobTitle
                    })
                    .ToList(),
                Pagination = new PaginationDto
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages
                }
            };
        }

        public async Task<SummarisedWgTimeViewDto> GetSummarisedWorkgroupTimeSummaryAsync(
           QueryParameters<string> query,
           string workGroup)
        {
            ValidateWorkGroup(workGroup);

            var rawEntries = await _repository.GetSummarisedWorkgroupTimeAsync(workGroup);
            var entries = _mapper.Map<IEnumerable<SummarisedWgTimeEntryDto>>(rawEntries).ToList();

            var allRows = BuildWgSummarisedTimeRows(entries);
            var summary = BuildWgSummarisedTimeSummary(allRows);

            // Apply sort before paging so the requested order is respected
            allRows = ApplySortToWgSummarisedTimeRows(allRows, query.SortBy, query.Descending);

            var page = Math.Max(1, query.Page);
            var pageSize = Math.Max(1, query.PageSize);
            var totalRecords = allRows.Count;
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            var pagedRows = allRows
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new SummarisedWgTimeViewDto
            {
                Rows = pagedRows,
                Summary = summary,
                Pagination = new PaginationDto
                {
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages
                },
                ProjectTitleLookup = entries
                    .Where(r => !string.IsNullOrWhiteSpace(r.ParentProject))
                    .DistinctBy(r => r.ParentProject)
                    .Select(r => new ProjectTitleLookupItem
                    {
                        ParentProject = r.ParentProject!,
                        ProjectTitle = string.IsNullOrWhiteSpace(r.ProjectTitle) ? "" : r.ProjectTitle
                    })
                    .ToList()
            };
        }

        public async Task<PaginatedResult<WorkGroupDto>> GetWorkGroupsByProfitCentreAsync(
            QueryParameters<string> query, string profitCentre)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupsByProfitCentreAsync(parameters, profitCentre);
            return _mapper.Map<PaginatedResult<WorkGroupDto>>(pagedData);
        }

        public async Task<bool> SetSendEmailForProfitCentreWorkGroupsAsync(string profitCentre, short flag)
        {
            return await _repository.SetSendEmailForProfitCentreWorkGroupsAsync(profitCentre, flag);
        }

        public async Task<bool> SetSendEmailForAllWorkGroupsAsync(short flag)
        {
            return await _repository.SetSendEmailForAllWorkGroupsAsync(flag);
        }

        public async Task<bool> UpdateWorkGroupEmailAsync(string workGroupName, short sendEmail, string? emailRecipient)
        {
            return await _repository.UpdateWorkGroupEmailAsync(workGroupName, sendEmail, emailRecipient);
        }

        private static List<WgSummarisedStaffTimeUsageRowDto> BuildRows(
            IEnumerable<WgSummarisedStaffTimeUsageEntryDto> staffTimeUsageEntries)
        {
            return staffTimeUsageEntries
                .GroupBy(e => new { e.ParentProject, e.JobCode })
                .Select(g =>
                {
                    double HoursForMonth(string monthName) =>
                        g.Where(e => e.MonthName!.Equals(monthName, StringComparison.CurrentCultureIgnoreCase)).Sum(e => e.TotalTime ?? 0);

                    return new WgSummarisedStaffTimeUsageRowDto
                    {
                        ParentProject = g.Key.ParentProject,
                        JobCode = g.Key.JobCode,
                        JobTitle = string.IsNullOrWhiteSpace(g.First().JobTitle) ? string.Empty : g.First().JobTitle,
                        April = HoursForMonth("April"),
                        May = HoursForMonth("May"),
                        June = HoursForMonth("June"),
                        July = HoursForMonth("July"),
                        August = HoursForMonth("August"),
                        September = HoursForMonth("September"),
                        October = HoursForMonth("October"),
                        November = HoursForMonth("November"),
                        December = HoursForMonth("December"),
                        January = HoursForMonth("January"),
                        February = HoursForMonth("February"),
                        March = HoursForMonth("March"),
                        TotalTime = g.Sum(e => e.TotalTime ?? 0),
                        TotalCost = g.Sum(e => e.TotalCost ?? 0)
                    };
                })
                .OrderBy(r => r.ParentProject)
                .ThenBy(r => r.JobCode)
                .ToList();
        }

        /// <summary>
        /// Builds the three-row footer that appeared at the botton.
        /// </summary>
        private static WgSummarisedStaffTimeUsageSummaryDto BuildSummary(
            IReadOnlyList<WgSummarisedStaffTimeUsageRowDto> rows, double standardHoursPerMonth)
        {
            var totalHoursApril = rows.Sum(r => r.April);
            var totalHoursMay = rows.Sum(r => r.May);
            var totalHoursJune = rows.Sum(r => r.June);
            var totalHoursJuly = rows.Sum(r => r.July);
            var totalHoursAugust = rows.Sum(r => r.August);
            var totalHoursSeptember = rows.Sum(r => r.September);
            var totalHoursOctober = rows.Sum(r => r.October);
            var totalHoursNovember = rows.Sum(r => r.November);
            var totalHoursDecember = rows.Sum(r => r.December);
            var totalHoursJanuary = rows.Sum(r => r.January);
            var totalHoursFebruary = rows.Sum(r => r.February);
            var totalHoursMarch = rows.Sum(r => r.March);
            var grandTotalTime = rows.Sum(r => r.TotalTime);

            // Returns the standard hours allowance for a month
            double StandardHoursFor(double totalHoursInMonth)
            {
                return totalHoursInMonth == 0 ? 0 : standardHoursPerMonth;
            }

            // Percentage of recorded hours against the standard hours allowance for a single month, rounded to one decimal place;
            double PercentAllocated(double totalHoursInMonth, double standardHours)
            {
                return standardHours == 0 ? 0 : Math.Round(totalHoursInMonth / standardHours * 100, 1);
            }

            return new WgSummarisedStaffTimeUsageSummaryDto
            {
                TotalApril = totalHoursApril,
                TotalMay = totalHoursMay,
                TotalJune = totalHoursJune,
                TotalJuly = totalHoursJuly,
                TotalAugust = totalHoursAugust,
                TotalSeptember = totalHoursSeptember,
                TotalOctober = totalHoursOctober,
                TotalNovember = totalHoursNovember,
                TotalDecember = totalHoursDecember,
                TotalJanuary = totalHoursJanuary,
                TotalFebruary = totalHoursFebruary,
                TotalMarch = totalHoursMarch,
                GrandTotalTime = grandTotalTime,
                GrandTotalCost = rows.Sum(r => r.TotalCost),
                StandardHoursPerMonth = standardHoursPerMonth,

                // Sum of the standard hours allowance for each month that had recorded activity;
                TotalStandardHours =
                    StandardHoursFor(totalHoursApril)     + StandardHoursFor(totalHoursMay)      +
                    StandardHoursFor(totalHoursJune)      + StandardHoursFor(totalHoursJuly)     +
                    StandardHoursFor(totalHoursAugust)    + StandardHoursFor(totalHoursSeptember)+
                    StandardHoursFor(totalHoursOctober)   + StandardHoursFor(totalHoursNovember) +
                    StandardHoursFor(totalHoursDecember)  + StandardHoursFor(totalHoursJanuary)  +
                    StandardHoursFor(totalHoursFebruary)  + StandardHoursFor(totalHoursMarch),

                // Percentage of total recorded hours against the sum of standard hours for all months that had activity;
                GrandTotalPercentAllocated = (
                    StandardHoursFor(totalHoursApril)      + StandardHoursFor(totalHoursMay)       +
                    StandardHoursFor(totalHoursJune)       + StandardHoursFor(totalHoursJuly)      +
                    StandardHoursFor(totalHoursAugust)     + StandardHoursFor(totalHoursSeptember) +
                    StandardHoursFor(totalHoursOctober)    + StandardHoursFor(totalHoursNovember)  +
                    StandardHoursFor(totalHoursDecember)   + StandardHoursFor(totalHoursJanuary)   +
                    StandardHoursFor(totalHoursFebruary)   + StandardHoursFor(totalHoursMarch)) > 0
                        ? Math.Round(grandTotalTime /
                            (StandardHoursFor(totalHoursApril)      + StandardHoursFor(totalHoursMay)       +
                             StandardHoursFor(totalHoursJune)       + StandardHoursFor(totalHoursJuly)      +
                             StandardHoursFor(totalHoursAugust)     + StandardHoursFor(totalHoursSeptember) +
                             StandardHoursFor(totalHoursOctober)    + StandardHoursFor(totalHoursNovember)  +
                             StandardHoursFor(totalHoursDecember)   + StandardHoursFor(totalHoursJanuary)   +
                             StandardHoursFor(totalHoursFebruary)   + StandardHoursFor(totalHoursMarch)) * 100, 1)
                        : 0,

                PercentAllocatedApril = PercentAllocated(totalHoursApril, StandardHoursFor(totalHoursApril)),
                PercentAllocatedMay = PercentAllocated(totalHoursMay, StandardHoursFor(totalHoursMay)),
                PercentAllocatedJune = PercentAllocated(totalHoursJune, StandardHoursFor(totalHoursJune)),
                PercentAllocatedJuly = PercentAllocated(totalHoursJuly, StandardHoursFor(totalHoursJuly)),
                PercentAllocatedAugust = PercentAllocated(totalHoursAugust, StandardHoursFor(totalHoursAugust)),
                PercentAllocatedSeptember = PercentAllocated(totalHoursSeptember, StandardHoursFor(totalHoursSeptember)),
                PercentAllocatedOctober = PercentAllocated(totalHoursOctober, StandardHoursFor(totalHoursOctober)),
                PercentAllocatedNovember = PercentAllocated(totalHoursNovember, StandardHoursFor(totalHoursNovember)),
                PercentAllocatedDecember = PercentAllocated(totalHoursDecember, StandardHoursFor(totalHoursDecember)),
                PercentAllocatedJanuary = PercentAllocated(totalHoursJanuary, StandardHoursFor(totalHoursJanuary)),
                PercentAllocatedFebruary = PercentAllocated(totalHoursFebruary, StandardHoursFor(totalHoursFebruary)),
                PercentAllocatedMarch = PercentAllocated(totalHoursMarch, StandardHoursFor(totalHoursMarch))
            };
        }

        private static void ValidateStaffName(string staffName)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(staffName))
                errors.Add(new BusinessValidationError("Staff Name is required", "STAFFNane_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);
        }

        private static void ValidateWorkGroup(string workGroup)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(workGroup))
                errors.Add(new BusinessValidationError("WorkGroup is required", "WORKGROUP_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);
        }

        private static List<SummarisedWgTimeRowDto> ApplySortToWgSummarisedTimeRows(
            List<SummarisedWgTimeRowDto> rows,
            string? sortBy,
            bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return rows;

            Func<SummarisedWgTimeRowDto, object> keySelector = sortBy switch
            {
                nameof(SummarisedWgTimeRowDto.ParentProject) => r => (object)(r.ParentProject ?? string.Empty),
                nameof(SummarisedWgTimeRowDto.April) => r => r.April,
                nameof(SummarisedWgTimeRowDto.May) => r => r.May,
                nameof(SummarisedWgTimeRowDto.June) => r => r.June,
                nameof(SummarisedWgTimeRowDto.July) => r => r.July,
                nameof(SummarisedWgTimeRowDto.August) => r => r.August,
                nameof(SummarisedWgTimeRowDto.September) => r => r.September,
                nameof(SummarisedWgTimeRowDto.October) => r => r.October,
                nameof(SummarisedWgTimeRowDto.November) => r => r.November,
                nameof(SummarisedWgTimeRowDto.December) => r => r.December,
                nameof(SummarisedWgTimeRowDto.January) => r => r.January,
                nameof(SummarisedWgTimeRowDto.February) => r => r.February,
                nameof(SummarisedWgTimeRowDto.March) => r => r.March,
                nameof(SummarisedWgTimeRowDto.TotalTime) => r => r.TotalTime,
                nameof(SummarisedWgTimeRowDto.TotalCost) => r => r.TotalCost,
                _ => r => (object)(r.ParentProject ?? string.Empty)
            };

            return descending
                ? rows.OrderByDescending(keySelector).ToList()
                : rows.OrderBy(keySelector).ToList();
        }

        private static List<WgSummarisedStaffTimeUsageRowDto> ApplySortToWgStaffTimeRows(
            List<WgSummarisedStaffTimeUsageRowDto> rows,
            string? sortBy,
            bool descending)
        {
            if (string.IsNullOrWhiteSpace(sortBy))
                return rows;

            Func<WgSummarisedStaffTimeUsageRowDto, object> keySelector = sortBy switch
            {
                nameof(WgSummarisedStaffTimeUsageRowDto.ParentProject) => r => (object)(r.ParentProject ?? string.Empty),
                nameof(WgSummarisedStaffTimeUsageRowDto.JobCode) => r => (object)(r.JobCode ?? string.Empty),
                nameof(WgSummarisedStaffTimeUsageRowDto.April) => r => r.April,
                nameof(WgSummarisedStaffTimeUsageRowDto.May) => r => r.May,
                nameof(WgSummarisedStaffTimeUsageRowDto.June) => r => r.June,
                nameof(WgSummarisedStaffTimeUsageRowDto.July) => r => r.July,
                nameof(WgSummarisedStaffTimeUsageRowDto.August) => r => r.August,
                nameof(WgSummarisedStaffTimeUsageRowDto.September) => r => r.September,
                nameof(WgSummarisedStaffTimeUsageRowDto.October) => r => r.October,
                nameof(WgSummarisedStaffTimeUsageRowDto.November) => r => r.November,
                nameof(WgSummarisedStaffTimeUsageRowDto.December) => r => r.December,
                nameof(WgSummarisedStaffTimeUsageRowDto.January) => r => r.January,
                nameof(WgSummarisedStaffTimeUsageRowDto.February) => r => r.February,
                nameof(WgSummarisedStaffTimeUsageRowDto.March) => r => r.March,
                nameof(WgSummarisedStaffTimeUsageRowDto.TotalTime) => r => r.TotalTime,
                nameof(WgSummarisedStaffTimeUsageRowDto.TotalCost) => r => r.TotalCost,
                _ => r => (object)(r.ParentProject ?? string.Empty)
            };

            return descending
                ? rows.OrderByDescending(keySelector).ToList()
                : rows.OrderBy(keySelector).ToList();
        }

        private static List<SummarisedWgTimeRowDto> BuildWgSummarisedTimeRows(
           IEnumerable<SummarisedWgTimeEntryDto> entries)
        {
            return entries
                .GroupBy(e => e.ParentProject)
                .Select(g =>
                {
                    double HoursForMonth(string monthName) =>
                        g.Where(e => e.MonthName!.Equals(monthName, StringComparison.CurrentCultureIgnoreCase))
                         .Sum(e => e.TotalTime.GetValueOrDefault());

                    return new SummarisedWgTimeRowDto
                    {
                        ParentProject = g.Key,
                        April = HoursForMonth("April"),
                        May = HoursForMonth("May"),
                        June = HoursForMonth("June"),
                        July = HoursForMonth("July"),
                        August = HoursForMonth("August"),
                        September = HoursForMonth("September"),
                        October = HoursForMonth("October"),
                        November = HoursForMonth("November"),
                        December = HoursForMonth("December"),
                        January = HoursForMonth("January"),
                        February = HoursForMonth("February"),
                        March = HoursForMonth("March"),
                        TotalTime = g.Sum(e => e.TotalTime.GetValueOrDefault()),
                        TotalCost = g.Sum(e => e.TotalCost.GetValueOrDefault())
                    };
                })
                .OrderBy(r => r.ParentProject)
                .ToList();
        }

        private static SummarisedWgTimeSummaryDto BuildWgSummarisedTimeSummary(
            IReadOnlyList<SummarisedWgTimeRowDto> rows)
        {
            return new SummarisedWgTimeSummaryDto
            {
                TotalApril = rows.Sum(r => r.April),
                TotalMay = rows.Sum(r => r.May),
                TotalJune = rows.Sum(r => r.June),
                TotalJuly = rows.Sum(r => r.July),
                TotalAugust = rows.Sum(r => r.August),
                TotalSeptember = rows.Sum(r => r.September),
                TotalOctober = rows.Sum(r => r.October),
                TotalNovember = rows.Sum(r => r.November),
                TotalDecember = rows.Sum(r => r.December),
                TotalJanuary = rows.Sum(r => r.January),
                TotalFebruary = rows.Sum(r => r.February),
                TotalMarch = rows.Sum(r => r.March),
                GrandTotalTime = rows.Sum(r => r.TotalTime),
                GrandTotalCost = rows.Sum(r => r.TotalCost)
            };
        }

        // ─── WorkGroup Maintenance CRUD + lookups (migrated from FPS) ───────────────

        public async Task<PaginatedResult<WorkGroupDto>> GetPagedAsync(QueryParameters<string> query)
        {
            if (query is null)
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("Query parameters cannot be null.", "WORKGROUP_INVALID_QUERY")
                ]);
            }

            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _repository.GetPagedAsync(filter);
            return _mapper.Map<PaginatedResult<WorkGroupDto>>(result);
        }

        public async Task<WorkGroupDto?> GetByKeyAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("WorkGroupName cannot be null or empty.", "WORKGROUP_INVALID_KEY")
                ]);
            }

            var entity = await _repository.GetByKeyAsync(workGroupName);
            return entity is null ? null : _mapper.Map<WorkGroupDto>(entity);
        }

        public async Task<WorkGroupDto> CreateAsync(WorkGroupDto dto)
        {
            if (dto is null)
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("Workgroup data cannot be null.", "WORKGROUP_INVALID_DATA")
                ]);
            }

            if (string.IsNullOrWhiteSpace(dto.WorkGroupName))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("WorkGroupName is required.", "WORKGROUP_NAME_REQUIRED")
                ]);
            }

            if (string.IsNullOrWhiteSpace(dto.ProfitCentre))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("ProfitCentre is required.", "WORKGROUP_PROFITCENTRE_REQUIRED")
                ]);
            }

            var exists = await _repository.ExistsAsync(dto.WorkGroupName);
            if (exists)
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        $"A workgroup with the name '{dto.WorkGroupName}' already exists for the active FPS year.",
                        "WORKGROUP_DUPLICATE_NAME")
                ]);
            }

            var entity = _mapper.Map<WorkGroup>(dto);
            try
            {
                var created = await _repository.CreateAsync(entity);
                return _mapper.Map<WorkGroupDto>(created);
            }
            catch (Exception ex) when (IsForeignKeyViolation(ex))
            {
                throw BuildCostCentreValidationException();
            }
        }

        public async Task<WorkGroupDto> UpdateAsync(string originalWorkGroupName, WorkGroupDto dto)
        {
            if (string.IsNullOrWhiteSpace(originalWorkGroupName))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("Original WorkGroupName cannot be null or empty.", "WORKGROUP_INVALID_KEY")
                ]);
            }

            if (dto is null)
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("Workgroup data cannot be null.", "WORKGROUP_INVALID_DATA")
                ]);
            }

            if (string.IsNullOrWhiteSpace(dto.WorkGroupName))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("WorkGroupName is required.", "WORKGROUP_NAME_REQUIRED")
                ]);
            }

            if (string.IsNullOrWhiteSpace(dto.ProfitCentre))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("ProfitCentre is required.", "WORKGROUP_PROFITCENTRE_REQUIRED")
                ]);
            }

            var exists = await _repository.ExistsAsync(originalWorkGroupName);
            if (!exists)
            {
                throw new KeyNotFoundException(
                    $"Workgroup '{originalWorkGroupName}' not found for the active FPS year.");
            }

            var entity = _mapper.Map<WorkGroup>(dto);
            try
            {
                var updated = await _repository.UpdateAsync(originalWorkGroupName, entity);
                return _mapper.Map<WorkGroupDto>(updated);
            }
            catch (Exception ex) when (IsForeignKeyViolation(ex))
            {
                throw BuildCostCentreValidationException();
            }
        }

        public async Task<bool> DeleteAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("WorkGroupName cannot be null or empty.", "WORKGROUP_INVALID_KEY")
                ]);
            }

            try
            {
                return await _repository.DeleteAsync(workGroupName);
            }
            catch (Exception ex) when (IsForeignKeyViolation(ex))
            {
                throw new BusinessValidationErrorException(new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        "There are associated records in the system so this record cannot be deleted.",
                        "WORKGROUPGRADE_FK_VIOLATION")
                });
            }
        }

        public async Task<IEnumerable<string>> GetAllProfitCentresAsync()
            => await _repository.GetAllProfitCentresAsync();

        public async Task<IEnumerable<OwnerDto>> GetOwnersAsync()
        {
            var owners = await _repository.GetOwnersAsync();
            return _mapper.Map<IEnumerable<OwnerDto>>(owners);
        }

        public async Task<IEnumerable<double?>> GetCostCentresByProfitCentreAsync(string profitCentre)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
            {
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError("ProfitCentre cannot be null or empty.", "WORKGROUP_PROFITCENTRE_REQUIRED")
                ]);
            }

            return await _repository.GetCostCentresByProfitCentreAsync(profitCentre);
        }

        private static BusinessValidationErrorException BuildCostCentreValidationException()
        {
            return new BusinessValidationErrorException(new List<BusinessValidationError>
            {
                new BusinessValidationError(
                    "The Cost center is not present in the Cost Center table. Please input Cost center which is already present in CostCenter table.",
                    "COSTCENTRE_FK_VIOLATION")
            });
        }

        private static bool IsForeignKeyViolation(Exception? ex)
        {
            for (var current = ex; current is not null; current = current.InnerException)
            {
                if (current is PostgresException pgEx
                    && pgEx.SqlState == PostgresErrorCodes.ForeignKeyViolation)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
