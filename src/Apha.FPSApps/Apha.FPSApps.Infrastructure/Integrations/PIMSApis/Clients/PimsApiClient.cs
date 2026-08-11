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
        public IPimsReportApiClient PimsReport { get; }
        public IPimsReportGroupApiClient PimsReportGroup { get; }
        public IPimsReportGroupLinkApiClient PimsReportGroupLink { get; }
        public IPimsProjectManagerApiClient PimsProjectManager { get; }
        public IPimsProgramManagerLinkApiClient PimsProgramManagerLink { get; }
        public IPimsProfitCentreManagerLinkApiClient PimsProfitCentreManagerLink { get; }
        public IPimsSettingApiClient PimsSetting { get; }
        public IPimsAccessUserApiClient PimsAccessUser { get; }
        public IPimsAccessLevelApiClient PimsAccessLevel { get; }
        public IPimsAccessUserLevelApiClient PimsAccessUserLevel { get; }
        public IPimsAccessSystemApiClient PimsAccessSystem { get; }
        public IPimsFrequencyApiClient PimsFrequency { get; }
        public IPimsReviewItemApiClient PimsReviewItem { get; }
        
        public IPimsRadTrackProgApiClient PimsRadTrackProg { get; }
        public IPimsRiskApiClient PimsRisk { get; }
        public IPimsPublicationTypeApiClient PimsPublicationType { get; }

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
            PimsReport = new PimsReportApiClient(http, mapper);
            PimsReportGroup = new PimsReportGroupApiClient(http, mapper);
            PimsReportGroupLink = new PimsReportGroupLinkApiClient(http, mapper);
            PimsProjectManager = new PimsProjectManagerApiClient(http, mapper);
            PimsProgramManagerLink = new PimsProgramManagerLinkApiClient(http, mapper);
            PimsProfitCentreManagerLink = new PimsProfitCentreManagerLinkApiClient(http, mapper);
            PimsSetting = new PimsSettingApiClient(http, mapper);
            PimsAccessUser = new PimsAccessUserApiClient(http, mapper);
            PimsAccessLevel = new PimsAccessLevelApiClient(http, mapper);
            PimsAccessUserLevel = new PimsAccessUserLevelApiClient(http, mapper);
            PimsAccessSystem = new PimsAccessSystemApiClient(http, mapper);
            PimsFrequency = new PimsFrequencyApiClient(http, mapper);
            PimsReviewItem = new PimsReviewItemApiClient(http, mapper);
            PimsRadTrackProg = new PimsRadTrackProgApiClient(http, mapper);
            PimsRisk = new PimsRiskApiClient(http, mapper);
            PimsPublicationType = new PimsPublicationTypeApiClient(http, mapper);
        }
    }
}
