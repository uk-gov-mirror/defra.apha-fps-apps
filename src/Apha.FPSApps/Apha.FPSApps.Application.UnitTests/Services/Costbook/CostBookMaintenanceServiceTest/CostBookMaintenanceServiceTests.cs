using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Services.Costbook;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.Costbook.CostBookMaintenanceServiceTest
{
    public class CostBookMaintenanceServiceTests
    {
        
        private readonly ICostBookApiClient _costBookClient;
        private readonly ICostBookMaintenanceApiClient _maintenanceApiClient;
        private readonly CostBookMaintenanceService _service;

        public CostBookMaintenanceServiceTests()
        {
            _costBookClient       = Substitute.For<ICostBookApiClient>();
            _maintenanceApiClient = Substitute.For<ICostBookMaintenanceApiClient>();            
            _costBookClient.CostbookMaintenance.Returns(_maintenanceApiClient);
            _service = new CostBookMaintenanceService(_costBookClient);
        }

        // ── GetSettingsAsync ──────────────────────────────────────────────────

        #region GetSettingsAsync Tests

        [Fact]
        public async Task GetSettingsAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var dto = new MaintenanceSettingsDto { InflationAnimals = 2.5m, ProfitAnimals = 15m };
            var expected = ApiResponseDto<MaintenanceSettingsDto>.SuccessResponse(dto);
            _maintenanceApiClient.GetSettingsAsync().Returns(expected);

            // Act
            var result = await _service.GetSettingsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2.5m, result.Data?.InflationAnimals);
            await _maintenanceApiClient.Received(1).GetSettingsAsync();
        }

        [Fact]
        public async Task GetSettingsAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "ERR", Message = "Service unavailable" } };
            var expected = ApiResponseDto<MaintenanceSettingsDto>.FailureResponse(errors, new ApiMetaDto());
            _maintenanceApiClient.GetSettingsAsync().Returns(expected);

            // Act
            var result = await _service.GetSettingsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "ERR");
            await _maintenanceApiClient.Received(1).GetSettingsAsync();
        }

        #endregion

        // ── UpdateSettingsAsync ───────────────────────────────────────────────

        #region UpdateSettingsAsync Tests

        [Fact]
        public async Task UpdateSettingsAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var dto = new MaintenanceSettingsDto { InflationAnimals = 3.0m, ProfitAnimals = 20m };
            var expected = ApiResponseDto<MaintenanceSettingsDto>.SuccessResponse(dto);
            _maintenanceApiClient.UpdateSettingsAsync(dto).Returns(expected);

            // Act
            var result = await _service.UpdateSettingsAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3.0m, result.Data?.InflationAnimals);
            await _maintenanceApiClient.Received(1).UpdateSettingsAsync(dto);
        }

        [Fact]
        public async Task UpdateSettingsAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var dto = new MaintenanceSettingsDto { InflationAnimals = 3.0m };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "VALIDATION_ERROR", Message = "Invalid value" } };
            var expected = ApiResponseDto<MaintenanceSettingsDto>.FailureResponse(errors, new ApiMetaDto());
            _maintenanceApiClient.UpdateSettingsAsync(dto).Returns(expected);

            // Act
            var result = await _service.UpdateSettingsAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _maintenanceApiClient.Received(1).UpdateSettingsAsync(dto);
        }

        [Fact]
        public async Task UpdateSettingsAsync_VerifiesDelegationToApiClient()
        {
            // Arrange
            var dto = new MaintenanceSettingsDto { CurrentFinancialYear = 2025, WorkingDaysInYear = 220m };
            var expected = ApiResponseDto<MaintenanceSettingsDto>.SuccessResponse(dto);
            _maintenanceApiClient.UpdateSettingsAsync(dto).Returns(expected);

            // Act
            await _service.UpdateSettingsAsync(dto);

            // Assert — verify delegation occurs with exact argument
            await _maintenanceApiClient.Received(1).UpdateSettingsAsync(dto);
        }

        #endregion

        // ── GetAccountCategoriesAsync ─────────────────────────────────────────

        #region GetAccountCategoriesAsync Tests

        [Fact]
        public async Task GetAccountCategoriesAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var categories = new List<AccountCategoryMaintenanceDto>
            {
                new AccountCategoryMaintenanceDto { AccShortName = "ACC01", Csg7Group = "CSG001" },
                new AccountCategoryMaintenanceDto { AccShortName = "ACC02", Csg7Group = "CSG002" }
            };
            var expected = ApiResponseDto<List<AccountCategoryMaintenanceDto>>.SuccessResponse(categories);
            _maintenanceApiClient.GetAccountCategoriesAsync().Returns(expected);

            // Act
            var result = await _service.GetAccountCategoriesAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _maintenanceApiClient.Received(1).GetAccountCategoriesAsync();
        }

        [Fact]
        public async Task GetAccountCategoriesAsync_ApiClientReturnsEmptyList_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var expected = ApiResponseDto<List<AccountCategoryMaintenanceDto>>.SuccessResponse(new List<AccountCategoryMaintenanceDto>());
            _maintenanceApiClient.GetAccountCategoriesAsync().Returns(expected);

            // Act
            var result = await _service.GetAccountCategoriesAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _maintenanceApiClient.Received(1).GetAccountCategoriesAsync();
        }

        [Fact]
        public async Task GetAccountCategoriesAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "API_ERROR", Message = "Failed" } };
            var expected = ApiResponseDto<List<AccountCategoryMaintenanceDto>>.FailureResponse(errors, new ApiMetaDto());
            _maintenanceApiClient.GetAccountCategoriesAsync().Returns(expected);

            // Act
            var result = await _service.GetAccountCategoriesAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _maintenanceApiClient.Received(1).GetAccountCategoriesAsync();
        }

        #endregion

        // ── UpdateAccountCategoryAsync ────────────────────────────────────────

        #region UpdateAccountCategoryAsync Tests

        [Fact]
        public async Task UpdateAccountCategoryAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var accShortName = "ACC01";
            var dto = new AccountCategoryMaintenanceDto { AccShortName = accShortName, Csg7Group = "CSG003" };
            var expected = ApiResponseDto<AccountCategoryMaintenanceDto>.SuccessResponse(dto);
            _maintenanceApiClient.UpdateAccountCategoryAsync(accShortName, dto).Returns(expected);

            // Act
            var result = await _service.UpdateAccountCategoryAsync(accShortName, dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("CSG003", result.Data?.Csg7Group);
            await _maintenanceApiClient.Received(1).UpdateAccountCategoryAsync(accShortName, dto);
        }

        [Fact]
        public async Task UpdateAccountCategoryAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var accShortName = "NOTEXIST";
            var dto = new AccountCategoryMaintenanceDto { AccShortName = accShortName, Csg7Group = "CSG001" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Code = "NOT_FOUND", Message = "Account category not found" } };
            var expected = ApiResponseDto<AccountCategoryMaintenanceDto>.FailureResponse(errors, new ApiMetaDto());
            _maintenanceApiClient.UpdateAccountCategoryAsync(accShortName, dto).Returns(expected);

            // Act
            var result = await _service.UpdateAccountCategoryAsync(accShortName, dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "NOT_FOUND");
            await _maintenanceApiClient.Received(1).UpdateAccountCategoryAsync(accShortName, dto);
        }

        [Fact]
        public async Task UpdateAccountCategoryAsync_VerifiesDelegationWithCorrectArguments()
        {
            // Arrange
            var accShortName = "ACC01";
            var dto = new AccountCategoryMaintenanceDto { AccShortName = accShortName, Csg7Group = "CSG002" };
            var expected = ApiResponseDto<AccountCategoryMaintenanceDto>.SuccessResponse(dto);
            _maintenanceApiClient.UpdateAccountCategoryAsync(accShortName, dto).Returns(expected);

            // Act
            await _service.UpdateAccountCategoryAsync(accShortName, dto);

            // Assert — verify delegation with both required arguments
            await _maintenanceApiClient.Received(1).UpdateAccountCategoryAsync(accShortName, dto);
        }

        #endregion
    }
}
