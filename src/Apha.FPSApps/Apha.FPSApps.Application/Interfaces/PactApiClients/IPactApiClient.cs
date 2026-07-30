namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactApiClient
    {
        IPactJobCodeApiClient PactJobCode { get; }
        IPactTimeCodeValidApiClient PactTimeCodeValid { get; }
        IPactWorkGroupApiClient PactWorkGroup { get; }
        IPactMonthApiClient PactMonth { get; }
        IPactCalenderMonthApiClient PactCalenderMonth { get; }
        IPactProjectInvoiceApiClient PactProjectInvoice { get; }
        IPactProjectSubContractApiClient PactProjectSubContract { get; }
        IPactTestCapabilityApiClient PactWorkGroupTestCapability { get; }
        IPactTestRequirementApiClient PactTestRequirement { get; }
        IPactTestorProductApiClient PactTestList { get; }
        IPactProjectMonthApiClient PactProjectMonth { get; }
        IPactProjectProfileApiClient PactProjectProfile { get; }
        IPactMonthlyOutputApiClient PactMonthlyOutput { get; }        
        IPactSummarisedWgTimeApiClient PactSummarisedWgTime { get; }            
        IPactWorkGroupReportEmailApiClient PactWorkGroupReportEmail { get; }
        IPactMonthlyTimeApiClient PactMonthlyTime { get; }
        IPactRecreateSummaryApiClient PactRecreateSummary { get; }
        IPactReleaseSummaryApiClient PactReleaseSummary { get; }
        IPactBosworthInterfaceApiClient PactBosworthInterface { get; }
        IPactTestActualBreakdownApiClient PactTestActualBreakdown { get; }
        IPactTestPlanCrossTabApiClient PactTestPlanCrossTab { get; }
    }
}
