using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.CostBookApiClients;


public interface ICostBookMaintenanceApiClient
{
    
    Task<ApiResponseDto<MaintenanceSettingsDto>> GetSettingsAsync();

    Task<ApiResponseDto<MaintenanceSettingsDto>> UpdateSettingsAsync(MaintenanceSettingsDto dto);

    Task<ApiResponseDto<List<AccountCategoryMaintenanceDto>>> GetAccountCategoriesAsync();

    Task<ApiResponseDto<List<AccountCategoryMaintenanceDto>>> GetPaginatedAccountCategoriesAsync(QueryParameters<string> query);

    Task<ApiResponseDto<AccountCategoryMaintenanceDto>> UpdateAccountCategoryAsync(string accShortName, AccountCategoryMaintenanceDto dto);
}
