using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;
using Xunit;

namespace Apha.FPS.DataAccess.UnitTests.Repository.TestRCCostRepositoryTest
{
    public class TestRCCostRepositoryTests
    {
        private const int DefaultFpsYear = 2025;
        private const string DefaultUserEmail = "test@example.com";
        private const string DefaultTestCode = "TEST001";
        private const string DefaultProfitCentre = "PC001";

        private static Mock<IFpsRequestContext> CreateMockRequestContext(int year = DefaultFpsYear)
        {
            var mock = new Mock<IFpsRequestContext>();
            mock.Setup(x => x.FpsYear).Returns(year);
            mock.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);
            return mock;
        }

        private static TestRCCostRepository CreateRepository(
            IEnumerable<TestRCCost>? testRCCosts = null,
            int fpsYear = DefaultFpsYear)
        {
            var mockRequestContext = CreateMockRequestContext(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            var testRCCostSet = RepositoryTestHelper.CreateMockDbSet(
                testRCCosts ?? Enumerable.Empty<TestRCCost>());
            mockContext.Setup(x => x.TestRCCosts).Returns(testRCCostSet.Object);

            return new TestRCCostRepository(mockContext.Object, mockRequestContext.Object);
        }

        #region GetPagedByTestCodeAsync

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithMatchingRecords_ReturnsPagedData()
        {
            // Arrange
            var entities = new List<TestRCCost>
            {
                CreateEntity(DefaultTestCode, "PC001"),
                CreateEntity(DefaultTestCode, "PC002")
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
            var entities = new List<TestRCCost> { CreateEntity(DefaultTestCode, "PC001") };
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
                .Select(i => new TestRCCost
                {
                    TestCode = DefaultTestCode,
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
            var entities = new List<TestRCCost>
            {
                CreateEntity(DefaultTestCode, "PC001"),
                CreateEntity(DefaultTestCode, "PC002")
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
            var entities = new List<TestRCCost> { CreateEntity(DefaultTestCode, DefaultProfitCentre) };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.GetByTestCodeAsync("NOTEXIST");

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetByKeyAsync

        [Fact]
        public async Task GetByKeyAsync_WithExistingCompositeKey_ReturnsRecord()
        {
            // Arrange
            var entities = new List<TestRCCost> { CreateEntity(DefaultTestCode, DefaultProfitCentre) };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.GetByKeyAsync(DefaultTestCode, DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DefaultProfitCentre, result!.ProfitCentre);
        }

        [Fact]
        public async Task GetByKeyAsync_WithNonExistingKey_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestRCCost>());

            // Act
            var result = await repo.GetByKeyAsync("NOTEXIST", "PC999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByKeyAsync_WithWrongProfitCentre_ReturnsNull()
        {
            // Arrange
            var entities = new List<TestRCCost> { CreateEntity(DefaultTestCode, "PC001") };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.GetByKeyAsync(DefaultTestCode, "WRONG_PC");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_WithExistingRecord_ReturnsTrue()
        {
            // Arrange
            var entities = new List<TestRCCost> { CreateEntity(DefaultTestCode, DefaultProfitCentre) };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.ExistsAsync(DefaultTestCode, DefaultProfitCentre);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingRecord_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestRCCost>());

            // Act
            var result = await repo.ExistsAsync("NOTEXIST", "PC999");

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetByTestCodeAsync — Multiple profit centres sorted

        [Fact]
        public async Task GetByTestCodeAsync_MultipleRecords_ReturnsSortedByProfitCentre()
        {
            // Arrange
            var entities = new List<TestRCCost>
            {
                new() { TestCode = DefaultTestCode, ProfitCentre = "PC003", FpsYear = DefaultFpsYear, Price = 300m },
                new() { TestCode = DefaultTestCode, ProfitCentre = "PC001", FpsYear = DefaultFpsYear, Price = 100m },
                new() { TestCode = DefaultTestCode, ProfitCentre = "PC002", FpsYear = DefaultFpsYear, Price = 200m }
            };
            var repo = CreateRepository(entities);

            // Act
            var result = (await repo.GetByTestCodeAsync(DefaultTestCode)).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("PC001", result.First().ProfitCentre);
            Assert.Equal("PC003", result.Last().ProfitCentre);
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_ValidEntity_ReturnsAddedEntity()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestRCCost>());
            var entity = CreateEntity(DefaultTestCode, DefaultProfitCentre);

            // Act
            var result = await repo.AddAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DefaultTestCode, result.TestCode);
            Assert.Equal(DefaultProfitCentre, result.ProfitCentre);
        }

        [Fact]
        public async Task AddAsync_ValidEntity_CallsDbSetAdd()
        {
            // Arrange
            var entity = CreateEntity(DefaultTestCode, DefaultProfitCentre);
            var mockRequestContext = CreateMockRequestContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<TestRCCost>());
            mockContext.Setup(x => x.TestRCCosts).Returns(mockSet.Object);
            var repo = new TestRCCostRepository(mockContext.Object, mockRequestContext.Object);

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
            var existing = CreateEntity(DefaultTestCode, DefaultProfitCentre);
            var repo = CreateRepository(new List<TestRCCost> { existing });

            var updated = new TestRCCost
            {
                TestCode = DefaultTestCode,
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
            var repo = CreateRepository(Enumerable.Empty<TestRCCost>());
            var entity = new TestRCCost
            {
                TestCode = "NOTEXIST",
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
            var entities = new List<TestRCCost> { CreateEntity(DefaultTestCode, DefaultProfitCentre) };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.DeleteAsync(DefaultTestCode, DefaultProfitCentre);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingRecord_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestRCCost>());

            // Act
            var result = await repo.DeleteAsync("NOTEXIST", "PC999");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ExistingRecord_CallsDbSetRemove()
        {
            // Arrange
            var entity = CreateEntity(DefaultTestCode, DefaultProfitCentre);
            var mockRequestContext = CreateMockRequestContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(new List<TestRCCost> { entity });
            mockContext.Setup(x => x.TestRCCosts).Returns(mockSet.Object);
            var repo = new TestRCCostRepository(mockContext.Object, mockRequestContext.Object);

            // Act
            await repo.DeleteAsync(DefaultTestCode, DefaultProfitCentre);

            // Assert
            mockSet.Verify(s => s.Remove(It.IsAny<TestRCCost>()), Moq.Times.Once);
        }

        #endregion

        #region Helper Methods

        private static TestRCCost CreateEntity(string testCode, string profitCentre) =>
            new()
            {
                TestCode = testCode,
                ProfitCentre = profitCentre,
                FpsYear = DefaultFpsYear,
                Price = 150m
            };

        #endregion

        #region GetPagedByTestCodeAsync - Filters

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithJsonTestCodeFilter_FiltersResults()
        {
            // Arrange
            var entities = new List<TestRCCost>
            {
                new() { TestCode = DefaultTestCode, ProfitCentre = "PC001", FpsYear = DefaultFpsYear, Price = 100m },
                new() { TestCode = DefaultTestCode, ProfitCentre = "PC002", FpsYear = DefaultFpsYear, Price = 200m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ProfitCentre\":\"PC001\"}"
            };

            // Act
            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("PC001", result.Data.First().ProfitCentre);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithNullFilter_ReturnsAll()
        {
            // Arrange
            var entities = new List<TestRCCost>
            {
                new() { TestCode = DefaultTestCode, ProfitCentre = "PC001", FpsYear = DefaultFpsYear, Price = 100m },
                new() { TestCode = DefaultTestCode, ProfitCentre = "PC002", FpsYear = DefaultFpsYear, Price = 200m }
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
        public async Task GetPagedByTestCodeAsync_SortByProfitCentreAscending_ReturnsOrderedResults()
        {
            // Arrange
            var entities = new List<TestRCCost>
            {
                new() { TestCode = DefaultTestCode, ProfitCentre = "ZZZPC", FpsYear = DefaultFpsYear, Price = 100m },
                new() { TestCode = DefaultTestCode, ProfitCentre = "AAAPC", FpsYear = DefaultFpsYear, Price = 200m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "profitcentre", Descending = false };

            // Act
            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode);

            // Assert
            Assert.Equal("AAAPC", result.Data.First().ProfitCentre);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_SortByProfitCentreDescending_ReturnsOrderedResults()
        {
            // Arrange
            var entities = new List<TestRCCost>
            {
                new() { TestCode = DefaultTestCode, ProfitCentre = "AAAPC", FpsYear = DefaultFpsYear, Price = 100m },
                new() { TestCode = DefaultTestCode, ProfitCentre = "ZZZPC", FpsYear = DefaultFpsYear, Price = 200m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "profitcentre", Descending = true };

            // Act
            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode);

            // Assert
            Assert.Equal("ZZZPC", result.Data.First().ProfitCentre);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_SortByTestCodeAscending_ReturnsOrderedResults()
        {
            // Arrange
            var entities = new List<TestRCCost>
            {
                new() { TestCode = DefaultTestCode, ProfitCentre = "PC002", FpsYear = DefaultFpsYear, Price = 200m },
                new() { TestCode = DefaultTestCode, ProfitCentre = "PC001", FpsYear = DefaultFpsYear, Price = 100m }
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
            var entities = new List<TestRCCost>
            {
                new() { TestCode = DefaultTestCode, ProfitCentre = "PC001", FpsYear = DefaultFpsYear, Price = 999m },
                new() { TestCode = DefaultTestCode, ProfitCentre = "PC002", FpsYear = DefaultFpsYear, Price = 1m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "price", Descending = false };

            // Act
            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode);

            // Assert
            Assert.Equal("PC002", result.Data.First().ProfitCentre);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_UnknownSortBy_DefaultsToSortByProfitCentre()
        {
            // Arrange
            var entities = new List<TestRCCost>
            {
                new() { TestCode = DefaultTestCode, ProfitCentre = "ZZZPC", FpsYear = DefaultFpsYear, Price = 100m },
                new() { TestCode = DefaultTestCode, ProfitCentre = "AAAPC", FpsYear = DefaultFpsYear, Price = 200m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "unknown" };

            // Act
            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode);

            // Assert
            Assert.Equal("AAAPC", result.Data.First().ProfitCentre);
        }

        #endregion
    }
}
