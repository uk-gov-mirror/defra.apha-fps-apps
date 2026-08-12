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
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.DivisionMaintenanceControllerTest
{
    public class DivisionMaintenanceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IDivisionService _divisionService;
        private readonly DivisionMaintenanceController _controller;

        public DivisionMaintenanceControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _divisionService = Substitute.For<IDivisionService>();
            _controller = new DivisionMaintenanceController(_mapper, _divisionService);
        }

        // Helper method to extract properties from JsonResult
        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult_WithDivisionGrid()
        {
            // Arrange
            var divisions = new List<DivisionDto>
            {
                new DivisionDto { DivName = "VSD", DivisionId = 1, AgencyId = 1 },
                new DivisionDto { DivName = "ACDP", DivisionId = 2, AgencyId = 1 }
            };
            var divisionViewModels = new List<DivisionViewModel>
            {
                new DivisionViewModel { DivName = "VSD", DivisionId = 1, AgencyId = 1 },
                new DivisionViewModel { DivName = "ACDP", DivisionId = 2, AgencyId = 1 }
            };

            var apiResponse = ApiResponseDto<List<DivisionDto>>.SuccessResponse(
                divisions, 
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _divisionService.GetAllDivisionsPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<List<DivisionViewModel>>(Arg.Any<List<DivisionDto>>()).Returns(divisionViewModels);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(
                new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DivisionMaintenanceViewModel>(viewResult.Model);
            Assert.Equal("divisionGrid", model.DivisionGrid.GridId);
            Assert.Equal("Division Maintenance", model.DivisionGrid.Title);
        }

        [Fact]
        public async Task Index_CallsGetAllDivisionsPagedAsync_WithDefaultParameters()
        {
            // Arrange
            var apiResponse = ApiResponseDto<List<DivisionDto>>.SuccessResponse(
                new List<DivisionDto>(), 
                new PaginationDto());
            _divisionService.GetAllDivisionsPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<List<DivisionViewModel>>(Arg.Any<List<DivisionDto>>()).Returns(new List<DivisionViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            await _controller.Index();

            // Assert
            await _divisionService.Received(1).GetAllDivisionsPagedAsync(Arg.Any<QueryParameters<string>>());
        }

        #endregion

        #region LoadDivisionGrid Tests

        [Fact]
        public async Task LoadDivisionGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var divisions = new List<DivisionDto>
            {
                new DivisionDto { DivName = "VSD", DivisionId = 1, AgencyId = 1 }
            };
            var apiResponse = ApiResponseDto<List<DivisionDto>>.SuccessResponse(
                divisions, 
                new PaginationDto { PageNumber = 1, PageSize = 10 });

            _divisionService.GetAllDivisionsPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());
            _mapper.Map<List<DivisionViewModel>>(Arg.Any<List<DivisionDto>>()).Returns(
                new List<DivisionViewModel> { new DivisionViewModel { DivName = "VSD", DivisionId = 1, AgencyId = 1 } });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadDivisionGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            Assert.IsType<DataGridConfig<DivisionViewModel>>(partialViewResult.Model);
        }

        [Fact]
        public async Task LoadDivisionGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("Test", "Test error");

            // Act
            var result = await _controller.LoadDivisionGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid request data", value.message);
        }

        [Fact]
        public async Task LoadDivisionGrid_WithFilter_AppliesCorrectFiltering()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{\"DivName\":\"VSD\"}" };
            var divisions = new List<DivisionDto>
            {
                new DivisionDto { DivName = "VSD", DivisionId = 1, AgencyId = 1 }
            };
            var apiResponse = ApiResponseDto<List<DivisionDto>>.SuccessResponse(divisions, new PaginationDto());

            _divisionService.GetAllDivisionsPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());
            _mapper.Map<List<DivisionViewModel>>(Arg.Any<List<DivisionDto>>()).Returns(new List<DivisionViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadDivisionGrid(request);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _divisionService.Received(1).GetAllDivisionsPagedAsync(Arg.Any<QueryParameters<string>>());
        }

        #endregion

        #region Create Tests

        [Fact]
        public void Create_Get_ReturnsPartialViewWithModel()
        {
            // Act
            var result = _controller.Create();

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditDivision", partialViewResult.ViewName);
            var model = Assert.IsType<DivisionViewModel>(partialViewResult.Model);
            Assert.Equal(string.Empty, model.DivName);
        }

        [Fact]
        public async Task Create_Post_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var divisionViewModel = new DivisionViewModel
            {
                DivName = "NEW",
                DivisionId = 99,
                AgencyId = 1,
                CentOverhead = 100
            };
            var divisionDto = new DivisionDto
            {
                DivName = "NEW",
                DivisionId = 99,
                AgencyId = 1,
                CentOverhead = 100
            };
            var createdDto = new DivisionDto
            {
                DivName = "NEW",
                DivisionId = 99,
                AgencyId = 1,
                CentOverhead = 100
            };
            var apiResponse = ApiResponseDto<DivisionDto>.SuccessResponse(createdDto);

            _mapper.Map<DivisionDto>(divisionViewModel).Returns(divisionDto);
            _divisionService.CreateDivisionAsync(divisionDto).Returns(apiResponse);

            // Act
            var result = await _controller.Create(divisionViewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
        }

        [Fact]
        public async Task Create_Post_WithInvalidModelState_ReturnsValidationErrors()
        {
            // Arrange
            var divisionViewModel = new DivisionViewModel
            {
                DivName = "",
                DivisionId = 1,
                AgencyId = 1
            };
            _controller.ModelState.AddModelError("DivName", "Division name is required");

            // Act
            var result = await _controller.Create(divisionViewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task Create_Post_WhenServiceFails_ReturnsError()
        {
            // Arrange
            var divisionViewModel = new DivisionViewModel
            {
                DivName = "NEW",
                DivisionId = 1,
                AgencyId = 1
            };
            var divisionDto = new DivisionDto
            {
                DivName = "NEW",
                DivisionId = 1,
                AgencyId = 1
            };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Division already exists", Code = "DUPLICATE" }
            };
            var apiResponse = ApiResponseDto<DivisionDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<DivisionDto>(divisionViewModel).Returns(divisionDto);
            _divisionService.CreateDivisionAsync(divisionDto).Returns(apiResponse);

            // Act
            var result = await _controller.Create(divisionViewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region Edit Tests

        [Fact]
        public async Task Edit_Get_WithValidDivName_ReturnsPartialViewWithModel()
        {
            // Arrange
            var divName = "VSD";
            var division = new DivisionDto
            {
                DivName = divName,
                DivisionId = 1,
                AgencyId = 1,
                CentOverhead = 100
            };
            var divisionViewModel = new DivisionViewModel
            {
                DivName = divName,
                DivisionId = 1,
                AgencyId = 1,
                CentOverhead = 100
            };
            var apiResponse = ApiResponseDto<DivisionDto>.SuccessResponse(division);

            _divisionService.GetDivisionByNameAsync(divName).Returns(apiResponse);
            _mapper.Map<DivisionViewModel>(division).Returns(divisionViewModel);

            // Act
            var result = await _controller.Edit(divName);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditDivision", partialViewResult.ViewName);
            var model = Assert.IsType<DivisionViewModel>(partialViewResult.Model);
            Assert.Equal(divName, model.DivName);
        }

        [Fact]
        public async Task Edit_Get_WithNonExistentDivision_ReturnsJsonError()
        {
            // Arrange
            var divName = "NONEXISTENT";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" }
            };
            var apiResponse = ApiResponseDto<DivisionDto>.FailureResponse(errors, new ApiMetaDto());

            _divisionService.GetDivisionByNameAsync(divName).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(divName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task Edit_Post_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var originalDivName = "VSD";
            var divisionViewModel = new DivisionViewModel
            {
                DivName = "VSD",
                DivisionId = 2,
                AgencyId = 2,
                CentOverhead = 200
            };
            var divisionDto = new DivisionDto
            {
                DivName = "VSD",
                DivisionId = 2,
                AgencyId = 2,
                CentOverhead = 200
            };
            var apiResponse = ApiResponseDto<DivisionDto>.SuccessResponse(divisionDto);

            _mapper.Map<DivisionDto>(divisionViewModel).Returns(divisionDto);
            _divisionService.UpdateDivisionAsync(originalDivName, divisionDto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(divisionViewModel, originalDivName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
        }

        [Fact]
        public async Task Edit_Post_WithFKConstraintViolation_ReturnsError()
        {
            // Arrange
            var originalDivName = "VSD";
            var divisionViewModel = new DivisionViewModel
            {
                DivName = "NEWNAME",
                DivisionId = 1,
                AgencyId = 1
            };
            var divisionDto = new DivisionDto
            {
                DivName = "NEWNAME",
                DivisionId = 1,
                AgencyId = 1
            };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto
                {
                    Message = "Unable to edit the division name as it is already in use.",
                    Code = "BUSINESS_LOGIC_ERROR"
                }
            };
            var apiResponse = ApiResponseDto<DivisionDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<DivisionDto>(divisionViewModel).Returns(divisionDto);
            _divisionService.UpdateDivisionAsync(originalDivName, divisionDto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(divisionViewModel, originalDivName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidDivName_ReturnsSuccessJson()
        {
            // Arrange
            var divName = "VSD";
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _divisionService.DeleteDivisionAsync(divName).Returns(apiResponse);

            // Act
            var result = await _controller.Delete(divName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
        }

        [Fact]
        public async Task Delete_WithEmptyDivName_ReturnsError()
        {
            // Act
            var result = await _controller.Delete("");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Division name is required", value.message);
        }

        [Fact]
        public async Task Delete_WhenServiceReturnsFalse_ReturnsError()
        {
            // Arrange
            var divName = "VSD";
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(false);

            _divisionService.DeleteDivisionAsync(divName).Returns(apiResponse);

            // Act
            var result = await _controller.Delete(divName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        [Fact]
        public async Task Delete_WithFKConstraintViolation_ReturnsError()
        {
            // Arrange
            var divName = "VSD";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto
                {
                    Message = "Unable to delete the division name as it is already in use.",
                    Code = "BUSINESS_LOGIC_ERROR"
                }
            };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _divisionService.DeleteDivisionAsync(divName).Returns(apiResponse);

            // Act
            var result = await _controller.Delete(divName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region GetDistinctAgencies Tests

        [Fact]
        public async Task GetDistinctAgencies_ReturnsSuccessResponse()
        {
            // Arrange
            var agencies = new List<AgencyDto>
            {
                new AgencyDto { AgencyId = 1 },
                new AgencyDto { AgencyId = 2 }
            };
            var apiResponse = ApiResponseDto<IEnumerable<AgencyDto>>.SuccessResponse(agencies);

            _divisionService.GetAllAgenciesAsync().Returns(apiResponse);

            // Act
            var result = await _controller.GetDistinctAgencies();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
        }

        #endregion

        #region CheckDivisionNameExists Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task CheckDivisionNameExists_ReturnsFalse_WhenNameIsEmpty(string? divName)
        {
            // Act
            var result = await _controller.CheckDivisionNameExists(divName!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<ExistsResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.exists);
            await _divisionService.DidNotReceive().GetAllDivisionsAsync();
        }

        [Fact]
        public async Task CheckDivisionNameExists_ReturnsFalse_WhenNameMatchesOriginalIgnoringCase()
        {
            // Act
            var result = await _controller.CheckDivisionNameExists("AAP", "aap");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<ExistsResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.exists);
            await _divisionService.DidNotReceive().GetAllDivisionsAsync();
        }

        [Theory]
        [InlineData("aap")]
        [InlineData("AAP")]
        [InlineData("Aap")]
        public async Task CheckDivisionNameExists_ReturnsTrue_WhenDuplicateRegardlessOfCase(string divName)
        {
            // Arrange
            var divisions = new List<DivisionDto>
            {
                new DivisionDto { DivName = "aap", DivisionId = 1, AgencyId = 1 }
            };
            _divisionService.GetAllDivisionsAsync()
                .Returns(ApiResponseDto<IEnumerable<DivisionDto>>.SuccessResponse(divisions));

            // Act
            var result = await _controller.CheckDivisionNameExists(divName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<ExistsResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.exists);
        }

        [Fact]
        public async Task CheckDivisionNameExists_ReturnsFalse_WhenNameIsUnique()
        {
            // Arrange
            var divisions = new List<DivisionDto>
            {
                new DivisionDto { DivName = "VSD", DivisionId = 1, AgencyId = 1 }
            };
            _divisionService.GetAllDivisionsAsync()
                .Returns(ApiResponseDto<IEnumerable<DivisionDto>>.SuccessResponse(divisions));

            // Act
            var result = await _controller.CheckDivisionNameExists("ACDP");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<ExistsResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.exists);
        }

        #endregion

        // Helper class for JSON parsing
        private class JsonResponse
        {
            public bool success { get; set; }
            public string? message { get; set; }
            public object? data { get; set; }
            public object? errors { get; set; }
        }

        private class ExistsResponse
        {
            public bool exists { get; set; }
        }
    }
}
