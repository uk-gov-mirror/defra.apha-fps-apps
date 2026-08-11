using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.AccessUserRepositoryTest
{
    public class AccessUserRepositoryTests
    {
        private static AccessUserRepository CreateRepository(
            IEnumerable<AccessUser>? accessUsers = null)
        {
            var mockContext       = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var accessUsersMockSet = RepositoryTestHelper.CreateMockDbSet(accessUsers ?? Enumerable.Empty<AccessUser>());

            RepositoryTestHelper.SetupDbSetOperations(accessUsersMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.AccessUsers).Returns(accessUsersMockSet.Object);

            return new AccessUserRepository(mockContext.Object);
        }

        private static (
            AccessUserRepository Repo,
            Mock<DbSet<AccessUser>> AccessUsersDbSet,
            Mock<PimsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<AccessUser>? accessUsers = null)
        {
            var mockContext       = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var accessUsersMockSet = RepositoryTestHelper.CreateMockDbSet(accessUsers ?? Enumerable.Empty<AccessUser>());

            RepositoryTestHelper.SetupDbSetOperations(accessUsersMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.AccessUsers).Returns(accessUsersMockSet.Object);

            var repo = new AccessUserRepository(mockContext.Object);
            return (repo, accessUsersMockSet, mockContext);
        }

        // ── helpers ───────────────────────────────────────────────────────────────

        private static AccessUser MakeUser(int systemid = 1, string ntlogin = "DOM\\user1") =>
            new AccessUser { SystemId = systemid, NtLogin = ntlogin, UserName = "User One", UserEmail = "user1@example.com" };

        // ── GetAllAsync ───────────────────────────────────────────────────────────

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ReturnsAllAccessUsers_WhenDataExists()
        {
            // Arrange
            var users = new List<AccessUser>
            {
                MakeUser(1, "dom\\u1"),
                MakeUser(1, "dom\\u2"),
                MakeUser(2, "dom\\u3")
            };
            var repo = CreateRepository(users);

            // Act
            var result = await repo.GetAllAsync();

            // Assert
            Assert.Equal(3, result.Count);
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

        // ── GetBySystemIdAsync ────────────────────────────────────────────────────

        #region GetBySystemIdAsync

        [Fact]
        public async Task GetBySystemIdAsync_ReturnsMatchingUsers_WhenSystemIdExists()
        {
            // Arrange
            var users = new List<AccessUser>
            {
                MakeUser(1, "dom\\u1"),
                MakeUser(1, "dom\\u2"),
                MakeUser(2, "dom\\u3")
            };
            var repo = CreateRepository(users);

            // Act
            var result = await repo.GetBySystemIdAsync(1);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, u => Assert.Equal(1, u.SystemId));
        }

        [Fact]
        public async Task GetBySystemIdAsync_ReturnsEmptyList_WhenSystemIdNotFound()
        {
            // Arrange
            var users = new List<AccessUser> { MakeUser(1, "dom\\u1") };
            var repo = CreateRepository(users);

            // Act
            var result = await repo.GetBySystemIdAsync(99);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        // ── GetByNtLoginAsync ─────────────────────────────────────────────────────

        #region GetByNtLoginAsync

        [Fact]
        public async Task GetByNtLoginAsync_ReturnsMatchingUsers_WhenNtLoginExists()
        {
            // Arrange
            const string ntlogin = "dom\\jsmith";
            var users = new List<AccessUser>
            {
                MakeUser(1, ntlogin),
                MakeUser(2, ntlogin),
                MakeUser(1, "dom\\other")
            };
            var repo = CreateRepository(users);

            // Act
            var result = await repo.GetByNtLoginAsync(ntlogin);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, u => Assert.Equal(ntlogin, u.NtLogin));
        }

        [Fact]
        public async Task GetByNtLoginAsync_ReturnsEmptyList_WhenNtLoginNotFound()
        {
            // Arrange
            var users = new List<AccessUser> { MakeUser(1, "dom\\u1") };
            var repo = CreateRepository(users);

            // Act
            var result = await repo.GetByNtLoginAsync("dom\\unknown");

            // Assert
            Assert.Empty(result);
        }

        #endregion

        // ── GetByIdAsync ──────────────────────────────────────────────────────────

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ReturnsUser_WhenCompositePkExists()
        {
            // Arrange
            const string ntlogin = "dom\\user1";
            var users = new List<AccessUser>
            {
                MakeUser(1, ntlogin),
                MakeUser(1, "dom\\user2"),
                MakeUser(2, ntlogin)
            };
            var repo = CreateRepository(users);

            // Act
            var result = await repo.GetByIdAsync(1, ntlogin);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.SystemId);
            Assert.Equal(ntlogin, result.NtLogin);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenSystemIdDoesNotMatch()
        {
            // Arrange
            var users = new List<AccessUser> { MakeUser(1, "dom\\user") };
            var repo = CreateRepository(users);

            // Act
            var result = await repo.GetByIdAsync(99, "dom\\user");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNtLoginDoesNotMatch()
        {
            // Arrange
            var users = new List<AccessUser> { MakeUser(1, "dom\\user") };
            var repo = CreateRepository(users);

            // Act
            var result = await repo.GetByIdAsync(1, "dom\\unknown");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.GetByIdAsync(1, "dom\\user");

            // Assert
            Assert.Null(result);
        }

        #endregion

        // ── AddAsync ──────────────────────────────────────────────────────────────

        #region AddAsync

        [Fact]
        public async Task AddAsync_ReturnsAddedEntity()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks();
            var user = MakeUser(1, "dom\\newuser");

            // Act
            var result = await repo.AddAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Same(user, result);
        }

        [Fact]
        public async Task AddAsync_CallsDbSetAdd()
        {
            // Arrange
            var (repo, accessUsersDbSet, _) = CreateRepositoryWithMocks();
            var user = MakeUser(1, "dom\\newuser");

            // Act
            await repo.AddAsync(user);

            // Assert
            accessUsersDbSet.Verify(
                x => x.Add(It.Is<AccessUser>(u => u.NtLogin == "dom\\newuser")),
                Times.Once);
        }

        [Fact]
        public async Task AddAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks();
            var user = MakeUser(1, "dom\\newuser");

            // Act
            await repo.AddAsync(user);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────────

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ReturnsUpdatedEntity()
        {
            // Arrange
            var existing = MakeUser(1, "dom\\user");
            var (repo, _, _) = CreateRepositoryWithMocks(new List<AccessUser> { existing });
            var updatedEntity = new AccessUser
            {
                SystemId = 1,
                NtLogin  = "dom\\user",
                UserName = "Updated User Name"
            };

            // Act
            var result = await repo.UpdateAsync(updatedEntity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated User Name", result.UserName);
        }

        [Fact]
        public async Task UpdateAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var existing = MakeUser(1, "dom\\user");
            var (repo, _, mockContext) = CreateRepositoryWithMocks(new List<AccessUser> { existing });

            // Act
            await repo.UpdateAsync(MakeUser(1, "dom\\user"));

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        // ── ExistsAsync ───────────────────────────────────────────────────────────

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenCompositePkExists()
        {
            // Arrange
            var users = new List<AccessUser> { MakeUser(1, "dom\\user") };
            var repo = CreateRepository(users);

            // Act
            var result = await repo.ExistsAsync(1, "dom\\user");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenSystemIdDoesNotMatch()
        {
            // Arrange
            var users = new List<AccessUser> { MakeUser(1, "dom\\user") };
            var repo = CreateRepository(users);

            // Act
            var result = await repo.ExistsAsync(99, "dom\\user");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenNtLoginDoesNotMatch()
        {
            // Arrange
            var users = new List<AccessUser> { MakeUser(1, "dom\\user") };
            var repo = CreateRepository(users);

            // Act
            var result = await repo.ExistsAsync(1, "dom\\unknown");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.ExistsAsync(1, "dom\\user");

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
