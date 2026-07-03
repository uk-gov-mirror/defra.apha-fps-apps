/*
 * TRANSFORMENGINE MIGRATION — RequestMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - Added AnimalCostViewDto -> AsuViewRes mapping with ForMember overrides:
 *       Id        <- IndCounter
 *       Project   <- JobCode
 *       AnimalDays <- TotalDays
 *       Cost      <- AnimalCost (decimal? -> decimal with GetValueOrDefault)
 *   - Added PaginatedResult<AnimalCostViewDto> -> PaginationRes<AsuViewRes> mapping to support
 *     the GetAsuViewAsync controller action that returns PaginationRes<AsuViewRes>
 *
 * PRESERVED:
 *   - All existing CreateMap entries and their ForMember overrides
 *   - Generic pagination mappings (PaginationReq <-> QueryParameters, PaginationRes <-> PaginatedResult)
 *   - All comment blocks documenting prior phase mappings
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm Cost field mapping (AnimalCost decimal? -> decimal) is
 *     correct once DB column nullability is verified in Phase 14 security/build gate
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

            // TRANSFORMENGINE: AsuView mappings — Phase 5 (AnimalController.GetAsuViewAsync)
            //   AnimalCostViewDto fields do not align 1:1 with AsuViewRes; ForMember overrides required:
            //     Id        <- IndCounter  (view's row key)
            //     Project   <- JobCode     (display column in fps_asuview.js grid: 'project')
            //     AnimalDays <- TotalDays  (total days per project row from AnimalCostView)
            //     Cost      <- AnimalCost  (decimal? nullable unwrapped to decimal with GetValueOrDefault)
            //   AnimalType maps by convention (same property name on both types)
            CreateMap<AnimalCostViewDto, AsuViewRes>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.IndCounter))
                .ForMember(d => d.Project, o => o.MapFrom(s => s.JobCode))
                .ForMember(d => d.AnimalDays, o => o.MapFrom(s => s.TotalDays))
                .ForMember(d => d.Cost, o => o.MapFrom(s => s.AnimalCost.GetValueOrDefault()));
            CreateMap<PaginatedResult<AnimalCostViewDto>, PaginationRes<AsuViewRes>>();
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

            // TRANSFORMENGINE: VLA profitability mappings — frmJobcodeTotalsVLA Phase 5
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

          
        }
    }
}
