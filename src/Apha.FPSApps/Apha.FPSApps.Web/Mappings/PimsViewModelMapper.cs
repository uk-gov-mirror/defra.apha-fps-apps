using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;

namespace Apha.FPSApps.Web.Mappings
{
    public class PimsViewModelMapper : Profile
    {
        public PimsViewModelMapper()
        {
            CreateMap(typeof(PaginationFilter<>), typeof(QueryParameters<>)).ReverseMap();
            CreateMap<PaginationModel, PaginationDto>().ReverseMap();

            CreateMap<ProjectListItem, ProjectListViewDto>().ReverseMap();
            CreateMap<ProjectListViewModel, ProposedProjectDto>().ReverseMap();
            CreateMap<ProposedProjectViewModel, ProposedProjectDto>().ReverseMap();
            CreateMap<ProjectDetailsViewModel, ProjectDetailDto>().ReverseMap();
            CreateMap<ProjectDetailsViewModel, ProposedProjectDto>().ReverseMap();
            CreateMap<ProjectCommentItem, CommentDto>()
                .ForMember(dest => dest.CommentText, opt => opt.MapFrom(src => src.Comment))
                .ForMember(dest => dest.MadeBy, opt => opt.MapFrom(src => src.MadeBy))
                .ForMember(dest => dest.DateEntered, opt => opt.MapFrom(src => src.DateEntered))
                .ReverseMap()
                .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.CommentText))
                .ForMember(dest => dest.MadeBy, opt => opt.MapFrom(src => src.MadeBy))
                .ForMember(dest => dest.DateEntered, opt => opt.MapFrom(src => src.DateEntered));
            
            // Maps CommentNo, Project, Year, Topic, CommentText → CommentDto fields; ReverseMap for pre-population on edit
            CreateMap<AddEditCommentViewModel, CommentDto>().ReverseMap();
            // Plan grid item — maps from plan fields on the shared DTO
            CreateMap<AdditionalCostDto, AdditionalCostPlanItem>().ReverseMap();
            CreateMap<AdditionalCostDto, AdditionalCostActualItem>().ReverseMap();
            CreateMap<AnimalCostDto, AnimalCostPlanItem>().ReverseMap();
            CreateMap<AnimalCostDto, AnimalCostActualItem>().ReverseMap();
            CreateMap<TestCostDto, TestCostPlanItem>().ReverseMap();
            CreateMap<TestCostDto, TestCostActualItem>().ReverseMap();
            CreateMap<StaffCostDto, StaffCostPlanItem>().ReverseMap();
            CreateMap<StaffCostDto, StaffCostActualItem>().ReverseMap();
            CreateMap<PactPayDto, PactPayItem>().ReverseMap();
            CreateMap<MonthlyPactDto, MonthlyPactItem>().ReverseMap();
            CreateMap<MilestoneItem, MilestoneDto>().ReverseMap();
            CreateMap<PMDMilestoneItem, MilestoneDto>().ReverseMap();
            CreateMap<MilestoneDto, PMDMilestoneItem>().ReverseMap();
            CreateMap<MilestoneFormDatesItem, MilestoneFormDatesDto>().ReverseMap();
            CreateMap<LogMilestoneItem, LogMilestoneDto>().ReverseMap();
            CreateMap<InvoiceItem, RadTrackInvoiceDto>().ReverseMap();
            CreateMap<InvoiceViewModel, RadTrackInvoiceDto>().ReverseMap();
            CreateMap<InvoiceTotalsItem, RadTrackInvoiceTotalsDto>().ReverseMap();
            CreateMap<StagingMilestoneItem, StagingMilestoneDto>().ReverseMap();
            CreateMap<YearlyFinancialDataItem, YearlyFinancialDataDto>().ReverseMap();
            CreateMap<PactCostsItem, PactProjectYearCostsDto>().ReverseMap();
        }
    }
}
