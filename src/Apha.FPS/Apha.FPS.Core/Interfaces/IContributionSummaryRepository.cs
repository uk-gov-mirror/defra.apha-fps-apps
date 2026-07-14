using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IContributionSummaryRepository
    {
        /// <summary>
        /// Returns all time-seller rows for the given selling profit centre,
        /// scoped to the current FPS year via the global query filter.
        /// </summary>
        Task<List<ContributionSummaryView>> GetBySellingPcAsync(string sellingPc);
    }
}
