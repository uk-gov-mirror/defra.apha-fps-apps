using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface ITestRequirementService
    {
        Task<PaginatedResult<TestRequirementtDto>> GetPagedTestReqmtAsync(QueryParameters<string> query, string testCode);
        Task<PaginatedResult<TestRequirementtDto>> GetPagedTestReqmtByProjectAsync(QueryParameters<string> query, string parentProject);
        Task<IEnumerable<TestRequirementtDto>> GetAllTestReqmtForExportAsync(string testCode, string? filterJson);
        Task<IEnumerable<TestRequirementtDto>> GetAllActiveAsync();
        Task<TestRequirementtDto?> GetTestReqmtByIdAsync(string testCode, string buyer);
        Task<TestRequirementtDto?> GetTestReqmtPricingAsync(string testCode, string? projectCode = null);
        Task<TestRequirementtDto> AddTestReqmtAsync(TestRequirementtDto dto);
        Task<TestRequirementtDto> UpdateTestReqmtAsync(TestRequirementtDto dto);
        Task<bool> DeleteTestReqmtAsync(string testCode, string buyer);
        Task<PaginatedResult<TestSupplierViewDto>> GetPagedBySupplierTestCodeAsync(
            QueryParameters<string> query, string testCode, bool showRejected);

        // TestReqBreakdown
        Task<PaginatedResult<TestReqBreakdownDto>> GetPlannedTestsByWorkgroupAsync(QueryParameters<string> query);
        Task<PaginatedResult<TestActualBreakdownDto>> GetActualsTestsWithPlannedDataByWorkgroupAsync(QueryParameters<string> query);
    }
}
