using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class BosworthInterfaceService : IBosworthInterfaceService
    {
        private readonly IPactApiClient _pactApiClient;

        public BosworthInterfaceService(IPactApiClient pactApiClient)
        {
            _pactApiClient = pactApiClient;
        }

        public async Task<ApiResponseDto<List<TimePurchaseProjectDto>>> GetTimePurchaseProjectAsync(string project)
            => await _pactApiClient.PactBosworthInterface.GetTimePurchaseProjectAsync(project);

        public async Task<ApiResponseDto<List<TimeSaleProfitCentreDto>>> GetTimeSaleProfitCentreAsync(string profitCentre)
            => await _pactApiClient.PactBosworthInterface.GetTimeSaleProfitCentreAsync(profitCentre);

        public async Task<ApiResponseDto<List<TimeSaleWorkGroupDto>>> GetTimeSaleWorkGroupAsync(string workGroup)
            => await _pactApiClient.PactBosworthInterface.GetTimeSaleWorkGroupAsync(workGroup);

        public async Task<ApiResponseDto<List<TestSaleSellingWorkgroupDto>>> GetTestSaleSellingWorkgroupAsync(string workGroup)
            => await _pactApiClient.PactBosworthInterface.GetTestSaleSellingWorkgroupAsync(workGroup);

        public async Task<ApiResponseDto<List<TestSaleBuyingProjectDto>>> GetTestSaleBuyingProjectAsync(string parentProject)
            => await _pactApiClient.PactBosworthInterface.GetTestSaleBuyingProjectAsync(parentProject);
    }
}