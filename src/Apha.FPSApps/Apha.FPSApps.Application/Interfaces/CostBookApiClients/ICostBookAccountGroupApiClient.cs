using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.CostBookApiClients;


public interface ICostBookAccountGroupApiClient
{
    Task<ApiResponseDto<List<AccountGroupDto>>> GetAllAccountGroupsAsync();

    Task<ApiResponseDto<List<AccountGroupDto>>> GetPaginatedAccountGroupsAsync(QueryParameters<string> query);

    Task<ApiResponseDto<AccountGroupDto>> GetAccountGroupAsync(string csg7Group);

    Task<ApiResponseDto<AccountGroupDto>> AddAccountGroupAsync(AccountGroupDto dto);

    Task<ApiResponseDto<AccountGroupDto>> UpdateAccountGroupAsync(string csg7Group, AccountGroupDto dto);

    Task<ApiResponseDto<bool>> DeleteAccountGroupAsync(string csg7Group);
}
