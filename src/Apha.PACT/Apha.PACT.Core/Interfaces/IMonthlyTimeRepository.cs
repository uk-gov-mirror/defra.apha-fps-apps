using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface IMonthlyTimeRepository
    {
        Task<bool> HasMonthlyTimeEntriesAsync(string workGroup, string timeCode, string parentProject);
        Task<IEnumerable<MonthlyTime>> GetMonthlyTimeByTimeCodeAndProjectAsync(string timeCode, string workGroup, string parentProject);
        Task<PagedData<MonthlyTime>> GetPagedMonthlyTimeAsync(PaginationParameters<string> parameters, string? timeCode, string? workGroup, string? parentProject);
        Task<MonthlyTime?> GetMonthlyTimeByIdAsync(string pactStaffId, string timeCode, double month, string parentProject);
        Task<MonthlyTime> CreateMonthlyTimeAsync(MonthlyTime entity);
        Task<MonthlyTime> UpdateMonthlyTimeAsync(MonthlyTime entity);
        Task<bool> DeleteMonthlyTimeAsync(string pactStaffId, string timeCode, double month, string parentProject);
    }
}
