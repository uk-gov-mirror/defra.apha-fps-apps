using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;

namespace Apha.Costbook.Core.Interfaces
{
   
    public interface IAccountGroupRepository
    {       
        Task<List<AccountGroup>> GetAllAccountGroupAsync();

        Task<PagedData<AccountGroup>> GetPaginatedAsync(PaginationParameters<string> query);

        Task<AccountGroup?> GetByCsg7GroupAsync(string csg7Group);

        Task<bool> ExistsAsync(string csg7Group);

        Task<AccountGroup> AddAccountGroupAsync(AccountGroup accountGroup);

        Task<AccountGroup> UpdateAccountGroupAsync(AccountGroup accountGroup);

        Task<bool> DeleteAccountGroupAsync(string csg7Group);
    }
}
