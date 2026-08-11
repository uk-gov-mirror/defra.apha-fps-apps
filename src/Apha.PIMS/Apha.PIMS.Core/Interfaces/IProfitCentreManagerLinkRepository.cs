using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IProfitCentreManagerLinkRepository
    {
        Task<List<ProfitCentreManagerLink>> GetAllProfitCentreManagerLinksAsync();

        Task<PagedData<ProfitCentreManagerLink>> GetPagedByManagerAsync(PaginationParameters<string> query, string manager);

        Task<List<ProfitCentreLookup>> GetProfitCentresAsync();

        Task<List<ProfitCentreManagerLink>> GetByProfitCentreAsync(string profitCentre);

        Task<List<ProfitCentreManagerLink>> GetByManagerAsync(string manager);

        Task<ProfitCentreManagerLink?> GetProfitCentreManagerLinkByIdAsync(string profitCentre, string manager);

        Task<ProfitCentreManagerLink> AddProfitCentreManagerLinkAsync(ProfitCentreManagerLink entity);

        Task<bool> DeleteProfitCentreManagerLinkAsync(string profitCentre, string manager);

        Task<bool> ProfitCentreManagerLinkExistsAsync(string profitCentre, string manager);
    }
}
