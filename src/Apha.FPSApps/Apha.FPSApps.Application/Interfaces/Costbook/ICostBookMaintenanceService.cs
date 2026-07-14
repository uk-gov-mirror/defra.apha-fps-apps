using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apha.FPSApps.Application.Interfaces.Costbook
{    
    public interface ICostBookMaintenanceService
    {
       
        Task<ApiResponseDto<MaintenanceSettingsDto>> GetSettingsAsync();

        
        Task<ApiResponseDto<MaintenanceSettingsDto>> UpdateSettingsAsync(MaintenanceSettingsDto dto);

        Task<ApiResponseDto<List<AccountCategoryMaintenanceDto>>> GetAccountCategoriesAsync();

        Task<ApiResponseDto<List<AccountCategoryMaintenanceDto>>> GetPaginatedAccountCategoriesAsync(QueryParameters<string> query);

        Task<ApiResponseDto<AccountCategoryMaintenanceDto>> UpdateAccountCategoryAsync(string accShortName, AccountCategoryMaintenanceDto dto);
    }
}
