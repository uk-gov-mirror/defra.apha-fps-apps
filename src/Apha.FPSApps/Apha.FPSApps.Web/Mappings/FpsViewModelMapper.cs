using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
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
            CreateMap<TestReqBreakdownItem, TestReqBreakdownDto>()
                .ForMember(d => d.Pc, o => o.MapFrom(s => s.PC))
                .ForMember(d => d.WgPrice, o => o.MapFrom(s => s.WGPrice))
                .ReverseMap()
                .ForMember(d => d.PC, o => o.MapFrom(s => s.Pc))
                .ForMember(d => d.WGPrice, o => o.MapFrom(s => s.WgPrice));
            CreateMap<TestActualBreakdownItem, TestActualBreakdownDto>().ReverseMap();
            CreateMap<ProgramViewModel, ProgramDto>().ReverseMap();
            CreateMap<AnimalMaintenanceViewModel, AnimalDto>().ReverseMap();
            CreateMap<UserPermissionViewModel, UserDto>().ReverseMap();
            CreateMap<EmployeeViewModel, EmployeeDto>().ReverseMap();
            CreateMap<StaffJobViewDto, StaffJobDto>().ReverseMap();
            CreateMap<ProjectDto, ProjectViewModel>().ReverseMap();
            CreateMap<ProjectSpecificQueryDto, ProjectSpecificQueryItem>().ReverseMap();
            CreateMap<ProjectDto, ProgramProjectEditViewModel>().ReverseMap();
            CreateMap<ProjectDto, ProgramProjectItem>()
                .ForMember(d => d.TransferIncome, o => o.MapFrom(s => s.TransferIncome))
                .ReverseMap();
            CreateMap<AnimalPlanItem, AnimalCostViewDto>().ReverseMap();
            CreateMap<AnimalPlanItem, AnimalRequestDto>().ReverseMap();
            CreateMap<AnimalCostsItem, AnimalCostViewDto>().ReverseMap();
            CreateMap<AnimalSnapshotItem, AnimalSnapshotViewDto>().ReverseMap();
            CreateMap<TimeSnapshotItem, ProgramPlanCostViewDto>().ReverseMap();
            CreateMap<ProjectSnapshotItem, ProjectSnapshotViewDto>().ReverseMap();
            CreateMap<TestSnapshotItem, TestFeePlanViewDto>().ReverseMap();
            CreateMap<GenericBidItem, GenericBidViewDto>().ReverseMap();
            CreateMap<ExceptionalCostSnapshotItem, ProjectExceptionalCostViewDto>().ReverseMap();
            CreateMap<CompareStaff2Item, TimeCostCalcsViewDto>().ReverseMap();
            CreateMap<ActualProjectCostItem, ProjectSubContractDto>().ReverseMap();
            CreateMap<DivisionViewModel, DivisionDto>().ReverseMap();
            CreateMap<DivisionGradeItem, DivisionGradeDto>().ReverseMap();
            CreateMap<GradeItem, GradeDto>().ReverseMap();

            // Maps CostCentreItem (DataGrid row: CostCentreNo double, ProfitCentre string) <-> CostCentreDto
            CreateMap<CostCentreItem, CostCentreDto>().ReverseMap();

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

            // Work Group Staff Maintenance
            CreateMap<WorkGroupEmployeeStaffItem, WorkGroupEmployeeStaffDto>().ReverseMap();
            // Resource Set-Up
            CreateMap<SetUpStaffResourcesItem, WorkGroupEmployeeStaffDto>()
                .ForMember(d => d.PersonStatus, o => o.Ignore())
                .ForMember(d => d.PersonClass, o => o.Ignore())
                .ForMember(d => d.TimeRecorder, o => o.Ignore())
                .ForMember(d => d.StartDate, o => o.Ignore())
                .ForMember(d => d.EndDate, o => o.Ignore())
                .ForMember(d => d.HoursPerWeek, o => o.Ignore())
                .ReverseMap();
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

            // Staff Plan Details view
            CreateMap<StaffPlanDetailsViewItem, ProjectStaffPlanDetailsViewDto>().ReverseMap();

            // Project Group Staff Plan view
            CreateMap<ProjectGroupStaffPlanViewItem, ProjectGroupStaffPlanViewDto>().ReverseMap();

            // Workgroup Staff Plan view
            CreateMap<WgStaffPlanViewItem, WgStaffPlanViewDto>().ReverseMap();

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

            // Resource Allocation — Staff Jobs grid
            CreateMap<ResourceStaffJobDetailDto, ResourceStaffJobItem>()
                .ForMember(d => d.Project, o => o.MapFrom(s => s.JobCode))
                .ForMember(d => d.Description, o => o.MapFrom(s => s.JobDescription))
                .ForMember(d => d.Hour, o => o.MapFrom(s => s.PlannedHours))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.ProjectStatus))
                .ForMember(d => d.StaffId, o => o.Ignore());

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

            // TestListVla grid row ↔ DTO (frmTestList / fsubTest_MainList):
            CreateMap<TestorProductDto, TestListVlaItem>().ReverseMap();

            // TestRCCost grid row ↔ DTO (fsubTestRCPrice / Component Charges general tab):
            CreateMap<TestRCCostItem, TestRCCostDto>().ReverseMap();

            // TestRequirementRCCost grid row ↔ DTO (fsubTestRequirementRCPrice / Component Charges project tab):
            CreateMap<TestRequirementRCCostItem, TestRequirementRCCostDto>().ReverseMap();

            // TestRequirementItem grid row ↔ DTO (Test Requirements tab — stage2TestRequirementsGrid):
            // Convention ReverseMap: Buyer, NoRequired, UnitPrice, TestCode, FpsYear all match DTO names.
            CreateMap<TestRequirementItem, TestRequirementDto>().ReverseMap();

            // ResourceAllocation - Stage 2 Check Resource Allocation
            CreateMap<ResourceStaffAllocationDto, ResourceStaffAllocationItem>().ReverseMap();
            CreateMap<ResourceStaffJobDto, ResourceStaffJobItem>().ReverseMap();

            // TRANSFORMENGINE: WorkgroupMaintenance mappings added — Phase 10 (Step 15b)
            // DataGrid row: WorkgroupMaintenanceItem <-> WorkGroupDto (grid display and row selection)
            // All property names match exactly between Item and Dto (convention-based) — no ForMember needed.
            // Phase 11 adds [DataGridColumn] attributes to WorkgroupMaintenanceItem.
            CreateMap<WorkgroupMaintenanceItem, WorkGroupDto>().ReverseMap();

            // Modal form ViewModel: WorkgroupMaintenanceViewModel does not map 1:1 to Dto —
            // the ViewModel wraps DataGridConfig<WorkgroupMaintenanceItem>; modal binding uses
            // WorkgroupMaintenanceItem directly from the grid row item. No ViewModel <-> Dto map needed.

            // ResourceMgmtReplan — Resource Re-allocation Screen (frmRM_RePlan)
            CreateMap<ResourceMgmtReplanViewDto, ResourceMgmtReplanGridItem>().ReverseMap();
            CreateMap<ProjectStaffReplanDto, ResourceMgmtReplanGridItem>()
                .ForMember(d => d.StaffRowKey, o => o.MapFrom(s => $"{s.ParentProject}|{s.WgGrade}"));
            CreateMap<ResourceMgmtReplanStaffJobDto, ResourceMgmtReplanAllTimeItem>().ReverseMap();
            CreateMap<StaffJobViewDto, ResourceMgmtReplanAllTimeItem>()
                .ForMember(d => d.StaffId, o => o.MapFrom(s => s.StaffID));
            CreateMap<ResourceMgmtReplanStaffJobDto, ResourceMgmtReplanStagedItem>().ReverseMap();

            CreateMap<Apha.FPSApps.Application.Dtos.FPS.BatchJobHistoryDto, YearEndHistoryItem>();
        }
    }
}
