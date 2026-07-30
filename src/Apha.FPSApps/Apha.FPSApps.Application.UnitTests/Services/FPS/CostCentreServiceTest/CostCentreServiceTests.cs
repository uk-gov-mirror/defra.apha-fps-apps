using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.CostCentreServiceTest
{
    public class CostCentreServiceTests
    {
        private readonly IFpsApiClient _mockFpsClient;
        private readonly IFpsCostCentreApiClient _mockCostCentreApiClient;
        private readonly CostCentreService _sut;

        public CostCentreServiceTests()
        {
            _mockFpsClient             = Substitute.For<IFpsApiClient>();
            _mockCostCentreApiClient   = Substitute.For<IFpsCostCentreApiClient>();
            _mockFpsClient.FpsCostCentre.Returns(_mockCostCentreApiClient);
            _sut = new CostCentreService(_mockFpsClient);
        }

        private static CostCentreDto BuildDto(double no = 100.0, string pc = "PC01") =>
            new() { CostCentreNo = no, ProfitCentre = pc, FpsYear = 2024 };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenFpsClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new CostCentreService(null!));
        }

        #endregion

        #region GetAllCostCentresAsync Tests

        [Fact]
        public async Task GetAllCostCentresAsync_DelegatesTo_ApiClient()
        {
            // Arrange
            var apiResponse = ApiResponseDto<List<CostCentreWorkgroupDto>>.SuccessResponse(
                new List<CostCentreWorkgroupDto> { new() { ProfitCentre = "PC01" } });
            _mockCostCentreApiClient.GetAllCostCentresAsync().Returns(apiResponse);

            // Act
            var result = await _sut.GetAllCostCentresAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _mockCostCentreApiClient.Received(1).GetAllCostCentresAsync();
        }

        [Fact]
        public async Task GetAllCostCentresAsync_PropagatesFailureResponse()
        {
            // Arrange
            var errors      = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<List<CostCentreWorkgroupDto>>.FailureResponse(errors, new ApiMetaDto());
            _mockCostCentreApiClient.GetAllCostCentresAsync().Returns(apiResponse);

            // Act
            var result = await _sut.GetAllCostCentresAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetAllCostCentresPagedAsync Tests

        [Fact]
        public async Task GetAllCostCentresPagedAsync_DelegatesTo_ApiClient()
        {
            // Arrange
            var query      = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos       = new List<CostCentreDto> { BuildDto() };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var apiResponse = ApiResponseDto<List<CostCentreDto>>.SuccessResponse(dtos, pagination);

            _mockCostCentreApiClient.GetAllCostCentresPagedAsync(query).Returns(apiResponse);

            // Act
            var result = await _sut.GetAllCostCentresPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.NotNull(result.Pagination);
            await _mockCostCentreApiClient.Received(1).GetAllCostCentresPagedAsync(query);
        }

        [Fact]
        public async Task GetAllCostCentresPagedAsync_PropagatesApiErrors()
        {
            // Arrange
            var query   = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<List<CostCentreDto>>.FailureResponse(errors, new ApiMetaDto());

            _mockCostCentreApiClient.GetAllCostCentresPagedAsync(query).Returns(apiResponse);

            // Act
            var result = await _sut.GetAllCostCentresPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetAllCostCentresPagedAsync_ReturnsEmptyList_WhenNoData()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = ApiResponseDto<List<CostCentreDto>>.SuccessResponse(new List<CostCentreDto>());

            _mockCostCentreApiClient.GetAllCostCentresPagedAsync(query).Returns(apiResponse);

            // Act
            var result = await _sut.GetAllCostCentresPagedAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        #endregion

        #region GetCostCentreByIdAsync Tests

        [Fact]
        public async Task GetCostCentreByIdAsync_DelegatesTo_ApiClient()
        {
            // Arrange
            var dto         = BuildDto();
            var apiResponse = ApiResponseDto<CostCentreDto>.SuccessResponse(dto);

            _mockCostCentreApiClient.GetCostCentreByIdAsync(100.0).Returns(apiResponse);

            // Act
            var result = await _sut.GetCostCentreByIdAsync(100.0);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(100.0, result.Data!.CostCentreNo);
            await _mockCostCentreApiClient.Received(1).GetCostCentreByIdAsync(100.0);
        }

        [Fact]
        public async Task GetCostCentreByIdAsync_PropagatesNotFoundError()
        {
            // Arrange
            var errors      = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = ApiResponseDto<CostCentreDto>.FailureResponse(errors, new ApiMetaDto());

            _mockCostCentreApiClient.GetCostCentreByIdAsync(999.0).Returns(apiResponse);

            // Act
            var result = await _sut.GetCostCentreByIdAsync(999.0);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateCostCentreAsync Tests

        [Fact]
        public async Task CreateCostCentreAsync_DelegatesTo_ApiClient()
        {
            // Arrange
            var dto         = BuildDto();
            var apiResponse = ApiResponseDto<CostCentreDto>.SuccessResponse(dto);

            _mockCostCentreApiClient.CreateCostCentreAsync(dto).Returns(apiResponse);

            // Act
            var result = await _sut.CreateCostCentreAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _mockCostCentreApiClient.Received(1).CreateCostCentreAsync(dto);
        }

        [Fact]
        public async Task CreateCostCentreAsync_PropagatesApiErrors()
        {
            // Arrange
            var dto    = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Conflict", Code = "CONFLICT" } };
            var apiResponse = ApiResponseDto<CostCentreDto>.FailureResponse(errors, new ApiMetaDto());

            _mockCostCentreApiClient.CreateCostCentreAsync(dto).Returns(apiResponse);

            // Act
            var result = await _sut.CreateCostCentreAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("CONFLICT", result.Errors!.First().Code);
        }

        #endregion

        #region UpdateCostCentreAsync Tests

        [Fact]
        public async Task UpdateCostCentreAsync_DelegatesTo_ApiClient()
        {
            // Arrange
            var dto         = BuildDto(100.0, "PC02");
            var apiResponse = ApiResponseDto<CostCentreDto>.SuccessResponse(dto);

            _mockCostCentreApiClient.UpdateCostCentreAsync(100.0, dto).Returns(apiResponse);

            // Act
            var result = await _sut.UpdateCostCentreAsync(100.0, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _mockCostCentreApiClient.Received(1).UpdateCostCentreAsync(100.0, dto);
        }

        [Fact]
        public async Task UpdateCostCentreAsync_ForwardsOriginalCostCentreNo_ForRenameSupport()
        {
            // Arrange
            var dto         = BuildDto(200.0, "PC01");   // renaming 100 → 200
            var apiResponse = ApiResponseDto<CostCentreDto>.SuccessResponse(dto);

            _mockCostCentreApiClient.UpdateCostCentreAsync(100.0, dto).Returns(apiResponse);

            // Act
            await _sut.UpdateCostCentreAsync(100.0, dto);

            // Assert
            await _mockCostCentreApiClient.Received(1).UpdateCostCentreAsync(100.0, dto);
        }

        [Fact]
        public async Task UpdateCostCentreAsync_PropagatesApiErrors()
        {
            // Arrange
            var dto    = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var apiResponse = ApiResponseDto<CostCentreDto>.FailureResponse(errors, new ApiMetaDto());

            _mockCostCentreApiClient.UpdateCostCentreAsync(100.0, dto).Returns(apiResponse);

            // Act
            var result = await _sut.UpdateCostCentreAsync(100.0, dto);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteCostCentreAsync Tests

        [Fact]
        public async Task DeleteCostCentreAsync_DelegatesTo_ApiClient()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _mockCostCentreApiClient.DeleteCostCentreAsync(100.0).Returns(apiResponse);

            // Act
            var result = await _sut.DeleteCostCentreAsync(100.0);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _mockCostCentreApiClient.Received(1).DeleteCostCentreAsync(100.0);
        }

        [Fact]
        public async Task DeleteCostCentreAsync_PropagatesApiErrors()
        {
            // Arrange
            var errors      = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "DELETE_ERROR" } };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _mockCostCentreApiClient.DeleteCostCentreAsync(100.0).Returns(apiResponse);

            // Act
            var result = await _sut.DeleteCostCentreAsync(100.0);

            // Assert
            Assert.False(result.Success);
        }

        #endregion
    }
}
