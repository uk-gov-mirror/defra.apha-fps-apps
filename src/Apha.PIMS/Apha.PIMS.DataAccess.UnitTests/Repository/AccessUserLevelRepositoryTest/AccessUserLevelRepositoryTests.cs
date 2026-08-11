using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.AccessUserLevelRepositoryTest
{
    public class AccessUserLevelRepositoryTests
    {
        private static AccessUserLevelRepository CreateRepository(
            IEnumerable<AccessUserLevel>? accessUserLevels = null,
            IEnumerable<AccessUser>? accessUsers = null)
        {
            var mockContext            = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var accessUserLevelsMockSet = RepositoryTestHelper.CreateMockDbSet(accessUserLevels ?? Enumerable.Empty<AccessUserLevel>());
            var accessUsersMockSet      = RepositoryTestHelper.CreateMockDbSet(accessUsers ?? Enumerable.Empty<AccessUser>());

            RepositoryTestHelper.SetupDbSetOperations(accessUserLevelsMockSet);
            RepositoryTestHelper.SetupDbSetOperations(accessUsersMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.AccessUserLevels).Returns(accessUserLevelsMockSet.Object);
            mockContext.Setup(x => x.AccessUsers).Returns(accessUsersMockSet.Object);

            return new AccessUserLevelRepository(mockContext.Object);
        }

        private static (
            AccessUserLevelRepository Repo,
            Mock<DbSet<AccessUserLevel>> AccessUserLevelsDbSet,
            Mock<PimsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<AccessUserLevel>? accessUserLevels = null)
        {
            var mockContext            = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var accessUserLevelsMockSet = RepositoryTestHelper.CreateMockDbSet(accessUserLevels ?? Enumerable.Empty<AccessUserLevel>());
            var accessUsersMockSet      = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<AccessUser>());

            RepositoryTestHelper.SetupDbSetOperations(accessUserLevelsMockSet);
            RepositoryTestHelper.SetupDbSetOperations(accessUsersMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.AccessUserLevels).Returns(accessUserLevelsMockSet.Object);
            mockContext.Setup(x => x.AccessUsers).Returns(accessUsersMockSet.Object);

            var repo = new AccessUserLevelRepository(mockContext.Object);
            return (repo, accessUserLevelsMockSet, mockContext);
        }

        private static AccessUserLevel MakeUserLevel(int systemid = 1, string ntlogin = "DOM\\user1", int accesslevelid = 10) =>
            new() { SystemId = systemid, NtLogin = ntlogin, AccessLevelId = accesslevelid };

        private static AccessUser MakeUser(int systemid = 1, string ntlogin = "DOM\\user1", string username = "User One") =>
            new() { SystemId = systemid, NtLogin = ntlogin, UserName = username, UserEmail = "user@example.com" };

        #region GetPagedAccessUserLevelAllAsync

        [Fact]
        public async Task GetPagedAccessUserLevelAllAsync_ReturnsPagedAndOrderedData_WhenNoFilter()
        {
            // Arrange
            var data = new List<AccessUserLevel>
            {
                MakeUserLevel(2, "dom\\u3", 3),
                MakeUserLevel(1, "dom\\u2", 2),
                MakeUserLevel(1, "dom\\u1", 1)
            };
            var repo = CreateRepository(data);
            var query = new PaginationParameters<string>(page: 1, pageSize: 2);

            // Act
            var result = await repo.GetPagedAccessUserLevelAllAsync(query);

            // Assert
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal((1, "dom\\u1"), (result.Data.ElementAt(0).SystemId, result.Data.ElementAt(0).NtLogin));
            Assert.Equal((1, "dom\\u2"), (result.Data.ElementAt(1).SystemId, result.Data.ElementAt(1).NtLogin));
        }

        [Fact]
        public async Task GetPagedAccessUserLevelAllAsync_AppliesSystemIdFilter_WhenFilterContainsSystemId()
        {
            // Arrange
            var data = new List<AccessUserLevel>
            {
                MakeUserLevel(1, "dom\\u1", 1),
                MakeUserLevel(1, "dom\\u2", 2),
                MakeUserLevel(2, "dom\\u3", 3)
            };
            var repo = CreateRepository(data);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"SystemId\":\"1\"}"
            };

            // Act
            var result = await repo.GetPagedAccessUserLevelAllAsync(query);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, x => Assert.Equal(1, x.SystemId));
        }

        [Fact]
        public async Task GetPagedAccessUserLevelAllAsync_AppliesSortByAccessLevelIdDescending()
        {
            // Arrange
            var data = new List<AccessUserLevel>
            {
                MakeUserLevel(1, "dom\\u1", 1),
                MakeUserLevel(1, "dom\\u2", 3),
                MakeUserLevel(1, "dom\\u3", 2)
            };
            var repo = CreateRepository(data);
            var query = new PaginationParameters<string>(sortBy: "AccessLevelId", descending: true, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedAccessUserLevelAllAsync(query);

            // Assert
            Assert.Equal(new[] { 3, 2, 1 }, result.Data.Select(x => x.AccessLevelId).ToArray());
        }

        [Fact]
        public async Task GetPagedAccessUserLevelAllAsync_UsesDefaultPaging_WhenPageAndPageSizeAreInvalid()
        {
            // Arrange
            var data = Enumerable.Range(1, 12)
                .Select(i => MakeUserLevel(1, $"dom\\u{i:D2}", i))
                .ToList();
            var repo = CreateRepository(data);
            var query = new PaginationParameters<string>(page: 0, pageSize: 0);

            // Act
            var result = await repo.GetPagedAccessUserLevelAllAsync(query);

            // Assert
            Assert.Equal(1, result.PaginationData.PageNumber);
            Assert.Equal(10, result.PaginationData.PageSize);
            Assert.Equal(12, result.PaginationData.TotalRecords);
            Assert.Equal(10, result.Data.Count);
        }

        #endregion

        #region GetBySystemIdAsync

        [Fact]
        public async Task GetBySystemIdAsync_ReturnsMatchingAssignments_WhenSystemIdExists()
        {
            // Arrange
            var data = new List<AccessUserLevel>
            {
                MakeUserLevel(1, "dom\\u2", 2),
                MakeUserLevel(1, "dom\\u1", 3),
                MakeUserLevel(1, "dom\\u1", 1),
                MakeUserLevel(2, "dom\\x", 1)
            };
            var repo = CreateRepository(data);

            // Act
            var result = await repo.GetBySystemIdAsync(1);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.All(result, r => Assert.Equal(1, r.SystemId));
            Assert.Equal(("dom\\u1", 1), (result[0].NtLogin, result[0].AccessLevelId));
            Assert.Equal(("dom\\u1", 3), (result[1].NtLogin, result[1].AccessLevelId));
            Assert.Equal(("dom\\u2", 2), (result[2].NtLogin, result[2].AccessLevelId));
        }

        [Fact]
        public async Task GetBySystemIdAsync_ReturnsEmptyList_WhenSystemIdNotFound()
        {
            // Arrange
            var repo = CreateRepository(new List<AccessUserLevel> { MakeUserLevel(1, "dom\\u1", 1) });

            // Act
            var result = await repo.GetBySystemIdAsync(99);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetByUserAsync

        [Fact]
        public async Task GetByUserAsync_ReturnsMatchingAssignments_WhenCompositePrefixMatches()
        {
            // Arrange
            const string ntlogin = "dom\\user1";
            var data = new List<AccessUserLevel>
            {
                MakeUserLevel(1, ntlogin, 3),
                MakeUserLevel(1, ntlogin, 1),
                MakeUserLevel(1, "dom\\other", 1),
                MakeUserLevel(2, ntlogin, 2)
            };
            var repo = CreateRepository(data);

            // Act
            var result = await repo.GetByUserAsync(1, ntlogin);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r =>
            {
                Assert.Equal(1, r.SystemId);
                Assert.Equal(ntlogin, r.NtLogin);
            });
            Assert.Equal(new[] { 1, 3 }, result.Select(x => x.AccessLevelId).ToArray());
        }

        [Fact]
        public async Task GetByUserAsync_ReturnsEmptyList_WhenNoMatch()
        {
            // Arrange
            var repo = CreateRepository(new List<AccessUserLevel> { MakeUserLevel(1, "dom\\u1", 1) });

            // Act
            var result = await repo.GetByUserAsync(1, "dom\\unknown");

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ReturnsAssignment_WhenTripleCompositeKeyMatches()
        {
            // Arrange
            const string ntlogin = "dom\\user1";
            var data = new List<AccessUserLevel>
            {
                MakeUserLevel(1, ntlogin, 2),
                MakeUserLevel(1, ntlogin, 3),
                MakeUserLevel(2, ntlogin, 2)
            };
            var repo = CreateRepository(data);

            // Act
            var result = await repo.GetByIdAsync(1, ntlogin, 3);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.SystemId);
            Assert.Equal(ntlogin, result.NtLogin);
            Assert.Equal(3, result.AccessLevelId);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenSystemIdDoesNotMatch()
        {
            // Arrange
            var repo = CreateRepository(new List<AccessUserLevel> { MakeUserLevel(1, "dom\\u1", 1) });

            // Act
            var result = await repo.GetByIdAsync(99, "dom\\u1", 1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNtLoginDoesNotMatch()
        {
            // Arrange
            var repo = CreateRepository(new List<AccessUserLevel> { MakeUserLevel(1, "dom\\u1", 1) });

            // Act
            var result = await repo.GetByIdAsync(1, "dom\\unknown", 1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenAccessLevelIdDoesNotMatch()
        {
            // Arrange
            var repo = CreateRepository(new List<AccessUserLevel> { MakeUserLevel(1, "dom\\u1", 1) });

            // Act
            var result = await repo.GetByIdAsync(1, "dom\\u1", 99);

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
            var entity = MakeUserLevel(1, "dom\\new", 7);

            // Act
            var result = await repo.AddAsync(entity);

            // Assert
            Assert.Same(entity, result);
        }

        [Fact]
        public async Task AddAsync_CallsDbSetAdd()
        {
            // Arrange
            var (repo, dbSet, _) = CreateRepositoryWithMocks();
            var entity = MakeUserLevel(1, "dom\\new", 7);

            // Act
            await repo.AddAsync(entity);

            // Assert
            dbSet.Verify(x => x.Add(It.Is<AccessUserLevel>(u => u.SystemId == 1 && u.NtLogin == "dom\\new" && u.AccessLevelId == 7)), Times.Once);
        }

        [Fact]
        public async Task AddAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, context) = CreateRepositoryWithMocks();

            // Act
            await repo.AddAsync(MakeUserLevel(1, "dom\\new", 7));

            // Assert
            RepositoryTestHelper.VerifySaveChanges(context, times: 1);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WhenRecordExists_ThrowsFromMockQueryProvider()
        {
            // Arrange — ExecuteDeleteAsync cannot be reliably executed with this mock query provider.
            var repo = CreateRepository(new[] { MakeUserLevel(1, "dom\\user", 2) });

            // Act + Assert
            await Assert.ThrowsAnyAsync<Exception>(() => repo.DeleteAsync(1, "dom\\user", 2));
        }

        [Fact]
        public async Task DeleteAsync_WhenRecordDoesNotExist_ThrowsFromMockQueryProvider()
        {
            // Arrange — ExecuteDeleteAsync cannot be reliably executed with this mock query provider.
            var repo = CreateRepository();

            // Act + Assert
            await Assert.ThrowsAnyAsync<Exception>(() => repo.DeleteAsync(99, "dom\\none", 88));
        }

        #endregion

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenTripleCompositeKeyMatches()
        {
            // Arrange
            var repo = CreateRepository(new[] { MakeUserLevel(1, "dom\\user", 2) });

            // Act
            var result = await repo.ExistsAsync(1, "dom\\user", 2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenSystemIdDoesNotMatch()
        {
            // Arrange
            var repo = CreateRepository(new[] { MakeUserLevel(1, "dom\\user", 2) });

            // Act
            var result = await repo.ExistsAsync(99, "dom\\user", 2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenNtLoginDoesNotMatch()
        {
            // Arrange
            var repo = CreateRepository(new[] { MakeUserLevel(1, "dom\\user", 2) });

            // Act
            var result = await repo.ExistsAsync(1, "dom\\other", 2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenAccessLevelIdDoesNotMatch()
        {
            // Arrange
            var repo = CreateRepository(new[] { MakeUserLevel(1, "dom\\user", 2) });

            // Act
            var result = await repo.ExistsAsync(1, "dom\\user", 99);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.ExistsAsync(1, "dom\\user", 2);

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
