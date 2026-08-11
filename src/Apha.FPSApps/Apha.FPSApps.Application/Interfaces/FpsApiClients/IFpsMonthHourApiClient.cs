using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsMonthHourApiClient
    {
        Task<ApiResponseDto<List<MonthHourDto>>> GetAllMonthHourAsync(QueryParameters<string> query);
        Task<ApiResponseDto<IEnumerable<MonthHourDto>>> GetMonthHoursByYearAsync(short year);
        Task<ApiResponseDto<IEnumerable<short>>> GetDistinctYearsAsync();
        Task<ApiResponseDto<List<YearEndMonthHourDto>>> GetYearEndMonthHoursAsync();
        Task<ApiResponseDto<MonthHourDto>> SaveMonthHourAsync(MonthHourDto dto);
    }
}
