using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface IProjectSubContractRepository
    {
        Task<PagedData<ProjectSubContract>> GetPagedProjectSubContractsAsync(PaginationParameters<string> query, string? project);
        Task<decimal> GetTotalAmountAsync(string? project);
        Task<PagedData<ProjectSubContract>> GetFpsProjectSubContractsAsync(PaginationParameters<string> query, string? project, bool filterByAnimalAcctCodes = false);
        Task<decimal> GetFpsProjectSubContractTotalAmountAsync(string? project, bool filterByAnimalAcctCodes = false);
        Task<ProjectSubContract?> GetByIdAsync(int subContCounter);
        Task<ProjectSubContract> CreateAsync(ProjectSubContract entity);
        Task<ProjectSubContract> UpdateAsync(ProjectSubContract entity);
        Task<bool> DeleteAsync(int subContCounter);
        Task<List<MonthlySubContractsSummary>> GetMonthlySubContractsSummaryAsync(PaginationParameters<string> parameters);
        Task<HashSet<string>> GetValidProjectsAsync();
        int GetCurrentFpsYear();
        Task<PagedData<SubContractRmsImportRow>> GetFailedSubContractRmsAsync(PaginationParameters<string> query, string importedBy);
        Task<ProjectSubcontractStaging?> GetFailedSubContractRmsByIdAsync(int id, string importedBy);        
        Task<bool> DeleteFailedSubContractRmsByIdAsync(int id, string importedBy);
        Task<int> DeleteFailedSubContractRmsByUserAsync(string importedBy);
        Task<SubContractRmsImportResult> ImportSubContractRmsAsync(List<ProjectSubContract> passedRows, List<ProjectSubcontractStaging> failedRows);
    }
}
