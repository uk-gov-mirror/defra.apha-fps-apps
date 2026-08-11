using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class TestorProductService : ITestorProductService
    {
        private readonly IPactApiClient _apiClient;

        public TestorProductService(IPactApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponseDto<List<TestorProductDto>>> GetAllTestorProductsAsync()
           => await _apiClient.PactTestList.GetAllTestorProductsAsync();

        public async Task<ApiResponseDto<List<TestorProductDto>>> GetPagedTestOrProductsAsync(QueryParameters<string> query)
            => await _apiClient.PactTestList.GetPagedTestOrProductsAsync(query);

        public async Task<ApiResponseDto<List<TestFeePlanViewDto>>> GetTestSnapshotPagedAsync(QueryParameters<string> query)
            => await _apiClient.PactTestList.GetTestSnapshotPagedAsync(query);

        public async Task<ApiResponseDto<TestorProductDto>> GetTestOrProductByIdAsync(string itemCode)
            => await _apiClient.PactTestList.GetTestOrProductByIdAsync(itemCode);

        public async Task<ApiResponseDto<TestorProductDto>> CreateTestOrProductAsync(TestorProductDto dto)
            => await _apiClient.PactTestList.CreateTestOrProductAsync(dto);

        public async Task<ApiResponseDto<TestorProductDto>> UpdateTestOrProductAsync(string itemCode, TestorProductDto dto)
            => await _apiClient.PactTestList.UpdateTestOrProductAsync(itemCode, dto);

        public async Task<ApiResponseDto<bool>> DeleteTestOrProductAsync(string itemCode)
            => await _apiClient.PactTestList.DeleteTestOrProductAsync(itemCode);

        public async Task<ApiResponseDto<List<string>>> GetOwnersAsync()
            => await _apiClient.PactTestList.GetOwnersAsync();

        public async Task<ApiResponseDto<List<TestPriceCheckDto>>> GetTestPriceCheckPagedAsync(
            QueryParameters<string> query, string priceFilter, string? owner)
            => await _apiClient.PactTestList.GetTestPriceCheckPagedAsync(query, priceFilter, owner);

        public async Task<ApiResponseDto<TestPriceCheckDto>> GetTestPriceCheckByKeyAsync(string testCode, string jobCode)
            => await _apiClient.PactTestList.GetTestPriceCheckByKeyAsync(testCode, jobCode);

        public async Task<ApiResponseDto<bool>> UpdateTestPriceCheckByKeyAsync(string testCode, string jobCode, TestPriceCheckDto dto)
            => await _apiClient.PactTestList.UpdateTestPriceCheckByKeyAsync(testCode, jobCode, dto);
    }
}

