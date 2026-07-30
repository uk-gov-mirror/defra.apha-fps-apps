using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsApiClient : IPimsApiClient
    {
        public IPimsProjectListApiClient PimsProjectList { get; }
        public IPimsProjectDetailsApiClient PimsProjectDetails { get; }
        public IPimsProjectCommentApiClient PimsProjectComment { get; }
        public IPimsProposedProjectApiClient PimsProposedProject { get; }
        public IPimsProjectYearCostsApiClient PimsProjectYearCosts { get; }

        public IPimsMilestoneApiClient PimsMilestone { get; }
        public IPimsRadTrackInvoiceApiClient PimsRadTrackInvoice { get; }

        
        public IPimsYearlyFinancialDataApiClient PimsYearlyFinancialData { get; }

        public PimsApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            PimsProjectList = new PimsProjectListApiClient(http, mapper);
            PimsProjectDetails = new PimsProjectDetailsApiClient(http, mapper);
            PimsProjectComment = new PimsProjectCommentApiClient(http, mapper);
            PimsProposedProject = new PimsProposedProjectApiClient(http, mapper);
            PimsProjectYearCosts = new PimsProjectYearCostsApiClient(http, mapper);
            PimsMilestone = new PimsMilestoneApiClient(http, mapper);
            PimsRadTrackInvoice = new PimsRadTrackInvoiceApiClient(http, mapper);
            
            PimsYearlyFinancialData = new PimsYearlyFinancialDataApiClient(http, mapper);
        }
    }
}
