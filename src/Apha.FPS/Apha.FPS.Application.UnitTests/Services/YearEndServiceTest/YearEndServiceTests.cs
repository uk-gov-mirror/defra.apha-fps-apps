using Apha.Common.Contracts.Email;
using Apha.Common.Utilities.Email;
using Apha.Common.Utilities.EventPublisher;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.YearEndServiceTest
{
    public class YearEndServiceTests
    {
        private const string JobName = "YearEnd-DataSetup";
        private const int PlannedYear = 2025;
        private const int ContextYear = 2024;
        private const string RequestedBy = "user@example.com";
        private const string CorrelationId = "corr-001";

        private readonly IYearEndRepository _yearEndRepository;
        private readonly IFpsSettingRepository _fpsSettingRepository;
        private readonly IMonthHourRepository _monthHourRepository;
        private readonly IYearMasterRepository _yearMasterRepository;
        private readonly IEventPublisherService _eventPublisherService;
        private readonly IGraphEmailService _emailService;
        private readonly ILogger<YearEndService> _logger;
        private readonly IMapper _mapper;
        private readonly IOptions<YearEndEmailSettings> _emailSettings;
        private readonly YearEndService _sut;

        public YearEndServiceTests()
        {
            _yearEndRepository = Substitute.For<IYearEndRepository>();
            _fpsSettingRepository = Substitute.For<IFpsSettingRepository>();
            _monthHourRepository = Substitute.For<IMonthHourRepository>();
            _yearMasterRepository = Substitute.For<IYearMasterRepository>();
            _eventPublisherService = Substitute.For<IEventPublisherService>();
            _emailService = Substitute.For<IGraphEmailService>();
            _logger = Substitute.For<ILogger<YearEndService>>();
            _mapper = Substitute.For<IMapper>();
            _emailSettings = Options.Create(new YearEndEmailSettings
            {
                DataSetupInitiatedEmailRecipient = "initiation@example.com",
                DataSetupInitiatedEmailSubject = "Year End Initiated",
                DataSetupInitiatedEmailBody = "Initiation body",
                DataSetupApprovalEmailRecipient = "approval@example.com",
                DataSetupApprovalEmailSubject = "Year End Approved",
                DataSetupApprovalEmailBody = "Approval body"
            });

            _sut = new YearEndService(
                _yearEndRepository,
                _fpsSettingRepository,
                _monthHourRepository,
                _yearMasterRepository,
                _eventPublisherService,
                _emailService,
                _emailSettings,
                _logger,
                _mapper);
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static List<YearEndFpsSetting> ValidSettings() =>
        [
            new YearEndFpsSetting { Id = "HoursInDay", Setting = "8", ExistsForPlannedYear = "Yes" },
            new YearEndFpsSetting { Id = "CapApprovalReceivedForReset", Setting = "yes", ExistsForPlannedYear = "Yes" }
        ];

        private static List<YearEndMonthHour> ValidMonthHours() =>
            Enumerable.Range(1, 12)
                .Select(m => new YearEndMonthHour
                {
                    Month = (short)m,
                    Days = 20,
                    VidHours = 5,
                    CvlHours = 3,
                    ExistsForPlannedYear = "Yes"
                })
                .ToList();

        private void SetupValidConfiguration()
        {
            _fpsSettingRepository.GetYearEndSettingsAsync().Returns(ValidSettings());
            _monthHourRepository.GetYearEndMonthHoursAsync().Returns(ValidMonthHours());
        }

        private void SetupYearMasterNotFound() =>
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear).Returns((YearMaster?)null);

        // -----------------------------------------------------------------------
        // GetBatchJobsHistoryAsync
        // -----------------------------------------------------------------------

        #region GetBatchJobsHistoryAsync

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WhenDataExists_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<BatchJobHistory>
            {
                Data = [new BatchJobHistory { JobId = 1, JobName = JobName, Status = "Completed" }],
                PaginationData = new PaginationData { TotalRecords = 1, PageNumber = 1, PageSize = 10 }
            };
            var expectedResult = new PaginatedResult<BatchJobHistoryDto>
            {
                Data = [new BatchJobHistoryDto { JobId = 1, JobName = JobName, Status = "Completed" }]
            };

            _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _yearEndRepository.GetBatchJobsHistoryAsync(filter, JobName).Returns(pagedData);
            _mapper.Map<PaginatedResult<BatchJobHistoryDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetBatchJobsHistoryAsync(query, JobName);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().ContainSingle();
            result.Data.First().JobId.Should().Be(1);

            await _yearEndRepository.Received(1).GetBatchJobsHistoryAsync(filter, JobName);
            _mapper.Received(1).Map<PaginatedResult<BatchJobHistoryDto>>(pagedData);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WhenNoData_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var filter = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new PagedData<BatchJobHistory> { Data = [], PaginationData = new PaginationData { TotalRecords = 0, PageNumber = 1, PageSize = 10 } };
            var expectedResult = new PaginatedResult<BatchJobHistoryDto> { Data = [] };

            _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _yearEndRepository.GetBatchJobsHistoryAsync(filter, JobName).Returns(pagedData);
            _mapper.Map<PaginatedResult<BatchJobHistoryDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetBatchJobsHistoryAsync(query, JobName);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();

            await _yearEndRepository.Received(1).GetBatchJobsHistoryAsync(filter, JobName);
        }

        [Fact]
        public async Task GetBatchJobsHistoryAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new PaginationParameters<string>();
            _mapper.Map<PaginationParameters<string>>(query).Returns(filter);
            _yearEndRepository.GetBatchJobsHistoryAsync(filter, JobName)
                .Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.GetBatchJobsHistoryAsync(query, JobName));
        }

        #endregion

        // -----------------------------------------------------------------------
        // CanInitiateYearEndDataSetupRequestAsync
        // -----------------------------------------------------------------------

        #region CanInitiateYearEndDataSetupRequestAsync

        [Fact]
        public async Task CanInitiateYearEndDataSetupRequestAsync_WhenRepositoryReturnsTrue_ReturnsTrue()
        {
            // Arrange
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act
            var result = await _sut.CanInitiateYearEndDataSetupRequestAsync(JobName);

            // Assert
            result.Should().BeTrue();
            await _yearEndRepository.Received(1).CanInitiateYearEndDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanInitiateYearEndDataSetupRequestAsync_WhenRepositoryReturnsFalse_ReturnsFalse()
        {
            // Arrange
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(false);

            // Act
            var result = await _sut.CanInitiateYearEndDataSetupRequestAsync(JobName);

            // Assert
            result.Should().BeFalse();
            await _yearEndRepository.Received(1).CanInitiateYearEndDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanInitiateYearEndDataSetupRequestAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName)
                .Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.CanInitiateYearEndDataSetupRequestAsync(JobName));
        }

        #endregion

        // -----------------------------------------------------------------------
        // CanApproveYearEndDataSetupRequestAsync
        // -----------------------------------------------------------------------

        #region CanApproveYearEndDataSetupRequestAsync

        [Fact]
        public async Task CanApproveYearEndDataSetupRequestAsync_WhenRepositoryReturnsTrue_ReturnsTrue()
        {
            // Arrange
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act
            var result = await _sut.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName);

            // Assert
            result.Should().BeTrue();
            await _yearEndRepository.Received(1).CanApproveOrRejectYearEndDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanApproveYearEndDataSetupRequestAsync_WhenRepositoryReturnsFalse_ReturnsFalse()
        {
            // Arrange
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(false);

            // Act
            var result = await _sut.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName);

            // Assert
            result.Should().BeFalse();
            await _yearEndRepository.Received(1).CanApproveOrRejectYearEndDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanApproveYearEndDataSetupRequestAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName)
                .Throws(new Exception("Repository error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _sut.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName));
        }

        #endregion

        // -----------------------------------------------------------------------
        // EnqueueYearEndDataSetupInitiationJobAsync — validation
        // -----------------------------------------------------------------------

        #region EnqueueYearEndDataSetupInitiationJobAsync — validation

        [Theory]
        [InlineData(0)]
        [InlineData(1899)]
        [InlineData(10000)]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenPlannedYearIsInvalid_ThrowsBusinessValidationError(int invalidYear)
        {
            // Arrange
            SetupValidConfiguration();
            _yearMasterRepository.GetFpsYearByIdAsync(invalidYear).Returns((YearMaster?)null);
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupInitiationJobAsync(invalidYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_PlannedYear");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenRequestedByIsEmpty_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupValidConfiguration();
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear).Returns((YearMaster?)null);
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, string.Empty, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_User");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenPlannedYearAlreadyCompleted_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupValidConfiguration();
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear)
                .Returns(new YearMaster { FpsYear = PlannedYear, YearStatus = "planned", Active = true });
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Rerun");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenJobIsAlreadyRunning_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupValidConfiguration();
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear).Returns((YearMaster?)null);
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(false);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Initiation");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenSettingsAreMissingForPlannedYear_ThrowsBusinessValidationError()
        {
            // Arrange
            var missingSettings = new List<YearEndFpsSetting>
            {
                new YearEndFpsSetting { Id = "HoursInDay", Setting = "8", ExistsForPlannedYear = "No" }
            };
            _fpsSettingRepository.GetYearEndSettingsAsync().Returns(missingSettings);
            _monthHourRepository.GetYearEndMonthHoursAsync().Returns(ValidMonthHours());
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear).Returns((YearMaster?)null);
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "Missing_Config");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenHoursInDayIsInvalid_ThrowsBusinessValidationError()
        {
            // Arrange
            var settings = new List<YearEndFpsSetting>
            {
                new YearEndFpsSetting { Id = "HoursInDay", Setting = "invalid", ExistsForPlannedYear = "Yes" },
                new YearEndFpsSetting { Id = "CapApprovalReceivedForReset", Setting = "yes", ExistsForPlannedYear = "Yes" }
            };
            _fpsSettingRepository.GetYearEndSettingsAsync().Returns(settings);
            _monthHourRepository.GetYearEndMonthHoursAsync().Returns(ValidMonthHours());
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear).Returns((YearMaster?)null);
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "Missing_HoursInDay");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenCapApprovalValueIsInvalid_ThrowsBusinessValidationError()
        {
            // Arrange
            var settings = new List<YearEndFpsSetting>
            {
                new YearEndFpsSetting { Id = "HoursInDay", Setting = "8", ExistsForPlannedYear = "Yes" },
                new YearEndFpsSetting { Id = "CapApprovalReceivedForReset", Setting = "maybe", ExistsForPlannedYear = "Yes" }
            };
            _fpsSettingRepository.GetYearEndSettingsAsync().Returns(settings);
            _monthHourRepository.GetYearEndMonthHoursAsync().Returns(ValidMonthHours());
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear).Returns((YearMaster?)null);
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "Missing_CapApprovalReceivedForReset");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenMonthConfigIsMissingForPlannedYear_ThrowsBusinessValidationError()
        {
            // Arrange
            _fpsSettingRepository.GetYearEndSettingsAsync().Returns(ValidSettings());
            var monthHoursWithMissing = new List<YearEndMonthHour>
            {
                new YearEndMonthHour { Month = 1, Days = 20, VidHours = 5, CvlHours = 3, ExistsForPlannedYear = "No" }
            };
            _monthHourRepository.GetYearEndMonthHoursAsync().Returns(monthHoursWithMissing);
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear).Returns((YearMaster?)null);
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "Missing_Config");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenMonthHoursHaveNegativeValues_ThrowsBusinessValidationError()
        {
            // Arrange
            _fpsSettingRepository.GetYearEndSettingsAsync().Returns(ValidSettings());
            var invalidMonthHours = new List<YearEndMonthHour>
            {
                new YearEndMonthHour { Month = 1, Days = -1, VidHours = 5, CvlHours = 3, ExistsForPlannedYear = "Yes" }
            };
            _monthHourRepository.GetYearEndMonthHoursAsync().Returns(invalidMonthHours);
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear).Returns((YearMaster?)null);
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "Missing_Config");
        }

        #endregion

        // -----------------------------------------------------------------------
        // EnqueueYearEndDataSetupInitiationJobAsync — success
        // -----------------------------------------------------------------------

        #region EnqueueYearEndDataSetupInitiationJobAsync — success

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenAllValid_ReturnsQueueDto()
        {
            // Arrange
            SetupValidConfiguration();
            SetupYearMasterNotFound();
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueDataSetupInitiationBatchJobAsync(JobName, RequestedBy, CorrelationId, Arg.Any<string>())
                .Returns(queueEntry);

            var expectedDto = new BatchJobQueueDto { RequestedBy = RequestedBy };
            _mapper.Map<BatchJobQueueDto>(queueEntry).Returns(expectedDto);

            // Act
            var result = await _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            result.Should().NotBeNull();
            result.RequestedBy.Should().Be(RequestedBy);

            await _yearEndRepository.Received(1).EnqueueDataSetupInitiationBatchJobAsync(
                JobName, RequestedBy, CorrelationId, Arg.Any<string>());
            await _emailService.Received(1).SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenEmailFails_StillReturnsQueueDto()
        {
            // Arrange — email failure must be swallowed, not propagated
            SetupValidConfiguration();
            SetupYearMasterNotFound();
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueDataSetupInitiationBatchJobAsync(JobName, RequestedBy, CorrelationId, Arg.Any<string>())
                .Returns(queueEntry);

            var expectedDto = new BatchJobQueueDto { RequestedBy = RequestedBy };
            _mapper.Map<BatchJobQueueDto>(queueEntry).Returns(expectedDto);

            _emailService.SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>())
                .Throws(new Exception("SMTP failure"));

            // Act
            var result = await _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert — no exception propagated; dto still returned
            result.Should().NotBeNull();
            result.RequestedBy.Should().Be(RequestedBy);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenAllValid_SendsInitiationEmail()
        {
            // Arrange
            SetupValidConfiguration();
            SetupYearMasterNotFound();
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueDataSetupInitiationBatchJobAsync(JobName, RequestedBy, CorrelationId, Arg.Any<string>())
                .Returns(queueEntry);
            _mapper.Map<BatchJobQueueDto>(queueEntry).Returns(new BatchJobQueueDto());

            // Act
            await _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert — email sent with recipient from settings
            await _emailService.Received(1).SendEmailAsync(
                Arg.Is<EmailMessageModel>(m => m.To.Contains("initiation@example.com")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenAllValid_CallsEnqueueOnRepository()
        {
            // Arrange
            SetupValidConfiguration();
            SetupYearMasterNotFound();
            _yearEndRepository.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid() };
            _yearEndRepository.EnqueueDataSetupInitiationBatchJobAsync(JobName, RequestedBy, CorrelationId, Arg.Any<string>())
                .Returns(queueEntry);
            _mapper.Map<BatchJobQueueDto>(queueEntry).Returns(new BatchJobQueueDto());

            // Act
            await _sut.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            await _yearEndRepository.Received(1).EnqueueDataSetupInitiationBatchJobAsync(
                JobName, RequestedBy, CorrelationId, Arg.Any<string>());
        }

        #endregion

        // -----------------------------------------------------------------------
        // EnqueueYearEndDataSetupApprovalJobAsync — validation
        // -----------------------------------------------------------------------

        #region EnqueueYearEndDataSetupApprovalJobAsync — validation

        [Theory]
        [InlineData(0)]
        [InlineData(1899)]
        [InlineData(10000)]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenPlannedYearIsInvalid_ThrowsBusinessValidationError(int invalidYear)
        {
            // Arrange
            SetupValidConfiguration();
            _yearMasterRepository.GetFpsYearByIdAsync(invalidYear).Returns((YearMaster?)null);
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJobAsync(invalidYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_PlannedYear");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenRequestedByIsEmpty_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupValidConfiguration();
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear).Returns((YearMaster?)null);
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJobAsync(PlannedYear, ContextYear, string.Empty, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_User");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenPlannedYearAlreadyCompleted_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupValidConfiguration();
            _yearMasterRepository.GetFpsYearByIdAsync(PlannedYear)
                .Returns(new YearMaster { FpsYear = PlannedYear, YearStatus = "close", Active = true });
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Rerun");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenNoInitiatedRequestExists_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupValidConfiguration();
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(false);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Approval");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenInitiatorAndApproverAreSamePerson_ThrowsBusinessValidationError()
        {
            // Arrange
            SetupValidConfiguration();
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(RequestedBy);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Approval");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenSettingsAreMissingForPlannedYear_ThrowsBusinessValidationError()
        {
            // Arrange
            var missingSettings = new List<YearEndFpsSetting>
            {
                new YearEndFpsSetting { Id = "HoursInDay", Setting = "8", ExistsForPlannedYear = "No" }
            };
            _fpsSettingRepository.GetYearEndSettingsAsync().Returns(missingSettings);
            _monthHourRepository.GetYearEndMonthHoursAsync().Returns(ValidMonthHours());
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "Missing_Config");
        }

        #endregion

        // -----------------------------------------------------------------------
        // EnqueueYearEndDataSetupApprovalJobAsync — success
        // -----------------------------------------------------------------------

        #region EnqueueYearEndDataSetupApprovalJobAsync — success

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenAllValid_ReturnsEventTriggerDto()
        {
            // Arrange
            SetupValidConfiguration();
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueDataSetupApprovalBatchJobAsync(JobName, RequestedBy, CorrelationId, Arg.Any<string>())
                .Returns(queueEntry);

            _eventPublisherService.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                .Returns("evt-123");

            var triggerDto = new BatchJobEventTriggerDto { EventId = "evt-123" };
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(triggerDto);
            _mapper.Map<BatchJobEventTriggerDto>(triggerDto).Returns(triggerDto);

            // Act
            var result = await _sut.EnqueueYearEndDataSetupApprovalJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            result.Should().NotBeNull();
            result.EventId.Should().Be("evt-123");

            await _yearEndRepository.Received(1).EnqueueDataSetupApprovalBatchJobAsync(
                JobName, RequestedBy, CorrelationId, Arg.Any<string>());
            await _eventPublisherService.Received(1).PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenAllValid_SendsApprovalEmail()
        {
            // Arrange
            SetupValidConfiguration();
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid() };
            _yearEndRepository.EnqueueDataSetupApprovalBatchJobAsync(JobName, RequestedBy, CorrelationId, Arg.Any<string>())
                .Returns(queueEntry);
            _eventPublisherService.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                .Returns("evt-456");

            var triggerDto = new BatchJobEventTriggerDto();
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(triggerDto);
            _mapper.Map<BatchJobEventTriggerDto>(triggerDto).Returns(triggerDto);

            // Act
            await _sut.EnqueueYearEndDataSetupApprovalJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert — email sent to approval recipient from settings
            await _emailService.Received(1).SendEmailAsync(
                Arg.Is<EmailMessageModel>(m => m.To.Contains("approval@example.com")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenEmailFails_StillReturnsEventTriggerDto()
        {
            // Arrange — email failure must be swallowed, not propagated
            SetupValidConfiguration();
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid() };
            _yearEndRepository.EnqueueDataSetupApprovalBatchJobAsync(JobName, RequestedBy, CorrelationId, Arg.Any<string>())
                .Returns(queueEntry);
            _eventPublisherService.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                .Returns("evt-789");

            var triggerDto = new BatchJobEventTriggerDto { EventId = "evt-789" };
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(triggerDto);
            _mapper.Map<BatchJobEventTriggerDto>(triggerDto).Returns(triggerDto);

            _emailService.SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>())
                .Throws(new Exception("SMTP failure"));

            // Act
            var result = await _sut.EnqueueYearEndDataSetupApprovalJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert — no exception propagated; dto still returned
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJobAsync_WhenAllValid_PublishesEventWithCorrectJobName()
        {
            // Arrange
            SetupValidConfiguration();
            SetupYearMasterNotFound();
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid() };
            _yearEndRepository.EnqueueDataSetupApprovalBatchJobAsync(JobName, RequestedBy, CorrelationId, Arg.Any<string>())
                .Returns(queueEntry);
            _eventPublisherService.PublishAsync(Arg.Any<EventDetail>(), Arg.Any<CancellationToken>())
                .Returns("evt-999");

            var triggerDto = new BatchJobEventTriggerDto();
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(triggerDto);
            _mapper.Map<BatchJobEventTriggerDto>(triggerDto).Returns(triggerDto);

            // Act
            await _sut.EnqueueYearEndDataSetupApprovalJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert — event published with the correct job name
            await _eventPublisherService.Received(1).PublishAsync(
                Arg.Is<EventDetail>(e => e.JobName == JobName && e.RequestedBy == RequestedBy),
                Arg.Any<CancellationToken>());
        }

        #endregion

        // -----------------------------------------------------------------------
        // EnqueueYearEndDataSetupRejectJobAsync — validation
        // -----------------------------------------------------------------------

        #region EnqueueYearEndDataSetupRejectJobAsync — validation

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenRequestedByIsEmpty_ThrowsBusinessValidationError()
        {
            // Arrange
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear, ContextYear, string.Empty, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_User");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenNoInitiatedRequestExists_ThrowsBusinessValidationError()
        {
            // Arrange
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(false);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(string.Empty);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Approval");
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenInitiatorAndRejectorAreSamePerson_ThrowsBusinessValidationError()
        {
            // Arrange
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns(RequestedBy);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId));

            ex.Errors.Should().ContainSingle(e => e.Code == "INVALID_Approval");
        }

        #endregion

        // -----------------------------------------------------------------------
        // EnqueueYearEndDataSetupRejectJobAsync — success
        // -----------------------------------------------------------------------

        #region EnqueueYearEndDataSetupRejectJobAsync — success

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenAllValid_ReturnsTrue()
        {
            // Arrange
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueDataSetupRejectBatchJobAsync(JobName, RequestedBy, CorrelationId, Arg.Any<string>())
                .Returns(queueEntry);
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(new BatchJobEventTriggerDto());

            // Act
            var result = await _sut.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            result.Should().BeTrue();
            await _yearEndRepository.Received(1).EnqueueDataSetupRejectBatchJobAsync(
                JobName, RequestedBy, CorrelationId, Arg.Any<string>());
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenAllValid_SendsEmail()
        {
            // Arrange
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid(), RequestedBy = RequestedBy };
            _yearEndRepository.EnqueueDataSetupRejectBatchJobAsync(JobName, RequestedBy, CorrelationId, Arg.Any<string>())
                .Returns(queueEntry);
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(new BatchJobEventTriggerDto());

            // Act
            await _sut.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert
            await _emailService.Received(1).SendEmailAsync(
                Arg.Is<EmailMessageModel>(m => m.To.Contains("approval@example.com")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenEmailFails_StillReturnsTrue()
        {
            // Arrange — email failure must be swallowed, not propagated
            _yearEndRepository.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);
            _yearEndRepository.GetYearEndDataSetupRequestInitiatorAsync(JobName).Returns("other@example.com");

            var queueEntry = new BatchJobQueue { JobqueueId = Guid.NewGuid() };
            _yearEndRepository.EnqueueDataSetupRejectBatchJobAsync(JobName, RequestedBy, CorrelationId, Arg.Any<string>())
                .Returns(queueEntry);
            _mapper.Map<BatchJobEventTriggerDto>(queueEntry).Returns(new BatchJobEventTriggerDto());

            _emailService.SendEmailAsync(Arg.Any<EmailMessageModel>(), Arg.Any<CancellationToken>())
                .Throws(new Exception("SMTP failure"));

            // Act
            var result = await _sut.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear, ContextYear, RequestedBy, CorrelationId);

            // Assert — email failure swallowed; true still returned
            result.Should().BeTrue();
        }

        #endregion
    }
}
