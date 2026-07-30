using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.GradeServiceTest
{
    public class GradeServiceTests
    {
        private readonly IFpsApiClient _mockFpsClient;
        private readonly IFpsGradeApiClient _mockGradeApiClient;
        private readonly GradeService _sut;

        public GradeServiceTests()
        {
            _mockFpsClient       = Substitute.For<IFpsApiClient>();
            _mockGradeApiClient  = Substitute.For<IFpsGradeApiClient>();
            _mockFpsClient.FpsGrade.Returns(_mockGradeApiClient);
            _sut = new GradeService(_mockFpsClient);
        }

        private static GradeDto BuildDto(string code = "A") =>
            new() { GradeCode = code, Description = "Grade A", AvSalary = 50000m, FpsYear = 2025 };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenFpsClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new GradeService(null!));
        }

        #endregion

        #region GetAllPagedAsync Tests

        [Fact]
        public async Task GetAllPagedAsync_ReturnsApiResponse()
        {
            // Arrange
            var query      = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos       = new List<GradeDto> { BuildDto() };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var apiResponse = ApiResponseDto<List<GradeDto>>.SuccessResponse(dtos, pagination);

            _mockGradeApiClient.GetAllPagedAsync(query).Returns(apiResponse);

            // Act
            var result = await _sut.GetAllPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.NotNull(result.Pagination);
            await _mockGradeApiClient.Received(1).GetAllPagedAsync(query);
        }

        [Fact]
        public async Task GetAllPagedAsync_PropagatesApiErrors()
        {
            // Arrange
            var query   = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<List<GradeDto>>.FailureResponse(errors, new ApiMetaDto());

            _mockGradeApiClient.GetAllPagedAsync(query).Returns(apiResponse);

            // Act
            var result = await _sut.GetAllPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetAllPagedAsync_PassesFilterAndSortParameters()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 2, PageSize = 5, SortBy = "GradeCode", Descending = true,
                Filter = "{\"GradeCode\":\"A\"}"
            };
            var apiResponse = ApiResponseDto<List<GradeDto>>.SuccessResponse([]);

            _mockGradeApiClient.GetAllPagedAsync(query).Returns(apiResponse);

            // Act
            await _sut.GetAllPagedAsync(query);

            // Assert
            await _mockGradeApiClient.Received(1).GetAllPagedAsync(
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 2 && q.PageSize == 5 &&
                    q.SortBy == "GradeCode" && q.Descending == true));
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ReturnsApiResponse()
        {
            // Arrange
            var dto         = BuildDto("A");
            var apiResponse = ApiResponseDto<GradeDto>.SuccessResponse(dto);

            _mockGradeApiClient.GetByIdAsync("A").Returns(apiResponse);

            // Act
            var result = await _sut.GetByIdAsync("A");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("A", result.Data!.GradeCode);
            await _mockGradeApiClient.Received(1).GetByIdAsync("A");
        }

        [Fact]
        public async Task GetByIdAsync_PropagatesNotFoundError()
        {
            // Arrange
            var errors      = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = ApiResponseDto<GradeDto>.FailureResponse(errors, new ApiMetaDto());

            _mockGradeApiClient.GetByIdAsync("NOTEXIST").Returns(apiResponse);

            // Act
            var result = await _sut.GetByIdAsync("NOTEXIST");

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ReturnsSuccessResponse()
        {
            // Arrange
            var dto         = BuildDto("A");
            var apiResponse = ApiResponseDto<GradeDto>.SuccessResponse(dto);

            _mockGradeApiClient.CreateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("A", result.Data!.GradeCode);
            await _mockGradeApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_PropagatesApiErrors()
        {
            // Arrange
            var dto    = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Already exists", Code = "CONFLICT" } };
            var apiResponse = ApiResponseDto<GradeDto>.FailureResponse(errors, new ApiMetaDto());

            _mockGradeApiClient.CreateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("CONFLICT", result.Errors!.First().Code);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ReturnsSuccessResponse()
        {
            // Arrange
            var dto         = BuildDto("A");
            var apiResponse = ApiResponseDto<GradeDto>.SuccessResponse(dto);

            _mockGradeApiClient.UpdateAsync("A", dto).Returns(apiResponse);

            // Act
            var result = await _sut.UpdateAsync("A", dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _mockGradeApiClient.Received(1).UpdateAsync("A", dto);
        }

        [Fact]
        public async Task UpdateAsync_ForwardsOriginalCodeForRenameSupport()
        {
            // Arrange
            var dto         = BuildDto("B");   // renaming A → B
            var apiResponse = ApiResponseDto<GradeDto>.SuccessResponse(dto);

            _mockGradeApiClient.UpdateAsync("A", dto).Returns(apiResponse);

            // Act
            await _sut.UpdateAsync("A", dto);

            // Assert
            await _mockGradeApiClient.Received(1).UpdateAsync("A", dto);
        }

        [Fact]
        public async Task UpdateAsync_PropagatesApiErrors()
        {
            // Arrange
            var dto    = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var apiResponse = ApiResponseDto<GradeDto>.FailureResponse(errors, new ApiMetaDto());

            _mockGradeApiClient.UpdateAsync("A", dto).Returns(apiResponse);

            // Act
            var result = await _sut.UpdateAsync("A", dto);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ReturnsSuccessResponse()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _mockGradeApiClient.DeleteAsync("A").Returns(apiResponse);

            // Act
            var result = await _sut.DeleteAsync("A");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _mockGradeApiClient.Received(1).DeleteAsync("A");
        }

        [Fact]
        public async Task DeleteAsync_PropagatesApiErrors()
        {
            // Arrange
            var errors      = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "DELETE_ERROR" } };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _mockGradeApiClient.DeleteAsync("A").Returns(apiResponse);

            // Act
            var result = await _sut.DeleteAsync("A");

            // Assert
            Assert.False(result.Success);
        }

        #endregion
    }
}
