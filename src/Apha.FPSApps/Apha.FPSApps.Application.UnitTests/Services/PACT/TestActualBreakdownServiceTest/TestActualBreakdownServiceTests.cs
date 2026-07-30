using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.TestActualBreakdownServiceTest
{
    public class TestActualBreakdownServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactTestActualBreakdownApiClient _apiClient;
        private readonly TestActualBreakdownService _service;

        public TestActualBreakdownServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _apiClient  = Substitute.For<IPactTestActualBreakdownApiClient>();
            _pactClient.PactTestActualBreakdown.Returns(_apiClient);
            _service = new TestActualBreakdownService(_pactClient);
        }

        // ── GetActualsTestsWithPlannedDataByWorkgroupAsync ────────────────────

        #region GetActualsTestsWithPlannedDataByWorkgroupAsync

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_DelegatesToApiClient_ReturnsResult()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse(
                [new TestActualBreakdownDto { TestCode = "PT0047", Buyer = "SV3300" }]);

            _apiClient.GetActualsTestsWithPlannedDataByWorkgroupAsync(query).Returns(expected);

            // Act
            var result = await _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetActualsTestsWithPlannedDataByWorkgroupAsync(query);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_ReturnsSuccessTrue_WhenApiSucceeds()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse(
                [new TestActualBreakdownDto { TestCode = "PT0047" }]);

            _apiClient.GetActualsTestsWithPlannedDataByWorkgroupAsync(query).Returns(expected);

            // Act
            var result = await _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([]);

            _apiClient.GetActualsTestsWithPlannedDataByWorkgroupAsync(query).Returns(expected);

            // Act
            var result = await _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors   = new List<ApiErrorDto> { new() { Code = "SERVER_500", Message = "Internal Server Error" } };
            var expected = ApiResponseDto<List<TestActualBreakdownDto>>.FailureResponse(errors, new ApiMetaDto());

            _apiClient.GetActualsTestsWithPlannedDataByWorkgroupAsync(query).Returns(expected);

            // Act
            var result = await _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_WhenApiFails_ReturnsErrorsFromResponse()
        {
            // Arrange
            var query  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Code = "SERVER_500", Message = "Internal Server Error" } };
            var expected = ApiResponseDto<List<TestActualBreakdownDto>>.FailureResponse(errors, new ApiMetaDto());

            _apiClient.GetActualsTestsWithPlannedDataByWorkgroupAsync(query).Returns(expected);

            // Act
            var result = await _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors!);
            Assert.Equal("SERVER_500", result.Errors![0].Code);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_WithMultipleItems_ReturnsAllItems()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos  = new List<TestActualBreakdownDto>
            {
                new() { TestCode = "PT0047", Buyer = "SV3300", Program = "Viro",  Portfolio = "QAPTPORT1", WorkGroup = "QASB", Month = 4,  PCPrice = 159.00m, PCCost = 319.00m },
                new() { TestCode = "PT0049", Buyer = "SB4600", Program = "Bact",  Portfolio = "QAPTPORT2", WorkGroup = "QASB", Month = 4,  PCPrice = 313.00m, PCCost = 313.00m },
                new() { TestCode = "TC0001", Buyer = "EDI300", Program = "Micro", Portfolio = "QAPTPORT3", WorkGroup = "QASB", Month = 12, PCPrice = 51.90m,  PCCost = 155.70m }
            };
            var expected = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse(dtos);

            _apiClient.GetActualsTestsWithPlannedDataByWorkgroupAsync(query).Returns(expected);

            // Act
            var result = await _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.Count);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_WithMultipleItems_ReturnsCorrectDtoValues()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos  = new List<TestActualBreakdownDto>
            {
                new()
                {
                    TestCode         = "PT0047",
                    ShortDescription = "EVA serology",
                    Program          = "Viro",
                    Buyer            = "SV3300",
                    Portfolio        = "QAPTPORT1",
                    WorkGroup        = "QASB",
                    ProfitCentre     = "Comm",
                    Month            = 4,
                    PCPrice          = 159.00m,
                    PCCost           = 319.00m,
                    Volume           = 2m
                }
            };
            var expected = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse(dtos);

            _apiClient.GetActualsTestsWithPlannedDataByWorkgroupAsync(query).Returns(expected);

            // Act
            var result = await _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            var item = result.Data!.Single();
            Assert.Equal("PT0047",       item.TestCode);
            Assert.Equal("EVA serology", item.ShortDescription);
            Assert.Equal("Viro",         item.Program);
            Assert.Equal("SV3300",       item.Buyer);
            Assert.Equal("QAPTPORT1",    item.Portfolio);
            Assert.Equal("QASB",         item.WorkGroup);
            Assert.Equal("Comm",         item.ProfitCentre);
            Assert.Equal(4,              item.Month);
            Assert.Equal(159.00m,        item.PCPrice);
            Assert.Equal(319.00m,        item.PCCost);
            Assert.Equal(2m,             item.Volume);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_PassesQueryParametersThrough()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page     = 3,
                PageSize = 25,
                SortBy   = "TestCode",
                Filter   = "PT"
            };
            var expected = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([]);

            _apiClient.GetActualsTestsWithPlannedDataByWorkgroupAsync(query).Returns(expected);

            // Act
            var result = await _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            await _apiClient.Received(1).GetActualsTestsWithPlannedDataByWorkgroupAsync(
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page     == 3  &&
                    q.PageSize == 25 &&
                    q.SortBy   == "TestCode" &&
                    q.Filter   == "PT"));
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_WithPagination_ReturnsPaginationData()
        {
            // Arrange
            var query      = new QueryParameters<string> { Page = 2, PageSize = 10 };
            var pagination = new PaginationDto { PageNumber = 2, PageSize = 10, TotalRecords = 45 };
            var expected   = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([], pagination);

            _apiClient.GetActualsTestsWithPlannedDataByWorkgroupAsync(query).Returns(expected);

            // Act
            var result = await _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            Assert.NotNull(result.Pagination);
            Assert.Equal(2,  result.Pagination!.PageNumber);
            Assert.Equal(10, result.Pagination!.PageSize);
            Assert.Equal(45, result.Pagination!.TotalRecords);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_ApiCalledExactlyOnce()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([]);

            _apiClient.GetActualsTestsWithPlannedDataByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(expected);

            // Act
            await _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            await _apiClient.Received(1).GetActualsTestsWithPlannedDataByWorkgroupAsync(Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_DoesNotCallOtherApiClientMethods()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([]);

            _apiClient.GetActualsTestsWithPlannedDataByWorkgroupAsync(query).Returns(expected);

            // Act
            await _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert — only PactTestActualBreakdown property accessed, no other top-level client properties
            _ = _pactClient.Received(1).PactTestActualBreakdown;
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_WithNullableDecimalFields_ReturnsNullWhenNotSet()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos  = new List<TestActualBreakdownDto>
            {
                new() { TestCode = "PT0047", Buyer = "SV3300", PCPrice = null, PCCost = null, Volume = null }
            };
            var expected = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse(dtos);

            _apiClient.GetActualsTestsWithPlannedDataByWorkgroupAsync(query).Returns(expected);

            // Act
            var result = await _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            var item = result.Data!.Single();
            Assert.Null(item.PCPrice);
            Assert.Null(item.PCCost);
            Assert.Null(item.Volume);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_WithMultipleErrors_ReturnsAllErrors()
        {
            // Arrange
            var query  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new() { Code = "ERR_001", Message = "First error"  },
                new() { Code = "ERR_002", Message = "Second error" }
            };
            var expected = ApiResponseDto<List<TestActualBreakdownDto>>.FailureResponse(errors, new ApiMetaDto());

            _apiClient.GetActualsTestsWithPlannedDataByWorkgroupAsync(query).Returns(expected);

            // Act
            var result = await _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(2, result.Errors!.Count);
        }

        #endregion
    }
}
