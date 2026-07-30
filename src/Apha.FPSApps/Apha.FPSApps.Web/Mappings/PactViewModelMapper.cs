using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;using Apha.FPSApps.Web.Areas.PACT.Models;
using AutoMapper;

namespace Apha.FPSApps.Web.Mappings
{
    public class PactViewModelMapper : Profile
    {
        public PactViewModelMapper() 
        {
            CreateMap<WorkGroupDto, WorkGroup>().ReverseMap();
            CreateMap<ProjectDto, Project>().ReverseMap();
            CreateMap<ProfitCentreDto, ProfitCentre>().ReverseMap();
            CreateMap<PactProjectViewModel, ProjectDto>().ReverseMap();
            CreateMap<ProjectJobCodeViewModel, JobCodeDto>().ReverseMap();
            CreateMap<JobCodeViewModel, JobCodeDto>().ReverseMap();
            CreateMap<PortfolioJobCodeViewModel, JobCodeDto>().ReverseMap();
            CreateMap<TimeCodeValidDto, TimeCodeViewModel>().ReverseMap();
            CreateMap<TimeCodeValidDto, ValidTimeCodeViewModel>()
                .ForMember(dest => dest.Project, opt => opt.MapFrom(src => src.ParentProject))
                .ForMember(dest => dest.OriginalWorkGroup, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.ParentProject, opt => opt.MapFrom(src => src.ParentProject));
            CreateMap<ProjectInvoiceItem, ProjectInvoiceDto>().ReverseMap();
            CreateMap<InvoiceItem, ProjectInvoiceDto>().ReverseMap();
            CreateMap<ProjectSubContractItem, ProjectSubContractDto>().ReverseMap();
            // Mapping for standalone SubContract page
            CreateMap<SubContractItem, ProjectSubContractDto>()
                .ForMember(dest => dest.DailyRate, opt => opt.Ignore())
                .ForMember(dest => dest.AnimalDays, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Counter, opt => opt.MapFrom(src => src.SubContCounter));

            CreateMap<SubContractRmsItem, ProjectSubContractDto>().ReverseMap();
            CreateMap<SubContractRmsImportRowDto, SubContractRmsFailedItem>().ReverseMap();

            CreateMap<TestCapabilityItem, TestCapabilityDto>().ReverseMap();
            CreateMap<ConstituentTestItem, TestCapabilityDto>().ReverseMap();

            // Mapping for WorkGroup-focused Test Capability view
            CreateMap<WorkGroupTestCapabilityItem, TestCapabilityDto>().ReverseMap();

            CreateMap<PortfolioTimeCodeViewModel, TimeCodeValidDto>().ReverseMap();

            CreateMap<TestRequirementItem, TestRequirementDto>().ReverseMap();
            CreateMap<TestPurchaseRequirementItem, TestRequirementDto>().ReverseMap();

            CreateMap<ProgramViewModel, ProgramDto>().ReverseMap();
            CreateMap<ProgramProjectItem, ProjectDto>().ReverseMap();
            CreateMap<TestorProductDto, TestOrProductViewModel>().ReverseMap();

            CreateMap<ProjectMonthItem, ProjectMonthDto>().ReverseMap();
            CreateMap<PactStaffDto, WorkGroupPeopleItem>().ReverseMap();
            CreateMap<WorkGroupDto, WorkGroup>().ReverseMap();
            CreateMap<WorkGroupPersonDto, WorkGroupPerson>().ReverseMap();
            CreateMap<MonthlyOutputLogDto, MonthlyOutputLogItem>().ReverseMap();
            CreateMap<MonthlyTimeLogDto, MonthlyTimeLogItem>().ReverseMap();
            CreateMap<CalenderMonthDto, CalenderMonth>().ReverseMap();
            CreateMap<WorkGroupTimeCodeDto, WorkGroupTimeCodeItem>().ReverseMap();
            CreateMap<WorkGroupValidTimeCodeDto, WorkGroupValidTimeCodeItem>().ReverseMap();
            CreateMap<WgSummarisedStaffTimeUsageRowDto, WgSummarisedStaffTimeUsageRow>()
     .ForMember(dest => dest.April, opt => opt.MapFrom(src => Math.Round(src.April, 2)))
     .ForMember(dest => dest.May, opt => opt.MapFrom(src => Math.Round(src.May, 2)))
     .ForMember(dest => dest.June, opt => opt.MapFrom(src => Math.Round(src.June, 2)))
     .ForMember(dest => dest.July, opt => opt.MapFrom(src => Math.Round(src.July, 2)))
     .ForMember(dest => dest.August, opt => opt.MapFrom(src => Math.Round(src.August, 2)))
     .ForMember(dest => dest.September, opt => opt.MapFrom(src => Math.Round(src.September, 2)))
     .ForMember(dest => dest.October, opt => opt.MapFrom(src => Math.Round(src.October, 2)))
     .ForMember(dest => dest.November, opt => opt.MapFrom(src => Math.Round(src.November, 2)))
     .ForMember(dest => dest.December, opt => opt.MapFrom(src => Math.Round(src.December, 2)))
     .ForMember(dest => dest.January, opt => opt.MapFrom(src => Math.Round(src.January, 2)))
     .ForMember(dest => dest.February, opt => opt.MapFrom(src => Math.Round(src.February, 2)))
     .ForMember(dest => dest.March, opt => opt.MapFrom(src => Math.Round(src.March, 2)))
     .ForMember(dest => dest.TotalTime, opt => opt.MapFrom(src => Math.Round(src.TotalTime, 2)))
     .ForMember(dest => dest.TotalCost, opt => opt.MapFrom(src => Math.Round(src.TotalCost, 2)))
     .ReverseMap();
            CreateMap<WgSummarisedStaffTimeUsageSummaryDto, WgSummarisedStaffTimeUsageSummary>()
                .ForMember(dest => dest.TotalApril, opt => opt.MapFrom(src => Math.Round(src.TotalApril, 2)))
                .ForMember(dest => dest.TotalMay, opt => opt.MapFrom(src => Math.Round(src.TotalMay, 2)))
                .ForMember(dest => dest.TotalJune, opt => opt.MapFrom(src => Math.Round(src.TotalJune, 2)))
                .ForMember(dest => dest.TotalJuly, opt => opt.MapFrom(src => Math.Round(src.TotalJuly, 2)))
                .ForMember(dest => dest.TotalAugust, opt => opt.MapFrom(src => Math.Round(src.TotalAugust, 2)))
                .ForMember(dest => dest.TotalSeptember, opt => opt.MapFrom(src => Math.Round(src.TotalSeptember, 2)))
                .ForMember(dest => dest.TotalOctober, opt => opt.MapFrom(src => Math.Round(src.TotalOctober, 2)))
                .ForMember(dest => dest.TotalNovember, opt => opt.MapFrom(src => Math.Round(src.TotalNovember, 2)))
                .ForMember(dest => dest.TotalDecember, opt => opt.MapFrom(src => Math.Round(src.TotalDecember, 2)))
                .ForMember(dest => dest.TotalJanuary, opt => opt.MapFrom(src => Math.Round(src.TotalJanuary, 2)))
                .ForMember(dest => dest.TotalFebruary, opt => opt.MapFrom(src => Math.Round(src.TotalFebruary, 2)))
                .ForMember(dest => dest.TotalMarch, opt => opt.MapFrom(src => Math.Round(src.TotalMarch, 2)))
                .ForMember(dest => dest.GrandTotalTime, opt => opt.MapFrom(src => Math.Round(src.GrandTotalTime, 2)))
                .ForMember(dest => dest.GrandTotalCost, opt => opt.MapFrom(src => Math.Round(src.GrandTotalCost, 2)))
                .ForMember(dest => dest.StandardHoursPerMonth, opt => opt.MapFrom(src => Math.Round(src.StandardHoursPerMonth, 2)))
                .ForMember(dest => dest.TotalStandardHours, opt => opt.MapFrom(src => Math.Round(src.TotalStandardHours, 2)))
                .ForMember(dest => dest.GrandTotalPercentAllocated, opt => opt.MapFrom(src => Math.Round(src.GrandTotalPercentAllocated, 2)))
                .ForMember(dest => dest.PercentAllocatedApril, opt => opt.MapFrom(src => Math.Round(src.PercentAllocatedApril, 2)))
                .ForMember(dest => dest.PercentAllocatedMay, opt => opt.MapFrom(src => Math.Round(src.PercentAllocatedMay, 2)))
                .ForMember(dest => dest.PercentAllocatedJune, opt => opt.MapFrom(src => Math.Round(src.PercentAllocatedJune, 2)))
                .ForMember(dest => dest.PercentAllocatedJuly, opt => opt.MapFrom(src => Math.Round(src.PercentAllocatedJuly, 2)))
                .ForMember(dest => dest.PercentAllocatedAugust, opt => opt.MapFrom(src => Math.Round(src.PercentAllocatedAugust, 2)))
                .ForMember(dest => dest.PercentAllocatedSeptember, opt => opt.MapFrom(src => Math.Round(src.PercentAllocatedSeptember, 2)))
                .ForMember(dest => dest.PercentAllocatedOctober, opt => opt.MapFrom(src => Math.Round(src.PercentAllocatedOctober, 2)))
                .ForMember(dest => dest.PercentAllocatedNovember, opt => opt.MapFrom(src => Math.Round(src.PercentAllocatedNovember, 2)))
                .ForMember(dest => dest.PercentAllocatedDecember, opt => opt.MapFrom(src => Math.Round(src.PercentAllocatedDecember, 2)))
                .ForMember(dest => dest.PercentAllocatedJanuary, opt => opt.MapFrom(src => Math.Round(src.PercentAllocatedJanuary, 2)))
                .ForMember(dest => dest.PercentAllocatedFebruary, opt => opt.MapFrom(src => Math.Round(src.PercentAllocatedFebruary, 2)))
                .ForMember(dest => dest.PercentAllocatedMarch, opt => opt.MapFrom(src => Math.Round(src.PercentAllocatedMarch, 2)))
                .ReverseMap();

            CreateMap<SummarisedWgTimeDto, SummarisedWgTimePivotRow>()
                .ForMember(dest => dest.April, opt => opt.MapFrom(src => Math.Round(src.April ?? 0, 2)))
                .ForMember(dest => dest.May, opt => opt.MapFrom(src => Math.Round(src.May ?? 0, 2)))
                .ForMember(dest => dest.June, opt => opt.MapFrom(src => Math.Round(src.June ?? 0, 2)))
                .ForMember(dest => dest.July, opt => opt.MapFrom(src => Math.Round(src.July ?? 0, 2)))
                .ForMember(dest => dest.August, opt => opt.MapFrom(src => Math.Round(src.August ?? 0, 2)))
                .ForMember(dest => dest.September, opt => opt.MapFrom(src => Math.Round(src.September ?? 0, 2)))
                .ForMember(dest => dest.October, opt => opt.MapFrom(src => Math.Round(src.October ?? 0, 2)))
                .ForMember(dest => dest.November, opt => opt.MapFrom(src => Math.Round(src.November ?? 0, 2)))
                .ForMember(dest => dest.December, opt => opt.MapFrom(src => Math.Round(src.December ?? 0, 2)))
                .ForMember(dest => dest.January, opt => opt.MapFrom(src => Math.Round(src.January ?? 0, 2)))
                .ForMember(dest => dest.February, opt => opt.MapFrom(src => Math.Round(src.February ?? 0, 2)))
                .ForMember(dest => dest.March, opt => opt.MapFrom(src => Math.Round(src.March ?? 0, 2)))
                .ForMember(dest => dest.SumOfTime, opt => opt.MapFrom(src => Math.Round(src.SumOfTime, 2)))
                .ForMember(dest => dest.SumOfCost, opt => opt.MapFrom(src => Math.Round(src.SumOfCost, 2)))
                .ForMember(dest => dest.Budget, opt => opt.MapFrom(src => src.Budget.HasValue ? Math.Round(src.Budget.Value, 2) : (decimal?)null))
                .ForMember(dest => dest.PercentSpent, opt => opt.MapFrom(src => src.PercentSpent.HasValue ? Math.Round(src.PercentSpent.Value, 2) : (decimal?)null));
            CreateMap<SummarisedWgTimeSummaryDto, SummarisedWgTimeSummary>()
                .ForMember(dest => dest.TotalApril, opt => opt.MapFrom(src => Math.Round(src.TotalApril, 2)))
                .ForMember(dest => dest.TotalMay, opt => opt.MapFrom(src => Math.Round(src.TotalMay, 2)))
                .ForMember(dest => dest.TotalJune, opt => opt.MapFrom(src => Math.Round(src.TotalJune, 2)))
                .ForMember(dest => dest.TotalJuly, opt => opt.MapFrom(src => Math.Round(src.TotalJuly, 2)))
                .ForMember(dest => dest.TotalAugust, opt => opt.MapFrom(src => Math.Round(src.TotalAugust, 2)))
                .ForMember(dest => dest.TotalSeptember, opt => opt.MapFrom(src => Math.Round(src.TotalSeptember, 2)))
                .ForMember(dest => dest.TotalOctober, opt => opt.MapFrom(src => Math.Round(src.TotalOctober, 2)))
                .ForMember(dest => dest.TotalNovember, opt => opt.MapFrom(src => Math.Round(src.TotalNovember, 2)))
                .ForMember(dest => dest.TotalDecember, opt => opt.MapFrom(src => Math.Round(src.TotalDecember, 2)))
                .ForMember(dest => dest.TotalJanuary, opt => opt.MapFrom(src => Math.Round(src.TotalJanuary, 2)))
                .ForMember(dest => dest.TotalFebruary, opt => opt.MapFrom(src => Math.Round(src.TotalFebruary, 2)))
                .ForMember(dest => dest.TotalMarch, opt => opt.MapFrom(src => Math.Round(src.TotalMarch, 2)))
                .ForMember(dest => dest.GrandTotalTime, opt => opt.MapFrom(src => Math.Round(src.GrandTotalTime, 2)))
                .ForMember(dest => dest.GrandTotalCost, opt => opt.MapFrom(src => Math.Round(src.GrandTotalCost, 2)));

            CreateMap<RecreateSummaryLogDto, RecreateSummaryLogItem>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.Comments));

            CreateMap<BatchJobHistoryDto, BatchJobHistoryItem>();

            CreateMap<ProfitCentreCostDto, ProfitCenterCostItem>().ReverseMap();

            CreateMap<ReleasePeriodDto, PeriodMonth>().ReverseMap();

            CreateMap<ReleasePeriodDto, ReleasePeriodItem>().ReverseMap();

            CreateMap<WgTestCapabilitiesWithDescriptionDto, WgTestCapabilitiesWithDescriptionItem>().ReverseMap();
        }
    }
}
