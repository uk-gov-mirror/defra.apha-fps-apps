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
using System.Text.Json;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PIMS.Controllers.MaintenanceControllerTest
{
    public class MaintenanceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IMaintenanceService _service;
        private readonly MaintenanceController _controller;

        public MaintenanceControllerTests()
        {
            _mapper     = Substitute.For<IMapper>();
            _service    = Substitute.For<IMaintenanceService>();
            _controller = new MaintenanceController(_mapper, _service);
        }

        // ── helpers ───────────────────────────────────────────────────────────────

        private static JsonElement GetJsonElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private static ApiResponseDto<T> SuccessResponse<T>(T data) =>
            ApiResponseDto<T>.SuccessResponse(data);

        private static ApiResponseDto<T> FailureResponse<T>() =>
            ApiResponseDto<T>.FailureResponse(
                new List<ApiErrorDto> { new ApiErrorDto { Code = "ERR", Message = "Error" } },
                new ApiMetaDto());

        private static PaginatedResult<T> EmptyPaged<T>() =>
            new(new List<T>(), 0, 1, 10);

        private static PaginatedResult<T> Paged<T>(List<T> items) =>
            new(items, items.Count, 1, 10);

        private void SetupIndexMocks()
        {
            _service.GetPagedReportsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(EmptyPaged<ReportDto>()));
            _service.GetAllSettingsAsync()
                .Returns(SuccessResponse(new List<SettingDto>()));

            _mapper.Map<List<ReportItem>>(Arg.Any<List<ReportDto>>()).Returns(new List<ReportItem>());
        }

        // ════════════════════════════════════════════════════════════════════════════
        //  INDEX
        // ════════════════════════════════════════════════════════════════════════════

        #region Index

        [Fact]
        public async Task Index_ServiceReturnsData_ReturnsViewResult()
        {
            // Arrange
            SetupIndexMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_ServiceReturnsData_ViewModelIsMaintenanceViewModel()
        {
            // Arrange
            SetupIndexMocks();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<MaintenanceViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_CallsGetPagedReportsAsync()
        {
            // Arrange
            SetupIndexMocks();

            // Act
            await _controller.Index();

            // Assert
            await _service.Received(1).GetPagedReportsAsync(Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task Index_CallsGetAllSettingsAsync()
        {
            // Arrange
            SetupIndexMocks();

            // Act
            await _controller.Index();

            // Assert
            await _service.Received(1).GetAllSettingsAsync();
        }

        [Fact]
        public async Task Index_CallsGetAllSettingsAsync_Once()
        {
            // Arrange
            SetupIndexMocks();

            // Act
            await _controller.Index();

            // Assert
            await _service.Received(1).GetAllSettingsAsync();
        }

        [Fact]
        public async Task Index_SettingsReturnWorkingHours_PopulatesWorkingHoursSettingItem()
        {
            // Arrange
            SetupIndexMocks();
            var settingDto = new SettingDto { Id = "HoursInDay", SettingValue = "7.4" };
            _service.GetAllSettingsAsync()
                .Returns(SuccessResponse(new List<SettingDto> { settingDto }));
            var settingItem = new SettingItem { Id = "HoursInDay", SettingValue = "7.4" };
            _mapper.Map<SettingItem>(settingDto).Returns(settingItem);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<MaintenanceViewModel>(viewResult.Model);
            Assert.NotNull(vm.WorkingHoursSettingItem);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  REPORTS TAB — LoadReportsGrid
        // ════════════════════════════════════════════════════════════════════════════

        #region LoadReportsGrid

        [Fact]
        public async Task LoadReportsGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            _service.GetPagedReportsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(EmptyPaged<ReportDto>()));
            _mapper.Map<List<ReportItem>>(Arg.Any<List<ReportDto>>()).Returns(new List<ReportItem>());

            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadReportsGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadReportsGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Filter", "Required");
            var request = new PaginationFilter<string>();

            // Act
            var result = await _controller.LoadReportsGrid(request);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadReportsGrid_ServiceReturnsEmptyData_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            _service.GetPagedReportsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(EmptyPaged<ReportDto>()));
            _mapper.Map<List<ReportItem>>(Arg.Any<List<ReportDto>>()).Returns(new List<ReportItem>());

            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadReportsGrid(request);

            // Assert
            var partial    = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ReportItem>>(partial.Model);
            Assert.Empty(gridConfig.Data!);
        }

        [Fact]
        public async Task LoadReportsGrid_ServiceReturnsFailure_ReturnsPartialViewWithEmptyGrid()
        {
            // Arrange
            _service.GetPagedReportsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(FailureResponse<PaginatedResult<ReportDto>>());
            _mapper.Map<List<ReportItem>>(Arg.Any<List<ReportDto>>()).Returns(new List<ReportItem>());

            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadReportsGrid(request);

            // Assert
            var partial    = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ReportItem>>(partial.Model);
            Assert.Empty(gridConfig.Data!);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  REPORTS TAB — GetAddEditReportPartial
        // ════════════════════════════════════════════════════════════════════════════

        #region GetAddEditReportPartial

        [Fact]
        public async Task GetAddEditReportPartial_NoId_ReturnsPartialViewWithEmptyModel()
        {
            // Act
            var result = await _controller.GetAddEditReportPartial(null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditReport", partial.ViewName);
            Assert.IsType<ReportItem>(partial.Model);
        }

        [Fact]
        public async Task GetAddEditReportPartial_ValidId_ServiceReturnsData_ReturnsPopulatedModel()
        {
            // Arrange
            var dto  = new ReportDto { Id = 7, ReportName = "TestReport", Type = "R" };
            var item = new ReportItem { Id = 7, ReportName = "TestReport" };
            _service.GetReportByIdAsync(7).Returns(SuccessResponse(dto));
            _mapper.Map<ReportItem>(dto).Returns(item);

            // Act
            var result = await _controller.GetAddEditReportPartial(7);

            // Assert
            var partial    = Assert.IsType<PartialViewResult>(result);
            var model      = Assert.IsType<ReportItem>(partial.Model);
            Assert.Equal(7, model.Id);
        }

        [Fact]
        public async Task GetAddEditReportPartial_ValidId_ServiceReturnsFailure_ReturnsEmptyModel()
        {
            // Arrange
            _service.GetReportByIdAsync(99).Returns(FailureResponse<ReportDto>());

            // Act
            var result = await _controller.GetAddEditReportPartial(99);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model   = Assert.IsType<ReportItem>(partial.Model);
            Assert.Equal(0, model.Id);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  REPORTS TAB — SaveReport
        // ════════════════════════════════════════════════════════════════════════════

        #region SaveReport

        [Fact]
        public async Task SaveReport_NewReport_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item = new ReportItem { Id = 0, ReportName = "New Report" };
            var dto  = new ReportDto { Id = 0, ReportName = "New Report", Type = "R" };
            _mapper.Map<ReportDto>(item).Returns(dto);
            _service.CreateReportAsync(dto).Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.SaveReport(item);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveReport_ExistingReport_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item = new ReportItem { Id = 5, ReportName = "Existing Report" };
            var dto  = new ReportDto { Id = 5, ReportName = "Existing Report", Type = "R" };
            _mapper.Map<ReportDto>(item).Returns(dto);
            _service.UpdateReportAsync(5, dto).Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.SaveReport(item);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveReport_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var item = new ReportItem { Id = 0, ReportName = "Bad Report" };
            var dto  = new ReportDto { Id = 0, ReportName = "Bad Report", Type = "R" };
            _mapper.Map<ReportDto>(item).Returns(dto);
            _service.CreateReportAsync(dto).Returns(FailureResponse<ReportDto>());

            // Act
            var result = await _controller.SaveReport(item);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveReport_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Reportname", "Required");
            var item = new ReportItem();

            // Act
            var result = await _controller.SaveReport(item);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  REPORTS TAB — DeleteReport
        // ════════════════════════════════════════════════════════════════════════════

        #region DeleteReport

        [Fact]
        public async Task DeleteReport_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            _service.DeleteReportAsync(3).Returns(SuccessResponse(true));

            // Act
            var result = await _controller.DeleteReport(3);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
            await _service.Received(1).DeleteReportAsync(3);
        }

        [Fact]
        public async Task DeleteReport_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _service.DeleteReportAsync(99).Returns(FailureResponse<bool>());

            // Act
            var result = await _controller.DeleteReport(99);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  REPORTS TAB — LoadReportGroupsGrid
        // ════════════════════════════════════════════════════════════════════════════

        #region LoadReportGroupsGrid

        [Fact]
        public async Task LoadReportGroupsGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            _service.GetPagedReportGroupsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int?>())
                .Returns(SuccessResponse(EmptyPaged<ReportGroupDto>()));
            _mapper.Map<List<ReportGroupItem>>(Arg.Any<List<ReportGroupDto>>()).Returns(new List<ReportGroupItem>());
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadReportGroupsGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadReportGroupsGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Filter", "Required");

            // Act
            var result = await _controller.LoadReportGroupsGrid(new PaginationFilter<string>());

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadReportGroupsGrid_ServiceReturnsData_GridContainsItems()
        {
            // Arrange
            var dtos  = new List<ReportGroupDto> { new() { GroupId = 1, Description = "Group A" } };
            var items = new List<ReportGroupItem> { new() { GroupId = 1, Description = "Group A" } };
            _service.GetPagedReportGroupsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int?>())
                .Returns(SuccessResponse(Paged(dtos)));
            _mapper.Map<List<ReportGroupItem>>(Arg.Any<List<ReportGroupDto>>()).Returns(items);
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadReportGroupsGrid(request);

            // Assert
            var partial    = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ReportGroupItem>>(partial.Model);
            Assert.Single(gridConfig.Data!);
        }

        [Fact]
        public async Task LoadReportGroupsGrid_ServiceReturnsFailure_ReturnsEmptyGrid()
        {
            // Arrange
            _service.GetPagedReportGroupsAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int?>())
                .Returns(FailureResponse<PaginatedResult<ReportGroupDto>>());
            _mapper.Map<List<ReportGroupItem>>(Arg.Any<List<ReportGroupDto>>()).Returns(new List<ReportGroupItem>());
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadReportGroupsGrid(request);

            // Assert
            var partial    = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ReportGroupItem>>(partial.Model);
            Assert.Empty(gridConfig.Data!);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  REPORTS TAB — GetAddEditReportGroupPartial
        // ════════════════════════════════════════════════════════════════════════════

        #region GetAddEditReportGroupPartial

        [Fact]
        public async Task GetAddEditReportGroupPartial_NoId_ReturnsPartialViewWithEmptyModel()
        {
            // Arrange
            _service.GetAllReportGroupsAsync().Returns(SuccessResponse(new List<ReportGroupDto>()));

            // Act
            var result = await _controller.GetAddEditReportGroupPartial(null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditReportGroup", partial.ViewName);
            Assert.IsType<ReportGroupViewModel>(partial.Model);
        }

        [Fact]
        public async Task GetAddEditReportGroupPartial_ValidId_ServiceReturnsData_ReturnsPopulatedModel()
        {
            // Arrange
            var dto  = new ReportGroupDto { GroupId = 2, Description = "Grp B" };
            var item = new ReportGroupViewModel { GroupId = 2, Description = "Grp B" };
            _service.GetReportGroupByIdAsync(2).Returns(SuccessResponse(dto));
            _service.GetAllReportGroupsAsync().Returns(SuccessResponse(new List<ReportGroupDto> { dto }));
            _mapper.Map<ReportGroupViewModel>(dto).Returns(item);

            // Act
            var result = await _controller.GetAddEditReportGroupPartial(2);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model   = Assert.IsType<ReportGroupViewModel>(partial.Model);
            Assert.Equal(2, model.GroupId);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  REPORTS TAB — SaveReportGroup
        // ════════════════════════════════════════════════════════════════════════════

        #region SaveReportGroup

        [Fact]
        public async Task SaveReportGroup_NewGroup_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item = new ReportGroupViewModel { GroupId = 0, Reportid = 1, Description = "New Group" };
            var dto  = new ReportGroupLinkDto { GroupId = 0, ReportId = 1 };
            _service.CreateReportGroupLinkAsync(Arg.Is<ReportGroupLinkDto>(x => x.GroupId == dto.GroupId && x.ReportId == dto.ReportId))
                .Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.SaveReportGroup(item);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveReportGroup_ExistingGroup_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item = new ReportGroupViewModel { GroupId = 3, Reportid = 9, Description = "Existing Group" };
            var dto  = new ReportGroupLinkDto { GroupId = 3, ReportId = 9 };
            _service.CreateReportGroupLinkAsync(Arg.Is<ReportGroupLinkDto>(x => x.GroupId == dto.GroupId && x.ReportId == dto.ReportId))
                .Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.SaveReportGroup(item);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveReportGroup_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Description", "Required");
            var item = new ReportGroupViewModel();

            // Act
            var result = await _controller.SaveReportGroup(item);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveReportGroup_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var item = new ReportGroupViewModel { GroupId = 0, Reportid = 1, Description = "Bad Group" };
            _service.CreateReportGroupLinkAsync(Arg.Any<ReportGroupLinkDto>())
                .Returns(FailureResponse<ReportGroupLinkDto>());

            // Act
            var result = await _controller.SaveReportGroup(item);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  REPORTS TAB — DeleteReportGroup
        // ════════════════════════════════════════════════════════════════════════════

        #region DeleteReportGroup

        [Fact]
        public async Task DeleteReportGroup_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            _service.DeleteReportGroupAsync(4).Returns(SuccessResponse(true));

            // Act
            var result = await _controller.DeleteReportGroup(4);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
            await _service.Received(1).DeleteReportGroupAsync(4);
        }

        [Fact]
        public async Task DeleteReportGroup_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _service.DeleteReportGroupAsync(99).Returns(FailureResponse<bool>());

            // Act
            var result = await _controller.DeleteReportGroup(99);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  PROGRAMME TAB — LoadRadTrackProgsGrid
        // ════════════════════════════════════════════════════════════════════════════

        #region LoadRadTrackProgsGrid

        [Fact]
        public async Task LoadRadTrackProgsGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            _service.GetPagedRadTrackProgsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(EmptyPaged<RadTrackProgDto>()));
            _mapper.Map<List<RadTrackProgItem>>(Arg.Any<List<RadTrackProgDto>>()).Returns(new List<RadTrackProgItem>());

            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadRadTrackProgsGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadRadTrackProgsGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Filter", "Required");

            // Act
            var result = await _controller.LoadRadTrackProgsGrid(new PaginationFilter<string>());

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadRadTrackProgsGrid_ServiceReturnsData_GridContainsItems()
        {
            // Arrange
            var dtos  = new List<RadTrackProgDto> { new() { Program = "PROG1" } };
            var items = new List<RadTrackProgItem> { new() { Program = "PROG1" } };
            _service.GetPagedRadTrackProgsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(Paged(dtos)));
            _mapper.Map<List<RadTrackProgItem>>(Arg.Any<List<RadTrackProgDto>>()).Returns(items);
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadRadTrackProgsGrid(request);

            // Assert
            var partial    = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<RadTrackProgItem>>(partial.Model);
            Assert.Single(gridConfig.Data!);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  PROGRAMME TAB — GetAddEditRadTrackProgPartial
        // ════════════════════════════════════════════════════════════════════════════

        #region GetAddEditRadTrackProgPartial

        [Fact]
        public async Task GetAddEditRadTrackProgPartial_NoProgram_ReturnsPartialViewWithEmptyModel()
        {
            // Arrange
            _service.GetRadTrackProgProgramsAsync().Returns(SuccessResponse(new List<string>()));

            // Act
            var result = await _controller.GetAddEditRadTrackProgPartial(null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditRadTrackProg", partial.ViewName);
            Assert.IsType<RadTrackProgItem>(partial.Model);
        }

        [Fact]
        public async Task GetAddEditRadTrackProgPartial_ValidProgram_ServiceReturnsData_ReturnsPopulatedModel()
        {
            // Arrange
            var dto  = new RadTrackProgDto { Program = "PROG2" };
            var item = new RadTrackProgItem { Program = "PROG2" };
            _service.GetRadTrackProgByIdAsync("PROG2").Returns(SuccessResponse(dto));
            _service.GetRadTrackProgProgramsAsync().Returns(SuccessResponse(new List<string> { "PROG2" }));
            _mapper.Map<RadTrackProgItem>(dto).Returns(item);

            // Act
            var result = await _controller.GetAddEditRadTrackProgPartial("PROG2");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model   = Assert.IsType<RadTrackProgItem>(partial.Model);
            Assert.Equal("PROG2", model.Program);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  PROGRAMME TAB — SaveRadTrackProg
        // ════════════════════════════════════════════════════════════════════════════

        #region SaveRadTrackProg

        [Fact]
        public async Task SaveRadTrackProg_ExistingProg_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item = new RadTrackProgItem { Program = "PROG3" };
            var dto  = new RadTrackProgDto { Program = "PROG3" };
            _mapper.Map<RadTrackProgDto>(item).Returns(dto);
            _service.UpdateRadTrackProgAsync("PROG3", dto).Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.SaveRadTrackProg(item, isEditMode: true);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveRadTrackProg_NewProg_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item = new RadTrackProgItem { Program = "NEW_PROG" };
            var dto  = new RadTrackProgDto { Program = "NEW_PROG" };
            _mapper.Map<RadTrackProgDto>(item).Returns(dto);
            _service.CreateRadTrackProgAsync(dto).Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.SaveRadTrackProg(item, isEditMode: false);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveRadTrackProg_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Program", "Required");
            var item = new RadTrackProgItem();

            // Act
            var result = await _controller.SaveRadTrackProg(item);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveRadTrackProg_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var item = new RadTrackProgItem { Program = "PROG_BAD" };
            var dto  = new RadTrackProgDto { Program = "PROG_BAD" };
            _mapper.Map<RadTrackProgDto>(item).Returns(dto);
            _service.UpdateRadTrackProgAsync("PROG_BAD", dto).Returns(FailureResponse<RadTrackProgDto>());

            // Act
            var result = await _controller.SaveRadTrackProg(item, isEditMode: true);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  PROGRAMME TAB — DeleteRadTrackProg
        // ════════════════════════════════════════════════════════════════════════════

        #region DeleteRadTrackProg

        [Fact]
        public async Task DeleteRadTrackProg_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            _service.DeleteRadTrackProgAsync("PROG1").Returns(SuccessResponse(true));

            // Act
            var result = await _controller.DeleteRadTrackProg("PROG1");

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteRadTrackProg_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _service.DeleteRadTrackProgAsync("UNKNOWN").Returns(FailureResponse<bool>());

            // Act
            var result = await _controller.DeleteRadTrackProg("UNKNOWN");

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  MANAGER TAB — LoadProjectManagersGrid
        // ════════════════════════════════════════════════════════════════════════════

        #region LoadProjectManagersGrid

        [Fact]
        public async Task LoadProjectManagersGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            _service.GetPagedProjectManagersAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(EmptyPaged<ProjectManagerDto>()));
            _mapper.Map<List<ProjectManagerItem>>(Arg.Any<List<ProjectManagerDto>>()).Returns(new List<ProjectManagerItem>());

            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadProjectManagersGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadProjectManagersGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Filter", "Required");

            // Act
            var result = await _controller.LoadProjectManagersGrid(new PaginationFilter<string>());

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadProjectManagersGrid_ServiceReturnsData_GridContainsItems()
        {
            // Arrange
            var dtos  = new List<ProjectManagerDto> { new() { Projectmanager = "Smith, J." } };
            var items = new List<ProjectManagerItem> { new() { Projectmanager = "Smith, J." } };
            _service.GetPagedProjectManagersAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(Paged(dtos)));
            _mapper.Map<List<ProjectManagerItem>>(Arg.Any<List<ProjectManagerDto>>()).Returns(items);

            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadProjectManagersGrid(request);

            // Assert
            var partial    = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProjectManagerItem>>(partial.Model);
            Assert.Single(gridConfig.Data!);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  MANAGER TAB — GetAddEditProjectManagerPartial
        // ════════════════════════════════════════════════════════════════════════════

        #region GetAddEditProjectManagerPartial

        [Fact]
        public async Task GetAddEditProjectManagerPartial_NoId_ReturnsPartialViewWithEmptyModel()
        {
            // Arrange
            _service.GetManagerNamesAsync().Returns(SuccessResponse(new List<string>()));

            // Act
            var result = await _controller.GetAddEditProjectManagerPartial(null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditProjectManager", partial.ViewName);
            Assert.IsType<ProjectManagerItem>(partial.Model);
        }

        [Fact]
        public async Task GetAddEditProjectManagerPartial_ValidId_ServiceReturnsData_ReturnsPopulatedModel()
        {
            // Arrange
            var dto  = new ProjectManagerDto { Projectmanager = "Jones, B." };
            var item = new ProjectManagerItem { Projectmanager = "Jones, B." };
            _service.GetProjectManagerByIdAsync("Jones, B.").Returns(SuccessResponse(dto));
            _service.GetManagerNamesAsync().Returns(SuccessResponse(new List<string> { "Jones, B." }));
            _mapper.Map<ProjectManagerItem>(dto).Returns(item);

            // Act
            var result = await _controller.GetAddEditProjectManagerPartial("Jones, B.");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model   = Assert.IsType<ProjectManagerItem>(partial.Model);
            Assert.Equal("Jones, B.", model.Projectmanager);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  MANAGER TAB — SaveProjectManager
        // ════════════════════════════════════════════════════════════════════════════

        #region SaveProjectManager

        [Fact]
        public async Task SaveProjectManager_ExistingManager_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item = new ProjectManagerItem { Projectmanager = "Smith, J." };
            var dto  = new ProjectManagerDto { Projectmanager = "Smith, J." };
            _mapper.Map<ProjectManagerDto>(item).Returns(dto);
            _service.UpdateProjectManagerAsync("Smith, J.", dto).Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.SaveProjectManager(item, isEditMode: true);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveProjectManager_NewManager_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item = new ProjectManagerItem { Projectmanager = "Jones, A." };
            var dto  = new ProjectManagerDto { Projectmanager = "Jones, A." };
            _mapper.Map<ProjectManagerDto>(item).Returns(dto);
            _service.CreateProjectManagerAsync(dto).Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.SaveProjectManager(item, isEditMode: false);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveProjectManager_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Projectmanager", "Required");
            var item = new ProjectManagerItem();

            // Act
            var result = await _controller.SaveProjectManager(item);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveProjectManager_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var item = new ProjectManagerItem { Projectmanager = "Nobody" };
            var dto  = new ProjectManagerDto { Projectmanager = "Nobody" };
            _mapper.Map<ProjectManagerDto>(item).Returns(dto);
            _service.UpdateProjectManagerAsync("Nobody", dto).Returns(FailureResponse<ProjectManagerDto>());

            // Act
            var result = await _controller.SaveProjectManager(item, isEditMode: true);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  MANAGER TAB — DeleteProjectManager
        // ════════════════════════════════════════════════════════════════════════════

        #region DeleteProjectManager

        [Fact]
        public async Task DeleteProjectManager_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            _service.DeleteProjectManagerAsync("Smith, J.").Returns(SuccessResponse(true));

            // Act
            var result = await _controller.DeleteProjectManager("Smith, J.");

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteProjectManager_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _service.DeleteProjectManagerAsync("Unknown").Returns(FailureResponse<bool>());

            // Act
            var result = await _controller.DeleteProjectManager("Unknown");

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  MANAGER TAB — LoadProgramManagerLinksGrid
        // ════════════════════════════════════════════════════════════════════════════

        #region LoadProgramManagerLinksGrid

        [Fact]
        public async Task LoadProgramManagerLinksGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            _mapper.Map<List<ProgramManagerLinkItem>>(Arg.Any<List<ProgramManagerLinkDto>>())
                .Returns(new List<ProgramManagerLinkItem>());
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadProgramManagerLinksGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadProgramManagerLinksGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Filter", "Required");

            // Act
            var result = await _controller.LoadProgramManagerLinksGrid(new PaginationFilter<string>(), null);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadProgramManagerLinksGrid_WithManagerFilter_GridFiltered()
        {
            // Arrange
            var dtos = new List<ProgramManagerLinkDto>
            {
                new() { Program = "P1", Manager = "Smith" },
                new() { Program = "P2", Manager = "Jones" }
            };
            _service.GetPagedProgramManagerLinksByManagerAsync(Arg.Any<QueryParameters<string>>(), "Smith")
                .Returns(SuccessResponse(Paged(dtos)));
            _mapper.Map<List<ProgramManagerLinkItem>>(Arg.Any<List<ProgramManagerLinkDto>>())
                .Returns(new List<ProgramManagerLinkItem> { new() { Program = "P1", Manager = "Smith" } });
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadProgramManagerLinksGrid(request, "Smith");

            // Assert
            var partial    = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProgramManagerLinkItem>>(partial.Model);
            Assert.Single(gridConfig.Data!);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  MANAGER TAB — SaveProgramManagerLink
        // ════════════════════════════════════════════════════════════════════════════

        #region SaveProgramManagerLink

        [Fact]
        public async Task SaveProgramManagerLink_ValidDto_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto = new ProgramManagerLinkDto { Program = "P1", Manager = "Smith" };
            _service.CreateProgramManagerLinkAsync(dto).Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.SaveProgramManagerLink(dto);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveProgramManagerLink_NullDto_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.SaveProgramManagerLink(null!);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveProgramManagerLink_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = new ProgramManagerLinkDto { Program = "P_BAD", Manager = "Smith" };
            _service.CreateProgramManagerLinkAsync(dto).Returns(FailureResponse<ProgramManagerLinkDto>());

            // Act
            var result = await _controller.SaveProgramManagerLink(dto);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  MANAGER TAB — DeleteProgramManagerLink
        // ════════════════════════════════════════════════════════════════════════════

        #region DeleteProgramManagerLink

        [Fact]
        public async Task DeleteProgramManagerLink_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            _service.DeleteProgramManagerLinkAsync("P1", "Smith").Returns(SuccessResponse(true));

            // Act
            var result = await _controller.DeleteProgramManagerLink("P1", "Smith");

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
            await _service.Received(1).DeleteProgramManagerLinkAsync("P1", "Smith");
        }

        [Fact]
        public async Task DeleteProgramManagerLink_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _service.DeleteProgramManagerLinkAsync("X", "Y").Returns(FailureResponse<bool>());

            // Act
            var result = await _controller.DeleteProgramManagerLink("X", "Y");

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  MANAGER TAB — LoadProfitCentreManagerLinksGrid
        // ════════════════════════════════════════════════════════════════════════════

        #region LoadProfitCentreManagerLinksGrid

        [Fact]
        public async Task LoadProfitCentreManagerLinksGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            _service.GetAllProfitCentreManagerLinksAsync().Returns(SuccessResponse(new List<ProfitCentreManagerLinkDto>()));
            _mapper.Map<List<ProfitCentreManagerLinkItem>>(Arg.Any<List<ProfitCentreManagerLinkDto>>())
                .Returns(new List<ProfitCentreManagerLinkItem>());
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadProfitCentreManagerLinksGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadProfitCentreManagerLinksGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Filter", "Required");

            // Act
            var result = await _controller.LoadProfitCentreManagerLinksGrid(new PaginationFilter<string>(), null);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadProfitCentreManagerLinksGrid_ServiceReturnsFailure_ReturnsEmptyGrid()
        {
            // Arrange
            _service.GetAllProfitCentreManagerLinksAsync().Returns(FailureResponse<List<ProfitCentreManagerLinkDto>>());
            _mapper.Map<List<ProfitCentreManagerLinkItem>>(Arg.Any<List<ProfitCentreManagerLinkDto>>())
                .Returns(new List<ProfitCentreManagerLinkItem>());
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadProfitCentreManagerLinksGrid(request, null);

            // Assert
            var partial    = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProfitCentreManagerLinkItem>>(partial.Model);
            Assert.Empty(gridConfig.Data!);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  MANAGER TAB — SaveProfitCentreManagerLink
        // ════════════════════════════════════════════════════════════════════════════

        #region SaveProfitCentreManagerLink

        [Fact]
        public async Task SaveProfitCentreManagerLink_ValidDto_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto = new ProfitCentreManagerLinkDto { ProfitCentre = "RC01", Manager = "Smith" };
            _service.CreateProfitCentreManagerLinkAsync(dto).Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.SaveProfitCentreManagerLink(dto);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveProfitCentreManagerLink_NullDto_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.SaveProfitCentreManagerLink(null!);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveProfitCentreManagerLink_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = new ProfitCentreManagerLinkDto { ProfitCentre = "RC_BAD", Manager = "Jones" };
            _service.CreateProfitCentreManagerLinkAsync(dto).Returns(FailureResponse<ProfitCentreManagerLinkDto>());

            // Act
            var result = await _controller.SaveProfitCentreManagerLink(dto);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  MANAGER TAB — DeleteProfitCentreManagerLink
        // ════════════════════════════════════════════════════════════════════════════

        #region DeleteProfitCentreManagerLink

        [Fact]
        public async Task DeleteProfitCentreManagerLink_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            _service.DeleteProfitCentreManagerLinkAsync("RC01", "Smith").Returns(SuccessResponse(true));

            // Act
            var result = await _controller.DeleteProfitCentreManagerLink("RC01", "Smith");

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
            await _service.Received(1).DeleteProfitCentreManagerLinkAsync("RC01", "Smith");
        }

        [Fact]
        public async Task DeleteProfitCentreManagerLink_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _service.DeleteProfitCentreManagerLinkAsync("X", "Y").Returns(FailureResponse<bool>());

            // Act
            var result = await _controller.DeleteProfitCentreManagerLink("X", "Y");

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  TIME TAB — SaveSetting
        // ════════════════════════════════════════════════════════════════════════════

        #region SaveSetting

        [Fact]
        public async Task SaveSetting_ValidDto_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto = new SettingDto { Id = "HoursInDay", SettingValue = "7.4" };
            _service.UpdateSettingAsync("HoursInDay", dto).Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.SaveSetting(dto);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveSetting_NullDto_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.SaveSetting(null!);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveSetting_EmptyId_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = new SettingDto { Id = "", SettingValue = "7.4" };

            // Act
            var result = await _controller.SaveSetting(dto);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveSetting_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = new SettingDto { Id = "WorkingDays", SettingValue = "250" };
            _service.UpdateSettingAsync("WorkingDays", dto).Returns(FailureResponse<SettingDto>());

            // Act
            var result = await _controller.SaveSetting(dto);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  TIME TAB — LoadTimeTabSettings
        // ════════════════════════════════════════════════════════════════════════════

        #region LoadTimeTabSettings

        [Fact]
        public async Task LoadTimeTabSettings_ServiceReturnsSuccess_ReturnsJsonWithWorkingHoursAndDays()
        {
            // Arrange
            var settings = new List<SettingDto>
            {
                new() { Id = "HoursInDay", SettingValue = "7.2" },
                new() { Id = "DaysInYear",  SettingValue = "220.5" }
            };
            _service.GetAllSettingsAsync().Returns(SuccessResponse(settings));

            // Act
            var result = await _controller.LoadTimeTabSettings();

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.Equal("7.2", element.GetProperty("workingHours").GetString());
            Assert.Equal("220.5", element.GetProperty("workingDays").GetString());
        }

        [Fact]
        public async Task LoadTimeTabSettings_ServiceReturnsFailure_ReturnsJsonWithNullValues()
        {
            // Arrange
            _service.GetAllSettingsAsync().Returns(FailureResponse<List<SettingDto>>());

            // Act
            var result = await _controller.LoadTimeTabSettings();

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.TryGetProperty("workingHours", out var hoursProperty) && hoursProperty.ValueKind == System.Text.Json.JsonValueKind.Null);
            Assert.True(element.TryGetProperty("workingDays", out var daysProperty) && daysProperty.ValueKind == System.Text.Json.JsonValueKind.Null);
        }

        [Fact]
        public async Task LoadTimeTabSettings_ServiceReturnsNullData_ReturnsJsonWithNullValues()
        {
            // Arrange
            var response = new ApiResponseDto<List<SettingDto>> { Success = false, Data = null };
            _service.GetAllSettingsAsync().Returns(response);

            // Act
            var result = await _controller.LoadTimeTabSettings();

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.TryGetProperty("workingHours", out var hoursProperty) && hoursProperty.ValueKind == System.Text.Json.JsonValueKind.Null);
            Assert.True(element.TryGetProperty("workingDays", out var daysProperty) && daysProperty.ValueKind == System.Text.Json.JsonValueKind.Null);
        }

        [Fact]
        public async Task LoadTimeTabSettings_CallsGetAllSettingsAsync()
        {
            // Arrange
            _service.GetAllSettingsAsync().Returns(SuccessResponse(new List<SettingDto>()));

            // Act
            await _controller.LoadTimeTabSettings();

            // Assert
            await _service.Received(1).GetAllSettingsAsync();
        }

        [Fact]
        public async Task LoadTimeTabSettings_CaseInsensitiveIdMatch_ReturnsCorrectValues()
        {
            // Arrange
            var settings = new List<SettingDto>
            {
                new() { Id = "hoursinday", SettingValue = "7.5" },  // lowercase
                new() { Id = "DaysInYear",  SettingValue = "225" }   // mixed case
            };
            _service.GetAllSettingsAsync().Returns(SuccessResponse(settings));

            // Act
            var result = await _controller.LoadTimeTabSettings();

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.Equal("7.5", element.GetProperty("workingHours").GetString());
            Assert.Equal("225", element.GetProperty("workingDays").GetString());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  ADMIN MAINTENANCE TAB — LoadAccessUsersGrid
        // ════════════════════════════════════════════════════════════════════════════

        #region LoadAccessUsersGrid

        [Fact]
        public async Task LoadAccessUsersGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            _service.GetAllAccessSystemsAsync()
                .Returns(SuccessResponse(new List<AccessSystemDto> { new() { SystemId = 1, SystemName = "PIMS" } }));
            _service.GetPagedAccessUsersAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(EmptyPaged<AccessUserDto>()));
            _mapper.Map<List<AccessUserItem>>(Arg.Any<List<AccessUserDto>>()).Returns(new List<AccessUserItem>());
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadAccessUsersGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadAccessUsersGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Filter", "Required");

            // Act
            var result = await _controller.LoadAccessUsersGrid(new PaginationFilter<string>());

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadAccessUsersGrid_ServiceReturnsData_GridContainsItems()
        {
            // Arrange
            var dtos  = new List<AccessUserDto> { new() { SystemId = 1, NtLogin = "jsmith" } };
            var items = new List<AccessUserItem> { new() { SystemId = 1, NtLogin = "jsmith" } };
            _service.GetAllAccessSystemsAsync()
                .Returns(SuccessResponse(new List<AccessSystemDto> { new() { SystemId = 1, SystemName = "PIMS" } }));
            _service.GetPagedAccessUsersAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(Paged(dtos)));
            _mapper.Map<List<AccessUserItem>>(Arg.Any<List<AccessUserDto>>()).Returns(items);
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadAccessUsersGrid(request);

            // Assert
            var partial    = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<AccessUserItem>>(partial.Model);
            Assert.Single(gridConfig.Data!);
        }

        [Fact]
        public async Task LoadAccessUsersGrid_ServiceReturnsFailure_ReturnsEmptyGrid()
        {
            // Arrange
            _service.GetAllAccessSystemsAsync()
                .Returns(SuccessResponse(new List<AccessSystemDto> { new() { SystemId = 1, SystemName = "PIMS" } }));
            _service.GetPagedAccessUsersAsync(Arg.Any<QueryParameters<string>>())
                .Returns(FailureResponse<PaginatedResult<AccessUserDto>>());
            _mapper.Map<List<AccessUserItem>>(Arg.Any<List<AccessUserDto>>()).Returns(new List<AccessUserItem>());
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadAccessUsersGrid(request);

            // Assert
            var partial    = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<AccessUserItem>>(partial.Model);
            Assert.Empty(gridConfig.Data!);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  ADMIN MAINTENANCE TAB — GetAddEditAccessUserPartial
        // ════════════════════════════════════════════════════════════════════════════

        #region GetAddEditAccessUserPartial

        [Fact]
        public async Task GetAddEditAccessUserPartial_NoArgs_ReturnsPartialViewWithEmptyModel()
        {
            // Act
            var result = await _controller.GetAddEditAccessUserPartial(null, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditAccessUser", partial.ViewName);
            Assert.IsType<AccessUserItem>(partial.Model);
        }

        [Fact]
        public async Task GetAddEditAccessUserPartial_ValidArgs_ServiceReturnsData_ReturnsPopulatedModel()
        {
            // Arrange
            var dto  = new AccessUserDto { SystemId = 1, NtLogin = "jsmith", UserName = "John Smith" };
            var item = new AccessUserItem { SystemId = 1, NtLogin = "jsmith", UserName = "John Smith" };
            _service.GetAccessUserByIdAsync(1, "jsmith").Returns(SuccessResponse(dto));
            _mapper.Map<AccessUserItem>(dto).Returns(item);

            // Act
            var result = await _controller.GetAddEditAccessUserPartial(1, "jsmith");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model   = Assert.IsType<AccessUserItem>(partial.Model);
            Assert.Equal("jsmith", model.NtLogin);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  ADMIN MAINTENANCE TAB — SaveAccessUser
        // ════════════════════════════════════════════════════════════════════════════

        #region SaveAccessUser

        [Fact]
        public async Task SaveAccessUser_NewUser_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item = new AccessUserItem { SystemId = 0, NtLogin = "newuser", UserName = "New User" };
            var dto  = new AccessUserDto  { SystemId = 0, NtLogin = "newuser", UserName = "New User" };
            _mapper.Map<AccessUserDto>(item).Returns(dto);
            _service.CreateAccessUserAsync(dto).Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.SaveAccessUser(item);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveAccessUser_ExistingUser_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item = new AccessUserItem { SystemId = 2, NtLogin = "jsmith", UserName = "John Smith" };
            var dto  = new AccessUserDto  { SystemId = 2, NtLogin = "jsmith", UserName = "John Smith" };
            _mapper.Map<AccessUserDto>(item).Returns(dto);
            _service.UpdateAccessUserAsync(2, "jsmith", dto).Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.SaveAccessUser(item);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveAccessUser_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Ntlogin", "Required");
            var item = new AccessUserItem();

            // Act
            var result = await _controller.SaveAccessUser(item);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveAccessUser_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var item = new AccessUserItem { SystemId = 0, NtLogin = "baduser", UserName = "Bad" };
            var dto  = new AccessUserDto  { SystemId = 0, NtLogin = "baduser", UserName = "Bad" };
            _mapper.Map<AccessUserDto>(item).Returns(dto);
            _service.CreateAccessUserAsync(dto).Returns(FailureResponse<AccessUserDto>());

            // Act
            var result = await _controller.SaveAccessUser(item);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  ADMIN MAINTENANCE TAB — DeleteAccessUser
        // ════════════════════════════════════════════════════════════════════════════

        #region DeleteAccessUser

        [Fact]
        public async Task DeleteAccessUser_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            _service.GetAccessUserLevelsByUserAsync(1, "jsmith")
                .Returns(SuccessResponse(new List<AccessUserLevelDto>()));
            _service.DeleteAccessUserAsync(1, "jsmith").Returns(SuccessResponse(true));

            // Act
            var result = await _controller.DeleteAccessUser(1, "jsmith");

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
            await _service.Received(1).GetAccessUserLevelsByUserAsync(1, "jsmith");
            await _service.Received(1).DeleteAccessUserAsync(1, "jsmith");
        }

        [Fact]
        public async Task DeleteAccessUser_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _service.GetAccessUserLevelsByUserAsync(99, "nobody")
                .Returns(SuccessResponse(new List<AccessUserLevelDto>()));
            _service.DeleteAccessUserAsync(99, "nobody").Returns(FailureResponse<bool>());

            // Act
            var result = await _controller.DeleteAccessUser(99, "nobody");

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
            await _service.Received(1).GetAccessUserLevelsByUserAsync(99, "nobody");
        }

        [Fact]
        public async Task DeleteAccessUser_UserHasAccessReferences_ReturnsGuidanceMessage_AndSkipsDelete()
        {
            // Arrange
            _service.GetAccessUserLevelsByUserAsync(1, "jsmith")
                .Returns(SuccessResponse(new List<AccessUserLevelDto>
                {
                    new() { SystemId = 1, NtLogin = "jsmith", AccessLevelId = 1 }
                }));

            // Act
            var result = await _controller.DeleteAccessUser(1, "jsmith");

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Contains("Delete related records from the User Access grid first", element.GetProperty("message").GetString());
            await _service.DidNotReceive().DeleteAccessUserAsync(1, "jsmith");
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  ADMIN MAINTENANCE TAB — LoadAccessUserLevelsGrid
        // ════════════════════════════════════════════════════════════════════════════

        #region LoadAccessUserLevelsGrid

        [Fact]
        public async Task LoadAccessUserLevelsGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            _service.GetPagedAccessUserLevelsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(new PaginatedResult<AccessUserLevelDto>()));
            _mapper.Map<List<AccessUserLevelItem>>(Arg.Any<object>())
                .Returns(new List<AccessUserLevelItem>());
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadAccessUserLevelsGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadAccessUserLevelsGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Filter", "Required");

            // Act
            var result = await _controller.LoadAccessUserLevelsGrid(new PaginationFilter<string>());

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadAccessUserLevelsGrid_ServiceReturnsData_GridContainsItems()
        {
            // Arrange
            var dtos  = new List<AccessUserLevelDto> { new() { SystemId = 1, NtLogin = "jsmith", AccessLevelId = 2 } };
            var items = new List<AccessUserLevelItem> { new() { SystemId = 1, NtLogin = "jsmith", AccessLevelId = 2 } };
            var paged = new PaginatedResult<AccessUserLevelDto>(dtos, 1, 1, 10);
            _service.GetPagedAccessUserLevelsAsync(Arg.Any<QueryParameters<string>>()).Returns(SuccessResponse(paged));
            _mapper.Map<List<AccessUserLevelItem>>(Arg.Any<object>()).Returns(items);
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadAccessUserLevelsGrid(request);

            // Assert
            var partial    = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<AccessUserLevelItem>>(partial.Model);
            Assert.Single(gridConfig.Data!);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  ADMIN MAINTENANCE TAB — GetAddEditAccessUserLevelPartial
        // ════════════════════════════════════════════════════════════════════════════

        #region GetAddEditAccessUserLevelPartial

        [Fact]
        public async Task GetAddEditAccessUserLevelPartial_NoArgs_ReturnsPartialViewWithEmptyModel()
        {
            // Act
            var result = await _controller.GetAddEditAccessUserLevelPartial(null, null, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditAccessUserLevel", partial.ViewName);
            Assert.IsType<AccessUserLevelItem>(partial.Model);
        }

        [Fact]
        public async Task GetAddEditAccessUserLevelPartial_ValidArgs_ServiceReturnsData_ReturnsPopulatedModel()
        {
            // Arrange
            var dto  = new AccessUserLevelDto { SystemId = 1, NtLogin = "jsmith", AccessLevelId = 3 };
            var item = new AccessUserLevelItem { SystemId = 1, NtLogin = "jsmith", AccessLevelId = 3 };
            _service.GetAccessUserLevelByIdAsync(1, "jsmith", 3).Returns(SuccessResponse(dto));
            _mapper.Map<AccessUserLevelItem>(dto).Returns(item);

            // Act
            var result = await _controller.GetAddEditAccessUserLevelPartial(1, "jsmith", 3);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model   = Assert.IsType<AccessUserLevelItem>(partial.Model);
            Assert.Equal(3, model.AccessLevelId);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  ADMIN MAINTENANCE TAB — SaveAccessUserLevel
        // ════════════════════════════════════════════════════════════════════════════

        #region SaveAccessUserLevel

        [Fact]
        public async Task SaveAccessUserLevel_ValidItem_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item = new AccessUserLevelItem { SystemId = 1, NtLogin = "jsmith", AccessLevelId = 2 };
            var dto  = new AccessUserLevelDto  { SystemId = 1, NtLogin = "jsmith", AccessLevelId = 2 };
            _mapper.Map<AccessUserLevelDto>(item).Returns(dto);
            _service.CreateAccessUserLevelAsync(dto).Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.SaveAccessUserLevel(item);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveAccessUserLevel_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Ntlogin", "Required");
            var item = new AccessUserLevelItem();

            // Act
            var result = await _controller.SaveAccessUserLevel(item);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveAccessUserLevel_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var item = new AccessUserLevelItem { SystemId = 1, NtLogin = "bad", AccessLevelId = 99 };
            var dto  = new AccessUserLevelDto  { SystemId = 1, NtLogin = "bad", AccessLevelId = 99 };
            _mapper.Map<AccessUserLevelDto>(item).Returns(dto);
            _service.CreateAccessUserLevelAsync(dto).Returns(FailureResponse<AccessUserLevelDto>());

            // Act
            var result = await _controller.SaveAccessUserLevel(item);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  ADMIN MAINTENANCE TAB — DeleteAccessUserLevel
        // ════════════════════════════════════════════════════════════════════════════

        #region DeleteAccessUserLevel

        [Fact]
        public async Task DeleteAccessUserLevel_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            _service.DeleteAccessUserLevelAsync(1, "jsmith", 2).Returns(SuccessResponse(true));

            // Act
            var result = await _controller.DeleteAccessUserLevel(1, "jsmith", 2);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
            await _service.Received(1).DeleteAccessUserLevelAsync(1, "jsmith", 2);
        }

        [Fact]
        public async Task DeleteAccessUserLevel_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _service.DeleteAccessUserLevelAsync(99, "x", 99).Returns(FailureResponse<bool>());

            // Act
            var result = await _controller.DeleteAccessUserLevel(99, "x", 99);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  OTHER TAB — LoadFrequenciesGrid
        // ════════════════════════════════════════════════════════════════════════════

        #region LoadFrequenciesGrid

        [Fact]
        public async Task LoadFrequenciesGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            _service.GetPagedFrequenciesAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(EmptyPaged<FrequencyDto>()));
            _mapper.Map<List<FrequencyItem>>(Arg.Any<List<FrequencyDto>>()).Returns(new List<FrequencyItem>());
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadFrequenciesGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadFrequenciesGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Filter", "Required");

            // Act
            var result = await _controller.LoadFrequenciesGrid(new PaginationFilter<string>());

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadFrequenciesGrid_ServiceReturnsData_GridContainsItems()
        {
            // Arrange
            var dtos  = new List<FrequencyDto>  { new() { Frequencyid = 1, FrequencyValue = "Monthly" } };
            var items = new List<FrequencyItem> { new() { Frequencyid = 1, FrequencyValue = "Monthly" } };
            _service.GetPagedFrequenciesAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(Paged(dtos)));
            _mapper.Map<List<FrequencyItem>>(Arg.Any<List<FrequencyDto>>()).Returns(items);
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadFrequenciesGrid(request);

            // Assert
            var partial    = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<FrequencyItem>>(partial.Model);
            Assert.Single(gridConfig.Data!);
        }

        [Fact]
        public async Task LoadFrequenciesGrid_ServiceReturnsFailure_ReturnsEmptyGrid()
        {
            // Arrange
            _service.GetPagedFrequenciesAsync(Arg.Any<QueryParameters<string>>())
                .Returns(FailureResponse<PaginatedResult<FrequencyDto>>());
            _mapper.Map<List<FrequencyItem>>(Arg.Any<List<FrequencyDto>>()).Returns(new List<FrequencyItem>());
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadFrequenciesGrid(request);

            // Assert
            var partial    = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<FrequencyItem>>(partial.Model);
            Assert.Empty(gridConfig.Data!);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  OTHER TAB — GetAddEditFrequencyPartial
        // ════════════════════════════════════════════════════════════════════════════

        #region GetAddEditFrequencyPartial

        [Fact]
        public async Task GetAddEditFrequencyPartial_NoId_ReturnsPartialViewWithEmptyModel()
        {
            // Act
            var result = await _controller.GetAddEditFrequencyPartial(null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditFrequency", partial.ViewName);
            Assert.IsType<FrequencyItem>(partial.Model);
        }

        [Fact]
        public async Task GetAddEditFrequencyPartial_ValidId_ServiceReturnsData_ReturnsPopulatedModel()
        {
            // Arrange
            var dto  = new FrequencyDto  { Frequencyid = 5, FrequencyValue = "Weekly" };
            var item = new FrequencyItem { Frequencyid = 5, FrequencyValue = "Weekly" };
            _service.GetFrequencyByIdAsync(5).Returns(SuccessResponse(dto));
            _mapper.Map<FrequencyItem>(dto).Returns(item);

            // Act
            var result = await _controller.GetAddEditFrequencyPartial(5);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model   = Assert.IsType<FrequencyItem>(partial.Model);
            Assert.Equal(5, model.Frequencyid);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  OTHER TAB — SaveFrequency
        // ════════════════════════════════════════════════════════════════════════════

        #region SaveFrequency

        [Fact]
        public async Task SaveFrequency_NewFrequency_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item = new FrequencyItem { Frequencyid = 0, FrequencyValue = "Daily" };
            var dto  = new FrequencyDto  { Frequencyid = 0, FrequencyValue = "Daily" };
            _mapper.Map<FrequencyDto>(item).Returns(dto);
            _service.CreateFrequencyAsync(dto).Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.SaveFrequency(item, false);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveFrequency_ExistingFrequency_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item = new FrequencyItem { Frequencyid = 3, FrequencyValue = "Monthly" };
            var dto  = new FrequencyDto  { Frequencyid = 3, FrequencyValue = "Monthly" };
            _mapper.Map<FrequencyDto>(item).Returns(dto);
            _service.UpdateFrequencyAsync(3, dto).Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.SaveFrequency(item, true);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveFrequency_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("FrequencyValue", "Required");
            var item = new FrequencyItem();

            // Act
            var result = await _controller.SaveFrequency(item, false);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveFrequency_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var item = new FrequencyItem { Frequencyid = 0, FrequencyValue = "Bad" };
            var dto  = new FrequencyDto  { Frequencyid = 0, FrequencyValue = "Bad" };
            _mapper.Map<FrequencyDto>(item).Returns(dto);
            _service.CreateFrequencyAsync(dto).Returns(FailureResponse<FrequencyDto>());

            // Act
            var result = await _controller.SaveFrequency(item, false);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  OTHER TAB — DeleteFrequency
        // ════════════════════════════════════════════════════════════════════════════

        #region DeleteFrequency

        [Fact]
        public async Task DeleteFrequency_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            _service.DeleteFrequencyAsync(5).Returns(SuccessResponse(true));

            // Act
            var result = await _controller.DeleteFrequency(5);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
            await _service.Received(1).DeleteFrequencyAsync(5);
        }

        [Fact]
        public async Task DeleteFrequency_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _service.DeleteFrequencyAsync(99).Returns(FailureResponse<bool>());

            // Act
            var result = await _controller.DeleteFrequency(99);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  OTHER TAB — LoadReviewItemsGrid
        // ════════════════════════════════════════════════════════════════════════════

        #region LoadReviewItemsGrid

        [Fact]
        public async Task LoadReviewItemsGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            _service.GetPagedReviewItemsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(EmptyPaged<ReviewItemDto>()));
            _mapper.Map<List<ReviewItemItem>>(Arg.Any<List<ReviewItemDto>>()).Returns(new List<ReviewItemItem>());
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadReviewItemsGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadReviewItemsGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Filter", "Required");

            // Act
            var result = await _controller.LoadReviewItemsGrid(new PaginationFilter<string>());

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadReviewItemsGrid_ServiceReturnsData_GridContainsItems()
        {
            // Arrange
            var dtos  = new List<ReviewItemDto>  { new() { Itemid = 1, Item = "Design Review" } };
            var items = new List<ReviewItemItem> { new() { Itemid = 1, Item = "Design Review" } };
            _service.GetPagedReviewItemsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(Paged(dtos)));
            _mapper.Map<List<ReviewItemItem>>(Arg.Any<List<ReviewItemDto>>()).Returns(items);
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadReviewItemsGrid(request);

            // Assert
            var partial    = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ReviewItemItem>>(partial.Model);
            Assert.Single(gridConfig.Data!);
        }

        [Fact]
        public async Task LoadReviewItemsGrid_ServiceReturnsFailure_ReturnsEmptyGrid()
        {
            // Arrange
            _service.GetPagedReviewItemsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(FailureResponse<PaginatedResult<ReviewItemDto>>());
            _mapper.Map<List<ReviewItemItem>>(Arg.Any<List<ReviewItemDto>>()).Returns(new List<ReviewItemItem>());
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadReviewItemsGrid(request);

            // Assert
            var partial    = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ReviewItemItem>>(partial.Model);
            Assert.Empty(gridConfig.Data!);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  OTHER TAB — GetAddEditReviewItemPartial
        // ════════════════════════════════════════════════════════════════════════════

        #region GetAddEditReviewItemPartial

        [Fact]
        public async Task GetAddEditReviewItemPartial_NoId_ReturnsPartialViewWithEmptyModel()
        {
            // Act
            var result = await _controller.GetAddEditReviewItemPartial(null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditReviewItem", partial.ViewName);
            Assert.IsType<ReviewItemItem>(partial.Model);
        }

        [Fact]
        public async Task GetAddEditReviewItemPartial_ValidId_ServiceReturnsData_ReturnsPopulatedModel()
        {
            // Arrange
            var dto  = new ReviewItemDto  { Itemid = 4, Item = "Test Review" };
            var item = new ReviewItemItem { Itemid = 4, Item = "Test Review" };
            _service.GetReviewItemByIdAsync(4).Returns(SuccessResponse(dto));
            _mapper.Map<ReviewItemItem>(dto).Returns(item);

            // Act
            var result = await _controller.GetAddEditReviewItemPartial(4);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model   = Assert.IsType<ReviewItemItem>(partial.Model);
            Assert.Equal(4, model.Itemid);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  OTHER TAB — SaveReviewItem
        // ════════════════════════════════════════════════════════════════════════════

        #region SaveReviewItem

        [Fact]
        public async Task SaveReviewItem_NewItem_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var reviewItem = new ReviewItemItem { Itemid = 0, Item = "New Review" };
            var dto        = new ReviewItemDto  { Itemid = 0, Item = "New Review" };
            _mapper.Map<ReviewItemDto>(reviewItem).Returns(dto);
            _service.CreateReviewItemAsync(dto).Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.SaveReviewItem(reviewItem, isEdit: false);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveReviewItem_ExistingItem_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var reviewItem = new ReviewItemItem { Itemid = 7, Item = "Existing Review" };
            var dto        = new ReviewItemDto  { Itemid = 7, Item = "Existing Review" };
            _mapper.Map<ReviewItemDto>(reviewItem).Returns(dto);
            _service.UpdateReviewItemAsync(7, dto).Returns(SuccessResponse(dto));

            // Act
            var result = await _controller.SaveReviewItem(reviewItem, isEdit: true);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveReviewItem_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Item", "Required");
            var reviewItem = new ReviewItemItem();

            // Act
            var result = await _controller.SaveReviewItem(reviewItem, isEdit: false);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task SaveReviewItem_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var reviewItem = new ReviewItemItem { Itemid = 0, Item = "Bad" };
            var dto        = new ReviewItemDto  { Itemid = 0, Item = "Bad" };
            _mapper.Map<ReviewItemDto>(reviewItem).Returns(dto);
            _service.CreateReviewItemAsync(dto).Returns(FailureResponse<ReviewItemDto>());

            // Act
            var result = await _controller.SaveReviewItem(reviewItem, isEdit: false);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════════════
        //  OTHER TAB — DeleteReviewItem
        // ════════════════════════════════════════════════════════════════════════════

        #region DeleteReviewItem

        [Fact]
        public async Task DeleteReviewItem_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            _service.DeleteReviewItemAsync(6).Returns(SuccessResponse(true));

            // Act
            var result = await _controller.DeleteReviewItem(6);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.True(element.GetProperty("success").GetBoolean());
            await _service.Received(1).DeleteReviewItemAsync(6);
        }

        [Fact]
        public async Task DeleteReviewItem_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _service.DeleteReviewItemAsync(99).Returns(FailureResponse<bool>());

            // Act
            var result = await _controller.DeleteReviewItem(99);

            // Assert
            var json    = Assert.IsType<JsonResult>(result);
            var element = GetJsonElement(json);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion
    }
}
