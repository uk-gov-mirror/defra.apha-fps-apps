using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactTestPlanCrossTabApiClientTest
{
    public class PactTestPlanCrossTabApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactTestPlanCrossTabApiClient _client;

        public PactTestPlanCrossTabApiClientTests()
        {
            _http   = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactTestPlanCrossTabApiClient(_http, _mapper);
        }

        // ── GetPagedTestPlanCrossTabAsync ─────────────────────────────────────

        #region GetPagedTestPlanCrossTabAsync

        [Fact]
        public async Task GetPagedTestPlanCrossTabAsync_WithSuccessResponse_ReturnsSuccess()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var res = new TestPlanCostBreakdownRes
            {
                Columns    = ["testcode", "shortdescription", "Jan", "Feb"],
                Rows       = [new Dictionary<string, string?> { ["testcode"] = "PT0047", ["Jan"] = "5" }],
                TotalCount = 1,
                Page       = 1,
                PageSize   = 20
            };
            var apiResponse = new ApiResponse<TestPlanCostBreakdownRes> { Success = true, Data = res };

            _http.GetAsync<TestPlanCostBreakdownRes>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetPagedTestPlanCrossTabAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTabAsync_WithSuccessResponse_ReturnsCorrectColumns()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var res = new TestPlanCostBreakdownRes
            {
                Columns    = ["testcode", "shortdescription", "Jan", "Feb"],
                Rows       = [],
                TotalCount = 0,
                Page       = 1,
                PageSize   = 20
            };
            var apiResponse = new ApiResponse<TestPlanCostBreakdownRes> { Success = true, Data = res };

            _http.GetAsync<TestPlanCostBreakdownRes>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetPagedTestPlanCrossTabAsync(query);

            // Assert
            Assert.NotNull(result.Data);
            Assert.Equal(4, result.Data!.Columns.Count);
            Assert.Contains("testcode",          result.Data.Columns);
            Assert.Contains("shortdescription",  result.Data.Columns);
            Assert.Contains("Jan",               result.Data.Columns);
            Assert.Contains("Feb",               result.Data.Columns);
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTabAsync_WithSuccessResponse_ReturnsCorrectRows()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var row1  = new Dictionary<string, string?> { ["testcode"] = "PT0047", ["Jan"] = "5", ["Feb"] = "3" };
            var row2  = new Dictionary<string, string?> { ["testcode"] = "PT0049", ["Jan"] = "2", ["Feb"] = "7" };
            var res = new TestPlanCostBreakdownRes
            {
                Columns    = ["testcode", "Jan", "Feb"],
                Rows       = [row1, row2],
                TotalCount = 2,
                Page       = 1,
                PageSize   = 20
            };
            var apiResponse = new ApiResponse<TestPlanCostBreakdownRes> { Success = true, Data = res };

            _http.GetAsync<TestPlanCostBreakdownRes>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetPagedTestPlanCrossTabAsync(query);

            // Assert
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data!.Rows.Count);
            Assert.Equal("PT0047", result.Data.Rows[0]["testcode"]);
            Assert.Equal("PT0049", result.Data.Rows[1]["testcode"]);
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTabAsync_WithSuccessResponse_ReturnsPaginationValues()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 10 };
            var res = new TestPlanCostBreakdownRes
            {
                Columns    = ["testcode"],
                Rows       = [],
                TotalCount = 250,
                Page       = 2,
                PageSize   = 10
            };
            var apiResponse = new ApiResponse<TestPlanCostBreakdownRes> { Success = true, Data = res };

            _http.GetAsync<TestPlanCostBreakdownRes>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetPagedTestPlanCrossTabAsync(query);

            // Assert
            Assert.NotNull(result.Data);
            Assert.Equal(250, result.Data!.TotalCount);
            Assert.Equal(2,   result.Data.Page);
            Assert.Equal(10,  result.Data.PageSize);
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTabAsync_WithEmptyResult_ReturnsSuccessWithEmptyCollections()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var res = new TestPlanCostBreakdownRes
            {
                Columns    = [],
                Rows       = [],
                TotalCount = 0,
                Page       = 1,
                PageSize   = 20
            };
            var apiResponse = new ApiResponse<TestPlanCostBreakdownRes> { Success = true, Data = res };

            _http.GetAsync<TestPlanCostBreakdownRes>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetPagedTestPlanCrossTabAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data!.Columns);
            Assert.Empty(result.Data!.Rows);
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTabAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var apiResponse = new ApiResponse<TestPlanCostBreakdownRes>
            {
                Success = false,
                Errors  = [new ApiError { Code = "SERVER_500", Message = "Internal Server Error" }]
            };
            var failureDto = ApiResponseDto<TestPlanCostBreakdownDto>.FailureResponse(
                [new ApiErrorDto { Code = "SERVER_500", Message = "Internal Server Error" }],
                new ApiMetaDto());

            _http.GetAsync<TestPlanCostBreakdownRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestPlanCostBreakdownDto>>(apiResponse).Returns(failureDto);

            // Act
            var result = await _client.GetPagedTestPlanCrossTabAsync(query);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTabAsync_WhenApiFails_ReturnsErrors()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var apiResponse = new ApiResponse<TestPlanCostBreakdownRes>
            {
                Success = false,
                Errors  = [new ApiError { Code = "SERVER_500", Message = "Internal Server Error" }]
            };
            var failureDto = ApiResponseDto<TestPlanCostBreakdownDto>.FailureResponse(
                [new ApiErrorDto { Code = "SERVER_500", Message = "Internal Server Error" }],
                new ApiMetaDto());

            _http.GetAsync<TestPlanCostBreakdownRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestPlanCostBreakdownDto>>(apiResponse).Returns(failureDto);

            // Act
            var result = await _client.GetPagedTestPlanCrossTabAsync(query);

            // Assert
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors!);
            Assert.Equal("SERVER_500", result.Errors![0].Code);
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTabAsync_WhenApiReturnsNullData_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var apiResponse = new ApiResponse<TestPlanCostBreakdownRes>
            {
                Success = false,
                Data    = null
            };
            var failureDto = ApiResponseDto<TestPlanCostBreakdownDto>.FailureResponse([], new ApiMetaDto());

            _http.GetAsync<TestPlanCostBreakdownRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TestPlanCostBreakdownDto>>(apiResponse).Returns(failureDto);

            // Act
            var result = await _client.GetPagedTestPlanCrossTabAsync(query);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTabAsync_CallsHttpWithAnyUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var res = new TestPlanCostBreakdownRes { Columns = ["testcode"], Rows = [], TotalCount = 0, Page = 1, PageSize = 20 };
            var apiResponse = new ApiResponse<TestPlanCostBreakdownRes> { Success = true, Data = res };

            _http.GetAsync<TestPlanCostBreakdownRes>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            await _client.GetPagedTestPlanCrossTabAsync(query);

            // Assert
            await _http.Received(1).GetAsync<TestPlanCostBreakdownRes>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetPagedTestPlanCrossTabAsync_MapsRowValues_Correctly()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 20 };
            var row   = new Dictionary<string, string?>
            {
                ["testcode"]        = "PT0047",
                ["shortdescription"] = "EVA serology",
                ["Jan"]             = "10",
                ["Feb"]             = null
            };
            var res = new TestPlanCostBreakdownRes
            {
                Columns    = ["testcode", "shortdescription", "Jan", "Feb"],
                Rows       = [row],
                TotalCount = 1,
                Page       = 1,
                PageSize   = 20
            };
            var apiResponse = new ApiResponse<TestPlanCostBreakdownRes> { Success = true, Data = res };

            _http.GetAsync<TestPlanCostBreakdownRes>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetPagedTestPlanCrossTabAsync(query);

            // Assert
            var resultRow = result.Data!.Rows.Single();
            Assert.Equal("PT0047",       resultRow["testcode"]);
            Assert.Equal("EVA serology", resultRow["shortdescription"]);
            Assert.Equal("10",           resultRow["Jan"]);
            Assert.Null(resultRow["Feb"]);
        }

        #endregion
    }
}
