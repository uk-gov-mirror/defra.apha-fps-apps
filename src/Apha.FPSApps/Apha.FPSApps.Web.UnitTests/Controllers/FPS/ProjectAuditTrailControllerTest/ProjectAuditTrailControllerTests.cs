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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProjectAuditTrailControllerTest
{
    public class ProjectAuditTrailControllerTests
    {
        private const string TestProject = "PROJ001";

        private readonly IMapper _mapper;
        private readonly IProjectAuditTrailService _auditTrailService;
        private readonly IProjectService _projectService;
        private readonly ProjectAuditTrailController _controller;

        public ProjectAuditTrailControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _auditTrailService = Substitute.For<IProjectAuditTrailService>();
            _projectService = Substitute.For<IProjectService>();
            _controller = new ProjectAuditTrailController(_mapper, _auditTrailService, _projectService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        // ── Index ────────────────────────────────────────────────────────────

        #region Index

        [Fact]
        public async Task Index_ServiceReturnsProjectList_ReturnsViewResultWithPopulatedProjectList()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PROJ001" },
                new() { ParentProject = "PROJ002" }
            };
            var projectResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects);
            _projectService.GetAllProjectsAsync().Returns(projectResponse);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsType<ProjectAuditTrailViewModel>(viewResult.Model);
            Assert.Equal(2, viewModel.ProjectList.Count);
        }

        [Fact]
        public async Task Index_ProjectServiceReturnsEmpty_ReturnsViewResultWithEmptyProjectList()
        {
            // Arrange
            var projectResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());
            _projectService.GetAllProjectsAsync().Returns(projectResponse);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsType<ProjectAuditTrailViewModel>(viewResult.Model);
            Assert.Empty(viewModel.ProjectList);
        }

        [Fact]
        public async Task Index_ProjectServiceReturnsFailure_ReturnsViewResultWithEmptyProjectList()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "ERROR", Message = "API error" } };
            var projectResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());
            _projectService.GetAllProjectsAsync().Returns(projectResponse);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsType<ProjectAuditTrailViewModel>(viewResult.Model);
            Assert.Empty(viewModel.ProjectList);
        }

        [Fact]
        public async Task Index_Always_ReturnsViewResultWithFiveGridConfigs()
        {
            // Arrange
            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsType<ProjectAuditTrailViewModel>(viewResult.Model);
            Assert.NotNull(viewModel.ProjectLogsGrid);
            Assert.NotNull(viewModel.StaffJobLogsGrid);
            Assert.NotNull(viewModel.TestRequirementLogsGrid);
            Assert.NotNull(viewModel.AnimalRequestLogsGrid);
            Assert.NotNull(viewModel.AdditionalCostLogsGrid);
        }

        #endregion

        // ── LoadProjectLogsGrid ──────────────────────────────────────────────

        #region LoadProjectLogsGrid

        [Fact]
        public async Task LoadProjectLogsGrid_ValidProjectServiceReturnsData_ReturnsPartialViewWithGridData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var logs = new List<ProjectLogDto> { new() };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var serviceResponse = ApiResponseDto<List<ProjectLogDto>>.SuccessResponse(logs, paginationDto);
            var items = new List<ProjectLogItem> { new() };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _auditTrailService.GetProjectLogsAsync(queryParameters, TestProject, null, null)
                .Returns(serviceResponse);
            _mapper.Map<List<ProjectLogItem>>(Arg.Any<List<ProjectLogDto>>()).Returns(items);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadProjectLogsGrid(request, TestProject);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectLogItem>>(partialView.Model);
            Assert.Single(gridConfig.Data);
        }

        [Fact]
        public async Task LoadProjectLogsGrid_NullProject_ReturnsEmptyGridPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadProjectLogsGrid(request, null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectLogItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
            // Service should NOT be called when project is null/empty
            await _auditTrailService.DidNotReceive()
                .GetProjectLogsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>(),
                    Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>());
        }

        [Fact]
        public async Task LoadProjectLogsGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page is required");
            var request = new PaginationFilter<string>();

            // Act
            var result = await _controller.LoadProjectLogsGrid(request, TestProject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Invalid request data", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task LoadProjectLogsGrid_ServiceReturnsFailure_ReturnsPartialViewWithEmptyItems()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            var serviceResponse = ApiResponseDto<List<ProjectLogDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _auditTrailService.GetProjectLogsAsync(queryParameters, TestProject, null, null)
                .Returns(serviceResponse);

            // Act
            var result = await _controller.LoadProjectLogsGrid(request, TestProject);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectLogItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
            _mapper.DidNotReceive().Map<List<ProjectLogItem>>(Arg.Any<List<ProjectLogDto>>());
        }

        #endregion

        // ── LoadStaffJobLogsGrid ─────────────────────────────────────────────

        #region LoadStaffJobLogsGrid

        [Fact]
        public async Task LoadStaffJobLogsGrid_ValidProjectServiceReturnsData_ReturnsPartialViewWithGridData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var logs = new List<StaffJobLogDto> { new() };
            var serviceResponse = ApiResponseDto<List<StaffJobLogDto>>.SuccessResponse(logs, new PaginationDto());
            var items = new List<StaffJobLogItem> { new() };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _auditTrailService.GetStaffJobLogsAsync(queryParameters, TestProject, null, null)
                .Returns(serviceResponse);
            _mapper.Map<List<StaffJobLogItem>>(Arg.Any<List<StaffJobLogDto>>()).Returns(items);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadStaffJobLogsGrid(request, TestProject);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<StaffJobLogItem>>(partialView.Model);
            Assert.Single(gridConfig.Data);
        }

        [Fact]
        public async Task LoadStaffJobLogsGrid_NullProject_ReturnsEmptyGridPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadStaffJobLogsGrid(request, null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<StaffJobLogItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
            await _auditTrailService.DidNotReceive()
                .GetStaffJobLogsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>(),
                    Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>());
        }

        [Fact]
        public async Task LoadStaffJobLogsGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page is required");
            var request = new PaginationFilter<string>();

            // Act
            var result = await _controller.LoadStaffJobLogsGrid(request, TestProject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadStaffJobLogsGrid_ServiceReturnsFailure_ReturnsPartialViewWithEmptyItems()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            var serviceResponse = ApiResponseDto<List<StaffJobLogDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _auditTrailService.GetStaffJobLogsAsync(queryParameters, TestProject, null, null)
                .Returns(serviceResponse);

            // Act
            var result = await _controller.LoadStaffJobLogsGrid(request, TestProject);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<StaffJobLogItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
        }

        #endregion

        // ── LoadTestRequirementLogsGrid ──────────────────────────────────────

        #region LoadTestRequirementLogsGrid

        [Fact]
        public async Task LoadTestRequirementLogsGrid_ValidProjectServiceReturnsData_ReturnsPartialViewWithGridData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var logs = new List<TestRequirementLogDto> { new() };
            var serviceResponse = ApiResponseDto<List<TestRequirementLogDto>>.SuccessResponse(logs, new PaginationDto());
            var items = new List<TestRequirementLogItem> { new() };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _auditTrailService.GetTestRequirementLogsAsync(queryParameters, TestProject, null, null)
                .Returns(serviceResponse);
            _mapper.Map<List<TestRequirementLogItem>>(Arg.Any<List<TestRequirementLogDto>>()).Returns(items);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadTestRequirementLogsGrid(request, TestProject);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<TestRequirementLogItem>>(partialView.Model);
            Assert.Single(gridConfig.Data);
        }

        [Fact]
        public async Task LoadTestRequirementLogsGrid_NullProject_ReturnsEmptyGridPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadTestRequirementLogsGrid(request, null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<TestRequirementLogItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
        }

        [Fact]
        public async Task LoadTestRequirementLogsGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page is required");
            var request = new PaginationFilter<string>();

            // Act
            var result = await _controller.LoadTestRequirementLogsGrid(request, TestProject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadTestRequirementLogsGrid_ServiceReturnsFailure_ReturnsPartialViewWithEmptyItems()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResponse = ApiResponseDto<List<TestRequirementLogDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR" } }, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _auditTrailService.GetTestRequirementLogsAsync(queryParameters, TestProject, null, null)
                .Returns(serviceResponse);

            // Act
            var result = await _controller.LoadTestRequirementLogsGrid(request, TestProject);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<TestRequirementLogItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
        }

        #endregion

        // ── LoadAnimalRequestLogsGrid ────────────────────────────────────────

        #region LoadAnimalRequestLogsGrid

        [Fact]
        public async Task LoadAnimalRequestLogsGrid_ValidProjectServiceReturnsData_ReturnsPartialViewWithGridData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var logs = new List<AnimalRequestLogDto> { new() };
            var serviceResponse = ApiResponseDto<List<AnimalRequestLogDto>>.SuccessResponse(logs, new PaginationDto());
            var items = new List<AnimalRequestLogItem> { new() };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _auditTrailService.GetAnimalRequestLogsAsync(queryParameters, TestProject, null, null)
                .Returns(serviceResponse);
            _mapper.Map<List<AnimalRequestLogItem>>(Arg.Any<List<AnimalRequestLogDto>>()).Returns(items);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadAnimalRequestLogsGrid(request, TestProject);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<AnimalRequestLogItem>>(partialView.Model);
            Assert.Single(gridConfig.Data);
        }

        [Fact]
        public async Task LoadAnimalRequestLogsGrid_NullProject_ReturnsEmptyGridPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadAnimalRequestLogsGrid(request, null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<AnimalRequestLogItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
        }

        [Fact]
        public async Task LoadAnimalRequestLogsGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page is required");
            var request = new PaginationFilter<string>();

            // Act
            var result = await _controller.LoadAnimalRequestLogsGrid(request, TestProject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadAnimalRequestLogsGrid_ServiceReturnsFailure_ReturnsPartialViewWithEmptyItems()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResponse = ApiResponseDto<List<AnimalRequestLogDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR" } }, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _auditTrailService.GetAnimalRequestLogsAsync(queryParameters, TestProject, null, null)
                .Returns(serviceResponse);

            // Act
            var result = await _controller.LoadAnimalRequestLogsGrid(request, TestProject);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<AnimalRequestLogItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
        }

        #endregion

        // ── LoadAdditionalCostLogsGrid ───────────────────────────────────────

        #region LoadAdditionalCostLogsGrid

        [Fact]
        public async Task LoadAdditionalCostLogsGrid_ValidProjectServiceReturnsData_ReturnsPartialViewWithGridData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var logs = new List<AdditionalCostLogDto> { new() };
            var serviceResponse = ApiResponseDto<List<AdditionalCostLogDto>>.SuccessResponse(logs, new PaginationDto());
            var items = new List<AdditionalCostLogItem> { new() };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _auditTrailService.GetAdditionalCostLogsAsync(queryParameters, TestProject, null, null)
                .Returns(serviceResponse);
            _mapper.Map<List<AdditionalCostLogItem>>(Arg.Any<List<AdditionalCostLogDto>>()).Returns(items);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadAdditionalCostLogsGrid(request, TestProject);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<AdditionalCostLogItem>>(partialView.Model);
            Assert.Single(gridConfig.Data);
        }

        [Fact]
        public async Task LoadAdditionalCostLogsGrid_NullProject_ReturnsEmptyGridPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadAdditionalCostLogsGrid(request, null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<AdditionalCostLogItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
        }

        [Fact]
        public async Task LoadAdditionalCostLogsGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page is required");
            var request = new PaginationFilter<string>();

            // Act
            var result = await _controller.LoadAdditionalCostLogsGrid(request, TestProject);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadAdditionalCostLogsGrid_ServiceReturnsFailure_ReturnsPartialViewWithEmptyItems()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResponse = ApiResponseDto<List<AdditionalCostLogDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR" } }, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _auditTrailService.GetAdditionalCostLogsAsync(queryParameters, TestProject, null, null)
                .Returns(serviceResponse);

            // Act
            var result = await _controller.LoadAdditionalCostLogsGrid(request, TestProject);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<AdditionalCostLogItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
        }

        #endregion
    }
}
