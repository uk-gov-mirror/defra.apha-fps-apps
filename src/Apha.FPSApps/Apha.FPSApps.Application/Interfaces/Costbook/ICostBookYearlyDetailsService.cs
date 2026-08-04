using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.Costbook;

public interface ICostBookYearlyDetailsService
{
    Task<ApiResponseDto<ProjectHeaderDto>> GetProjectHeaderAsync(string projectId);
    Task<ApiResponseDto<List<ProjectYearDto>>> GetProjectYearsAsync(string projectId);
    Task<ApiResponseDto<ProjectYearDto>> AddProjectYearAsync(string projectId, int year, ProjectYearDto dto);
    Task<ApiResponseDto<ProjectYearDto>> UpdateProjectYearAsync(string projectId, int year, ProjectYearDto dto);
    Task<ApiResponseDto<bool>> DeleteProjectYearAsync(string projectId, int year);

    // ── Staff — now paginated ─────────────────────────────────────────────────
    Task<ApiResponseDto<PaginatedResult<StaffRequirementDto>>> GetStaffRequirementsAsync(string projectId, int year, QueryParameters<string> query);
    Task<ApiResponseDto<StaffRequirementDto>> AddStaffRequirementAsync(string projectId, int year, StaffRequirementDto dto);
    Task<ApiResponseDto<StaffRequirementDto>> UpdateStaffRequirementAsync(string projectId, int year, int srIdentity, StaffRequirementDto dto);
    Task<ApiResponseDto<bool>> DeleteStaffRequirementAsync(string projectId, int year, int srIdentity);

    Task<ApiResponseDto<PaginatedResult<TestRequirementDto>>> GetTestRequirementsAsync(string projectId, int year, QueryParameters<string> query);
    Task<ApiResponseDto<TestRequirementDto>> AddTestRequirementAsync(string projectId, int year, TestRequirementDto dto);
    Task<ApiResponseDto<TestRequirementDto>> UpdateTestRequirementAsync(string projectId, int year, string testCode, TestRequirementDto dto);
    Task<ApiResponseDto<bool>> DeleteTestRequirementAsync(string projectId, int year, string testCode);

    Task<ApiResponseDto<PaginatedResult<AnimalRequirementDto>>> GetAnimalRequirementsAsync(string projectId, int year, QueryParameters<string> query);
    Task<ApiResponseDto<AnimalRequirementDto>> AddAnimalRequirementAsync(string projectId, int year, AnimalRequirementDto dto);
    Task<ApiResponseDto<AnimalRequirementDto>> UpdateAnimalRequirementAsync(string projectId, int year, int arIdentity, AnimalRequirementDto dto);
    Task<ApiResponseDto<bool>> DeleteAnimalRequirementAsync(string projectId, int year, int arIdentity);

    Task<ApiResponseDto<PaginatedResult<AdditionalCostDto>>> GetAdditionalCostsAsync(string projectId, int year, QueryParameters<string> query);
    Task<ApiResponseDto<AdditionalCostDto>> AddAdditionalCostAsync(string projectId, int year, AdditionalCostDto dto);
    Task<ApiResponseDto<AdditionalCostDto>> UpdateAdditionalCostAsync(string projectId, int year, int acIdentity, AdditionalCostDto dto);
    Task<ApiResponseDto<bool>> DeleteAdditionalCostAsync(string projectId, int year, int acIdentity);

    Task<ApiResponseDto<List<PayRateDto>>> GetPayRatesAsync(string projectId, int year, bool isDefra);
    Task<ApiResponseDto<List<AnimalRateDto>>> GetAnimalRatesAsync(string projectId, int year, bool isDefra);
    Task<ApiResponseDto<List<AccountCategoryDto>>> GetAccountCategoriesAsync();
    Task<ApiResponseDto<List<TestCodeLookupDto>>> GetTestCodeLookupsAsync(string projectId, int year, bool isDefra);
    Task<ApiResponseDto<List<AnimalLookupDto>>> GetAllAnimalsAsync();
    Task<ApiResponseDto<string>> GetAdditionalCostinflamationAsync(string projectId, int year);
}
