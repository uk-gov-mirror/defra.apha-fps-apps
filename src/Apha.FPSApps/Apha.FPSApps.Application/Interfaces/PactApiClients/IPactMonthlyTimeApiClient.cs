using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactMonthlyTimeApiClient
    {
        Task<ApiResponseDto<List<MonthlyTimeDto>>> GetMonthlyTimeByTimeCodeAndProjectAsync(string timeCode, string workGroup, string parentProject);
        Task<ApiResponseDto<List<MonthlyTimeDto>>> GetPagedMonthlyTimeAsync(QueryParameters<string> query, string? timeCode, string? workGroup, string? parentProject);
        Task<ApiResponseDto<MonthlyTimeDto>> GetMonthlyTimeByIdAsync(string pactStaffId, string timeCode, double month, string parentProject);
        Task<ApiResponseDto<MonthlyTimeDto>> CreateMonthlyTimeAsync(MonthlyTimeDto dto);
        Task<ApiResponseDto<MonthlyTimeDto>> UpdateMonthlyTimeAsync(MonthlyTimeDto dto);
        Task<ApiResponseDto<bool>> DeleteMonthlyTimeAsync(string pactStaffId, string timeCode, double month, string parentProject);
    }
}
