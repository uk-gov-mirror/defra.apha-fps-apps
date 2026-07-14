using Apha.Costbook.DataAccess;

namespace Apha.Costbook.Core.Interfaces
{
    public interface ISettingsRepository
    {       
        Task<string?> GetSettingValueByIdAsync(string id);

        Task<List<Settings>> GetAllUserUpdatableAsync();
        
        Task<bool> UpdateMultipleAsync(Dictionary<string, string> settingsById);
    }
}
