using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface ITestsRequiredByWgService
    {
        Task<ApiResponseDto<List<TestsRequiredByWgDto>>> GetTestsRequiredByWgAsync(string? profitCentre);
    }
}
