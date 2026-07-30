using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactRecreateSummaryApiClientTest
{
    public class PactRecreateSummaryApiClientTests
    {
        private readonly IPactHttpExecutor _mockHttp;
        private readonly IMapper _mockMapper;
        private readonly PactRecreateSummaryApiClient _client;

        private const string TestUserId = "TestUser1";
        private const string TestUserName = "Test User";
        private const short TestPeriod = 1;
        private const int TestPageNumber = 1;
        private const int TestPageSize = 20;
        private const int TestTotalRecords = 50;

        public PactRecreateSummaryApiClientTests()
        {
            _mockHttp = Substitute.For<IPactHttpExecutor>();
            _mockMapper = Substitute.For<IMapper>();
            _client = new PactRecreateSummaryApiClient(_mockHttp, _mockMapper);
        }

        #region GetAllRecreateSummariesLogsAsync

        [Fact]
        public async Task GetAllRecreateSummariesLogsAsync_WithSuccessfulResponse_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = TestPageNumber,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            var apiResponse = new ApiResponse<List<RecreateSummaryLogRes>>
            {
                Success = true,
                Data = new List<RecreateSummaryLogRes>
                {
                    new() { Id = 1, UserId = TestUserId, Comments = TestUserName, Period = TestPeriod, DateDone = DateTime.UtcNow }
                },
                Pagination = new Pagination
                {
                    TotalRecords = TestTotalRecords,
                    PageNumber = TestPageNumber,
                    PageSize = TestPageSize,
                    TotalPages = 3
                }
            };

            var mappedDtos = new List<RecreateSummaryLogDto>
            {
                new() { Id = 1, UserId = TestUserId, Comments = TestUserName, Period = TestPeriod, DateDone = DateTime.UtcNow }
            };

            _mockHttp.GetAsync<List<RecreateSummaryLogRes>>(Arg.Any<string>()).Returns(Task.FromResult(apiResponse));
            _mockMapper.Map<ApiResponseDto<List<RecreateSummaryLogDto>>>(Arg.Any<ApiResponse<List<RecreateSummaryLogRes>>>())
                .Returns(new ApiResponseDto<List<RecreateSummaryLogDto>> { Success = true, Data = mappedDtos });

            // Act
            var result = await _client.GetRecreateSummaryLogAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.data);
            Assert.Equal(TestTotalRecords, result.Data.TotalCount);
            Assert.Equal(TestPageNumber, result.Data.PageNumber);
            Assert.Equal(TestPageSize, result.Data.PageSize);
            await _mockHttp.Received(1).GetAsync<List<RecreateSummaryLogRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetAllRecreateSummariesLogsAsync_WithFailedResponse_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = TestPageNumber,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            var apiResponse = new ApiResponse<List<RecreateSummaryLogRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "API Error", Code = "ERR001" } }
            };

            _mockHttp.GetAsync<List<RecreateSummaryLogRes>>(Arg.Any<string>()).Returns(Task.FromResult(apiResponse));
            _mockMapper.Map<ApiResponseDto<List<RecreateSummaryLogDto>>>(Arg.Any<ApiResponse<List<RecreateSummaryLogRes>>>())
                .Returns(new ApiResponseDto<List<RecreateSummaryLogDto>> 
                { 
                    Success = false, 
                    Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERR001" } }
                });

            // Act
            var result = await _client.GetRecreateSummaryLogAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API Error", result.Errors.First().Message);
        }

        [Fact]
        public async Task GetAllRecreateSummariesLogsAsync_WithNullData_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = TestPageNumber,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            var apiResponse = new ApiResponse<List<RecreateSummaryLogRes>>
            {
                Success = true,
                Data = null,
                Pagination = new Pagination
                {
                    TotalRecords = 0,
                    PageNumber = TestPageNumber,
                    PageSize = TestPageSize,
                    TotalPages = 0
                }
            };

            _mockHttp.GetAsync<List<RecreateSummaryLogRes>>(Arg.Any<string>()).Returns(Task.FromResult(apiResponse));
            _mockMapper.Map<ApiResponseDto<List<RecreateSummaryLogDto>>>(Arg.Any<ApiResponse<List<RecreateSummaryLogRes>>>())
                .Returns(new ApiResponseDto<List<RecreateSummaryLogDto>> { Success = true, Data = null });

            // Act
            var result = await _client.GetRecreateSummaryLogAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.data);
            Assert.Equal(0, result.Data.TotalCount);
        }

        [Fact]
        public async Task GetRecreateSummaryLogAsync_WithNullPagination_UsesFallbackValues()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = TestPageNumber,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            var apiResponse = new ApiResponse<List<RecreateSummaryLogRes>>
            {
                Success = true,
                Data = new List<RecreateSummaryLogRes>(),
                Pagination = null
            };

            _mockHttp.GetAsync<List<RecreateSummaryLogRes>>(Arg.Any<string>()).Returns(Task.FromResult(apiResponse));
            _mockMapper.Map<ApiResponseDto<List<RecreateSummaryLogDto>>>(Arg.Any<ApiResponse<List<RecreateSummaryLogRes>>>())
                .Returns(new ApiResponseDto<List<RecreateSummaryLogDto>> { Success = true, Data = new List<RecreateSummaryLogDto>() });

            // Act
            var result = await _client.GetRecreateSummaryLogAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0, result.Data!.TotalCount);
            Assert.Equal(TestPageNumber, result.Data.PageNumber);
            Assert.Equal(TestPageSize, result.Data.PageSize);
        }

        #endregion

        #region GetRecreateSummaryBatchJobHistoryAsync

        [Fact]
        public async Task GetRecreateSummaryBatchJobHistoryAsync_WithSuccessfulResponse_ReturnsMappedDtoList()
        {
            // Arrange
            const string jobName = "RecreateSummary";
            var query = new QueryParameters<string> { Page = TestPageNumber, PageSize = TestPageSize };

            var apiData = new List<BatchJobHistoryRes>
            {
                new() { JobId = 1, JobName = jobName, Status = "Completed", RequestedBy = TestUserId, StartDateTime = DateTime.UtcNow },
                new() { JobId = 1, JobName = jobName, Status = "Running",   RequestedBy = TestUserId, StartDateTime = DateTime.UtcNow.AddMinutes(-5) }
            };
            var apiResponse = new ApiResponse<List<BatchJobHistoryRes>> { Success = true, Data = apiData };

            var mappedDto = new ApiResponseDto<List<BatchJobHistoryDto>>
            {
                Success = true,
                Data = new List<BatchJobHistoryDto>
                {
                    new() { JobId = 1, JobName = jobName, Status = "Completed", RequestedBy = TestUserId, StartDateTime = DateTime.UtcNow },
                    new() { JobId = 1, JobName = jobName, Status = "Running",   RequestedBy = TestUserId, StartDateTime = DateTime.UtcNow.AddMinutes(-5) }
                }
            };

            _mockHttp.GetAsync<List<BatchJobHistoryRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mockMapper.Map<ApiResponseDto<List<BatchJobHistoryDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetRecreateSummaryBatchJobHistoryAsync(query, jobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);

            await _mockHttp.Received(1).GetAsync<List<BatchJobHistoryRes>>(
                Arg.Is<string>(url => url.Contains(jobName)));
            _mockMapper.Received(1).Map<ApiResponseDto<List<BatchJobHistoryDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetRecreateSummaryBatchJobHistoryAsync_WithFailedResponse_ReturnsFailureResponse()
        {
            // Arrange
            const string jobName = "RecreateSummary";
            var query = new QueryParameters<string> { Page = TestPageNumber, PageSize = TestPageSize };

            var apiResponse = new ApiResponse<List<BatchJobHistoryRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Code = "ERR001", Message = "API Error" } }
            };
            var mappedFailure = new ApiResponseDto<List<BatchJobHistoryDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "ERR001", Message = "API Error" } },
                Meta = new ApiMetaDto()
            };

            _mockHttp.GetAsync<List<BatchJobHistoryRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mockMapper.Map<ApiResponseDto<List<BatchJobHistoryDto>>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetRecreateSummaryBatchJobHistoryAsync(query, jobName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("ERR001", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetRecreateSummaryBatchJobHistoryAsync_UrlContainsJobNameEncoded()
        {
            // Arrange
            const string jobName = "Recreate Summary"; // contains a space — must be URL-encoded
            var query = new QueryParameters<string> { Page = TestPageNumber, PageSize = TestPageSize };

            var apiResponse = new ApiResponse<List<BatchJobHistoryRes>> { Success = true, Data = new List<BatchJobHistoryRes>() };
            var mappedDto = new ApiResponseDto<List<BatchJobHistoryDto>> { Success = true, Data = new List<BatchJobHistoryDto>() };

            _mockHttp.GetAsync<List<BatchJobHistoryRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mockMapper.Map<ApiResponseDto<List<BatchJobHistoryDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetRecreateSummaryBatchJobHistoryAsync(query, jobName);

            // Assert — URL must contain percent-encoded space (%20), not a raw space
            await _mockHttp.Received(1).GetAsync<List<BatchJobHistoryRes>>(
                Arg.Is<string>(url => url.Contains("Recreate%20Summary")));
        }

        [Fact]
        public async Task GetRecreateSummaryBatchJobHistoryAsync_UsesCorrectBaseEndpoint()
        {
            // Arrange
            const string jobName = "RecreateSummary";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            var apiResponse = new ApiResponse<List<BatchJobHistoryRes>> { Success = true, Data = new List<BatchJobHistoryRes>() };
            var mappedDto = new ApiResponseDto<List<BatchJobHistoryDto>> { Success = true, Data = new List<BatchJobHistoryDto>() };

            _mockHttp.GetAsync<List<BatchJobHistoryRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mockMapper.Map<ApiResponseDto<List<BatchJobHistoryDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetRecreateSummaryBatchJobHistoryAsync(query, jobName);

            // Assert
            await _mockHttp.Received(1).GetAsync<List<BatchJobHistoryRes>>(
                Arg.Is<string>(url => url.Contains(PactApiEndpoints.GetRecreateSummaryBatchJobHistory)));
        }

        #endregion

        #region CanRunRecreateSummaryBatchJobAsync

        [Fact]
        public async Task CanRunRecreateSummaryBatchJobAsync_WhenJobCanRun_ReturnsTrueResponse()
        {
            // Arrange
            const string jobName = "RecreateSummary";
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };

            _mockHttp.GetAsync<bool>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.CanRunRecreateSummaryBatchJobAsync(jobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);

            await _mockHttp.Received(1).GetAsync<bool>(
                Arg.Is<string>(url => url.Contains(jobName)));
        }

        [Fact]
        public async Task CanRunRecreateSummaryBatchJobAsync_WhenJobIsRunning_ReturnsFalseResponse()
        {
            // Arrange
            const string jobName = "RecreateSummary";
            var apiResponse = new ApiResponse<bool> { Success = true, Data = false };

            _mockHttp.GetAsync<bool>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.CanRunRecreateSummaryBatchJobAsync(jobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
        }

        [Fact]
        public async Task CanRunRecreateSummaryBatchJobAsync_WithFailedResponse_ReturnsFailureResponse()
        {
            // Arrange
            const string jobName = "RecreateSummary";
            var apiResponse = new ApiResponse<bool>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Code = "ERR001", Message = "Service error" } }
            };
            var mappedFailure = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "ERR001", Message = "Service error" } },
                Meta = new ApiMetaDto()
            };

            _mockHttp.GetAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
            _mockMapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.CanRunRecreateSummaryBatchJobAsync(jobName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("ERR001", result.Errors![0].Code);
        }

        [Fact]
        public async Task CanRunRecreateSummaryBatchJobAsync_UrlContainsJobNameAndCorrectEndpoint()
        {
            // Arrange
            const string jobName = "RecreateSummary";
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            _mockHttp.GetAsync<bool>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            await _client.CanRunRecreateSummaryBatchJobAsync(jobName);

            // Assert
            await _mockHttp.Received(1).GetAsync<bool>(
                Arg.Is<string>(url =>
                    url.Contains(PactApiEndpoints.CanRunRecreateSummaryBatchJob) &&
                    url.Contains(jobName)));
        }

        #endregion

        #region TriggerRecreateSummariesBatchJobAsync

        [Fact]
        public async Task TriggerRecreateSummariesBatchJobAsync_WithSuccessfulResponse_ReturnsMappedDto()
        {
            // Arrange
            const int month = 6;
            var apiData = new BatchJobEventTriggerRes
            {
                EventId = "event-abc-123",
                Jobqueue = new BatchJobQueueRes
                {
                    JobqueueId = Guid.NewGuid(),
                    JobExecutionId = Guid.NewGuid(),
                    JobId = 1,
                    StatusId = 10,
                    RequestedBy = TestUserId,
                    StartDateTime = DateTime.UtcNow
                }
            };
            var apiResponse = new ApiResponse<BatchJobEventTriggerRes> { Success = true, Data = apiData };

            var mappedDto = new ApiResponseDto<BatchJobEventTriggerDto>
            {
                Success = true,
                Data = new BatchJobEventTriggerDto { EventId = "event-abc-123" }
            };

            _mockHttp.PostAsync<RecreateSummariesReq, BatchJobEventTriggerRes>(
                    PactApiEndpoints.TriggerRecreateSummariesBatchJob,
                    Arg.Is<RecreateSummariesReq>(r => r.Month == month))
                .Returns(apiResponse);
            _mockMapper.Map<ApiResponseDto<BatchJobEventTriggerDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.TriggerRecreateSummariesBatchJobAsync(month);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("event-abc-123", result.Data.EventId);

            await _mockHttp.Received(1).PostAsync<RecreateSummariesReq, BatchJobEventTriggerRes>(
                PactApiEndpoints.TriggerRecreateSummariesBatchJob,
                Arg.Is<RecreateSummariesReq>(r => r.Month == month));
            _mockMapper.Received(1).Map<ApiResponseDto<BatchJobEventTriggerDto>>(apiResponse);
        }

        [Fact]
        public async Task TriggerRecreateSummariesBatchJobAsync_WithSuccessButNullData_ReturnsFailureResponse()
        {
            // Arrange
            const int month = 6;
            var apiResponse = new ApiResponse<BatchJobEventTriggerRes> { Success = true, Data = null };

            var mappedFailure = new ApiResponseDto<BatchJobEventTriggerDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "NO_DATA", Message = "No data returned" } },
                Meta = new ApiMetaDto()
            };

            _mockHttp.PostAsync<RecreateSummariesReq, BatchJobEventTriggerRes>(
                    PactApiEndpoints.TriggerRecreateSummariesBatchJob, Arg.Any<RecreateSummariesReq>())
                .Returns(apiResponse);
            _mockMapper.Map<ApiResponseDto<BatchJobEventTriggerDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.TriggerRecreateSummariesBatchJobAsync(month);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            _mockMapper.Received(1).Map<ApiResponseDto<BatchJobEventTriggerDto>>(apiResponse);
        }

        [Fact]
        public async Task TriggerRecreateSummariesBatchJobAsync_WithFailedResponse_ReturnsFailureResponseWithErrors()
        {
            // Arrange
            const int month = 0;
            var apiResponse = new ApiResponse<BatchJobEventTriggerRes>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Code = "INVALID_MONTH", Message = "Month must be between 1 and 12." } }
            };

            var mappedFailure = new ApiResponseDto<BatchJobEventTriggerDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "INVALID_MONTH", Message = "Month must be between 1 and 12." } },
                Meta = new ApiMetaDto()
            };

            _mockHttp.PostAsync<RecreateSummariesReq, BatchJobEventTriggerRes>(
                    PactApiEndpoints.TriggerRecreateSummariesBatchJob, Arg.Any<RecreateSummariesReq>())
                .Returns(apiResponse);
            _mockMapper.Map<ApiResponseDto<BatchJobEventTriggerDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.TriggerRecreateSummariesBatchJobAsync(month);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("INVALID_MONTH", result.Errors[0].Code);

            await _mockHttp.Received(1).PostAsync<RecreateSummariesReq, BatchJobEventTriggerRes>(
                PactApiEndpoints.TriggerRecreateSummariesBatchJob,
                Arg.Is<RecreateSummariesReq>(r => r.Month == month));
        }

        [Fact]
        public async Task TriggerRecreateSummariesBatchJobAsync_UsesCorrectEndpointAndMonthInRequest()
        {
            // Arrange
            const int month = 3;
            var apiData = new BatchJobEventTriggerRes { EventId = "ev", Jobqueue = new BatchJobQueueRes { RequestedBy = TestUserId, StartDateTime = DateTime.UtcNow } };
            var apiResponse = new ApiResponse<BatchJobEventTriggerRes> { Success = true, Data = apiData };
            var mappedDto = new ApiResponseDto<BatchJobEventTriggerDto> { Success = true, Data = new BatchJobEventTriggerDto { EventId = "ev" } };

            _mockHttp.PostAsync<RecreateSummariesReq, BatchJobEventTriggerRes>(Arg.Any<string>(), Arg.Any<RecreateSummariesReq>())
                .Returns(apiResponse);
            _mockMapper.Map<ApiResponseDto<BatchJobEventTriggerDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.TriggerRecreateSummariesBatchJobAsync(month);

            // Assert
            await _mockHttp.Received(1).PostAsync<RecreateSummariesReq, BatchJobEventTriggerRes>(
                Arg.Is<string>(url => url == PactApiEndpoints.TriggerRecreateSummariesBatchJob),
                Arg.Is<RecreateSummariesReq>(r => r.Month == month));
        }

        #endregion
    }
}
