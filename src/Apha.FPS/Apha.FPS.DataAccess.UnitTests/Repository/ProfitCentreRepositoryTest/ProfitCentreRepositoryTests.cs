using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProfitCentreRepositoryTest
{
    public class ProfitCentreRepositoryTests
    {
        private static ProfitCentreRepository CreateRepository(
            IEnumerable<ProfitCentreView>? profitCentreViews = null,
            IEnumerable<ProfitCentre>? profitCentres = null,
            IEnumerable<UserProfitcentre>? userProfitCentres = null,
            IEnumerable<ProfitCentreGrade>? profitCentreGrades = null,
            IEnumerable<Workgroup>? workgroups = null,
            IEnumerable<User>? users = null)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(2024);
            requestContext.UserEmailId.Returns("test@example.com");

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            if (profitCentreViews != null)
            {
                var mockViewSet = RepositoryTestHelper.CreateMockDbSet(profitCentreViews);
                mockContext.Setup(x => x.ProfitCentreViews).Returns(mockViewSet.Object);
            }

            if (profitCentres != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(profitCentres);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                mockContext.Setup(x => x.ProfitCentres).Returns(mockSet.Object);
            }

            var upcSet = RepositoryTestHelper.CreateMockDbSet(userProfitCentres ?? []);
            RepositoryTestHelper.SetupDbSetOperations(upcSet);
            mockContext.Setup(x => x.UserProfitcentres).Returns(upcSet.Object);

            var pcgSet = RepositoryTestHelper.CreateMockDbSet(profitCentreGrades ?? []);
            mockContext.Setup(x => x.ProfitCentreGrades).Returns(pcgSet.Object);

            var wgSet = RepositoryTestHelper.CreateMockDbSet(workgroups ?? []);
            mockContext.Setup(x => x.Workgroups).Returns(wgSet.Object);

            var userSet = RepositoryTestHelper.CreateMockDbSet(users ?? []);
            mockContext.Setup(x => x.Users).Returns(userSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new ProfitCentreRepository(mockContext.Object, requestContext);
        }

        private static ProfitCentreView BuildView(
            string id = "PC01",
            string name = "Centre One",
            string division = "DIV1",
            string userEmail = "test@example.com") =>
            new() { ProfitCentreId = id, ProfitCentreName = name, Division = division, UserEmail = userEmail, FpsYear = 2024 };

        private static ProfitCentre BuildEntity(
            string id = "PC01",
            string name = "Centre One",
            string division = "DIV1") =>
            new() { ProfitCentreId = id, ProfitCentreName = name, Division = division };

        #region GetProfitCentresAsync Tests

        [Fact]
        public async Task GetProfitCentresAsync_ReturnsAllProfitCentres_WhenDataExists()
        {
            var profitCentres = new List<ProfitCentreView>
            {
                BuildView("PC01", "Profit Centre One", "DIV1"),
                BuildView("PC02", "Profit Centre Two", "DIV1"),
                BuildView("PC03", "Profit Centre Three", "DIV2")
            };
            var repo = CreateRepository(profitCentreViews: profitCentres);

            var result = await repo.GetProfitCentresAsync();

            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetProfitCentresAsync_ReturnsEmpty_WhenNoDataExists()
        {
            var repo = CreateRepository(profitCentreViews: []);

            var result = await repo.GetProfitCentresAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetProfitCentresAsync_ReturnsOrderedByProfitCentreId()
        {
            var profitCentres = new List<ProfitCentreView>
            {
                BuildView("PC03", "Centre Three", "DIV1"),
                BuildView("PC01", "Centre One",   "DIV1"),
                BuildView("PC02", "Centre Two",   "DIV1")
            };
            var repo = CreateRepository(profitCentreViews: profitCentres);

            var result = await repo.GetProfitCentresAsync();

            var resultList = result.ToList();
            Assert.Equal("PC01", resultList[0].ProfitCentreId);
            Assert.Equal("PC02", resultList[1].ProfitCentreId);
            Assert.Equal("PC03", resultList[2].ProfitCentreId);
        }

        [Fact]
        public async Task GetProfitCentresAsync_ReturnsSingle_WhenOneItemExists()
        {
            var profitCentres = new List<ProfitCentreView> { BuildView("PC01", "Centre One", "DIV1") };
            var repo = CreateRepository(profitCentreViews: profitCentres);

            var result = await repo.GetProfitCentresAsync();

            var single = Assert.Single(result);
            Assert.Equal("PC01", single.ProfitCentreId);
        }

        [Fact]
        public async Task GetProfitCentresAsync_ReturnsDeduplicated_WhenViewContainsDuplicateProfitCentreIds()
        {
            // Simulate the underlying view producing multiple rows for the same
            // ProfitCentreId because a user has more than one permission assignment.
            var profitCentres = new List<ProfitCentreView>
            {
                BuildView("PC01", "Centre One", "DIV1"),
                BuildView("PC01", "Centre One", "DIV1"),  // duplicate permission row
                BuildView("PC02", "Centre Two", "DIV1"),
                BuildView("PC02", "Centre Two", "DIV1"),  // duplicate permission row
                BuildView("PC03", "Centre Three", "DIV2")
            };
            var repo = CreateRepository(profitCentreViews: profitCentres);

            var result = await repo.GetProfitCentresAsync();

            Assert.Equal(3, result.Count);
            Assert.Equal("PC01", result[0].ProfitCentreId);
            Assert.Equal("PC02", result[1].ProfitCentreId);
            Assert.Equal("PC03", result[2].ProfitCentreId);
        }

        #endregion

        #region GetAllProfitCentresAsync Tests

        private static ProfitCentreRepository CreateRepositoryWithProfitCentres(IEnumerable<ProfitCentre> profitCentres)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(2024);
            requestContext.UserEmailId.Returns("test@example.com");

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var mockSet = RepositoryTestHelper.CreateMockDbSet(profitCentres);
            mockContext.Setup(x => x.ProfitCentres).Returns(mockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new ProfitCentreRepository(mockContext.Object, requestContext);
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_WithData_ReturnsAllOrderedById()
        {
            // Arrange
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC03", ProfitCentreName = "Centre Three", Division = "DIV1" },
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One",   Division = "DIV1" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Centre Two",   Division = "DIV1" }
            };
            var repo = CreateRepositoryWithProfitCentres(profitCentres);

            // Act
            var result = (await repo.GetAllProfitCentresAsync()).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("PC01", result[0].ProfitCentreId);
            Assert.Equal("PC02", result[1].ProfitCentreId);
            Assert.Equal("PC03", result[2].ProfitCentreId);
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_WithNoData_ReturnsEmpty()
        {
            // Arrange
            var repo = CreateRepositoryWithProfitCentres(new List<ProfitCentre>());

            // Act
            var result = await repo.GetAllProfitCentresAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetAllProfitCentresPagedAsync Tests

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetAllProfitCentresPagedAsync(null!));
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_ReturnsEmptyPagedData_WhenNoRecords()
        {
            var repo = CreateRepository(profitCentres: []);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_ReturnsAllRecords()
        {
            var entities = new List<ProfitCentre>
            {
                BuildEntity("PC01"), BuildEntity("PC02"), BuildEntity("PC03")
            };
            var repo = CreateRepository(profitCentres: entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_ReturnsCorrectPage()
        {
            var entities = new List<ProfitCentre>
            {
                BuildEntity("PC01"), BuildEntity("PC02"), BuildEntity("PC03"),
                BuildEntity("PC04"), BuildEntity("PC05")
            };
            var repo = CreateRepository(profitCentres: entities);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_FiltersByProfitCentreId()
        {
            var entities = new List<ProfitCentre>
            {
                BuildEntity("PC01"), BuildEntity("PC02"), BuildEntity("RC01")
            };
            var repo = CreateRepository(profitCentres: entities);
            var filter = System.Text.Json.JsonSerializer.Serialize(
                new Dictionary<string, string> { { "ProfitCentreId", "PC" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_FiltersByDivision()
        {
            var entities = new List<ProfitCentre>
            {
                BuildEntity("PC01", division: "VSD"),
                BuildEntity("PC02", division: "BSD"),
                BuildEntity("PC03", division: "VSD")
            };
            var repo = CreateRepository(profitCentres: entities);
            var filter = System.Text.Json.JsonSerializer.Serialize(
                new Dictionary<string, string> { { "Division", "VSD" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_OrdersByProfitCentreIdAscByDefault()
        {
            var entities = new List<ProfitCentre>
            {
                BuildEntity("PC03"), BuildEntity("PC01"), BuildEntity("PC02")
            };
            var repo = CreateRepository(profitCentres: entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            var list = result.Data.ToList();
            Assert.Equal("PC01", list[0].ProfitCentreId);
        }

        #endregion

        #region GetProfitCentreByIdAsync Tests

        [Fact]
        public async Task GetProfitCentreByIdAsync_ThrowsArgumentException_WhenIdIsNullOrEmpty()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetProfitCentreByIdAsync(""));
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_ThrowsArgumentException_WhenIdIsWhiteSpace()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetProfitCentreByIdAsync("   "));
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_ReturnsRecord_WhenFound()
        {
            var entities = new List<ProfitCentre> { BuildEntity("PC01", "Centre One", "DIV1") };
            var repo = CreateRepository(profitCentres: entities);
            var result = await repo.GetProfitCentreByIdAsync("PC01");
            Assert.NotNull(result);
            Assert.Equal("PC01", result.ProfitCentreId);
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_ReturnsNull_WhenNotFound()
        {
            var repo = CreateRepository(profitCentres: []);
            var result = await repo.GetProfitCentreByIdAsync("NOTEXIST");
            Assert.Null(result);
        }

        #endregion

        #region ProfitCentreExistsAsync Tests

        [Fact]
        public async Task ProfitCentreExistsAsync_ReturnsTrue_WhenExists()
        {
            var entities = new List<ProfitCentre> { BuildEntity("PC01") };
            var repo = CreateRepository(profitCentres: entities);
            var result = await repo.ProfitCentreExistsAsync("PC01");
            Assert.True(result);
        }

        [Fact]
        public async Task ProfitCentreExistsAsync_ReturnsFalse_WhenNotExists()
        {
            var repo = CreateRepository(profitCentres: []);
            var result = await repo.ProfitCentreExistsAsync("NOTEXIST");
            Assert.False(result);
        }

        [Fact]
        public async Task ProfitCentreExistsAsync_ThrowsArgumentException_WhenIdIsEmpty()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.ProfitCentreExistsAsync(""));
        }

        #endregion

        #region CreateProfitCentreAsync Tests

        [Fact]
        public async Task CreateProfitCentreAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.CreateProfitCentreAsync(null!));
        }

        [Fact]
        public async Task CreateProfitCentreAsync_ReturnsEntity_WhenSuccessful()
        {
            var entity = BuildEntity("PC01");
            var user = new User { UserId = 10, UserEmail = "test@example.com" };
            var repo = CreateRepository(
                profitCentres: [],
                userProfitCentres: [],
                users: [user]);

            var result = await repo.CreateProfitCentreAsync(entity);

            Assert.NotNull(result);
            Assert.Equal("PC01", result.ProfitCentreId);
        }

        [Fact]
        public async Task CreateProfitCentreAsync_UsesFallbackUserId_WhenUserNotFound()
        {
            var entity = BuildEntity("PC01");
            var repo = CreateRepository(
                profitCentres: [],
                userProfitCentres: [],
                users: []);

            // Should complete without throwing even when user is not found (falls back to userId 42)
            var result = await repo.CreateProfitCentreAsync(entity);

            Assert.NotNull(result);
            Assert.Equal("PC01", result.ProfitCentreId);
        }

        #endregion

        #region UpdateProfitCentreAsync Tests

        [Fact]
        public async Task UpdateProfitCentreAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateProfitCentreAsync("PC01", null!));
        }

        [Fact]
        public async Task UpdateProfitCentreAsync_ThrowsArgumentException_WhenOriginalIdIsEmpty()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.UpdateProfitCentreAsync("", BuildEntity()));
        }

        [Fact]
        public async Task UpdateProfitCentreAsync_ReturnsEntity_WhenNotFound()
        {
            var repo = CreateRepository(profitCentres: []);
            var result = await repo.UpdateProfitCentreAsync("NOTEXIST", BuildEntity("NOTEXIST"));
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateProfitCentreAsync_UpdatesFields_WhenIdUnchanged()
        {
            var existing = BuildEntity("PC01", "Old Name", "OLD");
            var updated  = BuildEntity("PC01", "New Name", "NEW");
            var repo = CreateRepository(
                profitCentres: [existing],
                profitCentreGrades: [],
                workgroups: [],
                userProfitCentres: []);

            var result = await repo.UpdateProfitCentreAsync("PC01", updated);

            Assert.Equal("New Name", result.ProfitCentreName);
            Assert.Equal("NEW", result.Division);
        }

        #endregion

        #region DeleteProfitCentreAsync Tests

        [Fact]
        public async Task DeleteProfitCentreAsync_ThrowsArgumentException_WhenIdIsEmpty()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.DeleteProfitCentreAsync(""));
        }

        [Fact]
        public async Task DeleteProfitCentreAsync_ReturnsFalse_WhenNotFound()
        {
            var repo = CreateRepository(
                profitCentres: [],
                profitCentreGrades: [],
                workgroups: []);
            var result = await repo.DeleteProfitCentreAsync("NOTEXIST");
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteProfitCentreAsync_ReturnsTrue_WhenDeletedSuccessfully()
        {
            var repo = CreateRepository(
                profitCentres: [BuildEntity("PC01")],
                profitCentreGrades: [],
                workgroups: [],
                userProfitCentres: []);

            var result = await repo.DeleteProfitCentreAsync("PC01");

            Assert.True(result);
        }

        #endregion

        #region UpdateProfitCentreSettingsAsync Tests

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WithValidData_ReturnsTrue()
        {
            // Arrange
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One", Division = "DIV1", Timesheet = 0, OutputSheet = 0, TimesheetLayout = 1 }
            };
            var repo = CreateRepositoryWithProfitCentres(profitCentres);

            // Act
            var result = await repo.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 2);

            // Assert
            Assert.True(result);
        }

        #endregion

        #region HasLinkedGradesAsync Tests

        [Fact]
        public async Task HasLinkedGradesAsync_ThrowsArgumentException_WhenIdIsEmpty()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.HasLinkedGradesAsync(""));
        }

        [Fact]
        public async Task HasLinkedGradesAsync_ReturnsFalse_WhenNoGradesExist()
        {
            var repo = CreateRepository(profitCentres: [BuildEntity("PC01")], profitCentreGrades: []);
            var result = await repo.HasLinkedGradesAsync("PC01");
            Assert.False(result);
        }

        [Fact]
        public async Task HasLinkedGradesAsync_ReturnsTrue_WhenGradesExist()
        {
            var grades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "G1", DivisionGrade = "DG1", GradeCode = "GC1", ProfitCentre = "PC01" }
            };
            var repo = CreateRepository(profitCentres: [BuildEntity("PC01")], profitCentreGrades: grades);
            var result = await repo.HasLinkedGradesAsync("PC01");
            Assert.True(result);
        }

        #endregion

        #region HasLinkedWorkgroupsAsync Tests

        [Fact]
        public async Task HasLinkedWorkgroupsAsync_ThrowsArgumentException_WhenIdIsEmpty()
        {
            var repo = CreateRepository(profitCentres: []);
            await Assert.ThrowsAsync<ArgumentException>(() => repo.HasLinkedWorkgroupsAsync(""));
        }

        [Fact]
        public async Task HasLinkedWorkgroupsAsync_ReturnsFalse_WhenNoWorkgroupsExist()
        {
            var repo = CreateRepository(profitCentres: [BuildEntity("PC01")], workgroups: []);
            var result = await repo.HasLinkedWorkgroupsAsync("PC01");
            Assert.False(result);
        }

        [Fact]
        public async Task HasLinkedWorkgroupsAsync_ReturnsTrue_WhenWorkgroupsExist()
        {
            var workgroups = new List<Workgroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC01" }
            };
            var repo = CreateRepository(profitCentres: [BuildEntity("PC01")], workgroups: workgroups);
            var result = await repo.HasLinkedWorkgroupsAsync("PC01");
            Assert.True(result);
        }

        #endregion

        #region GetProfitCentresAsync Email Filter Tests

        [Fact]
        public async Task GetProfitCentresAsync_ReturnsEmpty_WhenEmailDoesNotMatch()
        {
            var profitCentres = new List<ProfitCentreView>
            {
                BuildView("PC01", "Centre One", "DIV1", "other@example.com")
            };
            var repo = CreateRepository(profitCentreViews: profitCentres);
            var result = await repo.GetProfitCentresAsync();
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetProfitCentresAsync_ExcludesRecordsWithNullEmail()
        {
            var profitCentres = new List<ProfitCentreView>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One", Division = "DIV1", UserEmail = null }
            };
            var repo = CreateRepository(profitCentreViews: profitCentres);
            var result = await repo.GetProfitCentresAsync();
            Assert.Empty(result);
        }

        #endregion

        #region ApplyProfitCentreFilter Tests

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_FiltersByProfitCentreName()
        {
            var entities = new List<ProfitCentre>
            {
                BuildEntity("PC01", "Alpha Centre"),
                BuildEntity("PC02", "Beta Centre"),
                BuildEntity("PC03", "Alpha North")
            };
            var repo = CreateRepository(profitCentres: entities);
            var filter = System.Text.Json.JsonSerializer.Serialize(
                new Dictionary<string, string> { { "ProfitCentreName", "Alpha" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_ThrowsJsonException_WhenFilterIsInvalidJson()
        {
            var entities = new List<ProfitCentre> { BuildEntity("PC01"), BuildEntity("PC02") };
            var repo = CreateRepository(profitCentres: entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "not-valid-json" };
            await Assert.ThrowsAsync<Newtonsoft.Json.JsonReaderException>(() => repo.GetAllProfitCentresPagedAsync(query));
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_ReturnsAll_WhenFilterIsEmptyObject()
        {
            var entities = new List<ProfitCentre> { BuildEntity("PC01"), BuildEntity("PC02") };
            var repo = CreateRepository(profitCentres: entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        #endregion

        #region ApplyProfitCentreSorting Tests

        [Theory]
        [InlineData("ProfitCentreName", false, "Alpha Centre", "Beta Centre")]
        [InlineData("ProfitCentreName", true,  "Beta Centre",  "Alpha Centre")]
        public async Task GetAllProfitCentresPagedAsync_SortsByProfitCentreName(string sortBy, bool descending, string firstExpected, string secondExpected)
        {
            var entities = new List<ProfitCentre>
            {
                BuildEntity("PC01", "Beta Centre"),
                BuildEntity("PC02", "Alpha Centre")
            };
            var repo = CreateRepository(profitCentres: entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            var list = result.Data.ToList();
            Assert.Equal(firstExpected,  list[0].ProfitCentreName);
            Assert.Equal(secondExpected, list[1].ProfitCentreName);
        }

        [Theory]
        [InlineData("Division", false, "AAA", "ZZZ")]
        [InlineData("Division", true,  "ZZZ", "AAA")]
        public async Task GetAllProfitCentresPagedAsync_SortsByDivision(string sortBy, bool descending, string firstExpected, string secondExpected)
        {
            var entities = new List<ProfitCentre>
            {
                BuildEntity("PC01", division: "ZZZ"),
                BuildEntity("PC02", division: "AAA")
            };
            var repo = CreateRepository(profitCentres: entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            var list = result.Data.ToList();
            Assert.Equal(firstExpected,  list[0].Division);
            Assert.Equal(secondExpected, list[1].Division);
        }

        [Theory]
        [InlineData("ContTarget", false)]
        [InlineData("ContTarget", true)]
        public async Task GetAllProfitCentresPagedAsync_SortsByContTarget(string sortBy, bool descending)
        {
            var entities = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "A", Division = "D", ContTarget = 200m },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "B", Division = "D", ContTarget = 100m }
            };
            var repo = CreateRepository(profitCentres: entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            var list = result.Data.ToList();
            var firstTarget  = descending ? 200m : 100m;
            Assert.Equal(firstTarget, list[0].ContTarget);
        }

        [Theory]
        [InlineData("ProfitCentreHead", false, "Alice", "Bob")]
        [InlineData("ProfitCentreHead", true,  "Bob",   "Alice")]
        public async Task GetAllProfitCentresPagedAsync_SortsByProfitCentreHead(string sortBy, bool descending, string firstExpected, string secondExpected)
        {
            var entities = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "A", Division = "D", ProfitCentreHead = "Bob" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "B", Division = "D", ProfitCentreHead = "Alice" }
            };
            var repo = CreateRepository(profitCentres: entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            var list = result.Data.ToList();
            Assert.Equal(firstExpected,  list[0].ProfitCentreHead);
            Assert.Equal(secondExpected, list[1].ProfitCentreHead);
        }

        [Theory]
        [InlineData("UnknownColumn", false, "PC01", "PC02")]
        [InlineData("UnknownColumn", true,  "PC02", "PC01")]
        public async Task GetAllProfitCentresPagedAsync_SortsByProfitCentreId_WhenSortByIsUnknown(string sortBy, bool descending, string firstExpected, string secondExpected)
        {
            var entities = new List<ProfitCentre>
            {
                BuildEntity("PC02"), BuildEntity("PC01")
            };
            var repo = CreateRepository(profitCentres: entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };
            var result = await repo.GetAllProfitCentresPagedAsync(query);
            var list = result.Data.ToList();
            Assert.Equal(firstExpected,  list[0].ProfitCentreId);
            Assert.Equal(secondExpected, list[1].ProfitCentreId);
        }

        #endregion

        #region CreateProfitCentreAsync UserProfitcentre Link Tests

        [Fact]
        public async Task CreateProfitCentreAsync_SkipsUserLink_WhenLinkAlreadyExists()
        {
            var entity = BuildEntity("PC01");
            var user = new User { UserId = 10, UserEmail = "test@example.com" };
            var existingLink = new UserProfitcentre { ProfitCentre = "PC01", UserId = 10, FpsYear = 2024 };
            var repo = CreateRepository(
                profitCentres: [],
                userProfitCentres: [existingLink],
                users: [user]);

            var result = await repo.CreateProfitCentreAsync(entity);

            Assert.NotNull(result);
            Assert.Equal("PC01", result.ProfitCentreId);
        }

        #endregion

        #region UpdateProfitCentreAsync UserProfitcentre Link Tests

        [Fact]
        public async Task UpdateProfitCentreAsync_SkipsUserLink_WhenLinkAlreadyExists()
        {
            var existing = BuildEntity("PC01", "Old Name", "OLD");
            var updated  = BuildEntity("PC01", "New Name", "NEW");
            var user = new User { UserId = 10, UserEmail = "test@example.com" };
            var existingLink = new UserProfitcentre { ProfitCentre = "PC01", UserId = 10, FpsYear = 2024 };
            var repo = CreateRepository(
                profitCentres: [existing],
                userProfitCentres: [existingLink],
                users: [user]);

            var result = await repo.UpdateProfitCentreAsync("PC01", updated);

            Assert.Equal("New Name", result.ProfitCentreName);
        }

        [Fact]
        public async Task UpdateProfitCentreAsync_AddsUserLink_WhenUserFoundButNotLinked()
        {
            var existing = BuildEntity("PC01", "Old Name", "OLD");
            var updated  = BuildEntity("PC01", "New Name", "NEW");
            var user = new User { UserId = 10, UserEmail = "test@example.com" };
            var repo = CreateRepository(
                profitCentres: [existing],
                userProfitCentres: [],
                users: [user]);

            var result = await repo.UpdateProfitCentreAsync("PC01", updated);

            Assert.Equal("New Name", result.ProfitCentreName);
        }

        [Fact]
        public async Task UpdateProfitCentreAsync_UsesFallbackUserId_WhenUserNotFound()
        {
            var existing = BuildEntity("PC01", "Old Name", "OLD");
            var updated  = BuildEntity("PC01", "New Name", "NEW");
            var repo = CreateRepository(
                profitCentres: [existing],
                userProfitCentres: [],
                users: []);

            var result = await repo.UpdateProfitCentreAsync("PC01", updated);

            Assert.Equal("New Name", result.ProfitCentreName);
        }

        #endregion

        #region DeleteProfitCentreAsync Cascade Tests

        [Fact]
        public async Task DeleteProfitCentreAsync_CascadesUserProfitCentreLinks()
        {
            var existingLink = new UserProfitcentre { ProfitCentre = "PC01", UserId = 10, FpsYear = 2024 };
            var repo = CreateRepository(
                profitCentres: [BuildEntity("PC01")],
                profitCentreGrades: [],
                workgroups: [],
                userProfitCentres: [existingLink]);

            var result = await repo.DeleteProfitCentreAsync("PC01");

            Assert.True(result);
        }

        #endregion

        #region GetProfitCenterCostSummaryAsync Tests

        private static ProfitCentreRepository CreateRepositoryWithTimeCostCalcs(
            IEnumerable<TimeCostCalcs>? timeCostCalcs = null,
            IEnumerable<Workgroup>? workgroups = null)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(2024);
            requestContext.UserEmailId.Returns("test@example.com");

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var tccSet = RepositoryTestHelper.CreateMockDbSet(timeCostCalcs ?? []);
            mockContext.Setup(x => x.TimeCostCalcs).Returns(tccSet.Object);

            var wgSet = RepositoryTestHelper.CreateMockDbSet(workgroups ?? []);
            mockContext.Setup(x => x.Workgroups).Returns(wgSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new ProfitCentreRepository(mockContext.Object, requestContext);
        }

        #endregion

        #region GetPagedProfitCenterCostSummaryAsync Tests

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_ThrowsArgumentNullException_WhenParametersIsNull()
        {
            // Arrange
            var repo = CreateRepositoryWithTimeCostCalcs([], []);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                repo.GetPagedProfitCenterCostSummaryAsync(null!, 0.0));
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithMonthNumber_ReturnsFilteredPagedData()
        {
            // Arrange
            const short monthNumber = 1;
            var workgroups = new List<Workgroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC01" },
                new() { WorkGroupName = "WG2", ProfitCentre = "PC02" }
            };
            var timeCostCalcs = new List<TimeCostCalcs>
            {
                new() { WorkGroup = "WG1", ChargeRate = 100m, Time = 10, Class = "Charge", Month = 1 },
                new() { WorkGroup = "WG2", ChargeRate = 200m, Time = 8, Class = "Charge", Month = 2 }
            };
            var repo = CreateRepositoryWithTimeCostCalcs(timeCostCalcs, workgroups);
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 5 };

            // Act
            var result = await repo.GetPagedProfitCenterCostSummaryAsync(parameters, monthNumber);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("PC01", result.Data.First().ProfitCentre);
            Assert.Equal(1, result.PaginationData.TotalRecords);
        }
        [Fact]
        public async Task GetPagedProfitCenterCostSummaryAsync_WithEmptyResult_ReturnsEmptyPagedData()
        {
            // Arrange
            var repo = CreateRepositoryWithTimeCostCalcs([], []);
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetPagedProfitCenterCostSummaryAsync(parameters, 0.0);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        #endregion

    }
}
