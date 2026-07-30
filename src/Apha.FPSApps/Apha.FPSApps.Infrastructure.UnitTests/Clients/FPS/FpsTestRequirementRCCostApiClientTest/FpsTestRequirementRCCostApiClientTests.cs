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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsTestRequirementRCCostApiClientTest
{
    public class FpsTestRequirementRCCostApiClientTests
    {
        private const string DefaultTestCode    = "TEST001";
        private const string DefaultBuyer       = "BUYER01";
        private const string DefaultProfitCentre = "PC001";
        private const int    DefaultFpsYear     = 2025;
        private const string BaseUrl            = "api/v1/testrequirementrccost";

        private readonly IFpsHttpExecutor                    _http;
        private readonly IMapper                             _mapper;
        private readonly FpsTestRequirementRCCostApiClient   _client;

        public FpsTestRequirementRCCostApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsTestRequirementRCCostApiClient(_http, _mapper);
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
            var resList     = new List<TestRequirementRCCostRes> { new() { TestCode = DefaultTestCode } };
            var apiResponse = SuccessResponse(resList);
            var expectedDto = ApiResponseDto<List<TestRequirementRCCostDto>>.SuccessResponse(
                new List<TestRequirementRCCostDto> { new() { TestCode = DefaultTestCode } });

            _http.GetAsync<List<TestRequirementRCCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestRequirementRCCostDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<List<TestRequirementRCCostDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetByTestCodeAsync_UrlContainsTestCode_CorrectRouteCalled()
        {
            // Arrange
            var apiResponse = SuccessResponse(new List<TestRequirementRCCostRes>());
            var expectedDto = ApiResponseDto<List<TestRequirementRCCostDto>>.SuccessResponse(
                new List<TestRequirementRCCostDto>());
            string capturedUrl = string.Empty;

            _http.GetAsync<List<TestRequirementRCCostRes>>(Arg.Do<string>(u => capturedUrl = u))
                 .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestRequirementRCCostDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear);

            // Assert
            Assert.Equal($"{BaseUrl}/{DefaultTestCode}", capturedUrl);
        }

        [Fact]
        public async Task GetByTestCodeAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureResponse<List<TestRequirementRCCostRes>>();
            var failureDto  = ApiResponseDto<List<TestRequirementRCCostDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Server error" } }, new ApiMetaDto());

            _http.GetAsync<List<TestRequirementRCCostRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestRequirementRCCostDto>>>(apiResponse).Returns(failureDto);

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
            var res         = new TestRequirementRCCostRes { TestCode = DefaultTestCode };
            var apiResponse = SuccessResponse(res);
            var expectedDto = ApiResponseDto<TestRequirementRCCostDto>.SuccessResponse(
                new TestRequirementRCCostDto { TestCode = DefaultTestCode });

            _http.GetAsync<TestRequirementRCCostRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetByKeyAsync(
                DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ApiResponseDto<TestRequirementRCCostDto>>(apiResponse);
        }

        [Fact]
        public async Task GetByKeyAsync_UrlContainsCompositeKey_CorrectRouteCalled()
        {
            // Arrange
            var apiResponse = SuccessResponse(new TestRequirementRCCostRes());
            var expectedDto = ApiResponseDto<TestRequirementRCCostDto>.SuccessResponse(
                new TestRequirementRCCostDto());
            string capturedUrl = string.Empty;

            _http.GetAsync<TestRequirementRCCostRes>(Arg.Do<string>(u => capturedUrl = u))
                 .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetByKeyAsync(
                DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear);

            // Assert
            Assert.Equal(
                $"{BaseUrl}/{DefaultTestCode}/{DefaultBuyer}/{DefaultProfitCentre}",
                capturedUrl);
        }

        [Fact]
        public async Task GetByKeyAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureResponse<TestRequirementRCCostRes>();
            var failureDto  = ApiResponseDto<TestRequirementRCCostDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Server error" } }, new ApiMetaDto());

            _http.GetAsync<TestRequirementRCCostRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(apiResponse).Returns(failureDto);

            // Act
            var result = await _client.GetByKeyAsync(
                DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear);

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
            var dto         = new TestRequirementRCCostDto { TestCode = DefaultTestCode };
            var req         = new TestRequirementRCCostReq();
            var res         = new TestRequirementRCCostRes { TestCode = DefaultTestCode };
            var apiResponse = SuccessResponse(res);
            var expectedDto = ApiResponseDto<TestRequirementRCCostDto>.SuccessResponse(dto);

            _mapper.Map<TestRequirementRCCostReq>(dto).Returns(req);
            _http.PostAsync<TestRequirementRCCostReq, TestRequirementRCCostRes>(BaseUrl, req)
                 .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task CreateAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto         = new TestRequirementRCCostDto { TestCode = DefaultTestCode };
            var req         = new TestRequirementRCCostReq();
            var apiResponse = FailureResponse<TestRequirementRCCostRes>();
            var failureDto  = ApiResponseDto<TestRequirementRCCostDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Server error" } }, new ApiMetaDto());

            _mapper.Map<TestRequirementRCCostReq>(dto).Returns(req);
            _http.PostAsync<TestRequirementRCCostReq, TestRequirementRCCostRes>(BaseUrl, req)
                 .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(apiResponse).Returns(failureDto);

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
            var dto         = new TestRequirementRCCostDto { TestCode = DefaultTestCode };
            var req         = new TestRequirementRCCostReq();
            var res         = new TestRequirementRCCostRes { TestCode = DefaultTestCode };
            var apiResponse = SuccessResponse(res);
            var expectedDto = ApiResponseDto<TestRequirementRCCostDto>.SuccessResponse(dto);
            var expectedUrl = $"{BaseUrl}/{DefaultTestCode}/{DefaultBuyer}/{DefaultProfitCentre}/{DefaultFpsYear}";

            _mapper.Map<TestRequirementRCCostReq>(dto).Returns(req);
            _http.PutAsync<TestRequirementRCCostReq, TestRequirementRCCostRes>(expectedUrl, req)
                 .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateAsync(
                DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear, dto);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task UpdateAsync_UrlContainsAllPkSegments_CorrectRouteCalled()
        {
            // Arrange
            var dto         = new TestRequirementRCCostDto();
            var req         = new TestRequirementRCCostReq();
            var apiResponse = SuccessResponse(new TestRequirementRCCostRes());
            var expectedDto = ApiResponseDto<TestRequirementRCCostDto>.SuccessResponse(
                new TestRequirementRCCostDto());
            string capturedUrl = string.Empty;

            _mapper.Map<TestRequirementRCCostReq>(dto).Returns(req);
            _http.PutAsync<TestRequirementRCCostReq, TestRequirementRCCostRes>(
                    Arg.Do<string>(u => capturedUrl = u), req)
                 .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.UpdateAsync(
                DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear, dto);

            // Assert
            Assert.Equal(
                $"{BaseUrl}/{DefaultTestCode}/{DefaultBuyer}/{DefaultProfitCentre}/{DefaultFpsYear}",
                capturedUrl);
        }

        [Fact]
        public async Task UpdateAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto         = new TestRequirementRCCostDto { TestCode = DefaultTestCode };
            var req         = new TestRequirementRCCostReq();
            var apiResponse = FailureResponse<TestRequirementRCCostRes>();
            var failureDto  = ApiResponseDto<TestRequirementRCCostDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Server error" } }, new ApiMetaDto());
            var expectedUrl = $"{BaseUrl}/{DefaultTestCode}/{DefaultBuyer}/{DefaultProfitCentre}/{DefaultFpsYear}";

            _mapper.Map<TestRequirementRCCostReq>(dto).Returns(req);
            _http.PutAsync<TestRequirementRCCostReq, TestRequirementRCCostRes>(expectedUrl, req)
                 .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestRequirementRCCostDto>>(apiResponse).Returns(failureDto);

            // Act
            var result = await _client.UpdateAsync(
                DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear, dto);

            // Assert
            Assert.False(result.Success);
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
            var expectedUrl =
                $"{BaseUrl}/{DefaultTestCode}/{DefaultBuyer}/{DefaultProfitCentre}/{DefaultFpsYear}";

            _http.DeleteAsync<bool?>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteAsync(
                DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear);

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
            var expectedUrl =
                $"{BaseUrl}/{DefaultTestCode}/{DefaultBuyer}/{DefaultProfitCentre}/{DefaultFpsYear}";

            _http.DeleteAsync<bool?>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(failureDto);

            // Act
            var result = await _client.DeleteAsync(
                DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear);

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
            await _client.DeleteAsync(
                DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear);

            // Assert
            Assert.Equal(
                $"{BaseUrl}/{DefaultTestCode}/{DefaultBuyer}/{DefaultProfitCentre}/{DefaultFpsYear}",
                capturedUrl);
        }

        #endregion

        #region FpsApiDtoMapper profile — TestRequirementRCCost

        [Fact]
        public void FpsApiDtoMapper_TestRequirementRCCostDto_MapsToResAndReq()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<FpsApiDtoMapper>(), NullLoggerFactory.Instance);
            var mapper = config.CreateMapper();

            var dto = new TestRequirementRCCostDto { TestCode = "T001", Buyer = "B01", ProfitCentre = "PC01", FpsYear = 2025 };

            var res = mapper.Map<TestRequirementRCCostRes>(dto);
            Assert.Equal(dto.TestCode, res.TestCode);

            var req = mapper.Map<TestRequirementRCCostReq>(dto);
            Assert.Equal(dto.TestCode, req.TestCode);
        }

        #endregion
    }
}
