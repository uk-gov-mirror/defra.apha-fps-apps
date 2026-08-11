using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsSettingApiClient
    {
        Task<ApiResponseDto<decimal>> GetHoursPerDayAsync();
        Task<ApiResponseDto<List<SettingDto>>> GetAllSettingsAsync();
        Task<ApiResponseDto<List<YearEndSettingDto>>> GetYearEndSettingsAsync();
        Task<ApiResponseDto<SettingDto>> AddSettingAsync(SettingDto dto);
        Task<ApiResponseDto<SettingDto>> UpdateSettingAsync(string id, SettingDto dto);
        Task<ApiResponseDto<SettingDto>> SaveSettingAsync(SettingDto dto);
    }
}
