using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactMonthlyOutputApiClientTest
{
    public class PactMonthlyOutputApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactMonthlyOutputApiClient _client;

        public PactMonthlyOutputApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            SetupMapper();
            _client = new PactMonthlyOutputApiClient(_http, _mapper);
        }

        private void SetupMapper()
        {
            _mapper.Map<ApiResponseDto<List<MonthlyOutputLogDto>>>(Arg.Any<ApiResponse<List<MonthlyOutputLogRes>>>())
                .Returns(callInfo =>
                {
                    var response = callInfo.ArgAt<ApiResponse<List<MonthlyOutputLogRes>>>(0);
                    if (response == null || !response.Success || response.Data == null)
                        return ApiResponseDto<List<MonthlyOutputLogDto>>.FailureResponse(
                            response?.Errors?.Select(e => new ApiErrorDto { Message = e.Message, Code = e.Code }).ToList() ?? [],
                            new ApiMetaDto());

                    var dtoList = response.Data.Select(res => new MonthlyOutputLogDto
                    {
                        SequenceNo = res.SequenceNo,
                        TestCode = res.TestCode,
                        Buyer = res.Buyer,
                        Month = res.Month,
                        WorkGroup = res.WorkGroup,
                        Volume = res.Volume,
                        DateTime = res.DateTime,
                        UserId = res.UserId,
                        InsertDelete = res.InsertDelete,
                        FpsYear = res.FpsYear
                    }).ToList();

                    return ApiResponseDto<List<MonthlyOutputLogDto>>.SuccessResponse(dtoList);
                });
        }

        #region SearchAsync Tests

        [Fact]
        public async Task SearchAsync_WithSuccessResponse_ReturnsMappedDtoList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var filter = new MonthlyOutputLogFilterDto { WorkGroup = "WG1", TestCode = "TC1" };
            var resList = new List<MonthlyOutputLogRes>
            {
                new() { SequenceNo = 1, TestCode = "TC1", Buyer = "BuyerA", WorkGroup = "WG1" },
                new() { SequenceNo = 2, TestCode = "TC1", Buyer = "BuyerB", WorkGroup = "WG1" }
            };
            var httpResponse = new ApiResponse<List<MonthlyOutputLogRes>> { Success = true, Data = resList };

            _http.GetAsync<List<MonthlyOutputLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            var result = await _client.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<MonthlyOutputLogRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task SearchAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyOutputLogFilterDto();
            var httpResponse = new ApiResponse<List<MonthlyOutputLogRes>> { Success = true, Data = new List<MonthlyOutputLogRes>() };

            _http.GetAsync<List<MonthlyOutputLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            var result = await _client.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task SearchAsync_WhenHttpFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyOutputLogFilterDto { WorkGroup = "WG1" };
            var httpResponse = new ApiResponse<List<MonthlyOutputLogRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "HTTP Error", Code = "HTTP_ERROR" } }
            };

            _http.GetAsync<List<MonthlyOutputLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            var result = await _client.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task SearchAsync_WithAllFilters_AppendsFilterParamsToUrl()
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
            var httpResponse = new ApiResponse<List<MonthlyOutputLogRes>> { Success = true, Data = new List<MonthlyOutputLogRes>() };

            _http.GetAsync<List<MonthlyOutputLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            var result = await _client.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<MonthlyOutputLogRes>>(
                Arg.Is<string>(url =>
                    url.Contains("workGroup=WG1") &&
                    url.Contains("testCode=TC1") &&
                    url.Contains("buyer=BuyerA") &&
                    url.Contains("dateImported=2024-01-15") &&
                    url.Contains("month=1") &&
                    url.Contains("userId=user1") &&
                    url.Contains("insertDelete=I")));
        }

        [Fact]
        public async Task SearchAsync_WithNullFilters_DoesNotAppendFilterParamsToUrl()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyOutputLogFilterDto();
            var httpResponse = new ApiResponse<List<MonthlyOutputLogRes>> { Success = true, Data = new List<MonthlyOutputLogRes>() };

            _http.GetAsync<List<MonthlyOutputLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            var result = await _client.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            await _http.Received(1).GetAsync<List<MonthlyOutputLogRes>>(
                Arg.Is<string>(url =>
                    !url.Contains("workGroup=") &&
                    !url.Contains("testCode=") &&
                    !url.Contains("buyer=") &&
                    !url.Contains("dateImported=") &&
                    !url.Contains("month=") &&
                    !url.Contains("userId=") &&
                    !url.Contains("insertDelete=")));
        }

        [Fact]
        public async Task SearchAsync_UrlContainsBaseEndpoint()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyOutputLogFilterDto();
            var httpResponse = new ApiResponse<List<MonthlyOutputLogRes>> { Success = true, Data = new List<MonthlyOutputLogRes>() };

            _http.GetAsync<List<MonthlyOutputLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            await _client.SearchAsync(query, filter);

            // Assert
            await _http.Received(1).GetAsync<List<MonthlyOutputLogRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/monthlyoutput/log/search")));
        }

        [Fact]
        public async Task SearchAsync_HttpExecutorThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyOutputLogFilterDto();

            _http.GetAsync<List<MonthlyOutputLogRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("HTTP executor error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.SearchAsync(query, filter));
        }

        #endregion

        #region Live Methods Tests

        [Fact]
        public async Task GetLiveAsync_WithSuccessResponse_ReturnsMappedData()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<MonthlyOutputRes>>
            {
                Success = true,
                Data = [new MonthlyOutputRes { TestCode = "TC1", Buyer = "B1", WorkGroup = "WG1", Month = 6, Volume = 10 }]
            };
            var mapped = ApiResponseDto<List<PactMonthlyOutputDto>>.SuccessResponse(
            [
                new PactMonthlyOutputDto { TestCode = "TC1", Buyer = "B1", WorkGroup = "WG1", Month = 6, Volume = 10 }
            ]);

            _http.GetAsync<List<MonthlyOutputRes>>(Arg.Is<string>(url =>
                url.Contains("monthlyoutput/live") &&
                url.Contains("workGroup=WG1") &&
                url.Contains("testCode=TC1") &&
                url.Contains("buyer=B1") &&
                url.Contains("month=6"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<PactMonthlyOutputDto>>>(apiResponse).Returns(mapped);

            var result = await _client.GetLiveAsync(query, "WG1", "TC1", "B1", 6);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<MonthlyOutputRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "failed", Code = "ERR" }]
            };
            var mapped = new ApiResponseDto<PactMonthlyOutputDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "failed", Code = "ERR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<MonthlyOutputRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<PactMonthlyOutputDto>>(apiResponse).Returns(mapped);

            var result = await _client.GetLiveByKeyAsync("TC1", "B1", 6, "WG1");

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task UpdateLiveAsync_WithSuccessResponse_ReturnsMappedData()
        {
            var dto = new PactMonthlyOutputDto { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 9 };
            var request = new MonthlyOutputReq { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 9 };
            var apiResponse = new ApiResponse<MonthlyOutputRes>
            {
                Success = true,
                Data = new MonthlyOutputRes { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 9 }
            };
            var mapped = ApiResponseDto<PactMonthlyOutputDto>.SuccessResponse(dto);

            _mapper.Map<MonthlyOutputReq>(dto).Returns(request);
            _http.PutAsync<MonthlyOutputReq, MonthlyOutputRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<PactMonthlyOutputDto>>(apiResponse).Returns(mapped);

            var result = await _client.UpdateLiveAsync(dto);

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).PutAsync<MonthlyOutputReq, MonthlyOutputRes>(Arg.Any<string>(), request);
        }

        #endregion

        #region Staging Methods Tests

        [Fact]
        public async Task GetStagingAsync_WithPassedFilter_AppendsPassedQuery()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<StagingMonthlyOutputRes>> { Success = true, Data = [] };
            var mapped = ApiResponseDto<List<StagingMonthlyOutputDto>>.SuccessResponse([]);

            _http.GetAsync<List<StagingMonthlyOutputRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StagingMonthlyOutputDto>>>(apiResponse).Returns(mapped);

            var result = await _client.GetStagingAsync(query, true);

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<StagingMonthlyOutputRes>>(Arg.Is<string>(url => url.Contains("passed=true")));
        }

        #endregion
    }
}
