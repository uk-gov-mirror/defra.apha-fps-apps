/*
 * TRANSFORMENGINE MIGRATION — ContributionSummaryServiceTests.cs (FPSApps.Application)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: xUnit tests for the frontend Application-layer ContributionSummaryService
 *     (Apha.FPSApps.Application/Services/FPS/ContributionSummaryService.cs).
 *   - Uses NSubstitute for IFpsApiClient aggregate and IFpsContributionSummaryApiClient.
 *   - Verifies pure thin-delegate pattern: each service method must call exactly one
 *     corresponding API client method with the same arguments.
 *   - Covers all six delegate methods: GetByProfitCentreAsync, GetSummaryAsync,
 *     GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync.
 *
 * PRESERVED:
 *   - Naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult].
 *   - Pattern matches ProjectServiceTests (FPS) in the same project.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — fully automated thin-delegate coverage.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS
{
    public class ContributionSummaryServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsContributionSummaryApiClient _fpsContributionSummaryApiClient;
        private readonly ContributionSummaryService _service;

        public ContributionSummaryServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsContributionSummaryApiClient = Substitute.For<IFpsContributionSummaryApiClient>();
            _fpsClient.FpsContributionSummary.Returns(_fpsContributionSummaryApiClient);
            _service = new ContributionSummaryService(_fpsClient);
        }

        // ── GetByProfitCentreAsync ─────────────────────────────────────────────

        #region GetByProfitCentreAsync

        [Fact]
        public async Task GetByProfitCentreAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var profitCentre = "Bact";
            var data = new List<ContributionSummaryDto>
            {
                new() { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = profitCentre }
            };
            var expected = ApiResponseDto<List<ContributionSummaryDto>>.SuccessResponse(
                data, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

            _fpsContributionSummaryApiClient.GetByProfitCentreAsync(query, profitCentre).Returns(expected);

            // Act
            var result = await _service.GetByProfitCentreAsync(query, profitCentre);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsContributionSummaryApiClient.Received(1).GetByProfitCentreAsync(query, profitCentre);
        }

        [Fact]
        public async Task GetByProfitCentreAsync_ApiClientReturnsEmptyList_ReturnsDelegatedEmptyResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var profitCentre = "Bact";
            var expected = ApiResponseDto<List<ContributionSummaryDto>>.SuccessResponse(
                new List<ContributionSummaryDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });

            _fpsContributionSummaryApiClient.GetByProfitCentreAsync(query, profitCentre).Returns(expected);

            // Act
            var result = await _service.GetByProfitCentreAsync(query, profitCentre);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetByProfitCentreAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var profitCentre = "NOPE";
            var expected = ApiResponseDto<List<ContributionSummaryDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found." } },
                new ApiMetaDto());

            _fpsContributionSummaryApiClient.GetByProfitCentreAsync(query, profitCentre).Returns(expected);

            // Act
            var result = await _service.GetByProfitCentreAsync(query, profitCentre);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        // ── GetSummaryAsync ───────────────────────────────────────────────────

        #region GetSummaryAsync

        [Fact]
        public async Task GetSummaryAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var profitCentre = "Bact";
            int? fpsYear = null;
            var summaryDto = new ContributionSummarySummaryDto { ContributionTarget = 200m };
            var expected = ApiResponseDto<ContributionSummarySummaryDto>.SuccessResponse(summaryDto);

            _fpsContributionSummaryApiClient.GetSummaryAsync(profitCentre, fpsYear).Returns(expected);

            // Act
            var result = await _service.GetSummaryAsync(profitCentre, fpsYear);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(200m, result.Data!.ContributionTarget);
            await _fpsContributionSummaryApiClient.Received(1).GetSummaryAsync(profitCentre, fpsYear);
        }

        [Fact]
        public async Task GetSummaryAsync_WithExplicitYear_DelegatesToApiClientWithYear()
        {
            // Arrange
            var profitCentre = "Bact";
            int? fpsYear = 2026;
            var expected = ApiResponseDto<ContributionSummarySummaryDto>.SuccessResponse(
                new ContributionSummarySummaryDto());

            _fpsContributionSummaryApiClient.GetSummaryAsync(profitCentre, fpsYear).Returns(expected);

            // Act
            await _service.GetSummaryAsync(profitCentre, fpsYear);

            // Assert
            await _fpsContributionSummaryApiClient.Received(1).GetSummaryAsync(profitCentre, fpsYear);
        }

        [Fact]
        public async Task GetSummaryAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = ApiResponseDto<ContributionSummarySummaryDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "INTERNAL_ERROR", Message = "Failed." } },
                new ApiMetaDto());

            _fpsContributionSummaryApiClient.GetSummaryAsync("Bact", null).Returns(expected);

            // Act
            var result = await _service.GetSummaryAsync("Bact", null);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var expected = ApiResponseDto<ContributionSummaryDto>.SuccessResponse(dto);

            _fpsContributionSummaryApiClient.GetByIdAsync(1).Returns(expected);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.Id);
            await _fpsContributionSummaryApiClient.Received(1).GetByIdAsync(1);
        }

        [Fact]
        public async Task GetByIdAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = ApiResponseDto<ContributionSummaryDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found." } },
                new ApiMetaDto());

            _fpsContributionSummaryApiClient.GetByIdAsync(999).Returns(expected);

            // Act
            var result = await _service.GetByIdAsync(999);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── CreateAsync ───────────────────────────────────────────────────────

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var created = new ContributionSummaryDto { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var expected = ApiResponseDto<ContributionSummaryDto>.SuccessResponse(created);

            _fpsContributionSummaryApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.Id);
            await _fpsContributionSummaryApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Wg = "", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var expected = ApiResponseDto<ContributionSummaryDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "VALIDATION_ERROR", Message = "Wg required." } },
                new ApiMetaDto());

            _fpsContributionSummaryApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "VALIDATION_ERROR");
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var updated = new ContributionSummaryDto { Id = 1, Wg = "BAC1_UPD", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var expected = ApiResponseDto<ContributionSummaryDto>.SuccessResponse(updated);

            _fpsContributionSummaryApiClient.UpdateAsync(1, dto).Returns(expected);

            // Act
            var result = await _service.UpdateAsync(1, dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("BAC1_UPD", result.Data!.Wg);
            await _fpsContributionSummaryApiClient.Received(1).UpdateAsync(1, dto);
        }

        [Fact]
        public async Task UpdateAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var expected = ApiResponseDto<ContributionSummaryDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found." } },
                new ApiMetaDto());

            _fpsContributionSummaryApiClient.UpdateAsync(999, dto).Returns(expected);

            // Act
            var result = await _service.UpdateAsync(999, dto);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── DeleteAsync ───────────────────────────────────────────────────────

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsContributionSummaryApiClient.DeleteAsync(1).Returns(expected);

            // Act
            var result = await _service.DeleteAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsContributionSummaryApiClient.Received(1).DeleteAsync(1);
        }

        [Fact]
        public async Task DeleteAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = ApiResponseDto<bool>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found." } },
                new ApiMetaDto());
            _fpsContributionSummaryApiClient.DeleteAsync(999).Returns(expected);

            // Act
            var result = await _service.DeleteAsync(999);

            // Assert
            Assert.False(result.Success);
        }

        #endregion
    }
}
