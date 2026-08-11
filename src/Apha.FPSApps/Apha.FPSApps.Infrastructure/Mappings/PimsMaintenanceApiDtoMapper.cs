using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos.PIMS;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class PimsMaintenanceApiDtoMapper : Profile
    {
        public PimsMaintenanceApiDtoMapper()
        {
            CreateMap<ReportRes, ReportDto>().ReverseMap();
            CreateMap<ReportDto, ReportReq>().ReverseMap();

            CreateMap<ReportGroupRes, ReportGroupDto>().ReverseMap();
            CreateMap<ReportGroupDto, ReportGroupReq>().ReverseMap();

            CreateMap<ReportGroupLinkRes, ReportGroupLinkDto>().ReverseMap();
            CreateMap<ReportGroupLinkDto, ReportGroupLinkReq>().ReverseMap();

            CreateMap<ProjectManagerRes, ProjectManagerDto>().ReverseMap();
            CreateMap<ProjectManagerDto, ProjectManagerReq>().ReverseMap();

            CreateMap<ProgramManagerLinkRes, ProgramManagerLinkDto>().ReverseMap();
            CreateMap<ProgramManagerLinkDto, ProgramManagerLinkReq>().ReverseMap();

            CreateMap<ProfitCentreManagerLinkRes, ProfitCentreManagerLinkDto>().ReverseMap();
            CreateMap<ProfitCentreManagerLinkDto, ProfitCentreManagerLinkReq>().ReverseMap();

            CreateMap<SettingRes, SettingDto>()
                .ForMember(dest => dest.SettingValue, opt => opt.MapFrom(src => src.Setting))
                .ReverseMap()
                .ForMember(dest => dest.Setting, opt => opt.MapFrom(src => src.SettingValue));

            CreateMap<SettingDto, SettingReq>()
                .ForMember(dest => dest.Setting, opt => opt.MapFrom(src => src.SettingValue))
                .ReverseMap()
                .ForMember(dest => dest.SettingValue, opt => opt.MapFrom(src => src.Setting));

            CreateMap<AccessUserRes, AccessUserDto>().ReverseMap();
            CreateMap<AccessUserDto, AccessUserReq>().ReverseMap();

            CreateMap<AccessLevelRes, AccessLevelDto>().ReverseMap();

            CreateMap<AccessUserLevelRes, AccessUserLevelDto>().ReverseMap();
            CreateMap<AccessUserLevelDto, AccessUserLevelReq>().ReverseMap();

            CreateMap<AccessSystemRes, AccessSystemDto>();

            CreateMap<FrequencyRes, FrequencyDto>()
                .ForMember(dest => dest.Frequencyid, opt => opt.MapFrom(src => src.Frequencyid))
                .ReverseMap()
                .ForMember(dest => dest.FrequencyValue, opt => opt.MapFrom(src => src.FrequencyValue));

            CreateMap<FrequencyDto, FrequencyReq>()
                .ForMember(dest => dest.FrequencyId, opt => opt.MapFrom(src => src.Frequencyid))
                .ReverseMap()
                .ForMember(dest => dest.FrequencyValue, opt => opt.MapFrom(src => src.FrequencyValue));

            CreateMap<ReviewItemRes, ReviewItemDto>().ReverseMap();
            CreateMap<ReviewItemDto, ReviewItemReq>().ReverseMap();

            CreateMap<RadTrackProgRes, RadTrackProgDto>().ReverseMap();
            CreateMap<RadTrackProgDto, RadTrackProgReq>().ReverseMap();
        }
    }
}
