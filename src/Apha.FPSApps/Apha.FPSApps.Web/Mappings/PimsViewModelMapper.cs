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
            CreateMap<ProjectCommentItem, CommentDto>().ReverseMap();            
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
