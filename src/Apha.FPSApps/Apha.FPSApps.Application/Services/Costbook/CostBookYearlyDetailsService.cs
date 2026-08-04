using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Interfaces.Costbook;

using Apha.FPSApps.Application.Pagination;


namespace Apha.FPSApps.Application.Services.Costbook;

public class CostBookYearlyDetailsService : ICostBookYearlyDetailsService
{
    private readonly ICostBookApiClient _client;

    public CostBookYearlyDetailsService(ICostBookApiClient client) => _client = client;

    public Task<ApiResponseDto<ProjectHeaderDto>> GetProjectHeaderAsync(string projectId)
        => _client.YearlyDetails.GetProjectHeaderAsync(projectId);

    public Task<ApiResponseDto<List<ProjectYearDto>>> GetProjectYearsAsync(string projectId)
        => _client.YearlyDetails.GetProjectYearsAsync(projectId);

    public Task<ApiResponseDto<ProjectYearDto>> AddProjectYearAsync(string projectId, int year, ProjectYearDto dto)
        => _client.YearlyDetails.AddProjectYearAsync(projectId, year, dto);

    public Task<ApiResponseDto<ProjectYearDto>> UpdateProjectYearAsync(string projectId, int year, ProjectYearDto dto)
        => _client.YearlyDetails.UpdateProjectYearAsync(projectId, year, dto);

    public Task<ApiResponseDto<bool>> DeleteProjectYearAsync(string projectId, int year)
        => _client.YearlyDetails.DeleteProjectYearAsync(projectId, year);

    public Task<ApiResponseDto<PaginatedResult<StaffRequirementDto>>> GetStaffRequirementsAsync(
        string projectId, int year, QueryParameters<string> query)
        => _client.YearlyDetails.GetStaffRequirementsAsync(projectId, year, query);

    public Task<ApiResponseDto<StaffRequirementDto>> AddStaffRequirementAsync(string projectId, int year, StaffRequirementDto dto)
        => _client.YearlyDetails.AddStaffRequirementAsync(projectId, year, dto);

    public Task<ApiResponseDto<StaffRequirementDto>> UpdateStaffRequirementAsync(string projectId, int year, int srIdentity, StaffRequirementDto dto)
        => _client.YearlyDetails.UpdateStaffRequirementAsync(projectId, year, srIdentity, dto);

    public Task<ApiResponseDto<bool>> DeleteStaffRequirementAsync(string projectId, int year, int srIdentity)
        => _client.YearlyDetails.DeleteStaffRequirementAsync(projectId, year, srIdentity);
    
    public Task<ApiResponseDto<PaginatedResult<TestRequirementDto>>> GetTestRequirementsAsync(
       string projectId, int year, QueryParameters<string> query)
       => _client.YearlyDetails.GetTestRequirementsAsync(projectId, year, query);

    public Task<ApiResponseDto<TestRequirementDto>> AddTestRequirementAsync(string projectId, int year, TestRequirementDto dto)
        => _client.YearlyDetails.AddTestRequirementAsync(projectId, year, dto);

    public Task<ApiResponseDto<TestRequirementDto>> UpdateTestRequirementAsync(string projectId, int year, string testCode, TestRequirementDto dto)
        => _client.YearlyDetails.UpdateTestRequirementAsync(projectId, year, testCode, dto);

    public Task<ApiResponseDto<bool>> DeleteTestRequirementAsync(string projectId, int year, string testCode)
        => _client.YearlyDetails.DeleteTestRequirementAsync(projectId, year, testCode);

    public Task<ApiResponseDto<PaginatedResult<AnimalRequirementDto>>> GetAnimalRequirementsAsync(
        string projectId, int year, QueryParameters<string> query)
        => _client.YearlyDetails.GetAnimalRequirementsAsync(projectId, year, query);

    public Task<ApiResponseDto<AnimalRequirementDto>> AddAnimalRequirementAsync(string projectId, int year, AnimalRequirementDto dto)
        => _client.YearlyDetails.AddAnimalRequirementAsync(projectId, year, dto);

    public Task<ApiResponseDto<AnimalRequirementDto>> UpdateAnimalRequirementAsync(string projectId, int year, int arIdentity, AnimalRequirementDto dto)
        => _client.YearlyDetails.UpdateAnimalRequirementAsync(projectId, year, arIdentity, dto);

    public Task<ApiResponseDto<bool>> DeleteAnimalRequirementAsync(string projectId, int year, int arIdentity)
        => _client.YearlyDetails.DeleteAnimalRequirementAsync(projectId, year, arIdentity);

    public Task<ApiResponseDto<PaginatedResult<AdditionalCostDto>>> GetAdditionalCostsAsync(
        string projectId, int year, QueryParameters<string> query)
        => _client.YearlyDetails.GetAdditionalCostsAsync(projectId, year, query);

    public Task<ApiResponseDto<AdditionalCostDto>> AddAdditionalCostAsync(string projectId, int year, AdditionalCostDto dto)
        => _client.YearlyDetails.AddAdditionalCostAsync(projectId, year, dto);

    public Task<ApiResponseDto<AdditionalCostDto>> UpdateAdditionalCostAsync(string projectId, int year, int acIdentity, AdditionalCostDto dto)
        => _client.YearlyDetails.UpdateAdditionalCostAsync(projectId, year, acIdentity, dto);

    public Task<ApiResponseDto<bool>> DeleteAdditionalCostAsync(string projectId, int year, int acIdentity)
        => _client.YearlyDetails.DeleteAdditionalCostAsync(projectId, year, acIdentity);

    public Task<ApiResponseDto<List<PayRateDto>>> GetPayRatesAsync(string projectId, int year,bool isDefra)
        => _client.YearlyDetails.GetPayRatesAsync(projectId, year, isDefra);

    public Task<ApiResponseDto<List<AnimalRateDto>>> GetAnimalRatesAsync(string projectId, int year, bool isDefra)
        => _client.YearlyDetails.GetAnimalRatesAsync(projectId, year, isDefra);

    public Task<ApiResponseDto<List<AccountCategoryDto>>> GetAccountCategoriesAsync()
        => _client.YearlyDetails.GetAccountCategoriesAsync();

    public Task<ApiResponseDto<List<TestCodeLookupDto>>> GetTestCodeLookupsAsync(string projectId, int year, bool isDefra)
        => _client.YearlyDetails.GetTestCodeLookupsAsync(projectId, year, isDefra);

    public Task<ApiResponseDto<List<AnimalLookupDto>>> GetAllAnimalsAsync()
        => _client.YearlyDetails.GetAllAnimalsAsync();

    public Task<ApiResponseDto<string>> GetAdditionalCostinflamationAsync(string projectId, int year)
        => _client.YearlyDetails.GetAdditionalCostinflamationAsync(projectId, year);
}
