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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactMonthlyTimeApiClientTest
{
    public class PactMonthlyTimeApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactMonthlyTimeApiClient _client;

        public PactMonthlyTimeApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            SetupMapper();
            _client = new PactMonthlyTimeApiClient(_http, _mapper);
        }

        private void SetupMapper()
        {
            _mapper.Map<ApiResponseDto<List<MonthlyTimeLogDto>>>(Arg.Any<ApiResponse<List<MonthlyTimeLogRes>>>())
                .Returns(callInfo =>
                {
                    var response = callInfo.ArgAt<ApiResponse<List<MonthlyTimeLogRes>>>(0);
                    if (response == null || !response.Success || response.Data == null)
                        return ApiResponseDto<List<MonthlyTimeLogDto>>.FailureResponse(
                            response?.Errors?.Select(e => new ApiErrorDto { Message = e.Message, Code = e.Code }).ToList() ?? [],
                            new ApiMetaDto());

                    var dtoList = response.Data.Select(res => new MonthlyTimeLogDto
                    {
                        SequenceNo = res.SequenceNo,
                        TimeCode = res.TimeCode,
                        ParentProject = res.ParentProject,
                        Month = res.Month,
                        PactStaffId = res.PactStaffId,
                        WorkGroup = res.WorkGroup,
                        Hours = res.Hours,
                        DateTime = res.DateTime,
                        UserId = res.UserId,
                        InsertDelete = res.InsertDelete,
                        FpsYear = res.FpsYear
                    }).ToList();

                    return ApiResponseDto<List<MonthlyTimeLogDto>>.SuccessResponse(dtoList);
                });
        }

        #region SearchAsync Tests

        [Fact]
        public async Task SearchAsync_WithSuccessResponse_ReturnsMappedDtoList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var filter = new MonthlyTimeLogFilterDto { WorkGroup = "WG1", TimeCode = "TC1" };
            var resList = new List<MonthlyTimeLogRes>
            {
                new() { SequenceNo = 1, TimeCode = "TC1", PactStaffId = "S001", WorkGroup = "WG1" },
                new() { SequenceNo = 2, TimeCode = "TC1", PactStaffId = "S002", WorkGroup = "WG1" }
            };
            var httpResponse = new ApiResponse<List<MonthlyTimeLogRes>> { Success = true, Data = resList };

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            var result = await _client.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task SearchAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto();
            var httpResponse = new ApiResponse<List<MonthlyTimeLogRes>> { Success = true, Data = new List<MonthlyTimeLogRes>() };

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>()).Returns(httpResponse);

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
            var filter = new MonthlyTimeLogFilterDto { WorkGroup = "WG1" };
            var httpResponse = new ApiResponse<List<MonthlyTimeLogRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "HTTP Error", Code = "HTTP_ERROR" } }
            };

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>()).Returns(httpResponse);

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
            var httpResponse = new ApiResponse<List<MonthlyTimeLogRes>> { Success = true, Data = new List<MonthlyTimeLogRes>() };

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            var result = await _client.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<MonthlyTimeLogRes>>(
                Arg.Is<string>(url =>
                    url.Contains("workGroup=WG1") &&
                    url.Contains("timeCode=TC1") &&
                    url.Contains("pactStaffId=S001") &&
                    url.Contains("parentProject=PP1") &&
                    url.Contains("dateImported=2024-06-01") &&
                    url.Contains("month=6") &&
                    url.Contains("userId=USER1") &&
                    url.Contains("insertDelete=I")));
        }

        [Fact]
        public async Task SearchAsync_WithNullFilters_DoesNotAppendFilterParamsToUrl()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto();
            var httpResponse = new ApiResponse<List<MonthlyTimeLogRes>> { Success = true, Data = new List<MonthlyTimeLogRes>() };

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            var result = await _client.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            await _http.Received(1).GetAsync<List<MonthlyTimeLogRes>>(
                Arg.Is<string>(url =>
                    !url.Contains("workGroup=") &&
                    !url.Contains("timeCode=") &&
                    !url.Contains("pactStaffId=") &&
                    !url.Contains("parentProject=") &&
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
            var filter = new MonthlyTimeLogFilterDto();
            var httpResponse = new ApiResponse<List<MonthlyTimeLogRes>> { Success = true, Data = new List<MonthlyTimeLogRes>() };

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            await _client.SearchAsync(query, filter);

            // Assert
            await _http.Received(1).GetAsync<List<MonthlyTimeLogRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/monthlytime/log/search")));
        }

        [Fact]
        public async Task SearchAsync_WorkGroupOnly_AppendsOnlyWorkGroupToUrl()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto { WorkGroup = "WG1" };
            var httpResponse = new ApiResponse<List<MonthlyTimeLogRes>> { Success = true, Data = new List<MonthlyTimeLogRes>() };

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            await _client.SearchAsync(query, filter);

            // Assert
            await _http.Received(1).GetAsync<List<MonthlyTimeLogRes>>(
                Arg.Is<string>(url =>
                    url.Contains("workGroup=WG1") &&
                    !url.Contains("timeCode=") &&
                    !url.Contains("pactStaffId=") &&
                    !url.Contains("parentProject=") &&
                    !url.Contains("dateImported=") &&
                    !url.Contains("month=") &&
                    !url.Contains("userId=") &&
                    !url.Contains("insertDelete=")));
        }

        [Fact]
        public async Task SearchAsync_DateImportedOnly_AppendsFormattedDateToUrl()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto { DateImported = new DateTime(2024, 3, 5) };
            var httpResponse = new ApiResponse<List<MonthlyTimeLogRes>> { Success = true, Data = new List<MonthlyTimeLogRes>() };

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            await _client.SearchAsync(query, filter);

            // Assert
            await _http.Received(1).GetAsync<List<MonthlyTimeLogRes>>(
                Arg.Is<string>(url => url.Contains("dateImported=2024-03-05")));
        }

        [Fact]
        public async Task SearchAsync_MonthOnly_AppendsMonthToUrl()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto { Month = 9.0 };
            var httpResponse = new ApiResponse<List<MonthlyTimeLogRes>> { Success = true, Data = new List<MonthlyTimeLogRes>() };

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>()).Returns(httpResponse);

            // Act
            await _client.SearchAsync(query, filter);

            // Assert
            await _http.Received(1).GetAsync<List<MonthlyTimeLogRes>>(
                Arg.Is<string>(url => url.Contains("month=9")));
        }

        [Fact]
        public async Task SearchAsync_HttpExecutorThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto();

            _http.GetAsync<List<MonthlyTimeLogRes>>(Arg.Any<string>())
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
            var apiResponse = new ApiResponse<List<MonthlyTimeRes>>
            {
                Success = true,
                Data = [new MonthlyTimeRes { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1", Hours = 7 }]
            };
            var mapped = ApiResponseDto<List<MonthlyTimeDto>>.SuccessResponse(
            [
                new MonthlyTimeDto { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1", Hours = 7 }
            ]);

            _http.GetAsync<List<MonthlyTimeRes>>(Arg.Is<string>(url =>
                url.Contains("monthlytime/live") &&
                url.Contains("workGroup=WG1") &&
                url.Contains("timeCode=TC1") &&
                url.Contains("pactStaffId=S1") &&
                url.Contains("parentProject=PP1") &&
                url.Contains("month=6"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonthlyTimeDto>>>(apiResponse).Returns(mapped);

            var result = await _client.GetLiveAsync(query, "WG1", "TC1", "S1", "PP1", 6);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<MonthlyTimeRes>
            {
                Success = false,
                Errors = [new ApiError { Message = "failed", Code = "ERR" }]
            };
            var mapped = new ApiResponseDto<MonthlyTimeDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "failed", Code = "ERR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<MonthlyTimeRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyTimeDto>>(apiResponse).Returns(mapped);

            var result = await _client.GetLiveByKeyAsync("S1", "TC1", 6, "PP1");

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task UpdateLiveAsync_WithSuccessResponse_ReturnsMappedData()
        {
            var dto = new MonthlyTimeDto { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1", Hours = 7 };
            var request = new MonthlyTimeReq { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1", Hours = 7 };
            var apiResponse = new ApiResponse<MonthlyTimeRes>
            {
                Success = true,
                Data = new MonthlyTimeRes { PactStaffId = "S1", TimeCode = "TC1", ParentProject = "PP1", Month = 6, WorkGroup = "WG1", Hours = 7 }
            };
            var mapped = ApiResponseDto<MonthlyTimeDto>.SuccessResponse(dto);

            _mapper.Map<MonthlyTimeReq>(dto).Returns(request);
            _http.PutAsync<MonthlyTimeReq, MonthlyTimeRes>(Arg.Any<string>(), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MonthlyTimeDto>>(apiResponse).Returns(mapped);

            var result = await _client.UpdateLiveAsync(dto);

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).PutAsync<MonthlyTimeReq, MonthlyTimeRes>(Arg.Any<string>(), request);
        }

        #endregion

        #region Staging Methods Tests

        [Fact]
        public async Task GetStagingAsync_WithPassedFilter_AppendsPassedQuery()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<StagingMonthlyTimeRes>> { Success = true, Data = [] };
            var mapped = ApiResponseDto<List<StagingMonthlyTimeDto>>.SuccessResponse([]);

            _http.GetAsync<List<StagingMonthlyTimeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StagingMonthlyTimeDto>>>(apiResponse).Returns(mapped);

            var result = await _client.GetStagingAsync(query, false);

            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<StagingMonthlyTimeRes>>(Arg.Is<string>(url => url.Contains("passed=false")));
        }

        #endregion
    }
}
