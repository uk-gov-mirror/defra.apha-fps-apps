using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsAccessUserLevelApiClientTest
{
    public class PimsAccessUserLevelApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsAccessUserLevelApiClient _client;

        private const string BaseUrl = "api/v1/accessuserlevel";

        public PimsAccessUserLevelApiClientTests()
        {
            _http   = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsAccessUserLevelApiClient(_http, _mapper);
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

        private static AccessUserLevelRes MakeRes(int systemid = 1, string ntlogin = "DOM\\user1", int accesslevelid = 10) =>
            new AccessUserLevelRes { SystemId = systemid, NtLogin = ntlogin, AccessLevelId = accesslevelid };

        private static AccessUserLevelDto MakeDto(int systemid = 1, string ntlogin = "DOM\\user1", int accesslevelid = 10) =>
            new AccessUserLevelDto { SystemId = systemid, NtLogin = ntlogin, AccessLevelId = accesslevelid };

        #region GetPagedAsync

        [Fact]
        public async Task GetPagedAsync_HttpReturnsSuccess_ReturnsPaginatedResult()
        {
            var request = new QueryParameters<string> { Page = 2, PageSize = 5, Search = "dom" };
            var apiResp = SuccessApiResponse(new List<AccessUserLevelRes> { MakeRes(1, "dom\\u1", 1), MakeRes(1, "dom\\u2", 2) });
            apiResp.Pagination = new Pagination { PageNumber = 2, PageSize = 5, TotalRecords = 20, TotalPages = 4 };
            var mappedItems = new List<AccessUserLevelDto> { MakeDto(1, "dom\\u1", 1), MakeDto(1, "dom\\u2", 2) };
            _http.GetAsync<List<AccessUserLevelRes>>(Arg.Is<string>(s => s.StartsWith("api/v1/accessuserlevel/paged"))).Returns(apiResp);
            _mapper.Map<List<AccessUserLevelDto>>(apiResp.Data!).Returns(mappedItems);

            var result = await _client.GetPagedAsync(request);

            Assert.True(result.Success);
            Assert.Equal(20, result.Data!.TotalCount);
            Assert.Equal(2, result.Data.PageNumber);
            Assert.Equal(5, result.Data.PageSize);
            Assert.Equal(2, result.Data.data.Count());
        }

        [Fact]
        public async Task GetPagedAsync_WhenPaginationMissing_UsesRequestPagingAndItemCount()
        {
            var request = new QueryParameters<string> { Page = 3, PageSize = 7 };
            var apiResp = SuccessApiResponse(new List<AccessUserLevelRes> { MakeRes(1, "dom\\u1", 1) });
            var mappedItems = new List<AccessUserLevelDto> { MakeDto(1, "dom\\u1", 1) };
            _http.GetAsync<List<AccessUserLevelRes>>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<List<AccessUserLevelDto>>(apiResp.Data!).Returns(mappedItems);

            var result = await _client.GetPagedAsync(request);

            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.PageNumber);
            Assert.Equal(7, result.Data.PageSize);
            Assert.Equal(1, result.Data.TotalCount);
        }

        [Fact]
        public async Task GetPagedAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            var request = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResp = FailureApiResponse<List<AccessUserLevelRes>>();
            var errorDtos = new List<ApiErrorDto> { new ApiErrorDto { Code = "ERR", Message = "API error" } };
            var metaDto = new ApiMetaDto();
            _http.GetAsync<List<AccessUserLevelRes>>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<List<ApiErrorDto>>(apiResp.Errors!).Returns(errorDtos);
            _mapper.Map<ApiMetaDto>(apiResp.Meta).Returns(metaDto);

            var result = await _client.GetPagedAsync(request);

            Assert.False(result.Success);
            Assert.Equal("ERR", result.Errors![0].Code);
        }

        [Fact]
        public async Task GetPagedAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            var request = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<AccessUserLevelRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("timeout"));

            var result = await _client.GetPagedAsync(request);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region GetBySystemIdAsync

        [Fact]
        public async Task GetBySystemIdAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            const int systemid = 2;
            var expectedUrl = $"{BaseUrl}/{systemid}";
            var resList = new List<AccessUserLevelRes> { MakeRes(systemid, "dom\\u1", 1) };
            var apiResp = SuccessApiResponse(resList);
            var dto = SuccessDto(new List<AccessUserLevelDto> { MakeDto(systemid, "dom\\u1", 1) });
            _http.GetAsync<List<AccessUserLevelRes>>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AccessUserLevelDto>>>(apiResp).Returns(dto);

            var result = await _client.GetBySystemIdAsync(systemid);

            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<AccessUserLevelRes>>(expectedUrl);
        }

        [Fact]
        public async Task GetBySystemIdAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            _http.GetAsync<List<AccessUserLevelRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("timeout"));

            var result = await _client.GetBySystemIdAsync(1);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region GetByUserAsync

        [Fact]
        public async Task GetByUserAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            const int systemid = 1;
            const string ntlogin = "DOM\\user1";
            var encodedLogin = Uri.EscapeDataString(ntlogin);
            var expectedUrl = $"{BaseUrl}/{systemid}/{encodedLogin}";
            var apiResp = SuccessApiResponse(new List<AccessUserLevelRes> { MakeRes(systemid, ntlogin, 1) });
            var dto = SuccessDto(new List<AccessUserLevelDto> { MakeDto(systemid, ntlogin, 1) });
            _http.GetAsync<List<AccessUserLevelRes>>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AccessUserLevelDto>>>(apiResp).Returns(dto);

            var result = await _client.GetByUserAsync(systemid, ntlogin);

            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<AccessUserLevelRes>>(expectedUrl);
        }

        [Fact]
        public async Task GetByUserAsync_EncodesNtLoginInUrl()
        {
            const string ntlogin = "DOM\\jsmith";
            var encodedLogin = Uri.EscapeDataString(ntlogin);
            var apiResp = SuccessApiResponse(new List<AccessUserLevelRes> { MakeRes(1, ntlogin, 1) });
            var dto = SuccessDto(new List<AccessUserLevelDto> { MakeDto(1, ntlogin, 1) });
            _http.GetAsync<List<AccessUserLevelRes>>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AccessUserLevelDto>>>(apiResp).Returns(dto);

            await _client.GetByUserAsync(1, ntlogin);

            await _http.Received(1).GetAsync<List<AccessUserLevelRes>>(Arg.Is<string>(s => s.Contains(encodedLogin)));
        }

        [Fact]
        public async Task GetByUserAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            _http.GetAsync<List<AccessUserLevelRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("timeout"));

            var result = await _client.GetByUserAsync(1, "DOM\\user");

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            const int systemid = 1;
            const string ntlogin = "DOM\\user1";
            const int accesslevelid = 10;
            var encodedLogin = Uri.EscapeDataString(ntlogin);
            var expectedUrl = $"{BaseUrl}/{systemid}/{encodedLogin}/{accesslevelid}";
            var apiResp = SuccessApiResponse(MakeRes(systemid, ntlogin, accesslevelid));
            var dto = SuccessDto(MakeDto(systemid, ntlogin, accesslevelid));
            _http.GetAsync<AccessUserLevelRes>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<AccessUserLevelDto>>(apiResp).Returns(dto);

            var result = await _client.GetByIdAsync(systemid, ntlogin, accesslevelid);

            Assert.True(result.Success);
            await _http.Received(1).GetAsync<AccessUserLevelRes>(expectedUrl);
        }

        [Fact]
        public async Task GetByIdAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            _http.GetAsync<AccessUserLevelRes>(Arg.Any<string>()).ThrowsAsync(new Exception("timeout"));

            var result = await _client.GetByIdAsync(1, "DOM\\user", 10);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            var inputDto = MakeDto(1, "DOM\\newuser", 7);
            var req = new AccessUserLevelReq { SystemId = 1, NtLogin = "DOM\\newuser", AccessLevelId = 7 };
            var apiResp = SuccessApiResponse(MakeRes(1, "DOM\\newuser", 7));
            var dto = SuccessDto(MakeDto(1, "DOM\\newuser", 7));
            _mapper.Map<AccessUserLevelReq>(inputDto).Returns(req);
            _http.PostAsync<AccessUserLevelReq, AccessUserLevelRes>(BaseUrl, req).Returns(apiResp);
            _mapper.Map<ApiResponseDto<AccessUserLevelDto>>(apiResp).Returns(dto);

            var result = await _client.CreateAsync(inputDto);

            Assert.True(result.Success);
            _mapper.Received(1).Map<AccessUserLevelReq>(inputDto);
            await _http.Received(1).PostAsync<AccessUserLevelReq, AccessUserLevelRes>(BaseUrl, req);
        }

        [Fact]
        public async Task CreateAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            _mapper.Map<AccessUserLevelReq>(Arg.Any<AccessUserLevelDto>()).Returns(new AccessUserLevelReq());
            _http.PostAsync<AccessUserLevelReq, AccessUserLevelRes>(Arg.Any<string>(), Arg.Any<AccessUserLevelReq>())
                .ThrowsAsync(new Exception("POST failed"));

            var result = await _client.CreateAsync(MakeDto());

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            const int systemid = 1;
            const string ntlogin = "DOM\\user";
            const int accesslevelid = 10;
            var encodedLogin = Uri.EscapeDataString(ntlogin);
            var expectedUrl = $"{BaseUrl}/{systemid}/{encodedLogin}/{accesslevelid}";
            var apiResp = SuccessApiResponse(true);
            var dto = SuccessDto(true);
            _http.DeleteAsync<bool>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<bool>>(apiResp).Returns(dto);

            var result = await _client.DeleteAsync(systemid, ntlogin, accesslevelid);

            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<bool>(expectedUrl);
        }

        [Fact]
        public async Task DeleteAsync_EncodesNtLoginInUrl()
        {
            const string ntlogin = "DOM\\jsmith";
            var encodedLogin = Uri.EscapeDataString(ntlogin);
            var apiResp = SuccessApiResponse(true);
            var dto = SuccessDto(true);
            _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<bool>>(apiResp).Returns(dto);

            await _client.DeleteAsync(1, ntlogin, 10);

            await _http.Received(1).DeleteAsync<bool>(Arg.Is<string>(s => s.Contains(encodedLogin)));
        }

        [Fact]
        public async Task DeleteAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            _http.DeleteAsync<bool>(Arg.Any<string>()).ThrowsAsync(new Exception("DELETE failed"));

            var result = await _client.DeleteAsync(1, "DOM\\user", 10);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion
    }
}
