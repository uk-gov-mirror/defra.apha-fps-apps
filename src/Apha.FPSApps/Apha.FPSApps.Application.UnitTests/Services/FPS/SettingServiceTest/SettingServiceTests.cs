using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.SettingServiceTest
{
    public class SettingServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsSettingApiClient _fpsSettingApiClient;
        private readonly SettingService _sut;

        public SettingServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsSettingApiClient = Substitute.For<IFpsSettingApiClient>();
            _fpsClient.FpsSetting.Returns(_fpsSettingApiClient);
            _sut = new SettingService(_fpsClient);
        }

        #region GetHoursPerDayAsync

        [Fact]
        public async Task GetHoursPerDayAsync_WhenApiReturnsSuccess_ReturnsSuccessResponseWithValue()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<decimal>.SuccessResponse(7.5m);
            _fpsSettingApiClient.GetHoursPerDayAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(7.5m, result.Data);
            await _fpsSettingApiClient.Received(1).GetHoursPerDayAsync();
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenApiReturnsDefaultValue_ReturnsSuccessResponseWithEight()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<decimal>.SuccessResponse(8m);
            _fpsSettingApiClient.GetHoursPerDayAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(8m, result.Data);
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Setting not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<decimal>.FailureResponse(errors, new ApiMetaDto());
            _fpsSettingApiClient.GetHoursPerDayAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetHoursPerDayAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            var error = Assert.Single(result.Errors);
            Assert.Equal("Setting not found", error.Message);
            await _fpsSettingApiClient.Received(1).GetHoursPerDayAsync();
        }

        [Fact]
        public async Task GetHoursPerDayAsync_DelegatesToFpsSettingApiClient()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<decimal>.SuccessResponse(8m);
            _fpsSettingApiClient.GetHoursPerDayAsync().Returns(expectedResponse);

            // Act
            await _sut.GetHoursPerDayAsync();

            // Assert — verify delegation to the correct sub-client
            await _fpsSettingApiClient.Received(1).GetHoursPerDayAsync();
            await _fpsClient.Received(1).FpsSetting.GetHoursPerDayAsync();
        }

        [Fact]
        public async Task GetHoursPerDayAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _fpsSettingApiClient.GetHoursPerDayAsync().ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetHoursPerDayAsync());
            Assert.Equal("API unavailable", exception.Message);
            await _fpsSettingApiClient.Received(1).GetHoursPerDayAsync();
        }

        #endregion

        #region GetAllSettingsAsync

        [Fact]
        public async Task GetAllSettingsAsync_WhenApiReturnsSuccess_ReturnsMappedSettings()
        {
            // Arrange
            var settings = new List<SettingDto>
            {
                new SettingDto { Id = "HoursInDay", Setting = "7.5", FpsYear = 2024 },
                new SettingDto { Id = "CapApprovalReset", Setting = "true", FpsYear = 2024 }
            };
            var expectedResponse = ApiResponseDto<List<SettingDto>>.SuccessResponse(settings);
            _fpsSettingApiClient.GetAllSettingsAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllSettingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsSettingApiClient.Received(1).GetAllSettingsAsync();
        }

        [Fact]
        public async Task GetAllSettingsAsync_WhenApiReturnsEmptyList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<SettingDto>>.SuccessResponse(new List<SettingDto>());
            _fpsSettingApiClient.GetAllSettingsAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllSettingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _fpsSettingApiClient.Received(1).GetAllSettingsAsync();
        }

        [Fact]
        public async Task GetAllSettingsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Unauthorized", Code = "UNAUTHORIZED" } };
            var expectedResponse = ApiResponseDto<List<SettingDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsSettingApiClient.GetAllSettingsAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllSettingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _fpsSettingApiClient.Received(1).GetAllSettingsAsync();
        }

        [Fact]
        public async Task GetAllSettingsAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _fpsSettingApiClient.GetAllSettingsAsync().ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetAllSettingsAsync());
            Assert.Equal("API unavailable", exception.Message);
            await _fpsSettingApiClient.Received(1).GetAllSettingsAsync();
        }

        #endregion

        #region GetYearEndSettingsAsync

        [Fact]
        public async Task GetYearEndSettingsAsync_WhenApiReturnsSuccess_ReturnsMappedSettings()
        {
            // Arrange
            var settings = new List<YearEndSettingDto>
            {
                new YearEndSettingDto { Id = "HoursInDay", Setting = "7.5", FpsYear = 2024, ExistsForPlannedYear = "Yes" },
                new YearEndSettingDto { Id = "CapApprovalReset", Setting = "false", FpsYear = 2024, ExistsForPlannedYear = "No" }
            };
            var expectedResponse = ApiResponseDto<List<YearEndSettingDto>>.SuccessResponse(settings);
            _fpsSettingApiClient.GetYearEndSettingsAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetYearEndSettingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsSettingApiClient.Received(1).GetYearEndSettingsAsync();
        }

        [Fact]
        public async Task GetYearEndSettingsAsync_WhenApiReturnsEmptyList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<YearEndSettingDto>>.SuccessResponse(new List<YearEndSettingDto>());
            _fpsSettingApiClient.GetYearEndSettingsAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetYearEndSettingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _fpsSettingApiClient.Received(1).GetYearEndSettingsAsync();
        }

        [Fact]
        public async Task GetYearEndSettingsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<List<YearEndSettingDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsSettingApiClient.GetYearEndSettingsAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetYearEndSettingsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _fpsSettingApiClient.Received(1).GetYearEndSettingsAsync();
        }

        [Fact]
        public async Task GetYearEndSettingsAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _fpsSettingApiClient.GetYearEndSettingsAsync().ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetYearEndSettingsAsync());
            Assert.Equal("Service error", exception.Message);
            await _fpsSettingApiClient.Received(1).GetYearEndSettingsAsync();
        }

        #endregion

        #region AddSettingAsync

        [Fact]
        public async Task AddSettingAsync_WhenApiReturnsSuccess_ReturnsCreatedSetting()
        {
            // Arrange
            var dto = new SettingDto { Id = "NewKey", Setting = "NewValue", FpsYear = 2024 };
            var created = new SettingDto { Id = "NewKey", Setting = "NewValue", FpsYear = 2024 };
            var expectedResponse = ApiResponseDto<SettingDto>.SuccessResponse(created);
            _fpsSettingApiClient.AddSettingAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.AddSettingAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("NewKey", result.Data?.Id);
            await _fpsSettingApiClient.Received(1).AddSettingAsync(dto);
        }

        [Fact]
        public async Task AddSettingAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new SettingDto { Id = "DupKey" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Duplicate key", Code = "CONFLICT" } };
            var expectedResponse = ApiResponseDto<SettingDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsSettingApiClient.AddSettingAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.AddSettingAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _fpsSettingApiClient.Received(1).AddSettingAsync(dto);
        }

        [Fact]
        public async Task AddSettingAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            var dto = new SettingDto { Id = "Key" };
            _fpsSettingApiClient.AddSettingAsync(dto).ThrowsAsync(new Exception("Add failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.AddSettingAsync(dto));
            Assert.Equal("Add failed", exception.Message);
            await _fpsSettingApiClient.Received(1).AddSettingAsync(dto);
        }

        #endregion

        #region UpdateSettingAsync

        [Fact]
        public async Task UpdateSettingAsync_WhenApiReturnsSuccess_ReturnsUpdatedSetting()
        {
            // Arrange
            const string id = "HoursInDay";
            var dto = new SettingDto { Id = id, Setting = "8", FpsYear = 2024 };
            var updated = new SettingDto { Id = id, Setting = "8", FpsYear = 2024 };
            var expectedResponse = ApiResponseDto<SettingDto>.SuccessResponse(updated);
            _fpsSettingApiClient.UpdateSettingAsync(id, dto).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateSettingAsync(id, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("8", result.Data?.Setting);
            await _fpsSettingApiClient.Received(1).UpdateSettingAsync(id, dto);
        }

        [Fact]
        public async Task UpdateSettingAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string id = "MissingKey";
            var dto = new SettingDto { Id = id };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<SettingDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsSettingApiClient.UpdateSettingAsync(id, dto).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateSettingAsync(id, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _fpsSettingApiClient.Received(1).UpdateSettingAsync(id, dto);
        }

        [Fact]
        public async Task UpdateSettingAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            const string id = "Key";
            var dto = new SettingDto { Id = id };
            _fpsSettingApiClient.UpdateSettingAsync(id, dto).ThrowsAsync(new Exception("Update failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.UpdateSettingAsync(id, dto));
            Assert.Equal("Update failed", exception.Message);
            await _fpsSettingApiClient.Received(1).UpdateSettingAsync(id, dto);
        }

        #endregion

        #region SaveSettingAsync

        [Fact]
        public async Task SaveSettingAsync_WhenApiReturnsSuccess_ReturnsSavedSetting()
        {
            // Arrange
            var dto = new SettingDto { Id = "HoursInDay", Setting = "7.5", FpsYear = 2024 };
            var saved = new SettingDto { Id = "HoursInDay", Setting = "7.5", FpsYear = 2024 };
            var expectedResponse = ApiResponseDto<SettingDto>.SuccessResponse(saved);
            _fpsSettingApiClient.SaveSettingAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.SaveSettingAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("HoursInDay", result.Data?.Id);
            await _fpsSettingApiClient.Received(1).SaveSettingAsync(dto);
        }

        [Fact]
        public async Task SaveSettingAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new SettingDto { Id = "BadKey" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Validation failed", Code = "VALIDATION_ERROR" } };
            var expectedResponse = ApiResponseDto<SettingDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsSettingApiClient.SaveSettingAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.SaveSettingAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _fpsSettingApiClient.Received(1).SaveSettingAsync(dto);
        }

        [Fact]
        public async Task SaveSettingAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            var dto = new SettingDto { Id = "Key" };
            _fpsSettingApiClient.SaveSettingAsync(dto).ThrowsAsync(new Exception("Save failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.SaveSettingAsync(dto));
            Assert.Equal("Save failed", exception.Message);
            await _fpsSettingApiClient.Received(1).SaveSettingAsync(dto);
        }

        #endregion
    }
}
