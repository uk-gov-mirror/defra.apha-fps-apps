using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;

namespace Apha.Costbook.Application.Interfaces;

public interface IYearlyDetailsService
{
    Task<ProjectHeaderDto?> GetProjectHeaderAsync(string projectId);
    Task<IEnumerable<ProjectYearDto>> GetProjectYearsAsync(string projectId);
    Task<ProjectYearDto> AddProjectYearAsync(string projectId, int year, ProjectYearDto dto);
    Task<ProjectYearDto> UpdateProjectYearAsync(ProjectYearDto dto);
    Task<(bool Deleted, IReadOnlyList<string> Errors)> DeleteProjectYearAsync(string projectId, int year);

    // ?? Staff — now paginated ?????????????????????????????????????????????????
    Task<PaginatedResult<StaffRequirementDto>> GetStaffRequirementsAsync(string projectId, int year, QueryParameters<string> query);
    Task<StaffRequirementDto> AddStaffRequirementAsync(StaffRequirementDto dto);
    Task<StaffRequirementDto> UpdateStaffRequirementAsync(StaffRequirementDto dto);
    Task<bool> DeleteStaffRequirementAsync(int srIdentity);

    Task<PaginatedResult<TestRequirementDto>> GetTestRequirementsAsync(string projectId, int year, QueryParameters<string> query);
    Task<TestRequirementDto> AddTestRequirementAsync(TestRequirementDto dto);
    Task<TestRequirementDto> UpdateTestRequirementAsync(TestRequirementDto dto);
    Task<bool> DeleteTestRequirementAsync(string projectId, int year, string testCode);

    Task<PaginatedResult<AnimalRequirementDto>> GetAnimalRequirementsAsync(string projectId, int year, QueryParameters<string> query);
    Task<AnimalRequirementDto> AddAnimalRequirementAsync(AnimalRequirementDto dto);
    Task<AnimalRequirementDto> UpdateAnimalRequirementAsync(AnimalRequirementDto dto);
    Task<bool> DeleteAnimalRequirementAsync(int arIdentity);

    Task<PaginatedResult<AdditionalCostDto>> GetAdditionalCostsAsync(string projectId, int year, QueryParameters<string> query);
    Task<AdditionalCostDto> AddAdditionalCostAsync(AdditionalCostDto dto);
    Task<AdditionalCostDto> UpdateAdditionalCostAsync(AdditionalCostDto dto);
    Task<bool> DeleteAdditionalCostAsync(int acIdentity);

    Task<IEnumerable<PayRateDto>> GetPayRatesAsync(string projectId, int year, bool isDefra);
    Task<IEnumerable<AnimalRateDto>> GetAnimalRatesAsync(string projectId, int year, bool isDefra);
    Task<IEnumerable<AccountCategoryDto>> GetAccountCategoriesAsync();
    Task<IEnumerable<TestCodeLookupDto>> GetTestCodeLookupsAsync(string projectId, int year, bool isDefra);
    Task<IEnumerable<AnimalLookupDto>> GetAllAnimalsAsync();
    Task<string> GetAdditionalCostinflamationAsync(string projectId, int year);
}
