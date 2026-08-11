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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProgrammeSelectControllerTest
{
    public class ProgrammeSelectControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProgramService _programService;
        private readonly IProjectService _projectService;
        private readonly IAppStateService _appStateService;
        private readonly ProgrammeSelectController _controller;

        public ProgrammeSelectControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _programService = Substitute.For<IProgramService>();
            _projectService = Substitute.For<IProjectService>();
            _appStateService = Substitute.For<IAppStateService>();
            _controller = new ProgrammeSelectController(_mapper, _programService, _projectService, _appStateService);
        }

        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private static List<ProgramDto> BuildProgramList() =>
        [
            new() { ProgramNo = "P001", ProgramName = "Programme Alpha" },
            new() { ProgramNo = "P002", ProgramName = "Programme Beta" }
        ];

        private static List<ProjectDto> BuildProjectList() =>
        [
            new() { ParentProject = "AH0001", Program = "P001" },
            new() { ParentProject = "AH0002", Program = "P001" }
        ];

        private void SetupProgramList(List<ProgramDto>? programs = null)
        {
            programs ??= BuildProgramList();
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programs));
        }

        private void SetupProjectsByProgram(List<ProjectDto>? projects = null)
        {
            projects ??= BuildProjectList();
            _projectService.GetProjectsByProgramAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects, new PaginationDto()));
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        #region Index Tests

        [Fact]
        public async Task Index_WithValidProgramNo_ReturnsViewWithSelectedProgramme()
        {
            // Arrange
            SetupProgramList();
            SetupProjectsByProgram();

            // Act
            var result = await _controller.Index("P001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgrammeSelectViewModel>(viewResult.Model);
            Assert.Equal("P001", model.SelectedProgramNo);
        }

        [Fact]
        public async Task Index_WithValidProgramNo_SavesProgramNoToSession()
        {
            // Arrange
            SetupProgramList();
            SetupProjectsByProgram();

            // Act
            await _controller.Index("P001");

            // Assert
            await _appStateService.Received(1).SetSessionAsync(Arg.Any<string>(), "P001");
        }

        [Fact]
        public async Task Index_WithInvalidProgramNo_DoesNotSaveToSession()
        {
            // Arrange
            SetupProgramList();
            SetupProjectsByProgram();

            // Act
            await _controller.Index("INVALID");

            // Assert
            await _appStateService.DidNotReceive().SetSessionAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Index_WithNullProgramNo_ReturnsEmptySelectedProgramNo()
        {
            // Arrange
            SetupProgramList();
            SetupProjectsByProgram();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgrammeSelectViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.SelectedProgramNo);
        }

        [Fact]
        public async Task Index_WithNullProgramNo_DoesNotSaveToSession()
        {
            // Arrange
            SetupProgramList();
            SetupProjectsByProgram();

            // Act
            await _controller.Index(null);

            // Assert
            await _appStateService.DidNotReceive().SetSessionAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Index_PopulatesProgrammeList_WithOrderedItems()
        {
            // Arrange — return out-of-order list to verify ordering
            var programs = new List<ProgramDto>
            {
                new() { ProgramNo = "P002", ProgramName = "Programme Beta" },
                new() { ProgramNo = "P001", ProgramName = "Programme Alpha" }
            };
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programs));
            SetupProjectsByProgram();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgrammeSelectViewModel>(viewResult.Model);
            Assert.Equal(2, model.ProgrammeList.Count);
            Assert.Equal("P001", model.ProgrammeList[0].Value);
            Assert.Equal("P002", model.ProgrammeList[1].Value);
        }

        [Fact]
        public async Task Index_WhenProgramServiceFails_ReturnsEmptyProgrammeList()
        {
            // Arrange
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Error" } }, new ApiMetaDto()));
            SetupProjectsByProgram(new List<ProjectDto>());

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgrammeSelectViewModel>(viewResult.Model);
            Assert.Empty(model.ProgrammeList);
        }

        [Fact]
        public async Task Index_ProjectsGrid_HasCorrectConfiguration()
        {
            // Arrange
            SetupProgramList();
            SetupProjectsByProgram();

            // Act
            var result = await _controller.Index("P001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgrammeSelectViewModel>(viewResult.Model);
            Assert.NotNull(model.ProjectsGrid);
            Assert.Equal("projectsGrid", model.ProjectsGrid!.GridId);
            Assert.Equal("Projects", model.ProjectsGrid.Title);
            Assert.Equal("ParentProject", model.ProjectsGrid.KeyProperty);
            Assert.False(model.ProjectsGrid.AllowAdd);
            Assert.True(model.ProjectsGrid.AllowEdit);
            Assert.False(model.ProjectsGrid.AllowDelete);
            Assert.True(model.ProjectsGrid.AllowView);
            Assert.Equal("editProject", model.ProjectsGrid.EditFunction);
            Assert.Equal("planProject", model.ProjectsGrid.ViewFunction);
        }

        [Fact]
        public async Task Index_WithValidProgramNo_ProjectsGridBindUrlContainsProgramNo()
        {
            // Arrange
            SetupProgramList();
            SetupProjectsByProgram();

            // Act
            var result = await _controller.Index("P001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProgrammeSelectViewModel>(viewResult.Model);
            Assert.Contains("P001", model.ProjectsGrid!.BindGridUrl);
        }

        [Fact]
        public async Task Index_WithEmptyProgramNo_DoesNotCallGetProjectsByProgram()
        {
            // Arrange
            SetupProgramList();
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());

            // Act
            await _controller.Index(null);

            // Assert
            await _projectService.DidNotReceive()
                .GetProjectsByProgramAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Index_WithValidProgramNo_CallsGetProjectsByProgram()
        {
            // Arrange
            SetupProgramList();
            SetupProjectsByProgram();

            // Act
            await _controller.Index("P001");

            // Assert
            await _projectService.Received(1)
                .GetProjectsByProgramAsync(Arg.Any<QueryParameters<string>>(), "P001");
        }

        #endregion

        #region SaveProgrammeSession Tests

        [Fact]
        public async Task SaveProgrammeSession_SavesProgramNoToSession()
        {
            // Act
            var result = await _controller.SaveProgrammeSession("P001");

            // Assert
            await _appStateService.Received(1).SetSessionAsync(Arg.Any<string>(), "P001");
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SaveProgrammeSession_ReturnsOk()
        {
            // Act
            var result = await _controller.SaveProgrammeSession("P001");

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SaveProgrammeSession_WithEmptyString_StillCallsSession()
        {
            // Act
            await _controller.SaveProgrammeSession(string.Empty);

            // Assert
            await _appStateService.Received(1).SetSessionAsync(Arg.Any<string>(), string.Empty);
        }

        #endregion

        #region LoadProjectsGrid Tests

        [Fact]
        public async Task LoadProjectsGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            SetupProjectsByProgram();

            // Act
            var result = await _controller.LoadProjectsGrid(request, "P001");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            Assert.IsType<DataGridConfig<ProgrammeSelectProjectItem>>(partialView.Model);
        }

        [Fact]
        public async Task LoadProjectsGrid_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            _controller.ModelState.AddModelError("Test", "Test error");

            // Act
            var result = await _controller.LoadProjectsGrid(request, "P001");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadProjectsGrid_WithEmptyProgramNo_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            var queryParameters = new QueryParameters<string>();
            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);

            // Act
            var result = await _controller.LoadProjectsGrid(request, string.Empty);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<ProgrammeSelectProjectItem>>(partialView.Model);
            Assert.Empty(grid.Data);
            await _projectService.DidNotReceive().GetProjectsByProgramAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadProjectsGrid_ProjectsGridHasCorrectGridId()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            SetupProjectsByProgram();

            // Act
            var result = await _controller.LoadProjectsGrid(request, "P001");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<ProgrammeSelectProjectItem>>(partialView.Model);
            Assert.Equal("projectsGrid", grid.GridId);
        }

        [Fact]
        public async Task LoadProjectsGrid_WithProjectName_FiltersResultsServerSide()
        {
            // Arrange — mock returns only the API-filtered results (server-side filtering now)
            var request = new PaginationFilter<string>();
            var filteredProjects = new List<ProjectDto>
            {
                new() { ParentProject = "AH0001", Program = "P001" },
                new() { ParentProject = "AH0002", Program = "P001" }
            };
            _projectService.GetProjectsByProgramAsync(Arg.Any<QueryParameters<string>>(), "P001")
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(filteredProjects, new PaginationDto()));
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProjectsGrid(request, "P001", "AH");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<ProgrammeSelectProjectItem>>(partialView.Model);
            Assert.Equal(2, grid.Data.Count);
            Assert.All(grid.Data, item => Assert.Contains("AH", item.ParentProject));
        }

        [Fact]
        public async Task LoadProjectsGrid_WithNullProjectName_ReturnsAllProjects()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            SetupProjectsByProgram();

            // Act
            var result = await _controller.LoadProjectsGrid(request, "P001", null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<ProgrammeSelectProjectItem>>(partialView.Model);
            Assert.Equal(2, grid.Data.Count);
        }

        [Fact]
        public async Task LoadProjectsGrid_SetsCorrectPaginationSortProperties()
        {
            // Arrange
            var request = new PaginationFilter<string> { SortBy = "ParentProject", Descending = true };
            SetupProjectsByProgram();

            // Act
            var result = await _controller.LoadProjectsGrid(request, "P001");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<ProgrammeSelectProjectItem>>(partialView.Model);
            Assert.Equal("ParentProject", grid.Pagination.SortColumn);
            Assert.True(grid.Pagination.SortDirection);
        }

        #endregion
    }
}
