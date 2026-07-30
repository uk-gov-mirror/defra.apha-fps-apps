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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.TestActualBreakdownControllerTest
{
    public class TestActualBreakdownControllerTests
    {
        private readonly IMapper _mapper;
        private readonly ITestActualBreakdownService _service;
        private readonly TestActualBreakdownController _controller;

        public TestActualBreakdownControllerTests()
        {
            _mapper     = Substitute.For<IMapper>();
            _service    = Substitute.For<ITestActualBreakdownService>();
            _controller = new TestActualBreakdownController(_mapper, _service);
        }

        private void SetupQueryParamMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(c =>
                {
                    var f = c.Arg<PaginationFilter<string>>();
                    return new QueryParameters<string> { Page = f.Page, PageSize = f.PageSize, Filter = f.Filter };
                });
        }

        private void SetupItemMapper(List<TestActualBreakdownDto> dtos, List<TestActualBreakdownItem> items)
            => _mapper.Map<List<TestActualBreakdownItem>>(dtos).Returns(items);

        // ── Index ─────────────────────────────────────────────────────────────

        #region Index

        [Fact]
        public async Task Index_ReturnsViewWithViewModel()
        {
            var dtos     = new List<TestActualBreakdownDto> { new() { TestCode = "PT0047", Buyer = "SV3300" } };
            var items    = new List<TestActualBreakdownItem> { new() { TestCode = "PT0047" } };
            var pagedRes = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse(dtos, new PaginationDto());

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            SetupItemMapper(dtos, items);

            var result = await _controller.Index();

            var view  = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestActualBreakdownViewModel>(view.Model);
            Assert.NotNull(model.Grid);
        }

        [Fact]
        public async Task Index_GridId_IsCorrect()
        {
            var pagedRes = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([], new PaginationDto());

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            _mapper.Map<List<TestActualBreakdownItem>>(Arg.Any<List<TestActualBreakdownDto>>()).Returns([]);

            var result = await _controller.Index();

            var model = Assert.IsType<TestActualBreakdownViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("testActualBreakdownGrid", model.Grid.GridId);
        }

        [Fact]
        public async Task Index_GridContainsMappedRows()
        {
            var dtos = new List<TestActualBreakdownDto>
            {
                new() { TestCode = "PT0047", Buyer = "SV3300", Program = "Viro",  Month = 4, PCPrice = 159.00m, PCCost = 319.00m },
                new() { TestCode = "PT0049", Buyer = "SB4600", Program = "Bact",  Month = 4, PCPrice = 313.00m, PCCost = 313.00m }
            };
            var items = new List<TestActualBreakdownItem>
            {
                new() { TestCode = "PT0047" },
                new() { TestCode = "PT0049" }
            };
            var pagedRes = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse(dtos, new PaginationDto());

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            SetupItemMapper(dtos, items);

            var result = await _controller.Index();

            var model = Assert.IsType<TestActualBreakdownViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(2, model.Grid.Data.Count);
        }

        [Fact]
        public async Task Index_WhenServiceReturnsEmpty_GridHasNoRows()
        {
            var pagedRes = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([], new PaginationDto());

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            _mapper.Map<List<TestActualBreakdownItem>>(Arg.Any<List<TestActualBreakdownDto>>()).Returns([]);

            var result = await _controller.Index();

            var model = Assert.IsType<TestActualBreakdownViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Empty(model.Grid.Data);
        }

        [Fact]
        public async Task Index_WhenServiceFails_GridHasNoRows()
        {
            var errors   = new List<ApiErrorDto> { new() { Code = "API_ERROR" } };
            var pagedRes = ApiResponseDto<List<TestActualBreakdownDto>>.FailureResponse(errors, new ApiMetaDto());

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();

            var result = await _controller.Index();

            var model = Assert.IsType<TestActualBreakdownViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Empty(model.Grid.Data);
        }

        [Fact]
        public async Task Index_GridConfig_HasCorrectProperties()
        {
            var pagedRes = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([], new PaginationDto());

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            _mapper.Map<List<TestActualBreakdownItem>>(Arg.Any<List<TestActualBreakdownDto>>()).Returns([]);

            var result = await _controller.Index();

            var grid = Assert.IsType<TestActualBreakdownViewModel>(Assert.IsType<ViewResult>(result).Model).Grid;
            Assert.Equal("testActualBreakdownGrid",         grid.GridId);
            Assert.Equal("TestCode",                        grid.KeyProperty);
            Assert.False(grid.AllowAdd);
            Assert.False(grid.AllowEdit);
            Assert.False(grid.AllowDelete);
            Assert.True(grid.ShowPagination);
            Assert.Equal("/FPS/TestActualBreakdown/LoadGrid", grid.BindGridUrl);
        }

        [Fact]
        public async Task Index_PaginationData_IsPopulatedFromServiceResponse()
        {
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 4030 };
            var pagedRes   = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([], pagination);

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            _mapper.Map<List<TestActualBreakdownItem>>(Arg.Any<List<TestActualBreakdownDto>>()).Returns([]);

            var result = await _controller.Index();

            var model = Assert.IsType<TestActualBreakdownViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal(4030, model.Grid.Pagination.TotalRecords);
        }

        #endregion

        // ── LoadGrid ──────────────────────────────────────────────────────────

        #region LoadGrid

        [Fact]
        public async Task LoadGrid_ValidRequest_ReturnsPartialView()
        {
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var dtos     = new List<TestActualBreakdownDto> { new() { TestCode = "PT0047" } };
            var items    = new List<TestActualBreakdownItem> { new() { TestCode = "PT0047" } };
            var pagedRes = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse(dtos, new PaginationDto());

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            SetupItemMapper(dtos, items);

            var result = await _controller.LoadGrid(request);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadGrid_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Page", "Required");
            var request = new PaginationFilter<string>();

            var result = await _controller.LoadGrid(request);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadGrid_WithData_ReturnsMappedItems()
        {
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var dtos    = new List<TestActualBreakdownDto>
            {
                new() { TestCode = "PT0047", Buyer = "SV3300" },
                new() { TestCode = "PT0049", Buyer = "SB4600" }
            };
            var items    = new List<TestActualBreakdownItem>
            {
                new() { TestCode = "PT0047" },
                new() { TestCode = "PT0049" }
            };
            var pagedRes = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse(dtos, new PaginationDto());

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            SetupItemMapper(dtos, items);

            var result = await _controller.LoadGrid(request);

            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<TestActualBreakdownItem>>(partial.Model);
            Assert.Equal(2, grid.Data.Count);
        }

        [Fact]
        public async Task LoadGrid_EmptyData_ReturnsPartialViewWithNoRows()
        {
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var pagedRes = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([], new PaginationDto());

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            _mapper.Map<List<TestActualBreakdownItem>>(Arg.Any<List<TestActualBreakdownDto>>()).Returns([]);

            var result  = await _controller.LoadGrid(request);
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<TestActualBreakdownItem>>(partial.Model);

            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadGrid_WhenServiceFails_GridHasNoRows()
        {
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var errors   = new List<ApiErrorDto> { new() { Code = "API_ERROR" } };
            var pagedRes = ApiResponseDto<List<TestActualBreakdownDto>>.FailureResponse(errors, new ApiMetaDto());

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();

            var result  = await _controller.LoadGrid(request);
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<TestActualBreakdownItem>>(partial.Model);

            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadGrid_WithFilter_PassesFilterToService()
        {
            var filter   = "{\"TestCode\":\"PT\"}";
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = filter };
            var pagedRes = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([], new PaginationDto());

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            _mapper.Map<List<TestActualBreakdownItem>>(Arg.Any<List<TestActualBreakdownDto>>()).Returns([]);

            await _controller.LoadGrid(request);

            await _service.Received(1).GetActualsTestsWithPlannedDataByWorkgroupAsync(
                Arg.Is<QueryParameters<string>>(q => q.Filter == filter));
        }

        [Fact]
        public async Task LoadGrid_NullFilter_TreatedAsEmptyDictionary()
        {
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = null };
            var pagedRes = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([], new PaginationDto());

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            _mapper.Map<List<TestActualBreakdownItem>>(Arg.Any<List<TestActualBreakdownDto>>()).Returns([]);

            var result = await _controller.LoadGrid(request);

            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task LoadGrid_PaginationPopulated_WhenResponseHasPagination()
        {
            var request    = new PaginationFilter<string> { Page = 2, PageSize = 10, Filter = "{}" };
            var pagination = new PaginationDto { PageNumber = 2, PageSize = 10, TotalRecords = 4030 };
            var pagedRes   = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([], pagination);

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            _mapper.Map<List<TestActualBreakdownItem>>(Arg.Any<List<TestActualBreakdownDto>>()).Returns([]);

            var result  = await _controller.LoadGrid(request);
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<TestActualBreakdownItem>>(partial.Model);

            Assert.Equal(4030, grid.Pagination.TotalRecords);
            Assert.Equal(2,    grid.Pagination.PageNumber);
            Assert.Equal(10,   grid.Pagination.PageSize);
        }

        [Fact]
        public async Task LoadGrid_SortingParams_PassedToService()
        {
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}", SortBy = "buyer", Descending = true };
            var pagedRes = ApiResponseDto<List<TestActualBreakdownDto>>.SuccessResponse([], new PaginationDto());

            _service.GetActualsTestsWithPlannedDataByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            _mapper.Map<List<TestActualBreakdownItem>>(Arg.Any<List<TestActualBreakdownDto>>()).Returns([]);

            var result = await _controller.LoadGrid(request);

            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<TestActualBreakdownItem>>(partial.Model);
            Assert.Equal("buyer", grid.Pagination.SortColumn);
            Assert.True(grid.Pagination.SortDirection);
        }

        #endregion
    }
}
