using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Apha.FPSApps.Web.Areas.PACT.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using PactMonthlyOutputDto = Apha.FPSApps.Application.Dtos.PACT.PactMonthlyOutputDto;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.MonthlyOutputControllerTest
{
    public class MonthlyOutputControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IPactMonthlyOutputService _monthlyOutputService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IMonthService _monthService;
        private readonly Apha.Common.Utilities.ExcelExport.IExcelExportService _excelExportService;
        private readonly ITestCapabilityService _testCapabilityService;
        private readonly ITestRequirementService _testRequirementService;
        private readonly MonthlyOutputController _controller;

        public MonthlyOutputControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _monthlyOutputService = Substitute.For<IPactMonthlyOutputService>();
            _workGroupService = Substitute.For<IWorkGroupService>();
            _monthService = Substitute.For<IMonthService>();
            _excelExportService = Substitute.For<Apha.Common.Utilities.ExcelExport.IExcelExportService>();
            _testCapabilityService = Substitute.For<ITestCapabilityService>();
            _testRequirementService = Substitute.For<ITestRequirementService>();

            _controller = new MonthlyOutputController(
                _mapper,
                _monthlyOutputService,
                _workGroupService,
                _monthService,
                _excelExportService,
                _testCapabilityService,
                _testRequirementService);
        }

        [Fact]
        public async Task LoadLiveGrid_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("WorkGroup", "Required");

            var result = await _controller.LoadLiveGrid(new PaginationFilter<string> { Filter = "{}" }, "WG1", "TC1", "B1", 6);

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
        public async Task GetTestCodesByWorkGroup_WhenServiceFails_ReturnsEmptyJsonArray()
        {
            _testCapabilityService.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.GetTestCodesByWorkGroup("WG1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task GetTestCodesByWorkGroup_WhenServiceSucceeds_ReturnsDistinctTestCodes()
        {
            _testCapabilityService.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse(
                [
                    new TestCapabilityDto { TestCode = "TC2" },
                    new TestCapabilityDto { TestCode = "TC1" },
                    new TestCapabilityDto { TestCode = "TC2" }
                ]));

            var result = await _controller.GetTestCodesByWorkGroup("WG1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Equal(2, values.Count);
        }

        [Fact]
        public async Task GetBuyersByTestCode_WhenServiceFails_ReturnsEmptyJsonArray()
        {
            _testRequirementService.GetAllActiveAsync()
                .Returns(ApiResponseDto<List<TestRequirementDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.GetBuyersByTestCode("WG1", "TC1");

            var json = Assert.IsType<JsonResult>(result);
            var values = ((IEnumerable<object>)json.Value!).ToList();
            Assert.Empty(values);
        }

        [Fact]
        public async Task GetLiveRecord_WhenServiceReturnsNoData_ReturnsNotFound()
        {
            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse([]));
            _monthlyOutputService.GetLiveByKeyAsync("TC1", "B1", 6, "WG1")
                .Returns(ApiResponseDto<PactMonthlyOutputDto>.FailureResponse([], new ApiMetaDto()));

            var result = await _controller.GetLiveRecord("TC1", "B1", 6, "WG1");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task SaveLiveRecord_InvalidModelState_ReturnsFailureJson()
        {
            _controller.ModelState.AddModelError("Volume", "Invalid");

            var result = await _controller.SaveLiveRecord(new MonthlyOutputLiveItem());

            var json = Assert.IsType<JsonResult>(result);
            var success = json.Value?.GetType().GetProperty("success")?.GetValue(json.Value);
            Assert.NotNull(success);
            Assert.IsType<bool>(success);
            Assert.False((bool)success);
        }
    }
}
