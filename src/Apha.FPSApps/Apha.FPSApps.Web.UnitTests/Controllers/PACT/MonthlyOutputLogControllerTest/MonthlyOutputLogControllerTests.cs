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
using IWorkGroupService = Apha.FPSApps.Application.Interfaces.PACT.IWorkGroupService;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.MonthlyOutputLogControllerTest
{
    public class MonthlyOutputLogControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IPactMonthlyOutputService _logService;
        private readonly IWorkGroupService _workGroupService;
        private readonly ITestorProductService _testorProductService;
        private readonly IProjectService _projectService;
        private readonly MonthlyOutputLogController _controller;

        public MonthlyOutputLogControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _logService = Substitute.For<IPactMonthlyOutputService>();
            _workGroupService = Substitute.For<IWorkGroupService>();
            _testorProductService = Substitute.For<ITestorProductService>();
            _projectService = Substitute.For<IProjectService>();
            _controller = new MonthlyOutputLogController(
                _mapper, _logService, _workGroupService, _testorProductService, _projectService);
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

        private void SetupLogItemMapper(List<MonthlyOutputLogDto> dtos, List<MonthlyOutputLogItem> items)
        {
            _mapper.Map<List<MonthlyOutputLogItem>>(dtos).Returns(items);
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

        private static ApiResponseDto<List<MonthlyOutputLogDto>> BuildLogResponse(int count = 2)
        {
            var data = Enumerable.Range(1, count)
                .Select(i => new MonthlyOutputLogDto { SequenceNo = i, TestCode = $"TC{i}", WorkGroup = "WG1", Buyer = "BuyerA" })
                .ToList();
            return ApiResponseDto<List<MonthlyOutputLogDto>>.SuccessResponse(data);
        }

        #endregion

        #region Index

        [Fact]
        public async Task Index_WithSuccessfulDependencies_ReturnsViewWithViewModel()
        {
            // Arrange
            var logResponse = BuildLogResponse(0);
            var workGroupResponse = BuildWorkGroupResponse();
            var testsResponse = BuildTestorProductResponse();
            var projectsResponse = BuildProjectResponse();

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyOutputLogFilterDto>())
                .Returns(logResponse);
            _workGroupService.GetAllWorkGroupsAsync().Returns(workGroupResponse);
            _testorProductService.GetAllTestorProductsAsync().Returns(testsResponse);
            _projectService.GetAllPactProjectsAsync().Returns(projectsResponse);
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, new List<MonthlyOutputLogItem>());
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyOutputLogViewModel>(viewResult.Model);
            Assert.NotNull(model.LogGrid);
            Assert.Single(model.WorkGroupOptions);
            Assert.Single(model.TestCodeOptions);
            Assert.Single(model.ProjectOptions);
        }

        [Fact]
        public async Task Index_WorkGroupServiceFails_ReturnsViewWithEmptyWorkGroupOptions()
        {
            // Arrange
            var logResponse = BuildLogResponse(0);
            var failResponse = ApiResponseDto<List<WorkGroupDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Error" } }, new ApiMetaDto());

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyOutputLogFilterDto>())
                .Returns(logResponse);
            _workGroupService.GetAllWorkGroupsAsync().Returns(failResponse);
            _testorProductService.GetAllTestorProductsAsync().Returns(BuildTestorProductResponse());
            _projectService.GetAllPactProjectsAsync().Returns(BuildProjectResponse());
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, new List<MonthlyOutputLogItem>());
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyOutputLogViewModel>(viewResult.Model);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_TestorProductServiceFails_ReturnsViewWithEmptyTestCodeOptions()
        {
            // Arrange
            var logResponse = BuildLogResponse(0);
            var failResponse = ApiResponseDto<List<TestorProductDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Error" } }, new ApiMetaDto());

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyOutputLogFilterDto>())
                .Returns(logResponse);
            _workGroupService.GetAllWorkGroupsAsync().Returns(BuildWorkGroupResponse());
            _testorProductService.GetAllTestorProductsAsync().Returns(failResponse);
            _projectService.GetAllPactProjectsAsync().Returns(BuildProjectResponse());
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, new List<MonthlyOutputLogItem>());
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyOutputLogViewModel>(viewResult.Model);
            Assert.Empty(model.TestCodeOptions);
        }

        [Fact]
        public async Task Index_ProjectServiceFails_ReturnsViewWithEmptyProjectOptions()
        {
            // Arrange
            var logResponse = BuildLogResponse(0);
            var failResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Error" } }, new ApiMetaDto());

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyOutputLogFilterDto>())
                .Returns(logResponse);
            _workGroupService.GetAllWorkGroupsAsync().Returns(BuildWorkGroupResponse());
            _testorProductService.GetAllTestorProductsAsync().Returns(BuildTestorProductResponse());
            _projectService.GetAllPactProjectsAsync().Returns(failResponse);
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, new List<MonthlyOutputLogItem>());
            SetupPaginationMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MonthlyOutputLogViewModel>(viewResult.Model);
            Assert.Empty(model.ProjectOptions);
        }

        #endregion

        #region Search

        [Fact]
        public async Task Search_WithValidCriteria_ReturnsPartialViewWithDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var logResponse = BuildLogResponse(2);
            var items = logResponse.Data!.Select(d => new MonthlyOutputLogItem { SequenceNo = d.SequenceNo }).ToList();

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyOutputLogFilterDto>())
                .Returns(logResponse);
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, items);
            SetupPaginationMapper();

            // Act
            var result = await _controller.Search(request, "WG1", null, null, null, null, null, null, null);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            var model = Assert.IsType<DataGridConfig<MonthlyOutputLogItem>>(partialViewResult.Model);
            Assert.Equal(2, model.Data.Count);
        }

        [Fact]
        public async Task Search_WithNoSearchCriteria_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };

            // Act
            var result = await _controller.Search(request, null, null, null, null, null, null, null, null);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            var model = Assert.IsType<DataGridConfig<MonthlyOutputLogItem>>(partialViewResult.Model);
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
        public async Task Search_WithWorkGroupCriteria_CallsLogServiceWithFilter()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var logResponse = BuildLogResponse(1);
            var items = logResponse.Data!.Select(d => new MonthlyOutputLogItem { SequenceNo = d.SequenceNo }).ToList();

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyOutputLogFilterDto>())
                .Returns(logResponse);
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, items);
            SetupPaginationMapper();

            // Act
            await _controller.Search(request, "WG1", null, null, null, null, null, null, null);

            // Assert
            await _logService.Received(1).SearchAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Is<MonthlyOutputLogFilterDto>(f => f.WorkGroup == "WG1"));
        }

        [Fact]
        public async Task Search_WithBuyingTestProvided_UsesBuyingTestAsBuyer()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var logResponse = BuildLogResponse(1);
            var items = logResponse.Data!.Select(d => new MonthlyOutputLogItem { SequenceNo = d.SequenceNo }).ToList();

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyOutputLogFilterDto>())
                .Returns(logResponse);
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, items);
            SetupPaginationMapper();

            // Act
            await _controller.Search(request, null, null, "DirectBuyer", "BuyingTest", null, null, null, null);

            // Assert — buyingTest takes precedence over buyer
            await _logService.Received(1).SearchAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Is<MonthlyOutputLogFilterDto>(f => f.Buyer == "BuyingTest"));
        }

        [Fact]
        public async Task Search_WithBuyerButNoBuyingTest_UsesBuyerDirectly()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var logResponse = BuildLogResponse(1);
            var items = logResponse.Data!.Select(d => new MonthlyOutputLogItem { SequenceNo = d.SequenceNo }).ToList();

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyOutputLogFilterDto>())
                .Returns(logResponse);
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, items);
            SetupPaginationMapper();

            // Act
            await _controller.Search(request, null, null, "DirectBuyer", null, null, null, null, null);

            // Assert
            await _logService.Received(1).SearchAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Is<MonthlyOutputLogFilterDto>(f => f.Buyer == "DirectBuyer"));
        }

        [Fact]
        public async Task Search_WithAllFilterCriteria_PassesAllFiltersToService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var dateImported = new DateTime(2024, 1, 15);
            var logResponse = BuildLogResponse(1);
            var items = logResponse.Data!.Select(d => new MonthlyOutputLogItem { SequenceNo = d.SequenceNo }).ToList();

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyOutputLogFilterDto>())
                .Returns(logResponse);
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, items);
            SetupPaginationMapper();

            // Act
            await _controller.Search(request, "WG1", "TC1", "BuyerA", null, dateImported, 1.0, "user1", "I");

            // Assert
            await _logService.Received(1).SearchAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Is<MonthlyOutputLogFilterDto>(f =>
                    f.WorkGroup == "WG1" &&
                    f.TestCode == "TC1" &&
                    f.Buyer == "BuyerA" &&
                    f.DateImported == dateImported &&
                    f.Month == 1.0 &&
                    f.UserId == "user1" &&
                    f.InsertDelete == "I"));
        }

        [Fact]
        public async Task Search_WithEmptyResult_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var emptyResponse = ApiResponseDto<List<MonthlyOutputLogDto>>.SuccessResponse(new List<MonthlyOutputLogDto>());

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyOutputLogFilterDto>())
                .Returns(emptyResponse);
            SetupQueryParametersMapper();
            _mapper.Map<List<MonthlyOutputLogItem>>(Arg.Any<List<MonthlyOutputLogDto>>())
                .Returns(new List<MonthlyOutputLogItem>());
            SetupPaginationMapper();

            // Act
            var result = await _controller.Search(request, "WG1", null, null, null, null, null, null, null);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<MonthlyOutputLogItem>>(partialViewResult.Model);
            Assert.Empty(model.Data);
        }

        [Fact]
        public async Task Search_GridConfig_HasExpectedProperties()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var logResponse = BuildLogResponse(1);
            var items = logResponse.Data!.Select(d => new MonthlyOutputLogItem { SequenceNo = d.SequenceNo }).ToList();

            _logService.SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyOutputLogFilterDto>())
                .Returns(logResponse);
            SetupQueryParametersMapper();
            SetupLogItemMapper(logResponse.Data!, items);
            SetupPaginationMapper();

            // Act
            var result = await _controller.Search(request, "WG1", null, null, null, null, null, null, null);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<MonthlyOutputLogItem>>(partialViewResult.Model);
            Assert.Equal("moLogGrid", model.GridId);
            Assert.Equal("Monthly Output Log", model.Title);
            Assert.False(model.AllowAdd);
            Assert.False(model.AllowEdit);
            Assert.False(model.AllowDelete);
            Assert.True(model.ShowPagination);
            Assert.Equal("/PACT/MonthlyOutputLog/Search", model.BindGridUrl);
        }

        #endregion
    }
}
