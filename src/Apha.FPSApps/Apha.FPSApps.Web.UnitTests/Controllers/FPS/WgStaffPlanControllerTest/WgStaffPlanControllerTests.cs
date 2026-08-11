using System.Text.Json;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.WgStaffPlanControllerTest
{
    public class WgStaffPlanControllerTests
    {
        private const string TestResourceCentre = "RC01";
        private const string TestWorkGroup = "WG001";
        private const string TestGradeCode = "G1";
        private const string TestName = "Test Staff";
        private const string TestManager = "Manager01";

        private readonly IMapper _mapper;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IWorkGroupService _workGroupService;
        private readonly WgStaffPlanController _controller;

        public WgStaffPlanControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _profitCentreService = Substitute.For<IProfitCentreService>();
            _workGroupService = Substitute.For<IWorkGroupService>();
            _controller = new WgStaffPlanController(_mapper, _profitCentreService, _workGroupService);
        }

        // Helper method to extract properties from JsonResult
        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private class JsonResultSuccess
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public object? Data { get; set; }
        }

        private class JsonResultError
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
        }

        private static WgStaffPlanViewItem BuildViewItem(string name = TestName) =>
            new()
            {
                WorkGroup = TestWorkGroup,
                GradeCode = TestGradeCode,
                Name = name,
                Manager = TestManager
            };

        private static WgStaffPlanViewDto BuildDto(string name = TestName) =>
            new()
            {
                WorkGroup = TestWorkGroup,
                GradeCode = TestGradeCode,
                Name = name,
                Manager = TestManager
            };

        private static ProfitCentreDto BuildProfitCentreDto(string id = TestResourceCentre) =>
            new()
            {
                ProfitCentreId = id,
                ProfitCentreName = $"Resource Centre {id}"
            };

        private static WorkGroupViewDto BuildWorkGroupViewDto(string name = TestWorkGroup) =>
            new()
            {
                WorkGroupName = name,
                ProfitCentre = TestResourceCentre
            };

        #region Index Tests

        [Fact]
        public async Task Index_NoParameters_ReturnsViewWithEmptySelections()
        {
            // Arrange
            var profitCentres = new ApiResponseDto<List<ProfitCentreDto>>
            {
                Success = true,
                Data = new List<ProfitCentreDto> { BuildProfitCentreDto() }
            };

            _profitCentreService.GetProfitCentresAsync().Returns(profitCentres);
            _mapper.Map<List<WgStaffPlanViewItem>>(Arg.Any<List<WgStaffPlanViewDto>>())
                   .Returns(new List<WgStaffPlanViewItem>());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WgStaffPlanViewModel>(viewResult.Model);
            Assert.True(string.IsNullOrEmpty(model.SelectedResourceCentre));
            Assert.True(string.IsNullOrEmpty(model.SelectedWorkGroup));
            Assert.NotNull(model.Grid);
        }

        [Fact]
        public async Task Index_ValidResourceCentreOnly_ReturnsViewWithResourceCentreSelected()
        {
            // Arrange
            var profitCentres = new ApiResponseDto<List<ProfitCentreDto>>
            {
                Success = true,
                Data = new List<ProfitCentreDto> { BuildProfitCentreDto(TestResourceCentre) }
            };
            var workGroups = new ApiResponseDto<List<WorkGroupViewDto>>
            {
                Success = true,
                Data = new List<WorkGroupViewDto> { BuildWorkGroupViewDto() }
            };

            _profitCentreService.GetProfitCentresAsync().Returns(profitCentres);
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync(TestResourceCentre).Returns(workGroups);
            _mapper.Map<List<WgStaffPlanViewItem>>(Arg.Any<List<WgStaffPlanViewDto>>())
                   .Returns(new List<WgStaffPlanViewItem>());

            // Act
            var result = await _controller.Index(TestResourceCentre);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WgStaffPlanViewModel>(viewResult.Model);
            Assert.Equal(TestResourceCentre, model.SelectedResourceCentre);
            Assert.True(string.IsNullOrEmpty(model.SelectedWorkGroup));
            Assert.Single(model.WorkGroupList);
        }

        [Fact]
        public async Task Index_ValidResourceCentreAndWorkGroup_ReturnsViewWithBothSelected()
        {
            // Arrange
            var profitCentres = new ApiResponseDto<List<ProfitCentreDto>>
            {
                Success = true,
                Data = new List<ProfitCentreDto> { BuildProfitCentreDto(TestResourceCentre) }
            };
            var workGroups = new ApiResponseDto<List<WorkGroupViewDto>>
            {
                Success = true,
                Data = new List<WorkGroupViewDto> { BuildWorkGroupViewDto(TestWorkGroup) }
            };
            var staffPlanData = new ApiResponseDto<List<WgStaffPlanViewDto>>
            {
                Success = true,
                Data = new List<WgStaffPlanViewDto> { BuildDto() },
                Pagination = new PaginationDto { TotalRecords = 1, PageNumber = 1, PageSize = 10 }
            };

            _profitCentreService.GetProfitCentresAsync().Returns(profitCentres);
            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync(TestResourceCentre).Returns(workGroups);
            _profitCentreService.GetPagedWgStaffPlanAsync(Arg.Any<QueryParameters<string>>(), TestWorkGroup)
                               .Returns(staffPlanData);
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                   .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _mapper.Map<List<WgStaffPlanViewItem>>(Arg.Any<List<WgStaffPlanViewDto>>())
                   .Returns(new List<WgStaffPlanViewItem> { BuildViewItem() });

            // Act
            var result = await _controller.Index(TestResourceCentre, TestWorkGroup);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WgStaffPlanViewModel>(viewResult.Model);
            Assert.Equal(TestResourceCentre, model.SelectedResourceCentre);
            Assert.Equal(TestWorkGroup, model.SelectedWorkGroup);
            Assert.Single(model.WorkGroupList);
            Assert.NotNull(model.Grid);
        }

        [Fact]
        public async Task Index_InvalidResourceCentre_ReturnsViewWithEmptySelections()
        {
            // Arrange
            var profitCentres = new ApiResponseDto<List<ProfitCentreDto>>
            {
                Success = true,
                Data = new List<ProfitCentreDto> { BuildProfitCentreDto(TestResourceCentre) }
            };

            _profitCentreService.GetProfitCentresAsync().Returns(profitCentres);
            _mapper.Map<List<WgStaffPlanViewItem>>(Arg.Any<List<WgStaffPlanViewDto>>())
                   .Returns(new List<WgStaffPlanViewItem>());

            // Act
            var result = await _controller.Index("INVALID_RC");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WgStaffPlanViewModel>(viewResult.Model);
            Assert.True(string.IsNullOrEmpty(model.SelectedResourceCentre));
        }

        [Fact]
        public async Task Index_ProfitCentreServiceFails_ReturnsViewWithEmptyLists()
        {
            // Arrange
            var failedResponse = new ApiResponseDto<List<ProfitCentreDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Service error" } }
            };

            _profitCentreService.GetProfitCentresAsync().Returns(failedResponse);
            _mapper.Map<List<WgStaffPlanViewItem>>(Arg.Any<List<WgStaffPlanViewDto>>())
                   .Returns(new List<WgStaffPlanViewItem>());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WgStaffPlanViewModel>(viewResult.Model);
            Assert.Empty(model.ResourceCentreList);
        }

        #endregion

        #region GetWorkGroupsByResourceCentre Tests

        [Fact]
        public async Task GetWorkGroupsByResourceCentre_ValidResourceCentre_ReturnsSuccessWithWorkGroups()
        {
            // Arrange
            var workGroups = new ApiResponseDto<List<WorkGroupViewDto>>
            {
                Success = true,
                Data = new List<WorkGroupViewDto> 
                { 
                    BuildWorkGroupViewDto("WG001"),
                    BuildWorkGroupViewDto("WG002")
                }
            };

            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync(TestResourceCentre).Returns(workGroups);

            // Act
            var result = await _controller.GetWorkGroupsByResourceCentre(TestResourceCentre);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultSuccess>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.Success);
        }

        [Fact]
        public async Task GetWorkGroupsByResourceCentre_EmptyResourceCentre_ReturnsError()
        {
            // Act
            var result = await _controller.GetWorkGroupsByResourceCentre("");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.Equal("Resource Centre is required.", value.Message);
        }

        [Fact]
        public async Task GetWorkGroupsByResourceCentre_WhitespaceResourceCentre_ReturnsError()
        {
            // Act
            var result = await _controller.GetWorkGroupsByResourceCentre("   ");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.Equal("Resource Centre is required.", value.Message);
        }

        [Fact]
        public async Task GetWorkGroupsByResourceCentre_ServiceFails_ReturnsError()
        {
            // Arrange
            var failedResponse = new ApiResponseDto<List<WorkGroupViewDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Database connection failed" } }
            };

            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync(TestResourceCentre).Returns(failedResponse);

            // Act
            var result = await _controller.GetWorkGroupsByResourceCentre(TestResourceCentre);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultError>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.Success);
            Assert.Contains("Database connection failed", value.Message);
        }

        [Fact]
        public async Task GetWorkGroupsByResourceCentre_ServiceReturnsNull_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var response = new ApiResponseDto<List<WorkGroupViewDto>>
            {
                Success = true,
                Data = null
            };

            _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync(TestResourceCentre).Returns(response);

            // Act
            var result = await _controller.GetWorkGroupsByResourceCentre(TestResourceCentre);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResultSuccess>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.Success);
        }

        #endregion

        #region LoadGrid Tests

        [Fact]
        public async Task LoadGrid_ValidRequestWithWorkGroup_ReturnsPartialViewWithData()
        {
            // Arrange
            var request = new PaginationFilter<string> 
            { 
                Filter = "{}", 
                Page = 1, 
                PageSize = 10 
            };
            var staffPlanData = new ApiResponseDto<List<WgStaffPlanViewDto>>
            {
                Success = true,
                Data = new List<WgStaffPlanViewDto> { BuildDto() },
                Pagination = new PaginationDto { TotalRecords = 1, PageNumber = 1, PageSize = 10 }
            };

            _mapper.Map<QueryParameters<string>>(request)
                   .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _profitCentreService.GetPagedWgStaffPlanAsync(Arg.Any<QueryParameters<string>>(), TestWorkGroup)
                               .Returns(staffPlanData);
            _mapper.Map<List<WgStaffPlanViewItem>>(Arg.Any<List<WgStaffPlanViewDto>>())
                   .Returns(new List<WgStaffPlanViewItem> { BuildViewItem() });

            // Act
            var result = await _controller.LoadGrid(request, TestWorkGroup);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            var config = Assert.IsType<DataGridConfig<WgStaffPlanViewItem>>(partialViewResult.Model);
            Assert.Single(config.Data);
            Assert.Equal("wgStaffPlanGrid", config.GridId);
            Assert.Equal("/FPS/WgStaffPlan/LoadGrid", config.BindGridUrl);
            Assert.Equal("getWgStaffPlanExtraFilters", config.ExtraFilterMethod);
            Assert.Equal("StaffId", config.KeyProperty);
        }

        [Fact]
        public async Task LoadGrid_NoWorkGroup_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> 
            { 
                Filter = "{}", 
                Page = 1, 
                PageSize = 10 
            };

            // Act
            var result = await _controller.LoadGrid(request, null);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            var config = Assert.IsType<DataGridConfig<WgStaffPlanViewItem>>(partialViewResult.Model);
            Assert.Empty(config.Data);
        }

        [Fact]
        public async Task LoadGrid_EmptyWorkGroup_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> 
            { 
                Filter = "{}", 
                Page = 1, 
                PageSize = 10 
            };

            // Act
            var result = await _controller.LoadGrid(request, "");

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<WgStaffPlanViewItem>>(partialViewResult.Model);
            Assert.Empty(config.Data);
        }

        [Fact]
        public async Task LoadGrid_ServiceReturnsEmptyPage_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> 
            { 
                Filter = "{}", 
                Page = 1, 
                PageSize = 10 
            };
            var emptyResponse = new ApiResponseDto<List<WgStaffPlanViewDto>>
            {
                Success = true,
                Data = new List<WgStaffPlanViewDto>(),
                Pagination = new PaginationDto { TotalRecords = 0, PageNumber = 1, PageSize = 10 }
            };

            _mapper.Map<QueryParameters<string>>(request)
                   .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _profitCentreService.GetPagedWgStaffPlanAsync(Arg.Any<QueryParameters<string>>(), TestWorkGroup)
                               .Returns(emptyResponse);
            _mapper.Map<List<WgStaffPlanViewItem>>(Arg.Any<List<WgStaffPlanViewDto>>())
                   .Returns(new List<WgStaffPlanViewItem>());

            // Act
            var result = await _controller.LoadGrid(request, TestWorkGroup);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<WgStaffPlanViewItem>>(partialViewResult.Model);
            Assert.Empty(config.Data);
            Assert.Equal(0, config.Pagination.TotalRecords);
        }

        [Fact]
        public async Task LoadGrid_ServiceFails_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> 
            { 
                Filter = "{}", 
                Page = 1, 
                PageSize = 10 
            };
            var failedResponse = new ApiResponseDto<List<WgStaffPlanViewDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Service error" } }
            };

            _mapper.Map<QueryParameters<string>>(request)
                   .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _profitCentreService.GetPagedWgStaffPlanAsync(Arg.Any<QueryParameters<string>>(), TestWorkGroup)
                               .Returns(failedResponse);

            // Act
            var result = await _controller.LoadGrid(request, TestWorkGroup);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<WgStaffPlanViewItem>>(partialViewResult.Model);
            Assert.Empty(config.Data);
        }

        [Fact]
        public async Task LoadGrid_WithFilterAndSort_AppliesCorrectly()
        {
            // Arrange
            var request = new PaginationFilter<string> 
            { 
                Filter = "{\"StaffName\":\"Test\"}", 
                Page = 2, 
                PageSize = 20,
                SortBy = "StaffName",
                Descending = true
            };
            var staffPlanData = new ApiResponseDto<List<WgStaffPlanViewDto>>
            {
                Success = true,
                Data = new List<WgStaffPlanViewDto> { BuildDto() },
                Pagination = new PaginationDto { TotalRecords = 50, PageNumber = 2, PageSize = 20 }
            };

            _mapper.Map<QueryParameters<string>>(request)
                   .Returns(new QueryParameters<string> { Page = 2, PageSize = 20 });
            _profitCentreService.GetPagedWgStaffPlanAsync(Arg.Any<QueryParameters<string>>(), TestWorkGroup)
                               .Returns(staffPlanData);
            _mapper.Map<List<WgStaffPlanViewItem>>(Arg.Any<List<WgStaffPlanViewDto>>())
                   .Returns(new List<WgStaffPlanViewItem> { BuildViewItem() });

            // Act
            var result = await _controller.LoadGrid(request, TestWorkGroup);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<WgStaffPlanViewItem>>(partialViewResult.Model);
            Assert.Equal("StaffName", config.Pagination.SortColumn);
            Assert.True(config.Pagination.SortDirection);
            Assert.Equal(2, config.Pagination.PageNumber);
            Assert.Equal(20, config.Pagination.PageSize);
        }

        #endregion
    }
}
