using Apha.PACT.Application.Dtos;

namespace Apha.PACT.Application.Interfaces
{
    public interface IBosworthInterfaceService
    {
        Task<IEnumerable<TimePurchaseProjectDto>> GetTimePurchaseProjectAsync(string project);
        Task<IEnumerable<TimeSaleProfitCentreDto>> GetTimeSaleProfitCentreAsync(string profitCentre);
        Task<IEnumerable<TimeSaleWorkGroupDto>> GetTimeSaleWorkGroupAsync(string workGroup);
        Task<IEnumerable<TestSaleSellingWorkgroupDto>> GetTestSaleSellingWorkgroupAsync(string workGroup);
        Task<IEnumerable<TestSaleBuyingProjectDto>> GetTestSaleBuyingProjectAsync(string parentProject);
    }
}