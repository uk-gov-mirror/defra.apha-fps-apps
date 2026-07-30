using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;
using Xunit;

namespace Apha.FPS.DataAccess.UnitTests.Repository.CostCentreRepositoryTest
{
    public class CostCentreRepositoryTests
    {
        private static CostCentreRepository CreateRepository(IEnumerable<CostCentre>? costCentres = null)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(2024);
            requestContext.UserEmailId.Returns("test@example.com");

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var costCentreSet = RepositoryTestHelper.CreateMockDbSet(costCentres ?? []);
            RepositoryTestHelper.SetupDbSetOperations(costCentreSet);
            mockContext.Setup(x => x.CostCentres).Returns(costCentreSet.Object);

            RepositoryTestHelper.SetupTransaction(mockContext);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new CostCentreRepository(mockContext.Object, requestContext);
        }

        private static CostCentre BuildEntity(
            double costCentreNo = 100.0,
            string profitCentre = "PC01",
            int fpsYear = 2024) =>
            new() { CostCentreNo = costCentreNo, ProfitCentre = profitCentre, FpsYear = fpsYear };

        #region GetAllPagedAsync Tests

        [Fact]
        public async Task GetAllPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            var repo = CreateRepository();
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetAllPagedAsync(null!));
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsEmptyPagedData_WhenNoRecords()
        {
            var repo  = CreateRepository([]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAllPagedAsync(query);

            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsAllRecords()
        {
            var entities = new List<CostCentre>
            {
                BuildEntity(100.0), BuildEntity(200.0), BuildEntity(300.0)
            };
            var repo  = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAllPagedAsync(query);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsCorrectPage()
        {
            var entities = new List<CostCentre>
            {
                BuildEntity(100.0), BuildEntity(200.0), BuildEntity(300.0),
                BuildEntity(400.0), BuildEntity(500.0)
            };
            var repo  = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            var result = await repo.GetAllPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetAllPagedAsync_FiltersByCostCentreNo()
        {
            var entities = new List<CostCentre>
            {
                BuildEntity(100.0, "PC01"), BuildEntity(200.0, "PC02"), BuildEntity(300.0, "PC03")
            };
            var repo   = CreateRepository(entities);
            var filter = System.Text.Json.JsonSerializer.Serialize(
                new Dictionary<string, string> { { "CostCentreNo", "100" } });
            var query  = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };

            var result = await repo.GetAllPagedAsync(query);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetAllPagedAsync_FiltersByProfitCentre()
        {
            var entities = new List<CostCentre>
            {
                BuildEntity(100.0, "PC01"), BuildEntity(200.0, "PC02"), BuildEntity(300.0, "PC01")
            };
            var repo   = CreateRepository(entities);
            var filter = System.Text.Json.JsonSerializer.Serialize(
                new Dictionary<string, string> { { "ProfitCentre", "PC01" } });
            var query  = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };

            var result = await repo.GetAllPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsAll_WhenFilterIsEmptyObject()
        {
            var entities = new List<CostCentre>
            {
                BuildEntity(100.0), BuildEntity(200.0)
            };
            var repo  = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };

            var result = await repo.GetAllPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllPagedAsync_OrdersByCostCentreNoAscByDefault()
        {
            var entities = new List<CostCentre>
            {
                BuildEntity(300.0), BuildEntity(100.0), BuildEntity(200.0)
            };
            var repo  = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAllPagedAsync(query);
            var list   = result.Data.ToList();

            Assert.Equal(100.0, list[0].CostCentreNo);
        }

        [Theory]
        [InlineData("CostCentreNo", false, 100.0, 200.0)]
        [InlineData("CostCentreNo", true,  200.0, 100.0)]
        public async Task GetAllPagedAsync_SortsByCostCentreNo(string sortBy, bool descending, double firstExpected, double secondExpected)
        {
            var entities = new List<CostCentre>
            {
                BuildEntity(200.0), BuildEntity(100.0)
            };
            var repo  = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            var result = await repo.GetAllPagedAsync(query);
            var list   = result.Data.ToList();

            Assert.Equal(firstExpected,  list[0].CostCentreNo);
            Assert.Equal(secondExpected, list[1].CostCentreNo);
        }

        [Theory]
        [InlineData("ProfitCentre", false, "PC01", "PC02")]
        [InlineData("ProfitCentre", true,  "PC02", "PC01")]
        public async Task GetAllPagedAsync_SortsByProfitCentre(string sortBy, bool descending, string firstExpected, string secondExpected)
        {
            var entities = new List<CostCentre>
            {
                BuildEntity(200.0, "PC02"), BuildEntity(100.0, "PC01")
            };
            var repo  = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            var result = await repo.GetAllPagedAsync(query);
            var list   = result.Data.ToList();

            Assert.Equal(firstExpected,  list[0].ProfitCentre);
            Assert.Equal(secondExpected, list[1].ProfitCentre);
        }

        [Theory]
        [InlineData("UnknownColumn", false, 100.0, 200.0)]
        [InlineData("UnknownColumn", true,  200.0, 100.0)]
        public async Task GetAllPagedAsync_SortsByCostCentreNo_WhenSortByIsUnknown(string sortBy, bool descending, double firstExpected, double secondExpected)
        {
            var entities = new List<CostCentre>
            {
                BuildEntity(200.0), BuildEntity(100.0)
            };
            var repo  = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            var result = await repo.GetAllPagedAsync(query);
            var list   = result.Data.ToList();

            Assert.Equal(firstExpected,  list[0].CostCentreNo);
            Assert.Equal(secondExpected, list[1].CostCentreNo);
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsAll_WhenFilterIsInvalidJson()
        {
            var entities = new List<CostCentre> { BuildEntity(100.0), BuildEntity(200.0) };
            var repo  = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "null" };

            var result = await repo.GetAllPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllPagedAsync_IgnoresCostCentreNoFilter_WhenValueIsNonNumeric()
        {
            var entities = new List<CostCentre> { BuildEntity(100.0), BuildEntity(200.0) };
            var repo   = CreateRepository(entities);
            var filter = System.Text.Json.JsonSerializer.Serialize(
                new Dictionary<string, string> { { "CostCentreNo", "abc" } });
            var query  = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };

            var result = await repo.GetAllPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllPagedAsync_IgnoresProfitCentreFilter_WhenValueIsWhitespace()
        {
            var entities = new List<CostCentre>
            {
                BuildEntity(100.0, "PC01"), BuildEntity(200.0, "PC02")
            };
            var repo   = CreateRepository(entities);
            var filter = System.Text.Json.JsonSerializer.Serialize(
                new Dictionary<string, string> { { "ProfitCentre", "   " } });
            var query  = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };

            var result = await repo.GetAllPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            var repo   = CreateRepository([]);
            var result = await repo.GetByIdAsync(999.0, 2024);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsRecord_WhenFound()
        {
            var entities = new List<CostCentre> { BuildEntity(100.0, "PC01", 2024) };
            var repo     = CreateRepository(entities);

            var result = await repo.GetByIdAsync(100.0, 2024);

            Assert.NotNull(result);
            Assert.Equal(100.0, result.CostCentreNo);
            Assert.Equal("PC01", result.ProfitCentre);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenCostCentreNoMatchesButFpsYearDiffers()
        {
            var entities = new List<CostCentre> { BuildEntity(100.0, "PC01", 2023) };
            var repo     = CreateRepository(entities);

            // Searching for fpsYear 2024 but entity has 2023
            var result = await repo.GetByIdAsync(100.0, 2024);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsRecord_WhenCostCentreNoDiffersWithinTolerance()
        {
            // Stored value differs from the lookup value by less than the matching tolerance (1e-9)
            var entities = new List<CostCentre> { BuildEntity(100.0000000001, "PC01", 2024) };
            var repo     = CreateRepository(entities);

            var result = await repo.GetByIdAsync(100.0, 2024);

            Assert.NotNull(result);
            Assert.Equal("PC01", result.ProfitCentre);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenCostCentreNoDiffersBeyondTolerance()
        {
            // Stored value differs from the lookup value by more than the matching tolerance
            var entities = new List<CostCentre> { BuildEntity(100.001, "PC01", 2024) };
            var repo     = CreateRepository(entities);

            var result = await repo.GetByIdAsync(100.0, 2024);

            Assert.Null(result);
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenExists()
        {
            var entities = new List<CostCentre> { BuildEntity(100.0, "PC01", 2024) };
            var repo     = CreateRepository(entities);

            var result = await repo.ExistsAsync(100.0, 2024);

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenNotExists()
        {
            var repo   = CreateRepository([]);
            var result = await repo.ExistsAsync(999.0, 2024);
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenYearDiffers()
        {
            var entities = new List<CostCentre> { BuildEntity(100.0, "PC01", 2023) };
            var repo     = CreateRepository(entities);

            var result = await repo.ExistsAsync(100.0, 2024);

            Assert.False(result);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository([]);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_ReturnsEntity_WhenSuccessful()
        {
            var entity = BuildEntity(100.0, "PC01", 2024);
            var repo   = CreateRepository([]);

            var result = await repo.CreateAsync(entity);

            Assert.NotNull(result);
            Assert.Equal(100.0, result.CostCentreNo);
            Assert.Equal("PC01", result.ProfitCentre);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository([]);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAsync(100.0, 2024, null!));
        }

        [Fact]
        public async Task UpdateAsync_ReturnsPassedEntity_WhenRecordNotFound()
        {
            var entity = BuildEntity(100.0, "PC01", 2024);
            var repo   = CreateRepository([]);

            var result = await repo.UpdateAsync(999.0, 2024, entity);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesProfitCentre_WhenRecordExists()
        {
            // Arrange
            var existing = BuildEntity(100.0, "PC01", 2024);
            var updated  = BuildEntity(100.0, "PC02", 2024);
            var repo     = CreateRepository([existing]);

            // Act
            var result = await repo.UpdateAsync(100.0, 2024, updated);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PC02", result.ProfitCentre);
        }

        [Fact]
        public async Task UpdateAsync_DeletesAndInserts_WhenCostCentreNoChanges()
        {
            // Arrange: changing CostCentreNo (part of composite PK) triggers delete-old + insert-new
            var existing = BuildEntity(100.0, "PC01", 2024);
            var updated  = BuildEntity(200.0, "PC02");
            var repo     = CreateRepository([existing]);

            // Act
            var result = await repo.UpdateAsync(100.0, 2024, updated);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200.0, result.CostCentreNo);
            Assert.Equal("PC02", result.ProfitCentre);
            Assert.Equal(2024, result.FpsYear);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            var repo   = CreateRepository([]);
            var result = await repo.DeleteAsync(999.0, 2024);
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenDeletedSuccessfully()
        {
            var entities = new List<CostCentre> { BuildEntity(100.0, "PC01", 2024) };
            var repo     = CreateRepository(entities);

            var result = await repo.DeleteAsync(100.0, 2024);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenYearDiffers()
        {
            var entities = new List<CostCentre> { BuildEntity(100.0, "PC01", 2023) };
            var repo     = CreateRepository(entities);

            // Searching for fpsYear 2024 but entity has 2023 — should not find it
            var result = await repo.DeleteAsync(100.0, 2024);

            Assert.False(result);
        }

        #endregion
    }
}
