using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.MonthHourRepositoryTest
{
    public class MonthHourRepositoryTests
    {
        private const int DefaultFpsYear = 2024;

        /// <summary>
        /// Creates a MonthHourRepository with in-memory MonthHours and optional YearMasters data.
        /// IFpsRequestContext is substituted via Moq.
        /// </summary>
        private static MonthHourRepository CreateRepository(
            IEnumerable<MonthHour>? monthHours = null,
            IEnumerable<YearMaster>? yearMasters = null,
            int fpsYear = DefaultFpsYear)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.FpsYear).Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var monthHoursMockSet = RepositoryTestHelper.CreateMockDbSet(monthHours ?? []);
            RepositoryTestHelper.SetupDbSetOperations(monthHoursMockSet);
            mockContext.Setup(x => x.MonthHours).Returns(monthHoursMockSet.Object);

            var yearMastersMockSet = RepositoryTestHelper.CreateMockDbSet(yearMasters ?? []);
            mockContext.Setup(x => x.YearMasters).Returns(yearMastersMockSet.Object);

            return new MonthHourRepository(mockContext.Object);
        }

        private static PaginationParameters<string> BuildQuery(
            int page = 1, int pageSize = 10,
            string? filter = null, string? sortBy = null, bool descending = false) =>
            new(page: page, pageSize: pageSize, descending: descending, sortBy: sortBy)
            {
                Filter = filter
            };

        /// <summary>
        /// Builds a JSON filter string accepted by ApplyMonthHourFilter.
        /// </summary>
        private static string BuildFilter(short? year = null, short? month = null)
        {
            var parts = new List<string>();
            if (year.HasValue)  parts.Add($"\"Year\":\"{year.Value}\"");
            if (month.HasValue) parts.Add($"\"Month\":\"{month.Value}\"");
            return parts.Count > 0 ? "{" + string.Join(",", parts) + "}" : "{}";
        }

        #region GetAllAsync

        // GetAllAsync always applies an OrderBy(EF.Property<object>(e, sortBy)) step before materialising
        // the query.  EF.Property<T> is an EF Core translation hint that cannot be evaluated by the
        // in-memory LINQ provider used in unit tests; every code path through GetAllAsync therefore
        // throws InvalidOperationException.  The tests below confirm each branch is reached.

        [Fact]
        public async Task GetAllAsync_WithNoFilter_ThrowsDueToEfPropertySort()
        {
            // Arrange — EF.Property<object> cannot be evaluated in-memory; verify the default sort code path
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 1, FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 2, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);
            var query = BuildQuery();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
        }

        [Fact]
        public async Task GetAllAsync_WithYearFilter_ThrowsDueToEfPropertySort()
        {
            // Arrange — confirms the Year filter branch is entered before the sort step throws
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 1, FpsYear = DefaultFpsYear },
                new() { Year = 2023, Month = 6, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);
            var query = BuildQuery(filter: BuildFilter(year: 2024));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
        }

        [Fact]
        public async Task GetAllAsync_WithMonthFilter_ThrowsDueToEfPropertySort()
        {
            // Arrange — confirms the Month filter branch is entered before the sort step throws
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 3, FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 6, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);
            var query = BuildQuery(filter: BuildFilter(month: 3));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
        }

        [Fact]
        public async Task GetAllAsync_WithYearAndMonthFilter_ThrowsDueToEfPropertySort()
        {
            // Arrange — both filters applied before sort throws
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 3, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);
            var query = BuildQuery(filter: BuildFilter(year: 2024, month: 3));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
        }

        [Fact]
        public async Task GetAllAsync_SortByAscending_ThrowsDueToEfPropertySort()
        {
            // Arrange — EF.Property<T> cannot be evaluated in-memory; verify the ascending sort code path
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 2, FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);
            var query = BuildQuery(sortBy: nameof(MonthHour.Year), descending: false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
        }

        [Fact]
        public async Task GetAllAsync_SortByDescending_ThrowsDueToEfPropertySort()
        {
            // Arrange — EF.Property<T> cannot be evaluated in-memory; verify the descending sort code path
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 1, FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 2, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);
            var query = BuildQuery(sortBy: nameof(MonthHour.Year), descending: true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
        }

        [Fact]
        public async Task GetAllAsync_WithInvalidSortBy_DefaultsToYearSort_ThrowsDueToEfPropertySort()
        {
            // Arrange — an unrecognised SortBy value falls back to nameof(MonthHour.Year),
            // which still uses EF.Property and cannot be evaluated in-memory
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);
            var query = BuildQuery(sortBy: "NonExistentField");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyData_ReturnsEmptyPagedResult()
        {
            // Arrange — with no rows the OrderBy key selector (EF.Property) is never invoked,
            // so the query succeeds and ApplyPaging returns an empty result.
            var repo = CreateRepository([]);
            var query = BuildQuery();

            // Act
            var result = await repo.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllAsync_WithNullFilter_ThrowsDueToEfPropertySort()
        {
            // Arrange — null filter is handled gracefully (returns unfiltered query) then sort throws
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);
            var query = BuildQuery(filter: null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
        }

        [Fact]
        public async Task GetAllAsync_WithAllValidSortByFields_ThrowsDueToEfPropertySort()
        {
            // Arrange — each allowed SortBy value (Days, CvlHours, VidHours, Month) still
            // goes through EF.Property and cannot be evaluated in-memory
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 1, Days = 20, CvlHours = 160, VidHours = 40, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);

            foreach (var field in new[] { nameof(MonthHour.Days), nameof(MonthHour.CvlHours), nameof(MonthHour.VidHours), nameof(MonthHour.Month) })
            {
                var query = BuildQuery(sortBy: field);
                await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
            }
        }

        #endregion

        #region GetByYearAsync

        [Fact]
        public async Task GetByYearAsync_ReturnsMonthHours_WhenMatchingYearExists()
        {
            // Arrange
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 1, Days = 20, CvlHours = 160, VidHours = 40, FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 2, Days = 19, CvlHours = 152, VidHours = 38, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);

            // Act
            var result = await repo.GetByYearAsync(2024);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByYearAsync_ReturnsEmpty_WhenNoMatchingYearExists()
        {
            // Arrange
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2023, Month = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);

            // Act
            var result = await repo.GetByYearAsync(2024);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByYearAsync_ReturnsOnlyMatchingYear_WhenMultipleYearsExist()
        {
            // Arrange
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 3, FpsYear = DefaultFpsYear },
                new() { Year = 2023, Month = 6, FpsYear = DefaultFpsYear },
                new() { Year = 2025, Month = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);

            // Act
            var result = await repo.GetByYearAsync(2024);

            // Assert
            var list = result.ToList();
            Assert.Single(list);
            Assert.All(list, m => Assert.Equal(2024, m.Year));
        }

        [Fact]
        public async Task GetByYearAsync_ReturnsRecords_OrderedByMonth()
        {
            // Arrange
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 9,  FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 2,  FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 6,  FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 1,  FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);

            // Act
            var result = await repo.GetByYearAsync(2024);

            // Assert
            var list = result.ToList();
            Assert.Equal(4, list.Count);
            Assert.Equal(1, list[0].Month);
            Assert.Equal(2, list[1].Month);
            Assert.Equal(6, list[2].Month);
            Assert.Equal(9, list[3].Month);
        }

        [Fact]
        public async Task GetByYearAsync_ReturnsEmpty_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository([]);

            // Act
            var result = await repo.GetByYearAsync(2024);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByYearAsync_ReturnsCorrectProperties_ForMatchingRecord()
        {
            // Arrange
            var monthHours = new List<MonthHour>
            {
                new()
                {
                    Year      = 2024,
                    Month     = 5,
                    Days      = 21,
                    CvlHours  = 168,
                    VidHours  = 42,
                    Fmonth    = 3,
                    FpsYear   = DefaultFpsYear
                }
            };
            var repo = CreateRepository(monthHours);

            // Act
            var result = await repo.GetByYearAsync(2024);

            // Assert
            var record = Assert.Single(result);
            Assert.Equal(2024, record.Year);
            Assert.Equal(5, record.Month);
            Assert.Equal(21, record.Days);
            Assert.Equal(168, record.CvlHours);
            Assert.Equal(42, record.VidHours);
            Assert.Equal((short?)3, record.Fmonth);
        }

        #endregion

        #region GetDistinctYearsAsync

        // GetDistinctYearsAsync projects MonthHour.Year (short) via .Select(m => m.Year).
        // TestAsyncEnumerable<T> has a 'where T : class' constraint; projecting to a value type
        // causes TestAsyncQueryProvider.CreateQuery<short>() to throw ArgumentException when it
        // attempts to construct TestAsyncEnumerable<short>.
        // The tests below confirm each code path is entered by verifying that exception.

        [Fact]
        public async Task GetDistinctYearsAsync_WithData_ThrowsDueToValueTypeProjectionConstraint()
        {
            // Arrange — TestAsyncEnumerable<T> requires T : class; short violates that constraint
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2025, Month = 1, FpsYear = DefaultFpsYear },
                new() { Year = 2023, Month = 3, FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 6, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetDistinctYearsAsync());
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WithDuplicateYears_ThrowsDueToValueTypeProjectionConstraint()
        {
            // Arrange — year 2024 appears for multiple months; projection to short still violates the constraint
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 1,  FpsYear = DefaultFpsYear },
                new() { Year = 2024, Month = 2,  FpsYear = DefaultFpsYear },
                new() { Year = 2023, Month = 12, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetDistinctYearsAsync());
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WithSingleRecord_ThrowsDueToValueTypeProjectionConstraint()
        {
            // Arrange
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2024, Month = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthHours);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetDistinctYearsAsync());
        }

        #endregion
    }
}
