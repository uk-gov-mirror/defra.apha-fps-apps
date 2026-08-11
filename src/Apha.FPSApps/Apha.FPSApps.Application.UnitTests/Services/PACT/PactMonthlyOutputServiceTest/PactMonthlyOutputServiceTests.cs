using Apha.Common.Utilities.ExcelImport;
using Apha.Common.Utilities.Storage;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.PactMonthlyOutputServiceTest
{
    public class PactMonthlyOutputServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactMonthlyOutputApiClient _pactMonthlyOutputApiClient;
        private readonly IExcelImportService _excelImportService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IMonthService _monthService;
        private readonly IS3StorageService _s3StorageService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PactMonthlyOutputService> _logger;
        private readonly PactMonthlyOutputService _service;

        public PactMonthlyOutputServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactMonthlyOutputApiClient = Substitute.For<IPactMonthlyOutputApiClient>();
            _excelImportService = Substitute.For<IExcelImportService>();
            _workGroupService = Substitute.For<IWorkGroupService>();
            _monthService = Substitute.For<IMonthService>();
            _s3StorageService = Substitute.For<IS3StorageService>();
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _configuration = Substitute.For<IConfiguration>();
            _logger = Substitute.For<ILogger<PactMonthlyOutputService>>();

            _pactClient.PactMonthlyOutput.Returns(_pactMonthlyOutputApiClient);
            _service = new PactMonthlyOutputService(
                _pactClient,
                _excelImportService,
                _workGroupService,
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
            var filter = new MonthlyOutputLogFilterDto { WorkGroup = "WG1", TestCode = "TC1" };
            var logs = new List<MonthlyOutputLogDto>
            {
                new() { SequenceNo = 1, TestCode = "TC1", Buyer = "BuyerA", WorkGroup = "WG1" },
                new() { SequenceNo = 2, TestCode = "TC1", Buyer = "BuyerB", WorkGroup = "WG1" }
            };
            var expectedResponse = ApiResponseDto<List<MonthlyOutputLogDto>>.SuccessResponse(logs);
            _pactMonthlyOutputApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactMonthlyOutputApiClient.Received(1).SearchAsync(query, filter);
        }

        [Fact]
        public async Task SearchAsync_WithNoMatchingRecords_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyOutputLogFilterDto { WorkGroup = "WG_NONE" };
            var expectedResponse = ApiResponseDto<List<MonthlyOutputLogDto>>.SuccessResponse(new List<MonthlyOutputLogDto>());
            _pactMonthlyOutputApiClient.SearchAsync(query, filter).Returns(expectedResponse);

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
            var filter = new MonthlyOutputLogFilterDto
            {
                WorkGroup = "WG1",
                TestCode = "TC1",
                Buyer = "BuyerA",
                DateImported = new DateTime(2024, 1, 15),
                Month = 1.0,
                UserId = "user1",
                InsertDelete = "I"
            };
            var logs = new List<MonthlyOutputLogDto>
            {
                new() { SequenceNo = 1, TestCode = "TC1", Buyer = "BuyerA", WorkGroup = "WG1", Month = 1.0, UserId = "user1", InsertDelete = "I" }
            };
            var expectedResponse = ApiResponseDto<List<MonthlyOutputLogDto>>.SuccessResponse(logs);
            _pactMonthlyOutputApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pactMonthlyOutputApiClient.Received(1).SearchAsync(query, filter);
        }

        [Fact]
        public async Task SearchAsync_WithEmptyFilter_DelegatesToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyOutputLogFilterDto();
            var expectedResponse = ApiResponseDto<List<MonthlyOutputLogDto>>.SuccessResponse(new List<MonthlyOutputLogDto>());
            _pactMonthlyOutputApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            await _pactMonthlyOutputApiClient.Received(1).SearchAsync(query, filter);
        }

        [Fact]
        public async Task SearchAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyOutputLogFilterDto { WorkGroup = "WG1" };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<MonthlyOutputLogDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactMonthlyOutputApiClient.SearchAsync(query, filter).Returns(expectedResponse);

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
            var filter = new MonthlyOutputLogFilterDto();
            _pactMonthlyOutputApiClient
                .SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyOutputLogFilterDto>())
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
            var expected = ApiResponseDto<List<PactMonthlyOutputDto>>.SuccessResponse([]);
            _pactMonthlyOutputApiClient.GetLiveAsync(query, "WG1", "TC1", "Buyer1", 6).Returns(expected);

            var result = await _service.GetLiveAsync(query, "WG1", "TC1", "Buyer1", 6);

            Assert.Same(expected, result);
            await _pactMonthlyOutputApiClient.Received(1).GetLiveAsync(query, "WG1", "TC1", "Buyer1", 6);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WithValidKey_DelegatesToApiClient()
        {
            var dto = new PactMonthlyOutputDto { TestCode = "TC1", Buyer = "Buyer1", Month = 6, WorkGroup = "WG1" };
            var expected = ApiResponseDto<PactMonthlyOutputDto>.SuccessResponse(dto);
            _pactMonthlyOutputApiClient.GetLiveByKeyAsync("TC1", "Buyer1", 6, "WG1").Returns(expected);

            var result = await _service.GetLiveByKeyAsync("TC1", "Buyer1", 6, "WG1");

            Assert.Same(expected, result);
            await _pactMonthlyOutputApiClient.Received(1).GetLiveByKeyAsync("TC1", "Buyer1", 6, "WG1");
        }

        [Fact]
        public async Task UpdateLiveAsync_WithDto_DelegatesToApiClient()
        {
            var dto = new PactMonthlyOutputDto { TestCode = "TC1", Buyer = "Buyer1", Month = 6, WorkGroup = "WG1", Volume = 10 };
            var expected = ApiResponseDto<PactMonthlyOutputDto>.SuccessResponse(dto);
            _pactMonthlyOutputApiClient.UpdateLiveAsync(dto).Returns(expected);

            var result = await _service.UpdateLiveAsync(dto);

            Assert.Same(expected, result);
            await _pactMonthlyOutputApiClient.Received(1).UpdateLiveAsync(dto);
        }

        #endregion

        #region ValidateLiveAsync Tests

        [Fact]
        public async Task ValidateLiveAsync_WithValidData_ReturnsNoErrors()
        {
            var dto = new PactMonthlyOutputDto
            {
                WorkGroup = "WG1",
                TestCode = "TC1",
                Buyer = "Buyer1",
                Month = 6,
                Volume = 100
            };

            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
            [
                new WorkGroupDto { WorkGroupName = "WG1" }
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
            var dto = new PactMonthlyOutputDto
            {
                WorkGroup = "BAD-WG",
                TestCode = "",
                Buyer = "",
                Month = 99,
                Volume = 0
            };

            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
            [
                new WorkGroupDto { WorkGroupName = "WG1" }
            ]));
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse(
            [
                new MonthDto { Monthnumber = 6, Monthname = "June" }
            ]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            var fields = result.Data!.Select(x => x.Field).ToList();
            Assert.Contains("Volume", fields);
            Assert.Contains("WorkGroup", fields);
            Assert.Contains("TestCode", fields);
            Assert.Contains("Buyer", fields);
            Assert.Contains("Month", fields);
        }

        #endregion
    }
}
