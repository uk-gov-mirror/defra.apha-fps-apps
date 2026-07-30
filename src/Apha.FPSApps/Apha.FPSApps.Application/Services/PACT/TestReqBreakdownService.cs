using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class TestReqBreakdownService : ITestReqBreakdownService
    {
        private readonly IPactApiClient _pactClient;

        public TestReqBreakdownService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<TestReqBreakdownDto>>> GetPlannedTestsByWorkgroupAsync(QueryParameters<string> query)
            => await _pactClient.PactTestRequirement.GetPlannedTestsByWorkgroupAsync(query);
    }
}
