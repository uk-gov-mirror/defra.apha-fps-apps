using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Pagination;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apha.FPSApps.Application.Services.Costbook
{
    public class CostBookMaintenanceService : ICostBookMaintenanceService
    {
        private readonly ICostBookApiClient _costBookClient;

        public CostBookMaintenanceService(ICostBookApiClient costBookClient)
        {
            _costBookClient = costBookClient;
        }

        public Task<ApiResponseDto<MaintenanceSettingsDto>> GetSettingsAsync()
        {
            return _costBookClient.CostbookMaintenance.GetSettingsAsync();
        }

        public Task<ApiResponseDto<MaintenanceSettingsDto>> UpdateSettingsAsync(MaintenanceSettingsDto dto)
        {
            return _costBookClient.CostbookMaintenance.UpdateSettingsAsync(dto);
        }

        public Task<ApiResponseDto<List<AccountCategoryMaintenanceDto>>> GetAccountCategoriesAsync()
        {
            return _costBookClient.CostbookMaintenance.GetAccountCategoriesAsync();
        }

        public Task<ApiResponseDto<List<AccountCategoryMaintenanceDto>>> GetPaginatedAccountCategoriesAsync(QueryParameters<string> query)
        {
            return _costBookClient.CostbookMaintenance.GetPaginatedAccountCategoriesAsync(query);
        }

        public Task<ApiResponseDto<AccountCategoryMaintenanceDto>> UpdateAccountCategoryAsync(string accShortName, AccountCategoryMaintenanceDto dto)
        {
            return _costBookClient.CostbookMaintenance.UpdateAccountCategoryAsync(accShortName, dto);
        }
    }
}
