using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    /// <summary>
    /// Frontend API client interface for component charges per profit centre (TestRCCost).
    /// Targets backend route: GET/POST/PUT/DELETE /api/v1/testrccost
    /// Composite PK: TestCode + ProfitCentre + FpsYear.
    /// testCode + fpsYear are required business context from the parent TestListVla row.
    /// </summary>
    public interface IFpsTestRCCostApiClient
    {
        Task<ApiResponseDto<List<TestRCCostDto>>> GetByTestCodeAsync(string testCode, int fpsYear);

        Task<ApiResponseDto<TestRCCostDto>> GetByKeyAsync(string testCode, string profitCentre, int fpsYear);

        Task<ApiResponseDto<TestRCCostDto>> CreateAsync(TestRCCostDto dto);

        Task<ApiResponseDto<TestRCCostDto>> UpdateAsync(string testCode, string profitCentre, int fpsYear, TestRCCostDto dto);

        Task<ApiResponseDto<bool>> DeleteAsync(string testCode, string profitCentre, int fpsYear);
    }
}
