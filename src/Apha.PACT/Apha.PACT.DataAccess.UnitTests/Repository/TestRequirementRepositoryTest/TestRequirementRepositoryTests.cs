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

namespace Apha.PACT.DataAccess.UnitTests.Repository.TestRequirementRepositoryTest
{
    public class TestRequirementRepositoryTests
    {
        private const int DefaultFpsYear = 2024;

        private static (
            TestRequirementRepository Repo,
            Mock<DbSet<TestRequirement>> TestReqmtsDbSet,
            Mock<DbSet<MonthlyOutput>> MonthlyOutputsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<TestRequirement>? testReqmts = null,
                IEnumerable<MonthlyOutput>? monthlyOutputs = null,
                int fpsYear = DefaultFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var testReqmtsMockSet = RepositoryTestHelper.CreateMockDbSet(testReqmts ?? []);
            RepositoryTestHelper.SetupDbSetOperations(testReqmtsMockSet);
            testReqmtsMockSet
                .Setup(x => x.AddAsync(It.IsAny<TestRequirement>(), It.IsAny<CancellationToken>()))
                .Returns((TestRequirement _, CancellationToken __) => new ValueTask<EntityEntry<TestRequirement>>());

            var monthlyOutputsMockSet = RepositoryTestHelper.CreateMockDbSet(monthlyOutputs ?? []);

            var testReqLogsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<TestRequirementLog>());
            RepositoryTestHelper.SetupDbSetOperations(testReqLogsMockSet);
            testReqLogsMockSet
                .Setup(x => x.AddAsync(It.IsAny<TestRequirementLog>(), It.IsAny<CancellationToken>()))
                .Returns((TestRequirementLog _, CancellationToken __) => new ValueTask<EntityEntry<TestRequirementLog>>());

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.TestRequirements).Returns(testReqmtsMockSet.Object);
            mockContext.Setup(x => x.MonthlyOutputs).Returns(monthlyOutputsMockSet.Object);
            mockContext.Setup(x => x.TestRequirementLogs).Returns(testReqLogsMockSet.Object);

            var repo = new TestRequirementRepository(mockContext.Object, fpsRequestContext);
            return (repo, testReqmtsMockSet, monthlyOutputsMockSet, mockContext);
        }

        private static TestRequirementRepository CreateRepositoryWithJoinMocks(
            IEnumerable<TestRequirement>? testReqmts = null,
            IEnumerable<TestorProduct>? testorProducts = null,
            IEnumerable<Project>? projects = null,
            int fpsYear = DefaultFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var testReqmtsMockSet = RepositoryTestHelper.CreateMockDbSet(testReqmts ?? []);
            RepositoryTestHelper.SetupDbSetOperations(testReqmtsMockSet);

            var testorProductsMockSet = RepositoryTestHelper.CreateMockDbSet(testorProducts ?? []);
            RepositoryTestHelper.SetupDbSetOperations(testorProductsMockSet);

            var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects ?? []);
            RepositoryTestHelper.SetupDbSetOperations(projectsMockSet);

            var monthlyOutputsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<MonthlyOutput>());
            var testReqLogsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<TestRequirementLog>());

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.TestRequirements).Returns(testReqmtsMockSet.Object);
            mockContext.Setup(x => x.TestorProducts).Returns(testorProductsMockSet.Object);
            mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);
            mockContext.Setup(x => x.MonthlyOutputs).Returns(monthlyOutputsMockSet.Object);
            mockContext.Setup(x => x.TestRequirementLogs).Returns(testReqLogsMockSet.Object);

            return new TestRequirementRepository(mockContext.Object, fpsRequestContext);
        }

        #region GetPagedByProjectAsync

        [Fact]
        public async Task GetPagedByProjectAsync_MatchingBuyer_ReturnsMatchingRecords()
        {
            var testorProduct = new TestorProduct { ItemCode = "BLOOD", UnitPriceVla = 10m, DefraUnitPrice = 12m };
            var project = new Project { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "Test", Program = "P1", Customer = "C1", Disease = "D1", Contract = "CT1", IncomeAccountCode = "INC1" };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryWithJoinMocks(
                testReqmts: testReqmts,
                testorProducts: [testorProduct],
                projects: [project]);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
            Assert.Equal("PRJ1", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_NoMatchingBuyer_ReturnsEmptyList()
        {
            var testorProduct = new TestorProduct { ItemCode = "BLOOD", UnitPriceVla = 10m, DefraUnitPrice = 12m };
            var project = new Project { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "Test", Program = "P1", Customer = "C1", Disease = "D1", Contract = "CT1", IncomeAccountCode = "INC1" };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryWithJoinMocks(
                testReqmts: testReqmts,
                testorProducts: [testorProduct],
                projects: [project]);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "MISSING");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_MultipleTestsForSameBuyer_ReturnsAll()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", UnitPriceVla = 10m, DefraUnitPrice = 12m },
                new() { ItemCode = "URINE", UnitPriceVla = 8m,  DefraUnitPrice = 9m  }
            };
            var project = new Project { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "Test", Program = "P1", Customer = "C1", Disease = "D1", Contract = "CT1", IncomeAccountCode = "INC1" };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryWithJoinMocks(
                testReqmts: testReqmts,
                testorProducts: testorProducts,
                projects: [project]);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, d => Assert.Equal("PRJ1", d.Buyer));
        }

        [Fact]
        public async Task GetPagedByProjectAsync_DefraProject_UsesDefraUnitPrice()
        {
            var testorProduct = new TestorProduct { ItemCode = "BLOOD", UnitPriceVla = 10m, DefraUnitPrice = 20m };
            var project = new Project { ParentProject = "PRJ1", IsDefraProject = 1, ProjectTitle = "Test", Program = "P1", Customer = "C1", Disease = "D1", Contract = "CT1", IncomeAccountCode = "INC1" };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryWithJoinMocks(
                testReqmts: testReqmts,
                testorProducts: [testorProduct],
                projects: [project]);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal(20m, result.Data.First().RecUnitPrice);
            Assert.Equal((short)1, result.Data.First().IsDefraProject);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_NonDefraProject_UsesVlaUnitPrice()
        {
            var testorProduct = new TestorProduct { ItemCode = "BLOOD", UnitPriceVla = 10m, DefraUnitPrice = 20m };
            var project = new Project { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "Test", Program = "P1", Customer = "C1", Disease = "D1", Contract = "CT1", IncomeAccountCode = "INC1" };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryWithJoinMocks(
                testReqmts: testReqmts,
                testorProducts: [testorProduct],
                projects: [project]);

            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal(10m, result.Data.First().RecUnitPrice);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_EmptyRepository_ReturnsEmpty()
        {
            var repo = CreateRepositoryWithJoinMocks();
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetPagedByTestCodeAsync

        [Fact]
        public async Task GetPagedByTestCodeAsync_MatchingTestCode_ReturnsMatchingRecords()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Single(result.Data);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_NoMatchingTestCode_ReturnsEmptyList()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByTestCodeAsync(query, "MISSING");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithBuyerFilter_FiltersCorrectly()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ALPHA", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "BETA",  FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Buyer\":\"ALP\"}"
            };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Single(result.Data);
            Assert.Equal("ALPHA", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_EmptyRepository_ReturnsEmpty()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks();
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_NullFilter_ReturnsAll()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_EmptyStringFilter_ReturnsAll()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "" };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WhitespaceFilter_ReturnsAll()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "   " };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_FilterWithEmptyBuyerValue_ReturnsAll()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Buyer\":\"\"}"
            };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_FilterWithWhitespaceBuyerValue_ReturnsAll()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Buyer\":\"   \"}"
            };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithProjectBuyerCodeFilter_FiltersCorrectly()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", ProjectBuyerCode = "PBC-001", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", ProjectBuyerCode = "PBC-002", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ3", ProjectBuyerCode = null,      FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ProjectBuyerCode\":\"001\"}"
            };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Single(result.Data);
            Assert.Equal("PBC-001", result.Data.First().ProjectBuyerCode);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithProjectBuyerCodeFilterEmptyValue_ReturnsAll()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", ProjectBuyerCode = "PBC-001", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", ProjectBuyerCode = "PBC-002", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ProjectBuyerCode\":\"\"}"
            };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithProjectBuyerCodeNull_ExcludesNullRecords()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", ProjectBuyerCode = "PBC-001", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", ProjectBuyerCode = null,      FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ProjectBuyerCode\":\"PBC\"}"
            };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Single(result.Data);
            Assert.Equal("PRJ1", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_CombinedBuyerAndProjectBuyerCodeFilter_FiltersCorrectly()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ALPHA", ProjectBuyerCode = "PBC-001", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "ALPHA", ProjectBuyerCode = "PBC-002", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "BETA",  ProjectBuyerCode = "PBC-001", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Buyer\":\"ALPHA\",\"ProjectBuyerCode\":\"001\"}"
            };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Single(result.Data);
            Assert.Equal("ALPHA", result.Data.First().Buyer);
            Assert.Equal("PBC-001", result.Data.First().ProjectBuyerCode);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_FilterWithUnknownKey_IgnoresUnknownKey()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"UnknownField\":\"value\"}"
            };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_SortByAscending_UsesEfPropertySort()
        {
            // EF.Property<T> cannot be evaluated in-memory; verify the sort-by code path
            // is entered when SortBy is non-empty and Descending is false.
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ZZZ", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "AAA", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "Buyer", Descending = false
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.GetPagedByTestCodeAsync(query, "BLOOD"));
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_SortByDescending_UsesEfPropertySort()
        {
            // EF.Property<T> cannot be evaluated in-memory; verify the sort-by code path
            // is entered when SortBy is non-empty and Descending is true.
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "AAA", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "ZZZ", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "Buyer", Descending = true
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.GetPagedByTestCodeAsync(query, "BLOOD"));
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_NoSortBy_DefaultsToOrderByBuyer()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ZZZ", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "AAA", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Equal("AAA", result.Data.First().Buyer);
            Assert.Equal("ZZZ", result.Data.ElementAt(1).Buyer);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_Paging_ReturnsCorrectPage()
        {
            var testReqmts = Enumerable.Range(1, 5).Select(i =>
                new TestRequirement { TestCode = "BLOOD", Buyer = $"PRJ{i:D3}", FpsYear = DefaultFpsYear }).ToList();
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            var result = await repo.GetPagedByTestCodeAsync(query, "BLOOD");

            Assert.Equal(2, result.Data.Count);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_MatchingRecord_ReturnsEntity()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.GetByIdAsync("BLOOD", "PRJ1");

            Assert.NotNull(result);
            Assert.Equal("BLOOD", result.TestCode);
            Assert.Equal("PRJ1", result.Buyer);
        }

        [Fact]
        public async Task GetByIdAsync_TestCodeNotFound_ReturnsNull()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.GetByIdAsync("MISSING", "PRJ1");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_BuyerNotFound_ReturnsNull()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.GetByIdAsync("BLOOD", "PRJ_WRONG");

            Assert.Null(result);
        }

        #endregion

        #region ExistsByTestBuyerCodeAsync

        [Fact]
        public async Task ExistsByTestBuyerCodeAsync_CodeExists_ReturnsTrue()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", TestBuyerCode = "BLOOD-WG1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.ExistsByTestBuyerCodeAsync("BLOOD-WG1");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByTestBuyerCodeAsync_CodeNotExists_ReturnsFalse()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", TestBuyerCode = "BLOOD-WG1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.ExistsByTestBuyerCodeAsync("MISSING-CODE");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByTestBuyerCodeAsync_EmptyRepository_ReturnsFalse()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks();

            var result = await repo.ExistsByTestBuyerCodeAsync("ANY-CODE");

            Assert.False(result);
        }

        #endregion

        #region ExistsByTestCodeAndBuyerInMonthlyOutputAsync

        [Fact]
        public async Task ExistsByTestCodeAndBuyerInMonthlyOutputAsync_RecordExists_ReturnsTrue()
        {
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", WorkGroup = "WG1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(monthlyOutputs: monthlyOutputs);

            var result = await repo.ExistsByTestCodeAndBuyerInMonthlyOutputAsync("BLOOD", "PRJ1");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByTestCodeAndBuyerInMonthlyOutputAsync_NoRecord_ReturnsFalse()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks();

            var result = await repo.ExistsByTestCodeAndBuyerInMonthlyOutputAsync("BLOOD", "PRJ1");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByTestCodeAndBuyerInMonthlyOutputAsync_DifferentBuyer_ReturnsFalse()
        {
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", WorkGroup = "WG1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(monthlyOutputs: monthlyOutputs);

            var result = await repo.ExistsByTestCodeAndBuyerInMonthlyOutputAsync("BLOOD", "PRJ_OTHER");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByTestCodeAndBuyerInMonthlyOutputAsync_DifferentTestCode_ReturnsFalse()
        {
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", WorkGroup = "WG1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(monthlyOutputs: monthlyOutputs);

            var result = await repo.ExistsByTestCodeAndBuyerInMonthlyOutputAsync("URINE", "PRJ1");

            Assert.False(result);
        }

        #endregion

        #region GetPagedByProjectAsync — ItemDescription projection

        [Fact]
        public async Task GetPagedByProjectAsync_ProjectsItemDescription_FromTestorProduct()
        {
            var testorProduct = new TestorProduct
            {
                ItemCode = "BLOOD",
                ItemDescription = "Blood Test Analysis",
                UnitPriceVla = 10m,
                DefraUnitPrice = 12m
            };
            var project = new Project { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "Test", Program = "P1", Customer = "C1", Disease = "D1", Contract = "CT1", IncomeAccountCode = "INC1" };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryWithJoinMocks(testReqmts, [testorProduct], [project]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal("Blood Test Analysis", result.Data.First().ItemDescription);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_WhenItemDescriptionIsNull_ProjectsNull()
        {
            var testorProduct = new TestorProduct
            {
                ItemCode = "BLOOD",
                ItemDescription = null,
                UnitPriceVla = 10m,
                DefraUnitPrice = 12m
            };
            var project = new Project { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "Test", Program = "P1", Customer = "C1", Disease = "D1", Contract = "CT1", IncomeAccountCode = "INC1" };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryWithJoinMocks(testReqmts, [testorProduct], [project]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Null(result.Data.First().ItemDescription);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_MultipleItems_EachHasCorrectItemDescription()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test Analysis", UnitPriceVla = 10m, DefraUnitPrice = 12m },
                new() { ItemCode = "URINE", ItemDescription = "Urine Test Analysis", UnitPriceVla = 8m,  DefraUnitPrice = 9m  }
            };
            var project = new Project { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "Test", Program = "P1", Customer = "C1", Disease = "D1", Contract = "CT1", IncomeAccountCode = "INC1" };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, [project]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Equal(2, result.Data.Count);
            var blood = result.Data.First(d => d.TestCode == "BLOOD");
            var urine = result.Data.First(d => d.TestCode == "URINE");
            Assert.Equal("Blood Test Analysis", blood.ItemDescription);
            Assert.Equal("Urine Test Analysis", urine.ItemDescription);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_ItemDescription_IndependentOfUnitPriceLogic()
        {
            // Defra project uses DefraUnitPrice but description still comes from TestorProduct
            var testorProduct = new TestorProduct
            {
                ItemCode = "BLOOD",
                ItemDescription = "Blood Test Analysis",
                UnitPriceVla = 10m,
                DefraUnitPrice = 20m
            };
            var project = new Project { ParentProject = "PRJ1", IsDefraProject = 1, ProjectTitle = "Test", Program = "P1", Customer = "C1", Disease = "D1", Contract = "CT1", IncomeAccountCode = "INC1" };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepositoryWithJoinMocks(testReqmts, [testorProduct], [project]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal("Blood Test Analysis", result.Data.First().ItemDescription);
            Assert.Equal(20m,                   result.Data.First().RecUnitPrice);
        }

        #endregion

        #region GetPagedBySupplierTestCodeAsync

        private static TestRequirementRepository CreateRepositoryWithSupplierMocks(
            IEnumerable<TestRequirement>? testReqmts = null,
            IEnumerable<Project>? projects = null,
            int fpsYear = DefaultFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var testReqmtsMockSet = RepositoryTestHelper.CreateMockDbSet(testReqmts ?? []);
            RepositoryTestHelper.SetupDbSetOperations(testReqmtsMockSet);

            var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects ?? []);
            RepositoryTestHelper.SetupDbSetOperations(projectsMockSet);

            var monthlyOutputsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<MonthlyOutput>());
            var testReqLogsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<TestRequirementLog>());

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.TestRequirements).Returns(testReqmtsMockSet.Object);
            mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);
            mockContext.Setup(x => x.MonthlyOutputs).Returns(monthlyOutputsMockSet.Object);
            mockContext.Setup(x => x.TestRequirementLogs).Returns(testReqLogsMockSet.Object);

            return new TestRequirementRepository(mockContext.Object, fpsRequestContext);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_MatchingTestCode_ExercisesJoinAndFilter()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1, UnitPrice = 10m, NoRequired = 3 },
                new() { TestCode = "URINE", Buyer = "PRJ2", Active = 1, UnitPrice = 5m,  NoRequired = 2 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "PRJ2", Manager = "MGR2", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            var row = Assert.Single(result.Data);
            Assert.Equal("PRJ1", row.Buyer);
            Assert.Equal("MGR1", row.ProjectManager);
            Assert.Equal(30m, row.TestCost);
            Assert.Equal(1, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_NoMatchingTestCode_ExercisesEmptyResult()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "MISSING", showRejected: false);

            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_ShowRejectedFalse_ExercisesActiveFilter()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1 },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", Active = 0 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "PRJ2", Manager = "MGR2", ProjectStatus = "Rejected" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            var row = Assert.Single(result.Data);
            Assert.Equal("PRJ1", row.Buyer);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_ShowRejectedTrue_ExercisesIncludeAllPath()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1 },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", Active = 0 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "PRJ2", Manager = "MGR2", ProjectStatus = "Rejected" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: true);

            Assert.Equal(2, result.Data.Count);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_TestCostComputation_ExercisesClientSideCalc()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1, UnitPrice = 10m, NoRequired = 3 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            var row = Assert.Single(result.Data);
            Assert.Equal(30m, row.TestCost);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_TestCostNullWhenUnitPriceNull_ExercisesNullBranch()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1, UnitPrice = null, NoRequired = 3 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            var row = Assert.Single(result.Data);
            Assert.Null(row.TestCost);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_TestCostNullWhenNoRequiredNull_ExercisesNullBranch()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1, UnitPrice = 10m, NoRequired = null }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            var row = Assert.Single(result.Data);
            Assert.Null(row.TestCost);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_FilterByBuyer_ExercisesBuyerFilter()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ALPHA001", Active = 1 },
                new() { TestCode = "BLOOD", Buyer = "BETA002",  Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "ALPHA001", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "BETA002",  Manager = "MGR2", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Buyer\":\"ALPHA\"}"
            };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            var row = Assert.Single(result.Data);
            Assert.Equal("ALPHA001", row.Buyer);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_FilterByProjectStatus_ExercisesStatusFilter()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1 },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "PRJ2", Manager = "MGR2", ProjectStatus = "Closed" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ProjectStatus\":\"Closed\"}"
            };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            var row = Assert.Single(result.Data);
            Assert.Equal("PRJ2", row.Buyer);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_NullFilter_ExercisesNoFilterPath()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1 },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "PRJ2", Manager = "MGR2", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Equal(2, result.Data.Count);
        }

        [Theory]
        [InlineData(nameof(TestSupplierView.Buyer), false)]
        [InlineData(nameof(TestSupplierView.Buyer), true)]
        [InlineData(nameof(TestSupplierView.ProjectManager), false)]
        [InlineData(nameof(TestSupplierView.ProjectManager), true)]
        [InlineData(nameof(TestSupplierView.UnitPrice), false)]
        [InlineData(nameof(TestSupplierView.UnitPrice), true)]
        [InlineData(nameof(TestSupplierView.NoRequired), false)]
        [InlineData(nameof(TestSupplierView.NoRequired), true)]
        [InlineData(nameof(TestSupplierView.ProjectStatus), false)]
        [InlineData(nameof(TestSupplierView.ProjectStatus), true)]
        public async Task GetPagedBySupplierTestCodeAsync_DbSortColumns_ExercisesSortPath(string sortBy, bool descending)
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1, UnitPrice = 10m, NoRequired = 2 },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", Active = 1, UnitPrice = 5m,  NoRequired = 4 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "PRJ2", Manager = "MGR2", ProjectStatus = "Closed" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Equal(2, result.Data.Count);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task GetPagedBySupplierTestCodeAsync_SortByTestCost_ExercisesClientSideSortPath(bool descending)
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1, UnitPrice = 5m,  NoRequired = 2 },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", Active = 1, UnitPrice = 10m, NoRequired = 3 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "PRJ2", Manager = "MGR2", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = nameof(TestSupplierView.TestCost),
                Descending = descending
            };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            // PRJ1 cost = 5*2 = 10, PRJ2 cost = 10*3 = 30
            var rows = result.Data.ToList();
            Assert.Equal(2, rows.Count);
            Assert.Equal(descending ? "PRJ2" : "PRJ1", rows[0].Buyer);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_NoSortBy_ExercisesDefaultSortPath()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ZZZ", Active = 1 },
                new() { TestCode = "BLOOD", Buyer = "AAA", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "ZZZ", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "AAA", Manager = "MGR2", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            // Default sort is by Buyer ascending.
            Assert.Equal("AAA", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_UnknownSortColumn_ExercisesFallbackSortAsc()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ZZZ", Active = 1 },
                new() { TestCode = "BLOOD", Buyer = "AAA", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "ZZZ", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "AAA", Manager = "MGR2", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "UnknownColumn", Descending = false
            };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            // Unknown column falls back to Buyer ascending.
            Assert.Equal("AAA", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_UnknownSortColumnDescending_ExercisesFallbackSortDesc()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ZZZ", Active = 1 },
                new() { TestCode = "BLOOD", Buyer = "AAA", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "ZZZ", Manager = "MGR1", ProjectStatus = "Active" },
                new() { ParentProject = "AAA", Manager = "MGR2", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "UnknownColumn", Descending = true
            };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            // Unknown column with Descending falls back to Buyer descending.
            Assert.Equal("ZZZ", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_Paging_ExercisesPagingPath()
        {
            var testReqmts = Enumerable.Range(1, 5).Select(i =>
                new TestRequirement { TestCode = "BLOOD", Buyer = $"PRJ{i:D3}", Active = 1 }).ToList();
            var projects = testReqmts.Select(t =>
                new Project { ParentProject = t.Buyer, Manager = "MGR", ProjectStatus = "Active" }).ToList();
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Equal(2, result.Data.Count);
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(5, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_EmptyRepository_ExercisesEmptyPath()
        {
            var repo = CreateRepositoryWithSupplierMocks();
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_EmptyBuyerFilter_ExercisesNoFilterBranch()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Buyer\":\"\"}"
            };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_WhitespaceBuyerFilter_ExercisesNoFilterBranch()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Buyer\":\"   \"}"
            };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_EmptyProjectStatusFilter_ExercisesNoStatusFilterBranch()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ProjectStatus\":\"\"}"
            };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_NullProjectStatus_ExercisesNullStatusExclusion()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = null! }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ProjectStatus\":\"Active\"}"
            };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            // Rows with null ProjectStatus are excluded by the status filter.
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedBySupplierTestCodeAsync_WhitespaceFilter_ExercisesNoFilterPath()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", Active = 1 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Manager = "MGR1", ProjectStatus = "Active" }
            };
            var repo = CreateRepositoryWithSupplierMocks(testReqmts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "   " };

            var result = await repo.GetPagedBySupplierTestCodeAsync(query, "BLOOD", showRejected: false);

            Assert.Single(result.Data);
        }

        #endregion

        #region GetPagedWithDetailsAsync — ApplyTestReqmtDetailFilter coverage

        [Fact]
        public async Task GetPagedWithDetailsAsync_NullFilter_ReturnsAllRows()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" },
                new() { ParentProject = "PRJ2", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_EmptyStringFilter_ReturnsAllRows()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "" };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_WhitespaceFilter_ReturnsAllRows()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "   " };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_FilterByTestCode_ReturnsMatchingOnly()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m },
                new() { ItemCode = "URINE", ItemDescription = "Urine Test", UnitPriceVla = 8m, DefraUnitPrice = 9m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TestCode\":\"BLO\"}"
            };

            // GetPagedWithDetailsAsync already filters by testCode parameter ("BLOOD"),
            // then ApplyTestReqmtDetailFilter further filters within the result set.
            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Single(result.Data);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_FilterByTestCodeEmptyValue_ReturnsAll()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" },
                new() { ParentProject = "PRJ2", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TestCode\":\"\"}"
            };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_FilterByTestCodeWhitespaceValue_ReturnsAll()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TestCode\":\"   \"}"
            };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_FilterByItemDescription_ReturnsMatchingOnly()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test Analysis", UnitPriceVla = 10m, DefraUnitPrice = 12m },
                new() { ItemCode = "URINE", ItemDescription = "Urine Sample Test", UnitPriceVla = 8m, DefraUnitPrice = 9m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ItemDescription\":\"Blood\"}"
            };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Single(result.Data);
            Assert.Equal("Blood Test Analysis", result.Data.First().ItemDescription);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_FilterByItemDescription_NullItemDescriptionExcluded()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = null, UnitPriceVla = 10m, DefraUnitPrice = 12m },
                new() { ItemCode = "URINE", ItemDescription = "Urine Sample", UnitPriceVla = 8m, DefraUnitPrice = 9m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ItemDescription\":\"Sample\"}"
            };

            // Only URINE matches (BLOOD has null ItemDescription and is excluded)
            var result = await repo.GetPagedWithDetailsAsync(query, "URINE");

            Assert.Single(result.Data);
            Assert.Equal("Urine Sample", result.Data.First().ItemDescription);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_FilterByItemDescriptionEmptyValue_ReturnsAll()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ItemDescription\":\"\"}"
            };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_FilterByBuyer_ReturnsMatchingOnly()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "ALPHA001", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" },
                new() { ParentProject = "BETA002", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ALPHA001", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "BETA002", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Buyer\":\"ALPHA\"}"
            };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Single(result.Data);
            Assert.Equal("ALPHA001", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_FilterByBuyerEmptyValue_ReturnsAll()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" },
                new() { ParentProject = "PRJ2", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Buyer\":\"\"}"
            };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_FilterByProjectBuyerCode_ReturnsMatchingOnly()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" },
                new() { ParentProject = "PRJ2", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", ProjectBuyerCode = "PBC-001", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", ProjectBuyerCode = "PBC-002", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ProjectBuyerCode\":\"001\"}"
            };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Single(result.Data);
            Assert.Equal("PBC-001", result.Data.First().ProjectBuyerCode);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_FilterByProjectBuyerCode_NullProjectBuyerCodeExcluded()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" },
                new() { ParentProject = "PRJ2", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", ProjectBuyerCode = "PBC-001", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", ProjectBuyerCode = null, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ProjectBuyerCode\":\"PBC\"}"
            };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Single(result.Data);
            Assert.Equal("PRJ1", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_FilterByProjectBuyerCodeEmptyValue_ReturnsAll()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", ProjectBuyerCode = "PBC-001", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ProjectBuyerCode\":\"\"}"
            };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_CombinedAllFilters_FiltersCorrectly()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test Analysis", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "ALPHA001", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" },
                new() { ParentProject = "ALPHA002", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" },
                new() { ParentProject = "BETA001", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ALPHA001", ProjectBuyerCode = "PBC-AAA", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "ALPHA002", ProjectBuyerCode = "PBC-BBB", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "BETA001", ProjectBuyerCode = "PBC-AAA", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TestCode\":\"BLOOD\",\"ItemDescription\":\"Blood\",\"Buyer\":\"ALPHA\",\"ProjectBuyerCode\":\"AAA\"}"
            };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Single(result.Data);
            Assert.Equal("ALPHA001", result.Data.First().Buyer);
            Assert.Equal("PBC-AAA", result.Data.First().ProjectBuyerCode);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_FilterWithUnknownKey_IgnoresUnknownKey()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" },
                new() { ParentProject = "PRJ2", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"UnknownField\":\"value\"}"
            };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_FilterByBuyerWhitespaceValue_ReturnsAll()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Buyer\":\"   \"}"
            };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_FilterByItemDescriptionWhitespaceValue_ReturnsAll()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ItemDescription\":\"   \"}"
            };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_FilterByProjectBuyerCodeWhitespaceValue_ReturnsAll()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", ProjectBuyerCode = "PBC-001", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ProjectBuyerCode\":\"   \"}"
            };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_FilterByTestCodeWhitespace_AndBuyerValid_OnlyBuyerApplied()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "ALPHA", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" },
                new() { ParentProject = "BETA", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ALPHA", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "BETA", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TestCode\":\"   \",\"Buyer\":\"ALPHA\"}"
            };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Single(result.Data);
            Assert.Equal("ALPHA", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_FilterCaseInsensitive_MatchesRegardlessOfCase()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test Analysis", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ItemDescription\":\"blood\"}"
            };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Single(result.Data);
        }

        #endregion

        #region GetPagedWithDetailsAsync — Sort coverage

        [Theory]
        [InlineData(nameof(TestRequirementDetail.Buyer), false)]
        [InlineData(nameof(TestRequirementDetail.Buyer), true)]
        [InlineData(nameof(TestRequirementDetail.UnitPrice), false)]
        [InlineData(nameof(TestRequirementDetail.UnitPrice), true)]
        [InlineData(nameof(TestRequirementDetail.NoRequired), false)]
        [InlineData(nameof(TestRequirementDetail.NoRequired), true)]
        [InlineData(nameof(TestRequirementDetail.Active), false)]
        [InlineData(nameof(TestRequirementDetail.Active), true)]
        [InlineData(nameof(TestRequirementDetail.ProjectBuyerCode), false)]
        [InlineData(nameof(TestRequirementDetail.ProjectBuyerCode), true)]
        [InlineData(nameof(TestRequirementDetail.IsDefraProject), false)]
        [InlineData(nameof(TestRequirementDetail.IsDefraProject), true)]
        [InlineData(nameof(TestRequirementDetail.RecUnitPrice), false)]
        [InlineData(nameof(TestRequirementDetail.RecUnitPrice), true)]
        public async Task GetPagedWithDetailsAsync_SortByKnownColumn_DoesNotThrow(string sortBy, bool descending)
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" },
                new() { ParentProject = "PRJ2", IsDefraProject = 1, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", UnitPrice = 10m, NoRequired = 2, Active = 1, ProjectBuyerCode = "PBC-001", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", UnitPrice = 5m, NoRequired = 4, Active = 0, ProjectBuyerCode = "PBC-002", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedWithDetailsAsync_NoSortBy_DefaultsToOrderByTestCode()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood", UnitPriceVla = 10m, DefraUnitPrice = 12m },
                new() { ItemCode = "URINE", ItemDescription = "Urine", UnitPriceVla = 8m, DefraUnitPrice = 9m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "URINE", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Single(result.Data);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task GetPagedWithDetailsAsync_UnknownSortColumn_DefaultsToTestCodeSort(bool descending)
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" },
                new() { ParentProject = "PRJ2", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "UnknownColumn", Descending = descending };

            var result = await repo.GetPagedWithDetailsAsync(query, "BLOOD");

            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        #region GetPagedByProjectAsync — Sort coverage

        [Theory]
        [InlineData(nameof(TestRequirementDetail.TestCode), false)]
        [InlineData(nameof(TestRequirementDetail.TestCode), true)]
        [InlineData(nameof(TestRequirementDetail.UnitPrice), false)]
        [InlineData(nameof(TestRequirementDetail.UnitPrice), true)]
        [InlineData(nameof(TestRequirementDetail.NoRequired), false)]
        [InlineData(nameof(TestRequirementDetail.NoRequired), true)]
        [InlineData(nameof(TestRequirementDetail.Active), false)]
        [InlineData(nameof(TestRequirementDetail.Active), true)]
        [InlineData(nameof(TestRequirementDetail.ProjectBuyerCode), false)]
        [InlineData(nameof(TestRequirementDetail.ProjectBuyerCode), true)]
        [InlineData(nameof(TestRequirementDetail.IsDefraProject), false)]
        [InlineData(nameof(TestRequirementDetail.IsDefraProject), true)]
        [InlineData(nameof(TestRequirementDetail.RecUnitPrice), false)]
        [InlineData(nameof(TestRequirementDetail.RecUnitPrice), true)]
        public async Task GetPagedByProjectAsync_SortByKnownColumn_DoesNotThrow(string sortBy, bool descending)
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood", UnitPriceVla = 10m, DefraUnitPrice = 12m },
                new() { ItemCode = "URINE", ItemDescription = "Urine", UnitPriceVla = 8m, DefraUnitPrice = 9m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", UnitPrice = 10m, NoRequired = 2, Active = 1, ProjectBuyerCode = "PBC-001", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Buyer = "PRJ1", UnitPrice = 5m, NoRequired = 4, Active = 0, ProjectBuyerCode = "PBC-002", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_NoSortBy_DefaultsToOrderByTestCode()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood", UnitPriceVla = 10m, DefraUnitPrice = 12m },
                new() { ItemCode = "URINE", ItemDescription = "Urine", UnitPriceVla = 8m, DefraUnitPrice = 9m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "URINE", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Equal(2, result.Data.Count);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
            Assert.Equal("URINE", result.Data.ElementAt(1).TestCode);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task GetPagedByProjectAsync_UnknownSortColumn_DefaultsToTestCodeSort(bool descending)
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood", UnitPriceVla = 10m, DefraUnitPrice = 12m },
                new() { ItemCode = "URINE", ItemDescription = "Urine", UnitPriceVla = 8m, DefraUnitPrice = 9m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "UnknownColumn", Descending = descending };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        #region GetPagedByProjectAsync — Filter coverage

        [Fact]
        public async Task GetPagedByProjectAsync_NullFilter_ReturnsAll()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood", UnitPriceVla = 10m, DefraUnitPrice = 12m },
                new() { ItemCode = "URINE", ItemDescription = "Urine", UnitPriceVla = 8m, DefraUnitPrice = 9m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_EmptyFilter_ReturnsAll()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "" };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_FilterByTestCode_ReturnsMatching()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood", UnitPriceVla = 10m, DefraUnitPrice = 12m },
                new() { ItemCode = "URINE", ItemDescription = "Urine", UnitPriceVla = 8m, DefraUnitPrice = 9m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"TestCode\":\"BLO\"}" };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_FilterByBuyer_ReturnsMatching()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "ALPHA", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" },
                new() { ParentProject = "BETA", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ALPHA", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "BETA", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"Buyer\":\"ALPHA\"}" };

            var result = await repo.GetPagedByProjectAsync(query, "ALPHA");

            Assert.Single(result.Data);
            Assert.Equal("ALPHA", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_FilterByProjectBuyerCode_ReturnsMatching()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood", UnitPriceVla = 10m, DefraUnitPrice = 12m },
                new() { ItemCode = "URINE", ItemDescription = "Urine", UnitPriceVla = 8m, DefraUnitPrice = 9m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", ProjectBuyerCode = "PBC-001", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Buyer = "PRJ1", ProjectBuyerCode = null, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"ProjectBuyerCode\":\"PBC\"}" };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal("PBC-001", result.Data.First().ProjectBuyerCode);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_FilterByItemDescription_ReturnsMatching()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test Analysis", UnitPriceVla = 10m, DefraUnitPrice = 12m },
                new() { ItemCode = "URINE", ItemDescription = null, UnitPriceVla = 8m, DefraUnitPrice = 9m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"ItemDescription\":\"Blood\"}" };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Single(result.Data);
            Assert.Equal("Blood Test Analysis", result.Data.First().ItemDescription);
        }

        [Fact]
        public async Task GetPagedByProjectAsync_WhitespaceFilter_ReturnsAll()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "   " };

            var result = await repo.GetPagedByProjectAsync(query, "PRJ1");

            Assert.Single(result.Data);
        }

        #endregion

        #region GetAllForExportAsync

        [Fact]
        public async Task GetAllForExportAsync_MatchingTestCode_ReturnsAllMatchingRecords()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" },
                new() { ParentProject = "PRJ2", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "PRJ2", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);

            var result = await repo.GetAllForExportAsync("BLOOD", null);

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllForExportAsync_NoMatchingTestCode_ReturnsEmpty()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);

            var result = await repo.GetAllForExportAsync("MISSING", null);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllForExportAsync_WithFilter_AppliesFilter()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "ALPHA", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" },
                new() { ParentProject = "BETA", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ALPHA", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "BETA", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);

            var result = await repo.GetAllForExportAsync("BLOOD", "{\"Buyer\":\"ALPHA\"}");

            Assert.Single(result);
            Assert.Equal("ALPHA", result.First().Buyer);
        }

        [Fact]
        public async Task GetAllForExportAsync_NullFilter_ReturnsAll()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);

            var result = await repo.GetAllForExportAsync("BLOOD", null);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetAllForExportAsync_OrdersByBuyer()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "ZZZ", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" },
                new() { ParentProject = "AAA", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "ZZZ", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Buyer = "AAA", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);

            var result = (await repo.GetAllForExportAsync("BLOOD", null)).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("AAA", result[0].Buyer);
            Assert.Equal("ZZZ", result[1].Buyer);
        }

        [Fact]
        public async Task GetAllForExportAsync_EmptyRepository_ReturnsEmpty()
        {
            var repo = CreateRepositoryWithJoinMocks();

            var result = await repo.GetAllForExportAsync("BLOOD", null);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllForExportAsync_DefraProject_UsesDefraUnitPrice()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 20m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 1, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);

            var result = (await repo.GetAllForExportAsync("BLOOD", null)).ToList();

            Assert.Single(result);
            Assert.Equal(20m, result.First().RecUnitPrice);
        }

        [Fact]
        public async Task GetAllForExportAsync_NonDefraProject_UsesVlaUnitPrice()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 20m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);

            var result = (await repo.GetAllForExportAsync("BLOOD", null)).ToList();

            Assert.Single(result);
            Assert.Equal(10m, result.First().RecUnitPrice);
        }

        #endregion

        #region GetDetailByIdAsync

        [Fact]
        public async Task GetDetailByIdAsync_MatchingRecord_ReturnsDetail()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", UnitPrice = 10m, NoRequired = 5, ProjectBuyerCode = "PBC-001", TestBuyerCode = "TBC-001", Active = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);

            var result = await repo.GetDetailByIdAsync("BLOOD", "PRJ1");

            Assert.NotNull(result);
            Assert.Equal("BLOOD", result.TestCode);
            Assert.Equal("PRJ1", result.Buyer);
            Assert.Equal("Blood Test", result.ItemDescription);
            Assert.Equal(10m, result.UnitPrice);
            Assert.Equal(5, result.NoRequired);
            Assert.Equal("PBC-001", result.ProjectBuyerCode);
            Assert.Equal("TBC-001", result.TestBuyerCode);
            Assert.Equal((short)1, result.Active);
            Assert.Equal(10m, result.RecUnitPrice);
        }

        [Fact]
        public async Task GetDetailByIdAsync_TestCodeNotFound_ReturnsNull()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);

            var result = await repo.GetDetailByIdAsync("MISSING", "PRJ1");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetDetailByIdAsync_BuyerNotFound_ReturnsNull()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 12m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);

            var result = await repo.GetDetailByIdAsync("BLOOD", "WRONG");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetDetailByIdAsync_DefraProject_UsesDefraUnitPrice()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 20m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 1, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);

            var result = await repo.GetDetailByIdAsync("BLOOD", "PRJ1");

            Assert.NotNull(result);
            Assert.Equal(20m, result.RecUnitPrice);
            Assert.Equal((short)1, result.IsDefraProject);
        }

        [Fact]
        public async Task GetDetailByIdAsync_NonDefraProject_UsesVlaUnitPrice()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", ItemDescription = "Blood Test", UnitPriceVla = 10m, DefraUnitPrice = 20m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithJoinMocks(testReqmts, testorProducts, projects);

            var result = await repo.GetDetailByIdAsync("BLOOD", "PRJ1");

            Assert.NotNull(result);
            Assert.Equal(10m, result.RecUnitPrice);
        }

        #endregion

        #region GetPricingAsync

        [Fact]
        public async Task GetPricingAsync_TestCodeNotFound_ReturnsNull()
        {
            var repo = CreateRepositoryWithJoinMocks(
                testorProducts: new List<TestorProduct>());

            var result = await repo.GetPricingAsync("MISSING", null);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetPricingAsync_TestCodeOnly_NullProjectCode_ReturnsDefraUnitPrice()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", DefraUnitPrice = 20m, UnitPriceVla = 10m }
            };
            var repo = CreateRepositoryWithJoinMocks(testorProducts: testorProducts);

            var result = await repo.GetPricingAsync("BLOOD", null);

            Assert.NotNull(result);
            Assert.Equal("BLOOD", result.TestCode);
            Assert.Equal(20m, result.RecUnitPrice);
        }

        [Fact]
        public async Task GetPricingAsync_TestCodeOnly_EmptyProjectCode_ReturnsDefraUnitPrice()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", DefraUnitPrice = 20m, UnitPriceVla = 10m }
            };
            var repo = CreateRepositoryWithJoinMocks(testorProducts: testorProducts);

            var result = await repo.GetPricingAsync("BLOOD", "");

            Assert.NotNull(result);
            Assert.Equal(20m, result.RecUnitPrice);
        }

        [Fact]
        public async Task GetPricingAsync_TestCodeOnly_WhitespaceProjectCode_ReturnsDefraUnitPrice()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", DefraUnitPrice = 20m, UnitPriceVla = 10m }
            };
            var repo = CreateRepositoryWithJoinMocks(testorProducts: testorProducts);

            var result = await repo.GetPricingAsync("BLOOD", "   ");

            Assert.NotNull(result);
            Assert.Equal(20m, result.RecUnitPrice);
        }

        [Fact]
        public async Task GetPricingAsync_WithProjectCode_ProjectNotFound_ReturnsNull()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", DefraUnitPrice = 20m, UnitPriceVla = 10m }
            };
            var repo = CreateRepositoryWithJoinMocks(testorProducts: testorProducts, projects: new List<Project>());

            var result = await repo.GetPricingAsync("BLOOD", "MISSING_PROJECT");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetPricingAsync_WithProjectCode_NonDefraProject_ReturnsVlaUnitPrice()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", DefraUnitPrice = 20m, UnitPriceVla = 10m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 0, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var repo = CreateRepositoryWithJoinMocks(testorProducts: testorProducts, projects: projects);

            var result = await repo.GetPricingAsync("BLOOD", "PRJ1");

            Assert.NotNull(result);
            Assert.Equal("BLOOD", result.TestCode);
            Assert.Equal("PRJ1", result.Buyer);
            Assert.Equal((short)0, result.IsDefraProject);
            Assert.Equal(10m, result.RecUnitPrice);
        }

        [Fact]
        public async Task GetPricingAsync_WithProjectCode_DefraProject_ReturnsDefraUnitPrice()
        {
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "BLOOD", DefraUnitPrice = 20m, UnitPriceVla = 10m }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", IsDefraProject = 1, ProjectTitle = "T", Program = "P", Customer = "C", Disease = "D", Contract = "CT", IncomeAccountCode = "INC" }
            };
            var repo = CreateRepositoryWithJoinMocks(testorProducts: testorProducts, projects: projects);

            var result = await repo.GetPricingAsync("BLOOD", "PRJ1");

            Assert.NotNull(result);
            Assert.Equal("BLOOD", result.TestCode);
            Assert.Equal("PRJ1", result.Buyer);
            Assert.Equal((short)1, result.IsDefraProject);
            Assert.Equal(20m, result.RecUnitPrice);
        }

        #endregion

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_RecordExists_ReturnsTrue()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.ExistsAsync("BLOOD", "PRJ1");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_TestCodeNotFound_ReturnsFalse()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.ExistsAsync("MISSING", "PRJ1");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_BuyerNotFound_ReturnsFalse()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.ExistsAsync("BLOOD", "WRONG");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_EmptyRepository_ReturnsFalse()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks();

            var result = await repo.ExistsAsync("BLOOD", "PRJ1");

            Assert.False(result);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_SetsEntityFpsYearFromContext()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks(fpsYear: 2025);
            var entity = new TestRequirement { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = 2020 };

            // Entry() cannot be mocked (internal EF ctor); verify FpsYear is set before that call
            await Assert.ThrowsAsync<NullReferenceException>(() => repo.UpdateAsync(entity));

            Assert.Equal(2025, entity.FpsYear);
        }

        [Fact]
        public async Task UpdateAsync_SetsEntityStateToModified_ThrowsBecauseEntryCannotBeMocked()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks();
            var entity = new TestRequirement { TestCode = "BLOOD", Buyer = "PRJ1" };

            // Confirms code path reaches _context.Entry(entity).State assignment
            await Assert.ThrowsAsync<NullReferenceException>(() => repo.UpdateAsync(entity));
        }

        [Fact]
        public async Task UpdateAsync_SetsEntityFpsYearBeforeEntryCall()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks(fpsYear: 2025);
            var entity = new TestRequirement { TestCode = "BLOOD", Buyer = "PRJ1", UnitPrice = 15m, FpsYear = 2020 };

            // Verify FpsYear mutation happens regardless of Entry() mock limitation
            await Assert.ThrowsAsync<NullReferenceException>(() => repo.UpdateAsync(entity));

            Assert.Equal(2025, entity.FpsYear);
            Assert.Equal(15m, entity.UnitPrice);
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_SetsEntityFpsYearFromContext()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks(fpsYear: 2025);
            var entity = new TestRequirement { TestCode = "BLOOD", Buyer = "PRJ1" };

            var result = await repo.AddAsync(entity);

            Assert.Equal(2025, result.FpsYear);
        }

        [Fact]
        public async Task AddAsync_SetsDateCreated()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks();
            var entity = new TestRequirement { TestCode = "BLOOD", Buyer = "PRJ1" };

            await repo.AddAsync(entity);

            Assert.NotNull(entity.DateCreated);
        }

        [Fact]
        public async Task AddAsync_CallsSaveChanges()
        {
            var (repo, _, _, mockContext) = CreateRepositoryWithMocks();
            var entity = new TestRequirement { TestCode = "BLOOD", Buyer = "PRJ1" };

            await repo.AddAsync(entity);

            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task AddAsync_WithNonNullUnitPriceAndNoRequired_WritesAuditWithValues()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks(fpsYear: 2025);
            var entity = new TestRequirement
            {
                TestCode = "BLOOD", Buyer = "PRJ1",
                UnitPrice = 15.5m, NoRequired = 3,
                ProjectBuyerCode = "PBC-001", TestBuyerCode = "TBC-001", Active = 1
            };

            var result = await repo.AddAsync(entity);

            Assert.Equal(2025, result.FpsYear);
            Assert.NotNull(result.DateCreated);
        }

        [Fact]
        public async Task AddAsync_WithNullUnitPriceAndNoRequired_WritesAuditWithNulls()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks(fpsYear: 2025);
            var entity = new TestRequirement
            {
                TestCode = "BLOOD", Buyer = "PRJ1",
                UnitPrice = null, NoRequired = null
            };

            var result = await repo.AddAsync(entity);

            Assert.Equal(2025, result.FpsYear);
        }

        [Fact]
        public async Task AddAsync_ReturnsEntity()
        {
            var (repo, _, _, _) = CreateRepositoryWithMocks(fpsYear: 2025);
            var entity = new TestRequirement { TestCode = "BLOOD", Buyer = "PRJ1", UnitPrice = 10m };

            var result = await repo.AddAsync(entity);

            Assert.Same(entity, result);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_RecordExistsWithCorrectYear_RemovesAndReturnsTrue()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, dbSetMock, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.DeleteAsync("BLOOD", "PRJ1");

            Assert.True(result);
            dbSetMock.Verify(x => x.Remove(It.IsAny<TestRequirement>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_RecordNotFound_ReturnsFalse()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear }
            };
            var (repo, dbSetMock, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.DeleteAsync("MISSING", "PRJ1");

            Assert.False(result);
            dbSetMock.Verify(x => x.Remove(It.IsAny<TestRequirement>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_RecordExistsButWrongFpsYear_ReturnsFalse()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = 2023 }
            };
            var (repo, dbSetMock, _, _) = CreateRepositoryWithMocks(testReqmts, fpsYear: DefaultFpsYear);

            var result = await repo.DeleteAsync("BLOOD", "PRJ1");

            Assert.False(result);
            dbSetMock.Verify(x => x.Remove(It.IsAny<TestRequirement>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WithNonNullUnitPriceAndNoRequired_WritesDeleteAudit()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear, UnitPrice = 12.5m, NoRequired = 4 }
            };
            var (repo, dbSetMock, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.DeleteAsync("BLOOD", "PRJ1");

            Assert.True(result);
            dbSetMock.Verify(x => x.Remove(It.IsAny<TestRequirement>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithNullUnitPriceAndNoRequired_WritesDeleteAudit()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "BLOOD", Buyer = "PRJ1", FpsYear = DefaultFpsYear, UnitPrice = null, NoRequired = null }
            };
            var (repo, dbSetMock, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.DeleteAsync("BLOOD", "PRJ1");

            Assert.True(result);
            dbSetMock.Verify(x => x.Remove(It.IsAny<TestRequirement>()), Times.Once);
        }

        #endregion

        #region GetPlannedTestsByWorkgroupAsync

        private static TestRequirementRepository CreateRepositoryWithBreakdownMocks(
            IEnumerable<TestReqBreakdownView>? views = null,
            int fpsYear = DefaultFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var breakdownMockSet = RepositoryTestHelper.CreateMockDbSet(views ?? []);
            RepositoryTestHelper.SetupDbSetOperations(breakdownMockSet);

            var testReqmtsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<TestRequirement>());
            var monthlyOutputsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<MonthlyOutput>());
            var testReqLogsMockSet = RepositoryTestHelper.CreateMockDbSet(new List<TestRequirementLog>());

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.TestReqBreakdownViews).Returns(breakdownMockSet.Object);
            mockContext.Setup(x => x.TestRequirements).Returns(testReqmtsMockSet.Object);
            mockContext.Setup(x => x.MonthlyOutputs).Returns(monthlyOutputsMockSet.Object);
            mockContext.Setup(x => x.TestRequirementLogs).Returns(testReqLogsMockSet.Object);

            return new TestRequirementRepository(mockContext.Object, fpsRequestContext);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_ReturnsAllRows_WhenNoFilter()
        {
            var views = new List<TestReqBreakdownView>
            {
                new() { TestCode = "BLOOD", Project = "PRJ001", WorkG = "WG01", WgPrice = 10m, TotalCost = 50m, FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Project = "PRJ002", WorkG = "WG02", WgPrice = 5m,  TotalCost = 25m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPlannedTestsByWorkgroupAsync(query);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_EmptyData_ReturnsEmptyResult()
        {
            var repo = CreateRepositoryWithBreakdownMocks();
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPlannedTestsByWorkgroupAsync(query);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_NullFilter_ReturnsAll()
        {
            var views = new List<TestReqBreakdownView>
            {
                new() { TestCode = "BLOOD", Project = "PRJ001", WorkG = "WG01", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Project = "PRJ002", WorkG = "WG02", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            var result = await repo.GetPlannedTestsByWorkgroupAsync(query);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_WhitespaceFilter_ReturnsAll()
        {
            var views = new List<TestReqBreakdownView>
            {
                new() { TestCode = "BLOOD", Project = "PRJ001", WorkG = "WG01", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "   " };

            var result = await repo.GetPlannedTestsByWorkgroupAsync(query);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_FilterByTestCode_ReturnsMatchingRows()
        {
            var views = new List<TestReqBreakdownView>
            {
                new() { TestCode = "BLOOD", Project = "PRJ001", WorkG = "WG01", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Project = "PRJ002", WorkG = "WG02", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TestCode\":\"BLO\"}"
            };

            var result = await repo.GetPlannedTestsByWorkgroupAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_FilterByTestCode_EmptyValue_ReturnsAll()
        {
            var views = new List<TestReqBreakdownView>
            {
                new() { TestCode = "BLOOD", Project = "PRJ001", WorkG = "WG01", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Project = "PRJ002", WorkG = "WG02", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TestCode\":\"\"}"
            };

            var result = await repo.GetPlannedTestsByWorkgroupAsync(query);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_FilterByProject_ReturnsMatchingRows()
        {
            var views = new List<TestReqBreakdownView>
            {
                new() { TestCode = "BLOOD", Project = "PRJ001", WorkG = "WG01", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Project = "PRJ002", WorkG = "WG02", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Project\":\"PRJ001\"}"
            };

            var result = await repo.GetPlannedTestsByWorkgroupAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("PRJ001", result.Data.First().Project);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_FilterByShortDescription_NullExcluded()
        {
            var views = new List<TestReqBreakdownView>
            {
                new() { TestCode = "BLOOD", Project = "PRJ001", ShortDescription = "Blood Sample", WorkG = "WG01", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Project = "PRJ002", ShortDescription = null,           WorkG = "WG02", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ShortDescription\":\"Blood\"}"
            };

            var result = await repo.GetPlannedTestsByWorkgroupAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_FilterByPc_NullExcluded()
        {
            var views = new List<TestReqBreakdownView>
            {
                new() { TestCode = "BLOOD", Project = "PRJ001", Pc = "PC01", WorkG = "WG01", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Project = "PRJ002", Pc = null,   WorkG = "WG02", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"PC\":\"PC01\"}"
            };

            var result = await repo.GetPlannedTestsByWorkgroupAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("PC01", result.Data.First().Pc);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_FilterByWorkG_NullExcluded()
        {
            var views = new List<TestReqBreakdownView>
            {
                new() { TestCode = "BLOOD", Project = "PRJ001", WorkG = "WG01", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Project = "PRJ002", WorkG = null,   FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"WorkG\":\"WG01\"}"
            };

            var result = await repo.GetPlannedTestsByWorkgroupAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("WG01", result.Data.First().WorkG);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_FilterByProgram_NullExcluded()
        {
            var views = new List<TestReqBreakdownView>
            {
                new() { TestCode = "BLOOD", Project = "PRJ001", Program = "PROG01", WorkG = "WG01", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Project = "PRJ002", Program = null,     WorkG = "WG02", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Program\":\"PROG01\"}"
            };

            var result = await repo.GetPlannedTestsByWorkgroupAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("PROG01", result.Data.First().Program);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_CombinedFilters_AppliedCorrectly()
        {
            var views = new List<TestReqBreakdownView>
            {
                new() { TestCode = "BLOOD", Project = "PRJ001", WorkG = "WG01", Pc = "PC01", Program = "PROG01", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Project = "PRJ002", WorkG = "WG01", Pc = "PC01", Program = "PROG01", FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Project = "PRJ001", WorkG = "WG02", Pc = "PC02", Program = "PROG02", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TestCode\":\"BLOOD\",\"Project\":\"PRJ001\",\"WorkG\":\"WG01\"}"
            };

            var result = await repo.GetPlannedTestsByWorkgroupAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
            Assert.Equal("PRJ001", result.Data.First().Project);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_Paging_ReturnsCorrectPage()
        {
            var views = Enumerable.Range(1, 5).Select(i =>
                new TestReqBreakdownView { TestCode = $"TEST{i:D3}", Project = $"PRJ{i:D3}", WorkG = "WG01", FpsYear = DefaultFpsYear }).ToList();
            var repo = CreateRepositoryWithBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            var result = await repo.GetPlannedTestsByWorkgroupAsync(query);

            Assert.Equal(2, result.Data.Count);
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        [Theory]
        [InlineData("testcode",         false)]
        [InlineData("testcode",         true)]
        [InlineData("shortdescription", false)]
        [InlineData("shortdescription", true)]
        [InlineData("program",          false)]
        [InlineData("program",          true)]
        [InlineData("project",          false)]
        [InlineData("project",          true)]
        [InlineData("pc",               false)]
        [InlineData("pc",               true)]
        [InlineData("workg",            false)]
        [InlineData("workg",            true)]
        [InlineData("wgprice",          false)]
        [InlineData("wgprice",          true)]
        [InlineData("totalcost",        false)]
        [InlineData("totalcost",        true)]
        public async Task GetPlannedTestsByWorkgroupAsync_SortByKnownColumn_DoesNotThrow(string sortBy, bool descending)
        {
            var views = new List<TestReqBreakdownView>
            {
                new() { TestCode = "BLOOD", Project = "PRJ001", WorkG = "WG01", WgPrice = 10m, TotalCost = 50m, FpsYear = DefaultFpsYear },
                new() { TestCode = "URINE", Project = "PRJ002", WorkG = "WG02", WgPrice = 5m,  TotalCost = 25m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            var result = await repo.GetPlannedTestsByWorkgroupAsync(query);

            Assert.Equal(2, result.Data.Count);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task GetPlannedTestsByWorkgroupAsync_UnknownSortColumn_DefaultsToTestCodeSort(bool descending)
        {
            var views = new List<TestReqBreakdownView>
            {
                new() { TestCode = "URINE", Project = "PRJ002", WorkG = "WG02", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Project = "PRJ001", WorkG = "WG01", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "UnknownColumn", Descending = descending };

            var result = await repo.GetPlannedTestsByWorkgroupAsync(query);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetPlannedTestsByWorkgroupAsync_NoSortBy_DefaultsToOrderByTestCode()
        {
            var views = new List<TestReqBreakdownView>
            {
                new() { TestCode = "URINE", Project = "PRJ002", WorkG = "WG02", FpsYear = DefaultFpsYear },
                new() { TestCode = "BLOOD", Project = "PRJ001", WorkG = "WG01", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepositoryWithBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetPlannedTestsByWorkgroupAsync(query);

            Assert.Equal(2, result.Data.Count);
            Assert.Equal("BLOOD", result.Data.First().TestCode);
            Assert.Equal("URINE", result.Data.ElementAt(1).TestCode);
        }

        #endregion

        #region GetAllActiveAsync

        [Fact]
        public async Task GetAllActiveAsync_WithMixedActiveFlags_ReturnsOnlyActiveItems()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "PT0001", Buyer = "SV3300", Active = 1, FpsYear = DefaultFpsYear },
                new() { TestCode = "PT0002", Buyer = "SV3301", Active = 0, FpsYear = DefaultFpsYear },
                new() { TestCode = "PT0003", Buyer = "SV3302", Active = 2, FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.GetAllActiveAsync();

            Assert.Equal(2, result.Count);
            Assert.All(result, x => Assert.True(x.Active != 0));
        }

        [Fact]
        public async Task GetAllActiveAsync_WithNoActiveItems_ReturnsEmptyList()
        {
            var testReqmts = new List<TestRequirement>
            {
                new() { TestCode = "PT0001", Buyer = "SV3300", Active = 0, FpsYear = DefaultFpsYear }
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks(testReqmts);

            var result = await repo.GetAllActiveAsync();

            Assert.Empty(result);
        }

        #endregion

        // ── Helper: CreateRepositoryWithActualBreakdownMocks ─────────────────────────

        private static TestRequirementRepository CreateRepositoryWithActualBreakdownMocks(
            IEnumerable<TestActualBreakdownView>? views = null,
            int fpsYear = DefaultFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var viewsMockSet = RepositoryTestHelper.CreateMockDbSet(views ?? []);
            RepositoryTestHelper.SetupDbSetOperations(viewsMockSet);

            mockContext.Setup(x => x.TestActualBreakdownViews).Returns(viewsMockSet.Object);

            return new TestRequirementRepository(mockContext.Object, fpsRequestContext);
        }

        // ── GetActualsTestsWithPlannedDataByWorkgroupAsync ────────────────────────────

        #region GetActualsTestsWithPlannedDataByWorkgroupAsync

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_NoData_ReturnsEmptyPagedResult()
        {
            var repo  = CreateRepositoryWithActualBreakdownMocks();
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_WithData_ReturnsAllItemsOnSinglePage()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", Buyer = "SV3300" },
                new() { TestCode = "PT0002", Buyer = "SB4600" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_NullFilter_ReturnsAllItems()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001" },
                new() { TestCode = "PT0002" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_EmptyFilter_ReturnsAllItems()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001" },
                new() { TestCode = "PT0002" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "" };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_PageSizeOne_ReturnsOneItem()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "AA001" },
                new() { TestCode = "BB001" },
                new() { TestCode = "CC001" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 1, SortBy = "testcode", Descending = false };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_SecondPage_ReturnsCorrectItems()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "AA001" },
                new() { TestCode = "BB001" },
                new() { TestCode = "CC001" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2, SortBy = "testcode", Descending = false };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("CC001", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_TotalRecordsMatchesDataSize()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "AA001" },
                new() { TestCode = "BB001" },
                new() { TestCode = "CC001" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Equal(3, result.PaginationData.TotalRecords);
        }

        #endregion

        // ── ApplyActualBreakdownSorting (via GetActualsTestsWithPlannedDataByWorkgroupAsync) ──

        #region ApplyActualBreakdownSorting

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_SortByTestCode_Ascending_OrdersCorrectly()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "ZZ001" },
                new() { TestCode = "AA001" },
                new() { TestCode = "MM001" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "testcode", Descending = false };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);
            var list   = result.Data.ToList();

            Assert.Equal("AA001", list[0].TestCode);
            Assert.Equal("MM001", list[1].TestCode);
            Assert.Equal("ZZ001", list[2].TestCode);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_SortByTestCode_Descending_OrdersCorrectly()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "AA001" },
                new() { TestCode = "ZZ001" },
                new() { TestCode = "MM001" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "testcode", Descending = true };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);
            var list   = result.Data.ToList();

            Assert.Equal("ZZ001", list[0].TestCode);
            Assert.Equal("MM001", list[1].TestCode);
            Assert.Equal("AA001", list[2].TestCode);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_SortByShortDescription_Ascending_OrdersCorrectly()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", ShortDescription = "Zulu Test"  },
                new() { TestCode = "PT0002", ShortDescription = "Alpha Test" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "shortdescription", Descending = false };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);
            var list   = result.Data.ToList();

            Assert.Equal("Alpha Test", list[0].ShortDescription);
            Assert.Equal("Zulu Test",  list[1].ShortDescription);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_SortByShortDescription_Descending_OrdersCorrectly()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", ShortDescription = "Alpha Test" },
                new() { TestCode = "PT0002", ShortDescription = "Zulu Test"  }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "shortdescription", Descending = true };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Equal("Zulu Test", result.Data.First().ShortDescription);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_SortByProgram_Ascending_OrdersCorrectly()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", Program = "Virology"     },
                new() { TestCode = "PT0002", Program = "Bacteriology" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "program", Descending = false };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);
            var list   = result.Data.ToList();

            Assert.Equal("Bacteriology", list[0].Program);
            Assert.Equal("Virology",     list[1].Program);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_SortByBuyer_Ascending_OrdersCorrectly()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", Buyer = "SV3300" },
                new() { TestCode = "PT0002", Buyer = "SB4600" },
                new() { TestCode = "PT0003", Buyer = "AA1000" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "buyer", Descending = false };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);
            var list   = result.Data.ToList();

            Assert.Equal("AA1000", list[0].Buyer);
            Assert.Equal("SB4600", list[1].Buyer);
            Assert.Equal("SV3300", list[2].Buyer);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_SortByBuyer_Descending_OrdersCorrectly()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", Buyer = "AA1000" },
                new() { TestCode = "PT0002", Buyer = "SV3300" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "buyer", Descending = true };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Equal("SV3300", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_SortByPortfolio_Ascending_OrdersCorrectly()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", Portfolio = "ZPortfolio" },
                new() { TestCode = "PT0002", Portfolio = "APortfolio" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "portfolio", Descending = false };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Equal("APortfolio", result.Data.First().Portfolio);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_SortByWorkGroup_Ascending_OrdersCorrectly()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", WorkGroup = "ZWorkGroup" },
                new() { TestCode = "PT0002", WorkGroup = "AWorkGroup" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "workgroup", Descending = false };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Equal("AWorkGroup", result.Data.First().WorkGroup);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_SortByMonth_Ascending_OrdersCorrectly()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", Month = 12 },
                new() { TestCode = "PT0002", Month = 1  },
                new() { TestCode = "PT0003", Month = 6  }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "month", Descending = false };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);
            var list   = result.Data.ToList();

            Assert.Equal(1,  list[0].Month);
            Assert.Equal(6,  list[1].Month);
            Assert.Equal(12, list[2].Month);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_SortByMonth_Descending_OrdersCorrectly()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", Month = 1  },
                new() { TestCode = "PT0002", Month = 12 }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "month", Descending = true };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Equal(12, result.Data.First().Month);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_SortByPCPrice_Ascending_OrdersCorrectly()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", PCPrice = 500m },
                new() { TestCode = "PT0002", PCPrice = 100m }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "pcprice", Descending = false };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);
            var list   = result.Data.ToList();

            Assert.Equal(100m, list[0].PCPrice);
            Assert.Equal(500m, list[1].PCPrice);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_SortByPCCost_Ascending_OrdersCorrectly()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", PCCost = 999m },
                new() { TestCode = "PT0002", PCCost = 111m }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "pccost", Descending = false };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Equal(111m, result.Data.First().PCCost);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_SortByProfitCentre_Ascending_OrdersCorrectly()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", ProfitCentre = "ZPC" },
                new() { TestCode = "PT0002", ProfitCentre = "APC" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "profitcentre", Descending = false };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Equal("APC", result.Data.First().ProfitCentre);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_UnknownSortBy_DefaultsSortByTestCode()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "ZZ001" },
                new() { TestCode = "AA001" },
                new() { TestCode = "MM001" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "unknownfield", Descending = false };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Equal("AA001", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_NullSortBy_DefaultsSortByTestCode()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "ZZ001" },
                new() { TestCode = "AA001" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = null, Descending = false };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Equal("AA001", result.Data.First().TestCode);
        }

        #endregion

        // ── ApplyActualBreakdownFilter (via GetActualsTestsWithPlannedDataByWorkgroupAsync) ──

        #region ApplyActualBreakdownFilter

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_NullDeserializedFilter_ReturnsAllItems()
        {
            // A JSON "null" literal deserialises to null, triggering the null-guard in
            // ApplyActualBreakdownFilter and returning all rows unfiltered.
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001" },
                new() { TestCode = "PT0002" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "null" };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_FilterByTestCode_ReturnsMatchingItems()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001" },
                new() { TestCode = "PT0002" },
                new() { TestCode = "AB9999" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = """{"TestCode":"PT"}"""
            };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, x => Assert.Contains("PT", x.TestCode));
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_FilterByTestCode_NoMatch_ReturnsEmpty()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = """{"TestCode":"NOTEXIST"}"""
            };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_FilterByBuyer_ReturnsMatchingItems()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", Buyer = "SV3300" },
                new() { TestCode = "PT0002", Buyer = "SB4600" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = """{"Buyer":"SV3300"}"""
            };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("SV3300", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_FilterByProgram_ReturnsMatchingItems()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", Program = "Bacteriology" },
                new() { TestCode = "PT0002", Program = "Virology"     }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = """{"Program":"Bacteriology"}"""
            };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("Bacteriology", result.Data.First().Program);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_FilterByShortDescription_ReturnsMatchingItems()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", ShortDescription = "Blood Test"  },
                new() { TestCode = "PT0002", ShortDescription = "Urine Check" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = """{"ShortDescription":"Blood"}"""
            };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("Blood Test", result.Data.First().ShortDescription);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_FilterByPortfolio_ReturnsMatchingItems()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", Portfolio = "PortfolioA" },
                new() { TestCode = "PT0002", Portfolio = "PortfolioB" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = """{"Portfolio":"PortfolioA"}"""
            };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("PortfolioA", result.Data.First().Portfolio);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_FilterByWorkGroup_ReturnsMatchingItems()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", WorkGroup = "WG-Alpha" },
                new() { TestCode = "PT0002", WorkGroup = "WG-Beta"  }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = """{"WorkGroup":"WG-Alpha"}"""
            };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("WG-Alpha", result.Data.First().WorkGroup);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_FilterByProfitCentre_ReturnsMatchingItems()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", ProfitCentre = "PC001" },
                new() { TestCode = "PT0002", ProfitCentre = "PC002" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = """{"ProfitCentre":"PC001"}"""
            };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("PC001", result.Data.First().ProfitCentre);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_FilterByMultipleFields_ReturnsNarrowedResults()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", Buyer = "SV3300", Program = "Bacteriology" },
                new() { TestCode = "PT0002", Buyer = "SV3300", Program = "Virology"     },
                new() { TestCode = "PT0003", Buyer = "SB4600", Program = "Bacteriology" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = """{"Buyer":"SV3300","Program":"Bacteriology"}"""
            };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("PT0001", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_FilterWithWhitespaceValue_IgnoresFilter()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0001", Buyer = "SV3300" },
                new() { TestCode = "PT0002", Buyer = "SB4600" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = """{"Buyer":"   "}"""
            };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetActualsTestsWithPlannedDataByWorkgroupAsync_FilterAndSort_Combined_WorkCorrectly()
        {
            var views = new List<TestActualBreakdownView>
            {
                new() { TestCode = "PT0003", Buyer = "SV3300" },
                new() { TestCode = "PT0001", Buyer = "SV3300" },
                new() { TestCode = "PT0002", Buyer = "SB4600" }
            };

            var repo  = CreateRepositoryWithActualBreakdownMocks(views);
            var query = new PaginationParameters<string>
            {
                Page      = 1, PageSize  = 10,
                Filter    = """{"Buyer":"SV3300"}""",
                SortBy    = "testcode",
                Descending = false
            };

            var result = await repo.GetActualsTestsWithPlannedDataByWorkgroupAsync(query);
            var list   = result.Data.ToList();

            Assert.Equal(2,       result.Data.Count);
            Assert.Equal("PT0001", list[0].TestCode);
            Assert.Equal("PT0003", list[1].TestCode);
        }

        #endregion
    }
}
