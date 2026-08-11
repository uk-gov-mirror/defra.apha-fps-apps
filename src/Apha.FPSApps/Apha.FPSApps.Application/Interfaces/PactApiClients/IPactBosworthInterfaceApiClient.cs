using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;

namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactBosworthInterfaceApiClient
    {
        Task<ApiResponseDto<List<TimePurchaseProjectDto>>> GetTimePurchaseProjectAsync(string project);
        Task<ApiResponseDto<List<TimeSaleProfitCentreDto>>> GetTimeSaleProfitCentreAsync(string profitCentre);
        Task<ApiResponseDto<List<TimeSaleWorkGroupDto>>> GetTimeSaleWorkGroupAsync(string workGroup);
        Task<ApiResponseDto<List<TestSaleSellingWorkgroupDto>>> GetTestSaleSellingWorkgroupAsync(string workGroup);
        Task<ApiResponseDto<List<TestSaleBuyingProjectDto>>> GetTestSaleBuyingProjectAsync(string parentProject);
    }
}