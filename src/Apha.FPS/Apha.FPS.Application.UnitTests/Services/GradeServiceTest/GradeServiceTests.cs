using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.GradeServiceTest
{
    public class GradeServiceTests
    {
        private readonly IGradeRepository _mockRepository;
        private readonly IDivisionGradeRepository _mockDivisionGradeRepository;
        private readonly IProfitCentreGradeRepository _mockProfitCentreGradeRepository;
        private readonly IWorkGroupGradeRepository _mockWorkGroupGradeRepository;
        private readonly IMapper _mockMapper;
        private readonly GradeService _sut;

        public GradeServiceTests()
        {
            _mockRepository                  = Substitute.For<IGradeRepository>();
            _mockDivisionGradeRepository     = Substitute.For<IDivisionGradeRepository>();
            _mockProfitCentreGradeRepository = Substitute.For<IProfitCentreGradeRepository>();
            _mockWorkGroupGradeRepository    = Substitute.For<IWorkGroupGradeRepository>();
            _mockMapper                      = Substitute.For<IMapper>();
            _sut = new GradeService(
                _mockRepository,
                _mockDivisionGradeRepository,
                _mockProfitCentreGradeRepository,
                _mockWorkGroupGradeRepository,
                _mockMapper);
        }

        private static GradeDto BuildDto(string code = "A") =>
            new() { GradeCode = code, Description = "Grade A", AvSalary = 50000m, FpsYear = 2025 };

        private static Grade BuildEntity(string code = "A") =>
            new() { GradeCode = code, DescLong = "Grade A", AvSalary = 50000m, FpsYear = 2025 };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenRepositoryIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GradeService(null!, _mockDivisionGradeRepository, _mockProfitCentreGradeRepository, _mockWorkGroupGradeRepository, _mockMapper));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GradeService(_mockRepository, _mockDivisionGradeRepository, _mockProfitCentreGradeRepository, _mockWorkGroupGradeRepository, null!));
        }

        #endregion

        #region GetAllPagedAsync Tests

        [Fact]
        public async Task GetAllPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetAllPagedAsync(null!));
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsPaginatedResult()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData   = new PagedData<Grade>
            {
                Data           = new List<Grade> { BuildEntity() },
                PaginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var pagedResult = new PaginatedResult<GradeDto>
            {
                Data           = new List<GradeDto> { BuildDto() },
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAllPagedAsync(mappedParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<GradeDto>>(pagedData).Returns(pagedResult);

            // Act
            var result = await _sut.GetAllPagedAsync(query);

            // Assert
            result.Should().Be(pagedResult);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetAllPagedAsync(mappedParams);
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsEmptyResult_WhenNoData()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData   = new PagedData<Grade>
            {
                Data           = [],
                PaginationData = new PaginationData { TotalRecords = 0 }
            };
            var emptyResult = new PaginatedResult<GradeDto>
            {
                Data           = [],
                PaginationData = new PaginationDto { TotalRecords = 0 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAllPagedAsync(mappedParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<GradeDto>>(pagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetAllPagedAsync(query);

            // Assert
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ThrowsArgumentException_WhenCodeIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetByIdAsync(""));
        }

        [Fact]
        public async Task GetByIdAsync_ThrowsArgumentException_WhenCodeIsWhiteSpace()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetByIdAsync("   "));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            _mockRepository.GetByIdAsync("NOTEXIST").Returns((Grade?)null);

            var result = await _sut.GetByIdAsync("NOTEXIST");

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsMappedDto_WhenFound()
        {
            // Arrange
            var entity = BuildEntity("A");
            var dto    = BuildDto("A");

            _mockRepository.GetByIdAsync("A").Returns(entity);
            _mockMapper.Map<GradeDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetByIdAsync("A");

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).GetByIdAsync("A");
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_ThrowsArgumentException_WhenGradeCodeIsEmpty()
        {
            var dto = new GradeDto { GradeCode = "", Description = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_ThrowsArgumentException_WhenGradeCodeIsWhiteSpace()
        {
            var dto = new GradeDto { GradeCode = "   ", Description = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_ThrowsInvalidOperationException_WhenGradeAlreadyExists()
        {
            // Arrange
            var dto    = BuildDto("A");
            var entity = BuildEntity("A");

            _mockRepository.GetByIdAsync("A").Returns(entity);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateAsync(dto));
        }

        [Fact]
        public async Task CreateAsync_ReturnsMappedDto_WhenSuccessful()
        {
            // Arrange
            var dto     = BuildDto("A");
            var entity  = BuildEntity("A");
            var created = BuildEntity("A");

            _mockRepository.GetByIdAsync("A").Returns((Grade?)null);
            _mockMapper.Map<Grade>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).Returns(created);
            _mockMapper.Map<GradeDto>(created).Returns(dto);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).CreateAsync(entity);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.UpdateAsync("A", null!));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentException_WhenOriginalCodeIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAsync("", BuildDto()));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentException_WhenOriginalCodeIsWhiteSpace()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAsync("   ", BuildDto()));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentException_WhenDtoGradeCodeIsEmpty()
        {
            var dto = new GradeDto { GradeCode = "", Description = "Test" };
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAsync("A", dto));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsKeyNotFoundException_WhenOriginalGradeNotFound()
        {
            // Arrange
            var dto = BuildDto("A");

            _mockRepository.GetByIdAsync("NOTEXIST").Returns((Grade?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAsync("NOTEXIST", dto));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsInvalidOperationException_WhenRenameConflicts()
        {
            // Arrange
            var originalEntity = BuildEntity("A");
            var conflictEntity = BuildEntity("B");
            var dto            = BuildDto("B"); // renaming A → B, but B already exists

            _mockRepository.GetByIdAsync("A").Returns(originalEntity);
            _mockRepository.GetByIdAsync("B").Returns(conflictEntity);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateAsync("A", dto));
        }

        [Fact]
        public async Task UpdateAsync_ReturnsMappedDto_WhenSuccessful()
        {
            // Arrange
            var existingEntity = BuildEntity("A");
            var dto            = BuildDto("A");
            var entity         = BuildEntity("A");
            var updatedEntity  = BuildEntity("A");

            _mockRepository.GetByIdAsync("A").Returns(existingEntity);
            _mockMapper.Map<Grade>(dto).Returns(entity);
            _mockRepository.UpdateAsync("A", entity).Returns(updatedEntity);
            _mockMapper.Map<GradeDto>(updatedEntity).Returns(dto);

            // Act
            var result = await _sut.UpdateAsync("A", dto);

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).UpdateAsync("A", entity);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsMappedDto_WhenRenameSucceeds()
        {
            // Arrange
            var existingEntity = BuildEntity("A");
            var newDto         = BuildDto("B");   // renaming A → B
            var newEntity      = BuildEntity("B");
            var updatedEntity  = BuildEntity("B");

            // A exists; B does not → rename is valid
            _mockRepository.GetByIdAsync("A").Returns(existingEntity);
            _mockRepository.GetByIdAsync("B").Returns((Grade?)null);
            _mockMapper.Map<Grade>(newDto).Returns(newEntity);
            _mockRepository.UpdateAsync("A", newEntity).Returns(updatedEntity);
            _mockMapper.Map<GradeDto>(updatedEntity).Returns(newDto);

            // Act
            var result = await _sut.UpdateAsync("A", newDto);

            // Assert
            result.Should().Be(newDto);
            await _mockRepository.Received(1).UpdateAsync("A", newEntity);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ThrowsArgumentException_WhenCodeIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteAsync(""));
        }

        [Fact]
        public async Task DeleteAsync_ThrowsArgumentException_WhenCodeIsWhiteSpace()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.DeleteAsync("   "));
        }

        [Fact]
        public async Task DeleteAsync_ThrowsKeyNotFoundException_WhenGradeNotFound()
        {
            // Arrange
            _mockRepository.GetByIdAsync("NOTEXIST").Returns((Grade?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteAsync("NOTEXIST"));
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenDeleted()
        {
            // Arrange
            var existing = BuildEntity("A");
            _mockRepository.GetByIdAsync("A").Returns(existing);
            _mockDivisionGradeRepository.ExistsForGradeCodeAsync("A").Returns(false);
            _mockProfitCentreGradeRepository.ExistsForGradeCodeAsync("A").Returns(false);
            _mockWorkGroupGradeRepository.ExistsForGradeCodeAsync("A").Returns(false);
            _mockRepository.DeleteAsync("A").Returns(true);

            // Act
            var result = await _sut.DeleteAsync("A");

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteAsync("A");
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenRepositoryReturnsFalse()
        {
            // Arrange
            var existing = BuildEntity("A");
            _mockRepository.GetByIdAsync("A").Returns(existing);
            _mockDivisionGradeRepository.ExistsForGradeCodeAsync("A").Returns(false);
            _mockProfitCentreGradeRepository.ExistsForGradeCodeAsync("A").Returns(false);
            _mockWorkGroupGradeRepository.ExistsForGradeCodeAsync("A").Returns(false);
            _mockRepository.DeleteAsync("A").Returns(false);

            // Act
            var result = await _sut.DeleteAsync("A");

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_ThrowsInvalidOperationException_WhenHasDivisionGradeDependents()
        {
            // Arrange
            _mockRepository.GetByIdAsync("A").Returns(BuildEntity("A"));
            _mockDivisionGradeRepository.ExistsForGradeCodeAsync("A").Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteAsync("A"));
            await _mockRepository.DidNotReceive().DeleteAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteAsync_ThrowsInvalidOperationException_WhenHasProfitCentreGradeDependents()
        {
            // Arrange
            _mockRepository.GetByIdAsync("A").Returns(BuildEntity("A"));
            _mockDivisionGradeRepository.ExistsForGradeCodeAsync("A").Returns(false);
            _mockProfitCentreGradeRepository.ExistsForGradeCodeAsync("A").Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteAsync("A"));
            await _mockRepository.DidNotReceive().DeleteAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteAsync_ThrowsInvalidOperationException_WhenHasWorkGroupGradeDependents()
        {
            // Arrange
            _mockRepository.GetByIdAsync("A").Returns(BuildEntity("A"));
            _mockDivisionGradeRepository.ExistsForGradeCodeAsync("A").Returns(false);
            _mockProfitCentreGradeRepository.ExistsForGradeCodeAsync("A").Returns(false);
            _mockWorkGroupGradeRepository.ExistsForGradeCodeAsync("A").Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.DeleteAsync("A"));
            await _mockRepository.DidNotReceive().DeleteAsync(Arg.Any<string>());
        }

        #endregion
    }
}
