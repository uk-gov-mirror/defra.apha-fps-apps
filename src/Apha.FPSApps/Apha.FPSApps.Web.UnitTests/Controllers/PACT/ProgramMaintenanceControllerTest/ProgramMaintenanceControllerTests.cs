using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.ProgramMaintenanceControllerTest
{
    public class ProgramMaintenanceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProgramService _programService;
        private readonly IProjectService _projectService;
        private readonly ProgramMaintenanceController _controller;

        public ProgramMaintenanceControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _programService = Substitute.For<IProgramService>();
            _projectService = Substitute.For<IProjectService>();
            _controller = new ProgramMaintenanceController(_mapper, _programService, _projectService);

            // Setup TempData
            _controller.TempData = Substitute.For<ITempDataDictionary>();
        }

        [Fact]
        public async Task Index_WithNoProgramNo_ReturnsViewWithFirstProgramSelected()
        {
            // Arrange
            SetupProgramList();
            SetupProjectsGridMapper();
            _projectService.GetPagedPactProjectsByProgramAsync(Arg.Any<QueryParameters<string>>(), "P001")
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([], new PaginationDto()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PactProgramMaintenanceViewModel>(viewResult.Model);
            Assert.Equal("P001", model.SelectedProgramNo);
            Assert.Equal("projectsGrid", model.ProjectsGrid.GridId);
        }

        [Fact]
        public async Task Index_WithValidProgramNo_SelectsThatProgram()
        {
            // Arrange
            var programs = new List<ProgramDto>
            {
                new() { ProgramNo = "P001", ProgramName = "One" },
                new() { ProgramNo = "P002", ProgramName = "Two" }
            };
            SetupProgramList(programs);
            SetupProjectsGridMapper();
            _projectService.GetPagedPactProjectsByProgramAsync(Arg.Any<QueryParameters<string>>(), "P002")
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([], new PaginationDto()));

            // Act
            var result = await _controller.Index("P002");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PactProgramMaintenanceViewModel>(viewResult.Model);
            Assert.Equal("P002", model.SelectedProgramNo);
        }

        [Fact]
        public async Task Index_WithInvalidProgramNo_FallsBackToFirstProgram()
        {
            // Arrange
            SetupProgramList();
            SetupProjectsGridMapper();
            _projectService.GetPagedPactProjectsByProgramAsync(Arg.Any<QueryParameters<string>>(), "INVALID")
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([], new PaginationDto()));

            // Act
            var result = await _controller.Index("INVALID");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PactProgramMaintenanceViewModel>(viewResult.Model);
            Assert.Equal("P001", model.SelectedProgramNo);
            // Verify the project service was called with the invalid program (current controller behavior)
            await _projectService.Received(1).GetPagedPactProjectsByProgramAsync(Arg.Any<QueryParameters<string>>(), "INVALID");
        }

        [Fact]
        public async Task Index_ProgramListEmpty_ReturnsViewWithEmptySelection()
        {
            // Arrange
            _programService.GetAllProgramsForAllUsersAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse([]));
            _projectService.GetPagedPactProjectsByProgramAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([], new PaginationDto()));
            SetupProjectsGridMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PactProgramMaintenanceViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.SelectedProgramNo);
            Assert.Empty(model.ProgramList);
        }

        [Fact]
        public async Task Index_PopulatesProgramList()
        {
            // Arrange
            var programs = new List<ProgramDto>
            {
                new() { ProgramNo = "P001", ProgramName = "One" },
                new() { ProgramNo = "P002", ProgramName = "Two" }
            };
            SetupProgramList(programs);
            SetupProjectsGridMapper();
            _projectService.GetPagedPactProjectsByProgramAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([], new PaginationDto()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<PactProgramMaintenanceViewModel>(viewResult.Model);
            Assert.Equal(2, model.ProgramList.Count);
            Assert.Equal("P001", model.ProgramList[0].Value);
            Assert.Equal("P001 - One", model.ProgramList[0].Text);
        }
 

        [Fact]
        public async Task GetProgram_ProgramFound_ReturnsSuccessJson()
        {
            // Arrange
            var dto = new ProgramDto { ProgramNo = "P001", ProgramName = "Test" };
            _programService.GetProgramByIdAsync("P001")
                .Returns(ApiResponseDto<ProgramDto?>.SuccessResponse(dto));
            _mapper.Map<ProgramViewModel>(dto)
                .Returns(new ProgramViewModel { ProgramNo = "P001", ProgramName = "Test" });

            // Act
            var result = await _controller.GetProgram("P001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetProgram_ProgramNotFound_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found" } };
            _programService.GetProgramByIdAsync("MISSING")
                .Returns(ApiResponseDto<ProgramDto?>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetProgram("MISSING");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Program not found.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task GetProgram_DataIsNull_ReturnsFailureJson()
        {
            // Arrange
            _programService.GetProgramByIdAsync("P001")
                .Returns(new ApiResponseDto<ProgramDto?> { Success = true, Data = null });

            // Act
            var result = await _controller.GetProgram("P001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Save_ValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var model = new ProgramViewModel { ProgramNo = "P001", ProgramName = "Test" };
            _mapper.Map<ProgramDto>(model).Returns(new ProgramDto { ProgramNo = "P001", ProgramName = "Test" });
            _programService.UpdateProgramAsync(Arg.Any<ProgramDto>())
                .Returns(ApiResponseDto<ProgramDto>.SuccessResponse(new ProgramDto()));

            // Act
            var result = await _controller.Save(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal("Program saved successfully.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Save_InvalidModelState_ReturnsValidationErrors()
        {
            // Arrange
            var model = new ProgramViewModel { ProgramNo = "", ProgramName = "" };
            _controller.ModelState.AddModelError("$.ProgramNo", "Program number is required");

            // Act
            var result = await _controller.Save(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Please correct the errors below.", element.GetProperty("message").GetString());

            var errors = element.GetProperty("errors");
            Assert.True(errors.GetArrayLength() > 0);
            // Verify $. prefix is stripped
            Assert.Equal("ProgramNo", errors[0].GetProperty("field").GetString());
        }

        [Fact]
        public async Task Save_ModelStateErrorWithoutDollarPrefix_KeepsFieldNameAsIs()
        {
            // Arrange
            var model = new ProgramViewModel { ProgramNo = "P001", ProgramName = "Test" };
            _controller.ModelState.AddModelError("ProgramName", "Name is required");

            // Act
            var result = await _controller.Save(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            var errors = element.GetProperty("errors");
            Assert.Equal("ProgramName", errors[0].GetProperty("field").GetString());
        }

        [Fact]
        public async Task Save_ServiceReturnsFailure_ReturnsErrorJson()
        {
            // Arrange
            var model = new ProgramViewModel { ProgramNo = "P001", ProgramName = "Test" };
            _mapper.Map<ProgramDto>(model).Returns(new ProgramDto { ProgramNo = "P001", ProgramName = "Test" });
            var errors = new List<ApiErrorDto> { new() { Code = "CONFLICT", Message = "Duplicate program." } };
            _programService.UpdateProgramAsync(Arg.Any<ProgramDto>())
                .Returns(ApiResponseDto<ProgramDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Save(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Duplicate program.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task LoadProjectsGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _projectService.GetPagedPactProjectsByProgramAsync(Arg.Any<QueryParameters<string>>(), "P001")
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([], new PaginationDto()));
            SetupProjectsGridMapper();

            // Act
            var result = await _controller.LoadProjectsGrid(request, "P001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<ProgramProjectItem>>(partial.Model);
        }

        [Fact]
        public async Task LoadProjectsGrid_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Test error");

            // Act
            var result = await _controller.LoadProjectsGrid(new PaginationFilter<string> { Filter = "{}" }, "P001");

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadProjectsGrid_EmptyProgramNo_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.LoadProjectsGrid(new PaginationFilter<string> { Filter = "{}" }, string.Empty);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadProjectsGrid_NullProgramNo_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.LoadProjectsGrid(new PaginationFilter<string> { Filter = "{}" }, null!);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private void SetupProgramList(List<ProgramDto>? programs = null)
        {
            programs ??= [new ProgramDto { ProgramNo = "P001", ProgramName = "Program One" }];
            _programService.GetAllProgramsForAllUsersAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(programs));
        }

        private void SetupProjectsGridMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ProgramProjectItem>>(Arg.Any<List<ProjectDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }
    }
}
