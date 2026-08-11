using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IMonthHourRepository
    {
        Task<PagedData<MonthHour>> GetAllAsync(PaginationParameters<string> query);
        Task<IEnumerable<MonthHour>> GetByYearAsync(short year);
        Task<IEnumerable<short>> GetDistinctYearsAsync();
        Task<List<YearEndMonthHour>> GetYearEndMonthHoursAsync();
        Task<MonthHour> SaveAsync(MonthHour monthHour);
    }
}
