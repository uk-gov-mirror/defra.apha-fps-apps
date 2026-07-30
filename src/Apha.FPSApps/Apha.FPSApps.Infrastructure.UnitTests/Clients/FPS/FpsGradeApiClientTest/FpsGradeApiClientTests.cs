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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsGradeApiClientTest
{
    public class FpsGradeApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsGradeApiClient _client;

        public FpsGradeApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsGradeApiClient(_http, _mapper);
        }

        private static GradeRes BuildRes(string code = "A") =>
            new() { GradeCode = code, Description = "Grade A", AvSalary = 50000m, FpsYear = 2025 };

        private static GradeDto BuildDto(string code = "A") =>
            new() { GradeCode = code, Description = "Grade A", AvSalary = 50000m, FpsYear = 2025 };

        private static ApiResponse<T> SuccessApiResponse<T>(T data) =>
            new() { Success = true, Data = data };

        private static ApiResponse<T> FailureApiResponse<T>() =>
            new()
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Error", Code = "ERROR" } }
            };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenHttpIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FpsGradeApiClient(null!, _mapper));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FpsGradeApiClient(_http, null!));
        }

        #endregion

        #region GetAllPagedAsync Tests

        [Fact]
        public async Task GetAllPagedAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList     = new List<GradeRes> { BuildRes() };
            var apiResponse = SuccessApiResponse(resList);
            var expected    = ApiResponseDto<List<GradeDto>>.SuccessResponse(
                new List<GradeDto> { BuildDto() },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

            _http.GetAsync<List<GradeRes>>(Arg.Is<string>(u => u.Contains("Grade/paged")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<GradeDto>>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.GetAllPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetAllPagedAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = FailureApiResponse<List<GradeRes>>();
            var mappedResponse = new ApiResponseDto<List<GradeDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<GradeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<GradeDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllPagedAsync_WhenHttpThrows_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _http.GetAsync<List<GradeRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetAllPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var res         = BuildRes("A");
            var apiResponse = SuccessApiResponse(res);
            var expected    = ApiResponseDto<GradeDto>.SuccessResponse(BuildDto("A"));

            _http.GetAsync<GradeRes>(Arg.Is<string>(u => u.Contains("Grade/A"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<GradeDto>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.GetByIdAsync("A");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("A", result.Data!.GradeCode);
        }

        [Fact]
        public async Task GetByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureApiResponse<GradeRes>();
            var mappedResponse = new ApiResponseDto<GradeDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<GradeRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<GradeDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetByIdAsync("NOTEXIST");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetByIdAsync_WhenHttpThrows_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            _http.GetAsync<GradeRes>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetByIdAsync("A");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var dto         = BuildDto("A");
            var req         = new GradeReq { GradeCode = "A", Description = "Grade A", AvSalary = 50000m };
            var res         = BuildRes("A");
            var apiResponse = SuccessApiResponse(res);
            var expected    = ApiResponseDto<GradeDto>.SuccessResponse(dto);

            _mapper.Map<GradeReq>(dto).Returns(req);
            _http.PostAsync<GradeReq, GradeRes>(Arg.Is<string>(u => u.EndsWith("/Grade") || u == "api/v1/Grade"), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<GradeDto>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task CreateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = BuildDto();
            var req = new GradeReq { GradeCode = "A" };
            var apiResponse = FailureApiResponse<GradeRes>();
            var mappedResponse = new ApiResponseDto<GradeDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Failed", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<GradeReq>(dto).Returns(req);
            _http.PostAsync<GradeReq, GradeRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<GradeDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateAsync_WhenHttpThrows_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            var dto = BuildDto();
            var req = new GradeReq { GradeCode = "A" };

            _mapper.Map<GradeReq>(dto).Returns(req);
            _http.PostAsync<GradeReq, GradeRes>(Arg.Any<string>(), req)
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            var dto         = BuildDto("A");
            var req         = new GradeReq { GradeCode = "A", Description = "Grade A", AvSalary = 50000m };
            var res         = BuildRes("A");
            var apiResponse = SuccessApiResponse(res);
            var expected    = ApiResponseDto<GradeDto>.SuccessResponse(dto);

            _mapper.Map<GradeReq>(dto).Returns(req);
            _http.PutAsync<GradeReq, GradeRes>(Arg.Is<string>(u => u.Contains("Grade/A")), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<GradeDto>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.UpdateAsync("A", dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task UpdateAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = BuildDto();
            var req = new GradeReq { GradeCode = "A" };
            var apiResponse = FailureApiResponse<GradeRes>();
            var mappedResponse = new ApiResponseDto<GradeDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<GradeReq>(dto).Returns(req);
            _http.PutAsync<GradeReq, GradeRes>(Arg.Any<string>(), req).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<GradeDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateAsync("A", dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateAsync_WhenHttpThrows_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            var dto = BuildDto();
            var req = new GradeReq { GradeCode = "A" };

            _mapper.Map<GradeReq>(dto).Returns(req);
            _http.PutAsync<GradeReq, GradeRes>(Arg.Any<string>(), req)
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.UpdateAsync("A", dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task UpdateAsync_UsesOriginalCodeInUrl_ForRenameSupport()
        {
            // Arrange
            var dto         = BuildDto("B");    // renaming A → B
            var req         = new GradeReq { GradeCode = "B" };
            var res         = BuildRes("B");
            var apiResponse = SuccessApiResponse(res);
            var expected    = ApiResponseDto<GradeDto>.SuccessResponse(dto);

            _mapper.Map<GradeReq>(dto).Returns(req);
            _http.PutAsync<GradeReq, GradeRes>(Arg.Is<string>(u => u.Contains("Grade/A")), req)
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<GradeDto>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.UpdateAsync("A", dto);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).PutAsync<GradeReq, GradeRes>(
                Arg.Is<string>(u => u.Contains("Grade/A")), req);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var apiResponse = SuccessApiResponse<bool?>(true);
            var expected    = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Is<string>(u => u.Contains("Grade/A"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expected);

            // Act
            var result = await _client.DeleteAsync("A");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = FailureApiResponse<bool?>();
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteAsync("NOTEXIST");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteAsync_WhenHttpThrows_ReturnsFailureResponseWithInternalError()
        {
            // Arrange
            _http.DeleteAsync<bool?>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.DeleteAsync("A");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors!, e => e.Code == "INTERNAL_ERROR");
        }

        #endregion
    }
}
