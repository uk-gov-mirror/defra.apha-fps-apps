using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;

namespace Apha.Costbook.Application.Interfaces
{
    public interface IAccountGroupService
    {       
        Task<List<AccountGroupDto>> GetAllAccountGroupAsync();

        Task<PaginatedResult<AccountGroupDto>> GetPaginatedAsync(QueryParameters<string> query);

        Task<AccountGroupDto?> GetByCsg7GroupAsync(string csg7Group);

        Task<AccountGroupDto> AddAccountGroupAsync(AccountGroupDto dto);

        Task<AccountGroupDto> UpdateAccountGroupAsync(string csg7Group, AccountGroupDto dto);

        Task DeleteAccountGroupAsync(string csg7Group);
    }
}
