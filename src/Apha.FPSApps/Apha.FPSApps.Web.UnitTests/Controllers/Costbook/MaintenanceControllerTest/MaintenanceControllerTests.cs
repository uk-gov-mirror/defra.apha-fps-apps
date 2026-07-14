using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Controllers;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.Costbook.MaintenanceControllerTest
{
    public class MaintenanceControllerTests
    {
        private readonly IMapper _mapper;
        private readonly ICostBookMaintenanceService _maintenanceService;
        private readonly ICostBookAccountGroupService _accountGroupService;
        private readonly ICostBookCapsStaffService _capsStaffService;
        private readonly MaintenanceController _controller;

        public MaintenanceControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _maintenanceService = Substitute.For<ICostBookMaintenanceService>();
            _accountGroupService = Substitute.For<ICostBookAccountGroupService>();
            _capsStaffService = Substitute.For<ICostBookCapsStaffService>();
            _controller = new MaintenanceController(_mapper, _maintenanceService, _accountGroupService, _capsStaffService);
            _controller.TempData = Substitute.For<ITempDataDictionary>();
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        [Fact]
        public async Task Index_WithSettings_PopulatesViewModelAndDropdowns()
        {
            // Arrange
            var settingsDto = new MaintenanceSettingsDto
            {
                InflationAnimals = 2.5m,
                InflationExceptionalCosts = 1.1m,
                InflationStaff = 3.0m,
                InflationTests = 2.0m,
                CurrentFinancialYear = 2024,
                WorkingHoursInDay = 7.4m,
                WorkingDaysInYear = 220m,
                ProfitAnimals = 15m,
                ProfitExceptionalCosts = 12.5m,
                ProfitStaff = 10m,
                ProfitTests = 8m
            };
            var settingsResp = ApiResponseDto<MaintenanceSettingsDto>.SuccessResponse(settingsDto);
            _maintenanceService.GetSettingsAsync().Returns(settingsResp);

            var groups = new List<AccountGroupDto> { new() { Csg7Group = "CSG001" } };
            _accountGroupService.GetAllAccountGroupsAsync().Returns(ApiResponseDto<List<AccountGroupDto>>.SuccessResponse(groups));

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<MaintenanceViewModel>(viewResult.Model);
            Assert.Equal(2.5m, model.InflationAnimals);
            Assert.NotNull(model.Csg7GroupList);
            Assert.Single(model.Csg7GroupList);
        }

        [Fact]
        public async Task SaveInflationSettings_ValidRequest_WhenUpdateSucceeds_ReturnsSuccessJson()
        {
            // Arrange
            var item = new InflationSettingsItem
            {
                InflationAnimals = 1.0m,
                InflationExceptionalCosts = 0.5m,
                InflationStaff = 2.0m,
                InflationTests = 1.2m,
                CurrentFinancialYear = 2025,
                WorkingHoursInDay = 7.5m,
                WorkingDaysInYear = 225m
            };

            _maintenanceService.GetSettingsAsync().Returns(ApiResponseDto<MaintenanceSettingsDto>.SuccessResponse(new MaintenanceSettingsDto()));
            _maintenanceService.UpdateSettingsAsync(Arg.Any<MaintenanceSettingsDto>())
                .Returns(ApiResponseDto<MaintenanceSettingsDto>.SuccessResponse(new MaintenanceSettingsDto()));

            // Act
            var result = await _controller.SaveInflationSettings(item);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var obj = GetJsonResultElement(json);
            Assert.True(obj.GetProperty("success").GetBoolean());
            Assert.Equal("Inflation values saved successfully.", obj.GetProperty("message").GetString());
            await _maintenanceService.Received(1).UpdateSettingsAsync(Arg.Any<MaintenanceSettingsDto>());
        }

        [Fact]
        public async Task SaveInflationSettings_InvalidModel_ReturnsValidationJson()
        {
            // Arrange
            var item = new InflationSettingsItem { InflationAnimals = 1m };
            _controller.ModelState.AddModelError("InflationAnimals", "Required");

            // Act
            var result = await _controller.SaveInflationSettings(item);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var obj = GetJsonResultElement(json);
            Assert.False(obj.GetProperty("success").GetBoolean());
            Assert.True(obj.TryGetProperty("errors", out _));
        }

        [Fact]
        public async Task LoadAccountCategoryGrid_ValidRequest_ReturnsPartialViewWithGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var dtoList = new List<AccountCategoryMaintenanceDto>
            {
                new() { AccShortName = "ACC1", Csg7Group = "CSG1" }
            };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var pagedResp = ApiResponseDto<List<AccountCategoryMaintenanceDto>>.SuccessResponse(dtoList, pagination);

            var mappedItems = new List<AccountCategoryItem> { new() { AccShortName = "ACC1", Csg7Group = "CSG1" } };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 };

            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());
            _maintenanceService.GetPaginatedAccountCategoriesAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedResp);
            _mapper.Map<List<AccountCategoryItem>>(dtoList).Returns(mappedItems);
            _mapper.Map<PaginationModel>(pagination).Returns(paginationModel);

            // Act
            var result = await _controller.LoadAccountCategoryGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<AccountCategoryItem>>(partial.Model);
            Assert.Single(config.Data);
            Assert.Equal("accCatGrid", config.GridId);
        }

        [Fact]
        public async Task EditAccountCategory_Get_WhenFound_ReturnsPartialWithItem()
        {
            // Arrange
            var accShortName = "ACC1";
            var dto = new AccountCategoryMaintenanceDto { AccShortName = accShortName, Csg7Group = "CSG1" };
            _maintenanceService.GetAccountCategoriesAsync().Returns(ApiResponseDto<List<AccountCategoryMaintenanceDto>>.SuccessResponse(new List<AccountCategoryMaintenanceDto> { dto }));

            // ensure account group lookup used by PopulateAccCatDropdownAsync returns a value to avoid NRE
            var groups = new List<AccountGroupDto> { new() { Csg7Group = "CSG1" } };
            _accountGroupService.GetAllAccountGroupsAsync().Returns(ApiResponseDto<List<AccountGroupDto>>.SuccessResponse(groups));

            var mapped = new AccountCategoryItem { AccShortName = accShortName, Csg7Group = "CSG1" };
            _mapper.Map<AccountCategoryItem>(dto).Returns(mapped);

            // Act
            var result = await _controller.EditAccountCategory(accShortName);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<AccountCategoryItem>(partial.Model);
            Assert.Equal(accShortName, model.AccShortName);
        }

        [Fact]
        public async Task EditAccountCategory_Get_WhenNotFound_ReturnsNotFound()
        {
            // Arrange
            _maintenanceService.GetAccountCategoriesAsync().Returns(ApiResponseDto<List<AccountCategoryMaintenanceDto>>.SuccessResponse(new List<AccountCategoryMaintenanceDto>()));

            // Act
            var result = await _controller.EditAccountCategory("MISSING");

            // Assert
            var nf = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Contains("not found", nf.Value!.ToString()!, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CreateCsg7Group_Get_ReturnsPartialView()
        {
            // Act
            var result = _controller.CreateCsg7Group();

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditCsg7Group", partial.ViewName);
        }

        [Fact]
        public async Task CreateCsg7Group_Post_OnSuccess_ReturnsSuccessJson()
        {
            // Arrange
            var dto = new AccountGroupDto { Csg7Group = "CSG_NEW" };
            _accountGroupService.AddAccountGroupAsync(dto).Returns(ApiResponseDto<AccountGroupDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.CreateCsg7Group(dto);

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var obj = GetJsonResultElement(json);
            Assert.True(obj.GetProperty("success").GetBoolean());
            Assert.Equal("CSG7 group saved successfully.", obj.GetProperty("message").GetString());
        }

        [Fact]
        public async Task DeleteCsg7Group_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var dto = ApiResponseDto<bool>.FailureResponse(new List<ApiErrorDto> { new() { Message = "Child constraints", Code = "CHILD" } }, new ApiMetaDto());
            _accountGroupService.DeleteAccountGroupAsync("CSG001").Returns(dto);

            // Act
            var result = await _controller.DeleteCsg7Group("CSG001");

            // Assert
            var json = Assert.IsType<JsonResult>(result);
            var obj = GetJsonResultElement(json);
            Assert.False(obj.GetProperty("success").GetBoolean());
            Assert.True(obj.TryGetProperty("message", out _));
        }

        [Fact]
        public async Task LoadCapsStaffGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var dtoList = new List<StaffDto> { new() { Mnumber = "M1" } };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var pagedResp = ApiResponseDto<List<StaffDto>>.SuccessResponse(dtoList, pagination);

            var mappedItems = new List<CapsStaffItem> { new() { MNumber = "M1" } };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 };

            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());
            _capsStaffService.GetPaginatedCapsStaffAsync(Arg.Any<QueryParameters<string>>()).Returns(pagedResp);
            _mapper.Map<List<CapsStaffItem>>(dtoList).Returns(mappedItems);
            _mapper.Map<PaginationModel>(pagination).Returns(paginationModel);

            // Act
            var result = await _controller.LoadCapsStaffGrid(request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<CapsStaffItem>>(partial.Model);
            Assert.Equal("capsStaffGrid", config.GridId);
            Assert.Single(config.Data);
        }
    }
}
