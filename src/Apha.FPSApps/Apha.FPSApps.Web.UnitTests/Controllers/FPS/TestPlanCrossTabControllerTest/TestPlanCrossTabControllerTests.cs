using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.TestPlanCrossTabControllerTest
{
    public class TestPlanCrossTabControllerTests
    {
        private readonly IMapper _mapper;
        private readonly ITestPlanCrossTabService _service;
        private readonly TestPlanCrossTabController _controller;

        public TestPlanCrossTabControllerTests()
        {
            _mapper     = Substitute.For<IMapper>();
            _service    = Substitute.For<ITestPlanCrossTabService>();
            _controller = new TestPlanCrossTabController(_mapper, _service);
        }

        private void SetupQueryParamMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(c =>
                {
                    var f = c.Arg<PaginationFilter<string>>();
                    return new QueryParameters<string> { Page = f.Page, PageSize = f.PageSize, Filter = f.Filter, SortBy = f.SortBy, Descending = f.Descending };
                });
        }

        private static TestPlanCostBreakdownDto BuildDto(
            List<string>? columns = null,
            List<Dictionary<string, string?>>? rows = null,
            int totalCount = 0, int page = 1, int pageSize = 20)
            => new TestPlanCostBreakdownDto()
            {
                Columns    = columns ?? [],
                Rows       = rows    ?? [],
                TotalCount = totalCount,
                Page       = page,
                PageSize   = pageSize
            };

        // ── Index ─────────────────────────────────────────────────────────────

        #region Index

        [Fact]
        public async Task Index_ReturnsViewWithViewModel()
        {
            // Arrange
            var dto      = BuildDto(["testcode", "shortdescription"], [new() { ["testcode"] = "PT0047" }], 1, 1, 20);
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(dto);

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var view  = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestPlanCrossTabViewModel>(view.Model);
            Assert.NotNull(model.Grid);
        }

        [Fact]
        public async Task Index_GridId_IsCorrect()
        {
            // Arrange
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(BuildDto());

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var model = Assert.IsType<TestPlanCrossTabViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("testPlanCrossTabGrid", model.Grid.GridId);
        }

        [Fact]
        public async Task Index_GridContainsRows()
        {
            // Arrange
            var rows = new List<Dictionary<string, string?>>
            {
                new() { ["testcode"] = "PT0047", ["shortdescription"] = "EVA serology" },
                new() { ["testcode"] = "PT0049", ["shortdescription"] = "Bact test"    }
            };
            var dto      = BuildDto(["testcode", "shortdescription"], rows, 2, 1, 20);
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(dto);

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var model = Assert.IsType<TestPlanCrossTabViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(2, model.Grid.Data.Count);
        }

        [Fact]
        public async Task Index_GridContainsColumns()
        {
            // Arrange
            var dto      = BuildDto(["testcode", "shortdescription", "Jan", "Feb"], [], 0, 1, 20);
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(dto);

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var model = Assert.IsType<TestPlanCrossTabViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(4, model.Grid.Columns.Count);
        }

        [Fact]
        public async Task Index_WhenServiceReturnsEmpty_GridHasNoRows()
        {
            // Arrange
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(BuildDto());

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var model = Assert.IsType<TestPlanCrossTabViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Empty(model.Grid.Data);
        }

        [Fact]
        public async Task Index_WhenServiceFails_GridHasNoRows()
        {
            // Arrange
            var errors   = new List<ApiErrorDto> { new() { Code = "API_ERROR" } };
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.FailureResponse(errors, new ApiMetaDto());

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var model = Assert.IsType<TestPlanCrossTabViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Empty(model.Grid.Data);
        }

        [Fact]
        public async Task Index_WhenServiceFails_GridHasNoColumns()
        {
            // Arrange
            var errors   = new List<ApiErrorDto> { new() { Code = "API_ERROR" } };
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.FailureResponse(errors, new ApiMetaDto());

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var model = Assert.IsType<TestPlanCrossTabViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Empty(model.Grid.Columns);
        }

        [Fact]
        public async Task Index_GridConfig_HasCorrectProperties()
        {
            // Arrange
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(BuildDto());

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var grid = Assert.IsType<TestPlanCrossTabViewModel>(Assert.IsType<ViewResult>(result).Model).Grid;
            Assert.Equal("testPlanCrossTabGrid",               grid.GridId);
            Assert.Equal("testcode",                           grid.KeyProperty);
            Assert.False(grid.AllowAdd);
            Assert.False(grid.AllowEdit);
            Assert.False(grid.AllowDelete);
            Assert.True(grid.ShowPagination);
            Assert.Equal("/FPS/TestPlanCrossTab/LoadGrid",     grid.BindGridUrl);
        }

        [Fact]
        public async Task Index_FilterableColumns_OnlyTestcodeAndShortdescription()
        {
            // Arrange
            var dto      = BuildDto(["testcode", "shortdescription", "Jan", "Feb"], [], 0, 1, 20);
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(dto);

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var grid      = Assert.IsType<TestPlanCrossTabViewModel>(Assert.IsType<ViewResult>(result).Model).Grid;
            var filterable = grid.Columns.Where(c => c.IsFilterable).Select(c => c.PropertyName).ToList();
            Assert.Contains("testcode",          filterable);
            Assert.Contains("shortdescription",  filterable);
            Assert.DoesNotContain("Jan",         filterable);
            Assert.DoesNotContain("Feb",         filterable);
        }

        [Fact]
        public async Task Index_PaginationPopulated_FromResponse()
        {
            // Arrange
            var dto      = BuildDto(["testcode"], [], 500, 1, 20);
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(dto);

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var grid = Assert.IsType<TestPlanCrossTabViewModel>(Assert.IsType<ViewResult>(result).Model).Grid;
            Assert.Equal(500, grid.Pagination.TotalRecords);
            Assert.Equal(1,   grid.Pagination.PageNumber);
            Assert.Equal(20,  grid.Pagination.PageSize);
        }

        #endregion

        // ── LoadGrid ──────────────────────────────────────────────────────────

        #region LoadGrid

        [Fact]
        public async Task LoadGrid_ReturnsPartialView()
        {
            // Arrange
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 20, Filter = "{}" };
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(BuildDto());

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadGrid_ReturnsDataGridModel()
        {
            // Arrange
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 20, Filter = "{}" };
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(BuildDto());

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(partial.Model);
        }

        [Fact]
        public async Task LoadGrid_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 20 };
            _controller.ModelState.AddModelError("Page", "Invalid");

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadGrid_WithData_ReturnsMappedRows()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 20, Filter = "{}" };
            var rows    = new List<Dictionary<string, string?>>
            {
                new() { ["testcode"] = "PT0047" },
                new() { ["testcode"] = "PT0049" }
            };
            var dto      = BuildDto(["testcode"], rows, 2, 1, 20);
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(dto);

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(partial.Model);
            Assert.Equal(2, grid.Data.Count);
        }

        [Fact]
        public async Task LoadGrid_EmptyData_ReturnsPartialViewWithNoRows()
        {
            // Arrange
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 20, Filter = "{}" };
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(BuildDto());

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result  = await _controller.LoadGrid(request);
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(partial.Model);

            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadGrid_WhenServiceFails_GridHasNoRows()
        {
            // Arrange
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 20, Filter = "{}" };
            var errors   = new List<ApiErrorDto> { new() { Code = "API_ERROR" } };
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.FailureResponse(errors, new ApiMetaDto());

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result  = await _controller.LoadGrid(request);
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(partial.Model);

            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadGrid_WithFilter_PassesFilterToService()
        {
            // Arrange
            var filter   = "{\"testcode\":\"PT\"}";
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 20, Filter = filter };
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(BuildDto());

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            await _controller.LoadGrid(request);

            // Assert
            await _service.Received(1).GetPagedTestPlanCrossTabAsync(
                Arg.Is<QueryParameters<string>>(q => q.Filter == filter));
        }

        [Fact]
        public async Task LoadGrid_NullFilter_TreatedAsEmptyDictionary()
        {
            // Arrange
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 20, Filter = null };
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(BuildDto());

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task LoadGrid_PaginationPopulated_WhenResponseHasPagination()
        {
            // Arrange
            var request  = new PaginationFilter<string> { Page = 2, PageSize = 10, Filter = "{}" };
            var dto      = BuildDto(["testcode"], [], 4030, 2, 10);
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(dto);

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result  = await _controller.LoadGrid(request);
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(partial.Model);

            Assert.Equal(4030, grid.Pagination.TotalRecords);
            Assert.Equal(2,    grid.Pagination.PageNumber);
            Assert.Equal(10,   grid.Pagination.PageSize);
        }

        [Fact]
        public async Task LoadGrid_SortingParams_PassedToService()
        {
            // Arrange
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 20, Filter = "{}", SortBy = "testcode", Descending = true };
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(BuildDto());

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result  = await _controller.LoadGrid(request);
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(partial.Model);

            Assert.Equal("testcode", grid.Pagination.SortColumn);
            Assert.True(grid.Pagination.SortDirection);
        }

        [Fact]
        public async Task LoadGrid_MalformedFilter_TreatedAsEmpty()
        {
            // Arrange
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 20, Filter = "not_valid_json" };
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(BuildDto());

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result = await _controller.LoadGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(partial.Model);
            Assert.Empty(grid.CurrentFilters ?? []);
        }

        [Fact]
        public async Task LoadGrid_WithValidFilter_PopulatesCurrentFilters()
        {
            // Arrange
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 20, Filter = "{\"testcode\":\"PT0047\"}" };
            var response = ApiResponseDto<TestPlanCostBreakdownDto>.SuccessResponse(BuildDto());

            _service.GetPagedTestPlanCrossTabAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            SetupQueryParamMapper();

            // Act
            var result  = await _controller.LoadGrid(request);
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(partial.Model);

            Assert.NotNull(grid.CurrentFilters);
            Assert.True(grid.CurrentFilters.ContainsKey("testcode"));
            Assert.Equal("PT0047", grid.CurrentFilters["testcode"]);
        }

        #endregion
    }
}
