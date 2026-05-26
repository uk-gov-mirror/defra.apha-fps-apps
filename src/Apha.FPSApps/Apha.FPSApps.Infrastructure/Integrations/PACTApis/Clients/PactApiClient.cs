using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients
{
    public class PactApiClient : IPactApiClient
    {
        public IPactJobCodeApiClient PactJobCode { get; }
        public IPactTimeCodeValidApiClient PactTimeCodeValid { get; }
        public IPactWorkGroupApiClient PactWorkGroup { get; }
        public IPactMonthApiClient PactMonth { get; }
        public IPactProjectInvoiceApiClient PactProjectInvoice { get; }
        public IPactProjectSubContractApiClient PactProjectSubContract { get; }
        public IPactTestCapabilityApiClient PactWorkGroupTestCapability { get; }
        public IPactTestRequirementApiClient PactTestRequirement { get; }
        public IPactTestorProductApiClient PactTestList { get; }
        public IPactProjectMonthApiClient PactProjectMonth { get; }
        public IPactProjectProfileApiClient PactProjectProfile { get; }
        public IPactMonthlyOutputApiClient PactMonthlyOutput { get; }
        public IPactCalenderMonthApiClient PactCalenderMonth { get; }
        public IPactMonthlyTimeApiClient PactMonthlyTime { get; }

        public PactApiClient(IPactHttpExecutor http, IMapper mapper)
        {
            PactJobCode = new PactJobCodeApiClient(http, mapper);
            PactTimeCodeValid = new PactTimeCodeValidApiClient(http, mapper);
            PactWorkGroup = new PactWorkGroupApiClient(http, mapper);
            PactMonth = new PactMonthApiClient(http, mapper);
            PactProjectInvoice = new PactProjectInvoiceApiClient(http, mapper);
            PactProjectSubContract = new PactProjectSubContractApiClient(http, mapper);
            PactWorkGroupTestCapability = new PactTestCapabilityApiClient(http, mapper);
            PactTestRequirement = new PactTestRequirementApiClient(http, mapper);
            PactTestList = new PactTestorProductApiClient(http, mapper);
            PactProjectMonth = new PactProjectMonthApiClient(http, mapper);
            PactProjectProfile = new PactProjectProfileApiClient(http, mapper);
            PactMonthlyOutput = new PactMonthlyOutputApiClient(http, mapper);
            PactCalenderMonth = new PactCalenderMonthApiClient(http, mapper);
            PactMonthlyTime = new PactMonthlyTimeApiClient(http, mapper);
        }
    }
}
