using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsMonthHourApiClientTest
{
    public class FpsMonthHourApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsMonthHourApiClient _client;

        public FpsMonthHourApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsMonthHourApiClient(_http, _mapper);
        }

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        #region Constructor

        [Fact]
        public void Constructor_WhenHttpIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FpsMonthHourApiClient(null!, _mapper));
        }

        [Fact]
        public void Constructor_WhenMapperIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FpsMonthHourApiClient(_http, null!));
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
            var resList = new List<MonthHourRes>
            {
                new MonthHourRes { Year = 2024, Month = 1, Days = 20, FpsYear = 2024 },
                new MonthHourRes { Year = 2024, Month = 2, Days = 18, FpsYear = 2024 }
            };
            var apiResponse = new ApiResponse<List<MonthHourRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<List<MonthHourDto>>.SuccessResponse(
            [
                new MonthHourDto { Year = 2024, Month = 1, Days = 20, FpsYear = 2024 },
                new MonthHourDto { Year = 2024, Month = 2, Days = 18, FpsYear = 2024 }
            ]);

            _http.GetAsync<List<MonthHourRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonthHourDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllMonthHourAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<MonthHourRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<MonthHourDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllMonthHourAsync_WhenApiReturnsEmptyList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<MonthHourRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<MonthHourDto>>.SuccessResponse([]);

            _http.GetAsync<List<MonthHourRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonthHourDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllMonthHourAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _http.Received(1).GetAsync<List<MonthHourRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetAllMonthHourAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new ApiError { Message = "Unauthorized", Code = "UNAUTHORIZED" } };
            var apiResponse = new ApiResponse<List<MonthHourRes>> { Success = false, Errors = errors };
            var mappedFailure = new ApiResponseDto<List<MonthHourDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Unauthorized", Code = "UNAUTHORIZED" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<MonthHourRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonthHourDto>>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetAllMonthHourAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("UNAUTHORIZED", result.Errors![0].Code);
            await _http.Received(1).GetAsync<List<MonthHourRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetAllMonthHourAsync_CallsCorrectBaseEndpoint()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<MonthHourRes>> { Success = true, Data = [] };
            _http.GetAsync<List<MonthHourRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonthHourDto>>>(Arg.Any<object>())
                   .Returns(ApiResponseDto<List<MonthHourDto>>.SuccessResponse([]));

            // Act
            await _client.GetAllMonthHourAsync(query);

            // Assert — verify the URL contains the base endpoint
            await _http.Received(1).GetAsync<List<MonthHourRes>>(
                Arg.Is<string>(url => url.StartsWith("api/v1/monthhour")));
        }

        #endregion

        // -----------------------------------------------------------------------
        // GetMonthHoursByYearAsync
        // -----------------------------------------------------------------------

        #region GetMonthHoursByYearAsync

        [Fact]
        public async Task GetMonthHoursByYearAsync_WhenApiReturnsSuccess_ReturnsMappedCollection()
        {
            // Arrange
            const short year = 2024;
            var resList = new List<MonthHourRes>
            {
                new MonthHourRes { Year = year, Month = 1, Days = 20, FpsYear = 2024 },
                new MonthHourRes { Year = year, Month = 2, Days = 18, FpsYear = 2024 }
            };
            var apiResponse = new ApiResponse<List<MonthHourRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<IEnumerable<MonthHourDto>>.SuccessResponse(
            [
                new MonthHourDto { Year = year, Month = 1, Days = 20, FpsYear = 2024 },
                new MonthHourDto { Year = year, Month = 2, Days = 18, FpsYear = 2024 }
            ]);

            _http.GetAsync<List<MonthHourRes>>($"api/v1/monthhour/year/{year}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<MonthHourDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetMonthHoursByYearAsync(year);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count());
            await _http.Received(1).GetAsync<List<MonthHourRes>>($"api/v1/monthhour/year/{year}");
            _mapper.Received(1).Map<ApiResponseDto<IEnumerable<MonthHourDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetMonthHoursByYearAsync_WhenApiReturnsEmptyList_ReturnsSuccessWithEmptyCollection()
        {
            // Arrange
            const short year = 2099;
            var apiResponse = new ApiResponse<List<MonthHourRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<IEnumerable<MonthHourDto>>.SuccessResponse([]);

            _http.GetAsync<List<MonthHourRes>>($"api/v1/monthhour/year/{year}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<MonthHourDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetMonthHoursByYearAsync(year);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _http.Received(1).GetAsync<List<MonthHourRes>>($"api/v1/monthhour/year/{year}");
        }

        [Fact]
        public async Task GetMonthHoursByYearAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const short year = 2024;
            var errors = new List<ApiError> { new ApiError { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<MonthHourRes>> { Success = false, Errors = errors };
            var mappedFailure = new ApiResponseDto<IEnumerable<MonthHourDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<MonthHourRes>>($"api/v1/monthhour/year/{year}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<MonthHourDto>>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetMonthHoursByYearAsync(year);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _http.Received(1).GetAsync<List<MonthHourRes>>($"api/v1/monthhour/year/{year}");
        }

        [Theory]
        [InlineData((short)2023)]
        [InlineData((short)2024)]
        [InlineData((short)2025)]
        public async Task GetMonthHoursByYearAsync_CallsCorrectUrlForYear(short year)
        {
            // Arrange
            var apiResponse = new ApiResponse<List<MonthHourRes>> { Success = true, Data = [] };
            _http.GetAsync<List<MonthHourRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<MonthHourDto>>>(Arg.Any<object>())
                   .Returns(ApiResponseDto<IEnumerable<MonthHourDto>>.SuccessResponse([]));

            // Act
            await _client.GetMonthHoursByYearAsync(year);

            // Assert
            await _http.Received(1).GetAsync<List<MonthHourRes>>($"api/v1/monthhour/year/{year}");
        }

        #endregion

        // -----------------------------------------------------------------------
        // GetDistinctYearsAsync
        // -----------------------------------------------------------------------

        #region GetDistinctYearsAsync

        [Fact]
        public async Task GetDistinctYearsAsync_WhenApiReturnsSuccess_ReturnsMappedYears()
        {
            // Arrange
            var resList = new List<short> { 2022, 2023, 2024 };
            var apiResponse = new ApiResponse<List<short>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<IEnumerable<short>>.SuccessResponse(resList);

            _http.GetAsync<List<short>>("api/v1/monthhour/years").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<short>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetDistinctYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data?.Count());
            await _http.Received(1).GetAsync<List<short>>("api/v1/monthhour/years");
            _mapper.Received(1).Map<ApiResponseDto<IEnumerable<short>>>(apiResponse);
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WhenApiReturnsEmptyList_ReturnsSuccessWithEmptyCollection()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<short>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<IEnumerable<short>>.SuccessResponse([]);

            _http.GetAsync<List<short>>("api/v1/monthhour/years").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<short>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetDistinctYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _http.Received(1).GetAsync<List<short>>("api/v1/monthhour/years");
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Service error", Code = "SERVICE_ERROR" } };
            var apiResponse = new ApiResponse<List<short>> { Success = false, Errors = errors };
            var mappedFailure = new ApiResponseDto<IEnumerable<short>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Service error", Code = "SERVICE_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<short>>("api/v1/monthhour/years").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<short>>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetDistinctYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _http.Received(1).GetAsync<List<short>>("api/v1/monthhour/years");
        }

        [Fact]
        public async Task GetDistinctYearsAsync_CallsCorrectEndpointUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<short>> { Success = true, Data = [] };
            _http.GetAsync<List<short>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<short>>>(Arg.Any<object>())
                   .Returns(ApiResponseDto<IEnumerable<short>>.SuccessResponse([]));

            // Act
            await _client.GetDistinctYearsAsync();

            // Assert
            await _http.Received(1).GetAsync<List<short>>("api/v1/monthhour/years");
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
            var resList = new List<YearEndMonthHourRes>
            {
                new YearEndMonthHourRes { Month = 1, Days = 20, ExistsForPlannedYear = "Yes", FpsYear = 2025 },
                new YearEndMonthHourRes { Month = 2, Days = 18, ExistsForPlannedYear = "No",  FpsYear = 2025 }
            };
            var apiResponse = new ApiResponse<List<YearEndMonthHourRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<List<YearEndMonthHourDto>>.SuccessResponse(
            [
                new YearEndMonthHourDto { Month = 1, Days = 20, ExistsForPlannedYear = "Yes", FpsYear = 2025 },
                new YearEndMonthHourDto { Month = 2, Days = 18, ExistsForPlannedYear = "No",  FpsYear = 2025 }
            ]);

            _http.GetAsync<List<YearEndMonthHourRes>>("api/v1/monthhour/yearend").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<YearEndMonthHourDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetYearEndMonthHoursAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<YearEndMonthHourRes>>("api/v1/monthhour/yearend");
            _mapper.Received(1).Map<ApiResponseDto<List<YearEndMonthHourDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetYearEndMonthHoursAsync_WhenApiReturnsEmptyList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<YearEndMonthHourRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<YearEndMonthHourDto>>.SuccessResponse([]);

            _http.GetAsync<List<YearEndMonthHourRes>>("api/v1/monthhour/yearend").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<YearEndMonthHourDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetYearEndMonthHoursAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _http.Received(1).GetAsync<List<YearEndMonthHourRes>>("api/v1/monthhour/yearend");
        }

        [Fact]
        public async Task GetYearEndMonthHoursAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<YearEndMonthHourRes>> { Success = false, Errors = errors };
            var mappedFailure = new ApiResponseDto<List<YearEndMonthHourDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<YearEndMonthHourRes>>("api/v1/monthhour/yearend").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<YearEndMonthHourDto>>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetYearEndMonthHoursAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _http.Received(1).GetAsync<List<YearEndMonthHourRes>>("api/v1/monthhour/yearend");
        }

        #endregion

        // -----------------------------------------------------------------------
        // SaveMonthHourAsync
        // -----------------------------------------------------------------------

        #region SaveMonthHourAsync

        [Fact]
        public async Task SaveMonthHourAsync_WhenApiReturnsSuccess_ReturnsMappedDto()
        {
            // Arrange
            var dto = new MonthHourDto { Year = 2024, Month = 3, Days = 20, VidHours = 5, CvlHours = 3, FpsYear = 2024 };
            var req = new MonthHourReq { Year = 2024, Month = 3, Days = 20, VidHours = 5, CvlHours = 3, FpsYear = 2024 };
            var apiResponse = new ApiResponse<MonthHourRes>
            {
                Success = true,
                Data = new MonthHourRes { Year = 2024, Month = 3, Days = 20, FpsYear = 2024 }
            };
            var expectedDto = ApiResponseDto<MonthHourDto>.SuccessResponse(
                new MonthHourDto { Year = 2024, Month = 3, Days = 20, FpsYear = 2024 });

            _mapper.Map<MonthHourReq>(dto).Returns(req);
            _http.PostAsync<MonthHourReq, MonthHourRes>("api/v1/monthhour/save", req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthHourDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.SaveMonthHourAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal((short)3, result.Data?.Month);
            await _http.Received(1).PostAsync<MonthHourReq, MonthHourRes>("api/v1/monthhour/save", req);
            _mapper.Received(1).Map<ApiResponseDto<MonthHourDto>>(apiResponse);
        }

        [Fact]
        public async Task SaveMonthHourAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new MonthHourDto { Year = 2024, Month = 1, Days = -1 };
            var req = new MonthHourReq { Year = 2024, Month = 1, Days = -1 };
            var errors = new List<ApiError> { new ApiError { Message = "Validation error", Code = "VALIDATION_ERROR" } };
            var apiResponse = new ApiResponse<MonthHourRes> { Success = false, Errors = errors };
            var mappedFailure = new ApiResponseDto<MonthHourDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Validation error", Code = "VALIDATION_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _mapper.Map<MonthHourReq>(dto).Returns(req);
            _http.PostAsync<MonthHourReq, MonthHourRes>("api/v1/monthhour/save", req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthHourDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.SaveMonthHourAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _http.Received(1).PostAsync<MonthHourReq, MonthHourRes>("api/v1/monthhour/save", req);
        }

        [Fact]
        public async Task SaveMonthHourAsync_MapsInputDtoToRequestBeforePosting()
        {
            // Arrange
            var dto = new MonthHourDto { Year = 2024, Month = 6, Days = 21, FpsYear = 2024 };
            var req = new MonthHourReq { Year = 2024, Month = 6, Days = 21, FpsYear = 2024 };
            var apiResponse = new ApiResponse<MonthHourRes> { Success = true, Data = new MonthHourRes() };
            var expectedDto = ApiResponseDto<MonthHourDto>.SuccessResponse(new MonthHourDto());

            _mapper.Map<MonthHourReq>(dto).Returns(req);
            _http.PostAsync<MonthHourReq, MonthHourRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthHourDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.SaveMonthHourAsync(dto);

            // Assert
            _mapper.Received(1).Map<MonthHourReq>(dto);
        }

        [Fact]
        public async Task SaveMonthHourAsync_CallsCorrectEndpointUrl()
        {
            // Arrange
            var dto = new MonthHourDto { Year = 2024, Month = 1, Days = 20, FpsYear = 2024 };
            var req = new MonthHourReq { Year = 2024, Month = 1, Days = 20, FpsYear = 2024 };
            var apiResponse = new ApiResponse<MonthHourRes> { Success = true, Data = new MonthHourRes() };

            _mapper.Map<MonthHourReq>(dto).Returns(req);
            _http.PostAsync<MonthHourReq, MonthHourRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthHourDto>>(apiResponse)
                   .Returns(ApiResponseDto<MonthHourDto>.SuccessResponse(new MonthHourDto()));

            // Act
            await _client.SaveMonthHourAsync(dto);

            // Assert
            await _http.Received(1).PostAsync<MonthHourReq, MonthHourRes>("api/v1/monthhour/save", req);
        }

        #endregion
    }
}
