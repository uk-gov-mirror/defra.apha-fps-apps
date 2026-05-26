using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface IMonthlyTimeService
    {
        Task<IEnumerable<MonthlyTimeDto>> GetMonthlyTimeByTimeCodeAndProjectAsync(string timeCode, string workGroup, string parentProject);
        Task<PaginatedResult<MonthlyTimeDto>> GetPagedMonthlyTimeAsync(QueryParameters<string> query, string? timeCode, string? workGroup, string? parentProject);
        Task<MonthlyTimeDto?> GetMonthlyTimeByIdAsync(string pactStaffId, string timeCode, double month, string parentProject);
        Task<MonthlyTimeDto> CreateMonthlyTimeAsync(MonthlyTimeDto dto);
        Task<MonthlyTimeDto> UpdateMonthlyTimeAsync(MonthlyTimeDto dto);
        Task<bool> DeleteMonthlyTimeAsync(string pactStaffId, string timeCode, double month, string parentProject);
    }
}
