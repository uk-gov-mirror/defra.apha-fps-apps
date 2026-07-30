using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.RecreateSummaryControllerTest
{
    public class RecreateSummaryControllerTests
    {
        private const string EmptyFilterJson = "{}";
        private const string PartialViewName = "_DataGrid";
        private const string GridId = "summaryHistoryGrid";
        private const string JobName = "RecreateSummary";
        private const int DefaultPageNumber = 1;
        private const int DefaultPageSize = 10;

        private readonly IMapper _mapper;
        private readonly IRecreateSummaryService _service;
        private readonly IMonthService _monthService;
        private readonly ILogger<RecreateSummaryController> _logger;
        private readonly RecreateSummaryController _controller;

        public RecreateSummaryControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _service = Substitute.For<IRecreateSummaryService>();
            _monthService = Substitute.For<IMonthService>();
            _logger = Substitute.For<ILogger<RecreateSummaryController>>();
            _controller = new RecreateSummaryController(_mapper, _service, _monthService, _logger);
        }

        #region Helper Methods

        private void SetupQueryParametersMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(callInfo =>
                {
                    var filter = callInfo.Arg<PaginationFilter<string>>();
                    return new QueryParameters<string>
                    {
                        Page = filter.Page,
                        PageSize = filter.PageSize,
                        Search = filter.Search,
                        Filter = filter.Filter
                    };
                });
        }

        private void SetupHistoryItemMapper(List<BatchJobHistoryDto> dtos, List<BatchJobHistoryItem> items)
        {
            _mapper.Map<List<BatchJobHistoryItem>>(Arg.Any<IEnumerable<BatchJobHistoryDto>>()).Returns(items);
        }

        private void SetupPaginationMapper(PaginationDto? paginationDto = null)
        {
            var pagination = paginationDto ?? new PaginationDto
            {
                PageNumber = DefaultPageNumber,
                PageSize = DefaultPageSize,
                TotalRecords = 0,
                TotalPages = 0
            };

            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel
                {
                    PageNumber = pagination.PageNumber,
                    PageSize = pagination.PageSize,
                    TotalRecords = pagination.TotalRecords
                });
        }

        private void SetupMonthService(int count = 3, bool success = true)
        {
            var months = Enumerable.Range(1, count)
                .Select(i => new MonthDto { Monthnumber = (short)i, Monthname = $"Month{i}" })
                .ToList();

            _monthService.GetAllMonthsAsync()
                .Returns(new ApiResponseDto<List<MonthDto>>
                {
                    Success = success,
                    Data = success ? months : null
                });
        }

        private void SetupCanRunJob(bool canRun = true)
        {
            _service.CanRunRecreateSummaryBatchJobAsync(JobName)
                .Returns(new ApiResponseDto<bool> { Success = true, Data = canRun });
        }

        private static ApiResponseDto<List<BatchJobHistoryDto>> BuildHistorySuccessResponse(int count = 2)
        {
            var data = Enumerable.Range(1, count)
                .Select(i => new BatchJobHistoryDto
                {
                    JobId = i,
                    JobName = JobName,
                    Status = "Completed",
                    RequestedBy = $"User{i}",
                    StartDateTime = DateTime.UtcNow.AddMinutes(-i)
                })
                .ToList();

            return new ApiResponseDto<List<BatchJobHistoryDto>>
            {
                Success = true,
                Data = data,
                Pagination = new PaginationDto
                {
                    PageNumber = DefaultPageNumber,
                    PageSize = DefaultPageSize,
                    TotalRecords = count,
                    TotalPages = 1
                }
            };
        }

        private static ApiResponseDto<List<BatchJobHistoryDto>> BuildHistoryFailureResponse(string message = "API error")
        {
            return new ApiResponseDto<List<BatchJobHistoryDto>>
            {
                Success = false,
                Data = null,
                Errors = [new ApiErrorDto { Message = message }]
            };
        }

        private static List<BatchJobHistoryItem> BuildMappedHistoryItems(int count)
        {
            return Enumerable.Range(1, count)
                .Select(i => new BatchJobHistoryItem
                {
                    JobName = JobName,
                    RequestedBy = $"User{i}",
                    StartDateTime = DateTime.UtcNow.AddMinutes(-i),
                    Status = "Completed"
                })
                .ToList();
        }

        private void SetupHistoryService(ApiResponseDto<List<BatchJobHistoryDto>> response)
        {
            _service.GetRecreateSummaryBatchJobHistoryAsync(
                    Arg.Any<QueryParameters<string>>(), JobName)
                .Returns(response);
        }

        #endregion

        #region Index

        [Fact]
        public async Task Index_WithSuccessfulData_ReturnsViewWithPopulatedViewModel()
        {
            // Arrange
            var historyResponse = BuildHistorySuccessResponse(2);
            var mappedItems = BuildMappedHistoryItems(2);

            SetupQueryParametersMapper();
            SetupMonthService(3);
            SetupCanRunJob(true);
            SetupHistoryService(historyResponse);
            SetupHistoryItemMapper(historyResponse.Data!, mappedItems);
            SetupPaginationMapper(historyResponse.Pagination);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RecreateSummariesViewModel>(viewResult.Model);
            Assert.NotNull(model.HistoryGrid);
            Assert.Equal(GridId, model.HistoryGrid.GridId);
            Assert.Equal(3, model.Months.Count);
            Assert.True(model.CanRunJob);
        }

        [Fact]
        public async Task Index_CanRunJob_True_SetsViewModelCanRunJobTrue()
        {
            // Arrange
            SetupQueryParametersMapper();
            SetupMonthService();
            SetupCanRunJob(true);
            SetupHistoryService(BuildHistorySuccessResponse(0));
            SetupHistoryItemMapper([], []);
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RecreateSummariesViewModel>(viewResult.Model);
            Assert.True(model.CanRunJob);
        }

        [Fact]
        public async Task Index_CanRunJob_False_SetsViewModelCanRunJobFalse()
        {
            // Arrange
            SetupQueryParametersMapper();
            SetupMonthService();
            SetupCanRunJob(false);
            SetupHistoryService(BuildHistorySuccessResponse(0));
            SetupHistoryItemMapper([], []);
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RecreateSummariesViewModel>(viewResult.Model);
            Assert.False(model.CanRunJob);
        }

        [Fact]
        public async Task Index_WithEmptyMonths_ReturnsViewWithEmptyMonthsList()
        {
            // Arrange
            SetupQueryParametersMapper();
            SetupMonthService(0);
            SetupCanRunJob();
            SetupHistoryService(BuildHistorySuccessResponse(0));
            SetupHistoryItemMapper([], []);
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RecreateSummariesViewModel>(viewResult.Model);
            Assert.Empty(model.Months);
        }

        [Fact]
        public async Task Index_MonthsOrderedByNumber()
        {
            // Arrange – return months in reverse order to verify sorting
            var months = new List<MonthDto>
            {
                new() { Monthnumber = 3, Monthname = "March" },
                new() { Monthnumber = 1, Monthname = "January" },
                new() { Monthnumber = 2, Monthname = "February" }
            };
            _monthService.GetAllMonthsAsync()
                .Returns(new ApiResponseDto<List<MonthDto>> { Success = true, Data = months });

            SetupQueryParametersMapper();
            SetupCanRunJob();
            SetupHistoryService(BuildHistorySuccessResponse(0));
            SetupHistoryItemMapper([], []);
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RecreateSummariesViewModel>(viewResult.Model);
            Assert.Equal("1", model.Months[0].Value);
            Assert.Equal("2", model.Months[1].Value);
            Assert.Equal("3", model.Months[2].Value);
        }

        [Fact]
        public async Task Index_MonthItems_HaveCorrectTextFormat()
        {
            // Arrange
            _monthService.GetAllMonthsAsync()
                .Returns(new ApiResponseDto<List<MonthDto>>
                {
                    Success = true,
                    Data = [new MonthDto { Monthnumber = 6, Monthname = "June" }]
                });

            SetupQueryParametersMapper();
            SetupCanRunJob();
            SetupHistoryService(BuildHistorySuccessResponse(0));
            SetupHistoryItemMapper([], []);
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RecreateSummariesViewModel>(viewResult.Model);
            Assert.Equal("6 - June", model.Months[0].Text);
        }

        [Fact]
        public async Task Index_MonthServiceReturnsNull_ReturnsEmptyMonthsList()
        {
            // Arrange
            _monthService.GetAllMonthsAsync()
                .Returns(new ApiResponseDto<List<MonthDto>> { Success = false, Data = null });

            SetupQueryParametersMapper();
            SetupCanRunJob();
            SetupHistoryService(BuildHistorySuccessResponse(0));
            SetupHistoryItemMapper([], []);
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RecreateSummariesViewModel>(viewResult.Model);
            Assert.Empty(model.Months);
        }

        [Fact]
        public async Task Index_CallsMapperWithDefaultFilter()
        {
            // Arrange
            SetupQueryParametersMapper();
            SetupMonthService();
            SetupCanRunJob();
            SetupHistoryService(BuildHistorySuccessResponse(0));
            SetupHistoryItemMapper([], []);
            SetupPaginationMapper();

            // Act
            await _controller.Index();

            // Assert
            _mapper.Received(1).Map<QueryParameters<string>>(
                Arg.Is<PaginationFilter<string>>(f => f.Filter == EmptyFilterJson));
        }

        [Fact]
        public async Task Index_CallsHistoryServiceWithJobName()
        {
            // Arrange
            SetupQueryParametersMapper();
            SetupMonthService();
            SetupCanRunJob();
            SetupHistoryService(BuildHistorySuccessResponse(0));
            SetupHistoryItemMapper([], []);
            SetupPaginationMapper();

            // Act
            await _controller.Index();

            // Assert
            await _service.Received(1).GetRecreateSummaryBatchJobHistoryAsync(
                Arg.Any<QueryParameters<string>>(), JobName);
        }

        [Fact]
        public async Task Index_CallsCanRunJobWithJobName()
        {
            // Arrange
            SetupQueryParametersMapper();
            SetupMonthService();
            SetupCanRunJob();
            SetupHistoryService(BuildHistorySuccessResponse(0));
            SetupHistoryItemMapper([], []);
            SetupPaginationMapper();

            // Act
            await _controller.Index();

            // Assert
            await _service.Received(1).CanRunRecreateSummaryBatchJobAsync(JobName);
        }

        [Fact]
        public async Task Index_HistoryServiceFailure_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            SetupQueryParametersMapper();
            SetupMonthService();
            SetupCanRunJob();
            SetupHistoryService(BuildHistoryFailureResponse("Service error"));
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RecreateSummariesViewModel>(viewResult.Model);
            Assert.Empty(model.HistoryGrid.Data);
        }

        [Fact]
        public async Task Index_HistoryServiceReturnsNullData_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            SetupQueryParametersMapper();
            SetupMonthService();
            SetupCanRunJob();
            _service.GetRecreateSummaryBatchJobHistoryAsync(Arg.Any<QueryParameters<string>>(), JobName)
                .Returns(new ApiResponseDto<List<BatchJobHistoryDto>> { Success = true, Data = null, Pagination = null });
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<RecreateSummariesViewModel>(viewResult.Model);
            Assert.Empty(model.HistoryGrid.Data);
        }

        #endregion

        #region LoadHistoryGrid

        [Fact]
        public async Task LoadHistoryGrid_WithData_ReturnsPartialViewWithGrid()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1, PageSize = 10, Filter = "{}", SortBy = "startdatetime", Descending = true
            };
            var historyResponse = BuildHistorySuccessResponse(3);
            var mappedItems = BuildMappedHistoryItems(3);

            SetupQueryParametersMapper();
            SetupHistoryService(historyResponse);
            SetupHistoryItemMapper(historyResponse.Data!, mappedItems);
            SetupPaginationMapper(historyResponse.Pagination);

            // Act
            var result = await _controller.LoadHistoryGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal(PartialViewName, partial.ViewName);
            var grid = Assert.IsAssignableFrom<DataGridConfig<BatchJobHistoryItem>>(partial.Model);
            Assert.Equal(3, grid.Data.Count());
        }

        [Fact]
        public async Task LoadHistoryGrid_WithEmptyData_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };

            SetupQueryParametersMapper();
            SetupHistoryService(BuildHistorySuccessResponse(0));
            SetupHistoryItemMapper([], []);
            SetupPaginationMapper();

            // Act
            var result = await _controller.LoadHistoryGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal(PartialViewName, partial.ViewName);
            var grid = Assert.IsAssignableFrom<DataGridConfig<BatchJobHistoryItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadHistoryGrid_SortColumnsAndDirectionForwardedToGrid()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1, PageSize = 10, Filter = "{}", SortBy = "requestedby", Descending = false
            };

            SetupQueryParametersMapper();
            SetupHistoryService(BuildHistorySuccessResponse(1));
            SetupHistoryItemMapper(BuildHistorySuccessResponse(1).Data!, BuildMappedHistoryItems(1));
            SetupPaginationMapper();

            // Act
            var result = await _controller.LoadHistoryGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsAssignableFrom<DataGridConfig<BatchJobHistoryItem>>(partial.Model);
            Assert.Equal("requestedby", grid.Pagination.SortColumn);
            Assert.False(grid.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadHistoryGrid_CallsHistoryServiceWithJobName()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupQueryParametersMapper();
            SetupHistoryService(BuildHistorySuccessResponse(0));
            SetupHistoryItemMapper([], []);
            SetupPaginationMapper();

            // Act
            await _controller.LoadHistoryGrid(request);

            // Assert
            await _service.Received(1).GetRecreateSummaryBatchJobHistoryAsync(
                Arg.Any<QueryParameters<string>>(), JobName);
        }

        [Fact]
        public async Task LoadHistoryGrid_HistoryServiceFailure_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupQueryParametersMapper();
            SetupHistoryService(BuildHistoryFailureResponse("Service down"));
            SetupPaginationMapper();

            // Act
            var result = await _controller.LoadHistoryGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal(PartialViewName, partial.ViewName);
            var grid = Assert.IsAssignableFrom<DataGridConfig<BatchJobHistoryItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        #endregion

        #region TriggerJob

        [Fact]
        public async Task TriggerJob_OnSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var queueDto = new BatchJobQueueDto { JobqueueId = Guid.NewGuid(), RequestedBy = "user@test.com", StartDateTime = DateTime.UtcNow, JobId=1, StatusId=1, RequestedAtUtc = DateTime.UtcNow };
            var triggerDto = new BatchJobEventTriggerDto { Jobqueue = queueDto, EventId = "evt-001" };

            _service.TriggerRecreateSummariesBatchJobAsync(6)
                .Returns(new ApiResponseDto<BatchJobEventTriggerDto> { Success = true, Data = triggerDto });

            // Act
            var result = await _controller.TriggerJob(6);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            Assert.True(success);
        }

        [Fact]
        public async Task TriggerJob_OnFailure_ReturnsJsonWithSuccessFalseAndErrors()
        {
            // Arrange
            _service.TriggerRecreateSummariesBatchJobAsync(0)
                .Returns(new ApiResponseDto<BatchJobEventTriggerDto>
                {
                    Success = false,
                    Errors = [new ApiErrorDto { Message = "Month is invalid." }]
                });

            // Act
            var result = await _controller.TriggerJob(0);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            Assert.False(success);

            var errors = value.GetType().GetProperty("errors")!.GetValue(value) as IEnumerable<object>;
            Assert.NotNull(errors);
            Assert.NotEmpty(errors);
        }

        [Fact]
        public async Task TriggerJob_OnFailure_AllErrorMessagesIncluded()
        {
            // Arrange
            _service.TriggerRecreateSummariesBatchJobAsync(0)
                .Returns(new ApiResponseDto<BatchJobEventTriggerDto>
                {
                    Success = false,
                    Errors =
                    [
                        new ApiErrorDto { Message = "Month is invalid." },
                        new ApiErrorDto { Message = "Job is already running." }
                    ]
                });

            // Act
            var result = await _controller.TriggerJob(0);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value!;
            var errors = (value.GetType().GetProperty("errors")!.GetValue(value) as IEnumerable<object>)!.ToList();
            Assert.Equal(2, errors.Count);
        }

        [Fact]
        public async Task TriggerJob_OnFailureWithNullErrors_ReturnsFallbackErrorMessage()
        {
            // Arrange
            _service.TriggerRecreateSummariesBatchJobAsync(6)
                .Returns(new ApiResponseDto<BatchJobEventTriggerDto>
                {
                    Success = false,
                    Errors = null
                });

            // Act
            var result = await _controller.TriggerJob(6);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value!;
            var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
            Assert.False(success);

            var errors = (value.GetType().GetProperty("errors")!.GetValue(value) as IEnumerable<object>)!.ToList();
            Assert.Single(errors);
        }

        [Fact]
        public async Task TriggerJob_OnSuccess_CallsServiceWithCorrectMonth()
        {
            // Arrange
            const int month = 9;
            var triggerDto = new BatchJobEventTriggerDto
            {
                Jobqueue = new BatchJobQueueDto { JobqueueId = Guid.NewGuid(), RequestedBy = "u", StartDateTime = DateTime.UtcNow },
                EventId = "e"
            };
            _service.TriggerRecreateSummariesBatchJobAsync(month)
                .Returns(new ApiResponseDto<BatchJobEventTriggerDto> { Success = true, Data = triggerDto });

            // Act
            await _controller.TriggerJob(month);

            // Assert
            await _service.Received(1).TriggerRecreateSummariesBatchJobAsync(month);
        }

        [Fact]
        public async Task TriggerJob_ServiceThrowsException_PropagatesException()
        {
            // Arrange
            _service.TriggerRecreateSummariesBatchJobAsync(Arg.Any<int>())
                .Returns(Task.FromException<ApiResponseDto<BatchJobEventTriggerDto>>(
                    new InvalidOperationException("Unexpected error")));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.TriggerJob(6));
        }

        #endregion
    }
}
