namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsApiClient
    {
        IFpsStaffJobApiClient FpsStaffJob { get; }
        IFpsEmployeeApiClient FpsEmployee { get; }
        IFpsProgramApiClient FpsProgram { get; }
        IFpsProjectApiClient FpsProject { get; }
        IFpsLookupApiClient FpsLookup { get; }
        IFpsAnimalPlanApiClient FpsAnimalPlan { get; }
        IFpsSettingApiClient FpsSetting { get; }
        IFpsYearMasterApiClient FpsYearMaster { get; }
        IFpsProjectStaffPlanActualApiClient FpsProjectStaffPlanActual { get; }
        IFpsMonthlyOutputApiClient FpsMonthlyOutput { get; }
        IFpsDivisionApiClient FpsDivision { get; }
        IFpsAgencyApiClient FpsAgency { get; }
        IFpsAdditionalCostApiClient FpsAdditionalCost { get; }
        IFpsAccountCategoryApiClient FpsAccountCategory { get; }
        IFpsProfitCentreApiClient FpsProfitCentre { get; }
        IFpsProfitCentreGradeApiClient FpsProfitCentreGrade { get; }
        IFpsWorkGroupGradeApiClient FpsWorkGroupGrade { get; }
        IFpsWorkGroupEmployeeApiClient FpsWorkGroupEmployee { get; }
        IFpsDivisionGradeApiClient FpsMaintDG { get; }
        IFpsProjectStaffPlanApiClient FpsProjectStaffPlan { get; }
        IFpsProjectStaffPlanDetailsApiClient FpsProjectStaffPlanDetails { get; }
        IFpsProjectGroupStaffPlanApiClient FpsProjectGroupStaffPlan { get; }
        IFpsAnimalApiClient FpsAnimalMaster { get; }
        IFpsProjectGroupApiClient FpsProjectGroup { get; }
        IFpsBudgetBidsApiClient FpsBudgetBids { get; }
        IFpsPurchasesApiClient FpsPurchases { get; }
        IFpsUserApiClient FpsUserPermission { get; }
        IFpsGradeApiClient FpsGrade { get; }
        IFpsTestRCCostApiClient FpsTestRCCost { get; }
        IFpsTestRequirementRCCostApiClient FpsTestRequirementRCCost { get; }
        IFpsContributionSummaryApiClient FpsContributionSummary { get; }
        IFpsProjectAuditTrailApiClient FpsProjectAuditTrail { get; }
        IFpsTotalBusinessOverheadsApiClient FpsTotalBusinessOverheads { get; }
        IFpsCostCentreApiClient FpsCostCentre { get; }
        IFpsResourceAllocationApiClient FpsResourceAllocation { get; }
        IFpsResourceMgmtReplanApiClient FpsResourceMgmtReplan { get; }
        IFpsTestsRequiredByWgApiClient FpsTestsRequiredByWg { get; }
        IFpsTestsRequiredByRcApiClient FpsTestsRequiredByRc { get; }
        IFpsMonthHourApiClient FpsMonthHour { get; }
        IFpsYearEndApiClient FpsYearEnd { get; }
    }
}
