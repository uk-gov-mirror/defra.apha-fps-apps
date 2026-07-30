using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Moq;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.YearlyFinancialDataRepositoryTest
{
    public class YearlyFinancialDataRepositoryTests
    {


        // ── helper builders ──────────────────────────────────────────────

        private static YearlyFinancialDataRepository CreateRepository(
            IEnumerable<YearlyFinancialData>?  yearlyFinancialData  = null,
            IEnumerable<Projects>?             myTlkpProjects       = null,
            IEnumerable<Settings>?             databaseSettings     = null,
            IEnumerable<ProjectMonthFinal>?    projectMonthFinals   = null,
            IEnumerable<TimeCostCalcs>?        timeCostCalcs        = null,
            IEnumerable<ProjectRadTrackData>?  projectRadTrackData  = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var yfdMockSet  = RepositoryTestHelper.CreateMockDbSet(yearlyFinancialData ?? Enumerable.Empty<YearlyFinancialData>());
            var projMockSet = RepositoryTestHelper.CreateMockDbSet(myTlkpProjects      ?? Enumerable.Empty<Projects>());
            var setMockSet  = RepositoryTestHelper.CreateMockDbSet(databaseSettings    ?? Enumerable.Empty<Settings>());
            var pmfMockSet  = RepositoryTestHelper.CreateMockDbSet(projectMonthFinals  ?? Enumerable.Empty<ProjectMonthFinal>());
            var tccMockSet  = RepositoryTestHelper.CreateMockDbSet(timeCostCalcs       ?? Enumerable.Empty<TimeCostCalcs>());
            var rtdMockSet  = RepositoryTestHelper.CreateMockDbSet(projectRadTrackData ?? Enumerable.Empty<ProjectRadTrackData>());

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.YearlyFinancialData).Returns(yfdMockSet.Object);
            mockContext.Setup(x => x.MyTlkpProjects).Returns(projMockSet.Object);
            mockContext.Setup(x => x.DatabaseSettings).Returns(setMockSet.Object);
            mockContext.Setup(x => x.ProjectMonthFinals).Returns(pmfMockSet.Object);
            mockContext.Setup(x => x.TimeCostCalcs).Returns(tccMockSet.Object);
            mockContext.Setup(x => x.ProjectRadTrackData).Returns(rtdMockSet.Object);

            return new YearlyFinancialDataRepository(mockContext.Object);
        }

        private static YearlyFinancialData MakeYfd(
            short year = 2024, string project = "PP001",
            decimal? bfBudget = 10000m, string? costedBy = null,
            decimal? pyBudget = null, decimal? vlaBudget = null)
            => new()
            {
                Year      = year,
                Project   = project,
                BfBudget  = bfBudget,
                CostedBy  = costedBy,
                PyBudget  = pyBudget,
                VlaBudget = vlaBudget
            };



        private static Projects MakeProject(
            string parentproject = "PP001", short year = 2024,
            decimal? custincome = null, decimal? budgetCvl = null)
            => new()
            {
                Parentproject = parentproject,
                Year          = year,
                Custincome    = custincome,
                BudgetCvl     = budgetCvl
            };

        private static ProjectMonthFinal MakePmf(
            string project  = "PP001",
            short  year     = 2024,
            double monthNo  = 1.0,
            decimal? subcontracts  = null,
            decimal? animals       = null,
            decimal? transfercosts = null,
            decimal? totalcost     = null,
            decimal? timecosts     = null,
            double?  totalhours    = null)
            => new()
            {
                Project       = project,
                Year          = year,
                Monthno       = monthNo,
                Subcontracts  = subcontracts,
                Animals       = animals,
                Transfercosts = transfercosts,
                Totalcost     = totalcost,
                Timecosts     = timecosts,
                Totalhours    = totalhours
            };

        private static TimeCostCalcs MakeTcc(
            string project  = "PP001",
            short  year     = 2024,
            double month    = 1.0,
            decimal? pay     = null,
            decimal? nonpay  = null,
            decimal? overhead = null)
            => new()
            {
                Project  = project,
                Year     = year,
                Month    = month,
                Workgroup = "WG",
                Jobcode  = "JC",
                Staffid  = "S1",
                Pay      = pay,
                Nonpay   = nonpay,
                Overhead = overhead
            };

        private static ProjectRadTrackData MakeRtd(
            string parentproject   = "PP001",
            short  useprojectyear  = 0,
            DateTime? startdate    = null)
            => new()
            {
                Parentproject    = parentproject,
                Useprojectyear   = useprojectyear,
                Startdate        = startdate
            };

        private static Settings MakeSettings(string id, string? setting = null)
            => new() { Id = id, Setting = setting };

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_ReturnsOnlyRecordsForSpecifiedProject()
        {
            // Arrange
            var data = new[]
            {
                MakeYfd(2024, "PP001"),
                MakeYfd(2023, "PP001"),
                MakeYfd(2024, "PP002")   // different project — should be excluded
            };
            var repo   = CreateRepository(yearlyFinancialData: data);
            var paging = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetAllAsync("PP001", paging);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, r => Assert.Equal("PP001", r.Project));
        }

        [Fact]
        public async Task GetAllAsync_WithNoMatchingProject_ReturnsEmptyData()
        {
            // Arrange
            var data = new[] { MakeYfd(2024, "PP999") };
            var repo   = CreateRepository(yearlyFinancialData: data);
            var paging = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetAllAsync("PP001", paging);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllAsync_Pagination_ReturnsCorrectTotalRecords()
        {
            // Arrange
            var data = Enumerable.Range(2020, 5)
                .Select(y => MakeYfd((short)y, "PP001"))
                .ToArray();
            var repo   = CreateRepository(yearlyFinancialData: data);
            var paging = new PaginationParameters<string>(page: 1, pageSize: 2);

            // Act
            var result = await repo.GetAllAsync("PP001", paging);

            // Assert
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.Data.Count);  // page size is 2
        }

        [Fact]
        public async Task GetAllAsync_WithAllPagesRequested_ReturnsAllRecords()
        {
            // Arrange
            var data = Enumerable.Range(2020, 5)
                .Select(y => MakeYfd((short)y, "PP001"))
                .ToArray();
            var repo   = CreateRepository(yearlyFinancialData: data);
            var paging = new PaginationParameters<string>(page: -1, pageSize: 10);

            // Act
            var result = await repo.GetAllAsync("PP001", paging);

            // Assert
            Assert.Equal(5, result.Data.Count);
            Assert.Equal(1, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetAllAsync_WithSearchByCostedBy_FiltersCorrectly()
        {
            // Arrange
            var data = new[]
            {
                MakeYfd(2024, "PP001", costedBy: "alice"),
                MakeYfd(2023, "PP001", costedBy: "bob")
            };
            var repo   = CreateRepository(yearlyFinancialData: data);
            var paging = new PaginationParameters<string>(page: 1, pageSize: 10, search: "alice");

            // Act
            var result = await repo.GetAllAsync("PP001", paging);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("alice", result.Data.First().CostedBy);
        }

        #endregion

        #region GetByKeyAsync Tests

        [Fact]
        public async Task GetByKeyAsync_WithValidKey_ReturnsMatchingRecord()
        {
            // Arrange
            var data = new[] { MakeYfd(2024, "PP001"), MakeYfd(2023, "PP001") };
            var repo = CreateRepository(yearlyFinancialData: data);

            // Act
            var result = await repo.GetByKeyAsync(2024, "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal((short)2024, result.Year);
            Assert.Equal("PP001",     result.Project);
        }

        [Fact]
        public async Task GetByKeyAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var data = new[] { MakeYfd(2024, "PP001") };
            var repo = CreateRepository(yearlyFinancialData: data);

            // Act
            var result = await repo.GetByKeyAsync(9999, "UNKNOWN");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByKeyAsync_WithWrongYear_ReturnsNull()
        {
            // Arrange
            var data = new[] { MakeYfd(2024, "PP001") };
            var repo = CreateRepository(yearlyFinancialData: data);

            // Act
            var result = await repo.GetByKeyAsync(2025, "PP001");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByKeyAsync_WithWrongProject_ReturnsNull()
        {
            // Arrange
            var data = new[] { MakeYfd(2024, "PP001") };
            var repo = CreateRepository(yearlyFinancialData: data);

            // Act
            var result = await repo.GetByKeyAsync(2024, "PP002");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ExistsAsync Tests

        [Fact]
        public async Task ExistsAsync_WhenRecordExists_ReturnsTrue()
        {
            // Arrange
            var data = new[] { MakeYfd(2024, "PP001") };
            var repo = CreateRepository(yearlyFinancialData: data);

            // Act
            var result = await repo.ExistsAsync(2024, "PP001");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WhenRecordDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.ExistsAsync(2024, "PP001");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_WithWrongYearAndSameProject_ReturnsFalse()
        {
            // Arrange
            var data = new[] { MakeYfd(2024, "PP001") };
            var repo = CreateRepository(yearlyFinancialData: data);

            // Act
            var result = await repo.ExistsAsync(2025, "PP001");

            // Assert
            Assert.False(result);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidEntity_CallsSaveChangesAsync()
        {
            // Arrange
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var yfdMockSet  = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<YearlyFinancialData>());
            var pactMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<PactProjectYearCosts>());
            RepositoryTestHelper.SetupSaveChanges(mockContext);
            mockContext.Setup(x => x.YearlyFinancialData).Returns(yfdMockSet.Object);
            mockContext.Setup(x => x.PactProjectYearCosts).Returns(pactMockSet.Object);
            var repo   = new YearlyFinancialDataRepository(mockContext.Object);
            var entity = MakeYfd();

            // Act
            var result = await repo.CreateAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity, result);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CreateAsync_ReturnsTheSameEntityInstance()
        {
            // Arrange
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var yfdMockSet  = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<YearlyFinancialData>());
            var pactMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<PactProjectYearCosts>());
            RepositoryTestHelper.SetupSaveChanges(mockContext);
            mockContext.Setup(x => x.YearlyFinancialData).Returns(yfdMockSet.Object);
            mockContext.Setup(x => x.PactProjectYearCosts).Returns(pactMockSet.Object);
            var repo   = new YearlyFinancialDataRepository(mockContext.Object);
            var entity = MakeYfd(2024, "PP001", 99999m);

            // Act
            var result = await repo.CreateAsync(entity);

            // Assert
            Assert.Same(entity, result);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidEntity_CallsSaveChangesAsync()
        {
            // Arrange
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();
            var yfdMockSet  = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<YearlyFinancialData>());
            var pactMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<PactProjectYearCosts>());
            RepositoryTestHelper.SetupSaveChanges(mockContext);
            mockContext.Setup(x => x.YearlyFinancialData).Returns(yfdMockSet.Object);
            mockContext.Setup(x => x.PactProjectYearCosts).Returns(pactMockSet.Object);
            var repo   = new YearlyFinancialDataRepository(mockContext.Object);
            var entity = MakeYfd();

            // Act
            var result = await repo.UpdateAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity, result);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task UpdateAsync_ReturnsTheSameEntityInstance()
        {
            var entity = MakeYfd(2024, "PP001", 55555m);
            var repo   = CreateRepository(yearlyFinancialData: [entity]);

            var result = await repo.UpdateAsync(entity);

            Assert.Same(entity, result);
        }

        #endregion

        #region DeleteAsync Tests
       

        [Fact]
        public async Task DeleteAsync_WhenRecordExists_ThrowsFromMockQueryProvider()
        {
            // Arrange — ExecuteDeleteAsync is a bulk EF Core operation that cannot
            // be exercised against an in-memory mock query provider.
            var data = new[] { MakeYfd(2024, "PP001") };
            var repo = CreateRepository(yearlyFinancialData: data);

            // Act + Assert
            await Assert.ThrowsAnyAsync<Exception>(() => repo.DeleteAsync(2024, "PP001"));
        }

        [Fact]
        public async Task DeleteAsync_WhenRecordDoesNotExist_ThrowsFromMockQueryProvider()
        {
            // Arrange — ExecuteDeleteAsync is a bulk EF Core operation that cannot
            // be exercised against an in-memory mock query provider.
            var repo = CreateRepository();

            // Act + Assert
            await Assert.ThrowsAnyAsync<Exception>(() => repo.DeleteAsync(9999, "UNKNOWN"));
        }

        #endregion

        #region GetPactCostsAsync Tests

        [Fact]
        public async Task GetPactCostsAsync_WithNoMonthlyRows_ReturnsEmptyList()
        {
            var repo = CreateRepository();
            var result = await repo.GetPactCostsAsync("PP001", 2024);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPactCostsAsync_ReturnsOnlyRowsForProjectAndYear()
        {
            // Arrange — calendar year = fiscal year (useprojectyear = 0)
            var pmfData = new[]
            {
                MakePmf("PP001", 2024, 1.0),
                MakePmf("PP001", 2024, 2.0),
                MakePmf("PP002", 2024, 1.0),   // different project — excluded
                MakePmf("PP001", 2023, 1.0)    // different year — excluded
            };
            var rtd  = new[] { MakeRtd("PP001", useprojectyear: 0) };
            var repo = CreateRepository(projectMonthFinals: pmfData, projectRadTrackData: rtd);

            var result = await repo.GetPactCostsAsync("PP001", 2024);

            Assert.Equal(2, result.Count);
            Assert.All(result, r =>
            {
                Assert.Equal("PP001", r.Project);
                Assert.Equal(2024.0,  r.Year);
            });
        }

        [Fact]
        public async Task GetPactCostsAsync_ReturnsRowsOrderedByMonthNo()
        {
            var pmfData = new[]
            {
                MakePmf("PP001", 2024, 3.0),
                MakePmf("PP001", 2024, 1.0),
                MakePmf("PP001", 2024, 2.0)
            };
            var rtd  = new[] { MakeRtd("PP001", useprojectyear: 0) };
            var repo = CreateRepository(projectMonthFinals: pmfData, projectRadTrackData: rtd);

            var result = await repo.GetPactCostsAsync("PP001", 2024);

            Assert.Equal(3, result.Count);
            Assert.Equal(1.0, result[0].MonthNo);
            Assert.Equal(2.0, result[1].MonthNo);
            Assert.Equal(3.0, result[2].MonthNo);
        }

        [Fact]
        public async Task GetPactCostsAsync_AggregatesCostColumnsFromProjectMonthFinal()
        {
            // Two rows for same project+year+month — totals must be summed
            var pmfData = new[]
            {
                MakePmf("PP001", 2024, 1.0, subcontracts: 100m, animals: 50m,
                                            transfercosts: 200m, totalcost: 350m,
                                            timecosts: 10m, totalhours: 40d),
                MakePmf("PP001", 2024, 1.0, subcontracts: 100m, animals: 50m,
                                            transfercosts: 200m, totalcost: 350m,
                                            timecosts: 10m, totalhours: 40d)
            };
            var rtd  = new[] { MakeRtd("PP001", useprojectyear: 0) };
            var repo = CreateRepository(projectMonthFinals: pmfData, projectRadTrackData: rtd);

            var result = await repo.GetPactCostsAsync("PP001", 2024);

            Assert.Single(result);
            Assert.Equal(200m, result[0].SubContracts);
            Assert.Equal(100m, result[0].Animals);
            Assert.Equal(400m, result[0].Tests);
            Assert.Equal(700m, result[0].TotalCosts);
            Assert.Equal(20m,  result[0].TimeCost);
            Assert.Equal(80d,  result[0].Hours);
        }

        [Fact]
        public async Task GetPactCostsAsync_PopulatesPayAndNonPayOHFromTimeCostCalcs()
        {
            var pmfData = new[] { MakePmf("PP001", 2024, 1.0) };
            var tccData = new[]
            {
                MakeTcc("PP001", 2024, 1.0, pay: 1000m, nonpay: 300m, overhead: 200m),
                MakeTcc("PP001", 2024, 1.0, pay:  500m, nonpay: 100m, overhead:  50m)
            };
            var rtd  = new[] { MakeRtd("PP001", useprojectyear: 0) };
            var repo = CreateRepository(projectMonthFinals: pmfData, timeCostCalcs: tccData,
                                        projectRadTrackData: rtd);

            var result = await repo.GetPactCostsAsync("PP001", 2024);

            Assert.Single(result);
            Assert.Equal(1500m, result[0].Pay);       // 1000 + 500
            Assert.Equal(650m,  result[0].NonPayOH);  // (300+200) + (100+50)
        }

        [Fact]
        public async Task GetPactCostsAsync_WhenNoTimeCostCalcsRows_PayAndNonPayOHAreZero()
        {
            var pmfData = new[] { MakePmf("PP001", 2024, 1.0) };
            var rtd     = new[] { MakeRtd("PP001", useprojectyear: 0) };
            var repo    = CreateRepository(projectMonthFinals: pmfData, projectRadTrackData: rtd);

            var result = await repo.GetPactCostsAsync("PP001", 2024);

            Assert.Single(result);
            Assert.Equal(0m, result[0].Pay);
            Assert.Equal(0m, result[0].NonPayOH);
        }

        [Fact]
        public async Task GetPactCostsAsync_WhenProjectHasCustIncomeAndBudgetCvl_PopulatesAllRows()
        {
            var pmfData  = new[] { MakePmf("PP001", 2024, 1.0), MakePmf("PP001", 2024, 2.0) };
            var projects = new[] { MakeProject("PP001", 2024, custincome: 5000m, budgetCvl: 1500m) };
            var rtd      = new[] { MakeRtd("PP001", useprojectyear: 0) };
            var repo     = CreateRepository(projectMonthFinals: pmfData, myTlkpProjects: projects,
                                            projectRadTrackData: rtd);

            var result = await repo.GetPactCostsAsync("PP001", 2024);

            Assert.Equal(2, result.Count);
            Assert.All(result, r =>
            {
                Assert.Equal(5000m, r.CustIncome);
                Assert.Equal(1500m, r.BudgetCvl);
            });
        }

        [Fact]
        public async Task GetPactCostsAsync_WhenNoProjectRow_SetsCustIncomeAndBudgetCvlToNull()
        {
            var pmfData = new[] { MakePmf("PP001", 2024, 1.0) };
            var rtd     = new[] { MakeRtd("PP001", useprojectyear: 0) };
            var repo    = CreateRepository(projectMonthFinals: pmfData, projectRadTrackData: rtd);

            var result = await repo.GetPactCostsAsync("PP001", 2024);

            Assert.Single(result);
            Assert.Null(result[0].CustIncome);
            Assert.Null(result[0].BudgetCvl);
        }

        [Fact]
        public async Task GetPactCostsAsync_WithUseProjectYearMinus1_DerivesYearFromStartDate()
        {
            
            var pmfData = new[]
            {
                MakePmf("PP001", 2015, 1.0),
                MakePmf("PP001", 2015, 2.0),
                MakePmf("PP001", 2014, 12.0)  // MonthNo 12: shift=12+3-1=14 → Jan2014+14m=Mar2015 → year 2015
            };
            var rtd  = new[]
            {
                MakeRtd("PP001", useprojectyear: -1, startdate: new DateTime(2015, 1, 5))
            };
            var repo = CreateRepository(projectMonthFinals: pmfData, projectRadTrackData: rtd);

            var result = await repo.GetPactCostsAsync("PP001", 2015);

            // All three calendar rows derive to fiscal year 2015
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetPactCostsAsync_WithUseProjectYearMinus1_ExcludesRowsDerivedToDifferentYear()
        {
            // StartDate = April (month 4). MonthNo 1: shift = 1+3-4 = 0 → year unchanged = 2024.
            // MonthNo 12: shift = 12+3-4 = 11 → Jan2024 + 11 months = Dec 2024 → year 2024.
            // Request for year 2023 should return nothing.
            var pmfData = new[] { MakePmf("PP001", 2024, 1.0), MakePmf("PP001", 2024, 12.0) };
            var rtd     = new[] { MakeRtd("PP001", useprojectyear: -1, startdate: new DateTime(2024, 4, 1)) };
            var repo    = CreateRepository(projectMonthFinals: pmfData, projectRadTrackData: rtd);

            var result = await repo.GetPactCostsAsync("PP001", 2023);

            Assert.Empty(result);
        }

        #endregion

        #region GetSettingValueByIdAsync Tests

        [Fact]
        public async Task GetSettingValueByIdAsync_WhenSettingExists_ReturnsSettingValue()
        {
            // Arrange
            var settings = new[] { MakeSettings("HoursInDay", "7.4"), MakeSettings("DaysInYear", "220") };
            var repo     = CreateRepository(databaseSettings: settings);

            // Act
            var result = await repo.GetSettingValueByIdAsync("HoursInDay");

            // Assert
            Assert.Equal("7.4", result);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WhenSettingNotFound_ReturnsNull()
        {
            // Arrange
            var settings = new[] { MakeSettings("HoursInDay", "7.4") };
            var repo     = CreateRepository(databaseSettings: settings);

            // Act
            var result = await repo.GetSettingValueByIdAsync("UnknownSetting");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithEmptyTable_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository();

            // Act
            var result = await repo.GetSettingValueByIdAsync("HoursInDay");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WhenSettingValueIsNull_ReturnsNull()
        {
            // Arrange — row exists but the Setting column is null
            var settings = new[] { MakeSettings("EmptySetting", null) };
            var repo     = CreateRepository(databaseSettings: settings);

            // Act
            var result = await repo.GetSettingValueByIdAsync("EmptySetting");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSettingValueByIdAsync_WithMultipleSettings_ReturnsOnlyMatchingId()
        {
            // Arrange
            var settings = new[]
            {
                MakeSettings("HoursInDay",  "7.4"),
                MakeSettings("DaysInYear",  "220"),
                MakeSettings("WeeksInYear", "44")
            };
            var repo = CreateRepository(databaseSettings: settings);

            // Act
            var result = await repo.GetSettingValueByIdAsync("DaysInYear");

            // Assert
            Assert.Equal("220", result);
        }

        #endregion
    }
}
