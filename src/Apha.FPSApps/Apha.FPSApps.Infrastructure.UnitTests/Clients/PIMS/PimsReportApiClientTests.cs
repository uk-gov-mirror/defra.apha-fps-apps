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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS
{
    public class PimsReportApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsReportApiClient _client;

        private const string BaseUrl = "api/v1/report";

        public PimsReportApiClientTests()
        {
            _http   = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsReportApiClient(_http, _mapper);
        }

        // ── helpers ───────────────────────────────────────────────────────────────

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

        private static ReportRes MakeRes(int id = 1) =>
            new ReportRes { Id = id, ReportName = $"R{id}", Type = "R" };

        private static ReportDto MakeDto(int id = 1) =>
            new ReportDto { Id = id, ReportName = $"R{id}", Type = "R" };

        // ── GetAllAsync ───────────────────────────────────────────────────────────

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var resList = new List<ReportRes> { MakeRes(1) };
            var apiResp = SuccessApiResponse(resList);
            var dto     = SuccessDto(new List<ReportDto> { MakeDto(1) });
            _http.GetAsync<List<ReportRes>>(BaseUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<ReportDto>>>(apiResp).Returns(dto);

            // Act
            var result = await _client.GetAllReportsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<ReportRes>>(BaseUrl);
            _mapper.Received(1).Map<ApiResponseDto<List<ReportDto>>>(apiResp);
        }

        [Fact]
        public async Task GetAllAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResp = FailureApiResponse<List<ReportRes>>();
            var dto     = FailureDto<List<ReportDto>>();
            _http.GetAsync<List<ReportRes>>(BaseUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<ReportDto>>>(apiResp).Returns(dto);

            // Act
            var result = await _client.GetAllReportsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            // Arrange
            _http.GetAsync<List<ReportRes>>(Arg.Any<string>())
                 .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllReportsAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task GetAllAsync_UsesCorrectBaseUrl()
        {
            // Arrange
            var apiResp = SuccessApiResponse(new List<ReportRes>());
            var dto     = SuccessDto(new List<ReportDto>());
            _http.GetAsync<List<ReportRes>>(BaseUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<List<ReportDto>>>(apiResp).Returns(dto);

            // Act
            await _client.GetAllReportsAsync();

            // Assert
            await _http.Received(1).GetAsync<List<ReportRes>>(Arg.Is<string>(s => s == BaseUrl));
        }

        #endregion

        // ── GetByIdAsync ──────────────────────────────────────────────────────────

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var expectedUrl = $"{BaseUrl}/5";
            var apiResp = SuccessApiResponse(MakeRes(5));
            var dto     = SuccessDto(MakeDto(5));
            _http.GetAsync<ReportRes>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<ReportDto>>(apiResp).Returns(dto);

            // Act
            var result = await _client.GetReportByIdAsync(5);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(5, result.Data!.Id);
            await _http.Received(1).GetAsync<ReportRes>(expectedUrl);
            _mapper.Received(1).Map<ApiResponseDto<ReportDto>>(apiResp);
        }

        [Fact]
        public async Task GetByIdAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var expectedUrl = $"{BaseUrl}/99";
            var apiResp = FailureApiResponse<ReportRes>();
            var dto     = FailureDto<ReportDto>();
            _http.GetAsync<ReportRes>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<ReportDto>>(apiResp).Returns(dto);

            // Act
            var result = await _client.GetReportByIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetByIdAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            // Arrange
            _http.GetAsync<ReportRes>(Arg.Any<string>()).ThrowsAsync(new Exception("Timeout"));

            // Act
            var result = await _client.GetReportByIdAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task GetByIdAsync_ConstructsCorrectUrl()
        {
            // Arrange
            const int id = 7;
            var expectedUrl = $"{BaseUrl}/{id}";
            var apiResp = SuccessApiResponse(MakeRes(id));
            var dto     = SuccessDto(MakeDto(id));
            _http.GetAsync<ReportRes>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<ReportDto>>(apiResp).Returns(dto);

            // Act
            await _client.GetReportByIdAsync(id);

            // Assert
            await _http.Received(1).GetAsync<ReportRes>(Arg.Is<string>(s => s == $"{BaseUrl}/{id}"));
        }

        #endregion

        // ── CreateAsync ───────────────────────────────────────────────────────────

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            var inputDto = MakeDto(0);
            var req      = new ReportReq { ReportName = "New", Type = "R" };
            var apiResp  = SuccessApiResponse(MakeRes(10));
            var dto      = SuccessDto(MakeDto(10));
            _mapper.Map<ReportReq>(inputDto).Returns(req);
            _http.PostAsync<ReportReq, ReportRes>(BaseUrl, req).Returns(apiResp);
            _mapper.Map<ApiResponseDto<ReportDto>>(apiResp).Returns(dto);

            // Act
            var result = await _client.CreateReportAsync(inputDto);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<ReportReq>(inputDto);
            await _http.Received(1).PostAsync<ReportReq, ReportRes>(BaseUrl, req);
            _mapper.Received(1).Map<ApiResponseDto<ReportDto>>(apiResp);
        }

        [Fact]
        public async Task CreateAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var inputDto = MakeDto(0);
            var req      = new ReportReq();
            var apiResp  = FailureApiResponse<ReportRes>();
            var dto      = FailureDto<ReportDto>();
            _mapper.Map<ReportReq>(inputDto).Returns(req);
            _http.PostAsync<ReportReq, ReportRes>(Arg.Any<string>(), Arg.Any<ReportReq>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<ReportDto>>(apiResp).Returns(dto);

            // Act
            var result = await _client.CreateReportAsync(inputDto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            // Arrange
            _mapper.Map<ReportReq>(Arg.Any<ReportDto>()).Returns(new ReportReq());
            _http.PostAsync<ReportReq, ReportRes>(Arg.Any<string>(), Arg.Any<ReportReq>())
                 .ThrowsAsync(new Exception("POST failed"));

            // Act
            var result = await _client.CreateReportAsync(MakeDto(0));

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
            const int id    = 5;
            var inputDto    = MakeDto(id);
            var req         = new ReportReq { ReportName = "Updated", Type = "R" };
            var expectedUrl = $"{BaseUrl}/{id}";
            var apiResp     = SuccessApiResponse(MakeRes(id));
            var dto         = SuccessDto(MakeDto(id));
            _mapper.Map<ReportReq>(inputDto).Returns(req);
            _http.PutAsync<ReportReq, ReportRes>(expectedUrl, req).Returns(apiResp);
            _mapper.Map<ApiResponseDto<ReportDto>>(apiResp).Returns(dto);

            // Act
            var result = await _client.UpdateReportAsync(id, inputDto);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).PutAsync<ReportReq, ReportRes>(expectedUrl, req);
            _mapper.Received(1).Map<ApiResponseDto<ReportDto>>(apiResp);
        }

        [Fact]
        public async Task UpdateAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            // Arrange
            _mapper.Map<ReportReq>(Arg.Any<ReportDto>()).Returns(new ReportReq());
            _http.PutAsync<ReportReq, ReportRes>(Arg.Any<string>(), Arg.Any<ReportReq>())
                 .ThrowsAsync(new Exception("PUT failed"));

            // Act
            var result = await _client.UpdateReportAsync(1, MakeDto(1));

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task UpdateAsync_ConstructsCorrectUrl()
        {
            // Arrange
            const int id    = 3;
            var expectedUrl = $"{BaseUrl}/{id}";
            _mapper.Map<ReportReq>(Arg.Any<ReportDto>()).Returns(new ReportReq());
            var apiResp = SuccessApiResponse(MakeRes(id));
            var dto     = SuccessDto(MakeDto(id));
            _http.PutAsync<ReportReq, ReportRes>(expectedUrl, Arg.Any<ReportReq>()).Returns(apiResp);
            _mapper.Map<ApiResponseDto<ReportDto>>(apiResp).Returns(dto);

            // Act
            await _client.UpdateReportAsync(id, MakeDto(id));

            // Assert
            await _http.Received(1).PutAsync<ReportReq, ReportRes>(
                Arg.Is<string>(s => s == expectedUrl),
                Arg.Any<ReportReq>());
        }

        #endregion

        // ── DeleteAsync ───────────────────────────────────────────────────────────

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_HttpReturnsSuccess_ReturnsMappedResponse()
        {
            // Arrange
            const int id    = 8;
            var expectedUrl = $"{BaseUrl}/{id}";
            var apiResp     = SuccessApiResponse(true);
            var dto         = SuccessDto(true);
            _http.DeleteAsync<bool>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<bool>>(apiResp).Returns(dto);

            // Act
            var result = await _client.DeleteReportAsync(id);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<bool>(expectedUrl);
            _mapper.Received(1).Map<ApiResponseDto<bool>>(apiResp);
        }

        [Fact]
        public async Task DeleteAsync_HttpReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const int id    = 99;
            var expectedUrl = $"{BaseUrl}/{id}";
            var apiResp     = FailureApiResponse<bool>();
            var dto         = FailureDto<bool>();
            _http.DeleteAsync<bool>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<bool>>(apiResp).Returns(dto);

            // Act
            var result = await _client.DeleteReportAsync(id);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_HttpThrowsException_ReturnsInternalErrorResponse()
        {
            // Arrange
            _http.DeleteAsync<bool>(Arg.Any<string>()).ThrowsAsync(new Exception("DELETE failed"));

            // Act
            var result = await _client.DeleteReportAsync(1);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task DeleteAsync_ConstructsCorrectUrl()
        {
            // Arrange
            const int id    = 4;
            var expectedUrl = $"{BaseUrl}/{id}";
            var apiResp     = SuccessApiResponse(true);
            var dto         = SuccessDto(true);
            _http.DeleteAsync<bool>(expectedUrl).Returns(apiResp);
            _mapper.Map<ApiResponseDto<bool>>(apiResp).Returns(dto);

            // Act
            await _client.DeleteReportAsync(id);

            // Assert
            await _http.Received(1).DeleteAsync<bool>(Arg.Is<string>(s => s == expectedUrl));
        }

        #endregion
    }
}
