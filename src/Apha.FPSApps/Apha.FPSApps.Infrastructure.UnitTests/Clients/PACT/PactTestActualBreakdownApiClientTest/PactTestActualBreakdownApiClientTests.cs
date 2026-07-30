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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactTestActualBreakdownApiClientTest
{
    public class PactTestActualBreakdownApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactTestActualBreakdownApiClient _client;

        public PactTestActualBreakdownApiClientTests()
        {
            _http   = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactTestActualBreakdownApiClient(_http, _mapper);
        }

        // ── GetPagedAsync ─────────────────────────────────────────────────────

        #region GetPagedAsync

        [Fact]
        public async Task GetPagedAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var responseItems = new List<TestActualBreakdownRes>
            {
                new() { TestCode = "PT0047", Buyer = "SV3300", Program = "Viro", Portfolio = "QAPTPORT1", Month = 4, PCPrice = 159.00m, PCCost = 319.00m }
            };
            var apiResponse = new ApiResponse<List<TestActualBreakdownRes>> { Success = true, Data = responseItems };
            var expectedDto = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse(
                new List<TestActualBreakdownDto>
                {
                    new() { TestCode = "PT0047", Buyer = "SV3300", Program = "Viro", Portfolio = "QAPTPORT1", Month = 4, PCPrice = 159.00m, PCCost = 319.00m }
                });

            _http.GetAsync<List<TestActualBreakdownRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestActualBreakdownDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetPagedAsync_WithSuccessResponse_ReturnsCorrectDtoValues()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var responseItems = new List<TestActualBreakdownRes>
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
                    PCCost           = 319.00m
                }
            };
            var apiResponse = new ApiResponse<List<TestActualBreakdownRes>> { Success = true, Data = responseItems };
            var expectedDto = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse(
            [
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
                    PCCost           = 319.00m
                }
            ]);

            _http.GetAsync<List<TestActualBreakdownRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestActualBreakdownDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

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
        }

        [Fact]
        public async Task GetPagedAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestActualBreakdownRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([]);

            _http.GetAsync<List<TestActualBreakdownRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestActualBreakdownDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedAsync_WithMultipleItems_ReturnsAllMappedItems()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var responseItems = new List<TestActualBreakdownRes>
            {
                new() { TestCode = "PT0047", Buyer = "SV3300" },
                new() { TestCode = "PT0049", Buyer = "SB4600" },
                new() { TestCode = "TC0001A", Buyer = "EDI300" }
            };
            var apiResponse = new ApiResponse<List<TestActualBreakdownRes>> { Success = true, Data = responseItems };
            var expectedDto = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse(
            [
                new() { TestCode = "PT0047",  Buyer = "SV3300"   },
                new() { TestCode = "PT0049",  Buyer = "SB4600"   },
                new() { TestCode = "TC0001A", Buyer = "EDI300"   }
            ]);

            _http.GetAsync<List<TestActualBreakdownRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestActualBreakdownDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.Count);
        }

        [Fact]
        public async Task GetPagedAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query   = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors  = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<TestActualBreakdownRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<TestActualBreakdownDto>>
            {
                Success = false,
                Errors  = [new() { Message = "API Error", Code = "API_ERROR" }],
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<TestActualBreakdownRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestActualBreakdownDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetPagedAsync_WhenApiReturnsFailure_ErrorsArePreserved()
        {
            // Arrange
            var query  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<TestActualBreakdownRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<TestActualBreakdownDto>>
            {
                Success = false,
                Errors  = [new() { Message = "Not found", Code = "NOT_FOUND" }],
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<TestActualBreakdownRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestActualBreakdownDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors!);
            Assert.Equal("NOT_FOUND", result.Errors!.First().Code);
        }

        [Fact]
        public async Task GetPagedAsync_CallsHttpExecutorExactlyOnce()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestActualBreakdownRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([]);

            _http.GetAsync<List<TestActualBreakdownRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestActualBreakdownDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            await _http.Received(1).GetAsync<List<TestActualBreakdownRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetPagedAsync_OnSuccess_CallsMapperExactlyOnce()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestActualBreakdownRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([]);

            _http.GetAsync<List<TestActualBreakdownRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestActualBreakdownDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            _mapper.Received(1).Map<ApiResponseDto<List<TestActualBreakdownDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetPagedAsync_OnFailure_CallsMapperExactlyOnce()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors      = new List<ApiError> { new() { Code = "ERR" } };
            var apiResponse = new ApiResponse<List<TestActualBreakdownRes>> { Success = false, Errors = errors };
            var mappedDto   = new ApiResponseDto<List<TestActualBreakdownDto>>
            {
                Success = false,
                Errors  = [new() { Code = "ERR" }],
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<TestActualBreakdownRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestActualBreakdownDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            _mapper.Received(1).Map<ApiResponseDto<List<TestActualBreakdownDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetPagedAsync_UrlContainsQueryParameters()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 2, PageSize = 25, SortBy = "buyer" };
            var apiResponse = new ApiResponse<List<TestActualBreakdownRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([]);

            _http.GetAsync<List<TestActualBreakdownRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestActualBreakdownDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert — URL passed to http executor must carry paging params
            await _http.Received(1).GetAsync<List<TestActualBreakdownRes>>(
                Arg.Is<string>(url => url.Contains("page") || url.Contains("pageSize") || url.Contains("Page")));
        }

        [Fact]
        public async Task GetPagedAsync_WhenApiReturnsFailure_DoesNotReturnSuccessTrue()
        {
            // Arrange
            var query       = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TestActualBreakdownRes>>
            {
                Success = false,
                Errors  = [new() { Code = "SERVER_ERROR", Message = "Internal server error" }]
            };
            var mappedDto = new ApiResponseDto<List<TestActualBreakdownDto>>
            {
                Success = false,
                Errors  = [new() { Code = "SERVER_ERROR", Message = "Internal server error" }],
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<TestActualBreakdownRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TestActualBreakdownDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
        }

        #endregion
    }
}
