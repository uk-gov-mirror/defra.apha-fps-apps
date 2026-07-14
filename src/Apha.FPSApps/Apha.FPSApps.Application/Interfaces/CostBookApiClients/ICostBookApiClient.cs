namespace Apha.FPSApps.Application.Interfaces.CostBookApiClients;


public interface ICostBookApiClient
{
    ICostBookProjectApiClient Projects { get; }
    ICostBookCustomerApiClient Customers { get; }
    ICostBookDiseaseApiClient Diseases { get; }
    ICostBookProgramApiClient Programs { get; }
    ICostBookStaffApiClient Staff { get; }
    ICostBookContractApiClient Contracts { get; }
    ICostBookYearlyDetailsApiClient YearlyDetails { get; }
    ICostBookProjectSummaryApiClient ProjectSummary { get; }
    ICostBookSettingsApiClient CostbookSettings { get; }
    ICostBookMaintenanceApiClient CostbookMaintenance { get; }
    ICostBookCapsStaffApiClient CostbookCapsStaff { get; }
    ICostBookAccountGroupApiClient CostbookAccountGroup { get; }
}
