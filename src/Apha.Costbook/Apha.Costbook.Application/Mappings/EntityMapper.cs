using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess;
using AutoMapper;

namespace Apha.Costbook.Application.Mappings
{
    public class EntityMapper : Profile
    {
        public EntityMapper()
        {
            CreateMap(typeof(PaginationParameters<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PagedData<>), typeof(PaginatedResult<>)).ReverseMap();

            CreateMap<PaginationData, PaginationDto>().ReverseMap();
			CreateMap<Project, ProjectDto>().ReverseMap();
            CreateMap<Program, ProgramDto>().ReverseMap();
            CreateMap<Customer, CustomerDto>().ReverseMap();
            CreateMap<Disease, DiseaseDto>().ReverseMap();
            CreateMap<Staff, StaffDto>().ReverseMap();
            CreateMap<AccountGroup, AccountGroupDto>().ReverseMap();
            CreateMap<FpsAccountCategory, AccountCategoryMaintenanceDto>()
                .ForMember(dest => dest.FpsYear, opt => opt.MapFrom(src => src.FpsYear ?? 0));
           
            CreateMap<AccountCategoryMaintenanceDto, FpsAccountCategory>()
                .ForMember(dest => dest.FpsYear, opt => opt.MapFrom(src => (int?)src.FpsYear));
        }
    }
}
