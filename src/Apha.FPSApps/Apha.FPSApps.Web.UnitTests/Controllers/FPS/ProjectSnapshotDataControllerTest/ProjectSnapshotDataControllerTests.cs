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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProjectSnapshotDataControllerTest
{
    public class ProjectSnapshotDataControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly ProjectSnapshotDataController _controller;

        public ProjectSnapshotDataControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _projectService = Substitute.For<IProjectService>();
            _controller = new ProjectSnapshotDataController(_mapper, _projectService);
        }

        private static ProjectSnapshotViewDto BuildDto(string parentProject = "PP001") =>
            new()
            {
                ParentProject = parentProject,
                ProjectTitle = "Title",
                Program = "PRG",
                Customer = "Cust",
                Manager = "Mgr",
                TransferIncome = 100m,
                CustIncome = 200m,
                ProjectStatus = "Approved",
                Contract = "C1"
            };

        #region Index Tests

        [Fact]
        public void Index_ReturnsViewWithEmptyGridConfig()
        {
            var result = _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectSnapshotDataViewModel>(viewResult.Model);
            Assert.Equal("snapShotProjectDataGrid", model.SnapShotProjectDataGrid.GridId);
            Assert.Empty(model.SnapShotProjectDataGrid.Data);
            Assert.True(model.SnapShotProjectDataGrid.ShowPagination);
            Assert.False(model.SnapShotProjectDataGrid.AllowAdd);
        }

        #endregion

        #region LoadProjectSnapshotDataGrid Tests

        [Fact]
        public async Task LoadProjectSnapshotDataGrid_WithValidRequest_ReturnsPartialViewWithDataGrid()
        {
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, SortBy = "ParentProject", Descending = true };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<ProjectSnapshotViewDto> { BuildDto() };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var serviceResponse = ApiResponseDto<List<ProjectSnapshotViewDto>>.SuccessResponse(dtos, paginationDto);
            var items = new List<ProjectSnapshotItem> { new() { ParentProject = "PP001", ProjectTitle = "Title" } };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetPagedProjectSnapshotDataAsync(queryParameters).Returns(serviceResponse);
            _mapper.Map<List<ProjectSnapshotItem>>(Arg.Any<List<ProjectSnapshotViewDto>>()).Returns(items);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            var result = await _controller.LoadProjectSnapshotDataGrid(request);

            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectSnapshotItem>>(partialView.Model);
            Assert.Equal("snapShotProjectDataGrid", gridConfig.GridId);
            Assert.Single(gridConfig.Data);
            Assert.Equal("ParentProject", gridConfig.Pagination!.SortColumn);
            Assert.True(gridConfig.Pagination.SortDirection);
            await _projectService.Received(1).GetPagedProjectSnapshotDataAsync(queryParameters);
        }

        [Fact]
        public async Task LoadProjectSnapshotDataGrid_WhenServiceReturnsFailure_ReturnsPartialViewWithEmptyData()
        {
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var serviceResponse = ApiResponseDto<List<ProjectSnapshotViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetPagedProjectSnapshotDataAsync(queryParameters).Returns(serviceResponse);

            var result = await _controller.LoadProjectSnapshotDataGrid(request);

            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectSnapshotItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
            _mapper.DidNotReceive().Map<List<ProjectSnapshotItem>>(Arg.Any<List<ProjectSnapshotViewDto>>());
        }

        [Fact]
        public async Task LoadProjectSnapshotDataGrid_WithInvalidModelState_ReturnsJsonError()
        {
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _controller.ModelState.AddModelError("Page", "Invalid");

            var result = await _controller.LoadProjectSnapshotDataGrid(request);

            Assert.IsType<JsonResult>(result);
            await _projectService.DidNotReceive().GetPagedProjectSnapshotDataAsync(Arg.Any<QueryParameters<string>>());
        }

        #endregion
    }
}
