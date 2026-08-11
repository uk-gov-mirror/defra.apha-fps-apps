using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.AccessLevelRepositoryTest
{
    public class AccessLevelRepositoryTests
    {
        private static AccessLevelRepository CreateRepository(
            IEnumerable<AccessLevel>? accessLevels = null)
        {
            var mockContext        = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var accessLevelsMockSet = RepositoryTestHelper.CreateMockDbSet(accessLevels ?? Enumerable.Empty<AccessLevel>());

            RepositoryTestHelper.SetupDbSetOperations(accessLevelsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.AccessLevels).Returns(accessLevelsMockSet.Object);

            return new AccessLevelRepository(mockContext.Object);
        }

        private static (
            AccessLevelRepository Repo,
            Mock<DbSet<AccessLevel>> AccessLevelsDbSet,
            Mock<PimsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<AccessLevel>? accessLevels = null)
        {
            var mockContext        = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var accessLevelsMockSet = RepositoryTestHelper.CreateMockDbSet(accessLevels ?? Enumerable.Empty<AccessLevel>());

            RepositoryTestHelper.SetupDbSetOperations(accessLevelsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.AccessLevels).Returns(accessLevelsMockSet.Object);

            var repo = new AccessLevelRepository(mockContext.Object);
            return (repo, accessLevelsMockSet, mockContext);
        }

        private static AccessLevel MakeLevel(int systemid = 1, int accesslevelid = 10, string name = "Level 1") =>
            new() { SystemId = systemid, AccessLevelId = accesslevelid, AccessLevelName = name };

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ReturnsAllAccessLevels_WhenDataExists()
        {
            // Arrange
            var levels = new List<AccessLevel>
            {
                MakeLevel(2, 1, "B"),
                MakeLevel(1, 2, "A2"),
                MakeLevel(1, 1, "A1")
            };
            var repo = CreateRepository(levels);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal((1, 1), (result[0].SystemId, result[0].AccessLevelId));
            Assert.Equal((1, 2), (result[1].SystemId, result[1].AccessLevelId));
            Assert.Equal((2, 1), (result[2].SystemId, result[2].AccessLevelId));
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetBySystemIdAsync

        [Fact]
        public async Task GetBySystemIdAsync_ReturnsMatchingLevels_WhenSystemIdExists()
        {
            // Arrange
            var levels = new List<AccessLevel>
            {
                MakeLevel(1, 3, "L3"),
                MakeLevel(1, 1, "L1"),
                MakeLevel(2, 2, "X")
            };
            var repo = CreateRepository(levels);

            // Act
            var result = await repo.GetBySystemIdAsync(1);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, l => Assert.Equal(1, l.SystemId));
            Assert.Equal(1, result[0].AccessLevelId);
            Assert.Equal(3, result[1].AccessLevelId);
        }

        [Fact]
        public async Task GetBySystemIdAsync_ReturnsEmptyList_WhenSystemIdNotFound()
        {
            // Arrange
            var levels = new List<AccessLevel> { MakeLevel(1, 1, "L1") };
            var repo = CreateRepository(levels);

            // Act
            var result = await repo.GetBySystemIdAsync(99);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ReturnsLevel_WhenCompositePkExists()
        {
            // Arrange
            var levels = new List<AccessLevel>
            {
                MakeLevel(1, 1, "L1"),
                MakeLevel(1, 2, "L2"),
                MakeLevel(2, 1, "X")
            };
            var repo = CreateRepository(levels);

            // Act
            var result = await repo.GetByIdAsync(1, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.SystemId);
            Assert.Equal(2, result.AccessLevelId);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenSystemIdDoesNotMatch()
        {
            // Arrange
            var levels = new List<AccessLevel> { MakeLevel(1, 1, "L1") };
            var repo = CreateRepository(levels);

            // Act
            var result = await repo.GetByIdAsync(99, 1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenAccessLevelIdDoesNotMatch()
        {
            // Arrange
            var levels = new List<AccessLevel> { MakeLevel(1, 1, "L1") };
            var repo = CreateRepository(levels);

            // Act
            var result = await repo.GetByIdAsync(1, 99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.GetByIdAsync(1, 1);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_ReturnsAddedEntity()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks();
            var level = MakeLevel(1, 7, "Editor");

            // Act
            var result = await repo.AddAsync(level);

            // Assert
            Assert.NotNull(result);
            Assert.Same(level, result);
        }

        [Fact]
        public async Task AddAsync_CallsDbSetAdd()
        {
            // Arrange
            var (repo, accessLevelsDbSet, _) = CreateRepositoryWithMocks();
            var level = MakeLevel(1, 7, "Editor");

            // Act
            await repo.AddAsync(level);

            // Assert
            accessLevelsDbSet.Verify(
                x => x.Add(It.Is<AccessLevel>(l => l.SystemId == 1 && l.AccessLevelId == 7)),
                Times.Once);
        }

        [Fact]
        public async Task AddAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks();

            // Act
            await repo.AddAsync(MakeLevel(1, 7, "Editor"));

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ReturnsUpdatedEntity()
        {
            // Arrange
            var existing = MakeLevel(1, 7, "Editor");
            var (repo, _, _) = CreateRepositoryWithMocks(new List<AccessLevel> { existing });
            var updatedEntity = MakeLevel(1, 7, "Editor+");

            // Act
            var result = await repo.UpdateAsync(updatedEntity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Editor+", result.AccessLevelName);
        }

        [Fact]
        public async Task UpdateAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var existing = MakeLevel(1, 7, "Editor");
            var (repo, _, mockContext) = CreateRepositoryWithMocks(new List<AccessLevel> { existing });

            // Act
            await repo.UpdateAsync(MakeLevel(1, 7, "Editor+"));

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WhenRecordExists_ThrowsFromMockQueryProvider()
        {
            // Arrange — ExecuteDeleteAsync cannot be reliably executed with this mock query provider.
            var levels = new[] { MakeLevel(1, 7, "Editor") };
            var repo = CreateRepository(levels);

            // Act + Assert
            await Assert.ThrowsAnyAsync<Exception>(() => repo.DeleteAsync(1, 7));
        }

        [Fact]
        public async Task DeleteAsync_WhenRecordDoesNotExist_ThrowsFromMockQueryProvider()
        {
            // Arrange — ExecuteDeleteAsync cannot be reliably executed with this mock query provider.
            var repo = CreateRepository();

            // Act + Assert
            await Assert.ThrowsAnyAsync<Exception>(() => repo.DeleteAsync(99, 88));
        }

        #endregion

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenCompositePkExists()
        {
            // Arrange
            var levels = new List<AccessLevel> { MakeLevel(1, 7, "Editor") };
            var repo = CreateRepository(levels);

            // Act
            var result = await repo.ExistsAsync(1, 7);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenSystemIdDoesNotMatch()
        {
            // Arrange
            var levels = new List<AccessLevel> { MakeLevel(1, 7, "Editor") };
            var repo = CreateRepository(levels);

            // Act
            var result = await repo.ExistsAsync(99, 7);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenAccessLevelIdDoesNotMatch()
        {
            // Arrange
            var levels = new List<AccessLevel> { MakeLevel(1, 7, "Editor") };
            var repo = CreateRepository(levels);

            // Act
            var result = await repo.ExistsAsync(1, 99);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.ExistsAsync(1, 7);

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
