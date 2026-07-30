    using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.WorkGroupRepositoryTest
{
    public class WorkGroupRepositoryTests
    {
        // ── Factories ────────────────────────────────────────────────────────

        /// <summary>
        /// WorkGroupRepository has no IFpsYearContext dependency and only reads data.
        /// All query logic (AsNoTracking, OrderBy, ToListAsync) is exercised through the mock DbSet.
        /// </summary>
        private static (
            WorkGroupRepository Repo,
            Mock<DbSet<WorkGroup>> WorkGroupsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<WorkGroup> workGroups)
        {
            var fpsYearContext = Substitute.For<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);
            var workGroupsMockSet = RepositoryTestHelper.CreateMockDbSet(workGroups);

            mockContext.Setup(x => x.WorkGroups).Returns(workGroupsMockSet.Object);

            var repo = new WorkGroupRepository(mockContext.Object, fpsYearContext);
            return (repo, workGroupsMockSet, mockContext);
        }

        private static WorkGroupRepository CreateRepository(IEnumerable<WorkGroup> workGroups)
            => CreateRepositoryWithMocks(workGroups).Repo;

        // ── Time-code repository factory (three-set join) ────────────────────

        private static PactWorkGroupGradeView GradeView(string wgGrade, string workGroup) =>
            new() { WgGrade = wgGrade, WorkGroup = workGroup };

        private static WorkGroupStaffView StaffView(string pactId, string name, string workGroupGrade) =>
            new() { PactId = pactId, Name = name, WorkGroupGrade = workGroupGrade };

        private static MonthlyTime TimeRecord(
            string pactStaffId,
            string parentProject,
            string timeCode,
            double month,
            double? hours = 8.0) =>
            new()
            {
                PactStaffId   = pactStaffId,
                ParentProject = parentProject,
                TimeCode      = timeCode,
                Month         = month,
                Hours         = hours
            };

        private static WorkGroupRepository CreateTimeCodeRepository(
            IEnumerable<PactWorkGroupGradeView> gradeViews,
            IEnumerable<WorkGroupStaffView>     staffViews,
            IEnumerable<MonthlyTime>            monthlyTimes)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var gradeViewSet   = RepositoryTestHelper.CreateMockDbSet(gradeViews);
            var staffViewSet   = RepositoryTestHelper.CreateMockDbSet(staffViews);
            var monthlyTimeSet = RepositoryTestHelper.CreateMockDbSet(monthlyTimes);
            var workGroupSet   = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<WorkGroup>());

            mockContext.Setup(x => x.PactWorkGroupGradeViews).Returns(gradeViewSet.Object);
            mockContext.Setup(x => x.WorkGroupStaffViews).Returns(staffViewSet.Object);
            mockContext.Setup(x => x.MonthlyTimes).Returns(monthlyTimeSet.Object);
            mockContext.Setup(x => x.WorkGroups).Returns(workGroupSet.Object);

            return new WorkGroupRepository(mockContext.Object, fpsRequestContext);
        }

        /// <summary>Minimal single-row joined dataset.</summary>
        private static WorkGroupRepository CreateDefaultTimeCodeRepository(
            string workGroup     = "WG1",
            string wgGrade       = "G1",
            string pactId        = "S1",
            string name          = "Alice",
            string parentProject = "PP1",
            string timeCode      = "TC1",
            double month         = 1,
            double? hours        = 8.0) =>
            CreateTimeCodeRepository(
                [GradeView(wgGrade, workGroup)],
                [StaffView(pactId, name, wgGrade)],
                [TimeRecord(pactId, parentProject, timeCode, month, hours)]);

        #region GetAllWorkGroupsAsync

        [Fact]
        public async Task GetAllWorkGroupsAsync_WithData_ReturnsOrderedList()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "ZGroup", ProfitCentre = "PC2" },
                new() { WorkGroupName = "AGroup", ProfitCentre = "PC1" },
                new() { WorkGroupName = "MGroup", ProfitCentre = "PC3" }
            };
            var repo = CreateRepository(workGroups);

            var result = (await repo.GetAllWorkGroupsAsync()).ToList();

            Assert.Equal(3, result.Count);
            // OrderBy(w => w.WorkGroupName) applied in repository
            Assert.Equal("AGroup", result[0].WorkGroupName);
            Assert.Equal("MGroup", result[1].WorkGroupName);
            Assert.Equal("ZGroup", result[2].WorkGroupName);
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_EmptyData_ReturnsEmptyList()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetAllWorkGroupsAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_SingleEntry_ReturnsSingleItem()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1" }
            };
            var repo = CreateRepository(workGroups);

            var result = (await repo.GetAllWorkGroupsAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("WG1", result[0].WorkGroupName);
        }

        #endregion

        #region GetAllWorkGroupNamesAsync

        [Fact]
        public async Task GetAllWorkGroupNamesAsync_WithData_ReturnsAlphabeticallySortedNames()
        {
            // Arrange
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "ZGroup", ProfitCentre = "PC1" },
                new() { WorkGroupName = "AGroup", ProfitCentre = "PC1" },
                new() { WorkGroupName = "MGroup", ProfitCentre = "PC1" }
            };
            var repo = CreateRepository(workGroups);

            // Act
            var result = await repo.GetAllWorkGroupNamesAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("AGroup", result[0]);
            Assert.Equal("MGroup", result[1]);
            Assert.Equal("ZGroup", result[2]);
        }

        [Fact]
        public async Task GetAllWorkGroupNamesAsync_EmptyData_ReturnsEmptyList()
        {
            // Arrange
            var repo = CreateRepository([]);

            // Act
            var result = await repo.GetAllWorkGroupNamesAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllWorkGroupNamesAsync_SingleEntry_ReturnsSingleName()
        {
            // Arrange
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1" }
            };
            var repo = CreateRepository(workGroups);

            // Act
            var result = await repo.GetAllWorkGroupNamesAsync();

            // Assert
            Assert.Single(result);
            Assert.Equal("WG1", result[0]);
        }

        [Fact]
        public async Task GetAllWorkGroupNamesAsync_ReturnsOnlyNamesNotOtherFields()
        {
            // Arrange
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG_A", ProfitCentre = "PC_ALPHA", Description = "desc" },
                new() { WorkGroupName = "WG_B", ProfitCentre = "PC_BETA",  Description = "desc2" }
            };
            var repo = CreateRepository(workGroups);

            // Act
            var result = await repo.GetAllWorkGroupNamesAsync();

            // Assert — result is List<string>, not WorkGroup objects
            Assert.Equal(2, result.Count);
            Assert.All(result, name => Assert.IsType<string>(name));
            Assert.Contains("WG_A", result);
            Assert.Contains("WG_B", result);
        }

        #endregion

        // ────────────────────────────────────────────────────────────────────
        #region GetWorkGroupTimeCodeAsync — join / projection
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_MatchingJoin_ReturnsProjectedRow()
        {
            var repo  = CreateDefaultTimeCodeRepository();
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
            var row = result.Data.First();
            Assert.Equal("S1",    row.PACTStaffID);
            Assert.Equal("PP1",   row.ParentProject);
            Assert.Equal("WG1",   row.WorkGroup);
            Assert.Equal("Alice", row.Name);
            Assert.Equal("TC1",   row.TimeCode);
            Assert.Equal(1,       row.Month);
            Assert.Equal(8.0,     row.Hours);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_HoursIsNull_DefaultsToZero()
        {
            var repo  = CreateDefaultTimeCodeRepository(hours: null);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
            Assert.Equal(0, result.Data.First().Hours);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_GradeMismatch_ReturnsEmpty()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice", "G_DIFFERENT")],
                [TimeRecord("S1", "PP1", "TC1", 1)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_StaffIdMismatch_ReturnsEmpty()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice", "G1")],
                [TimeRecord("S_DIFFERENT", "PP1", "TC1", 1)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_EmptyAllSets_ReturnsEmpty()
        {
            var repo  = CreateTimeCodeRepository([], [], []);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_MultipleStaffSameGrade_ReturnsRowPerStaff()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice", "G1"), StaffView("S2", "Bob", "G1")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S2", "PP2", "TC2", 2)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        // ────────────────────────────────────────────────────────────────────
        #region GetWorkGroupTimeCodeAsync — workGroup filter
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_WorkGroupFilter_ReturnsMatchingRows()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1"), GradeView("G2", "WG2")],
                [StaffView("S1", "Alice", "G1"), StaffView("S2", "Bob", "G2")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S2", "PP2", "TC2", 2)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, "WG1", null);

            Assert.Single(result.Data);
            Assert.Equal("WG1", result.Data.First().WorkGroup);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_WorkGroupFilter_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultTimeCodeRepository(workGroup: "WG1");
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, "WG_MISSING", null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_NullWorkGroupFilter_ReturnsAllRows()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1"), GradeView("G2", "WG2")],
                [StaffView("S1", "Alice", "G1"), StaffView("S2", "Bob", "G2")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S2", "PP2", "TC2", 2)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_WhitespaceWorkGroupFilter_ReturnsAllRows()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1"), GradeView("G2", "WG2")],
                [StaffView("S1", "Alice", "G1"), StaffView("S2", "Bob", "G2")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S2", "PP2", "TC2", 2)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, "   ", null);

            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        // ────────────────────────────────────────────────────────────────────
        #region GetWorkGroupTimeCodeAsync — monthNumber filter
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_MonthNumberFilter_ReturnsMatchingRows()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice", "G1")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S1", "PP1", "TC2", 2)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, 1);

            Assert.Single(result.Data);
            Assert.Equal(1, result.Data.First().Month);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_MonthNumberFilter_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultTimeCodeRepository(month: 3);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, 99);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_NullMonthNumber_ReturnsAllRows()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice", "G1")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S1", "PP1", "TC2", 2)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        // ────────────────────────────────────────────────────────────────────
        #region GetWorkGroupTimeCodeAsync — column filter (ApplyWorkGroupTimeCodeFilter)
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_NullFilter_ReturnsAllRows()
        {
            var repo  = CreateDefaultTimeCodeRepository();
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_EmptyJsonFilter_ReturnsAllRows()
        {
            var repo  = CreateDefaultTimeCodeRepository();
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByPACTStaffID_PartialMatch_ReturnsRow()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("STAFF001", "Alice", "G1"), StaffView("STAFF002", "Bob", "G1")],
                [TimeRecord("STAFF001", "PP1", "TC1", 1), TimeRecord("STAFF002", "PP2", "TC2", 2)]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"PACTStaffID\":\"001\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
            Assert.Equal("STAFF001", result.Data.First().PACTStaffID);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByPACTStaffID_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultTimeCodeRepository(pactId: "STAFF001");
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"PACTStaffID\":\"NOMATCH\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByName_PartialMatch_ReturnsRow()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice Smith", "G1"), StaffView("S2", "Bob Jones", "G1")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S2", "PP2", "TC2", 2)]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Name\":\"Alice\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
            Assert.Equal("Alice Smith", result.Data.First().Name);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByName_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultTimeCodeRepository(name: "Alice");
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Name\":\"NOMATCH\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByWorkGroup_PartialMatch_ReturnsRow()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG_ALPHA"), GradeView("G2", "WG_BETA")],
                [StaffView("S1", "Alice", "G1"), StaffView("S2", "Bob", "G2")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S2", "PP2", "TC2", 2)]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"WorkGroup\":\"ALPHA\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
            Assert.Equal("WG_ALPHA", result.Data.First().WorkGroup);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByWorkGroup_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultTimeCodeRepository(workGroup: "WG1");
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"WorkGroup\":\"NOMATCH\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByParentProject_PartialMatch_ReturnsRow()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice", "G1"), StaffView("S2", "Bob", "G1")],
                [TimeRecord("S1", "PROJECT_A", "TC1", 1), TimeRecord("S2", "PROJECT_B", "TC2", 2)]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ParentProject\":\"PROJECT_A\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
            Assert.Equal("PROJECT_A", result.Data.First().ParentProject);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByParentProject_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultTimeCodeRepository(parentProject: "PP1");
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ParentProject\":\"NOMATCH\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByTimeCode_PartialMatch_ReturnsRow()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice", "G1"), StaffView("S2", "Bob", "G1")],
                [TimeRecord("S1", "PP1", "TIME_ALPHA", 1), TimeRecord("S2", "PP2", "TIME_BETA", 2)]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TimeCode\":\"ALPHA\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
            Assert.Equal("TIME_ALPHA", result.Data.First().TimeCode);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByTimeCode_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultTimeCodeRepository(timeCode: "TC1");
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TimeCode\":\"NOMATCH\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByMultipleFields_ReturnsRow()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice", "G1")],
                [TimeRecord("S1", "PP1", "TC1", 1)]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"PACTStaffID\":\"S1\",\"ParentProject\":\"PP1\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
        }
        #endregion

        #region GetPagedAsync — FPS CostCentre sorting (ApplyFpsWorkGroupSorting)

        [Fact]
        public async Task GetPagedAsync_SortByCostCentreAscending_OrdersByCostCentre()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG_C", ProfitCentre = "PC1", CostCentre = 300.0 },
                new() { WorkGroupName = "WG_A", ProfitCentre = "PC1", CostCentre = 100.0 },
                new() { WorkGroupName = "WG_B", ProfitCentre = "PC1", CostCentre = 200.0 }
            };
            var repo = CreateRepository(workGroups);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "CostCentre",
                Descending = false
            };

            var result = await repo.GetPagedAsync(query);

            var order = result.Data.Select(w => w.WorkGroupName).ToList();
            Assert.Equal(new[] { "WG_A", "WG_B", "WG_C" }, order);
            // Tolerance-based double comparison (no exact equality)
            Assert.Equal(100.0, result.Data.First().CostCentre!.Value, 3);
        }

        [Fact]
        public async Task GetPagedAsync_SortByCostCentreDescending_OrdersByCostCentreDescending()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG_A", ProfitCentre = "PC1", CostCentre = 100.0 },
                new() { WorkGroupName = "WG_C", ProfitCentre = "PC1", CostCentre = 300.0 },
                new() { WorkGroupName = "WG_B", ProfitCentre = "PC1", CostCentre = 200.0 }
            };
            var repo = CreateRepository(workGroups);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "CostCentre",
                Descending = true
            };

            var result = await repo.GetPagedAsync(query);

            var order = result.Data.Select(w => w.WorkGroupName).ToList();
            Assert.Equal(new[] { "WG_C", "WG_B", "WG_A" }, order);
            // Tolerance-based double comparison (no exact equality)
            Assert.Equal(300.0, result.Data.First().CostCentre!.Value, 3);
        }

        [Fact]
        public async Task GetPagedAsync_SortByCostCentre_IsCaseInsensitive()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG_B", ProfitCentre = "PC1", CostCentre = 200.0 },
                new() { WorkGroupName = "WG_A", ProfitCentre = "PC1", CostCentre = 100.0 }
            };
            var repo = CreateRepository(workGroups);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "costcentre", // lower-case — switch lowers the key
                Descending = false
            };

            var result = await repo.GetPagedAsync(query);

            Assert.Equal(new[] { "WG_A", "WG_B" }, result.Data.Select(w => w.WorkGroupName).ToList());
        }

        [Fact]
        public async Task GetPagedAsync_SortByCostCentreAscending_NullCostCentreOrdersFirst()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG_VALUE", ProfitCentre = "PC1", CostCentre = 100.0 },
                new() { WorkGroupName = "WG_NULL",  ProfitCentre = "PC1", CostCentre = null }
            };
            var repo = CreateRepository(workGroups);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "CostCentre",
                Descending = false
            };

            var result = await repo.GetPagedAsync(query);

            // Ascending: null sorts before a populated value
            Assert.Equal(new[] { "WG_NULL", "WG_VALUE" }, result.Data.Select(w => w.WorkGroupName).ToList());
        }

        // Covers every branch of ApplyFpsWorkGroupSorting (asc/desc for each sortable column + default).
        // Seed values are aligned so the alphabetically-"A" row is always first ascending
        // and the "C" row is always first descending, regardless of which column drives the sort.
        [Theory]
        [InlineData("WorkGroupName", false, "A")]
        [InlineData("WorkGroupName", true, "C")]
        [InlineData("ProfitCentre", false, "A")]
        [InlineData("ProfitCentre", true, "C")]
        [InlineData("CostCentre", false, "A")]
        [InlineData("CostCentre", true, "C")]
        [InlineData("Description", false, "A")]
        [InlineData("Description", true, "C")]
        [InlineData("Owner", false, "A")]
        [InlineData("Owner", true, "C")]
        [InlineData("CentralOverhead", false, "A")]
        [InlineData("CentralOverhead", true, "C")]
        [InlineData("UnknownColumn", false, "A")] // default arm → OrderBy(WorkGroupName)
        [InlineData(null, false, "A")]            // null SortBy → default arm
        public async Task GetPagedAsync_AppliesFpsWorkGroupSorting_ForEachColumn(
            string? sortBy, bool descending, string expectedFirstWorkGroupName)
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "B", ProfitCentre = "PB", CostCentre = 200.0, Description = "DescB", Owner = "OwnerB", CentralOverhead = 20m },
                new() { WorkGroupName = "A", ProfitCentre = "PA", CostCentre = 100.0, Description = "DescA", Owner = "OwnerA", CentralOverhead = 10m },
                new() { WorkGroupName = "C", ProfitCentre = "PC", CostCentre = 300.0, Description = "DescC", Owner = "OwnerC", CentralOverhead = 30m }
            };
            var repo = CreateRepository(workGroups);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                Descending = descending
            };

            var result = await repo.GetPagedAsync(query);

            Assert.Equal(expectedFirstWorkGroupName, result.Data.First().WorkGroupName);
        }
        #endregion
    }
}
