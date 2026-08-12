using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.DivisionRepositoryTest
{
    public class DivisionRepositoryTests
    {
        #region Helpers

        private static Division BuildDivision(string divName = "DIV001", int agencyId = 1, int? divisionId = 10, decimal? centOverhead = 500m) =>
            new() { DivName = divName, AgencyId = agencyId, DivisionId = divisionId, CentOverhead = centOverhead };

        private static DivisionRepository CreateRepository(
            IEnumerable<Division>? divisions = null,
            IEnumerable<ProfitCentre>? profitCentres = null,
            IEnumerable<DivisionGrade>? divisionGrades = null)
        {
            var mockFpsYearContext = new Mock<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            if (divisions != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(divisions);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                mockContext.Setup(x => x.Divisions).Returns(mockSet.Object);
            }

            if (profitCentres != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(profitCentres);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                mockContext.Setup(x => x.Set<ProfitCentre>()).Returns(mockSet.Object);
            }

            if (divisionGrades != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(divisionGrades);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                mockContext.Setup(x => x.Set<DivisionGrade>()).Returns(mockSet.Object);
            }

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new DivisionRepository(mockContext.Object);
        }

        private static (
            DivisionRepository Repo,
            Mock<DbSet<Division>> DivisionsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<Division>? divisions = null)
        {
            var mockFpsYearContext = new Mock<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var divisionsMockSet = RepositoryTestHelper.CreateMockDbSet(divisions ?? []);
            RepositoryTestHelper.SetupDbSetOperations(divisionsMockSet);
            mockContext.Setup(x => x.Divisions).Returns(divisionsMockSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new DivisionRepository(mockContext.Object);
            return (repo, divisionsMockSet, mockContext);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DivisionRepository(null!));
        }

        #endregion

        #region GetAllDivisionsAsync Tests

        [Fact]
        public async Task GetAllDivisionsAsync_ReturnsEmptyList_WhenNoDivisions()
        {
            var repo = CreateRepository(divisions: []);
            var result = await repo.GetAllDivisionsAsync();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllDivisionsAsync_ReturnsAllDivisions()
        {
            var divisions = new List<Division>
            {
                BuildDivision("ALPHA", 1, 10, 100m),
                BuildDivision("BETA",  2, 20, 200m),
                BuildDivision("GAMMA", 3, 30, 300m)
            };
            var repo = CreateRepository(divisions: divisions);
            var result = await repo.GetAllDivisionsAsync();
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllDivisionsAsync_ReturnsDivisionsOrderedByDivName()
        {
            var divisions = new List<Division>
            {
                BuildDivision("ZETA"),
                BuildDivision("ALPHA"),
                BuildDivision("MANGO")
            };
            var repo = CreateRepository(divisions: divisions);
            var result = await repo.GetAllDivisionsAsync();
            Assert.Equal("ALPHA", result[0].DivName);
            Assert.Equal("MANGO", result[1].DivName);
            Assert.Equal("ZETA", result[2].DivName);
        }

        [Fact]
        public async Task GetAllDivisionsAsync_MapsDivisionFieldsCorrectly()
        {
            var divisions = new List<Division>
            {
                new() { DivName = "DIV001", AgencyId = 5, DivisionId = 42, CentOverhead = 750m }
            };
            var repo = CreateRepository(divisions: divisions);
            var result = await repo.GetAllDivisionsAsync();
            var division = Assert.Single(result);
            Assert.Equal("DIV001", division.DivName);
            Assert.Equal(5, division.AgencyId);
            Assert.Equal(42, division.DivisionId);
            Assert.Equal(750m, division.CentOverhead);
        }

        #endregion

        #region GetAllDivisionsPagedAsync Tests

        [Fact]
        public async Task GetAllDivisionsPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            var repo = CreateRepository(divisions: []);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetAllDivisionsPagedAsync(null!));
        }

        [Fact]
        public async Task GetAllDivisionsPagedAsync_ReturnsEmptyPagedData_WhenNoDivisions()
        {
            var repo = CreateRepository(divisions: []);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllDivisionsPagedAsync(query);
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllDivisionsPagedAsync_ReturnsCorrectPage()
        {
            var divisions = new List<Division>
            {
                BuildDivision("DIV_A"), BuildDivision("DIV_B"), BuildDivision("DIV_C"),
                BuildDivision("DIV_D"), BuildDivision("DIV_E")
            };
            var repo = CreateRepository(divisions: divisions);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };
            var result = await repo.GetAllDivisionsPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(2, result.PaginationData.PageSize);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetAllDivisionsPagedAsync_FiltersByDivName()
        {
            var divisions = new List<Division>
            {
                BuildDivision("ALPHA"), BuildDivision("BETA"), BuildDivision("ALPHABET")
            };
            var repo = CreateRepository(divisions: divisions);
            var filter = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { { "DivName", "ALPHA" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllDivisionsPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, d => Assert.Contains("ALPHA", d.DivName));
        }

        [Fact]
        public async Task GetAllDivisionsPagedAsync_FiltersByDivisionId()
        {
            var divisions = new List<Division>
            {
                BuildDivision("DIV001", divisionId: 10),
                BuildDivision("DIV002", divisionId: 20),
                BuildDivision("DIV003", divisionId: 10)
            };
            var repo = CreateRepository(divisions: divisions);
            var filter = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { { "DivisionId", "10" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllDivisionsPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, d => Assert.Equal(10, d.DivisionId));
        }

        [Fact]
        public async Task GetAllDivisionsPagedAsync_FiltersByAgencyId()
        {
            var divisions = new List<Division>
            {
                BuildDivision("DIV001", agencyId: 1),
                BuildDivision("DIV002", agencyId: 2),
                BuildDivision("DIV003", agencyId: 1)
            };
            var repo = CreateRepository(divisions: divisions);
            var filter = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { { "AgencyId", "1" } });
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllDivisionsPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, d => Assert.Equal(1, d.AgencyId));
        }

        [Theory]
        [InlineData("divname", false)]
        [InlineData("divname", true)]
        [InlineData("divisionid", false)]
        [InlineData("divisionid", true)]
        [InlineData("agencyid", false)]
        [InlineData("agencyid", true)]
        [InlineData("centoverhead", false)]
        [InlineData("centoverhead", true)]
        [InlineData("unknown", false)]
        public async Task GetAllDivisionsPagedAsync_AppliesSortingWithoutException(string sortBy, bool descending)
        {
            var divisions = new List<Division>
            {
                BuildDivision("DIV_C", agencyId: 3, divisionId: 30, centOverhead: 300m),
                BuildDivision("DIV_A", agencyId: 1, divisionId: 10, centOverhead: 100m),
                BuildDivision("DIV_B", agencyId: 2, divisionId: 20, centOverhead: 200m)
            };
            var repo = CreateRepository(divisions: divisions);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };
            var result = await repo.GetAllDivisionsPagedAsync(query);
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task GetAllDivisionsPagedAsync_DefaultSortsAscendingByDivName_WhenSortByIsEmpty()
        {
            var divisions = new List<Division>
            {
                BuildDivision("ZETA"), BuildDivision("ALPHA"), BuildDivision("MANGO")
            };
            var repo = CreateRepository(divisions: divisions);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllDivisionsPagedAsync(query);
            Assert.Equal("ALPHA", result.Data.ElementAt(0).DivName);
            Assert.Equal("MANGO", result.Data.ElementAt(1).DivName);
            Assert.Equal("ZETA", result.Data.ElementAt(2).DivName);
        }

        [Fact]
        public async Task GetAllDivisionsPagedAsync_ReturnsCorrectPaginationMetadata()
        {
            var divisions = Enumerable.Range(1, 7).Select(i => BuildDivision($"DIV_{i:D3}")).ToList();
            var repo = CreateRepository(divisions: divisions);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 3 };
            var result = await repo.GetAllDivisionsPagedAsync(query);
            Assert.Equal(7, result.PaginationData.TotalRecords);
            Assert.Equal(3, result.PaginationData.TotalPages);
            Assert.Equal(1, result.PaginationData.PageNumber);
            Assert.Equal(3, result.PaginationData.PageSize);
        }

        #endregion

        #region GetDivisionByNameAsync Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetDivisionByNameAsync_ReturnsNull_WhenDivNameIsNullOrWhiteSpace(string? divName)
        {
            var repo = CreateRepository(divisions: []);
            var result = await repo.GetDivisionByNameAsync(divName!);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDivisionByNameAsync_ReturnsNull_WhenDivisionDoesNotExist()
        {
            var repo = CreateRepository(divisions: [BuildDivision("DIV001")]);
            var result = await repo.GetDivisionByNameAsync("MISSING");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDivisionByNameAsync_ReturnsDivision_WhenFound()
        {
            var division = new Division { DivName = "DIV001", AgencyId = 5, DivisionId = 42, CentOverhead = 750m };
            var repo = CreateRepository(divisions: [division]);
            var result = await repo.GetDivisionByNameAsync("DIV001");
            Assert.NotNull(result);
            Assert.Equal("DIV001", result.DivName);
            Assert.Equal(5, result.AgencyId);
            Assert.Equal(42, result.DivisionId);
            Assert.Equal(750m, result.CentOverhead);
        }

        [Fact]
        public async Task GetDivisionByNameAsync_ReturnsSingleMatch_WhenMultipleDivisionsExist()
        {
            var divisions = new List<Division>
            {
                BuildDivision("DIV001"), BuildDivision("DIV002"), BuildDivision("DIV003")
            };
            var repo = CreateRepository(divisions: divisions);
            var result = await repo.GetDivisionByNameAsync("DIV002");
            Assert.NotNull(result);
            Assert.Equal("DIV002", result.DivName);
        }

        #endregion

        #region CreateDivisionAsync Tests

        [Fact]
        public async Task CreateDivisionAsync_ThrowsArgumentNullException_WhenDivisionIsNull()
        {
            var repo = CreateRepository(divisions: []);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.CreateDivisionAsync(null!));
        }

        [Fact]
        public async Task CreateDivisionAsync_AddsDivision_WhenValid()
        {
            var (repo, divisionsMockSet, mockContext) = CreateRepositoryWithMocks([]);
            var newDivision = BuildDivision("DIV_NEW", agencyId: 3, divisionId: 99, centOverhead: 1000m);
            var result = await repo.CreateDivisionAsync(newDivision);
            Assert.NotNull(result);
            Assert.Equal("DIV_NEW", result.DivName);
            Assert.Equal(3, result.AgencyId);
            Assert.Equal(99, result.DivisionId);
            Assert.Equal(1000m, result.CentOverhead);
            divisionsMockSet.Verify(x => x.Add(It.IsAny<Division>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task CreateDivisionAsync_ReturnsTheSameEntity_ThatWasAdded()
        {
            var (repo, _, _) = CreateRepositoryWithMocks([]);
            var newDivision = BuildDivision("DIV_SAME");
            var result = await repo.CreateDivisionAsync(newDivision);
            Assert.Same(newDivision, result);
        }

        #endregion

        #region UpdateDivisionAsync Tests

        [Fact]
        public async Task UpdateDivisionAsync_ThrowsArgumentNullException_WhenDivisionIsNull()
        {
            var repo = CreateRepository(divisions: []);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateDivisionAsync("DIV001", null!));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateDivisionAsync_ThrowsArgumentException_WhenOriginalDivNameIsNullOrWhiteSpace(string? originalDivName)
        {
            var repo = CreateRepository(divisions: []);
            var division = BuildDivision("DIV001");
            await Assert.ThrowsAsync<ArgumentException>(() => repo.UpdateDivisionAsync(originalDivName!, division));
        }

        [Fact]
        public async Task UpdateDivisionAsync_ThrowsInvalidOperationException_WhenDivisionNotFound_SamePrimaryKey()
        {
            var repo = CreateRepository(divisions: []);
            var division = BuildDivision("MISSING");
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateDivisionAsync("MISSING", division));
        }

        [Fact]
        public async Task UpdateDivisionAsync_ThrowsInvalidOperationException_WhenDivisionNotFound_DifferentPrimaryKey()
        {
            var repo = CreateRepository(divisions: []);
            var division = BuildDivision("NEW_NAME");
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateDivisionAsync("OLD_NAME", division));
        }

        [Fact]
        public async Task UpdateDivisionAsync_UpdatesProperties_WhenPrimaryKeyIsUnchanged()
        {
            var existing = BuildDivision("DIV001", agencyId: 1, divisionId: 10, centOverhead: 100m);
            var mockFpsYearContext = new Mock<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);
            var divisionsMockSet = RepositoryTestHelper.CreateMockDbSet<Division>([existing]);
            RepositoryTestHelper.SetupDbSetOperations(divisionsMockSet);
            mockContext.Setup(x => x.Divisions).Returns(divisionsMockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new DivisionRepository(mockContext.Object);
            var updated = new Division { DivName = "DIV001", AgencyId = 9, DivisionId = 99, CentOverhead = 999m };

            var result = await repo.UpdateDivisionAsync("DIV001", updated);

            Assert.NotNull(result);
            Assert.Equal("DIV001", result.DivName);
            Assert.Equal(9, result.AgencyId);
            Assert.Equal(99, result.DivisionId);
            Assert.Equal(999m, result.CentOverhead);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task UpdateDivisionAsync_DeletesAndReInserts_WhenPrimaryKeyChanges()
        {
            var existing = BuildDivision("OLD_NAME", agencyId: 1, divisionId: 10, centOverhead: 100m);
            var mockFpsYearContext = new Mock<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);
            var divisionsMockSet = RepositoryTestHelper.CreateMockDbSet<Division>([existing]);
            RepositoryTestHelper.SetupDbSetOperations(divisionsMockSet);
            mockContext.Setup(x => x.Divisions).Returns(divisionsMockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new DivisionRepository(mockContext.Object);
            var updated = BuildDivision("NEW_NAME", agencyId: 2, divisionId: 20, centOverhead: 200m);

            var result = await repo.UpdateDivisionAsync("OLD_NAME", updated);

            Assert.NotNull(result);
            Assert.Equal("NEW_NAME", result.DivName);
            Assert.Equal(2, result.AgencyId);
            divisionsMockSet.Verify(x => x.Remove(It.IsAny<Division>()), Times.Once);
            divisionsMockSet.Verify(x => x.Add(It.Is<Division>(d => d.DivName == "NEW_NAME")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 2);
        }

        #endregion

        #region DeleteDivisionAsync Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteDivisionAsync_ReturnsFalse_WhenDivNameIsNullOrWhiteSpace(string? divName)
        {
            var repo = CreateRepository(divisions: []);
            var result = await repo.DeleteDivisionAsync(divName!);
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteDivisionAsync_ReturnsFalse_WhenDivisionNotFound()
        {
            var repo = CreateRepository(divisions: [BuildDivision("DIV001")]);
            var result = await repo.DeleteDivisionAsync("MISSING");
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteDivisionAsync_ReturnsTrue_WhenDivisionFound()
        {
            var (repo, _, _) = CreateRepositoryWithMocks([BuildDivision("DIV001")]);
            var result = await repo.DeleteDivisionAsync("DIV001");
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteDivisionAsync_CallsRemoveAndSave_WhenDivisionFound()
        {
            var (repo, divisionsMockSet, mockContext) = CreateRepositoryWithMocks([BuildDivision("DIV001")]);
            await repo.DeleteDivisionAsync("DIV001");
            divisionsMockSet.Verify(x => x.Remove(It.IsAny<Division>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteDivisionAsync_DoesNotCallRemove_WhenDivisionNotFound()
        {
            var (repo, divisionsMockSet, _) = CreateRepositoryWithMocks([BuildDivision("DIV001")]);
            await repo.DeleteDivisionAsync("MISSING");
            divisionsMockSet.Verify(x => x.Remove(It.IsAny<Division>()), Times.Never);
        }

        #endregion

        #region DivisionExistsAsync Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DivisionExistsAsync_ReturnsFalse_WhenDivNameIsNullOrWhiteSpace(string? divName)
        {
            var repo = CreateRepository(divisions: [BuildDivision("DIV001")]);
            var result = await repo.DivisionExistsAsync(divName!);
            Assert.False(result);
        }

        [Fact]
        public async Task DivisionExistsAsync_ReturnsFalse_WhenDivisionDoesNotExist()
        {
            var repo = CreateRepository(divisions: [BuildDivision("DIV001")]);
            var result = await repo.DivisionExistsAsync("MISSING");
            Assert.False(result);
        }

        [Fact]
        public async Task DivisionExistsAsync_ReturnsTrue_WhenDivisionExists()
        {
            var repo = CreateRepository(divisions: [BuildDivision("DIV001")]);
            var result = await repo.DivisionExistsAsync("DIV001");
            Assert.True(result);
        }

        [Fact]
        public async Task DivisionExistsAsync_ReturnsTrue_OnlyForExactMatch()
        {
            var repo = CreateRepository(divisions: [BuildDivision("DIV001")]);
            var exists = await repo.DivisionExistsAsync("DIV001");
            var notExists = await repo.DivisionExistsAsync("DIV002");
            Assert.True(exists);
            Assert.False(notExists);
        }

        [Theory]
        [InlineData("aap")]
        [InlineData("AAP")]
        [InlineData("Aap")]
        public async Task DivisionExistsAsync_ReturnsTrue_RegardlessOfCasing(string lookupName)
        {
            var repo = CreateRepository(divisions: [BuildDivision("aap")]);
            var result = await repo.DivisionExistsAsync(lookupName);
            Assert.True(result);
        }

        #endregion

        #region GetDivisionForeignKeyReferencesAsync Tests

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetDivisionForeignKeyReferencesAsync_ReturnsEmptyList_WhenDivNameIsNullOrWhiteSpace(string? divName)
        {
            var repo = CreateRepository(divisions: [], profitCentres: [], divisionGrades: []);
            var result = await repo.GetDivisionForeignKeyReferencesAsync(divName!);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDivisionForeignKeyReferencesAsync_ReturnsEmptyList_WhenNoReferences()
        {
            var repo = CreateRepository(
                divisions: [BuildDivision("DIV001")],
                profitCentres: [],
                divisionGrades: []);
            var result = await repo.GetDivisionForeignKeyReferencesAsync("DIV001");
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDivisionForeignKeyReferencesAsync_ReturnsProfitCentreTable_WhenReferenced()
        {
            var profitCentres = new List<ProfitCentre> { new() { Division = "DIV001" } };
            var repo = CreateRepository(
                divisions: [BuildDivision("DIV001")],
                profitCentres: profitCentres,
                divisionGrades: []);
            var result = await repo.GetDivisionForeignKeyReferencesAsync("DIV001");
            Assert.Contains("tblkpprofitcentre", result);
            Assert.DoesNotContain("divisiongrade", result);
        }

        [Fact]
        public async Task GetDivisionForeignKeyReferencesAsync_ReturnsDivisionGradeTable_WhenReferenced()
        {
            var divisionGrades = new List<DivisionGrade> { new() { Division = "DIV001", GradeCode = "GR01" } };
            var repo = CreateRepository(
                divisions: [BuildDivision("DIV001")],
                profitCentres: [],
                divisionGrades: divisionGrades);
            var result = await repo.GetDivisionForeignKeyReferencesAsync("DIV001");
            Assert.Contains("divisiongrade", result);
            Assert.DoesNotContain("tblkpprofitcentre", result);
        }

        [Fact]
        public async Task GetDivisionForeignKeyReferencesAsync_ReturnsBothTables_WhenBothReferenced()
        {
            var profitCentres = new List<ProfitCentre> { new() { Division = "DIV001" } };
            var divisionGrades = new List<DivisionGrade> { new() { Division = "DIV001", GradeCode = "GR01" } };
            var repo = CreateRepository(
                divisions: [BuildDivision("DIV001")],
                profitCentres: profitCentres,
                divisionGrades: divisionGrades);
            var result = await repo.GetDivisionForeignKeyReferencesAsync("DIV001");
            Assert.Equal(2, result.Count);
            Assert.Contains("tblkpprofitcentre", result);
            Assert.Contains("divisiongrade", result);
        }

        [Fact]
        public async Task GetDivisionForeignKeyReferencesAsync_IgnoresReferences_WhenDivisionNameDoesNotMatch()
        {
            var profitCentres = new List<ProfitCentre> { new() { Division = "OTHER" } };
            var divisionGrades = new List<DivisionGrade> { new() { Division = "OTHER", GradeCode = "GR01" } };
            var repo = CreateRepository(
                divisions: [BuildDivision("DIV001")],
                profitCentres: profitCentres,
                divisionGrades: divisionGrades);
            var result = await repo.GetDivisionForeignKeyReferencesAsync("DIV001");
            Assert.Empty(result);
        }

        #endregion
    }
}

