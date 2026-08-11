using Apha.PACT.Core.Entities;

namespace Apha.PACT.Core.Interfaces
{
    public interface IBosworthInterfaceRepository
    {
        Task<IEnumerable<TimePurchaseProject>> GetTimePurchaseProjectAsync(string project);
        Task<IEnumerable<TimeSaleProfitCentre>> GetTimeSaleProfitCentreAsync(string profitCentre);
        Task<IEnumerable<TimeSaleWorkGroup>> GetTimeSaleWorkGroupAsync(string workGroup);
        Task<IEnumerable<TestSaleSellingWorkgroup>> GetTestSaleSellingWorkgroupAsync(string workGroup);
        Task<IEnumerable<TestSaleBuyingProject>> GetTestSaleBuyingProjectAsync(string parentProject);
    }
}