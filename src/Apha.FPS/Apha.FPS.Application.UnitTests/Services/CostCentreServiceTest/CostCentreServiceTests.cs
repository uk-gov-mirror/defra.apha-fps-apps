using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Npgsql;
using Xunit;

namespace Apha.FPS.Application.UnitTests.Services.CostCentreServiceTest
{
    public class CostCentreServiceTests
    {
        private readonly ICostCentreRepository _mockRepository;
        private readonly IProfitCentreRepository _mockProfitCentreRepository;
        private readonly IMapper _mockMapper;
        private readonly CostCentreService _sut;

        public CostCentreServiceTests()
        {
            _mockRepository              = Substitute.For<ICostCentreRepository>();
            _mockProfitCentreRepository  = Substitute.For<IProfitCentreRepository>();
            _mockMapper                  = Substitute.For<IMapper>();
            _sut = new CostCentreService(_mockRepository, _mockProfitCentreRepository, _mockMapper);
        }

        private static CostCentreDto BuildDto(double no = 100.0, string pc = "PC01", int year = 2024) =>
            new() { CostCentreNo = no, ProfitCentre = pc, FpsYear = year };

        private static CostCentre BuildEntity(double no = 100.0, string pc = "PC01", int year = 2024) =>
            new() { CostCentreNo = no, ProfitCentre = pc, FpsYear = year };

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            var service = new CostCentreService(_mockRepository, _mockProfitCentreRepository, _mockMapper);
            Assert.NotNull(service);
        }

        #endregion

        #region GetAllCostCentresPagedAsync Tests

        [Fact]
        public async Task GetAllCostCentresPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.GetAllCostCentresPagedAsync(null!));
        }

        [Fact]
        public async Task GetAllCostCentresPagedAsync_ReturnsPaginatedResult()
        {
            // Arrange
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData    = new PagedData<CostCentre>
            {
                Data           = new List<CostCentre> { BuildEntity() },
                PaginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var pagedResult  = new PaginatedResult<CostCentreDto>
            {
                Data           = new List<CostCentreDto> { BuildDto() },
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAllPagedAsync(mappedParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<CostCentreDto>>(pagedData).Returns(pagedResult);

            // Act
            var result = await _sut.GetAllCostCentresPagedAsync(query);

            // Assert
            result.Should().Be(pagedResult);
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetAllPagedAsync(mappedParams);
        }

        [Fact]
        public async Task GetAllCostCentresPagedAsync_ReturnsEmptyResult_WhenNoData()
        {
            // Arrange
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string>();
            var pagedData    = new PagedData<CostCentre>
            {
                Data           = [],
                PaginationData = new PaginationData { TotalRecords = 0 }
            };
            var emptyResult  = new PaginatedResult<CostCentreDto>
            {
                Data           = [],
                PaginationData = new PaginationDto { TotalRecords = 0 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAllPagedAsync(mappedParams).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<CostCentreDto>>(pagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetAllCostCentresPagedAsync(query);

            // Assert
            result.Data.Should().BeEmpty();
        }

        #endregion

        #region GetCostCentreByIdAsync Tests

        [Fact]
        public async Task GetCostCentreByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            _mockRepository.GetByIdAsync(999.0, 2024).Returns((CostCentre?)null);

            // Act
            var result = await _sut.GetCostCentreByIdAsync(999.0, 2024);

            // Assert
            result.Should().BeNull();
            await _mockRepository.Received(1).GetByIdAsync(999.0, 2024);
        }

        [Fact]
        public async Task GetCostCentreByIdAsync_ReturnsMappedDto_WhenFound()
        {
            // Arrange
            var entity = BuildEntity(100.0, "PC01", 2024);
            var dto    = BuildDto(100.0, "PC01", 2024);

            _mockRepository.GetByIdAsync(100.0, 2024).Returns(entity);
            _mockMapper.Map<CostCentreDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetCostCentreByIdAsync(100.0, 2024);

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).GetByIdAsync(100.0, 2024);
            _mockMapper.Received(1).Map<CostCentreDto>(entity);
        }

        #endregion

        #region CreateCostCentreAsync Tests

        [Fact]
        public async Task CreateCostCentreAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.CreateCostCentreAsync(null!));
        }

        [Fact]
        public async Task CreateCostCentreAsync_ThrowsArgumentException_WhenProfitCentreIsEmpty()
        {
            var dto = new CostCentreDto { CostCentreNo = 100, ProfitCentre = "", FpsYear = 2024 };
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateCostCentreAsync(dto));
        }

        [Fact]
        public async Task CreateCostCentreAsync_ThrowsArgumentException_WhenProfitCentreIsWhiteSpace()
        {
            var dto = new CostCentreDto { CostCentreNo = 100, ProfitCentre = "   ", FpsYear = 2024 };
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateCostCentreAsync(dto));
        }

        [Fact]
        public async Task CreateCostCentreAsync_ThrowsInvalidOperationException_WhenCompositeKeyAlreadyExists()
        {
            // Arrange
            var dto = BuildDto(100.0, "PC01", 2024);
            _mockRepository.ExistsAsync(100.0, 2024).Returns(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateCostCentreAsync(dto));
            await _mockRepository.DidNotReceive().CreateAsync(Arg.Any<CostCentre>());
        }

        [Fact]
        public async Task CreateCostCentreAsync_ThrowsInvalidOperationException_WhenProfitCentreDoesNotExist()
        {
            // Arrange
            var dto = BuildDto(100.0, "PC_INVALID", 2024);
            _mockRepository.ExistsAsync(100.0, 2024).Returns(false);
            _mockProfitCentreRepository.ProfitCentreExistsAsync("PC_INVALID").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.CreateCostCentreAsync(dto));
            await _mockRepository.DidNotReceive().CreateAsync(Arg.Any<CostCentre>());
        }

        [Fact]
        public async Task CreateCostCentreAsync_ReturnsMappedDto_WhenSuccessful()
        {
            // Arrange
            var dto     = BuildDto(100.0, "PC01", 2024);
            var entity  = BuildEntity(100.0, "PC01", 2024);
            var created = BuildEntity(100.0, "PC01", 2024);

            _mockRepository.ExistsAsync(100.0, 2024).Returns(false);
            _mockProfitCentreRepository.ProfitCentreExistsAsync("PC01").Returns(true);
            _mockMapper.Map<CostCentre>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).Returns(created);
            _mockMapper.Map<CostCentreDto>(created).Returns(dto);

            // Act
            var result = await _sut.CreateCostCentreAsync(dto);

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).ExistsAsync(100.0, 2024);
            await _mockProfitCentreRepository.Received(1).ProfitCentreExistsAsync("PC01");
            await _mockRepository.Received(1).CreateAsync(entity);
        }

        #endregion

        #region UpdateCostCentreAsync Tests

        [Fact]
        public async Task UpdateCostCentreAsync_ThrowsArgumentNullException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _sut.UpdateCostCentreAsync(100.0, 2024, null!));
        }

        [Fact]
        public async Task UpdateCostCentreAsync_ThrowsArgumentException_WhenProfitCentreIsEmpty()
        {
            var dto = new CostCentreDto { CostCentreNo = 100, ProfitCentre = "", FpsYear = 2024 };
            await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateCostCentreAsync(100.0, 2024, dto));
        }

        [Fact]
        public async Task UpdateCostCentreAsync_ThrowsKeyNotFoundException_WhenOriginalRecordDoesNotExist()
        {
            // Arrange
            var dto = BuildDto(100.0, "PC01", 2024);
            _mockRepository.ExistsAsync(999.0, 2024).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateCostCentreAsync(999.0, 2024, dto));
            await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<double>(), Arg.Any<int>(), Arg.Any<CostCentre>());
        }

        [Fact]
        public async Task UpdateCostCentreAsync_ThrowsInvalidOperationException_WhenProfitCentreDoesNotExist()
        {
            // Arrange
            var dto = BuildDto(100.0, "PC_INVALID", 2024);
            _mockRepository.ExistsAsync(100.0, 2024).Returns(true);
            _mockProfitCentreRepository.ProfitCentreExistsAsync("PC_INVALID").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.UpdateCostCentreAsync(100.0, 2024, dto));
            await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<double>(), Arg.Any<int>(), Arg.Any<CostCentre>());
        }

        [Fact]
        public async Task UpdateCostCentreAsync_ReturnsMappedDto_WhenSuccessful()
        {
            // Arrange
            var dto     = BuildDto(100.0, "PC02", 2024);
            var entity  = BuildEntity(100.0, "PC02", 2024);
            var updated = BuildEntity(100.0, "PC02", 2024);

            _mockRepository.ExistsAsync(100.0, 2024).Returns(true);
            _mockProfitCentreRepository.ProfitCentreExistsAsync("PC02").Returns(true);
            _mockMapper.Map<CostCentre>(dto).Returns(entity);
            _mockRepository.UpdateAsync(100.0, 2024, entity).Returns(updated);
            _mockMapper.Map<CostCentreDto>(updated).Returns(dto);

            // Act
            var result = await _sut.UpdateCostCentreAsync(100.0, 2024, dto);

            // Assert
            result.Should().Be(dto);
            await _mockRepository.Received(1).UpdateAsync(100.0, 2024, entity);
        }

        [Fact]
        public async Task UpdateCostCentreAsync_ThrowsBusinessValidationError_WhenWorkgroupFkViolation()
        {
            // Arrange
            var dto    = BuildDto(100.0, "PC02", 2024);
            var entity = BuildEntity(100.0, "PC02", 2024);

            _mockRepository.ExistsAsync(100.0, 2024).Returns(true);
            _mockProfitCentreRepository.ProfitCentreExistsAsync("PC02").Returns(true);
            _mockMapper.Map<CostCentre>(dto).Returns(entity);
            _mockRepository.UpdateAsync(100.0, 2024, entity)
                .ThrowsAsync(new Exception("db error", BuildFkViolation("fk_workgroup_costcentre_10")));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.UpdateCostCentreAsync(100.0, 2024, dto));
            var error = Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_FK_VIOLATION", error.Code);
            Assert.Contains("cannot be edited", error.Message);
        }

        [Fact]
        public async Task UpdateCostCentreAsync_PropagatesOriginalException_WhenUnrelatedFkViolation()
        {
            // Arrange
            var dto      = BuildDto(100.0, "PC02", 2024);
            var entity   = BuildEntity(100.0, "PC02", 2024);
            var original = new Exception("db error", BuildFkViolation("fk_some_other_constraint"));

            _mockRepository.ExistsAsync(100.0, 2024).Returns(true);
            _mockProfitCentreRepository.ProfitCentreExistsAsync("PC02").Returns(true);
            _mockMapper.Map<CostCentre>(dto).Returns(entity);
            _mockRepository.UpdateAsync(100.0, 2024, entity).ThrowsAsync(original);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.UpdateCostCentreAsync(100.0, 2024, dto));
            Assert.Same(original, ex);
        }

        #endregion

        #region DeleteCostCentreAsync Tests

        [Fact]
        public async Task DeleteCostCentreAsync_ThrowsKeyNotFoundException_WhenRecordDoesNotExist()
        {
            // Arrange
            _mockRepository.ExistsAsync(999.0, 2024).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.DeleteCostCentreAsync(999.0, 2024));
            await _mockRepository.DidNotReceive().DeleteAsync(Arg.Any<double>(), Arg.Any<int>());
        }

        [Fact]
        public async Task DeleteCostCentreAsync_ReturnsTrue_WhenSuccessful()
        {
            // Arrange
            _mockRepository.ExistsAsync(100.0, 2024).Returns(true);
            _mockRepository.DeleteAsync(100.0, 2024).Returns(true);

            // Act
            var result = await _sut.DeleteCostCentreAsync(100.0, 2024);

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteAsync(100.0, 2024);
        }

        [Fact]
        public async Task DeleteCostCentreAsync_ReturnsFalse_WhenRepositoryReturnsFalse()
        {
            // Arrange
            _mockRepository.ExistsAsync(100.0, 2024).Returns(true);
            _mockRepository.DeleteAsync(100.0, 2024).Returns(false);

            // Act
            var result = await _sut.DeleteCostCentreAsync(100.0, 2024);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteCostCentreAsync_ThrowsBusinessValidationError_WhenWorkgroupFkViolation()
        {
            // Arrange
            _mockRepository.ExistsAsync(100.0, 2024).Returns(true);
            _mockRepository.DeleteAsync(100.0, 2024)
                .ThrowsAsync(new Exception("db error", BuildFkViolation("fk_workgroup_costcentre_10")));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.DeleteCostCentreAsync(100.0, 2024));
            var error = Assert.Single(ex.Errors);
            Assert.Equal("WORKGROUP_FK_VIOLATION", error.Code);
            Assert.Contains("cannot be deleted", error.Message);
        }

        [Fact]
        public async Task DeleteCostCentreAsync_PropagatesOriginalException_WhenUnrelatedFkViolation()
        {
            // Arrange
            var original = new Exception("db error", BuildFkViolation("fk_some_other_constraint"));
            _mockRepository.ExistsAsync(100.0, 2024).Returns(true);
            _mockRepository.DeleteAsync(100.0, 2024).ThrowsAsync(original);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _sut.DeleteCostCentreAsync(100.0, 2024));
            Assert.Same(original, ex);
        }

        #endregion

        // Builds a PostgresException carrying a foreign-key violation (SqlState 23503) for the
        // given constraint name, mimicking how Npgsql surfaces DB FK violations.
        private static PostgresException BuildFkViolation(string constraintName) =>
            new(
                messageText: "foreign key violation",
                severity: "ERROR",
                invariantSeverity: "ERROR",
                sqlState: PostgresErrorCodes.ForeignKeyViolation,
                detail: null,
                hint: null,
                position: 0,
                internalPosition: 0,
                internalQuery: null,
                where: null,
                schemaName: null,
                tableName: null,
                columnName: null,
                dataTypeName: null,
                constraintName: constraintName);
    }
}
