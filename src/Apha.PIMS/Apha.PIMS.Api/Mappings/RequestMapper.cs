using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Api.Mappings
{
    public class RequestMapper : Profile
    {
        public RequestMapper()
        {
            CreateMap(typeof(PaginationReq<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PaginationReq<>), typeof(PaginationParameters<>)).ReverseMap();
            CreateMap(typeof(PaginationRes<>), typeof(PaginatedResult<>)).ReverseMap();
            CreateMap<Pagination, PaginationDto>().ReverseMap();
            CreateMap<Pagination, PaginationData>().ReverseMap();

            CreateMap<ProjectListViewDto, ProjectListRes>().ReverseMap();
            CreateMap<ProjectListMilestoneDto, ProjectListMilestoneRes>().ReverseMap();
            CreateMap<ProjectDetailsMilestoneDto, ProjectDetailsMilestoneRes>().ReverseMap();
            CreateMap<ProjectDto, ProjectRes>().ReverseMap();
            CreateMap<ProposedProjectDto, ProposedProjectReq>().ReverseMap();
            CreateMap<ProposedProjectDto, ProposedProjectRes>().ReverseMap();
            CreateMap<ProjectsDto, ProjectsRes>().ReverseMap();

            
            CreateMap<CommentDto, CommentReq>()
                .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.CommentText))
                .ReverseMap()
                .ForMember(dest => dest.CommentText, opt => opt.MapFrom(src => src.Comment));

            
            CreateMap<CommentDto, CommentRes>()
                .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.CommentText))
                .ReverseMap()
                .ForMember(dest => dest.CommentText, opt => opt.MapFrom(src => src.Comment));

            CreateMap<ProjectDetailDto, ProjectDetailReq>().ReverseMap();
            CreateMap<ProjectDetailDto, ProjectDetailRes>().ReverseMap();
            // Risk rating lookup maintenance
            CreateMap<RiskDto, RiskReq>().ReverseMap();
            CreateMap<RiskDto, RiskRes>().ReverseMap();

            // Publication type lookup maintenance
            CreateMap<PublicationTypeDto, PublicationTypeReq>().ReverseMap();
            CreateMap<PublicationTypeDto, PublicationTypeRes>().ReverseMap();
            CreateMap<YearDto, YearRes>().ReverseMap();

            
            CreateMap<CommentTopicDto, CommentTopicRes>().ReverseMap();

            CreateMap<AdditionalCostDto, AdditionalCostRes>().ReverseMap();
            CreateMap<AnimalCostDto, AnimalCostRes>().ReverseMap();
            CreateMap<TestCostDto, TestCostRes>().ReverseMap();
            CreateMap<StaffCostDto, StaffCostRes>().ReverseMap();
            CreateMap<ProjectYearDetailsDto, ProjectYearDetailsRes>().ReverseMap();
            CreateMap<PactPayDto, PactPayRes>().ReverseMap();
            CreateMap<MonthlyPactDto, MonthlyPactRes>().ReverseMap();
            CreateMap<FpsYearTotalsDto, FpsYearTotalsRes>().ReverseMap();

            CreateMap<MilestoneDto, MilestoneRes>().ReverseMap();
            CreateMap<MilestoneDto, MilestoneReq>().ReverseMap();
            CreateMap<MilestoneTypeDto, MilestoneTypeRes>().ReverseMap();

            CreateMap<MilestoneFormDatesDto, MilestoneFormDatesReq>().ReverseMap();
            CreateMap<MilestoneFormDatesDto, MilestoneFormDatesRes>().ReverseMap();

            CreateMap<LogMilestoneDto, LogMilestoneRes>().ReverseMap();
            CreateMap<RadTrackInvoiceDto, RadTrackInvoiceReq>().ReverseMap();
            CreateMap<RadTrackInvoiceDto, RadTrackInvoiceRes>().ReverseMap();

            
            CreateMap<ReportDto, ReportReq>().ReverseMap();
            CreateMap<ReportDto, ReportRes>().ReverseMap();
            CreateMap<ReportGroupDto, ReportGroupReq>().ReverseMap();
            CreateMap<ReportGroupDto, ReportGroupRes>().ReverseMap();
            CreateMap<ReportGroupLinkDto, ReportGroupLinkReq>().ReverseMap();
            CreateMap<ReportGroupLinkDto, ReportGroupLinkRes>().ReverseMap();

           
            CreateMap<ProjectManagerDto, ProjectManagerReq>().ReverseMap();
            CreateMap<ProjectManagerDto, ProjectManagerRes>().ReverseMap();
            CreateMap<ProgramManagerLinkDto, ProgramManagerLinkReq>().ReverseMap();
            CreateMap<ProgramManagerLinkDto, ProgramManagerLinkRes>().ReverseMap();
            CreateMap<ProgramLookupDto, ProgramLookupRes>().ReverseMap();
            CreateMap<ProfitCentreLookupDto, ProfitCentreLookupRes>().ReverseMap();
            CreateMap<ProfitCentreManagerLinkDto, ProfitCentreManagerLinkReq>().ReverseMap();
            CreateMap<ProfitCentreManagerLinkDto, ProfitCentreManagerLinkRes>().ReverseMap();

            
            CreateMap<SettingDto, SettingReq>().ReverseMap();
            CreateMap<SettingDto, SettingRes>().ReverseMap();

           
            CreateMap<AccessUserDto, AccessUserReq>().ReverseMap();
            CreateMap<AccessUserDto, AccessUserRes>().ReverseMap();
            
            CreateMap<AccessLevelDto, AccessLevelRes>().ReverseMap();
            CreateMap<AccessUserLevelDto, AccessUserLevelReq>().ReverseMap();
            CreateMap<AccessUserLevelDto, AccessUserLevelRes>().ReverseMap();
            CreateMap<AccessSystemDto, AccessSystemRes>().ReverseMap();

            
            CreateMap<FrequencyDto, FrequencyReq>().ReverseMap();
            CreateMap<FrequencyDto, FrequencyRes>().ReverseMap();
            CreateMap<ReviewItemDto, ReviewItemReq>().ReverseMap();
            CreateMap<ReviewItemDto, ReviewItemRes>().ReverseMap();

            
            CreateMap<RadTrackProgDto, RadTrackProgReq>().ReverseMap();
            CreateMap<RadTrackProgDto, RadTrackProgRes>().ReverseMap();
            CreateMap<StagingMilestoneDto, StagingMilestoneReq>().ReverseMap();
            CreateMap<StagingMilestoneDto, StagingMilestoneRes>().ReverseMap();
            
            CreateMap<YearlyFinancialDataDto, YearlyFinancialDataReq>().ReverseMap();
            CreateMap<YearlyFinancialDataDto, YearlyFinancialDataRes>().ReverseMap();
            CreateMap<PactProjectYearCostsDto, PactProjectYearCostsRes>()
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => (short)src.Year))
                .ReverseMap()
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => (double)src.Year));

            CreateMap<ProjectYearManagerDto, ProjectYearManagerRes>().ReverseMap();
        }
    }
}
