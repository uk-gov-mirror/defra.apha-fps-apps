using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.MonthlyOutputRepositoryTest
{
    public class MonthlyOutputRepositoryTests
    {
        private const int DefaultFpsYear = 2024;

        private static MonthlyOutputRepository CreateRepository(
            IEnumerable<MonthlyOutputLog> logs)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(logs);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);

            mockContext.Setup(x => x.MonthlyOutputLogs).Returns(mockSet.Object);

            return new MonthlyOutputRepository(mockContext.Object, fpsRequestContext);
        }

        private static MonthlyOutputRepository CreateRepositoryWithOutputs(
            IEnumerable<MonthlyOutput> monthlyOutputs)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(monthlyOutputs);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);

            mockContext.Setup(x => x.MonthlyOutputs).Returns(mockSet.Object);

            return new MonthlyOutputRepository(mockContext.Object, fpsRequestContext);
        }

        private static PaginationParameters<string> DefaultQuery(int page = 1, int pageSize = 10)
            => new(page: page, pageSize: pageSize);

        private static List<MonthlyOutputLog> SeedData() =>
        [
            new() { SequenceNo = 1, WorkGroup = "WG1", TestCode = "TC1", Buyer = "BUYER_A",  Month = 1,  DateTime = new DateTime(2024, 1, 15), UserId = "SP001", InsertDelete = "I", FpsYear = DefaultFpsYear },
            new() { SequenceNo = 2, WorkGroup = "WG1", TestCode = "TC2", Buyer = "BUYER_B",  Month = 2,  DateTime = new DateTime(2024, 2, 10), UserId = "SP002", InsertDelete = "D", FpsYear = DefaultFpsYear },
            new() { SequenceNo = 3, WorkGroup = "WG2", TestCode = "TC1", Buyer = "BUYER_A",  Month = 3,  DateTime = new DateTime(2024, 3, 20), UserId = "SP001", InsertDelete = "U", FpsYear = DefaultFpsYear },
            new() { SequenceNo = 4, WorkGroup = "WG2", TestCode = "TC3", Buyer = "BUYER_C",  Month = 4,  DateTime = new DateTime(2024, 4, 5),  UserId = "SP003", InsertDelete = "I", FpsYear = DefaultFpsYear },
            new() { SequenceNo = 5, WorkGroup = "WG3", TestCode = "TC4", Buyer = "BUYER_D",  Month = 5,  DateTime = new DateTime(2024, 5, 1),  UserId = null,    InsertDelete = null, FpsYear = DefaultFpsYear },
        ];

        private static List<MonthlyOutput> MonthlyOutputSeedData() =>
        [
            new() { TestCode = "TC1", WorkGroup = "WG1", Buyer = "BUYER_A", Month = 1, FpsYear = DefaultFpsYear },
            new() { TestCode = "TC1", WorkGroup = "WG2", Buyer = "BUYER_B", Month = 2, FpsYear = DefaultFpsYear },
            new() { TestCode = "TC2", WorkGroup = "WG1", Buyer = "BUYER_C", Month = 3, FpsYear = DefaultFpsYear },
        ];

        #region GetMonthlyOutputLogAsync — no filters

        [Fact]
        public async Task GetMonthlyOutputLogAsync_NoFilters_ReturnsAllRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, null, null);

            Assert.NotNull(result);
            Assert.Equal(5, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_EmptyRepository_ReturnsEmptyResult()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, null, null);

            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — WorkGroup filter

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByWorkGroup_ReturnsMatchingRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), "WG1", null, null, null, null, null, null);

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, r => Assert.Equal("WG1", r.WorkGroup));
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByWorkGroup_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), "WG_NONE", null, null, null, null, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByWorkGroup_NullValue_ReturnsAllRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — TestCode filter

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByTestCode_ReturnsMatchingRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, "TC1", null, null, null, null, null);

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, r => Assert.Equal("TC1", r.TestCode));
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByTestCode_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, "TC_NONE", null, null, null, null, null);

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — Buyer filter

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByBuyer_ReturnsMatchingRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, "BUYER_A", null, null, null, null);

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, r => Assert.Equal("BUYER_A", r.Buyer));
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByBuyer_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, "BUYER_NONE", null, null, null, null);

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — DateImported filter

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByDateImported_ReturnsMatchingRows()
        {
            var repo = CreateRepository(SeedData());
            var targetDate = new DateTime(2024, 1, 15);

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, targetDate, null, null, null);

            Assert.Single(result.Data);
            Assert.Equal(targetDate.Date, result.Data.First().DateTime!.Value.Date);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByDateImported_DatePartOnly_IgnoresTime()
        {
            var data = new List<MonthlyOutputLog>
            {
                new() { SequenceNo = 1, DateTime = new DateTime(2024, 6, 1, 9, 30, 0),  FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, DateTime = new DateTime(2024, 6, 1, 18, 0, 0),  FpsYear = DefaultFpsYear },
                new() { SequenceNo = 3, DateTime = new DateTime(2024, 6, 2, 9, 0, 0),   FpsYear = DefaultFpsYear },
            };
            var repo = CreateRepository(data);

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, new DateTime(2024, 6, 1), null, null, null);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByDateImported_NullDateTime_NotIncluded()
        {
            var data = new List<MonthlyOutputLog>
            {
                new() { SequenceNo = 1, DateTime = null,                    FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, DateTime = new DateTime(2024, 6, 1), FpsYear = DefaultFpsYear },
            };
            var repo = CreateRepository(data);

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, new DateTime(2024, 6, 1), null, null, null);

            Assert.Single(result.Data);
            Assert.Equal(2, result.Data.First().SequenceNo);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByDateImported_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, new DateTime(2099, 1, 1), null, null, null);

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — Month filter

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByMonth_ReturnsMatchingRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, 1, null, null);

            Assert.Single(result.Data);
            Assert.Equal(1, result.Data.First().Month);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByMonth_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, 99, null, null);

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — UserId filter

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByUserId_ExactMatch_ReturnsMatchingRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, "SP001", null);

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, r => Assert.Contains("SP001", r.UserId));
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByUserId_PartialMatch_ReturnsMatchingRows()
        {
            var data = new List<MonthlyOutputLog>
            {
                new() { SequenceNo = 1, UserId = "SP001", FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, UserId = "SP001-TEMP", FpsYear = DefaultFpsYear },
                new() { SequenceNo = 3, UserId = "SP999", FpsYear = DefaultFpsYear },
            };
            var repo = CreateRepository(data);

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, "SP001", null);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByUserId_NullUserId_NotIncluded()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, "SP001", null);

            Assert.DoesNotContain(result.Data, r => r.UserId == null);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByUserId_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, "SP_NONE", null);

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — InsertDelete filter

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByInsertDelete_I_ReturnsInsertedRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, null, "I");

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, r => Assert.StartsWith("I", r.InsertDelete));
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByInsertDelete_D_ReturnsDeletedRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, null, "D");

            Assert.Single(result.Data);
            Assert.Equal("D", result.Data.First().InsertDelete);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByInsertDelete_NullValue_NotIncluded()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, null, "I");

            Assert.DoesNotContain(result.Data, r => r.InsertDelete == null);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_FilterByInsertDelete_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, null, "X");

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — combined filters

        [Fact]
        public async Task GetMonthlyOutputLogAsync_CombineWorkGroupAndTestCode_ReturnsIntersection()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), "WG1", "TC1", null, null, null, null, null);

            Assert.Single(result.Data);
            Assert.Equal("WG1", result.Data.First().WorkGroup);
            Assert.Equal("TC1", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_CombineWorkGroupAndBuyer_ReturnsIntersection()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), "WG2", null, "BUYER_A", null, null, null, null);

            Assert.Single(result.Data);
            Assert.Equal("WG2", result.Data.First().WorkGroup);
            Assert.Equal("BUYER_A", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_AllFiltersSet_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(
                DefaultQuery(),
                workGroup: "WG1",
                testCode: "TC1",
                buyer: "BUYER_C",
                dateImported: new DateTime(2024, 1, 15),
                month: 1,
                userId: "SP001",
                insertDelete: "I");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_AllFiltersSet_SingleMatch_ReturnsThatRow()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(
                DefaultQuery(),
                workGroup: "WG1",
                testCode: "TC1",
                buyer: "BUYER_A",
                dateImported: new DateTime(2024, 1, 15),
                month: 1,
                userId: "SP001",
                insertDelete: "I");

            Assert.Single(result.Data);
            Assert.Equal(1, result.Data.First().SequenceNo);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — ordering

        [Fact]
        public async Task GetMonthlyOutputLogAsync_ResultsOrderedByDateTimeDescending()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, null, null);

            var dateTimes = result.Data
                .Where(r => r.DateTime.HasValue)
                .Select(r => r.DateTime!.Value)
                .ToList();

            Assert.Equal(dateTimes.OrderByDescending(d => d).ToList(), dateTimes);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_SameDateTimeTiedBySequenceNoAscending()
        {
            var sameDate = new DateTime(2024, 7, 1);
            var data = new List<MonthlyOutputLog>
            {
                new() { SequenceNo = 3, DateTime = sameDate, FpsYear = DefaultFpsYear },
                new() { SequenceNo = 1, DateTime = sameDate, FpsYear = DefaultFpsYear },
                new() { SequenceNo = 2, DateTime = sameDate, FpsYear = DefaultFpsYear },
            };
            var repo = CreateRepository(data);

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(), null, null, null, null, null, null, null);

            var seqNos = result.Data.Select(r => r.SequenceNo).ToList();
            Assert.Equal([1, 2, 3], seqNos);
        }

        #endregion

        #region GetMonthlyOutputLogAsync — pagination

        [Fact]
        public async Task GetMonthlyOutputLogAsync_PaginationPage1_ReturnsFirstPageRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(page: 1, pageSize: 2), null, null, null, null, null, null, null);

            Assert.Equal(2, result.Data.Count);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_PaginationPage2_ReturnsSecondPageRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(page: 2, pageSize: 2), null, null, null, null, null, null, null);

            Assert.Equal(2, result.Data.Count);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_LastPageWithFewerRows_ReturnsRemainingRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(page: 3, pageSize: 2), null, null, null, null, null, null, null);

            Assert.Single(result.Data);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_PageSizeLargerThanData_ReturnsAllRows()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(page: 1, pageSize: 100), null, null, null, null, null, null, null);

            Assert.Equal(5, result.Data.Count);
            Assert.Equal(1, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetMonthlyOutputLogAsync_PaginationMetadata_IsCorrect()
        {
            var repo = CreateRepository(SeedData());

            var result = await repo.GetMonthlyOutputLogAsync(DefaultQuery(page: 2, pageSize: 3), null, null, null, null, null, null, null);

            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(3, result.PaginationData.PageSize);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.TotalPages);
        }

        #endregion

        #region ExistsByTestCodeAndWorkGroupAsync

        [Fact]
        public async Task ExistsByTestCodeAndWorkGroupAsync_MatchingTestCodeAndWorkGroup_ReturnsTrue()
        {
            var repo = CreateRepositoryWithOutputs(MonthlyOutputSeedData());

            var result = await repo.ExistsByTestCodeAndWorkGroupAsync("TC1", "WG1");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByTestCodeAndWorkGroupAsync_NonMatchingTestCode_ReturnsFalse()
        {
            var repo = CreateRepositoryWithOutputs(MonthlyOutputSeedData());

            var result = await repo.ExistsByTestCodeAndWorkGroupAsync("UNKNOWN", "WG1");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByTestCodeAndWorkGroupAsync_NonMatchingWorkGroup_ReturnsFalse()
        {
            var repo = CreateRepositoryWithOutputs(MonthlyOutputSeedData());

            var result = await repo.ExistsByTestCodeAndWorkGroupAsync("TC1", "UNKNOWN");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByTestCodeAndWorkGroupAsync_EmptyRepository_ReturnsFalse()
        {
            var repo = CreateRepositoryWithOutputs([]);

            var result = await repo.ExistsByTestCodeAndWorkGroupAsync("TC1", "WG1");

            Assert.False(result);
        }

        #endregion

        #region LiveRecordExistsAsync

        [Fact]
        public async Task LiveRecordExistsAsync_WithMatchingCompositeKey_ReturnsTrue()
        {
            var repo = CreateRepositoryWithOutputs(MonthlyOutputSeedData());

            var result = await repo.LiveRecordExistsAsync("TC1", "BUYER_A", 1, "WG1");

            Assert.True(result);
        }

        [Fact]
        public async Task LiveRecordExistsAsync_WithNonMatchingCompositeKey_ReturnsFalse()
        {
            var repo = CreateRepositoryWithOutputs(MonthlyOutputSeedData());

            var result = await repo.LiveRecordExistsAsync("TC1", "BUYER_X", 1, "WG1");

            Assert.False(result);
        }

        #endregion

        #region GetLiveByKeyAsync

        [Fact]
        public async Task GetLiveByKeyAsync_WithMatchingCompositeKey_ReturnsEntity()
        {
            var repo = CreateRepositoryWithOutputs(MonthlyOutputSeedData());

            var result = await repo.GetLiveByKeyAsync("TC1", "BUYER_A", 1, "WG1");

            Assert.NotNull(result);
            Assert.Equal("TC1", result!.TestCode);
            Assert.Equal("BUYER_A", result.Buyer);
            Assert.Equal("WG1", result.WorkGroup);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WithNonMatchingCompositeKey_ReturnsNull()
        {
            var repo = CreateRepositoryWithOutputs(MonthlyOutputSeedData());

            var result = await repo.GetLiveByKeyAsync("TC9", "BUYER_A", 1, "WG1");

            Assert.Null(result);
        }

        #endregion
    }
}
