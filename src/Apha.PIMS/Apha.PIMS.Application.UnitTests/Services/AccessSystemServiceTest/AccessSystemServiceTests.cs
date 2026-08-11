using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;
using NSubstitute;

namespace Apha.PIMS.Application.UnitTests.Services.AccessSystemServiceTest
{
    public class AccessSystemServiceTests
    {
        private readonly IAccessSystemRepository _repository;
        private readonly IMapper _mapper;
        private readonly AccessSystemService _service;

        public AccessSystemServiceTests()
        {
            _repository = Substitute.For<IAccessSystemRepository>();
            _mapper     = Substitute.For<IMapper>();
            _service    = new AccessSystemService(_repository, _mapper);
        }

        private static AccessSystem MakeEntity(int systemid = 1, string name = "PIMS") =>
            new() { SystemId = systemid, SystemName = name };

        private static AccessSystemDto MakeDto(int systemid = 1, string name = "PIMS") =>
            new() { SystemId = systemid, SystemName = name };

        #region Constructor

        [Fact]
        public void Constructor_NullRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AccessSystemService(null!, _mapper));
        }

        [Fact]
        public void Constructor_NullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AccessSystemService(_repository, null!));
        }

        #endregion

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_RepositoryReturnsEntities_ReturnsMappedDtoList()
        {
            // Arrange
            var entities = new List<AccessSystem> { MakeEntity(1, "PIMS"), MakeEntity(2, "PACT") };
            var dtos     = new List<AccessSystemDto> { MakeDto(1, "PIMS"), MakeDto(2, "PACT") };
            _repository.GetAllAsync().Returns(entities);
            _mapper.Map<List<AccessSystemDto>>(entities).Returns(dtos);

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
            _repository.GetAllAsync().Returns(new List<AccessSystem>());
            _mapper.Map<List<AccessSystemDto>>(Arg.Any<List<AccessSystem>>()).Returns(new List<AccessSystemDto>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_EntityExists_ReturnsMappedDto()
        {
            // Arrange
            var entity = MakeEntity(1, "PIMS");
            var dto    = MakeDto(1, "PIMS");
            _repository.GetByIdAsync(1).Returns(entity);
            _mapper.Map<AccessSystemDto>(entity).Returns(dto);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.SystemId);
            Assert.Equal("PIMS", result.SystemName);
        }

        [Fact]
        public async Task GetByIdAsync_EntityNotFound_ReturnsNull()
        {
            // Arrange
            _repository.GetByIdAsync(Arg.Any<int>()).Returns((AccessSystem?)null);

            // Act
            var result = await _service.GetByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_EntityExists_ReturnsTrue()
        {
            // Arrange
            _repository.ExistsAsync(1).Returns(true);

            // Act
            var result = await _service.ExistsAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_EntityNotFound_ReturnsFalse()
        {
            // Arrange
            _repository.ExistsAsync(Arg.Any<int>()).Returns(false);

            // Act
            var result = await _service.ExistsAsync(99);

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
