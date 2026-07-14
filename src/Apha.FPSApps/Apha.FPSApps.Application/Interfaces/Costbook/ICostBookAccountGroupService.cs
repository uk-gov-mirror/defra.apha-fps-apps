using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Apha.FPSApps.Application.Interfaces.Costbook
{
    public interface ICostBookAccountGroupService
    {
        Task<ApiResponseDto<List<AccountGroupDto>>> GetAllAccountGroupsAsync();

        Task<ApiResponseDto<List<AccountGroupDto>>> GetPaginatedAccountGroupsAsync(QueryParameters<string> query);

        Task<ApiResponseDto<AccountGroupDto>> GetAccountGroupAsync(string csg7Group);

        Task<ApiResponseDto<AccountGroupDto>> AddAccountGroupAsync(AccountGroupDto dto);

        Task<ApiResponseDto<AccountGroupDto>> UpdateAccountGroupAsync(string csg7Group, AccountGroupDto dto);

        Task<ApiResponseDto<bool>> DeleteAccountGroupAsync(string csg7Group);
    }
}
