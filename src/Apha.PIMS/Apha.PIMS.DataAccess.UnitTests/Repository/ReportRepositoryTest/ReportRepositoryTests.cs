using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.ReportRepositoryTest
{
    public class ReportRepositoryTests
    {
        // ── factory ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a ReportRepository backed by mock DbContext/DbSet.
        /// All parameters are optional — omitted sets are initialised as empty.
        /// </summary>
        private static ReportRepository CreateRepository(
            IEnumerable<Report>? reports = null)
        {
            var mockContext  = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var reportsMockSet = RepositoryTestHelper.CreateMockDbSet(reports ?? Enumerable.Empty<Report>());

            RepositoryTestHelper.SetupDbSetOperations(reportsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Reports).Returns(reportsMockSet.Object);

            return new ReportRepository(mockContext.Object);
        }

        /// <summary>
        /// Returns the repository plus the mocked DbSet and DbContext for
        /// tests that need to verify Add/Update/SaveChanges calls.
        /// </summary>
        private static (
            ReportRepository Repo,
            Mock<DbSet<Report>> ReportsDbSet,
            Mock<PimsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<Report>? reports = null)
        {
            var mockContext    = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var reportsMockSet = RepositoryTestHelper.CreateMockDbSet(reports ?? Enumerable.Empty<Report>());

            RepositoryTestHelper.SetupDbSetOperations(reportsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Reports).Returns(reportsMockSet.Object);

            var repo = new ReportRepository(mockContext.Object);
            return (repo, reportsMockSet, mockContext);
        }

        // ── helpers ───────────────────────────────────────────────────────────────

        private static Report MakeReport(int id, string name = "Test Report") =>
            new Report { Id = id, ReportName = name, Type = "R", Emailable = false };

        // ── GetAllAsync ───────────────────────────────────────────────────────────

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ReturnsAllReports_WhenDataExists()
        {
            // Arrange
            var reports = new List<Report>
            {
                MakeReport(1, "Report Alpha"),
                MakeReport(2, "Report Beta")
            };
            var repo = CreateRepository(reports);

            // Act
            var result = await repo.GetAllReportsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.ReportName == "Report Alpha");
            Assert.Contains(result, r => r.ReportName == "Report Beta");
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<Report>());

            // Act
            var result = await repo.GetAllReportsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        // ── GetByIdAsync ──────────────────────────────────────────────────────────

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ReturnsReport_WhenIdExists()
        {
            // Arrange
            var reports = new List<Report>
            {
                MakeReport(1, "Alpha"),
                MakeReport(2, "Beta")
            };
            var repo = CreateRepository(reports);

            // Act
            var result = await repo.GetReportByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.Id);
            Assert.Equal("Alpha", result.ReportName);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenIdDoesNotExist()
        {
            // Arrange
            var reports = new List<Report> { MakeReport(1, "Alpha") };
            var repo = CreateRepository(reports);

            // Act
            var result = await repo.GetReportByIdAsync(99);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.GetReportByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        #endregion

        // ── AddAsync ──────────────────────────────────────────────────────────────

        #region AddAsync

        [Fact]
        public async Task AddAsync_ReturnsAddedEntity()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks();
            var report = MakeReport(0, "New Report");

            // Act
            var result = await repo.AddReportAsync(report);

            // Assert
            Assert.NotNull(result);
            Assert.Same(report, result);
        }

        [Fact]
        public async Task AddAsync_CallsDbSetAdd()
        {
            // Arrange
            var (repo, reportsDbSet, _) = CreateRepositoryWithMocks();
            var report = MakeReport(0, "New Report");

            // Act
            await repo.AddReportAsync(report);

            // Assert
            reportsDbSet.Verify(x => x.Add(It.Is<Report>(r => r.ReportName == "New Report")), Times.Once);
        }

        [Fact]
        public async Task AddAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, mockContext) = CreateRepositoryWithMocks();
            var report = MakeReport(0, "New Report");

            // Act
            await repo.AddReportAsync(report);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        // ── UpdateAsync ───────────────────────────────────────────────────────────

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ReturnsUpdatedEntity()
        {
            // Arrange
            var existing = MakeReport(5, "Original Name");
            var (repo, _, _) = CreateRepositoryWithMocks(new List<Report> { existing });
            var updatedEntity = MakeReport(5, "Updated Name");

            // Act
            var result = await repo.UpdateReportAsync(updatedEntity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.ReportName);
        }

        [Fact]
        public async Task UpdateAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var existing = MakeReport(5, "Name");
            var (repo, _, mockContext) = CreateRepositoryWithMocks(new List<Report> { existing });

            // Act
            await repo.UpdateReportAsync(MakeReport(5, "Updated"));

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        // ── ExistsAsync ───────────────────────────────────────────────────────────

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenIdExists()
        {
            // Arrange
            var reports = new List<Report> { MakeReport(3, "Exists") };
            var repo = CreateRepository(reports);

            // Act
            var result = await repo.ReportExistsAsync(3);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenIdDoesNotExist()
        {
            // Arrange
            var reports = new List<Report> { MakeReport(1, "Alpha") };
            var repo = CreateRepository(reports);

            // Act
            var result = await repo.ReportExistsAsync(99);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.ReportExistsAsync(1);

            // Assert
            Assert.False(result);
        }

        #endregion
    }
}
