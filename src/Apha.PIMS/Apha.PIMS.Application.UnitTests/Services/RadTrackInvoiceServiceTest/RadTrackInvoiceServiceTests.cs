using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Services;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Application.UnitTests.Services.RadTrackInvoiceServiceTest
{
    public class RadTrackInvoiceServiceTests
    {
        private readonly IRadTrackInvoiceRepository _mockRepository;
        private readonly IMapper                    _mockMapper;
        private readonly RadTrackInvoiceService     _sut;

        public RadTrackInvoiceServiceTests()
        {
            _mockRepository = Substitute.For<IRadTrackInvoiceRepository>();
            _mockMapper     = Substitute.For<IMapper>();
            _sut            = new RadTrackInvoiceService(_mockRepository, _mockMapper);
        }

        // ── shared factory helpers ────────────────────────────────────────────────

        /// <summary>Returns a <see cref="RadTrackInvoiceDto"/> that passes all Create validation.</summary>
        private static RadTrackInvoiceDto ValidCreateDto() => new()
        {
            Project   = "PP001",
            Contract  = "C001",
            DueAmount = 5000.00,
            DueDate   = DateTime.Today.AddDays(30)
        };

        /// <summary>Returns a <see cref="RadTrackInvoiceDto"/> that passes all Update validation.</summary>
        private static RadTrackInvoiceDto ValidUpdateDto(int id = 1) => new()
        {
            InvoiceCounter = id,
            Project        = "PP001",
            Contract       = "C001",
            DueAmount      = 5000.00,
            DueDate        = DateTime.Today.AddDays(30)
        };

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WithValidParameters_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query            = new QueryParameters<RadTrackInvoiceFilter> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<RadTrackInvoiceFilter>(page: 1, pageSize: 10);

            var entities      = new List<RadTrackInvoice> { new() { InvoiceCounter = 1, Project = "PP001" } };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var pagedData     = new PagedData<RadTrackInvoice>(entities, paginationData);

            var dtos          = new List<RadTrackInvoiceDto> { new() { InvoiceCounter = 1, Project = "PP001" } };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };

            _mockMapper.Map<PaginationParameters<RadTrackInvoiceFilter>>(query).Returns(paginationParams);
            _mockRepository.GetAllAsync(paginationParams).Returns(pagedData);
            _mockMapper.Map<List<RadTrackInvoiceDto>>(pagedData.Data).Returns(dtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.Data.First().Project.Should().Be("PP001");
            result.PaginationData.TotalRecords.Should().Be(1);

            _mockMapper.Received(1).Map<PaginationParameters<RadTrackInvoiceFilter>>(query);
            await _mockRepository.Received(1).GetAllAsync(paginationParams);
            _mockMapper.Received(1).Map<List<RadTrackInvoiceDto>>(pagedData.Data);
            _mockMapper.Received(1).Map<PaginationDto>(pagedData.PaginationData);
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyResult_ReturnsPaginatedResultWithEmptyData()
        {
            // Arrange
            var query            = new QueryParameters<RadTrackInvoiceFilter> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<RadTrackInvoiceFilter>(page: 1, pageSize: 10);
            var pagedData        = new PagedData<RadTrackInvoice>([], new PaginationData { TotalRecords = 0 });
            var emptyDtos        = new List<RadTrackInvoiceDto>();
            var paginationDto    = new PaginationDto { TotalRecords = 0 };

            _mockMapper.Map<PaginationParameters<RadTrackInvoiceFilter>>(query).Returns(paginationParams);
            _mockRepository.GetAllAsync(paginationParams).Returns(pagedData);
            _mockMapper.Map<List<RadTrackInvoiceDto>>(pagedData.Data).Returns(emptyDtos);
            _mockMapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_WithNullParameters_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetAllAsync(null!));
            exception.Message.Should().Contain("Query parameters must not be null.");
            await _mockRepository.DidNotReceive().GetAllAsync(Arg.Any<PaginationParameters<RadTrackInvoiceFilter>>());
        }

        [Fact]
        public async Task GetAllAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query            = new QueryParameters<RadTrackInvoiceFilter> { Page = 1, PageSize = 10 };
            var paginationParams = new PaginationParameters<RadTrackInvoiceFilter>(page: 1, pageSize: 10);

            _mockMapper.Map<PaginationParameters<RadTrackInvoiceFilter>>(query).Returns(paginationParams);
            _mockRepository.GetAllAsync(paginationParams).Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAllAsync(query));
            exception.Message.Should().Be("Database connection failed");
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsMappedDto()
        {
            // Arrange
            const int id = 1;
            var entity   = new RadTrackInvoice { InvoiceCounter = id, Project = "PP001", InvoiceRef = "INV-001" };
            var dto      = new RadTrackInvoiceDto { InvoiceCounter = id, Project = "PP001", InvoiceRef = "INV-001" };

            _mockRepository.GetByIdAsync(id).Returns(entity);
            _mockMapper.Map<RadTrackInvoiceDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.InvoiceCounter.Should().Be(id);
            result.Project.Should().Be("PP001");
            result.InvoiceRef.Should().Be("INV-001");

            await _mockRepository.Received(1).GetByIdAsync(id);
            _mockMapper.Received(1).Map<RadTrackInvoiceDto>(entity);
        }

        [Fact]
        public async Task GetByIdAsync_WhenInvoiceNotFound_ReturnsNull()
        {
            // Arrange
            const int id = 99;
            _mockRepository.GetByIdAsync(id).Returns((RadTrackInvoice?)null);

            // Act
            var result = await _sut.GetByIdAsync(id);

            // Assert
            result.Should().BeNull();
            await _mockRepository.Received(1).GetByIdAsync(id);
            _mockMapper.DidNotReceive().Map<RadTrackInvoiceDto>(Arg.Any<RadTrackInvoice>());
        }

        [Fact]
        public async Task GetByIdAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            const int id = 1;
            _mockRepository.GetByIdAsync(id).Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetByIdAsync(id));
            exception.Message.Should().Be("Database connection failed");
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidDto_ReturnsMappedCreatedDto()
        {
            // Arrange
            var dto        = ValidCreateDto();
            var entity     = new RadTrackInvoice { Project = "PP001", DueAmount = 5000, DueDate = dto.DueDate };
            var created    = new RadTrackInvoice { InvoiceCounter = 1, Project = "PP001", DueAmount = 5000, DueDate = dto.DueDate };
            var createdDto = new RadTrackInvoiceDto { InvoiceCounter = 1, Project = "PP001", DueAmount = 5000 };

            _mockRepository.ExistsAsync(dto.Project, dto.Contract, dto.InvoiceRef).Returns(false);
            _mockMapper.Map<RadTrackInvoice>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).Returns(created);
            _mockMapper.Map<RadTrackInvoiceDto>(created).Returns(createdDto);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.InvoiceCounter.Should().Be(1);
            result.Project.Should().Be("PP001");

            _mockMapper.Received(1).Map<RadTrackInvoice>(dto);
            await _mockRepository.Received(1).CreateAsync(entity);
            _mockMapper.Received(1).Map<RadTrackInvoiceDto>(created);
        }

        [Fact]
        public async Task CreateAsync_WithNullDto_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(null!));
            exception.Message.Should().Contain("Invoice DTO must not be null.");
            await _mockRepository.DidNotReceive().CreateAsync(Arg.Any<RadTrackInvoice>());
        }

        [Theory]
        [InlineData(null,  "PROJECT_REQUIRED")]
        [InlineData("",    "PROJECT_REQUIRED")]
        [InlineData("   ", "PROJECT_REQUIRED")]
        public async Task CreateAsync_WithMissingProject_ThrowsBusinessValidationErrorException(
            string? project, string expectedCode)
        {
            // Arrange
            var dto = ValidCreateDto();
            dto.Project = project!;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));
            exception.Errors.Should().ContainSingle(e => e.Code == expectedCode);
            await _mockRepository.DidNotReceive().CreateAsync(Arg.Any<RadTrackInvoice>());
        }

        [Fact]
        public async Task CreateAsync_WithNullDueAmount_CreatesInvoiceSuccessfully()
        {
            // Arrange
            var dto = ValidCreateDto();
            dto.DueAmount = null;

            var entity = new RadTrackInvoice { Project = "PP001", DueAmount = null, DueDate = dto.DueDate };
            var created = new RadTrackInvoice { InvoiceCounter = 1, Project = "PP001", DueAmount = null, DueDate = dto.DueDate };
            var createdDto = new RadTrackInvoiceDto { InvoiceCounter = 1, Project = "PP001", DueAmount = null, DueDate = dto.DueDate };

            _mockMapper.Map<RadTrackInvoice>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).Returns(created);
            _mockMapper.Map<RadTrackInvoiceDto>(created).Returns(createdDto);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.InvoiceCounter.Should().Be(1);
            await _mockRepository.Received(1).CreateAsync(entity);
        }

        [Fact]
        public async Task CreateAsync_WithNullDueDate_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = ValidCreateDto();
            dto.DueDate = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));
            exception.Errors.Should().ContainSingle(e => e.Code == "DUE_DATE_REQUIRED");
            await _mockRepository.DidNotReceive().CreateAsync(Arg.Any<RadTrackInvoice>());
        }

        [Fact]
        public async Task CreateAsync_WithMultipleValidationErrors_ThrowsWithAllErrors()
        {
            // Arrange
            var dto = new RadTrackInvoiceDto
            {
                Project   = null,
                DueAmount = null,
                DueDate   = null
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));
            exception.Errors.Should().Contain(e => e.Code == "PROJECT_REQUIRED");
            exception.Errors.Should().Contain(e => e.Code == "DUE_DATE_REQUIRED");
            exception.Errors.Should().HaveCount(2);
            await _mockRepository.DidNotReceive().CreateAsync(Arg.Any<RadTrackInvoice>());
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateInvoiceRef_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = ValidCreateDto();
            dto.InvoiceRef = "INV-001";

            _mockRepository.ExistsAsync(dto.Project, dto.Contract, dto.InvoiceRef).Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));
            exception.Errors.Should().ContainSingle(e => e.Code == "INVOICE_REF_DUPLICATE");
            exception.Errors.First().Message.Should().Be(
                "An invoice with this reference already exists for the selected project and contract.");
            await _mockRepository.DidNotReceive().CreateAsync(Arg.Any<RadTrackInvoice>());
        }

        [Fact]
        public async Task CreateAsync_WithNullInvoiceRef_DoesNotCheckForDuplicates()
        {
            // Arrange
            var dto = ValidCreateDto();
            dto.InvoiceRef = null;

            var entity     = new RadTrackInvoice { Project = "PP001" };
            var created    = new RadTrackInvoice { InvoiceCounter = 1, Project = "PP001" };
            var createdDto = new RadTrackInvoiceDto { InvoiceCounter = 1, Project = "PP001" };

            _mockMapper.Map<RadTrackInvoice>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).Returns(created);
            _mockMapper.Map<RadTrackInvoiceDto>(created).Returns(createdDto);

            // Act
            await _sut.CreateAsync(dto);

            // Assert
            await _mockRepository.DidNotReceive().ExistsAsync(
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>());
        }

        [Fact]
        public async Task CreateAsync_WithEmptyInvoiceRef_DoesNotCheckForDuplicates()
        {
            // Arrange
            var dto = ValidCreateDto();
            dto.InvoiceRef = string.Empty;

            var entity     = new RadTrackInvoice { Project = "PP001" };
            var created    = new RadTrackInvoice { InvoiceCounter = 1, Project = "PP001" };
            var createdDto = new RadTrackInvoiceDto { InvoiceCounter = 1, Project = "PP001" };

            _mockMapper.Map<RadTrackInvoice>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).Returns(created);
            _mockMapper.Map<RadTrackInvoiceDto>(created).Returns(createdDto);

            // Act
            await _sut.CreateAsync(dto);

            // Assert
            await _mockRepository.DidNotReceive().ExistsAsync(
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>());
        }

        [Fact]
        public async Task CreateAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var dto    = ValidCreateDto();
            var entity = new RadTrackInvoice { Project = "PP001" };

            _mockMapper.Map<RadTrackInvoice>(dto).Returns(entity);
            _mockRepository.CreateAsync(entity).Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.CreateAsync(dto));
            exception.Message.Should().Be("Database connection failed");
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidDto_ReturnsMappedUpdatedDto()
        {
            // Arrange
            var dto      = ValidUpdateDto(id: 1);
            var existing = new RadTrackInvoice { InvoiceCounter = 1, Project = "PP001" };
            var updated  = new RadTrackInvoice { InvoiceCounter = 1, Project = "PP001", DueAmount = 5000 };
            var result   = new RadTrackInvoiceDto { InvoiceCounter = 1, Project = "PP001", DueAmount = 5000 };

            _mockRepository.GetByIdAsync(dto.InvoiceCounter).Returns(existing);
            _mockRepository.ExistsAsync(
                dto.Project, dto.Contract, dto.InvoiceRef,
                excludeInvoiceCounter: dto.InvoiceCounter).Returns(false);
            _mockRepository.UpdateAsync(existing).Returns(updated);
            _mockMapper.Map<RadTrackInvoiceDto>(updated).Returns(result);

            // Act
            var actual = await _sut.UpdateAsync(dto);

            // Assert
            actual.Should().NotBeNull();
            actual.InvoiceCounter.Should().Be(1);
            actual.Project.Should().Be("PP001");

            await _mockRepository.Received(1).GetByIdAsync(dto.InvoiceCounter);
            await _mockRepository.Received(1).UpdateAsync(existing);
            _mockMapper.Received(1).Map<RadTrackInvoiceDto>(updated);
        }

        [Fact]
        public async Task UpdateAsync_MapsSourceDtoOntoExistingEntity_BeforeCallingRepository()
        {
            // Arrange
            var dto      = ValidUpdateDto(id: 1);
            var existing = new RadTrackInvoice { InvoiceCounter = 1, Project = "OLD" };
            var updated  = new RadTrackInvoice { InvoiceCounter = 1, Project = "PP001" };
            var resultDto = new RadTrackInvoiceDto { InvoiceCounter = 1 };

            _mockRepository.GetByIdAsync(dto.InvoiceCounter).Returns(existing);
            _mockRepository.UpdateAsync(existing).Returns(updated);
            _mockMapper.Map<RadTrackInvoiceDto>(updated).Returns(resultDto);

            // Act
            await _sut.UpdateAsync(dto);

            // Assert — two-argument Map (dto → existing) must be called
            _mockMapper.Received(1).Map(dto, existing);
        }

        [Fact]
        public async Task UpdateAsync_WithNullDto_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAsync(null!));
            exception.Message.Should().Contain("Invoice DTO must not be null.");
            await _mockRepository.DidNotReceive().GetByIdAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task UpdateAsync_WithZeroInvoiceCounter_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = ValidUpdateDto();
            dto.InvoiceCounter = 0;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));
            exception.Errors.Should().ContainSingle(e => e.Code == "INVOICE_COUNTER_REQUIRED");
            await _mockRepository.DidNotReceive().GetByIdAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task UpdateAsync_WithNegativeInvoiceCounter_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = ValidUpdateDto();
            dto.InvoiceCounter = -1;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));
            exception.Errors.Should().ContainSingle(e => e.Code == "INVOICE_COUNTER_REQUIRED");
            await _mockRepository.DidNotReceive().GetByIdAsync(Arg.Any<int>());
        }

        [Theory]
        [InlineData(null,  "PROJECT_REQUIRED")]
        [InlineData("",    "PROJECT_REQUIRED")]
        [InlineData("   ", "PROJECT_REQUIRED")]
        public async Task UpdateAsync_WithMissingProject_ThrowsBusinessValidationErrorException(
            string? project, string expectedCode)
        {
            // Arrange
            var dto = ValidUpdateDto();
            dto.Project = project!;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));
            exception.Errors.Should().ContainSingle(e => e.Code == expectedCode);
            await _mockRepository.DidNotReceive().GetByIdAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task UpdateAsync_WithNullDueAmount_UpdatesInvoiceSuccessfully()
        {
            // Arrange
            var dto = ValidUpdateDto();
            dto.DueAmount = null;

            var existing = new RadTrackInvoice { InvoiceCounter = dto.InvoiceCounter, Project = "PP001" };
            var updated = new RadTrackInvoice { InvoiceCounter = dto.InvoiceCounter, Project = "PP001", DueAmount = null, DueDate = dto.DueDate };
            var resultDto = new RadTrackInvoiceDto { InvoiceCounter = dto.InvoiceCounter, Project = "PP001", DueAmount = null, DueDate = dto.DueDate };

            _mockRepository.GetByIdAsync(dto.InvoiceCounter).Returns(existing);
            _mockRepository.UpdateAsync(existing).Returns(updated);
            _mockMapper.Map<RadTrackInvoiceDto>(updated).Returns(resultDto);

            // Act
            var result = await _sut.UpdateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.InvoiceCounter.Should().Be(dto.InvoiceCounter);
            await _mockRepository.Received(1).UpdateAsync(existing);
        }

        [Fact]
        public async Task UpdateAsync_WithNullDueDate_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = ValidUpdateDto();
            dto.DueDate = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));
            exception.Errors.Should().ContainSingle(e => e.Code == "DUE_DATE_REQUIRED");
        }

        [Fact]
        public async Task UpdateAsync_WithMultipleValidationErrors_ThrowsWithAllErrors()
        {
            // Arrange
            var dto = new RadTrackInvoiceDto
            {
                InvoiceCounter = 0,
                Project        = null,
                DueAmount      = null,
                DueDate        = null
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));
            exception.Errors.Should().Contain(e => e.Code == "INVOICE_COUNTER_REQUIRED");
            exception.Errors.Should().Contain(e => e.Code == "PROJECT_REQUIRED");
            exception.Errors.Should().Contain(e => e.Code == "DUE_DATE_REQUIRED");
            exception.Errors.Should().HaveCount(3);
        }

        [Fact]
        public async Task UpdateAsync_WhenInvoiceNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = ValidUpdateDto(id: 99);
            _mockRepository.GetByIdAsync(dto.InvoiceCounter).Returns((RadTrackInvoice?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAsync(dto));
            exception.Message.Should().Contain("99");
            await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<RadTrackInvoice>());
        }

        [Fact]
        public async Task UpdateAsync_WithDuplicateInvoiceRef_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto      = ValidUpdateDto(id: 1);
            dto.InvoiceRef = "INV-DUP";
            var existing = new RadTrackInvoice { InvoiceCounter = 1, Project = "PP001" };

            _mockRepository.GetByIdAsync(dto.InvoiceCounter).Returns(existing);
            _mockRepository.ExistsAsync(dto.Project, dto.Contract, dto.InvoiceRef,
                excludeInvoiceCounter: dto.InvoiceCounter).Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));
            exception.Errors.Should().ContainSingle(e => e.Code == "INVOICE_REF_DUPLICATE");
            exception.Errors.First().Message.Should().Be(
                "An invoice with this reference already exists for the selected project and contract.");
            await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<RadTrackInvoice>());
        }

        [Fact]
        public async Task UpdateAsync_WithNullInvoiceRef_DoesNotCheckForDuplicates()
        {
            // Arrange
            var dto      = ValidUpdateDto(id: 1);
            dto.InvoiceRef = null;
            var existing = new RadTrackInvoice { InvoiceCounter = 1, Project = "PP001" };
            var updated  = new RadTrackInvoice { InvoiceCounter = 1, Project = "PP001" };
            var resultDto = new RadTrackInvoiceDto { InvoiceCounter = 1 };

            _mockRepository.GetByIdAsync(dto.InvoiceCounter).Returns(existing);
            _mockRepository.UpdateAsync(existing).Returns(updated);
            _mockMapper.Map<RadTrackInvoiceDto>(updated).Returns(resultDto);

            // Act
            await _sut.UpdateAsync(dto);

            // Assert
            await _mockRepository.DidNotReceive().ExistsAsync(
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>());
        }

        [Fact]
        public async Task UpdateAsync_WithEmptyInvoiceRef_DoesNotCheckForDuplicates()
        {
            // Arrange
            var dto      = ValidUpdateDto(id: 1);
            dto.InvoiceRef = string.Empty;
            var existing = new RadTrackInvoice { InvoiceCounter = 1, Project = "PP001" };
            var updated  = new RadTrackInvoice { InvoiceCounter = 1, Project = "PP001" };
            var resultDto = new RadTrackInvoiceDto { InvoiceCounter = 1 };

            _mockRepository.GetByIdAsync(dto.InvoiceCounter).Returns(existing);
            _mockRepository.UpdateAsync(existing).Returns(updated);
            _mockMapper.Map<RadTrackInvoiceDto>(updated).Returns(resultDto);

            // Act
            await _sut.UpdateAsync(dto);

            // Assert
            await _mockRepository.DidNotReceive().ExistsAsync(
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>());
        }

        [Fact]
        public async Task UpdateAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var dto      = ValidUpdateDto(id: 1);
            var existing = new RadTrackInvoice { InvoiceCounter = 1, Project = "PP001" };

            _mockRepository.GetByIdAsync(dto.InvoiceCounter).Returns(existing);
            _mockRepository.UpdateAsync(existing).Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.UpdateAsync(dto));
            exception.Message.Should().Be("Database connection failed");
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenInvoiceExists_ReturnsTrue()
        {
            // Arrange
            const int id = 1;
            _mockRepository.DeleteAsync(id).Returns(true);

            // Act
            var result = await _sut.DeleteAsync(id);

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteAsync(id);
        }

        [Fact]
        public async Task DeleteAsync_WhenInvoiceNotFound_ReturnsFalse()
        {
            // Arrange
            const int id = 99;
            _mockRepository.DeleteAsync(id).Returns(false);

            // Act
            var result = await _sut.DeleteAsync(id);

            // Assert
            result.Should().BeFalse();
            await _mockRepository.Received(1).DeleteAsync(id);
        }

        [Fact]
        public async Task DeleteAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            const int id = 1;
            _mockRepository.DeleteAsync(id).Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.DeleteAsync(id));
            exception.Message.Should().Be("Database connection failed");
        }

        #endregion

        #region GetTotalsAsync Tests

        [Fact]
        public async Task GetTotalsAsync_WithFilter_ReturnsMappedTotalsDto()
        {
            // Arrange
            var filter  = new RadTrackInvoiceFilter { Project = "PP001", Year = 2024 };
            var totals  = new RadTrackInvoiceTotals { TotalPlannedAmount = 10000, TotalDueAmount = 8000, TotalActualAmount = 7500 };
            var dto     = new RadTrackInvoiceTotalsDto { TotalPlannedAmount = 10000, TotalDueAmount = 8000, TotalActualAmount = 7500 };

            _mockRepository.GetTotalsAsync(filter).Returns(totals);
            _mockMapper.Map<RadTrackInvoiceTotalsDto>(totals).Returns(dto);

            // Act
            var result = await _sut.GetTotalsAsync(filter);

            // Assert
            result.Should().NotBeNull();
            result.TotalPlannedAmount.Should().Be(10000);
            result.TotalDueAmount.Should().Be(8000);
            result.TotalActualAmount.Should().Be(7500);

            await _mockRepository.Received(1).GetTotalsAsync(filter);
            _mockMapper.Received(1).Map<RadTrackInvoiceTotalsDto>(totals);
        }

        [Fact]
        public async Task GetTotalsAsync_WithNullFilter_ReturnsMappedTotalsDto()
        {
            // Arrange
            var totals = new RadTrackInvoiceTotals { TotalPlannedAmount = 5000, TotalDueAmount = 4000, TotalActualAmount = 3000 };
            var dto    = new RadTrackInvoiceTotalsDto { TotalPlannedAmount = 5000, TotalDueAmount = 4000, TotalActualAmount = 3000 };

            _mockRepository.GetTotalsAsync(null).Returns(totals);
            _mockMapper.Map<RadTrackInvoiceTotalsDto>(totals).Returns(dto);

            // Act
            var result = await _sut.GetTotalsAsync(null);

            // Assert
            result.Should().NotBeNull();
            result.TotalPlannedAmount.Should().Be(5000);
            await _mockRepository.Received(1).GetTotalsAsync(null);
            _mockMapper.Received(1).Map<RadTrackInvoiceTotalsDto>(totals);
        }

        [Fact]
        public async Task GetTotalsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var filter = new RadTrackInvoiceFilter { Project = "PP001" };
            _mockRepository.GetTotalsAsync(filter).Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetTotalsAsync(filter));
            exception.Message.Should().Be("Database connection failed");
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_WhenInvoiceExists_ReturnsTrue()
        {
            // Arrange
            _mockRepository.ExistsAsync("PP001", "C001", "INV-001", null).Returns(true);

            // Act
            var result = await _sut.ExistsAsync("PP001", "C001", "INV-001");

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).ExistsAsync("PP001", "C001", "INV-001", null);
        }

        [Fact]
        public async Task ExistsAsync_WhenInvoiceDoesNotExist_ReturnsFalse()
        {
            // Arrange
            _mockRepository.ExistsAsync("PP001", "C001", "INV-999", null).Returns(false);

            // Act
            var result = await _sut.ExistsAsync("PP001", "C001", "INV-999");

            // Assert
            result.Should().BeFalse();
            await _mockRepository.Received(1).ExistsAsync("PP001", "C001", "INV-999", null);
        }

        [Fact]
        public async Task ExistsAsync_WithExcludeCounter_PassesCounterToRepository()
        {
            // Arrange
            const int excludeId = 5;
            _mockRepository.ExistsAsync("PP001", "C001", "INV-001", excludeId).Returns(false);

            // Act
            var result = await _sut.ExistsAsync("PP001", "C001", "INV-001", excludeId);

            // Assert
            result.Should().BeFalse();
            await _mockRepository.Received(1).ExistsAsync("PP001", "C001", "INV-001", excludeId);
        }

        [Fact]
        public async Task ExistsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.ExistsAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>())
                .Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.ExistsAsync("PP001", "C001", "INV-001"));
            exception.Message.Should().Be("Database connection failed");
        }

        #endregion

        #region GetProjectsAsync Tests

        [Fact]
        public async Task GetProjectsAsync_WithData_ReturnsListOfProjects()
        {
            // Arrange
            var expected = new List<string> { "PP001", "PP002", "PP003" };
            _mockRepository.GetProjectsAsync().Returns(expected);

            // Act
            var result = await _sut.GetProjectsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expected);
            await _mockRepository.Received(1).GetProjectsAsync();
        }

        [Fact]
        public async Task GetProjectsAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.GetProjectsAsync().Returns(new List<string>());

            // Act
            var result = await _sut.GetProjectsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetProjectsAsync();
        }

        [Fact]
        public async Task GetProjectsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetProjectsAsync().Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetProjectsAsync());
            exception.Message.Should().Be("Database connection failed");
        }

        #endregion

        #region GetYearsAsync Tests

        [Fact]
        public async Task GetYearsAsync_WithData_ReturnsListOfYears()
        {
            // Arrange
            var expected = new List<int> { 2022, 2023, 2024 };
            _mockRepository.GetYearsAsync().Returns(expected);

            // Act
            var result = await _sut.GetYearsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expected);
            await _mockRepository.Received(1).GetYearsAsync();
        }

        [Fact]
        public async Task GetYearsAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.GetYearsAsync().Returns(new List<int>());

            // Act
            var result = await _sut.GetYearsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetYearsAsync();
        }

        [Fact]
        public async Task GetYearsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetYearsAsync().Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetYearsAsync());
            exception.Message.Should().Be("Database connection failed");
        }

        #endregion

        #region GetContractsAsync Tests

        [Fact]
        public async Task GetContractsAsync_WithData_ReturnsListOfContracts()
        {
            // Arrange
            var expected = new List<string> { "C001", "C002", "C003" };
            _mockRepository.GetContractsAsync().Returns(expected);

            // Act
            var result = await _sut.GetContractsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expected);
            await _mockRepository.Received(1).GetContractsAsync();
        }

        [Fact]
        public async Task GetContractsAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.GetContractsAsync().Returns(new List<string>());

            // Act
            var result = await _sut.GetContractsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetContractsAsync();
        }

        [Fact]
        public async Task GetContractsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetContractsAsync().Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetContractsAsync());
            exception.Message.Should().Be("Database connection failed");
        }

        #endregion

        #region GetProgramsAsync Tests

        [Fact]
        public async Task GetProgramsAsync_WithData_ReturnsListOfPrograms()
        {
            // Arrange
            var expected = new List<string> { "PROG1", "PROG2", "PROG3" };
            _mockRepository.GetProgramsAsync().Returns(expected);

            // Act
            var result = await _sut.GetProgramsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expected);
            await _mockRepository.Received(1).GetProgramsAsync();
        }

        [Fact]
        public async Task GetProgramsAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.GetProgramsAsync().Returns(new List<string>());

            // Act
            var result = await _sut.GetProgramsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetProgramsAsync();
        }

        [Fact]
        public async Task GetProgramsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetProgramsAsync().Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetProgramsAsync());
            exception.Message.Should().Be("Database connection failed");
        }

        #endregion
    }
}
