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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProgramMaintenanceControllerTest
{
    public class ProgramMaintenanceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProgramService _programService;
        private readonly IEmployeeService _employeeService;
        private readonly ProgramMaintenanceController _controller;

        public ProgramMaintenanceControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _programService = Substitute.For<IProgramService>();
            _employeeService = Substitute.For<IEmployeeService>();
            _controller = new ProgramMaintenanceController(_mapper, _programService, _employeeService);
        }

        // Helper method to extract properties from JsonResult
        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        #region Index Tests

        [Fact]
        public async Task Index_ReturnsViewResult_WithProgramGrid()
        {
            // Arrange
            var programs = new List<ProgramDto>
            {
                new ProgramDto { ProgramNo = "P001", ProgramName = "Program One", Directorate = "IT" },
                new ProgramDto { ProgramNo = "P002", ProgramName = "Program Two", Directorate = "Finance" }
            };
            var programViewModels = new List<ProgramViewModel>
            {
                new ProgramViewModel { ProgramNo = "P001", ProgramName = "Program One", Directorate = "IT" },
                new ProgramViewModel { ProgramNo = "P002", ProgramName = "Program Two", Directorate = "Finance" }
            };

            var apiResponse = ApiResponseDto<List<ProgramDto>>.SuccessResponse(programs, new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 2 });

            _programService.GetAllProgramsAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<List<ProgramViewModel>>(Arg.Any<List<ProgramDto>>()).Returns(programViewModels);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel { PageNumber = 1, PageSize = 15, TotalRecords = 2 });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<DataGridConfig<ProgramViewModel>>(viewResult.Model);
            Assert.Equal("programGrid", model.GridId);
            Assert.Equal("Program Maintenance", model.Title);
        }

        [Fact]
        public async Task Index_CallsGetAllProgramsAsync_WithDefaultParameters()
        {
            // Arrange
            var apiResponse = ApiResponseDto<List<ProgramDto>>.SuccessResponse(new List<ProgramDto>(), new PaginationDto());
            _programService.GetAllProgramsAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<List<ProgramViewModel>>(Arg.Any<List<ProgramDto>>()).Returns(new List<ProgramViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            await _controller.Index();

            // Assert
            await _programService.Received(1).GetAllProgramsAsync(Arg.Any<QueryParameters<string>>());
        }

        #endregion

        #region LoadProgramGrid Tests

        [Fact]
        public async Task LoadProgramGrid_WithValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var programs = new List<ProgramDto>
            {
                new ProgramDto { ProgramNo = "P001", ProgramName = "Updated Program", Directorate = "Finance" }
            };
            var apiResponse = ApiResponseDto<List<ProgramDto>>.SuccessResponse(programs, new PaginationDto { PageNumber = 1, PageSize = 10 });

            _programService.GetAllProgramsAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());
            _mapper.Map<List<ProgramViewModel>>(Arg.Any<List<ProgramDto>>()).Returns(new List<ProgramViewModel> { new ProgramViewModel { ProgramNo = "P001", ProgramName = "Updated Program", Directorate = "Finance" } });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProgramGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            Assert.IsType<DataGridConfig<ProgramViewModel>>(partialViewResult.Model);
        }

        [Fact]
        public async Task LoadProgramGrid_WithInvalidModelState_ReturnsJsonError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _controller.ModelState.AddModelError("Test", "Test error");

            // Act
            var result = await _controller.LoadProgramGrid(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid request data", value.message);
        }

        [Fact]
        public async Task LoadProgramGrid_WithNullFilter_HandlesGracefully()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = null };
            var apiResponse = ApiResponseDto<List<ProgramDto>>.SuccessResponse(new List<ProgramDto>(), new PaginationDto());

            _programService.GetAllProgramsAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());
            _mapper.Map<List<ProgramViewModel>>(Arg.Any<List<ProgramDto>>()).Returns(new List<ProgramViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProgramGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.NotNull(partialViewResult);
        }

        #endregion

        #region Create Tests

        [Fact]
        public async Task Create_Get_ReturnsPartialViewWithModel()
        {
            // Arrange           
            var managers = new List<ManagerDto>
            {
                new ManagerDto { Name = "John Manager" }
            };
            var managerResponse = ApiResponseDto<List<ManagerDto>>.SuccessResponse(managers);
                      
            _employeeService.GetAllManagersAsync().Returns(managerResponse);

            // Act
            var result = await _controller.Create();

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditProgram", partialViewResult.ViewName);
            var model = Assert.IsType<ProgramViewModel>(partialViewResult.Model);
            Assert.Equal(string.Empty, model.ProgramNo);
        }              

        #endregion

        #region Edit Tests

        [Fact]
        public async Task Edit_Get_WithValidProgramNo_ReturnsPartialViewWithModel()
        {
            // Arrange
            var programNo = "P001";
            var program = new ProgramDto { ProgramNo = programNo, ProgramName = "Test Program", Directorate = "Finance" };
            var programViewModel = new ProgramViewModel { ProgramNo = programNo, ProgramName = "Test Program", Directorate = "Finance" };
            
            var managers = new List<ManagerDto> { new ManagerDto { Name = "John Manager" } };
            var programResponse = ApiResponseDto<ProgramDto?>.SuccessResponse(program);           
            var managerResponse = ApiResponseDto<List<ManagerDto>>.SuccessResponse(managers);

            _programService.GetProgramByIdAsync(programNo).Returns(programResponse);            
            _employeeService.GetAllManagersAsync().Returns(managerResponse);
            _mapper.Map<ProgramViewModel>(program).Returns(programViewModel);

            // Act
            var result = await _controller.Edit(programNo);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditProgram", partialViewResult.ViewName);
            var model = Assert.IsType<ProgramViewModel>(partialViewResult.Model);
            Assert.Equal(programNo, model.ProgramNo);
        }

        [Fact]
        public async Task Edit_Get_WithNonExistentProgram_ReturnsNotFound()
        {
            // Arrange
            var programNo = "NONEXISTENT";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = ApiResponseDto<ProgramDto?>.FailureResponse(errors, new ApiMetaDto());

            _programService.GetProgramByIdAsync(programNo).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(programNo);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_WithValidModel_ReturnsSuccessJson()
        {
            // Arrange
            var programViewModel = new ProgramViewModel
            {
                ProgramNo = "P001",
                ProgramName = "Updated Program",
                Directorate = "Finance"
            };
            var programDto = new ProgramDto
            {
                ProgramNo = "P001",
                ProgramName = "Updated Program",
                Directorate = "Finance"
            };
            var apiResponse = ApiResponseDto<ProgramDto>.SuccessResponse(programDto);

            _mapper.Map<ProgramDto>(programViewModel).Returns(programDto);
            _programService.UpdateProgramAsync(programDto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(programViewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Program updated successfully.", value.message);
        }

        [Fact]
        public async Task Edit_Post_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var programViewModel = new ProgramViewModel { ProgramNo = "P001", ProgramName = "Updated Program", Directorate = "Finance" };
            var programDto = new ProgramDto { ProgramNo = "P001", ProgramName = "Updated Program", Directorate = "Finance" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var apiResponse = ApiResponseDto<ProgramDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<ProgramDto>(programViewModel).Returns(programDto);
            _programService.UpdateProgramAsync(programDto).Returns(apiResponse);

            // Act
            var result = await _controller.Edit(programViewModel);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidProgramNo_ReturnsSuccessJson()
        {
            // Arrange
            var programNo = "P001";
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _programService.DeleteProgramAsync(programNo).Returns(apiResponse);

            // Act
            var result = await _controller.Delete(programNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Program deleted successfully.", value.message);
        }

        [Fact]
        public async Task Delete_WhenServiceFails_ReturnsJsonError()
        {
            // Arrange
            var programNo = "P001";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Delete failed", Code = "DELETE_ERROR" } };
            var apiResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _programService.DeleteProgramAsync(programNo).Returns(apiResponse);

            // Act
            var result = await _controller.Delete(programNo);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public async Task LoadProgramGrid_ConfiguresGridCorrectly()
        {
            // Arrange
            var request = new PaginationFilter<string>
            {
                Filter = "{}",
                SortBy = "ProgramName",
                Descending = true,
                PageSize = 20
            };
            var apiResponse = ApiResponseDto<List<ProgramDto>>.SuccessResponse(new List<ProgramDto>(), new PaginationDto());

            _programService.GetAllProgramsAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());
            _mapper.Map<List<ProgramViewModel>>(Arg.Any<List<ProgramDto>>()).Returns(new List<ProgramViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProgramGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProgramViewModel>>(partialViewResult.Model);

            Assert.Equal("programGrid", gridConfig.GridId);
            Assert.Equal("Program Maintenance", gridConfig.Title);
            Assert.False(gridConfig.ShowCheckboxColumn);
            Assert.True(gridConfig.ShowPagination);
            Assert.Equal("ProgramNo", gridConfig.KeyProperty);
            Assert.Equal("addProgram", gridConfig.AddFunction);
            Assert.Equal("editProgram", gridConfig.EditFunction);
            Assert.Equal("deleteProgram", gridConfig.DeleteFunction);
            Assert.Equal("getProgramExtraFilters", gridConfig.ExtraFilterMethod);
            Assert.Equal("/FPS/ProgramMaintenance/LoadProgramGrid", gridConfig.BindGridUrl);
        }

        [Fact]
        public async Task Create_Post_CallsMapperAndService_InCorrectOrder()
        {
            // Arrange
            var programViewModel = new ProgramViewModel { ProgramNo = "P001", ProgramName = "Updated Program", Directorate = "Finance" };
            var programDto = new ProgramDto { ProgramNo = "P001", ProgramName = "Updated Program", Directorate = "Finance" };
            var apiResponse = ApiResponseDto<ProgramDto>.SuccessResponse(programDto);

            _mapper.Map<ProgramDto>(programViewModel).Returns(programDto);
            _programService.AddProgramAsync(programDto).Returns(apiResponse);

            // Act
            await _controller.Create(programViewModel);

            // Assert
            _mapper.Received(1).Map<ProgramDto>(programViewModel);
            await _programService.Received(1).AddProgramAsync(programDto);
        }

        [Fact]
        public async Task Edit_Post_CallsMapperAndService_InCorrectOrder()
        {
            // Arrange
            var programViewModel = new ProgramViewModel
            {
                ProgramNo = "P001",
                ProgramName = "Updated Program",
                Directorate = "Finance"
            };
            var programDto = new ProgramDto { ProgramNo = "P001", ProgramName = "Updated Program", Directorate = "Finance" };
            var apiResponse = ApiResponseDto<ProgramDto>.SuccessResponse(programDto);

            _mapper.Map<ProgramDto>(programViewModel).Returns(programDto);
            _programService.UpdateProgramAsync(programDto).Returns(apiResponse);

            // Act
            await _controller.Edit(programViewModel);

            // Assert
            _mapper.Received(1).Map<ProgramDto>(programViewModel);
            await _programService.Received(1).UpdateProgramAsync(programDto);
        }

        [Theory]
        [InlineData("P001")]
        [InlineData("PROG123")]
        [InlineData("TEST")]
        public async Task Delete_WithVariousProgramNos_CallsService(string programNo)
        {
            // Arrange
            var apiResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _programService.DeleteProgramAsync(programNo).Returns(apiResponse);

            // Act
            await _controller.Delete(programNo);

            // Assert
            await _programService.Received(1).DeleteProgramAsync(programNo);
        }

        [Fact]
        public async Task LoadProgramGrid_WithEmptyDataResponse_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var apiResponse = ApiResponseDto<List<ProgramDto>>.SuccessResponse(new List<ProgramDto>(), new PaginationDto());

            _programService.GetAllProgramsAsync(Arg.Any<QueryParameters<string>>()).Returns(apiResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(new QueryParameters<string>());
            _mapper.Map<List<ProgramViewModel>>(Arg.Any<List<ProgramDto>>()).Returns(new List<ProgramViewModel>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadProgramGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<ProgramViewModel>>(partialViewResult.Model);
            Assert.Empty(gridConfig.Data);
        }

        #endregion

        // Helper class to deserialize JSON responses
        private class JsonResponse
        {
            public bool success { get; set; }
            public string? message { get; set; }
            public object? data { get; set; }
            public object? errors { get; set; }
        }
    }
}