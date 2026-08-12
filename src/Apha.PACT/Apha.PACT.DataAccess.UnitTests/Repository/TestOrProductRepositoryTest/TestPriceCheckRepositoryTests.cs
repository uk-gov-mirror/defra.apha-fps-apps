using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.TestOrProductRepositoryTest
{
    public class TestPriceCheckRepositoryTests
    {
        private const string UserEmail = "test.user@example.com";
        private const int DefaultFpsYear = 2024;

        // ── Moq-based factory used by read tests (GetPaged / GetByKey) ─────────────
        // BuildTestPriceCheckBaseQuery joins TestRequirements → ProjectViews → TestorProducts
        // and filters by EF.Functions.ILike(p.UserEmail, UserEmailId).
        // The in-memory provider does not support ILike; the Moq path feeds real data
        // through TestAsyncQueryProvider whose LikeRewriter converts ILike → Contains.
        private static TestorProductRepository CreateRepositoryWithMocks(
            IEnumerable<TestRequirement>?  testRequirements = null,
            IEnumerable<ProjectView>?      projectViews     = null,
            IEnumerable<TestorProduct>?    testorProducts   = null,
            int fpsYear = DefaultFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);
            fpsRequestContext.UserEmailId.Returns(UserEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var testReqSet      = RepositoryTestHelper.CreateMockDbSet(testRequirements ?? []);
            var projectViewSet  = RepositoryTestHelper.CreateMockDbSet(projectViews     ?? []);
            var testorProductSet = RepositoryTestHelper.CreateMockDbSet(testorProducts  ?? []);

            RepositoryTestHelper.SetupDbSetOperations(testReqSet);
            RepositoryTestHelper.SetupDbSetOperations(projectViewSet);
            RepositoryTestHelper.SetupDbSetOperations(testorProductSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.TestRequirements).Returns(testReqSet.Object);
            mockContext.Setup(x => x.ProjectViews).Returns(projectViewSet.Object);
            mockContext.Setup(x => x.TestorProducts).Returns(testorProductSet.Object);

            return new TestorProductRepository(mockContext.Object, fpsRequestContext);
        }

        // ── Shared seed data ──────────────────────────────────────────────────────
        // T001 / JOB001 — non-Defra  → NormalPrice = UnitPriceVla  = 50m, TestPrice = 50m (standard)
        // T002 / JOB002 — Defra      → NormalPrice = DefraUnitPrice = 120m, TestPrice = 0m (zero)

        private static IEnumerable<TestRequirement> SeedRequirements() =>
        [
            new TestRequirement { TestCode = "T001", Buyer = "JOB001", NoRequired = 5,  UnitPrice = 50m, FpsYear = DefaultFpsYear },
            new TestRequirement { TestCode = "T002", Buyer = "JOB002", NoRequired = 10, UnitPrice = 0m,  FpsYear = DefaultFpsYear }
        ];

        private static IEnumerable<ProjectView> SeedProjectViews() =>
        [
            new ProjectView { ParentProject = "JOB001", IsDefraProject = 0,  Program = "PROG1", Manager = "Smith", UserEmail = UserEmail },
            new ProjectView { ParentProject = "JOB002", IsDefraProject = -1, Program = "PROG2", Manager = "Jones", UserEmail = UserEmail }
        ];

        private static IEnumerable<TestorProduct> SeedTestorProducts() =>
        [
            new TestorProduct { ItemCode = "T001", UnitPriceVla = 50m,  DefraUnitPrice = 80m,  Owner = "AB", FpsYear = DefaultFpsYear },
            new TestorProduct { ItemCode = "T002", UnitPriceVla = 100m, DefraUnitPrice = 120m, Owner = "CD", FpsYear = DefaultFpsYear }
        ];

        // Non-standard seed: T001 TestPrice=60m ≠ UnitPriceVla=50m (non-standard, non-zero)
        // → both T001 and T002 pass the "all" (default) price filter so sorting / column
        //   filter tests always have two rows in the result to verify order / matching.
        private static IEnumerable<TestRequirement> SeedRequirementsNonStandard() =>
        [
            new TestRequirement { TestCode = "T001", Buyer = "JOB001", NoRequired = 5,  UnitPrice = 60m, FpsYear = DefaultFpsYear },
            new TestRequirement { TestCode = "T002", Buyer = "JOB002", NoRequired = 10, UnitPrice = 0m,  FpsYear = DefaultFpsYear }
        ];

        // ── In-memory factory — used only by UpdateTestPriceCheckAsync test ───────
        private static (FpsDbContext Context, TestorProductRepository Repo) CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<FpsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultFpsYear);
            fpsRequestContext.UserEmailId.Returns(UserEmail);
            var context = new FpsDbContext(options, fpsRequestContext);
            var repo    = new TestorProductRepository(context, fpsRequestContext);
            return (context, repo);
        }

        private static async Task SeedInMemoryAsync(FpsDbContext context)
        {
            context.TestorProducts.AddRange(SeedTestorProducts());
            context.Projects.AddRange(
                new Project { ParentProject = "JOB001", IsDefraProject = 0,  Program = "PROG1", Manager = "Smith", ProjectTitle = "Project One", Customer = "CUST1", Disease = "DIS1", Contract = "CON1", ProjectStatus = "A", IncomeAccountCode = "IAC1", FpsYear = DefaultFpsYear },
                new Project { ParentProject = "JOB002", IsDefraProject = -1, Program = "PROG2", Manager = "Jones", ProjectTitle = "Project Two", Customer = "CUST2", Disease = "DIS2", Contract = "CON2", ProjectStatus = "A", IncomeAccountCode = "IAC2", FpsYear = DefaultFpsYear }
            );
            context.TestRequirements.AddRange(SeedRequirements());
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();
        }

        #region GetTestPriceCheckPagedAsync

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_PriceFilterAll_ReturnsMatchingRows()
        {
            var repo = CreateRepositoryWithMocks(SeedRequirements(), SeedProjectViews(), SeedTestorProducts());
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.NotNull(result);
            Assert.True(result.Data.Count > 0);
            // "Both" must return only zero-rated or non-standard rows, never standard-priced rows.
            Assert.All(result.Data, row => Assert.True(row.IsZeroPrice || row.IsNotStandard));
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_PriceFilterZero_ReturnsOnlyZeroPriceRows()
        {
            var repo = CreateRepositoryWithMocks(SeedRequirements(), SeedProjectViews(), SeedTestorProducts());
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "zero", null);

            Assert.NotNull(result);
            Assert.All(result.Data, row => Assert.Equal(0m, row.TestPrice));
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_OwnerFilter_ReturnsOnlyMatchingOwner()
        {
            var repo = CreateRepositoryWithMocks(SeedRequirements(), SeedProjectViews(), SeedTestorProducts());
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", "AB");

            Assert.NotNull(result);
            Assert.All(result.Data, row => Assert.Equal("AB", row.Owner));
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_SetsNormalPriceOnRows()
        {
            var repo = CreateRepositoryWithMocks(SeedRequirements(), SeedProjectViews(), SeedTestorProducts());
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.All(result.Data, row =>
            {
                var expected = row.IsDefraProject != 0 ? row.DefraUnitPrice : row.UnitPriceVla;
                Assert.Equal(expected, row.NormalPrice);
            });
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_SetsIsZeroPriceFlag()
        {
            var repo = CreateRepositoryWithMocks(SeedRequirements(), SeedProjectViews(), SeedTestorProducts());
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.All(result.Data, row =>
                Assert.Equal(row.TestPrice == 0m, row.IsZeroPrice));
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_NoMatchingData_ReturnsEmpty()
        {
            var repo = CreateRepositoryWithMocks();
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        #endregion

        #region GetTestPriceCheckByKeyAsync

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_ExistingKey_ReturnsRow()
        {
            var repo = CreateRepositoryWithMocks(SeedRequirements(), SeedProjectViews(), SeedTestorProducts());

            var result = await repo.GetTestPriceCheckByKeyAsync("T001", "JOB001");

            Assert.NotNull(result);
            Assert.Equal("T001",   result.TestCode);
            Assert.Equal("JOB001", result.JobCode);
        }

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_ExistingKey_SetsNormalPrice()
        {
            var repo = CreateRepositoryWithMocks(SeedRequirements(), SeedProjectViews(), SeedTestorProducts());

            var result = await repo.GetTestPriceCheckByKeyAsync("T001", "JOB001");

            Assert.NotNull(result);
            // T001/JOB001 — IsDefraProject=0 → NormalPrice = UnitPriceVla = 50m
            Assert.Equal(50m, result.NormalPrice);
        }

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_DefraProject_SetsNormalPriceToDefraUnitPrice()
        {
            var repo = CreateRepositoryWithMocks(SeedRequirements(), SeedProjectViews(), SeedTestorProducts());

            var result = await repo.GetTestPriceCheckByKeyAsync("T002", "JOB002");

            Assert.NotNull(result);
            // T002/JOB002 — IsDefraProject=-1 → NormalPrice = DefraUnitPrice = 120m
            Assert.Equal(120m, result.NormalPrice);
        }

        [Fact]
        public async Task GetTestPriceCheckByKeyAsync_NonExistentKey_ReturnsNull()
        {
            var repo = CreateRepositoryWithMocks(SeedRequirements(), SeedProjectViews(), SeedTestorProducts());

            var result = await repo.GetTestPriceCheckByKeyAsync("MISSING", "MISSING");

            Assert.Null(result);
        }

        #endregion

        #region UpdateTestPriceCheckAsync

        // ExecuteUpdateAsync issues a bulk SQL UPDATE and is not supported by the EF Core
        // in-memory provider. Update behaviour is tested at the service layer instead.
        // This test confirms the expected provider limitation is thrown, ensuring the method
        // is wired to ExecuteUpdateAsync (not a load-and-save pattern).
        [Fact]
        public async Task UpdateTestPriceCheckAsync_NotSupportedByInMemoryProvider_ThrowsInvalidOperationException()
        {
            var (context, repo) = CreateInMemoryContext();
            await SeedInMemoryAsync(context);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.UpdateTestPriceCheckAsync("T001", "JOB001", 0, 50m, 80m));
        }

        [Fact]
        public async Task UpdateTestPriceCheckAsync_DefraProject_ThrowsInvalidOperationException()
        {
            var (context, repo) = CreateInMemoryContext();
            await SeedInMemoryAsync(context);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.UpdateTestPriceCheckAsync("T001", "JOB001", -1, 40m, 90m));
        }

        [Fact]
        public async Task UpdateTestPriceCheckAsync_NullPrices_ThrowsInvalidOperationException()
        {
            var (context, repo) = CreateInMemoryContext();
            await SeedInMemoryAsync(context);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.UpdateTestPriceCheckAsync("T002", "JOB002", -1, null, null));
        }

        [Fact]
        public async Task UpdateTestPriceCheckAsync_NullTestPrice_ThrowsInvalidOperationException()
        {
            var (context, repo) = CreateInMemoryContext();
            await SeedInMemoryAsync(context);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => repo.UpdateTestPriceCheckAsync("T001", "JOB001", -1, null, 90m));
        }

        #endregion

        #region ApplyTestPriceCheckSorting

        [Theory]
        [InlineData("testcode",      false, "T001", "T002")]
        [InlineData("testcode",      true,  "T002", "T001")]
        [InlineData("jobcode",       false, "JOB001", "JOB002")]
        [InlineData("jobcode",       true,  "JOB002", "JOB001")]
        [InlineData("manager",       false, "Jones",  "Smith")]
        [InlineData("manager",       true,  "Smith",  "Jones")]
        [InlineData("program",       false, "PROG1",  "PROG2")]
        [InlineData("program",       true,  "PROG2",  "PROG1")]
        [InlineData("owner",         false, "AB", "CD")]
        [InlineData("owner",         true,  "CD", "AB")]
        [InlineData("notests",       false, "T001", "T002")]
        [InlineData("notests",       true,  "T002", "T001")]
        [InlineData("testprice",     false, "T002", "T001")]
        [InlineData("testprice",     true,  "T001", "T002")]
        [InlineData("unitpricevla",  false, "T001", "T002")]
        [InlineData("unitpricevla",  true,  "T002", "T001")]
        [InlineData("defraunitprice",false, "T001", "T002")]
        [InlineData("defraunitprice",true,  "T002", "T001")]
        [InlineData("normalprice",   false, "T001", "T002")]  // T001 VLA=50m, T002 Defra=120m
        [InlineData("normalprice",   true,  "T002", "T001")]
        [InlineData("unknown",       false, "T001", "T002")]  // default: sort by TestCode asc
        [InlineData("unknown",       true,  "T002", "T001")]  // default: sort by TestCode desc
        public async Task GetTestPriceCheckPagedAsync_Sorting_ReturnsExpectedOrder(
            string sortBy, bool descending, string expectedFirst, string expectedSecond)
        {
            // SeedRequirementsNonStandard: T001=60m (non-standard), T002=0m (zero)
            // → both pass the "all" price filter, giving two rows to verify order.
            var repo = CreateRepositoryWithMocks(
                SeedRequirementsNonStandard(), SeedProjectViews(), SeedTestorProducts());

            var parameters = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending
            };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.Equal(2, result.Data.Count);

            // Determine which field to read back depending on the sort column
            var firstValue = sortBy switch
            {
                "jobcode"        => result.Data.ElementAt(0).JobCode,
                "manager"        => result.Data.ElementAt(0).Manager,
                "program"        => result.Data.ElementAt(0).Program,
                "owner"          => result.Data.ElementAt(0).Owner,
                _                => result.Data.ElementAt(0).TestCode
            };
            var secondValue = sortBy switch
            {
                "jobcode"        => result.Data.ElementAt(1).JobCode,
                "manager"        => result.Data.ElementAt(1).Manager,
                "program"        => result.Data.ElementAt(1).Program,
                "owner"          => result.Data.ElementAt(1).Owner,
                _                => result.Data.ElementAt(1).TestCode
            };
            
            Assert.Equal(expectedFirst, firstValue);
            Assert.Equal(expectedSecond, secondValue);
        }

        #endregion

        #region ApplyTestPriceCheckFilter

        [Theory]
        [InlineData("TestCode", "T001", 1, "T001")]
        [InlineData("TestCode", "T002", 1, "T002")]
        [InlineData("TestCode", "NONE", 0, null)]
        public async Task GetTestPriceCheckPagedAsync_FilterByTestCode_ReturnsMatchingRows(
            string filterKey, string filterValue, int expectedCount, string? expectedTestCode)
        {
            var repo = CreateRepositoryWithMocks(
                SeedRequirementsNonStandard(), SeedProjectViews(), SeedTestorProducts());

            var filter = $"{{\"{filterKey}\":\"{filterValue}\"}}";
            var parameters = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10, Filter = filter
            };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.Equal(expectedCount, result.Data.Count);
            if (expectedTestCode != null)
                Assert.All(result.Data, row => Assert.Contains(filterValue, row.TestCode));
        }

        [Theory]
        [InlineData("JobCode", "JOB001", 1)]
        [InlineData("JobCode", "JOB002", 1)]
        [InlineData("JobCode", "NONE",   0)]
        public async Task GetTestPriceCheckPagedAsync_FilterByJobCode_ReturnsMatchingRows(
            string filterKey, string filterValue, int expectedCount)
        {
            var repo = CreateRepositoryWithMocks(
                SeedRequirementsNonStandard(), SeedProjectViews(), SeedTestorProducts());

            var filter = $"{{\"{filterKey}\":\"{filterValue}\"}}";
            var parameters = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10, Filter = filter
            };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.Equal(expectedCount, result.Data.Count);
        }

        [Theory]
        [InlineData("Owner", "AB", 1)]
        [InlineData("Owner", "CD", 1)]
        [InlineData("Owner", "ZZ", 0)]
        public async Task GetTestPriceCheckPagedAsync_FilterByOwner_ReturnsMatchingRows(
            string filterKey, string filterValue, int expectedCount)
        {
            var repo = CreateRepositoryWithMocks(
                SeedRequirementsNonStandard(), SeedProjectViews(), SeedTestorProducts());

            var filter = $"{{\"{filterKey}\":\"{filterValue}\"}}";
            var parameters = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10, Filter = filter
            };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.Equal(expectedCount, result.Data.Count);
        }

        [Theory]
        [InlineData("Program", "PROG1", 1)]
        [InlineData("Program", "PROG2", 1)]
        [InlineData("Program", "NONE",  0)]
        public async Task GetTestPriceCheckPagedAsync_FilterByProgram_ReturnsMatchingRows(
            string filterKey, string filterValue, int expectedCount)
        {
            var repo = CreateRepositoryWithMocks(
                SeedRequirementsNonStandard(), SeedProjectViews(), SeedTestorProducts());

            var filter = $"{{\"{filterKey}\":\"{filterValue}\"}}";
            var parameters = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10, Filter = filter
            };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.Equal(expectedCount, result.Data.Count);
        }

        [Theory]
        [InlineData("Manager", "Smith", 1)]
        [InlineData("Manager", "Jones", 1)]
        [InlineData("Manager", "NONE",  0)]
        public async Task GetTestPriceCheckPagedAsync_FilterByManager_ReturnsMatchingRows(
            string filterKey, string filterValue, int expectedCount)
        {
            var repo = CreateRepositoryWithMocks(
                SeedRequirementsNonStandard(), SeedProjectViews(), SeedTestorProducts());

            var filter = $"{{\"{filterKey}\":\"{filterValue}\"}}";
            var parameters = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10, Filter = filter
            };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.Equal(expectedCount, result.Data.Count);
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_NullFilter_ReturnsAllRows()
        {
            var repo = CreateRepositoryWithMocks(
                SeedRequirementsNonStandard(), SeedProjectViews(), SeedTestorProducts());

            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetTestPriceCheckPagedAsync_EmptyFilter_ReturnsAllRows()
        {
            var repo = CreateRepositoryWithMocks(
                SeedRequirementsNonStandard(), SeedProjectViews(), SeedTestorProducts());

            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "" };

            var result = await repo.GetTestPriceCheckPagedAsync(parameters, "all", null);

            Assert.Equal(2, result.Data.Count);
        }

        #endregion
    }
}
