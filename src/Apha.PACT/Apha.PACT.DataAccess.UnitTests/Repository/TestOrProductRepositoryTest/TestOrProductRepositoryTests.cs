using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.TestOrProductRepositoryTest
{
    public class TestOrProductRepositoryTests
    {
        private record RepositoryContext(
            TestorProductRepository Repo,
            Mock<DbSet<TestorProduct>> TestorProductsDbSet,
            Mock<FpsDbContext> MockContext,
            IFpsRequestContext FpsRequestContext);

        private static RepositoryContext CreateRepositoryContext(
                int fpsYear = 2024,
                IEnumerable<TestorProduct> testorProducts = null!,
                bool setupForModification = false)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var testorProductsList = (testorProducts ?? Enumerable.Empty<TestorProduct>()).ToList();
            var testorProductsMockSet = RepositoryTestHelper.CreateMockDbSet(testorProductsList);

            if (setupForModification)
            {
                RepositoryTestHelper.SetupSaveChanges(mockContext);
                mockContext.Setup(m => m.TestorProducts.AddAsync(It.IsAny<TestorProduct>(), default))
                    .Returns((TestorProduct _, CancellationToken __) => new ValueTask<EntityEntry<TestorProduct>>());
            }

            mockContext.Setup(x => x.TestorProducts).Returns(testorProductsMockSet.Object);

            var repo = new TestorProductRepository(mockContext.Object, fpsRequestContext);
            return new RepositoryContext(repo, testorProductsMockSet, mockContext, fpsRequestContext);
        }

        private static TestorProductRepository CreateRepository(
            IEnumerable<TestorProduct> testorProducts,
            Mock<FpsDbContext> mockContext = null!,
            IFpsRequestContext fpsRequestContext = null!)
        {
            fpsRequestContext ??= Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(2024);

            mockContext ??= RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var testorProductsList = testorProducts.ToList();
            var mockSet = RepositoryTestHelper.CreateMockDbSet(testorProductsList);
            mockContext.Setup(x => x.TestorProducts).Returns(mockSet.Object);

            return new TestorProductRepository(mockContext.Object, fpsRequestContext);
        }

        #region GetPagedTestOrProductsAsync

        [Fact]
        public async Task GetPagedTestOrProductsAsync_ReturnsPagedData()
        {
            // Arrange
            var testorProducts = Enumerable.Range(1, 25).Select(i => new TestorProduct
            {
                ItemCode = $"TEST{i:D3}",
                ItemDescription = $"Test {i}",
                DefraUnitPrice = i * 10m,
                FpsYear = 2024
            });
            var repo = CreateRepository(testorProducts);
            var parameters = new PaginationParameters<string>
            {
                Page = 2,
                PageSize = 10
            };

            // Act
            var result = await repo.GetPagedTestOrProductsAsync(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Data.Count);
            Assert.Equal(25, result.PaginationData.TotalRecords);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        #endregion

        #region GetTestOrProductByIdAsync

        [Fact]
        public async Task GetTestOrProductByIdAsync_ExistingItemCode_ReturnsEntity()
        {
            // Arrange
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TEST001", ItemDescription = "Test One", DefraUnitPrice = 100m, FpsYear = 2024 }
            };
            var repo = CreateRepository(testorProducts);

            // Act
            var result = await repo.GetTestOrProductByIdAsync("TEST001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TEST001", result.ItemCode);
        }

        [Fact]
        public async Task GetTestOrProductByIdAsync_NonExistentItemCode_ReturnsNull()
        {
            // Arrange
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TEST001", ItemDescription = "Test One", DefraUnitPrice = 100m, FpsYear = 2024 }
            };
            var repo = CreateRepository(testorProducts);

            // Act
            var result = await repo.GetTestOrProductByIdAsync("MISSING");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTestOrProductByIdAsync_EmptyList_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestorProduct>());

            // Act
            var result = await repo.GetTestOrProductByIdAsync("TEST001");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region CreateTestOrProductAsync

        [Fact]
        public async Task CreateTestOrProductAsync_ValidEntity_ReturnsSavedEntity()
        {
            // Arrange
            var context = CreateRepositoryContext(fpsYear: 2024, setupForModification: true);
            var entity = new TestorProduct { ItemCode = "NEW001", DefraUnitPrice = 100m };

            // Act
            var result = await context.Repo.CreateTestOrProductAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NEW001", result.ItemCode);
            Assert.Equal(2024, result.FpsYear);
            context.MockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task CreateTestOrProductAsync_MultipleEntities_SavesAll()
        {
            // Arrange
            var context = CreateRepositoryContext(fpsYear: 2024, setupForModification: true);
            var entity1 = new TestorProduct { ItemCode = "NEW001", DefraUnitPrice = 100m };
            var entity2 = new TestorProduct { ItemCode = "NEW002", DefraUnitPrice = 200m };

            // Act
            var result1 = await context.Repo.CreateTestOrProductAsync(entity1);
            var result2 = await context.Repo.CreateTestOrProductAsync(entity2);

            // Assert
            Assert.Equal(2024, result1.FpsYear);
            Assert.Equal(2024, result2.FpsYear);
            context.MockContext.Verify(x => x.SaveChangesAsync(default), Times.Exactly(2));
        }

        #endregion

        #region UpdateTestOrProductAsync

        private static (FpsDbContext Context, TestorProductRepository Repo) CreateInMemoryContext(int fpsYear)
        {
            var options = new DbContextOptionsBuilder<FpsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);
            var context = new FpsDbContext(options, fpsRequestContext);
            var repo = new TestorProductRepository(context, fpsRequestContext);
            return (context, repo);
        }

        [Fact]
        public async Task UpdateTestOrProductAsync_ValidEntity_ReturnsUpdatedEntity()
        {
            // Arrange
            var (context, repo) = CreateInMemoryContext(2024);
            context.TestorProducts.Add(new TestorProduct { ItemCode = "TEST001", ItemDescription = "Original", DefraUnitPrice = 100m, FpsYear = 2024 });
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var entityToUpdate = new TestorProduct { ItemCode = "TEST001", ItemDescription = "Updated", DefraUnitPrice = 150m };

            // Act
            var result = await repo.UpdateTestOrProductAsync(entityToUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TEST001", result.ItemCode);
            Assert.Equal(2024, result.FpsYear);
        }

        [Fact]
        public async Task UpdateTestOrProductAsync_UpdatesFpsYear()
        {
            // Arrange
            var (context, repo) = CreateInMemoryContext(2025);
            context.TestorProducts.Add(new TestorProduct { ItemCode = "TEST001", DefraUnitPrice = 150m, FpsYear = 2025 });
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var entityToUpdate = new TestorProduct { ItemCode = "TEST001", DefraUnitPrice = 150m, FpsYear = 2023 };

            // Act
            var result = await repo.UpdateTestOrProductAsync(entityToUpdate);

            // Assert
            Assert.Equal(2025, result.FpsYear);
        }

        #endregion

        #region DeleteTestOrProductAsync

        [Fact]
        public async Task DeleteTestOrProductAsync_ExistingEntity_ReturnsTrue()
        {
            // Arrange
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2024 }
            };
            var context = CreateRepositoryContext(fpsYear: 2024, testorProducts: testorProducts, setupForModification: true);

            // Act
            var result = await context.Repo.DeleteTestOrProductAsync("TEST001");

            // Assert
            Assert.True(result);
            context.MockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task DeleteTestOrProductAsync_NonExistentEntity_ReturnsFalse()
        {
            // Arrange
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2024 }
            };
            var context = CreateRepositoryContext(fpsYear: 2024, testorProducts: testorProducts);

            // Act
            var result = await context.Repo.DeleteTestOrProductAsync("MISSING");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteTestOrProductAsync_WrongFpsYear_ReturnsFalse()
        {
            // Arrange
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2023 }
            };
            var context = CreateRepositoryContext(fpsYear: 2024, testorProducts: testorProducts);

            // Act
            var result = await context.Repo.DeleteTestOrProductAsync("TEST001");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteTestOrProductAsync_MatchingFpsYear_ReturnsTrue()
        {
            // Arrange
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2024 }
            };
            var context = CreateRepositoryContext(fpsYear: 2024, testorProducts: testorProducts, setupForModification: true);

            // Act
            var result = await context.Repo.DeleteTestOrProductAsync("TEST001");

            // Assert
            Assert.True(result);
        }

        #endregion

        #region GetAllTestorProductsAsync

        [Fact]
        public async Task GetAllTestorProductsAsync_WithData_ReturnsAllOrderedByItemCode()
        {
            var products = new List<TestorProduct>
            {
                new() { ItemCode = "Z001", DefraUnitPrice = 10m, FpsYear = 2024 },
                new() { ItemCode = "A001", DefraUnitPrice = 20m, FpsYear = 2024 }
            };
            var repo = CreateRepository(products);

            var result = (await repo.GetAllTestorProductsAsync()).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("A001", result[0].ItemCode);
            Assert.Equal("Z001", result[1].ItemCode);
        }

        [Fact]
        public async Task GetAllTestorProductsAsync_WithNoData_ReturnsEmptyList()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetAllTestorProductsAsync();

            Assert.Empty(result);
        }

        #endregion

        #region GetPagedTestOrProductsAsync — filtering

        [Fact]
        public async Task GetPagedTestOrProductsAsync_WithItemCodeFilter_ReturnsFilteredResult()
        {
            var products = new List<TestorProduct>
            {
                new() { ItemCode = "ALPHA", ItemDescription = "Alpha", DefraUnitPrice = 10m, FpsYear = 2024 },
                new() { ItemCode = "BETA",  ItemDescription = "Beta",  DefraUnitPrice = 20m, FpsYear = 2024 }
            };
            var repo = CreateRepository(products);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = """{"ItemCode":"ALPHA"}"""
            };

            var result = await repo.GetPagedTestOrProductsAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("ALPHA", result.Data.First().ItemCode);
        }

        #endregion

        #region GetDescriptionsByCodesAsync

        [Fact]
        public async Task GetDescriptionsByCodesAsync_WithMatchingCodes_ReturnsDictionary()
        {
            var products = new List<TestorProduct>
            {
                new() { ItemCode = "T001", ItemDescription = "Desc One", DefraUnitPrice = 10m, FpsYear = 2024 },
                new() { ItemCode = "T002", ItemDescription = "Desc Two", DefraUnitPrice = 20m, FpsYear = 2024 }
            };
            var repo = CreateRepository(products);

            var result = await repo.GetDescriptionsByCodesAsync(["T001", "T002"]);

            Assert.Equal(2, result.Count);
            Assert.Equal("Desc One", result["T001"]);
            Assert.Equal("Desc Two", result["T002"]);
        }

        [Fact]
        public async Task GetDescriptionsByCodesAsync_WithNoMatchingCodes_ReturnsEmptyDictionary()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetDescriptionsByCodesAsync(["NONE"]);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDescriptionsByCodesAsync_WithNullDescription_ReturnsNullValue()
        {
            var products = new List<TestorProduct>
            {
                new() { ItemCode = "T001", ItemDescription = null, DefraUnitPrice = 10m, FpsYear = 2024 }
            };
            var repo = CreateRepository(products);

            var result = await repo.GetDescriptionsByCodesAsync(["T001"]);

            Assert.True(result.ContainsKey("T001"));
            Assert.Null(result["T001"]);
        }

        #endregion

        #region GetUnitPricesByCodesAsync

        [Fact]
        public async Task GetUnitPricesByCodesAsync_WithMatchingCodes_ReturnsDictionary()
        {
            var products = new List<TestorProduct>
            {
                new() { ItemCode = "T001", UnitPriceVla = 10.50m, FpsYear = 2024 },
                new() { ItemCode = "T002", UnitPriceVla = 20.75m, FpsYear = 2024 }
            };
            var repo = CreateRepository(products);

            var result = await repo.GetUnitPricesByCodesAsync(["T001", "T002"]);

            Assert.Equal(2, result.Count);
            Assert.Equal(10.50m, result["T001"]);
            Assert.Equal(20.75m, result["T002"]);
        }

        [Fact]
        public async Task GetUnitPricesByCodesAsync_WithNoMatchingCodes_ReturnsEmptyDictionary()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetUnitPricesByCodesAsync(["NONE"]);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUnitPricesByCodesAsync_WithNullUnitPrice_ReturnsNullValue()
        {
            var products = new List<TestorProduct>
            {
                new() { ItemCode = "T001", UnitPriceVla = null, FpsYear = 2024 }
            };
            var repo = CreateRepository(products);

            var result = await repo.GetUnitPricesByCodesAsync(["T001"]);

            Assert.True(result.ContainsKey("T001"));
            Assert.Null(result["T001"]);
        }

        #endregion

        #region UpdateUnitPriceByCodeAsync

        [Fact]
        public async Task UpdateUnitPriceByCodeAsync_MatchingRow_UpdatesPriceAndReturnsTrue()
        {
            // Arrange
            var (context, repo) = CreateInMemoryContext(2024);
            context.TestorProducts.Add(new TestorProduct { ItemCode = "T001", UnitPriceVla = 10m, FpsYear = 2024 });
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Act
            var result = await repo.UpdateUnitPriceByCodeAsync("T001", 55.25m);

            // Assert
            Assert.True(result);
            context.ChangeTracker.Clear();
            var row = await context.TestorProducts.FirstAsync(t => t.ItemCode == "T001");
            Assert.Equal(55.25m, row.UnitPriceVla);
        }

        [Fact]
        public async Task UpdateUnitPriceByCodeAsync_NoMatchingRows_ReturnsFalse()
        {
            // Arrange
            var (context, repo) = CreateInMemoryContext(2024);
            context.TestorProducts.Add(new TestorProduct { ItemCode = "T001", UnitPriceVla = 10m, FpsYear = 2024 });
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Act
            var result = await repo.UpdateUnitPriceByCodeAsync("MISSING", 55.25m);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task UpdateUnitPriceByCodeAsync_WrongFpsYear_ReturnsFalseAndLeavesRowsUnchanged()
        {
            // Arrange
            var (context, repo) = CreateInMemoryContext(2024);
            context.TestorProducts.Add(new TestorProduct { ItemCode = "T001", UnitPriceVla = 10m, FpsYear = 2023 });
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Act
            var result = await repo.UpdateUnitPriceByCodeAsync("T001", 55.25m);

            // Assert
            Assert.False(result);
            context.ChangeTracker.Clear();
            var row = await context.TestorProducts.IgnoreQueryFilters().FirstAsync(t => t.ItemCode == "T001");
            Assert.Equal(10m, row.UnitPriceVla);
        }

        [Fact]
        public async Task UpdateUnitPriceByCodeAsync_NullUnitPrice_SetsNullAndReturnsTrue()
        {
            // Arrange
            var (context, repo) = CreateInMemoryContext(2024);
            context.TestorProducts.Add(new TestorProduct { ItemCode = "T001", UnitPriceVla = 10m, FpsYear = 2024 });
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Act
            var result = await repo.UpdateUnitPriceByCodeAsync("T001", null);

            // Assert
            Assert.True(result);
            context.ChangeTracker.Clear();
            var row = await context.TestorProducts.FirstAsync(t => t.ItemCode == "T001");
            Assert.Null(row.UnitPriceVla);
        }

        #endregion

        #region GetOwnersAsync

        [Fact]
        public async Task GetOwnersAsync_ReturnsDistinctOwners()
        {
            // Arrange
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TEST001", Owner = "AB", DefraUnitPrice = 100m, FpsYear = 2024 },
                new() { ItemCode = "TEST002", Owner = "CD", DefraUnitPrice = 100m, FpsYear = 2024 },
                new() { ItemCode = "TEST003", Owner = "AB", DefraUnitPrice = 100m, FpsYear = 2024 },
                new() { ItemCode = "TEST004", Owner = null, DefraUnitPrice = 100m, FpsYear = 2024 }
            };
            var repo = CreateRepository(testorProducts);

            // Act
            var result = await repo.GetOwnersAsync();

            // Assert
            var ownersList = result.ToList();
            Assert.Equal(2, ownersList.Count);
            Assert.Contains("AB", ownersList);
            Assert.Contains("CD", ownersList);
        }

        [Fact]
        public async Task GetOwnersAsync_EmptyList_ReturnsEmpty()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestorProduct>());

            // Act
            var result = await repo.GetOwnersAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
