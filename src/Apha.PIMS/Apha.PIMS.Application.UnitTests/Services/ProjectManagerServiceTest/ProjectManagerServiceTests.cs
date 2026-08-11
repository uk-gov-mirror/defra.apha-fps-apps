using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Application.UnitTests.Services.ProjectManagerServiceTest
{
    public class ProjectManagerServiceTests
    {
        private readonly IProjectManagerRepository _repository;
        private readonly IMapper _mapper;
        private readonly ProjectManagerService _service;

        public ProjectManagerServiceTests()
        {
            _repository = Substitute.For<IProjectManagerRepository>();
            _mapper     = Substitute.For<IMapper>();
            _service    = new ProjectManagerService(_repository, _mapper);
        }

        // ── helpers ───────────────────────────────────────────────────────────────

        private static ProjectManager MakeEntity(string name = "J. Smith") =>
            new ProjectManager { Projectmanager = name, Email = "j.smith@apha.gov.uk", Disable = false };

        private static ProjectManagerDto MakeDto(string name = "J. Smith") =>
            new ProjectManagerDto { ProjectManager = name, Email = "j.smith@apha.gov.uk", Disable = false };

        // ── Constructor ───────────────────────────────────────────────────────────

        #region Constructor

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProjectManagerService(null!, _mapper));
        }

        [Fact]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ProjectManagerService(_repository, null!));
        }

        #endregion

        // ── GetAllAsync ───────────────────────────────────────────────────────────

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEntities_ReturnsMappedDtoList()
        {
            // Arrange
            var entities = new List<ProjectManager> { MakeEntity("Smith"), MakeEntity("Jones") };
            var dtos     = new List<ProjectManagerDto> { MakeDto("Smith"), MakeDto("Jones") };
            _repository.GetAllProjectManagersAsync().Returns(entities);
            _mapper.Map<List<ProjectManagerDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAllProjectManagersAsync();

            // Assert
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetAllProjectManagersAsync();
        }

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            _repository.GetAllProjectManagersAsync().Returns(new List<ProjectManager>());
            _mapper.Map<List<ProjectManagerDto>>(Arg.Any<List<ProjectManager>>()).Returns(new List<ProjectManagerDto>());

            // Act
            var result = await _service.GetAllProjectManagersAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _repository.GetAllProjectManagersAsync().ThrowsAsync(new InvalidOperationException("db error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetAllProjectManagersAsync());
        }

        #endregion

        // ── GetByIdAsync ──────────────────────────────────────────────────────────

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_EntityExists_ReturnsMappedDto()
        {
            // Arrange
            const string name = "J. Smith";
            var entity = MakeEntity(name);
            var dto    = MakeDto(name);
            _repository.GetProjectManagerByNameAsync(name).Returns(entity);
            _mapper.Map<ProjectManagerDto>(entity).Returns(dto);

            // Act
            var result = await _service.GetProjectManagerByNameAsync(name);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result!.ProjectManager);
        }

        [Fact]
        public async Task GetByIdAsync_EntityNotFound_ReturnsNull()
        {
            // Arrange
            _repository.GetProjectManagerByNameAsync(Arg.Any<string>()).Returns((ProjectManager?)null);

            // Act
            var result = await _service.GetProjectManagerByNameAsync("Unknown");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_EmptyName_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetProjectManagerByNameAsync(""));
        }

        [Fact]
        public async Task GetByIdAsync_WhitespaceName_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetProjectManagerByNameAsync("   "));
        }

        #endregion

        // ── CreateAsync ───────────────────────────────────────────────────────────

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidDto_ReturnsMappedCreatedDto()
        {
            // Arrange
            const string name = "New Manager";
            var dto     = MakeDto(name);
            var entity  = MakeEntity(name);
            var created = MakeEntity(name);
            var result_dto = MakeDto(name);
            _repository.ProjectManagerExistsAsync(name).Returns(false);
            _mapper.Map<ProjectManager>(dto).Returns(entity);
            _repository.AddProjectManagerAsync(entity).Returns(created);
            _mapper.Map<ProjectManagerDto>(created).Returns(result_dto);

            // Act
            var result = await _service.CreateProjectManagerAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(name, result.ProjectManager);
            await _repository.Received(1).AddProjectManagerAsync(entity);
        }

        [Fact]
        public async Task CreateAsync_DuplicateName_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = MakeDto("Existing Manager");
            _repository.ProjectManagerExistsAsync("Existing Manager").Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateProjectManagerAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateProjectManagerAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_EmptyName_ThrowsArgumentException()
        {
            var dto = new ProjectManagerDto { ProjectManager = "" };
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateProjectManagerAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_DoesNotCallAdd_WhenNameAlreadyExists()
        {
            // Arrange
            var dto = MakeDto("Existing");
            _repository.ProjectManagerExistsAsync("Existing").Returns(true);

            // Act + ignore exception
            try { await _service.CreateProjectManagerAsync(dto); } catch { }

            // Assert
            await _repository.DidNotReceive().AddProjectManagerAsync(Arg.Any<ProjectManager>());
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────────

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_EntityExists_ReturnsMappedUpdatedDto()
        {
            // Arrange
            const string name = "J. Smith";
            var dto     = MakeDto(name);
            var entity  = MakeEntity(name);
            var updated = MakeEntity(name);
            var result_dto = MakeDto(name);
            _repository.ProjectManagerExistsAsync(name).Returns(true);
            _mapper.Map<ProjectManager>(dto).Returns(entity);
            _repository.UpdateProjectManagerAsync(entity).Returns(updated);
            _mapper.Map<ProjectManagerDto>(updated).Returns(result_dto);

            // Act
            var result = await _service.UpdateProjectManagerAsync(dto);

            // Assert
            Assert.NotNull(result);
            await _repository.Received(1).UpdateProjectManagerAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_EntityNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _repository.ProjectManagerExistsAsync(Arg.Any<string>()).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateProjectManagerAsync(MakeDto("Unknown")));
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateProjectManagerAsync(null!));
        }

        [Fact]
        public async Task UpdateAsync_EmptyName_ThrowsArgumentException()
        {
            var dto = new ProjectManagerDto { ProjectManager = "" };
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateProjectManagerAsync(dto));
        }

        #endregion

        // ── DeleteAsync ───────────────────────────────────────────────────────────

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_EntityExists_ReturnsTrueAndCallsRepositoryDelete()
        {
            // Arrange
            const string name = "J. Smith";
            _repository.ProjectManagerExistsAsync(name).Returns(true);
            _repository.DeleteProjectManagerAsync(name).Returns(true);

            // Act
            var result = await _service.DeleteProjectManagerAsync(name);

            // Assert
            Assert.True(result);
            await _repository.Received(1).DeleteProjectManagerAsync(name);
        }

        [Fact]
        public async Task DeleteAsync_EntityNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _repository.ProjectManagerExistsAsync(Arg.Any<string>()).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteProjectManagerAsync("Unknown"));
        }

        [Fact]
        public async Task DeleteAsync_EmptyName_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteProjectManagerAsync(""));
        }

        #endregion

        // ── ExistsAsync ───────────────────────────────────────────────────────────

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_EntityExists_ReturnsTrue()
        {
            // Arrange
            _repository.ProjectManagerExistsAsync("J. Smith").Returns(true);

            // Act
            var result = await _service.ProjectManagerExistsAsync("J. Smith");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_EntityNotFound_ReturnsFalse()
        {
            // Arrange
            _repository.ProjectManagerExistsAsync(Arg.Any<string>()).Returns(false);

            // Act
            var result = await _service.ProjectManagerExistsAsync("Unknown");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_EmptyName_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ProjectManagerExistsAsync(""));
        }

        #endregion
    }
}
