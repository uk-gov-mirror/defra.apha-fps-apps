using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class TestPlanCrossTabService : ITestPlanCrossTabService
    {
        private readonly IPactApiClient _pactClient;

        public TestPlanCrossTabService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<TestPlanCostBreakdownDto>> GetPagedTestPlanCrossTabAsync(QueryParameters<string> query)
            => await _pactClient.PactTestPlanCrossTab.GetPagedTestPlanCrossTabAsync(query);
    }
}