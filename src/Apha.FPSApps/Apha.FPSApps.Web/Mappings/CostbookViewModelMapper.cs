using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;

namespace Apha.FPSApps.Web.Mappings
{
    public class CostbookViewModelMapper : Profile
    {
        public CostbookViewModelMapper()
        {

            CreateMap<PaginationDto, PaginationModel>().ReverseMap();

            // ── Existing project view model mappings ──────────────────────────
            CreateMap<ProjectDto, ProjectItemViewModel>().ReverseMap();
            CreateMap<ProjectDto, ProjectDetailViewModel>().ReverseMap();
            CreateMap<ProjectDto, ProjectCreateEditViewModel>().ReverseMap();

            // ── Yearly details: Dto ↔ ViewModel/Item ─────────────────────────
            CreateMap<ProjectYearDto, ProjectYearRateItem>().ReverseMap();
            CreateMap<StaffRequirementDto, StaffRequirementItem>().ReverseMap();
            CreateMap<TestRequirementDto, TestRequirementItem>().ReverseMap();
            CreateMap<AnimalRequirementDto, AnimalRequirementItem>().ReverseMap();
            CreateMap<AdditionalCostDto, AdditionalCostItem>().ReverseMap();
            
            CreateMap<InflationSettingsItem, MaintenanceSettingsDto>().ReverseMap();            
            CreateMap<ProfitMarginsItem, MaintenanceSettingsDto>().ReverseMap();            
            CreateMap<AccountCategoryItem, AccountCategoryMaintenanceDto>().ReverseMap();
            CreateMap<Csg7GroupItem, AccountGroupDto>().ReverseMap();
            CreateMap<CapsStaffItem, StaffDto>().ReverseMap();
           
        }
    }
}
