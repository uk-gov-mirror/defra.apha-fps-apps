/*
 * TRANSFORMENGINE MIGRATION — ContributionSummaryServiceTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: xUnit tests for ContributionSummaryService (backend Application layer).
 *   - Uses NSubstitute for IContributionSummaryRepository and IMapper mocks.
 *   - Uses FluentAssertions (.Should()) consistent with other Application.UnitTests files.
 *   - Covers all six service methods: GetByProfitCentreAsync, GetByIdAsync, CreateAsync,
 *     UpdateAsync, DeleteAsync, GetSummaryAsync.
 *   - Validates business rules: required Wg/Grade/ProfitCentre on create/update,
 *     ExistsAsync guard before UpdateAsync/DeleteAsync.
 *
 * PRESERVED:
 *   - Naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult].
 *   - Pattern matches ProjectServiceTests in the same project.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Add tests for fpsYear pass-through in GetSummaryAsync once
 *     IFpsRequestContext wiring is confirmed in Phase 4 repository implementation.
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

namespace Apha.FPS.Application.UnitTests.Services.ContributionSummaryServiceTest
{
    public class ContributionSummaryServiceTests
    {
        private readonly IContributionSummaryRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly ContributionSummaryService _sut;

        public ContributionSummaryServiceTests()
        {
            _mockRepository = Substitute.For<IContributionSummaryRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new ContributionSummaryService(_mockRepository, _mockMapper);
        }

        // ── GetByProfitCentreAsync ─────────────────────────────────────────────

        #region GetByProfitCentreAsync

        [Fact]
        public async Task GetByProfitCentreAsync_HappyPath_ReturnsMappedPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var profitCentre = "Bact";
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var entities = new List<ContributionSummary>
            {
                new() { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = profitCentre, FpsYear = 2026 },
                new() { Id = 2, Wg = "BAC2", Grade = "C_BAC2", ProfitCentre = profitCentre, FpsYear = 2026 }
            };
            var paginationData = new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var pagedData = new PagedData<ContributionSummary>(entities, paginationData);
            var dtos = new List<ContributionSummaryDto>
            {
                new() { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = profitCentre },
                new() { Id = 2, Wg = "BAC2", Grade = "C_BAC2", ProfitCentre = profitCentre }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var expectedResult = new PaginatedResult<ContributionSummaryDto>(dtos, paginationDto);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetByProfitCentreAsync(paginationParams, profitCentre).Returns(pagedData);
            _mockMapper.Map<PaginatedResult<ContributionSummaryDto>>(pagedData).Returns(expectedResult);

            // Act
            var result = await _sut.GetByProfitCentreAsync(query, profitCentre);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data.First().Wg.Should().Be("BAC1");
            _mockMapper.Received(1).Map<PaginationParameters<string>>(query);
            await _mockRepository.Received(1).GetByProfitCentreAsync(paginationParams, profitCentre);
            _mockMapper.Received(1).Map<PaginatedResult<ContributionSummaryDto>>(pagedData);
        }

        [Fact]
        public async Task GetByProfitCentreAsync_EmptyResult_ReturnsMappedEmptyResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var profitCentre = "Bact";
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);
            var emptyPagedData = new PagedData<ContributionSummary>(
                Enumerable.Empty<ContributionSummary>(),
                new PaginationData { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });
            var emptyResult = new PaginatedResult<ContributionSummaryDto>(
                Enumerable.Empty<ContributionSummaryDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetByProfitCentreAsync(paginationParams, profitCentre).Returns(emptyPagedData);
            _mockMapper.Map<PaginatedResult<ContributionSummaryDto>>(emptyPagedData).Returns(emptyResult);

            // Act
            var result = await _sut.GetByProfitCentreAsync(query, profitCentre);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetByProfitCentreAsync_NullProfitCentre_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await _sut.GetByProfitCentreAsync(query, string.Empty));
        }

        [Fact]
        public async Task GetByProfitCentreAsync_NullQuery_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await _sut.GetByProfitCentreAsync(null!, "Bact"));
        }

        [Fact]
        public async Task GetByProfitCentreAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var profitCentre = "Bact";
            var paginationParams = new PaginationParameters<string>(page: 1, pageSize: 10);

            _mockMapper.Map<PaginationParameters<string>>(query).Returns(paginationParams);
            _mockRepository.GetByProfitCentreAsync(paginationParams, profitCentre)
                .Returns(Task.FromException<PagedData<ContributionSummary>>(new Exception("Database error")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetByProfitCentreAsync(query, profitCentre));
            exception.Message.Should().Be("Database error");
        }

        #endregion

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_HappyPath_ReturnsMappedDto()
        {
            // Arrange
            var entity = new ContributionSummary { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var dto = new ContributionSummaryDto { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };

            _mockRepository.GetByIdAsync(1).Returns(entity);
            _mockMapper.Map<ContributionSummaryDto>(entity).Returns(dto);

            // Act
            var result = await _sut.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.Wg.Should().Be("BAC1");
            await _mockRepository.Received(1).GetByIdAsync(1);
            _mockMapper.Received(1).Map<ContributionSummaryDto>(entity);
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetByIdAsync(999).Returns((ContributionSummary?)null);

            // Act
            var result = await _sut.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
            _mockMapper.DidNotReceive().Map<ContributionSummaryDto>(Arg.Any<ContributionSummary>());
        }

        [Fact]
        public async Task GetByIdAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetByIdAsync(1)
                .Returns(Task.FromException<ContributionSummary?>(new Exception("Database error")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () => await _sut.GetByIdAsync(1));
            exception.Message.Should().Be("Database error");
        }

        #endregion

        // ── CreateAsync ───────────────────────────────────────────────────────

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_HappyPath_ReturnsMappedCreatedDto()
        {
            // Arrange
            var inputDto = new ContributionSummaryDto { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var entity = new ContributionSummary { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var createdEntity = new ContributionSummary { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var createdDto = new ContributionSummaryDto { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };

            _mockMapper.Map<ContributionSummary>(inputDto).Returns(entity);
            _mockRepository.CreateAsync(entity).Returns(createdEntity);
            _mockMapper.Map<ContributionSummaryDto>(createdEntity).Returns(createdDto);

            // Act
            var result = await _sut.CreateAsync(inputDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Wg.Should().Be("BAC1");
            _mockMapper.Received(1).Map<ContributionSummary>(inputDto);
            await _mockRepository.Received(1).CreateAsync(entity);
            _mockMapper.Received(1).Map<ContributionSummaryDto>(createdEntity);
        }

        [Fact]
        public async Task CreateAsync_NullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _sut.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_MissingWg_ThrowsArgumentException()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Wg = "", Grade = "C_BAC1", ProfitCentre = "Bact" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.CreateAsync(dto));
            exception.Message.Should().Contain("Work group code");
        }

        [Fact]
        public async Task CreateAsync_MissingGrade_ThrowsArgumentException()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Wg = "BAC1", Grade = "", ProfitCentre = "Bact" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.CreateAsync(dto));
            exception.Message.Should().Contain("Grade");
        }

        [Fact]
        public async Task CreateAsync_MissingProfitCentre_ThrowsArgumentException()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.CreateAsync(dto));
            exception.Message.Should().Contain("Profit centre");
        }

        [Fact]
        public async Task CreateAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var inputDto = new ContributionSummaryDto { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var entity = new ContributionSummary { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };

            _mockMapper.Map<ContributionSummary>(inputDto).Returns(entity);
            _mockRepository.CreateAsync(entity)
                .Returns(Task.FromException<ContributionSummary>(new Exception("Database error")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () => await _sut.CreateAsync(inputDto));
            exception.Message.Should().Be("Database error");
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_HappyPath_ReturnsMappedUpdatedDto()
        {
            // Arrange
            var id = 1;
            var inputDto = new ContributionSummaryDto { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var entity = new ContributionSummary { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var updatedEntity = new ContributionSummary { Id = id, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            var updatedDto = new ContributionSummaryDto { Id = id, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };

            _mockRepository.ExistsAsync(id).Returns(true);
            _mockMapper.Map<ContributionSummary>(inputDto).Returns(entity);
            _mockRepository.UpdateAsync(id, entity).Returns(updatedEntity);
            _mockMapper.Map<ContributionSummaryDto>(updatedEntity).Returns(updatedDto);

            // Act
            var result = await _sut.UpdateAsync(id, inputDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            await _mockRepository.Received(1).ExistsAsync(id);
            await _mockRepository.Received(1).UpdateAsync(id, entity);
        }

        [Fact]
        public async Task UpdateAsync_NullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _sut.UpdateAsync(1, null!));
        }

        [Fact]
        public async Task UpdateAsync_MissingWg_ThrowsArgumentException()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Wg = "", Grade = "C_BAC1", ProfitCentre = "Bact" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.UpdateAsync(1, dto));
        }

        [Fact]
        public async Task UpdateAsync_NotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var dto = new ContributionSummaryDto { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };
            _mockRepository.ExistsAsync(999).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _sut.UpdateAsync(999, dto));
            await _mockRepository.DidNotReceive().UpdateAsync(Arg.Any<int>(), Arg.Any<ContributionSummary>());
        }

        #endregion

        // ── DeleteAsync ───────────────────────────────────────────────────────

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_HappyPath_ReturnsTrue()
        {
            // Arrange
            _mockRepository.ExistsAsync(1).Returns(true);
            _mockRepository.DeleteAsync(1).Returns(true);

            // Act
            var result = await _sut.DeleteAsync(1);

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).ExistsAsync(1);
            await _mockRepository.Received(1).DeleteAsync(1);
        }

        [Fact]
        public async Task DeleteAsync_NotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _mockRepository.ExistsAsync(999).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await _sut.DeleteAsync(999));
            await _mockRepository.DidNotReceive().DeleteAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task DeleteAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.ExistsAsync(1).Returns(true);
            _mockRepository.DeleteAsync(1)
                .Returns(Task.FromException<bool>(new Exception("Database error")));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(async () => await _sut.DeleteAsync(1));
            exception.Message.Should().Be("Database error");
        }

        #endregion

        // ── GetSummaryAsync ───────────────────────────────────────────────────

        #region GetSummaryAsync

        [Fact]
        public async Task GetSummaryAsync_HappyPath_ReturnsMappedSummaryDto()
        {
            // Arrange
            var profitCentre = "Bact";
            var totals = new ContributionSummaryTotals
            {
                TotalBudgetBids = 100m,
                ContributionTarget = 200m,
                TotalToRecover = 300m,
                TotalTimeFeeFromPlanHrs = 500m
            };
            var summaryDto = new ContributionSummarySummaryDto
            {
                TotalBudgetBids = 100m,
                ContributionTarget = 200m,
                TotalToRecover = 300m,
                TotalTimeFeeFromPlanHrs = 500m
            };

            _mockRepository.GetSummaryTotalsAsync(profitCentre, null).Returns(totals);
            _mockMapper.Map<ContributionSummarySummaryDto>(totals).Returns(summaryDto);

            // Act
            var result = await _sut.GetSummaryAsync(profitCentre, null);

            // Assert
            result.Should().NotBeNull();
            result!.TotalBudgetBids.Should().Be(100m);
            result.ContributionTarget.Should().Be(200m);
            await _mockRepository.Received(1).GetSummaryTotalsAsync(profitCentre, null);
            _mockMapper.Received(1).Map<ContributionSummarySummaryDto>(totals);
        }

        [Fact]
        public async Task GetSummaryAsync_NoRowsFound_ReturnsNull()
        {
            // Arrange
            _mockRepository.GetSummaryTotalsAsync("Bact", null).Returns((ContributionSummaryTotals?)null);

            // Act
            var result = await _sut.GetSummaryAsync("Bact", null);

            // Assert
            result.Should().BeNull();
            _mockMapper.DidNotReceive().Map<ContributionSummarySummaryDto>(Arg.Any<ContributionSummaryTotals>());
        }

        [Fact]
        public async Task GetSummaryAsync_EmptyProfitCentre_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () => await _sut.GetSummaryAsync("", null));
        }

        [Fact]
        public async Task GetSummaryAsync_WithExplicitYear_PassesYearToRepository()
        {
            // Arrange
            var profitCentre = "Bact";
            int? fpsYear = 2026;
            _mockRepository.GetSummaryTotalsAsync(profitCentre, fpsYear).Returns((ContributionSummaryTotals?)null);

            // Act
            await _sut.GetSummaryAsync(profitCentre, fpsYear);

            // Assert
            await _mockRepository.Received(1).GetSummaryTotalsAsync(profitCentre, fpsYear);
        }

        #endregion
    }
}
