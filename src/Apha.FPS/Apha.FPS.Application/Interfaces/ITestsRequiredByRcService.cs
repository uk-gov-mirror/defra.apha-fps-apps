using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface ITestsRequiredByRcService
    {
        Task<List<TestsRequiredByRcDto>> GetTestsRequiredByRcAsync(string? profitCentre);
    }
}
