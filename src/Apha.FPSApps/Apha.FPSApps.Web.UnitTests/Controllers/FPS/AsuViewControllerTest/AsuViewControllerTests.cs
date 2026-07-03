/*
 * TRANSFORMENGINE MIGRATION — AsuViewControllerTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - New test class for AsuViewController (Phase 11 MVC controller)
 *   - Tests cover Index(), LoadAsuViewGrid(), and GetTotals() actions
 *   - Verifies: ViewResult with populated ViewModel, PartialViewResult with DataGridConfig,
 *     Json results for invalid ModelState, success/failure service responses, null animalType
 *     guards, and totals calculation
 *   - Mirrors AnimalJobControllerTests.cs NSubstitute + JsonElement pattern
 *
 * PRESERVED:
 *   - NSubstitute for IMapper and IAsuViewService mocks
 *   - GetJsonResultElement helper consistent with AnimalJobControllerTests.cs
 *   - [MethodName]_[StateUnderTest]_[ExpectedResult] naming
 *   - xUnit Assert.* APIs (no FluentAssertions in Web.UnitTests)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: PopulateDropdownsAsync is a private method; tested indirectly
 *     via Index() — direct isolation of dropdown population requires extracting to interface
 *   - TRANSFORMENGINE TODO: verify [Authorize] attribute roles in Phase 14 security gate
 */
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.AsuViewControllerTest
{
    /// <summary>
    /// xUnit tests for <see cref="AsuViewController"/> — the MVC controller
    /// for the ASU Data View page created in Phase 11.
    /// </summary>
    public class AsuViewControllerTests
    {
        private readonly IMapper          _mapper;
        private readonly IAsuViewService  _asuViewService;
        private readonly AsuViewController _controller;

        public AsuViewControllerTests()
        {
            _mapper         = Substitute.For<IMapper>();
            _asuViewService = Substitute.For<IAsuViewService>();
            _controller     = new AsuViewController(_mapper, _asuViewService);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private static AsuViewDto BuildAsuViewDto(string animalType = "CATTLE") =>
            new() { Id = 1, AnimalType = animalType, Project = "PRJ001", AnimalDays = 5.0, Cost = 250m };

        private static AsuViewItem BuildAsuViewItem(string animalType = "CATTLE") =>
            new() { Id = 1, AnimalType = animalType, Project = "PRJ001", AnimalDays = 5.0, Cost = 250m };

        private static AnimalDto BuildAnimalDto(string animalType = "CATTLE") =>
            new() { AnimalType = animalType, Species = "Bovine", SecurityLevel = "L1", DailyRate = 50m };

        private static ApiResponseDto<List<AnimalDto>> BuildAnimalTypeLookupSuccess() =>
            ApiResponseDto<List<AnimalDto>>.SuccessResponse(
                new List<AnimalDto> { BuildAnimalDto("CATTLE"), BuildAnimalDto("SHEEP") });

        private static ApiResponseDto<List<AnimalDto>> BuildAnimalTypeLookupFailure() =>
            ApiResponseDto<List<AnimalDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Lookup failed", Code = "ERROR" } },
                new ApiMetaDto());

        // ── Constructor Tests ─────────────────────────────────────────────────

        #region Constructor

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => new AsuViewController(null!, _asuViewService));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenAsuViewServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => new AsuViewController(_mapper, null!));
        }

        #endregion

        // ── Index Tests ───────────────────────────────────────────────────────

        #region Index

        // TRANSFORMENGINE: happy path — lookup succeeds; ViewModel populated with grid + dropdown
        [Fact]
        public async Task Index_ServiceReturnsAnimalTypeLookup_ReturnsViewResultWithPopulatedViewModel()
        {
            // Arrange
            _asuViewService.GetAnimalTypeLookupAsync()
                .Returns(BuildAnimalTypeLookupSuccess());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AsuViewViewModel>(viewResult.Model);
            Assert.NotNull(model.AsuViewGrid);
            Assert.Equal("asuViewGrid", model.AsuViewGrid.GridId);
            Assert.False(model.AsuViewGrid.AllowAdd);
            Assert.False(model.AsuViewGrid.AllowEdit);
            Assert.False(model.AsuViewGrid.AllowDelete);
            await _asuViewService.Received(1).GetAnimalTypeLookupAsync();
        }

        // TRANSFORMENGINE: dropdown populated from lookup result
        [Fact]
        public async Task Index_ServiceReturnsAnimalTypes_ViewModelHasAnimalTypeList()
        {
            // Arrange
            _asuViewService.GetAnimalTypeLookupAsync()
                .Returns(BuildAnimalTypeLookupSuccess());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AsuViewViewModel>(viewResult.Model);
            Assert.Equal(2, model.AnimalTypeList.Count);
        }

        // TRANSFORMENGINE: lookup failure — ViewModel still returned; dropdown is empty
        [Fact]
        public async Task Index_LookupServiceReturnsFailure_ReturnsViewResultWithEmptyDropdown()
        {
            // Arrange
            _asuViewService.GetAnimalTypeLookupAsync()
                .Returns(BuildAnimalTypeLookupFailure());

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AsuViewViewModel>(viewResult.Model);
            Assert.Empty(model.AnimalTypeList);
        }

        // TRANSFORMENGINE: lookup returns null Data — dropdown falls back to empty
        [Fact]
        public async Task Index_LookupServiceReturnsNullData_ReturnsViewResultWithEmptyDropdown()
        {
            // Arrange
            var nullDataResponse = ApiResponseDto<List<AnimalDto>>.SuccessResponse(null!);
            _asuViewService.GetAnimalTypeLookupAsync().Returns(nullDataResponse);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<AsuViewViewModel>(viewResult.Model);
            Assert.Empty(model.AnimalTypeList);
        }

        #endregion

        // ── LoadAsuViewGrid Tests ─────────────────────────────────────────────

        #region LoadAsuViewGrid

        // TRANSFORMENGINE: happy path — valid request + animalType; grid populated from service
        [Fact]
        public async Task LoadAsuViewGrid_ValidRequestAndAnimalType_ReturnsPartialViewWithDataGrid()
        {
            // Arrange
            var request    = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var animalType = "CATTLE";
            var queryParams = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var items       = new List<AsuViewDto> { BuildAsuViewDto() };
            var pagination  = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var response    = ApiResponseDto<List<AsuViewDto>>.SuccessResponse(items, pagination);
            var gridItems   = new List<AsuViewItem> { BuildAsuViewItem() };

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParams);
            _asuViewService.GetAsuViewAsync(queryParams, animalType).Returns(response);
            _mapper.Map<List<AsuViewItem>>(items).Returns(gridItems);

            // Act
            var result = await _controller.LoadAsuViewGrid(request, animalType);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            var grid = Assert.IsType<DataGridConfig<AsuViewItem>>(partial.Model);
            Assert.Equal("asuViewGrid", grid.GridId);
            Assert.Single(grid.Data);
        }

        // TRANSFORMENGINE: service returns empty page — PartialView with empty DataGridConfig
        [Fact]
        public async Task LoadAsuViewGrid_ServiceReturnsEmptyPage_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request    = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var animalType = "CATTLE";
            var queryParams = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var emptyResp   = ApiResponseDto<List<AsuViewDto>>.SuccessResponse(
                new List<AsuViewDto>(), new PaginationDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(queryParams);
            _asuViewService.GetAsuViewAsync(queryParams, animalType).Returns(emptyResp);
            _mapper.Map<List<AsuViewItem>>(Arg.Any<List<AsuViewDto>>())
                .Returns(new List<AsuViewItem>());

            // Act
            var result = await _controller.LoadAsuViewGrid(request, animalType);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<AsuViewItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        // TRANSFORMENGINE: invalid ModelState — JsonResult with success:false
        [Fact]
        public async Task LoadAsuViewGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page is required");
            var request = new PaginationFilter<string>();

            // Act
            var result = await _controller.LoadAsuViewGrid(request, "CATTLE");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Invalid request data", value.GetProperty("message").GetString());
            await _asuViewService.DidNotReceive()
                .GetAsuViewAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        // TRANSFORMENGINE: null animalType — returns empty grid (no backend call with null)
        [Fact]
        public async Task LoadAsuViewGrid_NullAnimalType_ReturnsEmptyGridWithoutCallingService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadAsuViewGrid(request, null);

            // Assert — returns PartialView with empty grid; service NOT called
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            var grid = Assert.IsType<DataGridConfig<AsuViewItem>>(partial.Model);
            Assert.Empty(grid.Data);
            await _asuViewService.DidNotReceive()
                .GetAsuViewAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        // TRANSFORMENGINE: whitespace animalType — treated same as null
        [Fact]
        public async Task LoadAsuViewGrid_WhitespaceAnimalType_ReturnsEmptyGridWithoutCallingService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.LoadAsuViewGrid(request, "   ");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<AsuViewItem>>(partial.Model);
            Assert.Empty(grid.Data);
            await _asuViewService.DidNotReceive()
                .GetAsuViewAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        #endregion

        // ── GetTotals Tests ───────────────────────────────────────────────────

        #region GetTotals

        // TRANSFORMENGINE: happy path — animalType supplied; service called; totals computed
        [Fact]
        public async Task GetTotals_ValidAnimalType_ServiceReturnsData_ReturnsJsonWithTotals()
        {
            // Arrange
            var items = new List<AsuViewDto>
            {
                new() { AnimalDays = 5.0, Cost = 250m },
                new() { AnimalDays = 3.0, Cost = 150m }
            };
            var response = ApiResponseDto<List<AsuViewDto>>.SuccessResponse(items);

            _asuViewService.GetAsuViewAsync(
                Arg.Is<QueryParameters<string>>(q => q.PageSize == int.MaxValue),
                "CATTLE")
                .Returns(response);

            // Act
            var result = await _controller.GetTotals("CATTLE");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            // totalAnimalDays = 5.0 + 3.0 = 8.0
            Assert.Equal(8.0, value.GetProperty("totalAnimalDays").GetDouble(), precision: 1);
        }

        // TRANSFORMENGINE: null animalType — returns zero totals without calling service
        [Fact]
        public async Task GetTotals_NullAnimalType_ReturnsZeroTotalsWithoutCallingService()
        {
            // Act
            var result = await _controller.GetTotals(null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(0.0, value.GetProperty("totalAnimalDays").GetDouble());
            await _asuViewService.DidNotReceive()
                .GetAsuViewAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        // TRANSFORMENGINE: whitespace animalType — returns zero totals without calling service
        [Fact]
        public async Task GetTotals_WhitespaceAnimalType_ReturnsZeroTotalsWithoutCallingService()
        {
            // Act
            var result = await _controller.GetTotals("   ");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(0.0, value.GetProperty("totalAnimalDays").GetDouble());
            await _asuViewService.DidNotReceive()
                .GetAsuViewAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        // TRANSFORMENGINE: service returns failure — JsonResult with success:false and message
        [Fact]
        public async Task GetTotals_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var errors   = new List<ApiErrorDto> { new() { Message = "Failed to load data", Code = "ERROR" } };
            var response = ApiResponseDto<List<AsuViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _asuViewService.GetAsuViewAsync(Arg.Any<QueryParameters<string>>(), "CATTLE")
                .Returns(response);

            // Act
            var result = await _controller.GetTotals("CATTLE");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.NotNull(value.GetProperty("message").GetString());
        }

        // TRANSFORMENGINE: service returns null Data — returns failure json
        [Fact]
        public async Task GetTotals_ServiceReturnsNullData_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var response = new ApiResponseDto<List<AsuViewDto>>
            {
                Success = true,
                Data    = null
            };

            _asuViewService.GetAsuViewAsync(Arg.Any<QueryParameters<string>>(), "CATTLE")
                .Returns(response);

            // Act
            var result = await _controller.GetTotals("CATTLE");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        // TRANSFORMENGINE: totals calculation — sum of AnimalDays and Cost over all rows
        [Fact]
        public async Task GetTotals_ServiceReturnsMultipleRows_TotalsAreSummedCorrectly()
        {
            // Arrange
            var items = new List<AsuViewDto>
            {
                new() { AnimalDays = 10.0, Cost = 500m },
                new() { AnimalDays = 4.0,  Cost = 200m },
                new() { AnimalDays = 6.0,  Cost = 300m }
            };
            var response = ApiResponseDto<List<AsuViewDto>>.SuccessResponse(items);

            _asuViewService.GetAsuViewAsync(Arg.Any<QueryParameters<string>>(), "CATTLE")
                .Returns(response);

            // Act
            var result = await _controller.GetTotals("CATTLE");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            // totalAnimalDays = 10 + 4 + 6 = 20
            Assert.Equal(20.0, value.GetProperty("totalAnimalDays").GetDouble(), precision: 1);
        }

        #endregion
    }
}
