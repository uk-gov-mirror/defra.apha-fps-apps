using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsYearEndApiClientTest
{
    public class FpsYearEndApiClientTests
    {
        private const string JobName = "YearEnd-DataSetup";
        private const int PlannedYear = 2025;

        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsYearEndApiClient _client;

        public FpsYearEndApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsYearEndApiClient(_http, _mapper);
        }

        // -----------------------------------------------------------------------
        // GetYearEndDataSetupBatchJobHistoryAsync
        // -----------------------------------------------------------------------

        #region GetYearEndDataSetupBatchJobHistoryAsync

        [Fact]
        public async Task GetYearEndDataSetupBatchJobHistoryAsync_WhenApiReturnsSuccess_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<BatchJobHistoryRes>
            {
                new BatchJobHistoryRes { JobId = 1, JobName = JobName, Status = "Completed", RequestedBy = "user@test.com" },
                new BatchJobHistoryRes { JobId = 1, JobName = JobName, Status = "Failed",    RequestedBy = "user@test.com" }
            };
            var pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 };
            var apiResponse = new ApiResponse<List<BatchJobHistoryRes>>
            {
                Success = true,
                Data = resList,
                Pagination = pagination
            };
            var mappedDto = ApiResponseDto<List<BatchJobHistoryDto>>.SuccessResponse(
            [
                new BatchJobHistoryDto { JobId = 1, JobName = JobName, Status = "Completed" },
                new BatchJobHistoryDto { JobId = 1, JobName = JobName, Status = "Failed" }
            ]);

            _http.GetAsync<List<BatchJobHistoryRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<BatchJobHistoryDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.TotalCount);
            await _http.Received(1).GetAsync<List<BatchJobHistoryRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetYearEndDataSetupBatchJobHistoryAsync_WhenApiReturnsEmptyList_ReturnsPaginatedResultWithNoData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<BatchJobHistoryRes>>
            {
                Success = true,
                Data = [],
                Pagination = new Pagination { TotalRecords = 0 }
            };
            var mappedDto = ApiResponseDto<List<BatchJobHistoryDto>>.SuccessResponse([]);

            _http.GetAsync<List<BatchJobHistoryRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<BatchJobHistoryDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(0, result.Data?.TotalCount);
            Assert.Empty(result.Data!.data);
        }

        [Fact]
        public async Task GetYearEndDataSetupBatchJobHistoryAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new ApiError { Message = "Unauthorized", Code = "UNAUTHORIZED" } };
            var apiResponse = new ApiResponse<List<BatchJobHistoryRes>> { Success = false, Errors = errors };
            var mappedFailure = new ApiResponseDto<List<BatchJobHistoryDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Unauthorized", Code = "UNAUTHORIZED" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<BatchJobHistoryRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<BatchJobHistoryDto>>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("UNAUTHORIZED", result.Errors![0].Code);
            await _http.Received(1).GetAsync<List<BatchJobHistoryRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetYearEndDataSetupBatchJobHistoryAsync_UrlContainsJobNameQueryParam()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<BatchJobHistoryRes>> { Success = true, Data = [] };
            var mappedDto = ApiResponseDto<List<BatchJobHistoryDto>>.SuccessResponse([]);

            _http.GetAsync<List<BatchJobHistoryRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<BatchJobHistoryDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);

            // Assert
            await _http.Received(1).GetAsync<List<BatchJobHistoryRes>>(
                Arg.Is<string>(url => url.Contains("jobName=") && url.Contains(Uri.EscapeDataString(JobName))));
        }

        [Fact]
        public async Task GetYearEndDataSetupBatchJobHistoryAsync_WhenPaginationIsNull_UsesFallbackValues()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var apiResponse = new ApiResponse<List<BatchJobHistoryRes>>
            {
                Success = true,
                Data = [],
                Pagination = null   // no pagination header
            };
            var mappedDto = ApiResponseDto<List<BatchJobHistoryDto>>.SuccessResponse([]);

            _http.GetAsync<List<BatchJobHistoryRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<BatchJobHistoryDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetYearEndDataSetupBatchJobHistoryAsync(query, JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(query.Page, result.Data?.PageNumber);
            Assert.Equal(query.PageSize, result.Data?.PageSize);
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
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            _http.GetAsync<bool>($"api/v1/yearend/dataSetup/caninitiate?jobName={Uri.EscapeDataString(JobName)}")
                 .Returns(apiResponse);

            // Act
            var result = await _client.GetCanInitiateDataSetupRequestAsync(JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).GetAsync<bool>(
                $"api/v1/yearend/dataSetup/caninitiate?jobName={Uri.EscapeDataString(JobName)}");
        }

        [Fact]
        public async Task GetCanInitiateDataSetupRequestAsync_WhenApiReturnsFalse_ReturnsSuccessWithFalse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool> { Success = true, Data = false };
            _http.GetAsync<bool>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetCanInitiateDataSetupRequestAsync(JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
        }

        [Fact]
        public async Task GetCanInitiateDataSetupRequestAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Service error", Code = "SERVICE_ERROR" } };
            var apiResponse = new ApiResponse<bool> { Success = false, Errors = errors };
            var mappedFailure = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Service error", Code = "SERVICE_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetCanInitiateDataSetupRequestAsync(JobName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task GetCanInitiateDataSetupRequestAsync_UrlContainsEscapedJobName()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            _http.GetAsync<bool>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            await _client.GetCanInitiateDataSetupRequestAsync(JobName);

            // Assert
            await _http.Received(1).GetAsync<bool>(
                Arg.Is<string>(url => url.Contains("jobName=") && url.Contains(Uri.EscapeDataString(JobName))));
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
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            _http.GetAsync<bool>($"api/v1/yearend/dataSetup/canapproveorreject?jobName={Uri.EscapeDataString(JobName)}")
                 .Returns(apiResponse);

            // Act
            var result = await _client.GetCanApproveOrRejectDataSetupRequestAsync(JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).GetAsync<bool>(
                $"api/v1/yearend/dataSetup/canapproveorreject?jobName={Uri.EscapeDataString(JobName)}");
        }

        [Fact]
        public async Task GetCanApproveDataSetupRequestAsync_WhenApiReturnsFalse_ReturnsSuccessWithFalse()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool> { Success = true, Data = false };
            _http.GetAsync<bool>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            var result = await _client.GetCanApproveOrRejectDataSetupRequestAsync(JobName);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
        }

        [Fact]
        public async Task GetCanApproveDataSetupRequestAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Forbidden", Code = "FORBIDDEN" } };
            var apiResponse = new ApiResponse<bool> { Success = false, Errors = errors };
            var mappedFailure = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Forbidden", Code = "FORBIDDEN" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.GetCanApproveOrRejectDataSetupRequestAsync(JobName);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task GetCanApproveDataSetupRequestAsync_UrlContainsEscapedJobName()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool> { Success = true, Data = false };
            _http.GetAsync<bool>(Arg.Any<string>()).Returns(apiResponse);

            // Act
            await _client.GetCanApproveOrRejectDataSetupRequestAsync(JobName);

            // Assert
            await _http.Received(1).GetAsync<bool>(
                Arg.Is<string>(url => url.Contains("jobName=") && url.Contains(Uri.EscapeDataString(JobName))));
        }

        #endregion

        // -----------------------------------------------------------------------
        // EnqueueYearEndDataSetupInitiationJobAsync
        // -----------------------------------------------------------------------

        #region EnqueueYearEndDataSetupInitiationJobAsync

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenApiReturnsSuccess_ReturnsMappedBatchJobQueueDto()
        {
            // Arrange
            var queueRes = new BatchJobQueueRes
            {
                JobId = 1,
                RequestedBy = "user@test.com",
                RequestedAtUtc = DateTime.UtcNow
            };
            var apiResponse = new ApiResponse<BatchJobQueueRes> { Success = true, Data = queueRes };
            var expectedDto = ApiResponseDto<BatchJobQueueDto>.SuccessResponse(
                new BatchJobQueueDto { JobId = 1, RequestedBy = "user@test.com" });

            _http.PostAsync<YearEndDataSetupReq, BatchJobQueueRes>(
                    "api/v1/yearend/dataSetup/initiation",
                    Arg.Is<YearEndDataSetupReq>(r => r.PlannedYear == PlannedYear))
                 .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BatchJobQueueDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1, result.Data?.JobId);
            await _http.Received(1).PostAsync<YearEndDataSetupReq, BatchJobQueueRes>(
                "api/v1/yearend/dataSetup/initiation",
                Arg.Is<YearEndDataSetupReq>(r => r.PlannedYear == PlannedYear));
            _mapper.Received(1).Map<ApiResponseDto<BatchJobQueueDto>>(apiResponse);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Job already running", Code = "CONFLICT" } };
            var apiResponse = new ApiResponse<BatchJobQueueRes> { Success = false, Errors = errors };
            var mappedFailure = new ApiResponseDto<BatchJobQueueDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Job already running", Code = "CONFLICT" }],
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<YearEndDataSetupReq, BatchJobQueueRes>(Arg.Any<string>(), Arg.Any<YearEndDataSetupReq>())
                 .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BatchJobQueueDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("CONFLICT", result.Errors![0].Code);
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_WhenApiReturnsSuccessWithNullData_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<BatchJobQueueRes> { Success = true, Data = null };
            var mappedFailure = new ApiResponseDto<BatchJobQueueDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "No data", Code = "NO_DATA" }],
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<YearEndDataSetupReq, BatchJobQueueRes>(Arg.Any<string>(), Arg.Any<YearEndDataSetupReq>())
                 .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BatchJobQueueDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.EnqueueYearEndDataSetupInitiationJobAsync(PlannedYear);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Theory]
        [InlineData(2025)]
        [InlineData(2026)]
        public async Task EnqueueYearEndDataSetupInitiationJobAsync_PostsRequestWithCorrectPlannedYear(int plannedYear)
        {
            // Arrange
            var apiResponse = new ApiResponse<BatchJobQueueRes>
            {
                Success = true,
                Data = new BatchJobQueueRes()
            };
            var expectedDto = ApiResponseDto<BatchJobQueueDto>.SuccessResponse(new BatchJobQueueDto());

            _http.PostAsync<YearEndDataSetupReq, BatchJobQueueRes>(Arg.Any<string>(), Arg.Any<YearEndDataSetupReq>())
                 .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BatchJobQueueDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.EnqueueYearEndDataSetupInitiationJobAsync(plannedYear);

            // Assert
            await _http.Received(1).PostAsync<YearEndDataSetupReq, BatchJobQueueRes>(
                "api/v1/yearend/dataSetup/initiation",
                Arg.Is<YearEndDataSetupReq>(r => r.PlannedYear == plannedYear));
        }

        #endregion

        // -----------------------------------------------------------------------
        // TriggerYearEndDataSetupApprovalJobAsync
        // -----------------------------------------------------------------------

        #region TriggerYearEndDataSetupApprovalJobAsync

        [Fact]
        public async Task TriggerYearEndDataSetupApprovalJobAsync_WhenApiReturnsSuccess_ReturnsMappedBatchJobEventTriggerDto()
        {
            // Arrange
            var triggerRes = new BatchJobEventTriggerRes
            {
                EventId = "evt-001",
                Jobqueue = new BatchJobQueueRes { JobId = 1, RequestedBy = "approver@test.com" }
            };
            var apiResponse = new ApiResponse<BatchJobEventTriggerRes> { Success = true, Data = triggerRes };
            var expectedDto = ApiResponseDto<BatchJobEventTriggerDto>.SuccessResponse(
                new BatchJobEventTriggerDto { EventId = "evt-001", Jobqueue = new BatchJobQueueDto { JobId = 1 } });

            _http.PostAsync<YearEndDataSetupReq, BatchJobEventTriggerRes>(
                    "api/v1/yearend/dataSetup/approval",
                    Arg.Is<YearEndDataSetupReq>(r => r.PlannedYear == PlannedYear))
                 .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BatchJobEventTriggerDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.TriggerYearEndDataSetupApprovalJobAsync(PlannedYear);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("evt-001", result.Data?.EventId);
            await _http.Received(1).PostAsync<YearEndDataSetupReq, BatchJobEventTriggerRes>(
                "api/v1/yearend/dataSetup/approval",
                Arg.Is<YearEndDataSetupReq>(r => r.PlannedYear == PlannedYear));
            _mapper.Received(1).Map<ApiResponseDto<BatchJobEventTriggerDto>>(apiResponse);
        }

        [Fact]
        public async Task TriggerYearEndDataSetupApprovalJobAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Approval not allowed", Code = "VALIDATION_ERROR" } };
            var apiResponse = new ApiResponse<BatchJobEventTriggerRes> { Success = false, Errors = errors };
            var mappedFailure = new ApiResponseDto<BatchJobEventTriggerDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Approval not allowed", Code = "VALIDATION_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<YearEndDataSetupReq, BatchJobEventTriggerRes>(Arg.Any<string>(), Arg.Any<YearEndDataSetupReq>())
                 .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BatchJobEventTriggerDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.TriggerYearEndDataSetupApprovalJobAsync(PlannedYear);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("VALIDATION_ERROR", result.Errors![0].Code);
        }

        [Fact]
        public async Task TriggerYearEndDataSetupApprovalJobAsync_WhenApiReturnsSuccessWithNullData_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<BatchJobEventTriggerRes> { Success = true, Data = null };
            var mappedFailure = new ApiResponseDto<BatchJobEventTriggerDto>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "No data", Code = "NO_DATA" }],
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<YearEndDataSetupReq, BatchJobEventTriggerRes>(Arg.Any<string>(), Arg.Any<YearEndDataSetupReq>())
                 .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BatchJobEventTriggerDto>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.TriggerYearEndDataSetupApprovalJobAsync(PlannedYear);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Theory]
        [InlineData(2025)]
        [InlineData(2026)]
        public async Task TriggerYearEndDataSetupApprovalJobAsync_PostsRequestWithCorrectPlannedYear(int plannedYear)
        {
            // Arrange
            var apiResponse = new ApiResponse<BatchJobEventTriggerRes>
            {
                Success = true,
                Data = new BatchJobEventTriggerRes { Jobqueue = new BatchJobQueueRes() }
            };
            var expectedDto = ApiResponseDto<BatchJobEventTriggerDto>.SuccessResponse(
                new BatchJobEventTriggerDto { Jobqueue = new BatchJobQueueDto() });

            _http.PostAsync<YearEndDataSetupReq, BatchJobEventTriggerRes>(Arg.Any<string>(), Arg.Any<YearEndDataSetupReq>())
                 .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<BatchJobEventTriggerDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.TriggerYearEndDataSetupApprovalJobAsync(plannedYear);

            // Assert
            await _http.Received(1).PostAsync<YearEndDataSetupReq, BatchJobEventTriggerRes>(
                "api/v1/yearend/dataSetup/approval",
                Arg.Is<YearEndDataSetupReq>(r => r.PlannedYear == plannedYear));
        }

        #endregion

        // -----------------------------------------------------------------------
        // EnqueueYearEndDataSetupRejectJobAsync
        // -----------------------------------------------------------------------

        #region EnqueueYearEndDataSetupRejectJobAsync

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenApiReturnsSuccess_ReturnsSuccessWithTrue()
        {
            // Arrange
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };

            _http.PostAsync<YearEndDataSetupReq, bool>(
                    "api/v1/yearend/dataSetup/reject",
                    Arg.Is<YearEndDataSetupReq>(r => r.PlannedYear == PlannedYear))
                 .Returns(apiResponse);

            // Act
            var result = await _client.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).PostAsync<YearEndDataSetupReq, bool>(
                "api/v1/yearend/dataSetup/reject",
                Arg.Is<YearEndDataSetupReq>(r => r.PlannedYear == PlannedYear));
        }

        [Fact]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Rejection not allowed", Code = "VALIDATION_ERROR" } };
            var apiResponse = new ApiResponse<bool> { Success = false, Errors = errors };
            var mappedFailure = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Rejection not allowed", Code = "VALIDATION_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<YearEndDataSetupReq, bool>(Arg.Any<string>(), Arg.Any<YearEndDataSetupReq>())
                 .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedFailure);

            // Act
            var result = await _client.EnqueueYearEndDataSetupRejectJobAsync(PlannedYear);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
            Assert.Equal("VALIDATION_ERROR", result.Errors![0].Code);
        }

        [Theory]
        [InlineData(2025)]
        [InlineData(2026)]
        public async Task EnqueueYearEndDataSetupRejectJobAsync_PostsRequestWithCorrectPlannedYear(int plannedYear)
        {
            // Arrange
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };

            _http.PostAsync<YearEndDataSetupReq, bool>(Arg.Any<string>(), Arg.Any<YearEndDataSetupReq>())
                 .Returns(apiResponse);

            // Act
            await _client.EnqueueYearEndDataSetupRejectJobAsync(plannedYear);

            // Assert
            await _http.Received(1).PostAsync<YearEndDataSetupReq, bool>(
                "api/v1/yearend/dataSetup/reject",
                Arg.Is<YearEndDataSetupReq>(r => r.PlannedYear == plannedYear));
        }

        #endregion
    }
}
