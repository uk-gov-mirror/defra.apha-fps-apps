using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface IMonthlyOutputRepository
    {
        Task<PagedData<MonthlyOutputLog>> GetMonthlyOutputLogAsync(
            PaginationParameters<string> query,
            string? workGroup,
            string? testCode,
            string? buyer,
            DateTime? dateImported,
            double? month,
            string? userId,
            string? insertDelete);

        Task<bool> ExistsByTestCodeAndWorkGroupAsync(string testCode, string workGroup);
    }
}
