using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PACT
{
    public interface ITestActualBreakdownService
    {
        Task<ApiResponseDto<List<TestActualBreakdownDto>>> GetActualsTestsWithPlannedDataByWorkgroupAsync(QueryParameters<string> query);
    }
}
