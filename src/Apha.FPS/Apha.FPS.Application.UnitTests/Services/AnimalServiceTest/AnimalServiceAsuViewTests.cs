/*
 * TRANSFORMENGINE MIGRATION — AnimalServiceAsuViewTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - New test class for GetAnimalCostByAnimalTypeAsync added to AnimalService in Phase 3
 *   - Tests cover: happy path (paged result returned), empty result, null query guard,
 *     null/whitespace animalType guard, and repository exception propagation
 *   - Verifies mapper and repository are called with correct arguments (Received(1))
 *   - Mirrors AnimalServiceTests.cs patterns (NSubstitute, FluentAssertions available)
 *
 * PRESERVED:
 *   - NSubstitute + FluentAssertions pattern used in the existing AnimalServiceTests.cs
 *   - [MethodName]_[StateUnderTest]_[ExpectedResult] naming
 *   - Same IAnimalRepository + IMapper constructor injection pattern
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Application.UnitTests.Services.AnimalServiceTest
{
    /// <summary>
    /// xUnit tests for <see cref="AnimalService.GetAnimalCostByAnimalTypeAsync"/>
    /// added in Phase 3 for the ASU View resource family.
    /// </summary>
    public class AnimalServiceAsuViewTests
    {
        private readonly IAnimalRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly AnimalService _sut;

        public AnimalServiceAsuViewTests()
        {
            _mockRepository = Substitute.For<IAnimalRepository>();
            _mockMapper     = Substitute.For<IMapper>();
            _sut            = new AnimalService(_mockRepository, _mockMapper);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static AnimalCostView BuildCostViewEntity(string animalType = "CATTLE") =>
            new()
            {
                IndCounter      = 1,
                AnimalType      = animalType,
                JobCode         = "JOB001",
                NumberOfDays    = 5.0,
                NumberOfAnimals = 2.0
            };

        private static AnimalCostViewDto BuildCostViewDto(string animalType = "CATTLE") =>
            new()
            {
                IndCounter      = 1,
                AnimalType      = animalType,
                JobCode         = "JOB001",
                NumberOfDays    = 5,
                NumberOfAnimals = 2
            };

        // ── GetAnimalCostByAnimalTypeAsync Tests ──────────────────────────────

        #region GetAnimalCostByAnimalTypeAsync

        // TRANSFORMENGINE: happy path — mapper, repo, and result-mapper all called once
        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_ValidQueryAndAnimalType_ReturnsPaginatedResult()
        {
            // Arrange
            var animalType      = "CATTLE";
            var query           = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams    = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var pagedEntities   = new PagedData<AnimalCostView>
            {
                Data           = new List<AnimalCostView> { BuildCostViewEntity() },
                PaginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expectedResult = new PaginatedResult<AnimalCostViewDto>
            {
                Data           = new List<AnimalCostViewDto> { BuildCostViewDto() },
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAnimalCostByAnimalTypeAsync(mappedParams, animalType).Returns(pagedEntities);
            _mockMapper.Map<PaginatedResult<AnimalCostViewDto>>(pagedEntities).Returns(expectedResult);

            // Act
            var result = await _sut.GetAnimalCostByAnimalTypeAsync(query, animalType);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.PaginationData.TotalRecords.Should().Be(1);
            result.Data.First().AnimalType.Should().Be("CATTLE");

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetAnimalCostByAnimalTypeAsync(mappedParams, animalType);
            _mockMapper.Received(1).Map<PaginatedResult<AnimalCostViewDto>>(pagedEntities);
        }

        // TRANSFORMENGINE: empty result — repository returns no rows; mapper still called
        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_NoMatchingRecords_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var animalType   = "UNKNOWN";
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var emptyPaged   = new PagedData<AnimalCostView>
            {
                Data           = new List<AnimalCostView>(),
                PaginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };
            var emptyResult = new PaginatedResult<AnimalCostViewDto>
            {
                Data           = new List<AnimalCostViewDto>(),
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAnimalCostByAnimalTypeAsync(mappedParams, animalType).Returns(emptyPaged);
            _mockMapper.Map<PaginatedResult<AnimalCostViewDto>>(emptyPaged).Returns(emptyResult);

            // Act
            var result = await _sut.GetAnimalCostByAnimalTypeAsync(query, animalType);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);

            await _mockRepository.Received(1).GetAnimalCostByAnimalTypeAsync(mappedParams, animalType);
        }

        // TRANSFORMENGINE: null query guard — ArgumentNullException.ThrowIfNull(query)
        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_NullQuery_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _sut.GetAnimalCostByAnimalTypeAsync(null!, "CATTLE"));

            await _mockRepository.DidNotReceive()
                .GetAnimalCostByAnimalTypeAsync(Arg.Any<PaginationParameters<string>>(), Arg.Any<string>());
        }

        // TRANSFORMENGINE: null animalType — ArgumentException.ThrowIfNullOrWhiteSpace throws
        // ArgumentNullException (subclass of ArgumentException) when the argument is null;
        // use ThrowsAnyAsync to accept both ArgumentException and ArgumentNullException.
        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_NullAnimalType_ThrowsArgumentException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            await Assert.ThrowsAnyAsync<ArgumentException>(
                () => _sut.GetAnimalCostByAnimalTypeAsync(query, null!));

            await _mockRepository.DidNotReceive()
                .GetAnimalCostByAnimalTypeAsync(Arg.Any<PaginationParameters<string>>(), Arg.Any<string>());
        }

        // TRANSFORMENGINE: whitespace animalType — same guard as null
        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_WhitespaceAnimalType_ThrowsArgumentException()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.GetAnimalCostByAnimalTypeAsync(query, "   "));

            await _mockRepository.DidNotReceive()
                .GetAnimalCostByAnimalTypeAsync(Arg.Any<PaginationParameters<string>>(), Arg.Any<string>());
        }

        // TRANSFORMENGINE: repository exception propagates — no try/catch in service
        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var query        = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAnimalCostByAnimalTypeAsync(mappedParams, "CATTLE")
                .Throws(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _sut.GetAnimalCostByAnimalTypeAsync(query, "CATTLE"));

            exception.Message.Should().Be("Database connection failed");

            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            _mockMapper.DidNotReceive().Map<PaginatedResult<AnimalCostViewDto>>(
                Arg.Any<PagedData<AnimalCostView>>());
        }

        // TRANSFORMENGINE: verify pagination params are mapped (covers the _mapper.Map<PaginationParameters> call)
        [Fact]
        public async Task GetAnimalCostByAnimalTypeAsync_PassesMappedParamsToRepository()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page      = 2,
                PageSize  = 5,
                SortBy    = "AnimalType",
                Descending = true
            };
            var mappedParams = new PaginationParameters<string>
            {
                Page      = 2,
                PageSize  = 5,
                SortBy    = "AnimalType",
                Descending = true
            };
            var emptyPaged = new PagedData<AnimalCostView>
            {
                Data           = new List<AnimalCostView>(),
                PaginationData = new PaginationData { PageNumber = 2, PageSize = 5, TotalRecords = 0 }
            };
            var emptyResult = new PaginatedResult<AnimalCostViewDto>
            {
                Data           = new List<AnimalCostViewDto>(),
                PaginationData = new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 0 }
            };

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(mappedParams);
            _mockRepository.GetAnimalCostByAnimalTypeAsync(mappedParams, "CATTLE").Returns(emptyPaged);
            _mockMapper.Map<PaginatedResult<AnimalCostViewDto>>(emptyPaged).Returns(emptyResult);

            // Act
            await _sut.GetAnimalCostByAnimalTypeAsync(query, "CATTLE");

            // Assert — confirms the mapped PaginationParameters (not raw QueryParameters) were passed to repo
            await _mockRepository.Received(1).GetAnimalCostByAnimalTypeAsync(
                Arg.Is<PaginationParameters<string>>(p =>
                    p.Page == 2 && p.PageSize == 5 && p.SortBy == "AnimalType" && p.Descending == true),
                "CATTLE");
        }

        #endregion
    }
}
