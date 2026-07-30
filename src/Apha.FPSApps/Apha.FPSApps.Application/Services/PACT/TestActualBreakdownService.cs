using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class TestActualBreakdownService : ITestActualBreakdownService
    {
        private readonly IPactApiClient _pactClient;

        public TestActualBreakdownService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<TestActualBreakdownDto>>> GetActualsTestsWithPlannedDataByWorkgroupAsync(QueryParameters<string> query)
            => await _pactClient.PactTestActualBreakdown.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);
    }
}