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
        IFpsWorkgroupApiClient FpsWorkgroup { get; }
        IFpsDivisionGradeApiClient FpsMaintDG { get; }
        IFpsProjectStaffPlanApiClient FpsProjectStaffPlan { get; }
        IFpsProjectGroupStaffPlanApiClient FpsProjectGroupStaffPlan { get; }
        IFpsAnimalApiClient FpsAnimalMaster { get; }
        IFpsProjectGroupApiClient FpsProjectGroup { get; }
        // TRANSFORMENGINE: FpsGrade added � Phase 7 (IFpsGradeApiClient for frmMaintGrade ? api/v1/Grade)
        IFpsGradeApiClient FpsGrade { get; }
        // TRANSFORMENGINE: FpsContributionSummary added — Phase 7 (IFpsContributionSummaryApiClient for frmTimeSellerPC — api/v1/contributionsummary)
        IFpsContributionSummaryApiClient FpsContributionSummary { get; }
    }
}
