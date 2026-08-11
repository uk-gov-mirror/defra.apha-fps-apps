using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProgramRepositoryTest
{
    public class ProgramRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a ProgramRepository with in-memory Programs, UserPrograms, and Users data.
        /// IFpsRequestContext is substituted via NSubstitute.
        /// Get() JOIN logic across Programs/UserPrograms/Users is covered by integration tests.
        /// ExecuteDeleteAsync() used in DeleteProgramAsync is not mockable and is covered by integration tests.
        /// </summary>
        private static ProgramRepository CreateRepository(
            IEnumerable<Core.Entities.Program> programs,
            IEnumerable<UserProgram> userPrograms,
            IEnumerable<User> users,
            int fpsYear = DefaultTestFpsYear,
            string userEmailId = "test@example.com", // always lowercase — matches middleware ToLowerInvariant()
            IEnumerable<ProgramView>? programViews = null)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(fpsYear);
            requestContext.UserEmailId.Returns(userEmailId);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var programsMockSet = RepositoryTestHelper.CreateMockDbSet(programs);
            var userProgramsMockSet = RepositoryTestHelper.CreateMockDbSet(userPrograms);
            var usersMockSet = RepositoryTestHelper.CreateMockDbSet(users);
            var programViewsMockSet = RepositoryTestHelper.CreateMockDbSet(programViews ?? []);

            RepositoryTestHelper.SetupDbSetOperations(programsMockSet);
            RepositoryTestHelper.SetupDbSetOperations(userProgramsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Programs).Returns(programsMockSet.Object);
            mockContext.Setup(x => x.UserPrograms).Returns(userProgramsMockSet.Object);
            mockContext.Setup(x => x.Users).Returns(usersMockSet.Object);
            mockContext.Setup(x => x.ProgramViews).Returns(programViewsMockSet.Object);

            return new ProgramRepository(mockContext.Object, requestContext);
        }

        private static (
            ProgramRepository Repo,
            Mock<DbSet<Core.Entities.Program>> ProgramsDbSet,
            Mock<DbSet<UserProgram>> UserProgramsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<Core.Entities.Program> programs,
                IEnumerable<UserProgram> userPrograms,
                IEnumerable<User> users,
                int fpsYear = DefaultTestFpsYear,
                string userEmailId = "test@example.com")
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(fpsYear);
            requestContext.UserEmailId.Returns(userEmailId);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var programsMockSet = RepositoryTestHelper.CreateMockDbSet(programs);
            var userProgramsMockSet = RepositoryTestHelper.CreateMockDbSet(userPrograms);
            var usersMockSet = RepositoryTestHelper.CreateMockDbSet(users);

            RepositoryTestHelper.SetupDbSetOperations(programsMockSet);
            RepositoryTestHelper.SetupDbSetOperations(userProgramsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Programs).Returns(programsMockSet.Object);
            mockContext.Setup(x => x.UserPrograms).Returns(userProgramsMockSet.Object);
            mockContext.Setup(x => x.Users).Returns(usersMockSet.Object);

            var repo = new ProgramRepository(mockContext.Object, requestContext);
            return (repo, programsMockSet, userProgramsMockSet, mockContext);
        }

        #region GetAllProgramsAsync

        [Fact]
        public async Task GetAllProgramsAsync_ReturnsPrograms_WhenUserEmailMatchesExactly()
        {
            // Arrange — DB email already lowercase, matches the normalised UserEmailId
            var views = new List<ProgramView>
            {
                new() { ProgramNo = "P001", ProgramName = "Alpha", UserEmail = "test@example.com" },
                new() { ProgramNo = "P002", ProgramName = "Beta",  UserEmail = "test@example.com" }
            };
            var repo = CreateRepository([], [], [], programViews: views);

            // Act
            var result = (await repo.GetAllProgramsAsync()).ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Theory]
        [InlineData("Test@Example.COM")]
        [InlineData("TEST@EXAMPLE.COM")]
        [InlineData("Test@example.com")]
        public async Task GetAllProgramsAsync_ReturnsPrograms_WhenDbEmailIsMixedCase(string dbEmail)
        {
            // Arrange — DB stores mixed-case email; middleware normalises incoming to lowercase.
            // The query must use LOWER(UserEmail) so the comparison still matches.
            var views = new List<ProgramView>
            {
                new() { ProgramNo = "P001", ProgramName = "Alpha", UserEmail = dbEmail }
            };
            var repo = CreateRepository([], [], [],
                userEmailId: "test@example.com", // lowercase — as set by middleware
                programViews: views);

            // Act
            var result = (await repo.GetAllProgramsAsync()).ToList();

            // Assert — must find the record despite casing mismatch in DB
            Assert.Single(result);
            Assert.Equal("P001", result[0].ProgramNo);
        }

        [Fact]
        public async Task GetAllProgramsAsync_ExcludesPrograms_WhenEmailBelongsToDifferentUser()
        {
            // Arrange — two records with different emails; only the matching one should be returned
            var views = new List<ProgramView>
            {
                new() { ProgramNo = "P001", UserEmail = "test@example.com" },
                new() { ProgramNo = "P002", UserEmail = "other@example.com" }
            };
            var repo = CreateRepository([], [], [],
                userEmailId: "test@example.com",
                programViews: views);

            // Act
            var result = (await repo.GetAllProgramsAsync()).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("P001", result[0].ProgramNo);
        }

        [Fact]
        public async Task GetAllProgramsAsync_ExcludesPrograms_WhenDbEmailIsNull()
        {
            // Arrange — null UserEmail in DB must not match any user
            var views = new List<ProgramView>
            {
                new() { ProgramNo = "P001", UserEmail = null }
            };
            var repo = CreateRepository([], [], [], programViews: views);

            // Act
            var result = (await repo.GetAllProgramsAsync()).ToList();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetAllProgramsForAllUsers

        [Fact]
        public async Task GetAllProgramsForAllUsers_ReturnsAllPrograms_WithoutEmailFilter()
        {
            // Arrange — Programs table has records; no user email filtering expected
            var programs = new List<Core.Entities.Program>
            {
                new() { ProgramNo = "P001", ProgramName = "Alpha" },
                new() { ProgramNo = "P002", ProgramName = "Beta" }
            };
            var repo = CreateRepository(programs, [], []);

            // Act
            var result = (await repo.GetAllProgramsForAllUsers()).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("P001", result[0].ProgramNo);
            Assert.Equal("P002", result[1].ProgramNo);
        }

        [Fact]
        public async Task GetAllProgramsForAllUsers_ReturnsEmptyList_WhenNoProgramsExist()
        {
            // Arrange
            var repo = CreateRepository([], [], []);

            // Act
            var result = (await repo.GetAllProgramsForAllUsers()).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllProgramsForAllUsers_ReturnsAllPrograms_RegardlessOfUserEmail()
        {
            // Arrange — programs exist but current user email does not matter for unfiltered
            var programs = new List<Core.Entities.Program>
            {
                new() { ProgramNo = "P001", ProgramName = "Alpha" },
                new() { ProgramNo = "P002", ProgramName = "Beta" },
                new() { ProgramNo = "P003", ProgramName = "Gamma" }
            };
            var repo = CreateRepository(programs, [], [], userEmailId: "differentuser@example.com");

            // Act
            var result = (await repo.GetAllProgramsForAllUsers()).ToList();

            // Assert — all programs returned regardless of the user context
            Assert.Equal(3, result.Count);
        }

        #endregion

        #region GetProgramByIdAsync

        [Fact]
        public async Task GetProgramByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var programs = new List<Core.Entities.Program>
            {
                new() { ProgramNo = "P001", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(programs, [], []);

            // Act
            var result = await repo.GetProgramByIdAsync("P999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProgramByIdAsync_ReturnsNull_WhenProgramsIsEmpty()
        {
            // Arrange
            var repo = CreateRepository([], [], []);

            // Act
            var result = await repo.GetProgramByIdAsync("P001");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProgramByIdAsync_ReturnsProgram_WhenFound()
        {
            // Arrange
            var programs = new List<Core.Entities.Program>
            {
                new() { ProgramNo = "P001", ProgramName = "Program One", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(programs, [], []);

            // Act
            var result = await repo.GetProgramByIdAsync("P001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("P001", result.ProgramNo);
            Assert.Equal("Program One", result.ProgramName);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetProgramByIdAsync_ThrowsArgumentException_WhenIdIsNullOrWhiteSpace(string id)
        {
            // Arrange
            var repo = CreateRepository([], [], []);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetProgramByIdAsync(id));
        }

        #endregion

        #region AddProgramAsync

        [Fact]
        public async Task AddProgramAsync_ThrowsArgumentNullException_WhenProgramIsNull()
        {
            // Arrange
            var repo = CreateRepository([], [], []);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddProgramAsync(null!));
        }

        [Fact]
        public async Task AddProgramAsync_AddsProgram_AndSetsYearAndUserProgram_WhenRequestingUserExists()
        {
            // Arrange
            var requestingUser = new User { UserId = 1, UserEmail = "test@example.com" };
            var (repo, programsMockSet, userProgramsMockSet, mockContext) =
                CreateRepositoryWithMocks([], [], [requestingUser]);

            var newProgram = new Core.Entities.Program { ProgramNo = "P001", ProgramName = "Program One" };

            // Act
            var result = await repo.AddProgramAsync(newProgram);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("P001", result.ProgramNo);
            Assert.Equal(DefaultTestFpsYear, result.FpsYear);
            programsMockSet.Verify(x => x.Add(It.IsAny<Core.Entities.Program>()), Times.Once);
            userProgramsMockSet.Verify(x => x.Add(It.IsAny<UserProgram>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task AddProgramAsync_SetsCorrectUserProgramFields_WhenRequestingUserExists()
        {
            // Arrange
            var requestingUser = new User { UserId = 7, UserEmail = "test@example.com" };
            UserProgram? capturedUserProgram = null;
            var (repo, _, userProgramsMockSet, _) =
                CreateRepositoryWithMocks([], [], [requestingUser]);

            userProgramsMockSet
                .Setup(x => x.Add(It.IsAny<UserProgram>()))
                .Callback<UserProgram>(up => capturedUserProgram = up);

            var newProgram = new Core.Entities.Program { ProgramNo = "P001", ProgramName = "Program One" };

            // Act
            await repo.AddProgramAsync(newProgram);

            // Assert
            Assert.NotNull(capturedUserProgram);
            Assert.Equal("P001", capturedUserProgram!.ProgramNo);
            Assert.Equal(7, capturedUserProgram.UserID);
            Assert.Equal(DefaultTestFpsYear, capturedUserProgram.FpsYear);
        }

        [Fact]
        public async Task AddProgramAsync_AddsProgramOnly_WhenRequestingUserNotFound()
        {
            // Arrange — no user found by email means UserProgram should NOT be added
            var (repo, programsMockSet, userProgramsMockSet, mockContext) =
                CreateRepositoryWithMocks([], [], []);

            var newProgram = new Core.Entities.Program { ProgramNo = "P001", ProgramName = "Program One" };

            // Act
            var result = await repo.AddProgramAsync(newProgram);

            // Assert
            Assert.NotNull(result);
            programsMockSet.Verify(x => x.Add(It.IsAny<Core.Entities.Program>()), Times.Once);
            userProgramsMockSet.Verify(x => x.Add(It.IsAny<UserProgram>()), Times.Never);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task AddProgramAsync_SetsFpsCalYear_FromYearContext()
        {
            // Arrange
            const int customYear = 2025;
            var (repo, _, _, _) = CreateRepositoryWithMocks([], [], [], fpsYear: customYear);
            var newProgram = new Core.Entities.Program { ProgramNo = "P001" };

            // Act
            var result = await repo.AddProgramAsync(newProgram);

            // Assert
            Assert.Equal(customYear, result.FpsYear);
        }

        #endregion

        #region UpdateProgramAsync

        [Fact]
        public async Task UpdateProgramAsync_ThrowsArgumentNullException_WhenProgramIsNull()
        {
            // Arrange
            var repo = CreateRepository([], [], []);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateProgramAsync(null!, string.Empty));
        }

        [Fact]
        public async Task UpdateProgramAsync_UpdatesProgram_AndAddsUserProgram_WhenRequestingUserExistsAndLinkIsMissing()
        {
            // Arrange — requesting user exists but no UserProgram link yet ? link should be created
            var requestingUser = new User { UserId = 1, UserEmail = "test@example.com" };
            var (repo, programsMockSet, userProgramsMockSet, mockContext) =
                CreateRepositoryWithMocks([], [], [requestingUser]);

            var program = new Core.Entities.Program { ProgramNo = "P001", ProgramName = "Updated Name" };

            // Act
            var result = await repo.UpdateProgramAsync(program, program.ProgramNo);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("P001", result.ProgramNo);
            programsMockSet.Verify(x => x.Update(It.IsAny<Core.Entities.Program>()), Times.Once);
            userProgramsMockSet.Verify(x => x.Add(It.IsAny<UserProgram>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task UpdateProgramAsync_UpdatesProgram_AndSkipsUserProgram_WhenLinkAlreadyExists()
        {
            // Arrange — UserProgram link already exists ? should NOT add a duplicate
            var requestingUser = new User { UserId = 1, UserEmail = "test@example.com" };
            var existingLink = new UserProgram { ProgramNo = "P001", UserID = 1, FpsYear = DefaultTestFpsYear };
            var (repo, programsMockSet, userProgramsMockSet, mockContext) =
                CreateRepositoryWithMocks([], [existingLink], [requestingUser]);

            var program = new Core.Entities.Program { ProgramNo = "P001", ProgramName = "Updated Name" };

            // Act
            var result = await repo.UpdateProgramAsync(program, program.ProgramNo);

            // Assert
            Assert.NotNull(result);
            programsMockSet.Verify(x => x.Update(It.IsAny<Core.Entities.Program>()), Times.Once);
            userProgramsMockSet.Verify(x => x.Add(It.IsAny<UserProgram>()), Times.Never);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task UpdateProgramAsync_UpdatesProgramOnly_WhenRequestingUserNotFound()
        {
            // Arrange — no user found by email means UserProgram should NOT be touched
            var (repo, programsMockSet, userProgramsMockSet, mockContext) =
                CreateRepositoryWithMocks([], [], []);

            var program = new Core.Entities.Program { ProgramNo = "P001", ProgramName = "Updated Name" };

            // Act
            var result = await repo.UpdateProgramAsync(program, program.ProgramNo);

            // Assert
            Assert.NotNull(result);
            programsMockSet.Verify(x => x.Update(It.IsAny<Core.Entities.Program>()), Times.Once);
            userProgramsMockSet.Verify(x => x.Add(It.IsAny<UserProgram>()), Times.Never);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        #endregion

        #region DeleteProgramAsync

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteProgramAsync_ThrowsArgumentException_WhenIdIsNullOrWhiteSpace(string id)
        {
            // Arrange
            var repo = CreateRepository([], [], []);

            // Act & Assert
            // ExecuteDeleteAsync() is not mockable with Moq; full delete logic is covered by integration tests.
            await Assert.ThrowsAsync<ArgumentException>(() => repo.DeleteProgramAsync(id));
        }

        #endregion

        #region GetProgramTimeSnapshotAsync

        /// <summary>
        /// Creates a ProgramRepository with in-memory data for all six join sources used by
        /// GetProgramTimeSnapshotAsync. Filtering uses EF.Functions.ILike which is not translatable
        /// in-memory, so filter-based scenarios are covered by integration tests.
        /// </summary>
        private static ProgramRepository CreatePlanCostRepository(
            IEnumerable<Core.Entities.Program> programs,
            IEnumerable<Project> projects,
            IEnumerable<StaffJob> staffJobs,
            IEnumerable<StaffGeneralView> staff,
            IEnumerable<WorkgroupGradeGeneralView> workgroupGrades,
            IEnumerable<ProfitCentreGradeView> profitCentreGrades,
            int fpsYear = DefaultTestFpsYear,
            string userEmailId = "test@example.com")
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(fpsYear);
            requestContext.UserEmailId.Returns(userEmailId);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var programsMockSet = RepositoryTestHelper.CreateMockDbSet(programs);
            var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects);
            var staffJobsMockSet = RepositoryTestHelper.CreateMockDbSet(staffJobs);
            var staffMockSet = RepositoryTestHelper.CreateMockDbSet(staff);
            var workgroupGradesMockSet = RepositoryTestHelper.CreateMockDbSet(workgroupGrades);
            var profitCentreGradesMockSet = RepositoryTestHelper.CreateMockDbSet(profitCentreGrades);

            mockContext.Setup(x => x.Programs).Returns(programsMockSet.Object);
            mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);
            mockContext.Setup(x => x.StaffJobs).Returns(staffJobsMockSet.Object);
            mockContext.Setup(x => x.StaffGeneralViews).Returns(staffMockSet.Object);
            mockContext.Setup(x => x.WorkgroupGradeGeneralViews).Returns(workgroupGradesMockSet.Object);
            mockContext.Setup(x => x.ProfitCentreGradeViews).Returns(profitCentreGradesMockSet.Object);

            return new ProgramRepository(mockContext.Object, requestContext);
        }

        private static (
            IEnumerable<Core.Entities.Program> Programs,
            IEnumerable<Project> Projects,
            IEnumerable<StaffJob> StaffJobs,
            IEnumerable<StaffGeneralView> Staff,
            IEnumerable<WorkgroupGradeGeneralView> WorkgroupGrades,
            IEnumerable<ProfitCentreGradeView> ProfitCentreGrades)
            BuildPlanCostSeedData()
        {
            var programs = new List<Core.Entities.Program>
            {
                new() { ProgramNo = "P001", Directorate = "Dir A" },
                new() { ProgramNo = "ZT_prog", Directorate = "Dir B" }
            };

            var projects = new List<Project>
            {
                new() { ParentProject = "J001", Program = "P001", Customer = "Cust A", Contract = "C1", ProjectStatus = "Open" },
                new() { ParentProject = "J002", Program = "ZT_prog", Customer = "Cust B", Contract = "C2", ProjectStatus = "Open" }
            };

            var staffJobs = new List<StaffJob>
            {
                new() { JobCode = "J001", StaffId = "1", PlannedHours = 10 },
                new() { JobCode = "J002", StaffId = "2", PlannedHours = 5 }
            };

            var staff = new List<StaffGeneralView>
            {
                new() { StaffId = "1", Name = "Alice", WorkGroupGrade = "WG1" },
                new() { StaffId = "2", Name = "Bob", WorkGroupGrade = "WG2" }
            };

            var workgroupGrades = new List<WorkgroupGradeGeneralView>
            {
                new() { WgGrade = "WG1", ProfitCentreGrade = "PC1", GradeCode = "G1", WorkGroup = "Group1" },
                new() { WgGrade = "WG2", ProfitCentreGrade = "PC2", GradeCode = "G2", WorkGroup = "Group2" }
            };

            var profitCentreGrades = new List<ProfitCentreGradeView>
            {
                new() { PcGrade = "PC1", ProfitCentre = "RC1", ChargeRate = 100 },
                new() { PcGrade = "PC2", ProfitCentre = "RC2", ChargeRate = 200 }
            };

            return (programs, projects, staffJobs, staff, workgroupGrades, profitCentreGrades);
        }

        [Fact]
        public async Task GetProgramTimeSnapshotAsync_ReturnsProjectedRows_ForMatchingJoins()
        {
            // Arrange
            var seed = BuildPlanCostSeedData();
            var repo = CreatePlanCostRepository(
                seed.Programs, seed.Projects, seed.StaffJobs, seed.Staff, seed.WorkgroupGrades, seed.ProfitCentreGrades);
            var query = new Core.Pagination.PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProgramTimeSnapshotAsync(query);

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, i => Assert.StartsWith("Plan - ", i.Version));
        }

        [Fact]
        public async Task GetProgramTimeSnapshotAsync_ComputesHoursCost_UsingChargeRate()
        {
            // Arrange
            var seed = BuildPlanCostSeedData();
            var repo = CreatePlanCostRepository(
                seed.Programs, seed.Projects, seed.StaffJobs, seed.Staff, seed.WorkgroupGrades, seed.ProfitCentreGrades);
            var query = new Core.Pagination.PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProgramTimeSnapshotAsync(query);

            // Assert — P001 row: 10 hours * 100 rate = 1000
            var normalRow = result.Data.Single(i => i.Program == "P001");
            Assert.Equal(1000m, normalRow.HoursCost);
        }

        [Fact]
        public async Task GetProgramTimeSnapshotAsync_ReturnsZeroHoursCost_ForExcludedPrograms()
        {
            // Arrange
            var seed = BuildPlanCostSeedData();
            var repo = CreatePlanCostRepository(
                seed.Programs, seed.Projects, seed.StaffJobs, seed.Staff, seed.WorkgroupGrades, seed.ProfitCentreGrades);
            var query = new Core.Pagination.PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetProgramTimeSnapshotAsync(query);

            // Assert — ZT_prog is an excluded program; cost must be zero regardless of charge rate
            var excludedRow = result.Data.Single(i => i.Program == "ZT_prog");
            Assert.Equal(0m, excludedRow.HoursCost);
        }

        [Fact]
        public async Task GetProgramTimeSnapshotAsync_OrdersByHoursCostDescending_ByDefault()
        {
            // Arrange
            var seed = BuildPlanCostSeedData();
            var repo = CreatePlanCostRepository(
                seed.Programs, seed.Projects, seed.StaffJobs, seed.Staff, seed.WorkgroupGrades, seed.ProfitCentreGrades);
            var query = new Core.Pagination.PaginationParameters<string> { Page = 1, PageSize = 10, Descending = true };

            // Act
            var result = await repo.GetProgramTimeSnapshotAsync(query);

            // Assert
            var costs = result.Data.Select(i => i.HoursCost).ToList();
            Assert.Equal(costs.OrderByDescending(c => c).ToList(), costs);
        }

        [Fact]
        public async Task GetProgramTimeSnapshotAsync_AppliesPaging()
        {
            // Arrange
            var seed = BuildPlanCostSeedData();
            var repo = CreatePlanCostRepository(
                seed.Programs, seed.Projects, seed.StaffJobs, seed.Staff, seed.WorkgroupGrades, seed.ProfitCentreGrades);
            var query = new Core.Pagination.PaginationParameters<string> { Page = 1, PageSize = 1 };

            // Act
            var result = await repo.GetProgramTimeSnapshotAsync(query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Theory]
        [InlineData("directorate", "P001")]
        [InlineData("program", "P001")]
        [InlineData("customer", "P001")]
        [InlineData("contract", "P001")]
        [InlineData("project", "P001")]
        [InlineData("resourcecentre", "P001")]
        [InlineData("workgroup", "P001")]
        [InlineData("gradecode", "P001")]
        [InlineData("name", "P001")]
        [InlineData("hours", "ZT_prog")]
        [InlineData("hourscost", "ZT_prog")]
        public async Task GetProgramTimeSnapshotAsync_SortsAscending_ByProperty(string sortBy, string expectedFirstProgram)
        {
            // Arrange
            var seed = BuildPlanCostSeedData();
            var repo = CreatePlanCostRepository(
                seed.Programs, seed.Projects, seed.StaffJobs, seed.Staff, seed.WorkgroupGrades, seed.ProfitCentreGrades);
            var query = new Core.Pagination.PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                Descending = false
            };

            // Act
            var result = await repo.GetProgramTimeSnapshotAsync(query);

            // Assert
            Assert.Equal(expectedFirstProgram, result.Data.First().Program);
        }

        [Theory]
        [InlineData("directorate", "ZT_prog")]
        [InlineData("program", "ZT_prog")]
        [InlineData("customer", "ZT_prog")]
        [InlineData("contract", "ZT_prog")]
        [InlineData("project", "ZT_prog")]
        [InlineData("resourcecentre", "ZT_prog")]
        [InlineData("workgroup", "ZT_prog")]
        [InlineData("gradecode", "ZT_prog")]
        [InlineData("name", "ZT_prog")]
        [InlineData("hours", "P001")]
        [InlineData("hourscost", "P001")]
        public async Task GetProgramTimeSnapshotAsync_SortsDescending_ByProperty(string sortBy, string expectedFirstProgram)
        {
            // Arrange
            var seed = BuildPlanCostSeedData();
            var repo = CreatePlanCostRepository(
                seed.Programs, seed.Projects, seed.StaffJobs, seed.Staff, seed.WorkgroupGrades, seed.ProfitCentreGrades);
            var query = new Core.Pagination.PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                Descending = true
            };

            // Act
            var result = await repo.GetProgramTimeSnapshotAsync(query);

            // Assert
            Assert.Equal(expectedFirstProgram, result.Data.First().Program);
        }

        [Theory]
        [InlineData("version")]
        [InlineData("status")]
        public async Task GetProgramTimeSnapshotAsync_SortsByTiedColumn_ReturnsAllRows(string sortBy)
        {
            // Arrange ? Version and Status are identical across the seed rows (tie); the sort branch
            // is still exercised and all rows must be returned.
            var seed = BuildPlanCostSeedData();
            var repo = CreatePlanCostRepository(
                seed.Programs, seed.Projects, seed.StaffJobs, seed.Staff, seed.WorkgroupGrades, seed.ProfitCentreGrades);
            var query = new Core.Pagination.PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy
            };

            // Act
            var result = await repo.GetProgramTimeSnapshotAsync(query);

            // Assert
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetProgramTimeSnapshotAsync_WithUnknownSortColumn_ReturnsUnsortedRows()
        {
            // Arrange ? an unrecognised SortBy hits the default switch arm (no ordering applied).
            var seed = BuildPlanCostSeedData();
            var repo = CreatePlanCostRepository(
                seed.Programs, seed.Projects, seed.StaffJobs, seed.Staff, seed.WorkgroupGrades, seed.ProfitCentreGrades);
            var query = new Core.Pagination.PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "nonexistentcolumn"
            };

            // Act
            var result = await repo.GetProgramTimeSnapshotAsync(query);

            // Assert
            Assert.Equal(2, result.Data.Count());
        }

        #endregion
    }
}