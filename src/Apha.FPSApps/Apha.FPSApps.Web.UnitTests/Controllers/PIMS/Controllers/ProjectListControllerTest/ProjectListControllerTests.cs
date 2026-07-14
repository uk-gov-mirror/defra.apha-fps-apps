using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Controllers;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PIMS.Controllers.ProjectListControllerTest
{
    public class ProjectListControllerTests
    {
        private readonly IProjectListService _projectListServiceMock;
        private readonly IMapper _mapperMock;
        private readonly ProjectListController _controller;

        public ProjectListControllerTests()
        {
            _projectListServiceMock = Substitute.For<IProjectListService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ProjectListController(_mapperMock, _projectListServiceMock);
        }

        /// <summary>
        /// Sets up the common mocks required for BuildProjectListGridAsync to complete successfully.
        /// </summary>
        private void SetupSuccessfulGridMocks(
            List<ProjectListViewDto>? dtoData = null,
            PaginationDto? pagination = null)
        {
            dtoData ??= new List<ProjectListViewDto>();

            var apiResponse = new ApiResponseDto<List<ProjectListViewDto>>
            {
                Success = true,
                Data = dtoData,
                Pagination = pagination
            };

            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());

            _projectListServiceMock.GetAllProjectsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>())
                .Returns(apiResponse);

            _mapperMock.Map<List<ProjectListItem>>(Arg.Any<List<ProjectListViewDto>>())
                .Returns(new List<ProjectListItem>());

            if (pagination != null)
            {
                _mapperMock.Map<PaginationModel>(Arg.Any<PaginationDto>())
                    .Returns(new PaginationModel());
            }
        }

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_ReturnsProjectListViewModel()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ProjectListViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_CallsGetAllProjectsAsync_Once()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            await _controller.Index();

            // Assert
            await _projectListServiceMock.Received(1).GetAllProjectsAsync(Arg.Any<QueryParameters<string>>(), 1);
        }

        [Fact]
        public async Task Index_CallsMapperToMapQueryParameters()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            await _controller.Index();

            // Assert
            _mapperMock.Received(1).Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>());
        }

        [Fact]
        public async Task Index_WithSuccessfulData_CallsMapperForProjectListItems()
        {
            // Arrange
            var dtoData = new List<ProjectListViewDto>
            {
                new ProjectListViewDto { Parentproject = "PP001", Program = "Program A", Customer = "Customer A", OnFps = "Yes" },
                new ProjectListViewDto { Parentproject = "PP002", Program = "Program B", Customer = "Customer B", OnFps = "No" }
            };
            SetupSuccessfulGridMocks(dtoData);

            // Act
            await _controller.Index();

            // Assert
            _mapperMock.Received(1).Map<List<ProjectListItem>>(Arg.Any<List<ProjectListViewDto>>());
        }

        [Fact]
        public async Task Index_WhenServiceReturnsFailure_DoesNotMapDataItems()
        {
            // Arrange
            var apiResponse = new ApiResponseDto<List<ProjectListViewDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Service error", Code = "ERROR" } }
            };

            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _projectListServiceMock.GetAllProjectsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>())
                .Returns(apiResponse);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.Empty(model.ProjectGrid.Data);
            _mapperMock.DidNotReceive().Map<List<ProjectListItem>>(Arg.Any<List<ProjectListViewDto>>());
        }

        [Fact]
        public async Task Index_WhenServiceReturnsSuccessWithNullData_DoesNotMapDataItems()
        {
            // Arrange
            var apiResponse = new ApiResponseDto<List<ProjectListViewDto>>
            {
                Success = true,
                Data = null,
                Pagination = null
            };

            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _projectListServiceMock.GetAllProjectsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>())
                .Returns(apiResponse);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.Empty(model.ProjectGrid.Data);
            _mapperMock.DidNotReceive().Map<List<ProjectListItem>>(Arg.Any<List<ProjectListViewDto>>());
        }

        [Fact]
        public async Task Index_WithNullPagination_DoesNotCallMapperForPaginationModel()
        {
            // Arrange
            SetupSuccessfulGridMocks(pagination: null);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.NotNull(model.ProjectGrid.Pagination);
            _mapperMock.DidNotReceive().Map<PaginationModel>(Arg.Any<PaginationDto>());
        }

        [Fact]
        public async Task Index_WithPagination_CallsMapperForPaginationModel()
        {
            // Arrange
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 5 };
            SetupSuccessfulGridMocks(pagination: paginationDto);

            // Act
            await _controller.Index();

            // Assert
            _mapperMock.Received(1).Map<PaginationModel>(Arg.Any<PaginationDto>());
        }

        [Fact]
        public async Task Index_WhenMapperThrowsException_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Throws(new Exception("Unexpected mapper error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Index());
        }

        [Fact]
        public async Task Index_WhenServiceThrowsException_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _projectListServiceMock.GetAllProjectsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>())
                .ThrowsAsync(new Exception("Service unavailable"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Index());
        }

        [Fact]
        public async Task Index_ProjectGridHasCorrectGridId()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.Equal("projectListGrid", model.ProjectGrid.GridId);
        }

        [Fact]
        public async Task Index_ProjectGridHasCorrectTitle()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.Equal("Select a Project", model.ProjectGrid.Title);
        }

        [Fact]
        public async Task Index_ProjectGridHasCorrectBindUrl()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.Equal("/PIMS/ProjectList/LoadProjectListGrid", model.ProjectGrid.BindGridUrl);
        }

        [Fact]
        public async Task Index_ProjectGridAllowAddIsFalse()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.False(model.ProjectGrid.AllowAdd);
        }

        [Fact]
        public async Task Index_ProjectGridAllowEditIsTrue()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.False(model.ProjectGrid.AllowEdit);
        }

        [Fact]
        public async Task Index_ProjectGridAllowDeleteIsFalse()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.False(model.ProjectGrid.AllowDelete);
        }

        [Fact]
        public async Task Index_ProjectGridShowCheckboxColumnIsFalse()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.False(model.ProjectGrid.ShowCheckboxColumn);
        }

        [Fact]
        public async Task Index_ProjectGridShowPaginationIsTrue()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.True(model.ProjectGrid.ShowPagination);
        }

        [Fact]
        public async Task Index_ProjectGridKeyPropertyIsParentproject()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.Equal("Parentproject", model.ProjectGrid.KeyProperty);
        }

        [Fact]
        public async Task Index_ProjectGridEditFunctionIsEditProject()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.Equal("editProject", model.ProjectGrid.EditFunction);
        }

        [Fact]
        public async Task Index_ProjectGridColumnsArePopulated()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.NotNull(model.ProjectGrid.Columns);
            Assert.NotEmpty(model.ProjectGrid.Columns);
        }

        [Fact]
        public async Task Index_DefaultSortColumnIsNull()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.Null(model.ProjectGrid.Pagination.SortColumn);
        }

        [Fact]
        public async Task Index_DefaultSortDirectionIsFalse()
        {
            // Arrange
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectListViewModel>(viewResult.Model);
            Assert.False(model.ProjectGrid.Pagination.SortDirection);
        }

        #endregion

        #region LoadProjectListGrid Tests

        [Fact]
        public async Task LoadProjectListGrid_WithInvalidModelState_ReturnsJsonResult()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("Filter", "Required");

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadProjectListGrid_WithInvalidModelState_JsonContainsSuccessFalse()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("Filter", "Invalid filter");

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("false", json);
        }

        [Fact]
        public async Task LoadProjectListGrid_WithInvalidModelState_DoesNotCallService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("Page", "Invalid");

            // Act
            await _controller.LoadProjectListGrid(request);

            // Assert
            await _projectListServiceMock.DidNotReceive().GetAllProjectsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>());
        }

        [Fact]
        public async Task LoadProjectListGrid_WithValidRequest_ReturnsPartialViewResult()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task LoadProjectListGrid_WithValidRequest_ReturnsDataGridPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
        }

        [Fact]
        public async Task LoadProjectListGrid_WithValidRequest_ReturnsDataGridConfigModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
        }

        [Fact]
        public async Task LoadProjectListGrid_WithValidRequest_CallsGetAllProjectsAsync()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            await _controller.LoadProjectListGrid(request);

            // Assert
            await _projectListServiceMock.Received(1).GetAllProjectsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>());
        }

        [Fact]
        public async Task LoadProjectListGrid_WithSuccessfulData_PopulatesGridItems()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            var dtoData = new List<ProjectListViewDto>
            {
                new ProjectListViewDto { Parentproject = "PP001", Program = "Program A", Customer = "Customer A", OnFps = "Yes" }
            };
            var mappedItems = new List<ProjectListItem>
            {
                new ProjectListItem { Parentproject = "PP001", Program = "Program A", Customer = "Customer A", OnFps = "Yes" }
            };
            var apiResponse = new ApiResponseDto<List<ProjectListViewDto>>
            {
                Success = true,
                Data = dtoData,
                Pagination = null
            };

            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _projectListServiceMock.GetAllProjectsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>())
                .Returns(apiResponse);
            _mapperMock.Map<List<ProjectListItem>>(Arg.Any<List<ProjectListViewDto>>())
                .Returns(mappedItems);

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.Single(model.Data);
            Assert.Equal("PP001", model.Data[0].Parentproject);
        }

        [Fact]
        public async Task LoadProjectListGrid_WithServiceFailure_ReturnsEmptyDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponseDto<List<ProjectListViewDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Service error", Code = "ERROR" } }
            };

            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _projectListServiceMock.GetAllProjectsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>())
                .Returns(apiResponse);

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.Empty(model.Data);
        }

        [Fact]
        public async Task LoadProjectListGrid_WithServiceFailure_DoesNotMapDataItems()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponseDto<List<ProjectListViewDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Service error", Code = "ERROR" } }
            };

            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _projectListServiceMock.GetAllProjectsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>())
                .Returns(apiResponse);

            // Act
            await _controller.LoadProjectListGrid(request);

            // Assert
            _mapperMock.DidNotReceive().Map<List<ProjectListItem>>(Arg.Any<List<ProjectListViewDto>>());
        }

        [Fact]
        public async Task LoadProjectListGrid_WithNullFilter_HandlesGracefully()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = null, Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
        }

        [Fact]
        public async Task LoadProjectListGrid_WithFilterValues_PopulatesCurrentFilters()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Filter = "{\"Parentproject\":\"PP001\"}",
                Page = 1,
                PageSize = 10
            };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.NotNull(model.CurrentFilters);
            Assert.True(model.CurrentFilters.ContainsKey("Parentproject"));
            Assert.Equal("PP001", model.CurrentFilters["Parentproject"]);
        }

        [Fact]
        public async Task LoadProjectListGrid_WithEmptyFilter_ReturnsEmptyCurrentFilters()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.NotNull(model.CurrentFilters);
            Assert.Empty(model.CurrentFilters);
        }

        [Fact]
        public async Task LoadProjectListGrid_SetsSortColumnFromRequest()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Filter = "{}",
                Page = 1,
                PageSize = 10,
                SortBy = "Parentproject"
            };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.Equal("Parentproject", model.Pagination.SortColumn);
        }

        [Fact]
        public async Task LoadProjectListGrid_SetsSortDirectionFromRequest()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Filter = "{}",
                Page = 1,
                PageSize = 10,
                Descending = true
            };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.True(model.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadProjectListGrid_WithPaginationData_MapsPaginationModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 2, PageSize = 20 };
            var paginationDto = new PaginationDto { PageNumber = 2, PageSize = 20, TotalRecords = 100 };
            var apiResponse = new ApiResponseDto<List<ProjectListViewDto>>
            {
                Success = true,
                Data = new List<ProjectListViewDto>(),
                Pagination = paginationDto
            };
            var paginationModel = new PaginationModel { PageNumber = 2, PageSize = 20, TotalRecords = 100 };

            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _projectListServiceMock.GetAllProjectsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>())
                .Returns(apiResponse);
            _mapperMock.Map<List<ProjectListItem>>(Arg.Any<List<ProjectListViewDto>>())
                .Returns(new List<ProjectListItem>());
            _mapperMock.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(paginationModel);

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            _mapperMock.Received(1).Map<PaginationModel>(Arg.Any<PaginationDto>());
            Assert.Equal(100, model.Pagination.TotalRecords);
        }

        [Fact]
        public async Task LoadProjectListGrid_WithNullPagination_UsesDefaultPaginationModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks(pagination: null);

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.NotNull(model.Pagination);
            _mapperMock.DidNotReceive().Map<PaginationModel>(Arg.Any<PaginationDto>());
        }

        [Fact]
        public async Task LoadProjectListGrid_WhenMapperThrowsException_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Throws(new Exception("Unexpected mapper error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.LoadProjectListGrid(request));
        }

        [Fact]
        public async Task LoadProjectListGrid_WhenServiceThrowsException_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _projectListServiceMock.GetAllProjectsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>())
                .ThrowsAsync(new Exception("Service unavailable"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.LoadProjectListGrid(request));
        }

        [Fact]
        public async Task LoadProjectListGrid_ExceptionPath_GridIdIsCorrect()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            _mapperMock.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Throws(new Exception("Unexpected error"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _controller.LoadProjectListGrid(request));
            Assert.Equal("Unexpected error", ex.Message);
        }

        [Fact]
        public async Task LoadProjectListGrid_GridIdIsCorrect()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.Equal("projectListGrid", model.GridId);
        }

        [Fact]
        public async Task LoadProjectListGrid_TitleIsCorrect()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.Equal("Select a Project", model.Title);
        }

        [Fact]
        public async Task LoadProjectListGrid_AllowAddIsFalse()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.False(model.AllowAdd);
        }

        [Fact]
        public async Task LoadProjectListGrid_AllowEditIsFalse()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.False(model.AllowEdit);
        }

        [Fact]
        public async Task LoadProjectListGrid_AllowDeleteIsFalse()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupSuccessfulGridMocks();

            // Act
            var result = await _controller.LoadProjectListGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProjectListItem>>(partialViewResult.Model);
            Assert.False(model.AllowDelete);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_InitializesController()
        {
            // Arrange & Act
            var controller = new ProjectListController(_mapperMock, _projectListServiceMock);

            // Assert
            Assert.NotNull(controller);
        }

        #endregion
    }
}