using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Services;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.MonthHourServiceTest
{
    public class MonthHourServiceTests
    {
        private readonly IMonthHourRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly MonthHourService _sut;

        public MonthHourServiceTests()
        {
            _mockRepository = Substitute.For<IMonthHourRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new MonthHourService(_mockRepository, _mockMapper);
        }

        // -----------------------------------------------------------------------
        // GetAllMonthHourAsync
        // -----------------------------------------------------------------------

        #region GetAllMonthHourAsync

        [Fact]
        public async Task GetAllMonthHourAsync_WhenDataExists_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new Apha.FPS.Application.Pagination.QueryParameters<string> { Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<MonthHour>
            {
                Data = [
                    new MonthHour { Year = 2024, Month = 1, Days = 20, FpsYear = 2024 },
                    new MonthHour { Year = 2024, Month = 2, Days = 18, FpsYear = 2024 }
                ],
                PaginationData = new PaginationData { TotalRecords = 2, PageNumber = 1, PageSize = 10 }
            };
            var expectedResult = new Apha.FPS.Application.Pagination.PaginatedResult<MonthHourDto>
            {
                Data = [
                    new MonthHourDto { Year = 2024, Month = 1, Days = 20, FpsYear = 2024 },
                    new MonthHourDto { Year = 2024, Month = 2, Days = 18, FpsYear = 2024 }
                ]
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetAllAsync(parameters).Returns(pagedData);
            _mockMapper.Map<Apha.FPS.Application.Pagination.PaginatedResult<MonthHourDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetAllMonthHourAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().Year.Should().Be(2024);
            result.Data.First().Month.Should().Be(1);

            await _mockRepository.Received(1).GetAllAsync(parameters);
            _mockMapper.Received(1).Map<Apha.FPS.Application.Pagination.PaginatedResult<MonthHourDto>>(pagedData);
        }

        [Fact]
        public async Task GetAllMonthHourAsync_WhenNoDataExists_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = new Apha.FPS.Application.Pagination.QueryParameters<string> { Page = 1, PageSize = 10 };
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<MonthHour>
            {
                Data = [],
                PaginationData = new PaginationData { TotalRecords = 0, PageNumber = 1, PageSize = 10 }
            };
            var expectedResult = new Apha.FPS.Application.Pagination.PaginatedResult<MonthHourDto> { Data = [] };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetAllAsync(parameters).Returns(pagedData);
            _mockMapper.Map<Apha.FPS.Application.Pagination.PaginatedResult<MonthHourDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetAllMonthHourAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();

            await _mockRepository.Received(1).GetAllAsync(parameters);
        }

        [Fact]
        public async Task GetAllMonthHourAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query = new Apha.FPS.Application.Pagination.QueryParameters<string>();
            var parameters = new PaginationParameters<string>();
            _mockMapper.Map<PaginationParameters<string>>(query).Returns(parameters);
            _mockRepository.GetAllAsync(parameters).Throws(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAllMonthHourAsync(query));
            exception.Message.Should().Be("Database error");

            await _mockRepository.Received(1).GetAllAsync(parameters);
        }

        #endregion

        // -----------------------------------------------------------------------
        // GetMonthHoursByYearAsync
        // -----------------------------------------------------------------------

        #region GetMonthHoursByYearAsync

        [Fact]
        public async Task GetMonthHoursByYearAsync_WhenRecordsExistForYear_ReturnsMappedDtos()
        {
            // Arrange
            const short year = 2024;
            var entities = new List<MonthHour>
            {
                new MonthHour { Year = year, Month = 1, Days = 20, VidHours = 5, CvlHours = 3, FpsYear = 2024 },
                new MonthHour { Year = year, Month = 2, Days = 18, VidHours = 4, CvlHours = 2, FpsYear = 2024 }
            };
            var expectedDtos = new List<MonthHourDto>
            {
                new MonthHourDto { Year = year, Month = 1, Days = 20, VidHours = 5, CvlHours = 3, FpsYear = 2024 },
                new MonthHourDto { Year = year, Month = 2, Days = 18, VidHours = 4, CvlHours = 2, FpsYear = 2024 }
            };

            _mockRepository.GetByYearAsync(year).Returns(entities);
            _mockMapper.Map<IEnumerable<MonthHourDto>>(entities).Returns(expectedDtos);

            // Act
            var result = await _sut.GetMonthHoursByYearAsync(year);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().Year.Should().Be(year);
            result.First().Month.Should().Be(1);

            await _mockRepository.Received(1).GetByYearAsync(year);
            _mockMapper.Received(1).Map<IEnumerable<MonthHourDto>>(entities);
        }

        [Fact]
        public async Task GetMonthHoursByYearAsync_WhenNoRecordsForYear_ReturnsEmptyCollection()
        {
            // Arrange
            const short year = 2099;
            var empty = Enumerable.Empty<MonthHour>();
            _mockRepository.GetByYearAsync(year).Returns(empty);
            _mockMapper.Map<IEnumerable<MonthHourDto>>(empty).Returns(Enumerable.Empty<MonthHourDto>());

            // Act
            var result = await _sut.GetMonthHoursByYearAsync(year);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetByYearAsync(year);
        }

        [Fact]
        public async Task GetMonthHoursByYearAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            const short year = 2024;
            _mockRepository.GetByYearAsync(year).Throws(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetMonthHoursByYearAsync(year));
            exception.Message.Should().Be("Database error");

            await _mockRepository.Received(1).GetByYearAsync(year);
        }

        #endregion

        // -----------------------------------------------------------------------
        // GetDistinctYearsAsync
        // -----------------------------------------------------------------------

        #region GetDistinctYearsAsync

        [Fact]
        public async Task GetDistinctYearsAsync_WhenYearsExist_ReturnsYears()
        {
            // Arrange
            var years = new List<short> { 2022, 2023, 2024 };
            _mockRepository.GetDistinctYearsAsync().Returns(years.AsEnumerable());

            // Act
            var result = await _sut.GetDistinctYearsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().ContainInOrder((short)2022, (short)2023, (short)2024);

            await _mockRepository.Received(1).GetDistinctYearsAsync();
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WhenNoYearsExist_ReturnsEmptyCollection()
        {
            // Arrange
            _mockRepository.GetDistinctYearsAsync().Returns(Enumerable.Empty<short>());

            // Act
            var result = await _sut.GetDistinctYearsAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetDistinctYearsAsync();
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetDistinctYearsAsync().Throws(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetDistinctYearsAsync());
            exception.Message.Should().Be("Database error");

            await _mockRepository.Received(1).GetDistinctYearsAsync();
        }

        #endregion

        // -----------------------------------------------------------------------
        // GetYearEndMonthHoursAsync
        // -----------------------------------------------------------------------

        #region GetYearEndMonthHoursAsync

        [Fact]
        public async Task GetYearEndMonthHoursAsync_WhenDataExists_ReturnsMappedDtos()
        {
            // Arrange
            var entities = new List<YearEndMonthHour>
            {
                new YearEndMonthHour { Month = 1, Days = 20, VidHours = 5, CvlHours = 3, ExistsForPlannedYear = "Yes", FpsYear = 2025 },
                new YearEndMonthHour { Month = 2, Days = 18, VidHours = 4, CvlHours = 2, ExistsForPlannedYear = "No",  FpsYear = 2025 }
            };
            var expectedDtos = new List<YearEndMonthHourDto>
            {
                new YearEndMonthHourDto { Month = 1, Days = 20, ExistsForPlannedYear = "Yes", FpsYear = 2025 },
                new YearEndMonthHourDto { Month = 2, Days = 18, ExistsForPlannedYear = "No",  FpsYear = 2025 }
            };

            _mockRepository.GetYearEndMonthHoursAsync().Returns(entities);
            _mockMapper.Map<List<YearEndMonthHourDto>>(entities).Returns(expectedDtos);

            // Act
            var result = await _sut.GetYearEndMonthHoursAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Month.Should().Be(1);
            result[0].ExistsForPlannedYear.Should().Be("Yes");
            result[1].ExistsForPlannedYear.Should().Be("No");

            await _mockRepository.Received(1).GetYearEndMonthHoursAsync();
            _mockMapper.Received(1).Map<List<YearEndMonthHourDto>>(entities);
        }

        [Fact]
        public async Task GetYearEndMonthHoursAsync_WhenNoDataExists_ReturnsEmptyList()
        {
            // Arrange
            var empty = new List<YearEndMonthHour>();
            _mockRepository.GetYearEndMonthHoursAsync().Returns(empty);
            _mockMapper.Map<List<YearEndMonthHourDto>>(empty).Returns(new List<YearEndMonthHourDto>());

            // Act
            var result = await _sut.GetYearEndMonthHoursAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetYearEndMonthHoursAsync();
        }

        [Fact]
        public async Task GetYearEndMonthHoursAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetYearEndMonthHoursAsync().Throws(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetYearEndMonthHoursAsync());
            exception.Message.Should().Be("Database error");

            await _mockRepository.Received(1).GetYearEndMonthHoursAsync();
        }

        #endregion

        // -----------------------------------------------------------------------
        // SaveMonthHourAsync — validation
        // -----------------------------------------------------------------------

        #region SaveMonthHourAsync — validation

        [Fact]
        public async Task SaveMonthHourAsync_WhenDaysIsNegative_ThrowsBusinessValidationError()
        {
            // Arrange
            var dto = new MonthHourDto { Year = 2024, Month = 1, Days = -1, VidHours = 5, CvlHours = 3 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.SaveMonthHourAsync(dto));
            ex.Errors.Should().ContainSingle(e => e.Code == "Missing_Config");

            await _mockRepository.DidNotReceive().SaveAsync(Arg.Any<MonthHour>());
        }

        [Fact]
        public async Task SaveMonthHourAsync_WhenVidHoursIsNegative_ThrowsBusinessValidationError()
        {
            // Arrange
            var dto = new MonthHourDto { Year = 2024, Month = 1, Days = 20, VidHours = -1, CvlHours = 3 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.SaveMonthHourAsync(dto));
            ex.Errors.Should().ContainSingle(e => e.Code == "Missing_Config");

            await _mockRepository.DidNotReceive().SaveAsync(Arg.Any<MonthHour>());
        }

        [Fact]
        public async Task SaveMonthHourAsync_WhenCvlHoursIsNegative_ThrowsBusinessValidationError()
        {
            // Arrange
            var dto = new MonthHourDto { Year = 2024, Month = 1, Days = 20, VidHours = 5, CvlHours = -1 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.SaveMonthHourAsync(dto));
            ex.Errors.Should().ContainSingle(e => e.Code == "Missing_Config");

            await _mockRepository.DidNotReceive().SaveAsync(Arg.Any<MonthHour>());
        }

        [Fact]
        public async Task SaveMonthHourAsync_WhenAllValuesAreNegative_ThrowsBusinessValidationError()
        {
            // Arrange
            var dto = new MonthHourDto { Year = 2024, Month = 1, Days = -5, VidHours = -1, CvlHours = -2 };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(() => _sut.SaveMonthHourAsync(dto));
            ex.Errors.Should().ContainSingle(e => e.Code == "Missing_Config");

            await _mockRepository.DidNotReceive().SaveAsync(Arg.Any<MonthHour>());
        }

        #endregion

        // -----------------------------------------------------------------------
        // SaveMonthHourAsync — success
        // -----------------------------------------------------------------------

        #region SaveMonthHourAsync — success

        [Fact]
        public async Task SaveMonthHourAsync_WhenAllValuesAreValid_ReturnsMappedDto()
        {
            // Arrange
            var dto = new MonthHourDto { Year = 2024, Month = 3, Days = 20, VidHours = 5, CvlHours = 3, FpsYear = 2024 };
            var entity = new MonthHour { Year = 2024, Month = 3, Days = 20, VidHours = 5, CvlHours = 3, FpsYear = 2024 };
            var savedEntity = new MonthHour { Year = 2024, Month = 3, Days = 20, VidHours = 5, CvlHours = 3, FpsYear = 2024 };
            var expectedDto = new MonthHourDto { Year = 2024, Month = 3, Days = 20, VidHours = 5, CvlHours = 3, FpsYear = 2024 };

            _mockMapper.Map<MonthHour>(dto).Returns(entity);
            _mockRepository.SaveAsync(entity).Returns(savedEntity);
            _mockMapper.Map<MonthHourDto>(savedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.SaveMonthHourAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Year.Should().Be(2024);
            result.Month.Should().Be(3);
            result.Days.Should().Be(20);

            await _mockRepository.Received(1).SaveAsync(entity);
            _mockMapper.Received(1).Map<MonthHour>(dto);
            _mockMapper.Received(1).Map<MonthHourDto>(savedEntity);
        }

        [Fact]
        public async Task SaveMonthHourAsync_WhenValuesAreZero_PassesValidationAndCallsRepository()
        {
            // Arrange — zero is not negative, so validation should pass
            var dto = new MonthHourDto { Year = 2024, Month = 1, Days = 0, VidHours = 0, CvlHours = 0 };
            var entity = new MonthHour { Year = 2024, Month = 1, Days = 0, VidHours = 0, CvlHours = 0 };
            var savedEntity = new MonthHour { Year = 2024, Month = 1, Days = 0, VidHours = 0, CvlHours = 0 };
            var expectedDto = new MonthHourDto { Year = 2024, Month = 1, Days = 0, VidHours = 0, CvlHours = 0 };

            _mockMapper.Map<MonthHour>(dto).Returns(entity);
            _mockRepository.SaveAsync(entity).Returns(savedEntity);
            _mockMapper.Map<MonthHourDto>(savedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.SaveMonthHourAsync(dto);

            // Assert
            result.Should().NotBeNull();
            await _mockRepository.Received(1).SaveAsync(entity);
        }

        [Fact]
        public async Task SaveMonthHourAsync_WhenValuesAreNull_PassesValidationAndCallsRepository()
        {
            // Arrange — nullable decimal properties being null are not negative, so validation passes
            var dto = new MonthHourDto { Year = 2024, Month = 1, Days = null, VidHours = null, CvlHours = null };
            var entity = new MonthHour { Year = 2024, Month = 1, Days = null, VidHours = null, CvlHours = null };
            var savedEntity = new MonthHour { Year = 2024, Month = 1, Days = null, VidHours = null, CvlHours = null };
            var expectedDto = new MonthHourDto { Year = 2024, Month = 1, Days = null, VidHours = null, CvlHours = null };

            _mockMapper.Map<MonthHour>(dto).Returns(entity);
            _mockRepository.SaveAsync(entity).Returns(savedEntity);
            _mockMapper.Map<MonthHourDto>(savedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.SaveMonthHourAsync(dto);

            // Assert
            result.Should().NotBeNull();
            await _mockRepository.Received(1).SaveAsync(entity);
        }

        [Fact]
        public async Task SaveMonthHourAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var dto = new MonthHourDto { Year = 2024, Month = 1, Days = 20, VidHours = 5, CvlHours = 3 };
            var entity = new MonthHour { Year = 2024, Month = 1, Days = 20, VidHours = 5, CvlHours = 3 };
            _mockMapper.Map<MonthHour>(dto).Returns(entity);
            _mockRepository.SaveAsync(entity).Throws(new Exception("Save failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.SaveMonthHourAsync(dto));
            exception.Message.Should().Be("Save failed");

            await _mockRepository.Received(1).SaveAsync(entity);
        }

        #endregion
    }
}
