using Apha.Common.Helpers.Repository;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess.Repositories;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.Costbook.DataAccess.UnitTests.Repository.FpsAccountCategoryRepositoryTest
{
    public class FpsAccountCategoryRepositoryTests
    {
        private static FpsAccountCategoryRepository CreateRepository(IEnumerable<FpsAccountCategory> items)
        {
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);

            var mockSet = RepositoryTestHelper.CreateMockDbSet(items);
            mockContext.Setup(x => x.Set<FpsAccountCategory>()).Returns(mockSet.Object);
            mockContext.Setup(x => x.FpsAccountCategories).Returns(mockSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new FpsAccountCategoryRepository(mockContext.Object);
        }

        #region GetAllForMaintenanceAsync Tests

        [Fact]
        public async Task GetAllForMaintenanceAsync_ReturnsOnlyProjectSpecificTrue_AndOrdered()
        {
            // Arrange
            var categories = new List<FpsAccountCategory>
            {
                new FpsAccountCategory { AccShortName = "B02", ProjectSpecific = -1 },
                new FpsAccountCategory { AccShortName = "A01", ProjectSpecific = -1 },
                new FpsAccountCategory { AccShortName = "C03", ProjectSpecific = 0 } // should be excluded
            };

            var repo = CreateRepository(categories);

            // Act
            var result = await repo.GetAllForMaintenanceAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            // should be ordered by AccShortName ascending
            Assert.Equal("A01", result[0].AccShortName);
            Assert.Equal("B02", result[1].AccShortName);
        }

        [Fact]
        public async Task GetAllForMaintenanceAsync_ReturnsEmptyWhenNoneMatch()
        {
            // Arrange
            var categories = new List<FpsAccountCategory>
            {
                new FpsAccountCategory { AccShortName = "X01", ProjectSpecific = 0 },
                new FpsAccountCategory { AccShortName = "Y02", ProjectSpecific = null }
            };

            var repo = CreateRepository(categories);

            // Act
            var result = await repo.GetAllForMaintenanceAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region GetPaginatedAsync Tests

        [Fact]
        public async Task GetPaginatedAsync_NoFilter_ReturnsPagedData()
        {
            // Arrange
            var items = Enumerable.Range(1, 12)
                .Select(i => new FpsAccountCategory { AccShortName = $"ACC{i:000}", ProjectSpecific = -1 })
                .ToList();

            var repo = CreateRepository(items);

            var query = new Apha.Costbook.Core.Pagination.PaginationParameters<string>
            {
                Page = 2,
                PageSize = 5,
                Filter = null,
                SortBy = null,
                Descending = false
            };

            // Act
            var paged = await repo.GetPaginatedAsync(query);

            // Assert
            Assert.NotNull(paged);
            Assert.Equal(5, paged.Data.Count());
            Assert.Equal(12, paged.PaginationData.TotalRecords);
            Assert.Equal(3, paged.PaginationData.TotalPages);
            Assert.Equal(2, paged.PaginationData.PageNumber);
            Assert.Equal(5, paged.PaginationData.PageSize);
        }

        [Fact]
        public async Task GetPaginatedAsync_SortByAccShortName_Descending()
        {
            // Arrange
            var items = new List<FpsAccountCategory>
            {
                new FpsAccountCategory { AccShortName = "A01", ProjectSpecific = -1 },
                new FpsAccountCategory { AccShortName = "B02", ProjectSpecific = -1 },
                new FpsAccountCategory { AccShortName = "C03", ProjectSpecific = -1 }
            };

            var repo = CreateRepository(items);

            var query = new Apha.Costbook.Core.Pagination.PaginationParameters<string>
            {
                Page = 1,
                PageSize = 2,
                Filter = null,
                SortBy = "accshortname",
                Descending = true
            };

            // Act
            var paged = await repo.GetPaginatedAsync(query);

            // Assert
            Assert.NotNull(paged);
            var data = paged.Data.ToList();
            Assert.Equal(2, data.Count);
            Assert.Equal("C03", data[0].AccShortName);
            Assert.Equal("B02", data[1].AccShortName);
        }

        #endregion

        #region GetByAccShortNameAsync & ExistsAsync Tests

        [Fact]
        public async Task GetByAccShortNameAsync_Existing_ReturnsEntity()
        {
            // Arrange
            var items = new List<FpsAccountCategory>
            {
                new FpsAccountCategory { AccShortName = "ACC001", ProjectSpecific = -1, AccountDescription = "Test" }
            };
            var repo = CreateRepository(items);

            // Act
            var result = await repo.GetByAccShortNameAsync("ACC001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("ACC001", result!.AccShortName);
            Assert.Equal("Test", result.AccountDescription);
        }

        [Fact]
        public async Task GetByAccShortNameAsync_NonExisting_ReturnsNull()
        {
            var repo = CreateRepository(new List<FpsAccountCategory>());

            var result = await repo.GetByAccShortNameAsync("MISSING");
            Assert.Null(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsTrueWhenExists()
        {
            var items = new List<FpsAccountCategory>
            {
                new FpsAccountCategory { AccShortName = "EX1", ProjectSpecific = -1 }
            };
            var repo = CreateRepository(items);

            var exists = await repo.ExistsAsync("EX1");
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalseWhenMissing()
        {
            var repo = CreateRepository(new List<FpsAccountCategory>());

            var exists = await repo.ExistsAsync("MISSING");
            Assert.False(exists);
        }

        #endregion

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_AddsAndSaves()
        {
            // Arrange
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(new List<FpsAccountCategory>());
            mockContext.Setup(x => x.Set<FpsAccountCategory>()).Returns(mockSet.Object);
            mockContext.Setup(x => x.FpsAccountCategories).Returns(mockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new FpsAccountCategoryRepository(mockContext.Object);

            var newItem = new FpsAccountCategory
            {
                AccShortName = "NEW01",
                AccountDescription = "New account",
                ProjectSpecific = -1
            };

            // Act
            var result = await repo.AddAsync(newItem);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NEW01", result.AccShortName);
            Assert.Equal("New account", result.AccountDescription);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_Existing_UpdatesFieldsAndSaves()
        {
            // Arrange
            var existing = new FpsAccountCategory
            {
                AccShortName = "UPD01",
                AccountDescription = "Old",
                AccountType = "A",
                ConstituentAccountCodes = "X",
                Csg7Group = "G1",
                ProjectSpecific = -1,
                RcSpecific = 0
            };

            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(new List<FpsAccountCategory> { existing });
            mockContext.Setup(x => x.Set<FpsAccountCategory>()).Returns(mockSet.Object);
            mockContext.Setup(x => x.FpsAccountCategories).Returns(mockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new FpsAccountCategoryRepository(mockContext.Object);

            var updated = new FpsAccountCategory
            {
                AccShortName = "UPD01",
                AccountDescription = "NewDesc",
                AccountType = "B",
                ConstituentAccountCodes = "Y",
                Csg7Group = "G2",
                ProjectSpecific = -1,
                RcSpecific = 1
            };

            // Act
            var result = await repo.UpdateAsync(updated);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NewDesc", result.AccountDescription);
            Assert.Equal("B", result.AccountType);
            Assert.Equal("Y", result.ConstituentAccountCodes);
            Assert.Equal("G2", result.Csg7Group);
            Assert.Equal(1, result.RcSpecific);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_NonExisting_ThrowsKeyNotFoundException()
        {
            // Arrange
            var repo = CreateRepository(new List<FpsAccountCategory>());
            var entity = new FpsAccountCategory { AccShortName = "NOTEXIST" };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.UpdateAsync(entity));
        }

        #endregion

        
    }
}
