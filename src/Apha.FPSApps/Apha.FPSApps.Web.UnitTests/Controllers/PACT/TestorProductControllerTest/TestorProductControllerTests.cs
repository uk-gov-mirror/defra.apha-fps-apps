using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Handler;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.TestListControllerTest
{
    public class TestorProductControllerTests
    {
        private readonly IMapper _mapper;   
        private readonly ITestorProductService _testListService;
        private readonly IFpsYearContext _fpsYearContext;
        private readonly TestorProductController _controller;
        private const int CurrentYear = 2025;

        public TestorProductControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _testListService = Substitute.For<ITestorProductService>();
            _fpsYearContext = Substitute.For<IFpsYearContext>();
            _fpsYearContext.Year.Returns(CurrentYear);
            _controller = new TestorProductController(_mapper, _testListService, _fpsYearContext);
        }

        #region Helper Methods

        private void SetupTestOrProductPagedGridMapper()
        {
            var testDtos = new List<TestorProductDto>
            {
                new() { ItemCode = "T001", ItemDescription = "Test One" }
            };
            var viewModels = new List<TestOrProductViewModel>
            {
                new() { ItemCode = "T001", ItemDescription = "Test One" }
            };
            _mapper.Map<List<TestOrProductViewModel>>(Arg.Is<List<TestorProductDto>>(list => list.Count == 1 && list[0].ItemCode == "T001"))
                .Returns(viewModels);
        }

        private void SetupQueryParametersMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(callInfo =>
                {
                    var filter = callInfo.Arg<PaginationFilter<string>>();
                    return new QueryParameters<string>
                    {
                        Page = filter.Page,
                        PageSize = filter.PageSize,
                        Search = filter.Search,
                        Filter = filter.Filter
                    };
                });
        }

        #endregion

        #region Index

        [Fact]
        public async Task Index_Always_ReturnsViewResultWithTestGrid()
        {
            // Arrange
            var testDtos = new List<TestorProductDto>
            {
                new() { ItemCode = "T001", ItemDescription = "Test One" }
            };
            var expectedResponse = ApiResponseDto<List<TestorProductDto>>.SuccessResponse(testDtos);
            _testListService.GetPagedTestOrProductsAsync(Arg.Any<QueryParameters<string>>()).Returns(Task.FromResult(expectedResponse));
            SetupTestOrProductPagedGridMapper();
            SetupQueryParametersMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestListViewModel>(viewResult.Model);
            Assert.NotNull(model.TestGrid);
            Assert.Equal("testGrid", model.TestGrid.GridId);
        }

        #endregion

        #region LoadTestGrid

        [Fact]
        public async Task LoadTestGrid_WithValidRequest_ReturnsPartialViewWithData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var testDtos = new List<TestorProductDto>
            {
                new() { ItemCode = "T001", ItemDescription = "Test One" },
                new() { ItemCode = "T002", ItemDescription = "Test Two" }
            };
            var expectedResponse = ApiResponseDto<List<TestorProductDto>>.SuccessResponse(testDtos);
            _testListService.GetPagedTestOrProductsAsync(Arg.Any<QueryParameters<string>>()).Returns(Task.FromResult(expectedResponse));

            var viewModels = new List<TestOrProductViewModel>
            {
                new() { ItemCode = "T001", ItemDescription = "Test One" },
                new() { ItemCode = "T002", ItemDescription = "Test Two" }
            };
            _mapper.Map<List<TestOrProductViewModel>>(Arg.Is<List<TestorProductDto>>(list => list.Count == 2 && list[0].ItemCode == "T001"))
                .Returns(viewModels);
            SetupQueryParametersMapper();

            // Act
            var result = await _controller.LoadTestGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialViewResult.ViewName);
            var model = Assert.IsType<DataGridConfig<TestOrProductViewModel>>(partialViewResult.Model);
            Assert.Equal(2, model.Data.Count());
        }

        [Fact]
        public async Task LoadTestGrid_WithEmptyResult_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var expectedResponse = ApiResponseDto<List<TestorProductDto>>.SuccessResponse(new List<TestorProductDto>());
            _testListService.GetPagedTestOrProductsAsync(Arg.Any<QueryParameters<string>>()).Returns(Task.FromResult(expectedResponse));

            _mapper.Map<List<TestOrProductViewModel>>(Arg.Any<List<TestorProductDto>>())
                .Returns(new List<TestOrProductViewModel>());
            SetupQueryParametersMapper();

            // Act
            var result = await _controller.LoadTestGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<TestOrProductViewModel>>(partialViewResult.Model);
            Assert.Empty(model.Data);
        }

        [Fact]
        public async Task LoadTestGrid_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            _controller.ModelState.AddModelError("Page", "Invalid page number");

            // Act
            var result = await _controller.LoadTestGrid(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value);
        }

        [Fact]
        public async Task LoadTestGrid_WhenServiceFails_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var errors = new List<ApiErrorDto> { new() { Message = "Service error" } };
            var expectedResponse = ApiResponseDto<List<TestorProductDto>>.FailureResponse(errors, new ApiMetaDto());
            _testListService.GetPagedTestOrProductsAsync(Arg.Any<QueryParameters<string>>()).Returns(Task.FromResult(expectedResponse));

            _mapper.Map<List<TestOrProductViewModel>>(Arg.Any<List<TestorProductDto>>())
                .Returns(new List<TestOrProductViewModel>());
            SetupQueryParametersMapper();

            // Act
            var result = await _controller.LoadTestGrid(request);

            // Assert
            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<DataGridConfig<TestOrProductViewModel>>(partialViewResult.Model);
            Assert.Empty(model.Data);
        }

        #endregion

        #region GetOwners

        [Fact]
        public async Task GetOwners_WithValidRequest_ReturnsJsonWithOwners()
        {
            // Arrange
            var owners = new List<string> { "AB", "CD", "EF" };
            var expectedResponse = ApiResponseDto<List<string>>.SuccessResponse(owners);
            _testListService.GetOwnersAsync().Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.GetOwners();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = jsonResult.Value;
            var successProperty = resultValue?.GetType().GetProperty("success")?.GetValue(resultValue);
            var dataProperty = resultValue?.GetType().GetProperty("data")?.GetValue(resultValue);
            Assert.True((bool)successProperty!);
            Assert.Equal(owners, dataProperty);
        }

        [Fact]
        public async Task GetOwners_WhenServiceFails_ReturnsJsonWithFailure()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Service error" } };
            var expectedResponse = ApiResponseDto<List<string>>.FailureResponse(errors, new ApiMetaDto());
            _testListService.GetOwnersAsync().Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.GetOwners();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = jsonResult.Value;
            var successProperty = resultValue?.GetType().GetProperty("success")?.GetValue(resultValue);
            Assert.False((bool)successProperty!);
        }

        #endregion

        #region GetTestOrProduct

        [Fact]
        public async Task GetTestOrProduct_WithValidItemCode_ReturnsJsonWithData()
        {
            // Arrange
            var itemCode = "T001";
            var testDto = new TestorProductDto { ItemCode = itemCode, ItemDescription = "Test Product" };
            var expectedResponse = ApiResponseDto<TestorProductDto>.SuccessResponse(testDto);
            _testListService.GetTestOrProductByIdAsync(itemCode).Returns(Task.FromResult(expectedResponse));

            var viewModel = new TestOrProductViewModel { ItemCode = itemCode, ItemDescription = "Test Product" };
            _mapper.Map<TestOrProductViewModel>(Arg.Is<TestorProductDto>(d => d.ItemCode == itemCode)).Returns(viewModel);

            // Act
            var result = await _controller.GetTestOrProduct(itemCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = jsonResult.Value;
            var successProperty = resultValue?.GetType().GetProperty("success")?.GetValue(resultValue);
            Assert.True((bool)successProperty!);
        }

        [Fact]
        public async Task GetTestOrProduct_WithNullOrEmptyItemCode_ReturnsJsonWithFailure()
        {
            // Act
            var result = await _controller.GetTestOrProduct(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = jsonResult.Value;
            var successProperty = resultValue?.GetType().GetProperty("success")?.GetValue(resultValue);
            Assert.False((bool)successProperty!);
        }

        [Fact]
        public async Task GetTestOrProduct_WhenServiceFails_ReturnsJsonWithFailure()
        {
            // Arrange
            var itemCode = "T001";
            var errors = new List<ApiErrorDto> { new() { Message = "Not found" } };
            var expectedResponse = ApiResponseDto<TestorProductDto>.FailureResponse(errors, new ApiMetaDto());
            _testListService.GetTestOrProductByIdAsync(itemCode).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.GetTestOrProduct(itemCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = jsonResult.Value;
            var successProperty = resultValue?.GetType().GetProperty("success")?.GetValue(resultValue);
            Assert.False((bool)successProperty!);
        }

        #endregion

        #region CreateTestOrProduct

        [Fact]
        public async Task CreateTestOrProduct_WithValidModel_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new TestOrProductViewModel { ItemCode = "T001", ItemDescription = "New Test", DefraUnitPrice = 10.5m };
            var dto = new TestorProductDto { ItemCode = "T001", ItemDescription = "New Test", DefraUnitPrice = 10.5m, FpsYear = CurrentYear };
            var createdDto = new TestorProductDto { ItemCode = "T001", ItemDescription = "New Test", DefraUnitPrice = 10.5m };
            var expectedResponse = ApiResponseDto<TestorProductDto>.SuccessResponse(createdDto);

            _mapper.Map<TestorProductDto>(Arg.Is<TestOrProductViewModel>(m => m.ItemCode == "T001")).Returns(dto);
            _testListService.CreateTestOrProductAsync(Arg.Is<TestorProductDto>(d => d.ItemCode == "T001" && d.FpsYear == CurrentYear))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.CreateTestOrProduct(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = jsonResult.Value;
            var successProperty = resultValue?.GetType().GetProperty("success")?.GetValue(resultValue);
            Assert.True((bool)successProperty!);
        }

        [Fact]
        public async Task CreateTestOrProduct_WithInvalidModelState_ReturnsJsonWithValidationErrors()
        {
            // Arrange
            var model = new TestOrProductViewModel { ItemCode = "T001" };
            _controller.ModelState.AddModelError("DefraUnitPrice", "Required");

            // Act
            var result = await _controller.CreateTestOrProduct(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = jsonResult.Value;
            var successProperty = resultValue?.GetType().GetProperty("success")?.GetValue(resultValue);
            Assert.False((bool)successProperty!);
        }

        [Fact]
        public async Task CreateTestOrProduct_WhenServiceFails_ReturnsJsonWithFailure()
        {
            // Arrange
            var model = new TestOrProductViewModel { ItemCode = "T001", DefraUnitPrice = 10.5m };
            var dto = new TestorProductDto { ItemCode = "T001", DefraUnitPrice = 10.5m, FpsYear = CurrentYear };
            var errors = new List<ApiErrorDto> { new() { Message = "Creation failed", Code = "CREATE_ERROR" } };
            var expectedResponse = ApiResponseDto<TestorProductDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<TestorProductDto>(Arg.Any<TestOrProductViewModel>()).Returns(dto);
            _testListService.CreateTestOrProductAsync(Arg.Any<TestorProductDto>())
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.CreateTestOrProduct(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = jsonResult.Value;
            var successProperty = resultValue?.GetType().GetProperty("success")?.GetValue(resultValue);
            Assert.False((bool)successProperty!);
        }

        #endregion

        #region UpdateTestOrProduct

        [Fact]
        public async Task UpdateTestOrProduct_WithValidItemCodeAndModel_ReturnsJsonSuccess()
        {
            // Arrange
            var itemCode = "T001";
            var model = new TestOrProductViewModel { ItemCode = itemCode, ItemDescription = "Updated Test", DefraUnitPrice = 15.5m };
            var dto = new TestorProductDto { ItemCode = itemCode, ItemDescription = "Updated Test", DefraUnitPrice = 15.5m, FpsYear = CurrentYear };
            var updatedDto = new TestorProductDto { ItemCode = itemCode, ItemDescription = "Updated Test", DefraUnitPrice = 15.5m };
            var expectedResponse = ApiResponseDto<TestorProductDto>.SuccessResponse(updatedDto);

            _mapper.Map<TestorProductDto>(Arg.Is<TestOrProductViewModel>(m => m.ItemCode == itemCode)).Returns(dto);
            _testListService.UpdateTestOrProductAsync(itemCode, Arg.Is<TestorProductDto>(d => d.ItemCode == itemCode && d.FpsYear == CurrentYear))
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.UpdateTestOrProduct(itemCode, model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = jsonResult.Value;
            var successProperty = resultValue?.GetType().GetProperty("success")?.GetValue(resultValue);
            Assert.True((bool)successProperty!);
        }

        [Fact]
        public async Task UpdateTestOrProduct_WithNullOrEmptyItemCode_ReturnsJsonWithValidationErrors()
        {
            // Arrange
            var model = new TestOrProductViewModel { ItemCode = "T001" };

            // Act
            var result = await _controller.UpdateTestOrProduct(string.Empty, model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = jsonResult.Value;
            var successProperty = resultValue?.GetType().GetProperty("success")?.GetValue(resultValue);
            Assert.False((bool)successProperty!);
        }

        [Fact]
        public async Task UpdateTestOrProduct_WhenServiceFails_ReturnsJsonWithFailure()
        {
            // Arrange
            var itemCode = "T001";
            var model = new TestOrProductViewModel { ItemCode = itemCode, DefraUnitPrice = 15.5m };
            var dto = new TestorProductDto { ItemCode = itemCode, DefraUnitPrice = 15.5m, FpsYear = CurrentYear };
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var expectedResponse = ApiResponseDto<TestorProductDto>.FailureResponse(errors, new ApiMetaDto());

            _mapper.Map<TestorProductDto>(Arg.Any<TestOrProductViewModel>()).Returns(dto);
            _testListService.UpdateTestOrProductAsync(Arg.Any<string>(), Arg.Any<TestorProductDto>())
                .Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.UpdateTestOrProduct(itemCode, model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = jsonResult.Value;
            var successProperty = resultValue?.GetType().GetProperty("success")?.GetValue(resultValue);
            Assert.False((bool)successProperty!);
        }

        #endregion

        #region DeleteTestOrProduct

        [Fact]
        public async Task DeleteTestOrProduct_WithValidItemCode_ReturnsJsonSuccess()
        {
            // Arrange
            var itemCode = "T001";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _testListService.DeleteTestOrProductAsync(itemCode).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.DeleteTestOrProduct(itemCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = jsonResult.Value;
            var successProperty = resultValue?.GetType().GetProperty("success")?.GetValue(resultValue);
            Assert.True((bool)successProperty!);
        }

        [Fact]
        public async Task DeleteTestOrProduct_WithNullOrEmptyItemCode_ReturnsJsonWithFailure()
        {
            // Act
            var result = await _controller.DeleteTestOrProduct(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = jsonResult.Value;
            var successProperty = resultValue?.GetType().GetProperty("success")?.GetValue(resultValue);
            Assert.False((bool)successProperty!);
        }

        [Fact]
        public async Task DeleteTestOrProduct_WhenServiceFails_ReturnsJsonWithFailure()
        {
            // Arrange
            var itemCode = "T001";
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _testListService.DeleteTestOrProductAsync(itemCode).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _controller.DeleteTestOrProduct(itemCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var resultValue = jsonResult.Value;
            var successProperty = resultValue?.GetType().GetProperty("success")?.GetValue(resultValue);
            Assert.False((bool)successProperty!);
        }

        #endregion
    }
}
