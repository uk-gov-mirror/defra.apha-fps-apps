using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.ProjectDetailsRepositoryTest
{
    public class ProjectDetailsRepositoryTests
    {
        /// <summary>
        /// Creates a ProjectDetailsRepository with in-memory data for all DbSets.
        /// All parameters are optional — omitted sets are initialised as empty.
        /// </summary>
        private static ProjectDetailsRepository CreateRepository(
            IEnumerable<ProjectRadTrackData>? radtrackData = null,
            IEnumerable<Risk>? risks = null,
            IEnumerable<ProposedProject>? proposedProjects = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var radtrackDataMockSet = RepositoryTestHelper.CreateMockDbSet(radtrackData ?? Enumerable.Empty<ProjectRadTrackData>());
            var risksMockSet = RepositoryTestHelper.CreateMockDbSet(risks ?? Enumerable.Empty<Risk>());
            var proposedProjectsMockSet = RepositoryTestHelper.CreateMockDbSet(proposedProjects ?? Enumerable.Empty<ProposedProject>());

            RepositoryTestHelper.SetupDbSetOperations(radtrackDataMockSet);
            RepositoryTestHelper.SetupDbSetOperations(proposedProjectsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.ProjectRadTrackData).Returns(radtrackDataMockSet.Object);
            mockContext.Setup(x => x.Risks).Returns(risksMockSet.Object);
            mockContext.Setup(x => x.ProposedProjects).Returns(proposedProjectsMockSet.Object);

            return new ProjectDetailsRepository(mockContext.Object);
        }

        /// <summary>
        /// Returns the repository alongside its mocked DbSets and DbContext
        /// for tests that need to verify Add / Update / SaveChanges calls.
        /// </summary>
        private static (
            ProjectDetailsRepository Repo,
            Mock<DbSet<ProjectRadTrackData>> RadtrackDataDbSet,
            Mock<DbSet<ProposedProject>> ProposedProjectsDbSet,
            Mock<PimsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<ProjectRadTrackData>? radtrackData = null,
                IEnumerable<Risk>? risks = null,
                IEnumerable<ProposedProject>? proposedProjects = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var radtrackDataMockSet = RepositoryTestHelper.CreateMockDbSet(radtrackData ?? Enumerable.Empty<ProjectRadTrackData>());
            var risksMockSet = RepositoryTestHelper.CreateMockDbSet(risks ?? Enumerable.Empty<Risk>());
            var proposedProjectsMockSet = RepositoryTestHelper.CreateMockDbSet(proposedProjects ?? Enumerable.Empty<ProposedProject>());

            RepositoryTestHelper.SetupDbSetOperations(radtrackDataMockSet);
            RepositoryTestHelper.SetupDbSetOperations(proposedProjectsMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var realOptions = new DbContextOptionsBuilder<PimsDbContext>()
                .UseNpgsql("Host=localhost;Database=pims_test;Username=test;Password=test")
                .Options;
            var realContext = new PimsDbContext(realOptions);

            mockContext.Setup(x => x.Entry(It.IsAny<ProposedProject>()))
                .Returns((ProposedProject e) =>
                {
                    var entry = realContext.Entry(e);
                    if (entry.State == EntityState.Detached)
                    {
                        realContext.Attach(e);
                        entry = realContext.Entry(e);
                    }
                    return entry;
                });

            mockContext.Setup(x => x.ProjectRadTrackData).Returns(radtrackDataMockSet.Object);
            mockContext.Setup(x => x.Risks).Returns(risksMockSet.Object);
            mockContext.Setup(x => x.ProposedProjects).Returns(proposedProjectsMockSet.Object);

            var repo = new ProjectDetailsRepository(mockContext.Object);
            return (repo, radtrackDataMockSet, proposedProjectsMockSet, mockContext);
        }

        #region GetPimsDetailAsync — field mapping

        [Fact]
        public async Task GetPimsDetailAsync_ReturnsProjectDetail_WhenParentprojectExists()
        {
            // Arrange
            var radtrackData = new List<ProjectRadTrackData>
            {
                new()
                {
                    Parentproject  = "PP001",
                    Version        = "V1",
                    Fileref        = "FILE001",
                    Customerref    = "CUST001",
                    Startdate      = new DateTime(2023, 1, 1),
                    Enddate        = new DateTime(2024, 1, 1),
                    Costbooknumber = "CB001",
                    Riskid         = 1,
                    Useprojectyear = 1,
                    Revisedenddate = new DateTime(2024, 6, 1),
                    Closeddate     = new DateTime(2024, 12, 1)
                }
            };
            var repo = CreateRepository(radtrackData: radtrackData);

            // Act
            var result = await repo.GetPimsDetailAsync("PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PP001", result.Parentproject);
            Assert.Equal("V1", result.Version);
            Assert.Equal("FILE001", result.FileRef);
            Assert.Equal("CUST001", result.CustomerRef);
            Assert.Equal(new DateTime(2023, 1, 1), result.StartDate);
            Assert.Equal(new DateTime(2024, 1, 1), result.EndDate);
            Assert.Equal("CB001", result.CostbookNumber);
            Assert.Equal(1, result.Riskid);
            Assert.True(result.UseProjectYears);
            Assert.Equal(new DateTime(2024, 6, 1), result.RevisedEndDate);
            Assert.Equal(new DateTime(2024, 12, 1), result.ClosedDate);
        }

        [Fact]
        public async Task GetPimsDetailAsync_ReturnsNullRiskid_WhenRiskidIsNull()
        {
            // Arrange — Riskid is null on the radtrack record
            var radtrackData = new List<ProjectRadTrackData>
            {
                new() { Parentproject = "PP001", Riskid = null, Useprojectyear = 0 }
            };
            var repo = CreateRepository(radtrackData: radtrackData);

            // Act
            var result = await repo.GetPimsDetailAsync("PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Riskid);
        }

        [Theory]
        [InlineData((short)1, true)]
        [InlineData((short)0, false)]
        [InlineData((short)5, true)]
        public async Task GetPimsDetailAsync_MapsUseProjectYears_FromUseprojectyear(
            short useprojectyear, bool expectedUseProjectYears)
        {
            // Arrange
            var radtrackData = new List<ProjectRadTrackData>
            {
                new() { Parentproject = "PP001", Useprojectyear = useprojectyear }
            };
            var repo = CreateRepository(radtrackData: radtrackData);

            // Act
            var result = await repo.GetPimsDetailAsync("PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedUseProjectYears, result.UseProjectYears);
        }

        #endregion

        #region GetPimsDetailAsync — not found cases

        [Fact]
        public async Task GetPimsDetailAsync_ReturnsNull_WhenParentprojectDoesNotExist()
        {
            // Arrange
            var radtrackData = new List<ProjectRadTrackData>
            {
                new() { Parentproject = "PP001", Useprojectyear = 0 }
            };
            var repo = CreateRepository(radtrackData: radtrackData);

            // Act
            var result = await repo.GetPimsDetailAsync("UNKNOWN");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPimsDetailAsync_ReturnsNull_WhenProjectRadtrackdataIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(radtrackData: new List<ProjectRadTrackData>());

            // Act
            var result = await repo.GetPimsDetailAsync("PP001");

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("NONEXISTENT")]
        public async Task GetPimsDetailAsync_ReturnsNull_WhenIdDoesNotMatch(string parentproject)
        {
            // Arrange
            var radtrackData = new List<ProjectRadTrackData>
            {
                new() { Parentproject = "PP001", Useprojectyear = 0 }
            };
            var repo = CreateRepository(radtrackData: radtrackData);

            // Act
            var result = await repo.GetPimsDetailAsync(parentproject);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddPimsDetailAsync — return value & side effects

        [Fact]
        public async Task AddPimsDetailAsync_AddsEntityAndReturnsIt()
        {
            // Arrange
            var (repo, _, _, _) = CreateRepositoryWithMocks();
            var entity = new ProjectDetail
            {
                Parentproject  = "PP001",
                Version        = "V1",
                FileRef        = "FILE001",
                CustomerRef    = "CUST001",
                CostbookNumber = "CB001",
                UseProjectYears = false
            };

            // Act
            var result = await repo.AddPimsDetailAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Same(entity, result);
            Assert.Equal("PP001", result.Parentproject);
            Assert.Equal("V1", result.Version);
            Assert.Equal("FILE001", result.FileRef);
        }

        [Fact]
        public async Task AddPimsDetailAsync_CallsDbSetAdd()
        {
            // Arrange
            var (repo, radtrackDataDbSet, _, _) = CreateRepositoryWithMocks();
            var entity = new ProjectDetail { Parentproject = "PP001", UseProjectYears = false };

            // Act
            await repo.AddPimsDetailAsync(entity);

            // Assert
            radtrackDataDbSet.Verify(x => x.Add(It.IsAny<ProjectRadTrackData>()), Times.Once);
        }

        [Fact]
        public async Task AddPimsDetailAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, _, mockContext) = CreateRepositoryWithMocks();
            var entity = new ProjectDetail { Parentproject = "PP001", UseProjectYears = false };

            // Act
            await repo.AddPimsDetailAsync(entity);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        #region AddPimsDetailAsync — ProjectRadTrackData field mapping

        [Fact]
        public async Task AddPimsDetailAsync_MapsAllFields_ToProjectRadTrackData()
        {
            // Arrange
            var (repo, radtrackDataDbSet, _, _) = CreateRepositoryWithMocks();

            ProjectRadTrackData? captured = null;
            radtrackDataDbSet
                .Setup(x => x.Add(It.IsAny<ProjectRadTrackData>()))
                .Callback<ProjectRadTrackData>(e => captured = e);

            var entity = new ProjectDetail
            {
                Parentproject   = "PP001",
                Version         = "V1",
                FileRef         = "FILE001",
                CustomerRef     = "CUST001",
                StartDate       = new DateTime(2023, 1, 1),
                EndDate         = new DateTime(2024, 1, 1),
                CostbookNumber  = "CB001",
                Riskid          = 2,
                UseProjectYears = true,
                RevisedEndDate  = new DateTime(2024, 6, 1),
                ClosedDate      = new DateTime(2024, 12, 1)
            };

            // Act
            await repo.AddPimsDetailAsync(entity);

            // Assert
            Assert.NotNull(captured);
            Assert.Equal("PP001", captured!.Parentproject);
            Assert.Equal("V1", captured.Version);
            Assert.Equal("FILE001", captured.Fileref);
            Assert.Equal("CUST001", captured.Customerref);
            Assert.Equal(new DateTime(2023, 1, 1), captured.Startdate);
            Assert.Equal(new DateTime(2024, 1, 1), captured.Enddate);
            Assert.Equal("CB001", captured.Costbooknumber);
            Assert.Equal(2, captured.Riskid);
            Assert.Equal((short)-1, captured.Useprojectyear);
            Assert.Equal(new DateTime(2024, 6, 1), captured.Revisedenddate);
            Assert.Equal(new DateTime(2024, 12, 1), captured.Closeddate);
        }

        [Fact]
        public async Task AddPimsDetailAsync_MapsRiskid_WhenRiskidIsProvided()
        {
            // Arrange
            var (repo, radtrackDataDbSet, _, _) = CreateRepositoryWithMocks();

            ProjectRadTrackData? captured = null;
            radtrackDataDbSet
                .Setup(x => x.Add(It.IsAny<ProjectRadTrackData>()))
                .Callback<ProjectRadTrackData>(e => captured = e);

            var entity = new ProjectDetail
            {
                Parentproject   = "PP001",
                Riskid          = 2,
                UseProjectYears = false
            };

            // Act
            await repo.AddPimsDetailAsync(entity);

            // Assert
            Assert.NotNull(captured);
            Assert.Equal(2, captured!.Riskid);
        }

        [Fact]
        public async Task AddPimsDetailAsync_SetsNullRiskid_WhenRiskidIsNull()
        {
            // Arrange
            var (repo, radtrackDataDbSet, _, _) = CreateRepositoryWithMocks();

            ProjectRadTrackData? captured = null;
            radtrackDataDbSet
                .Setup(x => x.Add(It.IsAny<ProjectRadTrackData>()))
                .Callback<ProjectRadTrackData>(e => captured = e);

            var entity = new ProjectDetail
            {
                Parentproject   = "PP001",
                Riskid          = null,
                UseProjectYears = false
            };

            // Act
            await repo.AddPimsDetailAsync(entity);

            // Assert
            Assert.NotNull(captured);
            Assert.Null(captured!.Riskid);
        }

        [Theory]
        [InlineData(true, (short)-1)]
        [InlineData(false, (short)0)]
        public async Task AddPimsDetailAsync_MapsUseprojectyear_FromUseProjectYears(
            bool useProjectYears, short expectedUseprojectyear)
        {
            // Arrange
            var (repo, radtrackDataDbSet, _, _) = CreateRepositoryWithMocks();

            ProjectRadTrackData? captured = null;
            radtrackDataDbSet
                .Setup(x => x.Add(It.IsAny<ProjectRadTrackData>()))
                .Callback<ProjectRadTrackData>(e => captured = e);

            var entity = new ProjectDetail
            {
                Parentproject   = "PP001",
                UseProjectYears = useProjectYears
            };

            // Act
            await repo.AddPimsDetailAsync(entity);

            // Assert
            Assert.NotNull(captured);
            Assert.Equal(expectedUseprojectyear, captured!.Useprojectyear);
        }

        #endregion

        #region UpdatePimsDetailAsync — when record exists

        [Fact]
        public async Task UpdatePimsDetailAsync_UpdatesAllFields_WhenExistingRecordFound()
        {
            // Arrange
            var existingRecord = new ProjectRadTrackData
            {
                Parentproject  = "PP001",
                Version        = "OLD_V",
                Fileref        = "OLD_FILE",
                Customerref    = "OLD_CUST",
                Startdate      = new DateTime(2020, 1, 1),
                Enddate        = new DateTime(2021, 1, 1),
                Costbooknumber = "OLD_CB",
                Riskid         = null,
                Useprojectyear = 0,
                Revisedenddate = null,
                Closeddate     = null
            };
            var repo = CreateRepository(radtrackData: new List<ProjectRadTrackData> { existingRecord });

            var updatedEntity = new ProjectDetail
            {
                Parentproject   = "PP001",
                Version         = "NEW_V",
                FileRef         = "NEW_FILE",
                CustomerRef     = "NEW_CUST",
                StartDate       = new DateTime(2023, 3, 1),
                EndDate         = new DateTime(2024, 3, 1),
                CostbookNumber  = "NEW_CB",
                Riskid          = 3,
                UseProjectYears = true,
                RevisedEndDate  = new DateTime(2024, 9, 1),
                ClosedDate      = new DateTime(2025, 1, 1)
            };

            // Act
            var result = await repo.UpdatePimsDetailAsync(updatedEntity);

            // Assert
            Assert.Same(updatedEntity, result);
            Assert.Equal("NEW_V", existingRecord.Version);
            Assert.Equal("NEW_FILE", existingRecord.Fileref);
            Assert.Equal("NEW_CUST", existingRecord.Customerref);
            Assert.Equal(new DateTime(2023, 3, 1), existingRecord.Startdate);
            Assert.Equal(new DateTime(2024, 3, 1), existingRecord.Enddate);
            Assert.Equal("NEW_CB", existingRecord.Costbooknumber);
            Assert.Equal(3, existingRecord.Riskid);
            Assert.Equal((short)-1, existingRecord.Useprojectyear);
            Assert.Equal(new DateTime(2024, 9, 1), existingRecord.Revisedenddate);
            Assert.Equal(new DateTime(2025, 1, 1), existingRecord.Closeddate);
        }

        [Fact]
        public async Task UpdatePimsDetailAsync_ReturnsEntity_WhenExistingRecordFound()
        {
            // Arrange
            var existingRecord = new ProjectRadTrackData { Parentproject = "PP001", Useprojectyear = 0 };
            var repo = CreateRepository(radtrackData: new List<ProjectRadTrackData> { existingRecord });
            var entity = new ProjectDetail { Parentproject = "PP001", UseProjectYears = false };

            // Act
            var result = await repo.UpdatePimsDetailAsync(entity);

            // Assert
            Assert.Same(entity, result);
        }

        [Fact]
        public async Task UpdatePimsDetailAsync_CallsSaveChangesAsync_WhenRecordExists()
        {
            // Arrange
            var existingRecord = new ProjectRadTrackData { Parentproject = "PP001", Useprojectyear = 0 };
            var (repo, _, _, mockContext) = CreateRepositoryWithMocks(
                radtrackData: new List<ProjectRadTrackData> { existingRecord });
            var entity = new ProjectDetail { Parentproject = "PP001", UseProjectYears = false };

            // Act
            await repo.UpdatePimsDetailAsync(entity);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Theory]
        [InlineData(true, (short)-1)]
        [InlineData(false, (short)0)]
        public async Task UpdatePimsDetailAsync_MapsUseprojectyear_FromUseProjectYears(
            bool useProjectYears, short expectedUseprojectyear)
        {
            // Arrange
            var existingRecord = new ProjectRadTrackData { Parentproject = "PP001", Useprojectyear = 0 };
            var repo = CreateRepository(radtrackData: new List<ProjectRadTrackData> { existingRecord });
            var entity = new ProjectDetail { Parentproject = "PP001", UseProjectYears = useProjectYears };

            // Act
            await repo.UpdatePimsDetailAsync(entity);

            // Assert
            Assert.Equal(expectedUseprojectyear, existingRecord.Useprojectyear);
        }

        [Fact]
        public async Task UpdatePimsDetailAsync_MapsRiskid_WhenRiskidIsProvided()
        {
            // Arrange
            var existingRecord = new ProjectRadTrackData { Parentproject = "PP001", Useprojectyear = 0, Riskid = null };
            var repo = CreateRepository(radtrackData: new List<ProjectRadTrackData> { existingRecord });

            var entity = new ProjectDetail
            {
                Parentproject   = "PP001",
                Riskid          = 5,
                UseProjectYears = false
            };

            // Act
            await repo.UpdatePimsDetailAsync(entity);

            // Assert
            Assert.Equal(5, existingRecord.Riskid);
        }

        [Fact]
        public async Task UpdatePimsDetailAsync_SetsNullRiskid_WhenRiskidIsNull()
        {
            // Arrange — existing record has a non-null Riskid; update clears it when Riskid is null
            var existingRecord = new ProjectRadTrackData { Parentproject = "PP001", Useprojectyear = 0, Riskid = 3 };
            var repo = CreateRepository(radtrackData: new List<ProjectRadTrackData> { existingRecord });
            var entity = new ProjectDetail
            {
                Parentproject   = "PP001",
                Riskid          = null,
                UseProjectYears = false
            };

            // Act
            await repo.UpdatePimsDetailAsync(entity);

            // Assert
            Assert.Null(existingRecord.Riskid);
        }

        #endregion

        #region UpdatePimsDetailAsync — when record does not exist

        [Fact]
        public async Task UpdatePimsDetailAsync_ReturnsEntityUnchanged_WhenRecordNotFound()
        {
            // Arrange
            var repo = CreateRepository(radtrackData: new List<ProjectRadTrackData>());
            var entity = new ProjectDetail
            {
                Parentproject   = "UNKNOWN",
                Version         = "V1",
                UseProjectYears = false
            };

            // Act
            var result = await repo.UpdatePimsDetailAsync(entity);

            // Assert
            Assert.Same(entity, result);
            Assert.Equal("V1", result.Version);
        }

        [Fact]
        public async Task UpdatePimsDetailAsync_DoesNotCallSaveChangesAsync_WhenRecordNotFound()
        {
            // Arrange
            var (repo, _, _, mockContext) = CreateRepositoryWithMocks(
                radtrackData: new List<ProjectRadTrackData>());
            var entity = new ProjectDetail { Parentproject = "UNKNOWN", UseProjectYears = false };

            // Act
            await repo.UpdatePimsDetailAsync(entity);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 0);
        }

        #endregion

        #region GetProposedProjectAsync

        [Fact]
        public async Task GetProposedProjectAsync_ReturnsProposedProject_WhenExists()
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Id = 1, Parentproject = "PP001", Projecttitle = "TB Project",  Program = "PROG1", Customer = "CUST1", Projectstatus = "Proposed", Disease = "TB"  },
                new() { Id = 2, Parentproject = "PP002", Projecttitle = "FMD Project", Program = "PROG2", Customer = "CUST2", Projectstatus = "Active",   Disease = "FMD" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);

            // Act
            var result = await repo.GetProposedProjectAsync("PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("PP001", result.Parentproject);
            Assert.Equal("TB Project", result.Projecttitle);
            Assert.Equal("Proposed", result.Projectstatus);
        }

        [Fact]
        public async Task GetProposedProjectAsync_ReturnsNull_WhenProjectDoesNotExist()
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Id = 1, Parentproject = "PP001", Projecttitle = "TB Project" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);

            // Act
            var result = await repo.GetProposedProjectAsync("UNKNOWN");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProposedProjectAsync_ReturnsNull_WhenProposedProjectsIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(proposedProjects: new List<ProposedProject>());

            // Act
            var result = await repo.GetProposedProjectAsync("PP001");

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("NONEXISTENT")]
        public async Task GetProposedProjectAsync_ReturnsNull_WhenIdDoesNotMatch(string parentproject)
        {
            // Arrange
            var proposedProjects = new List<ProposedProject>
            {
                new() { Id = 1, Parentproject = "PP001", Projecttitle = "TB Project" }
            };
            var repo = CreateRepository(proposedProjects: proposedProjects);

            // Act
            var result = await repo.GetProposedProjectAsync(parentproject);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region UpdateProposedProjectAsync

        [Fact]
        public async Task UpdateProposedProjectAsync_ReturnsEntity()
        {
            // Arrange
            var (repo, _, _, _) = CreateRepositoryWithMocks();
            var entity = new ProposedProject
            {
                Id            = 1,
                Parentproject = "PP001",
                Projecttitle  = "Updated Project",
                Program       = "PROG1",
                Customer      = "CUST1",
                Projectstatus = "Active",
                Disease       = "FMD"
            };

            // Act — transferTo same as Parentproject ? takes the Update/SaveChanges branch
            var result = await repo.UpdateProposedProjectAsync(entity, "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Same(entity, result);
            Assert.Equal("PP001", result.Parentproject);
            Assert.Equal("Updated Project", result.Projecttitle);
        }

        [Fact]
        public async Task UpdateProposedProjectAsync_CallsDbSetUpdate()
        {
            // Arrange
            var (repo, _, proposedProjectsDbSet, _) = CreateRepositoryWithMocks();
            var entity = new ProposedProject { Id = 1, Parentproject = "PP001" };

            // Act — transferTo same as Parentproject ? Update should be called
            await repo.UpdateProposedProjectAsync(entity, "PP001");

            // Assert
            proposedProjectsDbSet.Verify(x => x.Update(entity), Times.Once);
        }

        [Fact]
        public async Task UpdateProposedProjectAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, _, mockContext) = CreateRepositoryWithMocks();
            var entity = new ProposedProject { Id = 1, Parentproject = "PP001" };

            // Act — transferTo same as Parentproject ? SaveChanges should be called
            await repo.UpdateProposedProjectAsync(entity, "PP001");

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task UpdateProposedProjectAsync_WhenTransferToDiffers_CallsDbSetUpdateBeforeTransferBranch()
        {
            // Arrange — repository now updates/saves non-parent fields before transfer logic.
            var (repo, _, proposedProjectsDbSet, _) = CreateRepositoryWithMocks();
            var entity = new ProposedProject { Id = 1, Parentproject = "PP001" };

            // Act + Assert — transfer branch still throws in this unit-test setup,
            // but initial ProposedProjects.Update should already have been called.
            await Assert.ThrowsAnyAsync<Exception>(() =>
                repo.UpdateProposedProjectAsync(entity, "PP002"));

            proposedProjectsDbSet.Verify(x => x.Update(It.IsAny<ProposedProject>()), Times.Once);
        }

        #endregion
    }
}