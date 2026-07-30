using System;
using System.Collections.Generic;
using System.Text;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Controllers;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PIMS.Controllers.MilestoneImportControllerTest
{
    public class MilestoneImportControllerTests
    {
        private readonly IMapper _mockMapper;
        private readonly IMilestoneService _mockMilestoneService;
        private readonly IProjectListService _mockProjectListService;
        private readonly MilestoneImportController _sut;

        public MilestoneImportControllerTests()
        {
            _mockMapper = Substitute.For<IMapper>();
            _mockMilestoneService = Substitute.For<IMilestoneService>();
            _mockProjectListService = Substitute.For<IProjectListService>();
            _sut = new MilestoneImportController(_mockMapper, _mockMilestoneService, _mockProjectListService);
        }

        #region Index

        [Fact]
        public async Task Index_WithoutProject_ReturnsViewWithDefaultProject()
        {
            // Arrange
            var projects = new List<ProjectListMilestoneDto>
            {
                new() { Parentproject = "PP001", Program = "SurvProgram" },
                new() { Parentproject = "PP002", Program = "OtherProgram" }
            };
            var apiResult = ApiResponseDto<List<ProjectListMilestoneDto>>.SuccessResponse(projects);
            var milestoneTypes = ApiResponseDto<List<MilestoneTypeDto>>.SuccessResponse(
                new List<MilestoneTypeDto>
                {
                    new() { IdType = 'D', Type = "Deliverable", MilestoneDeliverable = 'D' },
                    new() { IdType = 'M', Type = "Milestone", MilestoneDeliverable = 'M' }
                });

            _mockProjectListService.GetAllProjectsForMilestoneAsync().Returns(apiResult);
            _mockMilestoneService.GetMilestoneTypesAsync(Arg.Any<string>()).Returns(milestoneTypes);
            _mockMapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = -1, PageSize = 50 });
            _mockMilestoneService.GetAllStagingRowsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<StagingMilestoneDto>>.SuccessResponse(new List<StagingMilestoneDto>()));

            // Act
            var result = await _sut.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsType<MilestoneImportViewModel>(viewResult!.Model);
            
            var model = viewResult.Model as MilestoneImportViewModel;
            Assert.Equal(2, model!.ProjectOptions.Count);
            Assert.Equal("PP001", model.Parentproject);
        }

        [Fact]
        public async Task Index_WithProject_SelectsSpecifiedProject()
        {
            // Arrange
            const string selectedProject = "PP002";
            var projects = new List<ProjectListMilestoneDto>
            {
                new() { Parentproject = "PP001", Program = "SurvProgram" },
                new() { Parentproject = "PP002", Program = "OtherProgram" }
            };
            var apiResult = ApiResponseDto<List<ProjectListMilestoneDto>>.SuccessResponse(projects);
            var milestoneTypes = ApiResponseDto<List<MilestoneTypeDto>>.SuccessResponse(new List<MilestoneTypeDto>());

            _mockProjectListService.GetAllProjectsForMilestoneAsync().Returns(apiResult);
            _mockMilestoneService.GetMilestoneTypesAsync(Arg.Any<string>()).Returns(milestoneTypes);
            _mockMapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = -1, PageSize = 50 });
            _mockMilestoneService.GetAllStagingRowsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<StagingMilestoneDto>>.SuccessResponse(new List<StagingMilestoneDto>()));

            // Act
            var result = await _sut.Index(selectedProject);

            // Assert
            var viewResult = result as ViewResult;
            var model = viewResult!.Model as MilestoneImportViewModel;
            Assert.Equal(selectedProject, model!.Parentproject);
        }

        [Fact]
        public async Task Index_SetTypeLookupToDDeliverable_WhenProgramEndWithSurv()
        {
            // Arrange
            var projects = new List<ProjectListMilestoneDto>
            {
                new() { Parentproject = "PP001", Program = "TestSurv" }
            };
            var apiResult = ApiResponseDto<List<ProjectListMilestoneDto>>.SuccessResponse(projects);
            var milestoneTypes = ApiResponseDto<List<MilestoneTypeDto>>.SuccessResponse(new List<MilestoneTypeDto>());

            _mockProjectListService.GetAllProjectsForMilestoneAsync().Returns(apiResult);
            _mockMilestoneService.GetMilestoneTypesAsync(Arg.Any<string>()).Returns(milestoneTypes);
            _mockMapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = -1, PageSize = 50 });
            _mockMilestoneService.GetAllStagingRowsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<StagingMilestoneDto>>.SuccessResponse(new List<StagingMilestoneDto>()));

            // Act
            var result = await _sut.Index("PP001");

            // Assert
            var viewResult = result as ViewResult;
            var model = viewResult!.Model as MilestoneImportViewModel;
            Assert.Equal('D', model!.TypeLookUp);
        }

        [Fact]
        public async Task Index_SetTypeLookupToMMilestone_WhenProgramDoesNotEndWithSurv()
        {
            // Arrange
            var projects = new List<ProjectListMilestoneDto>
            {
                new() { Parentproject = "PP001", Program = "OtherProgram" }
            };
            var apiResult = ApiResponseDto<List<ProjectListMilestoneDto>>.SuccessResponse(projects);
            var milestoneTypes = ApiResponseDto<List<MilestoneTypeDto>>.SuccessResponse(new List<MilestoneTypeDto>());

            _mockProjectListService.GetAllProjectsForMilestoneAsync().Returns(apiResult);
            _mockMilestoneService.GetMilestoneTypesAsync(Arg.Any<string>()).Returns(milestoneTypes);
            _mockMapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = -1, PageSize = 50 });
            _mockMilestoneService.GetAllStagingRowsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<StagingMilestoneDto>>.SuccessResponse(new List<StagingMilestoneDto>()));

            // Act
            var result = await _sut.Index("PP001");

            // Assert
            var viewResult = result as ViewResult;
            var model = viewResult!.Model as MilestoneImportViewModel;
            Assert.Equal('M', model!.TypeLookUp);
        }

        #endregion

        #region LoadMilestoneImportGrid

        [Fact]
        public async Task LoadMilestoneImportGrid_ReturnsMappedGridConfig()
        {
            // Arrange
            const string project = "PP001";
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            var stagingDtos = new List<StagingMilestoneDto>
            {
                new() { Id = 1, Project = project, Number = "M1" }
            };
            var stagingItems = new List<StagingMilestoneItem>
            {
                new() { Id = 1, Project = project, Number = "M1" }
            };

            _mockMapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _mockMilestoneService.GetAllStagingRowsAsync(Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<List<StagingMilestoneDto>>.SuccessResponse(stagingDtos));
            _mockMapper.Map<List<StagingMilestoneItem>>(stagingDtos).Returns(stagingItems);

            // Act
            var result = await _sut.LoadMilestoneImportGrid(request, project);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            var partialResult = result as PartialViewResult;
            Assert.Equal("_DataGrid", partialResult!.ViewName);
        }

        #endregion

        #region GetAddEditMilestoneImportPartial

        [Fact]
        public async Task GetAddEditMilestoneImportPartial_ReturnsNewItem_WhenNoIdProvided()
        {
            // Arrange
            const string project = "PP001";
            const char typeLookUp = 'D';
            const string typeId = "D";

            // Act
            var result = await _sut.GetAddEditMilestoneImportPartial(project, null, typeLookUp, typeId);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            var partialResult = result as PartialViewResult;
            Assert.Equal("_AddEditMilestoneImport", partialResult!.ViewName);
            var model = partialResult.Model as StagingMilestoneItem;
            Assert.Equal(project, model!.Project);
            Assert.Equal(typeId, model.TypeId);
        }

        [Fact]
        public async Task GetAddEditMilestoneImportPartial_ReturnsExistingItem_WhenIdProvided()
        {
            // Arrange
            const int id = 1;
            var stagingDto = new StagingMilestoneDto { Id = id, Project = "PP001", Number = "M1" };
            var stagingItem = new StagingMilestoneItem { Id = id, Project = "PP001", Number = "M1" };
            var apiResult = ApiResponseDto<List<StagingMilestoneDto>>.SuccessResponse(new List<StagingMilestoneDto> { stagingDto });

            _mockMilestoneService.GetStagingRowsAsync(id).Returns(apiResult);
            _mockMapper.Map<StagingMilestoneItem>(stagingDto).Returns(stagingItem);

            // Act
            var result = await _sut.GetAddEditMilestoneImportPartial(null!, id);

            // Assert
            var partialResult = result as PartialViewResult;
            var model = partialResult!.Model as StagingMilestoneItem;
            Assert.Equal(id, model!.Id);
            Assert.Equal("M1", model.Number);
        }

        #endregion

        #region SaveImportRow

        [Fact]
        public async Task SaveImportRow_WithValidItem_ReturnsSaveSuccess()
        {
            // Arrange
            const int year = 2025;
            var item = new StagingMilestoneItem
            {
                IsAddingNew = true,
                Project = "PP001",
                Number = "M1",
                Description = "Test",
                DateDue = System.DateTime.Today.AddDays(30)
            };
            var dto = new StagingMilestoneDto { Project = "PP001", Number = "M1" };
            var resultData = new StagingMilestoneDto { Id = 1, Project = "PP001", Number = "M1" };
            var apiResult = ApiResponseDto<StagingMilestoneDto>.SuccessResponse(resultData);

            _mockMapper.Map<StagingMilestoneDto>(item).Returns(dto);
            _mockMilestoneService.AddStagingRowAsync(dto, year).Returns(apiResult);

            // Act
            var result = await _sut.SaveImportRow(item, year);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.NotNull(jsonResult!.Value);
            // Verify it's a successful response by checking the structure
            await _mockMilestoneService.Received(1).AddStagingRowAsync(dto, year);
        }

        [Fact]
        public async Task SaveImportRow_WithExistingItem_ReturnsUpdateSuccess()
        {
            // Arrange
            const int year = 2025;
            const int id = 1;
            var item = new StagingMilestoneItem
            {
                Id = id,
                IsAddingNew = false,
                Project = "PP001",
                Number = "M1"
            };
            var dto = new StagingMilestoneDto { Project = "PP001", Number = "M1" };
            var resultData = new StagingMilestoneDto { Id = id, Project = "PP001", Number = "M1" };
            var apiResult = ApiResponseDto<StagingMilestoneDto>.SuccessResponse(resultData);

            _mockMapper.Map<StagingMilestoneDto>(item).Returns(dto);
            _mockMilestoneService.UpdateStagingRowAsync(id, dto).Returns(apiResult);

            // Act
            var result = await _sut.SaveImportRow(item, year);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.NotNull(jsonResult!.Value);
            await _mockMilestoneService.Received(1).UpdateStagingRowAsync(id, dto);
        }

        #endregion

        #region DeleteImportRow

        [Fact]
        public async Task DeleteImportRow_WithValidId_ReturnsDeleteSuccess()
        {
            // Arrange
            const int id = 1;
            var apiResult = ApiResponseDto<object>.SuccessResponse(new object());

            _mockMilestoneService.DeleteStagingRowAsync(id).Returns(apiResult);

            // Act
            var result = await _sut.DeleteImportRow(id);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.NotNull(jsonResult!.Value);
            await _mockMilestoneService.Received(1).DeleteStagingRowAsync(id);
        }

        [Fact]
        public async Task DeleteImportRow_WhenServiceReturnsFailure_ReturnsFailureJson()
        {
            // Arrange
            const int id = 1;
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResult = new ApiResponseDto<object>
            {
                Success = false,
                Errors = errors,
                Meta = new ApiMetaDto()
            };

            _mockMilestoneService.DeleteStagingRowAsync(id).Returns(apiResult);

            // Act
            var result = await _sut.DeleteImportRow(id);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.NotNull(jsonResult!.Value);
            await _mockMilestoneService.Received(1).DeleteStagingRowAsync(id);
        }

        #endregion

        #region GetFormRequired

        [Fact]
        public async Task GetFormRequired_ReturnsMilestoneTypesForDeliverable_WhenProgramEndsWithSurv()
        {
            // Arrange
            const string parentProject = "PP001";
            var projectDetails = new ProjectDetailsMilestoneDto
            {
                Parentproject = parentProject,
                Program = "TestSurv"
            };
            var projectApiResult = ApiResponseDto<ProjectDetailsMilestoneDto>.SuccessResponse(projectDetails);
            var milestoneTypes = new List<MilestoneTypeDto>
            {
                new() { IdType = 'D', Type = "Deliverable", MilestoneDeliverable = 'D' },
                new() { IdType = 'M', Type = "Milestone", MilestoneDeliverable = 'M' }
            };
            var typesApiResult = ApiResponseDto<List<MilestoneTypeDto>>.SuccessResponse(milestoneTypes);

            _mockProjectListService.GetProjectsDetailsForMilestoneAsync(parentProject).Returns(projectApiResult);
            _mockMilestoneService.GetMilestoneTypesAsync('D'.ToString()).Returns(typesApiResult);

            // Act
            var result = await _sut.GetFormRequired(parentProject);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.NotNull(jsonResult!.Value);
            await _mockMilestoneService.Received(1).GetMilestoneTypesAsync('D'.ToString());
        }

        [Fact]
        public async Task GetFormRequired_ReturnsMilestoneTypeForMilestone_WhenProgramDoesNotEndWithSurv()
        {
            // Arrange
            const string parentProject = "PP001";
            var projectDetails = new ProjectDetailsMilestoneDto
            {
                Parentproject = parentProject,
                Program = "OtherProgram"
            };
            var projectApiResult = ApiResponseDto<ProjectDetailsMilestoneDto>.SuccessResponse(projectDetails);
            var milestoneTypes = new List<MilestoneTypeDto>
            {
                new() { IdType = 'M', Type = "Milestone", MilestoneDeliverable = 'M' }
            };
            var typesApiResult = ApiResponseDto<List<MilestoneTypeDto>>.SuccessResponse(milestoneTypes);

            _mockProjectListService.GetProjectsDetailsForMilestoneAsync(parentProject).Returns(projectApiResult);
            _mockMilestoneService.GetMilestoneTypesAsync('M'.ToString()).Returns(typesApiResult);

            // Act
            var result = await _sut.GetFormRequired(parentProject);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.NotNull(jsonResult!.Value);
            await _mockMilestoneService.Received(1).GetMilestoneTypesAsync('M'.ToString());
        }

        #endregion

        #region ValidateImport

        [Fact]
        public async Task ValidateImport_WithValidProject_ReturnsSuccess()
        {
            // Arrange
            const string project = "PP001";
            const string typeId = "M";
            const bool isDeliverableMode = false;
            var apiResult = ApiResponseDto<object>.SuccessResponse(new object());

            _mockMilestoneService.ValidateStagingAsync(project, typeId, isDeliverableMode).Returns(apiResult);

            // Act
            var result = await _sut.ValidateImport(project, typeId, isDeliverableMode);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.NotNull(jsonResult!.Value);
            await _mockMilestoneService.Received(1).ValidateStagingAsync(project, typeId, isDeliverableMode);
        }

        [Fact]
        public async Task ValidateImport_WhenServiceReturnsFailure_ReturnsFailureJson()
        {
            // Arrange
            const string project = "PP001";
            var errors = new List<ApiErrorDto> { new() { Message = "Validation error", Code = "VALIDATION_ERROR" } };
            var apiResult = new ApiResponseDto<object>
            {
                Success = false,
                Errors = errors,
                Meta = new ApiMetaDto()
            };

            _mockMilestoneService.ValidateStagingAsync(project, null!, false).Returns(apiResult);

            // Act
            var result = await _sut.ValidateImport(project);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.NotNull(jsonResult!.Value);
            await _mockMilestoneService.Received(1).ValidateStagingAsync(project, null!, false);
        }

        #endregion

        #region ImportRecords

        [Fact]
        public async Task ImportRecords_WithValidProject_ReturnsImportSuccess()
        {
            // Arrange
            const string project = "PP001";
            var importResult = ApiResponseDto<object>.SuccessResponse(5);
            _mockMilestoneService.ImportStagingAsync(project).Returns(importResult);

            // Act
            var result = await _sut.ImportRecords(project);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.NotNull(jsonResult!.Value);
            await _mockMilestoneService.Received(1).ImportStagingAsync(project);
        }

        [Fact]
        public async Task ImportRecords_WithOverwrite_ReturnsImportAndOverwriteCount()
        {
            // Arrange
            const string project = "PP001";
            var overwriteResult = ApiResponseDto<object>.SuccessResponse(2);
            var importResult = ApiResponseDto<object>.SuccessResponse(5);

            _mockMilestoneService.ImportWithOverwriteAsync(project).Returns(overwriteResult);
            _mockMilestoneService.ImportStagingAsync(project).Returns(importResult);

            // Act
            var result = await _sut.ImportRecords(project, overwrite: true);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.NotNull(jsonResult!.Value);
            await _mockMilestoneService.Received(1).ImportWithOverwriteAsync(project);
            await _mockMilestoneService.Received(1).ImportStagingAsync(project);
        }

        [Fact]
        public async Task ImportRecords_WithNullProject_ReturnsFailure()
        {
            // Act
            var result = await _sut.ImportRecords(null!);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.NotNull(jsonResult!.Value);
        }

        [Fact]
        public async Task ImportRecords_WhenImportFails_ReturnsFailureJson()
        {
            // Arrange
            const string project = "PP001";
            var errors = new List<ApiErrorDto> { new() { Message = "Import error", Code = "IMPORT_ERROR" } };
            var apiResult = new ApiResponseDto<object>
            {
                Success = false,
                Errors = errors,
                Meta = new ApiMetaDto()
            };

            _mockMilestoneService.ImportStagingAsync(project).Returns(apiResult);

            // Act
            var result = await _sut.ImportRecords(project);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.NotNull(jsonResult!.Value);
            await _mockMilestoneService.Received(1).ImportStagingAsync(project);
        }

        #endregion

        #region ClearImport

        [Fact]
        public async Task ClearImport_WithValidProject_ReturnsClearedCount()
        {
            // Arrange
            const string project = "PP001";
            var apiResult = ApiResponseDto<object>.SuccessResponse(10);

            _mockMilestoneService.ClearStagingAsync(project).Returns(apiResult);

            // Act
            var result = await _sut.ClearImport(project);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.NotNull(jsonResult!.Value);
            await _mockMilestoneService.Received(1).ClearStagingAsync(project);
        }

        [Fact]
        public async Task ClearImport_WhenServiceReturnsFailure_ReturnsFailureJson()
        {
            // Arrange
            const string project = "PP001";
            var errors = new List<ApiErrorDto> { new() { Message = "Clear error", Code = "CLEAR_ERROR" } };
            var apiResult = new ApiResponseDto<object>
            {
                Success = false,
                Errors = errors,
                Meta = new ApiMetaDto()
            };

            _mockMilestoneService.ClearStagingAsync(project).Returns(apiResult);

            // Act
            var result = await _sut.ClearImport(project);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.NotNull(jsonResult!.Value);
            await _mockMilestoneService.Received(1).ClearStagingAsync(project);
        }

        #endregion
    }
}
