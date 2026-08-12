using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.TestOrProductRepositoryTest
{
    public class TestFeePlanRepositoryTests
    {
        private const string UserEmail = "test.user@example.com";
        private const int DefaultFpsYear = 2024;

        // BuildTestFeePlanBaseQuery joins TestorProducts → TestRequirements → Projects → Programs
        // and filters by NoRequired != 0. The Moq path feeds real data through the async provider.
        private static TestorProductRepository CreateRepositoryWithMocks(
            IEnumerable<TestorProduct>?   testorProducts   = null,
            IEnumerable<TestRequirement>? testRequirements = null,
            IEnumerable<Project>?         projects         = null,
            IEnumerable<Program>?         programs         = null,
            int fpsYear = DefaultFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);
            fpsRequestContext.UserEmailId.Returns(UserEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var testorProductSet = RepositoryTestHelper.CreateMockDbSet(testorProducts   ?? []);
            var testReqSet       = RepositoryTestHelper.CreateMockDbSet(testRequirements ?? []);
            var projectSet       = RepositoryTestHelper.CreateMockDbSet(projects         ?? []);
            var programSet       = RepositoryTestHelper.CreateMockDbSet(programs         ?? []);

            RepositoryTestHelper.SetupDbSetOperations(testorProductSet);
            RepositoryTestHelper.SetupDbSetOperations(testReqSet);
            RepositoryTestHelper.SetupDbSetOperations(projectSet);
            RepositoryTestHelper.SetupDbSetOperations(programSet);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.TestorProducts).Returns(testorProductSet.Object);
            mockContext.Setup(x => x.TestRequirements).Returns(testReqSet.Object);
            mockContext.Setup(x => x.Projects).Returns(projectSet.Object);
            mockContext.Setup(x => x.Programs).Returns(programSet.Object);

            return new TestorProductRepository(mockContext.Object, fpsRequestContext);
        }

        // ── Shared seed data ──────────────────────────────────────────────────────
        // T001/JOB001/PROG1 — NoRequired = 5 (included), UnitPrice = 50 → TestFee = 250
        // T002/JOB002/PROG2 — NoRequired = 0 (excluded by NoRequired != 0)
        // T003/JOB002/PROG2 — NoRequired = 3 (included), UnitPrice = 20 → TestFee = 60
        private static IEnumerable<TestorProduct> SeedTestorProducts() =>
        [
            new TestorProduct { ItemCode = "T001", Owner = "AB", FpsYear = DefaultFpsYear },
            new TestorProduct { ItemCode = "T002", Owner = "CD", FpsYear = DefaultFpsYear },
            new TestorProduct { ItemCode = "T003", Owner = "EF", FpsYear = DefaultFpsYear }
        ];

        private static IEnumerable<TestRequirement> SeedRequirements() =>
        [
            new TestRequirement { TestCode = "T001", Buyer = "JOB001", NoRequired = 5, UnitPrice = 50m, FpsYear = DefaultFpsYear },
            new TestRequirement { TestCode = "T002", Buyer = "JOB002", NoRequired = 0, UnitPrice = 100m, FpsYear = DefaultFpsYear },
            new TestRequirement { TestCode = "T003", Buyer = "JOB002", NoRequired = 3, UnitPrice = 20m, FpsYear = DefaultFpsYear }
        ];

        private static IEnumerable<Project> SeedProjects() =>
        [
            new Project { ParentProject = "JOB001", Program = "PROG1", Customer = "CUST1", Contract = "CON1", ProjectStatus = "A", ProjectTitle = "P1", Disease = "D1", IncomeAccountCode = "I1", FpsYear = DefaultFpsYear },
            new Project { ParentProject = "JOB002", Program = "PROG2", Customer = "CUST2", Contract = "CON2", ProjectStatus = "B", ProjectTitle = "P2", Disease = "D2", IncomeAccountCode = "I2", FpsYear = DefaultFpsYear }
        ];

        private static IEnumerable<Program> SeedPrograms() =>
        [
            new Program { ProgramNo = "PROG1", Directorate = "Dir1", FpsYear = DefaultFpsYear },
            new Program { ProgramNo = "PROG2", Directorate = "Dir2", FpsYear = DefaultFpsYear }
        ];

        #region GetTestSnapshotPagedAsync

        [Fact]
        public async Task GetTestSnapshotPagedAsync_ReturnsMatchingRows()
        {
            var repo = CreateRepositoryWithMocks(SeedTestorProducts(), SeedRequirements(), SeedProjects(), SeedPrograms());
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetTestSnapshotPagedAsync(parameters);

            Assert.NotNull(result);
            // T001 and T003 pass NoRequired != 0; T002 is excluded.
            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetTestSnapshotPagedAsync_ExcludesZeroNoTestsRows()
        {
            var repo = CreateRepositoryWithMocks(SeedTestorProducts(), SeedRequirements(), SeedProjects(), SeedPrograms());
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetTestSnapshotPagedAsync(parameters);

            Assert.All(result.Data, row => Assert.NotEqual(0d, row.NoTests));
        }

        [Fact]
        public async Task GetTestSnapshotPagedAsync_SetsVersionAndTestFeeOnRows()
        {
            var repo = CreateRepositoryWithMocks(SeedTestorProducts(), SeedRequirements(), SeedProjects(), SeedPrograms());
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetTestSnapshotPagedAsync(parameters);

            var t001 = Assert.Single(result.Data, x => x.TestCode == "T001");
            Assert.StartsWith("Plan - ", t001.Version);
            Assert.Equal(250d, t001.TestFee);
        }

        [Fact]
        public async Task GetTestSnapshotPagedAsync_FilterByTestCode_ReturnsOnlyMatching()
        {
            var repo = CreateRepositoryWithMocks(SeedTestorProducts(), SeedRequirements(), SeedProjects(), SeedPrograms());
            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"TestCode\":\"T001\"}"
            };

            var result = await repo.GetTestSnapshotPagedAsync(parameters);

            var row = Assert.Single(result.Data);
            Assert.Equal("T001", row.TestCode);
        }

        [Theory]
        [InlineData("Directorate", "Dir1", "T001")]
        [InlineData("Customer", "CUST1", "T001")]
        [InlineData("Program", "PROG1", "T001")]
        [InlineData("Contract", "CON1", "T001")]
        [InlineData("Project", "JOB001", "T001")]
        [InlineData("Status", "A", "T001")]
        [InlineData("Owner", "AB", "T001")]
        [InlineData("Directorate", "Dir2", "T003")]
        [InlineData("Owner", "EF", "T003")]
        public async Task GetTestSnapshotPagedAsync_FilterByTextField_ReturnsOnlyMatching(
            string field, string value, string expectedTestCode)
        {
            var repo = CreateRepositoryWithMocks(SeedTestorProducts(), SeedRequirements(), SeedProjects(), SeedPrograms());
            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = $"{{\"{field}\":\"{value}\"}}"
            };

            var result = await repo.GetTestSnapshotPagedAsync(parameters);

            var row = Assert.Single(result.Data);
            Assert.Equal(expectedTestCode, row.TestCode);
        }

        [Fact]
        public async Task GetTestSnapshotPagedAsync_FilterByVersion_ReturnsMatchingRows()
        {
            var repo = CreateRepositoryWithMocks(SeedTestorProducts(), SeedRequirements(), SeedProjects(), SeedPrograms());
            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                // Version is projected as "Plan - ..."; a partial match should return the included rows.
                Filter = "{\"Version\":\"Plan\"}"
            };

            var result = await repo.GetTestSnapshotPagedAsync(parameters);

            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, row => Assert.StartsWith("Plan - ", row.Version));
        }

        [Fact]
        public async Task GetTestSnapshotPagedAsync_FilterByTextField_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepositoryWithMocks(SeedTestorProducts(), SeedRequirements(), SeedProjects(), SeedPrograms());
            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Owner\":\"ZZ\"}"
            };

            var result = await repo.GetTestSnapshotPagedAsync(parameters);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetTestSnapshotPagedAsync_EmptyFilter_ReturnsAllRows()
        {
            var repo = CreateRepositoryWithMocks(SeedTestorProducts(), SeedRequirements(), SeedProjects(), SeedPrograms());
            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = string.Empty
            };

            var result = await repo.GetTestSnapshotPagedAsync(parameters);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetTestSnapshotPagedAsync_InvalidTestFeeValue_IsIgnored()
        {
            var repo = CreateRepositoryWithMocks(SeedTestorProducts(), SeedRequirements(), SeedProjects(), SeedPrograms());
            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                // Non-numeric TestFee cannot be parsed and should be ignored, leaving all rows.
                Filter = "{\"TestFee\":\"abc\"}"
            };

            var result = await repo.GetTestSnapshotPagedAsync(parameters);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetTestSnapshotPagedAsync_FilterByTestFee_ReturnsRowsWithinTolerance()
        {
            var repo = CreateRepositoryWithMocks(SeedTestorProducts(), SeedRequirements(), SeedProjects(), SeedPrograms());
            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                // T001 has TestFee = 250; a near-exact value should still match via the tolerance range.
                Filter = "{\"TestFee\":\"249.9995\"}"
            };

            var result = await repo.GetTestSnapshotPagedAsync(parameters);

            var row = Assert.Single(result.Data);
            Assert.Equal("T001", row.TestCode);
            Assert.NotNull(row.TestFee);
            Assert.Equal(250d, row.TestFee!.Value, 3);
        }

        [Fact]
        public async Task GetTestSnapshotPagedAsync_FilterByTestFee_NoMatch_ReturnsEmpty()
        {
            var repo = CreateRepositoryWithMocks(SeedTestorProducts(), SeedRequirements(), SeedProjects(), SeedPrograms());
            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                // No seeded row has a TestFee anywhere near this value.
                Filter = "{\"TestFee\":\"9999\"}"
            };

            var result = await repo.GetTestSnapshotPagedAsync(parameters);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetTestSnapshotPagedAsync_SortByTestCodeDescending_OrdersRows()
        {
            var repo = CreateRepositoryWithMocks(SeedTestorProducts(), SeedRequirements(), SeedProjects(), SeedPrograms());
            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "testcode",
                Descending = true
            };

            var result = await repo.GetTestSnapshotPagedAsync(parameters);

            Assert.Equal("T003", result.Data.First().TestCode);
        }

        [Fact]
        public async Task GetTestSnapshotPagedAsync_NoMatchingData_ReturnsEmpty()
        {
            var repo = CreateRepositoryWithMocks();
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetTestSnapshotPagedAsync(parameters);

            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        #endregion
    }
}
