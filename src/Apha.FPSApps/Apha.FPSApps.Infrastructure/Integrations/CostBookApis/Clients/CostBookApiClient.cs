using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients
{
    public class CostBookApiClient : ICostBookApiClient
    {
        public ICostBookProjectApiClient Projects { get; }
        public ICostBookCustomerApiClient Customers { get; }
        public ICostBookDiseaseApiClient Diseases { get; }
        public ICostBookProgramApiClient Programs { get; }
        public ICostBookStaffApiClient Staff { get; }
        public ICostBookContractApiClient Contracts { get; }
        public ICostBookYearlyDetailsApiClient YearlyDetails { get; }
        public ICostBookProjectSummaryApiClient ProjectSummary { get; }
        public ICostBookSettingsApiClient CostbookSettings { get; }        
        public ICostBookMaintenanceApiClient CostbookMaintenance { get; }
        public ICostBookCapsStaffApiClient CostbookCapsStaff { get; }       
        public ICostBookAccountGroupApiClient CostbookAccountGroup { get; }

        public CostBookApiClient(ICostBookHttpExecutor http, IMapper mapper)
        {
            Projects = new CostBookProjectApiClient(http, mapper);
            Customers = new CostBookCustomerApiClient(http, mapper);
            Diseases = new CostBookDiseaseApiClient(http, mapper);
            Programs = new CostBookProgramApiClient(http, mapper);
            Staff = new CostBookStaffApiClient(http, mapper);
            Contracts = new CostBookContractApiClient(http, mapper);
            YearlyDetails = new CostBookYearlyDetailsApiClient(http, mapper);
            ProjectSummary = new CostBookProjectSummaryApiClient(http, mapper);
            CostbookSettings = new CostBookSettingsApiClient(http, mapper);
            CostbookMaintenance = new CostBookMaintenanceApiClient(http, mapper);
            CostbookCapsStaff = new CostBookCapsStaffApiClient(http, mapper);
            CostbookAccountGroup = new CostBookAccountGroupApiClient(http, mapper);
        }
    }
}
