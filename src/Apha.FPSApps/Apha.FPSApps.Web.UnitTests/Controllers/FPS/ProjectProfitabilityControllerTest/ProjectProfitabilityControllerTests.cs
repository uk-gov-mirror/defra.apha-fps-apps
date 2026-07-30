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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProjectProfitabilityControllerTest
{
    public class ProjectProfitabilityControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProgramService _programService;
        private readonly IProjectService _projectService;
        private readonly IAppStateService _appStateService;
        private readonly ProjectProfitabilityController _controller;

        public ProjectProfitabilityControllerTests()
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

        private static ApiResponseDto<IEnumerable<ProgramDto>> MakeProgramResponse(
            params (string no, string name)[] programmes)
        {
            var dtos = programmes
                .Select(p => new ProgramDto { ProgramNo = p.no, ProgramName = p.name })
                .Cast<ProgramDto>();
            return ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(dtos);
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

        private ProjectProfitabilityDto MakeItem(
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
                ProgramNo = "P001",
                ProgrammeTarget = 10000m
            };

        // ── Index ─────────────────────────────────────────────────────────────

        #region Index

        [Fact]
        public async Task Index_WithValidProgramNo_ReturnsViewWithCorrectModel()
        {
            // Arrange
            var programNo = "P001";
            _programService.GetAllProgramsAsync()
                .Returns(MakeProgramResponse(("P001", "Programme One"), ("P002", "Programme Two")));
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });

            // Act
            var result = await _controller.Index(programNo);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityViewModel>(viewResult.Model);
            Assert.Equal("P001", model.SelectedProgramNo);
            Assert.Equal(2, model.ProgrammeList.Count);
        }

        [Fact]
        public async Task Index_WithNullProgramNo_SelectsFirstProgramme()
        {
            // Arrange
            _programService.GetAllProgramsAsync()
                .Returns(MakeProgramResponse(("P001", "Programme One"), ("P002", "Programme Two")));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityViewModel>(viewResult.Model);
            Assert.Equal("P001", model.SelectedProgramNo);
        }

        [Fact]
        public async Task Index_WithInvalidProgramNo_FallsBackToFirstProgramme()
        {
            // Arrange
            _programService.GetAllProgramsAsync()
                .Returns(MakeProgramResponse(("P001", "Programme One")));

            // Act
            var result = await _controller.Index("INVALID");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityViewModel>(viewResult.Model);
            Assert.Equal("P001", model.SelectedProgramNo);
        }

        [Fact]
        public async Task Index_WhenNoProgrammesExist_ReturnsEmptySelectedProgramNo()
        {
            // Arrange
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.SuccessResponse(
                    Enumerable.Empty<ProgramDto>()));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.SelectedProgramNo);
            Assert.Empty(model.ProgrammeList);
        }

        [Fact]
        public async Task Index_ProgrammeListIsOrderedByProgramNo()
        {
            // Arrange
            _programService.GetAllProgramsAsync()
                .Returns(MakeProgramResponse(("P003", "C"), ("P001", "A"), ("P002", "B")));

            // Act
            var result = await _controller.Index("P001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityViewModel>(viewResult.Model);
            Assert.Equal("P001", model.ProgrammeList[0].Value);
            Assert.Equal("P002", model.ProgrammeList[1].Value);
            Assert.Equal("P003", model.ProgrammeList[2].Value);
        }

        [Fact]
        public async Task Index_ProgrammeWithNameShowsNumberAndName()
        {
            // Arrange
            _programService.GetAllProgramsAsync()
                .Returns(MakeProgramResponse(("P001", "Animal Health")));

            // Act
            var result = await _controller.Index("P001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityViewModel>(viewResult.Model);
            Assert.Equal("P001 — Animal Health", model.ProgrammeList[0].Text);
        }

        [Fact]
        public async Task Index_WhenProgramServiceFails_ReturnsEmptyProgrammeList()
        {
            // Arrange
            _programService.GetAllProgramsAsync()
                .Returns(ApiResponseDto<IEnumerable<ProgramDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "Service error" } },
                    new ApiMetaDto()));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectProfitabilityViewModel>(viewResult.Model);
            Assert.Empty(model.ProgrammeList);
        }

        #endregion

        // ── LoadProjectProfitabilityGrid ──────────────────────────────────────

        #region LoadProjectProfitabilityGrid

        [Fact]
        public async Task LoadProjectProfitabilityGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = MakeGridRequest();
            var programNo = "P001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var items = new List<ProjectProfitabilityDto> { MakeItem("PP001"), MakeItem("PP002") };
            var apiResponse = MakeProfitabilityResponse(items.ToArray());
            var mappedItems = new List<ProjectProfitabilityItem>
            {
                new() { JobCode = "PP001" },
                new() { JobCode = "PP002" }
            };

            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _projectService.GetProjectProfitabilityAsync(query, programNo, "all").Returns(apiResponse);
            _mapper.Map<List<ProjectProfitabilityItem>>(apiResponse.Data).Returns(mappedItems);
            _mapper.Map<PaginationModel>(apiResponse.Pagination).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProjectProfitabilityGrid(request, programNo, null, "all");

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
        }

        [Fact]
        public async Task LoadProjectProfitabilityGrid_WhenProgramNoIsNull_ReturnsEmptyGrid()
        {
            // Arrange
            var request = MakeGridRequest();

            // Act
            var result = await _controller.LoadProjectProfitabilityGrid(request, null, null, "all");

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectProfitabilityItem>>(partialResult.Model);
            Assert.Empty(gridConfig.Data);
            Assert.Equal("isProjectProfitGrid", gridConfig.GridId);
            Assert.Equal("Project Profitability", gridConfig.Title);
            await _projectService.DidNotReceive()
                .GetProjectProfitabilityAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task LoadProjectProfitabilityGrid_WhenProgramNoIsWhitespace_ReturnsEmptyGrid(string programNo)
        {
            // Arrange
            var request = MakeGridRequest();

            // Act
            var result = await _controller.LoadProjectProfitabilityGrid(request, programNo, null, "all");

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectProfitabilityItem>>(partialResult.Model);
            Assert.Empty(gridConfig.Data);
        }

        [Fact]
        public async Task LoadProjectProfitabilityGrid_WhenProgramNoIsEmpty_EmptyGridPreservesRequestFilters()
        {
            // Arrange
            var request = MakeGridRequest();
            request.SortBy = "JobCode";
            request.Descending = true;
            request.Filter = "{\"ParentProject\":\"PP001\"}";

            // Act
            var result = await _controller.LoadProjectProfitabilityGrid(request, null, null, "all");

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectProfitabilityItem>>(partialResult.Model);
            Assert.Empty(gridConfig.Data);
            Assert.Equal("JobCode", gridConfig.Pagination.SortColumn);
            Assert.True(gridConfig.Pagination.SortDirection);
            Assert.NotNull(gridConfig.CurrentFilters);
            Assert.Equal("PP001", gridConfig.CurrentFilters!["ParentProject"]);
        }

        [Fact]
        public async Task LoadProjectProfitabilityGrid_WhenServiceFails_Returns500()
        {
            // Arrange
            var request = MakeGridRequest();
            var programNo = "P001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            var failResponse = ApiResponseDto<List<ProjectProfitabilityDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _projectService.GetProjectProfitabilityAsync(query, programNo, "all").Returns(failResponse);

            // Act
            var result = await _controller.LoadProjectProfitabilityGrid(request, programNo, null, "all");

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        [Fact]
        public async Task LoadProjectProfitabilityGrid_GridConfigHasCorrectBindUrl()
        {
            // Arrange
            var request = MakeGridRequest();
            var programNo = "P001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = MakeProfitabilityResponse();
            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _projectService.GetProjectProfitabilityAsync(query, programNo, "all").Returns(apiResponse);
            _mapper.Map<List<ProjectProfitabilityItem>>(Arg.Any<List<ProjectProfitabilityDto>>())
                .Returns(new List<ProjectProfitabilityItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProjectProfitabilityGrid(request, programNo, null, "all");

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectProfitabilityItem>>(partialResult.Model);
            Assert.Equal("/FPS/ProjectProfitability/LoadProjectProfitabilityGrid", gridConfig.BindGridUrl);
            Assert.Equal("isProjectProfitGrid", gridConfig.GridId);
        }

        #endregion

        // ── GetProfitabilitySummary ───────────────────────────────────────────

        #region GetProfitabilitySummary

        [Fact]
        public async Task GetProfitabilitySummary_WithValidData_ReturnsCorrectTotals()
        {
            // Arrange
            var programNo = "P001";
            var query = new QueryParameters<string> { Page = 1, PageSize = int.MaxValue };
            var items = new List<ProjectProfitabilityDto>
            {
                new() { JobCode = "PP001", JcTotalStaffCosts = 1000m, JcTotalTestCosts = 200m, JcTotalAnimalCosts = 50m, JcTotalAdditionalCosts = 100m, TotalCosts = 1350m, BudgetCvl = 5000m, JcProfit = 3650m, TargetProfit = 3000m, OffTarget = 650m, ProgrammeTarget = 10000m },
                new() { JobCode = "PP002", JcTotalStaffCosts = 2000m, JcTotalTestCosts = 400m, JcTotalAnimalCosts = 80m, JcTotalAdditionalCosts = 200m, TotalCosts = 2680m, BudgetCvl = 6000m, JcProfit = 3320m, TargetProfit = 3000m, OffTarget = 320m, ProgrammeTarget = 10000m }
            };
            var apiResponse = MakeProfitabilityResponse(items.ToArray());

            _projectService.GetProjectProfitabilityAsync(
                Arg.Is<QueryParameters<string>>(q => q.Page == 1 && q.PageSize == int.MaxValue),
                programNo, "all")
                .Returns(apiResponse);

            // Act
            var result = await _controller.GetProfitabilitySummary(programNo, null, "all");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var summary = GetSummaryJson(okResult);
            Assert.Equal(10000m, summary.GetProperty("programmeTarget").GetDecimal());
        }

        [Fact]
        public async Task GetProfitabilitySummary_WhenProgramNoIsNull_ReturnsOkWithNullTarget()
        {
            // Act
            var result = await _controller.GetProfitabilitySummary(null!, null, "all");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            await _projectService.DidNotReceive().GetProjectProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetProfitabilitySummary_WhenProgramNoIsWhitespace_ReturnsOkWithNullTarget(string programNo)
        {
            // Act
            var result = await _controller.GetProfitabilitySummary(programNo, null, "all");

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _projectService.DidNotReceive().GetProjectProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetProfitabilitySummary_WhenServiceFails_Returns500()
        {
            // Arrange
            var programNo = "P001";
            var errors = new List<ApiErrorDto> { new() { Message = "Error" } };
            var failResponse = ApiResponseDto<List<ProjectProfitabilityDto>>.FailureResponse(errors, new ApiMetaDto());

            _projectService.GetProjectProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), programNo, "all")
                .Returns(failResponse);

            // Act
            var result = await _controller.GetProfitabilitySummary(programNo, null, "all");

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        [Fact]
        public async Task GetProfitabilitySummary_WithNoItems_ReturnsSurplusShortfallOfZero()
        {
            // Arrange
            var programNo = "P001";
            var emptyResponse = ApiResponseDto<List<ProjectProfitabilityDto>>.SuccessResponse(
                new List<ProjectProfitabilityDto>(), new PaginationDto());

            _projectService.GetProjectProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), programNo, "all")
                .Returns(emptyResponse);

            // Act
            var result = await _controller.GetProfitabilitySummary(programNo, null, "all");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            var summary = GetSummaryJson(okResult);
            Assert.Equal(0m, summary.GetProperty("programmeSurplusShortfall").GetDecimal());
        }

        [Fact]
        public async Task GetProfitabilitySummary_OffTargetSumIsCorrect()
        {
            // Arrange
            var programNo = "P001";
            var items = new List<ProjectProfitabilityDto>
            {
                new() { JcProfit = 3000m, TargetProfit = 2500m, OffTarget = 500m,  BudgetCvl = 5000m, ProgrammeTarget = 8000m },
                new() { JcProfit = 2000m, TargetProfit = 2500m, OffTarget = -500m, BudgetCvl = 4000m, ProgrammeTarget = 8000m }
            };
            var apiResponse = MakeProfitabilityResponse(items.ToArray());
            _projectService.GetProjectProfitabilityAsync(
                Arg.Any<QueryParameters<string>>(), programNo, "all")
                .Returns(apiResponse);

            // Act
            var result = await _controller.GetProfitabilitySummary(programNo, null, "all");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var summary = GetSummaryJson(okResult);
            Assert.Equal(0m, summary.GetProperty("totalOffTarget").GetDecimal());   // 500 + (-500) = 0
            Assert.Equal(5000m, summary.GetProperty("totalProfit").GetDecimal());   // 3000 + 2000
        }

        #endregion
    }
}
