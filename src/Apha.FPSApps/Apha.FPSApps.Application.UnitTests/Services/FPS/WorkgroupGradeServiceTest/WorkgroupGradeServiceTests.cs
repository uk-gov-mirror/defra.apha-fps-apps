using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.WorkGroupGradeServiceTest
{
    public class WorkGroupGradeServiceTests
    {
        private const string DefaultPcGrade = "G001";
        private const string DefaultWgGrade = "WG01";

        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsWorkGroupGradeApiClient _fpsWgGradeApiClient;
        private readonly WorkGroupGradeService _sut;

        public WorkGroupGradeServiceTests()
        {
            _fpsClient           = Substitute.For<IFpsApiClient>();
            _fpsWgGradeApiClient = Substitute.For<IFpsWorkGroupGradeApiClient>();
            _fpsClient.FpsWorkGroupGrade.Returns(_fpsWgGradeApiClient);
            _sut = new WorkGroupGradeService(_fpsClient);
        }

        #region GetWorkGroupGradeAsync Tests

        [Fact]
        public async Task GetWorkGroupGradeAsync_WithSuccessResponse_ReturnsGradeList()
        {
            // Arrange
            var grades = new List<WorkgroupGradeDto>
            {
                new() { WgGrade = DefaultWgGrade, ProfitCentreGrade = DefaultPcGrade }
            };
            var expectedResponse = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(grades);

            _fpsWgGradeApiClient.GetWorkGroupGradeAsync(Arg.Any<QueryParameters<string>>(), DefaultPcGrade)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetWorkGroupGradeAsync(DefaultPcGrade);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsWgGradeApiClient.Received(1)
                .GetWorkGroupGradeAsync(Arg.Any<QueryParameters<string>>(), DefaultPcGrade);
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(new List<WorkgroupGradeDto>());

            _fpsWgGradeApiClient.GetWorkGroupGradeAsync(Arg.Any<QueryParameters<string>>(), DefaultPcGrade)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetWorkGroupGradeAsync(DefaultPcGrade);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsWgGradeApiClient.GetWorkGroupGradeAsync(Arg.Any<QueryParameters<string>>(), DefaultPcGrade)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetWorkGroupGradeAsync(DefaultPcGrade);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region DeleteWorkGroupGradeAsync Tests

        [Fact]
        public async Task DeleteWorkGroupGradeAsync_WithSuccessResponse_ReturnsSuccess()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _fpsWgGradeApiClient.DeleteWorkGroupGradeAsync(DefaultWgGrade).Returns(expectedResponse);

            // Act
            var result = await _sut.DeleteWorkGroupGradeAsync(DefaultWgGrade);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsWgGradeApiClient.Received(1).DeleteWorkGroupGradeAsync(DefaultWgGrade);
        }

        [Fact]
        public async Task DeleteWorkGroupGradeAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _fpsWgGradeApiClient.DeleteWorkGroupGradeAsync(DefaultWgGrade).Returns(expectedResponse);

            // Act
            var result = await _sut.DeleteWorkGroupGradeAsync(DefaultWgGrade);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetAllWorkgroupGradesPagedAsync Tests

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_WithSuccessResponse_ReturnsGradeList()
        {
            // Arrange
            var grades = new List<WorkgroupGradeDto> { new() { WgGrade = DefaultWgGrade } };
            var expectedResponse = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(grades);
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _fpsWgGradeApiClient.GetAllWorkgroupGradesPagedAsync(query).Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllWorkgroupGradesPagedAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsWgGradeApiClient.Received(1).GetAllWorkgroupGradesPagedAsync(query);
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            var expectedResponse = ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(errors, new ApiMetaDto());
            var query = new QueryParameters<string>();

            _fpsWgGradeApiClient.GetAllWorkgroupGradesPagedAsync(query).Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllWorkgroupGradesPagedAsync(query);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetByWgGradeAsync Tests

        [Fact]
        public async Task GetByWgGradeAsync_WithSuccessResponse_ReturnsGrade()
        {
            // Arrange
            var grade = new WorkgroupGradeDto { WgGrade = DefaultWgGrade };
            var expectedResponse = ApiResponseDto<WorkgroupGradeDto>.SuccessResponse(grade);

            _fpsWgGradeApiClient.GetByWgGradeAsync(DefaultWgGrade).Returns(expectedResponse);

            // Act
            var result = await _sut.GetByWgGradeAsync(DefaultWgGrade);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(DefaultWgGrade, result.Data!.WgGrade);
            await _fpsWgGradeApiClient.Received(1).GetByWgGradeAsync(DefaultWgGrade);
        }

        [Fact]
        public async Task GetByWgGradeAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<WorkgroupGradeDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsWgGradeApiClient.GetByWgGradeAsync(DefaultWgGrade).Returns(expectedResponse);

            // Act
            var result = await _sut.GetByWgGradeAsync(DefaultWgGrade);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidDto_ReturnsCreatedGrade()
        {
            // Arrange
            var dto = new WorkgroupGradeDto { WgGrade = DefaultWgGrade };
            var expectedResponse = ApiResponseDto<WorkgroupGradeDto>.SuccessResponse(dto);

            _fpsWgGradeApiClient.CreateAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(DefaultWgGrade, result.Data!.WgGrade);
            await _fpsWgGradeApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new WorkgroupGradeDto { WgGrade = DefaultWgGrade };
            var errors = new List<ApiErrorDto> { new() { Message = "Conflict", Code = "CONFLICT" } };
            var expectedResponse = ApiResponseDto<WorkgroupGradeDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsWgGradeApiClient.CreateAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidDto_ReturnsUpdatedGrade()
        {
            // Arrange
            var dto = new WorkgroupGradeDto { WgGrade = DefaultWgGrade };
            var expectedResponse = ApiResponseDto<WorkgroupGradeDto>.SuccessResponse(dto);

            _fpsWgGradeApiClient.UpdateAsync(DefaultWgGrade, dto).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateAsync(DefaultWgGrade, dto);

            // Assert
            Assert.True(result.Success);
            await _fpsWgGradeApiClient.Received(1).UpdateAsync(DefaultWgGrade, dto);
        }

        [Fact]
        public async Task UpdateAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new WorkgroupGradeDto { WgGrade = DefaultWgGrade };
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<WorkgroupGradeDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsWgGradeApiClient.UpdateAsync(DefaultWgGrade, dto).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateAsync(DefaultWgGrade, dto);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithSuccessResponse_ReturnsSuccess()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _fpsWgGradeApiClient.DeleteAsync(DefaultWgGrade).Returns(expectedResponse);

            // Act
            var result = await _sut.DeleteAsync(DefaultWgGrade);

            // Assert
            Assert.True(result.Success);
            await _fpsWgGradeApiClient.Received(1).DeleteAsync(DefaultWgGrade);
        }

        [Fact]
        public async Task DeleteAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _fpsWgGradeApiClient.DeleteAsync(DefaultWgGrade).Returns(expectedResponse);

            // Act
            var result = await _sut.DeleteAsync(DefaultWgGrade);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetAllGradeCodesAsync Tests

        [Fact]
        public async Task GetAllGradeCodesAsync_WithSuccessResponse_ReturnsGradeCodes()
        {
            // Arrange
            var codes = new List<string> { "A", "B", "C" };
            var expectedResponse = ApiResponseDto<List<string>>.SuccessResponse(codes);

            _fpsWgGradeApiClient.GetAllGradeCodesAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllGradeCodesAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.Count);
            await _fpsWgGradeApiClient.Received(1).GetAllGradeCodesAsync();
        }

        [Fact]
        public async Task GetAllGradeCodesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            var expectedResponse = ApiResponseDto<List<string>>.FailureResponse(errors, new ApiMetaDto());

            _fpsWgGradeApiClient.GetAllGradeCodesAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAllGradeCodesAsync();

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetWorkgroupGradesByWorkGroupAsync Tests

        private const string DefaultWorkGroup = "WG-ADMIN";

        [Fact]
        public async Task GetWorkgroupGradesByWorkGroupAsync_WithSuccessResponse_ReturnsGradeList()
        {
            // Arrange
            var grades = new List<WorkgroupGradeDto>
            {
                new() { WgGrade = DefaultWgGrade, Workgroup = DefaultWorkGroup, GradeCode = "G1" },
                new() { WgGrade = "WG02",         Workgroup = DefaultWorkGroup, GradeCode = "G2" }
            };
            var expectedResponse = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(grades);

            _fpsWgGradeApiClient.GetWorkgroupGradesByWorkGroupAsync(DefaultWorkGroup)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetWorkgroupGradesByWorkGroupAsync(DefaultWorkGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            Assert.All(result.Data, g => Assert.Equal(DefaultWorkGroup, g.Workgroup));
            await _fpsWgGradeApiClient.Received(1).GetWorkgroupGradesByWorkGroupAsync(DefaultWorkGroup);
        }

        [Fact]
        public async Task GetWorkgroupGradesByWorkGroupAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(new List<WorkgroupGradeDto>());

            _fpsWgGradeApiClient.GetWorkgroupGradesByWorkGroupAsync(DefaultWorkGroup)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetWorkgroupGradesByWorkGroupAsync(DefaultWorkGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _fpsWgGradeApiClient.Received(1).GetWorkgroupGradesByWorkGroupAsync(DefaultWorkGroup);
        }

        [Fact]
        public async Task GetWorkgroupGradesByWorkGroupAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Work group not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsWgGradeApiClient.GetWorkgroupGradesByWorkGroupAsync(DefaultWorkGroup)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetWorkgroupGradesByWorkGroupAsync(DefaultWorkGroup);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("NOT_FOUND", result.Errors!.First().Code);
            await _fpsWgGradeApiClient.Received(1).GetWorkgroupGradesByWorkGroupAsync(DefaultWorkGroup);
        }

        [Fact]
        public async Task GetWorkgroupGradesByWorkGroupAsync_DelegatesToApiClient_WithExactWorkGroup()
        {
            // Arrange — verify the service passes the workGroup string through unchanged
            var expectedResponse = ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(new List<WorkgroupGradeDto>());

            _fpsWgGradeApiClient.GetWorkgroupGradesByWorkGroupAsync(DefaultWorkGroup)
                .Returns(expectedResponse);

            // Act
            await _sut.GetWorkgroupGradesByWorkGroupAsync(DefaultWorkGroup);

            // Assert — only this specific work group string was forwarded; any other value was not called
            await _fpsWgGradeApiClient.Received(1).GetWorkgroupGradesByWorkGroupAsync(DefaultWorkGroup);
            await _fpsWgGradeApiClient.DidNotReceive().GetWorkgroupGradesByWorkGroupAsync(
                Arg.Is<string>(s => s != DefaultWorkGroup));
        }

        #endregion
    }
}
