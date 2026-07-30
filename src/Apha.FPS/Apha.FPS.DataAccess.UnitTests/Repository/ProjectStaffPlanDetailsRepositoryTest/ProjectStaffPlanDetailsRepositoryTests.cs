using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProjectStaffPlanDetailsRepositoryTest
{
    public class ProjectStaffPlanDetailsRepositoryTests
    {
        private static ProjectStaffPlanDetailsRepository CreateRepository(
            IEnumerable<ProjectStaffPlanDetailsView>? views = null)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.FpsYear).Returns(2024);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            var mockSet = RepositoryTestHelper.CreateMockDbSet(views ?? Enumerable.Empty<ProjectStaffPlanDetailsView>());
            mockContext.Setup(x => x.ProjectStaffPlanDetailsViews).Returns(mockSet.Object);

            return new ProjectStaffPlanDetailsRepository(mockContext.Object);
        }

        private static PaginationParameters<string> DefaultQuery(
            int page = 1, int pageSize = 10,
            string? filter = null, string? sortBy = null, bool descending = false)
            => new PaginationParameters<string>
            {
                Page       = page,
                PageSize   = pageSize,
                Filter     = filter,
                SortBy     = sortBy,
                Descending = descending
            };

        private static List<ProjectStaffPlanDetailsView> SampleData() =>
        [
            new() { ProfitCentre = "PC_A", Program = "PROG1", Name = "Alice Smith", Manager = "Manager1", ProjectStatus = "Open",   WorkGroup = "WG_CSU",   GradeCode = "GR1", PlannedHours = 100, ChargeRate = 50m, Cost = 500m },
            new() { ProfitCentre = "PC_A", Program = "PROG1", Name = "Bob Jones",   Manager = "Manager2", ProjectStatus = "Closed", WorkGroup = "WG_BSU",   GradeCode = "GR2", PlannedHours = 80,  ChargeRate = 40m, Cost = 400m },
            new() { ProfitCentre = "PC_B", Program = "PROG2", Name = "Carol White", Manager = "Manager1", ProjectStatus = "Open",   WorkGroup = "WG_CSU",   GradeCode = "GR1", PlannedHours = 60,  ChargeRate = 30m, Cost = 300m },
            new() { ProfitCentre = "PC_C", Program = "PROG3", Name = "Dave Brown",  Manager = "Manager3", ProjectStatus = "Hold",   WorkGroup = "WG_OTHER", GradeCode = "GR3", PlannedHours = 40,  ChargeRate = 20m, Cost = 200m }
        ];

        #region GetPagedAsync — Happy path

        [Fact]
        public async Task GetPagedAsync_ReturnsAllRows_WhenNoFilter()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery());

            Assert.NotNull(result);
            Assert.Equal(4, result.Data.Count());
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsEmpty_WhenNoData()
        {
            var repo   = CreateRepository([]);
            var result = await repo.GetPagedAsync(DefaultQuery());

            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedAsync_DefaultSort_OrdersByProfitCentreThenWorkGroupThenProgram()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery());

            var list = result.Data.ToList();
            Assert.Equal("PC_A", list[0].ProfitCentre);
            Assert.Equal("PC_A", list[1].ProfitCentre);
            Assert.Equal("PC_B", list[2].ProfitCentre);
            Assert.Equal("PC_C", list[3].ProfitCentre);
        }

        #endregion

        #region GetPagedAsync — Paging

        [Fact]
        public async Task GetPagedAsync_Paging_ReturnsCorrectPage()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(page: 1, pageSize: 2));

            Assert.Equal(2, result.Data.Count());
            Assert.Equal(4, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedAsync_Paging_SecondPage_ReturnsRemainingRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(page: 2, pageSize: 2));

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetPagedAsync_Paging_PageBeyondData_ReturnsEmpty()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(page: 99, pageSize: 10));

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetPagedAsync — Filtering

        [Fact]
        public async Task GetPagedAsync_FilterByProfitCentre_ReturnsMatchingRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"ProfitCentre\":\"PC_A\"}"));

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Contains("PC_A", r.ProfitCentre!, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedAsync_FilterByProfitCentre_NoMatch_ReturnsEmpty()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"ProfitCentre\":\"ZZZZ\"}"));

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedAsync_FilterByProgram_ReturnsMatchingRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"Program\":\"PROG1\"}"));

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Contains("PROG1", r.Program!, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedAsync_FilterByName_ReturnsMatchingRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"Name\":\"Alice\"}"));

            Assert.Single(result.Data);
            Assert.Contains("Alice", result.Data.First().Name!, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetPagedAsync_FilterByManager_ReturnsMatchingRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"Manager\":\"Manager1\"}"));

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Contains("Manager1", r.Manager!, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedAsync_FilterByProjectStatus_ReturnsMatchingRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"ProjectStatus\":\"Open\"}"));

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Contains("Open", r.ProjectStatus!, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedAsync_FilterByWorkGroup_ReturnsMatchingRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"WorkGroup\":\"WG_CSU\"}"));

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Contains("WG_CSU", r.WorkGroup!, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedAsync_FilterByGradeCode_ReturnsMatchingRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"GradeCode\":\"GR1\"}"));

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Contains("GR1", r.GradeCode!, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedAsync_EmptyFilter_ReturnsAllRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: ""));

            Assert.Equal(4, result.Data.Count());
        }

        [Fact]
        public async Task GetPagedAsync_NullFilterModel_ReturnsAllRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "null"));

            Assert.Equal(4, result.Data.Count());
        }

        [Fact]
        public async Task GetPagedAsync_InvalidJsonFilter_ThrowsJsonException()
        {
            var repo = CreateRepository(SampleData());

            await Assert.ThrowsAsync<Newtonsoft.Json.JsonReaderException>(
                () => repo.GetPagedAsync(DefaultQuery(filter: "not-valid-json")));
        }

        #endregion

        #region GetPagedAsync — Sorting

        [Fact]
        public async Task GetPagedAsync_SortByProgramAscending_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "program", descending: false));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(string.Compare(list[i - 1].Program, list[i].Program, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByProgramDescending_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "program", descending: true));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(string.Compare(list[i - 1].Program, list[i].Program, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByName_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "name", descending: false));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(string.Compare(list[i - 1].Name, list[i].Name, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByManager_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "manager", descending: false));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(string.Compare(list[i - 1].Manager, list[i].Manager, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByProjectStatus_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "projectstatus", descending: false));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(string.Compare(list[i - 1].ProjectStatus, list[i].ProjectStatus, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByProfitCentre_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "profitcentre", descending: false));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(string.Compare(list[i - 1].ProfitCentre, list[i].ProfitCentre, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByWorkGroup_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "workgroup", descending: false));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(string.Compare(list[i - 1].WorkGroup, list[i].WorkGroup, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByGradeCode_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "gradecode", descending: false));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(string.Compare(list[i - 1].GradeCode, list[i].GradeCode, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByPlannedHoursDescending_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "plannedhours", descending: true));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(list[i - 1].PlannedHours >= list[i].PlannedHours);
        }

        [Fact]
        public async Task GetPagedAsync_SortByCostDescending_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "cost", descending: true));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(list[i - 1].Cost >= list[i].Cost);
        }

        [Fact]
        public async Task GetPagedAsync_UnknownSortBy_FallsBackToDefaultSort()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "unknownfield"));

            Assert.NotNull(result);
            Assert.Equal(4, result.Data.Count());
        }

        #endregion

        #region GetPagedAsync — PaginationData

        [Fact]
        public async Task GetPagedAsync_PaginationData_ReflectsTotalRecords()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(page: 1, pageSize: 10));

            Assert.Equal(4, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.PageNumber);
            Assert.Equal(10, result.PaginationData.PageSize);
        }

        [Fact]
        public async Task GetPagedAsync_PaginationData_ReflectsFilteredCount()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(page: 1, pageSize: 10, filter: "{\"WorkGroup\":\"WG_CSU\"}"));

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        #endregion
    }
}
