using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Controllers;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.Costbook.YearlyDetailsControllerTest;

public class YearlyDetailsControllerTests
{
    private readonly ICostBookYearlyDetailsService _service;
    private readonly ICostBookProjectSummaryService _summaryService;
    private readonly IMapper _mapper;
    private readonly YearlyDetailsController _controller;
    private static readonly string[] value = ["WgGrade is required."];

    public YearlyDetailsControllerTests()
    {
        _service = Substitute.For<ICostBookYearlyDetailsService>();
        _summaryService = Substitute.For<ICostBookProjectSummaryService>();
        _mapper = Substitute.For<IMapper>();
        _controller = new YearlyDetailsController(_service, _summaryService, _mapper);
        _controller.TempData = Substitute.For<ITempDataDictionary>();

        var urlHelper = Substitute.For<IUrlHelper>();
        urlHelper.Action(Arg.Any<UrlActionContext>()).Returns(string.Empty);
        _controller.Url = urlHelper;
    }

    private static JsonElement GetJsonResultElement(JsonResult jsonResult)
    {
        var json = JsonSerializer.Serialize(jsonResult.Value);
        return JsonSerializer.Deserialize<JsonElement>(json);
    }

    #region Index Tests

    [Fact]
    public async Task Index_RedirectsToProjects_WhenHeaderNotFound()
    {
        // Arrange
        _service.GetProjectHeaderAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<ProjectHeaderDto>.FailureResponse(null, new ApiMetaDto()));

        // Act
        var result = await _controller.Index("2024/001");

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Projects", redirect.ControllerName);
    }

    [Fact]
    public async Task Index_ReturnsViewWithViewModel_WhenHeaderExists()
    {
        // Arrange
        var header = new ProjectHeaderDto { ProjectId = "2024/001", ProjectTitle = "Test", IsDefraProject = 0 };
        _service.GetProjectHeaderAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<ProjectHeaderDto>.SuccessResponse(header));

        var years = new List<ProjectYearDto> { new() { YearValue = 1 }, new() { YearValue = 2 } };
        _service.GetProjectYearsAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(years));

        _mapper.Map<List<ProjectYearRateItem>>(Arg.Any<List<ProjectYearDto>>())
            .Returns(new List<ProjectYearRateItem>());

        // Act
        var result = await _controller.Index("2024/001");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<YearlyDetailsViewModel>(viewResult.Model);
        Assert.Equal("2024/001", model.ProjectHeaderDto.ProjectId);
        Assert.Equal(2, model.ProjectYears.Count);
        Assert.Equal(1, model.SelectedYear); // defaults to first year
    }

    [Fact]
    public async Task Index_UsesSelectedYear_WhenProvided()
    {
        // Arrange
        var header = new ProjectHeaderDto { ProjectId = "2024/001", IsDefraProject = 0 };
        _service.GetProjectHeaderAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<ProjectHeaderDto>.SuccessResponse(header));

        var years = new List<ProjectYearDto> { new() { YearValue = 1 }, new() { YearValue = 2 } };
        _service.GetProjectYearsAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(years));

        _mapper.Map<List<ProjectYearRateItem>>(Arg.Any<List<ProjectYearDto>>())
            .Returns(new List<ProjectYearRateItem>());

        // Act
        var result = await _controller.Index("2024/001", 2);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<YearlyDetailsViewModel>(viewResult.Model);
        Assert.Equal(2, model.SelectedYear);
    }

    #endregion

    #region AddProjectYear Tests

    [Fact]
    public async Task AddProjectYearGet_ReturnsPartialView()
    {
        // Act
        var result = _controller.AddProjectYearGet("2024/001", 3);

        // Assert
        var partialResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_AddProjectYear", partialResult.ViewName);
        var model = Assert.IsType<ProjectYearRateItem>(partialResult.Model);
        Assert.Equal(3, model.YearValue);
    }

    [Fact]
    public async Task AddProjectYear_ReturnsSuccess_WhenServiceSucceeds()
    {
        // Arrange
        var item = new ProjectYearRateItem { YearValue = 2 };
        var dto = new ProjectYearDto { YearValue = 2 };

        _mapper.Map<ProjectYearDto>(item).Returns(dto);
        _service.AddProjectYearAsync(Arg.Any<string>(), 2, dto)
            .Returns(ApiResponseDto<ProjectYearDto>.SuccessResponse(new ProjectYearDto { YearValue = 2 }));

        // Act
        var result = await _controller.AddProjectYear("2024/001", 2, item);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.True(element.GetProperty("success").GetBoolean());
        Assert.Equal(2, element.GetProperty("year").GetInt32());
    }

    [Fact]
    public async Task AddProjectYear_ReturnsFailure_WhenServiceFails()
    {
        // Arrange
        _mapper.Map<ProjectYearDto>(Arg.Any<ProjectYearRateItem>()).Returns(new ProjectYearDto());
        _service.AddProjectYearAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<ProjectYearDto>())
            .Returns(ApiResponseDto<ProjectYearDto>.FailureResponse(null, new ApiMetaDto()));

        // Act
        var result = await _controller.AddProjectYear("2024/001", 2, new ProjectYearRateItem());

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
    }

    #endregion

    #region Staff CRUD Tests

    [Fact]
    public async Task CreateStaff_Post_ReturnsSuccess_WhenServiceSucceeds()
    {
        // Arrange
        var item = new StaffRequirementFormItem { WgGrade = "HEO" };
        var dto = new StaffRequirementDto();

        _mapper.Map<StaffRequirementDto>(item).Returns(dto);
        _service.AddStaffRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), dto)
            .Returns(ApiResponseDto<StaffRequirementDto>.SuccessResponse(new StaffRequirementDto()));

        // Act
        var result = await _controller.CreateStaff("2024/001", 2024, item);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.True(element.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task CreateStaff_Post_ReturnsFailure_WhenServiceFails()
    {
        // Arrange
        _mapper.Map<StaffRequirementDto>(Arg.Any<StaffRequirementFormItem>()).Returns(new StaffRequirementDto());
        _service.AddStaffRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<StaffRequirementDto>())
            .Returns(ApiResponseDto<StaffRequirementDto>.FailureResponse(null, new ApiMetaDto()));

        // Act
        var result = await _controller.CreateStaff("2024/001", 2024, new StaffRequirementFormItem { WgGrade = "HEO" });

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task EditStaff_Get_ReturnsNotFound_WhenRowNotFound()
    {
        // Arrange
        var pagedResult = new PaginatedResult<StaffRequirementDto>(new List<StaffRequirementDto>(), 0);
        _service.GetStaffRequirementsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<QueryParameters<string>>())
            .Returns(ApiResponseDto<PaginatedResult<StaffRequirementDto>>.SuccessResponse(pagedResult));
        _service.GetPayRatesAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>())
            .Returns(ApiResponseDto<List<PayRateDto>>.SuccessResponse(new List<PayRateDto>()));

        // Act
        var result = await _controller.EditStaff("2024/001", 2024, 999, false);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditStaff_Get_ReturnsPartialView_WhenRowExists()
    {
        // Arrange
        var staffDto = new StaffRequirementDto { SrIdentity = 1, WgGrade = "HEO" };
        var pagedResult = new PaginatedResult<StaffRequirementDto>(new List<StaffRequirementDto> { staffDto }, 1);
        var staffItem = new StaffRequirementFormItem { SrIdentity = 1, WgGrade = "HEO" };

        _service.GetStaffRequirementsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<QueryParameters<string>>())
            .Returns(ApiResponseDto<PaginatedResult<StaffRequirementDto>>.SuccessResponse(pagedResult));
        _service.GetPayRatesAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>())
            .Returns(ApiResponseDto<List<PayRateDto>>.SuccessResponse(new List<PayRateDto>()));
        _mapper.Map<StaffRequirementFormItem>(staffDto).Returns(staffItem);

        // Act
        var result = await _controller.EditStaff("2024/001", 2024, 1, false);

        // Assert
        var partialResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_AddEditStaffRequirement", partialResult.ViewName);
    }

    [Fact]
    public async Task EditStaff_Post_ReturnsSuccess_WhenServiceSucceeds()
    {
        // Arrange
        var item = new StaffRequirementFormItem { WgGrade = "HEO" };
        _mapper.Map<StaffRequirementDto>(item).Returns(new StaffRequirementDto());
        _service.UpdateStaffRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<StaffRequirementDto>())
            .Returns(ApiResponseDto<StaffRequirementDto>.SuccessResponse(new StaffRequirementDto()));

        // Act
        var result = await _controller.EditStaff("2024/001", 2024, 1, item);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.True(element.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task DeleteStaff_ReturnsSuccess_WhenServiceSucceeds()
    {
        // Arrange
        _service.DeleteStaffRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), 1)
            .Returns(ApiResponseDto<bool>.SuccessResponse(true));

        // Act
        var result = await _controller.DeleteStaff("2024/001", 2024, 1);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.True(element.GetProperty("success").GetBoolean());
    }

    #endregion

    #region Test CRUD Tests

    [Fact]
    public async Task CreateTest_Post_ReturnsSuccess_WhenServiceSucceeds()
    {
        _mapper.Map<TestRequirementDto>(Arg.Any<TestRequirementItem>()).Returns(new TestRequirementDto());
        _service.AddTestRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<TestRequirementDto>())
            .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(new TestRequirementDto()));

        var result = await _controller.CreateTest("2024/001", 2024, new TestRequirementItem { TestCode = "TC001" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.True(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task EditTest_Get_ReturnsNotFound_WhenRowNotFound()
    {
        var pagedResult = new PaginatedResult<TestRequirementDto>(new List<TestRequirementDto>(), 0);
        _service.GetTestRequirementsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<QueryParameters<string>>())
            .Returns(ApiResponseDto<PaginatedResult<TestRequirementDto>>.SuccessResponse(pagedResult));
        _service.GetTestCodeLookupsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>())
            .Returns(ApiResponseDto<List<TestCodeLookupDto>>.SuccessResponse(new List<TestCodeLookupDto>()));

        var result = await _controller.EditTest("2024/001", 2024, "NOTFOUND", false);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditTest_Post_ReturnsSuccess_WhenServiceSucceeds()
    {
        _mapper.Map<TestRequirementDto>(Arg.Any<TestRequirementItem>()).Returns(new TestRequirementDto());
        _service.UpdateTestRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<TestRequirementDto>())
            .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(new TestRequirementDto()));

        var result = await _controller.EditTest("2024/001", 2024, "TC001", new TestRequirementItem { TestCode = "TC001" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.True(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task DeleteTest_ReturnsSuccess_WhenServiceSucceeds()
    {
        _service.DeleteTestRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), "TC001")
            .Returns(ApiResponseDto<bool>.SuccessResponse(true));

        var result = await _controller.DeleteTest("2024/001", 2024, "TC001");

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.True(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    #endregion

    #region Animal CRUD Tests

    [Fact]
    public async Task CreateAnimal_Post_ReturnsSuccess_WhenServiceSucceeds()
    {
        _mapper.Map<AnimalRequirementDto>(Arg.Any<AnimalRequirementItem>()).Returns(new AnimalRequirementDto());
        _service.AddAnimalRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<AnimalRequirementDto>())
            .Returns(ApiResponseDto<AnimalRequirementDto>.SuccessResponse(new AnimalRequirementDto()));

        var result = await _controller.CreateAnimal("2024/001", 2024, new AnimalRequirementItem { AnimalType = "CAT" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.True(element.GetProperty("success").GetBoolean());
        Assert.Equal("Animal Record Added Successfully", element.GetProperty("message").GetString());
    }

    [Fact]
    public async Task EditAnimal_Get_ReturnsNotFound_WhenRowNotFound()
    {
        _service.GetAnimalRequirementsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<QueryParameters<string>>())
            .Returns(ApiResponseDto<PaginatedResult<AnimalRequirementDto>>.SuccessResponse(new PaginatedResult<AnimalRequirementDto>(new List<AnimalRequirementDto>(), 0)));
        _service.GetAnimalRatesAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>())
            .Returns(ApiResponseDto<List<AnimalRateDto>>.SuccessResponse(new List<AnimalRateDto>()));

        var result = await _controller.EditAnimal("2024/001", 2024, 999, false);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditAnimal_Post_ReturnsSuccess_WhenServiceSucceeds()
    {
        _mapper.Map<AnimalRequirementDto>(Arg.Any<AnimalRequirementItem>()).Returns(new AnimalRequirementDto());
        _service.UpdateAnimalRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<AnimalRequirementDto>())
            .Returns(ApiResponseDto<AnimalRequirementDto>.SuccessResponse(new AnimalRequirementDto()));

        var result = await _controller.EditAnimal("2024/001", 2024, 1, new AnimalRequirementItem { AnimalType = "CAT" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.True(element.GetProperty("success").GetBoolean());
        Assert.Equal("Animal Record Updated Successfully", element.GetProperty("message").GetString());
    }

    [Fact]
    public async Task DeleteAnimal_ReturnsSuccess_WhenServiceSucceeds()
    {
        _service.DeleteAnimalRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), 1)
            .Returns(ApiResponseDto<bool>.SuccessResponse(true));

        var result = await _controller.DeleteAnimal("2024/001", 2024, 1);

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.True(element.GetProperty("success").GetBoolean());
        Assert.Equal("Animal Record Deleted Successfully", element.GetProperty("message").GetString());
    }

    #endregion

    #region AdditionalCost CRUD Tests

    [Fact]
    public async Task CreateAdditionalCost_Get_ReturnsPartialView()
    {
        _service.GetAccountCategoriesAsync()
            .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));

        var result = await _controller.CreateAdditionalCost("2024/001", 2024);

        var partialResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_AddEditAdditionalCost", partialResult.ViewName);
    }

    [Fact]
    public async Task CreateAdditionalCost_Post_ReturnsSuccess_WhenServiceSucceeds()
    {
        _mapper.Map<AdditionalCostDto>(Arg.Any<AdditionalCostItem>()).Returns(new AdditionalCostDto());
        _service.AddAdditionalCostAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<AdditionalCostDto>())
            .Returns(ApiResponseDto<AdditionalCostDto>.SuccessResponse(new AdditionalCostDto()));

        var result = await _controller.CreateAdditionalCost("2024/001", 2024,
            new AdditionalCostItem { Description = "Travel", AccountCat = "TRAVEL" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.True(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task EditAdditionalCost_Get_ReturnsNotFound_WhenRowNotFound()
    {
        _service.GetAdditionalCostsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<QueryParameters<string>>())
            .Returns(ApiResponseDto<PaginatedResult<AdditionalCostDto>>.SuccessResponse(new PaginatedResult<AdditionalCostDto>(new List<AdditionalCostDto>(), 0)));
        _service.GetAccountCategoriesAsync()
            .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));

        var result = await _controller.EditAdditionalCost("2024/001", 2024, 999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditAdditionalCost_Post_ReturnsSuccess_WhenServiceSucceeds()
    {
        _mapper.Map<AdditionalCostDto>(Arg.Any<AdditionalCostItem>()).Returns(new AdditionalCostDto());
        _service.UpdateAdditionalCostAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<AdditionalCostDto>())
            .Returns(ApiResponseDto<AdditionalCostDto>.SuccessResponse(new AdditionalCostDto()));

        var result = await _controller.EditAdditionalCost("2024/001", 2024, 1,
            new AdditionalCostItem { Description = "Travel", AccountCat = "TRAVEL" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.True(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task DeleteAdditionalCost_ReturnsSuccess_WhenServiceSucceeds()
    {
        _service.DeleteAdditionalCostAsync(Arg.Any<string>(), Arg.Any<int>(), 1)
            .Returns(ApiResponseDto<bool>.SuccessResponse(true));

        var result = await _controller.DeleteAdditionalCost("2024/001", 2024, 1);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.True(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    #endregion

    #region MarkupAndProfit Tests

    [Fact]
    public async Task EditMarkupAndProfit_ReturnsNotFound_WhenYearNotFound()
    {
        _service.GetProjectYearsAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(new List<ProjectYearDto>()));

        var result = await _controller.EditMarkupAndProfit("2024/001", 99,"");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditMarkupAndProfit_ReturnsPartialView_WhenYearExists()
    {
        var yearDto = new ProjectYearDto { YearValue = 1 };
        var rateItem = new ProjectYearRateItem { YearValue = 1 };

        _service.GetProjectYearsAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(new List<ProjectYearDto> { yearDto }));
        _mapper.Map<ProjectYearRateItem>(yearDto).Returns(rateItem);

        var result = await _controller.EditMarkupAndProfit("2024/001", 1,"");

        var partialResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_AddProjectYear", partialResult.ViewName);
    }

    [Fact]
    public async Task UpdateProjectYearRate_ReturnsSuccess_WhenServiceSucceeds()
    {
        _mapper.Map<ProjectYearDto>(Arg.Any<ProjectYearRateItem>()).Returns(new ProjectYearDto());
        _service.UpdateProjectYearAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<ProjectYearDto>())
            .Returns(ApiResponseDto<ProjectYearDto>.SuccessResponse(new ProjectYearDto()));

        var result = await _controller.UpdateProjectYearRate("2024/001", 1, new ProjectYearRateItem());

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.True(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    #endregion

    #region Helpers

    #endregion

    #region CreateStaff GET

    [Fact]
    public async Task CreateStaff_Get_ReturnsPartialView()
    {
        _service.GetPayRatesAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>())
            .Returns(ApiResponseDto<List<PayRateDto>>.SuccessResponse(new List<PayRateDto>()));

        var result = await _controller.CreateStaff("2024/001", 2024, false);

        var partialResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_AddEditStaffRequirement", partialResult.ViewName);
    }

    #endregion

    #region CreateTest GET

    [Fact]
    public async Task CreateTest_Get_ReturnsPartialView()
    {
        _service.GetTestCodeLookupsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>())
            .Returns(ApiResponseDto<List<TestCodeLookupDto>>.SuccessResponse(new List<TestCodeLookupDto>()));

        var result = await _controller.CreateTest("2024/001", 2024, false);

        var partialResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_AddEditTestRequirement", partialResult.ViewName);
    }

    #endregion

    #region CreateAnimal GET

    [Fact]
    public async Task CreateAnimal_Get_ReturnsPartialView()
    {
        _service.GetAnimalRatesAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>())
            .Returns(ApiResponseDto<List<AnimalRateDto>>.SuccessResponse(new List<AnimalRateDto>()));

        var result = await _controller.CreateAnimal("2024/001", 2024, false);

        var partialResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_AddEditAnimalRequirement", partialResult.ViewName);
    }

    #endregion

    #region ModelState Invalid Tests

    [Fact]
    public async Task CreateStaff_Post_ReturnsErrors_WhenModelStateInvalid()
    {
        _controller.ModelState.AddModelError("WgGrade", "WG Grade is required.");

        var result = await _controller.CreateStaff("2024/001", 2024, new StaffRequirementFormItem());

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
        Assert.True(element.GetProperty("errors").GetArrayLength() > 0);
    }

    [Fact]
    public async Task EditStaff_Post_ReturnsErrors_WhenModelStateInvalid()
    {
        _controller.ModelState.AddModelError("WgGrade", "WG Grade is required.");

        var result = await _controller.EditStaff("2024/001", 2024, 1, new StaffRequirementFormItem());

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
        Assert.True(element.GetProperty("errors").GetArrayLength() > 0);
    }

    [Fact]
    public async Task CreateTest_Post_ReturnsErrors_WhenModelStateInvalid()
    {
        _controller.ModelState.AddModelError("TestCode", "Test Code is required.");

        var result = await _controller.CreateTest("2024/001", 2024, new TestRequirementItem { TestCode = "" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
        Assert.True(element.GetProperty("errors").GetArrayLength() > 0);
    }

    [Fact]
    public async Task EditTest_Post_ReturnsErrors_WhenModelStateInvalid()
    {
        _controller.ModelState.AddModelError("TestCode", "Test Code is required.");

        var result = await _controller.EditTest("2024/001", 2024, "TC001", new TestRequirementItem { TestCode = "" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task CreateAnimal_Post_ReturnsErrors_WhenModelStateInvalid()
    {
        _controller.ModelState.AddModelError("AnimalType", "Animal Type is required.");

        var result = await _controller.CreateAnimal("2024/001", 2024, new AnimalRequirementItem { AnimalType = "" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task EditAnimal_Post_ReturnsErrors_WhenModelStateInvalid()
    {
        _controller.ModelState.AddModelError("AnimalType", "Animal Type is required.");

        var result = await _controller.EditAnimal("2024/001", 2024, 1, new AnimalRequirementItem { AnimalType = "" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task CreateAdditionalCost_Post_ReturnsErrors_WhenModelStateInvalid()
    {
        _controller.ModelState.AddModelError("Description", "Description is required.");

        var result = await _controller.CreateAdditionalCost("2024/001", 2024, new AdditionalCostItem { Description = "", AccountCat = "" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task EditAdditionalCost_Post_ReturnsErrors_WhenModelStateInvalid()
    {
        _controller.ModelState.AddModelError("Description", "Description is required.");

        var result = await _controller.EditAdditionalCost("2024/001", 2024, 1, new AdditionalCostItem { Description = "", AccountCat = "" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
    }

    #endregion

    #region Service Failure Tests

    [Fact]
    public async Task EditStaff_Post_ReturnsErrors_WhenServiceFails()
    {
        _mapper.Map<StaffRequirementDto>(Arg.Any<StaffRequirementFormItem>()).Returns(new StaffRequirementDto());
        _service.UpdateStaffRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<StaffRequirementDto>())
            .Returns(ApiResponseDto<StaffRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Update failed" } }, new ApiMetaDto()));

        var result = await _controller.EditStaff("2024/001", 2024, 1, new StaffRequirementFormItem { WgGrade = "HEO" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
        Assert.True(element.GetProperty("errors").GetArrayLength() > 0);
    }

    [Fact]
    public async Task CreateTest_Post_ReturnsErrors_WhenServiceFails()
    {
        _mapper.Map<TestRequirementDto>(Arg.Any<TestRequirementItem>()).Returns(new TestRequirementDto());
        _service.AddTestRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<TestRequirementDto>())
            .Returns(ApiResponseDto<TestRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _controller.CreateTest("2024/001", 2024, new TestRequirementItem { TestCode = "TC001" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.False(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task EditTest_Post_ReturnsErrors_WhenServiceFails()
    {
        _mapper.Map<TestRequirementDto>(Arg.Any<TestRequirementItem>()).Returns(new TestRequirementDto());
        _service.UpdateTestRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<TestRequirementDto>())
            .Returns(ApiResponseDto<TestRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _controller.EditTest("2024/001", 2024, "TC001", new TestRequirementItem { TestCode = "TC001" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.False(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task CreateAnimal_Post_ReturnsErrors_WhenServiceFails()
    {
        _mapper.Map<AnimalRequirementDto>(Arg.Any<AnimalRequirementItem>()).Returns(new AnimalRequirementDto());
        _service.AddAnimalRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<AnimalRequirementDto>())
            .Returns(ApiResponseDto<AnimalRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _controller.CreateAnimal("2024/001", 2024, new AnimalRequirementItem { AnimalType = "CAT" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.False(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task EditAnimal_Post_ReturnsErrors_WhenServiceFails()
    {
        _mapper.Map<AnimalRequirementDto>(Arg.Any<AnimalRequirementItem>()).Returns(new AnimalRequirementDto());
        _service.UpdateAnimalRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<AnimalRequirementDto>())
            .Returns(ApiResponseDto<AnimalRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _controller.EditAnimal("2024/001", 2024, 1, new AnimalRequirementItem { AnimalType = "CAT" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.False(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task CreateAdditionalCost_Post_ReturnsErrors_WhenServiceFails()
    {
        _mapper.Map<AdditionalCostDto>(Arg.Any<AdditionalCostItem>()).Returns(new AdditionalCostDto());
        _service.AddAdditionalCostAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<AdditionalCostDto>())
            .Returns(ApiResponseDto<AdditionalCostDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _controller.CreateAdditionalCost("2024/001", 2024,
            new AdditionalCostItem { Description = "Travel", AccountCat = "TRAVEL" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.False(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task EditAdditionalCost_Post_ReturnsErrors_WhenServiceFails()
    {
        _mapper.Map<AdditionalCostDto>(Arg.Any<AdditionalCostItem>()).Returns(new AdditionalCostDto());
        _service.UpdateAdditionalCostAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<AdditionalCostDto>())
            .Returns(ApiResponseDto<AdditionalCostDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Failed" } }, new ApiMetaDto()));

        var result = await _controller.EditAdditionalCost("2024/001", 2024, 1,
            new AdditionalCostItem { Description = "Travel", AccountCat = "TRAVEL" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.False(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    #endregion

    #region Index – edge cases

    [Fact]
    public async Task Index_RedirectsToProjects_WhenHeaderDataIsNull()
    {
        _service.GetProjectHeaderAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<ProjectHeaderDto>.SuccessResponse(null!));

        var result = await _controller.Index("2024/001");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
        Assert.Equal("Projects", redirect.ControllerName);
    }

    [Fact]
    public async Task Index_UsesEmptyYears_WhenYearsResponseFails()
    {
        var header = new ProjectHeaderDto { ProjectId = "2024/001", IsDefraProject = 0, StartFYear = 0 };
        _service.GetProjectHeaderAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<ProjectHeaderDto>.SuccessResponse(header));
        _service.GetProjectYearsAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<List<ProjectYearDto>>.FailureResponse(null, new ApiMetaDto()));

        var result = await _controller.Index("2024/001");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<YearlyDetailsViewModel>(viewResult.Model);
        Assert.Empty(model.ProjectYears);
        Assert.Equal(0, model.SelectedYear);
    }

    [Fact]
    public async Task Index_PopulatesDefraDropdowns_WhenIsDefraProject()
    {
        var header = new ProjectHeaderDto { ProjectId = "2024/001", IsDefraProject = -1 };
        _service.GetProjectHeaderAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<ProjectHeaderDto>.SuccessResponse(header));

        var years = new List<ProjectYearDto> { new() { YearValue = 1 } };
        _service.GetProjectYearsAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(years));

        _mapper.Map<List<ProjectYearRateItem>>(Arg.Any<List<ProjectYearDto>>())
            .Returns(new List<ProjectYearRateItem>());

        var result = await _controller.Index("2024/001", 1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<YearlyDetailsViewModel>(viewResult.Model);
        Assert.Equal(1, model.SelectedYear);
    }

    #endregion

    #region AddProjectYear / UpdateProjectYearRate – ModelState invalid

    [Fact]
    public async Task AddProjectYear_Post_ReturnsErrors_WhenModelStateInvalid()
    {
        _controller.ModelState.AddModelError("YearValue", "Year is required.");

        var result = await _controller.AddProjectYear("2024/001", 1, new ProjectYearRateItem());

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
        Assert.True(element.GetProperty("errors").GetArrayLength() > 0);
    }

    [Fact]
    public async Task UpdateProjectYearRate_Post_ReturnsErrors_WhenModelStateInvalid()
    {
        _controller.ModelState.AddModelError("YearValue", "Year is required.");

        var result = await _controller.UpdateProjectYearRate("2024/001", 1, new ProjectYearRateItem());

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
        Assert.True(element.GetProperty("errors").GetArrayLength() > 0);
    }

    #endregion

    #region Grid Loaders – LoadXxxGrid

    [Fact]
    public async Task LoadStaffGrid_ReturnsPartialView()
    {
        var pagedResult = new PaginatedResult<StaffRequirementDto>(new List<StaffRequirementDto>(), 0);
        _service.GetStaffRequirementsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<QueryParameters<string>>())
            .Returns(ApiResponseDto<PaginatedResult<StaffRequirementDto>>.SuccessResponse(pagedResult));
        _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
            .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
        _mapper.Map<List<StaffRequirementItem>>(Arg.Any<IEnumerable<StaffRequirementDto>>())
            .Returns(new List<StaffRequirementItem>());

        var result = await _controller.LoadStaffGrid(new PaginationFilter<string>(), "2024/001", 2024);

        Assert.IsType<PartialViewResult>(result);
    }

    [Fact]
    public async Task LoadStaffGrid_ReturnsEmptyData_WhenServiceFails()
    {
        _service.GetStaffRequirementsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<QueryParameters<string>>())
            .Returns(ApiResponseDto<PaginatedResult<StaffRequirementDto>>.FailureResponse(null, new ApiMetaDto()));
        _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
            .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });

        var result = await _controller.LoadStaffGrid(new PaginationFilter<string>(), "2024/001", 2024);

        Assert.IsType<PartialViewResult>(result);
    }

    [Fact]
    public async Task LoadTestGrid_ReturnsPartialView()
    {
        var pagedResult = new PaginatedResult<TestRequirementDto>(new List<TestRequirementDto>(), 0);
        _service.GetTestRequirementsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<QueryParameters<string>>())
            .Returns(ApiResponseDto<PaginatedResult<TestRequirementDto>>.SuccessResponse(pagedResult));
        _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
            .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
        _mapper.Map<List<TestRequirementItem>>(Arg.Any<IEnumerable<TestRequirementDto>>())
            .Returns(new List<TestRequirementItem>());

        var result = await _controller.LoadTestGrid(new PaginationFilter<string>(), "2024/001", 2024);

        Assert.IsType<PartialViewResult>(result);
    }

    [Fact]
    public async Task LoadTestGrid_ReturnsEmptyData_WhenServiceFails()
    {
        _service.GetTestRequirementsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<QueryParameters<string>>())
            .Returns(ApiResponseDto<PaginatedResult<TestRequirementDto>>.FailureResponse(null, new ApiMetaDto()));
        _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
            .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });

        var result = await _controller.LoadTestGrid(new PaginationFilter<string>(), "2024/001", 2024);

        Assert.IsType<PartialViewResult>(result);
    }

    [Fact]
    public async Task LoadAnimalGrid_ReturnsPartialView()
    {
        _service.GetAnimalRequirementsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<QueryParameters<string>>())
            .Returns(ApiResponseDto<PaginatedResult<AnimalRequirementDto>>.SuccessResponse(new PaginatedResult<AnimalRequirementDto>(new List<AnimalRequirementDto>(), 0)));
        _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
            .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
        _mapper.Map<List<AnimalRequirementItem>>(Arg.Any<IEnumerable<AnimalRequirementDto>>())
            .Returns(new List<AnimalRequirementItem>());

        var result = await _controller.LoadAnimalGrid(new PaginationFilter<string>(), "2024/001", 2024);

        Assert.IsType<PartialViewResult>(result);
    }

    [Fact]
    public async Task LoadAnimalGrid_ReturnsEmptyData_WhenServiceFails()
    {
        _service.GetAnimalRequirementsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<QueryParameters<string>>())
            .Returns(ApiResponseDto<PaginatedResult<AnimalRequirementDto>>.FailureResponse(null, new ApiMetaDto()));
        _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
            .Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });

        var result = await _controller.LoadAnimalGrid(new PaginationFilter<string>(), "2024/001", 2024);

        Assert.IsType<PartialViewResult>(result);
    }

    [Fact]
    public async Task LoadAdditionalCostGrid_ReturnsPartialView()
    {
        _service.GetAdditionalCostsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<QueryParameters<string>>())
            .Returns(ApiResponseDto<PaginatedResult<AdditionalCostDto>>.SuccessResponse(new PaginatedResult<AdditionalCostDto>(new List<AdditionalCostDto>(), 0)));
        _mapper.Map<List<AdditionalCostItem>>(Arg.Any<IEnumerable<AdditionalCostDto>>())
            .Returns(new List<AdditionalCostItem>());

        var result = await _controller.LoadAdditionalCostGrid(new PaginationFilter<string>(), "2024/001", 2024);

        Assert.IsType<PartialViewResult>(result);
    }

    [Fact]
    public async Task LoadAdditionalCostGrid_ReturnsEmptyData_WhenServiceFails()
    {
        _service.GetAdditionalCostsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<QueryParameters<string>>())
            .Returns(ApiResponseDto<PaginatedResult<AdditionalCostDto>>.FailureResponse(null, new ApiMetaDto()));

        var result = await _controller.LoadAdditionalCostGrid(new PaginationFilter<string>(), "2024/001", 2024);

        Assert.IsType<PartialViewResult>(result);
    }

    [Fact]
    public async Task LoadMarkupAndProfitGrid_ReturnsPartialView()
    {
        _service.GetProjectYearsAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(new List<ProjectYearDto> { new() { YearValue = 1 } }));
        _mapper.Map<List<ProjectYearRateItem>>(Arg.Any<IEnumerable<ProjectYearDto>>())
            .Returns(new List<ProjectYearRateItem> { new() { YearValue = 1 } });

        var result = await _controller.LoadMarkupAndProfitGrid(new PaginationFilter<string>(), "2024/001", 2024);

        Assert.IsType<PartialViewResult>(result);
    }

    [Fact]
    public async Task LoadMarkupAndProfitGrid_ReturnsEmptyData_WhenServiceFails()
    {
        _service.GetProjectYearsAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<List<ProjectYearDto>>.FailureResponse(null, new ApiMetaDto()));

        var result = await _controller.LoadMarkupAndProfitGrid(new PaginationFilter<string>(), "2024/001", 2024);

        Assert.IsType<PartialViewResult>(result);
    }

    #endregion

    #region AddProjectYearGet with Programme

    [Fact]
    public void AddProjectYearGet_IncludesProgramme_WhenProvided()
    {
        var result = _controller.AddProjectYearGet("2024/001", 2, "DEFRA-PROG");

        var partialResult = Assert.IsType<PartialViewResult>(result);
        var model = Assert.IsType<ProjectYearRateItem>(partialResult.Model);
        Assert.Equal("DEFRA-PROG", model.Programme);
    }

    [Fact]
    public void AddProjectYearGet_ProgrammeIsNull_WhenNotProvided()
    {
        var result = _controller.AddProjectYearGet("2024/001", 2);

        var partialResult = Assert.IsType<PartialViewResult>(result);
        var model = Assert.IsType<ProjectYearRateItem>(partialResult.Model);
        Assert.Null(model.Programme);
    }

    #endregion

    #region EditTest/Animal/AdditionalCost GET Success

    [Fact]
    public async Task EditTest_Get_ReturnsPartialView_WhenRowExists()
    {
        var testDto = new TestRequirementDto { TestCode = "TC001" };
        var pagedResult = new PaginatedResult<TestRequirementDto>(new List<TestRequirementDto> { testDto }, 1);
        _service.GetTestRequirementsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<QueryParameters<string>>())
            .Returns(ApiResponseDto<PaginatedResult<TestRequirementDto>>.SuccessResponse(pagedResult));
        _service.GetTestCodeLookupsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>())
            .Returns(ApiResponseDto<List<TestCodeLookupDto>>.SuccessResponse(new List<TestCodeLookupDto>()));
        _mapper.Map<TestRequirementItem>(testDto).Returns(new TestRequirementItem { TestCode = "TC001" });

        var result = await _controller.EditTest("2024/001", 2024, "TC001", false);

        var partialResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_AddEditTestRequirement", partialResult.ViewName);
    }

    [Fact]
    public async Task EditAnimal_Get_ReturnsPartialView_WhenRowExists()
    {
        var animalDto = new AnimalRequirementDto { ArIdentity = 1, AnimalType = "CAT" };
        _service.GetAnimalRequirementsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<QueryParameters<string>>())
            .Returns(ApiResponseDto<PaginatedResult<AnimalRequirementDto>>.SuccessResponse(new PaginatedResult<AnimalRequirementDto>(new List<AnimalRequirementDto> { animalDto }, 1)));
        _service.GetAnimalRatesAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>())
            .Returns(ApiResponseDto<List<AnimalRateDto>>.SuccessResponse(new List<AnimalRateDto>()));
        _mapper.Map<AnimalRequirementItem>(animalDto).Returns(new AnimalRequirementItem { ArIdentity = 1 });

        var result = await _controller.EditAnimal("2024/001", 2024, 1, false);

        var partialResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_AddEditAnimalRequirement", partialResult.ViewName);
    }

    [Fact]
    public async Task EditAdditionalCost_Get_ReturnsPartialView_WhenRowExists()
    {
        var acDto = new AdditionalCostDto { AcIdentity = 1, Description = "Travel" };
        _service.GetAdditionalCostsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<QueryParameters<string>>())
            .Returns(ApiResponseDto<PaginatedResult<AdditionalCostDto>>.SuccessResponse(new PaginatedResult<AdditionalCostDto>(new List<AdditionalCostDto> { acDto }, 1)));
        _service.GetAccountCategoriesAsync()
            .Returns(ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(new List<AccountCategoryDto>()));
        _mapper.Map<AdditionalCostItem>(acDto).Returns(new AdditionalCostItem { AcIdentity = 1 });

        var result = await _controller.EditAdditionalCost("2024/001", 2024, 1);

        var partialResult = Assert.IsType<PartialViewResult>(result);
        Assert.Equal("_AddEditAdditionalCost", partialResult.ViewName);
    }

    #endregion

    #region DeleteProjectYear

    [Fact]
    public async Task DeleteProjectYear_ReturnsSuccess_WhenServiceSucceeds()
    {
        _service.DeleteProjectYearAsync(Arg.Any<string>(), Arg.Any<int>())
            .Returns(ApiResponseDto<bool>.SuccessResponse(true));

        var result = await _controller.DeleteProjectYear("2024/001", 1);

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.True(element.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task DeleteProjectYear_ReturnsFailure_WithErrorMessage_WhenServiceFails()
    {
        _service.DeleteProjectYearAsync(Arg.Any<string>(), Arg.Any<int>())
            .Returns(ApiResponseDto<bool>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERR", Message = "Cannot delete year with records." } },
                new ApiMetaDto()));

        var result = await _controller.DeleteProjectYear("2024/001", 1);

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
        Assert.Equal("Cannot delete year with records.", element.GetProperty("message").GetString());
    }

    [Fact]
    public async Task DeleteProjectYear_ReturnsDefaultMessage_WhenServiceFailsWithNoErrors()
    {
        _service.DeleteProjectYearAsync(Arg.Any<string>(), Arg.Any<int>())
            .Returns(ApiResponseDto<bool>.FailureResponse(null, new ApiMetaDto()));

        var result = await _controller.DeleteProjectYear("2024/001", 1);

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
        Assert.Equal("Failed to delete project year.", element.GetProperty("message").GetString());
    }

    #endregion

    #region GetYearTotals

    [Fact]
    public async Task GetYearTotals_ReturnsJsonWithAllTotals()
    {
        var summary = new ProjectYearCostSummaryDto
        {
            Project = "2024/001",
            Year = 2024,
            StaffCostTotal = 100.0,
            TestCostTotal = 50.0,
            AnimalCostTotal = 25.0,
            AdditionalCostTotal = 10.0,
            GrandTotal = 185.0
        };
        _summaryService.GetProjectYearCostSummaryAsync("2024/001", 2024)
            .Returns(ApiResponseDto<ProjectYearCostSummaryDto>.SuccessResponse(summary));

        var result = await _controller.GetYearTotals("2024/001", 2024);

        var element = GetJsonResultElement(Assert.IsType<JsonResult>(result));
        Assert.Equal(100.0, element.GetProperty("staffCostTotal").GetDouble());
        Assert.Equal(50.0,  element.GetProperty("testCostTotal").GetDouble());
        Assert.Equal(25.0,  element.GetProperty("animalCostTotal").GetDouble());
        Assert.Equal(10.0,  element.GetProperty("additionalCostTotal").GetDouble());
        Assert.Equal(185.0, element.GetProperty("grandTotal").GetDouble());
        await _summaryService.Received(1).GetProjectYearCostSummaryAsync("2024/001", 2024);
    }

    [Fact]
    public async Task GetYearTotals_ReturnsZeroTotals_WhenServiceFails()
    {
        _summaryService.GetProjectYearCostSummaryAsync(Arg.Any<string>(), Arg.Any<int>())
            .Returns(ApiResponseDto<ProjectYearCostSummaryDto>.FailureResponse(null, new ApiMetaDto()));

        var result = await _controller.GetYearTotals("2024/001", 2024);

        var element = GetJsonResultElement(Assert.IsType<JsonResult>(result));
        Assert.Equal(0.0, element.GetProperty("staffCostTotal").GetDouble());
        Assert.Equal(0.0, element.GetProperty("testCostTotal").GetDouble());
        Assert.Equal(0.0, element.GetProperty("animalCostTotal").GetDouble());
        Assert.Equal(0.0, element.GetProperty("additionalCostTotal").GetDouble());
        Assert.Equal(0.0, element.GetProperty("grandTotal").GetDouble());
    }

    [Fact]
    public async Task GetYearTotals_ReturnsZeroTotals_WhenServiceReturnsNullData()
    {
        _summaryService.GetProjectYearCostSummaryAsync(Arg.Any<string>(), Arg.Any<int>())
            .Returns(ApiResponseDto<ProjectYearCostSummaryDto>.SuccessResponse(null!));

        var result = await _controller.GetYearTotals("2024/001", 2024);

        var element = GetJsonResultElement(Assert.IsType<JsonResult>(result));
        Assert.Equal(0.0, element.GetProperty("staffCostTotal").GetDouble());
        Assert.Equal(0.0, element.GetProperty("grandTotal").GetDouble());
    }

    [Fact]
    public async Task GetYearTotals_DecodesEncodedProjectId()
    {
        const string encodedId = "2024%2F001";
        const string decodedId = "2024/001";

        _summaryService.GetProjectYearCostSummaryAsync(Arg.Any<string>(), Arg.Any<int>())
            .Returns(ApiResponseDto<ProjectYearCostSummaryDto>.FailureResponse(null, new ApiMetaDto()));

        await _controller.GetYearTotals(encodedId, 2024);

        await _summaryService.Received(1).GetProjectYearCostSummaryAsync(decodedId, 2024);
    }

    #endregion

    #region Delete failure branches – Staff, Test, Animal, AdditionalCost

    [Fact]
    public async Task DeleteStaff_ReturnsFalse_WhenServiceReturnsFalse()
    {
        _service.DeleteStaffRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(ApiResponseDto<bool>.SuccessResponse(false));

        var result = await _controller.DeleteStaff("2024/001", 2024, 1);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.False(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task DeleteStaff_ReturnsFalse_WhenServiceFails()
    {
        _service.DeleteStaffRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(ApiResponseDto<bool>.FailureResponse(null, new ApiMetaDto()));

        var result = await _controller.DeleteStaff("2024/001", 2024, 1);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.False(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task DeleteTest_ReturnsFalse_WhenServiceReturnsFalse()
    {
        _service.DeleteTestRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>())
            .Returns(ApiResponseDto<bool>.SuccessResponse(false));

        var result = await _controller.DeleteTest("2024/001", 2024, "TC001");

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.False(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task DeleteTest_ReturnsFalse_WhenServiceFails()
    {
        _service.DeleteTestRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>())
            .Returns(ApiResponseDto<bool>.FailureResponse(null, new ApiMetaDto()));

        var result = await _controller.DeleteTest("2024/001", 2024, "TC001");

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.False(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task DeleteAnimal_ReturnsFalse_WhenServiceReturnsFalse()
    {
        _service.DeleteAnimalRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(ApiResponseDto<bool>.SuccessResponse(false));

        var result = await _controller.DeleteAnimal("2024/001", 2024, 1);

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
        Assert.Equal("Failed to delete animal entry.", element.GetProperty("message").GetString());
    }

    [Fact]
    public async Task DeleteAnimal_ReturnsFalse_WhenServiceFails()
    {
        _service.DeleteAnimalRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(ApiResponseDto<bool>.FailureResponse(null, new ApiMetaDto()));

        var result = await _controller.DeleteAnimal("2024/001", 2024, 1);

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
        Assert.Equal("Failed to delete animal entry.", element.GetProperty("message").GetString());
    }

    [Fact]
    public async Task DeleteAdditionalCost_ReturnsFalse_WhenServiceReturnsFalse()
    {
        _service.DeleteAdditionalCostAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(ApiResponseDto<bool>.SuccessResponse(false));

        var result = await _controller.DeleteAdditionalCost("2024/001", 2024, 1);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.False(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task DeleteAdditionalCost_ReturnsFalse_WhenServiceFails()
    {
        _service.DeleteAdditionalCostAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(ApiResponseDto<bool>.FailureResponse(null, new ApiMetaDto()));

        var result = await _controller.DeleteAdditionalCost("2024/001", 2024, 1);

        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.False(GetJsonResultElement(jsonResult).GetProperty("success").GetBoolean());
    }

    #endregion

    #region Index – YearRates populated when selectedYear > 0

    [Fact]
    public async Task Index_PopulatesYearRates_WhenSelectedYearIsGreaterThanZero()
    {
        var header = new ProjectHeaderDto { ProjectId = "2024/001", IsDefraProject = 0 };
        _service.GetProjectHeaderAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<ProjectHeaderDto>.SuccessResponse(header));

        var yearDto = new ProjectYearDto { YearValue = 1 };
        var years = new List<ProjectYearDto> { yearDto };
        _service.GetProjectYearsAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(years));

        var expectedRates = new List<ProjectYearRateItem> { new() { YearValue = 1 } };
        _mapper.Map<List<ProjectYearRateItem>>(Arg.Any<List<ProjectYearDto>>())
            .Returns(expectedRates);

        var result = await _controller.Index("2024/001", 1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<YearlyDetailsViewModel>(viewResult.Model);
        Assert.NotNull(model.YearRates);
        Assert.Single(model.YearRates);
        Assert.Equal(1, model.YearRates[0].YearValue);
    }

    [Fact]
    public async Task Index_DoesNotPopulateYearRates_WhenSelectedYearIsZero()
    {
        var header = new ProjectHeaderDto { ProjectId = "2024/001", IsDefraProject = 0, StartFYear = 0 };
        _service.GetProjectHeaderAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<ProjectHeaderDto>.SuccessResponse(header));

        // Return empty year list so selectedYear stays 0
        _service.GetProjectYearsAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(new List<ProjectYearDto>()));

        var result = await _controller.Index("2024/001");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<YearlyDetailsViewModel>(viewResult.Model);
        Assert.Equal(0, model.SelectedYear);
        Assert.Empty(model.YearRates);
    }

    #endregion

    #region Index – auto-add year when projectYears is empty

    [Fact]
    public async Task Index_AddsProjectYear_WhenProjectYearsEmptyAndStartFYearValid()
    {
        var header = new ProjectHeaderDto { ProjectId = "2024/001", IsDefraProject = 0, StartFYear = 2024 };
        _service.GetProjectHeaderAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<ProjectHeaderDto>.SuccessResponse(header));
        _service.GetProjectYearsAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(new List<ProjectYearDto>()));
        _service.AddProjectYearAsync(Arg.Any<string>(), 2024, Arg.Any<ProjectYearDto>())
            .Returns(ApiResponseDto<ProjectYearDto>.SuccessResponse(new ProjectYearDto { YearValue = 2024 }));

        var result = await _controller.Index("2024/001");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<YearlyDetailsViewModel>(viewResult.Model);
        Assert.Single(model.ProjectYears);
        Assert.Equal(2024, model.ProjectYears[0]);
        Assert.Equal(2024, model.SelectedYear);
        await _service.Received(1).AddProjectYearAsync("2024/001", 2024, Arg.Any<ProjectYearDto>());
    }

    [Fact]
    public async Task Index_SkipsAddProjectYear_WhenStartFYearIsZeroOrNull()
    {
        var header = new ProjectHeaderDto { ProjectId = "2024/001", IsDefraProject = 0, StartFYear = null };
        _service.GetProjectHeaderAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<ProjectHeaderDto>.SuccessResponse(header));
        _service.GetProjectYearsAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(new List<ProjectYearDto>()));

        var result = await _controller.Index("2024/001");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<YearlyDetailsViewModel>(viewResult.Model);
        Assert.Empty(model.ProjectYears);
        Assert.Equal(0, model.SelectedYear);
        await _service.DidNotReceive().AddProjectYearAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<ProjectYearDto>());
    }

    [Fact]
    public async Task Index_DoesNotUpdateYears_WhenAddProjectYearFails()
    {
        var header = new ProjectHeaderDto { ProjectId = "2024/001", IsDefraProject = 0, StartFYear = 2024 };
        _service.GetProjectHeaderAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<ProjectHeaderDto>.SuccessResponse(header));
        _service.GetProjectYearsAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(new List<ProjectYearDto>()));
        _service.AddProjectYearAsync(Arg.Any<string>(), 2024, Arg.Any<ProjectYearDto>())
            .Returns(ApiResponseDto<ProjectYearDto>.FailureResponse(null, new ApiMetaDto()));

        var result = await _controller.Index("2024/001");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<YearlyDetailsViewModel>(viewResult.Model);
        Assert.Empty(model.ProjectYears);
        Assert.Equal(0, model.SelectedYear);
    }

    #endregion

    #region MapApiErrors – validation details (Dictionary<string, string[]>) branch

    [Fact]
    public async Task CreateStaff_Post_ReturnsFieldErrors_WhenApiReturnsValidationDetails()
    {
        _mapper.Map<StaffRequirementDto>(Arg.Any<StaffRequirementFormItem>()).Returns(new StaffRequirementDto());

        var validationDetails = new Dictionary<string, string[]>
        {
            { "WgGrade", value }
        };
        var apiError = new ApiErrorDto { Code = "VALIDATION", Message = "Validation failed" };
        apiError.Details = validationDetails;

        _service.AddStaffRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<StaffRequirementDto>())
            .Returns(ApiResponseDto<StaffRequirementDto>.FailureResponse(
                [apiError], new ApiMetaDto()));

        var result = await _controller.CreateStaff("2024/001", 2024, new StaffRequirementFormItem { WgGrade = "HEO" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
        var errors = element.GetProperty("errors");
        Assert.True(errors.GetArrayLength() > 0);
        Assert.Equal("WgGrade", errors[0].GetProperty("field").GetString());
        Assert.Equal("WgGrade is required.", errors[0].GetProperty("message").GetString());
    }

    [Fact]
    public async Task CreateStaff_Post_ReturnsDefaultError_WhenApiErrorListIsEmpty()
    {
        _mapper.Map<StaffRequirementDto>(Arg.Any<StaffRequirementFormItem>()).Returns(new StaffRequirementDto());
        _service.AddStaffRequirementAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<StaffRequirementDto>())
            .Returns(ApiResponseDto<StaffRequirementDto>.FailureResponse(
                new List<ApiErrorDto>(), new ApiMetaDto()));

        var result = await _controller.CreateStaff("2024/001", 2024, new StaffRequirementFormItem { WgGrade = "HEO" });

        var jsonResult = Assert.IsType<JsonResult>(result);
        var element = GetJsonResultElement(jsonResult);
        Assert.False(element.GetProperty("success").GetBoolean());
        var errors = element.GetProperty("errors");
        Assert.Equal(1, errors.GetArrayLength());
        Assert.Equal("An unexpected error occurred.", errors[0].GetProperty("message").GetString());
    }

    #endregion

    #region PopulateDropdownsAsync – failure / empty data branches

    [Fact]
    public async Task Index_SetsEmptyAccountCatOptions_WhenAccountCategoryServiceFails()
    {
        var header = new ProjectHeaderDto { ProjectId = "2024/001", IsDefraProject = 0 };
        _service.GetProjectHeaderAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<ProjectHeaderDto>.SuccessResponse(header));
        _service.GetProjectYearsAsync(Arg.Any<string>())
            .Returns(ApiResponseDto<List<ProjectYearDto>>.SuccessResponse(new List<ProjectYearDto>()));
        _service.GetAccountCategoriesAsync()
            .Returns(ApiResponseDto<List<AccountCategoryDto>>.FailureResponse(null, new ApiMetaDto()));

        var result = await _controller.Index("2024/001");

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<YearlyDetailsViewModel>(viewResult.Model);
        Assert.Empty(model.AccountCatOptions);
    }

    #endregion

    #region GetTestCodeOptionsAsync – failure path (ViewBag fallback)

    [Fact]
    public async Task CreateTest_Get_SetsEmptyTestCodeOptions_WhenServiceFails()
    {
        _service.GetTestCodeLookupsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>())
            .Returns(ApiResponseDto<List<TestCodeLookupDto>>.FailureResponse(null, new ApiMetaDto()));

        var result = await _controller.CreateTest("2024/001", 2024, false);

        Assert.IsType<PartialViewResult>(result);
        var options = Assert.IsAssignableFrom<IEnumerable<TestCodeLookupDto>>(_controller.ViewBag.TestCodeOptions);
        Assert.Empty(options);
    }

    [Fact]
    public async Task CreateTest_Get_SetsEmptyTestCodeOptions_WhenServiceReturnsNullData()
    {
        _service.GetTestCodeLookupsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>())
            .Returns(ApiResponseDto<List<TestCodeLookupDto>>.SuccessResponse(null!));

        var result = await _controller.CreateTest("2024/001", 2024, false);

        Assert.IsType<PartialViewResult>(result);
        var options = Assert.IsAssignableFrom<IEnumerable<TestCodeLookupDto>>(_controller.ViewBag.TestCodeOptions);
        Assert.Empty(options);
    }

    #endregion

    #region GetAnimalTypeOptionsAsync – failure path (ViewBag fallback)

    [Fact]
    public async Task CreateAnimal_Get_SetsEmptyAnimalTypeOptions_WhenServiceFails()
    {
        _service.GetAnimalRatesAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<bool>())
            .Returns(ApiResponseDto<List<AnimalRateDto>>.FailureResponse(null, new ApiMetaDto()));

        var result = await _controller.CreateAnimal("2024/001", 2024, false);

        Assert.IsType<PartialViewResult>(result);
        var options = Assert.IsAssignableFrom<IEnumerable<AnimalRateDto>>(_controller.ViewBag.AnimalTypeOptions);
        Assert.Empty(options);
    }

    #endregion

    #region GetAccountCatOptionsAsync – failure path (ViewBag fallback)

    [Fact]
    public async Task CreateAdditionalCost_Get_SetsEmptyAccountCatOptions_WhenServiceFails()
    {
        _service.GetAccountCategoriesAsync()
            .Returns(ApiResponseDto<List<AccountCategoryDto>>.FailureResponse(null, new ApiMetaDto()));

        var result = await _controller.CreateAdditionalCost("2024/001", 2024);

        Assert.IsType<PartialViewResult>(result);
        var options = Assert.IsAssignableFrom<IEnumerable<AccountCategoryDto>>(_controller.ViewBag.AccountCatOptions);
        Assert.Empty(options);
    }

    #endregion
}
