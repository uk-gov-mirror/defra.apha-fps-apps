using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IMonthHourService
    {
        Task<PaginatedResult<MonthHourDto>> GetAllMonthHourAsync(QueryParameters<string> query);
        Task<IEnumerable<MonthHourDto>> GetMonthHoursByYearAsync(short year);
        Task<IEnumerable<short>> GetDistinctYearsAsync();
        Task<List<YearEndMonthHourDto>> GetYearEndMonthHoursAsync();
        Task<MonthHourDto> SaveMonthHourAsync(MonthHourDto dto);
    }
}
