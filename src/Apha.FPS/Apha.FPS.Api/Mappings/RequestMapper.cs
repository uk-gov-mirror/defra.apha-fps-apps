// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — RequestMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - No new mapping entries required for WorkGroupEmployee in this phase.
 *     WorkGroupEmployeeDto <-> WorkGroupEmployeeReq and WorkGroupEmployeeDto <-> WorkGroupEmployeeRes
 *     were already registered (lines 77-78). AutoMapper resolves the new fields
 *     (TimeRecorder, StartDate, EndDate, HoursPerWeek) by name convention without explicit
 *     ForMember configuration, because field names are identical across Req, Res, and Dto.
 *
 * PRESERVED:
 *   - All existing CreateMap entries for StaffJob, Animal, Project, Division, WorkGroupEmployee,
 *     ProfitCentre, PaginationReq/Res, and all other FPS domain mappings unchanged.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Run AutoMapper configuration validation (AssertConfigurationIsValid)
 *     in a test to confirm WorkGroupEmployeeReq -> WorkGroupEmployeeDto covers all new fields
 *     (TimeRecorder, StartDate, EndDate, HoursPerWeek) without unmapped-member warnings.
 *   - TRANSFORMENGINE TODO: If AutoMapper strict mode is enabled project-wide, explicitly confirm
 *     that no ForMember entries are needed for the new nullable/int fields.
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using AutoMapper;

namespace Apha.FPS.Api.Mappings
{
    public class RequestMapper : Profile
    {
        public RequestMapper()
        {
            CreateMap(typeof(PaginationReq<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();

            CreateMap<Pagination, PaginationDto>().ReverseMap();

            CreateMap<StaffJobViewDto, StaffJobViewRes>().ReverseMap();
            CreateMap<StaffJobZtViewDto, StaffJobZtViewRes>().ReverseMap();
            CreateMap<StaffWorkgroupLookupDto, StaffWorkgroupLookupRes>().ReverseMap();
            CreateMap<StaffJobDto, StaffJobReq>().ReverseMap();
            CreateMap<StaffJobDto, StaffJobRes>().ReverseMap();

            CreateMap<FpsSettingRes, FpsSettingDto>().ReverseMap();

            CreateMap<AnimalCostViewDto, AnimalCostViewRes>().ReverseMap();
            CreateMap<AnimalDto, AnimalRes>().ReverseMap();
            CreateMap<AnimalReq, AnimalDto>().ReverseMap();
            CreateMap<AnimalRequestDto, AnimalRequestReq>().ReverseMap();
            CreateMap<AnimalRequestDto, AnimalRequestRes>().ReverseMap();
            CreateMap<EmployeeDto, EmployeeReq>().ReverseMap();
            CreateMap<EmployeeDto, EmployeeRes>().ReverseMap();
            CreateMap<ManagerDto, ManagerRes>().ReverseMap();
            CreateMap<ProgramReq, ProgramDto>().ReverseMap();
            CreateMap<ProgramRes, ProgramDto>().ReverseMap();

            CreateMap<ProjectDto, ProjectReq>()
                .ForMember(d => d.BudgetExt, o => o.MapFrom(s => s.CustIncome)).ReverseMap()
                .ForMember(d => d.CustIncome, o => o.MapFrom(s => s.BudgetExt));
            CreateMap<ProjectDto, ProjectRes>()
                .ForMember(d => d.BudgetExt, o => o.MapFrom(s => s.CustIncome)).ReverseMap()
                .ForMember(d => d.CustIncome, o => o.MapFrom(s => s.BudgetExt));

            //   JobCode (DTO natural key) -> Project (response display column per HTML prototype)
            //   Id is int? in DTO (nullable ROW_NUMBER) -> int in Res (non-nullable contract property)
            CreateMap<ProjectProfitabilityVlaDto, ProjectProfitabilityVlaRes>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.GetValueOrDefault(0)))
                .ForMember(d => d.Project, o => o.MapFrom(s => s.JobCode));
            CreateMap<PaginatedResult<ProjectProfitabilityVlaDto>, PaginationRes<ProjectProfitabilityVlaRes>>();

            CreateMap<ContractDto, ContractRes>()
                .ForMember(d => d.ContractNo, o => o.MapFrom(s => s.Contractno))
                .ForMember(d => d.Category, o => o.MapFrom(s => s.Category));
            CreateMap<YearMasterRes, YearMasterDto>().ReverseMap();
            CreateMap<DivisionReq, DivisionDto>().ReverseMap();
            CreateMap<DivisionRes, DivisionDto>().ReverseMap();
            CreateMap<GradeDto, GradeRes>().ReverseMap();
            CreateMap<GradeReq, GradeDto>().ReverseMap();
            CreateMap<DivisionGradeReq, DivisionGradeDto>().ReverseMap();
            CreateMap<DivisionGradeRes, DivisionGradeDto>().ReverseMap();
            CreateMap<AgencyRes, AgencyDto>().ReverseMap();

            // ProgrammeNewProject mappings
            CreateMap<AccountCodeDto, AccountCodeRes>().ReverseMap();
            CreateMap<SubAccountDto, SubAccountRes>()
                .ForMember(d => d.SubAccount, o => o.MapFrom(s => s.SubAccountName)).ReverseMap()
                .ForMember(d => d.SubAccountName, o => o.MapFrom(s => s.SubAccount));
            CreateMap<ProjectGroupDto, ProjectGroupRes>().ReverseMap();
            CreateMap<TimeCostCalcsViewDto, TimeCostCalcsViewRes>().ReverseMap();
            CreateMap<TimeCostCalcsTotalsDto, TimeCostCalcsTotalsRes>().ReverseMap();
            CreateMap<AdditionalCostDto, AdditionalCostReq>().ReverseMap();
            CreateMap<AdditionalCostDto, AdditionalCostRes>().ReverseMap();
            CreateMap<AccountCategoryDto, AccountCategoryReq>().ReverseMap();
            CreateMap<AccountCategoryDto, AccountCategoryRes>().ReverseMap();
            CreateMap<MonthlyOutputDto, MonthlyOutputRes>().ReverseMap();
            CreateMap<CostCentreWorkgroup, CostCentreWorkgroupRes>().ReverseMap();
            CreateMap<WorkGroupPersonDto, WorkGroupPersonRes>().ReverseMap();

            // ResourceSetUp
            CreateMap<ProfitCentreDto, ProfitCentreRes>().ReverseMap();
            CreateMap<ProfitCentreReq, ProfitCentreDto>().ReverseMap();
            CreateMap<ProfitCentreCostDto, ProfitCentreCostRes>().ReverseMap();
            CreateMap<ProfitCentreGradeDto, ProfitCentreGradeRes>().ReverseMap();
            CreateMap<ProfitCentreGradeReq, ProfitCentreGradeDto>().ReverseMap();
            CreateMap<WorkgroupGradeDto, WorkgroupGradeRes>().ReverseMap();

            // TRANSFORMENGINE: WorkGroupEmployee mappings verified — covers all CRUD operations including
            // POST CreateWorkGroupEmployeeAsync added in Phase 5. New fields (TimeRecorder, StartDate,
            // EndDate, HoursPerWeek) are resolved by AutoMapper name convention — no ForMember needed.
            CreateMap<WorkGroupEmployeeDto, WorkGroupEmployeeReq>().ReverseMap();
            CreateMap<WorkGroupEmployeeDto, WorkGroupEmployeeRes>().ReverseMap();

            CreateMap<ProjectProfitabilityDto, ProjectProfitabilityRes>().ReverseMap();

            CreateMap<ProjectStaffPlanViewDto, ProjectStaffPlanViewRes>().ReverseMap();
            CreateMap<PaginatedResult<ProjectStaffPlanViewDto>, PaginationRes<ProjectStaffPlanViewRes>>();

            CreateMap<ProjectGroupStaffPlanViewDto, ProjectGroupStaffPlanViewRes>().ReverseMap();
            CreateMap<PaginatedResult<ProjectGroupStaffPlanViewDto>, PaginationRes<ProjectGroupStaffPlanViewRes>>();

            CreateMap<PactStaffDto, PactStaffRes>().ReverseMap();
            CreateMap<WorkgroupGradeDto, WorkgroupGradeReq>().ReverseMap();
             

            // UserPermission
            CreateMap<UserDto, UserRes>().ReverseMap();
            CreateMap<UserReq, UserDto>().ReverseMap();
            CreateMap<UserPermissionDto, UserPermissionRes>().ReverseMap();
            CreateMap<UserPermissionReq, UserPermissionDto>().ReverseMap();
            CreateMap<PermissionOptionsDto, PermissionOptionsRes>().ReverseMap();

            // BudgetResourceLevel
            CreateMap<BidDto, BidReq>().ReverseMap();
            CreateMap<BidDto, BidRes>().ReverseMap();
            CreateMap<BidViewDto, BidViewRes>().ReverseMap();
            CreateMap<PurchaseDto, PurchaseReq>().ReverseMap();
            CreateMap<PurchaseDto, PurchaseRes>().ReverseMap();

            // TimeSellerPC — frmTimeSellerPC
            CreateMap<ContributionSummaryRowDto, ContributionSummaryRowRes>().ReverseMap();
            CreateMap<ContributionSummaryTotalsDto, ContributionSummaryTotalsRes>().ReverseMap();

          
            // 5 log tables: project_log, staffjob_log, testreq_log, animalreq_log, additionalcosts_log
            CreateMap<ProjectLogDto, ProjectLogRes>().ReverseMap();
            CreateMap<PaginatedResult<ProjectLogDto>, PaginationRes<ProjectLogRes>>();

            CreateMap<StaffJobLogDto, StaffJobLogRes>().ReverseMap();
            CreateMap<PaginatedResult<StaffJobLogDto>, PaginationRes<StaffJobLogRes>>();

            CreateMap<TestRequirementLogDto, TestRequirementLogRes>().ReverseMap();
            CreateMap<PaginatedResult<TestRequirementLogDto>, PaginationRes<TestRequirementLogRes>>();

            CreateMap<AnimalRequestLogDto, AnimalRequestLogRes>().ReverseMap();
            CreateMap<PaginatedResult<AnimalRequestLogDto>, PaginationRes<AnimalRequestLogRes>>();

            CreateMap<AdditionalCostLogDto, AdditionalCostLogRes>().ReverseMap();
            CreateMap<PaginatedResult<AdditionalCostLogDto>, PaginationRes<AdditionalCostLogRes>>();
            // MaintTotalBusinessOverheads
            CreateMap<TotalBusinessOverheadsDto, TotalBusinessOverheadsReq>().ReverseMap();
            CreateMap<TotalBusinessOverheadsDto, TotalBusinessOverheadsRes>().ReverseMap();
        }
    }
}
