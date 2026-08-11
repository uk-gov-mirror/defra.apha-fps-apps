using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PACT
{
    public interface ITestorProductService
    {
        Task<ApiResponseDto<List<TestorProductDto>>> GetAllTestorProductsAsync();
        Task<ApiResponseDto<List<TestorProductDto>>> GetPagedTestOrProductsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<List<TestFeePlanViewDto>>> GetTestSnapshotPagedAsync(QueryParameters<string> query);
        Task<ApiResponseDto<TestorProductDto>> GetTestOrProductByIdAsync(string itemCode);
        Task<ApiResponseDto<TestorProductDto>> CreateTestOrProductAsync(TestorProductDto dto);
        Task<ApiResponseDto<TestorProductDto>> UpdateTestOrProductAsync(string itemCode, TestorProductDto dto);
        Task<ApiResponseDto<bool>> DeleteTestOrProductAsync(string itemCode);
        Task<ApiResponseDto<List<string>>> GetOwnersAsync();
        Task<ApiResponseDto<List<TestPriceCheckDto>>> GetTestPriceCheckPagedAsync(QueryParameters<string> query, string priceFilter, string? owner);
        Task<ApiResponseDto<TestPriceCheckDto>> GetTestPriceCheckByKeyAsync(string testCode, string jobCode);
        Task<ApiResponseDto<bool>> UpdateTestPriceCheckByKeyAsync(string testCode, string jobCode, TestPriceCheckDto dto);
    }
}

