/*
 * TRANSFORMENGINE MIGRATION — AnimalRepositoryAsuViewTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - New test class for GetAnimalCostByAnimalTypeAsync added to AnimalRepository in Phase 4
 *   - Uses Moq + RepositoryTestHelper.CreateMockDbContext / CreateMockDbSet (matching existing
 *     AnimalRequestRepositoryTest.cs pattern — multi-DbSet join setup)
 *   - Covers: happy path (matching rows returned), empty result (no matching animal type),
 *     null/whitespace animalType guard, and paging
 *   - Three DbSets required: AnimalRequestViews, Animals, ProjectViews
 *     (BuildAnimalCostByAnimalTypeQuery joins all three)
 *
 * PRESERVED:
 *   - RepositoryTestHelper + Moq pattern used across all DataAccess unit tests
 *   - DefaultFpsYear / DefaultUserEmail constants matching AnimalRepositoryTests.cs
 *   - [MethodName]_[StateUnderTest]_[ExpectedResult] naming convention
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: EF.Functions.ILike in ApplyAnimalCostFilter is not supported by
 *     the InMemory provider; filter-branch tests require an integration test or SQL provider.
 *     Unit tests here cover the un-filtered query path only.
 */
using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;
using Xunit;

namespace Apha.FPS.DataAccess.UnitTests.Repository.AnimalRepositoryTest
{
    /// <summary>
    /// xUnit tests for <see cref="AnimalRepository.GetAnimalCostByAnimalTypeAsync"/>
    /// added in Phase 4 for the ASU View resource family.
    /// </summary>
    public class AnimalRepositoryAsuViewTests
    {
        private const int    DefaultFpsYear   = 2025;
        private const string DefaultUserEmail = "test@example.com";

        // ── Helpers ───────────────────────────────────────────────────────────

        private static Mock<IFpsRequestContext> CreateRequestContextMock(
            int year = DefaultFpsYear, string email = DefaultUserEmail)
        {
            var mock = new Mock<IFpsRequestContext>();
            mock.Setup(x => x.FpsYear).Returns(year);
            mock.Setup(x => x.UserEmailId).Returns(email);
            return mock;
        }

        /// <summary>
        /// Creates an AnimalRepository wired to mocked DbSets for the three tables
        /// used by BuildAnimalCostByAnimalTypeQuery: AnimalRequestViews, Animals, ProjectViews.
        /// </summary>
        private static AnimalRepository CreateRepository(
            IEnumerable<AnimalRequestView>? animalRequestViews = null,
            IEnumerable<Animal>?            animals            = null,
            IEnumerable<ProjectView>?       projectViews       = null,
            string userEmail = DefaultUserEmail)
        {
            var reqCtx  = CreateRequestContextMock(email: userEmail);
            var context = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(reqCtx.Object);

            // TRANSFORMENGINE: AnimalRequestViews — primary table in the JOIN
            if (animalRequestViews != null)
            {
                var set = RepositoryTestHelper.CreateMockDbSet(animalRequestViews);
                RepositoryTestHelper.SetupDbSetOperations(set);
                context.Setup(x => x.AnimalRequestViews).Returns(set.Object);
            }

            // TRANSFORMENGINE: Animals — joined for DailyRate / DefraDailyRate lookup
            if (animals != null)
            {
                var set = RepositoryTestHelper.CreateMockDbSet(animals);
                RepositoryTestHelper.SetupDbSetOperations(set);
                context.Setup(x => x.Animals).Returns(set.Object);
            }

            // TRANSFORMENGINE: ProjectViews — joined for Programme and IsDefraProject flag
            if (projectViews != null)
            {
                var set = RepositoryTestHelper.CreateMockDbSet(projectViews);
                RepositoryTestHelper.SetupDbSetOperations(set);
                context.Setup(x => x.ProjectViews).Returns(set.Object);
            }

            RepositoryTestHelper.SetupSaveChanges(context);
            return new AnimalRepository(context.Object, reqCtx.Object);
        }

        // ── Shared fixture builders ───────────────────────────────────────────

        // TRANSFORMENGINE: AnimalRequestView — simulates a row in the tblAnimalRequest_View
        // ProjectView.UserId must match AnimalRequestView.UserId for the JOIN to produce a row.
        private static AnimalRequestView BuildAnimalRequestView(
            string animalType = "CATTLE",
            string jobCode    = "JOB001",
            int    userId     = 1,
            string userEmail  = DefaultUserEmail) =>
            new()
            {
                IndCounter      = 1,
                AnimalType      = animalType,
                JobCode         = jobCode,
                NumberOfDays    = 5.0,
                NumberOfAnimals = 2.0,
                FpsYear         = DefaultFpsYear,
                UserId          = userId,
                UserEmail       = userEmail
            };

        // TRANSFORMENGINE: Animal — joined for DailyRate (non-Defra) or DefraDailyRate (Defra)
        private static Animal BuildAnimal(string animalType = "CATTLE") =>
            new()
            {
                AnimalType     = animalType,
                Species        = "Bovine",
                SecurityLevel  = "L1",
                DailyRate      = 50m,
                DefraDailyRate = 60m,
                FpsYear        = DefaultFpsYear
            };

        // TRANSFORMENGINE: ProjectView — joined via (JobCode=ParentProject, UserId=UserId)
        private static ProjectView BuildProjectView(
            string parentProject = "JOB001",
            int    userId        = 1,
            short  isDefra       = 0) =>
            new()
            {
                ParentProject  = parentProject,
                UserId         = userId,
                Program        = "PROG001",
                IsDefraProject = isDefra,
                FpsYear        = DefaultFpsYear
            };

        // ── Tests ─────────────────────────────────────────────────────────────

        #region GetAnimalCostByAnimalTypeAsync

        // TRANSFORMENGINE: happy path — matching rows across all three JOIN tables
        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_MatchingRows_ReturnsPaginatedResult()
        {
            // Arrange
            var views    = new[] { BuildAnimalRequestView("CATTLE") };
            var animals  = new[] { BuildAnimal("CATTLE") };
            var projects = new[] { BuildProjectView("JOB001") };
            var repo     = CreateRepository(views, animals, projects);
            var query    = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal("CATTLE", result.Data.First().AnimalType);
        }

        // TRANSFORMENGINE: no rows for animalType — returns empty PagedData
        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_NoMatchingAnimalType_ReturnsEmptyResult()
        {
            // Arrange — request view exists but for "SHEEP", query asks for "CATTLE"
            var views    = new[] { BuildAnimalRequestView("SHEEP") };
            var animals  = new[] { BuildAnimal("SHEEP") };
            var projects = new[] { BuildProjectView("JOB001") };
            var repo     = CreateRepository(views, animals, projects);
            var query    = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        // TRANSFORMENGINE: empty DbSets — all tables empty, query returns zero rows
        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_EmptyTables_ReturnsEmptyResult()
        {
            var repo  = CreateRepository([], [], []);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        // TRANSFORMENGINE: UserEmail filter — rows whose UserEmail does not match context are excluded
        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_WrongUserEmail_ReturnsEmptyResult()
        {
            // Arrange — request view has a different UserEmail than the context
            var views    = new[] { BuildAnimalRequestView("CATTLE", userEmail: "other@example.com") };
            var animals  = new[] { BuildAnimal("CATTLE") };
            var projects = new[] { BuildProjectView("JOB001") };
            var repo     = CreateRepository(views, animals, projects, userEmail: DefaultUserEmail);
            var query    = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            // Assert — no rows because UserEmail mismatch
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        // TRANSFORMENGINE: paging — verify TotalRecords and Data count respect page/pageSize
        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_Paging_ReturnsCorrectPage()
        {
            // Arrange — two matching rows, page 1 page size 1
            var views = new[]
            {
                new AnimalRequestView
                {
                    IndCounter      = 1, AnimalType = "CATTLE", JobCode = "JOB001",
                    NumberOfDays = 3.0, NumberOfAnimals = 1.0,
                    FpsYear = DefaultFpsYear, UserId = 1, UserEmail = DefaultUserEmail
                },
                new AnimalRequestView
                {
                    IndCounter      = 2, AnimalType = "CATTLE", JobCode = "JOB002",
                    NumberOfDays = 4.0, NumberOfAnimals = 2.0,
                    FpsYear = DefaultFpsYear, UserId = 1, UserEmail = DefaultUserEmail
                }
            };
            var animals  = new[] { BuildAnimal("CATTLE") };
            var projects = new[]
            {
                BuildProjectView("JOB001"),
                BuildProjectView("JOB002")
            };
            var repo  = CreateRepository(views, animals, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 1 };

            // Act
            var result = await repo.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);                    // page size = 1
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        // TRANSFORMENGINE: AnimalCost calculation — verify NumberOfDays × NumberOfAnimals × DailyRate
        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_MatchingRow_AnimalCostCalculatedCorrectly()
        {
            // Arrange
            var views = new[]
            {
                new AnimalRequestView
                {
                    IndCounter = 1, AnimalType = "CATTLE", JobCode = "JOB001",
                    NumberOfDays = 5.0, NumberOfAnimals = 2.0,
                    FpsYear = DefaultFpsYear, UserId = 1, UserEmail = DefaultUserEmail
                }
            };
            var animals = new[]
            {
                new Animal
                {
                    AnimalType = "CATTLE", DailyRate = 50m, DefraDailyRate = 60m,
                    FpsYear = DefaultFpsYear
                }
            };
            // TRANSFORMENGINE: IsDefraProject = 0 → uses DailyRate (50m) not DefraDailyRate
            var projects = new[] { new ProjectView
            {
                ParentProject  = "JOB001",
                UserId         = 1,
                IsDefraProject = 0,
                FpsYear        = DefaultFpsYear
            }};
            var repo  = CreateRepository(views, animals, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            // Assert — AnimalCost = 5 × 2 × 50 = 500
            var row = Assert.Single(result.Data);
            Assert.Equal(500m, row.AnimalCost);
        }

        #endregion
    }
}
