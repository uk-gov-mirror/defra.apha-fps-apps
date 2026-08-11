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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.TestSnapshotDataControllerTest
{
    public class TestSnapshotDataControllerTests
    {
        private readonly IMapper _mapper;
        private readonly ITestorProductService _testorProductService;
        private readonly TestSnapshotDataController _controller;

        public TestSnapshotDataControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _testorProductService = Substitute.For<ITestorProductService>();
            _controller = new TestSnapshotDataController(_mapper, _testorProductService);
        }

        private static TestFeePlanViewDto BuildDto(string testCode = "TC001") =>
            new()
            {
                Version = "V1",
                Directorate = "Dir",
                Customer = "Cust",
                Program = "PRG",
                Contract = "C1",
                Project = "P1",
                Status = "Approved",
                TestCode = testCode,
                UnitPrice = 10m,
                NoTests = 5,
                TestFee = 50d,
                Owner = "Owner"
            };

        #region Index Tests

        [Fact]
        public void Index_ReturnsViewWithEmptyGridConfig()
        {
            var result = _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestSnapshotDataViewModel>(viewResult.Model);
            Assert.Equal("snapShotTestDataGrid", model.SnapShotTestDataGrid.GridId);
            Assert.Empty(model.SnapShotTestDataGrid.Data);
            Assert.True(model.SnapShotTestDataGrid.ShowPagination);
            Assert.False(model.SnapShotTestDataGrid.AllowAdd);
        }

        #endregion

        #region LoadTestSnapshotDataGrid Tests

        [Fact]
        public async Task LoadTestSnapshotDataGrid_WithValidRequest_ReturnsPartialViewWithDataGrid()
        {
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, SortBy = "TestCode", Descending = true };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<TestFeePlanViewDto> { BuildDto() };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var serviceResponse = ApiResponseDto<List<TestFeePlanViewDto>>.SuccessResponse(dtos, paginationDto);
            var items = new List<TestSnapshotItem> { new() { TestCode = "TC001" } };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _testorProductService.GetTestSnapshotPagedAsync(queryParameters).Returns(serviceResponse);
            _mapper.Map<List<TestSnapshotItem>>(Arg.Any<List<TestFeePlanViewDto>>()).Returns(items);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            var result = await _controller.LoadTestSnapshotDataGrid(request);

            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<TestSnapshotItem>>(partialView.Model);
            Assert.Equal("snapShotTestDataGrid", gridConfig.GridId);
            Assert.Single(gridConfig.Data);
            Assert.Equal("TestCode", gridConfig.Pagination!.SortColumn);
            Assert.True(gridConfig.Pagination.SortDirection);
            await _testorProductService.Received(1).GetTestSnapshotPagedAsync(queryParameters);
        }

        [Fact]
        public async Task LoadTestSnapshotDataGrid_WhenServiceReturnsFailure_ReturnsPartialViewWithEmptyData()
        {
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var serviceResponse = ApiResponseDto<List<TestFeePlanViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _testorProductService.GetTestSnapshotPagedAsync(queryParameters).Returns(serviceResponse);

            var result = await _controller.LoadTestSnapshotDataGrid(request);

            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<TestSnapshotItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
            _mapper.DidNotReceive().Map<List<TestSnapshotItem>>(Arg.Any<List<TestFeePlanViewDto>>());
        }

        [Fact]
        public async Task LoadTestSnapshotDataGrid_WithInvalidModelState_ReturnsJsonError()
        {
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _controller.ModelState.AddModelError("Page", "Invalid");

            var result = await _controller.LoadTestSnapshotDataGrid(request);

            Assert.IsType<JsonResult>(result);
            await _testorProductService.DidNotReceive().GetTestSnapshotPagedAsync(Arg.Any<QueryParameters<string>>());
        }

        #endregion
    }
}
