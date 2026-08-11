using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.ProjectManagerRepositoryTest
{
    public class ProjectManagerRepositoryTests
    {
        private static ProjectManagerRepository CreateRepository(
            IEnumerable<ProjectManager>? managers = null)
        {
            var mockContext     = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var managersMockSet = RepositoryTestHelper.CreateMockDbSet(managers ?? Enumerable.Empty<ProjectManager>());

            RepositoryTestHelper.SetupDbSetOperations(managersMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.ProjectManagers).Returns(managersMockSet.Object);

            return new ProjectManagerRepository(mockContext.Object);
        }

        private static (
            ProjectManagerRepository Repo,
            Mock<DbSet<ProjectManager>> ManagersDbSet,
            Mock<PimsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<ProjectManager>? managers = null)
        {
            var mockContext     = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var managersMockSet = RepositoryTestHelper.CreateMockDbSet(managers ?? Enumerable.Empty<ProjectManager>());

            RepositoryTestHelper.SetupDbSetOperations(managersMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.ProjectManagers).Returns(managersMockSet.Object);

            var repo = new ProjectManagerRepository(mockContext.Object);
            return (repo, managersMockSet, mockContext);
        }

        // ── helpers ───────────────────────────────────────────────────────────────

        private static ProjectManager MakeManager(string name = "J. Smith", bool disable = false) =>
            new ProjectManager
            {
                Projectmanager = name,
                Email          = $"{name.Replace(". ", ".").Replace(" ", ".").ToLower()}@apha.gov.uk",
                LoginEmail     = $"{name.Replace(". ", ".").Replace(" ", ".").ToLower()}@login.apha.gov.uk",
                Disable        = disable
            };

        // ── GetAllAsync ───────────────────────────────────────────────────────────

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ReturnsAllManagers_WhenDataExists()
        {
            // Arrange
            var managers = new List<ProjectManager>
            {
                MakeManager("Smith, J."),
                MakeManager("Jones, A.")
            };
            var repo = CreateRepository(managers);

            // Act
            var result = await repo.GetAllProjectManagersAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, m => m.Projectmanager == "Smith, J.");
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.GetAllProjectManagersAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        // ── GetByIdAsync ──────────────────────────────────────────────────────────

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ReturnsManager_WhenNameExists()
        {
            // Arrange
            var managers = new List<ProjectManager>
            {
                MakeManager("Smith, J."),
                MakeManager("Jones, A.")
            };
            var repo = CreateRepository(managers);

            // Act
            var result = await repo.GetProjectManagerByNameAsync("Smith, J.");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Smith, J.", result!.Projectmanager);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNameDoesNotExist()
        {
            // Arrange
            var managers = new List<ProjectManager> { MakeManager("Smith, J.") };
            var repo = CreateRepository(managers);

            // Act
            var result = await repo.GetProjectManagerByNameAsync("Unknown Manager");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.GetProjectManagerByNameAsync("Smith, J.");

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("NONEXISTENT_MANAGER")]
        public async Task GetByIdAsync_ReturnsNull_WhenIdDoesNotMatch(string name)
        {
            // Arrange
            var managers = new List<ProjectManager> { MakeManager("Smith, J.") };
            var repo = CreateRepository(managers);

            // Act
            var result = await repo.GetProjectManagerByNameAsync(name);

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
            var manager = MakeManager("New Manager");

            // Act
            var result = await repo.AddProjectManagerAsync(manager);

            // Assert
            Assert.NotNull(result);
            Assert.Same(manager, result);
        }

        [Fact]
        public async Task AddAsync_CallsDbSetAdd()
        {
            // Arrange
            var (repo, managersDbSet, _) = CreateRepositoryWithMocks();
            var manager = MakeManager("New Manager");

            // Act
            await repo.AddProjectManagerAsync(manager);

            // Assert
            managersDbSet.Verify(
                x => x.Add(It.Is<ProjectManager>(m => m.Projectmanager == "New Manager")),
                Times.Once);
        }

        [Fact]
        public async Task AddAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks();
            var manager = MakeManager("New Manager");

            // Act
            await repo.AddProjectManagerAsync(manager);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task AddAsync_PreservesDisableField()
        {
            // Arrange
            var (repo, managersDbSet, _) = CreateRepositoryWithMocks();

            ProjectManager? captured = null;
            managersDbSet
                .Setup(x => x.Add(It.IsAny<ProjectManager>()))
                .Callback<ProjectManager>(m => captured = m);

            var manager = MakeManager("Manager X", disable: true);

            // Act
            await repo.AddProjectManagerAsync(manager);

            // Assert
            Assert.NotNull(captured);
            Assert.True(captured!.Disable);
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────────

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ReturnsUpdatedEntity()
        {
            // Arrange
            var existing = MakeManager("Smith, J.");
            var (repo, _, _) = CreateRepositoryWithMocks(new List<ProjectManager> { existing });
            var updatedEntity = new ProjectManager
            {
                Projectmanager = "Smith, J.",
                Email          = "new@apha.gov.uk",
                Disable        = true
            };

            // Act
            var result = await repo.UpdateProjectManagerAsync(updatedEntity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("new@apha.gov.uk", result.Email);
            Assert.True(result.Disable);
        }

        [Fact]
        public async Task UpdateAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var existing = MakeManager("Smith, J.");
            var (repo, _, mockContext) = CreateRepositoryWithMocks(new List<ProjectManager> { existing });

            // Act
            await repo.UpdateProjectManagerAsync(MakeManager("Smith, J."));

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        // ── ExistsAsync ───────────────────────────────────────────────────────────

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenNameExists()
        {
            // Arrange
            var managers = new List<ProjectManager> { MakeManager("Smith, J.") };
            var repo = CreateRepository(managers);

            // Act
            var result = await repo.ProjectManagerExistsAsync("Smith, J.");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenNameDoesNotExist()
        {
            // Arrange
            var managers = new List<ProjectManager> { MakeManager("Smith, J.") };
            var repo = CreateRepository(managers);

            // Act
            var result = await repo.ProjectManagerExistsAsync("Unknown Manager");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.ProjectManagerExistsAsync("Smith, J.");

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
