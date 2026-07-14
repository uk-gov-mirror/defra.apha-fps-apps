using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Services;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.Costbook.Application.UnitTests.Services.MaintenanceSettingsServiceTest
{
    public class MaintenanceSettingsServiceTests
    {
        private readonly ISettingsRepository _settingsRepository;
        private readonly MaintenanceSettingsService _service;

        public MaintenanceSettingsServiceTests()
        {
            _settingsRepository = Substitute.For<ISettingsRepository>();
            _service = new MaintenanceSettingsService(_settingsRepository);
        }

        // ── GetSettingsAsync ──────────────────────────────────────────────────

        #region GetSettingsAsync Tests

        [Fact]
        public async Task GetSettingsAsync_AllSettingsPresent_ReturnsMappedDto()
        {
            // Arrange
            var settings = BuildAllSettings(
                inflationAnimals: "2.5",
                inflationExceptional: "1.8",
                inflationStaff: "3.0",
                inflationTests: "2.0",
                currentYear: "2024",
                workingHoursInDay: "7.4",
                workingDaysInYear: "220",
                profitAnimals: "15.0",
                profitExceptional: "12.5",
                profitStaff: "10.0",
                profitTests: "8.0");
            _settingsRepository.GetAllUserUpdatableAsync().Returns(settings);

            // Act
            var dto = await _service.GetSettingsAsync();

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(2.5m, dto.InflationAnimals);
            Assert.Equal(1.8m, dto.InflationExceptionalCosts);
            Assert.Equal(3.0m, dto.InflationStaff);
            Assert.Equal(2.0m, dto.InflationTests);
            Assert.Equal(2024, dto.CurrentFinancialYear);
            Assert.Equal(7.4m, dto.WorkingHoursInDay);
            Assert.Equal(220m, dto.WorkingDaysInYear);
            Assert.Equal(15.0m, dto.ProfitAnimals);
            Assert.Equal(12.5m, dto.ProfitExceptionalCosts);
            Assert.Equal(10.0m, dto.ProfitStaff);
            Assert.Equal(8.0m, dto.ProfitTests);
            await _settingsRepository.Received(1).GetAllUserUpdatableAsync();
        }

        [Fact]
        public async Task GetSettingsAsync_MissingRequiredSetting_ThrowsInvalidOperationException()
        {
            // Arrange — omit InflationAnimals to simulate a missing row
            var settings = new List<Settings>
            {
                new Settings { Id = "InflationExceptional", Setting = "1.8" },
                new Settings { Id = "InflationStaff", Setting = "3.0" },
                // InflationAnimals is missing
            };
            _settingsRepository.GetAllUserUpdatableAsync().Returns(settings);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetSettingsAsync());
        }

        [Fact]
        public async Task GetSettingsAsync_UnparsableDecimalValue_ThrowsInvalidOperationException()
        {
            // Arrange — provide a non-numeric value for a decimal field
            var settings = BuildAllSettings(
                inflationAnimals: "NOT_A_NUMBER",
                inflationExceptional: "1.8",
                inflationStaff: "3.0",
                inflationTests: "2.0",
                currentYear: "2024",
                workingHoursInDay: "7.4",
                workingDaysInYear: "220",
                profitAnimals: "15.0",
                profitExceptional: "12.5",
                profitStaff: "10.0",
                profitTests: "8.0");
            _settingsRepository.GetAllUserUpdatableAsync().Returns(settings);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetSettingsAsync());
        }

        [Fact]
        public async Task GetSettingsAsync_UnparsableIntValue_ThrowsInvalidOperationException()
        {
            // Arrange — provide a non-integer value for CurrentYear
            var settings = BuildAllSettings(
                inflationAnimals: "2.5",
                inflationExceptional: "1.8",
                inflationStaff: "3.0",
                inflationTests: "2.0",
                currentYear: "NOTANINT",
                workingHoursInDay: "7.4",
                workingDaysInYear: "220",
                profitAnimals: "15.0",
                profitExceptional: "12.5",
                profitStaff: "10.0",
                profitTests: "8.0");
            _settingsRepository.GetAllUserUpdatableAsync().Returns(settings);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetSettingsAsync());
        }

        [Fact]
        public async Task GetSettingsAsync_ZeroValues_ReturnsDtoWithZeros()
        {
            // Arrange — all zero values are valid
            var settings = BuildAllSettings("0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0");
            _settingsRepository.GetAllUserUpdatableAsync().Returns(settings);

            // Act
            var dto = await _service.GetSettingsAsync();

            // Assert
            Assert.Equal(0m, dto.InflationAnimals);
            Assert.Equal(0, dto.CurrentFinancialYear);
        }

        #endregion

        // ── UpdateSettingsAsync ───────────────────────────────────────────────

        #region UpdateSettingsAsync Tests

        [Fact]
        public async Task UpdateSettingsAsync_ValidDto_CallsRepositoryBulkUpdate()
        {
            // Arrange
            var dto = new MaintenanceSettingsDto
            {
                InflationAnimals = 2.5m,
                InflationExceptionalCosts = 1.8m,
                InflationStaff = 3.0m,
                InflationTests = 2.0m,
                CurrentFinancialYear = 2024,
                WorkingHoursInDay = 7.4m,
                WorkingDaysInYear = 220m,
                ProfitAnimals = 15.0m,
                ProfitExceptionalCosts = 12.5m,
                ProfitStaff = 10.0m,
                ProfitTests = 8.0m
            };
            _settingsRepository.UpdateMultipleAsync(Arg.Any<Dictionary<string, string>>()).Returns(true);

            // Act
            await _service.UpdateSettingsAsync(dto);

            // Assert — ensure the repository was called with the exact keys used by the service implementation
            await _settingsRepository.Received(1).UpdateMultipleAsync(
                Arg.Is<Dictionary<string, string>>(d =>
                    d.Count == 11 &&
                    d.ContainsKey("InflationAnimals") &&
                    d.ContainsKey("InflationExceptional") &&
                    d.ContainsKey("InflationStaff") &&
                    d.ContainsKey("InflationTests") &&
                    d.ContainsKey("CurrentYear") &&
                    d.ContainsKey("HoursInDay") &&
                    d.ContainsKey("DaysInYear") &&
                    d.ContainsKey("Profitanimals") &&   // service uses this exact key string
                    d.ContainsKey("ProfitExceptional") &&
                    d.ContainsKey("Profitstaff") &&     // service uses this exact key string
                    d.ContainsKey("Profittests")));    // service uses this exact key string
        }

        [Fact]
        public async Task UpdateSettingsAsync_NullDto_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateSettingsAsync(null!));
            await _settingsRepository.DidNotReceive().UpdateMultipleAsync(Arg.Any<Dictionary<string, string>>());
        }

        [Fact]
        public async Task UpdateSettingsAsync_RepositoryReturnsFalse_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new MaintenanceSettingsDto
            {
                InflationAnimals = 2.5m,
                InflationExceptionalCosts = 1.8m,
                InflationStaff = 3.0m,
                InflationTests = 2.0m,
                CurrentFinancialYear = 2024,
                WorkingHoursInDay = 7.4m,
                WorkingDaysInYear = 220m,
                ProfitAnimals = 15.0m,
                ProfitExceptionalCosts = 12.5m,
                ProfitStaff = 10.0m,
                ProfitTests = 8.0m
            };
            _settingsRepository.UpdateMultipleAsync(Arg.Any<Dictionary<string, string>>()).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateSettingsAsync(dto));
        }

        [Fact]
        public async Task UpdateSettingsAsync_BuildsDictionaryWithCorrectKeys()
        {
            // Arrange — verify VBA ID strings are used verbatim
            var dto = new MaintenanceSettingsDto
            {
                InflationAnimals = 5m,
                InflationExceptionalCosts = 4m,
                InflationStaff = 3m,
                InflationTests = 2m,
                CurrentFinancialYear = 2025,
                WorkingHoursInDay = 7.5m,
                WorkingDaysInYear = 225m,
                ProfitAnimals = 20m,
                ProfitExceptionalCosts = 18m,
                ProfitStaff = 16m,
                ProfitTests = 14m
            };
            Dictionary<string, string>? capturedDict = null;
            // TRANSFORMENGINE (Phase 14 security fix): removed erroneous 'await' on NSubstitute setup call —
            // Arg.Do().Returns() is synchronous mock configuration, not an awaitable expression
            _settingsRepository.UpdateMultipleAsync(Arg.Do<Dictionary<string, string>>(d => capturedDict = d))
                .Returns(true);

            // Act
            await _service.UpdateSettingsAsync(dto);

            // Assert — keys match VBA constants exactly
            Assert.NotNull(capturedDict);
            Assert.True(capturedDict!.ContainsKey("InflationAnimals"));
            Assert.True(capturedDict.ContainsKey("InflationExceptional"));   // NOT InflationExceptionalCosts
            Assert.True(capturedDict.ContainsKey("ProfitExceptional"));      // NOT ProfitExceptionalCosts
            Assert.True(capturedDict.ContainsKey("CurrentYear"));
            Assert.Equal(11, capturedDict.Count);
        }

        #endregion

        // ── Private Helpers ───────────────────────────────────────────────────

        private static List<Settings> BuildAllSettings(
            string inflationAnimals, string inflationExceptional, string inflationStaff, string inflationTests,
            string currentYear, string workingHoursInDay, string workingDaysInYear,
            string profitAnimals, string profitExceptional, string profitStaff, string profitTests)
        {
            return new List<Settings>
            {
                new Settings { Id = "InflationAnimals",     Setting = inflationAnimals,     Userupdateable = true },
                new Settings { Id = "InflationExceptional", Setting = inflationExceptional, Userupdateable = true },
                new Settings { Id = "InflationStaff",       Setting = inflationStaff,       Userupdateable = true },
                new Settings { Id = "InflationTests",       Setting = inflationTests,       Userupdateable = true },
                new Settings { Id = "CurrentYear",          Setting = currentYear,          Userupdateable = true },
                // use the IDs expected by the service implementation
                new Settings { Id = "HoursInDay",           Setting = workingHoursInDay,    Userupdateable = true },
                new Settings { Id = "DaysInYear",           Setting = workingDaysInYear,    Userupdateable = true },
                new Settings { Id = "ProfitAnimals",        Setting = profitAnimals,        Userupdateable = true },
                new Settings { Id = "ProfitExceptional",    Setting = profitExceptional,    Userupdateable = true },
                new Settings { Id = "ProfitStaff",          Setting = profitStaff,          Userupdateable = true },
                new Settings { Id = "ProfitTests",          Setting = profitTests,          Userupdateable = true },
            };
        }
    }
}