using Apha.Costbook.Application.Dtos;

namespace Apha.Costbook.Application.Interfaces
{
    public interface IMaintenanceSettingsService
    {
        Task<MaintenanceSettingsDto> GetSettingsAsync();

        Task UpdateSettingsAsync(MaintenanceSettingsDto dto);
    }
}
