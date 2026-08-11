using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IProfitCentreManagerLinkService
    {
        Task<List<ProfitCentreManagerLinkDto>> GetAllProfitCentreManagerLinksAsync();

        Task<PaginatedResult<ProfitCentreManagerLinkDto>> GetPagedByManagerAsync(QueryParameters<string> query, string manager);

        Task<List<ProfitCentreLookupDto>> GetProfitCentresAsync();

        Task<List<ProfitCentreManagerLinkDto>> GetByProfitCentreAsync(string profitCentre);

        Task<List<ProfitCentreManagerLinkDto>> GetByManagerAsync(string manager);

        Task<ProfitCentreManagerLinkDto?> GetProfitCentreManagerLinkByIdAsync(string profitCentre, string manager);

        Task<ProfitCentreManagerLinkDto> CreateProfitCentreManagerLinkAsync(ProfitCentreManagerLinkDto dto);

        Task<bool> DeleteProfitCentreManagerLinkAsync(string profitCentre, string manager);

        Task<bool> ProfitCentreManagerLinkExistsAsync(string profitCentre, string manager);
    }
}
