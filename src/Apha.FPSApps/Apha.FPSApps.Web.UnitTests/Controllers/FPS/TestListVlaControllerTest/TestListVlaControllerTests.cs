using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Microsoft.AspNetCore.Http;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Handler;
using Apha.FPSApps.Web.Mappings;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.TestListVlaControllerTest
{
    public class TestListVlaControllerTests
    {
        private const string DefaultItemCode = "TEST001";
        private const string DefaultTestCode = "TEST001";
        private const string DefaultBuyer = "BUYER01";
        private const string DefaultProfitCentre = "PC001";
        private const int DefaultFpsYear = 2025;

        private readonly IMapper _mapper;
        private readonly ITestListVlaService _testListVlaService;
        private readonly ITestRequirementService _testRequirementService;
        private readonly ITestCapabilityService _testCapabilityService;
        private readonly IFpsApiClient _fpsApiClient;
        private readonly IFpsTestRCCostApiClient _fpsTestRCCostApiClient;
        private readonly IFpsTestRequirementRCCostApiClient _fpsTestRequirementRCCostApiClient;
        private readonly IFpsYearContext _fpsYearContext;
        private readonly TestListVlaController _controller;

        public TestListVlaControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _testListVlaService = Substitute.For<ITestListVlaService>();
            _testRequirementService = Substitute.For<ITestRequirementService>();
            _testCapabilityService = Substitute.For<ITestCapabilityService>();
            _fpsApiClient = Substitute.For<IFpsApiClient>();
            _fpsTestRCCostApiClient = Substitute.For<IFpsTestRCCostApiClient>();
            _fpsTestRequirementRCCostApiClient = Substitute.For<IFpsTestRequirementRCCostApiClient>();
            _fpsYearContext = Substitute.For<IFpsYearContext>();
            _fpsYearContext.Year.Returns(DefaultFpsYear);
            _fpsApiClient.FpsTestRCCost.Returns(_fpsTestRCCostApiClient);
            _fpsApiClient.FpsTestRequirementRCCost.Returns(_fpsTestRequirementRCCostApiClient);

            _controller = new TestListVlaController(
                _mapper,
                _testListVlaService,
                _testRequirementService,
                _testCapabilityService,
                _fpsApiClient,
                _fpsYearContext);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
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
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        #region Index

        [Fact]
        public void Index_Always_ReturnsViewResultWithViewModel()
        {
            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestListVlaViewModel>(viewResult.Model);
            Assert.Equal(DefaultFpsYear, model.FpsYear);
        }

        [Fact]
        public void Index_Always_PopulatesAllFiveGridConfigs()
        {
            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestListVlaViewModel>(viewResult.Model);
            Assert.NotNull(model.TestListGrid);
            Assert.NotNull(model.TestRequirementsGrid);
            Assert.NotNull(model.ComponentChargesGeneralGrid);
            Assert.NotNull(model.ComponentChargesProjectGrid);
            Assert.NotNull(model.SuppliersGrid);
        }

        [Fact]
        public void Index_Always_MainGridIsReadOnly()
        {
            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestListVlaViewModel>(viewResult.Model);
            Assert.False(model.TestListGrid.AllowAdd);
            Assert.False(model.TestListGrid.AllowEdit);
            Assert.False(model.TestListGrid.AllowDelete);
        }

        [Fact]
        public void Index_Always_SuppliersGridIsReadOnly()
        {
            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestListVlaViewModel>(viewResult.Model);
            Assert.False(model.SuppliersGrid.AllowAdd);
            Assert.False(model.SuppliersGrid.AllowEdit);
            Assert.False(model.SuppliersGrid.AllowDelete);
        }

        #endregion

        #region LoadTestListVlaGrid

        [Fact]
        public async Task LoadTestListVlaGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("test", "Error");

            // Act
            var result = await _controller.LoadTestListVlaGrid(new PaginationFilter<string>());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadTestListVlaGrid_ServiceReturnsData_ReturnsPartialViewWithGridConfig()
        {
            // Arrange
            SetupGridMapper();
            var query = new QueryParameters<string>();
            var response = ApiResponseDto<List<TestorProductDto>>.SuccessResponse(
                new List<TestorProductDto> { new() { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear } },
                new PaginationDto { TotalRecords = 1 });

            _testListVlaService.GetAllAsync(Arg.Any<QueryParameters<string>>())
                .Returns(response);
            _mapper.Map<List<TestListVlaItem>>(Arg.Any<List<TestorProductDto>>())
                .Returns(new List<TestListVlaItem> { new() { ItemCode = DefaultItemCode } });

            // Act
            var result = await _controller.LoadTestListVlaGrid(new PaginationFilter<string>());

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        [Fact]
        public async Task LoadTestListVlaGrid_ServiceReturnsEmpty_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            SetupGridMapper();
            var response = ApiResponseDto<List<TestorProductDto>>.SuccessResponse(
                new List<TestorProductDto>(),
                new PaginationDto { TotalRecords = 0 });

            _testListVlaService.GetAllAsync(Arg.Any<QueryParameters<string>>())
                .Returns(response);
            _mapper.Map<List<TestListVlaItem>>(Arg.Any<List<TestorProductDto>>())
                .Returns(new List<TestListVlaItem>());

            // Act
            var result = await _controller.LoadTestListVlaGrid(new PaginationFilter<string>());

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<TestListVlaItem>>(partialView.Model);
            Assert.Empty(config.Data);
        }

        #endregion

        #region LoadComponentChargesGeneralGrid

        [Fact]
        public async Task LoadComponentChargesGeneralGrid_InvalidModelState_ReturnsJsonFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("test", "Error");

            // Act
            var result = await _controller.LoadComponentChargesGeneralGrid(new PaginationFilter<string>(), DefaultTestCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadComponentChargesGeneralGrid_NullTestCode_ReturnsPartialViewWithEmptyData()
        {
            // Act — no testCode provided
            var result = await _controller.LoadComponentChargesGeneralGrid(new PaginationFilter<string>(), null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        [Fact]
        public async Task LoadComponentChargesGeneralGrid_WithTestCode_CallsRCCostApiClient()
        {
            // Arrange
            var response = ApiResponseDto<List<TestRCCostDto>>.SuccessResponse(new List<TestRCCostDto>());
            _fpsTestRCCostApiClient.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear).Returns(response);
            _mapper.Map<List<TestRCCostItem>>(Arg.Any<List<TestRCCostDto>>())
                .Returns(new List<TestRCCostItem>());

            // Act
            await _controller.LoadComponentChargesGeneralGrid(new PaginationFilter<string>(), DefaultTestCode);

            // Assert
            await _fpsTestRCCostApiClient.Received(1).GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear);
        }

        #endregion

        #region LoadComponentChargesProjectGrid

        [Fact]
        public async Task LoadComponentChargesProjectGrid_WithTestCode_CallsRequirementRCCostApiClient()
        {
            // Arrange
            var response = ApiResponseDto<List<TestRequirementRCCostDto>>.SuccessResponse(new List<TestRequirementRCCostDto>());
            _fpsTestRequirementRCCostApiClient.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear).Returns(response);
            _mapper.Map<List<TestRequirementRCCostItem>>(Arg.Any<List<TestRequirementRCCostDto>>())
                .Returns(new List<TestRequirementRCCostItem>());

            // Act
            await _controller.LoadComponentChargesProjectGrid(new PaginationFilter<string>(), DefaultTestCode);

            // Assert
            await _fpsTestRequirementRCCostApiClient.Received(1).GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear);
        }

        [Fact]
        public async Task LoadComponentChargesProjectGrid_NullTestCode_ReturnsPartialViewWithEmptyData()
        {
            // Act
            var result = await _controller.LoadComponentChargesProjectGrid(new PaginationFilter<string>(), null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        #endregion

        #region LoadTestRequirementsGrid

        [Fact]
        public async Task LoadTestRequirementsGrid_InvalidModelState_ReturnsJsonFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("test", "Error");

            // Act
            var result = await _controller.LoadTestRequirementsGrid(new PaginationFilter<string>(), DefaultTestCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element    = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadTestRequirementsGrid_NullTestCode_ReturnsPartialViewWithEmptyData()
        {
            // Act — no testCode provided
            var result = await _controller.LoadTestRequirementsGrid(new PaginationFilter<string>(), null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        [Fact]
        public async Task LoadTestRequirementsGrid_WithTestCode_CallsTestRequirementService()
        {
            // Arrange
            SetupGridMapper();
            var response = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                new List<TestRequirementDto> { new() { TestCode = DefaultTestCode } },
                new PaginationDto { TotalRecords = 1 });

            _testRequirementService
                .GetPagedTestReqmtAsync(Arg.Any<QueryParameters<string>>(), DefaultTestCode)
                .Returns(response);
            _mapper.Map<List<TestRequirementItem>>(Arg.Any<List<TestRequirementDto>>())
                .Returns(new List<TestRequirementItem> { new() { Buyer = DefaultBuyer } });

            // Act
            await _controller.LoadTestRequirementsGrid(new PaginationFilter<string>(), DefaultTestCode);

            // Assert
            await _testRequirementService.Received(1)
                .GetPagedTestReqmtAsync(Arg.Any<QueryParameters<string>>(), DefaultTestCode);
        }

        [Fact]
        public async Task LoadTestRequirementsGrid_WithTestCode_ReturnsPartialViewWithGridTitle()
        {
            // Arrange
            SetupGridMapper();
            var response = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(
                new List<TestRequirementDto>(),
                new PaginationDto());

            _testRequirementService
                .GetPagedTestReqmtAsync(Arg.Any<QueryParameters<string>>(), DefaultTestCode)
                .Returns(response);
            _mapper.Map<List<TestRequirementItem>>(Arg.Any<List<TestRequirementDto>>())
                .Returns(new List<TestRequirementItem>());

            // Act
            var result = await _controller.LoadTestRequirementsGrid(
                new PaginationFilter<string>(), DefaultTestCode);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var config = Assert.IsType<DataGridConfig<TestRequirementItem>>(partialView.Model);
            Assert.Contains(DefaultTestCode, config.Title);
        }

        #endregion

        #region LoadComponentChargesProjectGrid (additional)

        [Fact]
        public async Task LoadComponentChargesProjectGrid_InvalidModelState_ReturnsJsonFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("test", "Error");

            // Act
            var result = await _controller.LoadComponentChargesProjectGrid(new PaginationFilter<string>(), DefaultTestCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element    = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        #region LoadTestListVlaGrid (failure path)

        [Fact]
        public async Task LoadTestListVlaGrid_ServiceReturnsFailure_ReturnsPartialViewWithEmptyItems()
        {
            // Arrange
            SetupGridMapper();
            var response = ApiResponseDto<List<TestorProductDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Service error" } },
                new ApiMetaDto());

            _testListVlaService.GetAllAsync(Arg.Any<QueryParameters<string>>())
                .Returns(response);

            // Act
            var result = await _controller.LoadTestListVlaGrid(new PaginationFilter<string>());

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var config = Assert.IsType<DataGridConfig<TestListVlaItem>>(partialView.Model);
            Assert.Empty(config.Data);
        }

        #endregion

        #region LoadTestRequirementsGrid (null-data branch)

        [Fact]
        public async Task LoadTestRequirementsGrid_WithTestCode_ServiceReturnsNullData_ReturnsPartialViewWithEmptyItems()
        {
            // Arrange
            SetupGridMapper();
            var response = ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(null!, null);

            _testRequirementService
                .GetPagedTestReqmtAsync(Arg.Any<QueryParameters<string>>(), DefaultTestCode)
                .Returns(response);

            // Act
            var result = await _controller.LoadTestRequirementsGrid(
                new PaginationFilter<string>(), DefaultTestCode);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var config = Assert.IsType<DataGridConfig<TestRequirementItem>>(partialView.Model);
            Assert.Empty(config.Data);
        }

        #endregion

        #region LoadSuppliersGrid

        [Fact]
        public async Task LoadSuppliersGrid_InvalidModelState_ReturnsJsonFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("test", "Error");

            // Act
            var result = await _controller.LoadSuppliersGrid(new PaginationFilter<string>(), DefaultTestCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element    = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadSuppliersGrid_NullTestCode_ReturnsPartialViewWithEmptyData()
        {
            // Act — no testCode provided
            var result = await _controller.LoadSuppliersGrid(new PaginationFilter<string>(), null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        [Fact]
        public async Task LoadSuppliersGrid_WithTestCode_CallsTestCapabilityService()
        {
            // Arrange
            SetupGridMapper();
            var response = ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse(
                new List<TestCapabilityDto> { new() { TestCode = DefaultTestCode } },
                new PaginationDto { TotalRecords = 1 });

            _testCapabilityService
                .GetPagedByTestCodeAsync(Arg.Any<QueryParameters<string>>(), DefaultTestCode)
                .Returns(response);
            _mapper.Map<List<TestCapabilityItem>>(Arg.Any<List<TestCapabilityDto>>())
                .Returns(new List<TestCapabilityItem>());

            // Act
            await _controller.LoadSuppliersGrid(new PaginationFilter<string>(), DefaultTestCode);

            // Assert
            await _testCapabilityService.Received(1)
                .GetPagedByTestCodeAsync(Arg.Any<QueryParameters<string>>(), DefaultTestCode);
        }

        [Fact]
        public async Task LoadSuppliersGrid_WithTestCode_ReturnsPartialViewWithGridTitle()
        {
            // Arrange
            SetupGridMapper();
            var response = ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse(
                new List<TestCapabilityDto>(),
                new PaginationDto());

            _testCapabilityService
                .GetPagedByTestCodeAsync(Arg.Any<QueryParameters<string>>(), DefaultTestCode)
                .Returns(response);
            _mapper.Map<List<TestCapabilityItem>>(Arg.Any<List<TestCapabilityDto>>())
                .Returns(new List<TestCapabilityItem>());

            // Act
            var result = await _controller.LoadSuppliersGrid(
                new PaginationFilter<string>(), DefaultTestCode);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var config = Assert.IsType<DataGridConfig<TestCapabilityItem>>(partialView.Model);
            Assert.Contains(DefaultTestCode, config.Title);
        }

        #endregion

        #region LoadSuppliersGrid (null-data branch)

        [Fact]
        public async Task LoadSuppliersGrid_WithTestCode_ServiceReturnsNullData_ReturnsPartialViewWithEmptyItems()
        {
            // Arrange
            SetupGridMapper();
            var response = ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse(null!, null);

            _testCapabilityService
                .GetPagedByTestCodeAsync(Arg.Any<QueryParameters<string>>(), DefaultTestCode)
                .Returns(response);

            // Act
            var result = await _controller.LoadSuppliersGrid(
                new PaginationFilter<string>(), DefaultTestCode);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
            var config = Assert.IsType<DataGridConfig<TestCapabilityItem>>(partialView.Model);
            Assert.Empty(config.Data);
        }

        #endregion

        #region FpsViewModelMapper profile — TestListVla types

        [Fact]
        public void FpsViewModelMapper_TestListVlaItem_MapsToDto()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<FpsViewModelMapper>(), NullLoggerFactory.Instance);
            var mapper = config.CreateMapper();

            var item = new TestListVlaItem { ItemCode = "VLA01" };
            var dto = mapper.Map<TestorProductDto>(item);
            Assert.Equal(item.ItemCode, dto.ItemCode);
        }

        [Fact]
        public void FpsViewModelMapper_TestRCCostItem_MapsToDto()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<FpsViewModelMapper>(), NullLoggerFactory.Instance);
            var mapper = config.CreateMapper();

            var item = new TestRCCostItem { TestCode = "T001" };
            var dto = mapper.Map<TestRCCostDto>(item);
            Assert.Equal(item.TestCode, dto.TestCode);
        }

        [Fact]
        public void FpsViewModelMapper_TestRequirementRCCostItem_MapsToDto()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<FpsViewModelMapper>(), NullLoggerFactory.Instance);
            var mapper = config.CreateMapper();

            var item = new TestRequirementRCCostItem { TestCode = "T001" };
            var dto = mapper.Map<TestRequirementRCCostDto>(item);
            Assert.Equal(item.TestCode, dto.TestCode);
        }

        [Fact]
        public void FpsViewModelMapper_TestRequirementItem_MapsToDto()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<FpsViewModelMapper>(), NullLoggerFactory.Instance);
            var mapper = config.CreateMapper();

            var item = new TestRequirementItem { TestCode = "T001" };
            var dto = mapper.Map<TestRequirementDto>(item);
            Assert.Equal(item.TestCode, dto.TestCode);
        }

       

        #endregion
    }
}
