using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Microsoft.AspNetCore.Http;

namespace Apha.FPSApps.Application.Interfaces.PACT
{
    public interface IProjectSubContractService
    {
        Task<ApiResponseDto<List<ProjectSubContractDto>>> GetPagedProjectSubContractsAsync(QueryParameters<string> query, string? project);
        Task<ApiResponseDto<List<ProjectSubContractDto>>> GetPagedProjectSubContractsManualAsync(QueryParameters<string> query, string? project);
        Task<ApiResponseDto<decimal>> GetTotalAmountAsync(string? project);
        Task<ApiResponseDto<ProjectSubContractDto>> GetByIdAsync(int subContCounter);
        Task<ApiResponseDto<ProjectSubContractDto>> CreateAsync(ProjectSubContractDto dto);
        Task<ApiResponseDto<ProjectSubContractDto>> UpdateAsync(int subContCounter, ProjectSubContractDto dto);
        Task<ApiResponseDto<bool>> DeleteAsync(int subContCounter);
        Task<ApiResponseDto<List<ProjectSubContractDto>>> GetFpsProjectSubContractsAsync(QueryParameters<string> query, string? project, bool filterByAnimalAcctCodes = false);
        Task<ApiResponseDto<decimal>> GetFpsProjectSubContractTotalAmountAsync(string? project, bool filterByAnimalAcctCodes = false);
        Task<ApiResponseDto<MonthlySubContractsPivotDto>> GetMonthlySubContractsSummaryAsync(QueryParameters<string> query);
        Task<ApiResponseDto<List<SubContractRmsImportRowDto>>> GetFailedSubContractRmsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<SubContractRmsImportRowDto>> GetFailedSubContractRmsByIdAsync(int id);
        Task<ApiResponseDto<bool>> SaveFailedSubContractRmsAsync(int id, SubContractRmsImportRowDto dto);
        Task<ApiResponseDto<bool>> DeleteFailedSubContractRmsByIdAsync(int id);
        Task<ApiResponseDto<SubContractRmsImportResultDto>> ImportSubContractRmsAsync(IFormFile file);
        Task<ApiResponseDto<bool>> DeleteFailedSubContractRmsByUserAsync();
    }
}
