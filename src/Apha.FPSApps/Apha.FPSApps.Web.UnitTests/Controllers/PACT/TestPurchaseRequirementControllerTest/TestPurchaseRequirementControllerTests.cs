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
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.TestPurchaseRequirementControllerTest
{
    public class TestPurchaseRequirementControllerTests
    {
        private readonly IMapper _mapper;
        private readonly ITestRequirementService _testReqmtService;
        private readonly ITestorProductService _testorProductService;
        private readonly IProjectService _projectService;
        private readonly TestPurchaseRequirementController _controller;

        public TestPurchaseRequirementControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _testReqmtService = Substitute.For<ITestRequirementService>();
            _testorProductService = Substitute.For<ITestorProductService>();
            _projectService = Substitute.For<IProjectService>();
            _controller = new TestPurchaseRequirementController(
                _mapper,
                _testReqmtService,
                _testorProductService,
                _projectService);

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Substitute.For<ITempDataProvider>());
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private void SetupGridMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<TestPurchaseRequirementItem>>(Arg.Any<List<TestRequirementDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        private void SetupDropdowns()
        {
            _testorProductService.GetAllTestorProductsAsync()
                .Returns(ApiResponseDto<List<TestorProductDto>>.SuccessResponse([]));
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([]));
        }

        // ── INDEX ─────────────────────────────────────────────────────────────

        #region Index

        [Fact]
        public async Task Index_WithParentProject_ReturnsViewWithViewModel()
        {
            // Arrange
            const string parentProject = "PRJ001";
            _testReqmtService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), parentProject)
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse([], new PaginationDto()));
            SetupGridMapper();

            // Act
            var result = await _controller.Index(parentProject);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestPurchaseRequirementViewModel>(viewResult.Model);
            Assert.Equal(parentProject, model.ParentProject);
            Assert.Equal("testPurchaseReqGrid", model.TestPurchaseReqGrid.GridId);
        }

        [Fact]
        public async Task Index_WithNullParentProject_UsesEmptyStringAndReturnsView()
        {
            // Arrange
            _testReqmtService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), string.Empty)
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse([], new PaginationDto()));
            SetupGridMapper();

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestPurchaseRequirementViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.ParentProject);
        }

        [Fact]
        public async Task Index_ServiceReturnsItems_PopulatesGrid()
        {
            // Arrange
            var items = new List<TestRequirementDto>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ001" }
            };
            _testReqmtService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), "PRJ001")
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(items, new PaginationDto()));
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<TestPurchaseRequirementItem>>(items)
                .Returns([new TestPurchaseRequirementItem { TestCode = "BLOOD", Buyer = "PRJ001" }]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.Index("PRJ001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<TestPurchaseRequirementViewModel>(viewResult.Model);
        }

        [Fact]
        public async Task Index_ServiceFails_ReturnsViewWithEmptyGrid()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "ERR" } };
            _testReqmtService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(ApiResponseDto<List<TestRequirementDto>>.FailureResponse(errors, new ApiMetaDto()));
            SetupGridMapper();

            // Act
            var result = await _controller.Index("PRJ001");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestPurchaseRequirementViewModel>(viewResult.Model);
            Assert.Empty(model.TestPurchaseReqGrid.Data);
        }

        #endregion

        // ── GRID ──────────────────────────────────────────────────────────────

        #region LoadTestPurchaseReqGrid

        [Fact]
        public async Task LoadTestPurchaseReqGrid_ValidRequest_ReturnsDataGridPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _testReqmtService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), "PRJ001")
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse([], new PaginationDto()));
            SetupGridMapper();

            // Act
            var result = await _controller.LoadTestPurchaseReqGrid(request, "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<TestPurchaseRequirementItem>>(partial.Model);
        }

        [Fact]
        public async Task LoadTestPurchaseReqGrid_InvalidModelState_ReturnsJsonError()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Test error");

            // Act
            var result = await _controller.LoadTestPurchaseReqGrid(new PaginationFilter<string>(), "PRJ001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Invalid request data", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task LoadTestPurchaseReqGrid_ServiceFails_ReturnsEmptyGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var errors = new List<ApiErrorDto> { new() { Code = "ERR" } };
            _testReqmtService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), "PRJ001")
                .Returns(ApiResponseDto<List<TestRequirementDto>>.FailureResponse(errors, new ApiMetaDto()));
            SetupGridMapper();

            // Act
            var result = await _controller.LoadTestPurchaseReqGrid(request, "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadTestPurchaseReqGrid_WithNullFilter_UsesEmptyDictionary()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = null };
            _testReqmtService.GetPagedTestReqmtbyProjectAsync(Arg.Any<QueryParameters<string>>(), "PRJ001")
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse([], new PaginationDto()));
            SetupGridMapper();

            // Act
            var result = await _controller.LoadTestPurchaseReqGrid(request, "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        #endregion

        // ── CRUD ──────────────────────────────────────────────────────────────

        #region GetTestPurchaseReq

        [Fact]
        public async Task GetTestPurchaseReq_NullTestCode_ReturnsAddFormWithDefaults()
        {
            // Arrange
            SetupDropdowns();

            // Act
            var result = await _controller.GetTestPurchaseReq(null, "BUY001", "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTestPurchaseRequirement", partial.ViewName);
            var model = Assert.IsType<TestPurchaseRequirementItem>(partial.Model);
            Assert.Equal(string.Empty, model.TestCode);
            Assert.False(model.IsEdit);
            Assert.Equal((short?)1, model.Active);
            Assert.Null(model.NoRequired);
        }

        [Fact]
        public async Task GetTestPurchaseReq_NullTestCode_SetsBuyerFromBuyerParam()
        {
            // Arrange
            SetupDropdowns();

            // Act
            var result = await _controller.GetTestPurchaseReq(null, "BUY001", "PRJ001");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<TestPurchaseRequirementItem>(partial.Model);
            Assert.Equal("BUY001", model.Buyer);
            Assert.Equal("PRJ001", model.ProjectBuyerCode);
        }

        [Fact]
        public async Task GetTestPurchaseReq_EmptyTestCode_ReturnsAddForm()
        {
            // Arrange
            SetupDropdowns();

            // Act
            var result = await _controller.GetTestPurchaseReq(string.Empty, null, null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTestPurchaseRequirement", partial.ViewName);
        }

        [Fact]
        public async Task GetTestPurchaseReq_ValidTestCode_ReturnsEditFormWithData()
        {
            // Arrange
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ001" };
            var item = new TestPurchaseRequirementItem { TestCode = "BLOOD", Buyer = "PRJ001" };
            SetupDropdowns();
            _testReqmtService.GetTestReqmtByIdAsync("BLOOD", "PRJ001")
                .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(dto));
            _mapper.Map<TestPurchaseRequirementItem>(dto).Returns(item);

            // Act
            var result = await _controller.GetTestPurchaseReq("BLOOD", "PRJ001", null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTestPurchaseRequirement", partial.ViewName);
            var model = Assert.IsType<TestPurchaseRequirementItem>(partial.Model);
            Assert.True(model.IsEdit);
        }

        [Fact]
        public async Task GetTestPurchaseReq_TestCodeNotFound_ReturnsNotFound()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND" } };
            SetupDropdowns();
            _testReqmtService.GetTestReqmtByIdAsync("MISSING", "PRJ001")
                .Returns(ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetTestPurchaseReq("MISSING", "PRJ001", null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetTestPurchaseReq_ValidTestCode_ServiceDataNull_ReturnsNotFound()
        {
            // Arrange
            SetupDropdowns();
            _testReqmtService.GetTestReqmtByIdAsync("BLOOD", "PRJ001")
                .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(null!));

            // Act
            var result = await _controller.GetTestPurchaseReq("BLOOD", "PRJ001", null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetTestPurchaseReq_DropdownsLoaded_LoadsTestorProductsAndBuyers()
        {
            // Arrange
            _testorProductService.GetAllTestorProductsAsync()
                .Returns(ApiResponseDto<List<TestorProductDto>>.SuccessResponse(
                [
                    new TestorProductDto { ItemCode = "BLOOD" },
                    new TestorProductDto { ItemCode = "URINE" }
                ]));
            _projectService.GetAllPactProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                [
                    new ProjectDto { ParentProject = "PRJ001" }
                ]));

            // Act
            var result = await _controller.GetTestPurchaseReq(null, null, null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _testorProductService.Received(1).GetAllTestorProductsAsync();
            await _projectService.Received(1).GetAllPactProjectsAsync();
        }

        #endregion

        #region SaveTestPurchaseReq

        [Fact]
        public async Task SaveTestPurchaseReq_InvalidModelState_ReturnsJsonError()
        {
            // Arrange
            _controller.ModelState.AddModelError("TestCode", "Test Code is required.");
            var model = new TestPurchaseRequirementItem { Buyer = "PRJ001" };

            // Act
            var result = await _controller.SaveTestPurchaseReq(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Please correct the errors below.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveTestPurchaseReq_ValidCreateRequest_CallsCreateAndReturnsSuccess()
        {
            // Arrange
            var model = new TestPurchaseRequirementItem
            {
                TestCode = "BLOOD",
                Buyer = "PRJ001",
                IsEdit = false
            };
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ001" };
            _mapper.Map<TestRequirementDto>(model).Returns(dto);
            _testReqmtService.CreateTestReqmtAsync(dto)
                .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.SaveTestPurchaseReq(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Contains("saved", element.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
            await _testReqmtService.Received(1).CreateTestReqmtAsync(dto);
            await _testReqmtService.DidNotReceive().UpdateTestReqmtAsync(Arg.Any<TestRequirementDto>());
        }

        [Fact]
        public async Task SaveTestPurchaseReq_ValidUpdateRequest_CallsUpdateAndReturnsSuccess()
        {
            // Arrange
            var model = new TestPurchaseRequirementItem
            {
                TestCode = "BLOOD",
                Buyer = "PRJ001",
                IsEdit = true
            };
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ001" };
            _mapper.Map<TestRequirementDto>(model).Returns(dto);
            _testReqmtService.UpdateTestReqmtAsync(dto)
                .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.SaveTestPurchaseReq(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Contains("updated", element.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
            await _testReqmtService.Received(1).UpdateTestReqmtAsync(dto);
            await _testReqmtService.DidNotReceive().CreateTestReqmtAsync(Arg.Any<TestRequirementDto>());
        }

        [Fact]
        public async Task SaveTestPurchaseReq_CreateFails_ReturnsJsonFailureWithServiceMessage()
        {
            // Arrange
            var model = new TestPurchaseRequirementItem
            {
                TestCode = "BLOOD",
                Buyer = "PRJ001",
                IsEdit = false
            };
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ001" };
            var errors = new List<ApiErrorDto> { new() { Code = "CONFLICT", Message = "Duplicate record." } };
            _mapper.Map<TestRequirementDto>(model).Returns(dto);
            _testReqmtService.CreateTestReqmtAsync(dto)
                .Returns(ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.SaveTestPurchaseReq(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Duplicate record.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveTestPurchaseReq_UpdateFails_ReturnsJsonFailureWithServiceMessage()
        {
            // Arrange
            var model = new TestPurchaseRequirementItem
            {
                TestCode = "BLOOD",
                Buyer = "PRJ001",
                IsEdit = true
            };
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ001" };
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Record not found." } };
            _mapper.Map<TestRequirementDto>(model).Returns(dto);
            _testReqmtService.UpdateTestReqmtAsync(dto)
                .Returns(ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.SaveTestPurchaseReq(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Record not found.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task SaveTestPurchaseReq_CreateFailsWithNoErrors_ReturnsDefaultFailureMessage()
        {
            // Arrange
            var model = new TestPurchaseRequirementItem
            {
                TestCode = "BLOOD",
                Buyer = "PRJ001",
                IsEdit = false
            };
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ001" };
            _mapper.Map<TestRequirementDto>(model).Returns(dto);
            _testReqmtService.CreateTestReqmtAsync(dto)
                .Returns(ApiResponseDto<TestRequirementDto>.FailureResponse([], new ApiMetaDto()));

            // Act
            var result = await _controller.SaveTestPurchaseReq(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Contains("Failed", element.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region DeleteTestPurchaseReq

        [Fact]
        public async Task DeleteTestPurchaseReq_ValidRequest_ReturnsJsonSuccess()
        {
            // Arrange
            _testReqmtService.DeleteTestReqmtAsync("BLOOD", "PRJ001")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteTestPurchaseReq("BLOOD", "PRJ001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Contains("deleted", element.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task DeleteTestPurchaseReq_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Record not found." } };
            _testReqmtService.DeleteTestReqmtAsync("MISSING", "PRJ001")
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteTestPurchaseReq("MISSING", "PRJ001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Equal("Record not found.", element.GetProperty("message").GetString());
        }

        [Fact]
        public async Task DeleteTestPurchaseReq_ServiceFailsWithNoErrors_ReturnsDefaultMessage()
        {
            // Arrange
            _testReqmtService.DeleteTestReqmtAsync("BLOOD", "PRJ001")
                .Returns(ApiResponseDto<bool>.FailureResponse([], new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteTestPurchaseReq("BLOOD", "PRJ001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
            Assert.Contains("Failed", element.GetProperty("message").GetString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task DeleteTestPurchaseReq_CallsServiceWithCorrectKeys()
        {
            // Arrange
            _testReqmtService.DeleteTestReqmtAsync("BLOOD", "PRJ001")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            await _controller.DeleteTestPurchaseReq("BLOOD", "PRJ001");

            // Assert
            await _testReqmtService.Received(1).DeleteTestReqmtAsync("BLOOD", "PRJ001");
        }

        #endregion

        #region GetTestReqmtPricing

        [Fact]
        public async Task GetTestReqmtPricing_EmptyTestCode_ReturnsJsonFailure()
        {
            // Act
            var result = await _controller.GetTestReqmtPricing(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetTestReqmtPricing_WhitespaceTestCode_ReturnsJsonFailure()
        {
            // Act
            var result = await _controller.GetTestReqmtPricing("   ");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetTestReqmtPricing_ValidTestCode_ReturnsJsonWithRecUnitPrice()
        {
            // Arrange
            var dto = new TestRequirementDto { TestCode = "BLOOD", RecUnitPrice = 10.5m };
            _testReqmtService.GetTestReqmtPricingAsync("BLOOD", null)
                .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.GetTestReqmtPricing("BLOOD");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal(10.5m, element.GetProperty("recUnitPrice").GetDecimal());
        }

        [Fact]
        public async Task GetTestReqmtPricing_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND" } };
            _testReqmtService.GetTestReqmtPricingAsync("MISSING", null)
                .Returns(ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetTestReqmtPricing("MISSING");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetTestReqmtPricing_WithProjectCode_PassesProjectCodeToService()
        {
            // Arrange
            var dto = new TestRequirementDto { TestCode = "BLOOD", RecUnitPrice = 5.0m, IsDefraProject = 1 };
            _testReqmtService.GetTestReqmtPricingAsync("BLOOD", "PRJ001")
                .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.GetTestReqmtPricing("BLOOD", "PRJ001");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal((short)1, element.GetProperty("isDefraProject").GetInt16());
            await _testReqmtService.Received(1).GetTestReqmtPricingAsync("BLOOD", "PRJ001");
        }

        [Fact]
        public async Task GetTestReqmtPricing_WithNullProjectCode_IsDefraProjectIsNull()
        {
            // Arrange
            var dto = new TestRequirementDto { TestCode = "BLOOD", RecUnitPrice = 10.5m };
            _testReqmtService.GetTestReqmtPricingAsync("BLOOD", null)
                .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.GetTestReqmtPricing("BLOOD", null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
            Assert.Equal(JsonValueKind.Null, element.GetProperty("isDefraProject").ValueKind);
        }

        #endregion
    }
}
