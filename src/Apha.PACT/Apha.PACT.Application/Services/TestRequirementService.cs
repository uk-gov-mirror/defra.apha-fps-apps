using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Services
{
    public class TestRequirementService : ITestRequirementService
    {
        private readonly ITestRequirementRepository _testReqmtRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;

        public TestRequirementService(
            ITestRequirementRepository testReqmtRepository,
            IProjectRepository projectRepository,
            IMapper mapper)
        {
            _testReqmtRepository = testReqmtRepository;
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<TestRequirementtDto>> GetPagedTestReqmtAsync(QueryParameters<string> query, string testCode)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _testReqmtRepository.GetPagedWithDetailsAsync(parameters, testCode);
            var dtos = _mapper.Map<List<TestRequirementtDto>>(pagedData.Data);
            var paginationDto = _mapper.Map<PaginationDto>(pagedData.PaginationData);
            return new PaginatedResult<TestRequirementtDto>(dtos, paginationDto);
        }

        public async Task<PaginatedResult<TestRequirementtDto>> GetPagedTestReqmtByProjectAsync(QueryParameters<string> query, string parentProject)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _testReqmtRepository.GetPagedByProjectAsync(parameters, parentProject);
            var dtos = _mapper.Map<List<TestRequirementtDto>>(pagedData.Data);
            var paginationDto = _mapper.Map<PaginationDto>(pagedData.PaginationData);
            return new PaginatedResult<TestRequirementtDto>(dtos, paginationDto);
        }

        public async Task<IEnumerable<TestRequirementtDto>> GetAllTestReqmtForExportAsync(string testCode, string? filterJson)
        {
            var items = await _testReqmtRepository.GetAllForExportAsync(testCode, filterJson);
            return _mapper.Map<IEnumerable<TestRequirementtDto>>(items);
        }

        public async Task<IEnumerable<TestRequirementtDto>> GetAllActiveAsync()
        {
            var items = await _testReqmtRepository.GetAllActiveAsync();
            return _mapper.Map<IEnumerable<TestRequirementtDto>>(items);
        }

        public async Task<TestRequirementtDto?> GetTestReqmtByIdAsync(string testCode, string buyer)
        {
            var detail = await _testReqmtRepository.GetDetailByIdAsync(testCode, buyer);
            return detail is null ? null : _mapper.Map<TestRequirementtDto>(detail);
        }

        public async Task<TestRequirementtDto?> GetTestReqmtPricingAsync(string testCode, string? projectCode = null)
        {
            var detail = await _testReqmtRepository.GetPricingAsync(testCode, projectCode);
            return detail is null ? null : _mapper.Map<TestRequirementtDto>(detail);
        }

        public async Task<PaginatedResult<TestSupplierViewDto>> GetPagedBySupplierTestCodeAsync(
            QueryParameters<string> query, string testCode, bool showRejected)
        {
            ArgumentNullException.ThrowIfNull(query);
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var result = await _testReqmtRepository.GetPagedBySupplierTestCodeAsync(parameters, testCode, showRejected);
            return _mapper.Map<PaginatedResult<TestSupplierViewDto>>(result);
        }

        public async Task<TestRequirementtDto> AddTestReqmtAsync(TestRequirementtDto dto)
        {
            // ITrig: both fields null
            if (string.IsNullOrWhiteSpace(dto.Buyer) && string.IsNullOrWhiteSpace(dto.TestCode))
                throw new InvalidOperationException("Must fill in Project Buyer or Test Buyer");

            // ITrig: project must exist when ProjectBuyerCode is provided
            if (!string.IsNullOrWhiteSpace(dto.ProjectBuyerCode))
            {
                var projectExists = await _projectRepository.ExistsAsync(dto.ProjectBuyerCode);
                if (!projectExists)
                    throw new InvalidOperationException("Not a valid project.");
            }

            // ITrig: TestCapability must exist when TestBuyerCode is provided
            if (!string.IsNullOrWhiteSpace(dto.TestBuyerCode))
            {
                var capabilityExists = await _testReqmtRepository.ExistsByTestBuyerCodeAsync(dto.TestBuyerCode);
                if (!capabilityExists)
                    throw new InvalidOperationException("This workgroup is not setup to do this test.");
            }

            // Duplicate check: no existing record may exist with same TestCode + Buyer
            var exists = await _testReqmtRepository.ExistsAsync(dto.TestCode, dto.Buyer);
            if (exists)
                throw new InvalidOperationException("A record with the same TestCode and Buyer already exists.");

            var entity = _mapper.Map<TestRequirement>(dto);
            var created = await _testReqmtRepository.AddAsync(entity);
            return _mapper.Map<TestRequirementtDto>(created);
        }

        public async Task<TestRequirementtDto> UpdateTestReqmtAsync(TestRequirementtDto dto)
        {
            // UTrig: both fields null
            if (string.IsNullOrWhiteSpace(dto.ProjectBuyerCode) && string.IsNullOrWhiteSpace(dto.TestBuyerCode))
                throw new InvalidOperationException("Cannot update, you must fill in project buyer or test buyer.");

            // UTrig: TestCapability must exist when TestBuyerCode is provided
            if (!string.IsNullOrWhiteSpace(dto.TestBuyerCode))
            {
                var capabilityExists = await _testReqmtRepository.ExistsByTestBuyerCodeAsync(dto.TestBuyerCode);
                if (!capabilityExists)
                    throw new InvalidOperationException("Cannot update, test buyers workgroup is not setup to do this test.");
            }

            // UTrig: no MonthlyOutput records may exist for this TestCode + Buyer
            var hasMonthlyOutput = await _testReqmtRepository.ExistsByTestCodeAndBuyerInMonthlyOutputAsync(dto.TestCode, dto.Buyer);
            if (hasMonthlyOutput)
                throw new InvalidOperationException("Cannot update, existing data in Monthly Output.");

            // UTrig: project must exist when ProjectBuyerCode is provided
            if (!string.IsNullOrWhiteSpace(dto.ProjectBuyerCode))
            {
                var projectExists = await _projectRepository.ExistsAsync(dto.ProjectBuyerCode);
                if (!projectExists)
                    throw new InvalidOperationException("Cannot update, project does not exist.");
            }

            var entity = _mapper.Map<TestRequirement>(dto);
            var updated = await _testReqmtRepository.UpdateAsync(entity);
            return _mapper.Map<TestRequirementtDto>(updated);
        }

        public async Task<bool> DeleteTestReqmtAsync(string testCode, string buyer)
        {
            // DTrig: no MonthlyOutput records may exist for this TestCode + Buyer
            var hasMonthlyOutput = await _testReqmtRepository.ExistsByTestCodeAndBuyerInMonthlyOutputAsync(testCode, buyer);
            if (hasMonthlyOutput)
                throw new InvalidOperationException("Cannot delete, existing data in MonthlyOutput.");

            return await _testReqmtRepository.DeleteAsync(testCode, buyer);
        }

        // ── TestReqBreakdown (fps.vtestreqbreakdown) ──────────────────────────────

        public async Task<PaginatedResult<TestReqBreakdownDto>> GetPlannedTestsByWorkgroupAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _testReqmtRepository.GetPlannedTestsByWorkgroupAsync(parameters);
            return _mapper.Map<PaginatedResult<TestReqBreakdownDto>>(pagedData);
        }

      

        public async Task<PaginatedResult<TestActualBreakdownDto>> GetActualsTestsWithPlannedDataByWorkgroupAsync(
            QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData  = await _testReqmtRepository.GetActualsTestsWithPlannedDataByWorkgroupAsync(parameters);
            return _mapper.Map<PaginatedResult<TestActualBreakdownDto>>(pagedData);
        }
    }
}
