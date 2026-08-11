using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsProfitCentreManagerLinkApiClient
    {
        Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetAllProfitCentreManagerLinksAsync();

        Task<ApiResponseDto<List<ProfitCentreLookupDto>>> GetProfitCentresAsync();

        Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetByProfitCentreAsync(string profitCentre);

        Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetByManagerAsync(string manager);

        Task<ApiResponseDto<PaginatedResult<ProfitCentreManagerLinkDto>>> GetPagedByManagerAsync(QueryParameters<string> query, string manager);

        Task<ApiResponseDto<ProfitCentreManagerLinkDto>> GetProfitCentreManagerLinkByIdAsync(string profitCentre, string manager);

        Task<ApiResponseDto<ProfitCentreManagerLinkDto>> CreateProfitCentreManagerLinkAsync(ProfitCentreManagerLinkDto dto);

        Task<ApiResponseDto<bool>> DeleteProfitCentreManagerLinkAsync(string profitCentre, string manager);
    }
}
