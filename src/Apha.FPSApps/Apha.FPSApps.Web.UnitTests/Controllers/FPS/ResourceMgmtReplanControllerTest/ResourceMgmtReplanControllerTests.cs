using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ResourceMgmtReplanControllerTest
{
    public class ResourceMgmtReplanControllerTests
    {
        private const string DefaultResourceCentre = "RC01";
        private const string DefaultWorkGroup = "WorkGroupA";
        private const string DefaultJobCode = "JOB001";
        private const string DefaultWgGrade = "WG01";

        private readonly IMapper _mapper;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IProjectService _projectService;
        private readonly IPlanStaffZTCodeService _planStaffZTCodeService;
        private readonly ResourceMgmtReplanController _controller;

        public ResourceMgmtReplanControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _profitCentreService = Substitute.For<IProfitCentreService>();
            _workGroupService = Substitute.For<IWorkGroupService>();
            _projectService = Substitute.For<IProjectService>();
            _planStaffZTCodeService = Substitute.For<IPlanStaffZTCodeService>();

            _controller = new ResourceMgmtReplanController(
                _mapper,
                _profitCentreService,
                _workGroupService,
                _projectService,
                _planStaffZTCodeService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private static List<ProfitCentreDto> BuildProfitCentreList() =>
        [
            new() { ProfitCentreId = "RC01", ProfitCentreName = "Resource Centre One" },
            new() { ProfitCentreId = "RC02", ProfitCentreName = "Resource Centre Two" }
        ];

        private static List<WorkGroupViewDto> BuildWorkGroupList() =>
        [
            new() { WorkGroupName = "WorkGroupA" },
            new() { WorkGroupName = "WorkGroupB" },
            new() { WorkGroupName = "WorkGroupA" }  // duplicate to test Distinct
        ];

        private static List<ProjectStaffReplanDto> BuildReplanDtoList() =>
        [
            new() { WorkGroup = "WorkGroupA", WgGrade = "WG01", PlannedHours = 10.0 },
            new() { WorkGroup = "WorkGroupA", WgGrade = "WG01", PlannedHours = 8.0 }
        ];

        private static List<StaffJobViewDto> BuildStaffJobDtoList() =>
        [
            new() { StaffID = "S001", JobCode = "JOB001", PlannedHours = 20 },
            new() { StaffID = "S002", JobCode = "JOB001", PlannedHours = 15 }
        ];

        // ── Constructor Tests ─────────────────────────────────────────────────

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ResourceMgmtReplanController(
                null!, _profitCentreService, _workGroupService, _projectService, _planStaffZTCodeService));
        }

        [Fact]
        public void Constructor_WithNullProfitCentreService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ResourceMgmtReplanController(
                _mapper, null!, _workGroupService, _projectService, _planStaffZTCodeService));
        }

        [Fact]
        public void Constructor_WithNullWorkGroupService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ResourceMgmtReplanController(
                _mapper, _profitCentreService, null!, _projectService, _planStaffZTCodeService));
        }

        [Fact]
        public void Constructor_WithNullProjectService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ResourceMgmtReplanController(
                _mapper, _profitCentreService, _workGroupService, null!, _planStaffZTCodeService));
        }

        [Fact]
        public void Constructor_WithNullPlanStaffZTCodeService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ResourceMgmtReplanController(
                _mapper, _profitCentreService, _workGroupService, _projectService, null!));
        }

        #endregion

        // ── Index Tests ───────────────────────────────────────────────────────

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewWithViewModel()
        {
            // Arrange
            var profitCentres = BuildProfitCentreList();
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(profitCentres));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ResourceMgmtReplanViewModel>(viewResult.Model);
            Assert.Equal(2, model.ResourceCentres.Count);
            Assert.NotNull(model.RePlanGrid);
            Assert.NotNull(model.AllTimeGrid);
            Assert.Empty(model.RePlanGrid.Data);
            Assert.Empty(model.AllTimeGrid.Data);
        }

        [Fact]
        public async Task Index_WhenProfitCentreServiceFails_ReturnsViewWithEmptyResourceCentres()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Service error", Code = "ERR" } };
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ResourceMgmtReplanViewModel>(viewResult.Model);
            Assert.Empty(model.ResourceCentres);
        }

        #endregion

        // ── GetWorkGroups Tests ───────────────────────────────────────────────

        #region GetWorkGroups Tests

        [Fact]
        public async Task GetWorkGroups_WithValidResourceCentre_ReturnsDistinctSortedWorkGroups()
        {
            // Arrange
            var workGroups = BuildWorkGroupList();
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync(DefaultResourceCentre)
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(workGroups));

            // Act
            var result = await _controller.GetWorkGroups(DefaultResourceCentre);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);

            Assert.True(element.GetProperty("success").GetBoolean());
            var data = element.GetProperty("data");
            Assert.Equal(JsonValueKind.Array, data.ValueKind);
            Assert.Equal(2, data.GetArrayLength()); // duplicates removed
        }

        [Fact]
        public async Task GetWorkGroups_WithNullResourceCentre_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.GetWorkGroups(null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetWorkGroups_WithEmptyResourceCentre_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.GetWorkGroups(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetWorkGroups_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Service error", Code = "ERR" } };
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync(DefaultResourceCentre)
                .Returns(ApiResponseDto<List<WorkGroupViewDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetWorkGroups(DefaultResourceCentre);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── LoadRePlanGrid Tests ──────────────────────────────────────────────

        #region LoadRePlanGrid Tests

        [Fact]
        public async Task LoadRePlanGrid_WithNullWorkGroup_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadRePlanGrid(request, null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        [Fact]
        public async Task LoadRePlanGrid_WithEmptyWorkGroup_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadRePlanGrid(request, string.Empty);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        [Fact]
        public async Task LoadRePlanGrid_WithValidWorkGroup_ReturnsPartialViewWithGridData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = BuildReplanDtoList();

            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _projectService.GetProjectGroupStaffReplanAsync(query, DefaultWorkGroup)
                .Returns(ApiResponseDto<List<ProjectStaffReplanDto>>.SuccessResponse(dtos));
            _mapper.Map<ResourceMgmtReplanGridItem>(Arg.Any<ProjectStaffReplanDto>())
                .Returns(new ResourceMgmtReplanGridItem());
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadRePlanGrid(request, DefaultWorkGroup);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        [Fact]
        public async Task LoadRePlanGrid_WhenServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Load failed", Code = "ERR" } };

            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _projectService.GetProjectGroupStaffReplanAsync(query, DefaultWorkGroup)
                .Returns(ApiResponseDto<List<ProjectStaffReplanDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.LoadRePlanGrid(request, DefaultWorkGroup);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── LoadAllTimeGrid Tests ─────────────────────────────────────────────

        #region LoadAllTimeGrid Tests

        [Fact]
        public async Task LoadAllTimeGrid_WithNullJobCode_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadAllTimeGrid(request, null, DefaultWgGrade);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        [Fact]
        public async Task LoadAllTimeGrid_WithNullWgGrade_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadAllTimeGrid(request, DefaultJobCode, null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        [Fact]
        public async Task LoadAllTimeGrid_WithBothNullParams_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadAllTimeGrid(request, null, null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        [Fact]
        public async Task LoadAllTimeGrid_WithValidParams_ReturnsPartialViewWithGridData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = BuildStaffJobDtoList();

            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _planStaffZTCodeService.GetStaffJobsAllocationByJobCodeWgGradePagedAsync(query, DefaultJobCode, DefaultWgGrade)
                .Returns(ApiResponseDto<List<StaffJobViewDto>>.SuccessResponse(dtos));
            _mapper.Map<ResourceMgmtReplanAllTimeItem>(Arg.Any<StaffJobViewDto>())
                .Returns(new ResourceMgmtReplanAllTimeItem());
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadAllTimeGrid(request, DefaultJobCode, DefaultWgGrade);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        [Fact]
        public async Task LoadAllTimeGrid_WhenServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Load failed", Code = "ERR" } };

            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _planStaffZTCodeService.GetStaffJobsAllocationByJobCodeWgGradePagedAsync(query, DefaultJobCode, DefaultWgGrade)
                .Returns(ApiResponseDto<List<StaffJobViewDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.LoadAllTimeGrid(request, DefaultJobCode, DefaultWgGrade);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion
    }
}
