using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IYearlyFinancialDataService
    {
        Task<PaginatedResult<YearlyFinancialDataDto>> GetAllAsync(QueryParameters<string> parameters);

        Task<YearlyFinancialDataDto?> GetByKeyAsync(short year, string project);

        Task<YearlyFinancialDataDto> CreateAsync(YearlyFinancialDataDto dto);

        Task<YearlyFinancialDataDto> UpdateAsync(YearlyFinancialDataDto dto);

        Task<bool> DeleteAsync(short year, string project);

        Task<IReadOnlyList<PactProjectYearCostsDto>> GetPactCostsAsync(string project, short year);

        Task<string?> GetSettingValueByIdAsync(string id);
    }
}
