using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Core.Interfaces;

namespace Apha.Costbook.Application.Services
{
    
    public class MaintenanceSettingsService : IMaintenanceSettingsService
    {
        private readonly ISettingsRepository _settingsRepository;

        
        private const string IdInflationAnimals       = "InflationAnimals";
        private const string IdInflationExceptional   = "InflationExceptional";
        private const string IdInflationStaff         = "InflationStaff";
        private const string IdInflationTests         = "InflationTests";
        private const string IdCurrentYear            = "CurrentYear";
        private const string IdWorkingHoursInDay      = "HoursInDay";
        private const string IdWorkingDaysInYear      = "DaysInYear";
        private const string IdProfitAnimals          = "Profitanimals";
        private const string IdProfitExceptional      = "ProfitExceptional";
        private const string IdProfitStaff            = "Profitstaff";
        private const string IdProfitTests            = "Profittests";

        public MaintenanceSettingsService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        
        public async Task<MaintenanceSettingsDto> GetSettingsAsync()
        {
            var settings = await _settingsRepository.GetAllUserUpdatableAsync();

            
            var lookup = settings.ToDictionary(s => s.Id, s => s.Setting ?? string.Empty, StringComparer.OrdinalIgnoreCase);

            return new MaintenanceSettingsDto
            {
                
                InflationAnimals       = ParseDecimal(lookup, IdInflationAnimals),
                InflationExceptionalCosts = ParseDecimal(lookup, IdInflationExceptional),
                InflationStaff         = ParseDecimal(lookup, IdInflationStaff),
                InflationTests         = ParseDecimal(lookup, IdInflationTests),                
                CurrentFinancialYear   = ParseInt(lookup, IdCurrentYear),
                WorkingHoursInDay      = ParseDecimal(lookup, IdWorkingHoursInDay),
                WorkingDaysInYear      = ParseDecimal(lookup, IdWorkingDaysInYear),                
                ProfitAnimals          = ParseDecimal(lookup, IdProfitAnimals),
                ProfitExceptionalCosts = ParseDecimal(lookup, IdProfitExceptional),
                ProfitStaff            = ParseDecimal(lookup, IdProfitStaff),
                ProfitTests            = ParseDecimal(lookup, IdProfitTests),
            };
        }

        
        public async Task UpdateSettingsAsync(MaintenanceSettingsDto dto)
        {
            if (dto is null)
                throw new ArgumentException("MaintenanceSettingsDto must not be null.", nameof(dto));

            
            var updates = new Dictionary<string, string>
            {
                [IdInflationAnimals]     = dto.InflationAnimals.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
                [IdInflationExceptional] = dto.InflationExceptionalCosts.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
                [IdInflationStaff]       = dto.InflationStaff.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
                [IdInflationTests]       = dto.InflationTests.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
                [IdCurrentYear]          = dto.CurrentFinancialYear.ToString(System.Globalization.CultureInfo.InvariantCulture),
                [IdWorkingHoursInDay]    = dto.WorkingHoursInDay.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
                [IdWorkingDaysInYear]    = dto.WorkingDaysInYear.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
                [IdProfitAnimals]        = dto.ProfitAnimals.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
                [IdProfitExceptional]    = dto.ProfitExceptionalCosts.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
                [IdProfitStaff]          = dto.ProfitStaff.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
                [IdProfitTests]          = dto.ProfitTests.ToString("G", System.Globalization.CultureInfo.InvariantCulture),
            };

            
            var success = await _settingsRepository.UpdateMultipleAsync(updates);
            if (!success)
                throw new InvalidOperationException("Maintenance settings update failed — no rows were updated in tbl_settings.");
        }

        
        private static decimal ParseDecimal(Dictionary<string, string> lookup, string id)
        {
            if (!lookup.TryGetValue(id, out var raw))
                throw new InvalidOperationException($"Required setting '{id}' was not found in tbl_settings (Userupdateable rows).");

            if (!decimal.TryParse(raw, System.Globalization.NumberStyles.Any,
                                  System.Globalization.CultureInfo.InvariantCulture, out var value))
                throw new InvalidOperationException($"Setting '{id}' value '{raw}' could not be parsed as a decimal.");

            return value;
        }

        
        private static int ParseInt(Dictionary<string, string> lookup, string id)
        {
            if (!lookup.TryGetValue(id, out var raw))
                throw new InvalidOperationException($"Required setting '{id}' was not found in tbl_settings (Userupdateable rows).");

            if (!int.TryParse(raw, out var value))
                throw new InvalidOperationException($"Setting '{id}' value '{raw}' could not be parsed as an integer.");

            return value;
        }
    }
}
