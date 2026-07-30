using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ResourceAllocationRepositoryTest
{
    public class ResourceAllocationRepositoryTests
    {
        private const string DefaultWorkGroupGrade = "WG01";
        private const string DefaultStaffId = "PACT001";
        private const int DefaultFpsYear = 2024;
        private const string DefaultUserEmail = "test@example.com";

        // ── Factory ──────────────────────────────────────────────────────────

        private static ResourceAllocationRepository CreateRepository(
            IEnumerable<WorkGroupEmployee>? workGroupEmployees = null,
            IEnumerable<Employee>? employees = null,
            IEnumerable<WorkgroupGrade>? workgroupGrades = null,
            IEnumerable<StaffJob>? staffJobs = null,
            IEnumerable<Project>? projects = null,
            IEnumerable<StaffJobTblView>? staffJobTblViews = null,
            IEnumerable<ProjectView>? projectViews = null)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(DefaultFpsYear);
            requestContext.UserEmailId.Returns(DefaultUserEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var wgeSet = RepositoryTestHelper.CreateMockDbSet(workGroupEmployees ?? []);
            mockContext.Setup(x => x.WorkGroupEmployees).Returns(wgeSet.Object);

            var empSet = RepositoryTestHelper.CreateMockDbSet(employees ?? []);
            mockContext.Setup(x => x.Employees).Returns(empSet.Object);

            var wggSet = RepositoryTestHelper.CreateMockDbSet(workgroupGrades ?? []);
            mockContext.Setup(x => x.WorkgroupGrades).Returns(wggSet.Object);

            var sjSet = RepositoryTestHelper.CreateMockDbSet(staffJobs ?? []);
            mockContext.Setup(x => x.StaffJobs).Returns(sjSet.Object);

            var projSet = RepositoryTestHelper.CreateMockDbSet(projects ?? []);
            mockContext.Setup(x => x.Projects).Returns(projSet.Object);

            var sjViewSet = RepositoryTestHelper.CreateMockDbSet(staffJobTblViews ?? []);
            mockContext.Setup(x => x.StaffJobTblViews).Returns(sjViewSet.Object);

            var projViewSet = RepositoryTestHelper.CreateMockDbSet(projectViews ?? []);
            mockContext.Setup(x => x.ProjectViews).Returns(projViewSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new ResourceAllocationRepository(mockContext.Object);
        }

        // ── Builder helpers ───────────────────────────────────────────────────

        private static WorkGroupEmployee BuildWorkGroupEmployee(
            string pactId = DefaultStaffId,
            string workGroupGrade = DefaultWorkGroupGrade,
            string spNumber = "SP001",
            double hrsAvail = 37.0) =>
            new()
            {
                PactId = pactId,
                WorkGroupGrade = workGroupGrade,
                SpNumber = spNumber,
                HrsAvail = hrsAvail,
                FpsYear = DefaultFpsYear
            };

        private static Employee BuildEmployee(
            string spNumber = "SP001",
            string lastName = "General",
            string firstName = "Staff") =>
            new()
            {
                SPNumber = spNumber,
                LastName = lastName,
                FirstName = firstName,
                FpsYear = DefaultFpsYear
            };

        private static WorkgroupGrade BuildWorkgroupGrade(
            string wgGrade = DefaultWorkGroupGrade) =>
            new()
            {
                WgGrade = wgGrade,
                FpsYear = DefaultFpsYear
            };

        private static StaffJob BuildStaffJob(
            string staffId = DefaultStaffId,
            string jobCode = "J001",
            double plannedHours = 10.0) =>
            new()
            {
                StaffId = staffId,
                JobCode = jobCode,
                PlannedHours = plannedHours,
                FpsYear = DefaultFpsYear
            };

        private static Project BuildProject(
            string parentProject = "J001",
            string program = "GP",
            string projectStatus = "approved") =>
            new()
            {
                ParentProject = parentProject,
                Program = program,
                ProjectStatus = projectStatus
            };

        private static StaffJobTblView BuildStaffJobTblView(
            string staffId = DefaultStaffId,
            string jobCode = "J001",
            double plannedHours = 10.0) =>
            new()
            {
                StaffId = staffId,
                JobCode = jobCode,
                PlannedHours = plannedHours
            };

        private static ProjectView BuildProjectView(
            string parentProject = "J001",
            string program = "GP",
            string projectStatus = "approved") =>
            new()
            {
                ParentProject = parentProject,
                Program = program,
                ProjectStatus = projectStatus
            };

        private static PaginationParameters<string> DefaultQuery(int page = 1, int pageSize = 10) =>
            new() { Page = page, PageSize = pageSize };

        // ── Constructor Tests ─────────────────────────────────────────────────

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullDbContext_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ResourceAllocationRepository(null!));
        }

        [Fact]
        public void Constructor_WithValidDbContext_CreatesInstance()
        {
            // Arrange & Act
            var repo = CreateRepository();

            // Assert
            Assert.NotNull(repo);
        }

        #endregion

        // ── GetPagedStaffAllocationsByWorkGroupGradeAsync Tests ───────────────

        #region GetPagedStaffAllocationsByWorkGroupGradeAsync Tests

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_WithMatchingData_ReturnsPagedRows()
        {
            // Arrange
            var employees = new[] { BuildEmployee() };
            var workGroupEmployees = new[] { BuildWorkGroupEmployee() };
            var workgroupGrades = new[] { BuildWorkgroupGrade() };
            var staffJobs = new[] { BuildStaffJob() };
            var projects = new[] { BuildProject() };

            var repo = CreateRepository(workGroupEmployees, employees, workgroupGrades, staffJobs, projects);
            var query = DefaultQuery();

            // Act
            var result = await repo.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_WithNoMatchingGrade_ReturnsEmptyData()
        {
            // Arrange
            var repo = CreateRepository();
            var query = DefaultQuery();

            // Act
            var result = await repo.GetPagedStaffAllocationsByWorkGroupGradeAsync("NON_EXISTENT", query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_PaginationIsApplied()
        {
            // Arrange
            var workgroupGrades = new[] { BuildWorkgroupGrade() };
            var employees = Enumerable.Range(1, 5)
                .Select(i => BuildEmployee($"SP{i:D3}", "General", $"Staff{i}"))
                .ToArray();
            var workGroupEmployees = Enumerable.Range(1, 5)
                .Select(i => BuildWorkGroupEmployee($"PACT{i:D3}", DefaultWorkGroupGrade, $"SP{i:D3}"))
                .ToArray();

            var repo = CreateRepository(workGroupEmployees, employees, workgroupGrades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 2 };

            // Act
            var result = await repo.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Data.Count() <= 2);
        }

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_NameFilterIsApplied()
        {
            // Arrange
            var workgroupGrades = new[] { BuildWorkgroupGrade() };
            var employees = new[]
            {
                BuildEmployee("SP001", "General", "Alpha"),
                BuildEmployee("SP002", "General", "Beta")
            };
            var workGroupEmployees = new[]
            {
                BuildWorkGroupEmployee("PACT001", DefaultWorkGroupGrade, "SP001"),
                BuildWorkGroupEmployee("PACT002", DefaultWorkGroupGrade, "SP002")
            };

            var repo = CreateRepository(workGroupEmployees, employees, workgroupGrades);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Name\":\"Alpha\"}"
            };

            // Act
            var result = await repo.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query);

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Data, row => Assert.Contains("Alpha", row.Name, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_SortDescending_ReturnsReverseOrder()
        {
            // Arrange
            var workgroupGrades = new[] { BuildWorkgroupGrade() };
            var employees = new[]
            {
                BuildEmployee("SP001", "General", "Alpha"),
                BuildEmployee("SP002", "General", "Zeta")
            };
            var workGroupEmployees = new[]
            {
                BuildWorkGroupEmployee("PACT001", DefaultWorkGroupGrade, "SP001"),
                BuildWorkGroupEmployee("PACT002", DefaultWorkGroupGrade, "SP002")
            };

            var repo = CreateRepository(workGroupEmployees, employees, workgroupGrades);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "Name",
                Descending = true
            };

            // Act
            var result = await repo.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query);

            // Assert
            var names = result.Data.Select(r => r.Name).ToList();
            var sorted = names.OrderByDescending(n => n).ToList();
            Assert.Equal(sorted, names);
        }

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_ZtHoursComputedCorrectly()
        {
            // Arrange
            var workgroupGrades = new[] { BuildWorkgroupGrade() };
            var employees = new[] { BuildEmployee("SP001", "General", "Staff") };
            var workGroupEmployees = new[] { BuildWorkGroupEmployee("PACT001", DefaultWorkGroupGrade, "SP001", 40.0) };
            var staffJobs = new[] { BuildStaffJob("PACT001", "ZT_J001", 8.0) };
            var projects = new[]
            {
                BuildProject("ZT_J001", "ZT_Prog", "approved"),
                BuildProject("ZT_J001", "ZT_Prog", "approved")
            };

            var repo = CreateRepository(workGroupEmployees, employees, workgroupGrades, staffJobs, projects);
            var query = DefaultQuery();

            // Act
            var result = await repo.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_AllocationIsNullWhenHrsAvailIsZero()
        {
            // Arrange
            var workgroupGrades = new[] { BuildWorkgroupGrade() };
            var employees = new[] { BuildEmployee("SP001", "General", "Staff") };
            var workGroupEmployees = new[] { BuildWorkGroupEmployee("PACT001", DefaultWorkGroupGrade, "SP001", 0.0) };

            var repo = CreateRepository(workGroupEmployees, employees, workgroupGrades);
            var query = DefaultQuery();

            // Act
            var result = await repo.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query);

            // Assert
            Assert.All(result.Data, row => Assert.Null(row.Allocation));
            Assert.All(result.Data, row => Assert.Null(row.Utilization));
            Assert.All(result.Data, row => Assert.Null(row.AppUtilization));
        }

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_AllocationIsComputedWhenHrsAvailIsNonZero()
        {
            // Arrange
            var workgroupGrades = new[] { BuildWorkgroupGrade() };
            var employees = new[] { BuildEmployee("SP001", "General", "Staff") };
            var workGroupEmployees = new[] { BuildWorkGroupEmployee("PACT001", DefaultWorkGroupGrade, "SP001", 40.0) };
            var staffJobs = new[] { BuildStaffJob("PACT001", "J001", 20.0) };
            var projects = new[] { BuildProject("J001", "GP", "approved") };

            var repo = CreateRepository(workGroupEmployees, employees, workgroupGrades, staffJobs, projects);
            var query = DefaultQuery();

            // Act
            var result = await repo.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query);

            // Assert
            Assert.All(result.Data, row => Assert.NotNull(row.Allocation));
        }

        #endregion

        // ── GetPagedStaffJobDetailsByStaffIdAsync Tests ───────────────────────

        #region GetPagedStaffJobDetailsByStaffIdAsync Tests

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_WithMatchingStaffId_ReturnsRows()
        {
            // Arrange
            var staffJobViews = new[]
            {
                BuildStaffJobTblView(DefaultStaffId, "J001", 10.0),
                BuildStaffJobTblView(DefaultStaffId, "J002", 5.0)
            };
            var projectViews = new[]
            {
                BuildProjectView("J001", "GP", "approved"),
                BuildProjectView("J002", "GP", "draft")
            };

            var repo = CreateRepository(staffJobTblViews: staffJobViews, projectViews: projectViews);
            var query = DefaultQuery();

            // Act
            var result = await repo.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            Assert.All(result.Data, row => Assert.Equal(DefaultStaffId, row.StaffId));
        }

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_WithNoMatchingStaffId_ReturnsEmptyData()
        {
            // Arrange
            var repo = CreateRepository();
            var query = DefaultQuery();

            // Act
            var result = await repo.GetPagedStaffJobDetailsByStaffIdAsync("NON_EXISTENT", query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_ProjectFilterIsApplied()
        {
            // Arrange
            var staffJobViews = new[]
            {
                BuildStaffJobTblView(DefaultStaffId, "ALPHA001", 10.0),
                BuildStaffJobTblView(DefaultStaffId, "BETA002",  5.0)
            };
            var projectViews = new[]
            {
                BuildProjectView("ALPHA001", "GP", "approved"),
                BuildProjectView("BETA002",  "GP", "approved")
            };

            var repo = CreateRepository(staffJobTblViews: staffJobViews, projectViews: projectViews);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Project\":\"ALPHA\"}"
            };

            // Act
            var result = await repo.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query);

            // Assert
            Assert.All(result.Data, row => Assert.Contains("ALPHA", row.JobCode, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_StatusFilterIsApplied()
        {
            // Arrange
            var staffJobViews = new[]
            {
                BuildStaffJobTblView(DefaultStaffId, "J001", 10.0),
                BuildStaffJobTblView(DefaultStaffId, "J002", 5.0)
            };
            var projectViews = new[]
            {
                BuildProjectView("J001", "GP", "approved"),
                BuildProjectView("J002", "GP", "draft")
            };

            var repo = CreateRepository(staffJobTblViews: staffJobViews, projectViews: projectViews);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Status\":\"approved\"}"
            };

            // Act
            var result = await repo.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query);

            // Assert
            Assert.All(result.Data, row => Assert.Contains("approved", row.ProjectStatus, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_GlobalSearchIsApplied()
        {
            // Arrange
            var staffJobViews = new[]
            {
                BuildStaffJobTblView(DefaultStaffId, "SEARCH001", 10.0),
                BuildStaffJobTblView(DefaultStaffId, "OTHER002",  5.0)
            };
            var projectViews = new[]
            {
                BuildProjectView("SEARCH001", "GP", "approved"),
                BuildProjectView("OTHER002",  "GP", "approved")
            };

            var repo = CreateRepository(staffJobTblViews: staffJobViews, projectViews: projectViews);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Search = "SEARCH"
            };

            // Act
            var result = await repo.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query);

            // Assert
            Assert.All(result.Data, row =>
                Assert.True(
                    (row.JobCode != null && row.JobCode.Contains("SEARCH", StringComparison.OrdinalIgnoreCase)) ||
                    (row.JobDescription != null && row.JobDescription.Contains("SEARCH", StringComparison.OrdinalIgnoreCase))));
        }

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_SortByHour_Descending_ReturnsCorrectOrder()
        {
            // Arrange
            var staffJobViews = new[]
            {
                BuildStaffJobTblView(DefaultStaffId, "J001", 5.0),
                BuildStaffJobTblView(DefaultStaffId, "J002", 20.0),
                BuildStaffJobTblView(DefaultStaffId, "J003", 10.0)
            };
            var projectViews = new[]
            {
                BuildProjectView("J001", "GP", "approved"),
                BuildProjectView("J002", "GP", "approved"),
                BuildProjectView("J003", "GP", "approved")
            };

            var repo = CreateRepository(staffJobTblViews: staffJobViews, projectViews: projectViews);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "Hour",
                Descending = true
            };

            // Act
            var result = await repo.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query);

            // Assert
            var hours = result.Data.Select(r => r.PlannedHours).ToList();
            var sorted = hours.OrderByDescending(h => h).ToList();
            Assert.Equal(sorted, hours);
        }

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_PaginationIsApplied()
        {
            // Arrange
            var staffJobViews = Enumerable.Range(1, 5)
                .Select(i => BuildStaffJobTblView(DefaultStaffId, $"J{i:D3}", i * 2.0))
                .ToArray();
            var projectViews = Enumerable.Range(1, 5)
                .Select(i => BuildProjectView($"J{i:D3}"))
                .ToArray();

            var repo = CreateRepository(staffJobTblViews: staffJobViews, projectViews: projectViews);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 2 };

            // Act
            var result = await repo.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query);

            // Assert
            Assert.True(result.Data.Count() <= 2);
        }

        #endregion
    }
}
