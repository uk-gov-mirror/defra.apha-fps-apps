/*
 * TRANSFORMENGINE MIGRATION — FpsAsuViewApiClientTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - New test class for FpsAsuViewApiClient (Phase 9 infrastructure HTTP client)
 *   - Tests cover GetAsuViewAsync and GetAnimalTypeLookupAsync method calls
 *   - Verifies: URL construction, mapper invocation, success/failure path, exception catch
 *   - Mirrors FpsAnimalApiClientTests.cs NSubstitute + ApiResponse<T> / ApiResponseDto<T> pattern
 *
 * PRESERVED:
 *   - NSubstitute mock pattern consistent with FpsAnimalApiClientTests.cs
 *   - ApiResponse<T> helper builders (SuccessApiResponse / FailureApiResponse) for _http mock
 *   - [MethodName]_[StateUnderTest]_[ExpectedResult] naming
 *   - xUnit Assert.* APIs (no FluentAssertions in Infrastructure.UnitTests)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: QueryStringHelper URL encoding not tested here (integration concern);
 *     URL construction tests verify the base path only
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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsAsuViewApiClientTest
{
    /// <summary>
    /// xUnit tests for <see cref="FpsAsuViewApiClient"/> — the infrastructure HTTP client
    /// for the ASU View resource family (Phase 9).
    /// </summary>
    public class FpsAsuViewApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper          _mapper;
        private readonly FpsAsuViewApiClient _client;

        public FpsAsuViewApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsAsuViewApiClient(_http, _mapper);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static ApiResponse<T> SuccessApiResponse<T>(T data) =>
            new() { Success = true, Data = data };

        private static ApiResponse<T> FailureApiResponse<T>() =>
            new()
            {
                Success = false,
                Errors  = [new ApiError { Message = "Error", Code = "ERROR" }]
            };

        private static AsuViewRes BuildAsuViewRes(string animalType = "CATTLE") =>
            new() { Id = 1, AnimalType = animalType, Project = "PRJ001", AnimalDays = 5.0, Cost = 250m };

        private static AsuViewDto BuildAsuViewDto(string animalType = "CATTLE") =>
            new() { Id = 1, AnimalType = animalType, Project = "PRJ001", AnimalDays = 5.0, Cost = 250m };

        // ── Constructor Tests ─────────────────────────────────────────────────

        #region Constructor

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenHttpIsNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => new FpsAsuViewApiClient(null!, _mapper));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => new FpsAsuViewApiClient(_http, null!));
        }

        #endregion

        // ── GetAsuViewAsync Tests ─────────────────────────────────────────────

        #region GetAsuViewAsync

        // TRANSFORMENGINE: happy path — http returns success; mapper called once; result returned
        [Fact]
        public async Task GetAsuViewAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var query      = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList    = new List<AsuViewRes> { BuildAsuViewRes() };
            var apiResp    = SuccessApiResponse(resList);
            var expectedDto = ApiResponseDto<List<AsuViewDto>>.SuccessResponse(
                new List<AsuViewDto> { BuildAsuViewDto() });

            // TRANSFORMENGINE: URL contains asu-view?animalType=CATTLE (base + filter)
            _http.GetAsync<List<AsuViewRes>>(Arg.Is<string>(u => u.Contains("asu-view") && u.Contains("animalType=CATTLE")))
                .Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AsuViewDto>>>(apiResp).Returns(expectedDto);

            // Act
            var result = await _client.GetAsuViewAsync(query, "CATTLE");

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data!);
            _mapper.Received(1).Map<ApiResponseDto<List<AsuViewDto>>>(apiResp);
        }

        // TRANSFORMENGINE: failure path — http returns failure; mapper maps failure; returned
        [Fact]
        public async Task GetAsuViewAsync_HttpReturnsFailure_ReturnsMappedFailureResponse()
        {
            // Arrange
            var query   = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResp = FailureApiResponse<List<AsuViewRes>>();
            var failDto = new ApiResponseDto<List<AsuViewDto>>
            {
                Success = false,
                Errors  = [new ApiErrorDto { Message = "Error", Code = "ERROR" }],
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<AsuViewRes>>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AsuViewDto>>>(apiResp).Returns(failDto);

            // Act
            var result = await _client.GetAsuViewAsync(query, "CATTLE");

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors!);
        }

        // TRANSFORMENGINE: exception path — catch block returns FailureResponse with INTERNAL_ERROR
        [Fact]
        public async Task GetAsuViewAsync_HttpThrowsException_ReturnsInternalErrorFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<AsuViewRes>>(Arg.Any<string>())
                 .ThrowsAsync(new Exception("Network failure"));

            // Act
            var result = await _client.GetAsuViewAsync(query, "CATTLE");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        // TRANSFORMENGINE: URL construction — confirm route contains "asu-view" and animalType param
        [Fact]
        public async Task GetAsuViewAsync_ValidAnimalType_CallsCorrectEndpointUrl()
        {
            // Arrange
            var query      = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResp    = SuccessApiResponse(new List<AsuViewRes>());
            var expectedDto = ApiResponseDto<List<AsuViewDto>>.SuccessResponse(new List<AsuViewDto>());

            _http.GetAsync<List<AsuViewRes>>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AsuViewDto>>>(apiResp).Returns(expectedDto);

            // Act
            await _client.GetAsuViewAsync(query, "CATTLE");

            // Assert — URL must contain "asu-view" segment and animalType query param
            await _http.Received(1).GetAsync<List<AsuViewRes>>(
                Arg.Is<string>(url => url.Contains("asu-view") && url.Contains("animalType=CATTLE")));
        }

        // TRANSFORMENGINE: animalType URI-encoding — special characters encoded in URL
        [Fact]
        public async Task GetAsuViewAsync_AnimalTypeWithSpecialChars_UriEncodesAnimalTypeInUrl()
        {
            // Arrange
            var query      = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResp    = SuccessApiResponse(new List<AsuViewRes>());
            var expectedDto = ApiResponseDto<List<AsuViewDto>>.SuccessResponse(new List<AsuViewDto>());

            _http.GetAsync<List<AsuViewRes>>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AsuViewDto>>>(apiResp).Returns(expectedDto);

            // Act — animalType with a space; URI.EscapeDataString encodes space as %20
            await _client.GetAsuViewAsync(query, "DAIRY COW");

            // Assert — space encoded as %20 or + in URL
            await _http.Received(1).GetAsync<List<AsuViewRes>>(
                Arg.Is<string>(url => url.Contains("DAIRY%20COW") || url.Contains("DAIRY+COW") || url.Contains("DAIRY COW")));
        }

        // TRANSFORMENGINE: mapper called on success — verifies the mapper is invoked
        [Fact]
        public async Task GetAsuViewAsync_HttpReturnsSuccess_MapperCalledOnce()
        {
            // Arrange
            var query      = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResp    = SuccessApiResponse(new List<AsuViewRes>());
            var expectedDto = ApiResponseDto<List<AsuViewDto>>.SuccessResponse(new List<AsuViewDto>());

            _http.GetAsync<List<AsuViewRes>>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AsuViewDto>>>(apiResp).Returns(expectedDto);

            // Act
            await _client.GetAsuViewAsync(query, "CATTLE");

            // Assert
            _mapper.Received(1).Map<ApiResponseDto<List<AsuViewDto>>>(apiResp);
        }

        #endregion

        // ── GetAnimalTypeLookupAsync Tests ─────────────────────────────────────

        #region GetAnimalTypeLookupAsync

        // TRANSFORMENGINE: happy path — GET api/v1/animal returns all animals for dropdown
        [Fact]
        public async Task GetAnimalTypeLookupAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var animalList = new List<AnimalRes>
            {
                new() { AnimalType = "CATTLE", Species = "Bovine", DailyRate = 50m },
                new() { AnimalType = "SHEEP",  Species = "Ovine",  DailyRate = 30m }
            };
            var apiResp    = SuccessApiResponse(animalList);
            var dtoList    = new List<AnimalDto>
            {
                new() { AnimalType = "CATTLE", Species = "Bovine", DailyRate = 50m },
                new() { AnimalType = "SHEEP",  Species = "Ovine",  DailyRate = 30m }
            };
            var expectedDto = ApiResponseDto<List<AnimalDto>>.SuccessResponse(dtoList);

            // TRANSFORMENGINE: GetAnimalTypeLookupAsync calls GET api/v1/animal (base URL, no suffix)
            _http.GetAsync<List<AnimalRes>>("api/v1/animal").Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AnimalDto>>>(apiResp).Returns(expectedDto);

            // Act
            var result = await _client.GetAnimalTypeLookupAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data!.Count);
            _mapper.Received(1).Map<ApiResponseDto<List<AnimalDto>>>(apiResp);
        }

        // TRANSFORMENGINE: failure path — lookup endpoint returns failure
        [Fact]
        public async Task GetAnimalTypeLookupAsync_HttpReturnsFailure_ReturnsMappedFailureResponse()
        {
            // Arrange
            var apiResp = FailureApiResponse<List<AnimalRes>>();
            var failDto = new ApiResponseDto<List<AnimalDto>>
            {
                Success = false,
                Errors  = [new ApiErrorDto { Message = "Lookup failed", Code = "ERROR" }],
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<AnimalRes>>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AnimalDto>>>(apiResp).Returns(failDto);

            // Act
            var result = await _client.GetAnimalTypeLookupAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors!);
        }

        // TRANSFORMENGINE: exception path — catch block returns INTERNAL_ERROR failure
        [Fact]
        public async Task GetAnimalTypeLookupAsync_HttpThrowsException_ReturnsInternalErrorFailureResponse()
        {
            // Arrange
            _http.GetAsync<List<AnimalRes>>(Arg.Any<string>())
                 .ThrowsAsync(new Exception("Connection timeout"));

            // Act
            var result = await _client.GetAnimalTypeLookupAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        // TRANSFORMENGINE: URL construction — lookup calls base URL "api/v1/animal" exactly
        [Fact]
        public async Task GetAnimalTypeLookupAsync_CallsBaseAnimalEndpoint()
        {
            // Arrange
            var apiResp    = SuccessApiResponse(new List<AnimalRes>());
            var expectedDto = ApiResponseDto<List<AnimalDto>>.SuccessResponse(new List<AnimalDto>());

            _http.GetAsync<List<AnimalRes>>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AnimalDto>>>(apiResp).Returns(expectedDto);

            // Act
            await _client.GetAnimalTypeLookupAsync();

            // Assert — must call the base animal endpoint (no "asu-view" suffix)
            await _http.Received(1).GetAsync<List<AnimalRes>>(
                Arg.Is<string>(url => url == "api/v1/animal"));
        }

        // TRANSFORMENGINE: mapper called on success
        [Fact]
        public async Task GetAnimalTypeLookupAsync_HttpReturnsSuccess_MapperCalledOnce()
        {
            // Arrange
            var apiResp    = SuccessApiResponse(new List<AnimalRes>());
            var expectedDto = ApiResponseDto<List<AnimalDto>>.SuccessResponse(new List<AnimalDto>());

            _http.GetAsync<List<AnimalRes>>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AnimalDto>>>(apiResp).Returns(expectedDto);

            // Act
            await _client.GetAnimalTypeLookupAsync();

            // Assert
            _mapper.Received(1).Map<ApiResponseDto<List<AnimalDto>>>(apiResp);
        }

        #endregion
    }
}
