using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsTestsRequiredByWgApiClient
    {
        Task<ApiResponseDto<List<TestsRequiredByWgDto>>> GetTestsRequiredByWgAsync(string? profitCentre);
    }
}
