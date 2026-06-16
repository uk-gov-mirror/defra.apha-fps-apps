/*
 * TRANSFORMENGINE MIGRATION — FpsContributionSummaryApiClientTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: xUnit tests for FpsContributionSummaryApiClient (Infrastructure layer).
 *   - Uses NSubstitute for IFpsHttpExecutor and IMapper.
 *   - Covers all six HTTP methods: GetByProfitCentreAsync, GetSummaryAsync, GetByIdAsync,
 *     CreateAsync, UpdateAsync, DeleteAsync.
 *   - Verifies URL construction, mapper use, failure response pass-through,
 *     and exception catch-block handling (INTERNAL_ERROR on throw).
 *
 * PRESERVED:
 *   - Naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult].
 *   - Pattern matches FpsProjectApiClientTests in the same project.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — fully automated.
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsContributionSummaryApiClientTest
{
    public class FpsContributionSummaryApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsContributionSummaryApiClient _client;

        public FpsContributionSummaryApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsContributionSummaryApiClient(_http, _mapper);
        }

        // ── GetByProfitCentreAsync ─────────────────────────────────────────────

        #region GetByProfitCentreAsync

        [Fact]
        public async Task GetByProfitCentreAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var profitCentre = "Bact";
            var httpResponse = new ApiResponse<List<ContributionSummaryRes>>
            {
                Success = true,
                Data = new List<ContributionSummaryRes> { new() { Id = 1, Wg = "BAC1" } }
            };
            var expectedDto = ApiResponseDto<List<ContributionSummaryDto>>.SuccessResponse(
                new List<ContributionSummaryDto> { new() { Id = 1, Wg = "BAC1" } });

            _http.GetAsync<List<ContributionSummaryRes>>(
                    Arg.Is<string>(url => url.Contains("contributionsummary") && url.Contains("profitCentre=Bact")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ContributionSummaryDto>>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetByProfitCentreAsync(query, profitCentre);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            _mapper.Received(1).Map<ApiResponseDto<List<ContributionSummaryDto>>>(httpResponse);
        }

        [Fact]
        public async Task GetByProfitCentreAsync_HttpReturnsFailure_ReturnsMappedFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var httpResponse = new ApiResponse<List<ContributionSummaryRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Code = "NOT_FOUND", Message = "Not found." } }
            };
            var mappedFailure = ApiResponseDto<List<ContributionSummaryDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found." } },
                new ApiMetaDto());

            _http.GetAsync<List<ContributionSummaryRes>>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ContributionSummaryDto>>>(httpResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetByProfitCentreAsync(query, "Bact");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetByProfitCentreAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<ContributionSummaryRes>>(Arg.Any<string>())
                .Throws(new Exception("Network error"));

            // Act
            var result = await _client.GetByProfitCentreAsync(query, "Bact");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task GetByProfitCentreAsync_UrlContainsProfitCentre()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var profitCentre = "Viro";
            var httpResponse = new ApiResponse<List<ContributionSummaryRes>> { Success = true, Data = new List<ContributionSummaryRes>() };
            var dto = ApiResponseDto<List<ContributionSummaryDto>>.SuccessResponse(new List<ContributionSummaryDto>());

            _http.GetAsync<List<ContributionSummaryRes>>(
                    Arg.Is<string>(url => url.Contains("profitCentre=Viro")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<List<ContributionSummaryDto>>>(httpResponse).Returns(dto);

            // Act
            await _client.GetByProfitCentreAsync(query, profitCentre);

            // Assert
            await _http.Received(1).GetAsync<List<ContributionSummaryRes>>(
                Arg.Is<string>(url => url.Contains("profitCentre=Viro")));
        }

        #endregion

        // ── GetSummaryAsync ───────────────────────────────────────────────────

        #region GetSummaryAsync

        [Fact]
        public async Task GetSummaryAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = new ApiResponse<ContributionSummarySummaryRes>
            {
                Success = true,
                Data = new ContributionSummarySummaryRes { ContributionTarget = 200m }
            };
            var expectedDto = ApiResponseDto<ContributionSummarySummaryDto>.SuccessResponse(
                new ContributionSummarySummaryDto { ContributionTarget = 200m });

            _http.GetAsync<ContributionSummarySummaryRes>(
                    Arg.Is<string>(url => url.Contains("contributionsummary/summary") && url.Contains("profitCentre=Bact")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<ContributionSummarySummaryDto>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetSummaryAsync("Bact", null);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(200m, result.Data!.ContributionTarget);
            _mapper.Received(1).Map<ApiResponseDto<ContributionSummarySummaryDto>>(httpResponse);
        }

        [Fact]
        public async Task GetSummaryAsync_WithFpsYear_AppendsYearToUrl()
        {
            // Arrange
            var httpResponse = new ApiResponse<ContributionSummarySummaryRes> { Success = true, Data = new ContributionSummarySummaryRes() };
            var expectedDto = ApiResponseDto<ContributionSummarySummaryDto>.SuccessResponse(new ContributionSummarySummaryDto());

            _http.GetAsync<ContributionSummarySummaryRes>(
                    Arg.Is<string>(url => url.Contains("fpsYear=2026")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<ContributionSummarySummaryDto>>(httpResponse).Returns(expectedDto);

            // Act
            await _client.GetSummaryAsync("Bact", 2026);

            // Assert
            await _http.Received(1).GetAsync<ContributionSummarySummaryRes>(
                Arg.Is<string>(url => url.Contains("fpsYear=2026")));
        }

        [Fact]
        public async Task GetSummaryAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            _http.GetAsync<ContributionSummarySummaryRes>(Arg.Any<string>())
                .Throws(new Exception("Timeout"));

            // Act
            var result = await _client.GetSummaryAsync("Bact", null);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task GetSummaryAsync_HttpReturnsFailure_ReturnsMappedFailureResponse()
        {
            // Arrange
            var httpResponse = new ApiResponse<ContributionSummarySummaryRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Code = "ERROR", Message = "Service error." } }
            };
            var mappedFailure = ApiResponseDto<ContributionSummarySummaryDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERROR" } },
                new ApiMetaDto());

            _http.GetAsync<ContributionSummarySummaryRes>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<ContributionSummarySummaryDto>>(httpResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetSummaryAsync("Bact", null);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = new ApiResponse<ContributionSummaryRes>
            {
                Success = true,
                Data = new ContributionSummaryRes { Id = 1, Wg = "BAC1", Grade = "C_BAC1" }
            };
            var expectedDto = ApiResponseDto<ContributionSummaryDto>.SuccessResponse(
                new ContributionSummaryDto { Id = 1, Wg = "BAC1", Grade = "C_BAC1" });

            _http.GetAsync<ContributionSummaryRes>(
                    Arg.Is<string>(url => url.Contains("contributionsummary/1")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<ContributionSummaryDto>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.Id);
            _mapper.Received(1).Map<ApiResponseDto<ContributionSummaryDto>>(httpResponse);
        }

        [Fact]
        public async Task GetByIdAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            _http.GetAsync<ContributionSummaryRes>(Arg.Any<string>())
                .Throws(new Exception("Network error"));

            // Act
            var result = await _client.GetByIdAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task GetByIdAsync_UrlContainsId()
        {
            // Arrange
            var httpResponse = new ApiResponse<ContributionSummaryRes> { Success = true, Data = new ContributionSummaryRes() };
            var dto = ApiResponseDto<ContributionSummaryDto>.SuccessResponse(new ContributionSummaryDto());

            _http.GetAsync<ContributionSummaryRes>(Arg.Is<string>(url => url.Contains("/42")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<ContributionSummaryDto>>(httpResponse).Returns(dto);

            // Act
            await _client.GetByIdAsync(42);

            // Assert
            await _http.Received(1).GetAsync<ContributionSummaryRes>(
                Arg.Is<string>(url => url.Contains("/42")));
        }

        #endregion

        // ── CreateAsync ───────────────────────────────────────────────────────

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var req = new ContributionSummaryReq { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var httpResponse = new ApiResponse<ContributionSummaryRes>
            {
                Success = true,
                Data = new ContributionSummaryRes { Id = 1, Wg = "BAC1", Grade = "C_BAC1" }
            };
            var expectedDto = ApiResponseDto<ContributionSummaryDto>.SuccessResponse(
                new ContributionSummaryDto { Id = 1, Wg = "BAC1", Grade = "C_BAC1" });

            _mapper.Map<ContributionSummaryReq>(dto).Returns(req);
            _http.PostAsync<ContributionSummaryReq, ContributionSummaryRes>(
                    Arg.Is<string>(url => url.Contains("contributionsummary")), req)
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<ContributionSummaryDto>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.Id);
            _mapper.Received(1).Map<ContributionSummaryReq>(dto);
            _mapper.Received(1).Map<ApiResponseDto<ContributionSummaryDto>>(httpResponse);
        }

        [Fact]
        public async Task CreateAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            _mapper.Map<ContributionSummaryReq>(dto).Returns(new ContributionSummaryReq());
            _http.PostAsync<ContributionSummaryReq, ContributionSummaryRes>(Arg.Any<string>(), Arg.Any<ContributionSummaryReq>())
                .Throws(new Exception("Post failed"));

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task CreateAsync_HttpReturnsFailure_ReturnsMappedFailureResponse()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var httpResponse = new ApiResponse<ContributionSummaryRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Code = "VALIDATION_ERROR", Message = "Wg required." } }
            };
            var mappedFailure = ApiResponseDto<ContributionSummaryDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "VALIDATION_ERROR" } },
                new ApiMetaDto());

            _mapper.Map<ContributionSummaryReq>(dto).Returns(new ContributionSummaryReq());
            _http.PostAsync<ContributionSummaryReq, ContributionSummaryRes>(Arg.Any<string>(), Arg.Any<ContributionSummaryReq>())
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<ContributionSummaryDto>>(httpResponse).Returns(mappedFailure);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var req = new ContributionSummaryReq { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var httpResponse = new ApiResponse<ContributionSummaryRes>
            {
                Success = true,
                Data = new ContributionSummaryRes { Id = 1, Wg = "BAC1_UPD", Grade = "C_BAC1" }
            };
            var expectedDto = ApiResponseDto<ContributionSummaryDto>.SuccessResponse(
                new ContributionSummaryDto { Id = 1, Wg = "BAC1_UPD" });

            _mapper.Map<ContributionSummaryReq>(dto).Returns(req);
            _http.PutAsync<ContributionSummaryReq, ContributionSummaryRes>(
                    Arg.Is<string>(url => url.Contains("contributionsummary/1")), req)
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<ContributionSummaryDto>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateAsync(1, dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("BAC1_UPD", result.Data!.Wg);
            _mapper.Received(1).Map<ContributionSummaryReq>(dto);
            _mapper.Received(1).Map<ApiResponseDto<ContributionSummaryDto>>(httpResponse);
        }

        [Fact]
        public async Task UpdateAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            _mapper.Map<ContributionSummaryReq>(dto).Returns(new ContributionSummaryReq());
            _http.PutAsync<ContributionSummaryReq, ContributionSummaryRes>(Arg.Any<string>(), Arg.Any<ContributionSummaryReq>())
                .Throws(new Exception("Put failed"));

            // Act
            var result = await _client.UpdateAsync(1, dto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task UpdateAsync_UrlContainsId()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Id = 7, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var httpResponse = new ApiResponse<ContributionSummaryRes> { Success = true, Data = new ContributionSummaryRes() };
            var expectedDto = ApiResponseDto<ContributionSummaryDto>.SuccessResponse(new ContributionSummaryDto());

            _mapper.Map<ContributionSummaryReq>(dto).Returns(new ContributionSummaryReq());
            _http.PutAsync<ContributionSummaryReq, ContributionSummaryRes>(
                    Arg.Is<string>(url => url.Contains("/7")), Arg.Any<ContributionSummaryReq>())
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<ContributionSummaryDto>>(httpResponse).Returns(expectedDto);

            // Act
            await _client.UpdateAsync(7, dto);

            // Assert
            await _http.Received(1).PutAsync<ContributionSummaryReq, ContributionSummaryRes>(
                Arg.Is<string>(url => url.Contains("/7")), Arg.Any<ContributionSummaryReq>());
        }

        #endregion

        // ── DeleteAsync ───────────────────────────────────────────────────────

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var httpResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Is<string>(url => url.Contains("contributionsummary/1")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<bool>>(httpResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteAsync(1);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<bool>>(httpResponse);
        }

        [Fact]
        public async Task DeleteAsync_HttpThrowsException_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            _http.DeleteAsync<bool?>(Arg.Any<string>()).Throws(new Exception("Delete failed"));

            // Act
            var result = await _client.DeleteAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task DeleteAsync_UrlContainsId()
        {
            // Arrange
            var httpResponse = new ApiResponse<bool?> { Success = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Is<string>(url => url.Contains("/99")))
                .Returns(httpResponse);
            _mapper.Map<ApiResponseDto<bool>>(httpResponse).Returns(expectedDto);

            // Act
            await _client.DeleteAsync(99);

            // Assert
            await _http.Received(1).DeleteAsync<bool?>(Arg.Is<string>(url => url.Contains("/99")));
        }

        [Fact]
        public async Task DeleteAsync_HttpReturnsFailure_ReturnsMappedFailureResponse()
        {
            // Arrange
            var httpResponse = new ApiResponse<bool?>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Code = "NOT_FOUND", Message = "Row not found." } }
            };
            var mappedFailure = ApiResponseDto<bool>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "NOT_FOUND" } },
                new ApiMetaDto());

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(httpResponse);
            _mapper.Map<ApiResponseDto<bool>>(httpResponse).Returns(mappedFailure);

            // Act
            var result = await _client.DeleteAsync(999);

            // Assert
            Assert.False(result.Success);
        }

        #endregion
    }
}
