using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsAccessSystemApiClientTest
{
    public class PimsAccessSystemApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsAccessSystemApiClient _client;

        private const string BaseUrl = "api/v1/accesssystem";

        public PimsAccessSystemApiClientTests()
        {
            _http   = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsAccessSystemApiClient(_http, _mapper);
        }

        private static ApiResponse<T> SuccessApiResponse<T>(T data) =>
            new ApiResponse<T> { Success = true, Data = data };

        private static ApiResponse<T> FailureApiResponse<T>() =>
            new ApiResponse<T>
            {
                Success = false,
                Errors = new List<ApiError> { new ApiError { Code = "ERR", Message = "API error" } }
            };

        private static ApiResponseDto<T> SuccessDto<T>(T data) =>
            ApiResponseDto<T>.SuccessResponse(data);

        private static ApiResponseDto<T> FailureDto<T>() =>
            ApiResponseDto<T>.FailureResponse(
                new List<ApiErrorDto> { new ApiErrorDto { Code = "ERR", Message = "Error" } },
                new ApiMetaDto());

        private static AccessSystemRes MakeRes(int systemid = 1, string name = "PIMS") =>
            new AccessSystemRes { SystemId = systemid, SystemName = name };

        private static AccessSystemDto MakeDto(int systemid = 1, string name = "PIMS") =>
            new AccessSystemDto { SystemId = systemid, SystemName = name };

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            var resList = new List<AccessSystemRes> { MakeRes() };
            var apiResp = SuccessApiResponse(resList);
            var dto     = SuccessDto(new List<AccessSystemDto> { MakeDto() });
            _http.GetAsync<List<AccessSystemRes>>(BaseUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AccessSystemDto>>>(apiResp).Returns(dto);

            var result = await _client.GetAllAsync();

            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<AccessSystemRes>>(BaseUrl);
            _mapper.Received(1).Map<ApiResponseDto<List<AccessSystemDto>>>(apiResp);
        }

        [Fact]
        public async Task GetAllAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            var apiResp = FailureApiResponse<List<AccessSystemRes>>();
            var dto     = FailureDto<List<AccessSystemDto>>();
            _http.GetAsync<List<AccessSystemRes>>(BaseUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AccessSystemDto>>>(apiResp).Returns(dto);

            var result = await _client.GetAllAsync();

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            _http.GetAsync<List<AccessSystemRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            var result = await _client.GetAllAsync();

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            const int systemid = 2;
            var expectedUrl = $"{BaseUrl}/{systemid}";
            var apiResp = SuccessApiResponse(MakeRes(systemid, "FPS"));
            var dto = SuccessDto(MakeDto(systemid, "FPS"));
            _http.GetAsync<AccessSystemRes>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<AccessSystemDto>>(apiResp).Returns(dto);

            var result = await _client.GetByIdAsync(systemid);

            Assert.True(result.Success);
            Assert.Equal(systemid, result.Data!.SystemId);
            await _http.Received(1).GetAsync<AccessSystemRes>(expectedUrl);
            _mapper.Received(1).Map<ApiResponseDto<AccessSystemDto>>(apiResp);
        }

        [Fact]
        public async Task GetByIdAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            var apiResp = FailureApiResponse<AccessSystemRes>();
            var dto = FailureDto<AccessSystemDto>();
            _http.GetAsync<AccessSystemRes>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<AccessSystemDto>>(apiResp).Returns(dto);

            var result = await _client.GetByIdAsync(99);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetByIdAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            _http.GetAsync<AccessSystemRes>(Arg.Any<string>()).ThrowsAsync(new Exception("timeout"));

            var result = await _client.GetByIdAsync(1);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion
    }
}
