using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class MonthlyTimeService : IMonthlyTimeService
    {
        private readonly IPactApiClient _pactClient;

        public MonthlyTimeService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<MonthlyTimeDto>>> GetMonthlyTimeByTimeCodeAndProjectAsync(string timeCode, string workGroup, string parentProject)
            => await _pactClient.PactMonthlyTime.GetMonthlyTimeByTimeCodeAndProjectAsync(timeCode, workGroup, parentProject);

        public async Task<ApiResponseDto<List<MonthlyTimeDto>>> GetPagedMonthlyTimeAsync(QueryParameters<string> query, string? timeCode, string? workGroup, string? parentProject)
            => await _pactClient.PactMonthlyTime.GetPagedMonthlyTimeAsync(query, timeCode, workGroup, parentProject);

        public async Task<ApiResponseDto<MonthlyTimeDto>> GetMonthlyTimeByIdAsync(string pactStaffId, string timeCode, double month, string parentProject)
            => await _pactClient.PactMonthlyTime.GetMonthlyTimeByIdAsync(pactStaffId, timeCode, month, parentProject);

        public async Task<ApiResponseDto<MonthlyTimeDto>> CreateMonthlyTimeAsync(MonthlyTimeDto dto)
            => await _pactClient.PactMonthlyTime.CreateMonthlyTimeAsync(dto);

        public async Task<ApiResponseDto<MonthlyTimeDto>> UpdateMonthlyTimeAsync(MonthlyTimeDto dto)
            => await _pactClient.PactMonthlyTime.UpdateMonthlyTimeAsync(dto);

        public async Task<ApiResponseDto<bool>> DeleteMonthlyTimeAsync(string pactStaffId, string timeCode, double month, string parentProject)
            => await _pactClient.PactMonthlyTime.DeleteMonthlyTimeAsync(pactStaffId, timeCode, month, parentProject);
    }
}
