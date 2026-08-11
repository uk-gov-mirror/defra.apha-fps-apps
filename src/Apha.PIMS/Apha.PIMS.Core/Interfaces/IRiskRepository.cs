using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IRiskRepository
    {
        Task<List<Risk>> GetAllRiskRatingsAsync();

        Task<PagedData<Risk>> GetPagedRiskRatingsAsync(PaginationParameters<string> query);

        Task<Risk?> GetRiskRatingByIdAsync(int riskId);

        Task<Risk> AddRiskRatingAsync(Risk entity);

        Task<Risk> UpdateRiskRatingAsync(Risk entity);

        Task<bool> DeleteRiskRatingAsync(int riskId);

        Task<bool> RiskRatingExistsAsync(int riskId);
    }
}
