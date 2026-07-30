using Apha.Common.Helpers.Repository;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.Costbook.DataAccess.UnitTests.Repository.StaffRequirementRepositoryTest;

public class StaffRequirementRepositoryTests
{
    private const int DefaultFpsYear = 2025;

    /// <summary>
    /// Creates a StaffRequirementRepository with in-memory DbSets.
    /// The GetStaffRequirementsByProjectYearAsync method uses multi-table JOINs across
    /// WorkGroupGrades, Projects, and EuGradeConversions and is covered by integration tests.
    /// ExecuteDeleteAsync (DeleteStaffRequirementAsync) also requires integration tests.
    /// </summary>
    private static (
        StaffRequirementRepository Repo,
        Mock<DbSet<StaffRequirement>> StaffRequirementsDbSet,
        Mock<CostbookDbContext> Context)
        CreateRepository(
            IEnumerable<StaffRequirement>? staffRequirements = null,
            IEnumerable<WorkGroupGrade>? workGroupGrades = null,
            IEnumerable<ProfitCentreGrade>? profitCentreGrades = null,
            IEnumerable<Project>? projects = null,
            IEnumerable<EuGradeConversion>? euGradeConversions = null)
    {
        var mockFpsYearContext = new Mock<IFPSYearContext>();
        mockFpsYearContext.Setup(x => x.FPSYear).Returns(DefaultFpsYear);

        var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFpsYearContext.Object);

        var staffReqMockSet = RepositoryTestHelper.CreateMockDbSet(staffRequirements ?? []);
        RepositoryTestHelper.SetupDbSetOperations(staffReqMockSet);
        mockContext.Setup(x => x.StaffRequirements).Returns(staffReqMockSet.Object);

        var wggMockSet = RepositoryTestHelper.CreateMockDbSet(workGroupGrades ?? []);
        mockContext.Setup(x => x.WorkGroupGrades).Returns(wggMockSet.Object);

        var pcgMockSet = RepositoryTestHelper.CreateMockDbSet(profitCentreGrades ?? []);
        mockContext.Setup(x => x.ProfitCentreGrades).Returns(pcgMockSet.Object);

        var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects ?? []);
        mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);

        var euMockSet = RepositoryTestHelper.CreateMockDbSet(euGradeConversions ?? []);
        mockContext.Setup(x => x.EuGradeConversions).Returns(euMockSet.Object);

        RepositoryTestHelper.SetupSaveChanges(mockContext);

        var mockSettingsRepo = new Mock<ISettingsRepository>();
        mockSettingsRepo.Setup(x => x.GetSettingValueByIdAsync("CurrentYear")).ReturnsAsync(DefaultFpsYear.ToString());

        var mockProjectRepo = new Mock<IProjectRepository>();
        mockProjectRepo.Setup(x => x.GetInflationFactorAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(1.0);

        var repo = new StaffRequirementRepository(mockContext.Object, mockFpsYearContext.Object, mockSettingsRepo.Object, mockProjectRepo.Object);
        return (repo, staffReqMockSet, mockContext);
    }

    #region AddStaffRequirementAsync

    [Fact]
    public async Task AddStaffRequirementAsync_AddsEntity_AndCallsSaveChanges()
    {
        // Arrange
        var wggs = new List<WorkGroupGrade>
        {
            new() { WgGrade = "HEO", FpsYear = DefaultFpsYear, ProfitCentreGrade = "PCG1", GradeCode = "GC01", WorkGroup = "Science" }
        };
        var pcgs = new List<ProfitCentreGrade>
        {
            new() { PcGrade = "PCG1", FpsYear = DefaultFpsYear, PayRate = 100m, Npr = 50m, Ohr = 25m, ChargeRate = 10m, DefraChargeRate = 10m, DivisionGrade = "D1", GradeCode = "GC01", ProfitCentre = "PC1" }
        };

        var (repo, staffReqDbSet, mockContext) = CreateRepository(workGroupGrades: wggs, profitCentreGrades: pcgs);
        var newReq = new StaffRequirement
        {
            SrIdentity = 1,
            Project = "2024/001",
            Year = 2024,
            WgGrade = "HEO",
            Name = "John Smith",
            Nohours = 100.0,
            Nodays = 13.0,
            Chargerate = 45.50
        };

        // Act
        var result = await repo.AddStaffRequirementAsync(newReq);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("2024/001", result.Project);
        Assert.Equal("HEO", result.WgGrade);
        Assert.Equal("John Smith", result.Name);
        Assert.Equal(100.0, result.Nohours);
        Assert.Equal(45.50, result.Chargerate);
        Assert.Equal(100.0, result.Payrate);
        Assert.Equal(50.0, result.Npr);
        Assert.Equal(25.0, result.Ohr);
        staffReqDbSet.Verify(x => x.Add(It.IsAny<StaffRequirement>()), Times.Once);
        RepositoryTestHelper.VerifySaveChanges(mockContext);
    }

    [Fact]
    public async Task AddStaffRequirementAsync_DecodesUrlEncodedProject()
    {
        // Arrange
        var (repo, _, _) = CreateRepository();
        var newReq = new StaffRequirement
        {
            Project = "2024%2F001",
            Year = 2024,
            WgGrade = "EO"
        };

        // Act
        var result = await repo.AddStaffRequirementAsync(newReq);

        // Assert
        Assert.Equal("2024/001", result.Project);
    }

    [Fact]
    public async Task AddStaffRequirementAsync_ReturnsSameEntityReference()
    {
        // Arrange
        var (repo, _, _) = CreateRepository();
        var newReq = new StaffRequirement
        {
            Project = "2024/001",
            Year = 2024,
            WgGrade = "SEO"
        };

        // Act
        var result = await repo.AddStaffRequirementAsync(newReq);

        // Assert
        Assert.Same(newReq, result);
    }

    #endregion

    #region UpdateStaffRequirementAsync

    [Fact]
    public async Task UpdateStaffRequirementAsync_UpdatesEntity_AndCallsSaveChanges()
    {
        // Arrange
        var existing = new StaffRequirement
        {
            SrIdentity = 1,
            Project = "2024/001",
            Year = 2024,
            WgGrade = "HEO",
            Nohours = 100.0,
            Chargerate = 45.50
        };
        var wggs = new List<WorkGroupGrade>
        {
            new() { WgGrade = "HEO", FpsYear = DefaultFpsYear, ProfitCentreGrade = "PCG1", GradeCode = "GC01", WorkGroup = "Science" }
        };
        var pcgs = new List<ProfitCentreGrade>
        {
            new() { PcGrade = "PCG1", FpsYear = DefaultFpsYear, PayRate = 200m, Npr = 70m, Ohr = 30m, ChargeRate = 10m, DefraChargeRate = 10m, DivisionGrade = "D1", GradeCode = "GC01", ProfitCentre = "PC1" }
        };
        var (repo, staffReqDbSet, mockContext) = CreateRepository(staffRequirements: [existing], workGroupGrades: wggs, profitCentreGrades: pcgs);

        existing.Nohours = 200.0;

        // Act
        var result = await repo.UpdateStaffRequirementAsync(existing);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200.0, result.Nohours);
        Assert.Equal(200.0, result.Payrate);
        Assert.Equal(70.0, result.Npr);
        Assert.Equal(30.0, result.Ohr);
        staffReqDbSet.Verify(x => x.Update(It.IsAny<StaffRequirement>()), Times.Once);
        RepositoryTestHelper.VerifySaveChanges(mockContext);
    }

    [Fact]
    public async Task UpdateStaffRequirementAsync_DecodesUrlEncodedProject()
    {
        // Arrange
        var (repo, _, _) = CreateRepository();
        var entity = new StaffRequirement
        {
            Project = "2024%2F001",
            Year = 2024,
            WgGrade = "EO"
        };

        // Act
        var result = await repo.UpdateStaffRequirementAsync(entity);

        // Assert
        Assert.Equal("2024/001", result.Project);
    }

    #endregion

    #region GetStaffRequirementsByProjectYearAsync

    private static PaginationParameters<string> DefaultQuery(int page = 1, int pageSize = 100) =>
        new() { Page = page, PageSize = pageSize };

    /// <summary>
    /// Helper to create a matching WorkGroupGrade for a given WgGrade.
    /// The query joins on wgg.GradeCode → eu.VlaGrade, so when wgg is null
    /// in LINQ-to-Objects the join key accessor throws NullReferenceException.
    /// Always provide a matching WorkGroupGrade for each StaffRequirement in tests.
    /// </summary>
    private static WorkGroupGrade CreateWgg(string wgGrade, string gradeCode = "GC01") =>
        new() { WgGrade = wgGrade, FpsYear = DefaultFpsYear, WorkGroup = "Default", GradeCode = gradeCode, ProfitCentreGrade = "PCG1" };

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_ReturnsEmptyList_WhenNoRequirements()
    {
        // Arrange
        var (repo, _, _) = CreateRepository();

        // Act
        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, DefaultQuery());

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_FiltersByProjectAndYear()
    {
        // Arrange
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "HEO" },
            new() { SrIdentity = 2, Project = "2024/002", Year = 2024, WgGrade = "EO" },
            new() { SrIdentity = 3, Project = "2024/001", Year = 2025, WgGrade = "SEO" }
        };
        var wggs = new List<WorkGroupGrade> { CreateWgg("HEO"), CreateWgg("EO"), CreateWgg("SEO") };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs);

        // Act
        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, DefaultQuery());

        // Assert
        Assert.Single(result.Data);
        Assert.Equal(1, result.Data.First().SrIdentity);
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_JoinsWorkGroupGradeData()
    {
        // Arrange
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "HEO" }
        };
        var wggs = new List<WorkGroupGrade>
        {
            new() { WgGrade = "HEO", FpsYear = DefaultFpsYear, WorkGroup = "Science", GradeCode = "GC01", ProfitCentreGrade = "PCG1" }
        };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs);

        // Act
        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, DefaultQuery());

        // Assert
        var item = Assert.Single(result.Data);
        Assert.Equal("Science", item.WorkGroup);
        Assert.Equal("GC01", item.GradeCode);
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_JoinsProjectData()
    {
        // Arrange
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "HEO" }
        };
        var wggs = new List<WorkGroupGrade> { CreateWgg("HEO") };
        var projects = new List<Project>
        {
            new() { ProjectId = "2024/001", Programme = "Programme X", Euroconvrate = 1.15 }
        };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs, projects: projects);

        // Act
        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, DefaultQuery());

        // Assert
        var item = Assert.Single(result.Data);
        Assert.Equal("Programme X", item.Programme);
        Assert.Equal(1.15, item.EuroConvRate);
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_ReturnsNullProjectFields_WhenNoMatchingProject()
    {
        // Arrange
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "HEO" }
        };
        var wggs = new List<WorkGroupGrade> { CreateWgg("HEO") };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs, projects: []);

        // Act
        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, DefaultQuery());

        // Assert
        var item = Assert.Single(result.Data);
        Assert.Null(item.Programme);
        Assert.Null(item.EuroConvRate);
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_JoinsEuGradeConversion()
    {
        // Arrange
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "HEO" }
        };
        var wggs = new List<WorkGroupGrade>
        {
            new() { WgGrade = "HEO", FpsYear = DefaultFpsYear, WorkGroup = "Science", GradeCode = "GC01", ProfitCentreGrade = "PCG1" }
        };
        var euConversions = new List<EuGradeConversion>
        {
            new() { VlaGrade = "GC01", EuGrade = "AD5" }
        };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs, euGradeConversions: euConversions);

        // Act
        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, DefaultQuery());

        // Assert
        var item = Assert.Single(result.Data);
        Assert.Equal("AD5", item.EuGrade);
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_ReturnsNullEuGrade_WhenNoMatchingConversion()
    {
        // Arrange
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "HEO" }
        };
        var wggs = new List<WorkGroupGrade>
        {
            new() { WgGrade = "HEO", FpsYear = DefaultFpsYear, WorkGroup = "Science", GradeCode = "GC01", ProfitCentreGrade = "PCG1" }
        };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs, euGradeConversions: []);

        // Act
        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, DefaultQuery());

        // Assert
        var item = Assert.Single(result.Data);
        Assert.Null(item.EuGrade);
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_MapsAllFields()
    {
        // Arrange
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 42, Project = "2024/001", Year = 2024, WgGrade = "HEO", Name = "John Smith", Nohours = 100.0, Nodays = 13.0, Chargerate = 45.50, Payrate = 30.0, Npr = 5.0, Ohr = 10.0 }
        };
        var wggs = new List<WorkGroupGrade> { CreateWgg("HEO") };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs);

        // Act
        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, DefaultQuery());

        // Assert
        var item = Assert.Single(result.Data);
        Assert.Equal(42, item.SrIdentity);
        Assert.Equal("2024/001", item.Project);
        Assert.Equal(2024, item.Year);
        Assert.Equal("HEO", item.WgGrade);
        Assert.Equal("John Smith", item.Name);
        Assert.Equal(100.0, item.Nohours);
        Assert.Equal(13.0, item.Nodays);
        Assert.Equal(45.50, item.Chargerate);
        Assert.Equal(30.0, item.Payrate);
        Assert.Equal(5.0, item.Npr);
        Assert.Equal(10.0, item.Ohr);
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_DefaultSortsByWgGrade()
    {
        // Arrange
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "SEO" },
            new() { SrIdentity = 2, Project = "2024/001", Year = 2024, WgGrade = "EO" },
            new() { SrIdentity = 3, Project = "2024/001", Year = 2024, WgGrade = "HEO" }
        };
        var wggs = new List<WorkGroupGrade> { CreateWgg("SEO"), CreateWgg("EO"), CreateWgg("HEO") };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs);

        // Act
        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, DefaultQuery());

        // Assert
        Assert.Equal(3, result.Data.Count());
        Assert.Equal("EO", result.Data.First().WgGrade);
        Assert.Equal("SEO", result.Data.Last().WgGrade);
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_SortsByName_Descending()
    {
        // Arrange
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "HEO", Name = "Alice" },
            new() { SrIdentity = 2, Project = "2024/001", Year = 2024, WgGrade = "EO", Name = "Charlie" },
            new() { SrIdentity = 3, Project = "2024/001", Year = 2024, WgGrade = "SEO", Name = "Bob" }
        };
        var wggs = new List<WorkGroupGrade> { CreateWgg("HEO"), CreateWgg("EO"), CreateWgg("SEO") };
        var query = new PaginationParameters<string> { Page = 1, PageSize = 100, SortBy = "name", Descending = true };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs);

        // Act
        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, query);

        // Assert
        Assert.Equal("Charlie", result.Data.First().Name);
        Assert.Equal("Alice", result.Data.Last().Name);
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_AppliesPaging()
    {
        // Arrange
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "A" },
            new() { SrIdentity = 2, Project = "2024/001", Year = 2024, WgGrade = "B" },
            new() { SrIdentity = 3, Project = "2024/001", Year = 2024, WgGrade = "C" },
            new() { SrIdentity = 4, Project = "2024/001", Year = 2024, WgGrade = "D" },
            new() { SrIdentity = 5, Project = "2024/001", Year = 2024, WgGrade = "E" }
        };
        var wggs = new List<WorkGroupGrade> { CreateWgg("A"), CreateWgg("B"), CreateWgg("C"), CreateWgg("D"), CreateWgg("E") };
        var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs);

        // Act
        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, query);

        // Assert
        Assert.Equal(2, result.Data.Count());
        Assert.Equal(5, result.PaginationData.TotalRecords);
        Assert.Equal(2, result.PaginationData.PageNumber);
        Assert.Equal(3, result.PaginationData.TotalPages);
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_DecodesUrlEncodedProject()
    {
        // Arrange
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "HEO" }
        };
        var wggs = new List<WorkGroupGrade> { CreateWgg("HEO") };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs);

        // Act
        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024%2F001", 2024, DefaultQuery());

        // Assert
        Assert.Single(result.Data);
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_JoinsAllThreeTables()
    {
        // Arrange
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "HEO" }
        };
        var wggs = new List<WorkGroupGrade>
        {
            new() { WgGrade = "HEO", FpsYear = DefaultFpsYear, WorkGroup = "Science", GradeCode = "GC01", ProfitCentreGrade = "PCG1" }
        };
        var projects = new List<Project>
        {
            new() { ProjectId = "2024/001", Programme = "Programme A", Euroconvrate = 1.20 }
        };
        var euConversions = new List<EuGradeConversion>
        {
            new() { VlaGrade = "GC01", EuGrade = "AD5" }
        };
        var (repo, _, _) = CreateRepository(
            staffRequirements: reqs,
            workGroupGrades: wggs,
            projects: projects,
            euGradeConversions: euConversions);

        // Act
        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, DefaultQuery());

        // Assert
        var item = Assert.Single(result.Data);
        Assert.Equal("Science", item.WorkGroup);
        Assert.Equal("GC01", item.GradeCode);
        Assert.Equal("Programme A", item.Programme);
        Assert.Equal(1.20, item.EuroConvRate);
        Assert.Equal("AD5", item.EuGrade);
    }

    [Theory]
    [InlineData("sridentity", false)]
    [InlineData("sridentity", true)]
    [InlineData("project", false)]
    [InlineData("project", true)]
    [InlineData("year", false)]
    [InlineData("year", true)]
    [InlineData("wggrade", false)]
    [InlineData("wggrade", true)]
    [InlineData("nohours", false)]
    [InlineData("nohours", true)]
    [InlineData("chargerate", false)]
    [InlineData("chargerate", true)]
    public async Task GetStaffRequirementsByProjectYearAsync_WithDifferentSortFields_SortsCorrectly(
        string sortBy, bool descending)
    {
        // Arrange
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "HEO", Name = "Alice", Nohours = 100, Chargerate = 50 },
            new() { SrIdentity = 2, Project = "2024/001", Year = 2024, WgGrade = "EO", Name = "Bob", Nohours = 200, Chargerate = 30 }
        };
        var wggs = new List<WorkGroupGrade> { CreateWgg("HEO"), CreateWgg("EO") };
        var query = new PaginationParameters<string> { Page = 1, PageSize = 100, SortBy = sortBy, Descending = descending };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs);

        // Act
        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, query);

        // Assert
        Assert.Equal(2, result.Data.Count());
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_WithInvalidSortBy_UsesDefaultWgGradeSort()
    {
        // Arrange
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "SEO" },
            new() { SrIdentity = 2, Project = "2024/001", Year = 2024, WgGrade = "EO" }
        };
        var wggs = new List<WorkGroupGrade> { CreateWgg("SEO"), CreateWgg("EO") };
        var query = new PaginationParameters<string> { Page = 1, PageSize = 100, SortBy = "invalid_field" };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs);

        // Act
        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, query);

        // Assert
        Assert.Equal("EO", result.Data.First().WgGrade);
    }

    #endregion

    #region UpdateStaffRequirementAsync - additional cases

    [Fact]
    public async Task UpdateStaffRequirementAsync_ReturnsSameEntityReference()
    {
        var (repo, _, _) = CreateRepository();
        var entity = new StaffRequirement
        {
            Project = "2024/001",
            Year = 2024,
            WgGrade = "EO"
        };

        var result = await repo.UpdateStaffRequirementAsync(entity);

        Assert.Same(entity, result);
    }

    #endregion

    #region AddStaffRequirementAsync - additional cases

    [Fact]
    public async Task AddStaffRequirementAsync_WithNullOptionalFields_Succeeds()
    {
        var (repo, _, _) = CreateRepository();
        var newReq = new StaffRequirement
        {
            Project = "2024/001",
            Year = 2024,
            WgGrade = "HEO",
            Name = null,
            Nohours = null,
            Nodays = null,
            Chargerate = null,
            Payrate = null,
            Npr = null,
            Ohr = null
        };

        var result = await repo.AddStaffRequirementAsync(newReq);

        Assert.NotNull(result);
        Assert.Null(result.Name);
        Assert.Null(result.Nohours);
        Assert.Null(result.Chargerate);
    }

    #endregion

    #region GetStaffRequirementsByProjectYearAsync - additional cases

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_ReturnsMultipleRecords_WhenMultipleExist()
    {
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "HEO" },
            new() { SrIdentity = 2, Project = "2024/001", Year = 2024, WgGrade = "EO" },
            new() { SrIdentity = 3, Project = "2024/001", Year = 2024, WgGrade = "SEO" }
        };
        var wggs = new List<WorkGroupGrade> { CreateWgg("HEO"), CreateWgg("EO"), CreateWgg("SEO") };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs);

        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, DefaultQuery());

        Assert.Equal(3, result.Data.Count());
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_SortsByName_Ascending()
    {
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "HEO", Name = "Charlie" },
            new() { SrIdentity = 2, Project = "2024/001", Year = 2024, WgGrade = "EO",  Name = "Alice" },
            new() { SrIdentity = 3, Project = "2024/001", Year = 2024, WgGrade = "SEO", Name = "Bob" }
        };
        var wggs = new List<WorkGroupGrade> { CreateWgg("HEO"), CreateWgg("EO"), CreateWgg("SEO") };
        var query = new PaginationParameters<string> { Page = 1, PageSize = 100, SortBy = "name", Descending = false };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs);

        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, query);

        Assert.Equal("Alice",   result.Data.First().Name);
        Assert.Equal("Charlie", result.Data.Last().Name);
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_SortsBySrIdentity_Ascending()
    {
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 3, Project = "2024/001", Year = 2024, WgGrade = "SEO" },
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "HEO" },
            new() { SrIdentity = 2, Project = "2024/001", Year = 2024, WgGrade = "EO"  }
        };
        var wggs = new List<WorkGroupGrade> { CreateWgg("SEO"), CreateWgg("HEO"), CreateWgg("EO") };
        var query = new PaginationParameters<string> { Page = 1, PageSize = 100, SortBy = "sridentity", Descending = false };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs);

        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, query);

        Assert.Equal(1, result.Data.First().SrIdentity);
        Assert.Equal(3, result.Data.Last().SrIdentity);
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_SortsBySrIdentity_Descending()
    {
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "HEO" },
            new() { SrIdentity = 3, Project = "2024/001", Year = 2024, WgGrade = "SEO" },
            new() { SrIdentity = 2, Project = "2024/001", Year = 2024, WgGrade = "EO"  }
        };
        var wggs = new List<WorkGroupGrade> { CreateWgg("HEO"), CreateWgg("SEO"), CreateWgg("EO") };
        var query = new PaginationParameters<string> { Page = 1, PageSize = 100, SortBy = "sridentity", Descending = true };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs);

        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, query);

        Assert.Equal(3, result.Data.First().SrIdentity);
        Assert.Equal(1, result.Data.Last().SrIdentity);
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_SortsByNohours_Ascending()
    {
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "HEO", Nohours = 300 },
            new() { SrIdentity = 2, Project = "2024/001", Year = 2024, WgGrade = "EO",  Nohours = 100 },
            new() { SrIdentity = 3, Project = "2024/001", Year = 2024, WgGrade = "SEO", Nohours = 200 }
        };
        var wggs = new List<WorkGroupGrade> { CreateWgg("HEO"), CreateWgg("EO"), CreateWgg("SEO") };
        var query = new PaginationParameters<string> { Page = 1, PageSize = 100, SortBy = "nohours", Descending = false };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs);

        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, query);

        Assert.Equal(100, result.Data.First().Nohours);
        Assert.Equal(300, result.Data.Last().Nohours);
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_SortsByChargerate_Descending()
    {
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "HEO", Chargerate = 20 },
            new() { SrIdentity = 2, Project = "2024/001", Year = 2024, WgGrade = "EO",  Chargerate = 80 },
            new() { SrIdentity = 3, Project = "2024/001", Year = 2024, WgGrade = "SEO", Chargerate = 50 }
        };
        var wggs = new List<WorkGroupGrade> { CreateWgg("HEO"), CreateWgg("EO"), CreateWgg("SEO") };
        var query = new PaginationParameters<string> { Page = 1, PageSize = 100, SortBy = "chargerate", Descending = true };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs);

        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, query);

        Assert.Equal(80, result.Data.First().Chargerate);
        Assert.Equal(20, result.Data.Last().Chargerate);
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_SortsByWgGrade_Descending()
    {
        var reqs = new List<StaffRequirement>
        {
            new() { SrIdentity = 1, Project = "2024/001", Year = 2024, WgGrade = "A" },
            new() { SrIdentity = 2, Project = "2024/001", Year = 2024, WgGrade = "C" },
            new() { SrIdentity = 3, Project = "2024/001", Year = 2024, WgGrade = "B" }
        };
        var wggs = new List<WorkGroupGrade> { CreateWgg("A"), CreateWgg("C"), CreateWgg("B") };
        var query = new PaginationParameters<string> { Page = 1, PageSize = 100, SortBy = "wggrade", Descending = true };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs);

        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, query);

        Assert.Equal("C", result.Data.First().WgGrade);
        Assert.Equal("A", result.Data.Last().WgGrade);
    }

    [Fact]
    public async Task GetStaffRequirementsByProjectYearAsync_PaginationData_IsPopulatedCorrectly()
    {
        var reqs = Enumerable.Range(1, 7).Select(i =>
            new StaffRequirement { SrIdentity = i, Project = "2024/001", Year = 2024, WgGrade = $"G{i:D2}" }).ToList();
        var wggs = reqs.Select(r => CreateWgg(r.WgGrade)).ToList();
        var query = new PaginationParameters<string> { Page = 1, PageSize = 3 };
        var (repo, _, _) = CreateRepository(staffRequirements: reqs, workGroupGrades: wggs);

        var result = await repo.GetStaffRequirementsByProjectYearAsync("2024/001", 2024, query);

        Assert.Equal(3, result.Data.Count());
        Assert.Equal(7, result.PaginationData.TotalRecords);
        Assert.Equal(1, result.PaginationData.PageNumber);
        Assert.Equal(3, result.PaginationData.TotalPages);
    }

    #endregion
}
