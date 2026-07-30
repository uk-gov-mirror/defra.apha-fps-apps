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
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.StaffPlanDetailsControllerTest
{
    public class StaffPlanDetailsControllerTests
    {
        private const string DefaultProfitCentre = "PC01";

        private readonly IMapper _mapper;
        private readonly IProjectStaffPlanDetailsService _staffPlanDetailsService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly StaffPlanDetailsController _controller;

        public StaffPlanDetailsControllerTests()
        {
            _mapper                  = Substitute.For<IMapper>();
            _staffPlanDetailsService = Substitute.For<IProjectStaffPlanDetailsService>();
            _profitCentreService     = Substitute.For<IProfitCentreService>();
            _controller              = new StaffPlanDetailsController(_mapper, _staffPlanDetailsService, _profitCentreService);

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
        }

        private static List<ProjectStaffPlanDetailsViewDto> BuildDtoList() =>
        [
            new() { ProfitCentre = "PC01", Program = "AH0032", Name = "E_WILDLIFE, General", Manager = "Mgr1",
                    ProjectStatus = "Open", PlannedHours = 25344, ChargeRate = 53.34m, Cost = 1351848.96m,
                    WorkGroup = "Wildlife", GradeCode = "E" },
            new() { ProfitCentre = "PC01", Program = "ED1044", Name = "C_SVCA, General", Manager = "Mgr2",
                    ProjectStatus = "Closed", PlannedHours = 12000, ChargeRate = 69.92m, Cost = 839040.00m,
                    WorkGroup = "SVCA", GradeCode = "C" }
        ];

        private static ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>> SuccessResponse(
            List<ProjectStaffPlanDetailsViewDto>? data = null,
            int totalRecords = 2) =>
            ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>.SuccessResponse(
                data ?? BuildDtoList(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = totalRecords });

        private static List<ProfitCentreDto> BuildProfitCentreList() =>
        [
            new() { ProfitCentreId = "PC01" },
            new() { ProfitCentreId = "PC02" }
        ];

        private void SetupProfitCentreSuccess(List<ProfitCentreDto>? list = null)
        {
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(list ?? BuildProfitCentreList()));
        }

        #region Index

        [Fact]
        public async Task Index_ReturnsViewWithStaffPlanDetailsViewModel()
        {
            // Arrange
            SetupProfitCentreSuccess();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<StaffPlanDetailsViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_PopulatesProfitCentreOptions()
        {
            // Arrange
            SetupProfitCentreSuccess();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<StaffPlanDetailsViewModel>(viewResult.Model);
            Assert.Equal(2, model.ProfitCentreOptions.Count);
        }

        [Fact]
        public async Task Index_NoProfitCentreSelected_GridIsEmpty()
        {
            // Arrange
            SetupProfitCentreSuccess();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<StaffPlanDetailsViewModel>(viewResult.Model);
            Assert.NotNull(model.Grid);
            Assert.Empty(model.Grid.Data);
            Assert.Null(model.SelectedProfitCentre);
            await _staffPlanDetailsService.DidNotReceive().GetPagedAsync(Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task Index_WhenProfitCentreServiceFails_OptionsAreEmpty()
        {
            // Arrange
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Error", Code = "ERR" }], new ApiMetaDto()));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<StaffPlanDetailsViewModel>(viewResult.Model);
            Assert.Empty(model.ProfitCentreOptions);
        }

        [Fact]
        public async Task Index_ProfitCentreWithBlankId_IsExcludedFromOptions()
        {
            // Arrange
            SetupProfitCentreSuccess([new() { ProfitCentreId = "PC01" }, new() { ProfitCentreId = " " }]);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<StaffPlanDetailsViewModel>(viewResult.Model);
            Assert.Single(model.ProfitCentreOptions);
        }

        [Fact]
        public async Task Index_GridHasReadOnlyConfiguration()
        {
            // Arrange
            SetupProfitCentreSuccess();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model      = Assert.IsType<StaffPlanDetailsViewModel>(viewResult.Model);
            Assert.False(model.Grid.AllowAdd);
            Assert.False(model.Grid.AllowEdit);
            Assert.False(model.Grid.AllowDelete);
            Assert.Equal("staffPlanDetailsGrid", model.Grid.GridId);
        }

        #endregion

        #region LoadGrid

        [Fact]
        public async Task LoadGrid_WithProfitCentre_ReturnsPartialViewWithData()
        {
            // Arrange
            _staffPlanDetailsService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<List<StaffPlanDetailsViewItem>>(Arg.Any<List<ProjectStaffPlanDetailsViewDto>>())
                .Returns(new List<StaffPlanDetailsViewItem> { new(), new() });

            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadGrid(request, DefaultProfitCentre);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<StaffPlanDetailsViewItem>>(partial.Model);
            Assert.Equal(2, grid.Data.Count);
        }

        [Fact]
        public async Task LoadGrid_ReturnsCorrectPartialViewName()
        {
            // Arrange
            _staffPlanDetailsService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse());
            _mapper.Map<List<StaffPlanDetailsViewItem>>(Arg.Any<List<ProjectStaffPlanDetailsViewDto>>())
                .Returns(new List<StaffPlanDetailsViewItem>());

            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadGrid(request, DefaultProfitCentre);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadGrid_WithoutProfitCentre_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadGrid(request, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<StaffPlanDetailsViewItem>>(partial.Model);
            Assert.Empty(grid.Data);
            await _staffPlanDetailsService.DidNotReceive().GetPagedAsync(Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task LoadGrid_WhenServiceFails_ReturnsEmptyGrid()
        {
            // Arrange
            _staffPlanDetailsService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "API error", Code = "API_ERROR" }], new ApiMetaDto()));

            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadGrid(request, DefaultProfitCentre);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<StaffPlanDetailsViewItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadGrid_WhenServiceReturnsNullData_ReturnsEmptyGrid()
        {
            // Arrange
            _staffPlanDetailsService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(new ApiResponseDto<List<ProjectStaffPlanDetailsViewDto>> { Success = true, Data = null, Pagination = null });

            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadGrid(request, DefaultProfitCentre);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<StaffPlanDetailsViewItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadGrid_PreservesPaginationMetadata()
        {
            // Arrange
            _staffPlanDetailsService.GetPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(SuccessResponse(totalRecords: 42));
            _mapper.Map<List<StaffPlanDetailsViewItem>>(Arg.Any<List<ProjectStaffPlanDetailsViewDto>>())
                .Returns(new List<StaffPlanDetailsViewItem>());

            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadGrid(request, DefaultProfitCentre);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<StaffPlanDetailsViewItem>>(partial.Model);
            Assert.Equal(42, grid.Pagination.TotalRecords);
        }

        [Fact]
        public async Task LoadGrid_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Invalid");
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadGrid(request, DefaultProfitCentre);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        #endregion
    }
}
