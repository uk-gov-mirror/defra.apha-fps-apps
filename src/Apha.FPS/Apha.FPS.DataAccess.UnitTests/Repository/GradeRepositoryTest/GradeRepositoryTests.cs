using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.GradeRepositoryTest
{
    public class GradeRepositoryTests
    {
        #region Helpers

        private static Grade BuildGrade(
            string code       = "A",
            string? descLong  = "Grade A",
            decimal? avSalary = 50000m,
            int fpsYear       = 2025,
            string? pactCode  = null,
            double? avLeaveHrs = null,
            double? avSickHrs  = null) =>
            new()
            {
                GradeCode   = code,
                DescLong    = descLong,
                AvSalary    = avSalary,
                FpsYear     = fpsYear,
                PactCode    = pactCode,
                AvLeaveHrs  = avLeaveHrs,
                AvSickHrs   = avSickHrs
            };

        private static GradeRepository CreateRepository(IEnumerable<Grade>? grades = null)
        {
            var mockFpsYearContext = new Mock<IFpsRequestContext>();
            mockFpsYearContext.Setup(x => x.FpsYear).Returns(2025);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            if (grades != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(grades);
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
                mockContext.Setup(x => x.Grades).Returns(mockSet.Object);
            }

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new GradeRepository(mockContext.Object);
        }

        private static (
            GradeRepository Repo,
            Mock<DbSet<Grade>> DbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<Grade>? grades = null)
        {
            var mockFpsYearContext = new Mock<IFpsRequestContext>();
            mockFpsYearContext.Setup(x => x.FpsYear).Returns(2025);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var dbSet = RepositoryTestHelper.CreateMockDbSet(grades ?? []);
            RepositoryTestHelper.SetupDbSetOperations(dbSet);
            mockContext.Setup(x => x.Grades).Returns(dbSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var repo = new GradeRepository(mockContext.Object);
            return (repo, dbSet, mockContext);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new GradeRepository(null!));
        }

        #endregion

        #region GetAllPagedAsync Tests

        [Fact]
        public async Task GetAllPagedAsync_ThrowsArgumentNullException_WhenQueryIsNull()
        {
            var repo = CreateRepository(grades: []);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetAllPagedAsync(null!));
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsEmptyPagedData_WhenNoRecords()
        {
            var repo  = CreateRepository(grades: []);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllPagedAsync(query);
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsAllRecords()
        {
            var grades = new List<Grade>
            {
                BuildGrade("A"),
                BuildGrade("B"),
                BuildGrade("C")
            };
            var repo  = CreateRepository(grades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task GetAllPagedAsync_ReturnsCorrectPage()
        {
            var grades = new List<Grade>
            {
                BuildGrade("A"), BuildGrade("B"), BuildGrade("C"),
                BuildGrade("D"), BuildGrade("E")
            };
            var repo  = CreateRepository(grades: grades);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetAllPagedAsync_FiltersByGradeCode()
        {
            var grades = new List<Grade>
            {
                BuildGrade("A01"),
                BuildGrade("A02"),
                BuildGrade("B01")
            };
            var repo   = CreateRepository(grades: grades);
            var filter = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { { "GradeCode", "A" } });
            var query  = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllPagedAsync_FiltersByDescription()
        {
            var grades = new List<Grade>
            {
                BuildGrade("A", descLong: "Senior Engineer"),
                BuildGrade("B", descLong: "Junior Engineer"),
                BuildGrade("C", descLong: "Manager")
            };
            var repo   = CreateRepository(grades: grades);
            var filter = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, string> { { "Description", "Engineer" } });
            var query  = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = filter };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetAllPagedAsync_OrdersByGradeCodeAscByDefault()
        {
            var grades = new List<Grade>
            {
                BuildGrade("C"),
                BuildGrade("A"),
                BuildGrade("B")
            };
            var repo  = CreateRepository(grades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var result = await repo.GetAllPagedAsync(query);
            var list  = result.Data.ToList();
            Assert.Equal("A", list[0].GradeCode);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenCodeIsNullOrWhiteSpace()
        {
            var repo   = CreateRepository(grades: []);
            var result = await repo.GetByIdAsync("");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenCodeIsWhiteSpace()
        {
            var repo   = CreateRepository(grades: []);
            var result = await repo.GetByIdAsync("   ");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsRecord_WhenFound()
        {
            var grades = new List<Grade> { BuildGrade("A", descLong: "Grade A") };
            var repo   = CreateRepository(grades: grades);
            var result = await repo.GetByIdAsync("A");
            Assert.NotNull(result);
            Assert.Equal("A", result.GradeCode);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            var repo   = CreateRepository(grades: []);
            var result = await repo.GetByIdAsync("NONEXISTENT");
            Assert.Null(result);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository(grades: []);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.CreateAsync(null!));
        }

        [Fact]
        public async Task CreateAsync_ThrowsInvalidOperationException_WhenCodeAlreadyExists()
        {
            var existing = BuildGrade("A");
            var (repo, _, _) = CreateRepositoryWithMocks([existing]);
            var duplicate = BuildGrade("A");
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.CreateAsync(duplicate));
        }

        [Fact]
        public async Task CreateAsync_ThrowsInvalidOperationException_WhenCodeAlreadyExistsWithDifferentCasing()
        {
            var existing = BuildGrade("ABC");
            var (repo, _, _) = CreateRepositoryWithMocks([existing]);
            var duplicate = BuildGrade("abc");
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.CreateAsync(duplicate));
        }

        [Fact]
        public async Task CreateAsync_SetsYearFromContext()
        {
            var (repo, dbSet, _) = CreateRepositoryWithMocks([]);
            var entity = BuildGrade("NEW");
            entity.FpsYear = 0;

            dbSet.Setup(x => x.Add(It.IsAny<Grade>()));

            var result = await repo.CreateAsync(entity);
            Assert.Equal(2025, result.FpsYear);
        }

        [Fact]
        public async Task CreateAsync_AddsEntityAndSavesChanges()
        {
            var (repo, dbSet, context) = CreateRepositoryWithMocks([]);
            dbSet.Setup(x => x.Add(It.IsAny<Grade>()));

            var entity = BuildGrade("NEW");
            var result = await repo.CreateAsync(entity);

            Assert.NotNull(result);
            Assert.Equal("NEW", result.GradeCode);
            dbSet.Verify(x => x.Add(It.Is<Grade>(g => g.GradeCode == "NEW")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(context);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentNullException_WhenEntityIsNull()
        {
            var repo = CreateRepository(grades: []);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.UpdateAsync("A", null!));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentException_WhenOriginalCodeIsEmpty()
        {
            var repo   = CreateRepository(grades: []);
            var entity = BuildGrade("A");
            await Assert.ThrowsAsync<ArgumentException>(() => repo.UpdateAsync("", entity));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsArgumentException_WhenOriginalCodeIsWhiteSpace()
        {
            var repo   = CreateRepository(grades: []);
            var entity = BuildGrade("A");
            await Assert.ThrowsAsync<ArgumentException>(() => repo.UpdateAsync("   ", entity));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsInvalidOperationException_WhenCodeChangesButOriginalNotFound()
        {
            var (repo, _, _) = CreateRepositoryWithMocks([]);
            var entity = BuildGrade("B"); // attempting rename from A → B, but A not found
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateAsync("A", entity));
        }

        [Fact]
        public async Task UpdateAsync_ThrowsInvalidOperationException_WhenSameCodeButRecordNotFound()
        {
            var (repo, _, _) = CreateRepositoryWithMocks([]);
            var entity = BuildGrade("A");
            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateAsync("A", entity));
        }

        [Fact]
        public async Task UpdateAsync_UpdatesFieldsInPlace_WhenCodeIsUnchanged()
        {
            var existing = BuildGrade("A", descLong: "Original Desc", avSalary: 50000m);
            var (repo, dbSet, context) = CreateRepositoryWithMocks([existing]);

            var updated = BuildGrade("A", descLong: "Updated Desc", avSalary: 60000m,
                pactCode: "PC01", avLeaveHrs: 10.0, avSickHrs: 5.0);

            var result = await repo.UpdateAsync("A", updated);

            Assert.Equal("A", result.GradeCode);
            Assert.Equal("Updated Desc", result.DescLong);
            Assert.Equal(60000m, result.AvSalary);
            Assert.Equal("PC01", result.PactCode);
            Assert.Equal(10.0, result.AvLeaveHrs);
            Assert.Equal(5.0, result.AvSickHrs);
            Assert.Equal(2025, result.FpsYear);
            RepositoryTestHelper.VerifySaveChanges(context);
        }

        [Fact]
        public async Task UpdateAsync_DeletesAndReinserts_WhenCodeChanges()
        {
            var existing = BuildGrade("A");
            var (repo, dbSet, context) = CreateRepositoryWithMocks([existing]);
            dbSet.Setup(x => x.Remove(It.IsAny<Grade>()));
            dbSet.Setup(x => x.Add(It.IsAny<Grade>()));

            var updated = BuildGrade("B", descLong: "Updated", avSalary: 60000m);

            var result = await repo.UpdateAsync("A", updated);

            Assert.Equal("B", result.GradeCode);
            Assert.Equal(2025, result.FpsYear);
            dbSet.Verify(x => x.Remove(It.Is<Grade>(g => g.GradeCode == "A")), Times.Once);
            dbSet.Verify(x => x.Add(It.Is<Grade>(g => g.GradeCode == "B")), Times.Once);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenCodeIsNullOrWhiteSpace()
        {
            var repo   = CreateRepository(grades: []);
            var result = await repo.DeleteAsync("");
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenCodeIsWhiteSpace()
        {
            var repo   = CreateRepository(grades: []);
            var result = await repo.DeleteAsync("   ");
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenRecordNotFound()
        {
            var (repo, _, _) = CreateRepositoryWithMocks([]);
            var result = await repo.DeleteAsync("NOTEXIST");
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsTrue_WhenDeleted()
        {
            var existing = BuildGrade("A");
            var (repo, dbSet, context) = CreateRepositoryWithMocks([existing]);
            dbSet.Setup(x => x.Remove(It.IsAny<Grade>()));
            var result = await repo.DeleteAsync("A");
            Assert.True(result);
        }

        #endregion

        #region Sorting Tests

        [Theory]
        [InlineData("gradecode",   false, "A",     "B")]
        [InlineData("gradecode",   true,  "B",     "A")]
        [InlineData("description", false, "50000", "60000")]  // sorted by DescLong
        [InlineData("avsalary",    false, "50000", "60000")]
        [InlineData("avsalary",    true,  "60000", "50000")]
        public async Task GetAllPagedAsync_AppliesSorting_Correctly(
            string sortBy, bool descending, string expectedFirst, string expectedSecond)
        {
            // Two grades with distinct values for all sortable columns
            var grades = new List<Grade>
            {
                BuildGrade("B", descLong: "Grade B", avSalary: 60000m),
                BuildGrade("A", descLong: "Grade A", avSalary: 50000m)
            };
            var repo  = CreateRepository(grades: grades);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending
            };
            var result = await repo.GetAllPagedAsync(query);
            var list   = result.Data.ToList();

            var actualFirst = sortBy switch
            {
                "description" => list[0].AvSalary?.ToString(),
                "avsalary"    => list[0].AvSalary?.ToString(),
                _             => list[0].GradeCode
            };
            var actualSecond = sortBy switch
            {
                "description" => list[1].AvSalary?.ToString(),
                "avsalary"    => list[1].AvSalary?.ToString(),
                _             => list[1].GradeCode
            };
            Assert.Equal(expectedFirst, actualFirst);
            Assert.Equal(expectedSecond, actualSecond);
        }

        [Fact]
        public async Task GetAllPagedAsync_SortsByGradeCodeAsc_WhenSortByIsUnknown()
        {
            var grades = new List<Grade>
            {
                BuildGrade("C"),
                BuildGrade("A"),
                BuildGrade("B")
            };
            var repo  = CreateRepository(grades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "unknown" };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal("A", result.Data.First().GradeCode);
        }

        [Fact]
        public async Task GetAllPagedAsync_SortsByGradeCodeAsc_WhenSortByIsNull()
        {
            var grades = new List<Grade>
            {
                BuildGrade("C"),
                BuildGrade("A")
            };
            var repo  = CreateRepository(grades: grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = null };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal("A", result.Data.First().GradeCode);
        }

        [Fact]
        public async Task GetAllPagedAsync_SortsByGradeCodeDesc_WhenDescendingTrue()
        {
            var grades = new List<Grade>
            {
                BuildGrade("A"),
                BuildGrade("C"),
                BuildGrade("B")
            };
            var repo  = CreateRepository(grades: grades);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10, SortBy = "gradecode", Descending = true
            };
            var result = await repo.GetAllPagedAsync(query);
            Assert.Equal("C", result.Data.First().GradeCode);
        }

        #endregion
    }
}
