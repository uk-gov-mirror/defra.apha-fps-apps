using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;
using Xunit;

namespace Apha.FPS.DataAccess.UnitTests.Repository.TestRequirementRCCostRepositoryTest
{
    public class TestRequirementRCCostRepositoryTests
    {
        private const int DefaultFpsYear = 2025;
        private const string DefaultUserEmail = "test@example.com";
        private const string DefaultTestCode = "TEST001";
        private const string DefaultBuyer = "BUYER01";
        private const string DefaultProfitCentre = "PC001";

        private static Mock<IFpsRequestContext> CreateMockRequestContext(int year = DefaultFpsYear)
        {
            var mock = new Mock<IFpsRequestContext>();
            mock.Setup(x => x.FpsYear).Returns(year);
            mock.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);
            return mock;
        }

        private static TestRequirementRCCostRepository CreateRepository(
            IEnumerable<TestRequirementRCCost>? testReqCosts = null,
            int fpsYear = DefaultFpsYear)
        {
            var mockRequestContext = CreateMockRequestContext(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            var testReqCostSet = RepositoryTestHelper.CreateMockDbSet(
                testReqCosts ?? Enumerable.Empty<TestRequirementRCCost>());
            mockContext.Setup(x => x.TestRequirementRCCosts).Returns(testReqCostSet.Object);

            return new TestRequirementRCCostRepository(mockContext.Object, mockRequestContext.Object);
        }

        #region GetPagedByTestCodeAsync

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithMatchingRecords_ReturnsPagedData()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                CreateEntity(DefaultTestCode, "BUYER01", "PC001"),
                CreateEntity(DefaultTestCode, "BUYER02", "PC002")
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithNonMatchingTestCode_ReturnsEmpty()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                CreateEntity(DefaultTestCode, DefaultBuyer, DefaultProfitCentre)
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetPagedByTestCodeAsync(query, "NOTEXIST");

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithPaging_ReturnsCorrectPage()
        {
            // Arrange — repository enforces Math.Max(pageSize, 10), so use pageSize=10 with 15 items
            var entities = Enumerable.Range(1, 15)
                .Select(i => new TestRequirementRCCost
                {
                    TestCode = DefaultTestCode,
                    Buyer = $"BUYER{i:D2}",
                    ProfitCentre = $"PC{i:D3}",
                    FpsYear = DefaultFpsYear,
                    Price = i * 10m
                }).ToList();
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 10 };

            // Act
            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode);

            // Assert
            Assert.Equal(5, result.Data.Count());
            Assert.Equal(15, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetByTestCodeAsync

        [Fact]
        public async Task GetByTestCodeAsync_WithMatchingRecords_ReturnsList()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                CreateEntity(DefaultTestCode, "BUYER01", "PC001"),
                CreateEntity(DefaultTestCode, "BUYER02", "PC002")
            };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.GetByTestCodeAsync(DefaultTestCode);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByTestCodeAsync_WithNonMatchingTestCode_ReturnsEmpty()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                CreateEntity(DefaultTestCode, DefaultBuyer, DefaultProfitCentre)
            };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.GetByTestCodeAsync("NOTEXIST");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByTestCodeAsync_MultipleRecords_ReturnsSortedByBuyerThenProfitCentre()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                new() { TestCode = DefaultTestCode, Buyer = "BUYER02", ProfitCentre = "PC001", FpsYear = DefaultFpsYear, Price = 200m },
                new() { TestCode = DefaultTestCode, Buyer = "BUYER01", ProfitCentre = "PC002", FpsYear = DefaultFpsYear, Price = 100m },
                new() { TestCode = DefaultTestCode, Buyer = "BUYER01", ProfitCentre = "PC001", FpsYear = DefaultFpsYear, Price = 50m }
            };
            var repo = CreateRepository(entities);

            // Act
            var result = (await repo.GetByTestCodeAsync(DefaultTestCode)).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("BUYER01", result.First().Buyer);
            Assert.Equal("PC001", result.First().ProfitCentre);
        }

        #endregion

        #region GetByKeyAsync

        [Fact]
        public async Task GetByKeyAsync_WithExistingCompositeKey_ReturnsRecord()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                CreateEntity(DefaultTestCode, DefaultBuyer, DefaultProfitCentre)
            };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.GetByKeyAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DefaultBuyer, result!.Buyer);
            Assert.Equal(DefaultProfitCentre, result.ProfitCentre);
        }

        [Fact]
        public async Task GetByKeyAsync_WithNonExistingKey_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestRequirementRCCost>());

            // Act
            var result = await repo.GetByKeyAsync("NOTEXIST", "B999", "PC999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByKeyAsync_WithWrongBuyer_ReturnsNull()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                CreateEntity(DefaultTestCode, "BUYER01", DefaultProfitCentre)
            };
            var repo = CreateRepository(entities);

            // Act — correct TestCode, ProfitCentre, FpsYear but wrong Buyer
            var result = await repo.GetByKeyAsync(DefaultTestCode, "WRONG_BUYER", DefaultProfitCentre);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByKeyAsync_WithWrongProfitCentre_ReturnsNull()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                CreateEntity(DefaultTestCode, DefaultBuyer, "PC001")
            };
            var repo = CreateRepository(entities);

            // Act — correct TestCode, Buyer, FpsYear but wrong ProfitCentre
            var result = await repo.GetByKeyAsync(DefaultTestCode, DefaultBuyer, "WRONG_PC");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_WithExistingRecord_ReturnsTrue()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                CreateEntity(DefaultTestCode, DefaultBuyer, DefaultProfitCentre)
            };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.ExistsAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingRecord_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestRequirementRCCost>());

            // Act
            var result = await repo.ExistsAsync("NOTEXIST", "B999", "PC999");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_PartialKeyMatch_ReturnsFalse()
        {
            // Arrange — same TestCode and Buyer but different ProfitCentre
            var entities = new List<TestRequirementRCCost>
            {
                CreateEntity(DefaultTestCode, DefaultBuyer, "PC001")
            };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.ExistsAsync(DefaultTestCode, DefaultBuyer, "DIFFERENT_PC");

            // Assert
            Assert.False(result);
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_ValidEntity_ReturnsAddedEntity()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestRequirementRCCost>());
            var entity = CreateEntity(DefaultTestCode, DefaultBuyer, DefaultProfitCentre);

            // Act
            var result = await repo.AddAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DefaultTestCode, result.TestCode);
            Assert.Equal(DefaultBuyer, result.Buyer);
            Assert.Equal(DefaultProfitCentre, result.ProfitCentre);
        }

        [Fact]
        public async Task AddAsync_ValidEntity_CallsDbSetAdd()
        {
            // Arrange
            var entity = CreateEntity(DefaultTestCode, DefaultBuyer, DefaultProfitCentre);
            var mockRequestContext = CreateMockRequestContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<TestRequirementRCCost>());
            mockContext.Setup(x => x.TestRequirementRCCosts).Returns(mockSet.Object);
            var repo = new TestRequirementRCCostRepository(mockContext.Object, mockRequestContext.Object);

            // Act
            await repo.AddAsync(entity);

            // Assert
            mockSet.Verify(s => s.Add(entity), Moq.Times.Once);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ExistingEntity_ReturnsUpdatedEntity()
        {
            // Arrange
            var existing = CreateEntity(DefaultTestCode, DefaultBuyer, DefaultProfitCentre);
            var repo = CreateRepository(new List<TestRequirementRCCost> { existing });

            var updated = new TestRequirementRCCost
            {
                TestCode = DefaultTestCode,
                Buyer = DefaultBuyer,
                ProfitCentre = DefaultProfitCentre,
                FpsYear = DefaultFpsYear,
                Price = 999m
            };

            // Act
            var result = await repo.UpdateAsync(updated);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(999m, result.Price);
        }

        [Fact]
        public async Task UpdateAsync_NonExistingEntity_ThrowsKeyNotFoundException()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestRequirementRCCost>());
            var entity = new TestRequirementRCCost
            {
                TestCode = "NOTEXIST",
                Buyer = "B999",
                ProfitCentre = "PC999",
                FpsYear = DefaultFpsYear,
                Price = 100m
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.UpdateAsync(entity));
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingRecord_ReturnsTrue()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost> { CreateEntity(DefaultTestCode, DefaultBuyer, DefaultProfitCentre) };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.DeleteAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingRecord_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestRequirementRCCost>());

            // Act
            var result = await repo.DeleteAsync("NOTEXIST", "B999", "PC999");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ExistingRecord_CallsDbSetRemove()
        {
            // Arrange
            var entity = CreateEntity(DefaultTestCode, DefaultBuyer, DefaultProfitCentre);
            var mockRequestContext = CreateMockRequestContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(new List<TestRequirementRCCost> { entity });
            mockContext.Setup(x => x.TestRequirementRCCosts).Returns(mockSet.Object);
            var repo = new TestRequirementRCCostRepository(mockContext.Object, mockRequestContext.Object);

            // Act
            await repo.DeleteAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre);

            // Assert
            mockSet.Verify(s => s.Remove(It.IsAny<TestRequirementRCCost>()), Moq.Times.Once);
        }

        #endregion

        #region Helper Methods

        private static TestRequirementRCCost CreateEntity(string testCode, string buyer, string profitCentre) =>
            new()
            {
                TestCode = testCode,
                Buyer = buyer,
                ProfitCentre = profitCentre,
                FpsYear = DefaultFpsYear,
                Price = 200m
            };

        #endregion

        #region GetPagedByTestCodeAsync - Filters

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithJsonBuyerFilter_FiltersResults()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                new() { TestCode = DefaultTestCode, Buyer = "BUYERA", ProfitCentre = "PC001", FpsYear = DefaultFpsYear, Price = 100m },
                new() { TestCode = DefaultTestCode, Buyer = "BUYERB", ProfitCentre = "PC002", FpsYear = DefaultFpsYear, Price = 200m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Buyer\":\"BUYERA\"}"
            };

            // Act
            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("BUYERA", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithJsonProfitCentreFilter_FiltersResults()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                new() { TestCode = DefaultTestCode, Buyer = DefaultBuyer, ProfitCentre = "ALPHA", FpsYear = DefaultFpsYear, Price = 100m },
                new() { TestCode = DefaultTestCode, Buyer = DefaultBuyer, ProfitCentre = "BETA",  FpsYear = DefaultFpsYear, Price = 200m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ProfitCentre\":\"ALPHA\"}"
            };

            // Act
            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("ALPHA", result.Data.First().ProfitCentre);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithNullFilter_ReturnsAll()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                new() { TestCode = DefaultTestCode, Buyer = "B1", ProfitCentre = "PC001", FpsYear = DefaultFpsYear, Price = 100m },
                new() { TestCode = DefaultTestCode, Buyer = "B2", ProfitCentre = "PC002", FpsYear = DefaultFpsYear, Price = 200m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            // Act
            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetPagedByTestCodeAsync - Sorting

        [Fact]
        public async Task GetPagedByTestCodeAsync_SortByBuyerAscending_ReturnsOrderedResults()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                new() { TestCode = DefaultTestCode, Buyer = "ZBUYER", ProfitCentre = "PC001", FpsYear = DefaultFpsYear, Price = 100m },
                new() { TestCode = DefaultTestCode, Buyer = "ABUYER", ProfitCentre = "PC002", FpsYear = DefaultFpsYear, Price = 200m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "buyer", Descending = false };

            // Act
            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode);

            // Assert
            Assert.Equal("ABUYER", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_SortByBuyerDescending_ReturnsOrderedResults()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                new() { TestCode = DefaultTestCode, Buyer = "ABUYER", ProfitCentre = "PC001", FpsYear = DefaultFpsYear, Price = 100m },
                new() { TestCode = DefaultTestCode, Buyer = "ZBUYER", ProfitCentre = "PC002", FpsYear = DefaultFpsYear, Price = 200m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "buyer", Descending = true };

            // Act
            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode);

            // Assert
            Assert.Equal("ZBUYER", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_SortByProfitCentreAscending_ReturnsOrderedResults()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                new() { TestCode = DefaultTestCode, Buyer = DefaultBuyer, ProfitCentre = "ZZZPC", FpsYear = DefaultFpsYear, Price = 100m },
                new() { TestCode = DefaultTestCode, Buyer = DefaultBuyer, ProfitCentre = "AAAPC", FpsYear = DefaultFpsYear, Price = 200m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "profitcentre", Descending = false };

            // Act
            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode);

            // Assert
            Assert.Equal("AAAPC", result.Data.First().ProfitCentre);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_SortByTestCodeAscending_ReturnsOrderedResults()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                new() { TestCode = DefaultTestCode, Buyer = "B1", ProfitCentre = "PC001", FpsYear = DefaultFpsYear, Price = 100m },
                new() { TestCode = DefaultTestCode, Buyer = "B2", ProfitCentre = "PC002", FpsYear = DefaultFpsYear, Price = 200m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "testcode", Descending = false };

            // Act
            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_SortByPriceAscending_ReturnsOrderedResults()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                new() { TestCode = DefaultTestCode, Buyer = "B1", ProfitCentre = "PC001", FpsYear = DefaultFpsYear, Price = 999m },
                new() { TestCode = DefaultTestCode, Buyer = "B2", ProfitCentre = "PC002", FpsYear = DefaultFpsYear, Price = 1m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "price", Descending = false };

            // Act
            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode);

            // Assert
            Assert.Equal(1m, result.Data.First().Price);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_UnknownSortBy_DefaultsToSortByBuyerThenProfitCentre()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                new() { TestCode = DefaultTestCode, Buyer = "ZBUYER", ProfitCentre = "PC001", FpsYear = DefaultFpsYear, Price = 100m },
                new() { TestCode = DefaultTestCode, Buyer = "ABUYER", ProfitCentre = "PC002", FpsYear = DefaultFpsYear, Price = 200m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "unknown" };

            // Act
            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode);

            // Assert
            Assert.Equal("ABUYER", result.Data.First().Buyer);
        }

        #endregion
    }
}
