using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class CostbookApiDtoMapper : Profile
    {
        public CostbookApiDtoMapper()
        {
            CreateMap(typeof(ApiResponse<>), typeof(ApiResponseDto<>)).ReverseMap();
            CreateMap<ApiError, ApiErrorDto>().ReverseMap();
            CreateMap<ApiMeta, ApiMetaDto>().ReverseMap();

            // ── Existing project mappings ─────────────────────────────────────
            CreateMap<ProjectDto, ProjectRes>().ReverseMap();
            CreateMap<ProjectDto, ProjectReq>().ReverseMap();
            CreateMap<CustomerDto, CustomerRes>().ReverseMap();
            CreateMap<DiseaseDto, DiseaseRes>().ReverseMap();
            CreateMap<Application.Dtos.CostBook.ProgramDto, Common.Contracts.Costbook.ProgramRes>().ReverseMap();
            CreateMap<StaffDto, StaffRes>().ReverseMap();
            CreateMap<ContractDto, ContractRes>().ReverseMap();
            CreateMap<ProjectEditDataDto, ProjectEditRes>().ReverseMap();

            // ── Yearly details: Res/Req ↔ Dto (used by CostBookYearlyDetailsApiClient) ──
            CreateMap<ProjectHeaderRes, ProjectHeaderDto>().ReverseMap();
            CreateMap<ProjectYearRes, ProjectYearDto>().ReverseMap();
            CreateMap<ProjectYearDto, ProjectYearReq>().ReverseMap();
            CreateMap<StaffRequirementRes, StaffRequirementDto>().ReverseMap();
            CreateMap<StaffRequirementDto, StaffRequirementReq>().ReverseMap();
            CreateMap<TestRequirementRes, TestRequirementDto>().ReverseMap();
            CreateMap<TestRequirementDto, TestRequirementReq>().ReverseMap();
            CreateMap<AnimalRequirementRes, AnimalRequirementDto>().ReverseMap();
            CreateMap<AnimalRequirementDto, AnimalRequirementReq>().ReverseMap();
            CreateMap<AdditionalCostRes, AdditionalCostDto>().ReverseMap();
            CreateMap<AdditionalCostDto, AdditionalCostReq>().ReverseMap();
            CreateMap<PayRateRes, PayRateDto>().ReverseMap();
            CreateMap<AnimalRateRes, AnimalRateDto>().ReverseMap();
            CreateMap<AccountCategoryRes, AccountCategoryDto>().ReverseMap();
            CreateMap<TestCodeLookupRes, TestCodeLookupDto>().ReverseMap();
            CreateMap<AnimalLookupRes, AnimalLookupDto>().ReverseMap();

            CreateMap<StaffYearsRowRes, StaffYearsRowDto>().ReverseMap();
            CreateMap<StaffYearsPivotRes, StaffYearsPivotDto>().ReverseMap();
            CreateMap<StaffEffortRowRes, StaffEffortRowDto>().ReverseMap();
            CreateMap<StaffEffortPivotRes, StaffEffortPivotDto>().ReverseMap();
            CreateMap<ProjectCostsRowRes, ProjectCostsRowDto>().ReverseMap();
            CreateMap<ProjectCostsPivotRes, ProjectCostsPivotDto>().ReverseMap();
            CreateMap<ProjectYearCostSummaryRes, ProjectYearCostSummaryDto>().ReverseMap();

            
            CreateMap<MaintenanceSettingsRes, MaintenanceSettingsDto>().ReverseMap();
            CreateMap<MaintenanceSettingsDto, MaintenanceSettingsReq>().ReverseMap();
            CreateMap<StaffRes, StaffDto>().ReverseMap();
            CreateMap<StaffDto, StaffReq>().ReverseMap();
            CreateMap<AccountGroupRes, AccountGroupDto>().ReverseMap();
            CreateMap<AccountGroupDto, AccountGroupReq>().ReverseMap();
            CreateMap<AccountCategoryMaintenanceRes, AccountCategoryMaintenanceDto>().ReverseMap();
            CreateMap<AccountCategoryMaintenanceDto, AccountCategoryMaintenanceReq>().ReverseMap();
        }
    }
}
