using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.AccessSystemRepositoryTest
{
    public class AccessSystemRepositoryTests
    {
        private static AccessSystemRepository CreateRepository(IEnumerable<AccessSystem>? accessSystems = null)
        {
            var mockContext         = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var accessSystemsMockSet = RepositoryTestHelper.CreateMockDbSet(accessSystems ?? Enumerable.Empty<AccessSystem>());

            RepositoryTestHelper.SetupDbSetOperations(accessSystemsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.AccessSystems).Returns(accessSystemsMockSet.Object);

            return new AccessSystemRepository(mockContext.Object);
        }

        private static AccessSystem MakeSystem(int systemid = 1, string name = "PIMS") =>
            new() { SystemId = systemid, SystemName = name };

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ReturnsAllAccessSystems_WhenDataExists()
        {
            // Arrange
            var systems = new List<AccessSystem>
            {
                MakeSystem(3, "S3"),
                MakeSystem(1, "S1"),
                MakeSystem(2, "S2")
            };
            var repo = CreateRepository(systems);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0].SystemId);
            Assert.Equal(2, result[1].SystemId);
            Assert.Equal(3, result[2].SystemId);
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

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ReturnsSystem_WhenSystemIdExists()
        {
            // Arrange
            var systems = new List<AccessSystem>
            {
                MakeSystem(1, "PIMS"),
                MakeSystem(2, "PACT")
            };
            var repo = CreateRepository(systems);

            // Act
            var result = await repo.GetByIdAsync(2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result!.SystemId);
            Assert.Equal("PACT", result.SystemName);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenSystemIdDoesNotMatch()
        {
            // Arrange
            var systems = new List<AccessSystem> { MakeSystem(1, "PIMS") };
            var repo = CreateRepository(systems);

            // Act
            var result = await repo.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.GetByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenSystemIdExists()
        {
            // Arrange
            var systems = new List<AccessSystem> { MakeSystem(1, "PIMS") };
            var repo = CreateRepository(systems);

            // Act
            var result = await repo.ExistsAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenSystemIdDoesNotMatch()
        {
            // Arrange
            var systems = new List<AccessSystem> { MakeSystem(1, "PIMS") };
            var repo = CreateRepository(systems);

            // Act
            var result = await repo.ExistsAsync(99);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.ExistsAsync(1);

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
