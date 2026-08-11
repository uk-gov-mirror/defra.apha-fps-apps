using Apha.Common.Utilities.ExcelImport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.Common.Utilities.Storage;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.PactMonthlyTimeServiceTest
{
    public class PactMonthlyTimeServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactMonthlyTimeApiClient _pactMonthlyTimeApiClient;
        private readonly IExcelImportService _excelImportService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IPactTimeCodeValidService _timeCodeValidService;
        private readonly IMonthService _monthService;
        private readonly IS3StorageService _s3StorageService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PactMonthlyTimeService> _logger;
        private readonly PactMonthlyTimeService _service;

        public PactMonthlyTimeServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactMonthlyTimeApiClient = Substitute.For<IPactMonthlyTimeApiClient>();
            _excelImportService = Substitute.For<IExcelImportService>();
            _workGroupService = Substitute.For<IWorkGroupService>();
            _timeCodeValidService = Substitute.For<IPactTimeCodeValidService>();
            _monthService = Substitute.For<IMonthService>();
            _s3StorageService = Substitute.For<IS3StorageService>();
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _configuration = Substitute.For<IConfiguration>();
            _logger = Substitute.For<ILogger<PactMonthlyTimeService>>();

            _pactClient.PactMonthlyTime.Returns(_pactMonthlyTimeApiClient);
            _service = new PactMonthlyTimeService(
                _pactClient,
                _excelImportService,
                _workGroupService,
                _timeCodeValidService,
                _monthService,
                _s3StorageService,
                _httpContextAccessor,
                _configuration,
                _logger);
        }

        #region SearchAsync Tests

        [Fact]
        public async Task SearchAsync_WithValidQueryAndFilter_ReturnsSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto { WorkGroup = "WG1", TimeCode = "TC1" };
            var logs = new List<MonthlyTimeLogDto>
            {
                new() { SequenceNo = 1, TimeCode = "TC1", PactStaffId = "S001", WorkGroup = "WG1" },
                new() { SequenceNo = 2, TimeCode = "TC1", PactStaffId = "S002", WorkGroup = "WG1" }
            };
            var expectedResponse = ApiResponseDto<List<MonthlyTimeLogDto>>.SuccessResponse(logs);
            _pactMonthlyTimeApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactMonthlyTimeApiClient.Received(1).SearchAsync(query, filter);
        }

        [Fact]
        public async Task SearchAsync_WithNoMatchingRecords_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto { WorkGroup = "WG_NONE" };
            var expectedResponse = ApiResponseDto<List<MonthlyTimeLogDto>>.SuccessResponse(new List<MonthlyTimeLogDto>());
            _pactMonthlyTimeApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task SearchAsync_WithAllFilters_PassesFilterToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var filter = new MonthlyTimeLogFilterDto
            {
                WorkGroup = "WG1",
                TimeCode = "TC1",
                PactStaffId = "S001",
                ParentProject = "PP1",
                DateImported = new DateTime(2024, 6, 1),
                Month = 6.0,
                UserId = "USER1",
                InsertDelete = "I"
            };
            var logs = new List<MonthlyTimeLogDto>
            {
                new() { SequenceNo = 1, TimeCode = "TC1", PactStaffId = "S001", WorkGroup = "WG1", Month = 6.0, UserId = "USER1", InsertDelete = "I" }
            };
            var expectedResponse = ApiResponseDto<List<MonthlyTimeLogDto>>.SuccessResponse(logs);
            _pactMonthlyTimeApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pactMonthlyTimeApiClient.Received(1).SearchAsync(query, filter);
        }

        [Fact]
        public async Task SearchAsync_WithEmptyFilter_DelegatesToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto();
            var expectedResponse = ApiResponseDto<List<MonthlyTimeLogDto>>.SuccessResponse(new List<MonthlyTimeLogDto>());
            _pactMonthlyTimeApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            await _pactMonthlyTimeApiClient.Received(1).SearchAsync(query, filter);
        }

        [Fact]
        public async Task SearchAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto { WorkGroup = "WG1" };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<MonthlyTimeLogDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactMonthlyTimeApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task SearchAsync_ApiClientThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto();
            _pactMonthlyTimeApiClient
                .SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .ThrowsAsync(new Exception("API client error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.SearchAsync(query, filter));
        }

        #endregion

        #region Live Methods Tests

        [Fact]
        public async Task GetLiveAsync_WithValidFilters_DelegatesToApiClient()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<MonthlyTimeDto>>.SuccessResponse([]);
            _pactMonthlyTimeApiClient.GetLiveAsync(query, "WG1", "TC1", "S001", "PP1", 6).Returns(expected);

            var result = await _service.GetLiveAsync(query, "WG1", "TC1", "S001", "PP1", 6);

            Assert.Same(expected, result);
            await _pactMonthlyTimeApiClient.Received(1).GetLiveAsync(query, "WG1", "TC1", "S001", "PP1", 6);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WithValidKey_DelegatesToApiClient()
        {
            var dto = new MonthlyTimeDto { PactStaffId = "S001", TimeCode = "TC1", Month = 6, ParentProject = "PP1" };
            var expected = ApiResponseDto<MonthlyTimeDto>.SuccessResponse(dto);
            _pactMonthlyTimeApiClient.GetLiveByKeyAsync("S001", "TC1", 6, "PP1").Returns(expected);

            var result = await _service.GetLiveByKeyAsync("S001", "TC1", 6, "PP1");

            Assert.Same(expected, result);
            await _pactMonthlyTimeApiClient.Received(1).GetLiveByKeyAsync("S001", "TC1", 6, "PP1");
        }

        [Fact]
        public async Task UpdateLiveAsync_WithDto_DelegatesToApiClient()
        {
            var dto = new MonthlyTimeDto { PactStaffId = "S001", TimeCode = "TC1", Month = 6, ParentProject = "PP1", Hours = 7 };
            var expected = ApiResponseDto<MonthlyTimeDto>.SuccessResponse(dto);
            _pactMonthlyTimeApiClient.UpdateLiveAsync(dto).Returns(expected);

            var result = await _service.UpdateLiveAsync(dto);

            Assert.Same(expected, result);
            await _pactMonthlyTimeApiClient.Received(1).UpdateLiveAsync(dto);
        }

        #endregion

        #region ValidateLiveAsync Tests

        [Fact]
        public async Task ValidateLiveAsync_WithValidData_ReturnsNoErrors()
        {
            var dto = new MonthlyTimeDto
            {
                WorkGroup = "WG1",
                PactStaffId = "S001",
                TimeCode = "TC1",
                ParentProject = "PP1",
                Month = 6,
                Hours = 8
            };

            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
            [
                new WorkGroupDto { WorkGroupName = "WG1" }
            ]));
            _timeCodeValidService.GetTimeCodeValidsByWorkGroupAsync("WG1").Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
            [
                new TimeCodeValidDto { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1" }
            ]));
            _timeCodeValidService.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1").Returns(ApiResponseDto<List<string>>.SuccessResponse(
            [
                "PP1"
            ]));
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse(
            [
                new MonthDto { Monthnumber = 6, Monthname = "June" }
            ]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task ValidateLiveAsync_WithInvalidData_ReturnsExpectedErrors()
        {
            var dto = new MonthlyTimeDto
            {
                WorkGroup = "BAD-WG",
                PactStaffId = "",
                TimeCode = "BAD-TC",
                ParentProject = "BAD-PP",
                Month = 99,
                Hours = 0
            };

            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
            [
                new WorkGroupDto { WorkGroupName = "WG1" }
            ]));
            _timeCodeValidService.GetTimeCodeValidsByWorkGroupAsync("BAD-WG").Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
            [
                new TimeCodeValidDto { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1" }
            ]));
            _timeCodeValidService.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("BAD-WG", "BAD-TC").Returns(ApiResponseDto<List<string>>.SuccessResponse(
            [
                "PP1"
            ]));
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse(
            [
                new MonthDto { Monthnumber = 6, Monthname = "June" }
            ]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            var fields = result.Data!.Select(x => x.Field).ToList();
            Assert.Contains("Hours", fields);
            Assert.Contains("WorkGroup", fields);
            Assert.Contains("PactStaffId", fields);
            Assert.Contains("TimeCode", fields);
            Assert.Contains("ParentProject", fields);
            Assert.Contains("Month", fields);
        }

        #endregion
    }
}
