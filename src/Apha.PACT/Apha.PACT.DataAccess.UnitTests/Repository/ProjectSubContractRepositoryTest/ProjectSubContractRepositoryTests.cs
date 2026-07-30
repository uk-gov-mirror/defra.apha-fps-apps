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

namespace Apha.PACT.DataAccess.UnitTests.Repository.ProjectSubContractRepositoryTest
{
    public class ProjectSubContractRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a ProjectSubContractRepository alongside mocked DbSet and context for call verification.
        /// AddAsync is set up explicitly since it differs from the base SetupDbSetOperations.
        /// UpdateAsync uses Entry().State — tested via Callback+Throws pattern (mirrors JobCodeRepositoryTests).
        /// </summary>
        private static (
            ProjectSubContractRepository Repo,
            Mock<DbSet<ProjectSubContract>> SubContractsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<ProjectSubContract> subContracts,
                int fpsYear = DefaultTestFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var subContractsMockSet = RepositoryTestHelper.CreateMockDbSet(subContracts);

            RepositoryTestHelper.SetupDbSetOperations(subContractsMockSet);
            subContractsMockSet
                .Setup(x => x.AddAsync(It.IsAny<ProjectSubContract>(), It.IsAny<CancellationToken>()))
                .Returns((ProjectSubContract _, CancellationToken __) => new ValueTask<EntityEntry<ProjectSubContract>>());
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.ProjectSubContracts).Returns(subContractsMockSet.Object);

            var repo = new ProjectSubContractRepository(mockContext.Object, fpsRequestContext);
            return (repo, subContractsMockSet, mockContext);
        }

        private static ProjectSubContractRepository CreateRepository(
            IEnumerable<ProjectSubContract> subContracts,
            int fpsYear = DefaultTestFpsYear)
            => CreateRepositoryWithMocks(subContracts, fpsYear).Repo;

        private static ProjectSubContractRepository CreateRepositoryWithMonthlySummary(
            IEnumerable<MonthlySubContractsSummary> summaryData,
            int fpsYear = DefaultTestFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var subContractsMockSet = RepositoryTestHelper.CreateMockDbSet<ProjectSubContract>([]);
            mockContext.Setup(x => x.ProjectSubContracts).Returns(subContractsMockSet.Object);

            var summaryMockSet = RepositoryTestHelper.CreateMockDbSet(summaryData);
            mockContext.Setup(x => x.MonthlySubContractsSummary).Returns(summaryMockSet.Object);

            return new ProjectSubContractRepository(mockContext.Object, fpsRequestContext);
        }

        private static MonthlySubContractsSummary MakeMonthlySummary(string program, string parentProject, int month, decimal? amount = null)
            => new() { FpsYear = DefaultTestFpsYear, Program = program, ParentProject = parentProject, Month = month, MonthlyAmount = amount };

        private static (
            ProjectSubContractRepository Repo,
            Mock<DbSet<ProjectSubcontractStaging>> StagingDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithStaging(
                IEnumerable<ProjectSubcontractStaging> stagingRows,
                int fpsYear = DefaultTestFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var stagingMockSet = RepositoryTestHelper.CreateMockDbSet(stagingRows);
            var subContractsMockSet = RepositoryTestHelper.CreateMockDbSet<ProjectSubContract>([]);

            RepositoryTestHelper.SetupDbSetOperations(stagingMockSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            stagingMockSet
                .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProjectSubcontractStaging>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            subContractsMockSet
                .Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<ProjectSubContract>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            mockContext.Setup(x => x.ProjectSubcontractStagings).Returns(stagingMockSet.Object);
            mockContext.Setup(x => x.ProjectSubContracts).Returns(subContractsMockSet.Object);

            var repo = new ProjectSubContractRepository(mockContext.Object, fpsRequestContext);
            return (repo, stagingMockSet, mockContext);
        }

        #region GetPagedProjectSubContractsAsync

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_WithProject_ReturnsFilteredPagedResult()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 2, Project = "PRJ2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);
            var query = new PaginationParameters<string>();

            var result = await repo.GetPagedProjectSubContractsAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal(1, result.Data.First().SubContCounter);
            Assert.Equal(1, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProjectSubContractsAsync_NullProject_ReturnsAllRecordsPaged()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 2, Project = "PRJ2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);
            var query = new PaginationParameters<string>();

            var result = await repo.GetPagedProjectSubContractsAsync(query, null);

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetTotalAmountAsync

        [Fact]
        public async Task GetTotalAmountAsync_WithMatchingProject_ReturnsSumOfAmounts()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", Amount = 800m,  FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 2, Project = "PRJ1", Amount = 200m,  FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 3, Project = "PRJ2", Amount = 1000m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);

            var result = await repo.GetTotalAmountAsync("PRJ1");

            Assert.Equal(1000m, result);
        }

        [Fact]
        public async Task GetTotalAmountAsync_NullProject_ReturnsTotalOfAllAmounts()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", Amount = 500m, FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 2, Project = "PRJ2", Amount = 300m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);

            var result = await repo.GetTotalAmountAsync(null);

            Assert.Equal(800m, result);
        }

        [Fact]
        public async Task GetTotalAmountAsync_NoMatchingRecords_ReturnsZero()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetTotalAmountAsync("PRJ_NONE");

            Assert.Equal(0m, result);
        }

        #endregion

        #region GetFpsProjectSubContractsAsync

        [Fact]
        public async Task GetFpsProjectSubContractsAsync_WithProject_ReturnsOnlyAnimalRecordsForProject()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", AcctCode = "LargeAnimals", FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 2, Project = "PRJ1", AcctCode = "SmallAnimals", FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 3, Project = "PRJ1", AcctCode = "SubContract",  FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 4, Project = "PRJ2", AcctCode = "LargeAnimals", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);
            var query = new PaginationParameters<string>();

            var result = await repo.GetFpsProjectSubContractsAsync(query, "PRJ1", filterByAnimalAcctCodes: true);

            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, d => Assert.Equal("PRJ1", d.Project));
        }

        [Fact]
        public async Task GetFpsProjectSubContractsAsync_FilterByAnimalFalse_WithProject_ReturnsNonAnimalRecordsForProject()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", AcctCode = "LargeAnimals", FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 2, Project = "PRJ1", AcctCode = "SubContract",  FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 3, Project = "PRJ1", AcctCode = "Consumables",  FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 4, Project = "PRJ2", AcctCode = "SubContract",  FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);
            var query = new PaginationParameters<string>();

            var result = await repo.GetFpsProjectSubContractsAsync(query, "PRJ1", filterByAnimalAcctCodes: false);

            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, d => Assert.Equal("PRJ1", d.Project));
            Assert.DoesNotContain(result.Data, d => d.AcctCode == "LargeAnimals");
        }

        [Fact]
        public async Task GetFpsProjectSubContractsAsync_NullProject_ReturnsAllAnimalRecords()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", AcctCode = "LargeAnimals", FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 2, Project = "PRJ2", AcctCode = "Mice",         FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 3, Project = "PRJ1", AcctCode = "SubContract",  FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);
            var query = new PaginationParameters<string>();

            var result = await repo.GetFpsProjectSubContractsAsync(query, null, filterByAnimalAcctCodes: true);

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetFpsProjectSubContractsAsync_FilterByAnimalFalse_NullProject_ReturnsAllNonAnimalRecords()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", AcctCode = "LargeAnimals", FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 2, Project = "PRJ2", AcctCode = "Mice",         FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 3, Project = "PRJ1", AcctCode = "SubContract",  FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 4, Project = "PRJ2", AcctCode = "Consumables",  FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);
            var query = new PaginationParameters<string>();

            var result = await repo.GetFpsProjectSubContractsAsync(query, null, filterByAnimalAcctCodes: false);

            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.DoesNotContain(result.Data, d => d.AcctCode == "LargeAnimals" || d.AcctCode == "Mice");
        }

        [Fact]
        public async Task GetFpsProjectSubContractsAsync_NoAnimalRecords_ReturnsEmpty()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", AcctCode = "SubContract", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);
            var query = new PaginationParameters<string>();

            var result = await repo.GetFpsProjectSubContractsAsync(query, null, filterByAnimalAcctCodes: true);

            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetFpsProjectSubContractTotalAmountAsync

        [Fact]
        public async Task GetFpsProjectSubContractTotalAmountAsync_WithProject_ReturnsSumOfAnimalAmounts()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", AcctCode = "LargeAnimals", Amount = 400m,  FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 2, Project = "PRJ1", AcctCode = "SmallAnimals", Amount = 100m,  FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 3, Project = "PRJ1", AcctCode = "SubContract",  Amount = 999m,  FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 4, Project = "PRJ2", AcctCode = "LargeAnimals", Amount = 600m,  FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);

            var result = await repo.GetFpsProjectSubContractTotalAmountAsync("PRJ1", filterByAnimalAcctCodes: true);

            Assert.Equal(500m, result);
        }

        [Fact]
        public async Task GetFpsProjectSubContractTotalAmountAsync_FilterByAnimalFalse_WithProject_ReturnsSumOfNonAnimalAmounts()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", AcctCode = "LargeAnimals", Amount = 400m, FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 2, Project = "PRJ1", AcctCode = "SubContract",  Amount = 300m, FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 3, Project = "PRJ1", AcctCode = "Consumables",  Amount = 200m, FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 4, Project = "PRJ2", AcctCode = "SubContract",  Amount = 999m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);

            var result = await repo.GetFpsProjectSubContractTotalAmountAsync("PRJ1", filterByAnimalAcctCodes: false);

            Assert.Equal(500m, result);
        }

        [Fact]
        public async Task GetFpsProjectSubContractTotalAmountAsync_NullProject_ReturnsTotalOfAllAnimalAmounts()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", AcctCode = "LargeAnimals", Amount = 300m, FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 2, Project = "PRJ2", AcctCode = "Mice",         Amount = 200m, FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 3, Project = "PRJ1", AcctCode = "SubContract",  Amount = 999m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);

            var result = await repo.GetFpsProjectSubContractTotalAmountAsync(null, filterByAnimalAcctCodes: true);

            Assert.Equal(500m, result);
        }

        [Fact]
        public async Task GetFpsProjectSubContractTotalAmountAsync_FilterByAnimalFalse_NullProject_ReturnsTotalOfAllNonAnimalAmounts()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", AcctCode = "LargeAnimals", Amount = 300m, FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 2, Project = "PRJ2", AcctCode = "Mice",         Amount = 200m, FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 3, Project = "PRJ1", AcctCode = "SubContract",  Amount = 400m, FpsYear = DefaultTestFpsYear },
                new() { SubContCounter = 4, Project = "PRJ2", AcctCode = "Consumables",  Amount = 150m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);

            var result = await repo.GetFpsProjectSubContractTotalAmountAsync(null, filterByAnimalAcctCodes: false);

            Assert.Equal(550m, result);
        }

        [Fact]
        public async Task GetFpsProjectSubContractTotalAmountAsync_NoAnimalRecords_ReturnsZero()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetFpsProjectSubContractTotalAmountAsync(null, filterByAnimalAcctCodes: true);

            Assert.Equal(0m, result);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ExistingId_ReturnsSubContract()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, Project = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(subContracts);

            var result = await repo.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.SubContCounter);
        }

        [Fact]
        public async Task GetByIdAsync_NonExistentId_ReturnsNull()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetByIdAsync(99);

            Assert.Null(result);
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ValidEntity_SetsFpsYearAndSaves()
        {
            var (repo, subContractsMockSet, mockContext) = CreateRepositoryWithMocks([]);
            var entity = new ProjectSubContract { Project = "PRJ1", Amount = 500m };

            var result = await repo.CreateAsync(entity);

            Assert.NotNull(result);
            Assert.Equal(DefaultTestFpsYear, result.FpsYear);
            subContractsMockSet.Verify(x => x.AddAsync(It.IsAny<ProjectSubContract>(), It.IsAny<CancellationToken>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task CreateAsync_SetsFpsYear_FromYearContext()
        {
            const int customYear = 2025;
            var (repo, _, _) = CreateRepositoryWithMocks([], fpsYear: customYear);
            var entity = new ProjectSubContract { Project = "PRJ1" };

            var result = await repo.CreateAsync(entity);

            Assert.Equal(customYear, result.FpsYear);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ValidEntity_SetsFpsYearBeforeEntryIsCalled()
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var subContractsMockSet = RepositoryTestHelper.CreateMockDbSet<ProjectSubContract>([]);
            mockContext.Setup(x => x.ProjectSubContracts).Returns(subContractsMockSet.Object);

            var entryWasCalled = false;
            mockContext.Setup(x => x.Entry(It.IsAny<ProjectSubContract>()))
                .Callback(() => entryWasCalled = true)
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            var repo = new ProjectSubContractRepository(mockContext.Object, fpsRequestContext);
            var entity = new ProjectSubContract { SubContCounter = 1, Project = "PRJ1" };

            await Assert.ThrowsAsync<NotSupportedException>(() => repo.UpdateAsync(entity));

            Assert.Equal(DefaultTestFpsYear, entity.FpsYear);
            Assert.True(entryWasCalled);
        }

        [Fact]
        public async Task UpdateAsync_SetsFpsYear_FromYearContext()
        {
            const int customYear = 2025;
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(customYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var subContractsMockSet = RepositoryTestHelper.CreateMockDbSet<ProjectSubContract>([]);
            mockContext.Setup(x => x.ProjectSubContracts).Returns(subContractsMockSet.Object);

            mockContext.Setup(x => x.Entry(It.IsAny<ProjectSubContract>()))
                .Throws(new NotSupportedException("Entry() is not supported in mocked DbContext"));

            var repo = new ProjectSubContractRepository(mockContext.Object, fpsRequestContext);
            var entity = new ProjectSubContract { SubContCounter = 1 };

            await Assert.ThrowsAsync<NotSupportedException>(() => repo.UpdateAsync(entity));

            Assert.Equal(customYear, entity.FpsYear);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingId_RemovesAndReturnsTrue()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, FpsYear = DefaultTestFpsYear }
            };
            var (repo, subContractsMockSet, mockContext) = CreateRepositoryWithMocks(subContracts);

            var result = await repo.DeleteAsync(1);

            Assert.True(result);
            RepositoryTestHelper.VerifyRemove(subContractsMockSet);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentId_ReturnsFalse()
        {
            var repo = CreateRepository([]);

            var result = await repo.DeleteAsync(99);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WrongFpsYear_ReturnsFalse()
        {
            var subContracts = new List<ProjectSubContract>
            {
                new() { SubContCounter = 1, FpsYear = 2020 }
            };
            var repo = CreateRepository(subContracts, fpsYear: DefaultTestFpsYear);

            var result = await repo.DeleteAsync(1);

            Assert.False(result);
        }

        #endregion

        #region GetMonthlySubContractsSummaryAsync

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_NoFilter_ReturnsAllRowsOrderedByProgramParentProjectMonth()
        {
            var repo = CreateRepositoryWithMonthlySummary(
            [
                MakeMonthlySummary("BETA",  "ZZ",  2, 200m),
                MakeMonthlySummary("ADMIN", "AH",  3, 300m),
                MakeMonthlySummary("ADMIN", "AH",  1, 100m)
            ]);
            var parameters = new PaginationParameters<string>();

            var result = await repo.GetMonthlySubContractsSummaryAsync(parameters);

            Assert.Equal(3, result.Count);
            Assert.Equal("ADMIN", result[0].Program); Assert.Equal(1, result[0].Month);
            Assert.Equal("ADMIN", result[1].Program); Assert.Equal(3, result[1].Month);
            Assert.Equal("BETA",  result[2].Program);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_NoData_ReturnsEmptyList()
        {
            var repo = CreateRepositoryWithMonthlySummary([]);
            var parameters = new PaginationParameters<string>();

            var result = await repo.GetMonthlySubContractsSummaryAsync(parameters);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_EmptyFilter_ReturnsAllRows()
        {
            var repo = CreateRepositoryWithMonthlySummary(
            [
                MakeMonthlySummary("ADMIN", "AH", 1, 100m),
                MakeMonthlySummary("BETA",  "ZZ", 2, 200m)
            ]);
            var parameters = new PaginationParameters<string> { Filter = "" };

            var result = await repo.GetMonthlySubContractsSummaryAsync(parameters);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_WhitespaceFilter_ReturnsAllRows()
        {
            var repo = CreateRepositoryWithMonthlySummary(
            [
                MakeMonthlySummary("ADMIN", "AH", 1, 100m)
            ]);
            var parameters = new PaginationParameters<string> { Filter = "   " };

            var result = await repo.GetMonthlySubContractsSummaryAsync(parameters);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_FilterByProgram_ReturnsMatchingRows()
        {
            var repo = CreateRepositoryWithMonthlySummary(
            [
                MakeMonthlySummary("ADMIN", "AH", 1, 100m),
                MakeMonthlySummary("BETA",  "ZZ", 2, 200m)
            ]);
            var parameters = new PaginationParameters<string> { Filter = """{"Program":"ADMIN"}""" };

            var result = await repo.GetMonthlySubContractsSummaryAsync(parameters);

            Assert.Single(result);
            Assert.Equal("ADMIN", result[0].Program);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_FilterByProgram_PartialMatch_ReturnsContainingRows()
        {
            var repo = CreateRepositoryWithMonthlySummary(
            [
                MakeMonthlySummary("ADMIN",          "AH", 1, 100m),
                MakeMonthlySummary("ADMINISTRATION", "BX", 2, 200m),
                MakeMonthlySummary("BETA",           "CZ", 3, 300m)
            ]);
            var parameters = new PaginationParameters<string> { Filter = """{"Program":"ADMIN"}""" };

            var result = await repo.GetMonthlySubContractsSummaryAsync(parameters);

            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Contains("ADMIN", r.Program));
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_FilterByParentProject_ReturnsMatchingRows()
        {
            var repo = CreateRepositoryWithMonthlySummary(
            [
                MakeMonthlySummary("ADMIN", "AH001", 1, 100m),
                MakeMonthlySummary("ADMIN", "BX002", 2, 200m)
            ]);
            var parameters = new PaginationParameters<string> { Filter = """{"ParentProject":"AH"}""" };

            var result = await repo.GetMonthlySubContractsSummaryAsync(parameters);

            Assert.Single(result);
            Assert.Equal("AH001", result[0].ParentProject);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_FilterByBothFields_ReturnsMatchingRows()
        {
            var repo = CreateRepositoryWithMonthlySummary(
            [
                MakeMonthlySummary("ADMIN", "AH001", 1, 100m),
                MakeMonthlySummary("ADMIN", "BX002", 2, 200m),
                MakeMonthlySummary("BETA",  "AH001", 3, 300m)
            ]);
            var parameters = new PaginationParameters<string> { Filter = """{"Program":"ADMIN","ParentProject":"AH"}""" };

            var result = await repo.GetMonthlySubContractsSummaryAsync(parameters);

            Assert.Single(result);
            Assert.Equal("ADMIN", result[0].Program);
            Assert.Equal("AH001", result[0].ParentProject);
        }

        [Fact]
        public async Task GetMonthlySubContractsSummaryAsync_FilterNoMatch_ReturnsEmptyList()
        {
            var repo = CreateRepositoryWithMonthlySummary(
            [
                MakeMonthlySummary("ADMIN", "AH", 1, 100m)
            ]);
            var parameters = new PaginationParameters<string> { Filter = """{"Program":"NONEXISTENT"}""" };

            var result = await repo.GetMonthlySubContractsSummaryAsync(parameters);

            Assert.Empty(result);
        }

        #endregion

        #region FailedSubContractRms

        [Fact]
        public async Task GetValidProjectsAsync_WithMatchingFpsYear_ReturnsCaseInsensitiveHashSet()
        {
            // Arrange
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", FpsYear = DefaultTestFpsYear, ProjectTitle = "T", Program = "P", Customer = "C", ProjectStatus = "S", Disease = "D", Contract = "K", IncomeAccountCode = "I", TransferIncome = 0, CustIncome = 0, IsDefraProject = 0 },
                new() { ParentProject = "prj1", FpsYear = DefaultTestFpsYear, ProjectTitle = "T", Program = "P", Customer = "C", ProjectStatus = "S", Disease = "D", Contract = "K", IncomeAccountCode = "I", TransferIncome = 0, CustIncome = 0, IsDefraProject = 0 },
                new() { ParentProject = "PRJ2", FpsYear = 2020, ProjectTitle = "T", Program = "P", Customer = "C", ProjectStatus = "S", Disease = "D", Contract = "K", IncomeAccountCode = "I", TransferIncome = 0, CustIncome = 0, IsDefraProject = 0 }
            };

            var projectsSet = RepositoryTestHelper.CreateMockDbSet(projects);
            var subContractsMockSet = RepositoryTestHelper.CreateMockDbSet<ProjectSubContract>([]);
            mockContext.Setup(x => x.Projects).Returns(projectsSet.Object);
            mockContext.Setup(x => x.ProjectSubContracts).Returns(subContractsMockSet.Object);

            var repo = new ProjectSubContractRepository(mockContext.Object, fpsRequestContext);

            // Act
            var result = await repo.GetValidProjectsAsync();

            // Assert
            Assert.Single(result);
            Assert.Contains("PRJ1", result);
        }

        [Fact]
        public void GetCurrentFpsYear_ValidContext_ReturnsFpsYear()
        {
            // Arrange
            const int customYear = 2030;
            var repo = CreateRepository([], customYear);

            // Act
            var result = repo.GetCurrentFpsYear();

            // Assert
            Assert.Equal(customYear, result);
        }

        [Fact]
        public async Task GetFailedSubContractRmsAsync_NoFailedRows_ReturnsEmptyPagedData()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithStaging([]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "Id", Descending = false };

            // Act
            var result = await repo.GetFailedSubContractRmsAsync(query, "user1");

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
            Assert.Equal(0, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetFailedSubContractRmsAsync_WithRows_ReturnsLatestImportDateRowsOnly()
        {
            // Arrange
            var oldDate = new DateTime(2024, 4, 1, 9, 0, 0);
            var latestDate = new DateTime(2024, 4, 2, 9, 0, 0);
            var rows = new List<ProjectSubcontractStaging>
            {
                new() { Id = 1, ImportedBy = "user2", IsPassed = false, ImportedDate = oldDate, Project = "PRJ1" },
                new() { Id = 2, ImportedBy = "user2", IsPassed = false, ImportedDate = latestDate, Project = "PRJ2" },
                new() { Id = 3, ImportedBy = "user2", IsPassed = true, ImportedDate = latestDate, Project = "PRJ3" },
                new() { Id = 4, ImportedBy = "other", IsPassed = false, ImportedDate = latestDate, Project = "PRJ4" }
            };

            var (repo, _, _) = CreateRepositoryWithStaging(rows);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "Id", Descending = false };

            // Act
            var result = await repo.GetFailedSubContractRmsAsync(query, "user2");

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(2, result.Data.First().Id);
            Assert.Equal(1, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task DeleteFailedSubContractRmsByIdAsync_EntityExists_RemovesAndReturnsTrue()
        {
            // Arrange
            var rows = new List<ProjectSubcontractStaging>
            {
                new() { Id = 10, ImportedBy = "user3", IsPassed = false }
            };
            var (repo, stagingSet, context) = CreateRepositoryWithStaging(rows);

            // Act
            var result = await repo.DeleteFailedSubContractRmsByIdAsync(10, "user3");

            // Assert
            Assert.True(result);
            RepositoryTestHelper.VerifyRemove(stagingSet);
            RepositoryTestHelper.VerifySaveChanges(context);
        }

        [Fact]
        public async Task DeleteFailedSubContractRmsByIdAsync_EntityMissing_ReturnsFalse()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithStaging([]);

            // Act
            var result = await repo.DeleteFailedSubContractRmsByIdAsync(999, "user4");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ImportSubContractRmsAsync_BothListsEmpty_ReturnsZeroCounts()
        {
            // Arrange
            var (repo, _, context) = CreateRepositoryWithStaging([]);

            // Act
            var result = await repo.ImportSubContractRmsAsync([], []);

            // Assert
            Assert.Equal(0, result.PassedCount);
            Assert.Equal(0, result.FailedCount);
            RepositoryTestHelper.VerifySaveChanges(context, 0);
        }

        [Fact]
        public async Task ImportSubContractRmsAsync_WithPassedAndFailedRows_ReturnsCountsAndSaves()
        {
            // Arrange
            var (repo, _, context) = CreateRepositoryWithStaging([]);
            var passedRows = new List<ProjectSubContract>
            {
                new() { Project = "PRJ1", FpsYear = DefaultTestFpsYear }
            };
            var failedRows = new List<ProjectSubcontractStaging>
            {
                new() { Project = "PRJ2", ImportedBy = "user5", IsPassed = false }
            };

            // Act
            var result = await repo.ImportSubContractRmsAsync(passedRows, failedRows);

            // Assert
            Assert.Equal(1, result.PassedCount);
            Assert.Equal(1, result.FailedCount);
            context.Verify(x => x.ProjectSubContracts.AddRangeAsync(It.IsAny<IEnumerable<ProjectSubContract>>(), It.IsAny<CancellationToken>()), Times.Once);
            context.Verify(x => x.ProjectSubcontractStagings.AddRangeAsync(It.IsAny<IEnumerable<ProjectSubcontractStaging>>(), It.IsAny<CancellationToken>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(context);
        }

        #endregion


    }
}
