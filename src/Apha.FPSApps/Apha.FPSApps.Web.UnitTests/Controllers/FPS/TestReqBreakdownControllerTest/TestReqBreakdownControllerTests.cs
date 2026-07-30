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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.TestReqBreakdownControllerTest
{
    public class TestReqBreakdownControllerTests
    {
        private readonly IMapper _mapper;
        private readonly ITestReqBreakdownService _service;
        private readonly TestReqBreakdownController _controller;

        public TestReqBreakdownControllerTests()
        {
            _mapper     = Substitute.For<IMapper>();
            _service    = Substitute.For<ITestReqBreakdownService>();
            _controller = new TestReqBreakdownController(_mapper, _service);
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

        private void SetupItemMapper(List<TestReqBreakdownDto> dtos, List<TestReqBreakdownItem> items)
        {
            _mapper.Map<List<TestReqBreakdownItem>>(dtos).Returns(items);
        }

        // ── Index ─────────────────────────────────────────────────────────────

        #region Index

        [Fact]
        public async Task Index_ReturnsViewWithViewModel()
        {
            var dtos     = new List<TestReqBreakdownDto> { new() { TestCode = "BLOOD", Project = "PRJ001" } };
            var items    = new List<TestReqBreakdownItem> { new() { TestCode = "BLOOD", Project = "PRJ001" } };
            var pagedRes = ApiResponseDto<List<TestReqBreakdownDto>>.SuccessResponse(dtos, new PaginationDto());

            _service.GetPlannedTestsByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            SetupItemMapper(dtos, items);

            var result = await _controller.Index();

            var view  = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestReqBreakdownViewModel>(view.Model);
            Assert.NotNull(model.Grid);
            Assert.Equal("testReqBreakdownGrid", model.Grid.GridId);
        }

        [Fact]
        public async Task Index_GridContainsMappedRows()
        {
            var dtos = new List<TestReqBreakdownDto>
            {
                new() { TestCode = "BLOOD", Project = "PRJ001", WorkG = "WG01", WgPrice = 10m, TotalCost = 50m },
                new() { TestCode = "URINE", Project = "PRJ002", WorkG = "WG02", WgPrice = 5m,  TotalCost = 25m }
            };
            var items = new List<TestReqBreakdownItem>
            {
                new() { TestCode = "BLOOD", Project = "PRJ001" },
                new() { TestCode = "URINE", Project = "PRJ002" }
            };
            var pagedRes = ApiResponseDto<List<TestReqBreakdownDto>>.SuccessResponse(dtos, new PaginationDto());

            _service.GetPlannedTestsByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            SetupItemMapper(dtos, items);

            var result = await _controller.Index();

            var view  = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestReqBreakdownViewModel>(view.Model);
            Assert.Equal(2, model.Grid.Data.Count);
        }

        [Fact]
        public async Task Index_WhenServiceReturnsEmpty_GridHasNoRows()
        {
            var pagedRes = ApiResponseDto<List<TestReqBreakdownDto>>.SuccessResponse([], new PaginationDto());

            _service.GetPlannedTestsByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            _mapper.Map<List<TestReqBreakdownItem>>(Arg.Any<List<TestReqBreakdownDto>>()).Returns([]);

            var result = await _controller.Index();

            var view  = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestReqBreakdownViewModel>(view.Model);
            Assert.Empty(model.Grid.Data);
        }

        [Fact]
        public async Task Index_WhenServiceFails_GridHasNoRows()
        {
            var errors   = new List<ApiErrorDto> { new() { Code = "API_ERROR" } };
            var pagedRes = ApiResponseDto<List<TestReqBreakdownDto>>.FailureResponse(errors, new ApiMetaDto());

            _service.GetPlannedTestsByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();

            var result = await _controller.Index();

            var view  = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestReqBreakdownViewModel>(view.Model);
            Assert.Empty(model.Grid.Data);
        }

        [Fact]
        public async Task Index_GridConfig_HasCorrectProperties()
        {
            var pagedRes = ApiResponseDto<List<TestReqBreakdownDto>>.SuccessResponse([], new PaginationDto());

            _service.GetPlannedTestsByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            _mapper.Map<List<TestReqBreakdownItem>>(Arg.Any<List<TestReqBreakdownDto>>()).Returns([]);

            var result = await _controller.Index();

            var view  = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestReqBreakdownViewModel>(view.Model);
            var grid  = model.Grid;

            Assert.Equal("testReqBreakdownGrid", grid.GridId);
            Assert.Equal("TestCode", grid.KeyProperty);
            Assert.False(grid.AllowAdd);
            Assert.False(grid.AllowEdit);
            Assert.False(grid.AllowDelete);
            Assert.True(grid.ShowPagination);
            Assert.Equal("/FPS/TestReqBreakdown/LoadGrid", grid.BindGridUrl);
        }

        #endregion

        // ── LoadGrid ──────────────────────────────────────────────────────────

        #region LoadGrid

        [Fact]
        public async Task LoadGrid_ValidRequest_ReturnsPartialView()
        {
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var dtos     = new List<TestReqBreakdownDto> { new() { TestCode = "BLOOD", Project = "PRJ001" } };
            var items    = new List<TestReqBreakdownItem> { new() { TestCode = "BLOOD", Project = "PRJ001" } };
            var pagedRes = ApiResponseDto<List<TestReqBreakdownDto>>.SuccessResponse(dtos, new PaginationDto());

            _service.GetPlannedTestsByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            SetupItemMapper(dtos, items);

            var result = await _controller.LoadGrid(request);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadGrid_ValidRequest_PartialViewModelIsDataGridConfig()
        {
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var pagedRes = ApiResponseDto<List<TestReqBreakdownDto>>.SuccessResponse([], new PaginationDto());

            _service.GetPlannedTestsByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            _mapper.Map<List<TestReqBreakdownItem>>(Arg.Any<List<TestReqBreakdownDto>>()).Returns([]);

            var result = await _controller.LoadGrid(request);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.IsType<DataGridConfig<TestReqBreakdownItem>>(partial.Model);
        }

        [Fact]
        public async Task LoadGrid_CallsServiceWithMappedQueryParameters()
        {
            var request  = new PaginationFilter<string> { Page = 2, PageSize = 20, Filter = "{}" };
            var pagedRes = ApiResponseDto<List<TestReqBreakdownDto>>.SuccessResponse([], new PaginationDto());

            _service.GetPlannedTestsByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            _mapper.Map<List<TestReqBreakdownItem>>(Arg.Any<List<TestReqBreakdownDto>>()).Returns([]);

            await _controller.LoadGrid(request);

            await _service.Received(1)
                .GetPlannedTestsByWorkgroupAsync(Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 2 && q.PageSize == 20));
        }

        [Fact]
        public async Task LoadGrid_WithFilter_ParsesFilterCorrectly()
        {
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{\"TestCode\":\"BLOOD\"}" };
            var pagedRes = ApiResponseDto<List<TestReqBreakdownDto>>.SuccessResponse([], new PaginationDto());

            _service.GetPlannedTestsByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            _mapper.Map<List<TestReqBreakdownItem>>(Arg.Any<List<TestReqBreakdownDto>>()).Returns([]);

            var result = await _controller.LoadGrid(request);

            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<TestReqBreakdownItem>>(partial.Model);
            Assert.NotNull(grid.CurrentFilters);
            Assert.True(grid.CurrentFilters.ContainsKey("TestCode"));
            Assert.Equal("BLOOD", grid.CurrentFilters["TestCode"]);
        }

        [Fact]
        public async Task LoadGrid_PaginationDataFromResponse_IsPopulatedCorrectly()
        {
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var pagination = new PaginationDto { TotalRecords = 100, PageNumber = 1, PageSize = 10 };
            var pagedRes = ApiResponseDto<List<TestReqBreakdownDto>>.SuccessResponse([], pagination);

            _service.GetPlannedTestsByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();
            _mapper.Map<List<TestReqBreakdownItem>>(Arg.Any<List<TestReqBreakdownDto>>()).Returns([]);

            var result = await _controller.LoadGrid(request);

            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<TestReqBreakdownItem>>(partial.Model);
            Assert.Equal(100, grid.Pagination.TotalRecords);
            Assert.Equal(1,   grid.Pagination.PageNumber);
            Assert.Equal(10,  grid.Pagination.PageSize);
        }

        [Fact]
        public async Task LoadGrid_WhenServiceFails_ReturnsPartialWithEmptyData()
        {
            var request  = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var errors   = new List<ApiErrorDto> { new() { Code = "API_ERROR" } };
            var pagedRes = ApiResponseDto<List<TestReqBreakdownDto>>.FailureResponse(errors, new ApiMetaDto());

            _service.GetPlannedTestsByWorkgroupAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedRes);
            SetupQueryParamMapper();

            var result = await _controller.LoadGrid(request);

            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<TestReqBreakdownItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        #endregion
    }
}
