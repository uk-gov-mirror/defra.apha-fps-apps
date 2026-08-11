using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class SettingService : ISettingService
    {
        private readonly IFpsApiClient _fpsClient;

        public SettingService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<decimal>> GetHoursPerDayAsync()
        {
            return await _fpsClient.FpsSetting.GetHoursPerDayAsync();
        }

        public async Task<ApiResponseDto<List<SettingDto>>> GetAllSettingsAsync()
        {
            return await _fpsClient.FpsSetting.GetAllSettingsAsync();
        }

        public async Task<ApiResponseDto<List<YearEndSettingDto>>> GetYearEndSettingsAsync()
        {
            return await _fpsClient.FpsSetting.GetYearEndSettingsAsync();
        }

        public async Task<ApiResponseDto<SettingDto>> AddSettingAsync(SettingDto dto)
        {
            return await _fpsClient.FpsSetting.AddSettingAsync(dto);
        }

        public async Task<ApiResponseDto<SettingDto>> UpdateSettingAsync(string id, SettingDto dto)
        {
            return await _fpsClient.FpsSetting.UpdateSettingAsync(id, dto);
        }

        public async Task<ApiResponseDto<SettingDto>> SaveSettingAsync(SettingDto dto)
        {
            return await _fpsClient.FpsSetting.SaveSettingAsync(dto);
        }
    }
}
