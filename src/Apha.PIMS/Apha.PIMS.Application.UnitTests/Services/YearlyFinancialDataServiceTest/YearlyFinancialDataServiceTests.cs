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

namespace Apha.PIMS.Application.UnitTests.Services.YearlyFinancialDataServiceTest
{
    public class YearlyFinancialDataServiceTests
    {
        private readonly IYearlyFinancialDataRepository _repository;
        private readonly IMapper                        _mapper;
        private readonly YearlyFinancialDataService     _sut;

        public YearlyFinancialDataServiceTests()
        {
            _repository = Substitute.For<IYearlyFinancialDataRepository>();
            _mapper     = Substitute.For<IMapper>();
            _repository.GetPactCostsAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(new List<PactProjectYearCosts>().AsReadOnly());
            _sut        = new YearlyFinancialDataService(_repository, _mapper);
        }

        // ── shared factory helpers ────────────────────────────────────────

        private static YearlyFinancialDataDto ValidCreateDto() => new()
        {
            Year    = 2024,
            Project = "PP001",
            BfBudget = 10000m
        };

        private static YearlyFinancialDataDto ValidUpdateDto(short year = 2024, string project = "PP001") => new()
        {
            Year    = year,
            Project = project,
            BfBudget = 12000m
        };

        private static YearlyFinancialData EntityFor(short year = 2024, string project = "PP001")
            => new() { Year = year, Project = project };

        private static IReadOnlyList<PactProjectYearCosts> PactRows(params PactProjectYearCosts[] rows)
            => rows.ToList().AsReadOnly();

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRepository_ThrowsArgumentNullException()
        {
            var act = () => new YearlyFinancialDataService(null!, _mapper);
            act.Should().Throw<ArgumentNullException>().WithParameterName("repository");
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            var act = () => new YearlyFinancialDataService(_repository, null!);
            act.Should().Throw<ArgumentNullException>().WithParameterName("mapper");
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WithValidParameters_ReturnsMappedPaginatedResult()
        {
            // Arrange
            const string project      = "PP001";
            var query                 = new QueryParameters<string> { Page = 1, PageSize = 10, Filter = project };
            var paginationParams      = new PaginationParameters<string>(page: 1, pageSize: 10);
            var entities              = new List<YearlyFinancialData> { EntityFor() };
            var paginationData        = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };
            var pagedData             = new PagedData<YearlyFinancialData>(entities, paginationData);
            var dtos                  = new List<YearlyFinancialDataDto> { new() { Year = 2024, Project = project } };
            var paginationDto         = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 1 };

            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetAllAsync(project, paginationParams).Returns(pagedData);
            _mapper.Map<List<YearlyFinancialDataDto>>(pagedData.Data).Returns(dtos);
            _mapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

            // Act
            var result = await _sut.GetAllAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.Data.First().Project.Should().Be(project);
            result.PaginationData.TotalRecords.Should().Be(1);
            _mapper.Received(1).Map<PaginationParameters<string>>(query);
            await _repository.Received(1).GetAllAsync(project, paginationParams);
            _mapper.Received(1).Map<List<YearlyFinancialDataDto>>(pagedData.Data);
            _mapper.Received(1).Map<PaginationDto>(pagedData.PaginationData);
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyResult_ReturnsPaginatedResultWithEmptyData()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10, Filter = "PP001" };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var pagedData        = new PagedData<YearlyFinancialData>([], new PaginationData { TotalRecords = 0 });
            var emptyDtos        = new List<YearlyFinancialDataDto>();
            var paginationDto    = new PaginationDto { TotalRecords = 0 };

            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetAllAsync(Arg.Any<string>(), paginationParams).Returns(pagedData);
            _mapper.Map<List<YearlyFinancialDataDto>>(pagedData.Data).Returns(emptyDtos);
            _mapper.Map<PaginationDto>(pagedData.PaginationData).Returns(paginationDto);

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
            await _repository.DidNotReceive().GetAllAsync(Arg.Any<string>(), Arg.Any<PaginationParameters<string>>());
        }

        [Fact]
        public async Task GetAllAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10, Filter = "PP001" };
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            _mapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _repository.GetAllAsync(Arg.Any<string>(), paginationParams).Throws(new Exception("DB error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAllAsync(query));
            exception.Message.Should().Be("DB error");
        }

        #endregion

        #region GetByKeyAsync Tests

        [Fact]
        public async Task GetByKeyAsync_WithValidKey_ReturnsMappedDto()
        {
            // Arrange
            const short year     = 2024;
            const string project = "PP001";
            var entity           = EntityFor(year, project);
            var dto              = ValidCreateDto();

            _repository.GetByKeyAsync(year, project).Returns(entity);
            _mapper.Map<YearlyFinancialDataDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetByKeyAsync(year, project);

            // Assert
            result.Should().NotBeNull();
            result!.Year.Should().Be(dto.Year);
            await _repository.Received(1).GetByKeyAsync(year, project);
            _mapper.Received(1).Map<YearlyFinancialDataDto>(entity);
        }

        [Fact]
        public async Task GetByKeyAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            _repository.GetByKeyAsync(Arg.Any<short>(), Arg.Any<string>()).Returns((YearlyFinancialData?)null);

            // Act
            var result = await _sut.GetByKeyAsync(9999, "UNKNOWN");

            // Assert
            result.Should().BeNull();
            _mapper.DidNotReceive().Map<YearlyFinancialDataDto>(Arg.Any<YearlyFinancialData>());
        }

        [Fact]
        public async Task GetByKeyAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _repository.GetByKeyAsync(Arg.Any<short>(), Arg.Any<string>())
                       .Throws(new Exception("DB error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetByKeyAsync(2024, "PP001"));
            exception.Message.Should().Be("DB error");
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidDto_ReturnsMappedCreatedDto()
        {
            // Arrange
            var dto        = ValidCreateDto();
            var entity     = EntityFor();
            var created    = EntityFor();
            var createdDto = ValidCreateDto();

            _repository.ExistsAsync(dto.Year, dto.Project!).Returns(false);
            _mapper.Map<YearlyFinancialData>(dto).Returns(entity);
            _repository.CreateAsync(entity).Returns(created);
            _mapper.Map<YearlyFinancialDataDto>(created).Returns(createdDto);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Project.Should().Be("PP001");
            _mapper.Received(1).Map<YearlyFinancialData>(dto);
            await _repository.Received(1).CreateAsync(entity);
            _mapper.Received(1).Map<YearlyFinancialDataDto>(created);
        }

        [Fact]
        public async Task CreateAsync_WithNullDto_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.CreateAsync(null!));
            exception.Message.Should().Contain("YearlyFinancialData DTO must not be null.");
            await _repository.DidNotReceive().CreateAsync(Arg.Any<YearlyFinancialData>());
        }

        [Fact]
        public async Task CreateAsync_WithZeroYear_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = ValidCreateDto();
            dto.Year = 0;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));
            exception.Errors.Should().ContainSingle(e => e.Code == "YEAR_REQUIRED");
            await _repository.DidNotReceive().CreateAsync(Arg.Any<YearlyFinancialData>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task CreateAsync_WithMissingProject_ThrowsBusinessValidationErrorException(string? project)
        {
            // Arrange
            var dto = ValidCreateDto();
            dto.Project = project!;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));
            exception.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
            await _repository.DidNotReceive().CreateAsync(Arg.Any<YearlyFinancialData>());
        }

        [Fact]
        public async Task CreateAsync_WithBothYearAndProjectMissing_ThrowsWithBothErrors()
        {
            // Arrange
            var dto = new YearlyFinancialDataDto { Year = 0, Project = null };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));
            exception.Errors.Should().Contain(e => e.Code == "YEAR_REQUIRED");
            exception.Errors.Should().Contain(e => e.Code == "PROJECT_REQUIRED");
            exception.Errors.Should().HaveCount(2);
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateCompositeKey_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = ValidCreateDto();
            _repository.ExistsAsync(dto.Year, dto.Project!).Returns(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));
            exception.Errors.Should().ContainSingle(e => e.Code == "DUPLICATE_YEARLY_FINANCIAL_DATA");
            await _repository.DidNotReceive().CreateAsync(Arg.Any<YearlyFinancialData>());
        }

        [Fact]
        public async Task CreateAsync_WithAdjustmentAndNoComment_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = ValidCreateDto();
            dto.Adjustment = 10m;
            dto.AdjustmentComment = "  ";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.CreateAsync(dto));
            exception.Errors.Should().ContainSingle(e => e.Code == "ADJUSTMENT_COMMENT_REQUIRED");
            await _repository.DidNotReceive().CreateAsync(Arg.Any<YearlyFinancialData>());
        }

        [Fact]
        public async Task CreateAsync_WithLockedRecord_AppliesAccessCostingRulesBeforePersisting()
        {
            // Arrange
            var dto = ValidCreateDto();
            dto.Project = " PP001 ";
            dto.Locked = 1;
            dto.Adjustment = 15m;
            dto.AdjustmentComment = "Legacy adjustment";
            dto.ManDays = 22d;
            dto.PayCosts = 125m;
            dto.NonPayOhCosts = 60m;
            dto.TestCosts = 40m;
            dto.AnimalCosts = 10m;
            dto.NonAnimalCosts = 90m;

            var entity = EntityFor();
            var created = EntityFor();
            var createdDto = ValidCreateDto();
            var pactRows = PactRows(
                new PactProjectYearCosts { Project = "PP001", Year = 2024, TotalCosts = 100m, Hours = 770d, Pay = 125m, NonPayOH = 60m, Tests = 40m, Animals = 10m, SubContracts = 100m },
                new PactProjectYearCosts { Project = "PP001", Year = 2024, TotalCosts = 25m, Hours = 77d, Pay = 0m, NonPayOH = 0m, Tests = 0m, Animals = 0m, SubContracts = 0m });

            _repository.ExistsAsync(2024, "PP001").Returns(false);
            _repository.GetPactCostsAsync("PP001", 2024).Returns(pactRows);
            _mapper.Map<YearlyFinancialData>(dto).Returns(entity);
            _repository.CreateAsync(entity).Returns(created);
            _mapper.Map<YearlyFinancialDataDto>(created).Returns(createdDto);

            // Act
            await _sut.CreateAsync(dto);

            // Assert
            dto.Project.Should().Be("PP001");
            dto.ManHours.Should().BeNull();
            dto.ManYears.Should().BeNull();
            dto.ActualExpenditure.Should().Be(140m);
            dto.ActualManYears.Should().BeNull();
            dto.PayCostsChanged.Should().Be(0);
            dto.NonPayOhCostsChanged.Should().Be(0);
            dto.TestCostsChanged.Should().Be(0);
            dto.AnimalCostsChanged.Should().Be(0);
            dto.NonAnimalCostsChanged.Should().Be(0);
            dto.DateCosted.Should().NotBeNull();
            await _repository.Received(1).GetPactCostsAsync("PP001", 2024);
        }

        [Fact]
        public async Task CreateAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var dto    = ValidCreateDto();
            var entity = EntityFor();

            _repository.ExistsAsync(dto.Year, dto.Project!).Returns(false);
            _mapper.Map<YearlyFinancialData>(dto).Returns(entity);
            _repository.CreateAsync(entity).Throws(new Exception("DB error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.CreateAsync(dto));
            exception.Message.Should().Be("DB error");
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidDto_ReturnsMappedUpdatedDto()
        {
            // Arrange
            var dto      = ValidUpdateDto();
            var existing = EntityFor();
            var updated  = EntityFor();
            var result   = ValidUpdateDto();

            _repository.GetByKeyAsync(dto.Year, dto.Project!).Returns(existing);
            _repository.UpdateAsync(existing).Returns(updated);
            _mapper.Map<YearlyFinancialDataDto>(updated).Returns(result);

            // Act
            var actual = await _sut.UpdateAsync(dto);

            // Assert
            actual.Should().NotBeNull();
            actual.Project.Should().Be("PP001");
            await _repository.Received(1).GetByKeyAsync(dto.Year, dto.Project!);
            await _repository.Received(1).UpdateAsync(existing);
            _mapper.Received(1).Map<YearlyFinancialDataDto>(updated);
        }

        [Fact]
        public async Task UpdateAsync_MapsSourceDtoOntoExistingEntity_BeforeCallingRepository()
        {
            // Arrange
            var dto      = ValidUpdateDto();
            var existing = EntityFor();
            var updated  = EntityFor();
            var resultDto = ValidUpdateDto();

            _repository.GetByKeyAsync(dto.Year, dto.Project!).Returns(existing);
            _repository.UpdateAsync(existing).Returns(updated);
            _mapper.Map<YearlyFinancialDataDto>(updated).Returns(resultDto);

            // Act
            await _sut.UpdateAsync(dto);

            // Assert — two-argument Map call (dto → existing entity)
            _mapper.Received(1).Map(dto, existing);
        }

        [Fact]
        public async Task UpdateAsync_WithNullDto_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.UpdateAsync(null!));
            exception.Message.Should().Contain("YearlyFinancialData DTO must not be null.");
            await _repository.DidNotReceive().GetByKeyAsync(Arg.Any<short>(), Arg.Any<string>());
        }

        [Fact]
        public async Task UpdateAsync_WithZeroYear_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = ValidUpdateDto();
            dto.Year = 0;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));
            exception.Errors.Should().ContainSingle(e => e.Code == "YEAR_REQUIRED");
            await _repository.DidNotReceive().GetByKeyAsync(Arg.Any<short>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateAsync_WithMissingProject_ThrowsBusinessValidationErrorException(string? project)
        {
            // Arrange
            var dto = ValidUpdateDto();
            dto.Project = project!;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));
            exception.Errors.Should().ContainSingle(e => e.Code == "PROJECT_REQUIRED");
        }

        [Fact]
        public async Task UpdateAsync_WithAdjustmentAndNoComment_ThrowsBusinessValidationErrorException()
        {
            // Arrange
            var dto = ValidUpdateDto();
            dto.Adjustment = 5m;
            dto.AdjustmentComment = null;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.UpdateAsync(dto));
            exception.Errors.Should().ContainSingle(e => e.Code == "ADJUSTMENT_COMMENT_REQUIRED");
            await _repository.DidNotReceive().UpdateAsync(Arg.Any<YearlyFinancialData>());
        }

        [Fact]
        public async Task UpdateAsync_AppliesPactComparisonAndReportedFigureRules()
        {
            // Arrange
            var dto = ValidUpdateDto();
            dto.ManYears = 0.5d;
            dto.Locked = 1;
            dto.Adjustment = -10m;
            dto.AdjustmentComment = "Carry-over";
            dto.PayCosts = 140m;
            dto.NonPayOhCosts = 60m;
            dto.TestCosts = 41m;
            dto.AnimalCosts = 10m;
            dto.NonAnimalCosts = 95m;
            dto.DateCosted = new DateTime(2024, 5, 1);

            var existing = EntityFor();
            var updated = EntityFor();
            var result = ValidUpdateDto();
            var pactRows = PactRows(
                new PactProjectYearCosts { Project = "PP001", Year = 2024, TotalCosts = 120m, Hours = 770d, Pay = 140m, NonPayOH = 60m, Tests = 40m, Animals = 10m, SubContracts = 100m });

            _repository.GetByKeyAsync(dto.Year, dto.Project!).Returns(existing);
            _repository.GetPactCostsAsync(dto.Project!, dto.Year).Returns(pactRows);
            _repository.UpdateAsync(existing).Returns(updated);
            _mapper.Map<YearlyFinancialDataDto>(updated).Returns(result);

            // Act
            await _sut.UpdateAsync(dto);

            // Assert
            dto.ManHours.Should().BeNull();
            dto.ManDays.Should().BeNull();
            dto.ActualExpenditure.Should().Be(110m);
            dto.ActualManYears.Should().BeNull();
            dto.PayCostsChanged.Should().Be(0);
            dto.NonPayOhCostsChanged.Should().Be(0);
            dto.TestCostsChanged.Should().Be(1);
            dto.AnimalCostsChanged.Should().Be(0);
            dto.NonAnimalCostsChanged.Should().Be(1);
            dto.DateCosted.Should().Be(new DateTime(2024, 5, 1));
        }

        [Fact]
        public async Task UpdateAsync_WithLockedRecordAndNoAdjustment_ClearsActualExpenditure()
        {
            // Arrange
            var dto = ValidUpdateDto();
            dto.Locked = 1;
            dto.Adjustment = null;
            dto.AdjustmentComment = null;
            dto.PayCosts = 140m;
            dto.NonPayOhCosts = 60m;
            dto.TestCosts = 41m;
            dto.AnimalCosts = 10m;
            dto.NonAnimalCosts = 95m;

            var existing = EntityFor();
            var updated = EntityFor();
            var result = ValidUpdateDto();
            var pactRows = PactRows(
                new PactProjectYearCosts { Project = "PP001", Year = 2024, TotalCosts = 120m, Hours = 770d, Pay = 140m, NonPayOH = 60m, Tests = 40m, Animals = 10m, SubContracts = 100m });

            _repository.GetByKeyAsync(dto.Year, dto.Project!).Returns(existing);
            _repository.GetPactCostsAsync(dto.Project!, dto.Year).Returns(pactRows);
            _repository.UpdateAsync(existing).Returns(updated);
            _mapper.Map<YearlyFinancialDataDto>(updated).Returns(result);

            // Act
            await _sut.UpdateAsync(dto);

            // Assert
            dto.ActualExpenditure.Should().BeNull();
            dto.DateCosted.Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_WhenRecordNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = ValidUpdateDto(9999, "UNKNOWN");
            _repository.GetByKeyAsync(dto.Year, dto.Project!).Returns((YearlyFinancialData?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _sut.UpdateAsync(dto));
            exception.Message.Should().Contain("9999");
            await _repository.DidNotReceive().UpdateAsync(Arg.Any<YearlyFinancialData>());
        }

        [Fact]
        public async Task UpdateAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var dto      = ValidUpdateDto();
            var existing = EntityFor();
            _repository.GetByKeyAsync(dto.Year, dto.Project!).Returns(existing);
            _repository.UpdateAsync(existing).Throws(new Exception("DB error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.UpdateAsync(dto));
            exception.Message.Should().Be("DB error");
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WhenRecordExists_ReturnsTrue()
        {
            // Arrange
            _repository.DeleteAsync((short)2024, "PP001").Returns(true);

            // Act
            var result = await _sut.DeleteAsync((short)2024, "PP001");

            // Assert
            result.Should().BeTrue();
            await _repository.Received(1).DeleteAsync((short)2024, "PP001");
        }

        [Fact]
        public async Task DeleteAsync_WhenRecordNotFound_ReturnsFalse()
        {
            // Arrange
            _repository.DeleteAsync(Arg.Any<short>(), Arg.Any<string>()).Returns(false);

            // Act
            var result = await _sut.DeleteAsync((short)9999, "UNKNOWN");

            // Assert
            result.Should().BeFalse();
            await _repository.Received(1).DeleteAsync(Arg.Any<short>(), Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _repository.DeleteAsync(Arg.Any<short>(), Arg.Any<string>()).Throws(new Exception("DB error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.DeleteAsync((short)2024, "PP001"));
            exception.Message.Should().Be("DB error");
        }

        #endregion

        #region GetPactCostsAsync Tests

        [Fact]
        public async Task GetPactCostsAsync_WithValidProjectAndYear_ReturnsMappedList()
        {
            // Arrange
            const string project = "PP001";
            const short year     = 2024;
            var entities         = new List<PactProjectYearCosts> { new() { Project = project, Year = year } }.AsReadOnly();
            var dtos             = new List<PactProjectYearCostsDto> { new() { Project = project, Year = year } }.AsReadOnly();

            _repository.GetPactCostsAsync(project, year).Returns(entities);
            _mapper.Map<IReadOnlyList<PactProjectYearCostsDto>>(entities).Returns(dtos);

            // Act
            var result = await _sut.GetPactCostsAsync(project, year);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().Project.Should().Be(project);
            await _repository.Received(1).GetPactCostsAsync(project, year);
            _mapper.Received(1).Map<IReadOnlyList<PactProjectYearCostsDto>>(entities);
        }

        [Fact]
        public async Task GetPactCostsAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var emptyEntities = new List<PactProjectYearCosts>().AsReadOnly();
            var emptyDtos     = new List<PactProjectYearCostsDto>().AsReadOnly();

            _repository.GetPactCostsAsync(Arg.Any<string>(), Arg.Any<short>()).Returns(emptyEntities);
            _mapper.Map<IReadOnlyList<PactProjectYearCostsDto>>(emptyEntities).Returns(emptyDtos);

            // Act
            var result = await _sut.GetPactCostsAsync("PP001", 2024);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetPactCostsAsync_WithMissingProject_ThrowsArgumentException(string? project)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetPactCostsAsync(project!, 2024));
            exception.Message.Should().Contain("Project is required.");
        }

        [Fact]
        public async Task GetPactCostsAsync_WithZeroYear_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _sut.GetPactCostsAsync("PP001", 0));
            exception.Message.Should().Contain("Year must be a valid financial year.");
        }

        [Fact]
        public async Task GetPactCostsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _repository.GetPactCostsAsync(Arg.Any<string>(), Arg.Any<short>())
                       .Throws(new Exception("DB error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetPactCostsAsync("PP001", 2024));
            exception.Message.Should().Be("DB error");
        }

        #endregion

        #region GetSettingValueByIdAsync Tests

        [Fact]
        public async Task GetSettingValueByIdAsync_WithValidId_ReturnsSettingValue()
        {
            // Arrange
            _repository.GetSettingValueByIdAsync("HoursInDay").Returns("7.4");

            // Act
            var result = await _sut.GetSettingValueByIdAsync("HoursInDay");

            // Assert
            result.Should().Be("7.4");
            await _repository.Received(1).GetSettingValueByIdAsync("HoursInDay");
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WhenRepositoryReturnsNull_ReturnsEmptyString()
        {
            // Arrange
            _repository.GetSettingValueByIdAsync("UnknownSetting").Returns((string?)null);

            // Act
            var result = await _sut.GetSettingValueByIdAsync("UnknownSetting");

            // Assert
            result.Should().BeEmpty();
            await _repository.Received(1).GetSettingValueByIdAsync("UnknownSetting");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetSettingValueByIdAsync_WithNullOrWhitespaceId_ReturnsEmptyStringWithoutCallingRepository(string? id)
        {
            // Act
            var result = await _sut.GetSettingValueByIdAsync(id!);

            // Assert
            result.Should().BeEmpty();
            await _repository.DidNotReceive().GetSettingValueByIdAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithPaddedId_TrimsBeforeQuerying()
        {
            // Arrange
            _repository.GetSettingValueByIdAsync("DaysInYear").Returns("220");

            // Act
            var result = await _sut.GetSettingValueByIdAsync("  DaysInYear  ");

            // Assert
            result.Should().Be("220");
            await _repository.Received(1).GetSettingValueByIdAsync("DaysInYear");
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _repository.GetSettingValueByIdAsync(Arg.Any<string>())
                       .Throws(new Exception("DB error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetSettingValueByIdAsync("HoursInDay"));
            exception.Message.Should().Be("DB error");
        }

        #endregion
    }
}
