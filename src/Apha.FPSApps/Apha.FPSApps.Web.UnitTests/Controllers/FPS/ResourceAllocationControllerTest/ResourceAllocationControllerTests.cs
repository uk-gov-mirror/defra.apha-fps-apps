using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ResourceAllocationControllerTest
{
    public class ResourceAllocationControllerTests
    {
        private const string DefaultResourceCentre = "RC01";
        private const string DefaultWgGrade = "WG01";
        private const string DefaultStaffId = "PACT001";

        private readonly IMapper _mapper;
        private readonly IResourceAllocationService _resourceAllocationService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IWorkGroupGradeService _workGroupGradeService;
        private readonly IWorkGroupService _workGroupService;
        private readonly ResourceAllocationController _controller;

        public ResourceAllocationControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _resourceAllocationService = Substitute.For<IResourceAllocationService>();
            _profitCentreService = Substitute.For<IProfitCentreService>();
            _workGroupGradeService = Substitute.For<IWorkGroupGradeService>();
            _workGroupService = Substitute.For<IWorkGroupService>();

            _controller = new ResourceAllocationController(
                _mapper,
                _resourceAllocationService,
                _profitCentreService,
                _workGroupGradeService,
                _workGroupService);
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

        private static List<WorkgroupGradeDto> BuildGradeList() =>
        [
            new() { WgGrade = "WG01", Workgroup = "WorkGroup A", ProfitCentreGrade = "G001", GradeCode = "GC01" },
            new() { WgGrade = "WG02", Workgroup = "WorkGroup B", ProfitCentreGrade = "G002", GradeCode = "GC02" }
        ];

        private static List<ResourceStaffAllocationDto> BuildAllocationList() =>
        [
            new() { StaffId = "PACT001", Name = "Alpha, Staff", PlannedHours = 20.0, HrsAvail = 37.0, AppChargeHours = 18.0, ChargeHours = 22.0 },
            new() { StaffId = "PACT002", Name = "Beta, Staff",  PlannedHours = 15.0, HrsAvail = 37.0, AppChargeHours = 14.0, ChargeHours = 16.0 }
        ];

        private static List<ResourceStaffJobDetailDto> BuildJobDetailList() =>
        [
            new() { StaffId = "PACT001", JobCode = "J001", PlannedHours = 10.0 },
            new() { StaffId = "PACT001", JobCode = "J002", PlannedHours = 10.0 }
        ];

        // ── Constructor Tests ─────────────────────────────────────────────────

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ResourceAllocationController(
                null!, _resourceAllocationService, _profitCentreService, _workGroupGradeService, _workGroupService));
        }

        [Fact]
        public void Constructor_WithNullResourceAllocationService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ResourceAllocationController(
                _mapper, null!, _profitCentreService, _workGroupGradeService, _workGroupService));
        }

        [Fact]
        public void Constructor_WithNullProfitCentreService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ResourceAllocationController(
                _mapper, _resourceAllocationService, null!, _workGroupGradeService, _workGroupService));
        }

        [Fact]
        public void Constructor_WithNullWorkGroupGradeService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ResourceAllocationController(
                _mapper, _resourceAllocationService, _profitCentreService, null!, _workGroupService));
        }

        [Fact]
        public void Constructor_WithNullWorkGroupService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ResourceAllocationController(
                _mapper, _resourceAllocationService, _profitCentreService, _workGroupGradeService, null!));
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
            var model = Assert.IsType<ResourceAllocationViewModel>(viewResult.Model);

            Assert.Equal(2, model.ResourceCentres.Count);
            Assert.NotNull(model.StaffAllocationGrid);
            Assert.NotNull(model.StaffJobsGrid);
            Assert.Empty(model.StaffAllocationGrid.Data);
            Assert.Empty(model.StaffJobsGrid.Data);
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
            var model = Assert.IsType<ResourceAllocationViewModel>(viewResult.Model);
            Assert.Empty(model.ResourceCentres);
        }

        #endregion

        // ── GetGradesByResourceCentre Tests ───────────────────────────────────

        #region GetGradesByResourceCentre Tests

        [Fact]
        public async Task GetGradesByResourceCentre_WithValidResourceCentre_ReturnsGrades()
        {
            // Arrange
            var grades = BuildGradeList();
            _workGroupGradeService.GetWorkgroupGradesByWorkGroupAsync(DefaultResourceCentre)
                .Returns(ApiResponseDto<List<WorkgroupGradeDto>>.SuccessResponse(grades));

            // Act
            var result = await _controller.GetGradesByResourceCentre(DefaultResourceCentre);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);

            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal(2, element.GetProperty("data").GetArrayLength());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task GetGradesByResourceCentre_WithBlankResourceCentre_ReturnsFailureJson(string? resourceCentre)
        {
            // Act
            var result = await _controller.GetGradesByResourceCentre(resourceCentre!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);

            Assert.False(element.GetProperty("success").GetBoolean());
            await _workGroupGradeService.DidNotReceive()
                .GetWorkgroupGradesByWorkGroupAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetGradesByResourceCentre_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Grade load error", Code = "ERR" } };
            _workGroupGradeService.GetWorkgroupGradesByWorkGroupAsync(DefaultResourceCentre)
                .Returns(ApiResponseDto<List<WorkgroupGradeDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetGradesByResourceCentre(DefaultResourceCentre);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);

            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Grade load error", element.GetProperty("message").GetString());
        }

        #endregion

        // ── LoadStaffAllocationGrid Tests ─────────────────────────────────────

        #region LoadStaffAllocationGrid Tests

        [Fact]
        public async Task LoadStaffAllocationGrid_WithBlankWorkGroupGrade_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadStaffAllocationGrid(request, string.Empty);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            await _resourceAllocationService.DidNotReceive()
                .GetPagedStaffAllocationsByWorkGroupGradeAsync(Arg.Any<string>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadStaffAllocationGrid_WithValidGrade_ReturnsPopulatedGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = BuildAllocationList();
            var response = ApiResponseDto<List<ResourceStaffAllocationDto>>.SuccessResponse(data);

            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _resourceAllocationService.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWgGrade, query)
                .Returns(response);
            _mapper.Map<ResourceStaffAllocationItem>(Arg.Any<ResourceStaffAllocationDto>())
                .Returns(ci => new ResourceStaffAllocationItem { StaffId = ci.Arg<ResourceStaffAllocationDto>().StaffId });
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadStaffAllocationGrid(request, DefaultWgGrade);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<ResourceStaffAllocationItem>>(partial.Model);
            await _resourceAllocationService.Received(1)
                .GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWgGrade, query);
        }

        [Fact]
        public async Task LoadStaffAllocationGrid_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Alloc error", Code = "ERR" } };
            var response = ApiResponseDto<List<ResourceStaffAllocationDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _resourceAllocationService.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWgGrade, query)
                .Returns(response);

            // Act
            var result = await _controller.LoadStaffAllocationGrid(request, DefaultWgGrade);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Alloc error", element.GetProperty("message").GetString());
        }

        #endregion

        // ── GetStaffAllocationTotals Tests ────────────────────────────────────

        #region GetStaffAllocationTotals Tests

        [Fact]
        public async Task GetStaffAllocationTotals_WithBlankWorkGroupGrade_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.GetStaffAllocationTotals(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetStaffAllocationTotals_WithValidGrade_ReturnsTotalsJson()
        {
            // Arrange
            var data = BuildAllocationList(); // both have HrsAvail = 37.0
            var query = new QueryParameters<string> { Page = 1, PageSize = int.MaxValue };
            var response = ApiResponseDto<List<ResourceStaffAllocationDto>>.SuccessResponse(data);

            _resourceAllocationService.GetPagedStaffAllocationsByWorkGroupGradeAsync(
                    DefaultWgGrade, Arg.Is<QueryParameters<string>>(q => q.Page == 1 && q.PageSize == int.MaxValue))
                .Returns(response);

            // Act
            var result = await _controller.GetStaffAllocationTotals(DefaultWgGrade);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);

            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.True(element.TryGetProperty("hrsAvail", out _));
            Assert.True(element.TryGetProperty("plannedHrs", out _));
            Assert.True(element.TryGetProperty("allocationPct", out _));
            Assert.True(element.TryGetProperty("assuredChargeHrs", out _));
            Assert.True(element.TryGetProperty("assuredUtilPct", out _));
            Assert.True(element.TryGetProperty("totalChargeHrs", out _));
            Assert.True(element.TryGetProperty("totalUtilPct", out _));
        }

        [Fact]
        public async Task GetStaffAllocationTotals_WithZeroHrsAvail_ReturnsEmptyPercentages()
        {
            // Arrange
            var data = new List<ResourceStaffAllocationDto> { new() { HrsAvail = 0 } };
            var response = ApiResponseDto<List<ResourceStaffAllocationDto>>.SuccessResponse(data);

            _resourceAllocationService.GetPagedStaffAllocationsByWorkGroupGradeAsync(
                    DefaultWgGrade, Arg.Any<QueryParameters<string>>())
                .Returns(response);

            // Act
            var result = await _controller.GetStaffAllocationTotals(DefaultWgGrade);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);

            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal(string.Empty, element.GetProperty("allocationPct").GetString());
            Assert.Equal(string.Empty, element.GetProperty("assuredUtilPct").GetString());
            Assert.Equal(string.Empty, element.GetProperty("totalUtilPct").GetString());
        }

        [Fact]
        public async Task GetStaffAllocationTotals_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Totals error", Code = "ERR" } };
            var response = ApiResponseDto<List<ResourceStaffAllocationDto>>.FailureResponse(errors, new ApiMetaDto());

            _resourceAllocationService.GetPagedStaffAllocationsByWorkGroupGradeAsync(
                    DefaultWgGrade, Arg.Any<QueryParameters<string>>())
                .Returns(response);

            // Act
            var result = await _controller.GetStaffAllocationTotals(DefaultWgGrade);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── LoadStaffJobsGrid Tests ───────────────────────────────────────────

        #region LoadStaffJobsGrid Tests

        [Fact]
        public async Task LoadStaffJobsGrid_WithBlankStaffId_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadStaffJobsGrid(request, string.Empty);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            await _resourceAllocationService.DidNotReceive()
                .GetPagedStaffJobDetailsByStaffIdAsync(Arg.Any<string>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadStaffJobsGrid_WithValidStaffId_ReturnsPopulatedGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = BuildJobDetailList();
            var response = ApiResponseDto<List<ResourceStaffJobDetailDto>>.SuccessResponse(data);

            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _resourceAllocationService.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query)
                .Returns(response);
            _mapper.Map<ResourceStaffJobItem>(Arg.Any<ResourceStaffJobDetailDto>())
                .Returns(new ResourceStaffJobItem());
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadStaffJobsGrid(request, DefaultStaffId);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<ResourceStaffJobItem>>(partial.Model);
            await _resourceAllocationService.Received(1)
                .GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query);
        }

        [Fact]
        public async Task LoadStaffJobsGrid_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Jobs error", Code = "ERR" } };
            var response = ApiResponseDto<List<ResourceStaffJobDetailDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _resourceAllocationService.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query)
                .Returns(response);

            // Act
            var result = await _controller.LoadStaffJobsGrid(request, DefaultStaffId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Jobs error", element.GetProperty("message").GetString());
        }

        #endregion
    }
}
