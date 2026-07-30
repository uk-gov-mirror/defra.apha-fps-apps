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
using Microsoft.AspNetCore.Mvc.Routing;
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.StaffResourceControllerTest
{
    public class StaffResourceControllerTests
    {
        private const string DefaultProfitCentre    = "PC01";
        private const string DefaultWorkgroup        = "WG01";
        private const string WorkgroupGridUrl        = "/FPS/StaffResource/LoadWorkgroupGrid";
        private const string StaffGridUrl            = "/FPS/StaffResource/LoadStaffGrid";

        private readonly IMapper                _mapper;
        private readonly IProfitCentreService   _profitCentreService;
        private readonly IWorkGroupService       _workGroupService;
        private readonly IStaffJobService        _staffJobService;
        private readonly StaffResourceController _controller;

        public StaffResourceControllerTests()
        {
            _mapper              = Substitute.For<IMapper>();
            _profitCentreService = Substitute.For<IProfitCentreService>();
            _workGroupService    = Substitute.For<IWorkGroupService>();
            _staffJobService     = Substitute.For<IStaffJobService>();

            _controller = new StaffResourceController(
                _mapper,
                _profitCentreService,
                _workGroupService,
                _staffJobService);

            var urlHelper = Substitute.For<IUrlHelper>();
            urlHelper.Action(Arg.Is<UrlActionContext>(ctx => ctx.Action == nameof(StaffResourceController.LoadWorkgroupGrid)))
                     .Returns(WorkgroupGridUrl);
            urlHelper.Action(Arg.Is<UrlActionContext>(ctx => ctx.Action == nameof(StaffResourceController.LoadStaffGrid)))
                     .Returns(StaffGridUrl);
            _controller.Url = urlHelper;
        }

        // ─── Helpers ────────────────────────────────────────────────────────────────

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private static List<ProfitCentreDto> BuildProfitCentreList() =>
        [
            new() { ProfitCentreId = "PC01" },
            new() { ProfitCentreId = "PC02" }
        ];

        private static List<WorkGroupDto> BuildWorkgroupList() =>
        [
            new() { WorkGroupName = "WG01", ProfitCentre = DefaultProfitCentre },
            new() { WorkGroupName = "WG02", ProfitCentre = DefaultProfitCentre }
        ];

        private static List<StaffResourceUtilisationDto> BuildUtilisationList() =>
        [
            new() { WorkGroup = DefaultWorkgroup, Name = "John Doe",   WgGrade = "GR1", HrsAvail = 37.5, ApprovedSoct = 20.0, NotApprovedSoct = 5.0, Left = 12.5, PlannedZt = 0, AvailSoct = 37.5, ApprovedUtilPct = 53.33, NotApprovedUtilPct = 13.33, TotalUtilPct = 66.67 },
            new() { WorkGroup = DefaultWorkgroup, Name = "Jane Smith", WgGrade = "GR2", HrsAvail = 30.0, ApprovedSoct = 15.0, NotApprovedSoct = 3.0, Left = 12.0, PlannedZt = 0, AvailSoct = 30.0, ApprovedUtilPct = 50.00, NotApprovedUtilPct = 10.00, TotalUtilPct = 60.00 }
        ];

        private void SetupProfitCentreSuccess(List<ProfitCentreDto>? list = null)
        {
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(list ?? BuildProfitCentreList()));
        }

        private void SetupWorkgroupSuccess(string profitCentre, List<WorkGroupDto>? list = null)
        {
            _workGroupService.GetWorkGroupsByProfitCentreAsync(
                    Arg.Any<QueryParameters<string>>(), profitCentre)
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(list ?? BuildWorkgroupList()));
        }

        private void SetupUtilisationSuccess(string workgroup, List<StaffResourceUtilisationDto>? list = null)
        {
            _staffJobService.GetStaffResourceUtilisationAsync(
                    Arg.Any<QueryParameters<string>>(), workgroup)
                .Returns(ApiResponseDto<List<StaffResourceUtilisationDto>>.SuccessResponse(list ?? BuildUtilisationList()));
        }

        // ─── Index ──────────────────────────────────────────────────────────────────

        #region Index

        [Fact]
        public async Task Index_WithNoProfitCentre_ReturnsViewWithAutoSelectedFirst()
        {
            // Arrange
            SetupProfitCentreSuccess();
            SetupWorkgroupSuccess("PC01");
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<StaffResourceViewModel>(viewResult.Model);

            Assert.Equal("PC01", model.SelectedProfitCentre);
            Assert.Equal(2, model.ProfitCentreList.Count);
            Assert.NotNull(model.WorkgroupGrid);
            Assert.NotNull(model.StaffGrid);
        }

        [Fact]
        public async Task Index_WithExplicitProfitCentre_DoesNotAutoSelect()
        {
            // Arrange
            SetupProfitCentreSuccess();
            SetupWorkgroupSuccess(DefaultProfitCentre);
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.Index(DefaultProfitCentre);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<StaffResourceViewModel>(viewResult.Model);

            Assert.Equal(DefaultProfitCentre, model.SelectedProfitCentre);
        }

        [Fact]
        public async Task Index_WhenProfitCentreServiceFails_ProfitCentreListIsEmpty()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<StaffResourceViewModel>(viewResult.Model);

            Assert.Empty(model.ProfitCentreList);
        }

        [Fact]
        public async Task Index_SelectedProfitCentreItem_IsMarkedSelected()
        {
            // Arrange
            SetupProfitCentreSuccess();
            SetupWorkgroupSuccess(DefaultProfitCentre);
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.Index(DefaultProfitCentre);

            // Assert
            var model       = Assert.IsType<StaffResourceViewModel>(Assert.IsType<ViewResult>(result).Model);
            var selectedItem = model.ProfitCentreList.Single(i => i.Value == DefaultProfitCentre);

            Assert.True(selectedItem.Selected);
        }

        [Fact]
        public async Task Index_WorkgroupGridConfig_HasCorrectGridId()
        {
            // Arrange
            SetupProfitCentreSuccess();
            SetupWorkgroupSuccess(DefaultProfitCentre);
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.Index(DefaultProfitCentre);

            // Assert
            var model = Assert.IsType<StaffResourceViewModel>(Assert.IsType<ViewResult>(result).Model);

            Assert.Equal("ruvWorkgroupGrid", model.WorkgroupGrid.GridId);
            Assert.Equal("ruvStaffGrid",     model.StaffGrid.GridId);
        }

        [Fact]
        public async Task Index_GridConfigs_AreReadOnly()
        {
            // Arrange
            SetupProfitCentreSuccess();
            SetupWorkgroupSuccess(DefaultProfitCentre);
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.Index(DefaultProfitCentre);

            // Assert
            var model = Assert.IsType<StaffResourceViewModel>(Assert.IsType<ViewResult>(result).Model);

            Assert.False(model.WorkgroupGrid.AllowAdd);
            Assert.False(model.WorkgroupGrid.AllowEdit);
            Assert.False(model.WorkgroupGrid.AllowDelete);
            Assert.False(model.StaffGrid.AllowAdd);
            Assert.False(model.StaffGrid.AllowEdit);
            Assert.False(model.StaffGrid.AllowDelete);
        }

        [Fact]
        public async Task Index_GridConfigs_HaveCorrectBindUrls()
        {
            // Arrange
            SetupProfitCentreSuccess();
            SetupWorkgroupSuccess(DefaultProfitCentre);
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.Index(DefaultProfitCentre);

            // Assert
            var model = Assert.IsType<StaffResourceViewModel>(Assert.IsType<ViewResult>(result).Model);

            Assert.Equal("/FPS/StaffResource/LoadWorkgroupGrid", model.WorkgroupGrid.BindGridUrl);
            Assert.Equal("/FPS/StaffResource/LoadStaffGrid",     model.StaffGrid.BindGridUrl);
        }

        [Fact]
        public async Task Index_WorkgroupGrid_PopulatedWithWorkgroupItems()
        {
            // Arrange
            SetupProfitCentreSuccess();
            SetupWorkgroupSuccess(DefaultProfitCentre);
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.Index(DefaultProfitCentre);

            // Assert
            var model = Assert.IsType<StaffResourceViewModel>(Assert.IsType<ViewResult>(result).Model);

            Assert.Equal(2, model.WorkgroupGrid.Data.Count);
            Assert.Equal("WG01", model.WorkgroupGrid.Data[0].WorkGroupName);
            Assert.Equal("WG02", model.WorkgroupGrid.Data[1].WorkGroupName);
        }

        [Fact]
        public async Task Index_StaffGridData_IsEmptyOnInitialLoad()
        {
            // Arrange
            SetupProfitCentreSuccess();
            SetupWorkgroupSuccess(DefaultProfitCentre);
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.Index(DefaultProfitCentre);

            // Assert
            var model = Assert.IsType<StaffResourceViewModel>(Assert.IsType<ViewResult>(result).Model);

            Assert.Empty(model.StaffGrid.Data);
        }

        [Fact]
        public async Task Index_WhenWorkgroupServiceFails_WorkgroupGridDataIsEmpty()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            SetupProfitCentreSuccess();
            _workGroupService.GetWorkGroupsByProfitCentreAsync(Arg.Any<QueryParameters<string>>(), DefaultProfitCentre)
                .Returns(ApiResponseDto<List<WorkGroupDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Index(DefaultProfitCentre);

            // Assert
            var model = Assert.IsType<StaffResourceViewModel>(Assert.IsType<ViewResult>(result).Model);

            Assert.Empty(model.WorkgroupGrid.Data);
        }

        #endregion

        // ─── LoadWorkgroupGrid ───────────────────────────────────────────────────────

        #region LoadWorkgroupGrid

        [Fact]
        public async Task LoadWorkgroupGrid_WithValidProfitCentre_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            SetupWorkgroupSuccess(DefaultProfitCentre);
            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadWorkgroupGrid(request, DefaultProfitCentre);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _workGroupService.Received(1)
                .GetWorkGroupsByProfitCentreAsync(Arg.Any<QueryParameters<string>>(), DefaultProfitCentre);
        }

        [Fact]
        public async Task LoadWorkgroupGrid_WithNullProfitCentre_ReturnsEmptyGridPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());

            // Act
            var result = await _controller.LoadWorkgroupGrid(request, null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _workGroupService.DidNotReceive()
                .GetWorkGroupsByProfitCentreAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadWorkgroupGrid_WithInvalidModelState_ReturnsFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("key", "error");

            // Act
            var result = await _controller.LoadWorkgroupGrid(new PaginationFilter<string>(), DefaultProfitCentre);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);

            Assert.False(value.GetProperty("success").GetBoolean());
            await _workGroupService.DidNotReceive()
                .GetWorkGroupsByProfitCentreAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadWorkgroupGrid_WhenServiceFails_ReturnsEmptyGridPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            var errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());
            _workGroupService.GetWorkGroupsByProfitCentreAsync(Arg.Any<QueryParameters<string>>(), DefaultProfitCentre)
                .Returns(ApiResponseDto<List<WorkGroupDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.LoadWorkgroupGrid(request, DefaultProfitCentre);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig    = Assert.IsType<DataGridConfig<StaffResourceWorkgroupItem>>(partialResult.Model);

            Assert.Empty(gridConfig.Data);
        }

        [Fact]
        public async Task LoadWorkgroupGrid_WithValidData_GridDataMapsWorkgroupNames()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            SetupWorkgroupSuccess(DefaultProfitCentre);
            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadWorkgroupGrid(request, DefaultProfitCentre);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig    = Assert.IsType<DataGridConfig<StaffResourceWorkgroupItem>>(partialResult.Model);

            Assert.Equal(2, gridConfig.Data.Count);
            Assert.Equal("WG01", gridConfig.Data[0].WorkGroupName);
            Assert.Equal("WG02", gridConfig.Data[1].WorkGroupName);
        }

        #endregion

        // ─── LoadStaffGrid ───────────────────────────────────────────────────────────

        #region LoadStaffGrid

        [Fact]
        public async Task LoadStaffGrid_WithValidWorkgroup_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            SetupUtilisationSuccess(DefaultWorkgroup);
            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadStaffGrid(request, DefaultWorkgroup);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _staffJobService.Received(1)
                .GetStaffResourceUtilisationAsync(Arg.Any<QueryParameters<string>>(), DefaultWorkgroup);
        }

        [Fact]
        public async Task LoadStaffGrid_WithNullWorkgroup_ReturnsEmptyGridPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());

            // Act
            var result = await _controller.LoadStaffGrid(request, null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _staffJobService.DidNotReceive()
                .GetStaffResourceUtilisationAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadStaffGrid_WithInvalidModelState_ReturnsFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("key", "error");

            // Act
            var result = await _controller.LoadStaffGrid(new PaginationFilter<string>(), DefaultWorkgroup);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultElement(jsonResult);

            Assert.False(value.GetProperty("success").GetBoolean());
            await _staffJobService.DidNotReceive()
                .GetStaffResourceUtilisationAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadStaffGrid_WhenServiceFails_ReturnsEmptyGridPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            var errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());
            _staffJobService.GetStaffResourceUtilisationAsync(Arg.Any<QueryParameters<string>>(), DefaultWorkgroup)
                .Returns(ApiResponseDto<List<StaffResourceUtilisationDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.LoadStaffGrid(request, DefaultWorkgroup);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig    = Assert.IsType<DataGridConfig<StaffResourceStaffItem>>(partialResult.Model);

            Assert.Empty(gridConfig.Data);
        }

        [Fact]
        public async Task LoadStaffGrid_WithValidData_GridDataMapsFieldsCorrectly()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            SetupUtilisationSuccess(DefaultWorkgroup);
            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadStaffGrid(request, DefaultWorkgroup);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig    = Assert.IsType<DataGridConfig<StaffResourceStaffItem>>(partialResult.Model);
            var firstRow      = gridConfig.Data[0];

            Assert.Equal(2,           gridConfig.Data.Count);
            Assert.Equal("GR1",       firstRow.WgGrade);
            Assert.Equal("John Doe",  firstRow.Name);
            Assert.Equal(37.5,        firstRow.TotalH);
            Assert.Equal(20.0,        firstRow.ApprovedPlan);
            Assert.Equal(5.0,         firstRow.NotApprovedPlan);
            Assert.Equal(25.0,        firstRow.TotalPlan);
            Assert.Equal(53.33,       firstRow.ApprovedUtil);
            Assert.Equal(13.33,       firstRow.NotApprovedUtil);
            Assert.Equal(66.67,       firstRow.TotalUtil);
        }

        [Fact]
        public async Task LoadStaffGrid_WithValidData_TotalPlanIsSumOfApprovedAndNotApproved()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            var items   = new List<StaffResourceUtilisationDto>
            {
                new() { WorkGroup = DefaultWorkgroup, ApprovedSoct = 18.0, NotApprovedSoct = 7.0 }
            };
            SetupUtilisationSuccess(DefaultWorkgroup, items);
            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadStaffGrid(request, DefaultWorkgroup);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig    = Assert.IsType<DataGridConfig<StaffResourceStaffItem>>(partialResult.Model);

            Assert.Equal(25.0, gridConfig.Data[0].TotalPlan);
        }

        [Fact]
        public async Task LoadStaffGrid_GridConfig_HasCorrectGridId()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            SetupUtilisationSuccess(DefaultWorkgroup);
            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadStaffGrid(request, DefaultWorkgroup);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig    = Assert.IsType<DataGridConfig<StaffResourceStaffItem>>(partialResult.Model);

            Assert.Equal("ruvStaffGrid", gridConfig.GridId);
        }

        [Theory]
        [InlineData("WG01", 2)]
        [InlineData("WG02", 0)]
        public async Task LoadStaffGrid_WithDifferentWorkgroups_ReturnsExpectedCount(string workgroup, int expectedCount)
        {
            // Arrange
            var request = new PaginationFilter<string>();
            var data    = workgroup == "WG01" ? BuildUtilisationList() : new List<StaffResourceUtilisationDto>();
            var errors  = new List<ApiErrorDto> { new() { Message = "none", Code = "NONE" } };

            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());
            _staffJobService.GetStaffResourceUtilisationAsync(Arg.Any<QueryParameters<string>>(), workgroup)
                .Returns(ApiResponseDto<List<StaffResourceUtilisationDto>>.SuccessResponse(data));

            // Act
            var result = await _controller.LoadStaffGrid(request, workgroup);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig    = Assert.IsType<DataGridConfig<StaffResourceStaffItem>>(partialResult.Model);

            Assert.Equal(expectedCount, gridConfig.Data.Count);
        }

        #endregion
    }
}
