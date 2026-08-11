using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsTestsRequiredByRcApiClient
    {
        Task<ApiResponseDto<List<TestsRequiredByRcDto>>> GetTestsRequiredByRcAsync(string? profitCentre);
    }
}
