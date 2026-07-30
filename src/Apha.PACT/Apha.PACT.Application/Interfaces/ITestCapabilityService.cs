using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface ITestCapabilityService
    {
        Task<PaginatedResult<TestCapabilityDto>> GetPagedByWorkGroupAsync(QueryParameters<string> query, string? workGroup);
        Task<PaginatedResult<TestCapabilityDto>> GetPagedByTestCodeAsync(QueryParameters<string> query, string? testCode);
        Task<PaginatedResult<TestCapabilityDto>> GetPagedTestCapabilityByPortfolioAsync(QueryParameters<string> query, string? portfolio);
        Task<TestCapabilityDto?> GetTestCapabilityByIdAsync(string testCode, string workGroup);
        Task<TestCapabilityDto> AddTestCapabilityAsync(TestCapabilityDto dto);
        Task<TestCapabilityDto> UpdateTestCapabilityAsync(TestCapabilityDto dto);
        Task<bool> DeleteTestCapabilityAsync(string testCode, string workGroup);
        Task<PaginatedResult<WgTestCapabilitiesWithDescriptionDto>> GetPagedWgTestCapabilitiesWithDescriptionAsync(QueryParameters<string> query, string workGroup);

        // Plan CrossTab
        Task BuildTestPlanSummaryAsync();
        Task<TestPlanCostBreakdownDto> GetPagedTestPlanCrossTabAsync(QueryParameters<string> query);
    }
}
