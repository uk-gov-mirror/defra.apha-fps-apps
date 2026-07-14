using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.ProjectAuditTrailServiceTest
{
    public class ProjectAuditTrailServiceTests
    {
        private const string TestProject = "PROJ001";

        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsProjectAuditTrailApiClient _auditTrailApiClient;
        private readonly ProjectAuditTrailService _service;

        public ProjectAuditTrailServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _auditTrailApiClient = Substitute.For<IFpsProjectAuditTrailApiClient>();
            _fpsClient.FpsProjectAuditTrail.Returns(_auditTrailApiClient);
            _service = new ProjectAuditTrailService(_fpsClient);
        }

        // ── GetProjectLogsAsync ──────────────────────────────────────────────

        #region GetProjectLogsAsync

        [Fact]
        public async Task GetProjectLogsAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var logs = new List<ProjectLogDto> { new() };
            var expected = ApiResponseDto<List<ProjectLogDto>>.SuccessResponse(logs, new PaginationDto());
            _auditTrailApiClient.GetProjectLogsAsync(query, TestProject, null, null).Returns(expected);

            // Act
            var result = await _service.GetProjectLogsAsync(query, TestProject);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _auditTrailApiClient.Received(1).GetProjectLogsAsync(query, TestProject, null, null);
        }

        [Fact]
        public async Task GetProjectLogsAsync_ApiClientReturnsEmpty_ReturnsDelegatedEmptyResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<ProjectLogDto>>.SuccessResponse(new List<ProjectLogDto>(), new PaginationDto());
            _auditTrailApiClient.GetProjectLogsAsync(query, TestProject, null, null).Returns(expected);

            // Act
            var result = await _service.GetProjectLogsAsync(query, TestProject);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProjectLogsAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Code = "ERR", Message = "API Error" } };
            var expected = ApiResponseDto<List<ProjectLogDto>>.FailureResponse(errors, new ApiMetaDto());
            _auditTrailApiClient.GetProjectLogsAsync(query, TestProject, null, null).Returns(expected);

            // Act
            var result = await _service.GetProjectLogsAsync(query, TestProject);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors!);
        }

        [Fact]
        public async Task GetProjectLogsAsync_WithDateRange_PassesDateRangeToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var fromDate = new DateOnly(2024, 1, 1);
            var toDate = new DateOnly(2024, 12, 31);
            var expected = ApiResponseDto<List<ProjectLogDto>>.SuccessResponse(new List<ProjectLogDto>());
            _auditTrailApiClient.GetProjectLogsAsync(query, TestProject, fromDate, toDate).Returns(expected);

            // Act
            var result = await _service.GetProjectLogsAsync(query, TestProject, fromDate, toDate);

            // Assert
            Assert.True(result.Success);
            await _auditTrailApiClient.Received(1).GetProjectLogsAsync(query, TestProject, fromDate, toDate);
        }

        #endregion

        // ── GetStaffJobLogsAsync ─────────────────────────────────────────────

        #region GetStaffJobLogsAsync

        [Fact]
        public async Task GetStaffJobLogsAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var logs = new List<StaffJobLogDto> { new() };
            var expected = ApiResponseDto<List<StaffJobLogDto>>.SuccessResponse(logs, new PaginationDto());
            _auditTrailApiClient.GetStaffJobLogsAsync(query, TestProject, null, null).Returns(expected);

            // Act
            var result = await _service.GetStaffJobLogsAsync(query, TestProject);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _auditTrailApiClient.Received(1).GetStaffJobLogsAsync(query, TestProject, null, null);
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Code = "ERR", Message = "Error" } };
            var expected = ApiResponseDto<List<StaffJobLogDto>>.FailureResponse(errors, new ApiMetaDto());
            _auditTrailApiClient.GetStaffJobLogsAsync(query, TestProject, null, null).Returns(expected);

            // Act
            var result = await _service.GetStaffJobLogsAsync(query, TestProject);

            // Assert
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors!);
        }

        [Fact]
        public async Task GetStaffJobLogsAsync_ApiClientReturnsEmpty_ReturnsDelegatedEmptyResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<StaffJobLogDto>>.SuccessResponse(new List<StaffJobLogDto>());
            _auditTrailApiClient.GetStaffJobLogsAsync(query, TestProject, null, null).Returns(expected);

            // Act
            var result = await _service.GetStaffJobLogsAsync(query, TestProject);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        #endregion

        // ── GetTestRequirementLogsAsync ──────────────────────────────────────

        #region GetTestRequirementLogsAsync

        [Fact]
        public async Task GetTestRequirementLogsAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var logs = new List<TestRequirementLogDto> { new() };
            var expected = ApiResponseDto<List<TestRequirementLogDto>>.SuccessResponse(logs, new PaginationDto());
            _auditTrailApiClient.GetTestRequirementLogsAsync(query, TestProject, null, null).Returns(expected);

            // Act
            var result = await _service.GetTestRequirementLogsAsync(query, TestProject);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _auditTrailApiClient.Received(1).GetTestRequirementLogsAsync(query, TestProject, null, null);
        }

        [Fact]
        public async Task GetTestRequirementLogsAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestRequirementLogDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR" } }, new ApiMetaDto());
            _auditTrailApiClient.GetTestRequirementLogsAsync(query, TestProject, null, null).Returns(expected);

            // Act
            var result = await _service.GetTestRequirementLogsAsync(query, TestProject);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetTestRequirementLogsAsync_ApiClientReturnsEmpty_ReturnsDelegatedEmptyResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestRequirementLogDto>>.SuccessResponse(new List<TestRequirementLogDto>());
            _auditTrailApiClient.GetTestRequirementLogsAsync(query, TestProject, null, null).Returns(expected);

            // Act
            var result = await _service.GetTestRequirementLogsAsync(query, TestProject);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        #endregion

        // ── GetAnimalRequestLogsAsync ────────────────────────────────────────

        #region GetAnimalRequestLogsAsync

        [Fact]
        public async Task GetAnimalRequestLogsAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var logs = new List<AnimalRequestLogDto> { new() };
            var expected = ApiResponseDto<List<AnimalRequestLogDto>>.SuccessResponse(logs, new PaginationDto());
            _auditTrailApiClient.GetAnimalRequestLogsAsync(query, TestProject, null, null).Returns(expected);

            // Act
            var result = await _service.GetAnimalRequestLogsAsync(query, TestProject);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _auditTrailApiClient.Received(1).GetAnimalRequestLogsAsync(query, TestProject, null, null);
        }

        [Fact]
        public async Task GetAnimalRequestLogsAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<AnimalRequestLogDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR" } }, new ApiMetaDto());
            _auditTrailApiClient.GetAnimalRequestLogsAsync(query, TestProject, null, null).Returns(expected);

            // Act
            var result = await _service.GetAnimalRequestLogsAsync(query, TestProject);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAnimalRequestLogsAsync_ApiClientReturnsEmpty_ReturnsDelegatedEmptyResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<AnimalRequestLogDto>>.SuccessResponse(new List<AnimalRequestLogDto>());
            _auditTrailApiClient.GetAnimalRequestLogsAsync(query, TestProject, null, null).Returns(expected);

            // Act
            var result = await _service.GetAnimalRequestLogsAsync(query, TestProject);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        #endregion

        // ── GetAdditionalCostLogsAsync ───────────────────────────────────────

        #region GetAdditionalCostLogsAsync

        [Fact]
        public async Task GetAdditionalCostLogsAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var logs = new List<AdditionalCostLogDto> { new() };
            var expected = ApiResponseDto<List<AdditionalCostLogDto>>.SuccessResponse(logs, new PaginationDto());
            _auditTrailApiClient.GetAdditionalCostLogsAsync(query, TestProject, null, null).Returns(expected);

            // Act
            var result = await _service.GetAdditionalCostLogsAsync(query, TestProject);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _auditTrailApiClient.Received(1).GetAdditionalCostLogsAsync(query, TestProject, null, null);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<AdditionalCostLogDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR" } }, new ApiMetaDto());
            _auditTrailApiClient.GetAdditionalCostLogsAsync(query, TestProject, null, null).Returns(expected);

            // Act
            var result = await _service.GetAdditionalCostLogsAsync(query, TestProject);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAdditionalCostLogsAsync_ApiClientReturnsEmpty_ReturnsDelegatedEmptyResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<AdditionalCostLogDto>>.SuccessResponse(new List<AdditionalCostLogDto>());
            _auditTrailApiClient.GetAdditionalCostLogsAsync(query, TestProject, null, null).Returns(expected);

            // Act
            var result = await _service.GetAdditionalCostLogsAsync(query, TestProject);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        #endregion
    }
}
