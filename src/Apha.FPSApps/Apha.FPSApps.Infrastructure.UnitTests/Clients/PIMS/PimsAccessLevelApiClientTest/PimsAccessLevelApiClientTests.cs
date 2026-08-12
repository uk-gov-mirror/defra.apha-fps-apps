using Apha.Common.Constants;
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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsAccessLevelApiClientTest
{
    public class PimsAccessLevelApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsAccessLevelApiClient _client;

        public PimsAccessLevelApiClientTests()
        {
            _http   = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsAccessLevelApiClient(_http, _mapper);
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

        private static AccessLevelRes MakeRes(int systemid = 1, int accesslevelid = 10, string name = "Level 1") =>
            new AccessLevelRes { SystemId = systemid, AccessLevelId = accesslevelid, AccessLevelName = name };

        private static AccessLevelDto MakeDto(int systemid = 1, int accesslevelid = 10, string name = "Level 1") =>
            new AccessLevelDto { SystemId = systemid, AccessLevelId = accesslevelid, AccessLevelName = name };

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            var resList = new List<AccessLevelRes> { MakeRes() };
            var apiResp = SuccessApiResponse(resList);
            var dto     = SuccessDto(new List<AccessLevelDto> { MakeDto() });
            _http.GetAsync<List<AccessLevelRes>>(PimsApiEndpoints.GetAllAccessLevels).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AccessLevelDto>>>(apiResp).Returns(dto);

            var result = await _client.GetAllAsync();

            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<AccessLevelRes>>(PimsApiEndpoints.GetAllAccessLevels);
            _mapper.Received(1).Map<ApiResponseDto<List<AccessLevelDto>>>(apiResp);
        }

        [Fact]
        public async Task GetAllAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            var apiResp = FailureApiResponse<List<AccessLevelRes>>();
            var dto = FailureDto<List<AccessLevelDto>>();
            _http.GetAsync<List<AccessLevelRes>>(PimsApiEndpoints.GetAllAccessLevels).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AccessLevelDto>>>(apiResp).Returns(dto);

            var result = await _client.GetAllAsync();

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            _http.GetAsync<List<AccessLevelRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("network error"));

            var result = await _client.GetAllAsync();

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region GetBySystemIdAsync

        [Fact]
        public async Task GetBySystemIdAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            const int systemid = 2;
            var expectedUrl = string.Format(PimsApiEndpoints.GetAccessLevelsBySystemId, systemid);
            var resList = new List<AccessLevelRes> { MakeRes(systemid, 1) };
            var apiResp = SuccessApiResponse(resList);
            var dto = SuccessDto(new List<AccessLevelDto> { MakeDto(systemid, 1) });
            _http.GetAsync<List<AccessLevelRes>>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AccessLevelDto>>>(apiResp).Returns(dto);

            var result = await _client.GetBySystemIdAsync(systemid);

            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<AccessLevelRes>>(expectedUrl);
        }

        [Fact]
        public async Task GetBySystemIdAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            const int systemid = 2;
            var expectedUrl = string.Format(PimsApiEndpoints.GetAccessLevelsBySystemId, systemid);
            var apiResp = FailureApiResponse<List<AccessLevelRes>>();
            var dto = FailureDto<List<AccessLevelDto>>();
            _http.GetAsync<List<AccessLevelRes>>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AccessLevelDto>>>(apiResp).Returns(dto);

            var result = await _client.GetBySystemIdAsync(systemid);

            Assert.False(result.Success);
            await _http.Received(1).GetAsync<List<AccessLevelRes>>(expectedUrl);
            _mapper.Received(1).Map<ApiResponseDto<List<AccessLevelDto>>>(apiResp);
        }

        [Fact]
        public async Task GetBySystemIdAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            _http.GetAsync<List<AccessLevelRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("timeout"));

            var result = await _client.GetBySystemIdAsync(1);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            const int systemid = 1;
            const int accesslevelid = 10;
            var expectedUrl = string.Format(PimsApiEndpoints.GetAccessLevelById, systemid, accesslevelid);
            var apiResp = SuccessApiResponse(MakeRes(systemid, accesslevelid));
            var dto = SuccessDto(MakeDto(systemid, accesslevelid));
            _http.GetAsync<AccessLevelRes>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<AccessLevelDto>>(apiResp).Returns(dto);

            var result = await _client.GetByIdAsync(systemid, accesslevelid);

            Assert.True(result.Success);
            await _http.Received(1).GetAsync<AccessLevelRes>(expectedUrl);
            _mapper.Received(1).Map<ApiResponseDto<AccessLevelDto>>(apiResp);
        }

        [Fact]
        public async Task GetByIdAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            var apiResp = FailureApiResponse<AccessLevelRes>();
            var dto = FailureDto<AccessLevelDto>();
            _http.GetAsync<AccessLevelRes>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<AccessLevelDto>>(apiResp).Returns(dto);

            var result = await _client.GetByIdAsync(99, 88);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetByIdAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            _http.GetAsync<AccessLevelRes>(Arg.Any<string>()).ThrowsAsync(new Exception("timeout"));

            var result = await _client.GetByIdAsync(1, 10);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            var inputDto = MakeDto(1, 7, "Editor");
            var req = MakeRes(1, 7, "Editor");
            var apiResp = SuccessApiResponse(MakeRes(1, 7, "Editor"));
            var dto = SuccessDto(MakeDto(1, 7, "Editor"));
            _mapper.Map<AccessLevelRes>(inputDto).Returns(req);
            _http.PostAsync<AccessLevelRes, AccessLevelRes>(PimsApiEndpoints.CreateAccessLevel, req).Returns(apiResp);
            _mapper.Map<ApiResponseDto<AccessLevelDto>>(apiResp).Returns(dto);

            var result = await _client.CreateAsync(inputDto);

            Assert.True(result.Success);
            _mapper.Received(1).Map<AccessLevelRes>(inputDto);
            await _http.Received(1).PostAsync<AccessLevelRes, AccessLevelRes>(PimsApiEndpoints.CreateAccessLevel, req);
            _mapper.Received(1).Map<ApiResponseDto<AccessLevelDto>>(apiResp);
        }

        [Fact]
        public async Task CreateAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            var inputDto = MakeDto(1, 7, "Editor");
            var req = MakeRes(1, 7, "Editor");
            var apiResp = FailureApiResponse<AccessLevelRes>();
            var dto = FailureDto<AccessLevelDto>();
            _mapper.Map<AccessLevelRes>(inputDto).Returns(req);
            _http.PostAsync<AccessLevelRes, AccessLevelRes>(PimsApiEndpoints.CreateAccessLevel, req).Returns(apiResp);
            _mapper.Map<ApiResponseDto<AccessLevelDto>>(apiResp).Returns(dto);

            var result = await _client.CreateAsync(inputDto);

            Assert.False(result.Success);
            _mapper.Received(1).Map<AccessLevelRes>(inputDto);
            await _http.Received(1).PostAsync<AccessLevelRes, AccessLevelRes>(PimsApiEndpoints.CreateAccessLevel, req);
            _mapper.Received(1).Map<ApiResponseDto<AccessLevelDto>>(apiResp);
        }

        [Fact]
        public async Task CreateAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            _mapper.Map<AccessLevelRes>(Arg.Any<AccessLevelDto>()).Returns(new AccessLevelRes());
            _http.PostAsync<AccessLevelRes, AccessLevelRes>(Arg.Any<string>(), Arg.Any<AccessLevelRes>())
                 .ThrowsAsync(new Exception("POST failed"));

            var result = await _client.CreateAsync(MakeDto());

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            const int systemid = 1;
            const int accesslevelid = 7;
            var expectedUrl = string.Format(PimsApiEndpoints.UpdateAccessLevel, systemid, accesslevelid);
            var inputDto = MakeDto(systemid, accesslevelid, "Editor+");
            var req = MakeRes(systemid, accesslevelid, "Editor+");
            var apiResp = SuccessApiResponse(MakeRes(systemid, accesslevelid, "Editor+"));
            var dto = SuccessDto(MakeDto(systemid, accesslevelid, "Editor+"));
            _mapper.Map<AccessLevelRes>(inputDto).Returns(req);
            _http.PutAsync<AccessLevelRes, AccessLevelRes>(expectedUrl, req).Returns(apiResp);
            _mapper.Map<ApiResponseDto<AccessLevelDto>>(apiResp).Returns(dto);

            var result = await _client.UpdateAsync(systemid, accesslevelid, inputDto);

            Assert.True(result.Success);
            await _http.Received(1).PutAsync<AccessLevelRes, AccessLevelRes>(expectedUrl, req);
        }

        [Fact]
        public async Task UpdateAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            const int systemid = 1;
            const int accesslevelid = 7;
            var expectedUrl = string.Format(PimsApiEndpoints.UpdateAccessLevel, systemid, accesslevelid);
            var inputDto = MakeDto(systemid, accesslevelid, "Editor+");
            var req = MakeRes(systemid, accesslevelid, "Editor+");
            var apiResp = FailureApiResponse<AccessLevelRes>();
            var dto = FailureDto<AccessLevelDto>();
            _mapper.Map<AccessLevelRes>(inputDto).Returns(req);
            _http.PutAsync<AccessLevelRes, AccessLevelRes>(expectedUrl, req).Returns(apiResp);
            _mapper.Map<ApiResponseDto<AccessLevelDto>>(apiResp).Returns(dto);

            var result = await _client.UpdateAsync(systemid, accesslevelid, inputDto);

            Assert.False(result.Success);
            _mapper.Received(1).Map<AccessLevelRes>(inputDto);
            await _http.Received(1).PutAsync<AccessLevelRes, AccessLevelRes>(expectedUrl, req);
            _mapper.Received(1).Map<ApiResponseDto<AccessLevelDto>>(apiResp);
        }

        [Fact]
        public async Task UpdateAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            _mapper.Map<AccessLevelRes>(Arg.Any<AccessLevelDto>()).Returns(new AccessLevelRes());
            _http.PutAsync<AccessLevelRes, AccessLevelRes>(Arg.Any<string>(), Arg.Any<AccessLevelRes>())
                 .ThrowsAsync(new Exception("PUT failed"));

            var result = await _client.UpdateAsync(1, 7, MakeDto());

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            const int systemid = 1;
            const int accesslevelid = 7;
            var expectedUrl = string.Format(PimsApiEndpoints.DeleteAccessLevel, systemid, accesslevelid);
            var apiResp = SuccessApiResponse(true);
            var dto = SuccessDto(true);
            _http.DeleteAsync<bool>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<bool>>(apiResp).Returns(dto);

            var result = await _client.DeleteAsync(systemid, accesslevelid);

            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<bool>(expectedUrl);
            _mapper.Received(1).Map<ApiResponseDto<bool>>(apiResp);
        }

        [Fact]
        public async Task DeleteAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            var apiResp = FailureApiResponse<bool>();
            var dto = FailureDto<bool>();
            _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<bool>>(apiResp).Returns(dto);

            var result = await _client.DeleteAsync(99, 88);

            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            _http.DeleteAsync<bool>(Arg.Any<string>()).ThrowsAsync(new Exception("DELETE failed"));

            var result = await _client.DeleteAsync(1, 7);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion
    }
}
