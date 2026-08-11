using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // Mirrors RiskController — integer PK (riskid); full CRUD; route api/v1/risk-ratings
    public interface IPimsRiskApiClient
    {
        // GET /api/v1/risk-ratings — full list
        Task<ApiResponseDto<List<RiskDto>>> GetAllRiskRatingsAsync();

        // GET /api/v1/risk-ratings/paged — paged/sorted/filterable list
        Task<ApiResponseDto<PaginatedResult<RiskDto>>> GetPagedRiskRatingsAsync(QueryParameters<string> query);

        // GET /api/v1/risk-ratings/{riskid:int}
        Task<ApiResponseDto<RiskDto>> GetRiskRatingByIdAsync(int riskId);

        // POST /api/v1/risk-ratings
        Task<ApiResponseDto<RiskDto>> CreateRiskRatingAsync(RiskDto dto);

        // PUT /api/v1/risk-ratings/{riskid:int} — route PK is authoritative
        Task<ApiResponseDto<RiskDto>> UpdateRiskRatingAsync(int riskId, RiskDto dto);

        // DELETE /api/v1/risk-ratings/{riskid:int}
        Task<ApiResponseDto<bool>> DeleteRiskRatingAsync(int riskId);
    }
}
