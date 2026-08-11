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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PIMS.Controllers.PMDMilestoneControllerTest
{
    public class PMDMilestoneControllerTests
    {
        private readonly IMapper _mockMapper;
        private readonly IMilestoneService _mockMilestoneService;
        private readonly PMDMilestoneController _sut;

        public PMDMilestoneControllerTests()
        {
            _mockMapper = Substitute.For<IMapper>();
            _mockMilestoneService = Substitute.For<IMilestoneService>();
            _sut = new PMDMilestoneController(_mockMapper, _mockMilestoneService);
        }

        #region Index

        [Fact]
        public async Task Index_WithoutParentProject_ReturnsViewWithFirstProject()
        {
            // Arrange
            var managers = new List<ProjectYearManagerDto>
            {
                new() { ParentProject = "PP001", Manager = "Manager1" },
                new() { ParentProject = "PP002", Manager = "Manager2" }
            };
            var managersResponse = ApiResponseDto<List<ProjectYearManagerDto>>.SuccessResponse(managers);
            var milestonesDto = new List<MilestoneDto>
            {
                new() { Project = "PP001", Number = "M1" }
            };
            var milestonesResponse = ApiResponseDto<List<MilestoneDto>>.SuccessResponse(milestonesDto);

            _mockMilestoneService.GetProjectYearManagersAsync(Arg.Any<int>()).Returns(managersResponse);
            _mockMapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string> { Page = 1, PageSize = 50 });
            _mockMilestoneService.GetPMDMilestonesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(milestonesResponse);
            _mockMapper.Map<List<PMDMilestoneItem>>(Arg.Any<List<MilestoneDto>>())
                .Returns(new List<PMDMilestoneItem>());
            _mockMapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _sut.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsType<PMDMilestoneViewModel>(viewResult!.Model);
            var model = viewResult.Model as PMDMilestoneViewModel;
            Assert.Equal("PP001", model!.Parentproject);
        }

        [Fact]
        public async Task Index_WithParentProject_SelectsSpecifiedProject()
        {
            // Arrange
            const string selectedProject = "PP002";
            var managers = new List<ProjectYearManagerDto>
            {
                new() { ParentProject = "PP001", Manager = "Manager1" },
                new() { ParentProject = "PP002", Manager = "Manager2" }
            };
            var managersResponse = ApiResponseDto<List<ProjectYearManagerDto>>.SuccessResponse(managers);
            var milestonesDto = new List<MilestoneDto>();
            var milestonesResponse = ApiResponseDto<List<MilestoneDto>>.SuccessResponse(milestonesDto);

            _mockMilestoneService.GetProjectYearManagersAsync(Arg.Any<int>()).Returns(managersResponse);
            _mockMapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mockMilestoneService.GetPMDMilestonesAsync(Arg.Any<QueryParameters<string>>(), selectedProject)
                .Returns(milestonesResponse);
            _mockMapper.Map<List<PMDMilestoneItem>>(Arg.Any<List<MilestoneDto>>())
                .Returns(new List<PMDMilestoneItem>());
            _mockMapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _sut.Index(selectedProject);

            // Assert
            var viewResult = result as ViewResult;
            var model = viewResult!.Model as PMDMilestoneViewModel;
            Assert.Equal(selectedProject, model!.Parentproject);
        }

        [Fact]
        public async Task Index_WhenEmptyProjectList_ReturnsEmptyProjectOptions()
        {
            // Arrange
            var managersResponse = ApiResponseDto<List<ProjectYearManagerDto>>.SuccessResponse(new List<ProjectYearManagerDto>());
            var milestonesResponse = ApiResponseDto<List<MilestoneDto>>.SuccessResponse(new List<MilestoneDto>());

            _mockMilestoneService.GetProjectYearManagersAsync(Arg.Any<int>()).Returns(managersResponse);
            _mockMapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mockMilestoneService.GetPMDMilestonesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(milestonesResponse);
            _mockMapper.Map<List<PMDMilestoneItem>>(Arg.Any<List<MilestoneDto>>())
                .Returns(new List<PMDMilestoneItem>());
            _mockMapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _sut.Index();

            // Assert
            var viewResult = result as ViewResult;
            var model = viewResult!.Model as PMDMilestoneViewModel;
            Assert.Empty(model!.ProjectOptions);
        }

        [Fact]
        public async Task Index_SetupConfirmationSection_WhenProjectValid()
        {
            // Arrange
            var managers = new List<ProjectYearManagerDto>
            {
                new() { ParentProject = "PP001", Manager = "Manager1" }
            };
            var managersResponse = ApiResponseDto<List<ProjectYearManagerDto>>.SuccessResponse(managers);
            var milestonesResponse = ApiResponseDto<List<MilestoneDto>>.SuccessResponse(new List<MilestoneDto>());

            _mockMilestoneService.GetProjectYearManagersAsync(Arg.Any<int>()).Returns(managersResponse);
            _mockMapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mockMilestoneService.GetPMDMilestonesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(milestonesResponse);
            _mockMapper.Map<List<PMDMilestoneItem>>(Arg.Any<List<MilestoneDto>>())
                .Returns(new List<PMDMilestoneItem>());
            _mockMapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _sut.Index("PP001");

            // Assert
            var viewResult = result as ViewResult;
            var model = viewResult!.Model as PMDMilestoneViewModel;
            Assert.NotNull(model!.ConfirmationLabelText);
        }

        #endregion

        #region LoadMilestoneGrid

        [Fact]
        public async Task LoadMilestoneGrid_WithValidData_ReturnsMappedGridConfig()
        {
            // Arrange
            const string project = "PP001";
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            var milestoneDtos = new List<MilestoneDto>
            {
                new() { Project = project, Number = "M1" }
            };
            var milestonesResponse = ApiResponseDto<List<MilestoneDto>>.SuccessResponse(milestoneDtos);
            var pmdMilestoneItems = new List<PMDMilestoneItem>
            {
                new() { Project = project, Number = "M1" }
            };

            _mockMapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _mockMilestoneService.GetPMDMilestonesAsync(Arg.Any<QueryParameters<string>>(), project)
                .Returns(milestonesResponse);
            _mockMapper.Map<List<PMDMilestoneItem>>(milestoneDtos).Returns(pmdMilestoneItems);
            _mockMapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel { TotalRecords = 1 });

            // Act
            var result = await _sut.LoadMilestoneGrid(request, project);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            var partialResult = result as PartialViewResult;
            Assert.Equal("_DataGrid", partialResult!.ViewName);
        }

        [Fact]
        public async Task LoadMilestoneGrid_WithInvalidModelState_ReturnsValidationError()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            _sut.ModelState.AddModelError("key", "error message");

            // Act
            var result = await _sut.LoadMilestoneGrid(request, "PP001");

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.NotNull(jsonResult!.Value);
        }

        #endregion

        #region Edit

        [Fact]
        public async Task Edit_WithValidParameters_ReturnsMilestoneItem()
        {
            // Arrange
            const string parentproject = "PP001";
            const string number = "M1";
            var milestoneDto = new MilestoneDto { Project = parentproject, Number = number };
            var apiResult = ApiResponseDto<MilestoneDto>.SuccessResponse(milestoneDto);
            var milestoneItem = new MilestoneItem { Project = parentproject, Number = number };

            _mockMilestoneService.GetMilestoneAsync(parentproject, number).Returns(apiResult);
            _mockMapper.Map<MilestoneItem>(milestoneDto).Returns(milestoneItem);

            // Act
            var result = await _sut.Edit(parentproject, number);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            var model = viewResult!.Model as MilestoneItem;
            Assert.Equal(number, model!.Number);
        }

        [Fact]
        public async Task Edit_WithUrlEncodedNumber_DecodesNumberBeforeLookup()
        {
            // Arrange
            const string parentproject = "PP001";
            const string encodedNumber = "M%2F1";
            const string decodedNumber = "M/1";
            var milestoneDto = new MilestoneDto { Project = parentproject, Number = decodedNumber };
            var apiResult = ApiResponseDto<MilestoneDto>.SuccessResponse(milestoneDto);
            var milestoneItem = new MilestoneItem { Project = parentproject, Number = decodedNumber };

            _mockMilestoneService.GetMilestoneAsync(parentproject, decodedNumber).Returns(apiResult);
            _mockMapper.Map<MilestoneItem>(milestoneDto).Returns(milestoneItem);

            // Act
            var result = await _sut.Edit(parentproject, encodedNumber);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            var model = viewResult!.Model as MilestoneItem;
            Assert.Equal(decodedNumber, model!.Number);
        }

        [Fact]
        public async Task Edit_WithNullParentProject_RedirectsToIndex()
        {
            // Act
            var result = await _sut.Edit(null!, "M1");

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;
            Assert.Equal(nameof(PMDMilestoneController.Index), redirectResult!.ActionName);
        }

        [Fact]
        public async Task Edit_WithEmptyParentProject_RedirectsToIndex()
        {
            // Act
            var result = await _sut.Edit(string.Empty, "M1");

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Edit_WithNullNumber_RedirectsToIndex()
        {
            // Act
            var result = await _sut.Edit("PP001", null!);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Edit_WithEmptyNumber_RedirectsToIndex()
        {
            // Act
            var result = await _sut.Edit("PP001", string.Empty);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Edit_WhenMilestoneNotFound_ReturnsNewMilestoneItem()
        {
            // Arrange
            const string parentproject = "PP001";
            const string number = "UNKNOWN";
            var apiResult = ApiResponseDto<MilestoneDto>.SuccessResponse(null!);

            _mockMilestoneService.GetMilestoneAsync(parentproject, number).Returns(apiResult);

            // Act
            var result = await _sut.Edit(parentproject, number);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            var model = viewResult!.Model as MilestoneItem;
            Assert.Equal(parentproject, model!.Project);
            Assert.Equal(number, model.Number);
        }

        #endregion

        #region GetConfirmationState

        [Fact]
        public async Task GetConfirmationState_WithValidProject_ReturnsConfirmationData()
        {
            // Arrange
            const string parentproject = "PP001";

            // Act
            var result = await _sut.GetConfirmationState(parentproject);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.NotNull(jsonResult!.Value);
        }

        [Fact]
        public async Task GetConfirmationState_WithNullProject_ReturnsJsonResult()
        {
            // Act
            var result = await _sut.GetConfirmationState(null!);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.NotNull(jsonResult!.Value);
        }

        [Fact]
        public async Task GetConfirmationState_WithEmptyProject_ReturnsJsonResult()
        {
            // Act
            var result = await _sut.GetConfirmationState(string.Empty);

            // Assert
            Assert.IsType<JsonResult>(result);
            var jsonResult = result as JsonResult;
            Assert.NotNull(jsonResult!.Value);
        }

        #endregion
    }
}
