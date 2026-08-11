using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface ITestsRequiredByRcService
    {
        Task<ApiResponseDto<List<TestsRequiredByRcDto>>> GetTestsRequiredByRcAsync(string? profitCentre);
    }
}
