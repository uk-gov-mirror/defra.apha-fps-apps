using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;

using Apha.FPSApps.Application.Pagination;
using AutoMapper;
namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class FpsApiDtoMapper : Profile
    {
        public FpsApiDtoMapper()
        {
            CreateMap(typeof(ApiResponseDto<>), typeof(ApiResponse<>)).ReverseMap();
            CreateMap<ApiErrorDto, ApiError>().ReverseMap();
            CreateMap<ApiMetaDto, ApiMeta>().ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();
            CreateMap<PaginationDto, Pagination>().ReverseMap();

            CreateMap<StaffJobViewDto, StaffJobViewRes>().ReverseMap();
            CreateMap<StaffJobZtViewDto, StaffJobZtViewRes>().ReverseMap();
            CreateMap<StaffWorkgroupLookupDto, StaffWorkgroupLookupRes>().ReverseMap();
            CreateMap<StaffJobDto, StaffJobReq>().ReverseMap();
            CreateMap<StaffJobDto, StaffJobRes>().ReverseMap();
            CreateMap<ProgramDto, ProgramReq>().ReverseMap();
            CreateMap<ProgramDto, ProgramRes>().ReverseMap();
            CreateMap<ManagerDto, ManagerRes>().ReverseMap();
            CreateMap<EmployeeDto, EmployeeReq>().ReverseMap();
            CreateMap<EmployeeDto, EmployeeRes>().ReverseMap();

            // FPS Project
            // CustIncome in the FPS API wire format lives in ProjectReq.BudgetExt (see FPS RequestMapper)
            CreateMap<ProjectDto, ProjectReq>()
                .ForMember(d => d.BudgetExt, o => o.MapFrom(s => s.CustIncome))
                .ReverseMap()
                .ForMember(d => d.CustIncome, o => o.MapFrom(s => s.BudgetExt));
            CreateMap<ProjectDto, ProjectRes>().ReverseMap();
            
            // FPS Lookups
            CreateMap<StatusDto, StatusRes>().ReverseMap();
            CreateMap<DiseaseDto, DiseaseRes>().ReverseMap();
            CreateMap<CustomerDto, CustomerRes>().ReverseMap();
            CreateMap<ContractDto, ContractRes>().ReverseMap();
            CreateMap<ProjectGroupDto, ProjectGroupRes>().ReverseMap();
            
            // FPS Animal Plan
            CreateMap<AnimalCostViewDto, AnimalCostViewRes>().ReverseMap();
            CreateMap<AnimalDto, AnimalRes>().ReverseMap();
            CreateMap<AnimalRequestDto, AnimalRequestReq>().ReverseMap();
            CreateMap<AnimalRequestDto, AnimalRequestRes>().ReverseMap();
            
            // FPS Animal Master
            CreateMap<AnimalDto, AnimalReq>().ReverseMap();
            
            // YEar Master
            CreateMap<YearMasterDto, YearMasterRes>().ReverseMap();
            CreateMap<YearMasterDto, YearMasterReq>().ReverseMap();

            // Testor Product
            CreateMap<TestorProductDto, Apha.Common.Contracts.FPS.TestorProductRes>().ReverseMap();
            
            // View Project Plan vs Actual Staff
            CreateMap<TimeCostCalcsViewDto, TimeCostCalcsViewRes>().ReverseMap();
            CreateMap<TimeCostCalcsTotalsDto, TimeCostCalcsTotalsRes>().ReverseMap();
            
            // Division
            CreateMap<DivisionDto, DivisionRes>().ReverseMap();
            CreateMap<DivisionDto, DivisionReq>().ReverseMap();

            // Division Grade
            CreateMap<DivisionGradeDto, DivisionGradeRes>().ReverseMap();
            CreateMap<DivisionGradeDto, DivisionGradeReq>().ReverseMap();

            // Grade CRUD: maps frontend GradeDto to/from backend GradeReq (POST/PUT) and GradeRes (GET/POST/PUT responses)
            // Grade Mappings
            CreateMap<GradeDto, GradeReq>().ReverseMap();
            CreateMap<GradeDto, GradeRes>().ReverseMap();


            // Agency
            CreateMap<AgencyDto, AgencyRes>().ReverseMap();

            // Additional Cost
            CreateMap<AdditionalCostDto, AdditionalCostReq>().ReverseMap();
            CreateMap<AdditionalCostDto, AdditionalCostRes>().ReverseMap();
            CreateMap<AccountCategoryDto, AccountCategoryRes>().ReverseMap();
            CreateMap<AccountCategoryDto, AccountCategoryReq>().ReverseMap();

            // View Project Plan vs Actual Tests
            CreateMap<MonthlyOutputDto, MonthlyOutputRes>().ReverseMap();

            // ProgrammeNewProject (merged into ProjectDto - mappings above)
            CreateMap<AccountCodeDto, AccountCodeRes>().ReverseMap();
            CreateMap<SubAccountDto, SubAccountRes>()
                .ForMember(d => d.SubAccount, o => o.MapFrom(s => s.SubAccount)).ReverseMap();
            CreateMap<CostCentreWorkgroupDto, CostCentreWorkgroupRes>().ReverseMap();
            CreateMap<PactStaffDto, PactStaffRes>().ReverseMap();
            CreateMap<WorkGroupPersonDto, WorkGroupPersonRes>().ReverseMap();

            // Resource Set-Up
            CreateMap<ProfitCentreDto, ProfitCentreRes>().ReverseMap();
            CreateMap<ProfitCentreDto, ProfitCentreReq>().ReverseMap();
            CreateMap<ProfitCentreCostDto, ProfitCentreCostRes>().ReverseMap();
            CreateMap<ProfitCentreGradeDto, ProfitCentreGradeRes>().ReverseMap();
            CreateMap<ProfitCentreGradeDto, ProfitCentreGradeReq>().ReverseMap();
            CreateMap<WorkgroupGradeDto, WorkgroupGradeRes>().ReverseMap();
            CreateMap<WorkGroupEmployeeDto, WorkGroupEmployeeReq>().ReverseMap();
            CreateMap<WorkGroupEmployeeDto, WorkGroupEmployeeRes>().ReverseMap();
            CreateMap<WorkGroupEmployeeStaffDto, WorkGroupEmployeeReq>().ReverseMap();
            CreateMap<WorkGroupEmployeeStaffDto, WorkGroupEmployeeRes>().ReverseMap();

            // ProjectProfitability
            CreateMap<ProjectProfitabilityDto, ProjectProfitabilityRes>().ReverseMap();

            // ProjectProfitabilityVla
            //   ForMember(Id) handles int->int? coercion: Id=GetValueOrDefault(0) on reverse.
            //   TotalCount is on Res only; silently ignored in Res->Dto direction (see DEFERRED note above).
            CreateMap<ProjectProfitabilityVlaDto, ProjectProfitabilityVlaRes>()
                .ForMember(d => d.Project, o => o.MapFrom(s => s.JobCode))
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id.GetValueOrDefault(0)))
                .ReverseMap()
                .ForMember(d => d.JobCode, o => o.MapFrom(s => s.Project))
                .ForMember(d => d.Id, o => o.MapFrom(s => (int?)s.Id));


            // Staff Plan view
            CreateMap<ProjectStaffPlanViewDto, ProjectStaffPlanViewRes>().ReverseMap();

            // Project Group Staff Plan view
            CreateMap<ProjectGroupStaffPlanViewDto, ProjectGroupStaffPlanViewRes>().ReverseMap();

            CreateMap<PactStaffDto,PactStaffRes>().ReverseMap();

            // WorkgroupGrade  
            CreateMap<WorkgroupGradeDto, WorkgroupGradeReq>().ReverseMap();

            // Job Code (ZT lookup) - now served from PACT API
            CreateMap<FpsJobCodeZtDto, Apha.Common.Contracts.PACT.JobCodeZtRes>().ReverseMap();
                      

            // Income/Contribution from Time Sales (frmTimeSellerPC)
            CreateMap<ContributionSummaryRowDto, ContributionSummaryRowRes>().ReverseMap();
            CreateMap<ContributionSummaryTotalsDto, ContributionSummaryTotalsRes>().ReverseMap();

            // BudgetResourceLevel
            CreateMap<BidDto, BidReq>().ReverseMap();
            CreateMap<BidDto, BidRes>().ReverseMap();
            CreateMap<BidViewDto, BidViewRes>().ReverseMap();
            CreateMap<PurchaseDto, PurchaseReq>().ReverseMap();
            CreateMap<PurchaseDto, PurchaseRes>().ReverseMap();

            // Audit logs are read-only so no .ReverseMap() — frontend never writes back to backend audit tables.
            CreateMap<ProjectLogRes, ProjectLogDto>()
                .ForMember(d => d.CaseWorkSub, o => o.MapFrom(s => s.CaseworkSub))
                .ForMember(d => d.PlanCaseWorkDebit, o => o.MapFrom(s => s.PlanCaseworkDebit));

            // StaffJobLog: Res.Name (staff display name resolved server-side) maps to Dto.Name by convention.
            CreateMap<StaffJobLogRes, StaffJobLogDto>();

            // TestRequirementLog: type-coercion — Res.UnitPrice is double? but Dto.UnitPrice is decimal?;
            //   Res.NoRequired is int? but Dto.NoRequired is double?. Explicit ForMember casts applied.
            CreateMap<TestRequirementLogRes, TestRequirementLogDto>()
                .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.UnitPrice.HasValue ? (decimal?)Convert.ToDecimal(s.UnitPrice.Value) : null))
                .ForMember(d => d.NoRequired, o => o.MapFrom(s => s.NoRequired.HasValue ? (double?)Convert.ToDouble(s.NoRequired.Value) : null));

            // AnimalRequestLog: all property names and types align — convention mapping suffices.
            CreateMap<AnimalRequestLogRes, AnimalRequestLogDto>();

            // AdditionalCostLog: all property names and types align — convention mapping suffices.
            CreateMap<AdditionalCostLogRes, AdditionalCostLogDto>();
            // UserPermission
            CreateMap<UserDto, UserRes>().ReverseMap();
            CreateMap<UserDto, UserReq>().ReverseMap();
            CreateMap<UserPermissionDataDto, UserPermissionRes>().ReverseMap();
            CreateMap<UserPermissionDataDto, UserPermissionReq>().ReverseMap();
            CreateMap<PermissionOptionsDto, PermissionOptionsRes>().ReverseMap();

            // Total Business Overheads
            CreateMap<TotalBusinessOverheadsDto, TotalBusinessOverheadsReq>().ReverseMap();
            CreateMap<TotalBusinessOverheadsDto, TotalBusinessOverheadsRes>().ReverseMap();

        }
    }
}

