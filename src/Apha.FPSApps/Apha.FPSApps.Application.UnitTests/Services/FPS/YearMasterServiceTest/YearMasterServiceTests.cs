using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.YearMasterServiceTest
{
    public class YearMasterServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsYearMasterApiClient _fpsYearMasterApiClient;
        private readonly YearMasterService _yearMasterService;

        public YearMasterServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsYearMasterApiClient = Substitute.For<IFpsYearMasterApiClient>();
            _fpsClient.FpsYearMaster.Returns(_fpsYearMasterApiClient);
            _yearMasterService = new YearMasterService(_fpsClient);
        }

        #region GetAllYearMastersAsync Tests

        [Fact]
        public async Task GetAllYearMastersAsync_WithValidData_ReturnsSuccessResponse()
        {
            // Arrange
            var yearMasters = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2025, FpsYearCode = "2025", YearStatus = "Planned", Active = true },
                new YearMasterDto { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true },
                new YearMasterDto { FpsYear = 2023, FpsYearCode = "2023", YearStatus = "Closed", Active = true }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(yearMasters);

            _fpsYearMasterApiClient.GetAllFpsYearsAsync().Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetAllFpsYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data?.Count());
            await _fpsYearMasterApiClient.Received(1).GetAllFpsYearsAsync();
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(
                new List<YearMasterDto>()
            );

            _fpsYearMasterApiClient.GetAllFpsYearsAsync().Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetAllFpsYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithSingleYear_ReturnsSuccessWithSingleItem()
        {
            // Arrange
            var yearMasters = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(yearMasters);

            _fpsYearMasterApiClient.GetAllFpsYearsAsync().Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetAllFpsYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.Equal(2024, result.Data!.First().FpsYear);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.FailureResponse(
                errors,
                new ApiMetaDto()
            );

            _fpsYearMasterApiClient.GetAllFpsYearsAsync().Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetAllFpsYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetAllYearMastersAsync_CallsApiClientOnce()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(
                new List<YearMasterDto>()
            );

            _fpsYearMasterApiClient.GetAllFpsYearsAsync().Returns(expectedResponse);

            // Act
            await _yearMasterService.GetAllFpsYearsAsync();

            // Assert
            await _fpsYearMasterApiClient.Received(1).GetAllFpsYearsAsync();
        }

        [Fact]
        public async Task GetAllYearMastersAsync_WithMixedStatuses_ReturnsAllYears()
        {
            // Arrange
            var yearMasters = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2025, YearStatus = "Planned", Active = true },
                new YearMasterDto { FpsYear = 2024, YearStatus = "Open", Active = true },
                new YearMasterDto { FpsYear = 2023, YearStatus = "Closed", Active = true }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(yearMasters);

            _fpsYearMasterApiClient.GetAllFpsYearsAsync().Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetAllFpsYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Contains(result.Data!, y => y.YearStatus == "Open");
            Assert.Contains(result.Data!, y => y.YearStatus == "Planned");
            Assert.Contains(result.Data!, y => y.YearStatus == "Closed");
        }

        #endregion

        #region GetAllYearMastersPagedAsync Tests

        [Fact]
        public async Task GetAllYearMastersPagedAsync_WithValidQuery_ReturnsSuccessResponse()
        {
            // Arrange
            var queryParameters = new QueryParameters<int>
            {
                Page = 1,
                PageSize = 10,
                Filter = 2024
            };
            var yearMasters = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2024, FpsYearCode = "2024", YearStatus = "Open", Active = true }
            };
            var expectedResponse = ApiResponseDto<List<YearMasterDto>>.SuccessResponse(
                yearMasters,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            );

            _fpsYearMasterApiClient.GetAllFpsYearsPagedAsync(queryParameters).Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetAllFpsYearsPagedAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.Equal(2024, result.Data![0].FpsYear);
            await _fpsYearMasterApiClient.Received(1).GetAllFpsYearsPagedAsync(queryParameters);
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var queryParameters = new QueryParameters<int>
            {
                Page = 1,
                PageSize = 10
            };
            var expectedResponse = ApiResponseDto<List<YearMasterDto>>.SuccessResponse(
                new List<YearMasterDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            );

            _fpsYearMasterApiClient.GetAllFpsYearsPagedAsync(queryParameters).Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetAllFpsYearsPagedAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_WithMultiplePages_ReturnsCorrectPage()
        {
            // Arrange
            var queryParameters = new QueryParameters<int>
            {
                Page = 2,
                PageSize = 5
            };
            var yearMasters = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2019, FpsYearCode = "2019" },
                new YearMasterDto { FpsYear = 2018, FpsYearCode = "2018" }
            };
            var expectedResponse = ApiResponseDto<List<YearMasterDto>>.SuccessResponse(
                yearMasters,
                new PaginationDto
                {
                    PageNumber = 2,
                    PageSize = 5,
                    TotalPages = 3,
                    TotalRecords = 12
                }
            );

            _fpsYearMasterApiClient.GetAllFpsYearsPagedAsync(queryParameters).Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetAllFpsYearsPagedAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
        }

        [Theory]
        [InlineData(2024)]
        [InlineData(2025)]
        [InlineData(0)]
        public async Task GetAllYearMastersPagedAsync_WithDifferentFilters_PassesCorrectValue(int filter)
        {
            // Arrange
            var queryParameters = new QueryParameters<int>
            {
                Page = 1,
                PageSize = 10,
                Filter = filter
            };
            var expectedResponse = ApiResponseDto<List<YearMasterDto>>.SuccessResponse(
                new List<YearMasterDto>(),
                new PaginationDto()
            );

            _fpsYearMasterApiClient.GetAllFpsYearsPagedAsync(queryParameters).Returns(expectedResponse);

            // Act
            await _yearMasterService.GetAllFpsYearsPagedAsync(queryParameters);

            // Assert
            await _fpsYearMasterApiClient.Received(1).GetAllFpsYearsPagedAsync(queryParameters);
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var queryParameters = new QueryParameters<int> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<YearMasterDto>>.FailureResponse(
                errors,
                new ApiMetaDto()
            );

            _fpsYearMasterApiClient.GetAllFpsYearsPagedAsync(queryParameters).Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetAllFpsYearsPagedAsync(queryParameters);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_WithSortingParameters_PassesCorrectQuery()
        {
            // Arrange
            var queryParameters = new QueryParameters<int>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "FpsYear",
                Descending = true
            };
            var expectedResponse = ApiResponseDto<List<YearMasterDto>>.SuccessResponse(
                new List<YearMasterDto>(),
                new PaginationDto()
            );

            _fpsYearMasterApiClient.GetAllFpsYearsPagedAsync(queryParameters).Returns(expectedResponse);

            // Act
            await _yearMasterService.GetAllFpsYearsPagedAsync(queryParameters);

            // Assert
            await _fpsYearMasterApiClient.Received(1).GetAllFpsYearsPagedAsync(Arg.Is<QueryParameters<int>>(q =>
                q.SortBy == "FpsYear" &&
                q.Descending == true
            ));
        }

        #endregion

        #region GetYearMasterByIdAsync Tests

        [Fact]
        public async Task GetYearMasterByIdAsync_WithValidFpsYear_ReturnsYearMaster()
        {
            // Arrange
            var fpsYear = 2024;
            var yearMaster = new YearMasterDto
            {
                FpsYear = 2024,
                FpsYearCode = "2024",
                YearStatus = "Open",
                Active = true,
                Remarks = "Current fiscal year"
            };
            var expectedResponse = ApiResponseDto<YearMasterDto>.SuccessResponse(yearMaster);

            _fpsYearMasterApiClient.GetFpsYearByIdAsync(fpsYear).Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetFpsYearByIdAsync(fpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(fpsYear, result.Data.FpsYear);
            Assert.Equal("Open", result.Data.YearStatus);
            await _fpsYearMasterApiClient.Received(1).GetFpsYearByIdAsync(fpsYear);
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_WithNonExistentFpsYear_ReturnsFailureResponse()
        {
            // Arrange
            var fpsYear = 9999;
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Year master not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<YearMasterDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsYearMasterApiClient.GetFpsYearByIdAsync(fpsYear).Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetFpsYearByIdAsync(fpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData(2024)]
        [InlineData(2025)]
        [InlineData(2023)]
        public async Task GetYearMasterByIdAsync_WithVariousFpsYears_CallsApiClient(int fpsYear)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<YearMasterDto>.SuccessResponse(
                new YearMasterDto { FpsYear = fpsYear }
            );
            _fpsYearMasterApiClient.GetFpsYearByIdAsync(fpsYear).Returns(expectedResponse);

            // Act
            await _yearMasterService.GetFpsYearByIdAsync(fpsYear);

            // Assert
            await _fpsYearMasterApiClient.Received(1).GetFpsYearByIdAsync(fpsYear);
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_WithClosedYear_ReturnsClosedYearMaster()
        {
            // Arrange
            var fpsYear = 2023;
            var yearMaster = new YearMasterDto
            {
                FpsYear = 2023,
                FpsYearCode = "2023",
                YearStatus = "Closed",
                Active = true
            };
            var expectedResponse = ApiResponseDto<YearMasterDto>.SuccessResponse(yearMaster);

            _fpsYearMasterApiClient.GetFpsYearByIdAsync(fpsYear).Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetFpsYearByIdAsync(fpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Closed", result.Data?.YearStatus);
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_WithPlannedYear_ReturnsPlannedYearMaster()
        {
            // Arrange
            var fpsYear = 2025;
            var yearMaster = new YearMasterDto
            {
                FpsYear = 2025,
                FpsYearCode = "2025",
                YearStatus = "Planned",
                Active = true
            };
            var expectedResponse = ApiResponseDto<YearMasterDto>.SuccessResponse(yearMaster);

            _fpsYearMasterApiClient.GetFpsYearByIdAsync(fpsYear).Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetFpsYearByIdAsync(fpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Planned", result.Data?.YearStatus);
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_WhenApiReturnsError_ReturnsFailureResponse()
        {
            // Arrange
            var fpsYear = 2024;
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Internal server error", Code = "INTERNAL_ERROR" }
            };
            var expectedResponse = ApiResponseDto<YearMasterDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsYearMasterApiClient.GetFpsYearByIdAsync(fpsYear).Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetFpsYearByIdAsync(fpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public async Task GetAllYearMastersAsync_PassesExactResponse()
        {
            // Arrange
            var yearMasters = new List<YearMasterDto>
            {
                new YearMasterDto
                {
                    FpsYear = 2024,
                    FpsYearCode = "2024",
                    YearStatus = "Open",
                    Active = true,
                    Remarks = "Test"
                }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(yearMasters);

            _fpsYearMasterApiClient.GetAllFpsYearsAsync().Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetAllFpsYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Same(expectedResponse, result);
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_PassesExactQueryObject()
        {
            // Arrange
            var queryParameters = new QueryParameters<int>
            {
                Page = 1,
                PageSize = 10,
                Filter = 2024,
                SortBy = "FpsYear",
                Descending = true
            };
            var expectedResponse = ApiResponseDto<List<YearMasterDto>>.SuccessResponse(
                new List<YearMasterDto>(),
                new PaginationDto()
            );

            _fpsYearMasterApiClient.GetAllFpsYearsPagedAsync(queryParameters).Returns(expectedResponse);

            // Act
            await _yearMasterService.GetAllFpsYearsPagedAsync(queryParameters);

            // Assert
            await _fpsYearMasterApiClient.Received(1).GetAllFpsYearsPagedAsync(Arg.Is<QueryParameters<int>>(q =>
                q.Page == 1 &&
                q.PageSize == 10 &&
                q.Filter == 2024 &&
                q.SortBy == "FpsYear" &&
                q.Descending == true
            ));
        }

        [Fact]
        public async Task GetYearMasterByIdAsync_PassesExactFpsYear()
        {
            // Arrange
            var fpsYear = 2024;
            var yearMaster = new YearMasterDto { FpsYear = fpsYear };
            var expectedResponse = ApiResponseDto<YearMasterDto>.SuccessResponse(yearMaster);

            _fpsYearMasterApiClient.GetFpsYearByIdAsync(fpsYear).Returns(expectedResponse);

            // Act
            await _yearMasterService.GetFpsYearByIdAsync(fpsYear);

            // Assert
            await _fpsYearMasterApiClient.Received(1).GetFpsYearByIdAsync(Arg.Is<int>(y => y == fpsYear));
        }

        [Fact]
        public async Task GetAllYearMastersPagedAsync_CallsApiClientOnce()
        {
            // Arrange
            var queryParameters = new QueryParameters<int> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<YearMasterDto>>.SuccessResponse(
                new List<YearMasterDto>(),
                new PaginationDto()
            );

            _fpsYearMasterApiClient.GetAllFpsYearsPagedAsync(queryParameters).Returns(expectedResponse);

            // Act
            await _yearMasterService.GetAllFpsYearsPagedAsync(queryParameters);

            // Assert
            await _fpsYearMasterApiClient.Received(1).GetAllFpsYearsPagedAsync(Arg.Any<QueryParameters<int>>());
        }

        #endregion

        #region GetFpsPlannedYearAsync

        [Fact]
        public async Task GetFpsPlannedYearAsync_WhenPlannedYearExists_ReturnsPlannedFpsYear()
        {
            // Arrange
            var yearMasters = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2025, YearStatus = "Planned", Active = true },
                new YearMasterDto { FpsYear = 2024, YearStatus = "Open",    Active = true }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(yearMasters);
            _fpsYearMasterApiClient.GetAllFpsYearsAsync().Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetFpsPlannedYearAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2025, result.Data);
            await _fpsYearMasterApiClient.Received(1).GetAllFpsYearsAsync();
        }

        [Fact]
        public async Task GetFpsPlannedYearAsync_WhenNoPlannedYearExists_ReturnsOpenYearPlusOne()
        {
            // Arrange
            var yearMasters = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2024, YearStatus = "Open", Active = true },
                new YearMasterDto { FpsYear = 2023, YearStatus = "Closed", Active = true }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(yearMasters);
            _fpsYearMasterApiClient.GetAllFpsYearsAsync().Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetFpsPlannedYearAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2025, result.Data);   // open (2024) + 1
            await _fpsYearMasterApiClient.Received(1).GetAllFpsYearsAsync();
        }

        [Fact]
        public async Task GetFpsPlannedYearAsync_WhenMultiplePlannedYearsExist_ReturnsHighestPlannedYear()
        {
            // Arrange
            var yearMasters = new List<YearMasterDto>
            {
                new YearMasterDto { FpsYear = 2027, YearStatus = "Planned", Active = true },
                new YearMasterDto { FpsYear = 2026, YearStatus = "Planned", Active = true },
                new YearMasterDto { FpsYear = 2024, YearStatus = "Open",    Active = true }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.SuccessResponse(yearMasters);
            _fpsYearMasterApiClient.GetAllFpsYearsAsync().Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetFpsPlannedYearAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2027, result.Data);
            await _fpsYearMasterApiClient.Received(1).GetAllFpsYearsAsync();
        }

        [Fact]
        public async Task GetFpsPlannedYearAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<YearMasterDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsYearMasterApiClient.GetAllFpsYearsAsync().Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetFpsPlannedYearAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _fpsYearMasterApiClient.Received(1).GetAllFpsYearsAsync();
        }

        [Fact]
        public async Task GetFpsPlannedYearAsync_WhenApiReturnsNullData_ReturnsFailureResponse()
        {
            // Arrange
            var expectedResponse = new ApiResponseDto<IEnumerable<YearMasterDto>>
            {
                Success = false,
                Data = null,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "No data", Code = "NO_DATA" } },
                Meta = new ApiMetaDto()
            };
            _fpsYearMasterApiClient.GetAllFpsYearsAsync().Returns(expectedResponse);

            // Act
            var result = await _yearMasterService.GetFpsPlannedYearAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _fpsYearMasterApiClient.Received(1).GetAllFpsYearsAsync();
        }

        [Fact]
        public async Task GetFpsPlannedYearAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _fpsYearMasterApiClient.GetAllFpsYearsAsync().ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _yearMasterService.GetFpsPlannedYearAsync());
            Assert.Equal("API unavailable", exception.Message);
            await _fpsYearMasterApiClient.Received(1).GetAllFpsYearsAsync();
        }

        #endregion
    }
}
