using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Pagination;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apha.FPSApps.Application.Services.Costbook
{    
    public class CostBookAccountGroupService : ICostBookAccountGroupService
    {       
        private readonly ICostBookApiClient _costBookClient;

        public CostBookAccountGroupService(ICostBookApiClient costBookClient)
        {
            _costBookClient = costBookClient;
        }
        public Task<ApiResponseDto<List<AccountGroupDto>>> GetAllAccountGroupsAsync()
        {
            return _costBookClient.CostbookAccountGroup.GetAllAccountGroupsAsync();
        }

        public Task<ApiResponseDto<List<AccountGroupDto>>> GetPaginatedAccountGroupsAsync(QueryParameters<string> query)
        {
            return _costBookClient.CostbookAccountGroup.GetPaginatedAccountGroupsAsync(query);
        }

        public Task<ApiResponseDto<AccountGroupDto>> GetAccountGroupAsync(string csg7Group)
        {
            return _costBookClient.CostbookAccountGroup.GetAccountGroupAsync(csg7Group);
        }

        public Task<ApiResponseDto<AccountGroupDto>> AddAccountGroupAsync(AccountGroupDto dto)
        {
            return _costBookClient.CostbookAccountGroup.AddAccountGroupAsync(dto);
        }

        public Task<ApiResponseDto<AccountGroupDto>> UpdateAccountGroupAsync(string csg7Group, AccountGroupDto dto)
        {
            return _costBookClient.CostbookAccountGroup.UpdateAccountGroupAsync(csg7Group, dto);
        }

        public Task<ApiResponseDto<bool>> DeleteAccountGroupAsync(string csg7Group)
        {
            return _costBookClient.CostbookAccountGroup.DeleteAccountGroupAsync(csg7Group);
        }
    }
}
