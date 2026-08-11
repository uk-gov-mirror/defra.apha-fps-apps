using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface ITestsRequiredByWgRepository
    {
        Task<List<TestsRequiredByWgView>> GetTestsRequiredByWgAsync(string? profitCentre);
    }
}
