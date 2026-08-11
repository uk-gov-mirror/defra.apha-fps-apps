using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.PIMS.Application.UnitTests.Services.AccessLevelServiceTest
{
    public class AccessLevelServiceTests
    {
        private readonly IAccessLevelRepository _repository;
        private readonly IMapper _mapper;
        private readonly AccessLevelService _service;

        public AccessLevelServiceTests()
        {
            _repository = Substitute.For<IAccessLevelRepository>();
            _mapper     = Substitute.For<IMapper>();
            _service    = new AccessLevelService(_repository, _mapper);
        }

        private static AccessLevel MakeEntity(int systemid = 1, int accesslevelid = 10, string name = "Level 1") =>
            new() { SystemId = systemid, AccessLevelId = accesslevelid, AccessLevelName = name };

        private static AccessLevelDto MakeDto(int systemid = 1, int accesslevelid = 10, string name = "Level 1") =>
            new() { SystemId = systemid, AccessLevelId = accesslevelid, AccessLevelName = name };

        #region Constructor

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AccessLevelService(null!, _mapper));
        }

        [Fact]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AccessLevelService(_repository, null!));
        }

        #endregion

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEntities_ReturnsMappedDtoList()
        {
            // Arrange
            var entities = new List<AccessLevel> { MakeEntity(1, 1, "Read"), MakeEntity(1, 2, "Write") };
            var dtos     = new List<AccessLevelDto> { MakeDto(1, 1, "Read"), MakeDto(1, 2, "Write") };
            _repository.GetAllAsync().Returns(entities);
            _mapper.Map<List<AccessLevelDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
            await _repository.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEmptyList_ReturnsEmptyDtoList()
        {
            // Arrange
            _repository.GetAllAsync().Returns(new List<AccessLevel>());
            _mapper.Map<List<AccessLevelDto>>(Arg.Any<List<AccessLevel>>()).Returns(new List<AccessLevelDto>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetBySystemIdAsync

        [Fact]
        public async Task GetBySystemIdAsync_RepositoryReturnsEntities_ReturnsMappedList()
        {
            // Arrange
            var entities = new List<AccessLevel> { MakeEntity(2, 3, "Admin") };
            var dtos     = new List<AccessLevelDto> { MakeDto(2, 3, "Admin") };
            _repository.GetBySystemIdAsync(2).Returns(entities);
            _mapper.Map<List<AccessLevelDto>>(entities).Returns(dtos);

            // Act
            var result = await _service.GetBySystemIdAsync(2);

            // Assert
            Assert.Single(result);
            await _repository.Received(1).GetBySystemIdAsync(2);
        }

        [Fact]
        public async Task GetBySystemIdAsync_RepositoryReturnsEmpty_ReturnsEmptyList()
        {
            // Arrange
            _repository.GetBySystemIdAsync(99).Returns(new List<AccessLevel>());
            _mapper.Map<List<AccessLevelDto>>(Arg.Any<List<AccessLevel>>()).Returns(new List<AccessLevelDto>());

            // Act
            var result = await _service.GetBySystemIdAsync(99);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_EntityExists_ReturnsMappedDto()
        {
            // Arrange
            var entity = MakeEntity(1, 10, "Read");
            var dto    = MakeDto(1, 10, "Read");
            _repository.GetByIdAsync(1, 10).Returns(entity);
            _mapper.Map<AccessLevelDto>(entity).Returns(dto);

            // Act
            var result = await _service.GetByIdAsync(1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result!.AccessLevelId);
        }

        [Fact]
        public async Task GetByIdAsync_EntityNotFound_ReturnsNull()
        {
            // Arrange
            _repository.GetByIdAsync(Arg.Any<int>(), Arg.Any<int>()).Returns((AccessLevel?)null);

            // Act
            var result = await _service.GetByIdAsync(99, 88);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidDto_ReturnsMappedCreatedDto()
        {
            // Arrange
            var dto        = MakeDto(1, 7, "Editor");
            var entity     = MakeEntity(1, 7, "Editor");
            var created    = MakeEntity(1, 7, "Editor");
            var resultDto  = MakeDto(1, 7, "Editor");
            _repository.ExistsAsync(1, 7).Returns(false);
            _mapper.Map<AccessLevel>(dto).Returns(entity);
            _repository.AddAsync(entity).Returns(created);
            _mapper.Map<AccessLevelDto>(created).Returns(resultDto);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(7, result.AccessLevelId);
            await _repository.Received(1).AddAsync(entity);
        }

        [Fact]
        public async Task CreateAsync_DuplicateLevelExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = MakeDto(1, 7, "Editor");
            _repository.ExistsAsync(1, 7).Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_DoesNotCallAdd_WhenLevelAlreadyExists()
        {
            // Arrange
            var dto = MakeDto(1, 7, "Editor");
            _repository.ExistsAsync(1, 7).Returns(true);

            // Act + ignore exception
            try { await _service.CreateAsync(dto); } catch { }

            // Assert
            await _repository.DidNotReceive().AddAsync(Arg.Any<AccessLevel>());
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_EntityExists_ReturnsMappedUpdatedDto()
        {
            // Arrange
            var dto        = MakeDto(1, 7, "Editor+");
            var entity     = MakeEntity(1, 7, "Editor+");
            var updated    = MakeEntity(1, 7, "Editor+");
            var resultDto  = MakeDto(1, 7, "Editor+");
            _repository.ExistsAsync(1, 7).Returns(true);
            _mapper.Map<AccessLevel>(dto).Returns(entity);
            _repository.UpdateAsync(entity).Returns(updated);
            _mapper.Map<AccessLevelDto>(updated).Returns(resultDto);

            // Act
            var result = await _service.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            await _repository.Received(1).UpdateAsync(entity);
        }

        [Fact]
        public async Task UpdateAsync_EntityNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _repository.ExistsAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync(MakeDto(99, 88, "X")));
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.UpdateAsync(null!));
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_EntityExists_CallsRepositoryDelete()
        {
            // Arrange
            _repository.ExistsAsync(1, 7).Returns(true);

            // Act
            await _service.DeleteAsync(1, 7);

            // Assert
            await _repository.Received(1).DeleteAsync(1, 7);
        }

        [Fact]
        public async Task DeleteAsync_EntityNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _repository.ExistsAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync(99, 88));
        }

        #endregion

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_EntityExists_ReturnsTrue()
        {
            // Arrange
            _repository.ExistsAsync(1, 7).Returns(true);

            // Act
            var result = await _service.ExistsAsync(1, 7);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_EntityNotFound_ReturnsFalse()
        {
            // Arrange
            _repository.ExistsAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(false);

            // Act
            var result = await _service.ExistsAsync(99, 88);

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
