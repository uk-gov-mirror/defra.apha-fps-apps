using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class PimsApiDtoMapper : Profile
    {
        public PimsApiDtoMapper()
        {
            CreateMap(typeof(ApiResponseDto<>), typeof(ApiResponse<>)).ReverseMap();
            CreateMap<ApiErrorDto, ApiError>().ReverseMap();
            CreateMap<ApiMetaDto, ApiMeta>().ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();
            CreateMap<PaginationDto, Pagination>().ReverseMap();

            // Project List
            CreateMap<ProjectListRes, ProjectListViewDto>().ReverseMap();
            CreateMap<ProjectListMilestoneRes, ProjectListMilestoneDto>().ReverseMap();
            CreateMap<ProjectDetailsMilestoneRes, ProjectDetailsMilestoneDto>().ReverseMap();

            // FPS Project Details (read-only)
            CreateMap<ProjectRes, ProjectDto>().ReverseMap();

            // Proposed Project
            CreateMap<ProposedProjectRes, ProposedProjectDto>().ReverseMap();
            CreateMap<ProposedProjectDto, ProposedProjectReq>().ReverseMap();

            // FPS Yearly Details
            CreateMap<ProjectsRes, ProjectsDto>().ReverseMap();

            // Comments
            CreateMap<CommentRes, CommentDto>().ReverseMap();
            CreateMap<CommentDto, CommentReq>().ReverseMap();

            // PIMS Project Detail
            CreateMap<ProjectDetailRes, ProjectDetailDto>().ReverseMap();
            CreateMap<ProjectDetailDto, ProjectDetailReq>().ReverseMap();

            // Comment Topics
            CreateMap<CommentTopicRes, CommentTopicDto>().ReverseMap();

            // Risk
            CreateMap<RiskRes, RiskDto>().ReverseMap();

            // Year
            CreateMap<YearRes, YearDto>().ReverseMap();

            // Additional Cost
            CreateMap<AdditionalCostRes, AdditionalCostDto>().ReverseMap();

            // Animal Cost
            CreateMap<AnimalCostRes, AnimalCostDto>().ReverseMap();

            // Test Cost
            CreateMap<TestCostRes, TestCostDto>().ReverseMap();

            // Staff Cost
            CreateMap<StaffCostRes, StaffCostDto>().ReverseMap();

            // Project Year Details
            CreateMap<ProjectYearDetailsRes, ProjectYearDetailsDto>().ReverseMap();

            // Pact Pay
            CreateMap<PactPayRes, PactPayDto>().ReverseMap();

            // Monthly Pact Data
            CreateMap<MonthlyPactRes, MonthlyPactDto>().ReverseMap();

            // FPS Year Totals
            CreateMap<FpsYearTotalsRes, FpsYearTotalsDto>().ReverseMap();

            // Milestones
            CreateMap<MilestoneRes, MilestoneDto>().ReverseMap();
            CreateMap<MilestoneDto, MilestoneReq>().ReverseMap();
            CreateMap<MilestoneTypeRes, MilestoneTypeDto>().ReverseMap();

            CreateMap<MilestoneFormDatesRes, MilestoneFormDatesDto>().ReverseMap();
            CreateMap<MilestoneFormDatesDto, MilestoneFormDatesReq>().ReverseMap();

            CreateMap<LogMilestoneRes, LogMilestoneDto>().ReverseMap();
            CreateMap<RadTrackInvoiceRes, RadTrackInvoiceDto>().ReverseMap();
            CreateMap<RadTrackInvoiceDto, RadTrackInvoiceReq>().ReverseMap();

            // Staging Milestone
            CreateMap<StagingMilestoneRes, StagingMilestoneDto>().ReverseMap();
            CreateMap<StagingMilestoneDto, StagingMilestoneReq>().ReverseMap();

            CreateMap<YearlyFinancialDataRes, YearlyFinancialDataDto>().ReverseMap();
            CreateMap<YearlyFinancialDataDto, YearlyFinancialDataReq>().ReverseMap();
            CreateMap<PactProjectYearCostsRes, PactProjectYearCostsDto>().ReverseMap();

            // Project Year Manager
            CreateMap<ProjectYearManagerRes, ProjectYearManagerDto>().ReverseMap();
        }
    }
}
