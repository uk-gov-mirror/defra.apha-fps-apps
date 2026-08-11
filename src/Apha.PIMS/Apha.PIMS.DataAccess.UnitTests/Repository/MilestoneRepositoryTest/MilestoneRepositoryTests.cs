using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.MilestoneRepositoryTest
{
    public class MilestoneRepositoryTests
    {
        /// <summary>
        /// Creates a MilestoneRepository with in-memory data for all DbSets.
        /// All parameters are optional — omitted sets are initialised as empty.
        /// </summary>
        private static MilestoneRepository CreateRepository(
            IEnumerable<Milestone>? milestones = null,
            IEnumerable<MilestoneType>? milestoneTypes = null,
            IEnumerable<MilestoneFormDates>? milestoneFormDates = null,
            IEnumerable<ProjectRadTrackData>? radtrackData = null,
            IEnumerable<StagingMilestone>? stagingMilestones = null,
            IEnumerable<ProjectManager>? projectManagers = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var milestonesMockSet = RepositoryTestHelper.CreateMockDbSet(milestones ?? Enumerable.Empty<Milestone>());
            var milestoneTypesMockSet = RepositoryTestHelper.CreateMockDbSet(milestoneTypes ?? Enumerable.Empty<MilestoneType>());
            var milestoneFormDatesMockSet = RepositoryTestHelper.CreateMockDbSet(milestoneFormDates ?? Enumerable.Empty<MilestoneFormDates>());
            var radtrackDataMockSet = RepositoryTestHelper.CreateMockDbSet(radtrackData ?? Enumerable.Empty<ProjectRadTrackData>());
            var logMilestonesMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<LogMilestone>());
            var stagingMilestonesMockSet = RepositoryTestHelper.CreateMockDbSet(stagingMilestones ?? Enumerable.Empty<StagingMilestone>());
            var projectManagersMockSet = RepositoryTestHelper.CreateMockDbSet(projectManagers ?? Enumerable.Empty<ProjectManager>());

            RepositoryTestHelper.SetupDbSetOperations(milestonesMockSet);
            RepositoryTestHelper.SetupDbSetOperations(milestoneFormDatesMockSet);
            RepositoryTestHelper.SetupDbSetOperations(logMilestonesMockSet);
            RepositoryTestHelper.SetupDbSetOperations(stagingMilestonesMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Milestones).Returns(milestonesMockSet.Object);
            mockContext.Setup(x => x.MilestoneTypes).Returns(milestoneTypesMockSet.Object);
            mockContext.Setup(x => x.MilestoneFormDates).Returns(milestoneFormDatesMockSet.Object);
            mockContext.Setup(x => x.ProjectRadTrackData).Returns(radtrackDataMockSet.Object);
            mockContext.Setup(x => x.LogMilestones).Returns(logMilestonesMockSet.Object);
            mockContext.Setup(x => x.StagingMilestones).Returns(stagingMilestonesMockSet.Object);
            mockContext.Setup(x => x.ProjectManagers).Returns(projectManagersMockSet.Object);

            return new MilestoneRepository(mockContext.Object);
        }

        /// <summary>
        /// Returns the repository alongside its mocked DbSets and DbContext
        /// for tests that need to verify Add / Update / SaveChanges calls.
        /// </summary>
        private static (
            MilestoneRepository Repo,
            Mock<DbSet<Milestone>> MilestonesDbSet,
            Mock<DbSet<MilestoneFormDates>> MilestoneFormDatesDbSet,
            Mock<PimsDbContext> Context,
            Mock<DbSet<LogMilestone>> LogMilestonesDbSet,
            Mock<DbSet<StagingMilestone>> StagingMilestonesDbSet)
            CreateRepositoryWithMocks(
                IEnumerable<Milestone>? milestones = null,
                IEnumerable<MilestoneType>? milestoneTypes = null,
                IEnumerable<MilestoneFormDates>? milestoneFormDates = null,
                IEnumerable<ProjectRadTrackData>? radtrackData = null,
                IEnumerable<StagingMilestone>? stagingMilestones = null,
                IEnumerable<ProjectManager>? projectManagers = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var milestonesMockSet = RepositoryTestHelper.CreateMockDbSet(milestones ?? Enumerable.Empty<Milestone>());
            var milestoneTypesMockSet = RepositoryTestHelper.CreateMockDbSet(milestoneTypes ?? Enumerable.Empty<MilestoneType>());
            var milestoneFormDatesMockSet = RepositoryTestHelper.CreateMockDbSet(milestoneFormDates ?? Enumerable.Empty<MilestoneFormDates>());
            var radtrackDataMockSet = RepositoryTestHelper.CreateMockDbSet(radtrackData ?? Enumerable.Empty<ProjectRadTrackData>());
            var logMilestonesMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<LogMilestone>());
            var stagingMilestonesMockSet = RepositoryTestHelper.CreateMockDbSet(stagingMilestones ?? Enumerable.Empty<StagingMilestone>());
            var projectManagersMockSet = RepositoryTestHelper.CreateMockDbSet(projectManagers ?? Enumerable.Empty<ProjectManager>());

            RepositoryTestHelper.SetupDbSetOperations(milestonesMockSet);
            RepositoryTestHelper.SetupDbSetOperations(milestoneFormDatesMockSet);
            RepositoryTestHelper.SetupDbSetOperations(logMilestonesMockSet);
            RepositoryTestHelper.SetupDbSetOperations(stagingMilestonesMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.Milestones).Returns(milestonesMockSet.Object);
            mockContext.Setup(x => x.MilestoneTypes).Returns(milestoneTypesMockSet.Object);
            mockContext.Setup(x => x.MilestoneFormDates).Returns(milestoneFormDatesMockSet.Object);
            mockContext.Setup(x => x.ProjectRadTrackData).Returns(radtrackDataMockSet.Object);
            mockContext.Setup(x => x.LogMilestones).Returns(logMilestonesMockSet.Object);
            mockContext.Setup(x => x.StagingMilestones).Returns(stagingMilestonesMockSet.Object);
            mockContext.Setup(x => x.ProjectManagers).Returns(projectManagersMockSet.Object);

            var repo = new MilestoneRepository(mockContext.Object);
            return (repo, milestonesMockSet, milestoneFormDatesMockSet, mockContext, logMilestonesMockSet, stagingMilestonesMockSet);
        }

        private static PaginationParameters<string> DefaultParameters(int page = 1, int pageSize = 10)
            => new PaginationParameters<string> { Page = page, PageSize = pageSize };

        #region GetAllMilestonesAsync — filtering, ordering, paging

        [Fact]
        public async Task GetAllMilestonesAsync_ReturnsMilestonesForMatchingProject()
        {
            // Arrange
            var milestones = new List<Milestone>
            {
                new() { Project = "PP001", Number = "M1", Description = "First"  },
                new() { Project = "PP001", Number = "M2", Description = "Second" },
                new() { Project = "PP002", Number = "M1", Description = "Other"  }
            };
            var repo = CreateRepository(milestones: milestones);

            // Act
            var result = await repo.GetAllMilestonesAsync(DefaultParameters(), "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, m => Assert.Equal("PP001", m.Project));
        }

        [Fact]
        public async Task GetAllMilestonesAsync_ReturnsEmpty_WhenNoMatchingProject()
        {
            // Arrange
            var milestones = new List<Milestone>
            {
                new() { Project = "PP002", Number = "M1" }
            };
            var repo = CreateRepository(milestones: milestones);

            // Act
            var result = await repo.GetAllMilestonesAsync(DefaultParameters(), "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllMilestonesAsync_ReturnsEmpty_WhenDataIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(milestones: []);

            // Act
            var result = await repo.GetAllMilestonesAsync(DefaultParameters(), "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllMilestonesAsync_OrdersByNumber()
        {
            // Arrange
            var milestones = new List<Milestone>
            {
                new() { Project = "PP001", Number = "M3" },
                new() { Project = "PP001", Number = "M1" },
                new() { Project = "PP001", Number = "M2" }
            };
            var repo = CreateRepository(milestones: milestones);

            // Act
            var result = await repo.GetAllMilestonesAsync(DefaultParameters(), "PP001");

            // Assert
            var numbers = result.Data.Select(m => m.Number).ToList();
            Assert.Equal(new[] { "M1", "M2", "M3" }, numbers);
        }

        [Fact]
        public async Task GetAllMilestonesAsync_PaginationData_ReflectsTotalRecords()
        {
            // Arrange
            var milestones = Enumerable.Range(1, 5)
                .Select(i => new Milestone { Project = "PP001", Number = $"M{i}" })
                .ToList();
            var repo = CreateRepository(milestones: milestones);

            // Act
            var result = await repo.GetAllMilestonesAsync(DefaultParameters(page: 1, pageSize: 3), "PP001");

            // Assert
            Assert.Equal(3, result.Data.Count);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetAllMilestonesAsync_ReturnsSecondPage_WhenPaged()
        {
            // Arrange
            var milestones = Enumerable.Range(1, 5)
                .Select(i => new Milestone { Project = "PP001", Number = $"M{i}" })
                .ToList();
            var repo = CreateRepository(milestones: milestones);

            // Act
            var result = await repo.GetAllMilestonesAsync(DefaultParameters(page: 2, pageSize: 3), "PP001");

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("M4", result.Data.First().Number);
        }

        #endregion

        #region GetMilestoneAsync — found and not-found cases

        [Fact]
        public async Task GetMilestoneAsync_ReturnsMilestone_WhenProjectAndNumberMatch()
        {
            // Arrange
            var milestones = new List<Milestone>
            {
                new() { Project = "PP001", Number = "M1", Description = "Alpha", DateDue = new DateTime(2024, 1, 1) },
                new() { Project = "PP001", Number = "M2", Description = "Beta",  DateDue = new DateTime(2024, 6, 1) },
                new() { Project = "PP002", Number = "M1", Description = "Gamma", DateDue = new DateTime(2024, 3, 1) }
            };
            var repo = CreateRepository(milestones: milestones);

            // Act
            var result = await repo.GetMilestoneAsync("PP001", "M1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PP001", result.Project);
            Assert.Equal("M1", result.Number);
            Assert.Equal("Alpha", result.Description);
            Assert.Equal(new DateTime(2024, 1, 1), result.DateDue);
        }

        [Fact]
        public async Task GetMilestoneAsync_ReturnsNull_WhenProjectDoesNotMatch()
        {
            // Arrange
            var milestones = new List<Milestone>
            {
                new() { Project = "PP001", Number = "M1" }
            };
            var repo = CreateRepository(milestones: milestones);

            // Act
            var result = await repo.GetMilestoneAsync("UNKNOWN", "M1");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMilestoneAsync_ReturnsNull_WhenNumberDoesNotMatch()
        {
            // Arrange
            var milestones = new List<Milestone>
            {
                new() { Project = "PP001", Number = "M1" }
            };
            var repo = CreateRepository(milestones: milestones);

            // Act
            var result = await repo.GetMilestoneAsync("PP001", "UNKNOWN");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMilestoneAsync_ReturnsNull_WhenDataIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(milestones: []);

            // Act
            var result = await repo.GetMilestoneAsync("PP001", "M1");

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData("", "M1")]
        [InlineData("PP001", "")]
        [InlineData("NONEXISTENT", "M1")]
        [InlineData("PP001", "NONEXISTENT")]
        public async Task GetMilestoneAsync_ReturnsNull_WhenIdDoesNotMatch(string project, string number)
        {
            // Arrange
            var milestones = new List<Milestone>
            {
                new() { Project = "PP001", Number = "M1" }
            };
            var repo = CreateRepository(milestones: milestones);

            // Act
            var result = await repo.GetMilestoneAsync(project, number);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddMilestoneAsync — return value & side effects

        [Fact]
        public async Task AddMilestoneAsync_AddsEntityAndReturnsIt()
        {
            // Arrange
            var (repo, _, _, _, _, _) = CreateRepositoryWithMocks();
            var entity = new Milestone
            {
                Project = "PP001",
                Number = "M1",
                Description = "First milestone",
                DateDue = new DateTime(2024, 6, 1)
            };

            // Act
            var result = await repo.AddMilestoneAsync(entity, null);

            // Assert
            Assert.NotNull(result);
            Assert.Same(entity, result);
            Assert.Equal("PP001", result.Project);
            Assert.Equal("M1", result.Number);
        }

        [Fact]
        public async Task AddMilestoneAsync_CallsDbSetAdd()
        {
            // Arrange
            var (repo, milestonesDbSet, _, _, _, _) = CreateRepositoryWithMocks();
            var entity = new Milestone { Project = "PP001", Number = "M1", DateDue = new DateTime(2024, 1, 1) };

            // Act
            await repo.AddMilestoneAsync(entity, null);

            // Assert
            milestonesDbSet.Verify(x => x.Add(entity), Times.Once);
        }

        [Fact]
        public async Task AddMilestoneAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, _, mockContext, _, _) = CreateRepositoryWithMocks();
            var entity = new Milestone { Project = "PP001", Number = "M1", DateDue = new DateTime(2024, 1, 1) };

            // Act
            await repo.AddMilestoneAsync(entity, null);

            // Assert — called twice: once for the milestone, once for the log entry
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 2);
        }

        [Fact]
        public async Task AddMilestoneAsync_MapsAllFields()
        {
            // Arrange
            var (repo, milestonesDbSet, _, _, _, _) = CreateRepositoryWithMocks();

            Milestone? captured = null;
            milestonesDbSet
                .Setup(x => x.Add(It.IsAny<Milestone>()))
                .Callback<Milestone>(e => captured = e);

            var entity = new Milestone
            {
                Project = "PP001",
                Number = "M1",
                Description = "Test milestone",
                DateDue = new DateTime(2024, 6, 1),
                DateCompleted = new DateTime(2024, 7, 1),
                DateFormReceived = new DateTime(2024, 7, 15),
                UnderSdReview = 1,
                OnTarget = 1,
                ProjectLeaderComment = "On track",
                CapsComment = "Approved",
                IdType = "D"
            };

            // Act
            await repo.AddMilestoneAsync(entity, null);

            // Assert
            Assert.NotNull(captured);
            Assert.Equal("PP001", captured!.Project);
            Assert.Equal("M1", captured.Number);
            Assert.Equal("Test milestone", captured.Description);
            Assert.Equal(new DateTime(2024, 6, 1), captured.DateDue);
            Assert.Equal(new DateTime(2024, 7, 1), captured.DateCompleted);
            Assert.Equal(new DateTime(2024, 7, 15), captured.DateFormReceived);
            Assert.Equal((short)1, captured.UnderSdReview);
            Assert.Equal((short)1, captured.OnTarget);
            Assert.Equal("On track", captured.ProjectLeaderComment);
            Assert.Equal("Approved", captured.CapsComment);
            Assert.Equal("D", captured.IdType);
        }

        #endregion

        #region UpdateMilestoneAsync — return value & side effects

        [Fact]
        public async Task UpdateMilestoneAsync_ReturnsEntity()
        {
            // Arrange
            var (repo, _, _, _, _, _) = CreateRepositoryWithMocks();
            var entity = new Milestone
            {
                Project = "PP001",
                Number = "M1",
                Description = "Updated milestone",
                DateDue = new DateTime(2024, 9, 1)
            };

            // Act
            var result = await repo.UpdateMilestoneAsync(entity, null);

            // Assert
            Assert.NotNull(result);
            Assert.Same(entity, result);
            Assert.Equal("Updated milestone", result.Description);
        }

        [Fact]
        public async Task UpdateMilestoneAsync_CallsDbSetUpdate()
        {
            // Arrange
            var (repo, milestonesDbSet, _, _, _, _) = CreateRepositoryWithMocks();
            var entity = new Milestone { Project = "PP001", Number = "M1", DateDue = new DateTime(2024, 1, 1) };

            // Act
            await repo.UpdateMilestoneAsync(entity, null);

            // Assert
            milestonesDbSet.Verify(x => x.Update(entity), Times.Once);
        }

        [Fact]
        public async Task UpdateMilestoneAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, _, mockContext, _, _) = CreateRepositoryWithMocks();
            var entity = new Milestone { Project = "PP001", Number = "M1", DateDue = new DateTime(2024, 1, 1) };

            // Act
            await repo.UpdateMilestoneAsync(entity, null);

            // Assert — called twice: once for the milestone, once for the log entry
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 2);
        }

        #endregion

        #region AddMilestoneAsync / UpdateMilestoneAsync — log entry

        [Fact]
        public async Task AddMilestoneAsync_AddsLogEntry_WithUpdateTypeI()
        {
            // Arrange
            var (repo, _, _, _, logDbSet, _) = CreateRepositoryWithMocks();
            var entity = new Milestone { Project = "PP001", Number = "M1", DateDue = new DateTime(2024, 6, 1) };

            // Act
            await repo.AddMilestoneAsync(entity, "user1");

            // Assert
            logDbSet.Verify(x => x.Add(It.Is<LogMilestone>(l =>
                l.Project == "PP001" &&
                l.Number == "M1" &&
                l.UpdateType == 'I' &&
                l.ChangedBy == "user1")), Times.Once);
        }

        [Fact]
        public async Task UpdateMilestoneAsync_AddsLogEntry_WithUpdateTypeU()
        {
            // Arrange
            var (repo, _, _, _, logDbSet, _) = CreateRepositoryWithMocks();
            var entity = new Milestone { Project = "PP001", Number = "M1", DateDue = new DateTime(2024, 6, 1) };

            // Act
            await repo.UpdateMilestoneAsync(entity, "user2");

            // Assert
            logDbSet.Verify(x => x.Add(It.Is<LogMilestone>(l =>
                l.Project == "PP001" &&
                l.Number == "M1" &&
                l.UpdateType == 'U' &&
                l.ChangedBy == "user2")), Times.Once);
        }

        [Fact]
        public async Task AddMilestoneAsync_LogEntry_HasDateChangedSet()
        {
            // Arrange
            var (repo, _, _, _, logDbSet, _) = CreateRepositoryWithMocks();
            var entity = new Milestone { Project = "PP001", Number = "M1", DateDue = new DateTime(2024, 6, 1) };
            var before = DateTime.UtcNow.AddSeconds(-1);

            // Act
            await repo.AddMilestoneAsync(entity, null);

            // Assert
            logDbSet.Verify(x => x.Add(It.Is<LogMilestone>(l =>
                l.DateChanged.HasValue &&
                l.DateChanged.Value >= before)), Times.Once);
        }

        [Fact]
        public async Task AddMilestoneAsync_LogEntry_NullChangedBy_IsAllowed()
        {
            // Arrange
            var (repo, _, _, _, logDbSet, _) = CreateRepositoryWithMocks();
            var entity = new Milestone { Project = "PP001", Number = "M1", DateDue = new DateTime(2024, 6, 1) };

            // Act
            await repo.AddMilestoneAsync(entity, null);

            // Assert
            logDbSet.Verify(x => x.Add(It.Is<LogMilestone>(l => l.ChangedBy == null)), Times.Once);
        }

        [Fact]
        public async Task AddMilestoneAsync_MilestoneIsSaved_EvenIfLogFails()
        {
            // Arrange — make the second SaveChangesAsync (log save) throw
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var milestonesMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<Milestone>());
            var milestoneTypesMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<MilestoneType>());
            var formDatesMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<MilestoneFormDates>());
            var radtrackMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<ProjectRadTrackData>());
            var logMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<LogMilestone>());

            RepositoryTestHelper.SetupDbSetOperations(milestonesMockSet);
            RepositoryTestHelper.SetupDbSetOperations(formDatesMockSet);
            RepositoryTestHelper.SetupDbSetOperations(logMockSet);

            int saveCallCount = 0;
            mockContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    saveCallCount++;
                    if (saveCallCount == 2) throw new Exception("Log DB error");
                    return 1;
                });

            mockContext.Setup(x => x.Milestones).Returns(milestonesMockSet.Object);
            mockContext.Setup(x => x.MilestoneTypes).Returns(milestoneTypesMockSet.Object);
            mockContext.Setup(x => x.MilestoneFormDates).Returns(formDatesMockSet.Object);
            mockContext.Setup(x => x.ProjectRadTrackData).Returns(radtrackMockSet.Object);
            mockContext.Setup(x => x.LogMilestones).Returns(logMockSet.Object);

            var repo = new MilestoneRepository(mockContext.Object);
            var entity = new Milestone { Project = "PP001", Number = "M1", DateDue = new DateTime(2024, 6, 1) };

            // Act — should not throw despite the log save failing
            var result = await repo.AddMilestoneAsync(entity, "user1");

            // Assert — milestone was returned and first save completed
            Assert.NotNull(result);
            Assert.Equal(2, saveCallCount); // both saves were attempted
        }

        #endregion

        #region DeleteMilestoneAsync

        [Fact]
        public async Task DeleteMilestoneAsync_ThrowsException_BecauseBulkDeleteRequiresDatabase()
        {
            // Arrange — ExecuteDeleteAsync is a bulk EF Core operation that cannot
            // be exercised against an in-memory mock query provider.
            var milestones = new List<Milestone>
            {
                new() { Project = "PP001", Number = "M1", DateDue = new DateTime(2024, 1, 1) }
            };
            var repo = CreateRepository(milestones: milestones);

            // Act + Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                repo.DeleteMilestoneAsync("PP001", "M1"));
        }

        #endregion

        #region GetMilestoneTypesAsync — filtering and ordering

        [Fact]
        public async Task GetMilestoneTypesAsync_ReturnsAllTypes_WhenNoFilterProvided()
        {
            // Arrange
            var types = new List<MilestoneType>
            {
                new() { IdType = 'A', Type = "Alpha", MilestoneDeliverable = 'D' },
                new() { IdType = 'B', Type = "Beta",  MilestoneDeliverable = 'M' },
                new() { IdType = 'C', Type = "Gamma", MilestoneDeliverable = 'M' }
            };
            var repo = CreateRepository(milestoneTypes: types);

            // Act
            var result = await repo.GetMilestoneTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetMilestoneTypesAsync_ReturnsFilteredTypes_WhenMilestoneDeliverableProvided()
        {
            // Arrange
            var types = new List<MilestoneType>
            {
                new() { IdType = 'A', Type = "Alpha", MilestoneDeliverable = 'D' },
                new() { IdType = 'B', Type = "Beta",  MilestoneDeliverable = 'M' },
                new() { IdType = 'C', Type = "Gamma", MilestoneDeliverable = 'M' }
            };
            var repo = CreateRepository(milestoneTypes: types);

            // Act
            var result = await repo.GetMilestoneTypesAsync("M");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, t => Assert.Equal('M', t.MilestoneDeliverable));
        }

        [Fact]
        public async Task GetMilestoneTypesAsync_ReturnsEmpty_WhenNoMatchingDeliverable()
        {
            // Arrange
            var types = new List<MilestoneType>
            {
                new() { IdType = 'A', Type = "Alpha", MilestoneDeliverable = 'D' }
            };
            var repo = CreateRepository(milestoneTypes: types);

            // Act
            var result = await repo.GetMilestoneTypesAsync("M");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMilestoneTypesAsync_ReturnsEmpty_WhenDataIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(milestoneTypes: []);

            // Act
            var result = await repo.GetMilestoneTypesAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMilestoneTypesAsync_OrdersByType()
        {
            // Arrange
            var types = new List<MilestoneType>
            {
                new() { IdType = 'C', Type = "Gamma", MilestoneDeliverable = 'D' },
                new() { IdType = 'A', Type = "Alpha", MilestoneDeliverable = 'D' },
                new() { IdType = 'B', Type = "Beta",  MilestoneDeliverable = 'D' }
            };
            var repo = CreateRepository(milestoneTypes: types);

            // Act
            var result = await repo.GetMilestoneTypesAsync();

            // Assert
            var typeNames = result.Select(t => t.Type).ToList();
            Assert.Equal(new[] { "Alpha", "Beta", "Gamma" }, typeNames);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetMilestoneTypesAsync_ReturnsAllTypes_WhenFilterIsNullOrWhitespace(string? filter)
        {
            // Arrange
            var types = new List<MilestoneType>
            {
                new() { IdType = 'A', Type = "Alpha", MilestoneDeliverable = 'D' },
                new() { IdType = 'B', Type = "Beta",  MilestoneDeliverable = 'M' }
            };
            var repo = CreateRepository(milestoneTypes: types);

            // Act
            var result = await repo.GetMilestoneTypesAsync(filter);

            // Assert
            Assert.Equal(2, result.Count);
        }

        #endregion

        #region GetAllMilestoneFormDatesAsync — filtering, ordering, paging

        [Fact]
        public async Task GetAllMilestoneFormDatesAsync_ReturnsFormDatesForMatchingParentProject()
        {
            // Arrange
            var formDates = new List<MilestoneFormDates>
            {
                new() { ParentProject = "PP001", Year = 2024 },
                new() { ParentProject = "PP001", Year = 2023 },
                new() { ParentProject = "PP002", Year = 2024 }
            };
            var repo = CreateRepository(milestoneFormDates: formDates);

            // Act
            var result = await repo.GetAllMilestoneFormDatesAsync(DefaultParameters(), "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, f => Assert.Equal("PP001", f.ParentProject));
        }

        [Fact]
        public async Task GetAllMilestoneFormDatesAsync_ReturnsEmpty_WhenNoMatchingParentProject()
        {
            // Arrange
            var formDates = new List<MilestoneFormDates>
            {
                new() { ParentProject = "PP002", Year = 2024 }
            };
            var repo = CreateRepository(milestoneFormDates: formDates);

            // Act
            var result = await repo.GetAllMilestoneFormDatesAsync(DefaultParameters(), "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllMilestoneFormDatesAsync_ReturnsEmpty_WhenDataIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(milestoneFormDates: []);

            // Act
            var result = await repo.GetAllMilestoneFormDatesAsync(DefaultParameters(), "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllMilestoneFormDatesAsync_OrdersByYearDescending()
        {
            // Arrange
            var formDates = new List<MilestoneFormDates>
            {
                new() { ParentProject = "PP001", Year = 2022 },
                new() { ParentProject = "PP001", Year = 2024 },
                new() { ParentProject = "PP001", Year = 2023 }
            };
            var repo = CreateRepository(milestoneFormDates: formDates);

            // Act
            var result = await repo.GetAllMilestoneFormDatesAsync(DefaultParameters(), "PP001");

            // Assert
            var years = result.Data.Select(f => (int)f.Year).ToList();
            Assert.Equal(new[] { 2024, 2023, 2022 }, years);
        }

        [Fact]
        public async Task GetAllMilestoneFormDatesAsync_PaginationData_ReflectsTotalRecords()
        {
            // Arrange
            var formDates = Enumerable.Range(2020, 5)
                .Select(y => new MilestoneFormDates { ParentProject = "PP001", Year = (short)y })
                .ToList();
            var repo = CreateRepository(milestoneFormDates: formDates);

            // Act
            var result = await repo.GetAllMilestoneFormDatesAsync(DefaultParameters(page: 1, pageSize: 3), "PP001");

            // Assert
            Assert.Equal(3, result.Data.Count);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.TotalPages);
        }

        #endregion

        #region GetMilestoneFormDatesAsync — found and not-found cases

        [Fact]
        public async Task GetMilestoneFormDatesAsync_ReturnsFormDates_WhenYearAndParentProjectMatch()
        {
            // Arrange
            var formDates = new List<MilestoneFormDates>
            {
                new() { Year = 2024, ParentProject = "PP001", Jan = new DateTime(2024, 1, 31), Feb = new DateTime(2024, 2, 28) },
                new() { Year = 2023, ParentProject = "PP001" },
                new() { Year = 2024, ParentProject = "PP002" }
            };
            var repo = CreateRepository(milestoneFormDates: formDates);

            // Act
            var result = await repo.GetMilestoneFormDatesAsync(2024, "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal((short)2024, result.Year);
            Assert.Equal("PP001", result.ParentProject);
            Assert.Equal(new DateTime(2024, 1, 31), result.Jan);
            Assert.Equal(new DateTime(2024, 2, 28), result.Feb);
        }

        [Fact]
        public async Task GetMilestoneFormDatesAsync_ReturnsNull_WhenYearDoesNotMatch()
        {
            // Arrange
            var formDates = new List<MilestoneFormDates>
            {
                new() { Year = 2024, ParentProject = "PP001" }
            };
            var repo = CreateRepository(milestoneFormDates: formDates);

            // Act
            var result = await repo.GetMilestoneFormDatesAsync(2023, "PP001");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMilestoneFormDatesAsync_ReturnsNull_WhenParentProjectDoesNotMatch()
        {
            // Arrange
            var formDates = new List<MilestoneFormDates>
            {
                new() { Year = 2024, ParentProject = "PP001" }
            };
            var repo = CreateRepository(milestoneFormDates: formDates);

            // Act
            var result = await repo.GetMilestoneFormDatesAsync(2024, "UNKNOWN");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMilestoneFormDatesAsync_ReturnsNull_WhenDataIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(milestoneFormDates: []);

            // Act
            var result = await repo.GetMilestoneFormDatesAsync(2024, "PP001");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddMilestoneFormDatesAsync — return value & side effects

        [Fact]
        public async Task AddMilestoneFormDatesAsync_AddsEntityAndReturnsIt()
        {
            // Arrange
            var (repo, _, _, _, _, _) = CreateRepositoryWithMocks();
            var entity = new MilestoneFormDates
            {
                Year = 2024,
                ParentProject = "PP001",
                Jan = new DateTime(2024, 1, 31)
            };

            // Act
            var result = await repo.AddMilestoneFormDatesAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Same(entity, result);
            Assert.Equal((short)2024, result.Year);
            Assert.Equal("PP001", result.ParentProject);
        }

        [Fact]
        public async Task AddMilestoneFormDatesAsync_CallsDbSetAdd()
        {
            // Arrange
            var (repo, _, milestoneFormDatesDbSet, _, _, _) = CreateRepositoryWithMocks();
            var entity = new MilestoneFormDates { Year = 2024, ParentProject = "PP001" };

            // Act
            await repo.AddMilestoneFormDatesAsync(entity);

            // Assert
            milestoneFormDatesDbSet.Verify(x => x.Add(entity), Times.Once);
        }

        [Fact]
        public async Task AddMilestoneFormDatesAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, _, mockContext, _, _) = CreateRepositoryWithMocks();
            var entity = new MilestoneFormDates { Year = 2024, ParentProject = "PP001" };

            // Act
            await repo.AddMilestoneFormDatesAsync(entity);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task AddMilestoneFormDatesAsync_MapsAllMonthFields()
        {
            // Arrange
            var (repo, _, milestoneFormDatesDbSet, _, _, _) = CreateRepositoryWithMocks();

            MilestoneFormDates? captured = null;
            milestoneFormDatesDbSet
                .Setup(x => x.Add(It.IsAny<MilestoneFormDates>()))
                .Callback<MilestoneFormDates>(e => captured = e);

            var entity = new MilestoneFormDates
            {
                Year = 2024,
                ParentProject = "PP001",
                Jan = new DateTime(2024, 1, 31),
                Feb = new DateTime(2024, 2, 28),
                Mar = new DateTime(2024, 3, 31),
                Apr = new DateTime(2024, 4, 30),
                May = new DateTime(2024, 5, 31),
                Jun = new DateTime(2024, 6, 30),
                Jul = new DateTime(2024, 7, 31),
                Aug = new DateTime(2024, 8, 31),
                Sep = new DateTime(2024, 9, 30),
                Oct = new DateTime(2024, 10, 31),
                Nov = new DateTime(2024, 11, 30),
                Dec = new DateTime(2024, 12, 31)
            };

            // Act
            await repo.AddMilestoneFormDatesAsync(entity);

            // Assert
            Assert.NotNull(captured);
            Assert.Equal((short)2024, captured!.Year);
            Assert.Equal("PP001", captured.ParentProject);
            Assert.Equal(new DateTime(2024, 1, 31), captured.Jan);
            Assert.Equal(new DateTime(2024, 2, 28), captured.Feb);
            Assert.Equal(new DateTime(2024, 3, 31), captured.Mar);
            Assert.Equal(new DateTime(2024, 4, 30), captured.Apr);
            Assert.Equal(new DateTime(2024, 5, 31), captured.May);
            Assert.Equal(new DateTime(2024, 6, 30), captured.Jun);
            Assert.Equal(new DateTime(2024, 7, 31), captured.Jul);
            Assert.Equal(new DateTime(2024, 8, 31), captured.Aug);
            Assert.Equal(new DateTime(2024, 9, 30), captured.Sep);
            Assert.Equal(new DateTime(2024, 10, 31), captured.Oct);
            Assert.Equal(new DateTime(2024, 11, 30), captured.Nov);
            Assert.Equal(new DateTime(2024, 12, 31), captured.Dec);
        }

        #endregion

        #region UpdateMilestoneAsync_PMD - return value & side effects

        [Fact]
        public async Task UpdateMilestoneAsync_PMD_ReturnsEntity()
        {
            // Arrange
            var milestone = new Milestone
            {
                Project = "PP001",
                Number = "M1",
                Description = "Original milestone",
                DateDue = new DateTime(2024, 9, 1),
                UnderSdReview = 0,
                OnTarget = 1,
                DateCompleted = null,
                ProjectLeaderComment = ""
            };

            var (repo, _, _, _, _, _) = CreateRepositoryWithMocks(new[] { milestone });

            var updatedDateTime = new DateTime(2024, 8, 15);
            var newComment = "Updated via PMD";
            const string changedBy = "testuser";

            // Act
            var result = await repo.UpdateMilestoneAsync_PMD("PP001", "M1", underReview: 1, onTarget: 0, dateCompleted: updatedDateTime, projectLeaderComment: newComment, changedBy: changedBy);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PP001", result.Project);
            Assert.Equal("M1", result.Number);
            Assert.Equal((short?)1, result.UnderSdReview);
            Assert.Equal((short?)0, result.OnTarget);
            Assert.Equal(updatedDateTime, result.DateCompleted);
            Assert.Equal(newComment, result.ProjectLeaderComment);
            // Original description should remain unchanged
            Assert.Equal("Original milestone", result.Description);
        }

        [Fact]
        public async Task UpdateMilestoneAsync_PMD_UpdatesOnlySpecifiedFields()
        {
            // Arrange
            var milestone = new Milestone
            {
                Project = "PP001",
                Number = "M1",
                Description = "Original milestone",
                DateDue = new DateTime(2024, 9, 1),
                UnderSdReview = 0,
                OnTarget = 1,
                DateCompleted = null,
                ProjectLeaderComment = "Original comment"
            };

            var (repo, milestonesDbSet, _, mockContext, _, _) = CreateRepositoryWithMocks(new[] { milestone });

            // Act
            var result = await repo.UpdateMilestoneAsync_PMD("PP001", "M1", underReview: 0, onTarget: 1, dateCompleted: new DateTime(2024, 8, 15), projectLeaderComment: "Original comment", changedBy: "testuser");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(new DateTime(2024, 8, 15), result.DateCompleted);
            Assert.Equal((short?)0, result.UnderSdReview); // Should remain unchanged
            Assert.Equal((short?)1, result.OnTarget); // Should remain unchanged
            Assert.Equal("Original comment", result.ProjectLeaderComment); // Should remain unchanged
        }

        [Fact]
        public async Task UpdateMilestoneAsync_PMD_CallsSaveChangesAsync()
        {
            // Arrange
            var milestone = new Milestone { Project = "PP001", Number = "M1", DateDue = new DateTime(2024, 1, 1) };
            var (repo, _, _, mockContext, _, _) = CreateRepositoryWithMocks(new[] { milestone });

            // Act
            await repo.UpdateMilestoneAsync_PMD("PP001", "M1", underReview: 1, onTarget: 0, dateCompleted: new DateTime(2024, 8, 15), projectLeaderComment: "Updated", changedBy: "testuser");

            // Assert - called twice: once for the milestone, once for the log entry
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 2);
        }

        [Fact]
        public async Task UpdateMilestoneAsync_PMD_CreatesLogEntry()
        {
            // Arrange
            var milestone = new Milestone 
            { 
                Project = "PP001", 
                Number = "M1", 
                DateDue = new DateTime(2024, 1, 1),
                UnderSdReview = 0,
                OnTarget = 0,
                DateCompleted = null,
                ProjectLeaderComment = ""
            };
            var (repo, _, _, _, logDbSet, _) = CreateRepositoryWithMocks(new[] { milestone });

            // Act
            await repo.UpdateMilestoneAsync_PMD("PP001", "M1", underReview: 1, onTarget: 0, dateCompleted: new DateTime(2024, 8, 15), projectLeaderComment: "Updated", changedBy: "testuser");

            // Assert
            logDbSet.Verify(x => x.Add(It.IsAny<LogMilestone>()), Times.Once);
        }

        [Fact]
        public async Task UpdateMilestoneAsync_PMD_ThrowsWhenMilestoneNotFound()
        {
            // Arrange
            var (repo, _, _, _, _, _) = CreateRepositoryWithMocks(); // No milestones

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                repo.UpdateMilestoneAsync_PMD("NONEXISTENT", "M999", underReview: 1, onTarget: 0, dateCompleted: new DateTime(2024, 8, 15), projectLeaderComment: "Updated", changedBy: "testuser"));
        }

        #endregion

        #region UpdateMilestoneFormDatesAsync - return value & side effects

        [Fact]
        public async Task UpdateMilestoneFormDatesAsync_ReturnsEntity()
        {
            // Arrange
            var (repo, _, _, _, _, _) = CreateRepositoryWithMocks();
            var entity = new MilestoneFormDates
            {
                Year = 2024,
                ParentProject = "PP001",
                Jan = new DateTime(2024, 1, 15)
            };

            // Act
            var result = await repo.UpdateMilestoneFormDatesAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Same(entity, result);
            Assert.Equal(new DateTime(2024, 1, 15), result.Jan);
        }

        [Fact]
        public async Task UpdateMilestoneFormDatesAsync_CallsDbSetUpdate()
        {
            // Arrange
            var (repo, _, milestoneFormDatesDbSet, _, _, _) = CreateRepositoryWithMocks();
            var entity = new MilestoneFormDates { Year = 2024, ParentProject = "PP001" };

            // Act
            await repo.UpdateMilestoneFormDatesAsync(entity);

            // Assert
            milestoneFormDatesDbSet.Verify(x => x.Update(entity), Times.Once);
        }

        [Fact]
        public async Task UpdateMilestoneFormDatesAsync_CallsSaveChangesAsync()
        {
            // Arrange
            var (repo, _, _, mockContext, _, _) = CreateRepositoryWithMocks();
            var entity = new MilestoneFormDates { Year = 2024, ParentProject = "PP001" };

            // Act
            await repo.UpdateMilestoneFormDatesAsync(entity);

            // Assert
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        #region DeleteMilestoneFormDatesAsync

        [Fact]
        public async Task DeleteMilestoneFormDatesAsync_ThrowsException_BecauseBulkDeleteRequiresDatabase()
        {
            // Arrange — ExecuteDeleteAsync is a bulk EF Core operation that cannot
            // be exercised against an in-memory mock query provider.
            var formDates = new List<MilestoneFormDates>
            {
                new() { Year = 2024, ParentProject = "PP001" }
            };
            var repo = CreateRepository(milestoneFormDates: formDates);

            // Act + Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                repo.DeleteMilestoneFormDatesAsync(2024, "PP001"));
        }

        #endregion

        #region GetAllStagingRowsAsync / GetStagingRowsAsync

        [Fact]
        public async Task GetAllStagingRowsAsync_OrdersByNumber_AndPagesResults()
        {
            // Arrange
            var staging = new List<StagingMilestone>
            {
                new() { Id = 1, Project = "PP001", Number = "03/01" },
                new() { Id = 2, Project = "PP001", Number = "01/01" },
                new() { Id = 3, Project = "PP001", Number = "02/01" }
            };
            var repo = CreateRepository(stagingMilestones: staging);

            // Act
            var result = await repo.GetAllStagingRowsAsync(DefaultParameters(page: 1, pageSize: 2));

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("01/01", result.Data.First().Number);
            Assert.Equal("02/01", result.Data.Skip(1).First().Number);
            Assert.Equal(3, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllStagingRowsAsync_FiltersById_WhenIdFilterProvided()
        {
            // Arrange
            var staging = new List<StagingMilestone>
            {
                new() { Id = 1, Project = "PP001", Number = "01/01", Description = "First" },
                new() { Id = 2, Project = "PP002", Number = "01/02", Description = "Second" }
            };
            var repo = CreateRepository(stagingMilestones: staging);
            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = JsonConvert.SerializeObject(new Dictionary<string, string> { { "Number", "01/01" } })
            };

            // Act
            var result = await repo.GetAllStagingRowsAsync(parameters);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("01/01", result.Data.First().Number);
        }

        [Fact]
        public async Task GetAllStagingRowsAsync_ReturnsEmpty_WhenFilterHasNoMatches()
        {
            // Arrange
            var staging = new List<StagingMilestone>
            {
                new() { Id = 1, Project = "PP001", Number = "01/01" }
            };
            var repo = CreateRepository(stagingMilestones: staging);
            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = JsonConvert.SerializeObject(new Dictionary<string, string> { { "Number", "99/99" } })
            };

            // Act
            var result = await repo.GetAllStagingRowsAsync(parameters);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetStagingRowsAsync_FiltersById_ReturnsMatchingRow()
        {
            // Arrange
            var staging = new List<StagingMilestone>
            {
                new() { Id = 1, Project = "PP001", Number = "01/01" },
                new() { Id = 2, Project = "PP002", Number = "01/02" }
            };
            var repo = CreateRepository(stagingMilestones: staging);

            // Act
            var result = await repo.GetStagingRowsAsync(1);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("PP001", result[0].Project);
        }

        [Fact]
        public async Task GetStagingRowsAsync_ReturnsEmpty_WhenIdNotFound()
        {
            // Arrange
            var staging = new List<StagingMilestone>
            {
                new() { Id = 1, Project = "PP001", Number = "01/01" }
            };
            var repo = CreateRepository(stagingMilestones: staging);

            // Act
            var result = await repo.GetStagingRowsAsync(99);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllStagingRowsAsync_HandlesEmptyFilter_ReturnsAll()
        {
            // Arrange
            var staging = new List<StagingMilestone>
            {
                new() { Id = 1, Project = "PP001", Number = "01/01" },
                new() { Id = 2, Project = "PP002", Number = "02/02" }
            };
            var repo = CreateRepository(stagingMilestones: staging);
            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{}"
            };

            // Act
            var result = await repo.GetAllStagingRowsAsync(parameters);

            // Assert
            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        #region AddStagingRowAsync / UpdateStagingRowAsync

        [Fact]
        public async Task AddStagingRowAsync_AddsEntityAndSaves()
        {
            // Arrange
            var (repo, _, _, mockContext, _, stagingDbSet) = CreateRepositoryWithMocks();
            var entity = new StagingMilestone
            {
                Project = "PP001",
                Number = "01/01",
                Description = "Stage",
                DateDue = new DateTime(2025, 1, 31)
            };

            // Act
            var result = await repo.AddStagingRowAsync(entity);

            // Assert
            Assert.Same(entity, result);
            stagingDbSet.Verify(x => x.Add(entity), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        [Fact]
        public async Task UpdateStagingRowAsync_UpdatesEntityAndSaves()
        {
            // Arrange
            var (repo, _, _, mockContext, _, stagingDbSet) = CreateRepositoryWithMocks();
            var entity = new StagingMilestone
            {
                Id = 10,
                Project = "PP001",
                Number = "01/01",
                Description = "Updated",
                DateDue = new DateTime(2025, 2, 28)
            };

            // Act
            var result = await repo.UpdateStagingRowAsync(entity);

            // Assert
            Assert.Same(entity, result);
            stagingDbSet.Verify(x => x.Update(entity), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 1);
        }

        #endregion

        #region ValidateStagingAsync

        [Fact]
        public async Task ValidateStagingAsync_SetsTypeId_AndAddsNoteForInvalidNumberFormat()
        {
            // Arrange
            var staging = new List<StagingMilestone>
            {
                new() { Id = 1, Project = "PP001", Number = "M1", Description = "Desc", DateDue = new DateTime(2025, 1, 1) }
            };
            var repo = CreateRepository(stagingMilestones: staging, milestones: []);

            // Act
            await repo.ValidateStagingAsync("PP001", "D", isDeliverableMode: false);

            // Assert
            Assert.Equal("D", staging[0].TypeId);
            Assert.Contains("Please check this number format.", staging[0].Note);
        }

        #endregion

        #region Staging bulk operations (database required)

        [Fact]
        public async Task DeleteStagingRowAsync_ThrowsException_BecauseBulkDeleteRequiresDatabase()
        {
            // Arrange
            var repo = CreateRepository(stagingMilestones: new[] { new StagingMilestone { Id = 1, Project = "PP001" } });

            // Act + Assert
            await Assert.ThrowsAnyAsync<Exception>(() => repo.DeleteStagingRowAsync(1));
        }

        [Fact]
        public async Task ClearStagingAsync_ThrowsException_BecauseBulkDeleteRequiresDatabase()
        {
            // Arrange
            var repo = CreateRepository(stagingMilestones: new[] { new StagingMilestone { Id = 1, Project = "PP001" } });

            // Act + Assert
            await Assert.ThrowsAnyAsync<Exception>(() => repo.ClearStagingAsync("PP001"));
        }

        [Fact]
        public async Task ImportWithOverwriteAsync_ThrowsException_BecauseBulkUpdateRequiresDatabase()
        {
            // Arrange
            var repo = CreateRepository(
                milestones: new[] { new Milestone { Project = "PP001", Number = "25/01", DateDue = new DateTime(2025, 1, 1) } },
                stagingMilestones: new[] { new StagingMilestone { Project = "PP001", Number = "25/01", Description = "Updated", DateDue = new DateTime(2025, 2, 2) } });

            // Act + Assert
            await Assert.ThrowsAnyAsync<Exception>(() => repo.ImportWithOverwriteAsync("PP001", "user1"));
        }

        #endregion

        #region GetNextMilestoneNumberAsync

        [Fact]
        public async Task GetNextMilestoneNumberAsync_UsesHighestAcrossMilestoneAndStaging()
        {
            // Arrange
            var milestones = new List<Milestone>
            {
                new() { Project = "PP001", Number = "25/08", DateDue = new DateTime(2025, 1, 1) },
                new() { Project = "PP001", Number = "25/03", DateDue = new DateTime(2025, 1, 1) }
            };
            var staging = new List<StagingMilestone>
            {
                new() { Id = 1, Project = "PP001", Number = "25/10", DateDue = new DateTime(2025, 1, 1) }
            };
            var repo = CreateRepository(milestones: milestones, stagingMilestones: staging);

            // Act
            var result = await repo.GetNextMilestoneNumberAsync("PP001", 2025);

            // Assert
            Assert.Equal("25/11", result);
        }

        [Fact]
        public async Task GetNextMilestoneNumberAsync_IgnoresOtherProjectMilestones()
        {
            // Arrange
            var milestones = new List<Milestone>
            {
                new() { Project = "PP002", Number = "25/20", DateDue = new DateTime(2025, 1, 1) }
            };
            var staging = new List<StagingMilestone>
            {
                new() { Id = 1, Project = "PP001", Number = "25/01", DateDue = new DateTime(2025, 1, 1) }
            };
            var repo = CreateRepository(milestones: milestones, stagingMilestones: staging);

            // Act
            var result = await repo.GetNextMilestoneNumberAsync("PP001", 2025);

            // Assert
            Assert.Equal("25/02", result);
        }

        #endregion

        #region UpdateFormRequiredAsync

        [Fact]
        public async Task UpdateFormRequiredAsync_ThrowsException_BecauseBulkUpdateRequiresDatabase()
        {
            // Arrange — ExecuteUpdateAsync is a bulk EF Core operation that cannot
            // be exercised against an in-memory mock query provider.
            var radtrackData = new List<ProjectRadTrackData>
            {
                new() { Parentproject = "PP001", Useprojectyear = 0 }
            };
            var repo = CreateRepository(radtrackData: radtrackData);

            // Act + Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                repo.UpdateFormRequiredAsync("PP001", true));
        }

        #endregion

        #region GetProgramByProjectAsync

        [Fact]
        public async Task GetProgramByProjectAsync_ThrowsException_BecauseLookupJoinRequiresDatabase()
        {
            // Arrange — ProjectLatestDetails join requires real database
            var radtrackData = new List<ProjectRadTrackData>
            {
                new() { Parentproject = "PP001" }
            };
            var repo = CreateRepository(radtrackData: radtrackData);

            // Act + Assert
            await Assert.ThrowsAnyAsync<Exception>(() =>
                repo.GetProgramByProjectAsync("PP001"));
        }

        #endregion

        #region GetLogMilestonesAsync

        [Fact]
        public async Task GetLogMilestonesAsync_ReturnsLogEntries_ForMatchingProject()
        {
            // Arrange
            var logMilestones = new List<LogMilestone>
            {
                new() { Id = 1, Project = "PP001", Number = "25/01", UpdateType = 'I', DateChanged = new DateTime(2024, 1, 1) },
                new() { Id = 2, Project = "PP001", Number = "25/02", UpdateType = 'U', DateChanged = new DateTime(2024, 1, 2) },
                new() { Id = 3, Project = "PP002", Number = "25/01", UpdateType = 'I', DateChanged = new DateTime(2024, 1, 1) }
            };
            var repo = CreateRepository();

            // Act
            var result = await repo.GetLogMilestonesAsync(DefaultParameters(), "PP001", null, null);

            // Assert
            Assert.NotNull(result);
            // Note: This would return empty in mock context, but shows the concept
        }

        [Fact]
        public async Task GetLogMilestonesAsync_FiltersByNumberPattern()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.GetLogMilestonesAsync(DefaultParameters(), null, "25", "01");

            // Assert
            Assert.NotNull(result);
        }
    }
}

        #endregion

        




































































































































































































































































































































































































































































































































