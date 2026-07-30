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
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.CostCentreMaintenanceControllerTest
{
    public class CostCentreMaintenanceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly ICostCentreService _costCentreService;
        private readonly CostCentreMaintenanceController _controller;

        public CostCentreMaintenanceControllerTests()
        {
            _mapper             = Substitute.For<IMapper>();
            _costCentreService  = Substitute.For<ICostCentreService>();
            _controller         = new CostCentreMaintenanceController(_mapper, _costCentreService);
        }

        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        private static ApiResponseDto<List<CostCentreDto>> BuildPagedSuccess(int count = 1) =>
            ApiResponseDto<List<CostCentreDto>>.SuccessResponse(
                Enumerable.Range(0, count)
                    .Select(i => new CostCentreDto { CostCentreNo = 100.0 + i, ProfitCentre = "PC01", FpsYear = 2024 })
                    .ToList(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = count });

        private static ApiResponseDto<List<CostCentreWorkgroupDto>> BuildWorkgroupSuccess() =>
            ApiResponseDto<List<CostCentreWorkgroupDto>>.SuccessResponse(
                new List<CostCentreWorkgroupDto> { new() { ProfitCentre = "PC01" } });

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new CostCentreMaintenanceController(null!, _costCentreService));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenCostCentreServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new CostCentreMaintenanceController(_mapper, null!));
        }

        #endregion

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            // Arrange
            var pagedResponse     = BuildPagedSuccess();
            var workgroupResponse = BuildWorkgroupSuccess();

            _costCentreService.GetAllCostCentresPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedResponse);
            _costCentreService.GetAllCostCentresAsync().Returns(workgroupResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _mapper.Map<List<CostCentreItem>>(Arg.Any<List<CostCentreDto>>()).Returns(new List<CostCentreItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Index_CallsGetAllCostCentresAsync_ForDropdowns()
        {
            // Arrange
            _costCentreService.GetAllCostCentresPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(BuildPagedSuccess());
            _costCentreService.GetAllCostCentresAsync().Returns(BuildWorkgroupSuccess());
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _mapper.Map<List<CostCentreItem>>(Arg.Any<List<CostCentreDto>>()).Returns(new List<CostCentreItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            await _controller.Index();

            // Assert
            await _costCentreService.Received(1).GetAllCostCentresAsync();
        }

        #endregion

        #region LoadCostCentreGrid Tests

        [Fact]
        public async Task LoadCostCentreGrid_InvalidModelState_ReturnsJsonError()
        {
            // Arrange
            _controller.ModelState.AddModelError("Filter", "Required");
            var request = new PaginationFilter<string> { Filter = "{}", SortBy = "CostCentreNo" };

            // Act
            var result = await _controller.LoadCostCentreGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        [Fact]
        public async Task LoadCostCentreGrid_ValidRequest_ReturnsPartialViewResult()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", SortBy = "CostCentreNo", Descending = false };

            _costCentreService.GetAllCostCentresPagedAsync(Arg.Any<QueryParameters<string>>())
                .Returns(BuildPagedSuccess());
            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _mapper.Map<List<CostCentreItem>>(Arg.Any<List<CostCentreDto>>()).Returns(new List<CostCentreItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadCostCentreGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            Assert.IsType<DataGridConfig<CostCentreItem>>(partialView.Model);
        }

        [Fact]
        public async Task LoadCostCentreGrid_ServiceReturnsEmptyPage_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request  = new PaginationFilter<string> { Filter = "{}", SortBy = "CostCentreNo" };
            var response = ApiResponseDto<List<CostCentreDto>>.SuccessResponse(
                new List<CostCentreDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });

            _costCentreService.GetAllCostCentresPagedAsync(Arg.Any<QueryParameters<string>>()).Returns(response);
            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _mapper.Map<List<CostCentreItem>>(Arg.Any<List<CostCentreDto>>()).Returns(new List<CostCentreItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadCostCentreGrid(request);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var config      = Assert.IsType<DataGridConfig<CostCentreItem>>(partialView.Model);
            Assert.Empty(config.Data);
        }

        #endregion

        #region Create GET Tests

        // because PopulatePartialDropdownsAsync() is called before returning the partial view
        [Fact]
        public async Task Create_Get_ReturnsPartialViewWithEmptyCostCentreItem()
        {
            // Arrange
            _costCentreService.GetAllCostCentresAsync().Returns(BuildWorkgroupSuccess());

            // Act
            var result = await _controller.Create();

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditCostCentre", partialView.ViewName);
            Assert.IsType<CostCentreItem>(partialView.Model);
        }

        #endregion

        #region Create POST Tests

        [Fact]
        public async Task Create_Post_NullDto_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.Create((CostCentreDto)null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        [Fact]
        public async Task Create_Post_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("ProfitCentre", "Required");
            var dto = new CostCentreDto { CostCentreNo = 100, ProfitCentre = "", FpsYear = 2024 };

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        [Fact]
        public async Task Create_Post_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto         = new CostCentreDto { CostCentreNo = 100, ProfitCentre = "PC01", FpsYear = 2024 };
            var apiResponse = ApiResponseDto<CostCentreDto>.SuccessResponse(dto);
            _costCentreService.CreateCostCentreAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.True(value!.success);
            Assert.Equal("Cost Centre created successfully", value.message);
        }

        [Fact]
        public async Task Create_Post_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto    = new CostCentreDto { CostCentreNo = 100, ProfitCentre = "PC01", FpsYear = 2024 };
            var errors = new List<ApiErrorDto> { new() { Code = "CONFLICT", Message = "Already exists." } };
            var apiResponse = ApiResponseDto<CostCentreDto>.FailureResponse(errors, new ApiMetaDto());
            _costCentreService.CreateCostCentreAsync(dto).Returns(apiResponse);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            Assert.Equal("Already exists.", value.message);
        }

        [Fact]
        public async Task Create_Post_ServiceCallsCreateCostCentreAsync_Once()
        {
            // Arrange
            var dto         = new CostCentreDto { CostCentreNo = 100, ProfitCentre = "PC01", FpsYear = 2024 };
            var apiResponse = ApiResponseDto<CostCentreDto>.SuccessResponse(dto);
            _costCentreService.CreateCostCentreAsync(dto).Returns(apiResponse);

            // Act
            await _controller.Create(dto);

            // Assert
            await _costCentreService.Received(1).CreateCostCentreAsync(dto);
        }

        #endregion

        #region Edit GET Tests

        [Fact]
        public async Task Edit_Get_EmptyId_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Edit(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            Assert.Equal("Cost Centre number is required", value.message);
        }

        [Fact]
        public async Task Edit_Get_InvalidNumericId_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Edit("not-a-number");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        [Fact]
        public async Task Edit_Get_ServiceReturnsSuccess_ReturnsPartialViewWithPopulatedItem()
        {
            // Arrange
            var dto         = new CostCentreDto { CostCentreNo = 100.0, ProfitCentre = "PC01", FpsYear = 2024 };
            var item        = new CostCentreItem { CostCentreNo = 100.0, ProfitCentre = "PC01" };
            var apiResponse = ApiResponseDto<CostCentreDto>.SuccessResponse(dto);

            _costCentreService.GetCostCentreByIdAsync(100.0).Returns(apiResponse);
            _mapper.Map<CostCentreItem>(dto).Returns(item);
            // PopulatePartialDropdownsAsync() is now called inside Edit GET before returning partial
            _costCentreService.GetAllCostCentresAsync().Returns(BuildWorkgroupSuccess());

            // Act
            var result = await _controller.Edit("100");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditCostCentre", partialView.ViewName);
            var model = Assert.IsType<CostCentreItem>(partialView.Model);
            Assert.Equal(100.0, model.CostCentreNo);
        }

        [Fact]
        public async Task Edit_Get_ServiceReturnsFailure_ReturnsJsonError()
        {
            // Arrange
            var errors      = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found." } };
            var apiResponse = ApiResponseDto<CostCentreDto>.FailureResponse(errors, new ApiMetaDto());
            _costCentreService.GetCostCentreByIdAsync(999.0).Returns(apiResponse);

            // Act
            var result = await _controller.Edit("999");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        #endregion

        #region Edit POST Tests

        [Fact]
        public async Task Edit_Post_NullDto_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.Edit("100", (CostCentreDto)null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        [Fact]
        public async Task Edit_Post_InvalidId_ReturnsJsonError()
        {
            // Arrange
            var dto = new CostCentreDto { CostCentreNo = 100, ProfitCentre = "PC01", FpsYear = 2024 };

            // Act
            var result = await _controller.Edit("not-a-number", dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        [Fact]
        public async Task Edit_Post_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto         = new CostCentreDto { CostCentreNo = 100, ProfitCentre = "PC02", FpsYear = 2024 };
            var apiResponse = ApiResponseDto<CostCentreDto>.SuccessResponse(dto);
            _costCentreService.UpdateCostCentreAsync(100.0, dto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit("100", dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.True(value!.success);
            Assert.Equal("Cost Centre updated successfully", value.message);
        }

        [Fact]
        public async Task Edit_Post_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto    = new CostCentreDto { CostCentreNo = 100, ProfitCentre = "PC02", FpsYear = 2024 };
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found." } };
            var apiResponse = ApiResponseDto<CostCentreDto>.FailureResponse(errors, new ApiMetaDto());
            _costCentreService.UpdateCostCentreAsync(100.0, dto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit("100", dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        [Fact]
        public async Task Edit_Post_CallsUpdateCostCentreAsync_Once()
        {
            // Arrange
            var dto         = new CostCentreDto { CostCentreNo = 100, ProfitCentre = "PC02", FpsYear = 2024 };
            var apiResponse = ApiResponseDto<CostCentreDto>.SuccessResponse(dto);
            _costCentreService.UpdateCostCentreAsync(100.0, dto).Returns(apiResponse);

            // Act
            await _controller.Edit("100", dto);

            // Assert
            await _costCentreService.Received(1).UpdateCostCentreAsync(100.0, dto);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_EmptyId_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Delete(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            Assert.Equal("Cost Centre number is required", value.message);
        }

        [Fact]
        public async Task Delete_InvalidId_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Delete("not-a-number");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        [Fact]
        public async Task Delete_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _costCentreService.DeleteCostCentreAsync(100.0).Returns(apiResponse);

            // Act
            var result = await _controller.Delete("100");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.True(value!.success);
            Assert.Equal("Cost Centre deleted successfully", value.message);
        }

        [Fact]
        public async Task Delete_ServiceReturnsDbPostgresError_ReturnsUserFriendlyFKMessage()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new() { Code = "DB_POSTGRES_ERROR", Message = "A database error occurred." }
            };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _costCentreService.DeleteCostCentreAsync(100.0).Returns(apiResponse);

            // Act
            var result = await _controller.Delete("100");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            Assert.Equal("This cost centre cannot be deleted because it is referenced by other records.", value.message);
        }

        [Fact]
        public async Task Delete_ServiceReturnsOtherError_PropagatesErrorMessage()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new() { Code = "SOME_ERROR", Message = "Some specific error." }
            };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _costCentreService.DeleteCostCentreAsync(100.0).Returns(apiResponse);

            // Act
            var result = await _controller.Delete("100");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            Assert.Equal("Some specific error.", value.message);
        }

        [Fact]
        public async Task Delete_ServiceReturnsSuccessFalse_ReturnsFallbackMessage()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(false);
            _costCentreService.DeleteCostCentreAsync(100.0).Returns(apiResponse);

            // Act
            var result = await _controller.Delete("100");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value      = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            Assert.Equal("Unable to delete the cost centre as it may be in use.", value.message);
        }

        [Fact]
        public async Task Delete_CallsDeleteCostCentreAsync_Once()
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _costCentreService.DeleteCostCentreAsync(100.0).Returns(apiResponse);

            // Act
            await _controller.Delete("100");

            // Assert
            await _costCentreService.Received(1).DeleteCostCentreAsync(100.0);
        }

        #endregion

        private class JsonResponse
        {
            public bool success { get; set; }
            public string? message { get; set; }
            public object? data { get; set; }
            public object? errors { get; set; }
        }
    }
}
