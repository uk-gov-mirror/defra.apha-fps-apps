using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsSettingApiClient
    {
        Task<ApiResponseDto<List<SettingDto>>> GetAllSettingsAsync();

        Task<ApiResponseDto<List<SettingDto>>> GetAllUserUpdateableSettingsAsync();

        Task<ApiResponseDto<SettingDto>> GetSettingByIdAsync(string id);

        Task<ApiResponseDto<SettingDto>> UpdateSettingAsync(string id, SettingDto dto);
    }
}
