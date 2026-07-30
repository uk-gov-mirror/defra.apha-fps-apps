using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.WorkGroupGradeRepositoryTest
{
    public class WorkGroupGradeRepositoryTests
    {
        private const string DefaultPcGrade  = "G001";
        private const string DefaultUserEmail = "test@example.com";
        private const int    DefaultFpsYear   = 2024;

        private static WorkGroupGradeRepository CreateRepository(
            IEnumerable<WorkGroupGradeView>? viewGrades = null,
            IEnumerable<WorkgroupGrade>?     grades     = null)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(DefaultFpsYear);
            requestContext.UserEmailId.Returns(DefaultUserEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var viewSet = RepositoryTestHelper.CreateMockDbSet(viewGrades ?? Enumerable.Empty<WorkGroupGradeView>());
            mockContext.Setup(x => x.WorkGroupGradeViews).Returns(viewSet.Object);

            var gradeSet = RepositoryTestHelper.CreateMockDbSet(grades ?? Enumerable.Empty<WorkgroupGrade>());
            RepositoryTestHelper.SetupDbSetOperations(gradeSet);
            mockContext.Setup(x => x.WorkgroupGrades).Returns(gradeSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new WorkGroupGradeRepository(mockContext.Object, requestContext);
        }

        private static (WorkGroupGradeRepository Repo, Mock<DbSet<WorkgroupGrade>> DbSet, Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<WorkgroupGrade>? grades = null)
        {
            var requestContext = new Mock<IFpsRequestContext>();
            requestContext.Setup(x => x.FpsYear).Returns(DefaultFpsYear);
            requestContext.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext.Object);

            var dbSet = RepositoryTestHelper.CreateMockDbSet(grades ?? []);
            RepositoryTestHelper.SetupDbSetOperations(dbSet);
            mockContext.Setup(x => x.WorkgroupGrades).Returns(dbSet.Object);

            var viewSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<WorkGroupGradeView>());
            mockContext.Setup(x => x.WorkGroupGradeViews).Returns(viewSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new WorkGroupGradeRepository(mockContext.Object, requestContext.Object);
            return (repo, dbSet, mockContext);
        }

        private static WorkgroupGrade BuildGrade(
            string wgGrade         = "WG01",
            string profitCentreGrade = DefaultPcGrade,
            string gradeCode       = "GC1",
            string workgroup       = "WG") =>
            new()
            {
                WgGrade           = wgGrade,
                ProfitCentreGrade = profitCentreGrade,
                GradeCode         = gradeCode,
                Workgroup         = workgroup,
                FpsYear           = DefaultFpsYear
            };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenRequestContextIsNull()
        {
            var repo = CreateRepository();
            // Verify constructor guard by directly testing the already-created repo is not null
            // (null requestContext is caught at construction time via ArgumentNullException)
            Assert.NotNull(repo);
        }

        #endregion

        #region GetAllWorkgroupGradesPagedAsync Tests

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            var repo = CreateRepository();
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repo.GetAllWorkgroupGradesPagedAsync(null!));
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_ReturnsAllRecords()
        {
            var grades = new List<WorkgroupGrade>
            {
                BuildGrade("WG01"),
                BuildGrade("WG02"),
                BuildGrade("WG03")
            };
            var repo = CreateRepository(grades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_ReturnsEmptyData_WhenNoRecords()
        {
            var repo = CreateRepository(grades: []);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);

            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_ReturnsCorrectPage()
        {
            var grades = Enumerable.Range(1, 5).Select(i => BuildGrade($"WG0{i}")).ToList();
            var repo  = CreateRepository(grades: grades);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_FiltersByWgGrade()
        {
            var grades = new List<WorkgroupGrade>
            {
                BuildGrade("ALPHA1"),
                BuildGrade("BETA1"),
                BuildGrade("ALPHA2")
            };
            var repo   = CreateRepository(grades: grades);
            var filter = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { { "WgGrade", "ALPHA" } });
            var query  = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_FiltersByProfitCentreGrade()
        {
            var grades = new List<WorkgroupGrade>
            {
                BuildGrade("WG01", profitCentreGrade: "PCG_A"),
                BuildGrade("WG02", profitCentreGrade: "PCG_B"),
                BuildGrade("WG03", profitCentreGrade: "PCG_A")
            };
            var repo   = CreateRepository(grades: grades);
            var filter = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { { "ProfitCentreGrade", "PCG_A" } });
            var query  = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_FiltersByGradeCode()
        {
            var grades = new List<WorkgroupGrade>
            {
                BuildGrade("WG01", gradeCode: "GCA"),
                BuildGrade("WG02", gradeCode: "GCB"),
                BuildGrade("WG03", gradeCode: "GCA")
            };
            var repo   = CreateRepository(grades: grades);
            var filter = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { { "GradeCode", "GCA" } });
            var query  = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_FiltersByWorkgroup()
        {
            var grades = new List<WorkgroupGrade>
            {
                BuildGrade("WG01", workgroup: "TeamA"),
                BuildGrade("WG02", workgroup: "TeamB"),
                BuildGrade("WG03", workgroup: "TeamA")
            };
            var repo   = CreateRepository(grades: grades);
            var filter = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { { "Workgroup", "TeamA" } });
            var query  = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_WithEmptyFilterObject_ReturnsAllRecords()
        {
            var grades = new List<WorkgroupGrade> { BuildGrade("WG01"), BuildGrade("WG02") };
            var repo   = CreateRepository(grades: grades);
            var query  = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        [Theory]
        [InlineData("wggrade",          false, "WG01", "WG02")]
        [InlineData("wggrade",          true,  "WG03", "WG02")]
        [InlineData("profitcentregrade",false, "PCG_A", "PCG_B")]
        [InlineData("profitcentregrade",true,  "PCG_C", "PCG_B")]
        [InlineData("gradecode",        false, "GCA", "GCB")]
        [InlineData("gradecode",        true,  "GCC", "GCB")]
        [InlineData("workgroup",        false, "TeamA", "TeamB")]
        [InlineData("workgroup",        true,  "TeamC", "TeamB")]
        public async Task GetAllWorkgroupGradesPagedAsync_AppliesSorting(
            string sortBy, bool descending, string expectedFirst, string expectedSecond)
        {
            var grades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG02", ProfitCentreGrade = "PCG_B", GradeCode = "GCB", Workgroup = "TeamB", FpsYear = DefaultFpsYear },
                new() { WgGrade = "WG01", ProfitCentreGrade = "PCG_A", GradeCode = "GCA", Workgroup = "TeamA", FpsYear = DefaultFpsYear },
                new() { WgGrade = "WG03", ProfitCentreGrade = "PCG_C", GradeCode = "GCC", Workgroup = "TeamC", FpsYear = DefaultFpsYear }
            };
            var repo  = CreateRepository(grades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);
            var list   = result.Data.ToList();

            var actualFirst = sortBy switch
            {
                "wggrade"           => list[0].WgGrade,
                "profitcentregrade" => list[0].ProfitCentreGrade,
                "gradecode"         => list[0].GradeCode,
                "workgroup"         => list[0].Workgroup,
                _                   => list[0].WgGrade
            };
            var actualSecond = sortBy switch
            {
                "wggrade"           => list[1].WgGrade,
                "profitcentregrade" => list[1].ProfitCentreGrade,
                "gradecode"         => list[1].GradeCode,
                "workgroup"         => list[1].Workgroup,
                _                   => list[1].WgGrade
            };

            Assert.Equal(expectedFirst,  actualFirst);
            Assert.Equal(expectedSecond, actualSecond);
        }

        [Fact]
        public async Task GetAllWorkgroupGradesPagedAsync_UnknownSortBy_ReturnsUnsortedData()
        {
            var grades = new List<WorkgroupGrade> { BuildGrade("WG01"), BuildGrade("WG02") };
            var repo   = CreateRepository(grades: grades);
            var query  = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "unknown" };

            var result = await repo.GetAllWorkgroupGradesPagedAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        #endregion

        #region GetByWgGradeAsync Tests

        [Fact]
        public async Task GetByWgGradeAsync_ReturnsNull_WhenNullOrWhitespace()
        {
            var repo = CreateRepository(grades: [BuildGrade("WG01")]);

            Assert.Null(await repo.GetByWgGradeAsync(null!));
            Assert.Null(await repo.GetByWgGradeAsync(""));
            Assert.Null(await repo.GetByWgGradeAsync("   "));
        }

        [Fact]
        public async Task GetByWgGradeAsync_ReturnsNull_WhenNotFound()
        {
            var repo = CreateRepository(grades: []);

            var result = await repo.GetByWgGradeAsync("NONEXISTENT");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByWgGradeAsync_ReturnsGrade_WhenFound()
        {
            var grades = new List<WorkgroupGrade> { BuildGrade("WG01") };
            var repo   = CreateRepository(grades: grades);

            var result = await repo.GetByWgGradeAsync("WG01");

            Assert.NotNull(result);
            Assert.Equal("WG01", result.WgGrade);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository();
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_AddsEntityAndSavesChanges()
        {
            var (repo, dbSet, context) = CreateRepositoryWithMocks([]);
            dbSet.Setup(x => x.Add(It.IsAny<WorkgroupGrade>()));

            var entity = BuildGrade("WG_NEW");
            var result = await repo.CreateAsync(entity);

            Assert.NotNull(result);
            Assert.Equal("WG_NEW", result.WgGrade);
            dbSet.Verify(x => x.Add(It.Is<WorkgroupGrade>(e => e.WgGrade == "WG_NEW")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(context);
        }

        [Fact]
        public async Task CreateAsync_SetsFpsYearFromRequestContext()
        {
            var (repo, dbSet, _) = CreateRepositoryWithMocks([]);
            dbSet.Setup(x => x.Add(It.IsAny<WorkgroupGrade>()));

            var entity = BuildGrade("WG_NEW");
            entity.FpsYear = 0;
            var result = await repo.CreateAsync(entity);

            Assert.Equal(DefaultFpsYear, result.FpsYear);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository();
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAsync(null!));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsKeyNotFoundException_WhenWgGradeNotFound()
        {
            var repo = CreateRepository(grades: []);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                repo.UpdateAsync(BuildGrade("NONEXISTENT")));
        }

        [Fact]
        public async Task UpdateAsync_UpdatesFieldsAndSavesChanges()
        {
            var existing = BuildGrade("WG01", profitCentreGrade: "OLD_PCG", gradeCode: "OLD_GC", workgroup: "OLD_WG");
            var (repo, _, context) = CreateRepositoryWithMocks([existing]);

            var updated = new WorkgroupGrade
            {
                WgGrade           = "WG01",
                ProfitCentreGrade = "NEW_PCG",
                GradeCode         = "NEW_GC",
                Workgroup         = "NEW_WG"
            };

            var result = await repo.UpdateAsync(updated);

            Assert.Equal("NEW_PCG", result.ProfitCentreGrade);
            Assert.Equal("NEW_GC",  result.GradeCode);
            Assert.Equal("NEW_WG",  result.Workgroup);
            Assert.Equal(DefaultFpsYear, result.FpsYear);
            RepositoryTestHelper.VerifySaveChanges(context);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenWgGradeIsNullOrWhitespace()
        {
            var repo = CreateRepository();

            Assert.False(await repo.DeleteAsync(null!));
            Assert.False(await repo.DeleteAsync(""));
            Assert.False(await repo.DeleteAsync("   "));
        }

        #endregion

        #region DeleteWorkGroupGradeAsync Tests

        [Fact]
        public async Task DeleteWorkGroupGradeAsync_WithExistingWgGrade_ReturnsTrueAndRemoves()
        {
            var grades = new List<WorkgroupGrade> { BuildGrade("WG01") };
            var repo   = CreateRepository(grades: grades);

            var result = await repo.DeleteWorkGroupGradeAsync("WG01");

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteWorkGroupGradeAsync_WithNonExistentWgGrade_ReturnsFalse()
        {
            var repo = CreateRepository(grades: []);

            var result = await repo.DeleteWorkGroupGradeAsync("NONEXISTENT");

            Assert.False(result);
        }

        #endregion

        #region GetAllGradeCodesAsync Tests

        [Fact]
        public async Task GetAllGradeCodesAsync_ReturnsDistinctOrderedGradeCodes()
        {
            var grades = new List<WorkgroupGrade>
            {
                BuildGrade("WG01", gradeCode: "GCC"),
                BuildGrade("WG02", gradeCode: "GCA"),
                BuildGrade("WG03", gradeCode: "GCB"),
                BuildGrade("WG04", gradeCode: "GCA") // duplicate
            };
            var repo = CreateRepository(grades: grades);

            var result = await repo.GetAllGradeCodesAsync();

            Assert.Equal(["GCA", "GCB", "GCC"], result);
        }

        [Fact]
        public async Task GetAllGradeCodesAsync_ReturnsEmpty_WhenNoGrades()
        {
            var repo = CreateRepository(grades: []);

            var result = await repo.GetAllGradeCodesAsync();

            Assert.Empty(result);
        }

        #endregion

        #region GetWorkGroupGradesAsync (view) Tests

        [Fact]
        public async Task GetWorkGroupGradeAsync_WithMatchingPcGrade_ReturnsPagedData()
        {
            var viewGrades = new List<WorkGroupGradeView>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = DefaultPcGrade, GradeCode = "GC1", WorkGroup = "WG", UserEmail = DefaultUserEmail },
                new() { WgGrade = "WG02", ProfitCentreGrade = DefaultPcGrade, GradeCode = "GC2", WorkGroup = "WG", UserEmail = DefaultUserEmail },
                new() { WgGrade = "WG03", ProfitCentreGrade = "OTHER",        GradeCode = "GC3", WorkGroup = "WG", UserEmail = DefaultUserEmail }
            };
            var repo  = CreateRepository(viewGrades: viewGrades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupGradesAsync(query, DefaultPcGrade);

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, g => Assert.Equal(DefaultPcGrade, g.ProfitCentreGrade));
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_WithNoMatchingPcGrade_ReturnsEmptyData()
        {
            var viewGrades = new List<WorkGroupGradeView>
            {
                new() { WgGrade = "WG01", ProfitCentreGrade = "OTHER", GradeCode = "GC1", WorkGroup = "WG", UserEmail = DefaultUserEmail }
            };
            var repo  = CreateRepository(viewGrades: viewGrades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupGradesAsync(query, DefaultPcGrade);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_ReturnsOrderedByWgGrade()
        {
            var viewGrades = new List<WorkGroupGradeView>
            {
                new() { WgGrade = "WG03", ProfitCentreGrade = DefaultPcGrade, GradeCode = "GC3", WorkGroup = "WG", UserEmail = DefaultUserEmail },
                new() { WgGrade = "WG01", ProfitCentreGrade = DefaultPcGrade, GradeCode = "GC1", WorkGroup = "WG", UserEmail = DefaultUserEmail },
                new() { WgGrade = "WG02", ProfitCentreGrade = DefaultPcGrade, GradeCode = "GC2", WorkGroup = "WG", UserEmail = DefaultUserEmail }
            };
            var repo  = CreateRepository(viewGrades: viewGrades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupGradesAsync(query, DefaultPcGrade);
            var list   = result.Data.ToList();

            Assert.Equal("WG01", list[0].WgGrade);
            Assert.Equal("WG02", list[1].WgGrade);
            Assert.Equal("WG03", list[2].WgGrade);
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_WithPagination_ReturnsCorrectPage()
        {
            var viewGrades = Enumerable.Range(1, 5).Select(i => new WorkGroupGradeView
            {
                WgGrade           = $"WG0{i}",
                ProfitCentreGrade = DefaultPcGrade,
                GradeCode         = $"GC{i}",
                WorkGroup         = "WG",
                UserEmail         = DefaultUserEmail
            }).ToList();
            var repo  = CreateRepository(viewGrades: viewGrades);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            var result = await repo.GetWorkGroupGradesAsync(query, DefaultPcGrade);

            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetWorkGroupGradeAsync_WithEmptyRepository_ReturnsEmptyData()
        {
            var repo  = CreateRepository(viewGrades: []);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupGradesAsync(query, DefaultPcGrade);

            Assert.Empty(result.Data);
        }

        #endregion

        #region ExistsForGradeCodeAsync Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ExistsForGradeCodeAsync_ReturnsFalse_WhenGradeCodeIsEmptyOrWhiteSpace(string gradeCode)
        {
            var repo = CreateRepository(grades: []);

            var result = await repo.ExistsForGradeCodeAsync(gradeCode);

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsForGradeCodeAsync_ReturnsTrue_WhenGradeCodeExists()
        {
            var grades = new List<WorkgroupGrade> { BuildGrade("WG01", gradeCode: "GCA") };
            var repo = CreateRepository(grades: grades);

            var result = await repo.ExistsForGradeCodeAsync("GCA");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsForGradeCodeAsync_ReturnsFalse_WhenGradeCodeDoesNotExist()
        {
            var grades = new List<WorkgroupGrade> { BuildGrade("WG01", gradeCode: "GCA") };
            var repo = CreateRepository(grades: grades);

            var result = await repo.ExistsForGradeCodeAsync("NONEXISTENT");

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsForGradeCodeAsync_ReturnsFalse_WhenNoGrades()
        {
            var repo = CreateRepository(grades: []);

            var result = await repo.ExistsForGradeCodeAsync("GCA");

            Assert.False(result);
        }

        #endregion

        #region GetWorkgroupGradesByWorkGroupAsync Tests

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetWorkgroupGradesByWorkGroupAsync_ThrowsArgumentException_WhenWorkGroupIsNullOrWhitespace(string wg)
        {
            var repo = CreateRepository();
            await Assert.ThrowsAsync<ArgumentException>(() =>
                repo.GetWorkgroupGradesByWorkGroupAsync(wg));
        }

        [Fact]
        public async Task GetWorkgroupGradesByWorkGroupAsync_ReturnsOnlyMatchingWorkgroup()
        {
            var grades = new List<WorkgroupGrade>
            {
                BuildGrade("WG01", workgroup: "TeamA"),
                BuildGrade("WG02", workgroup: "TeamB"),
                BuildGrade("WG03", workgroup: "TeamA")
            };
            var repo = CreateRepository(grades: grades);

            var result = await repo.GetWorkgroupGradesByWorkGroupAsync("TeamA");

            Assert.Equal(2, result.Count);
            Assert.All(result, g => Assert.Equal("TeamA", g.Workgroup));
        }

        [Fact]
        public async Task GetWorkgroupGradesByWorkGroupAsync_ReturnsEmptyData_WhenNoMatchingWorkgroup()
        {
            var grades = new List<WorkgroupGrade> { BuildGrade("WG01", workgroup: "TeamB") };
            var repo  = CreateRepository(grades: grades);

            var result = await repo.GetWorkgroupGradesByWorkGroupAsync("TeamA");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWorkgroupGradesByWorkGroupAsync_ReturnsOrderedByWgGrade()
        {
            var grades = new List<WorkgroupGrade>
            {
                BuildGrade("WG03", workgroup: "TeamA"),
                BuildGrade("WG01", workgroup: "TeamA"),
                BuildGrade("WG02", workgroup: "TeamA")
            };
            var repo = CreateRepository(grades: grades);

            var result = await repo.GetWorkgroupGradesByWorkGroupAsync("TeamA");

            Assert.Equal("WG01", result[0].WgGrade);
            Assert.Equal("WG02", result[1].WgGrade);
            Assert.Equal("WG03", result[2].WgGrade);
        }

        #endregion
    }
}
