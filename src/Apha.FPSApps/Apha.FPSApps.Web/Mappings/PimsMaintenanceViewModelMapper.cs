using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using AutoMapper;

namespace Apha.FPSApps.Web.Mappings
{
    public class PimsMaintenanceViewModelMapper : Profile
    {
        public PimsMaintenanceViewModelMapper()
        {
            CreateMap<ReportItem, ReportDto>().ReverseMap();
            CreateMap<ReportGroupItem, ReportGroupDto>().ReverseMap();
            CreateMap<RadTrackProgItem, RadTrackProgDto>().ReverseMap();
            CreateMap<ProjectManagerItem, ProjectManagerDto>().ReverseMap();
            CreateMap<ProgramManagerLinkItem, ProgramManagerLinkDto>().ReverseMap();
            CreateMap<ProfitCentreManagerLinkItem, ProfitCentreManagerLinkDto>().ReverseMap();
            CreateMap<SettingItem, SettingDto>().ReverseMap();
            CreateMap<AccessUserItem, AccessUserDto>().ReverseMap();

            CreateMap<AccessUserLevelItem, AccessUserLevelDto>()
                .ForMember(dest => dest.SystemId, opt => opt.MapFrom(src => src.SystemId))
                .ForMember(dest => dest.NtLogin, opt => opt.MapFrom(src => src.NtLogin))
                .ForMember(dest => dest.AccessLevelId, opt => opt.MapFrom(src => src.AccessLevelId));
            CreateMap<AccessUserLevelDto, AccessUserLevelItem>()
                .ForMember(dest => dest.AccessLevelName, opt => opt.Ignore()); // populated by controller

            // ── Other Tab ────────────────────────────────────────────────────────────
            CreateMap<FrequencyItem, FrequencyDto>().ReverseMap();
            CreateMap<ReviewItemItem, ReviewItemDto>().ReverseMap();
            CreateMap<RiskItem, RiskDto>().ReverseMap();
            CreateMap<PublicationTypeItem, PublicationTypeDto>().ReverseMap();
            CreateMap<OtherReportGroupItem, ReportGroupDto>().ReverseMap();
        }
    }
}
