using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Apha.FPS.DataAccess.UnitTests.Repository.AnimalRepositoryTest
{
    public class AnimalRepositoryTests
    {
        private const int DefaultFpsYear = 2025;
        private const string DefaultUserEmail = "test@example.com";

        #region Helpers

        private static Animal BuildAnimal(
            string animalType = "CATTLE",
            string? species = "Bovine",
            string? securityLevel = "L1",
            decimal? dailyRate = 50m,
            decimal? defraDailyRate = 60m,
            bool planByWeek = false,
            int fpsYear = DefaultFpsYear) =>
            new()
            {
                AnimalType = animalType,
                Species = species,
                SecurityLevel = securityLevel,
                DailyRate = dailyRate,
                DefraDailyRate = defraDailyRate,
                PlanByWeek = planByWeek,
                FpsYear = fpsYear
            };

        private static Mock<IFpsRequestContext> CreateRequestContextMock()
        {
            var mock = new Mock<IFpsRequestContext>();
            mock.Setup(x => x.FpsYear).Returns(DefaultFpsYear);
            mock.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);
            return mock;
        }

        private static AnimalRepository CreateRepository(IEnumerable<Animal>? animals = null)
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            if (animals != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(animals);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                dbContext.Setup(x => x.Animals).Returns(mockSet.Object);
            }

            RepositoryTestHelper.SetupSaveChanges(dbContext);
            return new AnimalRepository(dbContext.Object, requestCtx.Object);
        }

        private static AnimalRepository CreateRepositoryForGlobalCost(
            IEnumerable<AnimalRequest> animalRequests,
            IEnumerable<Animal> animals)
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext  = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var animalRequestSet = RepositoryTestHelper.CreateMockDbSet(animalRequests);
            RepositoryTestHelper.SetupDbSetOperations(animalRequestSet);
            dbContext.Setup(x => x.AnimalRequests).Returns(animalRequestSet.Object);

            var animalSet = RepositoryTestHelper.CreateMockDbSet(animals);
            RepositoryTestHelper.SetupDbSetOperations(animalSet);
            dbContext.Setup(x => x.Animals).Returns(animalSet.Object);

            RepositoryTestHelper.SetupSaveChanges(dbContext);
            return new AnimalRepository(dbContext.Object, requestCtx.Object);
        }

        private static (AnimalRepository Repo, Mock<FpsDbContext> Context, Mock<DbSet<Animal>> DbSet)
            CreateRepositoryWithMocks(IEnumerable<Animal>? animals = null)
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var dbSet = RepositoryTestHelper.CreateMockDbSet(animals ?? []);
            RepositoryTestHelper.SetupDbSetOperations(dbSet);
            dbContext.Setup(x => x.Animals).Returns(dbSet.Object);

            RepositoryTestHelper.SetupSaveChanges(dbContext);
            return (new AnimalRepository(dbContext.Object, requestCtx.Object), dbContext, dbSet);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenDbContextIsNull()
        {
            var ctx = CreateRequestContextMock();
            Assert.Throws<ArgumentNullException>(() => new AnimalRepository(null!, ctx.Object));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenRequestContextIsNull()
        {
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(CreateRequestContextMock().Object);
            Assert.Throws<ArgumentNullException>(() => new AnimalRepository(dbContext.Object, null!));
        }

        #endregion

        #region GetAllAnimalsAsync (non-paged) Tests

        [Fact]
        public async Task GetAllAnimalsAsync_ReturnsOnlyCurrentFpsYear()
        {
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE", fpsYear: DefaultFpsYear),
                BuildAnimal("SHEEP", fpsYear: 2020)
            };
            var repo = CreateRepository(animals);
            var result = await repo.GetAllAnimalsAsync();
            Assert.Single(result);
            Assert.Equal("CATTLE", result.First().AnimalType);
        }

        [Fact]
        public async Task GetAllAnimalsAsync_ReturnsEmpty_WhenNoAnimals()
        {
            var repo = CreateRepository([]);
            var result = await repo.GetAllAnimalsAsync();
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAnimalsAsync_ReturnsOrderedByAnimalType()
        {
            var animals = new List<Animal>
            {
                BuildAnimal("SHEEP"),
                BuildAnimal("CATTLE"),
                BuildAnimal("PIG")
            };
            var repo = CreateRepository(animals);
            var result = (await repo.GetAllAnimalsAsync()).ToList();
            Assert.Equal("CATTLE", result[0].AnimalType);
            Assert.Equal("PIG", result[1].AnimalType);
            Assert.Equal("SHEEP", result[2].AnimalType);
        }

        [Fact]
        public async Task GetAllAnimalsAsync_ReturnsMultipleRecords_WhenMultipleExist()
        {
            var animals = new List<Animal> { BuildAnimal("CATTLE"), BuildAnimal("SHEEP") };
            var repo = CreateRepository(animals);
            var result = await repo.GetAllAnimalsAsync();
            Assert.Equal(2, result.Count());
        }

        #endregion

        #region GetAllAnimalsAsync (paged) Tests

        [Fact]
        public async Task GetAllAnimalsPagedAsync_ReturnsEmptyPagedData_WhenNoRecords()
        {
            var repo = CreateRepository([]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_ReturnsAllRecordsOnPage1()
        {
            var animals = new List<Animal> { BuildAnimal("CATTLE"), BuildAnimal("SHEEP"), BuildAnimal("PIG") };
            var repo = CreateRepository(animals);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_ReturnsCorrectPage()
        {
            var animals = new List<Animal>
            {
                BuildAnimal("A"), BuildAnimal("B"), BuildAnimal("C"),
                BuildAnimal("D"), BuildAnimal("E")
            };
            var repo = CreateRepository(animals);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_FiltersByAnimalType()
        {
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE"),
                BuildAnimal("SHEEP"),
                BuildAnimal("CAT")
            };
            var repo = CreateRepository(animals);
            var filter = System.Text.Json.JsonSerializer.Serialize(
                new Dictionary<string, string> { { "AnimalType", "CAT" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_FiltersBySpecies()
        {
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE", species: "Bovine"),
                BuildAnimal("SHEEP", species: "Ovine"),
                BuildAnimal("COW", species: "Bovine")
            };
            var repo = CreateRepository(animals);
            var filter = System.Text.Json.JsonSerializer.Serialize(
                new Dictionary<string, string> { { "Species", "Bovine" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_FiltersBySecurityLevel()
        {
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE", securityLevel: "L1"),
                BuildAnimal("SHEEP", securityLevel: "L2"),
                BuildAnimal("PIG", securityLevel: "L1")
            };
            var repo = CreateRepository(animals);
            var filter = System.Text.Json.JsonSerializer.Serialize(
                new Dictionary<string, string> { { "SecurityLevel", "L1" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_ReturnsAll_WhenFilterIsNull()
        {
            var animals = new List<Animal> { BuildAnimal("CATTLE"), BuildAnimal("SHEEP") };
            var repo = CreateRepository(animals);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_ReturnsAll_WhenFilterIsEmpty()
        {
            var animals = new List<Animal> { BuildAnimal("CATTLE"), BuildAnimal("SHEEP") };
            var repo = CreateRepository(animals);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "" };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        #endregion

        #region Sorting Tests

        [Theory]
        [InlineData("animaltype", false, "CATTLE", "SHEEP")]
        [InlineData("animaltype", true, "SHEEP", "CATTLE")]
        [InlineData("species", false, "Bovine", "Ovine")]
        [InlineData("species", true, "Ovine", "Bovine")]
        [InlineData("dailyrate", false, "40", "60")]
        [InlineData("dailyrate", true, "60", "40")]
        [InlineData("defradailyrate", false, "50", "70")]
        [InlineData("defradailyrate", true, "70", "50")]
        public async Task GetAllAnimalsPagedAsync_AppliesSorting_Correctly(
            string sortBy, bool descending, string expectedFirst, string expectedSecond)
        {
            var animals = new List<Animal>
            {
                BuildAnimal("SHEEP", species: "Ovine",  dailyRate: 60m, defraDailyRate: 70m),
                BuildAnimal("CATTLE", species: "Bovine", dailyRate: 40m, defraDailyRate: 50m)
            };
            var repo = CreateRepository(animals);
            var query = new PaginationParameters<string>
            { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };
            var result = await repo.GetAllAnimalsAsync(query);
            var list = result.Data.ToList();
            var actualFirst = sortBy switch
            {
                "species"        => list[0].Species,
                "dailyrate"      => list[0].DailyRate?.ToString(),
                "defradailyrate" => list[0].DefraDailyRate?.ToString(),
                _                => list[0].AnimalType
            };
            var actualSecond = sortBy switch
            {
                "species"        => list[1].Species,
                "dailyrate"      => list[1].DailyRate?.ToString(),
                "defradailyrate" => list[1].DefraDailyRate?.ToString(),
                _                => list[1].AnimalType
            };
            Assert.Equal(expectedFirst, actualFirst);
            Assert.Equal(expectedSecond, actualSecond);
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_SortsByPlanByWeek_Ascending()
        {
            var animals = new List<Animal>
            {
                BuildAnimal("SHEEP", planByWeek: true),
                BuildAnimal("CATTLE", planByWeek: false)
            };
            var repo = CreateRepository(animals);
            var query = new PaginationParameters<string>
            { Page = 1, PageSize = 10, SortBy = "planbyweek", Descending = false };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.False(result.Data.First().PlanByWeek);
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_UnknownSortBy_ReturnsAllRecords()
        {
            var animals = new List<Animal> { BuildAnimal("CATTLE"), BuildAnimal("SHEEP") };
            var repo = CreateRepository(animals);
            var query = new PaginationParameters<string>
            { Page = 1, PageSize = 10, SortBy = "unknown" };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllAnimalsPagedAsync_NullSortBy_ReturnsAllRecords()
        {
            var animals = new List<Animal> { BuildAnimal("CATTLE"), BuildAnimal("SHEEP") };
            var repo = CreateRepository(animals);
            var query = new PaginationParameters<string>
            { Page = 1, PageSize = 10, SortBy = null };
            var result = await repo.GetAllAnimalsAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        #endregion

        #region GetAnimalByIdAsync Tests

        [Fact]
        public async Task GetAnimalByIdAsync_ReturnsAnimal_WhenFound()
        {
            var repo = CreateRepository([BuildAnimal("CATTLE")]);
            var result = await repo.GetAnimalByIdAsync("CATTLE");
            Assert.NotNull(result);
            Assert.Equal("CATTLE", result.AnimalType);
        }

        [Fact]
        public async Task GetAnimalByIdAsync_ReturnsNull_WhenNotFound()
        {
            var repo = CreateRepository([]);
            var result = await repo.GetAnimalByIdAsync("NOTEXIST");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAnimalByIdAsync_ThrowsArgumentException_WhenEmpty()
        {
            var repo = CreateRepository([]);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetAnimalByIdAsync(""));
        }

        [Fact]
        public async Task GetAnimalByIdAsync_ThrowsArgumentException_WhenWhiteSpace()
        {
            var repo = CreateRepository([]);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetAnimalByIdAsync("   "));
        }

        [Fact]
        public async Task GetAnimalByIdAsync_ReturnsOnlyCurrentFpsYear()
        {
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE", fpsYear: DefaultFpsYear),
                BuildAnimal("CATTLE", fpsYear: 2020)
            };
            var repo = CreateRepository(animals);
            var result = await repo.GetAnimalByIdAsync("CATTLE");
            Assert.NotNull(result);
            Assert.Equal(DefaultFpsYear, result.FpsYear);
        }

        #endregion

        #region AddAnimalAsync Tests

        [Fact]
        public async Task AddAnimalAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository([]);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.AddAnimalAsync(null!));
        }

        [Fact]
        public async Task AddAnimalAsync_SetsFpsYearFromContext()
        {
            var (repo, _, dbSet) = CreateRepositoryWithMocks([]);
            dbSet.Setup(x => x.Add(It.IsAny<Animal>()));
            var entity = BuildAnimal("NEWANIMAL");
            entity.FpsYear = 0;
            var result = await repo.AddAnimalAsync(entity);
            Assert.Equal(DefaultFpsYear, result.FpsYear);
        }

        [Fact]
        public async Task AddAnimalAsync_AddsEntityAndSavesChanges()
        {
            var (repo, context, dbSet) = CreateRepositoryWithMocks([]);
            dbSet.Setup(x => x.Add(It.IsAny<Animal>()));
            var entity = BuildAnimal("NEWANIMAL");
            var result = await repo.AddAnimalAsync(entity);
            Assert.NotNull(result);
            Assert.Equal("NEWANIMAL", result.AnimalType);
            dbSet.Verify(x => x.Add(It.Is<Animal>(a => a.AnimalType == "NEWANIMAL")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(context);
        }

        #endregion

        #region UpdateAnimalAsync Tests

        [Fact]
        public async Task UpdateAnimalAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository([]);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAnimalAsync(null!));
        }

        [Fact]
        public async Task UpdateAnimalAsync_SetsFpsYearFromContext()
        {
            var (repo, _, dbSet) = CreateRepositoryWithMocks([BuildAnimal("CATTLE")]);
            dbSet.Setup(x => x.Update(It.IsAny<Animal>()));
            var entity = BuildAnimal("CATTLE");
            entity.FpsYear = 0;
            var result = await repo.UpdateAnimalAsync(entity);
            Assert.Equal(DefaultFpsYear, result.FpsYear);
        }

        [Fact]
        public async Task UpdateAnimalAsync_UpdatesEntityAndSavesChanges()
        {
            var (repo, context, dbSet) = CreateRepositoryWithMocks([BuildAnimal("CATTLE")]);
            dbSet.Setup(x => x.Update(It.IsAny<Animal>()));
            var entity = BuildAnimal("CATTLE", species: "Updated", dailyRate: 99m);
            var result = await repo.UpdateAnimalAsync(entity);
            Assert.NotNull(result);
            dbSet.Verify(x => x.Update(It.Is<Animal>(a => a.AnimalType == "CATTLE")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(context);
        }

        #endregion

        #region DeleteAnimalAsync Tests

        [Fact]
        public async Task DeleteAnimalAsync_ThrowsArgumentException_WhenEmpty()
        {
            var repo = CreateRepository([]);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.DeleteAnimalAsync(""));
        }

        [Fact]
        public async Task DeleteAnimalAsync_ThrowsArgumentException_WhenWhiteSpace()
        {
            var repo = CreateRepository([]);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.DeleteAnimalAsync("   "));
        }

        [Fact]
        public async Task DeleteAnimalAsync_ReturnsFalse_WhenNotFound()
        {
            var repo = CreateRepository([]);
            var result = await repo.DeleteAnimalAsync("NOTEXIST");
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAnimalAsync_ReturnsTrue_WhenFound()
        {
            var repo = CreateRepository([BuildAnimal("CATTLE")]);
            var result = await repo.DeleteAnimalAsync("CATTLE");
            Assert.True(result);
        }

        #endregion

        #region GetGlobalAnimalCostAsync Tests

        [Fact]
        public async Task GetGlobalAnimalCostAsync_ReturnsZero_WhenNoData()
        {
            var repo = CreateRepositoryForGlobalCost([], []);
            var result = await repo.GetGlobalAnimalCostAsync();
            Assert.Equal(0m, result);
        }

        [Fact]
        public async Task GetGlobalAnimalCostAsync_ReturnsSingleRowCost()
        {
            // 2 days × 3 animals × £10/day = £60
            var requests = new List<AnimalRequest>
            {
                new() { JobCode = "J1", AnimalType = "CATTLE", NumberOfDays = 2d, NumberOfAnimals = 3d }
            };
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE", dailyRate: 10m)
            };
            var repo = CreateRepositoryForGlobalCost(requests, animals);
            var result = await repo.GetGlobalAnimalCostAsync();
            Assert.Equal(60m, result);
        }

        [Fact]
        public async Task GetGlobalAnimalCostAsync_SumsMultipleRows()
        {
            // Row 1: 1 × 2 × 5 = 10; Row 2: 4 × 1 × 3 = 12 → total 22
            var requests = new List<AnimalRequest>
            {
                new() { JobCode = "J1", AnimalType = "CATTLE", NumberOfDays = 1d, NumberOfAnimals = 2d },
                new() { JobCode = "J2", AnimalType = "SHEEP",  NumberOfDays = 4d, NumberOfAnimals = 1d }
            };
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE", dailyRate: 5m),
                BuildAnimal("SHEEP",  dailyRate: 3m)
            };
            var repo = CreateRepositoryForGlobalCost(requests, animals);
            var result = await repo.GetGlobalAnimalCostAsync();
            Assert.Equal(22m, result);
        }

        [Fact]
        public async Task GetGlobalAnimalCostAsync_TreatsNullDailyRateAsZero()
        {
            var requests = new List<AnimalRequest>
            {
                new() { JobCode = "J1", AnimalType = "CATTLE", NumberOfDays = 5d, NumberOfAnimals = 2d }
            };
            var animals = new List<Animal>
            {
                BuildAnimal("CATTLE", dailyRate: null)
            };
            var repo = CreateRepositoryForGlobalCost(requests, animals);
            var result = await repo.GetGlobalAnimalCostAsync();
            Assert.Equal(0m, result);
        }

        #endregion
    }
}
