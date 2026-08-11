using Apha.PIMS.Application.Dtos;

namespace Apha.PIMS.Application.Interfaces
{
    public interface ISettingService
    {
        Task<List<SettingDto>> GetAllSettingsAsync();
        Task<List<SettingDto>> GetAllUserUpdateableSettingsAsync();

        Task<SettingDto?> GetSettingByIdAsync(string id);
        Task<SettingDto> UpdateSettingAsync(SettingDto dto);

        Task<bool> SettingExistsAsync(string id);
    }
}
