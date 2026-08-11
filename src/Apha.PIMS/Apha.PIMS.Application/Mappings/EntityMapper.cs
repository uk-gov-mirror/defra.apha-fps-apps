using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Application.Mappings
{
    public class EntityMapper : Profile
    {
        public EntityMapper()
        {
            CreateMap(typeof(PaginationParameters<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap(typeof(PagedData<>), typeof(PaginatedResult<>)).ReverseMap();
            CreateMap<PaginationData, PaginationDto>().ReverseMap();
            CreateMap<ProjectListView, ProjectListViewDto>().ReverseMap();
            CreateMap<ProjectListMilestone, ProjectListMilestoneDto>().ReverseMap();
            CreateMap<ProjectDetailsMilestone, ProjectDetailsMilestoneDto>().ReverseMap();
            CreateMap<Project, ProjectDto>().ReverseMap();
            CreateMap<ProposedProject, ProposedProjectDto>().ReverseMap();
            CreateMap<Projects, ProjectsDto>().ReverseMap();
            CreateMap<Comment, CommentDto>().ReverseMap();
            CreateMap<ProjectDetail, ProjectDetailDto>().ReverseMap();
            CreateMap<Risk, RiskDto>().ReverseMap();
            CreateMap<Year, YearDto>().ReverseMap();
            CreateMap<CommentTopic, CommentTopicDto>().ReverseMap();
            CreateMap<ProjSubContract, AdditionalCostDto>().ReverseMap();
            CreateMap<AdditionalCosts, AdditionalCostDto>().ReverseMap();
            CreateMap<ProjSubContract, AnimalCostDto>().ReverseMap();
            CreateMap<ProjectAnimalPlan, AnimalCostDto>().ReverseMap();
            CreateMap<ProjectStaffPlan, StaffCostDto>().ReverseMap();
            CreateMap<TimeCostCalcs, StaffCostDto>().ReverseMap();
            CreateMap<Projects, ProjectYearDetailsDto>().ReverseMap();
            CreateMap<PactPayCalc, PactPayDto>().ReverseMap();
            CreateMap<ProjectMonthFinal, MonthlyPactDto>().ReverseMap();
            CreateMap<FpsYearTotal, FpsYearTotalsDto>().ReverseMap();
            CreateMap<Milestone, MilestoneDto>()
               .ForMember(dest => dest.IsLate, opt => opt.Ignore());
            CreateMap<MilestoneDto, Milestone>();
            CreateMap<MilestoneType, MilestoneTypeDto>().ReverseMap();
            CreateMap<MilestoneFormDates, MilestoneFormDatesDto>().ReverseMap();
            CreateMap<LogMilestone, LogMilestoneDto>().ReverseMap();
            CreateMap<RadTrackInvoice, RadTrackInvoiceDto>().ReverseMap();
            CreateMap<RadTrackInvoiceTotals, RadTrackInvoiceTotalsDto>().ReverseMap();

            
            CreateMap<Report, ReportDto>().ReverseMap();
            CreateMap<ReportGroup, ReportGroupDto>().ReverseMap();
            CreateMap<ReportGroupLink, ReportGroupLinkDto>().ReverseMap();
            CreateMap<ProjectManager, ProjectManagerDto>().ReverseMap();
            CreateMap<ProgramManagerLink, ProgramManagerLinkDto>().ReverseMap();
            CreateMap<ProgramLookup, ProgramLookupDto>().ReverseMap();
            CreateMap<ProfitCentreLookup, ProfitCentreLookupDto>().ReverseMap();
            CreateMap<ProfitCentreManagerLink, ProfitCentreManagerLinkDto>().ReverseMap();
            CreateMap<Settings, SettingDto>().ReverseMap();
            CreateMap<AccessUser, AccessUserDto>().ReverseMap();
            CreateMap<AccessLevel, AccessLevelDto>().ReverseMap();
            CreateMap<AccessUserLevel, AccessUserLevelDto>().ReverseMap();
            CreateMap<AccessSystem, AccessSystemDto>().ReverseMap();
            CreateMap<Frequency, FrequencyDto>().ReverseMap();
            CreateMap<ReviewItem, ReviewItemDto>().ReverseMap();
            CreateMap<PublicationType, PublicationTypeDto>().ReverseMap();

            
            CreateMap<RadtrackProg, RadTrackProgDto>().ReverseMap();
            CreateMap<StagingMilestone, StagingMilestoneDto>().ReverseMap();
            CreateMap<YearlyFinancialData, YearlyFinancialDataDto>().ReverseMap();
            CreateMap<PactProjectYearCosts, PactProjectYearCostsDto>().ReverseMap();

            CreateMap<ProjectYearManager, ProjectYearManagerDto>().ReverseMap();
        }
    }
}
