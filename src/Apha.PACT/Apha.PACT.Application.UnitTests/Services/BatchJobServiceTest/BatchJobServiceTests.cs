using Apha.Common.Utilities.EventPublisher;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;
using Apha.PACT.Application.Services;
using Apha.PACT.Application.Validation;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using AutoMapper;
using NSubstitute;

namespace Apha.PACT.Application.UnitTests.Services.BatchJobServiceTest
{
    public class BatchJobServiceTests
    {
        private readonly IBatchJobRepository _repository;
        private readonly IRecreateAndReleaseSummaryRepository _releaseRepository;
        private readonly IEventPublisherService _eventPublisher;
        private readonly IMapper _mapper;
        private readonly BatchJobService _service;

        private const string JobName = "RecreateSummary";
        private const string RequestedBy = "user@test.com";
        private const int ValidMonth = 6;
        private const int ValidYear = 2024;

        public BatchJobServiceTests()
        {
            _repository = Substitute.For<IBatchJobRepository>();
            _releaseRepository = Substitute.For<IRecreateAndReleaseSummaryRepository>();
            _eventPublisher = Substitute.For<IEventPublisherService>();
            _mapper = Substitute.For<IMapper>();
            _service = new BatchJobService(_repository, _releaseRepository, _eventPublisher, _mapper);
        }

        #region GetBatchJobsHistoryAsync

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WithResults_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var filter = new PaginationParameters<string>();
            var pagedData = new PagedData<BatchJobHistory>(
                Array.Empty<BatchJobHistory>(), new PaginationData());
            var expectedResult = new PaginatedResult<BatchJobHistoryDto>();

            _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _repository.GetBatchJobsHistoryAsync(filter, JobName).Returns(pagedData);
            _mapper.Map<PaginatedResult<BatchJobHistoryDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _service.GetBatchJobsHistoryAsync(query, JobName);

            // Assert
            Assert.NotNull(result);
            Assert.Same(expectedResult, result);
            _mapper.Received(1).Map<PaginationParameters<string>>(query);
            await _repository.Received(1).GetBatchJobsHistoryAsync(filter, JobName);
            _mapper.Received(1).Map<PaginatedResult<BatchJobHistoryDto>>(pagedData);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var filter = new PaginationParameters<string>();
            _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _repository.GetBatchJobsHistoryAsync(filter, JobName)
                .Returns(Task.FromException<PagedData<BatchJobHistory>>(new InvalidOperationException("DB error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetBatchJobsHistoryAsync(query, JobName));
        }

        #endregion

        #region CanRunBatchJobAsync

        [Fact]
        public async Task CanRunBatchJobAsync_WhenJobCanRun_ReturnsTrue()
        {
            // Arrange
            _repository.CanRunBatchJobAsync(JobName).Returns(true);

            // Act
            var result = await _service.CanRunBatchJobAsync(JobName);

            // Assert
            Assert.True(result);
            await _repository.Received(1).CanRunBatchJobAsync(JobName);
        }

        [Fact]
        public async Task CanRunBatchJobAsync_WhenJobIsRunning_ReturnsFalse()
        {
            // Arrange
            _repository.CanRunBatchJobAsync(JobName).Returns(false);

            // Act
            var result = await _service.CanRunBatchJobAsync(JobName);

            // Assert
            Assert.False(result);
            await _repository.Received(1).CanRunBatchJobAsync(JobName);
        }

        #endregion

        #region TriggerRecreateSummariesJobAsync — validation failures

        [Fact]
        public async Task TriggerRecreateSummariesJobAsync_MonthTooLow_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupReleasePeriods([]);
            _repository.CanRunBatchJobAsync(Arg.Any<string>()).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.TriggerRecreateSummariesJobAsync(0, ValidYear, RequestedBy, Guid.NewGuid().ToString()));

            Assert.Contains(ex.Errors, e => e.Code == "INVALID_MONTH");
        }

        [Fact]
        public async Task TriggerRecreateSummariesJobAsync_MonthTooHigh_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupReleasePeriods([]);
            _repository.CanRunBatchJobAsync(Arg.Any<string>()).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.TriggerRecreateSummariesJobAsync(13, ValidYear, RequestedBy, Guid.NewGuid().ToString()));

            Assert.Contains(ex.Errors, e => e.Code == "INVALID_MONTH");
        }

        [Fact]
        public async Task TriggerRecreateSummariesJobAsync_InvalidYear_Zero_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupReleasePeriods([]);
            _repository.CanRunBatchJobAsync(Arg.Any<string>()).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.TriggerRecreateSummariesJobAsync(ValidMonth, 0, RequestedBy, Guid.NewGuid().ToString()));

            Assert.Contains(ex.Errors, e => e.Code == "INVALID_ContextYear");
        }

        [Fact]
        public async Task TriggerRecreateSummariesJobAsync_InvalidYear_TooLow_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupReleasePeriods([]);
            _repository.CanRunBatchJobAsync(Arg.Any<string>()).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.TriggerRecreateSummariesJobAsync(ValidMonth, 1899, RequestedBy, Guid.NewGuid().ToString()));

            Assert.Contains(ex.Errors, e => e.Code == "INVALID_ContextYear");
        }

        [Fact]
        public async Task TriggerRecreateSummariesJobAsync_EmptyRequestedBy_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupReleasePeriods([]);
            _repository.CanRunBatchJobAsync(Arg.Any<string>()).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.TriggerRecreateSummariesJobAsync(ValidMonth, ValidYear, string.Empty, Guid.NewGuid().ToString()));

            Assert.Contains(ex.Errors, e => e.Code == "INVALID_User");
        }

        [Fact]
        public async Task TriggerRecreateSummariesJobAsync_NullRequestedBy_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupReleasePeriods([]);
            _repository.CanRunBatchJobAsync(Arg.Any<string>()).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.TriggerRecreateSummariesJobAsync(ValidMonth, ValidYear, null!, Guid.NewGuid().ToString()));

            Assert.Contains(ex.Errors, e => e.Code == "INVALID_User");
        }

        [Fact]
        public async Task TriggerRecreateSummariesJobAsync_LaterPeriodAlreadyRun_ThrowsBusinessValidationError()
        {
            // Arrange – a period with FinalSummariesRun == -1 and EndPeriod >= month triggers the rerun guard
            var laterPeriod = new ReleasePeriod { FinalSummariesRun = -1, EndPeriod = ValidMonth + 1 };
            SetupReleasePeriods([laterPeriod]);
            _repository.CanRunBatchJobAsync(Arg.Any<string>()).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.TriggerRecreateSummariesJobAsync(ValidMonth, ValidYear, RequestedBy, Guid.NewGuid().ToString()));

            Assert.Contains(ex.Errors, e => e.Code == "INVALID_Rerun");
        }

        [Fact]
        public async Task TriggerRecreateSummariesJobAsync_JobAlreadyRunning_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupReleasePeriods([]);
            _repository.CanRunBatchJobAsync(Arg.Any<string>()).Returns(false);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.TriggerRecreateSummariesJobAsync(ValidMonth, ValidYear, RequestedBy, Guid.NewGuid().ToString()));

            Assert.Contains(ex.Errors, e => e.Code == "INVALID_Rerun");
        }

        [Fact]
        public async Task TriggerRecreateSummariesJobAsync_MultipleValidationErrors_AllReturned()
        {
            // Arrange – invalid month + empty user → two errors at minimum
            SetupReleasePeriods([]);
            _repository.CanRunBatchJobAsync(Arg.Any<string>()).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _service.TriggerRecreateSummariesJobAsync(0, ValidYear, string.Empty, Guid.NewGuid().ToString()));

            Assert.True(ex.Errors.Count >= 2);
            Assert.Contains(ex.Errors, e => e.Code == "INVALID_MONTH");
            Assert.Contains(ex.Errors, e => e.Code == "INVALID_User");
        }

        #endregion

        #region TriggerRecreateSummariesJobAsync — success path

        [Fact]
        public async Task TriggerRecreateSummariesJobAsync_ValidInput_EnqueuesJobAndPublishesEvent()
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var queueEntry = new BatchJobQueue
            {
                JobqueueId = Guid.NewGuid(),
                JobExecutionId = Guid.Parse(correlationId),
                JobId = 1,
                StatusId = 10,
                RequestedBy = RequestedBy,
                StartDateTime = DateTime.UtcNow,
                FpsYear = ValidYear
            };
            var expectedDto = new BatchJobEventTriggerDto { EventId = "event-123" };

            SetupReleasePeriods([]);
            _repository.CanRunBatchJobAsync(JobName).Returns(true);
            _repository.EnqueueBatchJobAsync(JobName, RequestedBy, correlationId, Arg.Any<string>())
                       .Returns(queueEntry);
            _eventPublisher.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                           .Returns("event-123");
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(expectedDto);

            // Act
            var result = await _service.TriggerRecreateSummariesJobAsync(ValidMonth, ValidYear, RequestedBy, correlationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("event-123", result.EventId);

            await _repository.Received(1).EnqueueBatchJobAsync(
                JobName, RequestedBy, correlationId, Arg.Any<string>());
            await _eventPublisher.Received(1).PublishAsync(
                Arg.Any<EventDetail>(), Arg.Any<CancellationToken>());
            _mapper.Received(1).Map<BatchJobEventTriggerDto>(queueEntry);
        }

        [Fact]
        public async Task TriggerRecreateSummariesJobAsync_ValidInput_NoteContainsJobNameAndMonth()
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), RequestedBy = RequestedBy, StartDateTime = DateTime.UtcNow };
            string? capturedNote = null;

            SetupReleasePeriods([]);
            _repository.CanRunBatchJobAsync(JobName).Returns(true);
            _repository.EnqueueBatchJobAsync(JobName, RequestedBy, correlationId, Arg.Do<string>(n => capturedNote = n))
                       .Returns(queueEntry);
            _eventPublisher.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>()).Returns("ev-1");
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(new BatchJobEventTriggerDto());

            // Act
            await _service.TriggerRecreateSummariesJobAsync(ValidMonth, ValidYear, RequestedBy, correlationId);

            // Assert
            Assert.NotNull(capturedNote);
            Assert.Contains(JobName, capturedNote);
            Assert.Contains(ValidMonth.ToString(), capturedNote);
        }

        [Fact]
        public async Task TriggerRecreateSummariesJobAsync_ValidInput_EventIdAssignedFromPublisher()
        {
            // Arrange
            const string publishedEventId = "evt-abc-123";
            var correlationId = Guid.NewGuid().ToString();
            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), RequestedBy = RequestedBy, StartDateTime = DateTime.UtcNow };
            var dto = new BatchJobEventTriggerDto { EventId = string.Empty };

            SetupReleasePeriods([]);
            _repository.CanRunBatchJobAsync(JobName).Returns(true);
            _repository.EnqueueBatchJobAsync(JobName, RequestedBy, correlationId, Arg.Any<string>()).Returns(queueEntry);
            _eventPublisher.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>()).Returns(publishedEventId);
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(dto);

            // Act
            var result = await _service.TriggerRecreateSummariesJobAsync(ValidMonth, ValidYear, RequestedBy, correlationId);

            // Assert – EventId is set from the publisher response, not the initial DTO state
            Assert.Equal(publishedEventId, result.EventId);
        }

        [Fact]
        public async Task TriggerRecreateSummariesJobAsync_ValidInput_PeriodExistsButNotFinalRun_AllowsTrigger()
        {
            // Arrange – period with FinalSummariesRun != -1 should NOT block execution
            var period = new ReleasePeriod { FinalSummariesRun = 0, EndPeriod = ValidMonth + 2 };
            SetupReleasePeriods([period]);
            var correlationId = Guid.NewGuid().ToString();
            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), RequestedBy = RequestedBy, StartDateTime = DateTime.UtcNow };

            _repository.CanRunBatchJobAsync(JobName).Returns(true);
            _repository.EnqueueBatchJobAsync(JobName, RequestedBy, correlationId, Arg.Any<string>()).Returns(queueEntry);
            _eventPublisher.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>()).Returns("ev");
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(new BatchJobEventTriggerDto { EventId = "ev" });

            // Act – should not throw
            var result = await _service.TriggerRecreateSummariesJobAsync(ValidMonth, ValidYear, RequestedBy, correlationId);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task TriggerRecreateSummariesJobAsync_EventPublisherThrows_PropagatesException()
        {
            // Arrange
            var correlationId = Guid.NewGuid().ToString();
            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), RequestedBy = RequestedBy, StartDateTime = DateTime.UtcNow };

            SetupReleasePeriods([]);
            _repository.CanRunBatchJobAsync(JobName).Returns(true);
            _repository.EnqueueBatchJobAsync(JobName, RequestedBy, correlationId, Arg.Any<string>()).Returns(queueEntry);
            _eventPublisher.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                           .Returns(Task.FromException<string>(new InvalidOperationException("Publish failed")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.TriggerRecreateSummariesJobAsync(ValidMonth, ValidYear, RequestedBy, correlationId));
        }

        #endregion

        // ── helpers ──────────────────────────────────────────────────────────

        private void SetupReleasePeriods(IList<ReleasePeriod> periods)
        {
            _releaseRepository.GetReleasePeriodsAsync().Returns(periods);
        }
    }
}
