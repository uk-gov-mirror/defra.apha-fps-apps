using System.Text.Json;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.WorkgroupMaintenanceControllerTest
{
    public class WorkgroupMaintenanceControllerTests
    {
        private const string TestWorkGroupName  = "WG001";
        private const string TestProfitCentre   = "PC01";
        private const string TestDescription    = "Test Workgroup";

        private readonly IMapper _mapper;
        private readonly IWorkgroupMaintenanceService _service;
        private readonly WorkgroupMaintenanceController _controller;

        public WorkgroupMaintenanceControllerTests()
        {
            _mapper     = Substitute.For<IMapper>();
            _service    = Substitute.For<IWorkgroupMaintenanceService>();
            _controller = new WorkgroupMaintenanceController(_mapper, _service);
        }

        // Helper method to extract properties from JsonResult
        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private class JsonResultSuccess
        {
            public bool    Success { get; set; }
            public string? Message { get; set; }
            public object? Data    { get; set; }
        }

        private class JsonResultError
        {
            public bool    Success { get; set; }
            public string? Message { get; set; }
            public object? Errors  { get; set; }
        }

        private static WorkgroupMaintenanceItem BuildItem(string name = TestWorkGroupName) =>
            new()
            {
                WorkGroupName = name,
                ProfitCentre  = TestProfitCentre,
                Description   = TestDescription
            };

        private static WorkGroupDto BuildDto(string name = TestWorkGroupName) =>
            new()
            {
                WorkGroupName = name,
                ProfitCentre  = TestProfitCentre,
                Description   = TestDescription
            };

        private static ApiResponseDto<WorkGroupDto> BuildSuccessResponse(string name = TestWorkGroupName) =>
            new() { Success = true, Data = BuildDto(name) };

        private static ApiResponseDto<WorkGroupDto> BuildFailureResponse(string errorMessage = "An error occurred") =>
            new()
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = errorMessage, Code = "ERR" } }
            };

        #region Index Tests

        [Fact]
        public async Task Index_ServiceReturnsData_ReturnsViewResultWithPopulatedModel()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pagedData = new ApiResponseDto<List<WorkGroupDto>>
            {
                Success    = true,
                Data       = new List<WorkGroupDto> { BuildDto() },
                Pagination = new PaginationDto { TotalRecords = 1, PageNumber = 1, PageSize = 10 }
            };

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                   .Returns(queryParameters);
            _service.GetPagedAsync(queryParameters).Returns(pagedData);
            _mapper.Map<List<WorkgroupMaintenanceItem>>(Arg.Any<List<WorkGroupDto>>())
                   .Returns(new List<WorkgroupMaintenanceItem> { BuildItem() });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                   .Returns(new PaginationModel { TotalRecords = 1, PageNumber = 1, PageSize = 10 });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkgroupMaintenanceViewModel>(viewResult.Model);
            Assert.NotNull(model.WorkgroupGrid);
        }

        #endregion

        #region LoadWorkgroupGrid Tests

        [Fact]
        public async Task LoadWorkgroupGrid_ValidRequest_ReturnsPartialViewWithDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            var pagedData = new ApiResponseDto<List<WorkGroupDto>>
            {
                Success    = true,
                Data       = new List<WorkGroupDto> { BuildDto() },
                Pagination = new PaginationDto { TotalRecords = 1, PageNumber = 1, PageSize = 10 }
            };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _service.GetPagedAsync(queryParameters).Returns(pagedData);
            _mapper.Map<List<WorkgroupMaintenanceItem>>(Arg.Any<List<WorkGroupDto>>())
                   .Returns(new List<WorkgroupMaintenanceItem> { BuildItem() });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                   .Returns(new PaginationModel { TotalRecords = 1, PageNumber = 1, PageSize = 10 });

            // Act
            var result = await _controller.LoadWorkgroupGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            var config = Assert.IsType<DataGridConfig<WorkgroupMaintenanceItem>>(partialViewResult.Model);
            Assert.Single(config.Data);
        }

        [Fact]
        public async Task LoadWorkgroupGrid_ServiceReturnsEmptyPage_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            var pagedData = new ApiResponseDto<List<WorkGroupDto>>
            {
                Success    = true,
                Data       = new List<WorkGroupDto>(),
                Pagination = new PaginationDto { TotalRecords = 0, PageNumber = 1, PageSize = 10 }
            };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _service.GetPagedAsync(queryParameters).Returns(pagedData);
            _mapper.Map<List<WorkgroupMaintenanceItem>>(Arg.Any<List<WorkGroupDto>>())
                   .Returns(new List<WorkgroupMaintenanceItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                   .Returns(new PaginationModel { TotalRecords = 0, PageNumber = 1, PageSize = 10 });

            // Act
            var result = await _controller.LoadWorkgroupGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<WorkgroupMaintenanceItem>>(partialViewResult.Model);
            Assert.Empty(config.Data);
        }

        [Fact]
        public async Task LoadWorkgroupGrid_InvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("Page", "Required");

            // Act
            var result = await _controller.LoadWorkgroupGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.NotNull(value.Message);
        }

        #endregion

        #region Create Tests

        [Fact]
        public void Create_Get_ReturnsPartialViewWithEmptyItem()
        {
            // Act
            var result = _controller.Create();

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditWorkgroup", partialViewResult.ViewName);
            Assert.IsType<WorkgroupMaintenanceItem>(partialViewResult.Model);
        }

        [Fact]
        public async Task Create_Post_ValidDto_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item     = BuildItem();
            var dto      = BuildDto();
            var response = BuildSuccessResponse();

            _mapper.Map<WorkGroupDto>(item).Returns(dto);
            _service.CreateAsync(dto).Returns(response);

            // Act
            var result = await _controller.Create(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultSuccess>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.Success);
        }

        [Fact]
        public async Task Create_Post_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var item = new WorkgroupMaintenanceItem { WorkGroupName = TestWorkGroupName };
            _controller.ModelState.AddModelError("ProfitCentre", "ResourceCentre is required");

            // Act
            var result = await _controller.Create(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.NotNull(value.Errors);
        }

        [Fact]
        public async Task Create_Post_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var item     = BuildItem();
            var dto      = BuildDto();
            var response = BuildFailureResponse("Failed to create WorkGroup.");

            _mapper.Map<WorkGroupDto>(item).Returns(dto);
            _service.CreateAsync(dto).Returns(response);

            // Act
            var result = await _controller.Create(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.Equal("Failed to create WorkGroup.", value.Message);
        }

        #endregion

        #region Edit Tests

        [Fact]
        public async Task Edit_Get_ValidWorkGroupName_ServiceReturnsData_ReturnsPartialView()
        {
            // Arrange
            var dto  = BuildDto();
            var item = BuildItem();
            var response = new ApiResponseDto<WorkGroupDto> { Success = true, Data = dto };

            _service.GetByWorkGroupNameAsync(TestWorkGroupName).Returns(response);
            // TRANSFORMENGINE: mapper must use the same dto instance that is stored in response.Data
            _mapper.Map<WorkgroupMaintenanceItem>(dto).Returns(item);

            // Act
            var result = await _controller.Edit(TestWorkGroupName);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditWorkgroup", partialViewResult.ViewName);
            var model = Assert.IsType<WorkgroupMaintenanceItem>(partialViewResult.Model);
            Assert.Equal(TestWorkGroupName, model.WorkGroupName);
        }

        [Fact]
        public async Task Edit_Get_NullOrWhiteSpaceWorkGroupName_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Edit(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
        }

        [Fact]
        public async Task Edit_Get_ServiceReturnsFailure_ReturnsJsonError()
        {
            // Arrange
            var response = BuildFailureResponse("Not found");
            _service.GetByWorkGroupNameAsync("NOTEXIST").Returns(response);

            // Act
            var result = await _controller.Edit("NOTEXIST");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
        }

        [Fact]
        public async Task Edit_Post_ValidDto_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var item     = BuildItem();
            var dto      = BuildDto();
            var response = BuildSuccessResponse();

            _mapper.Map<WorkGroupDto>(item).Returns(dto);
            _service.UpdateAsync(TestWorkGroupName, dto).Returns(response);

            // Act
            var result = await _controller.Edit(item, null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultSuccess>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.Success);
        }

        [Fact]
        public async Task Edit_Post_UsesOriginalWorkGroupNameWhenProvided()
        {
            // Arrange
            var originalName = "WG_ORIGINAL";
            var item         = BuildItem("WG_RENAMED");
            var dto          = BuildDto("WG_RENAMED");
            var response     = BuildSuccessResponse("WG_RENAMED");

            _mapper.Map<WorkGroupDto>(item).Returns(dto);
            _service.UpdateAsync(originalName, dto).Returns(response);

            // Act
            await _controller.Edit(item, originalName);

            // Assert
            await _service.Received(1).UpdateAsync(originalName, dto);
        }

        [Fact]
        public async Task Edit_Post_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var item = new WorkgroupMaintenanceItem { WorkGroupName = TestWorkGroupName };
            _controller.ModelState.AddModelError("ProfitCentre", "Required");

            // Act
            var result = await _controller.Edit(item, null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.NotNull(value.Errors);
        }

        [Fact]
        public async Task Edit_Post_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var item     = BuildItem();
            var dto      = BuildDto();
            var response = BuildFailureResponse("Failed to update WorkGroup.");

            _mapper.Map<WorkGroupDto>(item).Returns(dto);
            _service.UpdateAsync(TestWorkGroupName, dto).Returns(response);

            // Act
            var result = await _controller.Edit(item, null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.Equal("Failed to update WorkGroup.", value.Message);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_ValidWorkGroupName_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var response = new ApiResponseDto<bool> { Success = true };
            _service.DeleteAsync(TestWorkGroupName).Returns(response);

            // Act
            var result = await _controller.Delete(TestWorkGroupName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultSuccess>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.Success);
        }

        [Fact]
        public async Task Delete_NullOrWhiteSpaceWorkGroupName_ReturnsJsonError()
        {
            // Act
            var result = await _controller.Delete(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
        }

        [Fact]
        public async Task Delete_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var response = new ApiResponseDto<bool>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Failed to delete WorkGroup." } }
            };
            _service.DeleteAsync(TestWorkGroupName).Returns(response);

            // Act
            var result = await _controller.Delete(TestWorkGroupName);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.Equal("Failed to delete WorkGroup.", value.Message);
        }

        #endregion

        #region GetProfitCentres Tests

        [Fact]
        public async Task GetProfitCentres_ServiceReturnsSuccess_ReturnsJsonWithData()
        {
            // Arrange
            var response = new ApiResponseDto<List<string>>
            {
                Success = true,
                Data    = new List<string> { "PC01", "PC02" }
            };
            _service.GetProfitCentresAsync().Returns(response);

            // Act
            var result = await _controller.GetProfitCentres();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultSuccess>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.Success);
        }

        [Fact]
        public async Task GetProfitCentres_ServiceReturnsFailure_ReturnsJsonError()
        {
            // Arrange
            var response = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Failed to load profit centres" } }
            };
            _service.GetProfitCentresAsync().Returns(response);

            // Act
            var result = await _controller.GetProfitCentres();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
        }

        #endregion

        #region GetOwners Tests

        [Fact]
        public async Task GetOwners_ServiceReturnsSuccess_ReturnsJsonWithData()
        {
            // Arrange
            var response = new ApiResponseDto<List<OwnerDto>>
            {
                Success = true,
                Data    = new List<OwnerDto> { new() { Name = "Alice Smith" } }
            };
            _service.GetOwnersAsync().Returns(response);

            // Act
            var result = await _controller.GetOwners();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultSuccess>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.Success);
        }

        [Fact]
        public async Task GetOwners_ServiceReturnsFailure_ReturnsJsonError()
        {
            // Arrange
            var response = new ApiResponseDto<List<OwnerDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Failed to load owners" } }
            };
            _service.GetOwnersAsync().Returns(response);

            // Act
            var result = await _controller.GetOwners();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
        }

        #endregion

        #region GetCostCentres Tests

        [Fact]
        public async Task GetCostCentres_ValidProfitCentre_ServiceReturnsSuccess_ReturnsJsonWithData()
        {
            // Arrange
            var response = new ApiResponseDto<List<double?>>
            {
                Success = true,
                Data    = new List<double?> { 100.0, 200.0 }
            };
            _service.GetCostCentresAsync(TestProfitCentre).Returns(response);

            // Act
            var result = await _controller.GetCostCentres(TestProfitCentre);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultSuccess>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.Success);
        }

        [Fact]
        public async Task GetCostCentres_NullOrWhiteSpaceProfitCentre_ReturnsJsonError()
        {
            // Act
            var result = await _controller.GetCostCentres(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
        }

        [Fact]
        public async Task GetCostCentres_ServiceReturnsFailure_ReturnsJsonError()
        {
            // Arrange
            var response = new ApiResponseDto<List<double?>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Failed to load cost centres" } }
            };
            _service.GetCostCentresAsync(TestProfitCentre).Returns(response);

            // Act
            var result = await _controller.GetCostCentres(TestProfitCentre);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
        }

        #endregion
    }
}
