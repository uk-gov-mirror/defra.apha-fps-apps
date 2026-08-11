using Apha.Common.Utilities.StateManagement;
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
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProgramProjectControllerTest
{
    public class ProgramProjectControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly IProgramService _programService;
        private readonly IEmployeeService _employeeService;
        private readonly IAppStateService _appStateService;
        private readonly ProgramProjectController _controller;

        public ProgramProjectControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _projectService = Substitute.For<IProjectService>();
            _programService = Substitute.For<IProgramService>();
            _employeeService = Substitute.For<IEmployeeService>();
            _appStateService = Substitute.For<IAppStateService>();
            _controller = new ProgramProjectController(_mapper, _projectService, _programService, _employeeService, _appStateService);
        }

        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        #region LoadProjectGrid Tests

        [Fact]
        public async Task LoadProjectGrid_WithValidRequest_ReturnsPartialViewWithDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001" }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects, paginationDto);
            var projectViewModels = new List<ProjectViewModel>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" }
            };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectsByProgramAsync(queryParameters, programNo).Returns(serviceResponse);
            _mapper.Map<List<ProjectViewModel>>(projects).Returns(projectViewModels);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadProjectGrid(request, programNo);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectViewModel>>(partialView.Model);
            Assert.Equal("projectGrid", gridConfig.GridId);
            Assert.Equal("Projects",    gridConfig.Title);
            Assert.Equal("ParentProject",     gridConfig.KeyProperty);
            Assert.Single(gridConfig.Data);
            Assert.False(gridConfig.AllowAdd);
            Assert.False(gridConfig.AllowEdit);
            Assert.False(gridConfig.AllowDelete);
            Assert.True(gridConfig.AllowRowSelection);
        }

        [Fact]
        public async Task LoadProjectGrid_WhenModelStateIsInvalid_ReturnsFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page is required");
            var request = new PaginationFilter<string>();

            // Act
            var result = await _controller.LoadProjectGrid(request, "P001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid request data", value.message);
            await _projectService.DidNotReceive().GetProjectsByProgramAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadProjectGrid_WithNullProgramNo_ReturnsEmptyGridWithoutCallingService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);

            // Act
            var result = await _controller.LoadProjectGrid(request, null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _projectService.DidNotReceive().GetProjectsByProgramAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadProjectGrid_WhenServiceReturnsFailure_MapsEmptyProjectList()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectsByProgramAsync(queryParameters, programNo).Returns(serviceResponse);

            // Act
            var result = await _controller.LoadProjectGrid(request, programNo);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectViewModel>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
            _mapper.DidNotReceive().Map<List<ProjectViewModel>>(Arg.Any<List<ProjectDto>>());
        }

        [Fact]
        public async Task LoadProjectGrid_SetsPaginationSortFields_FromRequest()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 2, PageSize = 5, SortBy = "parentproject", Descending = true
            };
            var programNo = "P001";
            var queryParameters = new QueryParameters<string>
            {
                Page = 2, PageSize = 5, SortBy = "parentproject", Descending = true
            };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>(),
                new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 0 }
            );
            var paginationModel = new PaginationModel { PageNumber = 2, PageSize = 5 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectsByProgramAsync(queryParameters, programNo).Returns(serviceResponse);
            _mapper.Map<List<ProjectViewModel>>(Arg.Any<List<ProjectDto>>()).Returns(new List<ProjectViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadProjectGrid(request, programNo);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectViewModel>>(partialView.Model);
            Assert.Equal("parentproject", gridConfig.Pagination.SortColumn);
            Assert.True(gridConfig.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadProjectGrid_WithJsonFilter_PassesFilterDictToGrid()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"JobCode\":\"PP001\",\"JobDescription\":\"Alpha\"}"
            };
            var programNo = "P001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>(), new PaginationDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectsByProgramAsync(queryParameters, programNo).Returns(serviceResponse);
            _mapper.Map<List<ProjectViewModel>>(Arg.Any<List<ProjectDto>>()).Returns(new List<ProjectViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProjectGrid(request, programNo);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectViewModel>>(partialView.Model);
            Assert.NotNull(gridConfig.CurrentFilters);
            Assert.Equal("PP001", gridConfig.CurrentFilters["JobCode"]);
            Assert.Equal("Alpha",  gridConfig.CurrentFilters["JobDescription"]);
        }

        [Fact]
        public async Task LoadProjectGrid_WhenPaginationMapReturnsNull_UsesFallbackPaginationModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, SortBy = "parentproject", Descending = false };
            var programNo = "P001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>(), new PaginationDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectsByProgramAsync(queryParameters, programNo).Returns(serviceResponse);
            _mapper.Map<List<ProjectViewModel>>(Arg.Any<List<ProjectDto>>()).Returns(new List<ProjectViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns((PaginationModel?)null);

            // Act
            var result = await _controller.LoadProjectGrid(request, programNo);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectViewModel>>(partialView.Model);
            Assert.NotNull(gridConfig.Pagination);
            Assert.Equal("parentproject", gridConfig.Pagination.SortColumn);
        }

        [Fact]
        public async Task LoadProjectGrid_WhenProgramNoExceedsMaxLength_ReturnsFailureJson()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var oversizedProgramNo = new string('X', 21);

            // Act
            var result = await _controller.LoadProjectGrid(request, oversizedProgramNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid programme number.", value.message);
            await _projectService.DidNotReceive().GetProjectsByProgramAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_WithValidProgramNo_ReturnsViewWithCorrectGridConfig()
        {
            // Arrange
            var programNo = "P001";
            var programs = new List<ProgramDto>
            {
                new() { ProgramNo = "P001", ProgramName = "Programme Alpha" },
                new() { ProgramNo = "P002", ProgramName = "Programme Beta" }
            };
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programs));

            // Act
            var result = await _controller.Index(programNo);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramProjectViewModel>(viewResult.Model);
            Assert.Equal("P001",              model.SelectedProgramNo);
            Assert.Equal(2,                   model.ProgrammeList.Count);
            Assert.Equal("programProjectGrid", model.ProjectsGrid.GridId);
            Assert.True(model.ProjectsGrid.AllowEdit);
            Assert.True(model.ProjectsGrid.AllowDelete);
            Assert.True(model.ProjectsGrid.AllowRowSelection);
            Assert.False(model.ProjectsGrid.AllowAdd);
        }

        [Fact]
        public async Task Index_WithNullProgramNo_DefaultsToFirstProgramme()
        {
            // Arrange
            var programs = new List<ProgramDto>
            {
                new() { ProgramNo = "P001", ProgramName = "Programme Alpha" },
                new() { ProgramNo = "P002", ProgramName = "Programme Beta" }
            };
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programs));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramProjectViewModel>(viewResult.Model);
            Assert.Equal("P001", model.SelectedProgramNo);
        }

        [Fact]
        public async Task Index_WithInvalidProgramNo_FallsBackToFirstProgramme()
        {
            // Arrange
            var programs = new List<ProgramDto>
            {
                new() { ProgramNo = "P001", ProgramName = "Programme Alpha" }
            };
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programs));

            // Act
            var result = await _controller.Index("INVALID");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramProjectViewModel>(viewResult.Model);
            Assert.Equal("P001", model.SelectedProgramNo);
        }

        [Fact]
        public async Task Index_WhenProgrammeListIsEmpty_SetsEmptySelectedProgramNo()
        {
            // Arrange
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(Enumerable.Empty<ProgramDto>()));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramProjectViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.SelectedProgramNo);
            Assert.Empty(model.ProgrammeList);
        }

        [Fact]
        public async Task Index_WhenProgramServiceFails_ReturnsViewWithEmptyProgrammeList()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Service error", Code = "ERR" } };
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramProjectViewModel>(viewResult.Model);
            Assert.Empty(model.ProgrammeList);
            Assert.Equal(string.Empty, model.SelectedProgramNo);
        }

        [Fact]
        public async Task Index_WhenProgrammeListContainsNullProgramNo_FiltersItOut()
        {
            // Arrange
            var programs = new List<ProgramDto>
            {
                new() { ProgramNo = "P001", ProgramName = "Programme Alpha" },
                new() { ProgramNo = null!,   ProgramName = "Should Be Filtered" },
                new() { ProgramNo = "",     ProgramName = "Also Filtered" }
            };
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programs));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramProjectViewModel>(viewResult.Model);
            Assert.Single(model.ProgrammeList);
            Assert.Equal("P001", model.SelectedProgramNo);
        }

        #endregion

        #region LoadProgramProjectGrid Tests

        [Fact]
        public async Task LoadProgramProjectGrid_WithValidRequest_ReturnsPartialViewWithEditableGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001" }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects, paginationDto);
            var programProjectItems = new List<ProgramProjectItem>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" }
            };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectsByProgramAsync(queryParameters, programNo).Returns(serviceResponse);
            _mapper.Map<List<ProgramProjectItem>>(projects).Returns(programProjectItems);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadProgramProjectGrid(request, programNo);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<ProgramProjectItem>>(partialView.Model);
            Assert.Equal("programProjectGrid", gridConfig.GridId);
            Assert.Equal("Projects within Programme", gridConfig.Title);
            Assert.True(gridConfig.AllowEdit);
            Assert.True(gridConfig.AllowDelete);
            Assert.True(gridConfig.AllowRowSelection);
            Assert.False(gridConfig.AllowAdd);
            Assert.Single(gridConfig.Data);
        }

        [Fact]
        public async Task LoadProgramProjectGrid_WhenModelStateIsInvalid_ReturnsFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page is required");
            var request = new PaginationFilter<string>();

            // Act
            var result = await _controller.LoadProgramProjectGrid(request, "P001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid request data", value.message);
            await _projectService.DidNotReceive().GetProjectsByProgramAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadProgramProjectGrid_WithNullProgramNo_ReturnsEmptyGridWithoutCallingService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);

            // Act
            var result = await _controller.LoadProgramProjectGrid(request, null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _projectService.DidNotReceive().GetProjectsByProgramAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadProgramProjectGrid_WhenServiceReturnsFailure_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Service Error", Code = "SVC_ERR" } };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectsByProgramAsync(queryParameters, programNo).Returns(serviceResponse);

            // Act
            var result = await _controller.LoadProgramProjectGrid(request, programNo);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProgramProjectItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
            _mapper.DidNotReceive().Map<List<ProgramProjectItem>>(Arg.Any<List<ProjectDto>>());
        }

        [Fact]
        public async Task LoadProgramProjectGrid_SetsPaginationSortFields_FromRequest()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 2, PageSize = 5, SortBy = "parentproject", Descending = true
            };
            var programNo = "P001";
            var queryParameters = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>(), new PaginationDto { PageNumber = 2, PageSize = 5 });
            var paginationModel = new PaginationModel { PageNumber = 2, PageSize = 5 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectsByProgramAsync(queryParameters, programNo).Returns(serviceResponse);
            _mapper.Map<List<ProgramProjectItem>>(Arg.Any<List<ProjectDto>>()).Returns(new List<ProgramProjectItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadProgramProjectGrid(request, programNo);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProgramProjectItem>>(partialView.Model);
            Assert.Equal("parentproject", gridConfig.Pagination.SortColumn);
            Assert.True(gridConfig.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadProgramProjectGrid_WithJsonFilter_PassesFilterDictToGrid()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ParentProject\":\"PP001\"}"
            };
            var programNo = "P001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>(), new PaginationDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectsByProgramAsync(queryParameters, programNo).Returns(serviceResponse);
            _mapper.Map<List<ProgramProjectItem>>(Arg.Any<List<ProjectDto>>()).Returns(new List<ProgramProjectItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProgramProjectGrid(request, programNo);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProgramProjectItem>>(partialView.Model);
            Assert.NotNull(gridConfig.CurrentFilters);
            Assert.Equal("PP001", gridConfig.CurrentFilters["ParentProject"]);
        }

        [Fact]
        public async Task LoadProgramProjectGrid_WhenPaginationMapReturnsNull_UsesFallbackPaginationModel()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, SortBy = "projecttitle", Descending = true };
            var programNo = "P001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>(), new PaginationDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectsByProgramAsync(queryParameters, programNo).Returns(serviceResponse);
            _mapper.Map<List<ProgramProjectItem>>(Arg.Any<List<ProjectDto>>()).Returns(new List<ProgramProjectItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns((PaginationModel?)null);

            // Act
            var result = await _controller.LoadProgramProjectGrid(request, programNo);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProgramProjectItem>>(partialView.Model);
            Assert.NotNull(gridConfig.Pagination);
            Assert.Equal("projecttitle", gridConfig.Pagination.SortColumn);
            Assert.True(gridConfig.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadProgramProjectGrid_WhenProgramNoExceedsMaxLength_ReturnsFailureJson()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var oversizedProgramNo = new string('X', 21);

            // Act
            var result = await _controller.LoadProgramProjectGrid(request, oversizedProgramNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid programme number.", value.message);
            await _projectService.DidNotReceive().GetProjectsByProgramAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        #endregion

        #region GetProgramInfo Tests

        [Fact]
        public async Task GetProgramInfo_WithEmptyProgramNo_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.GetProgramInfo(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Programme number is required.", value.message);
            await _programService.DidNotReceive().GetProgramByIdAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetProgramInfo_WhenProgramFound_ReturnsProgrammeName()
        {
            // Arrange
            var programNo = "P001";
            var program = new ProgramDto { ProgramNo = "P001", ProgramName = "Programme Alpha" };
            _programService.GetProgramByIdAsync(programNo)
                .Returns(ApiResponseDto<ProgramDto?>.SuccessResponse(program));

            // Act
            var result = await _controller.GetProgramInfo(programNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonElement>(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Programme Alpha", value.GetProperty("programmeName").GetString());
        }

        [Fact]
        public async Task GetProgramInfo_WhenProgramDataIsNull_ReturnsNotFoundMessage()
        {
            // Arrange
            var programNo = "P999";
            _programService.GetProgramByIdAsync(programNo)
                .Returns(ApiResponseDto<ProgramDto?>.SuccessResponse(null));

            // Act
            var result = await _controller.GetProgramInfo(programNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Programme not found.", value.message);
        }

        [Fact]
        public async Task GetProgramInfo_WhenServiceFails_ReturnsNotFoundMessage()
        {
            // Arrange
            var programNo = "P001";
            var errors = new List<ApiErrorDto> { new() { Message = "Service error", Code = "ERR" } };
            _programService.GetProgramByIdAsync(programNo)
                .Returns(ApiResponseDto<ProgramDto?>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetProgramInfo(programNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Programme not found.", value.message);
        }

        [Fact]
        public async Task GetProgramInfo_WithWhitespaceProgramNo_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.GetProgramInfo("   ");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Programme number is required.", value.message);
            await _programService.DidNotReceive().GetProgramByIdAsync(Arg.Any<string>());
        }

        #endregion

        #region GetProjectTotals Tests

        [Fact]
        public async Task GetProjectTotals_WithNullProgramNo_ReturnsAllZeros()
        {
            // Act
            var result = await _controller.GetProjectTotals(null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonElement>(jsonResult);
            Assert.Equal(0m, value.GetProperty("budgetCvl").GetDecimal());
            Assert.Equal(0m, value.GetProperty("budgetExt").GetDecimal());
            Assert.Equal(0m, value.GetProperty("transferIncome").GetDecimal());
            Assert.Equal(0m, value.GetProperty("planCaseWorkDebit").GetDecimal());
            await _projectService.DidNotReceive().GetProjectsByProgramAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetProjectTotals_WithProjects_ReturnsCorrectSums()
        {
            // Arrange
            var programNo = "P001";
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", BudgetCvl = 100m, BudgetExt = 200m, TransferIncome = 50m, PlanCaseWorkDebit = 30m },
                new() { ParentProject = "PP002", BudgetCvl = 150m, BudgetExt = 250m, TransferIncome = 75m, PlanCaseWorkDebit = 45m }
            };
            _projectService.GetProjectsByProgramAsync(Arg.Any<QueryParameters<string>>(), programNo)
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects));

            // Act
            var result = await _controller.GetProjectTotals(programNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonElement>(jsonResult);
            Assert.Equal(250m, value.GetProperty("budgetCvl").GetDecimal());
            Assert.Equal(450m, value.GetProperty("budgetExt").GetDecimal());
            Assert.Equal(125m, value.GetProperty("transferIncome").GetDecimal());
            Assert.Equal(75m,  value.GetProperty("planCaseWorkDebit").GetDecimal());
        }

        [Fact]
        public async Task GetProjectTotals_WhenServiceFails_ReturnsAllZeros()
        {
            // Arrange
            var programNo = "P001";
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            _projectService.GetProjectsByProgramAsync(Arg.Any<QueryParameters<string>>(), programNo)
                .Returns(ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetProjectTotals(programNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonElement>(jsonResult);
            Assert.Equal(0m, value.GetProperty("budgetCvl").GetDecimal());
            Assert.Equal(0m, value.GetProperty("budgetExt").GetDecimal());
            Assert.Equal(0m, value.GetProperty("transferIncome").GetDecimal());
            Assert.Equal(0m, value.GetProperty("planCaseWorkDebit").GetDecimal());
        }

        [Fact]
        public async Task GetProjectTotals_UsesBulkPageSize_ToFetchAllProjects()
        {
            // Arrange
            var programNo = "P001";
            _projectService.GetProjectsByProgramAsync(
                    Arg.Is<QueryParameters<string>>(q => q.Page == 1 && q.PageSize == 9999), programNo)
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>()));

            // Act
            await _controller.GetProjectTotals(programNo);

            // Assert
            await _projectService.Received(1).GetProjectsByProgramAsync(
                Arg.Is<QueryParameters<string>>(q => q.Page == 1 && q.PageSize == 9999), programNo);
        }

        [Fact]
        public async Task GetProjectTotals_WhenProjectsHaveNullableFields_TreatsNullAsZero()
        {
            // Arrange
            var programNo = "P001";
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", BudgetCvl = null, BudgetExt = null, TransferIncome = 0m, PlanCaseWorkDebit = null }
            };
            _projectService.GetProjectsByProgramAsync(Arg.Any<QueryParameters<string>>(), programNo)
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects));

            // Act
            var result = await _controller.GetProjectTotals(programNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonElement>(jsonResult);
            Assert.Equal(0m, value.GetProperty("budgetCvl").GetDecimal());
            Assert.Equal(0m, value.GetProperty("budgetExt").GetDecimal());
            Assert.Equal(0m, value.GetProperty("transferIncome").GetDecimal());
            Assert.Equal(0m, value.GetProperty("planCaseWorkDebit").GetDecimal());
        }

        #endregion

        #region Edit GET Tests

        [Fact]
        public async Task EditGet_WithEmptyParentProject_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Edit(string.Empty);

            // Assert
            Assert.IsType<BadRequestResult>(result);
            await _projectService.DidNotReceive().GetProjectByIdAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task EditGet_WhenProjectNotFound_ReturnsNotFound()
        {
            // Arrange
            var parentProject = "PP999";
            _projectService.GetProjectByIdAsync(parentProject)
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(null!));

            // Act
            var result = await _controller.Edit(parentProject);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditGet_WhenServiceFails_ReturnsNotFound()
        {
            // Arrange
            var parentProject = "PP001";
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "ERR" } };
            _projectService.GetProjectByIdAsync(parentProject)
                .Returns(ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Edit(parentProject);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditGet_WithValidProject_ReturnsPartialViewWithMappedModel()
        {
            // Arrange
            var parentProject = "PP001";
            var projectDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Alpha Project" };
            var projectViewModel = new ProgramProjectEditViewModel { ParentProject = "PP001", ProjectTitle = "Alpha Project" };

            _projectService.GetProjectByIdAsync(parentProject)
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            _mapper.Map<ProgramProjectEditViewModel>(projectDto).Returns(projectViewModel);
            SetupDropdownMocks();

            // Act
            var result = await _controller.Edit(parentProject);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditProgramProject", partialView.ViewName);
            var model = Assert.IsType<ProgramProjectEditViewModel>(partialView.Model);
            Assert.Equal("PP001", model.ParentProject);
        }

        [Fact]
        public async Task EditGet_WithWhitespaceParentProject_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Edit("   ");

            // Assert
            Assert.IsType<BadRequestResult>(result);
            await _projectService.DidNotReceive().GetProjectByIdAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task EditGet_PopulatesDropdownsWithData_WhenServicesReturnItems()
        {
            // Arrange
            var parentProject = "PP001";
            var projectDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Alpha Project" };
            var projectViewModel = new ProgramProjectEditViewModel { ParentProject = "PP001", ProjectTitle = "Alpha Project" };
            var managers = new List<ManagerDto> { new() { Name = "John Smith" } };
            var programs = new List<ProgramDto> { new() { ProgramNo = "P001", ProgramName = "Programme Alpha" } };

            _projectService.GetProjectByIdAsync(parentProject)
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            _mapper.Map<ProgramProjectEditViewModel>(projectDto).Returns(projectViewModel);
            _employeeService.GetAllManagersAsync()
                .Returns(ApiResponseDto<List<ManagerDto>>.SuccessResponse(managers));
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programs));
            _projectService.GetAllCustomersAsync()
                .Returns(ApiResponseDto<List<CustomerDto>>.SuccessResponse(new List<CustomerDto>()));
            _projectService.GetAllProjectGroupsAsync()
                .Returns(ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(new List<ProjectGroupDto>()));
            _projectService.GetAllContractsAsync()
                .Returns(ApiResponseDto<List<ContractDto>>.SuccessResponse(new List<ContractDto>()));
            _projectService.GetAllDiseasesAsync()
                .Returns(ApiResponseDto<List<DiseaseDto>>.SuccessResponse(new List<DiseaseDto>()));
            _projectService.GetAllStatusesAsync()
                .Returns(ApiResponseDto<List<StatusDto>>.SuccessResponse(new List<StatusDto>()));

            // Act
            var result = await _controller.Edit(parentProject);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<ProgramProjectEditViewModel>(partialView.Model);
            Assert.Single(model.ManagerList);
            Assert.Equal("John Smith", model.ManagerList[0].Value);
            Assert.Single(model.ProgramList);
            Assert.Equal("P001", model.ProgramList[0].Value);
        }

        [Fact]
        public async Task EditGet_WhenAllDropdownServicesFail_ReturnsEmptyDropdowns()
        {
            // Arrange
            var parentProject = "PP001";
            var projectDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Alpha Project" };
            var projectViewModel = new ProgramProjectEditViewModel { ParentProject = "PP001" };
            var errors = new List<ApiErrorDto> { new() { Message = "Service error", Code = "ERR" } };

            _projectService.GetProjectByIdAsync(parentProject)
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));
            _mapper.Map<ProgramProjectEditViewModel>(projectDto).Returns(projectViewModel);

            _employeeService.GetAllManagersAsync()
                .Returns(ApiResponseDto<List<ManagerDto>>.FailureResponse(errors, new ApiMetaDto()));
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.FailureResponse(errors, new ApiMetaDto()));
            _projectService.GetAllCustomersAsync()
                .Returns(ApiResponseDto<List<CustomerDto>>.FailureResponse(errors, new ApiMetaDto()));
            _projectService.GetAllProjectGroupsAsync()
                .Returns(ApiResponseDto<List<ProjectGroupDto>>.FailureResponse(errors, new ApiMetaDto()));
            _projectService.GetAllContractsAsync()
                .Returns(ApiResponseDto<List<ContractDto>>.FailureResponse(errors, new ApiMetaDto()));
            _projectService.GetAllDiseasesAsync()
                .Returns(ApiResponseDto<List<DiseaseDto>>.FailureResponse(errors, new ApiMetaDto()));
            _projectService.GetAllStatusesAsync()
                .Returns(ApiResponseDto<List<StatusDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Edit(parentProject);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<ProgramProjectEditViewModel>(partialView.Model);
            Assert.Empty(model.ManagerList);
            Assert.Empty(model.ProgramList);
            Assert.Empty(model.CustomerList);
            Assert.Empty(model.ProjectGroupList);
            Assert.Empty(model.ContractList);
            Assert.Empty(model.DiseaseList);
            Assert.Empty(model.StatusList);
        }

        #endregion

        #region Edit POST Tests

        [Fact]
        public async Task EditPost_WithInvalidModelState_ReturnsFailureJsonWithErrors()
        {
            // Arrange
            _controller.ModelState.AddModelError("ProjectTitle", "Description is required");
            var model = new ProgramProjectEditViewModel { ParentProject = "PP001" };

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Please correct the errors below.", value.message);
            await _projectService.DidNotReceive().UpdateProjectAsync(Arg.Any<ProjectDto>());
        }

        [Fact]
        public async Task EditPost_WhenUpdateSucceeds_ReturnsSuccessJson()
        {
            // Arrange
            var model = new ProgramProjectEditViewModel { ParentProject = "PP001", ProjectTitle = "Updated Title" };
            var dto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Updated Title" };
            _mapper.Map<ProjectDto>(model).Returns(dto);
            _projectService.UpdateProjectAsync(dto)
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Project updated successfully.", value.message);
        }

        [Fact]
        public async Task EditPost_WhenUpdateFails_WithErrorMessage_ReturnsFailureJson()
        {
            // Arrange
            var model = new ProgramProjectEditViewModel { ParentProject = "PP001" };
            var dto = new ProjectDto { ParentProject = "PP001" };
            var errors = new List<ApiErrorDto> { new() { Message = "Database error", Code = "DB_ERR" } };
            _mapper.Map<ProjectDto>(model).Returns(dto);
            _projectService.UpdateProjectAsync(dto)
                .Returns(ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Database error", value.message);
        }

        [Fact]
        public async Task EditPost_WhenUpdateFails_WithNoErrors_ReturnsDefaultMessage()
        {
            // Arrange
            var model = new ProgramProjectEditViewModel { ParentProject = "PP001" };
            var dto = new ProjectDto { ParentProject = "PP001" };
            _mapper.Map<ProjectDto>(model).Returns(dto);
            _projectService.UpdateProjectAsync(dto)
                .Returns(ApiResponseDto<ProjectDto>.FailureResponse(null, new ApiMetaDto()));

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Failed to update project.", value.message);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithEmptyParentProject_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.Delete(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Project ID is required.", value.message);
            await _projectService.DidNotReceive().DeleteProjectAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task Delete_WhenDeleteSucceeds_ReturnsSuccessJson()
        {
            // Arrange
            var parentProject = "PP001";
            _projectService.DeleteProjectAsync(parentProject)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.Delete(parentProject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Project deleted successfully.", value.message);
        }

        [Fact]
        public async Task Delete_WhenDeleteFails_WithErrorMessage_ReturnsFailureJson()
        {
            // Arrange
            var parentProject = "PP001";
            var errors = new List<ApiErrorDto> { new() { Message = "Cannot delete active project", Code = "DEL_ERR" } };
            _projectService.DeleteProjectAsync(parentProject)
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Delete(parentProject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Cannot delete active project", value.message);
        }

        [Fact]
        public async Task Delete_WhenServiceSucceeds_ButDataIsFalse_ReturnsFailureJson()
        {
            // Arrange
            var parentProject = "PP001";
            _projectService.DeleteProjectAsync(parentProject)
                .Returns(ApiResponseDto<bool>.SuccessResponse(false));

            // Act
            var result = await _controller.Delete(parentProject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Failed to delete project.", value.message);
        }

        [Fact]
        public async Task Delete_WithWhitespaceParentProject_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.Delete("   ");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Project ID is required.", value.message);
            await _projectService.DidNotReceive().DeleteProjectAsync(Arg.Any<string>());
        }

        #endregion

        #region Project Group Mode Tests

        [Fact]
        public async Task Index_WithSourceProjectGroup_SetsIsProjectGroupModeTrue()
        {
            // Arrange
            var projectGroups = new List<ProjectGroupDto>
            {
                new() { ProjectGroupName = "Group A" },
                new() { ProjectGroupName = "Group B" }
            };
            _projectService.GetProjectGroupsByUserAsync()
                .Returns(ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(projectGroups));
            _appStateService.GetSessionAsync<string>(Arg.Any<string>())
                .Returns(string.Empty);

            // Act
            var result = await _controller.Index(null, "projectgroup");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramProjectViewModel>(viewResult.Model);
            Assert.True(model.IsProjectGroupMode);
            Assert.Equal(2, model.ProjectGroupList.Count);
            Assert.Empty(model.ProgrammeList);
            Assert.Equal("Projects within Project Group", model.ProjectsGrid.Title);
        }

        [Fact]
        public async Task Index_WithSourceNotProjectGroup_SetsIsProjectGroupModeFalse()
        {
            // Arrange
            var programs = new List<ProgramDto>
            {
                new() { ProgramNo = "P001", ProgramName = "Programme Alpha" }
            };
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programs));
            _appStateService.GetSessionAsync<string>(Arg.Any<string>())
                .Returns(string.Empty);

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramProjectViewModel>(viewResult.Model);
            Assert.False(model.IsProjectGroupMode);
            Assert.Single(model.ProgrammeList);
            Assert.Empty(model.ProjectGroupList);
            Assert.Equal("Projects within Programme", model.ProjectsGrid.Title);
        }

        [Fact]
        public async Task Index_InProjectGroupMode_SelectsFirstGroupWhenSessionIsEmpty()
        {
            // Arrange
            var projectGroups = new List<ProjectGroupDto>
            {
                new() { ProjectGroupName = "Alpha Group" },
                new() { ProjectGroupName = "Beta Group" }
            };
            _projectService.GetProjectGroupsByUserAsync()
                .Returns(ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(projectGroups));
            _appStateService.GetSessionAsync<string>(Arg.Any<string>())
                .Returns(string.Empty);

            // Act
            var result = await _controller.Index(null, "projectgroup");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramProjectViewModel>(viewResult.Model);
            Assert.Equal("Alpha Group", model.SelectedProjectGroup);
            await _appStateService.Received(1).SetSessionAsync(
                Arg.Is<string>(s => s.Contains("SelectedProjectGroup")),
                "Alpha Group"
            );
        }

        [Fact]
        public async Task Index_InProjectGroupMode_UsesSessionValueWhenValid()
        {
            // Arrange
            var projectGroups = new List<ProjectGroupDto>
            {
                new() { ProjectGroupName = "Alpha Group" },
                new() { ProjectGroupName = "Beta Group" }
            };
            _projectService.GetProjectGroupsByUserAsync()
                .Returns(ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(projectGroups));
            _appStateService.GetSessionAsync<string>(Arg.Any<string>())
                .Returns("Beta Group");

            // Act
            var result = await _controller.Index(null, "projectgroup");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramProjectViewModel>(viewResult.Model);
            Assert.Equal("Beta Group", model.SelectedProjectGroup);
        }

        [Fact]
        public async Task Index_InProjectGroupMode_FiltersOutNullOrEmptyProjectGroups()
        {
            // Arrange
            var projectGroups = new List<ProjectGroupDto>
            {
                new() { ProjectGroupName = "Valid Group" },
                new() { ProjectGroupName = null! },
                new() { ProjectGroupName = "" },
                new() { ProjectGroupName = "   " }
            };
            _projectService.GetProjectGroupsByUserAsync()
                .Returns(ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(projectGroups));
            _appStateService.GetSessionAsync<string>(Arg.Any<string>())
                .Returns(string.Empty);

            // Act
            var result = await _controller.Index(null, "projectgroup");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgramProjectViewModel>(viewResult.Model);
            Assert.Single(model.ProjectGroupList);
            Assert.Equal("Valid Group", model.ProjectGroupList.First().Value);
        }

        [Fact]
        public async Task LoadProgramProjectGrid_WithProjectGroup_CallsGetProjectsByProjectGroupAsync()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group A";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One", ProjectGroup = "Group A" }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects, paginationDto);
            var projectItems = new List<ProgramProjectItem>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One" }
            };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectsByProjectGroupAsync(queryParameters, projectGroup).Returns(serviceResponse);
            _mapper.Map<List<ProgramProjectItem>>(projects).Returns(projectItems);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProgramProjectGrid(request, null, projectGroup);

            // Assert
            await _projectService.Received(1).GetProjectsByProjectGroupAsync(queryParameters, projectGroup);
            await _projectService.DidNotReceive().GetProjectsByProgramAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProgramProjectItem>>(partialView.Model);
            Assert.Equal("Projects within Project Group", gridConfig.Title);
            Assert.Single(gridConfig.Data);
        }

        [Fact]
        public async Task LoadProgramProjectGrid_WithProgramNo_CallsGetProjectsByProgramAsync()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One" }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects, paginationDto);
            var projectItems = new List<ProgramProjectItem>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One" }
            };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectsByProgramAsync(queryParameters, programNo).Returns(serviceResponse);
            _mapper.Map<List<ProgramProjectItem>>(projects).Returns(projectItems);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProgramProjectGrid(request, programNo, null);

            // Assert
            await _projectService.Received(1).GetProjectsByProgramAsync(queryParameters, programNo);
            await _projectService.DidNotReceive().GetProjectsByProjectGroupAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProgramProjectItem>>(partialView.Model);
            Assert.Equal("Projects within Programme", gridConfig.Title);
        }

        [Fact]
        public async Task LoadProgramProjectGrid_WithBothParams_PrioritizesProjectGroup()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var projectGroup = "Group A";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projects = new List<ProjectDto> { new() { ParentProject = "PP001" } };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                projects, new PaginationDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _projectService.GetProjectsByProjectGroupAsync(queryParameters, projectGroup).Returns(serviceResponse);
            _mapper.Map<List<ProgramProjectItem>>(Arg.Any<List<ProjectDto>>()).Returns(new List<ProgramProjectItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProgramProjectGrid(request, programNo, projectGroup);

            // Assert
            await _projectService.Received(1).GetProjectsByProjectGroupAsync(queryParameters, projectGroup);
            await _projectService.DidNotReceive().GetProjectsByProgramAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetProjectTotals_WithProjectGroup_ReturnsProjectGroupTotals()
        {
            // Arrange
            var projectGroup = "Group A";
            var projects = new List<ProjectDto>
            {
                new() { BudgetCvl = 1000M, BudgetExt = 2000M, TransferIncome = 500, PlanCaseWorkDebit = 300M },
                new() { BudgetCvl = 500M, BudgetExt = 1500M, TransferIncome = 200, PlanCaseWorkDebit = 100M }
            };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects);

            _projectService.GetProjectsByProjectGroupAsync(
                Arg.Is<QueryParameters<string>>(q => q.PageSize == 9999), projectGroup)
                .Returns(serviceResponse);

            // Act
            var result = await _controller.GetProjectTotals(null, projectGroup);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonElement>(jsonResult);
            Assert.Equal(1500M, value.GetProperty("budgetCvl").GetDecimal());
            Assert.Equal(3500M, value.GetProperty("budgetExt").GetDecimal());
            Assert.Equal(700M,  value.GetProperty("transferIncome").GetDecimal());
            Assert.Equal(400M,  value.GetProperty("planCaseWorkDebit").GetDecimal());
            await _projectService.DidNotReceive().GetProjectsByProgramAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetProjectTotals_WithProgramNo_ReturnsProgrammeTotals()
        {
            // Arrange
            var programNo = "P001";
            var projects = new List<ProjectDto>
            {
                new() { BudgetCvl = 1000M, BudgetExt = 2000M, TransferIncome = 500, PlanCaseWorkDebit = 300M }
            };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects);

            _projectService.GetProjectsByProgramAsync(
                Arg.Is<QueryParameters<string>>(q => q.PageSize == 9999), programNo)
                .Returns(serviceResponse);

            var result = await _controller.GetProjectTotals(programNo, null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonElement>(jsonResult);
            Assert.Equal(1000M, value.GetProperty("budgetCvl").GetDecimal());
            Assert.Equal(2000M, value.GetProperty("budgetExt").GetDecimal());
            await _projectService.DidNotReceive().GetProjectsByProjectGroupAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetProjectTotals_WithBothParams_PrioritizesProjectGroup()
        {
            // Arrange
            var programNo = "P001";
            var projectGroup = "Group A";
            var projects = new List<ProjectDto> { new() { BudgetCvl = 1000M } };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects);

            _projectService.GetProjectsByProjectGroupAsync(
                Arg.Any<QueryParameters<string>>(), projectGroup)
                .Returns(serviceResponse);

            // Act
            var result = await _controller.GetProjectTotals(programNo, projectGroup);

            // Assert
            await _projectService.Received(1).GetProjectsByProjectGroupAsync(
                Arg.Any<QueryParameters<string>>(), projectGroup);
            await _projectService.DidNotReceive().GetProjectsByProgramAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetProjectTotals_WhenProjectGroupServiceFails_ReturnsZeroTotals()
        {
            // Arrange
            var projectGroup = "Group A";
            var errors = new List<ApiErrorDto> { new() { Message = "Service error" } };
            var serviceResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());

            _projectService.GetProjectsByProjectGroupAsync(
                Arg.Any<QueryParameters<string>>(), projectGroup)
                .Returns(serviceResponse);

            // Act
            var result = await _controller.GetProjectTotals(null, projectGroup);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonElement>(jsonResult);
            Assert.Equal(0M, value.GetProperty("budgetCvl").GetDecimal());
            Assert.Equal(0M, value.GetProperty("budgetExt").GetDecimal());
            Assert.Equal(0M, value.GetProperty("transferIncome").GetDecimal());
            Assert.Equal(0M, value.GetProperty("planCaseWorkDebit").GetDecimal());
        }

        [Fact]
        public async Task SaveProjectGroupSession_SavesProjectGroupToSession()
        {
            // Arrange
            var projectGroup = "Group A";

            // Act
            var result = await _controller.SaveProjectGroupSession(projectGroup);

            // Assert
            Assert.IsType<OkResult>(result);
            await _appStateService.Received(1).SetSessionAsync(
                Arg.Is<string>(s => s.Contains("SelectedProjectGroup")),
                projectGroup
            );
        }

        [Fact]
        public async Task SaveProjectGroupSession_WithEmptyString_SavesEmptyValue()
        {
            // Act
            var result = await _controller.SaveProjectGroupSession(string.Empty);

            // Assert
            Assert.IsType<OkResult>(result);
            await _appStateService.Received(1).SetSessionAsync(
                Arg.Is<string>(s => s.Contains("SelectedProjectGroup")),
                string.Empty
            );
        }

        #endregion

        private void SetupDropdownMocks()
        {
            _employeeService.GetAllManagersAsync()
                .Returns(ApiResponseDto<List<ManagerDto>>.SuccessResponse(new List<ManagerDto>()));
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(Enumerable.Empty<ProgramDto>()));
            _projectService.GetAllCustomersAsync()
                .Returns(ApiResponseDto<List<CustomerDto>>.SuccessResponse(new List<CustomerDto>()));
            _projectService.GetAllProjectGroupsAsync()
                .Returns(ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(new List<ProjectGroupDto>()));
            _projectService.GetAllContractsAsync()
                .Returns(ApiResponseDto<List<ContractDto>>.SuccessResponse(new List<ContractDto>()));
            _projectService.GetAllDiseasesAsync()
                .Returns(ApiResponseDto<List<DiseaseDto>>.SuccessResponse(new List<DiseaseDto>()));
            _projectService.GetAllStatusesAsync()
                .Returns(ApiResponseDto<List<StatusDto>>.SuccessResponse(new List<StatusDto>()));
        }

        private class JsonResponse
        {
            public bool success { get; set; }
            public string? message { get; set; }
            public object? data { get; set; }
            public object? errors { get; set; }
        }
    }
}
