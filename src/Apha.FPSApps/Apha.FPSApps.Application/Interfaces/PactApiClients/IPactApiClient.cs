namespace Apha.FPSApps.Application.Interfaces.PactApiClients
{
    public interface IPactApiClient
    {
        IPactJobCodeApiClient PactJobCode { get; }
        IPactTimeCodeValidApiClient PactTimeCodeValid { get; }
        IPactWorkGroupApiClient PactWorkGroup { get; }
        IPactMonthApiClient PactMonth { get; }
        IPactProjectInvoiceApiClient PactProjectInvoice { get; }
        IPactProjectSubContractApiClient PactProjectSubContract { get; }
        IPactTestCapabilityApiClient PactWorkGroupTestCapability { get; }
        IPactTestRequirementApiClient PactTestRequirement { get; }
        IPactTestorProductApiClient PactTestList { get; }
        IPactProjectMonthApiClient PactProjectMonth { get; }
        IPactProjectProfileApiClient PactProjectProfile { get; }
        IPactMonthlyOutputApiClient PactMonthlyOutput { get; }
        IPactCalenderMonthApiClient PactCalenderMonth { get; }
        IPactMonthlyTimeApiClient PactMonthlyTime { get; }
    }
}
