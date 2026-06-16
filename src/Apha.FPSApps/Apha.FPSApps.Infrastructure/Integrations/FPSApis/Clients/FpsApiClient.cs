using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsApiClient : IFpsApiClient
    {
        public IFpsStaffJobApiClient FpsStaffJob { get; }
        public IFpsEmployeeApiClient FpsEmployee { get; }
        public IFpsProgramApiClient FpsProgram { get; }
        public IFpsProjectApiClient FpsProject { get; }
        public IFpsLookupApiClient FpsLookup { get; }
        public IFpsAnimalPlanApiClient FpsAnimalPlan { get; }
        public IFpsSettingApiClient FpsSetting { get; }
        public IFpsYearMasterApiClient FpsYearMaster { get; }
        public IFpsProjectStaffPlanActualApiClient FpsProjectStaffPlanActual { get; }
        public IFpsMonthlyOutputApiClient FpsMonthlyOutput { get; }
        public IFpsDivisionApiClient FpsDivision { get; }
        public IFpsAgencyApiClient FpsAgency { get; }
        public IFpsAdditionalCostApiClient FpsAdditionalCost { get; }
        public IFpsAccountCategoryApiClient FpsAccountCategory { get; }
        public IFpsDivisionGradeApiClient FpsMaintDG { get; }
        public IFpsAnimalApiClient FpsAnimalMaster { get; }

        public IFpsProfitCentreApiClient FpsProfitCentre { get; }
        public IFpsProfitCentreGradeApiClient FpsProfitCentreGrade { get; }
        public IFpsWorkGroupGradeApiClient FpsWorkGroupGrade { get; }
        public IFpsWorkGroupEmployeeApiClient FpsWorkGroupEmployee { get; }
        public IFpsWorkgroupApiClient FpsWorkgroup { get; }
        public IFpsProjectStaffPlanApiClient FpsProjectStaffPlan { get; }
        public IFpsProjectGroupStaffPlanApiClient FpsProjectGroupStaffPlan { get; }
        public IFpsProjectGroupApiClient FpsProjectGroup { get; }

        public IFpsWorkGroupGradeApiClient FpsWorkgroupGrade { get; }

        // TRANSFORMENGINE: FpsGrade added � Phase 9 (FpsGradeApiClient for frmMaintGrade ? api/v1/Grade)
        public IFpsGradeApiClient FpsGrade { get; }
        // TRANSFORMENGINE: FpsContributionSummary added — Phase 7 (IFpsContributionSummaryApiClient for frmTimeSellerPC — api/v1/contributionsummary)
        public IFpsContributionSummaryApiClient FpsContributionSummary { get; }

        public FpsApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            FpsStaffJob = new FpsStaffJobApiClient(http, mapper);
            FpsEmployee = new FpsEmployeeApiClient(http, mapper);
            FpsProgram = new FpsProgramApiClient(http, mapper);
            FpsProject = new FpsProjectApiClient(http, mapper);
            FpsLookup = new FpsLookupApiClient(http, mapper);
            FpsAnimalPlan = new FpsAnimalPlanApiClient(http, mapper);
            FpsSetting = new FpsSettingApiClient(http, mapper);
            FpsYearMaster = new FpsYearMasterApiClient(http, mapper);
            FpsProjectStaffPlanActual = new FpsProjectStaffPlanActualApiClient(http, mapper);
            FpsMonthlyOutput = new FpsMonthlyOutputApiClient(http, mapper);
            FpsDivision = new FpsDivisionApiClient(http, mapper);
            FpsAgency = new FpsAgencyApiClient(http, mapper);
            FpsAdditionalCost = new FpsAdditionalCostApiClient(http, mapper);
            FpsAccountCategory = new FpsAccountCategoryApiClient(http, mapper);
            FpsProfitCentre = new FpsProfitCentreApiClient(http, mapper);
            FpsProfitCentreGrade = new FpsProfitCentreGradeApiClient(http, mapper);
            FpsWorkGroupGrade = new FpsWorkGroupGradeApiClient(http, mapper);
            FpsWorkGroupEmployee = new FpsWorkGroupEmployeeApiClient(http, mapper);
            FpsWorkgroup = new FpsWorkgroupApiClient(http, mapper);
            FpsMaintDG = new FpsDivisionGradeApiClient(http, mapper);
            FpsProjectStaffPlan = new FpsProjectStaffPlanApiClient(http, mapper);
            FpsProjectGroupStaffPlan = new FpsProjectGroupStaffPlanApiClient(http, mapper);
            FpsAnimalMaster = new FpsAnimalApiClient(http, mapper);
            FpsProjectGroup = new FpsProjectGroupApiClient(http, mapper);
            FpsWorkgroupGrade = new FpsWorkGroupGradeApiClient(http, mapper);
            // TRANSFORMENGINE: FpsGrade wired � FpsGradeApiClient registered on aggregate client
            FpsGrade = new FpsGradeApiClient(http, mapper);
            // TRANSFORMENGINE: FpsContributionSummary wired — Phase 9 real client registered (replaces FpsContributionSummaryApiClientStub)
            FpsContributionSummary = new FpsContributionSummaryApiClient(http, mapper);
        }
    }
}
