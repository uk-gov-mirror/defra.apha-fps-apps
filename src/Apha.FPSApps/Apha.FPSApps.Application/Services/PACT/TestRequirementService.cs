using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class TestRequirementService : ITestRequirementService
    {
        private readonly IPactApiClient _pactClient;

        public TestRequirementService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<TestRequirementDto>>> GetPagedTestReqmtAsync(QueryParameters<string> query, string testCode)
            => await _pactClient.PactTestRequirement.GetPagedTestReqmtAsync(query, testCode);
        public async Task<ApiResponseDto<List<TestRequirementDto>>> GetPagedTestReqmtbyProjectAsync(QueryParameters<string> query, string parentProject)
            => await _pactClient.PactTestRequirement.GetPagedTestReqmtbyProjectAsync(query, parentProject);

        public async Task<ApiResponseDto<List<TestRequirementDto>>> GetAllTestReqmtForExportAsync(string testCode, string? filter)
            => await _pactClient.PactTestRequirement.GetAllTestReqmtForExportAsync(testCode, filter);

        public async Task<ApiResponseDto<List<TestRequirementDto>>> GetAllActiveAsync()
            => await _pactClient.PactTestRequirement.GetAllActiveAsync();

        public async Task<ApiResponseDto<TestRequirementDto>> GetTestReqmtByIdAsync(string testCode, string buyer)
            => await _pactClient.PactTestRequirement.GetTestReqmtByIdAsync(testCode, buyer);

        public async Task<ApiResponseDto<TestRequirementDto>> CreateTestReqmtAsync(TestRequirementDto dto)
            => await _pactClient.PactTestRequirement.CreateTestReqmtAsync(dto);

        public async Task<ApiResponseDto<TestRequirementDto>> UpdateTestReqmtAsync(TestRequirementDto dto)
            => await _pactClient.PactTestRequirement.UpdateTestReqmtAsync(dto);

        public async Task<ApiResponseDto<bool>> DeleteTestReqmtAsync(string testCode, string buyer)
            => await _pactClient.PactTestRequirement.DeleteTestReqmtAsync(testCode, buyer);

        public async Task<ApiResponseDto<TestRequirementDto>> GetTestReqmtPricingAsync(string testCode, string? projectCode = null)
            => await _pactClient.PactTestRequirement.GetTestReqmtPricingAsync(testCode, projectCode);

        public async Task<ApiResponseDto<List<TestSupplierViewDto>>> GetPagedBySupplierTestCodeAsync(
            QueryParameters<string> query, string testCode, bool showRejected)
            => await _pactClient.PactTestRequirement.GetPagedBySupplierTestCodeAsync(query, testCode, showRejected);
    }
}
