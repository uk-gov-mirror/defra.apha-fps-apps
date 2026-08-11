using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class TestsRequiredByWgService : ITestsRequiredByWgService
    {
        private readonly IFpsApiClient _fpsClient;

        public TestsRequiredByWgService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        public async Task<ApiResponseDto<List<TestsRequiredByWgDto>>> GetTestsRequiredByWgAsync(string? profitCentre)
        {
            return await _fpsClient.FpsTestsRequiredByWg.GetTestsRequiredByWgAsync(profitCentre);
        }
    }
}
