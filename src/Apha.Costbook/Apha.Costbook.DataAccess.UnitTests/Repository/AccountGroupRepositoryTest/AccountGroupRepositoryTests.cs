using Apha.Common.Helpers.Repository;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess.Repositories;
using Moq;

namespace Apha.Costbook.DataAccess.UnitTests.Repository.AccountGroupRepositoryTest
{
    public class AccountGroupRepositoryTests
    {
        // ── Factory helper ────────────────────────────────────────────────────

        private static AccountGroupRepository CreateRepository(IEnumerable<AccountGroup> accountGroups)
        {
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);

            var accountGroupsMockSet = RepositoryTestHelper.CreateMockDbSet(accountGroups);
            mockContext.Setup(x => x.Set<AccountGroup>()).Returns(accountGroupsMockSet.Object);
            mockContext.Setup(x => x.AccountGroups).Returns(accountGroupsMockSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new AccountGroupRepository(mockContext.Object);
        }

        // ── GetAllAsync ───────────────────────────────────────────────────────

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ReturnsAllAccountGroups()
        {
            // Arrange
            var accountGroups = new List<AccountGroup>
            {
                new AccountGroup { Csg7group = "CSG001", Useinflation = true },
                new AccountGroup { Csg7group = "CSG002", Useinflation = false },
                new AccountGroup { Csg7group = "CSG003", Useinflation = true }
            };
            var repo = CreateRepository(accountGroups);

            // Act
            var result = await repo.GetAllAccountGroupAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoAccountGroups()
        {
            // Arrange
            var repo = CreateRepository(new List<AccountGroup>());

            // Act
            var result = await repo.GetAllAccountGroupAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsCorrectProperties()
        {
            // Arrange
            var accountGroups = new List<AccountGroup>
            {
                new AccountGroup { Csg7group = "CSG001", Useinflation = true }
            };
            var repo = CreateRepository(accountGroups);

            // Act
            var result = await repo.GetAllAccountGroupAsync();

            // Assert
            Assert.Single(result);
            var item = result[0];
            Assert.Equal("CSG001", item.Csg7group);
            Assert.True(item.Useinflation);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsGroupsWithNullUseinflation()
        {
            // Arrange
            var accountGroups = new List<AccountGroup>
            {
                new AccountGroup { Csg7group = "CSG001", Useinflation = true },
                new AccountGroup { Csg7group = "CSG002", Useinflation = null }
            };
            var repo = CreateRepository(accountGroups);

            // Act
            var result = await repo.GetAllAccountGroupAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, g => g.Useinflation == null);
        }

        #endregion

        // ── GetByCsg7GroupAsync ───────────────────────────────────────────────

        #region GetByCsg7GroupAsync Tests

        [Fact]
        public async Task GetByCsg7GroupAsync_ExistingKey_ReturnsAccountGroup()
        {
            // Arrange
            var accountGroups = new List<AccountGroup>
            {
                new AccountGroup { Csg7group = "CSG001", Useinflation = true },
                new AccountGroup { Csg7group = "CSG002", Useinflation = false }
            };
            var repo = CreateRepository(accountGroups);

            // Act
            var result = await repo.GetByCsg7GroupAsync("CSG001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CSG001", result!.Csg7group);
            Assert.True(result.Useinflation);
        }

        [Fact]
        public async Task GetByCsg7GroupAsync_NonExistentKey_ReturnsNull()
        {
            // Arrange
            var accountGroups = new List<AccountGroup>
            {
                new AccountGroup { Csg7group = "CSG001", Useinflation = true }
            };
            var repo = CreateRepository(accountGroups);

            // Act
            var result = await repo.GetByCsg7GroupAsync("NOTEXIST");

            // Assert
            Assert.Null(result);
        }

        #endregion

        // ── ExistsAsync ───────────────────────────────────────────────────────

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_ExistingKey_ReturnsTrue()
        {
            // Arrange
            var accountGroups = new List<AccountGroup>
            {
                new AccountGroup { Csg7group = "CSG001", Useinflation = true }
            };
            var repo = CreateRepository(accountGroups);

            // Act
            var result = await repo.ExistsAsync("CSG001");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_NonExistentKey_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository(new List<AccountGroup>());

            // Act
            var result = await repo.ExistsAsync("NOTEXIST");

            // Assert
            Assert.False(result);
        }

        #endregion

        // ── AddAsync ──────────────────────────────────────────────────────────

        #region AddAsync Tests

        [Fact]
        public async Task AddAsync_ValidEntity_AddsAndReturnsAccountGroup()
        {
            // Arrange
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);
            var accountGroupsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<AccountGroup>());
            mockContext.Setup(x => x.AccountGroups).Returns(accountGroupsMockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);
            var repo = new AccountGroupRepository(mockContext.Object);

            var newGroup = new AccountGroup { Csg7group = "CSG003", Useinflation = true };

            // Act
            var result = await repo.AddAccountGroupAsync(newGroup);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CSG003", result.Csg7group);
            Assert.True(result.Useinflation);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ExistingEntity_UpdatesUseinflationField()
        {
            // Arrange
            var existing = new AccountGroup { Csg7group = "CSG001", Useinflation = true };
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);
            var accountGroupsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<AccountGroup> { existing });
            mockContext.Setup(x => x.AccountGroups).Returns(accountGroupsMockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);
            var repo = new AccountGroupRepository(mockContext.Object);

            var updatedEntity = new AccountGroup { Csg7group = "CSG001", Useinflation = false };

            // Act
            var result = await repo.UpdateAccountGroupAsync(updatedEntity);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Useinflation);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_NonExistentEntity_ThrowsKeyNotFoundException()
        {
            // Arrange
            var repo = CreateRepository(new List<AccountGroup>());
            var entity = new AccountGroup { Csg7group = "NOTEXIST", Useinflation = true };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.UpdateAccountGroupAsync(entity));
        }

        #endregion
    }
}
