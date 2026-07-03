/*
 * TRANSFORMENGINE MIGRATION — AnimalControllerAsuViewTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - New test class created for the GetAsuViewAsync endpoint added to AnimalController in Phase 5
 *   - Tests cover the asu-view route: happy path, empty result, null/whitespace animalType guard,
 *     and service-layer exception propagation
 *   - Mirrors the constructor + dependency-mock pattern established in AnimalControllerTests.cs
 *
 * PRESERVED:
 *   - NSubstitute mock pattern consistent with existing AnimalControllerTests.cs
 *   - [MethodName]_[StateUnderTest]_[ExpectedResult] naming convention
 *   - Uses xUnit Assert.* APIs (no FluentAssertions — project does not reference it)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm that ExceptionMiddleware maps ArgumentException → 400
 *     in integration tests; this unit test asserts the exception is thrown, not the HTTP status
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

namespace Apha.FPS.Api.UnitTests.Controller.AnimalControllerTest
{
    /// <summary>
    /// xUnit tests for the <c>GetAsuViewAsync</c> action added to <see cref="AnimalController"/>
    /// in Phase 5 (ASU View resource family).
    /// </summary>
    public class AnimalControllerAsuViewTests
    {
        private readonly IAnimalService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly AnimalController _controller;

        public AnimalControllerAsuViewTests()
        {
            _serviceMock = Substitute.For<IAnimalService>();
            _mapperMock  = Substitute.For<IMapper>();
            _controller  = new AnimalController(_serviceMock, _mapperMock);
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        // TRANSFORMENGINE: reuse same builder shape as AnimalControllerTests.cs for consistency
        private static AnimalCostViewDto BuildCostViewDto(string animalType = "CATTLE") =>
            new() { IndCounter = 1, AnimalType = animalType, JobCode = "JOB001", NumberOfDays = 5, NumberOfAnimals = 2 };

        private static AsuViewRes BuildAsuViewRes(string animalType = "CATTLE") =>
            new() { Id = 1, AnimalType = animalType, Project = "PRJ001", AnimalDays = 10.0, Cost = 500m };

        private static PaginatedResult<AnimalCostViewDto> BuildPaginatedResult(
            IEnumerable<AnimalCostViewDto>? items = null) =>
            new()
            {
                Data           = (items ?? [BuildCostViewDto()]).ToList(),
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

        private static PaginationRes<AsuViewRes> BuildPaginationRes(IEnumerable<AsuViewRes>? items = null) =>
            new()
            {
                Data           = (items ?? [BuildAsuViewRes()]).ToList(),
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

        // ── GetAsuViewAsync Tests ─────────────────────────────────────────────

        #region GetAsuViewAsync

        // TRANSFORMENGINE: happy path — service returns paged data, mapper produces PaginationRes
        [Fact]
        public async Task GetAsuViewAsync_ValidAnimalType_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query      = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paged      = BuildPaginatedResult();
            var mappedRes  = BuildPaginationRes();

            _serviceMock.GetAnimalCostByAnimalTypeAsync(query, "CATTLE").Returns(paged);
            _mapperMock.Map<PaginationRes<AsuViewRes>>(paged).Returns(mappedRes);

            // Act
            var result = await _controller.GetAsuViewAsync(query, "CATTLE");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedRes, ok.Value);
            await _serviceMock.Received(1).GetAnimalCostByAnimalTypeAsync(query, "CATTLE");
            _mapperMock.Received(1).Map<PaginationRes<AsuViewRes>>(paged);
        }

        // TRANSFORMENGINE: empty page — service returns zero records; mapper still called
        [Fact]
        public async Task GetAsuViewAsync_ServiceReturnsEmptyPage_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query     = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var emptyPage = new PaginatedResult<AnimalCostViewDto>
            {
                Data           = new List<AnimalCostViewDto>(),
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };
            var emptyRes = new PaginationRes<AsuViewRes>
            {
                Data           = new List<AsuViewRes>(),
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };

            _serviceMock.GetAnimalCostByAnimalTypeAsync(query, "CATTLE").Returns(emptyPage);
            _mapperMock.Map<PaginationRes<AsuViewRes>>(emptyPage).Returns(emptyRes);

            // Act
            var result = await _controller.GetAsuViewAsync(query, "CATTLE");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var res = Assert.IsType<PaginationRes<AsuViewRes>>(ok.Value);
            Assert.Empty(res.Data!);
            Assert.Equal(0, res.PaginationData!.TotalRecords);
        }

        // TRANSFORMENGINE: null animalType — controller guard throws ArgumentException (→ 400 via middleware)
        [Fact]
        public async Task GetAsuViewAsync_NullAnimalType_ThrowsArgumentException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.GetAsuViewAsync(query, null));

            await _serviceMock.DidNotReceive()
                .GetAnimalCostByAnimalTypeAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        // TRANSFORMENGINE: empty-string animalType — same guard as null
        [Fact]
        public async Task GetAsuViewAsync_EmptyAnimalType_ThrowsArgumentException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.GetAsuViewAsync(query, ""));

            await _serviceMock.DidNotReceive()
                .GetAnimalCostByAnimalTypeAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        // TRANSFORMENGINE: whitespace-only animalType — same guard as null/empty
        [Fact]
        public async Task GetAsuViewAsync_WhitespaceAnimalType_ThrowsArgumentException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.GetAsuViewAsync(query, "   "));

            await _serviceMock.DidNotReceive()
                .GetAnimalCostByAnimalTypeAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        // TRANSFORMENGINE: service throws — exception propagates to middleware (no controller try/catch)
        [Fact]
        public async Task GetAsuViewAsync_ServiceThrowsException_PropagatesException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _serviceMock.GetAnimalCostByAnimalTypeAsync(query, "CATTLE")
                .ThrowsAsync(new Exception("Database error"));

            await Assert.ThrowsAsync<Exception>(
                () => _controller.GetAsuViewAsync(query, "CATTLE"));
        }

        // TRANSFORMENGINE: verify [HttpGet("asu-view")] attribute is present on the action
        [Fact]
        public void GetAsuViewAsync_HasHttpGetAttribute_WithAsuViewRoute()
        {
            var method = typeof(AnimalController).GetMethod(nameof(AnimalController.GetAsuViewAsync));
            Assert.NotNull(method);
            var attr = method!.GetCustomAttributes(typeof(HttpGetAttribute), true)
                .Cast<HttpGetAttribute>()
                .FirstOrDefault();
            Assert.NotNull(attr);
            Assert.Equal("asu-view", attr!.Template);
        }

        #endregion
    }
}
