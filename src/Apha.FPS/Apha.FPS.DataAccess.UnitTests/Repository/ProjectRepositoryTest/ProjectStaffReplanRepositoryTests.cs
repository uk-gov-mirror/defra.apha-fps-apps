using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProjectRepositoryTest
{
    /// <summary>
    /// Unit tests for <see cref="ProjectRepository.GetProjectStaffReplanAsync"/>.
    ///
    /// The method builds a LINQ join query across Projects, StaffJobs, WorkGroupEmployees,
    /// Employees, WorkgroupGrades, Workgroups, ProfitCentres, UserProfitcentres, and Users,
    /// then filters by workgroup name and EF.Functions.ILike(u.UserEmail, userEmailId).
    ///
    /// The Moq-based factory feeds data through <see cref="RepositoryTestHelper.CreateMockDbSet"/>
    /// whose underlying <c>TestAsyncQueryProvider</c> / <c>LikeRewriter</c> converts
    /// ILike → string.Contains so in-memory LINQ-to-objects executes correctly.
    /// </summary>
    public class ProjectStaffReplanRepositoryTests
    {
        private const string DefaultWorkGroup  = "WorkGroupA";
        private const string DefaultUserEmail  = "test@example.com";
        private const int    DefaultFpsYear    = 2024;
        private const string DefaultPcId       = "PC01";
        private const int    DefaultUserId     = 1;

        // ── Factory ──────────────────────────────────────────────────────────

        private static ProjectRepository CreateRepository(
            IEnumerable<Project>?           projects           = null,
            IEnumerable<StaffJob>?          staffJobs          = null,
            IEnumerable<WorkGroupEmployee>? workGroupEmployees = null,
            IEnumerable<Employee>?          employees          = null,
            IEnumerable<WorkgroupGrade>?    workgroupGrades    = null,
            IEnumerable<Workgroup>?         workgroups         = null,
            IEnumerable<ProfitCentre>?      profitCentres      = null,
            IEnumerable<UserProfitcentre>?  userProfitcentres  = null,
            IEnumerable<User>?              users              = null,
            string userEmailId = DefaultUserEmail,
            int    fpsYear     = DefaultFpsYear)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.UserEmailId).Returns(userEmailId);
            mockRequestContext.Setup(x => x.FpsYear).Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            mockContext.Setup(x => x.Projects)
                .Returns(RepositoryTestHelper.CreateMockDbSet(projects ?? []).Object);
            mockContext.Setup(x => x.StaffJobs)
                .Returns(RepositoryTestHelper.CreateMockDbSet(staffJobs ?? []).Object);
            mockContext.Setup(x => x.WorkGroupEmployees)
                .Returns(RepositoryTestHelper.CreateMockDbSet(workGroupEmployees ?? []).Object);
            mockContext.Setup(x => x.Employees)
                .Returns(RepositoryTestHelper.CreateMockDbSet(employees ?? []).Object);
            mockContext.Setup(x => x.WorkgroupGrades)
                .Returns(RepositoryTestHelper.CreateMockDbSet(workgroupGrades ?? []).Object);
            mockContext.Setup(x => x.Workgroups)
                .Returns(RepositoryTestHelper.CreateMockDbSet(workgroups ?? []).Object);
            mockContext.Setup(x => x.ProfitCentres)
                .Returns(RepositoryTestHelper.CreateMockDbSet(profitCentres ?? []).Object);
            mockContext.Setup(x => x.UserProfitcentres)
                .Returns(RepositoryTestHelper.CreateMockDbSet(userProfitcentres ?? []).Object);
            mockContext.Setup(x => x.Users)
                .Returns(RepositoryTestHelper.CreateMockDbSet(users ?? []).Object);

            // Other DbSets required by ProjectRepository to avoid NullReferenceException
            mockContext.Setup(x => x.ProjectViews)
                .Returns(RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<ProjectView>()).Object);
            mockContext.Setup(x => x.Programs)
                .Returns(RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<Program>()).Object);

            return new ProjectRepository(mockContext.Object, mockRequestContext.Object);
        }

        // ── Builder helpers ───────────────────────────────────────────────────

        private static Project BuildProject(
            string parentProject = "PP001",
            string program = "P001") =>
            new() { ParentProject = parentProject, Program = program };

        private static StaffJob BuildStaffJob(
            string jobCode  = "PP001",
            string staffId  = "S001",
            double hours    = 10.0) =>
            new() { JobCode = jobCode, StaffId = staffId, PlannedHours = hours };

        private static WorkGroupEmployee BuildWorkGroupEmployee(
            string pactId          = "S001",
            string spNumber        = "SP001",
            string workGroupGrade  = "WG01") =>
            new() { PactId = pactId, SpNumber = spNumber, WorkGroupGrade = workGroupGrade };

        private static Employee BuildEmployee(
            string spNumber   = "SP001",
            string lastName   = "Smith",
            string firstName  = "John") =>
            new() { SPNumber = spNumber, LastName = lastName, FirstName = firstName };

        private static WorkgroupGrade BuildWorkgroupGrade(
            string wgGrade    = "WG01",
            string workgroup  = DefaultWorkGroup,
            string gradeCode  = "GC01") =>
            new() { WgGrade = wgGrade, Workgroup = workgroup, GradeCode = gradeCode };

        private static Workgroup BuildWorkgroup(
            string workGroupName = DefaultWorkGroup,
            string profitCentre  = DefaultPcId) =>
            new() { WorkGroupName = workGroupName, ProfitCentre = profitCentre };

        private static ProfitCentre BuildProfitCentre(string profitCentreId = DefaultPcId) =>
            new() { ProfitCentreId = profitCentreId };

        private static UserProfitcentre BuildUserProfitcentre(
            string profitCentre = DefaultPcId,
            int    userId       = DefaultUserId) =>
            new() { ProfitCentre = profitCentre, UserId = userId };

        private static User BuildUser(
            int    userId    = DefaultUserId,
            string userEmail = DefaultUserEmail) =>
            new() { UserId = userId, UserEmail = userEmail };

        private static PaginationParameters<string> DefaultQuery(int page = 1, int pageSize = 10) =>
            new() { Page = page, PageSize = pageSize };

        /// <summary>
        /// Seeds all tables required to produce a valid joined row for the given workgroup / user.
        /// </summary>
        private static (
            List<Project>           Projects,
            List<StaffJob>          StaffJobs,
            List<WorkGroupEmployee> WorkGroupEmployees,
            List<Employee>          Employees,
            List<WorkgroupGrade>    WorkgroupGrades,
            List<Workgroup>         Workgroups,
            List<ProfitCentre>      ProfitCentres,
            List<UserProfitcentre>  UserProfitcentres,
            List<User>              Users
        ) BuildFullSeed(
            string workgroup   = DefaultWorkGroup,
            string userEmail   = DefaultUserEmail,
            string jobCode     = "PP001",
            string staffId     = "S001",
            double plannedHours = 10.0)
        {
            return (
                Projects:           [BuildProject(jobCode)],
                StaffJobs:          [BuildStaffJob(jobCode, staffId, plannedHours)],
                WorkGroupEmployees: [BuildWorkGroupEmployee(staffId)],
                Employees:          [BuildEmployee()],
                WorkgroupGrades:    [BuildWorkgroupGrade(workgroup: workgroup)],
                Workgroups:         [BuildWorkgroup(workgroup)],
                ProfitCentres:      [BuildProfitCentre()],
                UserProfitcentres:  [BuildUserProfitcentre()],
                Users:              [BuildUser(userEmail: userEmail)]
            );
        }

        // ── Constructor Tests ─────────────────────────────────────────────────

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullDbContext_ThrowsArgumentNullException()
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);

            Assert.Throws<ArgumentNullException>(() =>
                new ProjectRepository(null!, mockRequestContext.Object));
        }

        #endregion

        // ── GetProjectStaffReplanAsync Tests ──────────────────────────────────

        #region GetProjectStaffReplanAsync Tests

        [Fact]
        public async Task GetProjectStaffReplanAsync_WithMatchingData_ReturnsPagedRows()
        {
            // Arrange
            var seed = BuildFullSeed();
            var repo = CreateRepository(
                seed.Projects, seed.StaffJobs, seed.WorkGroupEmployees, seed.Employees,
                seed.WorkgroupGrades, seed.Workgroups, seed.ProfitCentres,
                seed.UserProfitcentres, seed.Users);
            var query = DefaultQuery();

            // Act
            var result = await repo.GetProjectStaffReplanAsync(query, DefaultWorkGroup);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Count() >= 1);
        }

        [Fact]
        public async Task GetProjectStaffReplanAsync_WithMatchingData_ReturnsCorrectWorkGroup()
        {
            // Arrange
            var seed = BuildFullSeed();
            var repo = CreateRepository(
                seed.Projects, seed.StaffJobs, seed.WorkGroupEmployees, seed.Employees,
                seed.WorkgroupGrades, seed.Workgroups, seed.ProfitCentres,
                seed.UserProfitcentres, seed.Users);
            var query = DefaultQuery();

            // Act
            var result = await repo.GetProjectStaffReplanAsync(query, DefaultWorkGroup);

            // Assert
            Assert.All(result.Data, row => Assert.Equal(DefaultWorkGroup, row.WorkGroup));
        }

        [Fact]
        public async Task GetProjectStaffReplanAsync_WithMatchingData_ReturnsFormattedStaffName()
        {
            // Arrange
            var seed = BuildFullSeed();
            var repo = CreateRepository(
                seed.Projects, seed.StaffJobs, seed.WorkGroupEmployees, seed.Employees,
                seed.WorkgroupGrades, seed.Workgroups, seed.ProfitCentres,
                seed.UserProfitcentres, seed.Users);
            var query = DefaultQuery();

            // Act
            var result = await repo.GetProjectStaffReplanAsync(query, DefaultWorkGroup);

            // Assert
            var row = result.Data.First();
            Assert.Equal("Smith, John", row.Name);
        }

        [Fact]
        public async Task GetProjectStaffReplanAsync_WithMatchingData_ReturnsCorrectPlannedHours()
        {
            // Arrange
            var seed = BuildFullSeed(plannedHours: 20.5);
            var repo = CreateRepository(
                seed.Projects, seed.StaffJobs, seed.WorkGroupEmployees, seed.Employees,
                seed.WorkgroupGrades, seed.Workgroups, seed.ProfitCentres,
                seed.UserProfitcentres, seed.Users);
            var query = DefaultQuery();

            // Act
            var result = await repo.GetProjectStaffReplanAsync(query, DefaultWorkGroup);

            // Assert
            Assert.Equal(20.5, result.Data.First().PlannedHours);
        }

        [Fact]
        public async Task GetProjectStaffReplanAsync_WithNoMatchingWorkgroup_ReturnsEmptyResult()
        {
            // Arrange
            var seed = BuildFullSeed(workgroup: "WorkGroupA");
            var repo = CreateRepository(
                seed.Projects, seed.StaffJobs, seed.WorkGroupEmployees, seed.Employees,
                seed.WorkgroupGrades, seed.Workgroups, seed.ProfitCentres,
                seed.UserProfitcentres, seed.Users);
            var query = DefaultQuery();

            // Act  — filter by a workgroup that has no matching grade record
            var result = await repo.GetProjectStaffReplanAsync(query, "NonExistentWorkGroup");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetProjectStaffReplanAsync_WithNoData_ReturnsEmptyResult()
        {
            // Arrange
            var repo = CreateRepository();
            var query = DefaultQuery();

            // Act
            var result = await repo.GetProjectStaffReplanAsync(query, DefaultWorkGroup);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectStaffReplanAsync_WithMultipleStaffRows_ReturnsAllMatchingRows()
        {
            // Arrange
            var projects = new[] { BuildProject("PP001"), BuildProject("PP002"), BuildProject("PP003") };
            var staffJobs = new[]
            {
                BuildStaffJob("PP001", "S001", 10.0),
                BuildStaffJob("PP002", "S002", 8.0),
                BuildStaffJob("PP003", "S003", 6.0)
            };
            var workGroupEmployees = new[]
            {
                BuildWorkGroupEmployee("S001", "SP001"),
                BuildWorkGroupEmployee("S002", "SP002"),
                BuildWorkGroupEmployee("S003", "SP003")
            };
            var employees = new[]
            {
                BuildEmployee("SP001", "Smith", "John"),
                BuildEmployee("SP002", "Jones", "Alice"),
                BuildEmployee("SP003", "Brown", "Bob")
            };
            var workgroupGrades    = new[] { BuildWorkgroupGrade(workgroup: DefaultWorkGroup) };
            var workgroups         = new[] { BuildWorkgroup() };
            var profitCentres      = new[] { BuildProfitCentre() };
            var userProfitcentres  = new[] { BuildUserProfitcentre() };
            var users              = new[] { BuildUser() };

            var repo = CreateRepository(projects, staffJobs, workGroupEmployees, employees,
                workgroupGrades, workgroups, profitCentres, userProfitcentres, users);
            var query = DefaultQuery();

            // Act
            var result = await repo.GetProjectStaffReplanAsync(query, DefaultWorkGroup);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task GetProjectStaffReplanAsync_WithDifferentWorkgroups_FiltersToRequestedWorkgroupOnly()
        {
            // Arrange — seed two workgroups: only WorkGroupA should appear in results
            var projects = new[] { BuildProject("PP001"), BuildProject("PP002") };
            var staffJobs = new[]
            {
                BuildStaffJob("PP001", "S001", 10.0),
                BuildStaffJob("PP002", "S002", 8.0)
            };
            var workGroupEmployees = new[]
            {
                BuildWorkGroupEmployee("S001", "SP001", "WG01"),
                BuildWorkGroupEmployee("S002", "SP002", "WG02")
            };
            var employees = new[]
            {
                BuildEmployee("SP001", "Smith", "John"),
                BuildEmployee("SP002", "Jones", "Alice")
            };
            var workgroupGrades = new[]
            {
                BuildWorkgroupGrade("WG01", "WorkGroupA"),
                BuildWorkgroupGrade("WG02", "WorkGroupB")
            };
            var workgroups = new[]
            {
                BuildWorkgroup("WorkGroupA"),
                BuildWorkgroup("WorkGroupB")
            };
            var profitCentres     = new[] { BuildProfitCentre() };
            var userProfitcentres = new[] { BuildUserProfitcentre() };
            var users             = new[] { BuildUser() };

            var repo = CreateRepository(projects, staffJobs, workGroupEmployees, employees,
                workgroupGrades, workgroups, profitCentres, userProfitcentres, users);
            var query = DefaultQuery();

            // Act
            var result = await repo.GetProjectStaffReplanAsync(query, "WorkGroupA");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("WorkGroupA", result.Data.First().WorkGroup);
        }

        [Fact]
        public async Task GetProjectStaffReplanAsync_WithPagination_ReturnsRequestedPage()
        {
            // Arrange — 5 staff rows, request page 2 with page size 2
            var projects           = Enumerable.Range(1, 5).Select(i => BuildProject($"PP00{i}")).ToArray();
            var staffJobs          = Enumerable.Range(1, 5).Select(i => BuildStaffJob($"PP00{i}", $"S00{i}", i * 2.0)).ToArray();
            var workGroupEmployees = Enumerable.Range(1, 5).Select(i => BuildWorkGroupEmployee($"S00{i}", $"SP00{i}")).ToArray();
            var employees          = Enumerable.Range(1, 5).Select(i => BuildEmployee($"SP00{i}", $"Last{i}", $"First{i}")).ToArray();
            var workgroupGrades    = new[] { BuildWorkgroupGrade(workgroup: DefaultWorkGroup) };
            var workgroups         = new[] { BuildWorkgroup() };
            var profitCentres      = new[] { BuildProfitCentre() };
            var userProfitcentres  = new[] { BuildUserProfitcentre() };
            var users              = new[] { BuildUser() };

            var repo = CreateRepository(projects, staffJobs, workGroupEmployees, employees,
                workgroupGrades, workgroups, profitCentres, userProfitcentres, users);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            // Act
            var result = await repo.GetProjectStaffReplanAsync(query, DefaultWorkGroup);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectStaffReplanAsync_WhenUserEmailDoesNotMatch_ReturnsEmptyResult()
        {
            // Arrange — user email in DB does not match the ILike pattern (requestContext email)
            var seed = BuildFullSeed(userEmail: "other@example.com");
            // requestContext will use "test@example.com", data has "other@example.com"
            var repo = CreateRepository(
                seed.Projects, seed.StaffJobs, seed.WorkGroupEmployees, seed.Employees,
                seed.WorkgroupGrades, seed.Workgroups, seed.ProfitCentres,
                seed.UserProfitcentres, seed.Users,
                userEmailId: "test@example.com");
            var query = DefaultQuery();

            // Act
            var result = await repo.GetProjectStaffReplanAsync(query, DefaultWorkGroup);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetProjectStaffReplanAsync_WithMatchingData_ReturnsCorrectWgGradeAndGradeCode()
        {
            // Arrange
            var seed = BuildFullSeed();
            var repo = CreateRepository(
                seed.Projects, seed.StaffJobs, seed.WorkGroupEmployees, seed.Employees,
                seed.WorkgroupGrades, seed.Workgroups, seed.ProfitCentres,
                seed.UserProfitcentres, seed.Users);
            var query = DefaultQuery();

            // Act
            var result = await repo.GetProjectStaffReplanAsync(query, DefaultWorkGroup);

            // Assert
            var row = result.Data.First();
            Assert.Equal("WG01",            row.WgGrade);
            Assert.Equal("GC01",            row.GradeCode);
            Assert.Equal(DefaultWorkGroup,  row.WorkGroup);
        }

        [Fact]
        public async Task GetProjectStaffReplanAsync_WithMatchingData_ReturnsCorrectParentProjectAndProgram()
        {
            // Arrange
            var seed = BuildFullSeed(jobCode: "PP999");
            // Override the project seed to have a specific program
            var projects = new[] { BuildProject("PP999", "PROG-X") };
            var repo = CreateRepository(
                projects, seed.StaffJobs, seed.WorkGroupEmployees, seed.Employees,
                seed.WorkgroupGrades, seed.Workgroups, seed.ProfitCentres,
                seed.UserProfitcentres, seed.Users);
            var query = DefaultQuery();

            // Act
            var result = await repo.GetProjectStaffReplanAsync(query, DefaultWorkGroup);

            // Assert
            var row = result.Data.First();
            Assert.Equal("PP999",   row.ParentProject);
            Assert.Equal("PROG-X",  row.Program);
        }

        #endregion
    }
}
