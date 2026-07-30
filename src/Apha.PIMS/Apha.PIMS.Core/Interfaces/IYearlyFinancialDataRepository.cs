using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IYearlyFinancialDataRepository
    {
        Task<PagedData<YearlyFinancialData>> GetAllAsync(string project, PaginationParameters<string> paging);

        Task<YearlyFinancialData?> GetByKeyAsync(short year, string project);

        Task<bool> ExistsAsync(short year, string project);

        Task<YearlyFinancialData> CreateAsync(YearlyFinancialData entity);

        Task<YearlyFinancialData> UpdateAsync(YearlyFinancialData entity);

        Task<bool> DeleteAsync(short year, string project);

        Task<IReadOnlyList<PactProjectYearCosts>> GetPactCostsAsync(string project, short year);

        Task<string?> GetSettingValueByIdAsync(string id);
    }
}
