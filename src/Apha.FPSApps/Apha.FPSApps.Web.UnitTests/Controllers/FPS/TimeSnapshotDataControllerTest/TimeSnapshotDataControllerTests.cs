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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.TimeSnapshotDataControllerTest
{
    public class TimeSnapshotDataControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProgramService _programService;
        private readonly TimeSnapshotDataController _controller;

        public TimeSnapshotDataControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _programService = Substitute.For<IProgramService>();
            _controller = new TimeSnapshotDataController(_mapper, _programService);
        }

        private static ProgramPlanCostViewDto BuildDto(string program = "PRG") =>
            new()
            {
                Version = "V1",
                Directorate = "Dir",
                Program = program,
                Customer = "Cust",
                Contract = "C1",
                Project = "P1",
                Status = "Approved",
                ResourceCentre = "RC1",
                WorkGroup = "WG1",
                GradeCode = "G1",
                Name = "Name",
                Hours = 10,
                HoursCost = 500m
            };

        #region Index Tests

        [Fact]
        public void Index_ReturnsViewWithEmptyGridConfig()
        {
            var result = _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TimeSnapshotDataViewModel>(viewResult.Model);
            Assert.Equal("snapShotTimeDataGrid", model.SnapShotTimeDataGrid.GridId);
            Assert.Empty(model.SnapShotTimeDataGrid.Data);
            Assert.True(model.SnapShotTimeDataGrid.ShowPagination);
            Assert.False(model.SnapShotTimeDataGrid.AllowAdd);
        }

        #endregion

        #region LoadTimeSnapshotDataGrid Tests

        [Fact]
        public async Task LoadTimeSnapshotDataGrid_WithValidRequest_ReturnsPartialViewWithDataGrid()
        {
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, SortBy = "Program", Descending = true };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<ProgramPlanCostViewDto> { BuildDto() };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var serviceResponse = ApiResponseDto<List<ProgramPlanCostViewDto>>.SuccessResponse(dtos, paginationDto);
            var items = new List<TimeSnapshotItem> { new() { Program = "PRG" } };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _programService.GetProgramTimeSnapshotAsync(queryParameters).Returns(serviceResponse);
            _mapper.Map<List<TimeSnapshotItem>>(Arg.Any<List<ProgramPlanCostViewDto>>()).Returns(items);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            var result = await _controller.LoadTimeSnapshotDataGrid(request);

            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<TimeSnapshotItem>>(partialView.Model);
            Assert.Equal("snapShotTimeDataGrid", gridConfig.GridId);
            Assert.Single(gridConfig.Data);
            Assert.Equal("Program", gridConfig.Pagination!.SortColumn);
            Assert.True(gridConfig.Pagination.SortDirection);
            await _programService.Received(1).GetProgramTimeSnapshotAsync(queryParameters);
        }

        [Fact]
        public async Task LoadTimeSnapshotDataGrid_WhenServiceReturnsFailure_ReturnsPartialViewWithEmptyData()
        {
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var serviceResponse = ApiResponseDto<List<ProgramPlanCostViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _programService.GetProgramTimeSnapshotAsync(queryParameters).Returns(serviceResponse);

            var result = await _controller.LoadTimeSnapshotDataGrid(request);

            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<TimeSnapshotItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
            _mapper.DidNotReceive().Map<List<TimeSnapshotItem>>(Arg.Any<List<ProgramPlanCostViewDto>>());
        }

        [Fact]
        public async Task LoadTimeSnapshotDataGrid_WithInvalidModelState_ReturnsJsonError()
        {
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _controller.ModelState.AddModelError("Page", "Invalid");

            var result = await _controller.LoadTimeSnapshotDataGrid(request);

            Assert.IsType<JsonResult>(result);
            await _programService.DidNotReceive().GetProgramTimeSnapshotAsync(Arg.Any<QueryParameters<string>>());
        }

        #endregion
    }
}
