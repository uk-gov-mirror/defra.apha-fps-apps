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
            CreateMap<WorkGroupViewDto, WorkGroupViewRes>().ReverseMap();

            // WorkGroup Maintenance (CRUD + lookups)
            CreateMap<WorkGroupDto, WorkGroupMaintenanceRes>().ReverseMap();
            CreateMap<WorkGroupDto, WorkGroupMaintenanceReq>().ReverseMap();
            CreateMap<OwnerDto, OwnerRes>().ReverseMap();

            CreateMap<MonthDto, MonthRes>().ReverseMap();
            CreateMap<CalenderMonthDto, CalenderMonthRes>().ReverseMap();
            CreateMap<ProjectInvoiceDto, ProjectInvoiceReq>().ReverseMap();
            CreateMap<ProjectInvoiceDto, ProjectInvoiceRes>().ReverseMap();
            CreateMap<MonthlyInvoicesSummaryItemDto, MonthlyInvoicesSummaryItemRes>().ReverseMap();
            CreateMap<PaginationDto, Pagination>().ReverseMap();
            CreateMap<MonthlyInvoicesPivotDto, MonthlyInvoicesPivotRes>().ReverseMap();
            CreateMap<ProjectSubContractDto, ProjectSubContractReq>().ReverseMap();
            CreateMap<ProjectSubContractDto, ProjectSubContractRes>().ReverseMap();
            CreateMap<SubContractRmsImportRowDto, SubContractRmsImportRowReq>().ReverseMap();
            CreateMap<SubContractRmsImportRowDto, SubContractRmsImportRowRes>().ReverseMap();
            CreateMap<SubContractRmsImportReqDto, SubContractRmsImportReq>().ReverseMap();
            CreateMap<SubContractRmsImportResultDto, SubContractRmsImportRes>().ReverseMap();
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
            CreateMap<MonthlyOutputRes, PactMonthlyOutputDto>().ReverseMap();
            CreateMap<PactMonthlyOutputDto, MonthlyOutputReq>();
            CreateMap<MonthlyOutputImportReqDto, MonthlyOutputImportReq>();
            CreateMap<MonthlyOutputImportRowDto, MonthlyOutputImportRowReq>();
            CreateMap<MonthlyOutputImportRes, MonthlyOutputImportResultDto>();
            CreateMap<StagingMonthlyOutputRes, StagingMonthlyOutputDto>().ReverseMap();
            CreateMap<StagingMonthlyOutputDto, StagingMonthlyOutputReq>().ReverseMap();
            CreateMap<MonthlyOutputValidateRes, MonthlyOutputValidateResultDto>().ReverseMap();
            CreateMap<MonthlyOutputMakeLiveRes, MonthlyOutputMakeLiveResultDto>().ReverseMap();
            CreateMap<MonthlyTimeDto, MonthlyTimeReq>().ReverseMap();
            CreateMap<MonthlyTimeDto, MonthlyTimeRes>().ReverseMap();
            CreateMap<StagingMonthlyTimeDto, StagingMonthlyTimeReq>().ReverseMap();
            CreateMap<StagingMonthlyTimeDto, StagingMonthlyTimeRes>().ReverseMap();
            CreateMap<BulkUpdateStagingMonthlyTimeNamesDto, BulkUpdateStagingMonthlyTimeNamesReq>().ReverseMap();
            CreateMap<BulkUpdateStagingMonthlyTimeNamesResultDto, BulkUpdateStagingMonthlyTimeNamesRes>().ReverseMap();
            CreateMap<MonthlyTimeImportRowDto, MonthlyTimeImportRowReq>().ReverseMap();
            CreateMap<MonthlyTimeImportRowDto, MonthlyTimeImportRowRes>().ReverseMap();
            CreateMap<MonthlyTimeImportReqDto, MonthlyTimeImportReq>().ReverseMap();
            CreateMap<MonthlyTimeImportResultDto, MonthlyTimeImportRes>().ReverseMap();
            CreateMap<MonthlyTimeValidateResultDto, MonthlyTimeValidateRes>().ReverseMap();
            CreateMap<MonthlyTimeMakeLiveResultDto, MonthlyTimeMakeLiveRes>().ReverseMap();
            CreateMap<MonthlyTimeLogDto, MonthlyTimeLogRes>().ReverseMap();
            CreateMap<CalenderMonthDto, CalenderMonthRes>().ReverseMap();
            CreateMap<WorkGroupTimeCodeDto, WorkGroupTimeCodeRes>().ReverseMap();
            CreateMap<WorkGroupValidTimeCodeDto, WorkGroupValidTimeCodeRes>().ReverseMap();
            CreateMap<WgSummarisedStaffTimeUsageRowDto, WgSummarisedStaffTimeUsageRowRes>().ReverseMap();
            CreateMap<WgSummarisedStaffTimeUsageSummaryDto, WgSummarisedStaffTimeUsageSummaryRes>().ReverseMap();
            CreateMap<JobTitleLookupItemDto, JobTitleLookupItemRes>().ReverseMap();
            CreateMap<WgSummarisedStaffTimeUsageDto, WgSummarisedStaffTimeUsageRes>().ReverseMap();
            CreateMap<SummarisedWgTimeDto, SummarisedWgTimeRes>().ReverseMap();
            CreateMap<SummarisedWgTimeSummaryDto, SummarisedWgTimeSummaryRes>().ReverseMap();
            CreateMap<SummarisedWgTimeViewDto, SummarisedWgTimePivotRes>().ReverseMap();
            CreateMap<ProjectTitleLookupRes, SummarisedWgTimeProjectTitleLookupItem>().ReverseMap();
            CreateMap<ApiResponse<SummarisedWgTimePivotRes>, ApiResponseDto<SummarisedWgTimeViewDto>>();
            CreateMap<WorkGroupReportEmailResultDto, WorkGroupReportEmailResultRes>().ReverseMap();
            CreateMap<TestSupplierViewDto, TestSupplierViewRes>().ReverseMap();
            CreateMap<RecreateSummaryLogDto, RecreateSummaryLogRes>().ReverseMap();
            CreateMap<ReleasePeriodDto, ReleasePeriodRes>().ReverseMap();
            CreateMap<ReleaseSummaryRes, ReleaseSummaryDto>();
            CreateMap<TestPriceCheckDto, TestPriceCheckRes>().ReverseMap();
            CreateMap<TestPriceCheckDto, TestPriceCheckReq>();
            CreateMap<TimePurchaseProjectDto, TimePurchaseProjectRes>().ReverseMap();
            CreateMap<TimeSaleProfitCentreDto, TimeSaleProfitCentreRes>().ReverseMap();
            CreateMap<TimeSaleWorkGroupDto, TimeSaleWorkGroupRes>().ReverseMap();
            CreateMap<TestSaleSellingWorkgroupDto, TestSaleSellingWorkgroupRes>().ReverseMap();
            CreateMap<TestSaleBuyingProjectDto, TestSaleBuyingProjectRes>().ReverseMap();
            CreateMap<WgTestCapabilitiesWithDescriptionDto, WgTestCapabilitiesWithDescriptionRes>().ReverseMap();
            CreateMap<TestReqBreakdownRes, TestReqBreakdownDto>().ReverseMap();
            CreateMap<BatchJobQueueDto, BatchJobQueueRes>().ReverseMap();
            CreateMap<BatchJobHistoryDto, BatchJobHistoryRes>().ReverseMap();
            CreateMap<BatchJobEventTriggerDto, BatchJobEventTriggerRes>().ReverseMap();
            CreateMap<TestActualBreakdownRes, TestActualBreakdownDto>().ReverseMap();
            CreateMap<TestPlanCostBreakdownRes, TestPlanCostBreakdownDto>().ReverseMap();
        }
    }
}
