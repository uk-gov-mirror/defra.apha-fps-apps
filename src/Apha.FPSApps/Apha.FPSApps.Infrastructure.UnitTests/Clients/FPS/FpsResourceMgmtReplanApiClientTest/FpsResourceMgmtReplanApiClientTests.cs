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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsResourceMgmtReplanApiClientTest
{
    public class FpsResourceMgmtReplanApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsResourceMgmtReplanApiClient _client;

        public FpsResourceMgmtReplanApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsResourceMgmtReplanApiClient(_http, _mapper);
        }

        // ── GetRePlanGridAsync Tests ──────────────────────────────────────────

        #region GetRePlanGridAsync Tests

        [Fact]
        public async Task GetRePlanGridAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var workGroup = "WorkGroupA";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<ResourceMgmtReplanViewRes>
            {
                new() { StaffRowKey = "P001|WG01", WorkGroup = workGroup, WgGrade = "WG01", PlannedHours = 10.0 },
                new() { StaffRowKey = "P002|WG01", WorkGroup = workGroup, WgGrade = "WG01", PlannedHours = 8.0 }
            };
            var apiResponse = new ApiResponse<List<ResourceMgmtReplanViewRes>>
            {
                Success = true,
                Data = resList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<ResourceMgmtReplanViewDto>>.SuccessResponse(
                new List<ResourceMgmtReplanViewDto>
                {
                    new() { StaffRowKey = "P001|WG01", WorkGroup = workGroup, WgGrade = "WG01", PlannedHours = 10.0 },
                    new() { StaffRowKey = "P002|WG01", WorkGroup = workGroup, WgGrade = "WG01", PlannedHours = 8.0 }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _http.GetAsync<List<ResourceMgmtReplanViewRes>>(Arg.Is<string>(url => url.Contains($"workGroup={workGroup}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ResourceMgmtReplanViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetRePlanGridAsync(workGroup, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ResourceMgmtReplanViewRes>>(
                Arg.Is<string>(url => url.Contains($"workGroup={workGroup}")));
        }

        [Fact]
        public async Task GetRePlanGridAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var workGroup = "WorkGroupA";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<List<ResourceMgmtReplanViewRes>>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<List<ResourceMgmtReplanViewDto>>
            {
                Success = false,
                Errors = [new() { Message = "API Error", Code = "ERR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ResourceMgmtReplanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ResourceMgmtReplanViewDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetRePlanGridAsync(workGroup, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("WorkGroupA")]
        [InlineData("WG-BUDGET-001")]
        [InlineData("Test WorkGroup")]
        public async Task GetRePlanGridAsync_WithVariousWorkGroups_ConstructsUrlWithWorkGroup(string workGroup)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ResourceMgmtReplanViewRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<ResourceMgmtReplanViewDto>>.SuccessResponse([], new PaginationDto());

            _http.GetAsync<List<ResourceMgmtReplanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ResourceMgmtReplanViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetRePlanGridAsync(workGroup, query);

            // Assert
            await _http.Received(1).GetAsync<List<ResourceMgmtReplanViewRes>>(
                Arg.Is<string>(url => url.Contains("workGroup=")));
        }

        #endregion

        // ── GetStaffJobsAsync Tests ───────────────────────────────────────────

        #region GetStaffJobsAsync Tests

        [Fact]
        public async Task GetStaffJobsAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var jobCode = "JOB001";
            var wgGrade = "WG01";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<ResourceMgmtReplanStaffJobRes>
            {
                new() { StaffId = "S001", JobCode = jobCode, PlannedHours = 20.0, WgGrade = wgGrade },
                new() { StaffId = "S002", JobCode = jobCode, PlannedHours = 15.0, WgGrade = wgGrade }
            };
            var apiResponse = new ApiResponse<List<ResourceMgmtReplanStaffJobRes>>
            {
                Success = true,
                Data = resList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>.SuccessResponse(
                new List<ResourceMgmtReplanStaffJobDto>
                {
                    new() { StaffId = "S001", JobCode = jobCode, PlannedHours = 20.0, WgGrade = wgGrade },
                    new() { StaffId = "S002", JobCode = jobCode, PlannedHours = 15.0, WgGrade = wgGrade }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _http.GetAsync<List<ResourceMgmtReplanStaffJobRes>>(
                Arg.Is<string>(url => url.Contains($"jobCode={jobCode}") && url.Contains($"wgGrade={wgGrade}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetStaffJobsAsync(jobCode, wgGrade, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
        }

        [Fact]
        public async Task GetStaffJobsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<List<ResourceMgmtReplanStaffJobRes>>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>
            {
                Success = false,
                Errors = [new() { Message = "API Error", Code = "ERR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ResourceMgmtReplanStaffJobRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetStaffJobsAsync("JOB001", "WG01", query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Theory]
        [InlineData("JOB001", "WG01")]
        [InlineData("FZ2000", "WG-GRADE-A")]
        public async Task GetStaffJobsAsync_WithVariousParams_ConstructsUrlWithJobCodeAndWgGrade(string jobCode, string wgGrade)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ResourceMgmtReplanStaffJobRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>.SuccessResponse([], new PaginationDto());

            _http.GetAsync<List<ResourceMgmtReplanStaffJobRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetStaffJobsAsync(jobCode, wgGrade, query);

            // Assert
            await _http.Received(1).GetAsync<List<ResourceMgmtReplanStaffJobRes>>(
                Arg.Is<string>(url => url.Contains($"jobCode={jobCode}") && url.Contains($"wgGrade={wgGrade}")));
        }

        #endregion

        // ── GetStagedRowsAsync Tests ──────────────────────────────────────────

        #region GetStagedRowsAsync Tests

        [Fact]
        public async Task GetStagedRowsAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var jobCode = "JOB001";
            var wgGrade = "WG01";
            var resList = new List<ResourceMgmtReplanStaffJobRes>
            {
                new() { StaffId = "S001", JobCode = jobCode, PlannedHours = 20.0, WgGrade = wgGrade }
            };
            var apiResponse = new ApiResponse<List<ResourceMgmtReplanStaffJobRes>>
            {
                Success = true,
                Data = resList
            };
            var expectedDto = ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>.SuccessResponse(
                new List<ResourceMgmtReplanStaffJobDto>
                {
                    new() { StaffId = "S001", JobCode = jobCode, PlannedHours = 20.0, WgGrade = wgGrade }
                });

            _http.GetAsync<List<ResourceMgmtReplanStaffJobRes>>(
                Arg.Is<string>(url => url.Contains($"jobCode={jobCode}") && url.Contains($"wgGrade={wgGrade}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetStagedRowsAsync(jobCode, wgGrade);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetStagedRowsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not found", Code = "404" } };
            var apiResponse = new ApiResponse<List<ResourceMgmtReplanStaffJobRes>>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>
            {
                Success = false,
                Errors = [new() { Message = "Not found", Code = "404" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ResourceMgmtReplanStaffJobRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetStagedRowsAsync("JOB001", "WG01");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Theory]
        [InlineData("JOB001", "WG01")]
        [InlineData("FZ2000", "WG-GRADE-B")]
        public async Task GetStagedRowsAsync_WithVariousParams_ConstructsUrlWithJobCodeAndWgGrade(string jobCode, string wgGrade)
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ResourceMgmtReplanStaffJobRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>.SuccessResponse([]);

            _http.GetAsync<List<ResourceMgmtReplanStaffJobRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetStagedRowsAsync(jobCode, wgGrade);

            // Assert
            await _http.Received(1).GetAsync<List<ResourceMgmtReplanStaffJobRes>>(
                Arg.Is<string>(url => url.Contains($"jobCode={jobCode}") && url.Contains($"wgGrade={wgGrade}")));
        }

        #endregion

        // ── CommitReplanAsync Tests ───────────────────────────────────────────

        #region CommitReplanAsync Tests

        [Fact]
        public async Task CommitReplanAsync_WithSuccessResponse_ReturnsTrueResponse()
        {
            // Arrange
            var jobCode = "JOB001";
            var wgGrade = "WG01";
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.PostAsync<object, bool>(
                Arg.Is<string>(url => url.Contains($"jobCode={jobCode}") && url.Contains($"wgGrade={wgGrade}")),
                Arg.Any<object>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CommitReplanAsync(jobCode, wgGrade);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task CommitReplanAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Commit failed", Code = "ERR" } };
            var apiResponse = new ApiResponse<bool> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new() { Message = "Commit failed", Code = "ERR" }],
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<object, bool>(Arg.Any<string>(), Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CommitReplanAsync("JOB001", "WG01");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Theory]
        [InlineData("JOB001", "WG01")]
        [InlineData("FZ2000", "WG-GRADE-A")]
        public async Task CommitReplanAsync_WithVariousParams_ConstructsUrlWithJobCodeAndWgGrade(string jobCode, string wgGrade)
        {
            // Arrange
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.PostAsync<object, bool>(Arg.Any<string>(), Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.CommitReplanAsync(jobCode, wgGrade);

            // Assert
            await _http.Received(1).PostAsync<object, bool>(
                Arg.Is<string>(url => url.Contains($"jobCode={jobCode}") && url.Contains($"wgGrade={wgGrade}")),
                Arg.Any<object>());
        }

        #endregion
    }
}
