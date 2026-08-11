using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.FPSApps.Web.Areas.PACT.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.MonthlyTimeControllerTest
{
    public class MonthlyTimeControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IPactMonthlyTimeService _monthlyTimeService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IEmployeeService _employeeService;
        private readonly IPactTimeCodeValidService _timeCodeValidService;
        private readonly IMonthService _monthService;
        private readonly Apha.Common.Utilities.ExcelExport.IExcelExportService _excelExportService;
        private readonly MonthlyTimeController _controller;

        public MonthlyTimeControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _monthlyTimeService = Substitute.For<IPactMonthlyTimeService>();
            _workGroupService = Substitute.For<IWorkGroupService>();
            _employeeService = Substitute.For<IEmployeeService>();
            _timeCodeValidService = Substitute.For<IPactTimeCodeValidService>();
            _monthService = Substitute.For<IMonthService>();
            _excelExportService = Substitute.For<Apha.Common.Utilities.ExcelExport.IExcelExportService>();

            _controller = new MonthlyTimeController(
                _mapper,
                _monthlyTimeService,
                _workGroupService,
                _employeeService,
                _timeCodeValidService,
                _monthService,
                _excelExportService);
        }

        [Fact]
        public async Task LoadLiveGrid_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("WorkGroup", "Required");

            var result = await _controller.LoadLiveGrid(new PaginationFilter<string> { Filter = "{}" }, "WG1", "TC1", "S1", "PP1", 6);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadStagingGrid_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Passed", "Invalid");

            var result = await _controller.LoadStagingGrid(new PaginationFilter<string> { Filter = "{}" }, true);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetStaffByWorkGroup_WhenServiceFails_ReturnsEmptyJsonArray()
        {
            _employeeService.GetPactWorkGroupStaffAsync("WG1")
                .Returns(ApiResponseDto<List<PactStaffDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.GetStaffByWorkGroup("WG1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task GetStaffByWorkGroup_WhenServiceSucceeds_ReturnsFilteredStaff()
        {
            _employeeService.GetPactWorkGroupStaffAsync("WG1")
                .Returns(ApiResponseDto<List<PactStaffDto>>.SuccessResponse(
                [
                    new PactStaffDto { PactId = "S1", Name = "A" },
                    new PactStaffDto { PactId = null, Name = "B" }
                ]));

            var result = await _controller.GetStaffByWorkGroup("WG1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Single(values);
        }

        [Fact]
        public async Task GetTimeCodesByWorkGroup_WithoutWorkGroup_ReturnsEmptyJsonArray()
        {
            var result = await _controller.GetTimeCodesByWorkGroup(null);

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task GetProjectsByWorkGroupAndTimeCode_WithoutInputs_ReturnsEmptyJsonArray()
        {
            var result = await _controller.GetProjectsByWorkGroupAndTimeCode("WG1", null);

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task GetAllTimeCodes_WhenServiceFails_ReturnsEmptyJsonArray()
        {
            _timeCodeValidService.GetAllDistinctTimeCodesAsync()
                .Returns(ApiResponseDto<List<string>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.GetAllTimeCodes();

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task SaveLiveRecord_InvalidModelState_ReturnsFailureJson()
        {
            _controller.ModelState.AddModelError("Hours", "Invalid");

            var result = await _controller.SaveLiveRecord(new MonthlyTimeLiveItem());

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.NotNull(success);
            Assert.IsType<bool>(success);
            Assert.False((bool)success);
        }
    }
}
