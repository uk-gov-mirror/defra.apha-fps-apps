using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.TimeCodeValidRepositoryTest
{
    public class TimeCodeValidRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        private static (
            TimeCodeValidRepository Repo,
            Mock<DbSet<TimeCodeValid>> TimeCodeValidsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<TimeCodeValid> timeCodes,
                int fpsYear = DefaultTestFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var timeCodesMockSet = RepositoryTestHelper.CreateMockDbSet(timeCodes);

            RepositoryTestHelper.SetupDbSetOperations(timeCodesMockSet);
            timeCodesMockSet
                .Setup(x => x.AddAsync(It.IsAny<TimeCodeValid>(), It.IsAny<CancellationToken>()))
                .Returns((TimeCodeValid _, CancellationToken __) => new ValueTask<EntityEntry<TimeCodeValid>>());
            timeCodesMockSet
                .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<TimeCodeValid>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            timeCodesMockSet
                .Setup(x => x.RemoveRange(It.IsAny<IEnumerable<TimeCodeValid>>()))
                .Verifiable();
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.TimeCodeValids).Returns(timeCodesMockSet.Object);

            var repo = new TimeCodeValidRepository(mockContext.Object, fpsRequestContext);
            return (repo, timeCodesMockSet, mockContext);
        }

        private static TimeCodeValidRepository CreateRepository(
            IEnumerable<TimeCodeValid> timeCodes,
            int fpsYear = DefaultTestFpsYear)
            => CreateRepositoryWithMocks(timeCodes, fpsYear).Repo;

        #region GetPagedByProjectAndTestCodeAsync

        [Fact]
        public async Task GetPagedByProjectAndTestCodeAsync_WithMatchingProjectAndTestCode_ReturnsFilteredPagedResult()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST1", FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC2", WorkGroup = "WG2", ParentProject = "PRJ1", TestCode = "TST2", FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC3", WorkGroup = "WG3", ParentProject = "PRJ2", TestCode = "TST1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(timeCodes);
            var query = new PaginationParameters<string>();

            var result = await repo.GetPagedByProjectAndTestCodeAsync(query, "PRJ1", "TST1");

            Assert.Single(result.Data);
            Assert.Equal("TC1", result.Data.First().TimeCode);
            Assert.Equal(1, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedByProjectAndTestCodeAsync_NoMatch_ReturnsEmptyPagedResult()
        {
            var repo = CreateRepository([]);
            var query = new PaginationParameters<string>();

            var result = await repo.GetPagedByProjectAndTestCodeAsync(query, "PRJ_NONE", "TST_NONE");

            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetTimeCodeValidsAsync

        [Fact]
        public async Task GetTimeCodeValidsAsync_WithMultipleRows_ReturnsAllRows()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC2", WorkGroup = "WG2", ParentProject = "PRJ2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(timeCodes);

            var result = await repo.GetTimeCodeValidsAsync();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, x => x.TimeCode == "TC1");
            Assert.Contains(result, x => x.TimeCode == "TC2");
        }

        #endregion

        #region GetTimeCodeValidsByWorkGroupAsync

        [Fact]
        public async Task GetTimeCodeValidsByWorkGroupAsync_WithMatchingWorkGroup_ReturnsFilteredOrderedRows()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC2", WorkGroup = "WG1", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC3", WorkGroup = "WG2", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(timeCodes);

            var result = (await repo.GetTimeCodeValidsByWorkGroupAsync("WG1")).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("TC1", result[0].TimeCode);
            Assert.Equal("TC2", result[1].TimeCode);
            Assert.All(result, x => Assert.Equal("WG1", x.WorkGroup));
        }

        [Fact]
        public async Task GetTimeCodeValidsByWorkGroupAsync_WithNoMatch_ReturnsEmptyList()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetTimeCodeValidsByWorkGroupAsync("WG_NONE");

            Assert.Empty(result);
        }

        #endregion

        #region GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync

        [Fact]
        public async Task GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync_WithDuplicates_ReturnsDistinctOrderedProjects()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ2", FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ2", FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC2", WorkGroup = "WG1", ParentProject = "PRJ3", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(timeCodes);

            var result = (await repo.GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1")).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("PRJ1", result[0]);
            Assert.Equal("PRJ2", result[1]);
        }

        [Fact]
        public async Task GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync_WithNoMatch_ReturnsEmptyList()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync("WG_NONE", "TC_NONE");

            Assert.Empty(result);
        }

        #endregion

        #region GetByJobCodeAsync

        [Fact]
        public async Task GetByJobCodeAsync_MatchingFilters_ReturnsFilteredList()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC2", WorkGroup = "WG2", ParentProject = "PRJ2", JobCode = "JC2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(timeCodes);

            var result = (await repo.GetByJobCodeAsync("JC1", "PRJ1")).ToList();

            Assert.Single(result);
            Assert.Equal("TC1", result[0].TimeCode);
        }

        [Fact]
        public async Task GetByJobCodeAsync_NoMatch_ReturnsEmptyList()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetByJobCodeAsync("JC_NONE", "PRJ_NONE");

            Assert.Empty(result);
        }

        #endregion

        #region GetPagedTimeCodesAsync

        [Fact]
        public async Task GetPagedTimeCodesAsync_WithJobCodeAndProject_ReturnsFilteredPagedResult()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC2", WorkGroup = "WG2", ParentProject = "PRJ2", JobCode = "JC2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(timeCodes);
            var query = new PaginationParameters<string>();

            var result = await repo.GetPagedTimeCodesAsync(query, "JC1", "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal("TC1", result.Data.First().TimeCode);
            Assert.Equal(1, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedTimeCodesAsync_NullFilters_ReturnsAllRecordsPaged()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC2", WorkGroup = "WG2", ParentProject = "PRJ2", JobCode = "JC2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(timeCodes);
            var query = new PaginationParameters<string>();

            var result = await repo.GetPagedTimeCodesAsync(query, null, null);

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetTimeCodeValidAsync

        [Fact]
        public async Task GetTimeCodeValidAsync_ExistingKey_ReturnsEntity()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(timeCodes);

            var result = await repo.GetTimeCodeValidAsync("WG1", "TC1", "PRJ1");

            Assert.NotNull(result);
            Assert.Equal("TC1", result.TimeCode);
            Assert.Equal("WG1", result.WorkGroup);
        }

        [Fact]
        public async Task GetTimeCodeValidAsync_NonExistentKey_ReturnsNull()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetTimeCodeValidAsync("WG_MISSING", "TC_MISSING", "PRJ1");

            Assert.Null(result);
        }

        #endregion

        #region CreateTimeCodeValidAsync

        [Fact]
        public async Task CreateTimeCodeValidAsync_ValidEntity_SetsFpsYearAndSaves()
        {
            var (repo, timeCodesMockSet, mockContext) = CreateRepositoryWithMocks([]);
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            var result = await repo.CreateTimeCodeValidAsync(entity);

            Assert.NotNull(result);
            Assert.Equal(DefaultTestFpsYear, result.FpsYear);
            timeCodesMockSet.Verify(x => x.AddAsync(It.IsAny<TimeCodeValid>(), It.IsAny<CancellationToken>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task CreateTimeCodeValidAsync_SetsFpsYear_FromYearContext()
        {
            const int customYear = 2025;
            var (repo, _, _) = CreateRepositoryWithMocks([], fpsYear: customYear);
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            var result = await repo.CreateTimeCodeValidAsync(entity);

            Assert.Equal(customYear, result.FpsYear);
        }

        #endregion

        #region UpdateTimeCodeValidAsync

        [Fact]
        public async Task UpdateTimeCodeValidAsync_ValidEntity_SetsFpsYearBeforeEntryIsCalled()
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var timeCodesMockSet = RepositoryTestHelper.CreateMockDbSet<TimeCodeValid>([]);
            mockContext.Setup(x => x.TimeCodeValids).Returns(timeCodesMockSet.Object);

            var entryWasCalled = false;
            mockContext.Setup(x => x.Entry(It.IsAny<TimeCodeValid>()))
                .Callback(() => entryWasCalled = true)
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            var repo = new TimeCodeValidRepository(mockContext.Object, fpsRequestContext);
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            await Assert.ThrowsAsync<NotSupportedException>(() => repo.UpdateTimeCodeValidAsync(entity));

            Assert.Equal(DefaultTestFpsYear, entity.FpsYear);
            Assert.True(entryWasCalled);
        }

        [Fact]
        public async Task UpdateTimeCodeValidAsync_SetsFpsYear_FromYearContext()
        {
            const int customYear = 2025;
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(customYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var timeCodesMockSet = RepositoryTestHelper.CreateMockDbSet<TimeCodeValid>([]);
            mockContext.Setup(x => x.TimeCodeValids).Returns(timeCodesMockSet.Object);

            mockContext.Setup(x => x.Entry(It.IsAny<TimeCodeValid>()))
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            var repo = new TimeCodeValidRepository(mockContext.Object, fpsRequestContext);
            var entity = new TimeCodeValid { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1" };

            await Assert.ThrowsAsync<NotSupportedException>(() => repo.UpdateTimeCodeValidAsync(entity));

            Assert.Equal(customYear, entity.FpsYear);
        }

        #endregion

        #region DeleteTimeCodeValidAsync

        [Fact]
        public async Task DeleteTimeCodeValidAsync_ExistingEntity_RemovesAndReturnsTrue()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var (repo, timeCodesMockSet, mockContext) = CreateRepositoryWithMocks(timeCodes);

            var result = await repo.DeleteTimeCodeValidAsync("WG1", "TC1", "PRJ1");

            Assert.True(result);
            RepositoryTestHelper.VerifyRemove(timeCodesMockSet);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteTimeCodeValidAsync_NonExistentEntity_ReturnsFalse()
        {
            var repo = CreateRepository([]);

            var result = await repo.DeleteTimeCodeValidAsync("WG_MISSING", "TC_MISSING", "PRJ1");

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteTimeCodeValidAsync_WrongFpsYear_ReturnsFalse()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", FpsYear = 2020 }
            };
            var repo = CreateRepository(timeCodes, fpsYear: DefaultTestFpsYear);

            var result = await repo.DeleteTimeCodeValidAsync("WG1", "TC1", "PRJ1");

            Assert.False(result);
        }

        #endregion

        #region DeleteAllByJobCodeAsync

        [Fact]
        public async Task DeleteAllByJobCodeAsync_WithMatchingEntities_RemovesAllAndReturnsTrue()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC2", WorkGroup = "WG2", ParentProject = "PRJ1", JobCode = "JC1", FpsYear = DefaultTestFpsYear }
            };
            var (repo, timeCodesMockSet, mockContext) = CreateRepositoryWithMocks(timeCodes);

            var result = await repo.DeleteAllByJobCodeAsync("JC1", "PRJ1");

            Assert.True(result);
            timeCodesMockSet.Verify(x => x.RemoveRange(It.IsAny<IEnumerable<TimeCodeValid>>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteAllByJobCodeAsync_NoMatchingEntities_DoesNotCallSaveChangesAndReturnsTrue()
        {
            var (repo, timeCodesMockSet, mockContext) = CreateRepositoryWithMocks([]);

            var result = await repo.DeleteAllByJobCodeAsync("JC_NONE", "PRJ_NONE");

            Assert.True(result);
            timeCodesMockSet.Verify(x => x.RemoveRange(It.IsAny<IEnumerable<TimeCodeValid>>()), Times.Never);
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 0);
        }

        #endregion

        #region CopyWorkGroupAsync

        [Fact]
        public async Task CopyWorkGroupAsync_WithSourceEntries_CreatesCopiesAndReturns()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC_SRC", Active = true, FpsYear = DefaultTestFpsYear }
            };
            var (repo, timeCodesMockSet, mockContext) = CreateRepositoryWithMocks(timeCodes);

            var result = (await repo.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1")).ToList();

            Assert.Single(result);
            Assert.Equal("JC_TGT", result[0].JobCode);
            Assert.Equal("JC_TGT", result[0].TimeCode);
            Assert.Equal("WG1", result[0].WorkGroup);
            Assert.Equal(DefaultTestFpsYear, result[0].FpsYear);
            timeCodesMockSet.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<TimeCodeValid>>(), It.IsAny<CancellationToken>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task CopyWorkGroupAsync_NoSourceEntries_ReturnsEmptyCollection()
        {
            var (repo, timeCodesMockSet, _) = CreateRepositoryWithMocks([]);

            var result = await repo.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1");

            Assert.Empty(result);
            timeCodesMockSet.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<TimeCodeValid>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CopyWorkGroupAsync_WithExistingWorkGroupInTarget_SkipsExistingAndCopiesNew()
        {
            // Arrange — WG1 exists in both source and target; WG2 exists only in source
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC_SRC", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC_SRC", Active = true,  FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC_SRC", WorkGroup = "WG2", ParentProject = "PRJ1", JobCode = "JC_SRC", Active = false, FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "JC_TGT", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC_TGT", Active = true,  FpsYear = DefaultTestFpsYear }
            };
            var (repo, timeCodesMockSet, mockContext) = CreateRepositoryWithMocks(timeCodes);

            // Act
            var result = (await repo.CopyWorkGroupAsync("JC_SRC", "JC_TGT", "PRJ1")).ToList();

            // Assert — only WG2 should be copied; WG1 skipped because it already exists in target
            Assert.Single(result);
            Assert.Equal("WG2",    result[0].WorkGroup);
            Assert.Equal("JC_TGT", result[0].JobCode);
            Assert.Equal("JC_TGT", result[0].TimeCode);
            Assert.False(result[0].Active);
            Assert.Equal(DefaultTestFpsYear, result[0].FpsYear);
            timeCodesMockSet.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<TimeCodeValid>>(), It.IsAny<CancellationToken>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        #endregion

        #region DeleteBulkAsync

        [Fact]
        public async Task DeleteBulkAsync_WithMatchingItems_RemovesExactPairsAndReturnsTrue()
        {
            // Arrange — two entities in store; only one pair passed in items
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC2", WorkGroup = "WG2", ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var (repo, timeCodesMockSet, mockContext) = CreateRepositoryWithMocks(timeCodes);
            var items = new List<(string WorkGroup, string TimeCode)> { ("WG1", "TC1") };

            // Act
            var result = await repo.DeleteBulkAsync(items, "PRJ1");

            // Assert
            Assert.True(result);
            timeCodesMockSet.Verify(x => x.RemoveRange(It.IsAny<IEnumerable<TimeCodeValid>>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteBulkAsync_WithNoMatchingItems_SkipsRemoveAndReturnsTrue()
        {
            // Arrange — store is empty so nothing can match
            var (repo, timeCodesMockSet, mockContext) = CreateRepositoryWithMocks([]);
            var items = new List<(string WorkGroup, string TimeCode)> { ("WG_NONE", "TC_NONE") };

            // Act
            var result = await repo.DeleteBulkAsync(items, "PRJ_NONE");

            // Assert — always returns true; no side-effects when nothing matches
            Assert.True(result);
            timeCodesMockSet.Verify(x => x.RemoveRange(It.IsAny<IEnumerable<TimeCodeValid>>()), Times.Never);
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 0);
        }

        [Fact]
        public async Task DeleteBulkAsync_WithWrongFpsYear_SkipsRemoveAndReturnsTrue()
        {
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", FpsYear = 2020 }
            };
            var (repo, timeCodesMockSet, mockContext) = CreateRepositoryWithMocks(timeCodes, fpsYear: DefaultTestFpsYear);
            var items = new List<(string WorkGroup, string TimeCode)> { ("WG1", "TC1") };

            // Act
            var result = await repo.DeleteBulkAsync(items, "PRJ1");

            // Assert
            Assert.True(result);
            timeCodesMockSet.Verify(x => x.RemoveRange(It.IsAny<IEnumerable<TimeCodeValid>>()), Times.Never);
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 0);
        }

        #endregion

        #region CopySelectedWorkGroupsAsync

        [Fact]
        public async Task CopySelectedWorkGroupsAsync_WithMatchingWorkGroups_CreatesCopiesAndReturns()
        {
            // Arrange — two source entries, both work groups requested
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC_SRC", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC_SRC", Active = true,  FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC_SRC", WorkGroup = "WG2", ParentProject = "PRJ1", JobCode = "JC_SRC", Active = false, FpsYear = DefaultTestFpsYear }
            };
            var (repo, timeCodesMockSet, mockContext) = CreateRepositoryWithMocks(timeCodes);
            var workGroups = new List<string> { "WG1", "WG2" };

            // Act
            var result = (await repo.CopySelectedWorkGroupsAsync(workGroups, "JC_SRC", "JC_TGT", "PRJ1")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal("JC_TGT", r.JobCode));
            Assert.All(result, r => Assert.Equal("JC_TGT", r.TimeCode));
            Assert.All(result, r => Assert.Equal("PRJ1", r.ParentProject));
            Assert.All(result, r => Assert.Equal(DefaultTestFpsYear, r.FpsYear));
            timeCodesMockSet.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<TimeCodeValid>>(), It.IsAny<CancellationToken>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task CopySelectedWorkGroupsAsync_WithNoMatchingWorkGroups_ReturnsEmptyCollection()
        {
            // Arrange — store is empty; no copies produced so AddRangeAsync must not be called
            var (repo, timeCodesMockSet, _) = CreateRepositoryWithMocks([]);
            var workGroups = new List<string> { "WG_NONE" };

            // Act
            var result = await repo.CopySelectedWorkGroupsAsync(workGroups, "JC_SRC", "JC_TGT", "PRJ1");

            // Assert
            Assert.Empty(result);
            timeCodesMockSet.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<TimeCodeValid>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CopySelectedWorkGroupsAsync_WithExistingWorkGroupInTarget_SkipsExistingAndCopiesNew()
        {
            // Arrange — WG1 already exists in target; WG2 exists only in source
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC_SRC", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC_SRC", Active = true,  FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC_SRC", WorkGroup = "WG2", ParentProject = "PRJ1", JobCode = "JC_SRC", Active = true,  FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "JC_TGT", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC_TGT", Active = true,  FpsYear = DefaultTestFpsYear }
            };
            var (repo, timeCodesMockSet, mockContext) = CreateRepositoryWithMocks(timeCodes);
            var workGroups = new List<string> { "WG1", "WG2" };

            // Act
            var result = (await repo.CopySelectedWorkGroupsAsync(workGroups, "JC_SRC", "JC_TGT", "PRJ1")).ToList();

            // Assert — only WG2 copied; WG1 skipped because it already exists in target
            Assert.Single(result);
            Assert.Equal("WG2",    result[0].WorkGroup);
            Assert.Equal("JC_TGT", result[0].JobCode);
            Assert.Equal("JC_TGT", result[0].TimeCode);
            Assert.Equal(DefaultTestFpsYear, result[0].FpsYear);
            timeCodesMockSet.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<TimeCodeValid>>(), It.IsAny<CancellationToken>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task CopySelectedWorkGroupsAsync_WithSubsetOfWorkGroups_ReturnsOnlyMatchingCopiesAndPreservesActiveFlag()
        {
            // Arrange — three source entries; only WG1 is in the requested work groups
            var timeCodes = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC_SRC", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC_SRC", Active = true,  FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC_SRC", WorkGroup = "WG2", ParentProject = "PRJ1", JobCode = "JC_SRC", Active = false, FpsYear = DefaultTestFpsYear },
                new() { TimeCode = "TC_SRC", WorkGroup = "WG3", ParentProject = "PRJ1", JobCode = "JC_SRC", Active = true,  FpsYear = DefaultTestFpsYear }
            };
            var (repo, timeCodesMockSet, mockContext) = CreateRepositoryWithMocks(timeCodes);
            var workGroups = new List<string> { "WG1" };

            // Act
            var result = (await repo.CopySelectedWorkGroupsAsync(workGroups, "JC_SRC", "JC_TGT", "PRJ1")).ToList();

            // Assert — only WG1 copied; Active flag preserved; copy fields set correctly
            Assert.Single(result);
            Assert.Equal("WG1",    result[0].WorkGroup);
            Assert.Equal("JC_TGT", result[0].JobCode);
            Assert.Equal("JC_TGT", result[0].TimeCode);
            Assert.True(result[0].Active);
            Assert.Equal(DefaultTestFpsYear, result[0].FpsYear);
            timeCodesMockSet.Verify(x => x.AddRangeAsync(It.IsAny<IEnumerable<TimeCodeValid>>(), It.IsAny<CancellationToken>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        #endregion
    }
}
