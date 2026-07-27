using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.FPS.BulkRates;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
namespace Apha.FPSApps.Web.Mappings
{
    public class FpsViewModelMapper : Profile
    {
        public FpsViewModelMapper()
        {
            CreateMap(typeof(PaginationFilter<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap<StaffJobItemViewModel, StaffJobViewDto>().ReverseMap();
            CreateMap<PaginationModel, PaginationDto>().ReverseMap();
            CreateMap<TestPriceCheckDto, TestPriceCheckItem>()
                .ForMember(d => d.IsDefraProjectList, o => o.Ignore());
            CreateMap<TestPriceCheckItem, TestPriceCheckDto>();
            CreateMap<ProgramViewModel, ProgramDto>().ReverseMap();
            CreateMap<AnimalMaintenanceViewModel, AnimalDto>().ReverseMap();
            CreateMap<UserPermissionViewModel, UserDto>().ReverseMap();
            CreateMap<EmployeeViewModel, EmployeeDto>().ReverseMap();
            CreateMap<StaffJobViewDto, StaffJobDto>().ReverseMap();
            CreateMap<ProjectDto, ProjectViewModel>().ReverseMap();
            CreateMap<ProjectDto, ProgramProjectEditViewModel>().ReverseMap();
            CreateMap<ProjectDto, ProgramProjectItem>()
                .ForMember(d => d.TransferIncome, o => o.MapFrom(s => s.TransferIncome))
                .ReverseMap();
            CreateMap<AnimalPlanItem, AnimalCostViewDto>().ReverseMap();
            CreateMap<AnimalPlanItem, AnimalRequestDto>().ReverseMap();
            CreateMap<CompareStaff2Item, TimeCostCalcsViewDto>().ReverseMap();
            CreateMap<ActualProjectCostItem, ProjectSubContractDto>().ReverseMap();
            CreateMap<DivisionViewModel, DivisionDto>().ReverseMap();
            CreateMap<DivisionGradeItem, DivisionGradeDto>().ReverseMap();
            CreateMap<GradeItem, GradeDto>().ReverseMap();
            CreateMap<ResourceCentreMaintenanceItem, ProfitCentreDto>().ReverseMap();
            CreateMap<TestPlanItem, TestRequirementDto>().ReverseMap();
            CreateMap<AdditionalCostItemViewModel, AdditionalCostDto>().ReverseMap();
            CreateMap<AccountCategoryViewModel, AccountCategoryDto>().ReverseMap();
            CreateMap<TestPlanActualItem, TestRequirementDto>().ReverseMap();
            CreateMap<ActualTestOutputItem, MonthlyOutputDto>().ReverseMap();

            // ProgrammeNewProject
            CreateMap<ProjectDto, ProgrammeNewProjectViewModel>().ReverseMap();

            // PortfolioNew
            CreateMap<ProjectDto, PortfolioNewViewModel>().ReverseMap();

            //Work Group Staff Maintenance
            CreateMap<WorkGroupEmployeeStaffItem, WorkGroupEmployeeStaffDto>().ReverseMap();
            // Resource Set-Up
            CreateMap<WorkGroupEmployeeItem, WorkGroupEmployeeDto>().ReverseMap();

            // ProfitCentreGradeMaint
            CreateMap<ProfitCentreGradeMaintItem, ProfitCentreGradeDto>().ReverseMap();

            // BudgetResourceLevel
            CreateMap<BudgetResourceCentreLevelItem, BidViewDto>().ReverseMap();
            CreateMap<PurchaseItem, PurchaseDto>().ReverseMap();
            CreateMap<WorkGroupItem, WorkGroupDto>()
                .ForMember(d => d.WorkGroupName, o => o.MapFrom(s => s.WorkGroupName))
                .ReverseMap()
                .ForMember(d => d.WorkGroup, o => o.MapFrom(s => s.WorkGroupName));

            // ProjectProfitability
            CreateMap<ProjectProfitabilityDto, ProjectProfitabilityItem>().ReverseMap();

            // ProjectProfitabilityVla
            //   are expected to match ProjectProfitabilityVlaDto exactly (JobCode, Program, Customer,
            //   Manager, Status, StaffCosts, TestCost, AnimalCosts, AdditionalCosts, TotalCosts,
            //   Budget, Profit, TargetProfit, OffTarget, Id).
            //   ProjectProfitabilityVlaItem is defined in Phase 11; see DEFERRED note in file header.
            CreateMap<ProjectProfitabilityVlaDto, ProjectProfitabilityVlaItem>().ReverseMap();

            // Staff Plan view
            CreateMap<StaffPlanViewItem, ProjectStaffPlanViewDto>().ReverseMap();

            // Project Group Staff Plan view
            CreateMap<ProjectGroupStaffPlanViewItem, ProjectGroupStaffPlanViewDto>().ReverseMap();

            // Test Supplier
            CreateMap<TestSupplierItem, Apha.FPSApps.Application.Dtos.PACT.TestSupplierViewDto>().ReverseMap();
            CreateMap<TestSupplierItem, TestRequirementDto>()
                .ForMember(d => d.TestCode, o => o.MapFrom(s => s.TestCode))
                .ForMember(d => d.Buyer, o => o.MapFrom(s => s.Buyer))
                .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.UnitPrice))
                .ForMember(d => d.NoRequired, o => o.MapFrom(s => s.NoRequired))
                .ForMember(d => d.ProjectBuyerCode, o => o.MapFrom(s => s.ProjectBuyerCode))
                .ForMember(d => d.TestBuyerCode, o => o.MapFrom(s => s.TestBuyerCode))
                .ForMember(d => d.Active, o => o.MapFrom(s => s.Active))
                .ForMember(d => d.RecUnitPrice, o => o.MapFrom(s => s.RecUnitPrice))
                .ReverseMap();
            CreateMap<MaintWGGradeItem, WorkgroupGradeDto>().ReverseMap();

            // Test Capability (FPS portfolio page — reuses PACT TestCapabilityDto)
            CreateMap<Apha.FPSApps.Web.Areas.FPS.Models.TestCapabilityItem, Apha.FPSApps.Application.Dtos.PACT.TestCapabilityDto>().ReverseMap();

            // Plan Staff ZT Code
            CreateMap<PlanStaffZTCodeItemViewModel, StaffJobViewDto>().ReverseMap();
            CreateMap<PlanStaffZTCodeItemViewModel, StaffJobDto>()
                .ForMember(d => d.StaffId, o => o.MapFrom(s => s.StaffID))
                .ReverseMap();

            // Contribution Summary — row grid item
            CreateMap<ContributionSummaryRowDto, ContributionSummaryRowItem>().ReverseMap();
            // Total Business Overheads
            CreateMap<TotalBusinessOverheadsViewModel, TotalBusinessOverheadsDto>().ReverseMap();

            // Misc Project Data
            CreateMap<ProjectDto, ProjectMiscItem>()
                .ForMember(d => d.ParentProject, o => o.MapFrom(s => s.ParentProject))
                .ForMember(d => d.Program, o => o.MapFrom(s => s.Program))
                .ForMember(d => d.CostCentre, o => o.MapFrom(s => s.CostCentre))
                .ForMember(d => d.OracleProjectCode, o => o.MapFrom(s => s.OracleProjectCode))
                .ForMember(d => d.SubAccountCode, o => o.MapFrom(s => s.SubAccountCode))
                .ReverseMap();

            // all 5 *LogItem ViewModel types are created in Phase 11.
            // Audit log items are read-only grid rows — .ReverseMap() is intentionally omitted.

            // UserEmail is NOT in ProjectLogDto (requires backend UserId→email resolution); Ignore() it.
            CreateMap<ProjectLogDto, ProjectLogItem>()
                .ForMember(d => d.UserEmail, o => o.Ignore());

            // UserEmail is NOT in StaffJobLogDto (requires UserId→email resolution); Ignore() it.
            CreateMap<StaffJobLogDto, StaffJobLogItem>()
                .ForMember(d => d.UserEmail, o => o.Ignore());

            // UserEmail is NOT in TestRequirementLogDto (requires UserId→email resolution); Ignore() it.
            CreateMap<TestRequirementLogDto, TestRequirementLogItem>()
                .ForMember(d => d.UserEmail, o => o.Ignore());

            // UserEmail is NOT in AnimalRequestLogDto (requires UserId→email resolution); Ignore() it.
            CreateMap<AnimalRequestLogDto, AnimalRequestLogItem>()
                .ForMember(d => d.UserEmail, o => o.Ignore());

            // UserEmail is NOT in AdditionalCostLogDto (requires UserId→email resolution); Ignore() it.
            CreateMap<AdditionalCostLogDto, AdditionalCostLogItem>()
                .ForMember(d => d.UserEmail, o => o.Ignore());

            // Bulk Rates — queue grid row. JobName mapped to its friendly display name since
            // the DataGrid renders raw property values with no per-column custom formatting.
            CreateMap<BulkRatesQueueEntryDto, BulkRatesQueueGridItem>()
                .ForMember(d => d.JobName, o => o.MapFrom(s => FriendlyBulkRatesJobName(s.JobName)));

            // Bulk Rates — staging grids (Detail page). ValidationSummary has no source member —
            // it's populated after mapping, from BulkRatesUploadResultDto, by
            // BulkRatesController.BuildStagingGridConfig.
            CreateMap<BulkRatesStagingFecRowDto, FecStagingGridItem>()
                .ForMember(d => d.ValidationSummary, o => o.Ignore());
            CreateMap<BulkRatesStagingAgrupRowDto, AgrupStagingGridItem>()
                .ForMember(d => d.Active, o => o.MapFrom(s => s.Active == 1))
                .ForMember(d => d.ValidationSummary, o => o.Ignore());
            CreateMap<BulkRatesStagingStaffRowDto, StaffStagingGridItem>();
            CreateMap<BulkRatesStagingAnimalRowDto, AnimalStagingGridItem>();
        }

        // AutoMapper's MapFrom builds an expression tree, which cannot contain a switch
        // expression — a plain method call is used instead.
        private static string FriendlyBulkRatesJobName(string jobName) => jobName switch
        {
            "BulkTestRatesUpdate" => "FEC Test Rates",
            "BulkStaffRatesUpdate" => "Staff Rates",
            "BulkAnimalRatesUpdate" => "Animal Rates",
            _ => jobName
        };
    }
}
