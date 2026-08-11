using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsSettingApiClientTest
{
    public class FpsSettingApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsSettingApiClient _client;

        public FpsSettingApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsSettingApiClient(_http, _mapper);
        }

        #region GetHoursPerDayAsync

        [Fact]
        public async Task GetHoursPerDayAsync_WhenApiReturnsSuccess_ReturnsMappedDecimal()
        {
            // Arrange
            var apiResponse = new ApiResponse<decimal> { Success = true, Data = 7.5m };
            var expectedDto = ApiResponseDto<decimal>.SuccessResponse(7.5m);

            _http.GetAsync<decimal>("api/v1/setting/hoursperday").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetHoursPerDayAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(7.5m, result.Data);
            await _http.Received(1).GetAsync<decimal>("api/v1/setting/hoursperday");
            _mapper.Received(1).Map<ApiResponseDto<decimal>>(apiResponse);
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenApiReturnsDefaultValue_ReturnsMappedEight()
        {
            // Arrange
            var apiResponse = new ApiResponse<decimal> { Success = true, Data = 8m };
            var expectedDto = ApiResponseDto<decimal>.SuccessResponse(8m);

            _http.GetAsync<decimal>("api/v1/setting/hoursperday").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetHoursPerDayAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(8m, result.Data);
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<decimal> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<decimal>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<decimal>("api/v1/setting/hoursperday").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetHoursPerDayAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            var error = Assert.Single(result.Errors);
            Assert.Equal("NOT_FOUND", error.Code);
            await _http.Received(1).GetAsync<decimal>("api/v1/setting/hoursperday");
        }

        [Fact]
        public async Task GetHoursPerDayAsync_CallsCorrectEndpointUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<decimal> { Success = true, Data = 8m };
            _http.GetAsync<decimal>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(Arg.Any<object>())
                   .Returns(ApiResponseDto<decimal>.SuccessResponse(8m));

            // Act
            await _client.GetHoursPerDayAsync();

            // Assert
            await _http.Received(1).GetAsync<decimal>("api/v1/setting/hoursperday");
        }

        #endregion

        #region GetAllSettingsAsync

        [Fact]
        public async Task GetAllSettingsAsync_WhenApiReturnsSuccess_ReturnsMappedSettingList()
        {
            // Arrange
            var settingResList = new List<FpsSettingRes>
            {
                new FpsSettingRes { Id = "HoursInDay", Setting = "7.5", FpsYear = 2024 },
                new FpsSettingRes { Id = "CapApproval", Setting = "true", FpsYear = 2024 }
            };
            var apiResponse = new ApiResponse<List<FpsSettingRes>> { Success = true, Data = settingResList };
            var expectedDto = ApiResponseDto<List<SettingDto>>.SuccessResponse(
            [
                new SettingDto { Id = "HoursInDay", Setting = "7.5", FpsYear = 2024 },
                new SettingDto { Id = "CapApproval", Setting = "true", FpsYear = 2024 }
            ]);

            _http.GetAsync<List<FpsSettingRes>>("api/v1/setting").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<SettingDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllSettingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<FpsSettingRes>>("api/v1/setting");
            _mapper.Received(1).Map<ApiResponseDto<List<SettingDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllSettingsAsync_WhenApiReturnsEmptyList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<FpsSettingRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<SettingDto>>.SuccessResponse([]);

            _http.GetAsync<List<FpsSettingRes>>("api/v1/setting").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<SettingDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllSettingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _http.Received(1).GetAsync<List<FpsSettingRes>>("api/v1/setting");
        }

        [Fact]
        public async Task GetAllSettingsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Unauthorized", Code = "UNAUTHORIZED" } };
            var apiResponse = new ApiResponse<List<FpsSettingRes>> { Success = false, Errors = errors };
            var mappedFailure = new ApiResponseDto<List<SettingDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Unauthorized", Code = "UNAUTHORIZED" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<FpsSettingRes>>("api/v1/setting").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<SettingDto>>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetAllSettingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("UNAUTHORIZED", result.Errors![0].Code);
            await _http.Received(1).GetAsync<List<FpsSettingRes>>("api/v1/setting");
        }

        [Fact]
        public async Task GetAllSettingsAsync_CallsCorrectEndpointUrl()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<FpsSettingRes>> { Success = true, Data = [] };
            _http.GetAsync<List<FpsSettingRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<SettingDto>>>(Arg.Any<object>())
                   .Returns(ApiResponseDto<List<SettingDto>>.SuccessResponse([]));

            // Act
            await _client.GetAllSettingsAsync();

            // Assert
            await _http.Received(1).GetAsync<List<FpsSettingRes>>("api/v1/setting");
        }

        #endregion

        #region GetYearEndSettingsAsync

        [Fact]
        public async Task GetYearEndSettingsAsync_WhenApiReturnsSuccess_ReturnsMappedList()
        {
            // Arrange
            var resList = new List<FpsYearEndSettingRes>
            {
                new FpsYearEndSettingRes { Id = "HoursInDay", Setting = "7.5", FpsYear = 2024, ExistsForPlannedYear = "Yes" },
                new FpsYearEndSettingRes { Id = "CapApproval", Setting = "false", FpsYear = 2024, ExistsForPlannedYear = "No" }
            };
            var apiResponse = new ApiResponse<List<FpsYearEndSettingRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<List<YearEndSettingDto>>.SuccessResponse(
            [
                new YearEndSettingDto { Id = "HoursInDay", Setting = "7.5", FpsYear = 2024, ExistsForPlannedYear = "Yes" },
                new YearEndSettingDto { Id = "CapApproval", Setting = "false", FpsYear = 2024, ExistsForPlannedYear = "No" }
            ]);

            _http.GetAsync<List<FpsYearEndSettingRes>>("api/v1/setting/yearend").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<YearEndSettingDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetYearEndSettingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<FpsYearEndSettingRes>>("api/v1/setting/yearend");
            _mapper.Received(1).Map<ApiResponseDto<List<YearEndSettingDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetYearEndSettingsAsync_WhenApiReturnsEmptyList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<FpsYearEndSettingRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<YearEndSettingDto>>.SuccessResponse([]);

            _http.GetAsync<List<FpsYearEndSettingRes>>("api/v1/setting/yearend").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<YearEndSettingDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetYearEndSettingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _http.Received(1).GetAsync<List<FpsYearEndSettingRes>>("api/v1/setting/yearend");
        }

        [Fact]
        public async Task GetYearEndSettingsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<FpsYearEndSettingRes>> { Success = false, Errors = errors };
            var mappedFailure = new ApiResponseDto<List<YearEndSettingDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<FpsYearEndSettingRes>>("api/v1/setting/yearend").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<YearEndSettingDto>>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetYearEndSettingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _http.Received(1).GetAsync<List<FpsYearEndSettingRes>>("api/v1/setting/yearend");
        }

        #endregion

        #region AddSettingAsync

        [Fact]
        public async Task AddSettingAsync_WhenApiReturnsSuccess_ReturnsMappedSettingDto()
        {
            // Arrange
            var dto = new SettingDto { Id = "NewKey", Setting = "NewValue", FpsYear = 2024 };
            var req = new FpsSettingReq { Id = "NewKey", Setting = "NewValue", FpsYear = 2024 };
            var apiResponse = new ApiResponse<FpsSettingRes>
            {
                Success = true,
                Data = new FpsSettingRes { Id = "NewKey", Setting = "NewValue", FpsYear = 2024 }
            };
            var expectedDto = ApiResponseDto<SettingDto>.SuccessResponse(
                new SettingDto { Id = "NewKey", Setting = "NewValue", FpsYear = 2024 });

            _mapper.Map<FpsSettingReq>(dto).Returns(req);
            _http.PostAsync<FpsSettingReq, FpsSettingRes>("api/v1/setting", req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SettingDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.AddSettingAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("NewKey", result.Data?.Id);
            await _http.Received(1).PostAsync<FpsSettingReq, FpsSettingRes>("api/v1/setting", req);
            _mapper.Received(1).Map<ApiResponseDto<SettingDto>>(apiResponse);
        }

        [Fact]
        public async Task AddSettingAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new SettingDto { Id = "DupKey" };
            var req = new FpsSettingReq { Id = "DupKey" };
            var errors = new List<ApiError> { new ApiError { Message = "Conflict", Code = "CONFLICT" } };
            var apiResponse = new ApiResponse<FpsSettingRes> { Success = false, Errors = errors };
            var mappedFailure = new ApiResponseDto<SettingDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Conflict", Code = "CONFLICT" }],
                Meta = new ApiMetaDto()
            };

            _mapper.Map<FpsSettingReq>(dto).Returns(req);
            _http.PostAsync<FpsSettingReq, FpsSettingRes>("api/v1/setting", req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SettingDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.AddSettingAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _http.Received(1).PostAsync<FpsSettingReq, FpsSettingRes>("api/v1/setting", req);
        }

        [Fact]
        public async Task AddSettingAsync_MapsInputDtoToRequestBeforePosting()
        {
            // Arrange
            var dto = new SettingDto { Id = "Key" };
            var req = new FpsSettingReq { Id = "Key" };
            var apiResponse = new ApiResponse<FpsSettingRes> { Success = true, Data = new FpsSettingRes() };
            var expectedDto = ApiResponseDto<SettingDto>.SuccessResponse(new SettingDto());

            _mapper.Map<FpsSettingReq>(dto).Returns(req);
            _http.PostAsync<FpsSettingReq, FpsSettingRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SettingDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.AddSettingAsync(dto);

            // Assert
            _mapper.Received(1).Map<FpsSettingReq>(dto);
        }

        #endregion

        #region UpdateSettingAsync

        [Fact]
        public async Task UpdateSettingAsync_WhenApiReturnsSuccess_ReturnsMappedSettingDto()
        {
            // Arrange
            const string id = "HoursInDay";
            var dto = new SettingDto { Id = id, Setting = "8", FpsYear = 2024 };
            var req = new FpsSettingReq { Id = id, Setting = "8", FpsYear = 2024 };
            var apiResponse = new ApiResponse<FpsSettingRes>
            {
                Success = true,
                Data = new FpsSettingRes { Id = id, Setting = "8", FpsYear = 2024 }
            };
            var expectedDto = ApiResponseDto<SettingDto>.SuccessResponse(
                new SettingDto { Id = id, Setting = "8", FpsYear = 2024 });

            _mapper.Map<FpsSettingReq>(dto).Returns(req);
            _http.PutAsync<FpsSettingReq, FpsSettingRes>($"api/v1/setting/{id}", req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SettingDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateSettingAsync(id, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("8", result.Data?.Setting);
            await _http.Received(1).PutAsync<FpsSettingReq, FpsSettingRes>($"api/v1/setting/{id}", req);
            _mapper.Received(1).Map<ApiResponseDto<SettingDto>>(apiResponse);
        }

        [Fact]
        public async Task UpdateSettingAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string id = "MissingKey";
            var dto = new SettingDto { Id = id };
            var req = new FpsSettingReq { Id = id };
            var errors = new List<ApiError> { new ApiError { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<FpsSettingRes> { Success = false, Errors = errors };
            var mappedFailure = new ApiResponseDto<SettingDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" }],
                Meta = new ApiMetaDto()
            };

            _mapper.Map<FpsSettingReq>(dto).Returns(req);
            _http.PutAsync<FpsSettingReq, FpsSettingRes>($"api/v1/setting/{id}", req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SettingDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.UpdateSettingAsync(id, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _http.Received(1).PutAsync<FpsSettingReq, FpsSettingRes>($"api/v1/setting/{id}", req);
        }

        [Fact]
        public async Task UpdateSettingAsync_UsesFormattedUrlWithId()
        {
            // Arrange
            const string id = "MyKey";
            var dto = new SettingDto { Id = id };
            var req = new FpsSettingReq { Id = id };
            var apiResponse = new ApiResponse<FpsSettingRes> { Success = true, Data = new FpsSettingRes() };

            _mapper.Map<FpsSettingReq>(dto).Returns(req);
            _http.PutAsync<FpsSettingReq, FpsSettingRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SettingDto>>(apiResponse)
                   .Returns(ApiResponseDto<SettingDto>.SuccessResponse(new SettingDto()));

            // Act
            await _client.UpdateSettingAsync(id, dto);

            // Assert
            await _http.Received(1).PutAsync<FpsSettingReq, FpsSettingRes>($"api/v1/setting/{id}", req);
        }

        #endregion

        #region SaveSettingAsync

        [Fact]
        public async Task SaveSettingAsync_WhenApiReturnsSuccess_ReturnsMappedSettingDto()
        {
            // Arrange
            var dto = new SettingDto { Id = "HoursInDay", Setting = "7.5", FpsYear = 2024 };
            var req = new FpsSettingReq { Id = "HoursInDay", Setting = "7.5", FpsYear = 2024 };
            var apiResponse = new ApiResponse<FpsSettingRes>
            {
                Success = true,
                Data = new FpsSettingRes { Id = "HoursInDay", Setting = "7.5", FpsYear = 2024 }
            };
            var expectedDto = ApiResponseDto<SettingDto>.SuccessResponse(
                new SettingDto { Id = "HoursInDay", Setting = "7.5", FpsYear = 2024 });

            _mapper.Map<FpsSettingReq>(dto).Returns(req);
            _http.PostAsync<FpsSettingReq, FpsSettingRes>("api/v1/setting/save", req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SettingDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.SaveSettingAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("HoursInDay", result.Data?.Id);
            await _http.Received(1).PostAsync<FpsSettingReq, FpsSettingRes>("api/v1/setting/save", req);
            _mapper.Received(1).Map<ApiResponseDto<SettingDto>>(apiResponse);
        }

        [Fact]
        public async Task SaveSettingAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new SettingDto { Id = "BadKey" };
            var req = new FpsSettingReq { Id = "BadKey" };
            var errors = new List<ApiError> { new ApiError { Message = "Validation error", Code = "VALIDATION_ERROR" } };
            var apiResponse = new ApiResponse<FpsSettingRes> { Success = false, Errors = errors };
            var mappedFailure = new ApiResponseDto<SettingDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Validation error", Code = "VALIDATION_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _mapper.Map<FpsSettingReq>(dto).Returns(req);
            _http.PostAsync<FpsSettingReq, FpsSettingRes>("api/v1/setting/save", req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SettingDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.SaveSettingAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _http.Received(1).PostAsync<FpsSettingReq, FpsSettingRes>("api/v1/setting/save", req);
        }

        [Fact]
        public async Task SaveSettingAsync_CallsCorrectEndpointUrl()
        {
            // Arrange
            var dto = new SettingDto { Id = "Key" };
            var req = new FpsSettingReq { Id = "Key" };
            var apiResponse = new ApiResponse<FpsSettingRes> { Success = true, Data = new FpsSettingRes() };

            _mapper.Map<FpsSettingReq>(dto).Returns(req);
            _http.PostAsync<FpsSettingReq, FpsSettingRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<SettingDto>>(apiResponse)
                   .Returns(ApiResponseDto<SettingDto>.SuccessResponse(new SettingDto()));

            // Act
            await _client.SaveSettingAsync(dto);

            // Assert
            await _http.Received(1).PostAsync<FpsSettingReq, FpsSettingRes>("api/v1/setting/save", req);
        }

        #endregion
    }
}
