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
using Microsoft.AspNetCore.Mvc.Rendering;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProjectProfitabilityControllerTest
{
    public class ProjectGroupProfitabilityControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProgramService _programService;
        private readonly IProjectService _projectService;
        private readonly IAppStateService _appStateService;
        private readonly ProjectProfitabilityController _controller;

        public ProjectGroupProfitabilityControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _programService = Substitute.For<IProgramService>();
            _projectService = Substitute.For<IProjectService>();
            _appStateService = Substitute.For<IAppStateService>();
            _controller = new ProjectProfitabilityController(_mapper, _programService, _projectService, _appStateService);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static JsonElement GetSummaryJson(OkObjectResult okResult)
        {
            var json = JsonSerializer.Serialize(okResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private static ApiResponseDto<List<ProjectGroupDto>> MakeProjectGroupResponse(
            params string[] groupNames)
        {
            var dtos = groupNames
                .Select(g => new ProjectGroupDto { ProjectGroupName = g })
                .ToList();
            return ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(dtos);
        }

        private static ApiResponseDto<List<ProjectProfitabilityDto>> MakeProfitabilityResponse(
            params ProjectProfitabilityDto[] items)
        {
            return ApiResponseDto<List<ProjectProfitabilityDto>>.SuccessResponse(
                items.ToList(),
                new PaginationDto { PageNumber = 1, PageSize = 100, TotalRecords = items.Length });
        }

        private static PaginationFilter<string> MakeGridRequest(int page = 1, int pageSize = 10) =>
            new() { Page = page, PageSize = pageSize };

        private ProjectProfitabilityDto MakeGroupItem(
            string jobCode,
            decimal staffCosts = 1000m,
            decimal budget = 5000m,
            decimal targetProfit = 500m) =>
            new()
            {
                JobCode = jobCode,
                JcTotalStaffCosts = staffCosts,
                BudgetCvl = budget,
                JcProfit = budget - staffCosts,
                TargetProfit = targetProfit,
                OffTarget = (budget - staffCosts) - targetProfit,
                ProgramNo = "P001"
            };

        // ── Index (project group mode) ─────────────────────────────────────

        #region Index — project group mode

        [Fact]
        public async Task Index_WithSourceProjectGroup_IsInProjectGroupMode()
        {
            // Arrange
            _projectService.GetProjectGroupsByUserAsync()
                .Returns(MakeProjectGroupResponse("Group1", "Group2"));
            _appStateService.GetSessionAsync<string>(Arg.Any<string>()).Returns((string?)null);

            // Act
            var result = await _controller.Index(null, "projectgroup");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityViewModel>(viewResult.Model);
            Assert.True(model.IsProjectGroupMode);
        }

        [Fact]
        public async Task Index_WithSourceProjectGroup_PopulatesProjectGroupList()
        {
            // Arrange
            _projectService.GetProjectGroupsByUserAsync()
                .Returns(MakeProjectGroupResponse("Group1", "Group2"));
            _appStateService.GetSessionAsync<string>(Arg.Any<string>()).Returns((string?)null);

            // Act
            var result = await _controller.Index(null, "projectgroup");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityViewModel>(viewResult.Model);
            Assert.Equal(2, model.ProjectGroupList.Count);
            Assert.Contains(model.ProjectGroupList, x => x.Value == "Group1");
            Assert.Contains(model.ProjectGroupList, x => x.Value == "Group2");
        }

        [Fact]
        public async Task Index_WithSourceProjectGroup_SelectsFirstGroupWhenSessionEmpty()
        {
            // Arrange
            _projectService.GetProjectGroupsByUserAsync()
                .Returns(MakeProjectGroupResponse("Alpha", "Beta"));
            _appStateService.GetSessionAsync<string>(Arg.Any<string>()).Returns((string?)null);

            // Act
            var result = await _controller.Index(null, "projectgroup");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityViewModel>(viewResult.Model);
            Assert.Equal("Alpha", model.SelectedProjectGroup);
        }

        [Fact]
        public async Task Index_WithSourceProjectGroup_RestoresGroupFromSession()
        {
            // Arrange
            _projectService.GetProjectGroupsByUserAsync()
                .Returns(MakeProjectGroupResponse("Alpha", "Beta"));
            _appStateService.GetSessionAsync<string>(Arg.Any<string>()).Returns("Beta");

            // Act
            var result = await _controller.Index(null, "projectgroup");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityViewModel>(viewResult.Model);
            Assert.Equal("Beta", model.SelectedProjectGroup);
        }

        [Fact]
        public async Task Index_WithSourceProjectGroup_InvalidSessionValue_FallsBackToFirstGroup()
        {
            // Arrange
            _projectService.GetProjectGroupsByUserAsync()
                .Returns(MakeProjectGroupResponse("Alpha", "Beta"));
            _appStateService.GetSessionAsync<string>(Arg.Any<string>()).Returns("INVALID_GROUP");

            // Act
            var result = await _controller.Index(null, "projectgroup");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityViewModel>(viewResult.Model);
            Assert.Equal("Alpha", model.SelectedProjectGroup);
        }

        [Fact]
        public async Task Index_WithSourceProjectGroup_DoesNotPopulateProgrammeList()
        {
            // Arrange
            _projectService.GetProjectGroupsByUserAsync()
                .Returns(MakeProjectGroupResponse("Group1"));
            _appStateService.GetSessionAsync<string>(Arg.Any<string>()).Returns((string?)null);

            // Act
            var result = await _controller.Index(null, "projectgroup");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityViewModel>(viewResult.Model);
            Assert.Empty(model.ProgrammeList);
            // Programme service must NOT be called in project group mode
            await _programService.DidNotReceive().GetAllProgramsAsync();
        }

        [Fact]
        public async Task Index_WithSourceProjectGroup_SetsSessionWithSelectedGroup()
        {
            // Arrange
            _projectService.GetProjectGroupsByUserAsync()
                .Returns(MakeProjectGroupResponse("Group1", "Group2"));
            _appStateService.GetSessionAsync<string>(Arg.Any<string>()).Returns("Group2");

            // Act
            await _controller.Index(null, "projectgroup");

            // Assert
            await _appStateService.Received(1).SetSessionAsync(Arg.Any<string>(), "Group2");
        }

        [Fact]
        public async Task Index_WithoutSourceParam_IsProgrammeModeNotProjectGroupMode()
        {
            // Arrange
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(
                    new List<ProgramDto> { new() { ProgramNo = "P001", ProgramName = "Prog One" } }));
            _appStateService.GetSessionAsync<string>(Arg.Any<string>()).Returns((string?)null);

            // Act
            var result = await _controller.Index(null, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityViewModel>(viewResult.Model);
            Assert.False(model.IsProjectGroupMode);
        }

        #endregion

        // ── LoadProjectProfitabilityGrid (project group mode) ─────────────

        #region LoadProjectProfitabilityGrid — project group mode

        [Fact]
        public async Task LoadProjectProfitabilityGrid_WithProjectGroup_CallsGroupProfitabilityService()
        {
            // Arrange
            var request = MakeGridRequest();
            var projectGroup = "Group1";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var items = new List<ProjectProfitabilityDto> { MakeGroupItem("PP001"), MakeGroupItem("PP002") };
            var apiResponse = MakeProfitabilityResponse(items.ToArray());

            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _projectService.GetProjectGroupProfitabilityAsync(query, projectGroup, "all").Returns(apiResponse);
            _mapper.Map<List<ProjectProfitabilityItem>>(apiResponse.Data).Returns(new List<ProjectProfitabilityItem>
            {
                new() { JobCode = "PP001" },
                new() { JobCode = "PP002" }
            });
            _mapper.Map<PaginationModel>(apiResponse.Pagination).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProjectProfitabilityGrid(request, null, projectGroup, "all");

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            await _projectService.Received(1).GetProjectGroupProfitabilityAsync(query, projectGroup, "all");
        }

        [Fact]
        public async Task LoadProjectProfitabilityGrid_WithProjectGroup_DoesNotCallProgrammeProfitabilityService()
        {
            // Arrange
            var request = MakeGridRequest();
            var projectGroup = "Group1";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = MakeProfitabilityResponse();

            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _projectService.GetProjectGroupProfitabilityAsync(query, projectGroup, "all").Returns(apiResponse);
            _mapper.Map<List<ProjectProfitabilityItem>>(Arg.Any<List<ProjectProfitabilityDto>>())
                .Returns(new List<ProjectProfitabilityItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            await _controller.LoadProjectProfitabilityGrid(request, null, projectGroup, "all");

            // Assert
            await _projectService.DidNotReceive().GetProjectProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadProjectProfitabilityGrid_WithBothProgramNoAndProjectGroup_ProjectGroupTakesPrecedence()
        {
            // Arrange — both supplied; projectGroup should be used
            var request = MakeGridRequest();
            var projectGroup = "Group1";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = MakeProfitabilityResponse();

            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _projectService.GetProjectGroupProfitabilityAsync(query, projectGroup, "all").Returns(apiResponse);
            _mapper.Map<List<ProjectProfitabilityItem>>(Arg.Any<List<ProjectProfitabilityDto>>())
                .Returns(new List<ProjectProfitabilityItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            await _controller.LoadProjectProfitabilityGrid(request, "P001", projectGroup, "all");

            // Assert
            await _projectService.Received(1).GetProjectGroupProfitabilityAsync(query, projectGroup, "all");
            await _projectService.DidNotReceive().GetProjectProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadProjectProfitabilityGrid_WithProjectGroup_WhenServiceFails_Returns500()
        {
            // Arrange
            var request = MakeGridRequest();
            var projectGroup = "Group1";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            var failResponse = ApiResponseDto<List<ProjectProfitabilityDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _projectService.GetProjectGroupProfitabilityAsync(query, projectGroup, "all").Returns(failResponse);

            // Act
            var result = await _controller.LoadProjectProfitabilityGrid(request, null, projectGroup, "all");

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        [Fact]
        public async Task LoadProjectProfitabilityGrid_WithProjectGroup_GridTitleIsProjectGroupProfitability()
        {
            // Arrange
            var request = MakeGridRequest();
            var projectGroup = "Group1";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = MakeProfitabilityResponse();

            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _projectService.GetProjectGroupProfitabilityAsync(query, projectGroup, "all").Returns(apiResponse);
            _mapper.Map<List<ProjectProfitabilityItem>>(Arg.Any<List<ProjectProfitabilityDto>>())
                .Returns(new List<ProjectProfitabilityItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProjectProfitabilityGrid(request, null, projectGroup, "all");

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectProfitabilityItem>>(partialResult.Model);
            Assert.Equal("Project Group Profitability", gridConfig.Title);
        }

        [Fact]
        public async Task LoadProjectProfitabilityGrid_WithNullBothProgramNoAndProjectGroup_ReturnsEmptyGrid()
        {
            // Arrange
            var request = MakeGridRequest();

            // Act
            var result = await _controller.LoadProjectProfitabilityGrid(request, null, null, "all");

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectProfitabilityItem>>(partialResult.Model);
            Assert.Empty(gridConfig.Data);
        }

        #endregion

        // ── GetProfitabilitySummary (project group mode) ───────────────────

        #region GetProfitabilitySummary — project group mode

        [Fact]
        public async Task GetProfitabilitySummary_WithProjectGroup_CallsGroupProfitabilityService()
        {
            // Arrange
            var projectGroup = "Group1";
            var items = new List<ProjectProfitabilityDto>
            {
                new() { JobCode = "PP001", JcTotalStaffCosts = 1000m, JcTotalTestCosts = 200m, JcTotalAnimalCosts = 50m,
                        JcTotalAdditionalCosts = 100m, TotalCosts = 1350m, BudgetCvl = 5000m,
                        JcProfit = 3650m, TargetProfit = 3000m, OffTarget = 650m },
                new() { JobCode = "PP002", JcTotalStaffCosts = 2000m, JcTotalTestCosts = 400m, JcTotalAnimalCosts = 80m,
                        JcTotalAdditionalCosts = 200m, TotalCosts = 2680m, BudgetCvl = 6000m,
                        JcProfit = 3320m, TargetProfit = 3000m, OffTarget = 320m }
            };
            var apiResponse = MakeProfitabilityResponse(items.ToArray());

            _projectService.GetProjectGroupProfitabilityAsync(
                Arg.Is<QueryParameters<string>>(q => q.Page == 1 && q.PageSize == int.MaxValue),
                projectGroup, "all")
                .Returns(apiResponse);

            // Act
            var result = await _controller.GetProfitabilitySummary(null, projectGroup, "all");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            await _projectService.Received(1).GetProjectGroupProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), projectGroup, "all");
        }

        [Fact]
        public async Task GetProfitabilitySummary_WithProjectGroup_DoesNotCallProgrammeSummaryService()
        {
            // Arrange
            var projectGroup = "Group1";
            var apiResponse = MakeProfitabilityResponse();

            _projectService.GetProjectGroupProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), projectGroup, "all")
                .Returns(apiResponse);

            // Act
            await _controller.GetProfitabilitySummary(null, projectGroup, "all");

            // Assert
            await _projectService.DidNotReceive().GetProjectProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetProfitabilitySummary_WithProjectGroup_ReturnsTotalsCorrectly()
        {
            // Arrange
            var projectGroup = "Group1";
            var items = new List<ProjectProfitabilityDto>
            {
                new() { JcTotalStaffCosts = 1000m, JcTotalTestCosts = 100m, JcTotalAnimalCosts = 50m,
                        JcTotalAdditionalCosts = 25m, TotalCosts = 1175m, BudgetCvl = 5000m,
                        JcProfit = 3825m, TargetProfit = 3000m, OffTarget = 825m },
                new() { JcTotalStaffCosts = 2000m, JcTotalTestCosts = 200m, JcTotalAnimalCosts = 60m,
                        JcTotalAdditionalCosts = 40m, TotalCosts = 2300m, BudgetCvl = 6000m,
                        JcProfit = 3700m, TargetProfit = 3000m, OffTarget = 700m }
            };
            var apiResponse = MakeProfitabilityResponse(items.ToArray());

            _projectService.GetProjectGroupProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), projectGroup, "all")
                .Returns(apiResponse);

            // Act
            var result = await _controller.GetProfitabilitySummary(null, projectGroup, "all");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var summary = GetSummaryJson(okResult);
            Assert.Equal(3000m, summary.GetProperty("totalStaffCosts").GetDecimal());    // 1000 + 2000
            Assert.Equal(300m,  summary.GetProperty("totalTestCosts").GetDecimal());     // 100 + 200
            Assert.Equal(110m,  summary.GetProperty("totalAnimalCosts").GetDecimal());   // 50 + 60
            Assert.Equal(65m,   summary.GetProperty("totalAdditionalCosts").GetDecimal()); // 25 + 40
            Assert.Equal(7525m, summary.GetProperty("totalProfit").GetDecimal());        // 3825 + 3700
        }

        [Fact]
        public async Task GetProfitabilitySummary_WithProjectGroup_NullProgrammeTargetInResponse()
        {
            // Arrange — project group mode does not return a single programme target
            var projectGroup = "Group1";
            var items = new List<ProjectProfitabilityDto>
            {
                new() { JcProfit = 2000m, TargetProfit = 1500m, OffTarget = 500m, BudgetCvl = 5000m, ProgrammeTarget = null }
            };
            var apiResponse = MakeProfitabilityResponse(items.ToArray());

            _projectService.GetProjectGroupProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), projectGroup, "all")
                .Returns(apiResponse);

            // Act
            var result = await _controller.GetProfitabilitySummary(null, projectGroup, "all");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetProfitabilitySummary_WithProjectGroup_WhenServiceFails_Returns500()
        {
            // Arrange
            var projectGroup = "Group1";
            var errors = new List<ApiErrorDto> { new() { Message = "Error" } };
            var failResponse = ApiResponseDto<List<ProjectProfitabilityDto>>.FailureResponse(errors, new ApiMetaDto());

            _projectService.GetProjectGroupProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), projectGroup, "all")
                .Returns(failResponse);

            // Act
            var result = await _controller.GetProfitabilitySummary(null, projectGroup, "all");

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetProfitabilitySummary_WhenBothProgramNoAndProjectGroupAreNull_ReturnsOkWithDefaults()
        {
            // Act
            var result = await _controller.GetProfitabilitySummary(null, null, "all");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            // Neither service should be called
            await _projectService.DidNotReceive().GetProjectProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<string>());
            await _projectService.DidNotReceive().GetProjectGroupProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData("approved")]
        [InlineData("not-approved")]
        [InlineData("all")]
        public async Task GetProfitabilitySummary_WithProjectGroup_ForwardsWorkTypeFilter(string workTypeFilter)
        {
            // Arrange
            var projectGroup = "Group1";
            var apiResponse = MakeProfitabilityResponse();

            _projectService.GetProjectGroupProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), projectGroup, workTypeFilter)
                .Returns(apiResponse);

            // Act
            await _controller.GetProfitabilitySummary(null, projectGroup, workTypeFilter);

            // Assert
            await _projectService.Received(1).GetProjectGroupProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), projectGroup, workTypeFilter);
        }

        #endregion
    }
}
