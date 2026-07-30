using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.RecreateSummaryServiceTest
{
    public class RecreateSummaryServiceServiceTests
    {
        private readonly IPactApiClient _mockPactClient;
        private readonly IPactRecreateSummaryApiClient _mockLogApiClient;
        private readonly RecreateSummaryService _service;

        private const string TestUserId = "TestUser1";
        private const string TestUserName = "Test User";
        private const short TestPeriod = 1;
        private const int TestPageNumber = 1;
        private const int TestPageSize = 20;
        private const int TestTotalRecords = 50;

        public RecreateSummaryServiceServiceTests()
        {
            _mockPactClient = Substitute.For<IPactApiClient>();
            _mockLogApiClient = Substitute.For<IPactRecreateSummaryApiClient>();
            _mockPactClient.PactRecreateSummary.Returns(_mockLogApiClient);
            _service = new RecreateSummaryService(_mockPactClient);
        }

        #region GetRecreateSummaryLogAsync
        [Fact]
        public async Task GetRecreateSummaryLogAsync_WithValidQuery_ReturnsSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = TestPageNumber,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            var expectedResponse = new ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>
            {
                Success = true,
                Data = new PaginatedResult<RecreateSummaryLogDto>(
                    new List<RecreateSummaryLogDto>
                    {
                        new() { Id = 1, UserId = TestUserId, Comments = TestUserName, Period = TestPeriod, DateDone = DateTime.UtcNow }
                    },
                    TestTotalRecords,
                    TestPageNumber,
                    TestPageSize)
            };

            _mockLogApiClient.GetRecreateSummaryLogAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetRecreateSummaryLogAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data.data);
            Assert.Equal(TestTotalRecords, result.Data.TotalCount);
            await _mockLogApiClient.Received(1).GetRecreateSummaryLogAsync(query);
        }

        [Fact]
        public async Task GetRecreateSummaryLogAsync_WithFailedApiResponse_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = TestPageNumber,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            var expectedResponse = new ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERR001" } }
            };

            _mockLogApiClient.GetRecreateSummaryLogAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetRecreateSummaryLogAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetAllRecreateSummariesLogsAsync_WithEmptyResult_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = TestPageNumber,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            var expectedResponse = new ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>
            {
                Success = true,
                Data = new PaginatedResult<RecreateSummaryLogDto>(
                    new List<RecreateSummaryLogDto>(),
                    0,
                    TestPageNumber,
                    TestPageSize)
            };

            _mockLogApiClient.GetRecreateSummaryLogAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetRecreateSummaryLogAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data.data);
            Assert.Equal(0, result.Data.TotalCount);
        }

        [Fact]
        public async Task GetAllRecreateSummariesLogsAsync_ApiClientThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = TestPageNumber,
                PageSize = TestPageSize,
                Filter = "{}"
            };

            _mockLogApiClient.GetRecreateSummaryLogAsync(query)
                .Returns(Task.FromException<ApiResponseDto<PaginatedResult<RecreateSummaryLogDto>>>(
                    new InvalidOperationException("API Client error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetRecreateSummaryLogAsync(query));
        }

        #endregion

        #region GetRecreateSummaryBatchJobHistoryAsync

        [Fact]
        public async Task GetRecreateSummaryBatchJobHistoryAsync_WithSuccessfulResponse_ReturnsBatchJobHistory()
        {
            // Arrange
            const string jobName = "RecreateSummary";
            var query = new QueryParameters<string> { Page = TestPageNumber, PageSize = TestPageSize };

            var expectedResponse = new ApiResponseDto<List<BatchJobHistoryDto>>
            {
                Success = true,
                Data = new List<BatchJobHistoryDto>
                {
                    new() { JobId = 1, JobName = jobName, Status = "Completed", RequestedBy = TestUserId, StartDateTime = DateTime.UtcNow,EndDateTime = DateTime.UtcNow,ErrorMessage ="Initiated" },
                    new() { JobId = 1, JobName = jobName, Status = "Running",   RequestedBy = TestUserId, StartDateTime = DateTime.UtcNow.AddMinutes(-5),EndDateTime = DateTime.UtcNow,ErrorMessage ="Initiated" }
                }
            };

            _mockLogApiClient.GetRecreateSummaryBatchJobHistoryAsync(query, jobName).Returns(expectedResponse);

            // Act
            var result = await _service.GetRecreateSummaryBatchJobHistoryAsync(query, jobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _mockLogApiClient.Received(1).GetRecreateSummaryBatchJobHistoryAsync(query, jobName);
        }

        [Fact]
        public async Task GetRecreateSummaryBatchJobHistoryAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            const string jobName = "RecreateSummary";
            var query = new QueryParameters<string> { Page = TestPageNumber, PageSize = TestPageSize };

            var expectedResponse = new ApiResponseDto<List<BatchJobHistoryDto>>
            {
                Success = true,
                Data = new List<BatchJobHistoryDto>()
            };

            _mockLogApiClient.GetRecreateSummaryBatchJobHistoryAsync(query, jobName).Returns(expectedResponse);

            // Act
            var result = await _service.GetRecreateSummaryBatchJobHistoryAsync(query, jobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _mockLogApiClient.Received(1).GetRecreateSummaryBatchJobHistoryAsync(query, jobName);
        }

        [Fact]
        public async Task GetRecreateSummaryBatchJobHistoryAsync_WithFailedResponse_ReturnsFailureResponse()
        {
            // Arrange
            const string jobName = "RecreateSummary";
            var query = new QueryParameters<string> { Page = TestPageNumber, PageSize = TestPageSize };

            var expectedResponse = new ApiResponseDto<List<BatchJobHistoryDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERR001" } }
            };

            _mockLogApiClient.GetRecreateSummaryBatchJobHistoryAsync(query, jobName).Returns(expectedResponse);

            // Act
            var result = await _service.GetRecreateSummaryBatchJobHistoryAsync(query, jobName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task GetRecreateSummaryBatchJobHistoryAsync_ApiClientThrowsException_PropagatesException()
        {
            // Arrange
            const string jobName = "RecreateSummary";
            var query = new QueryParameters<string> { Page = TestPageNumber, PageSize = TestPageSize };

            _mockLogApiClient.GetRecreateSummaryBatchJobHistoryAsync(query, jobName)
                .Returns(Task.FromException<ApiResponseDto<List<BatchJobHistoryDto>>>(
                    new InvalidOperationException("API Client error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GetRecreateSummaryBatchJobHistoryAsync(query, jobName));
        }

        #endregion

        #region CanRunRecreateSummaryBatchJobAsync

        [Fact]
        public async Task CanRunRecreateSummaryBatchJobAsync_WhenJobCanRun_ReturnsTrue()
        {
            // Arrange
            const string jobName = "RecreateSummary";
            var expectedResponse = new ApiResponseDto<bool> { Success = true, Data = true };

            _mockLogApiClient.CanRunRecreateSummaryBatchJobAsync(jobName).Returns(expectedResponse);

            // Act
            var result = await _service.CanRunRecreateSummaryBatchJobAsync(jobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _mockLogApiClient.Received(1).CanRunRecreateSummaryBatchJobAsync(jobName);
        }

        [Fact]
        public async Task CanRunRecreateSummaryBatchJobAsync_WhenJobIsRunning_ReturnsFalse()
        {
            // Arrange
            const string jobName = "RecreateSummary";
            var expectedResponse = new ApiResponseDto<bool> { Success = true, Data = false };

            _mockLogApiClient.CanRunRecreateSummaryBatchJobAsync(jobName).Returns(expectedResponse);

            // Act
            var result = await _service.CanRunRecreateSummaryBatchJobAsync(jobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            await _mockLogApiClient.Received(1).CanRunRecreateSummaryBatchJobAsync(jobName);
        }

        [Fact]
        public async Task CanRunRecreateSummaryBatchJobAsync_WithFailedResponse_ReturnsFailure()
        {
            // Arrange
            const string jobName = "RecreateSummary";
            var expectedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "ERR001", Message = "Service error" } }
            };

            _mockLogApiClient.CanRunRecreateSummaryBatchJobAsync(jobName).Returns(expectedResponse);

            // Act
            var result = await _service.CanRunRecreateSummaryBatchJobAsync(jobName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task CanRunRecreateSummaryBatchJobAsync_ApiClientThrowsException_PropagatesException()
        {
            // Arrange
            const string jobName = "RecreateSummary";

            _mockLogApiClient.CanRunRecreateSummaryBatchJobAsync(jobName)
                .Returns(Task.FromException<ApiResponseDto<bool>>(
                    new InvalidOperationException("API Client error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CanRunRecreateSummaryBatchJobAsync(jobName));
        }

        #endregion

        #region TriggerRecreateSummariesBatchJobAsync

        [Fact]
        public async Task TriggerRecreateSummariesBatchJobAsync_WithValidMonth_ReturnsSuccessResponse()
        {
            // Arrange
            const int month = 6;
            var expectedResponse = new ApiResponseDto<BatchJobEventTriggerDto>
            {
                Success = true,
                Data = new BatchJobEventTriggerDto
                {
                    EventId = "event-abc-123"
                }
            };

            _mockLogApiClient.TriggerRecreateSummariesBatchJobAsync(month).Returns(expectedResponse);

            // Act
            var result = await _service.TriggerRecreateSummariesBatchJobAsync(month);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("event-abc-123", result.Data.EventId);
            await _mockLogApiClient.Received(1).TriggerRecreateSummariesBatchJobAsync(month);
        }

        [Fact]
        public async Task TriggerRecreateSummariesBatchJobAsync_WithFailedResponse_ReturnsFailureResponse()
        {
            // Arrange
            const int month = 0; // invalid month — server returns error
            var expectedResponse = new ApiResponseDto<BatchJobEventTriggerDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto>
                {
                    new() { Code = "INVALID_MONTH", Message = "Month must be between 1 and 12." }
                }
            };

            _mockLogApiClient.TriggerRecreateSummariesBatchJobAsync(month).Returns(expectedResponse);

            // Act
            var result = await _service.TriggerRecreateSummariesBatchJobAsync(month);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("INVALID_MONTH", result.Errors![0].Code);
        }

        [Fact]
        public async Task TriggerRecreateSummariesBatchJobAsync_ApiClientThrowsException_PropagatesException()
        {
            // Arrange
            const int month = 6;

            _mockLogApiClient.TriggerRecreateSummariesBatchJobAsync(month)
                .Returns(Task.FromException<ApiResponseDto<BatchJobEventTriggerDto>>(
                    new InvalidOperationException("API Client error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.TriggerRecreateSummariesBatchJobAsync(month));
        }

        #endregion
    }
}
