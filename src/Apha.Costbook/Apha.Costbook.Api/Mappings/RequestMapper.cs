using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess;
using AutoMapper;

namespace Apha.Costbook.Api.Mappings;

public class RequestMapper : Profile
{
    public RequestMapper()
    {
        // ── Pagination ────────────────────────────────────────────────────────
        CreateMap(typeof(PaginationReq<>),       typeof(QueryParameters<>)).ReverseMap();
        CreateMap(typeof(PaginationRes<>),        typeof(PaginatedResult<>)).ReverseMap();
        CreateMap(typeof(QueryParameters<>),      typeof(PaginationParameters<>)).ReverseMap();
        CreateMap(typeof(PagedData<>),            typeof(PaginatedResult<>)).ReverseMap();
        CreateMap<Pagination,     PaginationDto>().ReverseMap();
        CreateMap<PaginationData, PaginationDto>().ReverseMap();

        // ── Project entity ↔ Dto/Res/Req ─────────────────────────────────────
        CreateMap<Project, ProjectDto>().ReverseMap();
        CreateMap<Project, ProjectHeaderDto>()
            .ForMember(dest => dest.EuroConvRate, opt => opt.MapFrom(src => src.Euroconvrate));
        CreateMap<ProjectDto, ProjectRes>().ReverseMap();
        CreateMap<ProjectDto, ProjectReq>().ReverseMap();

        // ── Lookup entities ───────────────────────────────────────────────────
        CreateMap<Customer,    CustomerDto>().ReverseMap();
        CreateMap<Disease,     DiseaseDto>().ReverseMap();
        CreateMap<Program,     ProgramDto>().ReverseMap();
        CreateMap<Staff,       StaffDto>().ReverseMap();
        CreateMap<CustomerDto, CustomerRes>().ReverseMap();
        CreateMap<DiseaseDto,  DiseaseRes>().ReverseMap();
        CreateMap<ProgramDto,  ProgramRes>().ReverseMap();
        CreateMap<StaffDto,    StaffRes>().ReverseMap();

        // ── Yearly details: entity ↔ Dto ─────────────────────────────────────
        CreateMap<ProjectYear,        ProjectYearDto>().ReverseMap();
        CreateMap<StaffRequirement,   StaffRequirementDto>().ReverseMap();
        CreateMap<TestRequirement,    TestRequirementDto>().ReverseMap();
        CreateMap<AnimalRequirement,  AnimalRequirementDto>().ReverseMap();
        CreateMap<AdditionalCost,     AdditionalCostDto>().ReverseMap();

        // ── Yearly details: Dto ↔ Res/Req ────────────────────────────────────
        CreateMap<ProjectHeaderDto,       ProjectHeaderRes>().ReverseMap();
        CreateMap<ProjectYearDto,         ProjectYearRes>().ReverseMap();
        CreateMap<ProjectYearDto,         ProjectYearReq>().ReverseMap();
        CreateMap<AddProjectYearReq,      ProjectYearDto>()
            .ForMember(dest => dest.YearValue, opt => opt.MapFrom(src => src.Year));
        CreateMap<StaffRequirementDto,    StaffRequirementRes>().ReverseMap();
        CreateMap<StaffRequirementDto,    StaffRequirementReq>().ReverseMap();
        CreateMap<TestRequirementDto,     TestRequirementRes>().ReverseMap();
        CreateMap<TestRequirementDto,     TestRequirementReq>().ReverseMap();
        CreateMap<AnimalRequirementDto,   AnimalRequirementRes>().ReverseMap();
        CreateMap<AnimalRequirementDto,   AnimalRequirementReq>().ReverseMap();
        CreateMap<AdditionalCostDto,      AdditionalCostRes>().ReverseMap();
        CreateMap<AdditionalCostDto,      AdditionalCostReq>().ReverseMap();
        CreateMap<PayRateDto,             PayRateRes>().ReverseMap();
        CreateMap<AnimalRateDto,          AnimalRateRes>().ReverseMap();
        CreateMap<AccountCategoryDto,     AccountCategoryRes>().ReverseMap();
        CreateMap<TestCodeLookupDto,       TestCodeLookupRes>().ReverseMap();
        CreateMap<AnimalLookupDto,         AnimalLookupRes>().ReverseMap();

        CreateMap<StaffYearsRowDto, StaffYearsRowRes>().ReverseMap();
        CreateMap<StaffYearsPivotDto, StaffYearsPivotRes>().ReverseMap();
        CreateMap<StaffEffortRowDto, StaffEffortRowRes>().ReverseMap();
        CreateMap<StaffEffortPivotDto, StaffEffortPivotRes>().ReverseMap();
        CreateMap<ProjectCostsRowDto, ProjectCostsRowRes>().ReverseMap();
        CreateMap<ProjectCostsPivotDto, ProjectCostsPivotRes>().ReverseMap();
        CreateMap<ProjectYearCostSummaryDto, ProjectYearCostSummaryRes>().ReverseMap();

        // ── Maintenance: CapsStaff (Tab 5) ───────────────────────────────────────
        CreateMap<StaffDto, StaffRes>().ReverseMap();
        CreateMap<StaffDto, StaffReq>().ReverseMap();

        // ── Maintenance: AccountGroup / CSG7 (Tab 3) ────────────────────────────
        CreateMap<AccountGroupDto, AccountGroupRes>().ReverseMap();
        CreateMap<AccountGroupDto, AccountGroupReq>().ReverseMap();

        // ── Maintenance: Settings (Tabs 1 + 4) ──────────────────────────────────
        CreateMap<MaintenanceSettingsDto, MaintenanceSettingsRes>().ReverseMap();
        CreateMap<MaintenanceSettingsDto, MaintenanceSettingsReq>().ReverseMap();

        // ── Maintenance: AccountCategory (Tab 2) ─────────────────────────────────
        CreateMap<AccountCategoryMaintenanceDto, AccountCategoryMaintenanceRes>().ReverseMap();
        CreateMap<AccountCategoryMaintenanceDto, AccountCategoryMaintenanceReq>().ReverseMap();

    }
}
