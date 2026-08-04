using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.AnimalCostsControllerTest
{
    public class AnimalCostsControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IAnimalPlanService _animalPlanService;
        private readonly IAnimalService _animalService;
        private readonly AnimalCostsController _controller;

        public AnimalCostsControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _animalPlanService = Substitute.For<IAnimalPlanService>();
            _animalService = Substitute.For<IAnimalService>();
            _controller = new AnimalCostsController(_mapper, _animalPlanService, _animalService);
        }

        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        private static AnimalDto BuildAnimalDto(string animalType = "CATTLE", decimal? dailyRate = 50m) =>
            new() { AnimalType = animalType, Species = "Bovine", SecurityLevel = "L1", DailyRate = dailyRate };

        private static ApiResponseDto<IEnumerable<AnimalDto>> BuildAnimalsResponse(
            IEnumerable<AnimalDto>? data = null) =>
            ApiResponseDto<IEnumerable<AnimalDto>>.SuccessResponse(
                data ?? new List<AnimalDto> { BuildAnimalDto() });

        private void SetupAnimalsService(IEnumerable<AnimalDto>? data = null)
        {
            _animalService.GetAllAnimalsAsync().Returns(BuildAnimalsResponse(data));
        }

        private void SetupPlanService(IEnumerable<AnimalCostViewDto>? data = null)
        {
            var list = data?.ToList() ?? new List<AnimalCostViewDto>
            {
                new() { IndCounter = 1, AnimalType = "CATTLE", JobCode = "JOB001", NumberOfDays = 5, NumberOfAnimals = 10 }
            };
            var response = ApiResponseDto<List<AnimalCostViewDto>>.SuccessResponse(
                list, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = list.Count });

            _animalPlanService
                .GetAnimalCostByAnimalTypeAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(response);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<AnimalCostsItem>>(Arg.Any<List<AnimalCostViewDto>>())
                .Returns(list.Select(d => new AnimalCostsItem { IndCounter = d.IndCounter, AnimalType = d.AnimalType, JobCode = d.JobCode }).ToList());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());
        }

        private record JsonResponse(bool success, string? message);

        #region Access / Authorization Attribute Tests

        [Fact]
        public void Controller_HasAuthorizeAttribute_WithExpectedRoles()
        {
            var attrs = typeof(AnimalCostsController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), true);
            Assert.NotEmpty(attrs);
            var auth = (AuthorizeAttribute)attrs[0];
            Assert.Contains("FPSAdmin", auth.Roles);
        }

        [Fact]
        public void Controller_HasAreaAttribute_FPS()
        {
            var attrs = typeof(AnimalCostsController)
                .GetCustomAttributes(typeof(AreaAttribute), true);
            Assert.NotEmpty(attrs);
            Assert.Equal("FPS", ((AreaAttribute)attrs[0]).RouteValue);
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewWithViewModel()
        {
            SetupAnimalsService();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<AnimalCostsViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_PopulatesAnimalTypeDropdown()
        {
            SetupAnimalsService(new List<AnimalDto> { BuildAnimalDto("CATTLE"), BuildAnimalDto("SHEEP") });

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AnimalCostsViewModel>(viewResult.Model);
            Assert.Equal(2, model.AnimalTypeList.Count);
        }

        [Fact]
        public async Task Index_Grid_HasExpectedGridIdAndBindUrl()
        {
            SetupAnimalsService();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AnimalCostsViewModel>(viewResult.Model);
            Assert.Equal("asuUsageList", model.AnimalCostsGrid.GridId);
            Assert.Equal("/FPS/AnimalCosts/LoadAnimalCostsGrid", model.AnimalCostsGrid.BindGridUrl);
        }

        [Fact]
        public async Task Index_Grid_IsReadOnly()
        {
            SetupAnimalsService();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AnimalCostsViewModel>(viewResult.Model);
            Assert.False(model.AnimalCostsGrid.AllowAdd);
            Assert.False(model.AnimalCostsGrid.AllowEdit);
            Assert.False(model.AnimalCostsGrid.AllowDelete);
        }

        #endregion

        #region LoadAnimalCostsGrid Tests

        [Fact]
        public async Task LoadAnimalCostsGrid_WithValidAnimalType_ReturnsPartialView()
        {
            SetupPlanService();
            var request = new PaginationFilter<string> { Filter = "{}" };

            var result = await _controller.LoadAnimalCostsGrid(request, "CATTLE");

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            Assert.IsType<DataGridConfig<AnimalCostsItem>>(partialViewResult.Model);
        }

        [Fact]
        public async Task LoadAnimalCostsGrid_WithoutAnimalType_ReturnsEmptyGridPartialView()
        {
            SetupPlanService();
            var request = new PaginationFilter<string> { Filter = "{}" };

            var result = await _controller.LoadAnimalCostsGrid(request, null);

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<AnimalCostsItem>>(partialViewResult.Model);
            Assert.Empty(gridConfig.Data);
            await _animalPlanService.DidNotReceive()
                .GetAnimalCostByAnimalTypeAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadAnimalCostsGrid_WithInvalidModelState_ReturnsEmptyGridPartialView()
        {
            SetupPlanService();
            _controller.ModelState.AddModelError("Test", "Test error");
            var request = new PaginationFilter<string> { Filter = "{}" };

            var result = await _controller.LoadAnimalCostsGrid(request, "CATTLE");

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<AnimalCostsItem>>(partialViewResult.Model);
            Assert.Empty(gridConfig.Data);
        }

        [Fact]
        public async Task LoadAnimalCostsGrid_WithValidAnimalType_PassesFilterToService()
        {
            SetupPlanService();
            var request = new PaginationFilter<string>
            {
                Filter = "{\"AnimalType\":\"CATTLE\"}",
                SortBy = "AnimalCost",
                Descending = true
            };

            await _controller.LoadAnimalCostsGrid(request, "CATTLE");

            await _animalPlanService.Received(1)
                .GetAnimalCostByAnimalTypeAsync(Arg.Any<QueryParameters<string>>(), "CATTLE");
        }

        [Fact]
        public async Task LoadAnimalCostsGrid_WithEmptyData_ReturnsEmptyGrid()
        {
            SetupPlanService(new List<AnimalCostViewDto>());
            var request = new PaginationFilter<string> { Filter = "{}" };

            var result = await _controller.LoadAnimalCostsGrid(request, "CATTLE");

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<AnimalCostsItem>>(partialViewResult.Model);
            Assert.Empty(gridConfig.Data);
        }

        #endregion

        #region GetAnimalTypes Tests

        [Fact]
        public async Task GetAnimalTypes_WithData_ReturnsSuccessJson()
        {
            SetupAnimalsService(new List<AnimalDto> { BuildAnimalDto("CATTLE"), BuildAnimalDto("SHEEP") });

            var result = await _controller.GetAnimalTypes();

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
        }

        [Fact]
        public async Task GetAnimalTypes_WhenServiceFails_ReturnsFailureJson()
        {
            _animalService.GetAllAnimalsAsync()
                .Returns(ApiResponseDto<IEnumerable<AnimalDto>>.FailureResponse(
                    new List<ApiErrorDto> { new() { Message = "err", Code = "ERR" } }, new ApiMetaDto()));

            var result = await _controller.GetAnimalTypes();

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task GetAnimalTypes_WhenServiceThrows_PropagatesException()
        {
            _animalService.GetAllAnimalsAsync().Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetAnimalTypes());
        }

        #endregion
    }
}
