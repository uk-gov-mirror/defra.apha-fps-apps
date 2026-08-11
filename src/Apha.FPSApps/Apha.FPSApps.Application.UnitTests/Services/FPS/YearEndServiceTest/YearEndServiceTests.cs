using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.YearEndServiceTest
{
    public class YearEndServiceTests
    {
        private const string JobName = "YearEnd-DataSetup";
        private const int PlannedYear = 2025;

        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsYearEndApiClient _fpsYearEndApiClient;
        private readonly YearEndService _sut;

        public YearEndServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsYearEndApiClient = Substitute.For<IFpsYearEndApiClient>();
            _fpsClient.FpsYearEnd.Returns(_fpsYearEndApiClient);
            _sut = new YearEndService(_fpsClient);
        }

        // -----------------------------------------------------------------------
        // GetYearEndDataSetupBatchJobHistoryAsync
        // -----------------------------------------------------------------------

        #region GetYearEndDataSetupBatchJobHistoryAsync

        [Fact]
        public async Task GetYearEndDataSetupBatchJobHistoryAsync_WhenApiReturnsSuccess_ReturnsPaginatedHistory()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var history = new List<BatchJobHistoryDto>
            {
                new BatchJobHistoryDto { JobId = 1, JobName = JobName, Status = "Completed", RequestedBy = "user@test.com" },
                new BatchJobHistoryDto { JobId = 1, JobName = JobName, Status = "Failed",    RequestedBy = "user@test.com" }
            };
            var paginated = new PaginatedResult<BatchJobHistoryDto>(history, 2);
            var expectedResponse = ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>.SuccessResponse(paginated);
            _fpsYearEndApiClient.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName).Returns(expectedResponse);

            // Act
            var result = await _sut.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.TotalCount);
            await _fpsYearEndApiClient.Received(1).GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);
        }

        [Fact]
        public async Task GetYearEndDataSetupBatchJobHistoryAsync_WhenApiReturnsEmptyResult_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginated = new PaginatedResult<BatchJobHistoryDto>(new List<BatchJobHistoryDto>(), 0);
            var expectedResponse = ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>.SuccessResponse(paginated);
            _fpsYearEndApiClient.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName).Returns(expectedResponse);

            // Act
            var result = await _sut.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!.data);
            await _fpsYearEndApiClient.Received(1).GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);
        }

        [Fact]
        public async Task GetYearEndDataSetupBatchJobHistoryAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Unauthorized", Code = "UNAUTHORIZED" } };
            var expectedResponse = ApiResponseDto<PaginatedResult<BatchJobHistoryDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsYearEndApiClient.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName).Returns(expectedResponse);

            // Act
            var result = await _sut.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _fpsYearEndApiClient.Received(1).GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);
        }

        [Fact]
        public async Task GetYearEndDataSetupBatchJobHistoryAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _fpsYearEndApiClient.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName)
                .ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName));
            Assert.Equal("API unavailable", exception.Message);
            await _fpsYearEndApiClient.Received(1).GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);
        }

        #endregion

        // -----------------------------------------------------------------------
        // GetCanInitiateDataSetupRequestAsync
        // -----------------------------------------------------------------------

        #region GetCanInitiateDataSetupRequestAsync

        [Fact]
        public async Task GetCanInitiateDataSetupRequestAsync_WhenApiReturnsTrue_ReturnsSuccessWithTrue()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsYearEndApiClient.GetCanInitiateDataSetupRequestAsync(JobName).Returns(expectedResponse);

            // Act
            var result = await _sut.GetCanInitiateDataSetupRequestAsync(JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsYearEndApiClient.Received(1).GetCanInitiateDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task GetCanInitiateDataSetupRequestAsync_WhenApiReturnsFalse_ReturnsSuccessWithFalse()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(false);
            _fpsYearEndApiClient.GetCanInitiateDataSetupRequestAsync(JobName).Returns(expectedResponse);

            // Act
            var result = await _sut.GetCanInitiateDataSetupRequestAsync(JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            await _fpsYearEndApiClient.Received(1).GetCanInitiateDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task GetCanInitiateDataSetupRequestAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Service error", Code = "SERVICE_ERROR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsYearEndApiClient.GetCanInitiateDataSetupRequestAsync(JobName).Returns(expectedResponse);

            // Act
            var result = await _sut.GetCanInitiateDataSetupRequestAsync(JobName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _fpsYearEndApiClient.Received(1).GetCanInitiateDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task GetCanInitiateDataSetupRequestAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _fpsYearEndApiClient.GetCanInitiateDataSetupRequestAsync(JobName)
                .ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.GetCanInitiateDataSetupRequestAsync(JobName));
            Assert.Equal("API unavailable", exception.Message);
            await _fpsYearEndApiClient.Received(1).GetCanInitiateDataSetupRequestAsync(JobName);
        }

        #endregion

        // -----------------------------------------------------------------------
        // GetCanApproveDataSetupRequestAsync
        // -----------------------------------------------------------------------

        #region GetCanApproveDataSetupRequestAsync

        [Fact]
        public async Task GetCanApproveDataSetupRequestAsync_WhenApiReturnsTrue_ReturnsSuccessWithTrue()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsYearEndApiClient.GetCanApproveOrRejectDataSetupRequestAsync(JobName).Returns(expectedResponse);

            // Act
            var result = await _sut.GetCanApproveOrRejectDataSetupRequestAsync(JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsYearEndApiClient.Received(1).GetCanApproveOrRejectDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task GetCanApproveDataSetupRequestAsync_WhenApiReturnsFalse_ReturnsSuccessWithFalse()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(false);
            _fpsYearEndApiClient.GetCanApproveOrRejectDataSetupRequestAsync(JobName).Returns(expectedResponse);

            // Act
            var result = await _sut.GetCanApproveOrRejectDataSetupRequestAsync(JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            await _fpsYearEndApiClient.Received(1).GetCanApproveOrRejectDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task GetCanApproveDataSetupRequestAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Forbidden", Code = "FORBIDDEN" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsYearEndApiClient.GetCanApproveOrRejectDataSetupRequestAsync(JobName).Returns(expectedResponse);

            // Act
            var result = await _sut.GetCanApproveOrRejectDataSetupRequestAsync(JobName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _fpsYearEndApiClient.Received(1).GetCanApproveOrRejectDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task GetCanApproveDataSetupRequestAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _fpsYearEndApiClient.GetCanApproveOrRejectDataSetupRequestAsync(JobName)
                .ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.GetCanApproveOrRejectDataSetupRequestAsync(JobName));
            Assert.Equal("API unavailable", exception.Message);
            await _fpsYearEndApiClient.Received(1).GetCanApproveOrRejectDataSetupRequestAsync(JobName);
        }

        #endregion

        // -----------------------------------------------------------------------
        // EnqueueYearEndDataSetupInitiationJobAsync
        // -----------------------------------------------------------------------

        #region EnqueueYearEndDataSetupInitiationJobAsync

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenApiReturnsSuccess_ReturnsBatchJobQueueDto()
        {
            // Arrange
            var queued = new BatchJobQueueDto
            {
                JobId = 1,
                RequestedBy = "user@test.com",
                RequestedAtUtc = DateTime.UtcNow
            };
            var expectedResponse = ApiResponseDto<BatchJobQueueDto>.SuccessResponse(queued);
            _fpsYearEndApiClient.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear).Returns(expectedResponse);

            // Act
            var result = await _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1, result.Data?.JobId);
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Job already running", Code = "CONFLICT" }
            };
            var expectedResponse = ApiResponseDto<BatchJobQueueDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsYearEndApiClient.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear).Returns(expectedResponse);

            // Act
            var result = await _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear);
        }

        [Theory]
        [InlineData(2025)]
        [InlineData(2026)]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_PassesPlannedYearToApiClient(int plannedYear)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<BatchJobQueueDto>.SuccessResponse(new BatchJobQueueDto());
            _fpsYearEndApiClient.EnqueueYearEndDataSetupInitiationJobAsync(plannedYear).Returns(expectedResponse);

            // Act
            await _sut.EnqueueYearEndDataSetupInitiationJobAsync(plannedYear);

            // Assert
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndDataSetupInitiationJobAsync(plannedYear);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _fpsYearEndApiClient.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear)
                .ThrowsAsync(new Exception("Enqueue failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear));
            Assert.Equal("Enqueue failed", exception.Message);
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear);
        }

        #endregion

        // -----------------------------------------------------------------------
        // TriggerYearEndDataSetupApprovalJobAsync
        // -----------------------------------------------------------------------

        #region TriggerYearEndDataSetupApprovalJobAsync

        [Fact]
        public async Task TriggerYearEndDataSetupApprovalJobAsync_WhenApiReturnsSuccess_ReturnsBatchJobEventTriggerDto()
        {
            // Arrange
            var eventTrigger = new BatchJobEventTriggerDto
            {
                EventId = "evt-001",
                Jobqueue = new BatchJobQueueDto { JobId = 1, RequestedBy = "approver@test.com" }
            };
            var expectedResponse = ApiResponseDto<BatchJobEventTriggerDto>.SuccessResponse(eventTrigger);
            _fpsYearEndApiClient.TriggerYearEndDataSetupApprovalJobAsync(PlannedYear).Returns(expectedResponse);

            // Act
            var result = await _sut.TriggerYearEndDataSetupApprovalJobAsync(PlannedYear);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("evt-001", result.Data?.EventId);
            await _fpsYearEndApiClient.Received(1).TriggerYearEndDataSetupApprovalJobAsync(PlannedYear);
        }

        [Fact]
        public async Task TriggerYearEndDataSetupApprovalJobAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Approval not allowed", Code = "VALIDATION_ERROR" }
            };
            var expectedResponse = ApiResponseDto<BatchJobEventTriggerDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsYearEndApiClient.TriggerYearEndDataSetupApprovalJobAsync(PlannedYear).Returns(expectedResponse);

            // Act
            var result = await _sut.TriggerYearEndDataSetupApprovalJobAsync(PlannedYear);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _fpsYearEndApiClient.Received(1).TriggerYearEndDataSetupApprovalJobAsync(PlannedYear);
        }

        [Theory]
        [InlineData(2025)]
        [InlineData(2026)]
        public async Task TriggerYearEndDataSetupApprovalJobAsync_PassesPlannedYearToApiClient(int plannedYear)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<BatchJobEventTriggerDto>.SuccessResponse(
                new BatchJobEventTriggerDto { Jobqueue = new BatchJobQueueDto() });
            _fpsYearEndApiClient.TriggerYearEndDataSetupApprovalJobAsync(plannedYear).Returns(expectedResponse);

            // Act
            await _sut.TriggerYearEndDataSetupApprovalJobAsync(plannedYear);

            // Assert
            await _fpsYearEndApiClient.Received(1).TriggerYearEndDataSetupApprovalJobAsync(plannedYear);
        }

        [Fact]
        public async Task TriggerYearEndDataSetupApprovalJobAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _fpsYearEndApiClient.TriggerYearEndDataSetupApprovalJobAsync(PlannedYear)
                .ThrowsAsync(new Exception("Approval failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.TriggerYearEndDataSetupApprovalJobAsync(PlannedYear));
            Assert.Equal("Approval failed", exception.Message);
            await _fpsYearEndApiClient.Received(1).TriggerYearEndDataSetupApprovalJobAsync(PlannedYear);
        }

        #endregion

        // -----------------------------------------------------------------------
        // EnqueueYearEndDataSetupRejectJobAsync
        // -----------------------------------------------------------------------

        #region EnqueueYearEndDataSetupRejectJobAsync

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenApiReturnsSuccess_ReturnsSuccessResponseWithTrue()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsYearEndApiClient.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear).Returns(expectedResponse);

            // Act
            var result = await _sut.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndDataSetupRejectJobAsync(PlannedYear);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Rejection not allowed", Code = "VALIDATION_ERROR" }
            };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsYearEndApiClient.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear).Returns(expectedResponse);

            // Act
            var result = await _sut.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndDataSetupRejectJobAsync(PlannedYear);
        }

        [Theory]
        [InlineData(2025)]
        [InlineData(2026)]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_PassesPlannedYearToApiClient(int plannedYear)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsYearEndApiClient.EnqueueYearEndDataSetupRejectJobAsync(plannedYear).Returns(expectedResponse);

            // Act
            await _sut.EnqueueYearEndDataSetupRejectJobAsync(plannedYear);

            // Assert
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndDataSetupRejectJobAsync(plannedYear);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenApiClientThrowsException_PropagatesException()
        {
            // Arrange
            _fpsYearEndApiClient.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear)
                .ThrowsAsync(new Exception("Reject failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear));
            Assert.Equal("Reject failed", exception.Message);
            await _fpsYearEndApiClient.Received(1).EnqueueYearEndDataSetupRejectJobAsync(PlannedYear);
        }

        #endregion
    }
}
