using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.MonthHourServiceTest
{
    public class MonthHourServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsMonthHourApiClient _fpsMonthHourApiClient;
        private readonly MonthHourService _sut;

        public MonthHourServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsMonthHourApiClient = Substitute.For<IFpsMonthHourApiClient>();
            _fpsClient.FpsMonthHour.Returns(_fpsMonthHourApiClient);
            _sut = new MonthHourService(_fpsClient);
        }

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        #region Constructor

        [Fact]
        public void Constructor_WhenFpsClientIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MonthHourService(null!));
        }

        #endregion

        // -----------------------------------------------------------------------
        // GetAllMonthHourAsync
        // -----------------------------------------------------------------------

        #region GetAllMonthHourAsync

        [Fact]
        public async Task GetAllMonthHourAsync_WhenApiReturnsSuccess_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var monthHours = new List<MonthHourDto>
            {
                new MonthHourDto { Year = 2024, Month = 1, Days = 20, FpsYear = 2024 },
                new MonthHourDto { Year = 2024, Month = 2, Days = 18, FpsYear = 2024 }
            };
            var expectedResponse = ApiResponseDto<List<MonthHourDto>>.SuccessResponse(monthHours);
            _fpsMonthHourApiClient.GetAllMonthHourAsync(query).Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllMonthHourAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsMonthHourApiClient.Received(1).GetAllMonthHourAsync(query);
        }

        [Fact]
        public async Task GetAllMonthHourAsync_WhenApiReturnsEmptyList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<MonthHourDto>>.SuccessResponse(new List<MonthHourDto>());
            _fpsMonthHourApiClient.GetAllMonthHourAsync(query).Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllMonthHourAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _fpsMonthHourApiClient.Received(1).GetAllMonthHourAsync(query);
        }

        [Fact]
        public async Task GetAllMonthHourAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Unauthorized", Code = "UNAUTHORIZED" } };
            var expectedResponse = ApiResponseDto<List<MonthHourDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsMonthHourApiClient.GetAllMonthHourAsync(query).Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllMonthHourAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _fpsMonthHourApiClient.Received(1).GetAllMonthHourAsync(query);
        }

        [Fact]
        public async Task GetAllMonthHourAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _fpsMonthHourApiClient.GetAllMonthHourAsync(query).ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAllMonthHourAsync(query));
            Assert.Equal("API unavailable", exception.Message);
            await _fpsMonthHourApiClient.Received(1).GetAllMonthHourAsync(query);
        }

        #endregion

        // -----------------------------------------------------------------------
        // GetMonthHoursByYearAsync
        // -----------------------------------------------------------------------

        #region GetMonthHoursByYearAsync

        [Fact]
        public async Task GetMonthHoursByYearAsync_WhenApiReturnsSuccess_ReturnsRecordsForYear()
        {
            // Arrange
            const short year = 2024;
            var monthHours = new List<MonthHourDto>
            {
                new MonthHourDto { Year = year, Month = 1, Days = 20, FpsYear = 2024 },
                new MonthHourDto { Year = year, Month = 2, Days = 18, FpsYear = 2024 }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<MonthHourDto>>.SuccessResponse(monthHours);
            _fpsMonthHourApiClient.GetMonthHoursByYearAsync(year).Returns(expectedResponse);

            // Act
            var result = await _sut.GetMonthHoursByYearAsync(year);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count());
            await _fpsMonthHourApiClient.Received(1).GetMonthHoursByYearAsync(year);
        }

        [Fact]
        public async Task GetMonthHoursByYearAsync_WhenApiReturnsEmptyCollection_ReturnsSuccessWithEmpty()
        {
            // Arrange
            const short year = 2099;
            var expectedResponse = ApiResponseDto<IEnumerable<MonthHourDto>>.SuccessResponse(
                Enumerable.Empty<MonthHourDto>());
            _fpsMonthHourApiClient.GetMonthHoursByYearAsync(year).Returns(expectedResponse);

            // Act
            var result = await _sut.GetMonthHoursByYearAsync(year);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _fpsMonthHourApiClient.Received(1).GetMonthHoursByYearAsync(year);
        }

        [Theory]
        [InlineData((short)2023)]
        [InlineData((short)2024)]
        [InlineData((short)2025)]
        public async Task GetMonthHoursByYearAsync_PassesYearToApiClient(short year)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<IEnumerable<MonthHourDto>>.SuccessResponse(
                Enumerable.Empty<MonthHourDto>());
            _fpsMonthHourApiClient.GetMonthHoursByYearAsync(year).Returns(expectedResponse);

            // Act
            await _sut.GetMonthHoursByYearAsync(year);

            // Assert
            await _fpsMonthHourApiClient.Received(1).GetMonthHoursByYearAsync(year);
        }

        [Fact]
        public async Task GetMonthHoursByYearAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const short year = 2024;
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<IEnumerable<MonthHourDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsMonthHourApiClient.GetMonthHoursByYearAsync(year).Returns(expectedResponse);

            // Act
            var result = await _sut.GetMonthHoursByYearAsync(year);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _fpsMonthHourApiClient.Received(1).GetMonthHoursByYearAsync(year);
        }

        [Fact]
        public async Task GetMonthHoursByYearAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            const short year = 2024;
            _fpsMonthHourApiClient.GetMonthHoursByYearAsync(year).ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetMonthHoursByYearAsync(year));
            Assert.Equal("API unavailable", exception.Message);
            await _fpsMonthHourApiClient.Received(1).GetMonthHoursByYearAsync(year);
        }

        #endregion

        // -----------------------------------------------------------------------
        // GetDistinctYearsAsync
        // -----------------------------------------------------------------------

        #region GetDistinctYearsAsync

        [Fact]
        public async Task GetDistinctYearsAsync_WhenApiReturnsSuccess_ReturnsYears()
        {
            // Arrange
            var years = new List<short> { 2022, 2023, 2024 };
            var expectedResponse = ApiResponseDto<IEnumerable<short>>.SuccessResponse(years);
            _fpsMonthHourApiClient.GetDistinctYearsAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetDistinctYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data?.Count());
            await _fpsMonthHourApiClient.Received(1).GetDistinctYearsAsync();
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WhenApiReturnsEmptyCollection_ReturnsSuccessWithEmpty()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<IEnumerable<short>>.SuccessResponse(
                Enumerable.Empty<short>());
            _fpsMonthHourApiClient.GetDistinctYearsAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetDistinctYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _fpsMonthHourApiClient.Received(1).GetDistinctYearsAsync();
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Service error", Code = "SERVICE_ERROR" } };
            var expectedResponse = ApiResponseDto<IEnumerable<short>>.FailureResponse(errors, new ApiMetaDto());
            _fpsMonthHourApiClient.GetDistinctYearsAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetDistinctYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _fpsMonthHourApiClient.Received(1).GetDistinctYearsAsync();
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _fpsMonthHourApiClient.GetDistinctYearsAsync().ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetDistinctYearsAsync());
            Assert.Equal("API unavailable", exception.Message);
            await _fpsMonthHourApiClient.Received(1).GetDistinctYearsAsync();
        }

        #endregion

        // -----------------------------------------------------------------------
        // GetYearEndMonthHoursAsync
        // -----------------------------------------------------------------------

        #region GetYearEndMonthHoursAsync

        [Fact]
        public async Task GetYearEndMonthHoursAsync_WhenApiReturnsSuccess_ReturnsMappedList()
        {
            // Arrange
            var yearEndHours = new List<YearEndMonthHourDto>
            {
                new YearEndMonthHourDto { Month = 1, Days = 20, ExistsForPlannedYear = "Yes", FpsYear = 2025 },
                new YearEndMonthHourDto { Month = 2, Days = 18, ExistsForPlannedYear = "No",  FpsYear = 2025 }
            };
            var expectedResponse = ApiResponseDto<List<YearEndMonthHourDto>>.SuccessResponse(yearEndHours);
            _fpsMonthHourApiClient.GetYearEndMonthHoursAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetYearEndMonthHoursAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsMonthHourApiClient.Received(1).GetYearEndMonthHoursAsync();
        }

        [Fact]
        public async Task GetYearEndMonthHoursAsync_WhenApiReturnsEmptyList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<YearEndMonthHourDto>>.SuccessResponse(
                new List<YearEndMonthHourDto>());
            _fpsMonthHourApiClient.GetYearEndMonthHoursAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetYearEndMonthHoursAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _fpsMonthHourApiClient.Received(1).GetYearEndMonthHoursAsync();
        }

        [Fact]
        public async Task GetYearEndMonthHoursAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<List<YearEndMonthHourDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsMonthHourApiClient.GetYearEndMonthHoursAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetYearEndMonthHoursAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _fpsMonthHourApiClient.Received(1).GetYearEndMonthHoursAsync();
        }

        [Fact]
        public async Task GetYearEndMonthHoursAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _fpsMonthHourApiClient.GetYearEndMonthHoursAsync().ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetYearEndMonthHoursAsync());
            Assert.Equal("API unavailable", exception.Message);
            await _fpsMonthHourApiClient.Received(1).GetYearEndMonthHoursAsync();
        }

        #endregion

        // -----------------------------------------------------------------------
        // SaveMonthHourAsync
        // -----------------------------------------------------------------------

        #region SaveMonthHourAsync

        [Fact]
        public async Task SaveMonthHourAsync_WhenApiReturnsSuccess_ReturnsSavedDto()
        {
            // Arrange
            var dto = new MonthHourDto { Year = 2024, Month = 3, Days = 20, VidHours = 5, CvlHours = 3, FpsYear = 2024 };
            var saved = new MonthHourDto { Year = 2024, Month = 3, Days = 20, VidHours = 5, CvlHours = 3, FpsYear = 2024 };
            var expectedResponse = ApiResponseDto<MonthHourDto>.SuccessResponse(saved);
            _fpsMonthHourApiClient.SaveMonthHourAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.SaveMonthHourAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal((short)3, result.Data?.Month);
            await _fpsMonthHourApiClient.Received(1).SaveMonthHourAsync(dto);
        }

        [Fact]
        public async Task SaveMonthHourAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new MonthHourDto { Year = 2024, Month = 1, Days = -1 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Validation error", Code = "VALIDATION_ERROR" }
            };
            var expectedResponse = ApiResponseDto<MonthHourDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsMonthHourApiClient.SaveMonthHourAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.SaveMonthHourAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _fpsMonthHourApiClient.Received(1).SaveMonthHourAsync(dto);
        }

        [Fact]
        public async Task SaveMonthHourAsync_PassesDtoToApiClient()
        {
            // Arrange
            var dto = new MonthHourDto { Year = 2024, Month = 6, Days = 21, FpsYear = 2024 };
            var expectedResponse = ApiResponseDto<MonthHourDto>.SuccessResponse(dto);
            _fpsMonthHourApiClient.SaveMonthHourAsync(dto).Returns(expectedResponse);

            // Act
            await _sut.SaveMonthHourAsync(dto);

            // Assert
            await _fpsMonthHourApiClient.Received(1).SaveMonthHourAsync(
                Arg.Is<MonthHourDto>(d => d.Year == 2024 && d.Month == 6 && d.Days == 21));
        }

        [Fact]
        public async Task SaveMonthHourAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            var dto = new MonthHourDto { Year = 2024, Month = 1, Days = 20 };
            _fpsMonthHourApiClient.SaveMonthHourAsync(dto).ThrowsAsync(new Exception("Save failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.SaveMonthHourAsync(dto));
            Assert.Equal("Save failed", exception.Message);
            await _fpsMonthHourApiClient.Received(1).SaveMonthHourAsync(dto);
        }

        #endregion
    }
}
