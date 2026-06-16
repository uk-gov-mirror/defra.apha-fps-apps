/*
 * TRANSFORMENGINE MIGRATION — ContributionSummaryControllerTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 15 — Build, Fix, and Final Validation
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - Phase 15 fix: GetByProfitCentreAsync test methods updated to match actual controller
 *     signature: controller accepts PaginationReq<string> (from Apha.Common.Contracts),
 *     maps it internally to QueryParameters<string> (Application.Pagination) via IMapper,
 *     then passes QueryParameters to the service. Tests now:
 *       (a) create a PaginationReq<string> for the controller call,
 *       (b) create a QueryParameters<string> for service mock setup, and
 *       (c) mock the mapper conversion between the two.
 *
 * PRESERVED:
 *   - Naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult].
 *   - All non-GetByProfitCentreAsync test methods unchanged.
 *   - Pattern matches ProjectControllerTests in the same project.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Add integration tests once ExceptionMiddleware is wired
 *     to verify 400/404 HTTP status codes from thrown ArgumentException / KeyNotFoundException.
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.ContributionSummaryControllerTest
{
    public class ContributionSummaryControllerTests
    {
        private readonly IContributionSummaryService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ContributionSummaryController _controller;

        public ContributionSummaryControllerTests()
        {
            _serviceMock = Substitute.For<IContributionSummaryService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ContributionSummaryController(_serviceMock, _mapperMock);
        }

        // ── GetByProfitCentreAsync ─────────────────────────────────────────────

        #region GetByProfitCentreAsync

        [Fact]
        public async Task GetByProfitCentreAsync_HappyPath_ReturnsOkWithMappedResult()
        {
            // Arrange
            // TRANSFORMENGINE: Phase 15 fix — controller takes PaginationReq<string> (API contract);
            //   mapper converts it to QueryParameters<string> before passing to service.
            var paginationReq = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var profitCentre = "Bact";
            var dtos = new List<ContributionSummaryDto>
            {
                new() { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = profitCentre },
                new() { Id = 2, Wg = "BAC2", Grade = "C_BAC2", ProfitCentre = profitCentre }
            };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2, TotalPages = 1 };
            var serviceResult = new PaginatedResult<ContributionSummaryDto>(dtos, pagination);
            var mappedResult = new PaginationRes<ContributionSummaryRes>
            {
                Data = new List<ContributionSummaryRes>
                {
                    new() { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = profitCentre },
                    new() { Id = 2, Wg = "BAC2", Grade = "C_BAC2", ProfitCentre = profitCentre }
                }
            };

            _mapperMock.Map<QueryParameters<string>>(paginationReq).Returns(query);
            _serviceMock.GetByProfitCentreAsync(query, profitCentre).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ContributionSummaryRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetByProfitCentreAsync(paginationReq, profitCentre);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            await _serviceMock.Received(1).GetByProfitCentreAsync(query, profitCentre);
        }

        [Fact]
        public async Task GetByProfitCentreAsync_EmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            // TRANSFORMENGINE: Phase 15 fix — use PaginationReq<string> for controller, QueryParameters<string> for service.
            var paginationReq = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var profitCentre = "Bact";
            var emptyResult = new PaginatedResult<ContributionSummaryDto>(
                Enumerable.Empty<ContributionSummaryDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 });
            var mappedEmpty = new PaginationRes<ContributionSummaryRes> { Data = new List<ContributionSummaryRes>() };

            _mapperMock.Map<QueryParameters<string>>(paginationReq).Returns(query);
            _serviceMock.GetByProfitCentreAsync(query, profitCentre).Returns(emptyResult);
            _mapperMock.Map<PaginationRes<ContributionSummaryRes>>(emptyResult).Returns(mappedEmpty);

            // Act
            var result = await _controller.GetByProfitCentreAsync(paginationReq, profitCentre);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedEmpty, okResult.Value);
        }

        [Fact]
        public async Task GetByProfitCentreAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            // TRANSFORMENGINE: Phase 15 fix — use PaginationReq<string> for controller, QueryParameters<string> for service.
            var paginationReq = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var profitCentre = "Bact";
            _mapperMock.Map<QueryParameters<string>>(paginationReq).Returns(query);
            _serviceMock.GetByProfitCentreAsync(query, profitCentre)
                .Throws(new ArgumentException("Profit centre code cannot be null or empty."));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.GetByProfitCentreAsync(paginationReq, profitCentre));
        }

        #endregion

        // ── GetSummaryAsync ───────────────────────────────────────────────────

        #region GetSummaryAsync

        [Fact]
        public async Task GetSummaryAsync_HappyPath_ReturnsOkWithMappedSummary()
        {
            // Arrange
            var profitCentre = "Bact";
            int? fpsYear = null;
            var summaryDto = new ContributionSummarySummaryDto
            {
                TotalBudgetBids = 100m,
                ContributionTarget = 200m,
                TotalToRecover = 300m
            };
            var summaryRes = new ContributionSummarySummaryRes
            {
                TotalBudgetBids = 100m,
                ContributionTarget = 200m,
                TotalToRecover = 300m
            };

            _serviceMock.GetSummaryAsync(profitCentre, fpsYear).Returns(summaryDto);
            _mapperMock.Map<ContributionSummarySummaryRes>(summaryDto).Returns(summaryRes);

            // Act
            var result = await _controller.GetSummaryAsync(profitCentre, fpsYear);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(summaryRes, okResult.Value);
            await _serviceMock.Received(1).GetSummaryAsync(profitCentre, fpsYear);
        }

        [Fact]
        public async Task GetSummaryAsync_ServiceReturnsNull_ReturnsOkWithEmptySummary()
        {
            // Arrange
            var profitCentre = "Bact";
            _serviceMock.GetSummaryAsync(profitCentre, null).Returns((ContributionSummarySummaryDto?)null);

            // Act
            var result = await _controller.GetSummaryAsync(profitCentre, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<ContributionSummarySummaryRes>(okResult.Value);
            _mapperMock.DidNotReceive().Map<ContributionSummarySummaryRes>(Arg.Any<ContributionSummarySummaryDto>());
        }

        [Fact]
        public async Task GetSummaryAsync_WithExplicitFpsYear_CallsServiceWithYear()
        {
            // Arrange
            var profitCentre = "Bact";
            int? fpsYear = 2026;
            _serviceMock.GetSummaryAsync(profitCentre, fpsYear)
                .Returns((ContributionSummarySummaryDto?)null);

            // Act
            await _controller.GetSummaryAsync(profitCentre, fpsYear);

            // Assert
            await _serviceMock.Received(1).GetSummaryAsync(profitCentre, fpsYear);
        }

        #endregion

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_HappyPath_ReturnsOkWithMappedDto()
        {
            // Arrange
            var id = 1;
            var dto = new ContributionSummaryDto { Id = id, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var res = new ContributionSummaryRes { Id = id, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };

            _serviceMock.GetByIdAsync(id).Returns(dto);
            _mapperMock.Map<ContributionSummaryRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetByIdAsync(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);
            await _serviceMock.Received(1).GetByIdAsync(id);
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ThrowsArgumentException()
        {
            // Arrange
            _serviceMock.GetByIdAsync(999).Returns((ContributionSummaryDto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetByIdAsync(999));
        }

        [Fact]
        public async Task GetByIdAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetByIdAsync(1).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetByIdAsync(1));
        }

        #endregion

        // ── CreateAsync ───────────────────────────────────────────────────────

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_HappyPath_ReturnsCreatedAtAction()
        {
            // Arrange
            var req = new ContributionSummaryReq { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var dto = new ContributionSummaryDto { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var created = new ContributionSummaryDto { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var res = new ContributionSummaryRes { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };

            _mapperMock.Map<ContributionSummaryDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).Returns(created);
            _mapperMock.Map<ContributionSummaryRes>(created).Returns(res);

            // Act
            var result = await _controller.CreateAsync(req);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(res, createdResult.Value);
            Assert.Equal(nameof(_controller.GetByIdAsync), createdResult.ActionName);
        }

        [Fact]
        public async Task CreateAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var req = new ContributionSummaryReq { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var dto = new ContributionSummaryDto { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };

            _mapperMock.Map<ContributionSummaryDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).Throws(new ArgumentException("Work group code (Wg) is required."));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.CreateAsync(req));
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_HappyPath_ReturnsOkWithMappedResult()
        {
            // Arrange
            var id = 1;
            var req = new ContributionSummaryReq { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var dto = new ContributionSummaryDto { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var updated = new ContributionSummaryDto { Id = id, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var res = new ContributionSummaryRes { Id = id, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };

            _mapperMock.Map<ContributionSummaryDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(id, dto).Returns(updated);
            _mapperMock.Map<ContributionSummaryRes>(updated).Returns(res);

            // Act
            var result = await _controller.UpdateAsync(id, req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);
            await _serviceMock.Received(1).UpdateAsync(id, dto);
        }

        [Fact]
        public async Task UpdateAsync_NotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var req = new ContributionSummaryReq { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var dto = new ContributionSummaryDto { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };

            _mapperMock.Map<ContributionSummaryDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(999, dto)
                .Throws(new KeyNotFoundException("Contribution summary row with Id '999' was not found."));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.UpdateAsync(999, req));
        }

        [Fact]
        public async Task UpdateAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var req = new ContributionSummaryReq { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var dto = new ContributionSummaryDto();

            _mapperMock.Map<ContributionSummaryDto>(req).Returns(dto);
            _serviceMock.UpdateAsync(1, dto).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateAsync(1, req));
        }

        #endregion

        // ── DeleteAsync ───────────────────────────────────────────────────────

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_HappyPath_ReturnsOkWithSuccess()
        {
            // Arrange
            _serviceMock.DeleteAsync(1).Returns(true);

            // Act
            var result = await _controller.DeleteAsync(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            await _serviceMock.Received(1).DeleteAsync(1);
        }

        [Fact]
        public async Task DeleteAsync_NotFound_ThrowsArgumentException()
        {
            // Arrange
            _serviceMock.DeleteAsync(999)
                .Throws(new KeyNotFoundException("Contribution summary row with Id '999' was not found."));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteAsync(999));
        }

        [Fact]
        public async Task DeleteAsync_ServiceReturnsFalse_ThrowsArgumentException()
        {
            // Arrange
            _serviceMock.DeleteAsync(1).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAsync(1));
        }

        #endregion
    }
}
