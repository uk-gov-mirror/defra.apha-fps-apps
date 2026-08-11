using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class TestsRequiredByRcService : ITestsRequiredByRcService
    {
        private readonly IFpsApiClient _fpsClient;

        public TestsRequiredByRcService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        public async Task<ApiResponseDto<List<TestsRequiredByRcDto>>> GetTestsRequiredByRcAsync(string? profitCentre)
        {
            return await _fpsClient.FpsTestsRequiredByRc.GetTestsRequiredByRcAsync(profitCentre);
        }
    }
}
