using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.BidSnapshotDataControllerTest
{
    public class BidSnapshotDataControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IBudgetBidsService _budgetBidsService;
        private readonly BidSnapshotDataController _controller;

        public BidSnapshotDataControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _budgetBidsService = Substitute.For<IBudgetBidsService>();
            _controller = new BidSnapshotDataController(_mapper, _budgetBidsService);
        }

        private static GenericBidViewDto BuildDto(string account = "ACC1") =>
            new()
            {
                ProfitCentre = "PC1",
                WorkGroupName = "WG01",
                Account = account,
                GenBid = 100m,
                AccountType = "TYPE1"
            };

        #region Index Tests

        [Fact]
        public void Index_ReturnsViewWithEmptyGridConfig()
        {
            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SnapShotBidViewModel>(viewResult.Model);
            Assert.Equal("snapShotBidGrid", model.SnapShotBidGrid.GridId);
            Assert.Empty(model.SnapShotBidGrid.Data);
            Assert.True(model.SnapShotBidGrid.ShowPagination);
            Assert.False(model.SnapShotBidGrid.AllowAdd);
        }

        #endregion

        #region LoadSnapShotBidDataGrid Tests

        [Fact]
        public async Task LoadSnapShotBidDataGrid_WithValidRequest_ReturnsPartialViewWithDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, SortBy = "Account", Descending = true };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<GenericBidViewDto> { BuildDto() };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var serviceResponse = ApiResponseDto<List<GenericBidViewDto>>.SuccessResponse(dtos, paginationDto);
            var items = new List<GenericBidItem> { new() { Account = "ACC1", WorkGroupName = "WG01" } };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _budgetBidsService.GetGenericBidsPagedAsync(queryParameters).Returns(serviceResponse);
            _mapper.Map<List<GenericBidItem>>(Arg.Any<List<GenericBidViewDto>>()).Returns(items);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadSnapShotBidDataGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<GenericBidItem>>(partialView.Model);
            Assert.Equal("snapShotBidGrid", gridConfig.GridId);
            Assert.Single(gridConfig.Data);
            Assert.Equal("Account", gridConfig.Pagination!.SortColumn);
            Assert.True(gridConfig.Pagination.SortDirection);
            await _budgetBidsService.Received(1).GetGenericBidsPagedAsync(queryParameters);
        }

        [Fact]
        public async Task LoadSnapShotBidDataGrid_WhenServiceReturnsFailure_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var serviceResponse = ApiResponseDto<List<GenericBidViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _budgetBidsService.GetGenericBidsPagedAsync(queryParameters).Returns(serviceResponse);

            // Act
            var result = await _controller.LoadSnapShotBidDataGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<GenericBidItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
            _mapper.DidNotReceive().Map<List<GenericBidItem>>(Arg.Any<List<GenericBidViewDto>>());
        }

        [Fact]
        public async Task LoadSnapShotBidDataGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _controller.ModelState.AddModelError("Page", "Invalid");

            // Act
            var result = await _controller.LoadSnapShotBidDataGrid(request);

            // Assert
            Assert.IsType<JsonResult>(result);
            await _budgetBidsService.DidNotReceive().GetGenericBidsPagedAsync(Arg.Any<QueryParameters<string>>());
        }

        #endregion
    }
}
