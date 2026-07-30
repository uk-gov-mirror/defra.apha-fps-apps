using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.WorkGroupTestCapabilityControllerTest
{
    public class WorkGroupTestCapabilityControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupService _workGroupService;
        private readonly ITestCapabilityService _testCapabilityService;
        private readonly WorkGroupTestCapabilityController _controller;

        public WorkGroupTestCapabilityControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _workGroupService = Substitute.For<IWorkGroupService>();
            _testCapabilityService = Substitute.For<ITestCapabilityService>();
            _controller = new WorkGroupTestCapabilityController(
                _mapper,
                _workGroupService,
                _testCapabilityService);

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Substitute.For<ITempDataProvider>());
            _controller.TempData = tempData;
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        private void SetupWorkGroupsResponse(List<WorkGroupDto> workGroups)
        {
            _workGroupService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(workGroups));
            _mapper.Map<List<WorkGroup>>(Arg.Any<List<WorkGroupDto>>())
                .Returns(workGroups.Select(w => new WorkGroup { WorkGroupName = w.WorkGroupName }).ToList());
        }

        private void SetupPagedTestCapabilityResponse(List<TestCapabilityDto> testCapabilities, PaginationDto? pagination = null)
        {
            _testCapabilityService.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse(
                    testCapabilities,
                    pagination ?? new PaginationDto()));
        }

        private void SetupMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<WorkGroupTestCapabilityItem>>(Arg.Any<List<TestCapabilityDto>>())
                .Returns(new List<WorkGroupTestCapabilityItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        #region Index

        [Fact]
        public async Task Index_WithWorkGroups_ReturnsViewWithWorkGroupOptions()
        {
            // Arrange
            var workGroups = new List<WorkGroupDto>
            {
                new() { WorkGroupName = "WG001" },
                new() { WorkGroupName = "WG002" },
                new() { WorkGroupName = "WG003" }
            };
            SetupWorkGroupsResponse(workGroups);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupTestCapabilityViewModel>(viewResult.Model);
            Assert.NotNull(model.WorkGroupOptions);
            Assert.Equal(3, model.WorkGroupOptions.Count);
            Assert.NotNull(model.TestCapabilityGrid);
        }

        [Fact]
        public async Task Index_WithNoWorkGroups_ReturnsViewWithEmptyWorkGroupOptions()
        {
            // Arrange
            SetupWorkGroupsResponse(new List<WorkGroupDto>());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupTestCapabilityViewModel>(viewResult.Model);
            Assert.NotNull(model.WorkGroupOptions);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_WithFailedWorkGroupsResponse_ReturnsViewWithEmptyWorkGroupOptions()
        {
            // Arrange
            _workGroupService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Code = "ERROR", Message = "Service error" } },
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow }));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupTestCapabilityViewModel>(viewResult.Model);
            Assert.NotNull(model.WorkGroupOptions);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_WithNullWorkGroupsData_ReturnsViewWithEmptyWorkGroupOptions()
        {
            // Arrange
            _workGroupService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(null!));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupTestCapabilityViewModel>(viewResult.Model);
            Assert.NotNull(model.WorkGroupOptions);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_Always_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            SetupWorkGroupsResponse(new List<WorkGroupDto>());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupTestCapabilityViewModel>(viewResult.Model);
            Assert.NotNull(model.TestCapabilityGrid);
            Assert.Equal("testCapabilitiesWGGrid", model.TestCapabilityGrid.GridId);
            Assert.Empty(model.TestCapabilityGrid.Data);
        }

        [Fact]
        public async Task Index_WithWorkGroups_SetsCorrectWorkGroupName()
        {
            // Arrange
            var workGroups = new List<WorkGroupDto>
            {
                new() { WorkGroupName = "TestWorkGroup" }
            };
            SetupWorkGroupsResponse(workGroups);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupTestCapabilityViewModel>(viewResult.Model);
            var firstItem = model.WorkGroupOptions.First();
            Assert.Equal("TestWorkGroup", firstItem.WorkGroupName);
        }

        #endregion

        #region LoadTestCapabilityGrid

        [Fact]
        public async Task LoadTestCapabilityGrid_WithValidRequest_ReturnsPartialViewWithGrid()
        {
            // Arrange
            const string workGroup = "WG001";
            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{}"
            };
            var testCapabilities = new List<TestCapabilityDto>
            {
                new() { TestCode = "TC001", PlanPortfolio = "Portfolio1" },
                new() { TestCode = "TC002", PlanPortfolio = "Portfolio2" }
            };
            SetupPagedTestCapabilityResponse(testCapabilities);
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, workGroup);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithNullWorkGroup_ReturnsPartialViewWithAllData()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{}"
            };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, null);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _controller.ModelState.AddModelError("Filter", "Invalid filter");

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value;
            Assert.NotNull(value);
            var successProperty = value!.GetType().GetProperty("success");
            Assert.NotNull(successProperty);
            Assert.False((bool)successProperty.GetValue(value)!);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithEmptyResult_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.Empty(model.Data);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithPaginationData_ReturnsGridWithPagination()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 2,
                PageSize = 10,
                Filter = "{}",
                SortBy = "TestCode",
                Descending = true
            };
            var pagination = new PaginationDto
            {
                TotalRecords = 50,
                PageNumber = 2,
                PageSize = 10,
                TotalPages = 5
            };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>(), pagination);
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.NotNull(model.Pagination);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithFilteredData_CallsServiceWithCorrectParameters()
        {
            // Arrange
            const string workGroup = "WG001";
            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"TestCode\":\"TC001\"}"
            };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            await _controller.LoadTestCapabilityGrid(request, workGroup);

            // Assert
            await _testCapabilityService.Received(1).GetPagedByWorkGroupAsync(
                Arg.Any<QueryParameters<string>>(),
                workGroup);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithNullFilter_UsesEmptyDictionary()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = null
            };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.NotNull(model.CurrentFilters);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithEmptyFilterString_UsesEmptyDictionary()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.NotNull(model.CurrentFilters);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithNullPagination_CreatesDefaultPaginationModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            _testCapabilityService.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse(new List<TestCapabilityDto>(), null));
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.NotNull(model.Pagination);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithFailedServiceResponse_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            _testCapabilityService.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Code = "ERROR", Message = "Service error" } },
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow }));
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.Empty(model.Data);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithSpecialCharactersInWorkGroup_HandlesCorrectly()
        {
            // Arrange
            const string workGroup = "WG-001/Test&Group";
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, workGroup);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.NotNull(partialViewResult);
            await _testCapabilityService.Received(1).GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), workGroup);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WhenExceptionThrown_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            _mapper.When(x => x.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()))
                .Do(x => throw new Exception("Mapping error"));

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value;
            Assert.NotNull(value);
            var successProperty = value!.GetType().GetProperty("success");
            Assert.NotNull(successProperty);
            Assert.False((bool)successProperty.GetValue(value)!);

            var messageProperty = value.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            Assert.Equal("An error occurred while loading the grid", messageProperty.GetValue(value));
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WhenServiceThrowsException_ReturnsJsonErrorWithExceptionMessage()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var query = new QueryParameters<string>();
            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _testCapabilityService.GetPagedByWorkGroupAsync(query, "WG001")
                .Returns<ApiResponseDto<List<TestCapabilityDto>>>(x => throw new InvalidOperationException("Service unavailable"));

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = jsonResult.Value;
            Assert.NotNull(value);

            var errorsProperty = value!.GetType().GetProperty("errors");
            Assert.NotNull(errorsProperty);
            var errors = errorsProperty.GetValue(value) as string[];
            Assert.NotNull(errors);
            Assert.Contains("Service unavailable", errors);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_WithInvalidJsonFilter_HandlesGracefully()
        {
            // Arrange
            var request = new PaginationFilter<string> 
            { 
                Page = 1, 
                PageSize = 10, 
                Filter = "{invalid json}" 
            };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            // Should handle gracefully and return partial view (ParseFilterDictionary catches JsonException)
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
        }

        #endregion

        #region Grid Configuration Tests

        [Fact]
        public async Task LoadTestCapabilityGrid_ConfiguresGridWithCorrectProperties()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.Equal("testCapabilitiesWGGrid", model.GridId);
            Assert.Equal("TestCode", model.KeyProperty);
            Assert.True(model.AllowRowSelection);
            Assert.Equal("onTestCapabilityRowSelect", model.RowSelectFunction);
            Assert.Equal("/PACT/WorkGroupTestCapability/LoadTestCapabilityGrid", model.BindGridUrl);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_ConfiguresGridWithFilterMethod()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.Equal("getTestCapabilityExtraFilters", model.ExtraFilterMethod);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_DisablesExportEditDeleteFlags()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.False(model.AllowExport);
            Assert.False(model.AllowEdit);
            Assert.False(model.AllowDelete);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_DoesNotSetCRUDFunctions()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.True(string.IsNullOrEmpty(model.AddFunction));
            Assert.True(string.IsNullOrEmpty(model.EditFunction));
            Assert.True(string.IsNullOrEmpty(model.DeleteFunction));
            Assert.True(string.IsNullOrEmpty(model.ExportUrl));
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_EnablesPaginationAndRowSelection()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.True(model.ShowPagination);
            Assert.True(model.AllowRowSelection);
            Assert.False(model.ShowCheckboxColumn);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_SetsSortColumnsFromRequest()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{}",
                SortBy = "TestCode",
                Descending = true
            };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partialViewResult.Model);
            Assert.Equal("TestCode", model.Pagination.SortColumn);
            Assert.True(model.Pagination.SortDirection);
        }

        #endregion

        #region Service Integration Tests

        [Fact]
        public async Task LoadTestCapabilityGrid_CallsGetPagedByWorkGroupAsync()
        {
            // Arrange
            const string workGroup = "WG001";
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            await _controller.LoadTestCapabilityGrid(request, workGroup);

            // Assert
            await _testCapabilityService.Received(1).GetPagedByWorkGroupAsync(
                Arg.Any<QueryParameters<string>>(),
                workGroup);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_MapsRequestToQueryParameters()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>());
            SetupMapper();

            // Act
            await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            _mapper.Received(1).Map<QueryParameters<string>>(request);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_MapsResponseDataToItems()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var testCapabilities = new List<TestCapabilityDto>
            {
                new() { TestCode = "TC001" }
            };
            SetupPagedTestCapabilityResponse(testCapabilities);
            SetupMapper();

            // Act
            await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            _mapper.Received(1).Map<List<WorkGroupTestCapabilityItem>>(testCapabilities);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_MapsPaginationDto()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var pagination = new PaginationDto
            {
                PageNumber = 1,
                PageSize = 10,
                TotalPages = 5,
                TotalRecords = 50
            };
            SetupPagedTestCapabilityResponse(new List<TestCapabilityDto>(), pagination);
            SetupMapper();

            // Act
            await _controller.LoadTestCapabilityGrid(request, "WG001");

            // Assert
            _mapper.Received(1).Map<PaginationModel>(pagination);
        }

        #endregion
    }
}
