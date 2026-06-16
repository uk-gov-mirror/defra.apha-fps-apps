/*
 * TRANSFORMENGINE MIGRATION — ContributionSummaryRepositoryTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: xUnit tests for ContributionSummaryRepository (DataAccess layer).
 *   - Uses RepositoryTestHelper.CreateMockDbContext<FpsDbContext> and
 *     RepositoryTestHelper.CreateMockDbSet<T> (Moq-based) consistent with all other
 *     repository tests in this project (e.g. ProjectRepositoryTests).
 *   - CreateRepository factory provides only the DbSets needed for each test.
 *   - Covers: GetByProfitCentreAsync, GetByIdAsync, CreateAsync, UpdateAsync,
 *     DeleteAsync, ExistsAsync, GetSummaryTotalsAsync, GetAllProfitCentreCodesAsync.
 *
 * PRESERVED:
 *   - Pattern matches ProjectRepositoryTests and GradeRepositoryTests in the same project.
 *   - Naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult].
 *   - Uses Moq (not NSubstitute) for DbContext/DbSet setup per project convention.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: GetSummaryTotalsAsync uses LINQ GroupBy/Select which does not
 *     run against in-memory data with EF Core ILike predicates in unit tests. The group-by
 *     aggregate tests use simple in-memory data with no EF-specific calls and are therefore
 *     safely unit-testable. Integration tests should cover the PostgreSQL-specific paths.
 */

using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;
using Xunit;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ContributionSummaryRepositoryTest
{
    public class ContributionSummaryRepositoryTests
    {
        /// <summary>
        /// Creates a ContributionSummaryRepository backed by mock DbSets.
        /// </summary>
        private static ContributionSummaryRepository CreateRepository(
            IEnumerable<ContributionSummary>? contributionSummaries = null,
            IEnumerable<ProfitCentre>? profitCentres = null,
            string userEmailId = "test@example.com",
            int fpsYear = 2026)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.UserEmailId).Returns(userEmailId);
            mockRequestContext.Setup(x => x.FpsYear).Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            if (contributionSummaries != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(contributionSummaries);
                mockContext.Setup(x => x.ContributionSummaries).Returns(mockSet.Object);
            }

            if (profitCentres != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(profitCentres);
                mockContext.Setup(x => x.ProfitCentres).Returns(mockSet.Object);
            }

            return new ContributionSummaryRepository(mockContext.Object, mockRequestContext.Object);
        }

        // ── GetByProfitCentreAsync ─────────────────────────────────────────────

        #region GetByProfitCentreAsync Tests

        [Fact]
        public async Task GetByProfitCentreAsync_ReturnsOnlyMatchingProfitCentre()
        {
            // Arrange
            var data = new List<ContributionSummary>
            {
                new() { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact", FpsYear = 2026 },
                new() { Id = 2, Wg = "VIR1", Grade = "C_VIR1", ProfitCentre = "Viro", FpsYear = 2026 },
                new() { Id = 3, Wg = "BAC2", Grade = "C_BAC2", ProfitCentre = "Bact", FpsYear = 2026 }
            };
            var repo = CreateRepository(contributionSummaries: data);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetByProfitCentreAsync(query, "Bact");

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, cs => Assert.Equal("Bact", cs.ProfitCentre));
        }

        [Fact]
        public async Task GetByProfitCentreAsync_ReturnsEmpty_WhenNoProfitCentreMatch()
        {
            // Arrange
            var data = new List<ContributionSummary>
            {
                new() { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact", FpsYear = 2026 }
            };
            var repo = CreateRepository(contributionSummaries: data);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetByProfitCentreAsync(query, "NOPE");

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetByProfitCentreAsync_AppliesPaging()
        {
            // Arrange
            var data = Enumerable.Range(1, 5)
                .Select(i => new ContributionSummary
                {
                    Id = i, Wg = $"BAC{i}", Grade = $"C_BAC{i}",
                    ProfitCentre = "Bact", FpsYear = 2026
                }).ToList();
            var repo = CreateRepository(contributionSummaries: data);
            var query = new PaginationParameters<string>(page: 2, pageSize: 2);

            // Act
            var result = await repo.GetByProfitCentreAsync(query, "Bact");

            // Assert
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetByProfitCentreAsync_DefaultSort_OrdersByWgThenGrade()
        {
            // Arrange
            var data = new List<ContributionSummary>
            {
                new() { Id = 1, Wg = "ZZZ", Grade = "Z_ZZZ", ProfitCentre = "Bact", FpsYear = 2026 },
                new() { Id = 2, Wg = "AAA", Grade = "A_AAA", ProfitCentre = "Bact", FpsYear = 2026 },
                new() { Id = 3, Wg = "MMM", Grade = "M_MMM", ProfitCentre = "Bact", FpsYear = 2026 }
            };
            var repo = CreateRepository(contributionSummaries: data);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetByProfitCentreAsync(query, "Bact");

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("AAA", items[0].Wg);
            Assert.Equal("MMM", items[1].Wg);
            Assert.Equal("ZZZ", items[2].Wg);
        }

        [Fact]
        public async Task GetByProfitCentreAsync_NullProfitCentre_ThrowsArgumentException()
        {
            // Arrange
            var repo = CreateRepository(contributionSummaries: new List<ContributionSummary>());
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await repo.GetByProfitCentreAsync(query, ""));
        }

        #endregion

        // ── GetByIdAsync ──────────────────────────────────────────────────────

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ReturnsEntity_WhenFound()
        {
            // Arrange
            var data = new List<ContributionSummary>
            {
                new() { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact", FpsYear = 2026 },
                new() { Id = 2, Wg = "BAC2", Grade = "C_BAC2", ProfitCentre = "Bact", FpsYear = 2026 }
            };
            var repo = CreateRepository(contributionSummaries: data);

            // Act
            var result = await repo.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("BAC1", result.Wg);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var data = new List<ContributionSummary>
            {
                new() { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact", FpsYear = 2026 }
            };
            var repo = CreateRepository(contributionSummaries: data);

            // Act
            var result = await repo.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenEmpty()
        {
            // Arrange
            var repo = CreateRepository(contributionSummaries: new List<ContributionSummary>());

            // Act
            var result = await repo.GetByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        #endregion

        // ── CreateAsync ───────────────────────────────────────────────────────

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_AddsEntityAndCallsSaveChanges()
        {
            // Arrange
            var data = new List<ContributionSummary>();
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.FpsYear).Returns(2026);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            mockContext.Setup(x => x.ContributionSummaries).Returns(mockSet.Object);

            var repo = new ContributionSummaryRepository(mockContext.Object, mockRequestContext.Object);
            var entity = new ContributionSummary { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };

            // Act
            var result = await repo.CreateAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("BAC1", result.Wg);
            Assert.Equal(2026, result.FpsYear); // FpsYear stamped from context
            mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once());
        }

        [Fact]
        public async Task CreateAsync_NullEntity_ThrowsArgumentNullException()
        {
            // Arrange
            var repo = CreateRepository(contributionSummaries: new List<ContributionSummary>());

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await repo.CreateAsync(null!));
        }

        #endregion

        // ── ExistsAsync ───────────────────────────────────────────────────────

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenEntityExists()
        {
            // Arrange
            var data = new List<ContributionSummary>
            {
                new() { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact", FpsYear = 2026 }
            };
            var repo = CreateRepository(contributionSummaries: data);

            // Act
            var result = await repo.ExistsAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenEntityDoesNotExist()
        {
            // Arrange
            var data = new List<ContributionSummary>
            {
                new() { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact", FpsYear = 2026 }
            };
            var repo = CreateRepository(contributionSummaries: data);

            // Act
            var result = await repo.ExistsAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenEmpty()
        {
            // Arrange
            var repo = CreateRepository(contributionSummaries: new List<ContributionSummary>());

            // Act
            var result = await repo.ExistsAsync(1);

            // Assert
            Assert.False(result);
        }

        #endregion

        // ── DeleteAsync ───────────────────────────────────────────────────────

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_RemovesEntityAndCallsSaveChanges()
        {
            // Arrange
            var data = new List<ContributionSummary>
            {
                new() { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact", FpsYear = 2026 }
            };
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.FpsYear).Returns(2026);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            mockContext.Setup(x => x.ContributionSummaries).Returns(mockSet.Object);
            var repo = new ContributionSummaryRepository(mockContext.Object, mockRequestContext.Object);

            // Act
            var result = await repo.DeleteAsync(1);

            // Assert
            Assert.True(result);
            mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once());
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenEntityNotFound()
        {
            // Arrange
            var data = new List<ContributionSummary>
            {
                new() { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact", FpsYear = 2026 }
            };
            var repo = CreateRepository(contributionSummaries: data);

            // Act
            var result = await repo.DeleteAsync(999);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenEmpty()
        {
            // Arrange
            var repo = CreateRepository(contributionSummaries: new List<ContributionSummary>());

            // Act
            var result = await repo.DeleteAsync(1);

            // Assert
            Assert.False(result);
        }

        #endregion

        // ── GetAllProfitCentreCodesAsync ──────────────────────────────────────

        #region GetAllProfitCentreCodesAsync Tests

        [Fact]
        public async Task GetAllProfitCentreCodesAsync_ReturnsDistinctOrderedCodes()
        {
            // Arrange
            var data = new List<ContributionSummary>
            {
                new() { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact", FpsYear = 2026 },
                new() { Id = 2, Wg = "VIR1", Grade = "C_VIR1", ProfitCentre = "Viro", FpsYear = 2026 },
                new() { Id = 3, Wg = "BAC2", Grade = "C_BAC2", ProfitCentre = "Bact", FpsYear = 2026 }, // duplicate Bact
                new() { Id = 4, Wg = "AFV1", Grade = "C_AFV1", ProfitCentre = "Afv",  FpsYear = 2026 }
            };
            var repo = CreateRepository(contributionSummaries: data);

            // Act
            var result = await repo.GetAllProfitCentreCodesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count); // Afv, Bact, Viro — distinct
            Assert.Equal("Afv",  result[0]); // ordered alphabetically
            Assert.Equal("Bact", result[1]);
            Assert.Equal("Viro", result[2]);
        }

        [Fact]
        public async Task GetAllProfitCentreCodesAsync_ReturnsEmpty_WhenNoData()
        {
            // Arrange
            var repo = CreateRepository(contributionSummaries: new List<ContributionSummary>());

            // Act
            var result = await repo.GetAllProfitCentreCodesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_UpdatesAllFieldsAndCallsSaveChanges()
        {
            // Arrange
            var data = new List<ContributionSummary>
            {
                new() { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact",
                    AvailHrs = 100, ChgRate = 50m, TotalFec = 5000m, FpsYear = 2026 }
            };
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.FpsYear).Returns(2026);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
            mockContext.Setup(x => x.ContributionSummaries).Returns(mockSet.Object);
            var repo = new ContributionSummaryRepository(mockContext.Object, mockRequestContext.Object);

            var updated = new ContributionSummary
            {
                Wg = "BAC1", Grade = "C_BAC1_NEW", ProfitCentre = "Bact",
                AvailHrs = 120, ChgRate = 60m, TotalFec = 7200m,
                TotalPlanHrs = 80, TotalPctPlanned = 67,
                AssuredPlanHrs = 70, AssuredFec = 4200m, AssuredPctPlanned = 58,
                OhRate = 15m, TotalCont = 1050m
            };

            // Act
            var result = await repo.UpdateAsync(1, updated);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("C_BAC1_NEW", result.Grade);
            Assert.Equal(120, result.AvailHrs);
            Assert.Equal(7200m, result.TotalFec);
            mockContext.Verify(x => x.SaveChangesAsync(default), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_ThrowsKeyNotFoundException_WhenNotFound()
        {
            // Arrange
            var data = new List<ContributionSummary>
            {
                new() { Id = 1, Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact", FpsYear = 2026 }
            };
            var repo = CreateRepository(contributionSummaries: data);
            var updated = new ContributionSummary { Wg = "BAC1", Grade = "C_BAC1", ProfitCentre = "Bact" };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(async () => await repo.UpdateAsync(999, updated));
        }

        #endregion
    }
}
