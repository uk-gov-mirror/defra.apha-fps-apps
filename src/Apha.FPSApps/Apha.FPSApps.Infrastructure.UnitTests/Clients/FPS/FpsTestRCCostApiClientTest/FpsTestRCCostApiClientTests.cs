using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Mappings;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsTestRCCostApiClientTest
{
    public class FpsTestRCCostApiClientTests
    {
        private const string DefaultTestCode   = "TEST001";
        private const string DefaultProfitCentre = "PC001";
        private const int    DefaultFpsYear    = 2025;
        private const string BaseUrl           = "api/v1/testrccost";

        private readonly IFpsHttpExecutor       _http;
        private readonly IMapper                _mapper;
        private readonly FpsTestRCCostApiClient _client;

        public FpsTestRCCostApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsTestRCCostApiClient(_http, _mapper);
        }

        // ── helpers ───────────────────────────────────────────────────────────

        private static ApiResponse<T> SuccessResponse<T>(T data) =>
            new() { Success = true, Data = data };

        private static ApiResponse<T> FailureResponse<T>() =>
            new() { Success = false };

        // ── GetByTestCodeAsync ────────────────────────────────────────────────

        #region GetByTestCodeAsync

        [Fact]
        public async Task GetByTestCodeAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var resList     = new List<TestRCCostRes> { new() { TestCode = DefaultTestCode } };
            var apiResponse = SuccessResponse(resList);
            var expectedDto = ApiResponseDto<List<TestRCCostDto>>.SuccessResponse(
                new List<TestRCCostDto> { new() { TestCode = DefaultTestCode } });

            _http.GetAsync<List<TestRCCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestRCCostDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<List<TestRCCostDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetByTestCodeAsync_UrlContainsTestCode_CorrectRouteCalled()
        {
            // Arrange
            var apiResponse = SuccessResponse(new List<TestRCCostRes>());
            var expectedDto = ApiResponseDto<List<TestRCCostDto>>.SuccessResponse(new List<TestRCCostDto>());
            string capturedUrl = string.Empty;

            _http.GetAsync<List<TestRCCostRes>>(Arg.Do<string>(u => capturedUrl = u)).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestRCCostDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear);

            // Assert
            Assert.Equal($"{BaseUrl}/{DefaultTestCode}", capturedUrl);
        }

        [Fact]
        public async Task GetByTestCodeAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureResponse<List<TestRCCostRes>>();
            var failureDto  = ApiResponseDto<List<TestRCCostDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Server error" } }, new ApiMetaDto());

            _http.GetAsync<List<TestRCCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestRCCostDto>>>(apiResponse).Returns(failureDto);

            // Act
            var result = await _client.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── GetByKeyAsync ─────────────────────────────────────────────────────

        #region GetByKeyAsync

        [Fact]
        public async Task GetByKeyAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var res         = new TestRCCostRes { TestCode = DefaultTestCode, ProfitCentre = DefaultProfitCentre };
            var apiResponse = SuccessResponse(res);
            var expectedDto = ApiResponseDto<TestRCCostDto>.SuccessResponse(
                new TestRCCostDto { TestCode = DefaultTestCode });

            _http.GetAsync<TestRCCostRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRCCostDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetByKeyAsync(DefaultTestCode, DefaultProfitCentre, DefaultFpsYear);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<TestRCCostDto>>(apiResponse);
        }

        [Fact]
        public async Task GetByKeyAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureResponse<TestRCCostRes>();
            var failureDto  = ApiResponseDto<TestRCCostDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Server error" } }, new ApiMetaDto());

            _http.GetAsync<TestRCCostRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRCCostDto>>(apiResponse).Returns(failureDto);

            // Act
            var result = await _client.GetByKeyAsync(DefaultTestCode, DefaultProfitCentre, DefaultFpsYear);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── CreateAsync ───────────────────────────────────────────────────────

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var dto         = new TestRCCostDto { TestCode = DefaultTestCode, ProfitCentre = DefaultProfitCentre };
            var req         = new TestRCCostReq();
            var res         = new TestRCCostRes { TestCode = DefaultTestCode };
            var apiResponse = SuccessResponse(res);
            var expectedDto = ApiResponseDto<TestRCCostDto>.SuccessResponse(dto);

            _mapper.Map<TestRCCostReq>(dto).Returns(req);
            _http.PostAsync<TestRCCostReq, TestRCCostRes>(BaseUrl, req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRCCostDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task CreateAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto         = new TestRCCostDto { TestCode = DefaultTestCode, ProfitCentre = DefaultProfitCentre };
            var req         = new TestRCCostReq();
            var apiResponse = FailureResponse<TestRCCostRes>();
            var failureDto  = ApiResponseDto<TestRCCostDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Server error" } }, new ApiMetaDto());

            _mapper.Map<TestRCCostReq>(dto).Returns(req);
            _http.PostAsync<TestRCCostReq, TestRCCostRes>(BaseUrl, req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRCCostDto>>(apiResponse).Returns(failureDto);

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
            var dto         = new TestRCCostDto { TestCode = DefaultTestCode, ProfitCentre = DefaultProfitCentre };
            var req         = new TestRCCostReq();
            var res         = new TestRCCostRes { TestCode = DefaultTestCode };
            var apiResponse = SuccessResponse(res);
            var expectedDto = ApiResponseDto<TestRCCostDto>.SuccessResponse(dto);
            var expectedUrl = $"{BaseUrl}/{DefaultTestCode}/{DefaultProfitCentre}/{DefaultFpsYear}";

            _mapper.Map<TestRCCostReq>(dto).Returns(req);
            _http.PutAsync<TestRCCostReq, TestRCCostRes>(expectedUrl, req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRCCostDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateAsync(DefaultTestCode, DefaultProfitCentre, DefaultFpsYear, dto);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task UpdateAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto         = new TestRCCostDto { TestCode = DefaultTestCode, ProfitCentre = DefaultProfitCentre };
            var req         = new TestRCCostReq();
            var apiResponse = FailureResponse<TestRCCostRes>();
            var failureDto  = ApiResponseDto<TestRCCostDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Server error" } }, new ApiMetaDto());
            var expectedUrl = $"{BaseUrl}/{DefaultTestCode}/{DefaultProfitCentre}/{DefaultFpsYear}";

            _mapper.Map<TestRCCostReq>(dto).Returns(req);
            _http.PutAsync<TestRCCostReq, TestRCCostRes>(expectedUrl, req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRCCostDto>>(apiResponse).Returns(failureDto);

            // Act
            var result = await _client.UpdateAsync(DefaultTestCode, DefaultProfitCentre, DefaultFpsYear, dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateAsync_UrlContainsAllPkSegments_CorrectRouteCalled()
        {
            // Arrange
            var dto         = new TestRCCostDto();
            var req         = new TestRCCostReq();
            var apiResponse = SuccessResponse(new TestRCCostRes());
            var expectedDto = ApiResponseDto<TestRCCostDto>.SuccessResponse(new TestRCCostDto());
            string capturedUrl = string.Empty;

            _mapper.Map<TestRCCostReq>(dto).Returns(req);
            _http.PutAsync<TestRCCostReq, TestRCCostRes>(
                    Arg.Do<string>(u => capturedUrl = u), req)
                 .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRCCostDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.UpdateAsync(DefaultTestCode, DefaultProfitCentre, DefaultFpsYear, dto);

            // Assert
            Assert.Equal($"{BaseUrl}/{DefaultTestCode}/{DefaultProfitCentre}/{DefaultFpsYear}", capturedUrl);
        }

        #endregion

        // ── DeleteAsync ───────────────────────────────────────────────────────

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var apiResponse = SuccessResponse<bool?>(true);
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);
            var expectedUrl = $"{BaseUrl}/{DefaultTestCode}/{DefaultProfitCentre}/{DefaultFpsYear}";

            _http.DeleteAsync<bool?>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteAsync(DefaultTestCode, DefaultProfitCentre, DefaultFpsYear);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureResponse<bool?>();
            var failureDto  = ApiResponseDto<bool>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Server error" } }, new ApiMetaDto());
            var expectedUrl = $"{BaseUrl}/{DefaultTestCode}/{DefaultProfitCentre}/{DefaultFpsYear}";

            _http.DeleteAsync<bool?>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(failureDto);

            // Act
            var result = await _client.DeleteAsync(DefaultTestCode, DefaultProfitCentre, DefaultFpsYear);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_UrlContainsAllPkSegments_CorrectRouteCalled()
        {
            // Arrange
            var apiResponse = SuccessResponse<bool?>(true);
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);
            string capturedUrl = string.Empty;

            _http.DeleteAsync<bool?>(Arg.Do<string>(u => capturedUrl = u)).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.DeleteAsync(DefaultTestCode, DefaultProfitCentre, DefaultFpsYear);

            // Assert
            Assert.Equal($"{BaseUrl}/{DefaultTestCode}/{DefaultProfitCentre}/{DefaultFpsYear}", capturedUrl);
        }

        #endregion

        #region FpsApiDtoMapper profile — TestRCCost

        [Fact]
        public void FpsApiDtoMapper_TestRCCostDto_MapsToResAndReq()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<FpsApiDtoMapper>(), NullLoggerFactory.Instance);
            var mapper = config.CreateMapper();

            var dto = new TestRCCostDto { TestCode = "T001", ProfitCentre = "PC01", FpsYear = 2025 };

            var res = mapper.Map<TestRCCostRes>(dto);
            Assert.Equal(dto.TestCode, res.TestCode);

            var req = mapper.Map<TestRCCostReq>(dto);
            Assert.Equal(dto.TestCode, req.TestCode);
        }

        #endregion
    }
}
