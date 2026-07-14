using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsContributionSummaryApiClient
    {
        Task<ApiResponseDto<List<ContributionSummaryRowDto>>> GetRowsAsync(string sellingPc);
        Task<ApiResponseDto<ContributionSummaryTotalsDto>> GetTotalsAsync(string sellingPc);
    }
}
