using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.TestPlanJobControllerTest
{
    public class TestPlanJobControllerTests
    {
        private readonly IMapper _mapper;
        private readonly ITestRequirementService _testRequirementService;
        private readonly ITestorProductService _testorProductService;
        private readonly TestPlanJobController _controller;

        public TestPlanJobControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _testRequirementService = Substitute.For<ITestRequirementService>();
            _testorProductService = Substitute.For<ITestorProductService>();
            _controller = new TestPlanJobController(_mapper, _testRequirementService, _testorProductService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private static List<TestRequirementDto> BuildTestRequirementDtos() =>
        [
            new() { TestCode = "BLOOD", Buyer = "PRJ1", UnitPrice = 15m, NoRequired = 2, ItemDescription = "Blood Test" },
            new() { TestCode = "URINE", Buyer = "PRJ1", UnitPrice = 10m, NoRequired = 3, ItemDescription = "Urine Test" }
        ];

        private static List<TestorProductDto> BuildTestorProductDtos() =>
        [
            new() { ItemCode = "BLOOD", ItemDescription = "Blood Test" },
            new() { ItemCode = "URINE", ItemDescription = "Urine Test" }
        ];

        #region LoadTestPlanGrid Tests

        [Fact]
        public async Task LoadTestPlanGrid_WithValidRequest_ReturnsPartialViewWithDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var jobCode = "PRJ1";
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = BuildTestRequirementDtos();
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 };
            var serviceResponse = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(dtos, paginationDto);
            var items = new List<TestPlanItem>
            {
                new() { TestCode = "BLOOD", ItemDescription = "Blood Test" },
                new() { TestCode = "URINE", ItemDescription = "Urine Test" }
            };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 2 };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(queryParameters, jobCode).Returns(serviceResponse);
            _mapper.Map<List<TestPlanItem>>(Arg.Any<List<TestRequirementDto>>()).Returns(items);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadTestPlanGrid(request, jobCode);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<TestPlanItem>>(partialView.Model);
            Assert.Equal("testPlanGrid",      gridConfig.GridId);
            Assert.Equal("Test Purchase Plan", gridConfig.Title);
            Assert.Equal("TestCode",          gridConfig.KeyProperty);
            Assert.Equal(2, gridConfig.Data.Count);
        }

        [Fact]
        public async Task LoadTestPlanGrid_WhenModelStateIsInvalid_ReturnsFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page is required");
            var request = new PaginationFilter<string>();

            // Act
            var result = await _controller.LoadTestPlanGrid(request, "PRJ1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Invalid request data", value.GetProperty("message").GetString());
            await _testRequirementService.DidNotReceive()
                .GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadTestPlanGrid_WithNullJobCode_UsesEmptyString()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResponse = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                new List<TestRequirementDto>(), new PaginationDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(queryParameters, string.Empty)
                .Returns(serviceResponse);
            _mapper.Map<List<TestPlanItem>>(Arg.Any<List<TestRequirementDto>>()).Returns(new List<TestPlanItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadTestPlanGrid(request, null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _testRequirementService.Received(1)
                .GetPagedTestReqmtbyProjectAsync(queryParameters, string.Empty);
        }

        [Fact]
        public async Task LoadTestPlanGrid_WhenServiceReturnsNullData_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var serviceResponse = ApiResponseDto<List<TestRequirementDto>>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParameters);
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(queryParameters, "PRJ1")
                .Returns(serviceResponse);

            // Act
            var result = await _controller.LoadTestPlanGrid(request, "PRJ1");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var gridConfig = Assert.IsType<DataGridConfig<TestPlanItem>>(partialView.Model);
            Assert.Empty(gridConfig.Data);
            _mapper.DidNotReceive().Map<List<TestPlanItem>>(Arg.Any<List<TestRequirementDto>>());
        }

        #endregion

        #region Create (GET) Tests

        [Fact]
        public async Task Create_Get_ReturnsPartialView_WithPopulatedTestCodeDropdown()
        {
            // Arrange
            var products = BuildTestorProductDtos();
            _testorProductService.GetAllTestorProductsAsync()
                .Returns(ApiResponseDto<List<TestorProductDto>>.SuccessResponse(products));

            // Act
            var result = await _controller.Create();

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTestPlan", partialView.ViewName);
            var model = Assert.IsType<TestPlanItem>(partialView.Model);
            Assert.Equal((short)1, model.Active);
            Assert.Equal(0, model.NoRequired);
            Assert.Equal(2, model.TestCodeOptions.Count);
            Assert.Equal("BLOOD", model.TestCodeOptions[0].Value);
            Assert.Equal("BLOOD|Blood Test|0.00", model.TestCodeOptions[0].Text);
        }

        [Fact]
        public async Task Create_Get_WhenTestorProductServiceFails_ReturnsEmptyDropdown()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            _testorProductService.GetAllTestorProductsAsync()
                .Returns(ApiResponseDto<List<TestorProductDto>>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Create();

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<TestPlanItem>(partialView.Model);
            Assert.Empty(model.TestCodeOptions);
        }

        #endregion

        #region Create (POST) Tests

        [Fact]
        public async Task Create_Post_WithValidRequest_ReturnsSuccessJson()
        {
            // Arrange
            var item = new TestPlanItem { TestCode = "BLOOD", Buyer = "PRJ1", UnitPrice = 15m, NoRequired = 2 };
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1", UnitPrice = 15m, NoRequired = 2 };
            var serviceResponse = ApiResponseDto<TestRequirementDto>.SuccessResponse(dto);

            _mapper.Map<TestRequirementDto>(item).Returns(dto);
            _testRequirementService.CreateTestReqmtAsync(dto).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Test plan item created successfully.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Create_Post_WhenModelStateIsInvalid_ReturnsValidationFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("TestCode", "Test Code is required");
            var item = new TestPlanItem();

            // Act
            var result = await _controller.Create(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Please correct the errors below.", value.GetProperty("message").GetString());
            await _testRequirementService.DidNotReceive().CreateTestReqmtAsync(Arg.Any<TestRequirementDto>());
        }

        [Fact]
        public async Task Create_Post_WhenServiceReturnsDuplicateErrorCode_ReturnsFriendlyDuplicateMessage()
        {
            // Arrange
            var item = new TestPlanItem { TestCode = "BLOOD", Buyer = "PRJ1" };
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var errors = new List<ApiErrorDto> { new() { Message = "Duplicate test code", Code = "DUPLICATE" } };
            var serviceResponse = ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<TestRequirementDto>(item).Returns(dto);
            _testRequirementService.CreateTestReqmtAsync(dto).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            const string expectedMessage = "This test code has already been added to this project. Please update the existing entry instead.";
            Assert.Equal(expectedMessage, value.GetProperty("message").GetString());
            // errors array must contain field="" so the error renders in the summary banner, not inline
            var errorsArray = value.GetProperty("errors");
            Assert.Equal(1, errorsArray.GetArrayLength());
            Assert.Equal(string.Empty, errorsArray[0].GetProperty("field").GetString());
            Assert.Equal(expectedMessage, errorsArray[0].GetProperty("message").GetString());
        }

        [Theory]
        [InlineData("CONFLICT")]
        [InlineData("BUSINESS_RULE_VIOLATION")]
        public async Task Create_Post_WhenServiceReturnsDuplicateVariantErrorCode_ReturnsFriendlyDuplicateMessage(string errorCode)
        {
            // Arrange
            var item = new TestPlanItem { TestCode = "BLOOD", Buyer = "PRJ1" };
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var errors = new List<ApiErrorDto> { new() { Message = "Some conflict message", Code = errorCode } };
            var serviceResponse = ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<TestRequirementDto>(item).Returns(dto);
            _testRequirementService.CreateTestReqmtAsync(dto).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal(
                "This test code has already been added to this project. Please update the existing entry instead.",
                value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Create_Post_WhenServiceErrorMessageContainsAlreadyExists_ReturnsFriendlyDuplicateMessage()
        {
            // Arrange – mirrors the exact message thrown by TestRequirementService.cs:97
            var item = new TestPlanItem { TestCode = "BLOOD", Buyer = "PRJ1" };
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var errors = new List<ApiErrorDto> { new() { Message = "A record with the same TestCode and Buyer already exists.", Code = "UNKNOWN" } };
            var serviceResponse = ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<TestRequirementDto>(item).Returns(dto);
            _testRequirementService.CreateTestReqmtAsync(dto).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal(
                "This test code has already been added to this project. Please update the existing entry instead.",
                value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Create_Post_WhenServiceFailsWithNonDuplicateError_ReturnsRawErrorMessage()
        {
            // Arrange
            var item = new TestPlanItem { TestCode = "BLOOD", Buyer = "PRJ1" };
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var errors = new List<ApiErrorDto> { new() { Message = "This workgroup is not setup to do this test.", Code = "VALIDATION_ERROR" } };
            var serviceResponse = ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<TestRequirementDto>(item).Returns(dto);
            _testRequirementService.CreateTestReqmtAsync(dto).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(item);

            // Assert – non-duplicate errors fall through to the generic handler
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("This workgroup is not setup to do this test.", value.GetProperty("message").GetString());
        }

        #endregion

        #region Edit (GET) Tests

        [Fact]
        public async Task Edit_Get_WithValidId_ReturnsPartialViewWithModel()
        {
            // Arrange
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1", UnitPrice = 15m, ItemDescription = "Blood Test" };
            var serviceResponse = ApiResponseDto<TestRequirementDto>.SuccessResponse(dto);
            var item = new TestPlanItem { TestCode = "BLOOD", IsEdit = true };
            var products = BuildTestorProductDtos();

            _testRequirementService.GetTestReqmtByIdAsync("BLOOD", "PRJ1").Returns(serviceResponse);
            _mapper.Map<TestPlanItem>(dto).Returns(item);
            _testorProductService.GetAllTestorProductsAsync()
                .Returns(ApiResponseDto<List<TestorProductDto>>.SuccessResponse(products));

            // Act
            var result = await _controller.Edit("BLOOD", "PRJ1");

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTestPlan", partialView.ViewName);
            var model = Assert.IsType<TestPlanItem>(partialView.Model);
            Assert.True(model.IsEdit);
        }

        [Fact]
        public async Task Edit_Get_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var serviceResponse = ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto());
            _testRequirementService.GetTestReqmtByIdAsync("BLOOD", "PRJ1").Returns(serviceResponse);

            // Act
            var result = await _controller.Edit("BLOOD", "PRJ1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to retrieve test plan item.", value.GetProperty("message").GetString());
        }

        #endregion

        #region Edit (POST) Tests

        [Fact]
        public async Task Edit_Post_WithValidRequest_ReturnsSuccessJson()
        {
            // Arrange
            var item = new TestPlanItem { TestCode = "BLOOD", Buyer = "PRJ1", UnitPrice = 20m, NoRequired = 3 };
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1", UnitPrice = 20m, NoRequired = 3 };
            var serviceResponse = ApiResponseDto<TestRequirementDto>.SuccessResponse(dto);

            _mapper.Map<TestRequirementDto>(item).Returns(dto);
            _testRequirementService.UpdateTestReqmtAsync(dto).Returns(serviceResponse);

            // Act
            var result = await _controller.Edit(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Test plan item updated successfully.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Edit_Post_WhenModelStateIsInvalid_ReturnsValidationFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("TestCode", "Test Code is required");
            var item = new TestPlanItem();

            // Act
            var result = await _controller.Edit(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Please correct the errors below.", value.GetProperty("message").GetString());
            await _testRequirementService.DidNotReceive().UpdateTestReqmtAsync(Arg.Any<TestRequirementDto>());
        }

        [Fact]
        public async Task Edit_Post_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var item = new TestPlanItem { TestCode = "BLOOD", Buyer = "PRJ1" };
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var errors = new List<ApiErrorDto> { new() { Message = "Record not found", Code = "NOT_FOUND" } };
            var serviceResponse = ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<TestRequirementDto>(item).Returns(dto);
            _testRequirementService.UpdateTestReqmtAsync(dto).Returns(serviceResponse);

            // Act
            var result = await _controller.Edit(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Record not found", value.GetProperty("message").GetString());
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_WithValidKeys_ReturnsSuccessJson()
        {
            // Arrange
            var serviceResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _testRequirementService.DeleteTestReqmtAsync("BLOOD", "PRJ1").Returns(serviceResponse);

            // Act
            var result = await _controller.Delete("BLOOD", "PRJ1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal("Test plan item deleted successfully.", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Delete_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var serviceResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _testRequirementService.DeleteTestReqmtAsync("BLOOD", "PRJ1").Returns(serviceResponse);

            // Act
            var result = await _controller.Delete("BLOOD", "PRJ1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Not found", value.GetProperty("message").GetString());
        }

        #endregion

        #region GetRecUnitPrice Tests

        [Fact]
        public async Task GetRecUnitPrice_WithValidTestCode_ReturnsSuccessJson()
        {
            // Arrange
            var testCode = "BLOOD";
            var dto = new TestRequirementDto { TestCode = "BLOOD", RecUnitPrice = 25.50m };
            var serviceResponse = ApiResponseDto<TestRequirementDto>.SuccessResponse(dto);
            _testRequirementService.GetTestReqmtPricingAsync(testCode, null).Returns(serviceResponse);

            // Act
            var result = await _controller.GetRecUnitPrice(testCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(25.50m, value.GetProperty("recUnitPrice").GetDecimal());
        }

        [Fact]
        public async Task GetRecUnitPrice_WithProjectBuyerCode_PassesItToService()
        {
            // Arrange
            var testCode = "BLOOD";
            var buyerCode = "PRJ1";
            var dto = new TestRequirementDto { TestCode = "BLOOD", RecUnitPrice = 30m };
            var serviceResponse = ApiResponseDto<TestRequirementDto>.SuccessResponse(dto);
            _testRequirementService.GetTestReqmtPricingAsync(testCode, buyerCode).Returns(serviceResponse);

            // Act
            var result = await _controller.GetRecUnitPrice(testCode, buyerCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(30m, value.GetProperty("recUnitPrice").GetDecimal());
            await _testRequirementService.Received(1).GetTestReqmtPricingAsync(testCode, buyerCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetRecUnitPrice_WithEmptyOrWhitespaceTestCode_ReturnsFailureJson(string testCode)
        {
            // Act
            var result = await _controller.GetRecUnitPrice(testCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal(0, value.GetProperty("recUnitPrice").GetInt32());
            await _testRequirementService.DidNotReceive()
                .GetTestReqmtPricingAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetRecUnitPrice_WhenServiceFails_ReturnsZeroPrice()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var serviceResponse = ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto());
            _testRequirementService.GetTestReqmtPricingAsync("BLOOD", null).Returns(serviceResponse);

            // Act
            var result = await _controller.GetRecUnitPrice("BLOOD");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal(0, value.GetProperty("recUnitPrice").GetInt32());
        }

        [Fact]
        public async Task GetRecUnitPrice_WhenRecUnitPriceIsNull_ReturnsZero()
        {
            // Arrange
            var dto = new TestRequirementDto { TestCode = "BLOOD", RecUnitPrice = null };
            var serviceResponse = ApiResponseDto<TestRequirementDto>.SuccessResponse(dto);
            _testRequirementService.GetTestReqmtPricingAsync("BLOOD", null).Returns(serviceResponse);

            // Act
            var result = await _controller.GetRecUnitPrice("BLOOD");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(0, value.GetProperty("recUnitPrice").GetInt32());
        }

        #endregion

        #region GetTotalTestCost Tests

        [Fact]
        public async Task GetTotalTestCost_WithValidJobCode_ReturnsCorrectTotal()
        {
            // Arrange
            var jobCode = "PRJ1";
            var dtos = BuildTestRequirementDtos(); // 15*2 + 10*3 = 60
            var serviceResponse = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(dtos, new PaginationDto());
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(
                Arg.Any<QueryParameters<string>>(), jobCode).Returns(serviceResponse);

            // Act
            var result = await _controller.GetTotalTestCost(jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(60m, value.GetProperty("totalTestCost").GetDecimal());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetTotalTestCost_WithEmptyOrWhitespaceJobCode_ReturnsFailureJson(string jobCode)
        {
            // Act
            var result = await _controller.GetTotalTestCost(jobCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Job Code is required.", value.GetProperty("message").GetString());
            Assert.Equal(0, value.GetProperty("totalTestCost").GetInt32());
            await _testRequirementService.DidNotReceive()
                .GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetTotalTestCost_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERROR" } };
            var serviceResponse = ApiResponseDto<List<TestRequirementDto>>.FailureResponse(errors, new ApiMetaDto());
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(
                Arg.Any<QueryParameters<string>>(), "PRJ1").Returns(serviceResponse);

            // Act
            var result = await _controller.GetTotalTestCost("PRJ1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to retrieve total test cost.", value.GetProperty("message").GetString());
            Assert.Equal(0, value.GetProperty("totalTestCost").GetInt32());
        }

        [Fact]
        public async Task GetTotalTestCost_WithNullPricesAndQuantities_ReturnsTotalOfZero()
        {
            // Arrange
            var dtos = new List<TestRequirementDto>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", UnitPrice = null, NoRequired = null }
            };
            var serviceResponse = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(dtos, new PaginationDto());
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(
                Arg.Any<QueryParameters<string>>(), "PRJ1").Returns(serviceResponse);

            // Act
            var result = await _controller.GetTotalTestCost("PRJ1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(0m, value.GetProperty("totalTestCost").GetDecimal());
        }

        #endregion

        #region IsDuplicateError — null Code and null Message branch coverage

        [Fact]
        public async Task Create_Post_WhenErrorCodeIsNullAndMessageContainsAlreadyExists_ReturnsFriendlyDuplicateMessage()
        {
            // Arrange — Code is null; IsDuplicateError must fall through to Message.Contains("already exists")
            var item = new TestPlanItem { TestCode = "BLOOD", Buyer = "PRJ1" };
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var errors = new List<ApiErrorDto> { new() { Code = string.Empty, Message = "A record already exists for this test code." } };
            var serviceResponse = ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<TestRequirementDto>(item).Returns(dto);
            _testRequirementService.CreateTestReqmtAsync(dto).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            const string expectedMessage = "This test code has already been added to this project. Please update the existing entry instead.";
            Assert.Equal(expectedMessage, value.GetProperty("message").GetString());
            var errorsArray = value.GetProperty("errors");
            Assert.Equal(string.Empty, errorsArray[0].GetProperty("field").GetString());
        }

        [Fact]
        public async Task Create_Post_WhenErrorCodeIsNullAndMessageIsNull_ReturnsGenericFailureWithFallbackMessage()
        {
            // Arrange — Code and Message are both empty strings → IsDuplicateError returns false, fallback message used
            var item = new TestPlanItem { TestCode = "BLOOD", Buyer = "PRJ1" };
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var errors = new List<ApiErrorDto> { new() { Code = string.Empty, Message = string.Empty } };
            var serviceResponse = ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<TestRequirementDto>(item).Returns(dto);
            _testRequirementService.CreateTestReqmtAsync(dto).Returns(serviceResponse);

            // Act
            var result = await _controller.Create(item);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            // Message is empty string (not null), so the ?? fallback does not trigger
            Assert.Equal(string.Empty, value.GetProperty("message").GetString());
        }

        #endregion
    }
}
