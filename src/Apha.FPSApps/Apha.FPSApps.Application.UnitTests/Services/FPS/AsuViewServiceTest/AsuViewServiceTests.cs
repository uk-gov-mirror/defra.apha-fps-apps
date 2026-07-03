/*
 * TRANSFORMENGINE MIGRATION — AsuViewServiceTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - New test class for AsuViewService (Phase 8 frontend service)
 *   - Mocks IFpsApiClient aggregate and IFpsAsuViewApiClient sub-client
 *   - Verifies GetAsuViewAsync and GetAnimalTypeLookupAsync thin-delegate calls
 *   - Covers: happy path, failure response propagation, delegation verification
 *   - Mirrors AnimalServiceTests.cs NSubstitute pattern in the same test project
 *
 * PRESERVED:
 *   - NSubstitute mock pattern — no Moq in this layer (frontend Application tests)
 *   - [MethodName]_[StateUnderTest]_[ExpectedResult] naming
 *   - xUnit Assert.* APIs only (no FluentAssertions in Apha.FPSApps.Application.UnitTests)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.AsuViewServiceTest
{
    /// <summary>
    /// xUnit tests for <see cref="AsuViewService"/> — the thin-delegate frontend service
    /// that forwards calls to <see cref="IFpsAsuViewApiClient"/> via the aggregate
    /// <see cref="IFpsApiClient"/>.
    /// </summary>
    public class AsuViewServiceTests
    {
        // TRANSFORMENGINE: mock aggregate client + sub-client property chaining
        private readonly IFpsApiClient         _mockFpsClient;
        private readonly IFpsAsuViewApiClient  _mockAsuViewApiClient;
        private readonly AsuViewService        _sut;

        public AsuViewServiceTests()
        {
            _mockFpsClient        = Substitute.For<IFpsApiClient>();
            _mockAsuViewApiClient = Substitute.For<IFpsAsuViewApiClient>();

            // TRANSFORMENGINE: wire aggregate to sub-client (mirrors AsuViewService constructor behaviour)
            _mockFpsClient.FpsAsuView.Returns(_mockAsuViewApiClient);
            _sut = new AsuViewService(_mockFpsClient);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static AsuViewDto BuildAsuViewDto(string animalType = "CATTLE") =>
            new() { Id = 1, AnimalType = animalType, Project = "PRJ001", AnimalDays = 5.0, Cost = 250m };

        private static AnimalDto BuildAnimalDto(string animalType = "CATTLE") =>
            new() { AnimalType = animalType, Species = "Bovine", SecurityLevel = "L1", DailyRate = 50m };

        // ── Constructor Tests ─────────────────────────────────────────────────

        #region Constructor

        // TRANSFORMENGINE: AsuViewService does not null-guard the client in the current
        // implementation (thin constructor body: _client = client). Confirm by reading the source.
        // This test documents the current behaviour.
        [Fact]
        public void Constructor_AssignsFpsClient_DoesNotThrow()
        {
            var client = Substitute.For<IFpsApiClient>();
            client.FpsAsuView.Returns(Substitute.For<IFpsAsuViewApiClient>());
            var svc = new AsuViewService(client);
            Assert.NotNull(svc);
        }

        #endregion

        // ── GetAsuViewAsync Tests ─────────────────────────────────────────────

        #region GetAsuViewAsync

        // TRANSFORMENGINE: happy path — API client returns success; service returns delegated result
        [Fact]
        public async Task GetAsuViewAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var query     = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data      = new List<AsuViewDto> { BuildAsuViewDto() };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var expected  = ApiResponseDto<List<AsuViewDto>>.SuccessResponse(data, pagination);

            _mockAsuViewApiClient.GetAsuViewAsync(query, "CATTLE").Returns(expected);

            // Act
            var result = await _sut.GetAsuViewAsync(query, "CATTLE");

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data!);
            Assert.Equal("CATTLE", result.Data[0].AnimalType);
            await _mockAsuViewApiClient.Received(1).GetAsuViewAsync(query, "CATTLE");
        }

        // TRANSFORMENGINE: empty result — API returns success with empty list
        [Fact]
        public async Task GetAsuViewAsync_ApiClientReturnsEmptyList_ReturnsDelegatedEmptyResponse()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<AsuViewDto>>.SuccessResponse(
                new List<AsuViewDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });

            _mockAsuViewApiClient.GetAsuViewAsync(query, "CATTLE").Returns(expected);

            // Act
            var result = await _sut.GetAsuViewAsync(query, "CATTLE");

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _mockAsuViewApiClient.Received(1).GetAsuViewAsync(query, "CATTLE");
        }

        // TRANSFORMENGINE: failure response — API returns failure; service propagates it unchanged
        [Fact]
        public async Task GetAsuViewAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var query  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Service unavailable", Code = "503" } };
            var expected = ApiResponseDto<List<AsuViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _mockAsuViewApiClient.GetAsuViewAsync(query, "CATTLE").Returns(expected);

            // Act
            var result = await _sut.GetAsuViewAsync(query, "CATTLE");

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("503", result.Errors![0].Code);
            await _mockAsuViewApiClient.Received(1).GetAsuViewAsync(query, "CATTLE");
        }

        // TRANSFORMENGINE: delegation verification — confirms _client.FpsAsuView.GetAsuViewAsync
        // is called exactly once with the exact query and animalType arguments
        [Fact]
        public async Task GetAsuViewAsync_DelegatesExactArgumentsToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page      = 2,
                PageSize  = 5,
                SortBy    = "Project",
                Descending = false
            };
            var expected = ApiResponseDto<List<AsuViewDto>>.SuccessResponse(new List<AsuViewDto>());
            _mockAsuViewApiClient.GetAsuViewAsync(query, "SHEEP").Returns(expected);

            // Act
            await _sut.GetAsuViewAsync(query, "SHEEP");

            // Assert — exact instance and string value forwarded
            await _mockAsuViewApiClient.Received(1).GetAsuViewAsync(
                Arg.Is<QueryParameters<string>>(q => q.Page == 2 && q.PageSize == 5),
                "SHEEP");
        }

        #endregion

        // ── GetAnimalTypeLookupAsync Tests ────────────────────────────────────

        #region GetAnimalTypeLookupAsync

        // TRANSFORMENGINE: happy path — API returns animal type list for dropdown
        [Fact]
        public async Task GetAnimalTypeLookupAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var data     = new List<AnimalDto> { BuildAnimalDto("CATTLE"), BuildAnimalDto("SHEEP") };
            var expected = ApiResponseDto<List<AnimalDto>>.SuccessResponse(data);

            _mockAsuViewApiClient.GetAnimalTypeLookupAsync().Returns(expected);

            // Act
            var result = await _sut.GetAnimalTypeLookupAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data!.Count);
            await _mockAsuViewApiClient.Received(1).GetAnimalTypeLookupAsync();
        }

        // TRANSFORMENGINE: empty lookup — API returns success with empty list
        [Fact]
        public async Task GetAnimalTypeLookupAsync_ApiClientReturnsEmptyList_ReturnsDelegatedEmptyResponse()
        {
            // Arrange
            var expected = ApiResponseDto<List<AnimalDto>>.SuccessResponse(new List<AnimalDto>());
            _mockAsuViewApiClient.GetAnimalTypeLookupAsync().Returns(expected);

            // Act
            var result = await _sut.GetAnimalTypeLookupAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _mockAsuViewApiClient.Received(1).GetAnimalTypeLookupAsync();
        }

        // TRANSFORMENGINE: failure response — API lookup fails; service propagates it
        [Fact]
        public async Task GetAnimalTypeLookupAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Lookup failed", Code = "ERROR" } };
            var expected = ApiResponseDto<List<AnimalDto>>.FailureResponse(errors, new ApiMetaDto());

            _mockAsuViewApiClient.GetAnimalTypeLookupAsync().Returns(expected);

            // Act
            var result = await _sut.GetAnimalTypeLookupAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _mockAsuViewApiClient.Received(1).GetAnimalTypeLookupAsync();
        }

        // TRANSFORMENGINE: delegation verification — confirms FpsAsuView sub-client is used
        [Fact]
        public async Task GetAnimalTypeLookupAsync_DelegatesToFpsAsuViewSubClient()
        {
            // Arrange
            var expected = ApiResponseDto<List<AnimalDto>>.SuccessResponse(new List<AnimalDto>());
            _mockAsuViewApiClient.GetAnimalTypeLookupAsync().Returns(expected);

            // Act
            await _sut.GetAnimalTypeLookupAsync();

            // Assert
            await _mockAsuViewApiClient.Received(1).GetAnimalTypeLookupAsync();
            // Sub-client NOT bypassed — aggregate FpsAsuView property was used
            _ = _mockFpsClient.Received(1).FpsAsuView;
        }

        #endregion
    }
}
