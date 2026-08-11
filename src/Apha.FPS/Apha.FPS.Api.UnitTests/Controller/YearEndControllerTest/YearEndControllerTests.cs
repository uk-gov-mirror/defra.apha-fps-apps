using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Validation;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Api.UnitTests.Controller.YearEndControllerTest
{
    public class YearEndControllerTests
    {
        private const string JobName = "YearEnd-DataSetup";
        private const string CorrelationId = "corr-001";
        private const string UserEmail = "user@example.com";
        private const int FpsYear = 2024;
        private const int PlannedYear = 2025;

        private readonly IYearEndService _yearEndService;
        private readonly IFpsRequestContext _fpsRequestContext;
        private readonly IMapper _mapper;
        private readonly YearEndController _sut;

        public YearEndControllerTests()
        {
            _yearEndService = Substitute.For<IYearEndService>();
            _fpsRequestContext = Substitute.For<IFpsRequestContext>();
            _mapper = Substitute.For<IMapper>();

            _fpsRequestContext.FpsYear.Returns(FpsYear);
            _fpsRequestContext.UserEmailId.Returns(UserEmail);

            _sut = new YearEndController(_yearEndService, _fpsRequestContext, _mapper);
        }

        // -----------------------------------------------------------------------
        // GetYearEndBatchJobHistory
        // -----------------------------------------------------------------------

        #region GetYearEndBatchJobHistory

        [Fact]
        public async Task GetYearEndBatchJobHistory_WhenDataExists_ReturnsOkWithMappedPaginationRes()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<BatchJobHistoryDto>
            {
                Data = [
                    new BatchJobHistoryDto { JobId = 1, JobName = JobName, Status = "Completed" },
                    new BatchJobHistoryDto { JobId = 1, JobName = JobName, Status = "Failed" }
                ]
            };
            var mappedResult = new PaginationRes<BatchJobHistoryRes>
            {
                Data = [
                    new BatchJobHistoryRes { JobId = 1, JobName = JobName, Status = "Completed" },
                    new BatchJobHistoryRes { JobId = 1, JobName = JobName, Status = "Failed" }
                ]
            };

            _yearEndService.GetBatchJobsHistoryAsync(query, JobName).Returns(serviceResult);
            _mapper.Map<PaginationRes<BatchJobHistoryRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _sut.GetYearEndDataSetupBatchJobHistory(query, JobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(mappedResult);

            await _yearEndService.Received(1).GetBatchJobsHistoryAsync(query, JobName);
            _mapper.Received(1).Map<PaginationRes<BatchJobHistoryRes>>(serviceResult);
        }

        [Fact]
        public async Task GetYearEndBatchJobHistory_WhenNoData_ReturnsOkWithEmptyPaginationRes()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<BatchJobHistoryDto> { Data = [] };
            var mappedResult = new PaginationRes<BatchJobHistoryRes> { Data = [] };

            _yearEndService.GetBatchJobsHistoryAsync(query, JobName).Returns(serviceResult);
            _mapper.Map<PaginationRes<BatchJobHistoryRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _sut.GetYearEndDataSetupBatchJobHistory(query, JobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);

            await _yearEndService.Received(1).GetBatchJobsHistoryAsync(query, JobName);
        }

        [Fact]
        public async Task GetYearEndBatchJobHistory_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            _yearEndService.GetBatchJobsHistoryAsync(query, JobName).Throws(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.GetYearEndDataSetupBatchJobHistory(query, JobName));
            exception.Message.Should().Be("Database error");
            await _yearEndService.Received(1).GetBatchJobsHistoryAsync(query, JobName);
        }

        #endregion

        // -----------------------------------------------------------------------
        // CanInitiateYearEndDataSetupRequestAsync
        // -----------------------------------------------------------------------

        #region CanInitiateYearEndDataSetupRequestAsync

        [Fact]
        public async Task CanInitiateYearEndDataSetupRequestAsync_WhenServiceReturnsTrue_ReturnsOkWithTrue()
        {
            // Arrange
            _yearEndService.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act
            var result = await _sut.CanInitiateYearEndDataSetupRequestAsync(JobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be(true);

            await _yearEndService.Received(1).CanInitiateYearEndDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanInitiateYearEndDataSetupRequestAsync_WhenServiceReturnsFalse_ReturnsOkWithFalse()
        {
            // Arrange
            _yearEndService.CanInitiateYearEndDataSetupRequestAsync(JobName).Returns(false);

            // Act
            var result = await _sut.CanInitiateYearEndDataSetupRequestAsync(JobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(false);

            await _yearEndService.Received(1).CanInitiateYearEndDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanInitiateYearEndDataSetupRequestAsync_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _yearEndService.CanInitiateYearEndDataSetupRequestAsync(JobName).Throws(new Exception("Service error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.CanInitiateYearEndDataSetupRequestAsync(JobName));
            exception.Message.Should().Be("Service error");
            await _yearEndService.Received(1).CanInitiateYearEndDataSetupRequestAsync(JobName);
        }

        #endregion

        // -----------------------------------------------------------------------
        // CanApproveYearEndDataSetupRequestAsync
        // -----------------------------------------------------------------------

        #region CanApproveYearEndDataSetupRequestAsync

        [Fact]
        public async Task CanApproveYearEndDataSetupRequestAsync_WhenServiceReturnsTrue_ReturnsOkWithTrue()
        {
            // Arrange
            _yearEndService.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(true);

            // Act
            var result = await _sut.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be(true);

            await _yearEndService.Received(1).CanApproveOrRejectYearEndDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanApproveYearEndDataSetupRequestAsync_WhenServiceReturnsFalse_ReturnsOkWithFalse()
        {
            // Arrange
            _yearEndService.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Returns(false);

            // Act
            var result = await _sut.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(false);

            await _yearEndService.Received(1).CanApproveOrRejectYearEndDataSetupRequestAsync(JobName);
        }

        [Fact]
        public async Task CanApproveYearEndDataSetupRequestAsync_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            _yearEndService.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName).Throws(new Exception("Service error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _sut.CanApproveOrRejectYearEndDataSetupRequestAsync(JobName));
            exception.Message.Should().Be("Service error");
            await _yearEndService.Received(1).CanApproveOrRejectYearEndDataSetupRequestAsync(JobName);
        }

        #endregion

        // -----------------------------------------------------------------------
        // EnqueueYearEndDataSetupInitiationJob
        // -----------------------------------------------------------------------

        #region EnqueueYearEndDataSetupInitiationJob

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJob_WhenValid_ReturnsOkWithMappedResult()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            var serviceResult = new BatchJobQueueDto { RequestedBy = UserEmail };
            var mappedRes = new BatchJobQueueRes { RequestedBy = UserEmail };

            _yearEndService
                .EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear, FpsYear, UserEmail, CorrelationId)
                .Returns(serviceResult);
            _mapper.Map<BatchJobQueueRes>(serviceResult).Returns(mappedRes);

            // Act
            var result = await _sut.EnqueueYearEndDataSetupInitiationJob(request, CorrelationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(mappedRes);

            await _yearEndService.Received(1).EnqueueYearEndDataSetupInitiationJobAsync(
                PlannedYear, FpsYear, UserEmail, CorrelationId);
            _mapper.Received(1).Map<BatchJobQueueRes>(serviceResult);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJob_PassesFpsRequestContextValuesToService()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            var serviceResult = new BatchJobQueueDto();
            _yearEndService
                .EnqueueYearEndDataSetupInitiationJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(serviceResult);
            _mapper.Map<BatchJobQueueRes>(serviceResult).Returns(new BatchJobQueueRes());

            // Act
            await _sut.EnqueueYearEndDataSetupInitiationJob(request, CorrelationId);

            // Assert — context values forwarded correctly
            await _yearEndService.Received(1).EnqueueYearEndDataSetupInitiationJobAsync(
                PlannedYear,
                FpsYear,
                UserEmail,
                CorrelationId);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJob_WhenServiceThrowsBusinessValidationException_PropagatesException()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = 0 };
            _yearEndService
                .EnqueueYearEndDataSetupInitiationJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new BusinessValidationErrorException([new BusinessValidationError("Invalid year", "INVALID_PlannedYear")]));

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupInitiationJob(request, CorrelationId));
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJob_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            _yearEndService
                .EnqueueYearEndDataSetupInitiationJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new Exception("Enqueue failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.EnqueueYearEndDataSetupInitiationJob(request, CorrelationId));
            exception.Message.Should().Be("Enqueue failed");
        }

        #endregion

        // -----------------------------------------------------------------------
        // EnqueueYearEndDataSetupApprovalJob
        // -----------------------------------------------------------------------

        #region EnqueueYearEndDataSetupApprovalJob

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJob_WhenValid_ReturnsOkWithMappedResult()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            var serviceResult = new BatchJobEventTriggerDto { EventId = "evt-001" };
            var mappedRes = new BatchJobEventTriggerRes { EventId = "evt-001" };

            _yearEndService
                .EnqueueYearEndDataSetupApprovalJobAsync(PlannedYear, FpsYear, UserEmail, CorrelationId)
                .Returns(serviceResult);
            _mapper.Map<BatchJobEventTriggerRes>(serviceResult).Returns(mappedRes);

            // Act
            var result = await _sut.EnqueueYearEndDataSetupApprovalJob(request, CorrelationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(mappedRes);

            await _yearEndService.Received(1).EnqueueYearEndDataSetupApprovalJobAsync(
                PlannedYear, FpsYear, UserEmail, CorrelationId);
            _mapper.Received(1).Map<BatchJobEventTriggerRes>(serviceResult);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJob_PassesFpsRequestContextValuesToService()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            var serviceResult = new BatchJobEventTriggerDto();
            _yearEndService
                .EnqueueYearEndDataSetupApprovalJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(serviceResult);
            _mapper.Map<BatchJobEventTriggerRes>(serviceResult).Returns(new BatchJobEventTriggerRes());

            // Act
            await _sut.EnqueueYearEndDataSetupApprovalJob(request, CorrelationId);

            // Assert — context values forwarded correctly
            await _yearEndService.Received(1).EnqueueYearEndDataSetupApprovalJobAsync(
                PlannedYear,
                FpsYear,
                UserEmail,
                CorrelationId);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJob_WhenServiceThrowsBusinessValidationException_PropagatesException()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            _yearEndService
                .EnqueueYearEndDataSetupApprovalJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new BusinessValidationErrorException([new BusinessValidationError("Same person", "INVALID_Approval")]));

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupApprovalJob(request, CorrelationId));
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupApprovalJob_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            _yearEndService
                .EnqueueYearEndDataSetupApprovalJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new Exception("Approval failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.EnqueueYearEndDataSetupApprovalJob(request, CorrelationId));
            exception.Message.Should().Be("Approval failed");
        }

        #endregion

        // -----------------------------------------------------------------------
        // EnqueueYearEndDataSetupRejectJob
        // -----------------------------------------------------------------------

        #region EnqueueYearEndDataSetupRejectJob

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJob_WhenValid_ReturnsOkWithServiceResult()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            const bool serviceResult = true;

            _yearEndService
                .EnqueueYearEndDataSetupRejectJobAsync(PlannedYear, FpsYear, UserEmail, CorrelationId)
                .Returns(serviceResult);

            // Act
            var result = await _sut.EnqueueYearEndDataSetupRejectJob(request, CorrelationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().Be(serviceResult);

            await _yearEndService.Received(1).EnqueueYearEndDataSetupRejectJobAsync(
                PlannedYear, FpsYear, UserEmail, CorrelationId);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJob_PassesFpsRequestContextValuesToService()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            _yearEndService
                .EnqueueYearEndDataSetupRejectJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(true);
            _mapper.Map<BatchJobEventTriggerRes>(true).Returns(new BatchJobEventTriggerRes());

            // Act
            await _sut.EnqueueYearEndDataSetupRejectJob(request, CorrelationId);

            // Assert — context values forwarded correctly
            await _yearEndService.Received(1).EnqueueYearEndDataSetupRejectJobAsync(
                PlannedYear, FpsYear, UserEmail, CorrelationId);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJob_WhenServiceThrowsBusinessValidationException_PropagatesException()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            _yearEndService
                .EnqueueYearEndDataSetupRejectJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new BusinessValidationErrorException([new BusinessValidationError("Same person", "INVALID_Approval")]));

            // Act & Assert
            await Assert.ThrowsAsync<BusinessValidationErrorException>(
                () => _sut.EnqueueYearEndDataSetupRejectJob(request, CorrelationId));
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJob_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new YearEndDataSetupReq { PlannedYear = PlannedYear };
            _yearEndService
                .EnqueueYearEndDataSetupRejectJobAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new Exception("Rejection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                () => _sut.EnqueueYearEndDataSetupRejectJob(request, CorrelationId));
            exception.Message.Should().Be("Rejection failed");
        }

        #endregion
    }
}
