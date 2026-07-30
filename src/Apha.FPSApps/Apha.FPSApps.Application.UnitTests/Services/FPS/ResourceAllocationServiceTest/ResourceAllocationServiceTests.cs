using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.ResourceAllocationServiceTest
{
    public class ResourceAllocationServiceTests
    {
        private const string DefaultWorkGroupGrade = "WG01";
        private const string DefaultStaffId = "PACT001";

        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsResourceAllocationApiClient _fpsResourceAllocationApiClient;
        private readonly ResourceAllocationService _sut;

        public ResourceAllocationServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsResourceAllocationApiClient = Substitute.For<IFpsResourceAllocationApiClient>();
            _fpsClient.FpsResourceAllocation.Returns(_fpsResourceAllocationApiClient);
            _sut = new ResourceAllocationService(_fpsClient);
        }

        // ── GetPagedStaffAllocationsByWorkGroupGradeAsync Tests ───────────────

        #region GetPagedStaffAllocationsByWorkGroupGradeAsync Tests

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_WithSuccessResponse_ReturnsSuccessResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<ResourceStaffAllocationDto>
            {
                new() { StaffId = "PACT001", Name = "Alpha, Staff", PlannedHours = 20.0 },
                new() { StaffId = "PACT002", Name = "Beta, Staff",  PlannedHours = 15.0 }
            };
            var expectedResponse = ApiResponseDto<List<ResourceStaffAllocationDto>>.SuccessResponse(data);

            _fpsResourceAllocationApiClient
                .GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _fpsResourceAllocationApiClient.Received(1)
                .GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query);
        }

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse =
                ApiResponseDto<List<ResourceStaffAllocationDto>>.SuccessResponse([]);

            _fpsResourceAllocationApiClient
                .GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse =
                ApiResponseDto<List<ResourceStaffAllocationDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsResourceAllocationApiClient
                .GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_DelegatesToFpsClientProperty()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse =
                ApiResponseDto<List<ResourceStaffAllocationDto>>.SuccessResponse([]);

            _fpsResourceAllocationApiClient
                .GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query)
                .Returns(expectedResponse);

            // Act
            await _sut.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query);

            // Assert
            _ = _fpsClient.Received(1).FpsResourceAllocation;
        }

        #endregion

        // ── GetPagedStaffJobDetailsByStaffIdAsync Tests ───────────────────────

        #region GetPagedStaffJobDetailsByStaffIdAsync Tests

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_WithSuccessResponse_ReturnsSuccessResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<ResourceStaffJobDetailDto>
            {
                new() { StaffId = DefaultStaffId, JobCode = "J001", PlannedHours = 10.0 }
            };
            var expectedResponse = ApiResponseDto<List<ResourceStaffJobDetailDto>>.SuccessResponse(data);

            _fpsResourceAllocationApiClient
                .GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _fpsResourceAllocationApiClient.Received(1)
                .GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query);
        }

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse =
                ApiResponseDto<List<ResourceStaffJobDetailDto>>.SuccessResponse([]);

            _fpsResourceAllocationApiClient
                .GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "Job API Error", Code = "JOB_API_ERROR" }
            };
            var expectedResponse =
                ApiResponseDto<List<ResourceStaffJobDetailDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsResourceAllocationApiClient
                .GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query)
                .Returns(expectedResponse);

            // Act
            var result = await _sut.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_DelegatesToFpsClientProperty()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse =
                ApiResponseDto<List<ResourceStaffJobDetailDto>>.SuccessResponse([]);

            _fpsResourceAllocationApiClient
                .GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query)
                .Returns(expectedResponse);

            // Act
            await _sut.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query);

            // Assert
            _ = _fpsClient.Received(1).FpsResourceAllocation;
        }

        #endregion
    }
}
