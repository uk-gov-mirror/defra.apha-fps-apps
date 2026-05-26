using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using AutoMapper;

namespace Apha.PACT.Api.Mappings
{
    public class RequestMapper : Profile
    {
        public RequestMapper()
        {
            CreateMap(typeof(PaginationReq<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();

            CreateMap<Pagination, PaginationDto>().ReverseMap();

            CreateMap<JobCodeReq, JobCodeDto>().ReverseMap();
            CreateMap<JobCodeRes, JobCodeDto>().ReverseMap();
            CreateMap<TimeCodeValidReq, TimeCodeValidDto>().ReverseMap();
            CreateMap<TimeCodeValidRes, TimeCodeValidDto>().ReverseMap();
            CreateMap<WorkGroupRes, WorkGroupDto>().ReverseMap();
            CreateMap<MonthRes, MonthDto>().ReverseMap();
            CreateMap<ProjectInvoiceReq, ProjectInvoiceDto>().ReverseMap();
            CreateMap<ProjectInvoiceRes, ProjectInvoiceDto>().ReverseMap();
            CreateMap<ProjectSubContractReq, ProjectSubContractDto>().ReverseMap();
            CreateMap<ProjectSubContractRes, ProjectSubContractDto>().ReverseMap();
            CreateMap<TestCapabilityReq, TestCapabilityDto>().ReverseMap();
            CreateMap<TestCapabilityRes, TestCapabilityDto>().ReverseMap();
            CreateMap<TestRequirementReq, TestRequirementtDto>().ReverseMap();
            CreateMap<TestRequirementtRes, TestRequirementtDto>().ReverseMap();            
            CreateMap<TestorProductReq, TestorProductDto>().ReverseMap();
            CreateMap<TestorProductRes, TestorProductDto>().ReverseMap();
            CreateMap<MonthlyInvoicesSummaryDto, MonthlyInvoicesSummaryItemRes>().ReverseMap();
            CreateMap<MonthlyInvoicesPivotDto, MonthlyInvoicesPivotRes>().ReverseMap();
            CreateMap<MonthlySubContractsSummaryDto, MonthlySubContractsSummaryItemRes>().ReverseMap();
            CreateMap<MonthlySubContractsPivotDto, MonthlySubContractsPivotRes>().ReverseMap();
            CreateMap<ProjectMonthReq, ProjectMonthDto>().ReverseMap();
            CreateMap<ProjectMonthRes, ProjectMonthDto>().ReverseMap();
            CreateMap<ProjectProfileDto, ProjectProfileRes>().ReverseMap();
            CreateMap<ProjectProfileCumulativeDto, ProjectProfileCumulativeRes>().ReverseMap();
            CreateMap<MonthlyOutputLogDto, MonthlyOutputLogRes>().ReverseMap();
            CreateMap<CalenderMonthRes, CalenderMonthDto>().ReverseMap();
            CreateMap<WorkGroupTimeCodeRes, WorkGroupTimeCodeDto>().ReverseMap();
            CreateMap<WorkGroupValidTimeCodeRes, WorkGroupValidTimeCodeDto>().ReverseMap();
            CreateMap<MonthlyTimeReq, MonthlyTimeDto>().ReverseMap();
            CreateMap<MonthlyTimeRes, MonthlyTimeDto>().ReverseMap();
        }
    }
}
