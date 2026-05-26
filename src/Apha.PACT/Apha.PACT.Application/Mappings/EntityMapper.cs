using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;
using AutoMapper;

namespace Apha.PACT.Application.Mappings
{
    public class EntityMapper : Profile
    {
        public EntityMapper()
        {
            CreateMap(typeof(PaginationParameters<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PagedData<>), typeof(PaginatedResult<>)).ReverseMap();

            CreateMap<PaginationData, PaginationDto>().ReverseMap();

            CreateMap<JobCode, JobCodeDto>().ReverseMap();
            CreateMap<TimeCodeValid, TimeCodeValidDto>().ReverseMap();
            CreateMap<WorkGroup, WorkGroupDto>().ReverseMap();
            CreateMap<Month, MonthDto>().ReverseMap();
            CreateMap<ProjectInvoice, ProjectInvoiceDto>().ReverseMap();
            CreateMap<ProjectSubContract, ProjectSubContractDto>().ReverseMap();
            CreateMap<TestCapability, TestCapabilityDto>().ReverseMap();
            CreateMap<TestRequirement, TestRequirementtDto>().ReverseMap();
            CreateMap<TestRequirementDetail, TestRequirementtDto>();
            CreateMap<TestorProduct, TestorProductDto>().ReverseMap();
            CreateMap<Month, MonthDto>().ReverseMap();
            CreateMap<ProjectMonth, ProjectMonthDto>().ReverseMap();
            CreateMap<ProjectMonthFinal, ProjectMonthFinalDto>().ReverseMap();
            CreateMap<MonthlyOutputLog, MonthlyOutputLogDto>().ReverseMap();
            CreateMap<CalenderMonth, CalenderMonthDto>().ReverseMap();
            CreateMap<WorkGroupTimeCode, WorkGroupTimeCodeDto>().ReverseMap();
            CreateMap<WorkGroupValidTimeCode, WorkGroupValidTimeCodeDto>().ReverseMap();
            CreateMap<MonthlyTime, MonthlyTimeDto>().ReverseMap();
        }
    }
}
