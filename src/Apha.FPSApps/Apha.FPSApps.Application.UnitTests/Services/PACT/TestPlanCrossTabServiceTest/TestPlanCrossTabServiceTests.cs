using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.TestPlanCrossTabServiceTest
{
    public class TestPlanCrossTabServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactTestPlanCrossTabApiClient _apiClient;
        private readonly TestPlanCrossTabService _service;

        public TestPlanCrossTabServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _apiClient  = Substitute.For<IPactTestPlanCrossTabApiClient>();
            _pactClient.PactTestPlanCrossTab.Returns(_apiClient);
            _service = new TestPlanCrossTabService(_pactClient);
        }

        // ── GetPagedTestPlanCrossTabAsync ─────────────────────────────────────

        #region GetPagedTestPlanCrossTabAsync

        [Fact]
        public async Task GetPagedTestPlanCrossTabAsync_DelegatesToApiClient_ReturnsResult()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var expected = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(new TestPlanCostBreakdownDto
            {
                Columns    = ["testcode", "Jan"],
                Rows       = [new Dictionary<string, string?> { ["testcode"] = "PT0047", ["Jan"] = "5" }],
                TotalCount = 1,
                Page       = 1,
                PageSize   = 20
            });

            _apiClient.GetPagedTestPlanCrossTabAsync(query).Returns(expected);

            // Act
            var result = await _service.GetPagedTestPlanCrossTabAsync(query);

            // Assert
            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetPagedTestPlanCrossTabAsync(query);
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTabAsync_ReturnsSuccessTrue_WhenApiSucceeds()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var expected = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(new TestPlanCostBreakdownDto
            {
                Columns = ["testcode", "Jan", "Feb"],
                Rows    = [],
                Page    = 1,
                PageSize = 20
            });

            _apiClient.GetPagedTestPlanCrossTabAsync(query).Returns(expected);

            // Act
            var result = await _service.GetPagedTestPlanCrossTabAsync(query);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTabAsync_WithEmptyResult_ReturnsSuccessWithEmptyCollections()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var expected = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(new TestPlanCostBreakdownDto
            {
                Columns    = [],
                Rows       = [],
                TotalCount = 0,
                Page       = 1,
                PageSize   = 20
            });

            _apiClient.GetPagedTestPlanCrossTabAsync(query).Returns(expected);

            // Act
            var result = await _service.GetPagedTestPlanCrossTabAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!.Columns);
            Assert.Empty(result.Data!.Rows);
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTabAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var errors   = new List<ApiErrorDto> { new() { Code = "SERVER_500", Message = "Internal Server Error" } };
            var expected = ApiResponseDto<TestPlanCostBreakdownDto>.FailureResponse(errors, new ApiMetaDto());

            _apiClient.GetPagedTestPlanCrossTabAsync(query).Returns(expected);

            // Act
            var result = await _service.GetPagedTestPlanCrossTabAsync(query);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTabAsync_WhenApiFails_ReturnsErrorsFromResponse()
        {
            // Arrange
            var query  = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var errors = new List<ApiErrorDto> { new() { Code = "SERVER_500", Message = "Internal Server Error" } };
            var expected = ApiResponseDto<TestPlanCostBreakdownDto>.FailureResponse(errors, new ApiMetaDto());

            _apiClient.GetPagedTestPlanCrossTabAsync(query).Returns(expected);

            // Act
            var result = await _service.GetPagedTestPlanCrossTabAsync(query);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors!);
            Assert.Equal("SERVER_500", result.Errors![0].Code);
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTabAsync_WithMultipleRows_ReturnsAllRows()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var rows  = new List<Dictionary<string, string?>>
            {
                new() { ["testcode"] = "PT0047", ["Jan"] = "5",  ["Feb"] = "3"  },
                new() { ["testcode"] = "PT0049", ["Jan"] = "2",  ["Feb"] = "7"  },
                new() { ["testcode"] = "TC0001", ["Jan"] = "11", ["Feb"] = "9"  }
            };
            var expected = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(new TestPlanCostBreakdownDto
            {
                Columns    = ["testcode", "Jan", "Feb"],
                Rows       = rows,
                TotalCount = 3,
                Page       = 1,
                PageSize   = 20
            });

            _apiClient.GetPagedTestPlanCrossTabAsync(query).Returns(expected);

            // Act
            var result = await _service.GetPagedTestPlanCrossTabAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.Rows.Count);
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTabAsync_ReturnsPaginationValues_FromApiClient()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 2, PageSize = 10 };
            var expected = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(new TestPlanCostBreakdownDto
            {
                Columns    = ["testcode"],
                Rows       = [],
                TotalCount = 250,
                Page       = 2,
                PageSize   = 10
            });

            _apiClient.GetPagedTestPlanCrossTabAsync(query).Returns(expected);

            // Act
            var result = await _service.GetPagedTestPlanCrossTabAsync(query);

            // Assert
            Assert.Equal(250, result.Data!.TotalCount);
            Assert.Equal(2,   result.Data.Page);
            Assert.Equal(10,  result.Data.PageSize);
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTabAsync_WithMultipleErrors_ReturnsAllErrors()
        {
            // Arrange
            var query  = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var errors = new List<ApiErrorDto>
            {
                new() { Code = "ERR_001", Message = "First error"  },
                new() { Code = "ERR_002", Message = "Second error" }
            };
            var expected = ApiResponseDto<TestPlanCostBreakdownDto>.FailureResponse(errors, new ApiMetaDto());

            _apiClient.GetPagedTestPlanCrossTabAsync(query).Returns(expected);

            // Act
            var result = await _service.GetPagedTestPlanCrossTabAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(2, result.Errors!.Count);
            Assert.Equal("ERR_001", result.Errors![0].Code);
            Assert.Equal("ERR_002", result.Errors![1].Code);
        }

        #endregion
    }
}
