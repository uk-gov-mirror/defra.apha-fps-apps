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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.AnimalSnapshotDataControllerTest
{
    public class AnimalSnapshotDataControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IAnimalService _animalService;
        private readonly AnimalSnapshotDataController _controller;

        public AnimalSnapshotDataControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _animalService = Substitute.For<IAnimalService>();
            _controller = new AnimalSnapshotDataController(_mapper, _animalService);
        }

        private static AnimalSnapshotViewDto BuildDto(string animalType = "CATTLE") =>
            new()
            {
                Directorate = "Dir",
                Program = "PRG",
                Contract = "C1",
                Project = "P1",
                ProjectStatus = "Approved",
                Species = "Bovine",
                SecurityLevel = "L1",
                AnimalType = animalType,
                DailyRate = 50m,
                JobCode = "JOB001",
                NumberOfDays = 5,
                NumberOfAnimals = 3,
                Cost = 750m
            };

        #region Index Tests

        [Fact]
        public void Index_ReturnsViewWithEmptyGridConfig()
        {
            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AnimalSnapshotDataViewModel>(viewResult.Model);
            Assert.Equal("snapShotAnimalDataGrid", model.SnapShotAnimalDataGrid.GridId);
            Assert.Empty(model.SnapShotAnimalDataGrid.Data);
            Assert.True(model.SnapShotAnimalDataGrid.ShowPagination);
            Assert.False(model.SnapShotAnimalDataGrid.AllowAdd);
        }

        #endregion

        #region LoadAnimalSnapshotDataGrid Tests

        [Fact]
        public async Task LoadAnimalSnapshotDataGrid_WithValidRequest_ReturnsPartialViewWithDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, SortBy = "AnimalType", Descending = true };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<AnimalSnapshotViewDto> { BuildDto() };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var serviceResponse = ApiResponseDto<List<AnimalSnapshotViewDto>>.SuccessResponse(dtos, paginationDto);
            var items = new List<AnimalSnapshotItem> { new() { AnimalType = "CATTLE" } };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _animalService.GetAnimalSnapshotAsync(queryParameters).Returns(serviceResponse);
            _mapper.Map<List<AnimalSnapshotItem>>(Arg.Any<List<AnimalSnapshotViewDto>>()).Returns(items);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadAnimalSnapshotDataGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<AnimalSnapshotItem>>(partialView.Model);
            Assert.Equal("snapShotAnimalDataGrid", gridConfig.GridId);
            Assert.Single(gridConfig.Data);
            Assert.Equal("AnimalType", gridConfig.Pagination!.SortColumn);
            Assert.True(gridConfig.Pagination.SortDirection);
            await _animalService.Received(1).GetAnimalSnapshotAsync(queryParameters);
        }

        [Fact]
        public async Task LoadAnimalSnapshotDataGrid_WhenServiceReturnsFailure_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var serviceResponse = ApiResponseDto<List<AnimalSnapshotViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _animalService.GetAnimalSnapshotAsync(queryParameters).Returns(serviceResponse);

            // Act
            var result = await _controller.LoadAnimalSnapshotDataGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<AnimalSnapshotItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
            _mapper.DidNotReceive().Map<List<AnimalSnapshotItem>>(Arg.Any<List<AnimalSnapshotViewDto>>());
        }

        [Fact]
        public async Task LoadAnimalSnapshotDataGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            _controller.ModelState.AddModelError("Page", "Invalid");

            // Act
            var result = await _controller.LoadAnimalSnapshotDataGrid(request);

            // Assert
            Assert.IsType<JsonResult>(result);
            await _animalService.DidNotReceive().GetAnimalSnapshotAsync(Arg.Any<QueryParameters<string>>());
        }

        #endregion
    }
}
