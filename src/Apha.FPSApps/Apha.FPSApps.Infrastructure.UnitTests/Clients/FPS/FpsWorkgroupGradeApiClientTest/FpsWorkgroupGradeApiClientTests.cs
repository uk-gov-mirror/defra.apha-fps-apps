using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsWorkGroupGradeApiClientTest
{
    public class FpsWorkGroupGradeApiClientTests
    {
        private const string DefaultPcGrade = "G001";
        private const string DefaultWgGrade = "WG01";

        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsWorkGroupGradeApiClient _client;

        public FpsWorkGroupGradeApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsWorkGroupGradeApiClient(_http, _mapper);
        }

        #region GetWorkGroupGradeAsync Tests

        [Fact]
        public async Task GetWorkGroupGradeAsync_WithSuccessResponse_ReturnsMappedGradeList()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList  = new List<WorkgroupGradeRes>
            {
                new() { WgGrade = DefaultWgGrade, ProfitCentreGrade = DefaultPcGrade }
            };
            var apiResponse = new ApiResponse<List<WorkgroupGradeRes>> { Success = true, Data = resList };
            var dtoList     = new List<WorkgroupGradeDto>
            {
                new() { WgGrade = DefaultWgGrade, ProfitCentreGrade = DefaultPcGrade }
            };
            var expectedDto = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(dtoList);

            _http.GetAsync<List<WorkgroupGradeRes>>(
                    Arg.Is<string>(url => url.Contains("wggrades") && url.Contains(DefaultPcGrade)))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetWorkGroupGradeAsync(query, DefaultPcGrade);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<WorkgroupGradeRes>>(
                Arg.Is<string>(url => url.Contains("wggrades") && url.Contains(DefaultPcGrade)));
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<WorkgroupGradeRes>>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "API Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<List<WorkgroupGradeDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<WorkgroupGradeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetWorkGroupGradeAsync(query, DefaultPcGrade);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_BuildsUrlWithPcGradeEncoded()
        {
            // Arrange
            const string pcGrade = "G 001";
            var query       = new QueryParameters<string>();
            var apiResponse = new ApiResponse<List<WorkgroupGradeRes>> { Success = true, Data = new List<WorkgroupGradeRes>() };
            var expectedDto = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(new List<WorkgroupGradeDto>());

            _http.GetAsync<List<WorkgroupGradeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetWorkGroupGradeAsync(query, pcGrade);

            // Assert
            await _http.Received(1).GetAsync<List<WorkgroupGradeRes>>(
                Arg.Is<string>(url => url.Contains("G%20001") || url.Contains("G+001") || url.Contains("G%20")));
        }

        #endregion

        #region DeleteWorkGroupGradeAsync Tests

        [Fact]
        public async Task DeleteWorkGroupGradeAsync_WithSuccessResponse_ReturnsSuccess()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>(Arg.Is<string>(url => url.Contains(DefaultWgGrade))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteWorkGroupGradeAsync(DefaultWgGrade);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<bool>(
                Arg.Is<string>(url => url.Contains(DefaultWgGrade)));
        }

        [Fact]
        public async Task DeleteWorkGroupGradeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.DeleteAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteWorkGroupGradeAsync(DefaultWgGrade);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetWorkgroupGradesByWorkGroupAsync Tests

        [Fact]
        public async Task GetWorkgroupGradesByWorkGroupAsync_WithSuccessResponse_ReturnsMappedGradeList()
        {
            // Arrange
            const string workGroup = "TeamA";
            var resList = new List<WorkgroupGradeRes>
            {
                new() { WgGrade = DefaultWgGrade, ProfitCentreGrade = DefaultPcGrade }
            };
            var apiResponse = new ApiResponse<List<WorkgroupGradeRes>> { Success = true, Data = resList };
            var dtoList     = new List<WorkgroupGradeDto>
            {
                new() { WgGrade = DefaultWgGrade, ProfitCentreGrade = DefaultPcGrade }
            };
            var expectedDto = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(dtoList);

            _http.GetAsync<List<WorkgroupGradeRes>>(
                    Arg.Is<string>(url => url.Contains("byworkgroup") && url.Contains(workGroup)))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetWorkgroupGradesByWorkGroupAsync(workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<WorkgroupGradeRes>>(
                Arg.Is<string>(url => url.Contains("byworkgroup") && url.Contains(workGroup)));
        }

        [Fact]
        public async Task GetWorkgroupGradesByWorkGroupAsync_WithFailureResponse_ReturnsFailureDto()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<WorkgroupGradeRes>> { Success = false };
            var mappedResponse = new ApiResponseDto<List<WorkgroupGradeDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<WorkgroupGradeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetWorkgroupGradesByWorkGroupAsync("TeamA");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetWorkgroupGradesByWorkGroupAsync_BuildsUrlWithWorkGroupEncoded()
        {
            // Arrange
            const string workGroup = "Team A";
            var apiResponse = new ApiResponse<List<WorkgroupGradeRes>> { Success = true, Data = new List<WorkgroupGradeRes>() };
            var expectedDto = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(new List<WorkgroupGradeDto>());

            _http.GetAsync<List<WorkgroupGradeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkgroupGradeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetWorkgroupGradesByWorkGroupAsync(workGroup);

            // Assert
            await _http.Received(1).GetAsync<List<WorkgroupGradeRes>>(
                Arg.Is<string>(url => url.Contains("Team%20A") || url.Contains("Team+A")));
        }

        #endregion
    }
}
