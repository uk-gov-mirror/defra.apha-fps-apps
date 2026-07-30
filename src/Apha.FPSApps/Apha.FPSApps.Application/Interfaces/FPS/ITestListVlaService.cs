using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    /// <summary>
    /// Frontend service interface for TestOrProduct VLA list view operations.
    /// Thin delegate surface — all methods forward to IPactApiClient.PactTestList.
    /// Backend routes: GET /api/v1/testorproduct (PACT API; FpsYear filter applied by PACT DbContext).
    /// </summary>
    public interface ITestListVlaService
    {
        Task<ApiResponseDto<List<TestorProductDto>>> GetAllAsync(QueryParameters<string> query);

        Task<ApiResponseDto<List<TestorProductDto>>> GetAllByYearAsync();

        Task<ApiResponseDto<TestorProductDto>> GetByIdAsync(string itemCode);
    }
}
