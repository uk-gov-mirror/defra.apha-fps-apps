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
    }
}
