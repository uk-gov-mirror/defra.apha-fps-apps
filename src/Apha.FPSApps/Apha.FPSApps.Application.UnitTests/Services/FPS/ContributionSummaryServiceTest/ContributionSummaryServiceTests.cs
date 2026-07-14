using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.ContributionSummaryServiceTest
{
    public class ContributionSummaryServiceTests
    {
        private readonly IFpsApiClient              _fpsClient;
        private readonly IFpsContributionSummaryApiClient  _apiClient;
        private readonly ContributionSummaryService _service;

        public ContributionSummaryServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _apiClient = Substitute.For<IFpsContributionSummaryApiClient>();
            _fpsClient.FpsContributionSummary.Returns(_apiClient);
            _service   = new ContributionSummaryService(_fpsClient);
        }

        // ── GetRowsAsync ─────────────────────────────────────────────────────

        #region GetRowsAsync — Happy path

        [Fact]
        public async Task GetRowsAsync_WithSuccessResponse_ReturnsDtoList()
        {
            // Arrange
            var sellingPc = "ASU";
            var rows = new List<ContributionSummaryRowDto>
            {
                new() { WgGrade = "G1", WorkGroup = "WG1", ProfitCentreGrade = "PCG1", Hrs = 100.0, Fec = 1000m },
                new() { WgGrade = "G2", WorkGroup = "WG2", ProfitCentreGrade = "PCG2", Hrs = 200.0, Fec = 2000m }
            };
            var response = ApiResponseDto<List<ContributionSummaryRowDto>>.SuccessResponse(rows);
            _apiClient.GetRowsAsync(sellingPc).Returns(response);

            // Act
            var result = await _service.GetRowsAsync(sellingPc);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _apiClient.Received(1).GetRowsAsync(sellingPc);
        }

        [Fact]
        public async Task GetRowsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var sellingPc = "ENV";
            var response  = ApiResponseDto<List<ContributionSummaryRowDto>>.SuccessResponse(new List<ContributionSummaryRowDto>());
            _apiClient.GetRowsAsync(sellingPc).Returns(response);

            // Act
            var result = await _service.GetRowsAsync(sellingPc);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetRowsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var sellingPc   = "ASU";
            var errors      = new List<ApiErrorDto> { new() { Code = "API_ERROR", Message = "API error" } };
            var failResponse = ApiResponseDto<List<ContributionSummaryRowDto>>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.GetRowsAsync(sellingPc).Returns(failResponse);

            // Act
            var result = await _service.GetRowsAsync(sellingPc);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetRowsAsync — Delegation

        [Fact]
        public async Task GetRowsAsync_DelegatesToFpsTimeSellerPcApiClient()
        {
            // Arrange
            var sellingPc = "ASU";
            var response  = ApiResponseDto<List<ContributionSummaryRowDto>>.SuccessResponse(new List<ContributionSummaryRowDto>());
            _apiClient.GetRowsAsync(sellingPc).Returns(response);

            // Act
            await _service.GetRowsAsync(sellingPc);

            // Assert
            await _apiClient.Received(1).GetRowsAsync(sellingPc);
            _ = _fpsClient.Received(1).FpsContributionSummary;
        }

        [Theory]
        [InlineData("ASU")]
        [InlineData("ENV")]
        [InlineData("BIO")]
        public async Task GetRowsAsync_PassesCorrectSellingPcToApiClient(string sellingPc)
        {
            // Arrange
            var response = ApiResponseDto<List<ContributionSummaryRowDto>>.SuccessResponse(new List<ContributionSummaryRowDto>());
            _apiClient.GetRowsAsync(sellingPc).Returns(response);

            // Act
            await _service.GetRowsAsync(sellingPc);

            // Assert
            await _apiClient.Received(1).GetRowsAsync(sellingPc);
        }

        #endregion

        // ── GetTotalsAsync ───────────────────────────────────────────────────

        #region GetTotalsAsync — Happy path

        [Fact]
        public async Task GetTotalsAsync_WithSuccessResponse_ReturnsDto()
        {
            // Arrange
            var sellingPc = "ASU";
            var totals = new ContributionSummaryTotalsDto
            {
                SellingPc        = sellingPc,
                ContTarget       = 50000m,
                SumOfGenBid      = 40000m,
                TotalFec         = 95000m,
                TotalContribution = 90000m,
                TotalAppFec      = 80000m,
                TotalToRecover   = 90000m,
                Surplus          = 5000m,
                AssuredSurplus   = -10000m,
                AnimalCosts      = 1500m,
                IsAsuMode        = true
            };
            _apiClient.GetTotalsAsync(sellingPc)
                .Returns(ApiResponseDto<ContributionSummaryTotalsDto>.SuccessResponse(totals));

            // Act
            var result = await _service.GetTotalsAsync(sellingPc);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(sellingPc,  result.Data?.SellingPc);
            Assert.Equal(50000m,     result.Data?.ContTarget);
            Assert.Equal(95000m,     result.Data?.TotalFec);
            Assert.Equal(5000m,      result.Data?.Surplus);
            Assert.Equal(-10000m,    result.Data?.AssuredSurplus);
            Assert.True(result.Data?.IsAsuMode);
            await _apiClient.Received(1).GetTotalsAsync(sellingPc);
        }

        [Fact]
        public async Task GetTotalsAsync_WithNonAsuSellingPc_ReturnsIsAsuModeFalse()
        {
            // Arrange
            var sellingPc = "ENV";
            var totals    = new ContributionSummaryTotalsDto { SellingPc = sellingPc, IsAsuMode = false, AnimalCosts = 0m };
            _apiClient.GetTotalsAsync(sellingPc)
                .Returns(ApiResponseDto<ContributionSummaryTotalsDto>.SuccessResponse(totals));

            // Act
            var result = await _service.GetTotalsAsync(sellingPc);

            // Assert
            Assert.True(result.Success);
            Assert.False(result.Data?.IsAsuMode);
            Assert.Equal(0m, result.Data?.AnimalCosts);
        }

        [Fact]
        public async Task GetTotalsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var sellingPc   = "ASU";
            var errors      = new List<ApiErrorDto> { new() { Code = "ERROR", Message = "API error" } };
            _apiClient.GetTotalsAsync(sellingPc)
                .Returns(ApiResponseDto<ContributionSummaryTotalsDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _service.GetTotalsAsync(sellingPc);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetTotalsAsync — Delegation

        [Fact]
        public async Task GetTotalsAsync_DelegatesToFpsTimeSellerPcApiClient()
        {
            // Arrange
            var sellingPc = "ASU";
            _apiClient.GetTotalsAsync(sellingPc)
                .Returns(ApiResponseDto<ContributionSummaryTotalsDto>.SuccessResponse(new ContributionSummaryTotalsDto()));

            // Act
            await _service.GetTotalsAsync(sellingPc);

            // Assert
            await _apiClient.Received(1).GetTotalsAsync(sellingPc);
            _ = _fpsClient.Received(1).FpsContributionSummary;
        }

        [Theory]
        [InlineData("ASU")]
        [InlineData("ENV")]
        [InlineData("BIO")]
        public async Task GetTotalsAsync_PassesCorrectSellingPcToApiClient(string sellingPc)
        {
            // Arrange
            _apiClient.GetTotalsAsync(sellingPc)
                .Returns(ApiResponseDto<ContributionSummaryTotalsDto>.SuccessResponse(new ContributionSummaryTotalsDto()));

            // Act
            await _service.GetTotalsAsync(sellingPc);

            // Assert
            await _apiClient.Received(1).GetTotalsAsync(sellingPc);
        }

        #endregion
    }
}
