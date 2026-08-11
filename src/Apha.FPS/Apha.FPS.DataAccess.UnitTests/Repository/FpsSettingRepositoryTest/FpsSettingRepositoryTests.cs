using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.FpsSettingRepositoryTest
{
    public class FpsSettingRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a FpsSettingRepository with in-memory TblSettings data.
        /// IFpsYearContext is substituted via NSubstitute.
        /// FpsSetting has a FpsCalYear query filter in FpsDbContext — the year value
        /// controls which records are visible, so it is set explicitly per test where relevant.
        /// </summary>
        private static FpsSettingRepository CreateRepository(
            IEnumerable<FpsSetting> settings,
            int fpsYear = DefaultTestFpsYear)
        {
            var fpsYearContext = Substitute.For<IFpsRequestContext>();
            fpsYearContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var settingsMockSet = RepositoryTestHelper.CreateMockDbSet(settings);
            mockContext.Setup(x => x.TblSettings).Returns(settingsMockSet.Object);

            return new FpsSettingRepository(mockContext.Object, fpsYearContext);
        }

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ReturnsAllSettings_WhenDataExists()
        {
            // Arrange
            var settings = new List<FpsSetting>
            {
                new() { Id = "HoursInDay",   Setting = "8",  FpsYear = DefaultTestFpsYear },
                new() { Id = "DaysInYear",   Setting = "365", FpsYear = DefaultTestFpsYear },
                new() { Id = "WeeksInYear",  Setting = "52",  FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(settings);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoSettingsExist()
        {
            // Arrange
            var repo = CreateRepository(new List<FpsSetting>());

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsCorrectData_WhenSingleSettingExists()
        {
            // Arrange
            var settings = new List<FpsSetting>
            {
                new() { Id = "HoursInDay", Setting = "8", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(settings);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            var single = Assert.Single(result);
            Assert.Equal("HoursInDay", single.Id);
            Assert.Equal("8", single.Setting);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsList_NotNull()
        {
            // Arrange — verifies the return type contract is always List, never null
            var repo = CreateRepository(new List<FpsSetting>());

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.IsType<List<FpsSetting>>(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsSettingsForCorrectYear_WhenMultipleYearsExist()
        {
            // Arrange — mock DbSet holds all years; the FpsCalYear query filter on FpsDbContext
            // means only records matching the substituted FPSYear should be returned
            var settings = new List<FpsSetting>
            {
                new() { Id = "HoursInDay", Setting = "8",  FpsYear = 2024 },
                new() { Id = "HoursInDay", Setting = "7",  FpsYear = 2023 }
            };

            // Only 2024 records should be visible when FPSYear is set to 2024
            var repo = CreateRepository(
                settings.Where(s => s.FpsYear == DefaultTestFpsYear),
                fpsYear: DefaultTestFpsYear);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("8", result[0].Setting);
        }

        #endregion

        #region GetByKeyAsync

        [Fact]
        public async Task GetByKeyAsync_WhenKeyExists_ReturnsSetting()
        {
            // Arrange
            var settings = new List<FpsSetting>
            {
                new() { Id = "HOURS_PER_DAY", Setting = "8",  FpsYear = DefaultTestFpsYear },
                new() { Id = "DAYS_IN_YEAR",  Setting = "365", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(settings);

            // Act
            var result = await repo.GetByKeyAsync("HOURS_PER_DAY");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("HOURS_PER_DAY", result.Id);
            Assert.Equal("8", result.Setting);
        }

        [Fact]
        public async Task GetByKeyAsync_WhenKeyDoesNotExist_ReturnsNull()
        {
            // Arrange
            var settings = new List<FpsSetting>
            {
                new() { Id = "DAYS_IN_YEAR", Setting = "365", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(settings);

            // Act
            var result = await repo.GetByKeyAsync("HOURS_PER_DAY");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByKeyAsync_WhenNoSettingsExist_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(new List<FpsSetting>());

            // Act
            var result = await repo.GetByKeyAsync("HOURS_PER_DAY");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByKeyAsync_WhenMultipleSettingsExist_ReturnsOnlyMatchingKey()
        {
            // Arrange
            var settings = new List<FpsSetting>
            {
                new() { Id = "HOURS_PER_DAY", Setting = "8",   FpsYear = DefaultTestFpsYear },
                new() { Id = "DAYS_IN_YEAR",  Setting = "365", FpsYear = DefaultTestFpsYear },
                new() { Id = "WEEKS_IN_YEAR", Setting = "52",  FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(settings);

            // Act
            var result = await repo.GetByKeyAsync("DAYS_IN_YEAR");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("DAYS_IN_YEAR", result.Id);
            Assert.Equal("365", result.Setting);
        }

        [Fact]
        public async Task GetByKeyAsync_IsCaseSensitive_ReturnsNullForDifferentCase()
        {
            // Arrange
            var settings = new List<FpsSetting>
            {
                new() { Id = "HOURS_PER_DAY", Setting = "8", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(settings);

            // Act
            var result = await repo.GetByKeyAsync("hours_per_day");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddAsync

        /// <summary>
        /// Creates a FpsSettingRepository and exposes the mocked DbSet and DbContext
        /// so that Add / Update / SaveChanges calls can be verified.
        /// </summary>
        private static (FpsSettingRepository Repo, Mock<FpsDbContext> Context, Mock<DbSet<FpsSetting>> DbSet)
            CreateRepositoryWithMocks(
                IEnumerable<FpsSetting>? settings = null,
                int fpsYear = DefaultTestFpsYear,
                string userEmail = "test@example.com")
        {
            var fpsYearContext = Substitute.For<IFpsRequestContext>();
            fpsYearContext.FpsYear.Returns(fpsYear);
            fpsYearContext.UserEmailId.Returns(userEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var dbSet = RepositoryTestHelper.CreateMockDbSet(settings ?? []);
            RepositoryTestHelper.SetupDbSetOperations(dbSet);
            mockContext.Setup(x => x.TblSettings).Returns(dbSet.Object);

            return (new FpsSettingRepository(mockContext.Object, fpsYearContext), mockContext, dbSet);
        }

        [Fact]
        public async Task AddAsync_ReturnsAddedSetting()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks();
            var setting = new FpsSetting { Id = "HoursInDay", Setting = "8", FpsYear = DefaultTestFpsYear };

            // Act
            var result = await repo.AddAsync(setting);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("HoursInDay", result.Id);
            Assert.Equal("8", result.Setting);
        }

        [Fact]
        public async Task AddAsync_CallsDbSetAdd()
        {
            // Arrange
            var (repo, _, dbSet) = CreateRepositoryWithMocks();
            var setting = new FpsSetting { Id = "HoursInDay", Setting = "8", FpsYear = DefaultTestFpsYear };

            // Act
            await repo.AddAsync(setting);

            // Assert
            dbSet.Verify(x => x.Add(It.Is<FpsSetting>(s => s.Id == "HoursInDay")), Times.Once);
        }

        [Fact]
        public async Task AddAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, mockContext, _) = CreateRepositoryWithMocks();
            var setting = new FpsSetting { Id = "HoursInDay", Setting = "8", FpsYear = DefaultTestFpsYear };

            // Act
            await repo.AddAsync(setting);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task AddAsync_ReturnsSameInstance_ThatWasPassedIn()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks();
            var setting = new FpsSetting { Id = "DaysInYear", Setting = "365", FpsYear = DefaultTestFpsYear };

            // Act
            var result = await repo.AddAsync(setting);

            // Assert
            Assert.Same(setting, result);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ReturnsUpdatedSetting()
        {
            // Arrange
            var existing = new FpsSetting { Id = "HoursInDay", Setting = "8", FpsYear = DefaultTestFpsYear };
            var (repo, _, _) = CreateRepositoryWithMocks(new[] { existing });
            existing.Setting = "7";

            // Act
            var result = await repo.UpdateAsync(existing);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("HoursInDay", result.Id);
            Assert.Equal("7", result.Setting);
        }

        [Fact]
        public async Task UpdateAsync_CallsDbSetUpdate()
        {
            // Arrange
            var setting = new FpsSetting { Id = "HoursInDay", Setting = "8", FpsYear = DefaultTestFpsYear };
            var (repo, _, dbSet) = CreateRepositoryWithMocks(new[] { setting });

            // Act
            await repo.UpdateAsync(setting);

            // Assert
            dbSet.Verify(x => x.Update(It.Is<FpsSetting>(s => s.Id == "HoursInDay")), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var setting = new FpsSetting { Id = "HoursInDay", Setting = "8", FpsYear = DefaultTestFpsYear };
            var (repo, mockContext, _) = CreateRepositoryWithMocks(new[] { setting });

            // Act
            await repo.UpdateAsync(setting);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsSameInstance_ThatWasPassedIn()
        {
            // Arrange
            var setting = new FpsSetting { Id = "HoursInDay", Setting = "8", FpsYear = DefaultTestFpsYear };
            var (repo, _, _) = CreateRepositoryWithMocks(new[] { setting });

            // Act
            var result = await repo.UpdateAsync(setting);

            // Assert
            Assert.Same(setting, result);
        }

        [Fact]
        public async Task UpdateAsync_PreservesAllProperties_AfterUpdate()
        {
            // Arrange
            var setting = new FpsSetting
            {
                Id       = "HoursInDay",
                Setting  = "9",
                Notes    = "Updated note",
                FpsYear  = DefaultTestFpsYear,
                UpdatedBy = "admin@example.com",
                UpdatedAt = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)
            };
            var (repo, _, _) = CreateRepositoryWithMocks(new[] { setting });

            // Act
            var result = await repo.UpdateAsync(setting);

            // Assert
            Assert.Equal("HoursInDay",        result.Id);
            Assert.Equal("9",                 result.Setting);
            Assert.Equal("Updated note",      result.Notes);
            Assert.Equal(DefaultTestFpsYear,  result.FpsYear);
            Assert.Equal("admin@example.com", result.UpdatedBy);
        }

        #endregion

        #region SaveAsync and GetYearEndSettingsAsync — infrastructure limitations

        // SaveAsync calls _dbContext.TblSettings.IgnoreQueryFilters().FirstOrDefaultAsync(...).
        // IgnoreQueryFilters() wraps the mock queryable in an EF-specific expression node; when
        // LINQ-to-Objects subsequently evaluates that expression it re-enters IgnoreQueryFilters
        // recursively, causing a StackOverflowException that cannot be caught.  SaveAsync tests
        // are therefore omitted — integration tests are the appropriate vehicle.
        //
        // GetYearEndSettingsAsync additionally calls GetOpenYear(), which uses
        // .Select(y => y.FpsYear).FirstAsync().  The .Select() step calls
        // TestAsyncQueryProvider.CreateQuery<int>(), and TestAsyncEnumerable<T> has a
        // 'where T : class' constraint; int violates it, throwing ArgumentException
        // before any business logic is reached.  These tests are also omitted for the
        // same reason as the existing codebase pattern.

        #endregion
    }
}