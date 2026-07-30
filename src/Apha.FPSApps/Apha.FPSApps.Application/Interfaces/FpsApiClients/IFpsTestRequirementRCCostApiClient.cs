using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    /// <summary>
    /// Frontend API client interface for project-specific component charges (TestRequirementRCCost).
    /// Targets backend route: GET/POST/PUT/DELETE /api/v1/testrequirementrccost
    /// Composite PK: TestCode + Buyer + ProfitCentre + FpsYear.
    /// testCode + fpsYear are required business context from the parent TestListVla row.
    /// buyer is from the test requirement tab row; profitCentre is from the RC cost subform row.
    /// </summary>
    public interface IFpsTestRequirementRCCostApiClient
    {
        Task<ApiResponseDto<List<TestRequirementRCCostDto>>> GetByTestCodeAsync(string testCode, int fpsYear);

        Task<ApiResponseDto<TestRequirementRCCostDto>> GetByKeyAsync(string testCode, string buyer, string profitCentre, int fpsYear);

        Task<ApiResponseDto<TestRequirementRCCostDto>> CreateAsync(TestRequirementRCCostDto dto);

        Task<ApiResponseDto<TestRequirementRCCostDto>> UpdateAsync(string testCode, string buyer, string profitCentre, int fpsYear, TestRequirementRCCostDto dto);

        Task<ApiResponseDto<bool>> DeleteAsync(string testCode, string buyer, string profitCentre, int fpsYear);
    }
}
