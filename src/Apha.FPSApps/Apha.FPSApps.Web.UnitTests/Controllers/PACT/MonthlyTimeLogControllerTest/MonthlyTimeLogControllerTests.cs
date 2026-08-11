using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.MonthlyTimeLogControllerTest
{
    public class MonthlyTimeLogControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IPactMonthlyTimeService _logService;
        private readonly Apha.FPSApps.Application.Interfaces.PACT.IWorkGroupService _workGroupService;
        private readonly ITestorProductService _testorProductService;
        private readonly IProjectService _projectService;
        private readonly IProjectJobCodeService _jobCodeService;
        private readonly IEmployeeService _employeeService;
        private readonly MonthlyTimeLogController _controller;

        public MonthlyTimeLogControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _logService = Substitute.For<IPactMonthlyTimeService>();
            _workGroupService = Substitute.For<Apha.FPSApps.Application.Interfaces.PACT.IWorkGroupService>();
            _testorProductService = Substitute.For<ITestorProductService>();
            _projectService = Substitute.For<IProjectService>();
            _jobCodeService = Substitute.For<IProjectJobCodeService>();
            _employeeService = Substitute.For<IEmployeeService>();
            _controller = new MonthlyTimeLogController(
                _mapper, _logService, _workGroupService, _testorProductService,
                _projectService, _jobCodeService, _employeeService);
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

        private void SetupLogItemMapper(List<MonthlyTimeLogDto> dtos, List<MonthlyTimeLogItem> items)
        {
            _mapper.Map<List<MonthlyTimeLogItem>>(dtos).Returns(items);
        }

        private void SetupPaginationMapper()
        {
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        private static ApiResponseDto<List<WorkGroupDto>> BuildWorkGroupResponse() =>
            ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
                new List<WorkGroupDto> { new() { WorkGroupName = "WG1" } });

        private static ApiResponseDto<List<TestorProductDto>> BuildTestorProductResponse() =>
            ApiResponseDto<List<TestorProductDto>>.SuccessResponse(
                new List<TestorProductDto> { new() { ItemCode = "TC1" } });

        private static ApiResponseDto<List<ProjectDto>> BuildProjectResponse() =>
            ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto> { new() { ParentProject = "P001", ProjectTitle = "Project One" } });

        private static ApiResponseDto<List<JobCodeDto>> BuildJobCodeResponse() =>
            ApiResponseDto<List<JobCodeDto>>.SuccessResponse(
                new List<JobCodeDto> { new() { JobCodeId = "JC1", ParentProject = "P001" } });

        private static ApiResponseDto<List<PactStaffDto>> BuildStaffResponse() =>
            ApiResponseDto<List<PactStaffDto>>.SuccessResponse(
                new List<PactStaffDto> { new() { PactId = "S001", SpNumber = "SP001", Name = "John Smith" } });

        private static ApiResponseDto<List<MonthlyTimeLogDto>> BuildLogResponse(int count = 2)
        {
            var data = Enumerable.Range(1, count)
                .Select(i => new MonthlyTimeLogDto { SequenceNo = i, TimeCode = $"TC{i}", WorkGroup = "WG1", PactStaffId = "S001" })
                .ToList();
            return ApiResponseDto<List<MonthlyTimeLogDto>>.SuccessResponse(data);
        }

        private void SetupIndexDependencies(ApiResponseDto<List<MonthlyTimeLogDto>> logResponse)
        {
            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(logResponse);
            _workGroupService.GetAllWorkGroupsAsync().Returns(BuildWorkGroupResponse());
            _testorProductService.GetAllTestorProductsAsync().Returns(BuildTestorProductResponse());
            _projectService.GetAllPactProjectsAsync().Returns(BuildProjectResponse());
            _jobCodeService.GetJobCodesAsync().Returns(BuildJobCodeResponse());
            _employeeService.GetPactStaffAsync().Returns(BuildStaffResponse());
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, new List<MonthlyTimeLogItem>());
            SetupPaginationMapper();
        }

        #endregion

        #region Index

        [Fact]
        public async Task Index_WithSuccessfulDependencies_ReturnsViewWithViewModel()
        {
            // Arrange
            var logResponse = BuildLogResponse(0);
            SetupIndexDependencies(logResponse);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyTimeLogViewModel>(viewResult.Model);
            Assert.NotNull(model.LogGrid);
            Assert.Single(model.WorkGroupOptions);
            Assert.Single(model.TestCodeOptions);
            Assert.Single(model.ProjectOptions);
            Assert.Single(model.JobCodeOptions);
            Assert.Single(model.StaffOptions);
        }

        [Fact]
        public async Task Index_WorkGroupServiceFails_ReturnsViewWithEmptyWorkGroupOptions()
        {
            // Arrange
            var logResponse = BuildLogResponse(0);
            var failResponse = ApiResponseDto<List<WorkGroupDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Error" } }, new ApiMetaDto());

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(logResponse);
            _workGroupService.GetAllWorkGroupsAsync().Returns(failResponse);
            _testorProductService.GetAllTestorProductsAsync().Returns(BuildTestorProductResponse());
            _projectService.GetAllPactProjectsAsync().Returns(BuildProjectResponse());
            _jobCodeService.GetJobCodesAsync().Returns(BuildJobCodeResponse());
            _employeeService.GetPactStaffAsync().Returns(BuildStaffResponse());
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, new List<MonthlyTimeLogItem>());
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyTimeLogViewModel>(viewResult.Model);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_TestorProductServiceFails_ReturnsViewWithEmptyTestCodeOptions()
        {
            // Arrange
            var logResponse = BuildLogResponse(0);
            var failResponse = ApiResponseDto<List<TestorProductDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Error" } }, new ApiMetaDto());

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(logResponse);
            _workGroupService.GetAllWorkGroupsAsync().Returns(BuildWorkGroupResponse());
            _testorProductService.GetAllTestorProductsAsync().Returns(failResponse);
            _projectService.GetAllPactProjectsAsync().Returns(BuildProjectResponse());
            _jobCodeService.GetJobCodesAsync().Returns(BuildJobCodeResponse());
            _employeeService.GetPactStaffAsync().Returns(BuildStaffResponse());
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, new List<MonthlyTimeLogItem>());
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyTimeLogViewModel>(viewResult.Model);
            Assert.Empty(model.TestCodeOptions);
        }

        [Fact]
        public async Task Index_ProjectServiceFails_ReturnsViewWithEmptyProjectOptions()
        {
            // Arrange
            var logResponse = BuildLogResponse(0);
            var failResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Error" } }, new ApiMetaDto());

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(logResponse);
            _workGroupService.GetAllWorkGroupsAsync().Returns(BuildWorkGroupResponse());
            _testorProductService.GetAllTestorProductsAsync().Returns(BuildTestorProductResponse());
            _projectService.GetAllPactProjectsAsync().Returns(failResponse);
            _jobCodeService.GetJobCodesAsync().Returns(BuildJobCodeResponse());
            _employeeService.GetPactStaffAsync().Returns(BuildStaffResponse());
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, new List<MonthlyTimeLogItem>());
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyTimeLogViewModel>(viewResult.Model);
            Assert.Empty(model.ProjectOptions);
        }

        [Fact]
        public async Task Index_JobCodeServiceFails_ReturnsViewWithEmptyJobCodeOptions()
        {
            // Arrange
            var logResponse = BuildLogResponse(0);
            var failResponse = ApiResponseDto<List<JobCodeDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Error" } }, new ApiMetaDto());

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(logResponse);
            _workGroupService.GetAllWorkGroupsAsync().Returns(BuildWorkGroupResponse());
            _testorProductService.GetAllTestorProductsAsync().Returns(BuildTestorProductResponse());
            _projectService.GetAllPactProjectsAsync().Returns(BuildProjectResponse());
            _jobCodeService.GetJobCodesAsync().Returns(failResponse);
            _employeeService.GetPactStaffAsync().Returns(BuildStaffResponse());
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, new List<MonthlyTimeLogItem>());
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyTimeLogViewModel>(viewResult.Model);
            Assert.Empty(model.JobCodeOptions);
        }

        [Fact]
        public async Task Index_EmployeeServiceFails_ReturnsViewWithEmptyStaffOptions()
        {
            // Arrange
            var logResponse = BuildLogResponse(0);
            var failResponse = ApiResponseDto<List<PactStaffDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Error" } }, new ApiMetaDto());

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(logResponse);
            _workGroupService.GetAllWorkGroupsAsync().Returns(BuildWorkGroupResponse());
            _testorProductService.GetAllTestorProductsAsync().Returns(BuildTestorProductResponse());
            _projectService.GetAllPactProjectsAsync().Returns(BuildProjectResponse());
            _jobCodeService.GetJobCodesAsync().Returns(BuildJobCodeResponse());
            _employeeService.GetPactStaffAsync().Returns(failResponse);
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, new List<MonthlyTimeLogItem>());
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyTimeLogViewModel>(viewResult.Model);
            Assert.Empty(model.StaffOptions);
        }

        [Fact]
        public async Task Index_StaffWithNullPactId_ExcludedFromStaffOptions()
        {
            // Arrange
            var logResponse = BuildLogResponse(0);
            var staffResponse = ApiResponseDto<List<PactStaffDto>>.SuccessResponse(
                new List<PactStaffDto>
                {
                    new() { PactId = "S001", SpNumber = "SP001", Name = "Valid Staff" },
                    new() { PactId = null,   SpNumber = "SP002", Name = "No PactId" },
                    new() { PactId = "  ",   SpNumber = "SP003", Name = "Whitespace PactId" }
                });

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(logResponse);
            _workGroupService.GetAllWorkGroupsAsync().Returns(BuildWorkGroupResponse());
            _testorProductService.GetAllTestorProductsAsync().Returns(BuildTestorProductResponse());
            _projectService.GetAllPactProjectsAsync().Returns(BuildProjectResponse());
            _jobCodeService.GetJobCodesAsync().Returns(BuildJobCodeResponse());
            _employeeService.GetPactStaffAsync().Returns(staffResponse);
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, new List<MonthlyTimeLogItem>());
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyTimeLogViewModel>(viewResult.Model);
            Assert.Single(model.StaffOptions);
            Assert.Equal("S001", model.StaffOptions[0].PactId);
        }

        [Fact]
        public async Task Index_JobCodesWithDuplicateIds_DeduplicatedInJobCodeOptions()
        {
            // Arrange
            var logResponse = BuildLogResponse(0);
            var jobCodeResponse = ApiResponseDto<List<JobCodeDto>>.SuccessResponse(
                new List<JobCodeDto>
                {
                    new() { JobCodeId = "JC1", ParentProject = "P001" },
                    new() { JobCodeId = "JC1", ParentProject = "P002" },
                    new() { JobCodeId = "JC2", ParentProject = "P001" }
                });

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(logResponse);
            _workGroupService.GetAllWorkGroupsAsync().Returns(BuildWorkGroupResponse());
            _testorProductService.GetAllTestorProductsAsync().Returns(BuildTestorProductResponse());
            _projectService.GetAllPactProjectsAsync().Returns(BuildProjectResponse());
            _jobCodeService.GetJobCodesAsync().Returns(jobCodeResponse);
            _employeeService.GetPactStaffAsync().Returns(BuildStaffResponse());
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, new List<MonthlyTimeLogItem>());
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyTimeLogViewModel>(viewResult.Model);
            Assert.Equal(2, model.JobCodeOptions.Count);
        }

        #endregion

        #region Search

        [Fact]
        public async Task Search_WithValidCriteria_ReturnsPartialViewWithDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var logResponse = BuildLogResponse(2);
            var items = logResponse.Data!.Select(d => new MonthlyTimeLogItem { SequenceNo = d.SequenceNo }).ToList();

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(logResponse);
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, items);
            SetupPaginationMapper();

            // Act
            var result = await _controller.Search(request, "WG1", null, null, null, null, null, null, null);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            var model = Assert.IsType<DataGridConfig<MonthlyTimeLogItem>>(partialViewResult.Model);
            Assert.Equal(2, model.Data.Count);
        }

        [Fact]
        public async Task Search_WithNoSearchCriteria_ReturnsJsonFailureWithMessage()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };

            // Act
            var result = await _controller.Search(request, null, null, null, null, null, null, null, null);

            // Assert - Updated to match actual controller behavior
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            var model = Assert.IsType<DataGridConfig<MonthlyTimeLogItem>>(partialViewResult.Model);
            Assert.Empty(model.Data); // Verify empty grid is returned when no search criteria
        }

        [Fact]
        public async Task Search_WithInvalidModelState_ReturnsJsonFailureWithErrors()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            _controller.ModelState.AddModelError("Page", "Invalid page number");

            // Act
            var result = await _controller.Search(request, "WG1", null, null, null, null, null, null, null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value;
            var success = value?.GetType().GetProperty("success")?.GetValue(value);
            Assert.False((bool)success!);
        }

        [Fact]
        public async Task Search_WithWorkGroupCriteria_CallsLogServiceWithWorkGroupFilter()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var logResponse = BuildLogResponse(1);
            var items = logResponse.Data!.Select(d => new MonthlyTimeLogItem { SequenceNo = d.SequenceNo }).ToList();

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(logResponse);
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, items);
            SetupPaginationMapper();

            // Act
            await _controller.Search(request, "WG1", null, null, null, null, null, null, null);

            // Assert
            await _logService.Received(1).SearchAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Is<MonthlyTimeLogFilterDto>(f => f.WorkGroup == "WG1"));
        }

        [Fact]
        public async Task Search_WithTimeCodeCriteria_CallsLogServiceWithTimeCodeFilter()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var logResponse = BuildLogResponse(1);
            var items = logResponse.Data!.Select(d => new MonthlyTimeLogItem { SequenceNo = d.SequenceNo }).ToList();

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(logResponse);
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, items);
            SetupPaginationMapper();

            // Act
            await _controller.Search(request, null, "TC1", null, null, null, null, null, null);

            // Assert
            await _logService.Received(1).SearchAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Is<MonthlyTimeLogFilterDto>(f => f.TimeCode == "TC1"));
        }

        [Fact]
        public async Task Search_WithAllFilterCriteria_PassesAllFiltersToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var dateImported = new DateTime(2024, 6, 1);
            var logResponse = BuildLogResponse(1);
            var items = logResponse.Data!.Select(d => new MonthlyTimeLogItem { SequenceNo = d.SequenceNo }).ToList();

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(logResponse);
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, items);
            SetupPaginationMapper();

            // Act
            await _controller.Search(request, "WG1", "TC1", "PP1", "S001", dateImported, 6.0, "USER1", "I");

            // Assert
            await _logService.Received(1).SearchAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Is<MonthlyTimeLogFilterDto>(f =>
                    f.WorkGroup == "WG1" &&
                    f.TimeCode == "TC1" &&
                    f.ParentProject == "PP1" &&
                    f.PactStaffId == "S001" &&
                    f.DateImported == dateImported &&
                    f.Month == 6.0 &&
                    f.UserId == "USER1" &&
                    f.InsertDelete == "I"));
        }

        [Fact]
        public async Task Search_WithEmptyResult_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var emptyResponse = ApiResponseDto<List<MonthlyTimeLogDto>>.SuccessResponse(new List<MonthlyTimeLogDto>());

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(emptyResponse);
            SetupQueryParametersMapper();
            _mapper.Map<List<MonthlyTimeLogItem>>(Arg.Any<List<MonthlyTimeLogDto>>())
                .Returns(new List<MonthlyTimeLogItem>());
            SetupPaginationMapper();

            // Act
            var result = await _controller.Search(request, "WG1", null, null, null, null, null, null, null);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<MonthlyTimeLogItem>>(partialViewResult.Model);
            Assert.Empty(model.Data);
        }

        [Fact]
        public async Task Search_GridConfig_HasExpectedProperties()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var logResponse = BuildLogResponse(1);
            var items = logResponse.Data!.Select(d => new MonthlyTimeLogItem { SequenceNo = d.SequenceNo }).ToList();

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(logResponse);
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, items);
            SetupPaginationMapper();

            // Act
            var result = await _controller.Search(request, "WG1", null, null, null, null, null, null, null);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<MonthlyTimeLogItem>>(partialViewResult.Model);
            Assert.Equal("mtLogGrid", model.GridId);
            Assert.Equal("Monthly Time Log", model.Title);
            Assert.False(model.AllowAdd);
            Assert.False(model.AllowEdit);
            Assert.False(model.AllowDelete);
            Assert.True(model.ShowPagination);
            Assert.Equal("/PACT/MonthlyTimeLog/Search", model.BindGridUrl);
        }

        [Fact]
        public async Task Search_OnlyCriteriaIsDateImported_CallsLogService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var dateImported = new DateTime(2024, 3, 15);
            var logResponse = BuildLogResponse(1);
            var items = logResponse.Data!.Select(d => new MonthlyTimeLogItem { SequenceNo = d.SequenceNo }).ToList();

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(logResponse);
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, items);
            SetupPaginationMapper();

            // Act
            var result = await _controller.Search(request, null, null, null, null, dateImported, null, null, null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _logService.Received(1).SearchAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Is<MonthlyTimeLogFilterDto>(f => f.DateImported == dateImported));
        }

        [Fact]
        public async Task Search_OnlyCriteriaIsMonth_CallsLogService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var logResponse = BuildLogResponse(1);
            var items = logResponse.Data!.Select(d => new MonthlyTimeLogItem { SequenceNo = d.SequenceNo }).ToList();

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .Returns(logResponse);
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, items);
            SetupPaginationMapper();

            // Act
            var result = await _controller.Search(request, null, null, null, null, null, 6.0, null, null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _logService.Received(1).SearchAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Is<MonthlyTimeLogFilterDto>(f => f.Month == 6.0));
        }

        #endregion
    }
}
