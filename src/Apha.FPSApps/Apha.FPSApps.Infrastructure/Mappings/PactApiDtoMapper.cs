using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class PactApiDtoMapper : Profile
    {
        public PactApiDtoMapper()
        {
            // PACT
            CreateMap<JobCodeDto, JobCodeReq>().ReverseMap();
            CreateMap<JobCodeDto, JobCodeRes>().ReverseMap();
            CreateMap<TimeCodeValidDto, TimeCodeValidReq>().ReverseMap();
            CreateMap<TimeCodeValidDto, TimeCodeValidRes>().ReverseMap();
            CreateMap<WorkGroupDto, WorkGroupRes>().ReverseMap();
            CreateMap<MonthDto, MonthRes>().ReverseMap();
            CreateMap<ProjectInvoiceDto, ProjectInvoiceReq>().ReverseMap();
            CreateMap<ProjectInvoiceDto, ProjectInvoiceRes>().ReverseMap();
            CreateMap<MonthlyInvoicesSummaryItemDto, MonthlyInvoicesSummaryItemRes>().ReverseMap();
            CreateMap<PaginationDto, Pagination>().ReverseMap();
            CreateMap<MonthlyInvoicesPivotDto, MonthlyInvoicesPivotRes>().ReverseMap();
            CreateMap<ProjectSubContractDto, ProjectSubContractReq>().ReverseMap();
            CreateMap<ProjectSubContractDto, ProjectSubContractRes>().ReverseMap();
            CreateMap<TestCapabilityDto, TestCapabilityReq>().ReverseMap();
            CreateMap<TestCapabilityDto, TestCapabilityRes>().ReverseMap();
            CreateMap<TestRequirementDto, TestRequirementReq>().ReverseMap();
            CreateMap<TestRequirementDto, TestRequirementtRes>().ReverseMap();
            CreateMap<TestorProductDto, TestorProductReq>().ReverseMap();
            CreateMap<TestorProductDto, TestorProductRes>().ReverseMap();
            CreateMap<MonthlySubContractsSummaryItemDto, MonthlySubContractsSummaryItemRes>().ReverseMap();
            CreateMap<MonthlySubContractsPivotDto, MonthlySubContractsPivotRes>().ReverseMap();     

            CreateMap<ProjectMonthDto, ProjectMonthReq>().ReverseMap();
            CreateMap<ProjectMonthDto, ProjectMonthRes>().ReverseMap();
            CreateMap<MonthDto, MonthRes>().ReverseMap();
            CreateMap<ProjectProfileDto, ProjectProfileRes>().ReverseMap();
            CreateMap<ProjectProfileCumulativeDto, ProjectProfileCumulativeRes>().ReverseMap();
            CreateMap<MonthlyOutputLogDto, MonthlyOutputLogRes>().ReverseMap();
            CreateMap<CalenderMonthDto, CalenderMonthRes>().ReverseMap();
            CreateMap<WorkGroupTimeCodeDto, WorkGroupTimeCodeRes>().ReverseMap();
            CreateMap<WorkGroupValidTimeCodeDto, WorkGroupValidTimeCodeRes>().ReverseMap();
            CreateMap<MonthlyTimeDto, MonthlyTimeReq>().ReverseMap();
            CreateMap<MonthlyTimeDto, MonthlyTimeRes>().ReverseMap();
        }
    }
}
