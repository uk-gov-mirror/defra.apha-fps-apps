using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactTestRequirementApiClient
    {
        Task<ApiResponseDto<List<TestRequirementDto>>> GetPagedTestReqmtAsync(QueryParameters<string> query, string testCode);
        Task<ApiResponseDto<List<TestRequirementDto>>> GetPagedTestReqmtbyProjectAsync(QueryParameters<string> query, string parentProject);
        Task<ApiResponseDto<List<TestRequirementDto>>> GetAllTestReqmtForExportAsync(string testCode, string? filter);
        Task<ApiResponseDto<TestRequirementDto>> GetTestReqmtByIdAsync(string testCode, string buyer);
        Task<ApiResponseDto<TestRequirementDto>> CreateTestReqmtAsync(TestRequirementDto dto);
        Task<ApiResponseDto<TestRequirementDto>> UpdateTestReqmtAsync(TestRequirementDto dto);
        Task<ApiResponseDto<bool>> DeleteTestReqmtAsync(string testCode, string buyer);
        Task<ApiResponseDto<TestRequirementDto>> GetTestReqmtPricingAsync(string testCode, string? projectCode = null);
        Task<ApiResponseDto<List<TestSupplierViewDto>>> GetPagedBySupplierTestCodeAsync(
            QueryParameters<string> query, string testCode, bool showRejected);
        Task<ApiResponseDto<List<TestReqBreakdownDto>>> GetPlannedTestsByWorkgroupAsync(QueryParameters<string> query);
    }
}
