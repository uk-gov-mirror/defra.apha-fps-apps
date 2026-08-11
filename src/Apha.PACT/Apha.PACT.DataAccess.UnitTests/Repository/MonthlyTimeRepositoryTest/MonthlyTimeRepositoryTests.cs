using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.MonthlyTimeRepositoryTest
{
    public class MonthlyTimeRepositoryTests
    {
        private const int DefaultFpsYear = 2024;

        private static MonthlyTimeRepository CreateRepository(
            IEnumerable<MonthlyTime> monthlyTimes,
            IEnumerable<MonthlyTimeLog>? monthlyTimeLogs = null)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var mockSet = RepositoryTestHelper.CreateMockDbSet(monthlyTimes);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);
            mockContext.Setup(x => x.MonthlyTimes).Returns(mockSet.Object);

            var logMockSet = RepositoryTestHelper.CreateMockDbSet(monthlyTimeLogs ?? []);
            RepositoryTestHelper.SetupDbSetOperations(logMockSet);
            mockContext.Setup(x => x.MonthlyTimeLogs).Returns(logMockSet.Object);

            return new MonthlyTimeRepository(mockContext.Object, fpsRequestContext);
        }

        // Shared log data used across SearchAsync tests
        private static readonly DateTime BaseDate = new(2024, 6, 15, 10, 0, 0);

        private static List<MonthlyTimeLog> DefaultLogs() =>
        [
            new() { SequenceNo = 1, WorkGroup = "WGA", TimeCode = "TC1", PactStaffId = "S1", ParentProject = "PP1", Month = 6, DateTime = BaseDate,          UserId = "CVLNT\\mUser1", InsertDelete = "I",  FpsYear = DefaultFpsYear },
            new() { SequenceNo = 2, WorkGroup = "WGB", TimeCode = "TC2", PactStaffId = "S2", ParentProject = "PP2", Month = 7, DateTime = BaseDate.AddDays(1), UserId = "CVLNT\\mUser2", InsertDelete = "D",  FpsYear = DefaultFpsYear },
            new() { SequenceNo = 3, WorkGroup = "WGA", TimeCode = "TC3", PactStaffId = "S3", ParentProject = "PP1", Month = 8, DateTime = BaseDate.AddDays(2), UserId = "CVLNT\\mUser3", InsertDelete = "UI", FpsYear = DefaultFpsYear }
        ];

        #region HasMonthlyTimeEntriesAsync

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_MatchingAllThreeFields_ReturnsTrue()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG1", "TC1", "PP1");

            Assert.True(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_NoMatchingRows_ReturnsFalse()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG2", "TC2", "PP2");

            Assert.False(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_WorkGroupDiffers_ReturnsFalse()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG_DIFFERENT", "TC1", "PP1");

            Assert.False(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_TimeCodeDiffers_ReturnsFalse()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG1", "TC_DIFFERENT", "PP1");

            Assert.False(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_ParentProjectDiffers_ReturnsFalse()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG1", "TC1", "PP_DIFFERENT");

            Assert.False(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_EmptyRepository_ReturnsFalse()
        {
            var repo = CreateRepository(Enumerable.Empty<MonthlyTime>());

            var result = await repo.HasMonthlyTimeEntriesAsync("WG1", "TC1", "PP1");

            Assert.False(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_MultipleRows_OnlyOneMatches_ReturnsTrue()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG2", TimeCode = "TC2", ParentProject = "PP2", PactStaffId = "S2", Month = 2, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG1", "TC1", "PP1");

            Assert.True(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_MultipleMatchingRows_ReturnsTrue()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S2", Month = 2, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG1", "TC1", "PP1");

            Assert.True(result);
        }

        #endregion

        #region SearchAsync — no filters

        [Fact]
        public async Task SearchAsync_NoFilters_ReturnsAllRecords()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter());

            Assert.Equal(3, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task SearchAsync_EmptyLogs_ReturnsEmptyResult()
        {
            var repo = CreateRepository([], []);
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter());

            Assert.Empty(result.Data);
        }

        #endregion

        #region SearchAsync — individual filters

        [Fact]
        public async Task SearchAsync_FilterByWorkGroup_ReturnsMatchingRecords()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { WorkGroup = "WGA" });

            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, r => Assert.Equal("WGA", r.WorkGroup));
        }

        [Fact]
        public async Task SearchAsync_FilterByWorkGroup_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { WorkGroup = "NONE" });

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task SearchAsync_FilterByTimeCode_ReturnsMatchingRecord()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { TimeCode = "TC1" });

            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal("TC1", result.Data.First().TimeCode);
        }

        [Fact]
        public async Task SearchAsync_FilterByPactStaffId_ReturnsMatchingRecord()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { PactStaffId = "S2" });

            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal("S2", result.Data.First().PactStaffId);
        }

        [Fact]
        public async Task SearchAsync_FilterByParentProject_ReturnsMatchingRecords()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { ParentProject = "PP1" });

            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, r => Assert.Equal("PP1", r.ParentProject));
        }

        [Fact]
        public async Task SearchAsync_FilterByMonth_ReturnsMatchingRecord()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { Month = 7 });

            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal(7, result.Data.First().Month);
        }

        [Fact]
        public async Task SearchAsync_FilterByUserId_PartialMatch_ReturnsMatchingRecord()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { UserId = "mUser1" });

            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Contains("mUser1", result.Data.First().UserId);
        }

        [Theory]
        [InlineData("I",  1)]   // exact prefix "I"
        [InlineData("D",  1)]   // exact prefix "D"
        [InlineData("UI", 1)]   // exact prefix "UI"
        public async Task SearchAsync_FilterByInsertDelete_ReturnsMatchingRecords(
            string insertDelete, int expectedCount)
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { InsertDelete = insertDelete });

            Assert.Equal(expectedCount, result.PaginationData.TotalRecords);
            Assert.All(result.Data, r => Assert.StartsWith(insertDelete, r.InsertDelete));
        }

        #endregion

        #region SearchAsync — dateImported filter

        [Fact]
        public async Task SearchAsync_FilterByDateImported_MatchingDate_ReturnsRecord()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            // BaseDate is 2024-06-15; pass any time on the same calendar day
            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { DateImported = BaseDate.Date });

            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal(BaseDate.Date, result.Data.First().DateTime!.Value.Date);
        }

        [Fact]
        public async Task SearchAsync_FilterByDateImported_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { DateImported = new DateTime(2000, 1, 1) });

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task SearchAsync_FilterByDateImported_NullDateTime_ExcludesRecord()
        {
            var logs = new List<MonthlyTimeLog>
            {
                new() { SequenceNo = 1, TimeCode = "TC1", PactStaffId = "S1", ParentProject = "PP1", Month = 1, DateTime = null, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository([], logs);
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter { DateImported = BaseDate.Date });

            Assert.Empty(result.Data);
        }

        #endregion

        #region SearchAsync — combined filters

        [Fact]
        public async Task SearchAsync_MultipleFilters_ReturnsIntersection()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query,
                new MonthlyTimeLogFilter { WorkGroup = "WGA", TimeCode = "TC1", ParentProject = "PP1" });

            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal("TC1", result.Data.First().TimeCode);
        }

        [Fact]
        public async Task SearchAsync_MultipleFilters_NoIntersection_ReturnsEmpty()
        {
            var repo = CreateRepository([], DefaultLogs());
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query,
                new MonthlyTimeLogFilter { WorkGroup = "WGA", TimeCode = "TC2" });

            Assert.Empty(result.Data);
        }

        #endregion

        #region SearchAsync — ordering and paging

        [Fact]
        public async Task SearchAsync_OrderedByDateTimeDescThenSequenceNoAsc()
        {
            var logs = new List<MonthlyTimeLog>
            {
                new() { SequenceNo = 1, TimeCode = "TC1", PactStaffId = "S1", ParentProject = "PP1", Month = 1, DateTime = BaseDate,          FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, TimeCode = "TC2", PactStaffId = "S2", ParentProject = "PP2", Month = 2, DateTime = BaseDate.AddDays(2), FpsYear = DefaultFpsYear },
                new() { SequenceNo = 3, TimeCode = "TC3", PactStaffId = "S3", ParentProject = "PP3", Month = 3, DateTime = BaseDate.AddDays(2), FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository([], logs);
            var query = new PaginationParameters<string>();

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter());
            var data = result.Data.ToList();

            // Most recent date first; within same date, ascending SequenceNo
            Assert.Equal(2, data[0].SequenceNo);
            Assert.Equal(3, data[1].SequenceNo);
            Assert.Equal(1, data[2].SequenceNo);
        }

        [Fact]
        public async Task SearchAsync_Paging_ReturnsCorrectPage()
        {
            var logs = Enumerable.Range(1, 10)
                .Select(i => new MonthlyTimeLog
                {
                    SequenceNo = i,
                    TimeCode = "TC",
                    PactStaffId = "S",
                    ParentProject = "PP",
                    Month = 1,
                    DateTime = BaseDate.AddMinutes(-i),
                    FpsYear = DefaultFpsYear
                })
                .ToList();

            var repo = CreateRepository([], logs);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 3 };

            var result = await repo.SearchAsync(query, new MonthlyTimeLogFilter());

            Assert.Equal(10, result.PaginationData.TotalRecords);
            Assert.Equal(3, result.Data.Count());
        }

        #endregion

        #region GetLiveByKeyAsync

        [Fact]
        public async Task GetLiveByKeyAsync_WithMatchingCompositeKey_ReturnsEntity()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1", FpsYear = DefaultFpsYear },
                new() { PactStaffId = "S2", TimeCode = "TC2", ParentProject = "PP2", Month = 7, WorkGroup = "WG2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthlyTimes);

            var result = await repo.GetLiveByKeyAsync("S1", "TC1", 6, "PP1");

            Assert.NotNull(result);
            Assert.Equal("S1", result!.PactStaffId);
            Assert.Equal("TC1", result.TimeCode);
            Assert.Equal("PP1", result.ParentProject);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WithNonMatchingCompositeKey_ReturnsNull()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(monthlyTimes);

            var result = await repo.GetLiveByKeyAsync("S9", "TC1", 6, "PP1");

            Assert.Null(result);
        }

        #endregion
    }
}

