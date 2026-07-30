using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Controllers;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Security.Claims;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PIMS.Controllers.YearlyFinancialDataControllerTest
{
    public class YearlyFinancialDataControllerTests
    {
        private readonly IMapper                     _mapper;
        private readonly IYearlyFinancialDataService _service;
        private readonly IProjectListService         _projectListService;
        private readonly IProjectDetailsService      _projectDetailsService;
        private readonly YearlyFinancialDataController _controller;

        public YearlyFinancialDataControllerTests()
        {
            _mapper                = Substitute.For<IMapper>();
            _service               = Substitute.For<IYearlyFinancialDataService>();
            _projectListService    = Substitute.For<IProjectListService>();
            _projectDetailsService = Substitute.For<IProjectDetailsService>();

            _controller = new YearlyFinancialDataController(
                _mapper,
                _service,
                _projectListService,
                _projectDetailsService);

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Substitute.For<ITempDataProvider>());
        }

        // ── helpers ──────────────────────────────────────────────────────

        private static PaginationFilter<string> DefaultRequest()
            => new() { Filter = "{}", Page = 1, PageSize = 10 };

        private static YearlyFinancialDataDto SampleDto(short year = 2024, string project = "PP001")
            => new() { Year = year, Project = project };

        private static YearlyFinancialDataItem SampleItem(short year = 2024, string project = "PP001")
            => new() { Year = year, Project = project };

        private static T? GetJsonProperty<T>(JsonResult result, string propertyName)
        {
            string serialized = JsonSerializer.Serialize(result.Value);
            using JsonDocument doc = JsonDocument.Parse(serialized);
            return doc.RootElement.TryGetProperty(propertyName, out JsonElement element)
                ? JsonSerializer.Deserialize<T>(element.GetRawText())
                : default;
        }

        private void SetupDefaultIndexMocks(List<ProjectListMilestoneDto>? projects = null)
        {
            _projectListService.GetAllProjectsForMilestoneAsync()
                .Returns(new ApiResponseDto<List<ProjectListMilestoneDto>>
                {
                    Success = true,
                    Data    = projects ?? [new ProjectListMilestoneDto { Parentproject = "PP001" }]
                });

            _projectDetailsService.GetPimsDetailAsync(Arg.Any<string>())
                .Returns(new ApiResponseDto<ProjectDetailDto>
                {
                    Success = true,
                    Data    = new ProjectDetailDto
                    {
                        StartDate = new DateTime(2023, 4, 1),
                        EndDate   = new DateTime(2024, 3, 31)
                    }
                });

            _service.GetSettingValueByIdAsync(Arg.Any<string>())
                .Returns(callInfo =>
                {
                    var id = callInfo.Arg<string>();
                    return new ApiResponseDto<string>
                    {
                        Success = true,
                        Data = id == "DaysInYear" ? "219" : "7.2"
                    };
                });
        }

        private void SetAuthenticatedUser(string userName)
        {
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, userName)
                ], "TestAuth"))
            };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_InitializesController()
        {
            var controller = new YearlyFinancialDataController(
                _mapper, _service, _projectListService, _projectDetailsService);
            Assert.NotNull(controller);
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            SetupDefaultIndexMocks();
            var result = await _controller.Index(null);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_ReturnsYearlyFinancialDataViewModel()
        {
            SetupDefaultIndexMocks();
            var result = await _controller.Index(null);
            Assert.IsType<YearlyFinancialDataViewModel>(Assert.IsType<ViewResult>(result).Model);
        }

        [Fact]
        public async Task Index_CallsGetAllProjectsForMilestoneAsync_Once()
        {
            SetupDefaultIndexMocks();
            await _controller.Index(null);
            await _projectListService.Received(1).GetAllProjectsForMilestoneAsync();
        }

        [Fact]
        public async Task Index_WithNoProject_LeavesSelectedProjectEmpty()
        {
            SetupDefaultIndexMocks(projects: [new ProjectListMilestoneDto { Parentproject = "PP001" }]);
            var result = await _controller.Index(null);
            var model  = Assert.IsType<YearlyFinancialDataViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(string.Empty, model.SelectedProject);
        }

        [Fact]
        public async Task Index_WithExplicitProject_UsesProvidedProject()
        {
            SetupDefaultIndexMocks();
            var result = await _controller.Index("PP999");
            var model  = Assert.IsType<YearlyFinancialDataViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("PP999", model.SelectedProject);
        }

        [Fact]
        public async Task Index_ProjectList_ContainsServiceReturnedProjects()
        {
            SetupDefaultIndexMocks(projects:
            [
                new ProjectListMilestoneDto { Parentproject = "PP001" },
                new ProjectListMilestoneDto { Parentproject = "PP002" }
            ]);
            var result = await _controller.Index(null);
            var model  = Assert.IsType<YearlyFinancialDataViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Contains(model.ProjectList, o => o.Value == "PP001");
            Assert.Contains(model.ProjectList, o => o.Value == "PP002");
        }

        [Fact]
        public async Task Index_CostCenterListGrid_IsIncludedInViewModel()
        {
            SetupDefaultIndexMocks();
            var result = await _controller.Index(null);
            var model  = Assert.IsType<YearlyFinancialDataViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.NotNull(model.CostCenterListGrid);
        }

        [Fact]
        public async Task Index_WhenProjectListDataIsNull_ProjectListIsEmpty()
        {
            _projectListService.GetAllProjectsForMilestoneAsync()
                .Returns(new ApiResponseDto<List<ProjectListMilestoneDto>> { Success = true, Data = null });
            _projectDetailsService.GetPimsDetailAsync(Arg.Any<string>())
                .Returns(new ApiResponseDto<ProjectDetailDto> { Success = false, Data = null });
            _service.GetSettingValueByIdAsync(Arg.Any<string>())
                .Returns(new ApiResponseDto<string> { Success = true, Data = "7.2" });

            var result = await _controller.Index(null);
            var model  = Assert.IsType<YearlyFinancialDataViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Empty(model.ProjectList);
        }

        [Fact]
        public async Task Index_WhenProjectListServiceThrowsException_PropagatesException()
        {
            _projectListService.GetAllProjectsForMilestoneAsync()
                .ThrowsAsync(new Exception("Service unavailable"));
            await Assert.ThrowsAsync<Exception>(() => _controller.Index(null));
        }

        #endregion

        #region LoadYearlyFinancialDataGrid Tests

        [Fact]
        public async Task LoadYearlyFinancialDataGrid_WithInvalidModelState_ReturnsJsonResult()
        {
            _controller.ModelState.AddModelError("key", "error");
            var result = await _controller.LoadYearlyFinancialDataGrid(DefaultRequest(), null);
            Assert.IsType<JsonResult>(result);
        }

        [Fact]
        public async Task LoadYearlyFinancialDataGrid_WithInvalidModelState_ReturnsFalseSuccess()
        {
            _controller.ModelState.AddModelError("key", "error");
            var jsonResult = Assert.IsType<JsonResult>(
                await _controller.LoadYearlyFinancialDataGrid(DefaultRequest(), null));
            Assert.False(GetJsonProperty<bool>(jsonResult, "success"));
        }

        [Fact]
        public async Task LoadYearlyFinancialDataGrid_WithInvalidModelState_DoesNotCallService()
        {
            _controller.ModelState.AddModelError("key", "error");
            await _controller.LoadYearlyFinancialDataGrid(DefaultRequest(), "PP001");
            await _service.DidNotReceive().GetAllAsync(Arg.Any<string>(), Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadYearlyFinancialDataGrid_WithValidModelState_ReturnsDataGridPartialView()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _service.GetAllAsync(Arg.Any<string>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<YearlyFinancialDataDto>> { Success = true, Data = [] });
            _mapper.Map<List<YearlyFinancialDataItem>>(Arg.Any<List<YearlyFinancialDataDto>>()).Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var result = Assert.IsType<PartialViewResult>(
                await _controller.LoadYearlyFinancialDataGrid(DefaultRequest(), "PP001"));
            Assert.Equal("_DataGrid", result.ViewName);
        }

        [Fact]
        public async Task LoadYearlyFinancialDataGrid_CallsGetAllAsync_Once()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _service.GetAllAsync(Arg.Any<string>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<YearlyFinancialDataDto>> { Success = true, Data = [] });
            _mapper.Map<List<YearlyFinancialDataItem>>(Arg.Any<List<YearlyFinancialDataDto>>()).Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            await _controller.LoadYearlyFinancialDataGrid(DefaultRequest(), "PP001");
            await _service.Received(1).GetAllAsync("PP001", Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadYearlyFinancialDataGrid_WhenServiceReturnsNullData_ReturnsEmptyGrid()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _service.GetAllAsync(Arg.Any<string>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<YearlyFinancialDataDto>> { Success = true, Data = null });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            var result = Assert.IsType<PartialViewResult>(
                await _controller.LoadYearlyFinancialDataGrid(DefaultRequest(), "PP001"));
            DataGridConfig<YearlyFinancialDataItem> grid =
                Assert.IsType<DataGridConfig<YearlyFinancialDataItem>>(result.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadYearlyFinancialDataGrid_WhenServiceReturnsMappedItems_ReturnsGridWithData()
        {
            var items = new List<YearlyFinancialDataItem> { SampleItem() };
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _service.GetAllAsync(Arg.Any<string>(), Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<YearlyFinancialDataDto>>
                {
                    Success = true,
                    Data    = [SampleDto()],
                    Pagination = new PaginationDto { TotalRecords = 1 }
                });
            _mapper.Map<List<YearlyFinancialDataItem>>(Arg.Any<List<YearlyFinancialDataDto>>()).Returns(items);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel { TotalRecords = 1 });

            var result = Assert.IsType<PartialViewResult>(
                await _controller.LoadYearlyFinancialDataGrid(DefaultRequest(), "PP001"));
            DataGridConfig<YearlyFinancialDataItem> grid =
                Assert.IsType<DataGridConfig<YearlyFinancialDataItem>>(result.Model);
            Assert.Single(grid.Data);
        }

        #endregion

        #region Create (GET) Tests

        [Fact]
        public async Task Create_Get_ReturnsPartialViewWithEmptyItem()
        {
            var result = _controller.Create((string?)null);
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.IsType<YearlyFinancialDataItem>(partialResult.Model);
        }

        [Fact]
        public async Task Create_Get_DoesNotCallService()
        {
            _controller.Create((string?)null);
            await _service.DidNotReceive().CreateAsync(Arg.Any<YearlyFinancialDataDto>());
        }

        [Fact]
        public void Create_Get_WithAuthenticatedUser_SetsCurrentCostingUserInViewData()
        {
            SetAuthenticatedUser("test.user");

            _controller.Create("PP001");

            Assert.Equal("test.user", _controller.ViewData["CurrentCostingUser"]);
        }

        #endregion

        #region Create (POST) Tests

        [Fact]
        public async Task Create_Post_WithValidDto_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto = SampleDto();
            _service.CreateAsync(dto)
                .Returns(new ApiResponseDto<YearlyFinancialDataDto> { Success = true, Data = dto });

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.True(GetJsonProperty<bool>(jsonResult, "success"));
            await _service.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task Create_Post_WithNullDto_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.Create((YearlyFinancialDataDto)null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.False(GetJsonProperty<bool>(jsonResult, "success"));
            await _service.DidNotReceive().CreateAsync(Arg.Any<YearlyFinancialDataDto>());
        }

        [Fact]
        public async Task Create_Post_WhenServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = SampleDto();
            _service.CreateAsync(dto)
                .Returns(new ApiResponseDto<YearlyFinancialDataDto>
                {
                    Success = false,
                    Errors  = [new ApiErrorDto { Message = "Duplicate key", Code = "DUPLICATE" }]
                });

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.False(GetJsonProperty<bool>(jsonResult, "success"));
        }

        #endregion

        #region Edit (GET) Tests

        [Fact]
        public async Task Edit_Get_WithValidKey_ReturnsPartialViewWithPopulatedItem()
        {
            // Arrange
            var dto  = SampleDto();
            var item = SampleItem();
            _service.GetByKeyAsync((short)2024, "PP001")
                .Returns(new ApiResponseDto<YearlyFinancialDataDto> { Success = true, Data = dto });
            _mapper.Map<YearlyFinancialDataItem>(dto).Returns(item);

            // Act
            var result = await _controller.Edit((short)2024, "PP001");

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal(item, partialResult.Model);
            await _service.Received(1).GetByKeyAsync((short)2024, "PP001");
            _mapper.Received(1).Map<YearlyFinancialDataItem>(dto);
        }

        [Fact]
        public async Task Edit_Get_WhenServiceReturnsFailure_ReturnsNotFound()
        {
            // Arrange
            _service.GetByKeyAsync(Arg.Any<short>(), Arg.Any<string>())
                .Returns(new ApiResponseDto<YearlyFinancialDataDto> { Success = false, Data = null });

            // Act
            var result = await _controller.Edit((short)9999, "UNKNOWN");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            _mapper.DidNotReceive().Map<YearlyFinancialDataItem>(Arg.Any<YearlyFinancialDataDto>());
        }

        [Fact]
        public async Task Edit_Get_WhenServiceReturnsNullData_ReturnsNotFound()
        {
            // Arrange
            _service.GetByKeyAsync(Arg.Any<short>(), Arg.Any<string>())
                .Returns(new ApiResponseDto<YearlyFinancialDataDto> { Success = true, Data = null });

            // Act
            var result = await _controller.Edit((short)2024, "PP001");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Edit_Get_WithAuthenticatedUser_SetsCurrentCostingUserInViewData()
        {
            SetAuthenticatedUser("test.user");
            var dto = SampleDto();
            var item = SampleItem();

            _service.GetByKeyAsync((short)2024, "PP001")
                .Returns(new ApiResponseDto<YearlyFinancialDataDto> { Success = true, Data = dto });
            _mapper.Map<YearlyFinancialDataItem>(dto).Returns(item);

            await _controller.Edit((short)2024, "PP001");

            Assert.Equal("test.user", _controller.ViewData["CurrentCostingUser"]);
        }

        #endregion

        #region Edit (POST) Tests

        [Fact]
        public async Task Edit_Post_WithValidDto_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto = SampleDto();
            _service.UpdateAsync((short)2024, "PP001", dto)
                .Returns(new ApiResponseDto<YearlyFinancialDataDto> { Success = true, Data = dto });

            // Act
            var result = await _controller.Edit((short)2024, "PP001", dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.True(GetJsonProperty<bool>(jsonResult, "success"));
            await _service.Received(1).UpdateAsync((short)2024, "PP001", dto);
        }

        [Fact]
        public async Task Edit_Post_WithNullDto_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.Edit((short)2024, "PP001", null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.False(GetJsonProperty<bool>(jsonResult, "success"));
            await _service.DidNotReceive().UpdateAsync(Arg.Any<short>(), Arg.Any<string>(), Arg.Any<YearlyFinancialDataDto>());
        }

        [Fact]
        public async Task Edit_Post_WhenServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = SampleDto();
            _service.UpdateAsync((short)2024, "PP001", dto)
                .Returns(new ApiResponseDto<YearlyFinancialDataDto>
                {
                    Success = false,
                    Errors  = [new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" }]
                });

            // Act
            var result = await _controller.Edit((short)2024, "PP001", dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.False(GetJsonProperty<bool>(jsonResult, "success"));
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidKey_WhenServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            _service.DeleteAsync((short)2024, "PP001")
                .Returns(new ApiResponseDto<object> { Success = true, Data = new object() });

            // Act
            var result = await _controller.Delete((short)2024, "PP001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.True(GetJsonProperty<bool>(jsonResult, "success"));
            await _service.Received(1).DeleteAsync((short)2024, "PP001");
        }

        [Fact]
        public async Task Delete_WhenServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _service.DeleteAsync(Arg.Any<short>(), Arg.Any<string>())
                .Returns(new ApiResponseDto<object>
                {
                    Success = false,
                    Errors  = [new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" }]
                });

            // Act
            var result = await _controller.Delete((short)9999, "UNKNOWN");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.False(GetJsonProperty<bool>(jsonResult, "success"));
        }

        #endregion

        #region GetPactCosts Tests

        [Fact]
        public async Task GetPactCosts_WhenServiceReturnsData_ReturnsJsonWithSuccessTrueAndMappedItem()
        {
            // Arrange
            var dto  = new PactProjectYearCostsDto { Project = "PP001", Year = 2024 };
            var item = new PactCostsItem();
            _service.GetPactCostsAsync("PP001", (short)2024)
                .Returns(new ApiResponseDto<PactProjectYearCostsDto> { Success = true, Data = dto });
            _mapper.Map<PactCostsItem>(dto).Returns(item);

            // Act
            var result = await _controller.GetPactCosts("PP001", (short)2024);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.True(GetJsonProperty<bool>(jsonResult, "success"));
            await _service.Received(1).GetPactCostsAsync("PP001", (short)2024);
            _mapper.Received(1).Map<PactCostsItem>(dto);
        }

        [Fact]
        public async Task GetPactCosts_WhenServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _service.GetPactCostsAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(new ApiResponseDto<PactProjectYearCostsDto>
                {
                    Success = false,
                    Data    = null
                });

            // Act
            var result = await _controller.GetPactCosts("PP001", (short)2024);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.False(GetJsonProperty<bool>(jsonResult, "success"));
        }

        [Fact]
        public async Task GetPactCosts_WhenDataIsNull_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _service.GetPactCostsAsync(Arg.Any<string>(), Arg.Any<short>())
                .Returns(new ApiResponseDto<PactProjectYearCostsDto> { Success = true, Data = null });

            // Act
            var result = await _controller.GetPactCosts("PP001", (short)2024);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.False(GetJsonProperty<bool>(jsonResult, "success"));
        }

        #endregion

        #region GetProjectDates Tests

        [Fact]
        public async Task GetProjectDates_WithNullProject_ReturnsJsonWithSuccessFalse()
        {
            var result = await _controller.GetProjectDates(null!);
            var json   = Assert.IsType<JsonResult>(result);
            Assert.False(GetJsonProperty<bool>(json, "success"));
            await _projectDetailsService.DidNotReceive().GetPimsDetailAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetProjectDates_WithWhitespaceProject_ReturnsJsonWithSuccessFalse()
        {
            var result = await _controller.GetProjectDates("   ");
            var json   = Assert.IsType<JsonResult>(result);
            Assert.False(GetJsonProperty<bool>(json, "success"));
            await _projectDetailsService.DidNotReceive().GetPimsDetailAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetProjectDates_WhenServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            _projectDetailsService.GetPimsDetailAsync("PP001")
                .Returns(new ApiResponseDto<ProjectDetailDto> { Success = false, Data = null });

            var result = await _controller.GetProjectDates("PP001");
            var json   = Assert.IsType<JsonResult>(result);
            Assert.False(GetJsonProperty<bool>(json, "success"));
        }

        [Fact]
        public async Task GetProjectDates_WhenServiceReturnsNullData_ReturnsJsonWithSuccessFalse()
        {
            _projectDetailsService.GetPimsDetailAsync("PP001")
                .Returns(new ApiResponseDto<ProjectDetailDto> { Success = true, Data = null });

            var result = await _controller.GetProjectDates("PP001");
            var json   = Assert.IsType<JsonResult>(result);
            Assert.False(GetJsonProperty<bool>(json, "success"));
        }

        [Fact]
        public async Task GetProjectDates_WithValidProject_CallsGetPimsDetailAsyncOnce()
        {
            _projectDetailsService.GetPimsDetailAsync("PP001")
                .Returns(new ApiResponseDto<ProjectDetailDto>
                {
                    Success = true,
                    Data    = new ProjectDetailDto { StartDate = new DateTime(2024, 4, 1) }
                });

            await _controller.GetProjectDates("PP001");

            await _projectDetailsService.Received(1).GetPimsDetailAsync("PP001");
        }

        [Fact]
        public async Task GetProjectDates_WithStartAndRevisedEndDate_ReturnsFormattedDates()
        {
            // RevisedEndDate takes priority over EndDate
            _projectDetailsService.GetPimsDetailAsync("PP001")
                .Returns(new ApiResponseDto<ProjectDetailDto>
                {
                    Success = true,
                    Data    = new ProjectDetailDto
                    {
                        StartDate       = new DateTime(2024, 4, 1),
                        EndDate         = new DateTime(2025, 3, 31),
                        RevisedEndDate  = new DateTime(2025, 6, 30)
                    }
                });

            var result = await _controller.GetProjectDates("PP001");
            var json   = Assert.IsType<JsonResult>(result);

            Assert.True(GetJsonProperty<bool>(json, "success"));
            Assert.Equal("01/04/2024", GetJsonProperty<string>(json, "startDate"));
            Assert.Equal("30/06/2025", GetJsonProperty<string>(json, "endDate"));   // RevisedEndDate used
        }

        [Fact]
        public async Task GetProjectDates_WithEndDateButNoRevisedEndDate_UsesEndDate()
        {
            _projectDetailsService.GetPimsDetailAsync("PP001")
                .Returns(new ApiResponseDto<ProjectDetailDto>
                {
                    Success = true,
                    Data    = new ProjectDetailDto
                    {
                        StartDate      = new DateTime(2024, 4, 1),
                        EndDate        = new DateTime(2025, 3, 31),
                        RevisedEndDate = null
                    }
                });

            var result = await _controller.GetProjectDates("PP001");
            var json   = Assert.IsType<JsonResult>(result);

            Assert.True(GetJsonProperty<bool>(json, "success"));
            Assert.Equal("31/03/2025", GetJsonProperty<string>(json, "endDate"));  // EndDate fallback
        }

        [Fact]
        public async Task GetProjectDates_WithNoDates_ReturnsEmptyStringDates()
        {
            _projectDetailsService.GetPimsDetailAsync("PP001")
                .Returns(new ApiResponseDto<ProjectDetailDto>
                {
                    Success = true,
                    Data    = new ProjectDetailDto { StartDate = null, EndDate = null, RevisedEndDate = null }
                });

            var result = await _controller.GetProjectDates("PP001");
            var json   = Assert.IsType<JsonResult>(result);

            Assert.True(GetJsonProperty<bool>(json, "success"));
            Assert.Equal(string.Empty, GetJsonProperty<string>(json, "startDate"));
            Assert.Equal(string.Empty, GetJsonProperty<string>(json, "endDate"));
        }

        #endregion

        #region CostedBy Normalisation Tests

        [Fact]
        public async Task Create_Post_WithEmailCostedBy_StripsEmailDomainBeforeCallingService()
        {
            // Arrange
            var dto = SampleDto();
            dto.CostedBy = "user.name@defradev.onmicrosoft.com";

            _service.CreateAsync(Arg.Any<YearlyFinancialDataDto>())
                .Returns(new ApiResponseDto<YearlyFinancialDataDto> { Success = true, Data = dto });

            // Act
            await _controller.Create(dto);

            // Assert — CostedBy is trimmed at @
            Assert.Equal("user.name", dto.CostedBy);
        }

        [Fact]
        public async Task Create_Post_WithNonEmailCostedBy_LeavesCostedByUnchanged()
        {
            // Arrange
            var dto = SampleDto();
            dto.CostedBy = "plainuser";

            _service.CreateAsync(Arg.Any<YearlyFinancialDataDto>())
                .Returns(new ApiResponseDto<YearlyFinancialDataDto> { Success = true, Data = dto });

            // Act
            await _controller.Create(dto);

            // Assert — no @ present, CostedBy unchanged
            Assert.Equal("plainuser", dto.CostedBy);
        }

        [Fact]
        public async Task Edit_Post_WithEmailCostedBy_StripsEmailDomainBeforeCallingService()
        {
            // Arrange
            var dto = SampleDto();
            dto.CostedBy = "another.user@defradev.onmicrosoft.com";

            _service.UpdateAsync((short)2024, "PP001", Arg.Any<YearlyFinancialDataDto>())
                .Returns(new ApiResponseDto<YearlyFinancialDataDto> { Success = true, Data = dto });

            // Act
            await _controller.Edit((short)2024, "PP001", dto);

            // Assert — CostedBy is trimmed at @
            Assert.Equal("another.user", dto.CostedBy);
        }

        [Fact]
        public async Task Edit_Post_WithNonEmailCostedBy_LeavesCostedByUnchanged()
        {
            // Arrange
            var dto = SampleDto();
            dto.CostedBy = "plainuser";

            _service.UpdateAsync((short)2024, "PP001", Arg.Any<YearlyFinancialDataDto>())
                .Returns(new ApiResponseDto<YearlyFinancialDataDto> { Success = true, Data = dto });

            // Act
            await _controller.Edit((short)2024, "PP001", dto);

            // Assert — no @ present, CostedBy unchanged
            Assert.Equal("plainuser", dto.CostedBy);
        }

        #endregion

        #region GetRequiredDoubleSettingAsync Error Branch Tests

        [Fact]
        public async Task Index_WhenSettingServiceReturnsFailed_ThrowsInvalidOperationException()
        {
            _projectListService.GetAllProjectsForMilestoneAsync()
                .Returns(new ApiResponseDto<List<ProjectListMilestoneDto>> { Success = true, Data = [] });

            _service.GetSettingValueByIdAsync(Arg.Any<string>())
                .Returns(new ApiResponseDto<string> { Success = false, Data = null });

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Index(null));
        }

        [Fact]
        public async Task Index_WhenSettingValueIsNonNumeric_ThrowsInvalidOperationException()
        {
            _projectListService.GetAllProjectsForMilestoneAsync()
                .Returns(new ApiResponseDto<List<ProjectListMilestoneDto>> { Success = true, Data = [] });

            _service.GetSettingValueByIdAsync(Arg.Any<string>())
                .Returns(new ApiResponseDto<string> { Success = true, Data = "not-a-number" });

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Index(null));
        }

        [Fact]
        public async Task Index_WhenSettingValueIsZero_ThrowsInvalidOperationException()
        {
            _projectListService.GetAllProjectsForMilestoneAsync()
                .Returns(new ApiResponseDto<List<ProjectListMilestoneDto>> { Success = true, Data = [] });

            _service.GetSettingValueByIdAsync(Arg.Any<string>())
                .Returns(new ApiResponseDto<string> { Success = true, Data = "0" });

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Index(null));
        }

        [Fact]
        public async Task Index_WhenSettingValueIsNegative_ThrowsInvalidOperationException()
        {
            _projectListService.GetAllProjectsForMilestoneAsync()
                .Returns(new ApiResponseDto<List<ProjectListMilestoneDto>> { Success = true, Data = [] });

            _service.GetSettingValueByIdAsync(Arg.Any<string>())
                .Returns(new ApiResponseDto<string> { Success = true, Data = "-1" });

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Index(null));
        }

        #endregion
    }
}
