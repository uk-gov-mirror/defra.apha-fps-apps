using Apha.Common.Contracts.PACT;
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
            CreateMap<Owner, OwnerDto>().ReverseMap();
            CreateMap<WorkGroupView, WorkGroupViewDto>();
            CreateMap<Month, MonthDto>().ReverseMap();
            CreateMap<MonthDto, MonthRes>().ReverseMap();
            CreateMap<ProjectInvoice, ProjectInvoiceDto>().ReverseMap();
            CreateMap<ProjectSubContract, ProjectSubContractDto>().ReverseMap();
            CreateMap<SubContractRmsImportRow, SubContractRmsImportRowDto>().ReverseMap();
            CreateMap<ProjectSubcontractStaging, SubContractRmsImportRowDto>();
            CreateMap<SubContractRmsImport, SubContractRmsImportDto>().ReverseMap();
            CreateMap<SubContractRmsImportResult, SubContractRmsImportResultDto>().ReverseMap();
            CreateMap<TestCapability, TestCapabilityDto>().ReverseMap();
            CreateMap<TestRequirement, TestRequirementtDto>().ReverseMap();
            CreateMap<TestRequirementDetail, TestRequirementtDto>();
            CreateMap<TestSupplierView, TestSupplierViewDto>().ReverseMap();
            CreateMap<TestorProduct, TestorProductDto>().ReverseMap();
            CreateMap<CalenderMonth, CalenderMonthDto>().ReverseMap();
            CreateMap<ProjectMonth, ProjectMonthDto>().ReverseMap();
            CreateMap<ProjectMonthFinal, ProjectMonthFinalDto>().ReverseMap();
            CreateMap<MonthlyOutputLog, MonthlyOutputLogDto>().ReverseMap();
            CreateMap<MonthlyTimeLog, MonthlyTimeLogDto>().ReverseMap();
            CreateMap<MonthlyTimeLogFilter, MonthlyTimeLogFilterDto>().ReverseMap();
            CreateMap<WorkGroupTimeCode, WorkGroupTimeCodeDto>().ReverseMap();
            CreateMap<WorkGroupValidTimeCode, WorkGroupValidTimeCodeDto>().ReverseMap();
            CreateMap<WgSummarisedStaffTimeUsageView, WgSummarisedStaffTimeUsageEntryDto>();
            CreateMap<SummarisedWgTimeView, SummarisedWgTimeDto>().ReverseMap();
            CreateMap<SummarisedWgTimeView, SummarisedWgTimeEntryDto>();
            CreateMap<SummarisedWgTimeDto, SummarisedWgTimeRes>().ReverseMap();
            CreateMap<RecreateSummaryLog, RecreateSummaryLogDto>().ReverseMap();
            CreateMap<RecreateSummaryLogWithComment, RecreateSummaryLogDto>().ReverseMap();
            CreateMap<ReleasePeriod, ReleasePeriodDto>().ReverseMap();
            CreateMap<ReleaseSummary, ReleaseSummaryDto>();
            CreateMap<JobCodeZtLookup, JobCodeZtDto>().ReverseMap();
            CreateMap<TestPriceCheckView, TestPriceCheckDto>().ReverseMap();
            CreateMap<TimePurchaseProject, TimePurchaseProjectDto>();
            CreateMap<TimeSaleProfitCentre, TimeSaleProfitCentreDto>();
            CreateMap<TestSaleSellingWorkgroup, TestSaleSellingWorkgroupDto>();
            CreateMap<TestSaleBuyingProject, TestSaleBuyingProjectDto>();
            CreateMap<WgTestCapabilitiesWithDescription, WgTestCapabilitiesWithDescriptionDto>();
            CreateMap<TestReqBreakdownView, TestReqBreakdownDto>().ReverseMap();
            CreateMap<BatchJobHistory, BatchJobHistoryDto>().ReverseMap();
            CreateMap<BatchJobQueue, BatchJobQueueDto>().ReverseMap();
            CreateMap<BatchJobQueue, BatchJobQueueRes>().ReverseMap();
            CreateMap<BatchJobQueue, BatchJobEventTriggerDto>()
                .ForMember(dest => dest.Jobqueue, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.EventId, opt => opt.Ignore());
            CreateMap<TestActualBreakdownView, TestActualBreakdownDto>().ReverseMap();

        }
    }
}