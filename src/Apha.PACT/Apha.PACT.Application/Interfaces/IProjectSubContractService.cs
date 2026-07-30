using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface IProjectSubContractService
    {
        Task<PaginatedResult<ProjectSubContractDto>> GetPagedProjectSubContractsAsync(QueryParameters<string> query, string? project);
        Task<decimal> GetTotalAmountAsync(string? project);
        Task<PaginatedResult<ProjectSubContractDto>> GetFpsProjectSubContractsAsync(QueryParameters<string> query, string? project, bool filterByAnimalAcctCodes = false);
        Task<decimal> GetFpsProjectSubContractTotalAmountAsync(string? project, bool filterByAnimalAcctCodes = false);
        Task<ProjectSubContractDto?> GetByIdAsync(int subContCounter);
        Task<ProjectSubContractDto> CreateAsync(ProjectSubContractDto dto);
        Task<ProjectSubContractDto> UpdateAsync(ProjectSubContractDto dto);
        Task<bool> DeleteAsync(int subContCounter);
        Task<MonthlySubContractsPivotDto> GetMonthlySubContractsSummaryAsync(QueryParameters<string> query);
        Task<PaginatedResult<SubContractRmsImportRowDto>> GetFailedSubContractRmsAsync(QueryParameters<string> query, string importedBy);
        Task<SubContractRmsImportRowDto?> GetFailedSubContractRmsByIdAsync(int id, string importedBy);
        Task<bool> SaveFailedSubContractRmsAsync(int id, SubContractRmsImportRowDto dto, string importedBy);
        Task<bool> DeleteFailedSubContractRmsByIdAsync(int id, string importedBy);
        Task<int> DeleteFailedSubContractRmsByUserAsync(string importedBy);
        Task<SubContractRmsImportResultDto> ImportSubContractRmsAsync(SubContractRmsImportDto request, string importedBy);
    }
}
