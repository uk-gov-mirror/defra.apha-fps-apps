/*
 * TRANSFORMENGINE MIGRATION — ContributionSummaryControllerTests.cs (FPSApps.Web)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: xUnit tests for the frontend MVC ContributionSummaryController
 *     (Apha.FPSApps.Web/Areas/FPS/Controllers/ContributionSummaryController.cs).
 *   - Uses NSubstitute for IMapper, IContributionSummaryService, IProfitCentreService.
 *   - Covers all public controller actions: Index, LoadContributionSummaryGrid, Create (GET/POST),
 *     Edit (GET/POST), Delete, GetSummary, GetProfitCentres.
 *
 * PRESERVED:
 *   - Naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult].
 *   - Pattern matches ProjectControllerTests in the same project.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Index() calls multiple services. Integration tests should verify
 *     the full Razor view render including DataGridConfig construction.
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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ContributionSummary
{
    public class ContributionSummaryControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IContributionSummaryService _contributionSummaryService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly ContributionSummaryController _controller;

        public ContributionSummaryControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _contributionSummaryService = Substitute.For<IContributionSummaryService>();
            _profitCentreService = Substitute.For<IProfitCentreService>();
            _controller = new ContributionSummaryController(
                _mapper, _contributionSummaryService, _profitCentreService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        // ── Index ──────────────────────────────────────────────────────────────

        #region Index

        [Fact]
        public async Task Index_ServiceReturnsData_ReturnsViewResultWithPopulatedModel()
        {
            // Arrange
            var profitCentres = new List<ContributionSummaryDto>(); // ProfitCentreDto used in practice
            var profitCentreResult = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(
                new List<ProfitCentreDto> { new() { ProfitCentreId = "Bact" } });
            _profitCentreService.GetProfitCentresAsync().Returns(profitCentreResult);

            // GetByProfitCentreAsync for grid
            var pageData = ApiResponseDto<List<ContributionSummaryDto>>.SuccessResponse(
                new List<ContributionSummaryDto> { new() { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });
            _contributionSummaryService.GetByProfitCentreAsync(
                    Arg.Any<QueryParameters<string>>(), Arg.Any<string>())
                .Returns(pageData);

            // GetSummaryAsync for summary boxes
            var summaryResult = ApiResponseDto<ContributionSummarySummaryDto>.SuccessResponse(
                new ContributionSummarySummaryDto { ContributionTarget = 100m });
            _contributionSummaryService.GetSummaryAsync(Arg.Any<string>(), null)
                .Returns(summaryResult);

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<ContributionSummaryItem>>(Arg.Any<List<ContributionSummaryDto>>())
                .Returns(new List<ContributionSummaryItem> { new() { Id = 1 } });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
            _mapper.Map<ContributionSummarySummaryItem>(Arg.Any<ContributionSummarySummaryDto>())
                .Returns(new ContributionSummarySummaryItem { ContributionTarget = 100m });

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ContributionSummaryViewModel>(viewResult.Model);
            Assert.NotNull(model);
        }

        #endregion

        // ── LoadContributionSummaryGrid ───────────────────────────────────────

        #region LoadContributionSummaryGrid

        [Fact]
        public async Task LoadContributionSummaryGrid_ServiceReturnsData_ReturnsPartialViewWithGridConfig()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var profitCentre = "Bact";
            var pageData = ApiResponseDto<List<ContributionSummaryDto>>.SuccessResponse(
                new List<ContributionSummaryDto> { new() { Id = 1, Wg = "BAC1", Grade = "C_BAC1" } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

            _contributionSummaryService.GetByProfitCentreAsync(
                    Arg.Any<QueryParameters<string>>(), profitCentre)
                .Returns(pageData);

            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());
            _mapper.Map<List<ContributionSummaryItem>>(Arg.Any<List<ContributionSummaryDto>>())
                .Returns(new List<ContributionSummaryItem> { new() { Id = 1 } });
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadContributionSummaryGrid(request, profitCentre);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            Assert.IsType<DataGridConfig<ContributionSummaryItem>>(partialResult.Model);
        }

        [Fact]
        public async Task LoadContributionSummaryGrid_NullProfitCentre_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());

            // Act — profitCentre is null so GetByProfitCentreAsync should NOT be called
            var result = await _controller.LoadContributionSummaryGrid(request, null);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialResult.ViewName);
            await _contributionSummaryService.DidNotReceive()
                .GetByProfitCentreAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task LoadContributionSummaryGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var request = new PaginationFilter<string>();
            _controller.ModelState.AddModelError("Filter", "Required");

            // Act
            var result = await _controller.LoadContributionSummaryGrid(request, "Bact");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonResultElement(jsonResult);
            Assert.False(json.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── Create GET ────────────────────────────────────────────────────────

        #region Create (GET)

        [Fact]
        public void Create_Get_ReturnsPartialViewWithEmptyItem()
        {
            // Act
            var result = _controller.Create();

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditContributionSummary", partialResult.ViewName);
            Assert.IsType<ContributionSummaryItem>(partialResult.Model);
        }

        #endregion

        // ── Create POST ───────────────────────────────────────────────────────

        #region Create (POST)

        [Fact]
        public async Task Create_Post_ValidDto_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var created = new ContributionSummaryDto { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var apiResult = ApiResponseDto<ContributionSummaryDto>.SuccessResponse(created);

            _contributionSummaryService.CreateAsync(dto).Returns(apiResult);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonResultElement(jsonResult);
            Assert.True(json.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Create_Post_NullDto_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.Create(null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonResultElement(jsonResult);
            Assert.False(json.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Create_Post_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var apiResult = ApiResponseDto<ContributionSummaryDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "VALIDATION_ERROR", Message = "Wg required." } },
                new ApiMetaDto());

            _contributionSummaryService.CreateAsync(dto).Returns(apiResult);

            // Act
            var result = await _controller.Create(dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonResultElement(jsonResult);
            Assert.False(json.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── Edit GET ──────────────────────────────────────────────────────────

        #region Edit (GET)

        [Fact]
        public async Task Edit_Get_ValidId_ServiceReturnsData_ReturnsPartialViewWithItem()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var apiResult = ApiResponseDto<ContributionSummaryDto>.SuccessResponse(dto);
            var item = new ContributionSummaryItem { Id = 1, Wg = "BAC1" };

            _contributionSummaryService.GetByIdAsync(1).Returns(apiResult);
            _mapper.Map<ContributionSummaryItem>(dto).Returns(item);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var partialResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditContributionSummary", partialResult.ViewName);
            Assert.Equal(item, partialResult.Model);
        }

        [Fact]
        public async Task Edit_Get_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var apiResult = ApiResponseDto<ContributionSummaryDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found." } },
                new ApiMetaDto());

            _contributionSummaryService.GetByIdAsync(999).Returns(apiResult);

            // Act
            var result = await _controller.Edit(999);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonResultElement(jsonResult);
            Assert.False(json.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Edit_Get_InvalidId_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.Edit(0);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonResultElement(jsonResult);
            Assert.False(json.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── Edit POST ─────────────────────────────────────────────────────────

        #region Edit (POST)

        [Fact]
        public async Task Edit_Post_ValidDto_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var updated = new ContributionSummaryDto { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var apiResult = ApiResponseDto<ContributionSummaryDto>.SuccessResponse(updated);

            _contributionSummaryService.UpdateAsync(1, dto).Returns(apiResult);

            // Act
            var result = await _controller.Edit(1, dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonResultElement(jsonResult);
            Assert.True(json.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Edit_Post_NullDto_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.Edit(1, null!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonResultElement(jsonResult);
            Assert.False(json.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Edit_Post_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var apiResult = ApiResponseDto<ContributionSummaryDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Row not found." } },
                new ApiMetaDto());

            _contributionSummaryService.UpdateAsync(1, dto).Returns(apiResult);

            // Act
            var result = await _controller.Edit(1, dto);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonResultElement(jsonResult);
            Assert.False(json.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── Delete ────────────────────────────────────────────────────────────

        #region Delete

        [Fact]
        public async Task Delete_ValidId_ServiceReturnsSuccess_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var apiResult = ApiResponseDto<bool>.SuccessResponse(true);
            _contributionSummaryService.DeleteAsync(1).Returns(apiResult);

            // Act
            var result = await _controller.Delete(1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonResultElement(jsonResult);
            Assert.True(json.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Delete_InvalidId_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.Delete(0);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonResultElement(jsonResult);
            Assert.False(json.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Delete_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var apiResult = ApiResponseDto<bool>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found." } },
                new ApiMetaDto());
            _contributionSummaryService.DeleteAsync(999).Returns(apiResult);

            // Act
            var result = await _controller.Delete(999);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonResultElement(jsonResult);
            Assert.False(json.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── GetSummary ────────────────────────────────────────────────────────

        #region GetSummary

        [Fact]
        public async Task GetSummary_ValidProfitCentre_ServiceReturnsData_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var summaryDto = new ContributionSummarySummaryDto { ContributionTarget = 200m };
            var apiResult = ApiResponseDto<ContributionSummarySummaryDto>.SuccessResponse(summaryDto);
            var summaryItem = new ContributionSummarySummaryItem { ContributionTarget = 200m };

            _contributionSummaryService.GetSummaryAsync("Bact", null).Returns(apiResult);
            _mapper.Map<ContributionSummarySummaryItem>(summaryDto).Returns(summaryItem);

            // Act
            var result = await _controller.GetSummary("Bact");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonResultElement(jsonResult);
            Assert.True(json.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetSummary_EmptyProfitCentre_ReturnsJsonWithSuccessFalse()
        {
            // Act
            var result = await _controller.GetSummary("");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonResultElement(jsonResult);
            Assert.False(json.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetSummary_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var apiResult = ApiResponseDto<ContributionSummarySummaryDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERROR", Message = "Failed." } },
                new ApiMetaDto());
            _contributionSummaryService.GetSummaryAsync("Bact", null).Returns(apiResult);

            // Act
            var result = await _controller.GetSummary("Bact");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonResultElement(jsonResult);
            Assert.False(json.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── GetProfitCentres ──────────────────────────────────────────────────

        #region GetProfitCentres

        [Fact]
        public async Task GetProfitCentres_ServiceReturnsData_ReturnsJsonWithSuccessTrue()
        {
            // Arrange
            var pcs = new List<ProfitCentreDto> { new() { ProfitCentreId = "Bact" }, new() { ProfitCentreId = "Viro" } };
            var apiResult = ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(pcs);
            _profitCentreService.GetProfitCentresAsync().Returns(apiResult);

            // Act
            var result = await _controller.GetProfitCentres();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonResultElement(jsonResult);
            Assert.True(json.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetProfitCentres_ServiceReturnsFailure_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            var apiResult = ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "ERROR", Message = "Failed." } },
                new ApiMetaDto());
            _profitCentreService.GetProfitCentresAsync().Returns(apiResult);

            // Act
            var result = await _controller.GetProfitCentres();

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = GetJsonResultElement(jsonResult);
            Assert.False(json.GetProperty("success").GetBoolean());
        }

        #endregion
    }
}
