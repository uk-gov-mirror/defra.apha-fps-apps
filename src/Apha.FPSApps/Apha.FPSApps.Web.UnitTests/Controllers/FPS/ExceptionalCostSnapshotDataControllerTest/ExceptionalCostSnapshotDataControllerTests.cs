using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ExceptionalCostSnapshotDataControllerTest
{
    public class ExceptionalCostSnapshotDataControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly ExceptionalCostSnapshotDataController _controller;

        public ExceptionalCostSnapshotDataControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _projectService = Substitute.For<IProjectService>();
            _controller = new ExceptionalCostSnapshotDataController(_mapper, _projectService);
        }

        private static ProjectExceptionalCostViewDto BuildDto(string project = "PP001") =>
            new()
            {
                Directorate = "DIR1",
                Programme = "P001",
                ContractNumber = "CON1",
                Project = project,
                AccountCat = "ACC1",
                Description = "Travel",
                ItemCost = 250.50m
            };

        #region Index Tests

        [Fact]
        public void Index_ReturnsViewWithEmptyGridConfig()
        {
            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ExceptionalCostSnapshotViewModel>(viewResult.Model);
            Assert.Equal("exceptionalCostSnapshotGrid", model.ExceptionalCostSnapshotGrid.GridId);
            Assert.Empty(model.ExceptionalCostSnapshotGrid.Data);
            Assert.True(model.ExceptionalCostSnapshotGrid.ShowPagination);
            Assert.False(model.ExceptionalCostSnapshotGrid.AllowAdd);
            Assert.False(model.ExceptionalCostSnapshotGrid.AllowEdit);
            Assert.False(model.ExceptionalCostSnapshotGrid.AllowDelete);
        }

        #endregion

        #region LoadExceptionalCostDataGrid Tests

        [Fact]
        public async Task LoadExceptionalCostDataGrid_WithValidRequest_ReturnsPartialViewWithDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, SortBy = "Directorate", Descending = true };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<ProjectExceptionalCostViewDto> { BuildDto() };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var serviceResponse = ApiResponseDto<List<ProjectExceptionalCostViewDto>>.SuccessResponse(dtos, paginationDto);
            var items = new List<ExceptionalCostSnapshotItem> { new() { Directorate = "DIR1", Project = "PP001" } };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectExceptionalCostsPagedAsync(queryParameters).Returns(serviceResponse);
            _mapper.Map<List<ExceptionalCostSnapshotItem>>(Arg.Any<List<ProjectExceptionalCostViewDto>>()).Returns(items);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadExceptionalCostDataGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<ExceptionalCostSnapshotItem>>(partialView.Model);
            Assert.Equal("exceptionalCostSnapshotGrid", gridConfig.GridId);
            Assert.Single(gridConfig.Data);
            Assert.Equal("Directorate", gridConfig.Pagination!.SortColumn);
            Assert.True(gridConfig.Pagination.SortDirection);
            await _projectService.Received(1).GetProjectExceptionalCostsPagedAsync(queryParameters);
        }

        [Fact]
        public async Task LoadExceptionalCostDataGrid_WhenServiceReturnsFailure_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var serviceResponse = ApiResponseDto<List<ProjectExceptionalCostViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectExceptionalCostsPagedAsync(queryParameters).Returns(serviceResponse);

            // Act
            var result = await _controller.LoadExceptionalCostDataGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ExceptionalCostSnapshotItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
            _mapper.DidNotReceive().Map<List<ExceptionalCostSnapshotItem>>(Arg.Any<List<ProjectExceptionalCostViewDto>>());
        }

        [Fact]
        public async Task LoadExceptionalCostDataGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _controller.ModelState.AddModelError("Page", "Invalid");

            // Act
            var result = await _controller.LoadExceptionalCostDataGrid(request);

            // Assert
            Assert.IsType<JsonResult>(result);
            await _projectService.DidNotReceive().GetProjectExceptionalCostsPagedAsync(Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadExceptionalCostDataGrid_WithFilter_ParsesCurrentFilters()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Directorate\":\"DIR1\"}"
            };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<ProjectExceptionalCostViewDto> { BuildDto() };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var serviceResponse = ApiResponseDto<List<ProjectExceptionalCostViewDto>>.SuccessResponse(dtos, paginationDto);

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectExceptionalCostsPagedAsync(queryParameters).Returns(serviceResponse);
            _mapper.Map<List<ExceptionalCostSnapshotItem>>(Arg.Any<List<ProjectExceptionalCostViewDto>>())
                .Returns(new List<ExceptionalCostSnapshotItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadExceptionalCostDataGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.NotNull(partialView.Model);
            var gridConfig = Assert.IsType<DataGridConfig<ExceptionalCostSnapshotItem>>(partialView.Model);
            Assert.NotNull(gridConfig.CurrentFilters);
            Assert.True(gridConfig.CurrentFilters.ContainsKey("Directorate"));
            Assert.Equal("DIR1", gridConfig.CurrentFilters["Directorate"]);
        }

        [Fact]
        public async Task LoadExceptionalCostDataGrid_WhenPaginationNull_UsesEmptyPaginationModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, SortBy = "Project", Descending = false };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<ProjectExceptionalCostViewDto> { BuildDto() };
            var serviceResponse = ApiResponseDto<List<ProjectExceptionalCostViewDto>>.SuccessResponse(dtos, null!);

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectExceptionalCostsPagedAsync(queryParameters).Returns(serviceResponse);
            _mapper.Map<List<ExceptionalCostSnapshotItem>>(Arg.Any<List<ProjectExceptionalCostViewDto>>())
                .Returns(new List<ExceptionalCostSnapshotItem> { new() });

            // Act
            var result = await _controller.LoadExceptionalCostDataGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ExceptionalCostSnapshotItem>>(partialView.Model);
            Assert.NotNull(gridConfig.Pagination);
            Assert.Equal("Project", gridConfig.Pagination!.SortColumn);
            Assert.False(gridConfig.Pagination.SortDirection);
        }

        #endregion
    }
}
