namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsApiClient
    {        
        IPimsProjectListApiClient PimsProjectList { get; }
        IPimsProjectDetailsApiClient PimsProjectDetails { get; }       
        IPimsProjectCommentApiClient PimsProjectComment { get; }
        IPimsProposedProjectApiClient PimsProposedProject { get; }
        IPimsProjectYearCostsApiClient PimsProjectYearCosts { get; }
        IPimsMilestoneApiClient PimsMilestone { get; }
        IPimsRadTrackInvoiceApiClient PimsRadTrackInvoice { get; }
        
        IPimsYearlyFinancialDataApiClient PimsYearlyFinancialData { get; }
        IPimsReportApiClient PimsReport { get; }
        IPimsReportGroupApiClient PimsReportGroup { get; }
        IPimsReportGroupLinkApiClient PimsReportGroupLink { get; }
        IPimsProjectManagerApiClient PimsProjectManager { get; }
        IPimsProgramManagerLinkApiClient PimsProgramManagerLink { get; }
        IPimsProfitCentreManagerLinkApiClient PimsProfitCentreManagerLink { get; }
        IPimsSettingApiClient PimsSetting { get; }
        IPimsAccessUserApiClient PimsAccessUser { get; }
        IPimsAccessLevelApiClient PimsAccessLevel { get; }
        IPimsAccessUserLevelApiClient PimsAccessUserLevel { get; }
        IPimsAccessSystemApiClient PimsAccessSystem { get; }
        IPimsFrequencyApiClient PimsFrequency { get; }
        IPimsReviewItemApiClient PimsReviewItem { get; }
        IPimsRadTrackProgApiClient PimsRadTrackProg { get; }
        IPimsRiskApiClient PimsRisk { get; }
        IPimsPublicationTypeApiClient PimsPublicationType { get; }
    }
}
