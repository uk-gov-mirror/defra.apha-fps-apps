using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PIMS;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.PIMS.YearlyFinancialDataServiceTest
{
    public class YearlyFinancialDataServiceTests
    {
        private readonly IPimsApiClient                    _pimsApiClient;
        private readonly IPimsYearlyFinancialDataApiClient _pimsYearlyFinancialDataApiClient;
        private readonly YearlyFinancialDataService        _sut;

        public YearlyFinancialDataServiceTests()
        {
            _pimsApiClient                    = Substitute.For<IPimsApiClient>();
            _pimsYearlyFinancialDataApiClient = Substitute.For<IPimsYearlyFinancialDataApiClient>();

           
            _pimsApiClient.PimsYearlyFinancialData.Returns(_pimsYearlyFinancialDataApiClient);
            _sut = new YearlyFinancialDataService(_pimsApiClient);
        }

        // ── helpers ──────────────────────────────────────────────────────

        private static List<ApiErrorDto> OneError(string message = "API error", string code = "ERR")
            => [new ApiErrorDto { Message = message, Code = code }];

        private static QueryParameters<string> DefaultQuery()
            => new() { Page = 1, PageSize = 10 };

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidClient_InitializesService()
        {
            var service = new YearlyFinancialDataService(_pimsApiClient);
            Assert.NotNull(service);
        }

        #endregion

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ApiClientReturnsData_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var query = DefaultQuery();
            var data = new List<YearlyFinancialDataDto>
            {
                new() { Year = 2024, Project = "PP001" },
                new() { Year = 2023, Project = "PP001" }
            };
            var expected = new ApiResponseDto<List<YearlyFinancialDataDto>> { Success = true, Data = data };
            _pimsYearlyFinancialDataApiClient.GetAllAsync("PP001", query).Returns(expected);

            // Act
            var result = await _sut.GetAllAsync("PP001", query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pimsYearlyFinancialDataApiClient.Received(1).GetAllAsync("PP001", query);
        }

        [Fact]
        public async Task GetAllAsync_ApiClientReturnsEmptyList_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var query    = DefaultQuery();
            var expected = new ApiResponseDto<List<YearlyFinancialDataDto>> { Success = true, Data = [] };
            _pimsYearlyFinancialDataApiClient.GetAllAsync("PP001", query).Returns(expected);

            // Act
            var result = await _sut.GetAllAsync("PP001", query);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _pimsYearlyFinancialDataApiClient.Received(1).GetAllAsync("PP001", query);
        }

        [Fact]
        public async Task GetAllAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var query    = DefaultQuery();
            var expected = new ApiResponseDto<List<YearlyFinancialDataDto>>
            {
                Success = false,
                Errors  = OneError("Backend unavailable", "SERVER_ERROR")
            };
            _pimsYearlyFinancialDataApiClient.GetAllAsync("PP001", query).Returns(expected);

            // Act
            var result = await _sut.GetAllAsync("PP001", query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _pimsYearlyFinancialDataApiClient.Received(1).GetAllAsync("PP001", query);
        }

        #endregion

        #region GetByKeyAsync Tests

        [Fact]
        public async Task GetByKeyAsync_ApiClientReturnsData_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var dto      = new YearlyFinancialDataDto { Year = 2024, Project = "PP001" };
            var expected = new ApiResponseDto<YearlyFinancialDataDto> { Success = true, Data = dto };
            _pimsYearlyFinancialDataApiClient.GetByKeyAsync((short)2024, "PP001").Returns(expected);

            // Act
            var result = await _sut.GetByKeyAsync((short)2024, "PP001");

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal((short)2024, result.Data.Year);
            Assert.Equal("PP001",     result.Data.Project);
            await _pimsYearlyFinancialDataApiClient.Received(1).GetByKeyAsync((short)2024, "PP001");
        }

        [Fact]
        public async Task GetByKeyAsync_ApiClientReturnsNotFound_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<YearlyFinancialDataDto>
            {
                Success = false,
                Errors  = OneError("Not found", "NOT_FOUND")
            };
            _pimsYearlyFinancialDataApiClient.GetByKeyAsync(Arg.Any<short>(), Arg.Any<string>()).Returns(expected);

            // Act
            var result = await _sut.GetByKeyAsync((short)9999, "UNKNOWN");

            // Assert
            Assert.False(result.Success);
            await _pimsYearlyFinancialDataApiClient.Received(1).GetByKeyAsync((short)9999, "UNKNOWN");
        }

        [Fact]
        public async Task GetByKeyAsync_ApiClientReturnsNullData_ReturnsDelegatedResponseWithNullData()
        {
            // Arrange
            var expected = new ApiResponseDto<YearlyFinancialDataDto> { Success = true, Data = null };
            _pimsYearlyFinancialDataApiClient.GetByKeyAsync(Arg.Any<short>(), Arg.Any<string>()).Returns(expected);

            // Act
            var result = await _sut.GetByKeyAsync((short)2024, "PP001");

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Data);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var dto      = new YearlyFinancialDataDto { Year = 2024, Project = "PP001" };
            var expected = new ApiResponseDto<YearlyFinancialDataDto> { Success = true, Data = dto };
            _pimsYearlyFinancialDataApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _pimsYearlyFinancialDataApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var dto      = new YearlyFinancialDataDto { Year = 2024, Project = "PP001" };
            var expected = new ApiResponseDto<YearlyFinancialDataDto>
            {
                Success = false,
                Errors  = OneError("Duplicate record", "DUPLICATE_YEARLY_FINANCIAL_DATA")
            };
            _pimsYearlyFinancialDataApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _pimsYearlyFinancialDataApiClient.Received(1).CreateAsync(dto);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var dto      = new YearlyFinancialDataDto { Year = 2024, Project = "PP001" };
            var expected = new ApiResponseDto<YearlyFinancialDataDto> { Success = true, Data = dto };
            _pimsYearlyFinancialDataApiClient.UpdateAsync((short)2024, "PP001", dto).Returns(expected);

            // Act
            var result = await _sut.UpdateAsync((short)2024, "PP001", dto);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _pimsYearlyFinancialDataApiClient.Received(1).UpdateAsync((short)2024, "PP001", dto);
        }

        [Fact]
        public async Task UpdateAsync_ApiClientReturnsNotFound_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var dto      = new YearlyFinancialDataDto { Year = 9999, Project = "UNKNOWN" };
            var expected = new ApiResponseDto<YearlyFinancialDataDto>
            {
                Success = false,
                Errors  = OneError("Record not found", "NOT_FOUND")
            };
            _pimsYearlyFinancialDataApiClient.UpdateAsync(Arg.Any<short>(), Arg.Any<string>(), dto).Returns(expected);

            // Act
            var result = await _sut.UpdateAsync((short)9999, "UNKNOWN", dto);

            // Assert
            Assert.False(result.Success);
            await _pimsYearlyFinancialDataApiClient.Received(1).UpdateAsync((short)9999, "UNKNOWN", dto);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<object> { Success = true, Data = new object() };
            _pimsYearlyFinancialDataApiClient.DeleteAsync((short)2024, "PP001").Returns(expected);

            // Act
            var result = await _sut.DeleteAsync((short)2024, "PP001");

            // Assert
            Assert.True(result.Success);
            await _pimsYearlyFinancialDataApiClient.Received(1).DeleteAsync((short)2024, "PP001");
        }

        [Fact]
        public async Task DeleteAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<object>
            {
                Success = false,
                Errors  = OneError("Delete failed", "DELETE_ERROR")
            };
            _pimsYearlyFinancialDataApiClient.DeleteAsync(Arg.Any<short>(), Arg.Any<string>()).Returns(expected);

            // Act
            var result = await _sut.DeleteAsync((short)9999, "UNKNOWN");

            // Assert
            Assert.False(result.Success);
            await _pimsYearlyFinancialDataApiClient.Received(1).DeleteAsync((short)9999, "UNKNOWN");
        }

        #endregion

        #region GetPactCostsAsync Tests

        [Fact]
        public async Task GetPactCostsAsync_ApiClientReturnsData_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var dto      = new PactProjectYearCostsDto { Project = "PP001", Year = 2024 };
            var expected = new ApiResponseDto<PactProjectYearCostsDto> { Success = true, Data = dto };
            _pimsYearlyFinancialDataApiClient.GetPactCostsAsync("PP001", (short)2024).Returns(expected);

            // Act
            var result = await _sut.GetPactCostsAsync("PP001", (short)2024);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("PP001", result.Data.Project);
            await _pimsYearlyFinancialDataApiClient.Received(1).GetPactCostsAsync("PP001", (short)2024);
        }

        [Fact]
        public async Task GetPactCostsAsync_ApiClientReturnsNullData_ReturnsDelegatedResponseWithNullData()
        {
            // Arrange
            var expected = new ApiResponseDto<PactProjectYearCostsDto> { Success = true, Data = null };
            _pimsYearlyFinancialDataApiClient.GetPactCostsAsync(Arg.Any<string>(), Arg.Any<short>()).Returns(expected);

            // Act
            var result = await _sut.GetPactCostsAsync("PP001", (short)2024);

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetPactCostsAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<PactProjectYearCostsDto>
            {
                Success = false,
                Errors  = OneError("Pact data unavailable", "PACT_ERROR")
            };
            _pimsYearlyFinancialDataApiClient.GetPactCostsAsync(Arg.Any<string>(), Arg.Any<short>()).Returns(expected);

            // Act
            var result = await _sut.GetPactCostsAsync("PP001", (short)2024);

            // Assert
            Assert.False(result.Success);
            await _pimsYearlyFinancialDataApiClient.Received(1).GetPactCostsAsync("PP001", (short)2024);
        }

        #endregion

        #region GetSettingValueByIdAsync Tests

        [Fact]
        public async Task GetSettingValueByIdAsync_ApiClientReturnsValue_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<string> { Success = true, Data = "7.4" };
            _pimsYearlyFinancialDataApiClient.GetSettingValueByIdAsync("HoursInDay").Returns(expected);

            // Act
            var result = await _sut.GetSettingValueByIdAsync("HoursInDay");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("7.4", result.Data);
            await _pimsYearlyFinancialDataApiClient.Received(1).GetSettingValueByIdAsync("HoursInDay");
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_ApiClientReturnsNullData_ReturnsDelegatedResponseWithNullData()
        {
            // Arrange
            var expected = new ApiResponseDto<string> { Success = true, Data = null };
            _pimsYearlyFinancialDataApiClient.GetSettingValueByIdAsync(Arg.Any<string>()).Returns(expected);

            // Act
            var result = await _sut.GetSettingValueByIdAsync("UnknownSetting");

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Data);
            await _pimsYearlyFinancialDataApiClient.Received(1).GetSettingValueByIdAsync("UnknownSetting");
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<string>
            {
                Success = false,
                Errors  = OneError("Setting not found", "NOT_FOUND")
            };
            _pimsYearlyFinancialDataApiClient.GetSettingValueByIdAsync(Arg.Any<string>()).Returns(expected);

            // Act
            var result = await _sut.GetSettingValueByIdAsync("HoursInDay");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _pimsYearlyFinancialDataApiClient.Received(1).GetSettingValueByIdAsync("HoursInDay");
        }

        #endregion
    }
}
