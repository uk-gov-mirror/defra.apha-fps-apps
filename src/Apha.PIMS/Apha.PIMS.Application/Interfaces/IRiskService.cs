using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IRiskService
    {
        Task<List<RiskDto>> GetAllRiskRatingsAsync();

        Task<PaginatedResult<RiskDto>> GetPagedRiskRatingsAsync(QueryParameters<string> query);

        Task<RiskDto?> GetRiskRatingByIdAsync(int riskId);

        Task<RiskDto> CreateRiskRatingAsync(RiskDto dto);

        Task<RiskDto> UpdateRiskRatingAsync(RiskDto dto);

        Task<bool> DeleteRiskRatingAsync(int riskId);

        Task<bool> RiskRatingExistsAsync(int riskId);
    }
}
