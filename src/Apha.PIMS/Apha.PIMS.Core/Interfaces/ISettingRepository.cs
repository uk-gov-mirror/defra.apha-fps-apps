using Apha.PIMS.Core.Entities;

namespace Apha.PIMS.Core.Interfaces
{
    public interface ISettingRepository
    {
        Task<List<Settings>> GetAllSettingsAsync();

        Task<List<Settings>> GetAllUserUpdateableSettingsAsync();

        Task<Settings?> GetSettingByIdAsync(string id);

        Task<Settings> UpdateSettingAsync(Settings entity);

        Task<bool> SettingExistsAsync(string id);
    }
}
