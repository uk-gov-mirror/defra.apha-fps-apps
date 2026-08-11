using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;
using System.Text.Json;

namespace Apha.PACT.Application.Services
{
    public class TestCapabilityService : ITestCapabilityService
    {
        private readonly ITestCapabilityRepository _testCapabilityRepository;
        private readonly ITestRequirementRepository _testReqmtRepository;
        private readonly ITestorProductRepository _testorProductRepository;
        private readonly IMonthlyOutputRepository _monthlyOutputRepository;
        private readonly IMapper _mapper;

        public TestCapabilityService(
            ITestCapabilityRepository testCapabilityRepository,
            ITestRequirementRepository testReqmtRepository,
            ITestorProductRepository testorProductRepository,
            IMonthlyOutputRepository monthlyOutputRepository,
            IMapper mapper)
        {
            _testCapabilityRepository = testCapabilityRepository;
            _testReqmtRepository = testReqmtRepository;
            _testorProductRepository = testorProductRepository;
            _monthlyOutputRepository = monthlyOutputRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<TestCapabilityDto>> GetPagedByWorkGroupAsync(QueryParameters<string> query, string? workGroup)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _testCapabilityRepository.GetPagedByWorkGroupAsync(parameters, workGroup);
            return _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData);
        }

        public async Task<PaginatedResult<TestCapabilityDto>> GetPagedByTestCodeAsync(QueryParameters<string> query, string? testCode)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _testCapabilityRepository.GetPagedByTestCodeAsync(parameters, testCode);
            return _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData);
        }

        public async Task<PaginatedResult<TestCapabilityDto>> GetPagedTestCapabilityByPortfolioAsync(QueryParameters<string> query, string? portfolio)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _testCapabilityRepository.GetPagedTestCapabilityByPortfolioAsync(parameters, portfolio);
            var result = _mapper.Map<PaginatedResult<TestCapabilityDto>>(pagedData);

            if (result.Data != null && result.Data.Any())
            {
                var testCodes = result.Data.Select(d => d.TestCode).Distinct().ToList();
                var descriptions = await _testorProductRepository.GetDescriptionsByCodesAsync(testCodes);
                var unitPrices = await _testorProductRepository.GetUnitPricesByCodesAsync(testCodes);
                foreach (var dto in result.Data)
                {
                    if (descriptions != null && descriptions.TryGetValue(dto.TestCode, out var desc))
                        dto.ItemDescription = desc;

                    // Unit Cost is sourced from the TestorProduct master (testorproduct.unitpricevla),
                    // not from tlkptestcapability, so every portfolio row for the same Test Code shows
                    // the same master price and reflects any update made to it.
                    if (unitPrices != null && unitPrices.TryGetValue(dto.TestCode, out var unitPrice))
                        dto.UnitCost = unitPrice;
                }

                if (HasItemDescriptionFilterOrSort(query))
                    result.Data = ApplyItemDescriptionFilterAndSort(result.Data, query);
            }

            return result;
        }

        public async Task<TestCapabilityDto?> GetTestCapabilityByIdAsync(string testCode, string workGroup)
        {
            var entity = await _testCapabilityRepository.GetByIdAsync(testCode, workGroup);
            if (entity is null)
                return null;

            var dto = _mapper.Map<TestCapabilityDto>(entity);

            // Unit Cost is sourced from the TestorProduct master (testorproduct.unitpricevla),
            // not from tlkptestcapability, so the edit form always shows the master price.
            var unitPrices = await _testorProductRepository.GetUnitPricesByCodesAsync([dto.TestCode]);
            if (unitPrices != null && unitPrices.TryGetValue(dto.TestCode, out var unitPrice))
                dto.UnitCost = unitPrice;

            return dto;
        }

        public async Task<TestCapabilityDto> AddTestCapabilityAsync(TestCapabilityDto dto)
        {
            ValidateRequiredFields(dto);

            var existing = await _testCapabilityRepository.GetByIdAsync(dto.TestCode, dto.WorkGroup);
            if (existing is not null)
                throw new InvalidOperationException(
                    $"A Test Capability record with TestCode '{dto.TestCode}' and WorkGroup '{dto.WorkGroup}' already exists.");

            var entity = _mapper.Map<TestCapability>(dto);
            var created = await _testCapabilityRepository.AddAsync(entity);
            return _mapper.Map<TestCapabilityDto>(created);
        }

        public async Task<TestCapabilityDto> UpdateTestCapabilityAsync(TestCapabilityDto dto)
        {
            ValidateRequiredFields(dto);

            // WorkGroup is part of the composite key. Use the original WorkGroup (when supplied)
            // to locate the existing record; fall back to the current WorkGroup for backwards compatibility.
            var lookupWorkGroup = string.IsNullOrWhiteSpace(dto.OriginalWorkGroup)
                ? dto.WorkGroup
                : dto.OriginalWorkGroup;

            var existing = await _testCapabilityRepository.GetByIdAsync(dto.TestCode, lookupWorkGroup);
            if (existing is null)
                throw new KeyNotFoundException(
                    $"A Test Capability record with TestCode '{dto.TestCode}' and WorkGroup '{lookupWorkGroup}' was not found.");

            var hasReqmts = await _testReqmtRepository.ExistsByTestBuyerCodeAsync(dto.TestCode + lookupWorkGroup);
            if (hasReqmts)
                throw new InvalidOperationException("Cannot update, test requirements are dependant on this.");

            // Unit Cost is the master price held on testorproduct.unitpricevla, not on
            // tlkptestcapability. Persist any changed unit cost to the TestorProduct master so it
            // is reflected for every portfolio row that shares the same Test Code.
            if (dto.UnitCost.HasValue)
                await _testorProductRepository.UpdateUnitPriceByCodeAsync(dto.TestCode, dto.UnitCost.Value);

            var entity = _mapper.Map<TestCapability>(dto);
            var updated = await _testCapabilityRepository.UpdateAsync(entity, lookupWorkGroup);
            return _mapper.Map<TestCapabilityDto>(updated);
        }

        private static IEnumerable<TestCapabilityDto> ApplyItemDescriptionFilterAndSort(
            IEnumerable<TestCapabilityDto> data, QueryParameters<string> query)
        {
            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filters = JsonSerializer.Deserialize<Dictionary<string, string>>(query.Filter);
                if (filters != null
                    && filters.TryGetValue("ItemDescription", out var itemDescFilter)
                    && !string.IsNullOrWhiteSpace(itemDescFilter))
                {
                    data = data
                        .Where(d => d.ItemDescription != null
                            && d.ItemDescription.Contains(itemDescFilter, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }
            }

            if (string.Equals(query.SortBy, "ItemDescription", StringComparison.OrdinalIgnoreCase))
            {
                data = query.Descending
                    ? data.OrderByDescending(d => d.ItemDescription).ToList()
                    : data.OrderBy(d => d.ItemDescription).ToList();
            }

            return data;
        }

        private static bool HasItemDescriptionFilterOrSort(QueryParameters<string> query)
        {
            if (string.Equals(query.SortBy, "ItemDescription", StringComparison.OrdinalIgnoreCase))
                return true;

            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filters = JsonSerializer.Deserialize<Dictionary<string, string>>(query.Filter);
                if (filters != null
                    && filters.TryGetValue("ItemDescription", out var value)
                    && !string.IsNullOrWhiteSpace(value))
                    return true;
            }

            return false;
        }

        private static void ValidateRequiredFields(TestCapabilityDto dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.TestCode))
                errors.Add("Test Code is required.");

            if (string.IsNullOrWhiteSpace(dto.WorkGroup))
                errors.Add("Work Group is required.");

            if (string.IsNullOrWhiteSpace(dto.PlanPortfolio))
                errors.Add("Plan Portfolio is required.");

            if (errors.Count > 0)
                throw new ArgumentException(string.Join(" ", errors));
        }

        public async Task<bool> DeleteTestCapabilityAsync(string testCode, string workGroup)
        {
            var hasReqmts = await _testReqmtRepository.ExistsByTestBuyerCodeAsync(testCode + workGroup);
            if (hasReqmts)
                throw new InvalidOperationException("Cannot delete, It is referenced by test requirements.");

            var hasMonthlyOutputs = await _monthlyOutputRepository.ExistsByTestCodeAndWorkGroupAsync(testCode, workGroup);
            if (hasMonthlyOutputs)
                throw new InvalidOperationException("Cannot delete, It is referenced by monthly outputs.");

            return await _testCapabilityRepository.DeleteAsync(testCode, workGroup);
        }

        public async Task<PaginatedResult<WgTestCapabilitiesWithDescriptionDto>> GetPagedWgTestCapabilitiesWithDescriptionAsync(QueryParameters<string> query, string workGroup)
        {
            var errors = new List<BusinessValidationError>();
            if (string.IsNullOrWhiteSpace(workGroup))
                errors.Add(new BusinessValidationError("Work Group is required", "WORKGROUP_REQUIRED"));
            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _testCapabilityRepository.GetPagedWgTestCapabilitiesWithDescriptionAsync(parameters, workGroup);
            return _mapper.Map<PaginatedResult<WgTestCapabilitiesWithDescriptionDto>>(pagedData);
        }

        // ── Plan CrossTab ─────────────────────────────────────────────────────

        public async Task BuildTestPlanSummaryAsync()
        {
            await _testCapabilityRepository.BuildTestPlanSummaryAsync();
        }

        public async Task<TestPlanCostBreakdownDto> GetPagedTestPlanCrossTabAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _testCapabilityRepository.GetPagedTestPlanCrossTabAsync(parameters);
            return new TestPlanCostBreakdownDto
            {
                Columns = result.Columns,
                Rows = result.Rows,
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            };
        }
    }
}
