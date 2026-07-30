using Apha.Common.Utilities.ExcelExport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.SubContractRmsControllerTest
{
    public class SubContractRmsControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectSubContractService _subContractService;
        private readonly IProjectService _projectService;
        private readonly IMonthService _monthService;
        private readonly IExcelExportService _excelExportService;
        private readonly SubContractRmsController _controller;

        public SubContractRmsControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _subContractService = Substitute.For<IProjectSubContractService>();
            _projectService = Substitute.For<IProjectService>();
            _monthService = Substitute.For<IMonthService>();
            _excelExportService = Substitute.For<IExcelExportService>();

            _controller = new SubContractRmsController(
                _mapper,
                _subContractService,
                _projectService,
                _monthService,
                _excelExportService);

            SetupDefaultDependencies();
        }

        private void SetupDefaultDependencies()
        {
            _projectService.GetAllPactProjectsAsync().Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([]));
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse([]));

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<SubContractRmsItem>>(Arg.Any<List<ProjectSubContractDto>>())
                .Returns([]);
            _mapper.Map<List<SubContractRmsFailedItem>>(Arg.Any<List<SubContractRmsImportRowDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            _subContractService.GetPagedProjectSubContractsManualAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Returns(ApiResponseDto<List<ProjectSubContractDto>>.SuccessResponse([], new PaginationDto()));
            _subContractService.GetFailedSubContractRmsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<SubContractRmsImportRowDto>>.SuccessResponse([], new PaginationDto()));
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        [Fact]
        public async Task Index_WithMonthFilter_ReturnsViewWithViewModel()
        {
            // Act
            var result = await _controller.Index(6);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SubContractRmsViewModel>(viewResult.Model);
            Assert.Equal(6, model.Month);
            Assert.NotNull(model.SubContractsGrid);
            Assert.NotNull(model.FailedSubContractsGrid);
        }

        [Fact]
        public async Task LoadRmsSubContractsGrid_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("request", "invalid");

            // Act
            var result = await _controller.LoadRmsSubContractsGrid(new PaginationFilter<string>(), 3);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadRmsSubContractsGrid_WithMonthFilter_MergesMonthIntoFilter()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };

            // Act
            var result = await _controller.LoadRmsSubContractsGrid(request, 7);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            Assert.Contains("\"Month\":\"7\"", request.Filter);
        }

        [Fact]
        public async Task GetSubContractRms_IdIsZero_ReturnsPartialViewWithNewModel()
        {
            // Act
            var result = await _controller.GetSubContractRms(0, 5);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditSubContractRms", partial.ViewName);
            var model = Assert.IsType<SubContractRmsItem>(partial.Model);
            Assert.Equal(5, model.Month);
            Assert.Equal(0, model.SubContCounter);
        }

        [Fact]
        public async Task GetSubContractRms_WhenServiceFails_ReturnsNotFound()
        {
            // Arrange
            _subContractService.GetByIdAsync(10)
                .Returns(ApiResponseDto<ProjectSubContractDto>.FailureResponse([], new ApiMetaDto()));

            // Act
            var result = await _controller.GetSubContractRms(10, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task SaveSubContractRms_WhenCreateSucceeds_ReturnsSuccessJson()
        {
            // Arrange
            var model = new SubContractRmsItem { SubContCounter = 0, Project = "P1", Month = 1 };
            var dto = new ProjectSubContractDto { SubContCounter = 0, Project = "P1" };

            _mapper.Map<ProjectSubContractDto>(model).Returns(dto);
            _subContractService.CreateAsync(dto).Returns(ApiResponseDto<ProjectSubContractDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.SaveSubContractRms(model);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(json);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Sub Contract saved successfully.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveSubContractRms_WhenModelStateInvalid_ReturnsValidationFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("Project", "Project is required");

            // Act
            var result = await _controller.SaveSubContractRms(new SubContractRmsItem());

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(json);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Please correct the errors below.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task DeleteSubContractRms_WhenServiceSucceeds_ReturnsSuccessJson()
        {
            // Arrange
            _subContractService.DeleteAsync(3).Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteSubContractRms(3);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(json);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public void DownloadTemplate_WhenTemplateMissing_ReturnsNotFound()
        {
            // Act
            var result = _controller.DownloadTemplate();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Import_WhenFileIsNull_ReturnsFailureJson()
        {
            // Act
            var result = await _controller.Import(null!);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(json);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Import_WhenServiceReturnsFailure_ReturnsFailureJson()
        {
            // Arrange
            var file = Substitute.For<IFormFile>();
            file.Length.Returns(10);
            _subContractService.ImportSubContractRmsAsync(file)
                .Returns(ApiResponseDto<SubContractRmsImportResultDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Import failed." }],
                    new ApiMetaDto()));

            // Act
            var result = await _controller.Import(file);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(json);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Import failed.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task LoadFailedSubContractRmsGrid_ValidRequest_ReturnsPartialView()
        {
            // Act
            var result = await _controller.LoadFailedSubContractRmsGrid(new PaginationFilter<string> { Filter = "{}" });

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<SubContractRmsFailedItem>>(partial.Model);
        }

        [Fact]
        public async Task ExportFailedSubContractRms_WhenDataAvailable_ReturnsFileContentResult()
        {
            // Arrange
            var responseData = new List<SubContractRmsImportRowDto>
            {
                new() { Id = 1, Project = "P1" }
            };
            var mappedItems = new List<SubContractRmsFailedItem>
            {
                new() { Id = 1, Project = "P1" }
            };
            var bytes = new byte[] { 1, 2, 3 };

            _subContractService.GetFailedSubContractRmsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<SubContractRmsImportRowDto>>.SuccessResponse(responseData));
            _mapper.Map<List<SubContractRmsFailedItem>>(responseData).Returns(mappedItems);
            _excelExportService.ExportToExcel(mappedItems, "SubContractRMS_Failed").Returns(bytes);

            // Act
            var result = await _controller.ExportFailedSubContractRms();

            // Assert
            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file.ContentType);
            Assert.Equal(bytes, file.FileContents);
        }

        [Fact]
        public async Task DeleteAllFailedSubContractRms_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            _subContractService.DeleteFailedSubContractRmsByUserAsync()
                .Returns(ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "delete failed" }],
                    new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteAllFailedSubContractRms();

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(json);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("delete failed", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task GetFailedSubContractRms_WhenServiceSucceeds_ReturnsPartialView()
        {
            // Arrange
            var dto = new SubContractRmsImportRowDto { Id = 9, Project = "P9" };
            var model = new SubContractRmsFailedItem { Id = 9, Project = "P9" };
            _subContractService.GetFailedSubContractRmsByIdAsync(9)
                .Returns(ApiResponseDto<SubContractRmsImportRowDto>.SuccessResponse(dto));
            _mapper.Map<SubContractRmsFailedItem>(dto).Returns(model);

            // Act
            var result = await _controller.GetFailedSubContractRms(9);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_EditFailedSubContractRms", partial.ViewName);
            Assert.IsType<SubContractRmsFailedItem>(partial.Model);
        }

        [Fact]
        public async Task SaveFailedSubContractRms_WhenValidationSucceedsAndMoved_ReturnsSuccessJsonWithMovedFlag()
        {
            // Arrange
            var model = new SubContractRmsFailedItem { Id = 4, Project = "P4" };
            var dto = new SubContractRmsImportRowDto { Id = 4, Project = "P4" };
            _mapper.Map<SubContractRmsImportRowDto>(model).Returns(dto);
            _subContractService.SaveFailedSubContractRmsAsync(4, dto)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.SaveFailedSubContractRms(model);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(json);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.True(value.GetProperty("movedToSubContract").GetBoolean());
            Assert.Equal("Record successfully validated and is now live.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveFailedSubContractRms_WhenModelStateInvalid_ReturnsValidationFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("Project", "Project is required");

            // Act
            var result = await _controller.SaveFailedSubContractRms(new SubContractRmsFailedItem());

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(json);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Please correct the errors below.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task DeleteFailedSubContractRms_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            _subContractService.DeleteFailedSubContractRmsByIdAsync(13)
                .Returns(ApiResponseDto<bool>.FailureResponse([], new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteFailedSubContractRms(13);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(json);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to delete failed record.", value.GetProperty("message").GetString());
        }
    }
}
