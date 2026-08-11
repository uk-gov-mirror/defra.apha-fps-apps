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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsAccessUserApiClientTest
{
    public class PimsAccessUserApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsAccessUserApiClient _client;

        private const string BaseUrl = "api/v1/accessuser";

        public PimsAccessUserApiClientTests()
        {
            _http   = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsAccessUserApiClient(_http, _mapper);
        }

        private static ApiResponse<T> SuccessApiResponse<T>(T data) =>
            new ApiResponse<T> { Success = true, Data = data };

        private static ApiResponse<T> FailureApiResponse<T>() =>
            new ApiResponse<T>
            {
                Success = false,
                Errors  = new List<ApiError> { new ApiError { Code = "ERR", Message = "API error" } }
            };

        private static ApiResponseDto<T> SuccessDto<T>(T data) =>
            ApiResponseDto<T>.SuccessResponse(data);

        private static ApiResponseDto<T> FailureDto<T>() =>
            ApiResponseDto<T>.FailureResponse(
                new List<ApiErrorDto> { new ApiErrorDto { Code = "ERR", Message = "Error" } },
                new ApiMetaDto());

        private static AccessUserRes MakeRes(int systemid = 1, string ntlogin = "DOM\\user1") =>
            new AccessUserRes { SystemId = systemid, NtLogin = ntlogin, UserName = "Test User" };

        private static AccessUserDto MakeDto(int systemid = 1, string ntlogin = "DOM\\user1") =>
            new AccessUserDto { SystemId = systemid, NtLogin = ntlogin, UserName = "Test User" };


        #region GetPagedAsync

        [Fact]
        public async Task GetPagedAsync_HttpReturnsSuccess_ReturnsPaginatedResult()
        {
            // Arrange
            var request = new QueryParameters<string> { Page = 2, PageSize = 5, Search = "dom" };
            var apiResp = SuccessApiResponse(new List<AccessUserRes> { MakeRes(1, "dom\\u1"), MakeRes(1, "dom\\u2") });
            apiResp.Pagination = new Pagination { PageNumber = 2, PageSize = 5, TotalRecords = 20, TotalPages = 4 };
            var mappedItems = new List<AccessUserDto> { MakeDto(1, "dom\\u1"), MakeDto(1, "dom\\u2") };
            _http.GetAsync<List<AccessUserRes>>(Arg.Is<string>(s => s.StartsWith("api/v1/accessuser/paged"))).Returns(apiResp);
            _mapper.Map<List<AccessUserDto>>(apiResp.Data!).Returns(mappedItems);

            // Act
            var result = await _client.GetPagedAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(20, result.Data!.TotalCount);
            Assert.Equal(2, result.Data.PageNumber);
            Assert.Equal(5, result.Data.PageSize);
            Assert.Equal(2, result.Data.data.Count());
            await _http.Received(1).GetAsync<List<AccessUserRes>>(Arg.Is<string>(s => s.StartsWith("api/v1/accessuser/paged")));
            _mapper.Received(1).Map<List<AccessUserDto>>(apiResp.Data!);
        }

        [Fact]
        public async Task GetPagedAsync_WhenPaginationMissing_UsesRequestPagingAndItemCount()
        {
            // Arrange
            var request = new QueryParameters<string> { Page = 3, PageSize = 7 };
            var apiResp = SuccessApiResponse(new List<AccessUserRes> { MakeRes(1, "dom\\u1") });
            var mappedItems = new List<AccessUserDto> { MakeDto(1, "dom\\u1") };
            _http.GetAsync<List<AccessUserRes>>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<List<AccessUserDto>>(apiResp.Data!).Returns(mappedItems);

            // Act
            var result = await _client.GetPagedAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data!.PageNumber);
            Assert.Equal(7, result.Data.PageSize);
            Assert.Equal(1, result.Data.TotalCount);
        }

        [Fact]
        public async Task GetPagedAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var request = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResp = FailureApiResponse<List<AccessUserRes>>();
            var errorDtos = new List<ApiErrorDto> { new ApiErrorDto { Code = "ERR", Message = "API error" } };
            var metaDto = new ApiMetaDto();
            _http.GetAsync<List<AccessUserRes>>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<List<ApiErrorDto>>(apiResp.Errors!).Returns(errorDtos);
            _mapper.Map<ApiMetaDto>(apiResp.Meta).Returns(metaDto);

            // Act
            var result = await _client.GetPagedAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("ERR", result.Errors![0].Code);
        }

        [Fact]
        public async Task GetPagedAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            // Arrange
            var request = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<AccessUserRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("timeout"));

            // Act
            var result = await _client.GetPagedAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            var resList = new List<AccessUserRes> { MakeRes() };
            var apiResp = SuccessApiResponse(resList);
            var dto     = SuccessDto(new List<AccessUserDto> { MakeDto() });
            _http.GetAsync<List<AccessUserRes>>(BaseUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AccessUserDto>>>(apiResp).Returns(dto);

            // Act
            var result = await _client.GetAllAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<AccessUserRes>>(BaseUrl);
            _mapper.Received(1).Map<ApiResponseDto<List<AccessUserDto>>>(apiResp);
        }

        [Fact]
        public async Task GetAllAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            // Arrange
            _http.GetAsync<List<AccessUserRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task GetAllAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResp = FailureApiResponse<List<AccessUserRes>>();
            var dto     = FailureDto<List<AccessUserDto>>();
            _http.GetAsync<List<AccessUserRes>>(BaseUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AccessUserDto>>>(apiResp).Returns(dto);

            // Act
            var result = await _client.GetAllAsync();

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── GetBySystemIdAsync ────────────────────────────────────────────────────

        #region GetBySystemIdAsync

        [Fact]
        public async Task GetBySystemIdAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            const int systemid  = 2;
            var expectedUrl     = $"{BaseUrl}/{systemid}";
            var resList         = new List<AccessUserRes> { MakeRes(systemid) };
            var apiResp         = SuccessApiResponse(resList);
            var dto             = SuccessDto(new List<AccessUserDto> { MakeDto(systemid) });
            _http.GetAsync<List<AccessUserRes>>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AccessUserDto>>>(apiResp).Returns(dto);

            // Act
            var result = await _client.GetBySystemIdAsync(systemid);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<AccessUserRes>>(expectedUrl);
        }

        [Fact]
        public async Task GetBySystemIdAsync_ConstructsCorrectUrl()
        {
            // Arrange
            const int systemid  = 5;
            var expectedUrl     = $"{BaseUrl}/{systemid}";
            var apiResp         = SuccessApiResponse(new List<AccessUserRes>());
            var dto             = SuccessDto(new List<AccessUserDto>());
            _http.GetAsync<List<AccessUserRes>>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<AccessUserDto>>>(apiResp).Returns(dto);

            // Act
            await _client.GetBySystemIdAsync(systemid);

            // Assert
            await _http.Received(1).GetAsync<List<AccessUserRes>>(Arg.Is<string>(s => s == $"{BaseUrl}/{systemid}"));
        }

        [Fact]
        public async Task GetBySystemIdAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            // Arrange
            _http.GetAsync<List<AccessUserRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("timeout"));

            // Act
            var result = await _client.GetBySystemIdAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        // ── GetByIdAsync ──────────────────────────────────────────────────────────

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            const int systemid  = 1;
            const string ntlogin = "DOM\\user1";
            var encodedLogin    = Uri.EscapeDataString(ntlogin);
            var expectedUrl     = $"{BaseUrl}/{systemid}/{encodedLogin}";
            var apiResp         = SuccessApiResponse(MakeRes(systemid, ntlogin));
            var dto             = SuccessDto(MakeDto(systemid, ntlogin));
            _http.GetAsync<AccessUserRes>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<AccessUserDto>>(apiResp).Returns(dto);

            // Act
            var result = await _client.GetByIdAsync(systemid, ntlogin);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<AccessUserRes>(expectedUrl);
            _mapper.Received(1).Map<ApiResponseDto<AccessUserDto>>(apiResp);
        }

        [Fact]
        public async Task GetByIdAsync_EncodesNtLoginInUrl()
        {
            // Arrange — backslash must be percent-encoded
            const int systemid   = 1;
            const string ntlogin = "DOM\\jsmith";
            var encodedLogin     = Uri.EscapeDataString(ntlogin); // "DOM%5Cjsmith"
            var expectedUrl      = $"{BaseUrl}/{systemid}/{encodedLogin}";
            var apiResp          = SuccessApiResponse(MakeRes(systemid, ntlogin));
            var dto              = SuccessDto(MakeDto(systemid, ntlogin));
            _http.GetAsync<AccessUserRes>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<AccessUserDto>>(apiResp).Returns(dto);

            // Act
            await _client.GetByIdAsync(systemid, ntlogin);

            // Assert
            await _http.Received(1).GetAsync<AccessUserRes>(
                Arg.Is<string>(s => s.Contains(Uri.EscapeDataString(ntlogin))));
        }

        [Fact]
        public async Task GetByIdAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResp = FailureApiResponse<AccessUserRes>();
            var dto     = FailureDto<AccessUserDto>();
            _http.GetAsync<AccessUserRes>(Arg.Any<string>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<AccessUserDto>>(apiResp).Returns(dto);

            // Act
            var result = await _client.GetByIdAsync(99, "DOM\\unknown");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetByIdAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            // Arrange
            _http.GetAsync<AccessUserRes>(Arg.Any<string>()).ThrowsAsync(new Exception("timeout"));

            // Act
            var result = await _client.GetByIdAsync(1, "DOM\\user");

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        // ── CreateAsync ───────────────────────────────────────────────────────────

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var inputDto = MakeDto(1, "DOM\\newuser");
            var req      = new AccessUserReq { SystemId = 1, NtLogin = "DOM\\newuser" };
            var apiResp  = SuccessApiResponse(MakeRes(1, "DOM\\newuser"));
            var dto      = SuccessDto(MakeDto(1, "DOM\\newuser"));
            _mapper.Map<AccessUserReq>(inputDto).Returns(req);
            _http.PostAsync<AccessUserReq, AccessUserRes>(BaseUrl, req).Returns(apiResp);
            _mapper.Map<ApiResponseDto<AccessUserDto>>(apiResp).Returns(dto);

            // Act
            var result = await _client.CreateAsync(inputDto);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<AccessUserReq>(inputDto);
            await _http.Received(1).PostAsync<AccessUserReq, AccessUserRes>(BaseUrl, req);
            _mapper.Received(1).Map<ApiResponseDto<AccessUserDto>>(apiResp);
        }

        [Fact]
        public async Task CreateAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            // Arrange
            _mapper.Map<AccessUserReq>(Arg.Any<AccessUserDto>()).Returns(new AccessUserReq());
            _http.PostAsync<AccessUserReq, AccessUserRes>(Arg.Any<string>(), Arg.Any<AccessUserReq>())
                 .ThrowsAsync(new Exception("POST failed"));

            // Act
            var result = await _client.CreateAsync(MakeDto());

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────────

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            const int systemid   = 1;
            const string ntlogin = "DOM\\user";
            var encodedLogin     = Uri.EscapeDataString(ntlogin);
            var expectedUrl      = $"{BaseUrl}/{systemid}/{encodedLogin}";
            var inputDto         = MakeDto(systemid, ntlogin);
            var req              = new AccessUserReq { SystemId = systemid, NtLogin = ntlogin };
            var apiResp          = SuccessApiResponse(MakeRes(systemid, ntlogin));
            var dto              = SuccessDto(MakeDto(systemid, ntlogin));
            _mapper.Map<AccessUserReq>(inputDto).Returns(req);
            _http.PutAsync<AccessUserReq, AccessUserRes>(expectedUrl, req).Returns(apiResp);
            _mapper.Map<ApiResponseDto<AccessUserDto>>(apiResp).Returns(dto);

            // Act
            var result = await _client.UpdateAsync(systemid, ntlogin, inputDto);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).PutAsync<AccessUserReq, AccessUserRes>(expectedUrl, req);
        }

        [Fact]
        public async Task UpdateAsync_EncodesNtLoginInUrl()
        {
            // Arrange
            const int systemid   = 1;
            const string ntlogin = "DOM\\jsmith";
            var encodedLogin     = Uri.EscapeDataString(ntlogin);
            _mapper.Map<AccessUserReq>(Arg.Any<AccessUserDto>()).Returns(new AccessUserReq());
            var apiResp = SuccessApiResponse(MakeRes(systemid, ntlogin));
            var dto     = SuccessDto(MakeDto(systemid, ntlogin));
            _http.PutAsync<AccessUserReq, AccessUserRes>(Arg.Any<string>(), Arg.Any<AccessUserReq>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<AccessUserDto>>(apiResp).Returns(dto);

            // Act
            await _client.UpdateAsync(systemid, ntlogin, MakeDto(systemid, ntlogin));

            // Assert
            await _http.Received(1).PutAsync<AccessUserReq, AccessUserRes>(
                Arg.Is<string>(s => s.Contains(encodedLogin)),
                Arg.Any<AccessUserReq>());
        }

        [Fact]
        public async Task UpdateAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            // Arrange
            _mapper.Map<AccessUserReq>(Arg.Any<AccessUserDto>()).Returns(new AccessUserReq());
            _http.PutAsync<AccessUserReq, AccessUserRes>(Arg.Any<string>(), Arg.Any<AccessUserReq>())
                 .ThrowsAsync(new Exception("PUT failed"));

            // Act
            var result = await _client.UpdateAsync(1, "DOM\\user", MakeDto());

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        // ── DeleteAsync ───────────────────────────────────────────────────────────

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            const int systemid   = 1;
            const string ntlogin = "DOM\\user";
            var encodedLogin     = Uri.EscapeDataString(ntlogin);
            var expectedUrl      = $"{BaseUrl}/{systemid}/{encodedLogin}";
            var apiResp          = SuccessApiResponse(true);
            var dto              = SuccessDto(true);
            _http.DeleteAsync<bool>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<bool>>(apiResp).Returns(dto);

            // Act
            var result = await _client.DeleteAsync(systemid, ntlogin);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<bool>(expectedUrl);
            _mapper.Received(1).Map<ApiResponseDto<bool>>(apiResp);
        }

        [Fact]
        public async Task DeleteAsync_EncodesNtLoginInUrl()
        {
            // Arrange
            const int systemid   = 2;
            const string ntlogin = "DOM\\jsmith";
            var encodedLogin     = Uri.EscapeDataString(ntlogin);
            var expectedUrl      = $"{BaseUrl}/{systemid}/{encodedLogin}";
            var apiResp          = SuccessApiResponse(true);
            var dto              = SuccessDto(true);
            _http.DeleteAsync<bool>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<bool>>(apiResp).Returns(dto);

            // Act
            await _client.DeleteAsync(systemid, ntlogin);

            // Assert
            await _http.Received(1).DeleteAsync<bool>(
                Arg.Is<string>(s => s.Contains(encodedLogin)));
        }

        [Fact]
        public async Task DeleteAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const int systemid   = 99;
            const string ntlogin = "DOM\\unknown";
            var encodedLogin     = Uri.EscapeDataString(ntlogin);
            var expectedUrl      = $"{BaseUrl}/{systemid}/{encodedLogin}";
            var apiResp          = FailureApiResponse<bool>();
            var dto              = FailureDto<bool>();
            _http.DeleteAsync<bool>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<bool>>(apiResp).Returns(dto);

            // Act
            var result = await _client.DeleteAsync(systemid, ntlogin);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            // Arrange
            _http.DeleteAsync<bool>(Arg.Any<string>()).ThrowsAsync(new Exception("DELETE failed"));

            // Act
            var result = await _client.DeleteAsync(1, "DOM\\user");

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion
    }
}
