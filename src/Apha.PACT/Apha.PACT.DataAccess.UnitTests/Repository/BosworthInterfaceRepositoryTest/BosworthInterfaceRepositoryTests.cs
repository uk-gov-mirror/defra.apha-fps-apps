using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.BosworthInterfaceRepositoryTest
{
    public class BosworthInterfaceRepositoryTests
    {
        private const int DefaultFpsYear = 2024;

        private static (
            BosworthInterfaceRepository Repo,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithData(
                List<TimeCostCalcs>? timeCostCalcs = null,
                List<WorkGroupStaffView>? staffViews = null,
                List<Project>? projects = null,
                List<PactWorkGroupGradeView>? gradeViews = null,
                List<WorkGroup>? workGroups = null,
                List<ProfitCentre>? profitCentres = null,
                List<MonthlyOutput>? monthlyOutputs = null,
                List<TestCapability>? testCapabilities = null,
                List<TestRequirement>? testRequirements = null,
                List<Program>? programs = null,
                int fpsYear = DefaultFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var timeCostCalcsDbSet = RepositoryTestHelper.CreateMockDbSet(timeCostCalcs ?? []);
            var staffViewsDbSet = RepositoryTestHelper.CreateMockDbSet(staffViews ?? []);
            var projectsDbSet = RepositoryTestHelper.CreateMockDbSet(projects ?? []);
            var gradeViewsDbSet = RepositoryTestHelper.CreateMockDbSet(gradeViews ?? []);
            var workGroupsDbSet = RepositoryTestHelper.CreateMockDbSet(workGroups ?? []);
            var profitCentresDbSet = RepositoryTestHelper.CreateMockDbSet(profitCentres ?? []);
            var monthlyOutputsDbSet = RepositoryTestHelper.CreateMockDbSet(monthlyOutputs ?? []);
            var testCapabilitiesDbSet = RepositoryTestHelper.CreateMockDbSet(testCapabilities ?? []);
            var testRequirementsDbSet = RepositoryTestHelper.CreateMockDbSet(testRequirements ?? []);
            var programsDbSet = RepositoryTestHelper.CreateMockDbSet(programs ?? []);

            mockContext.Setup(x => x.TimeCostCalcs).Returns(timeCostCalcsDbSet.Object);
            mockContext.Setup(x => x.WorkGroupStaffViews).Returns(staffViewsDbSet.Object);
            mockContext.Setup(x => x.Projects).Returns(projectsDbSet.Object);
            mockContext.Setup(x => x.PactWorkGroupGradeViews).Returns(gradeViewsDbSet.Object);
            mockContext.Setup(x => x.WorkGroups).Returns(workGroupsDbSet.Object);
            mockContext.Setup(x => x.ProfitCentres).Returns(profitCentresDbSet.Object);
            mockContext.Setup(x => x.MonthlyOutputs).Returns(monthlyOutputsDbSet.Object);
            mockContext.Setup(x => x.TestCapabilities).Returns(testCapabilitiesDbSet.Object);
            mockContext.Setup(x => x.TestRequirements).Returns(testRequirementsDbSet.Object);
            mockContext.Setup(x => x.Programs).Returns(programsDbSet.Object);

            var repo = new BosworthInterfaceRepository(mockContext.Object);
            return (repo, mockContext);
        }

        #region GetTimePurchaseProjectAsync

        [Fact]
        public async Task GetTimePurchaseProjectAsync_WithMatchingData_ReturnsJoinedResults()
        {
            var timeCostCalcs = new List<TimeCostCalcs>
            {
                new() { Project = "PRJ1", StaffId = "S1", WorkGroup = "WG1", JobCode = "JC1", Month = 1, FpsYear = DefaultFpsYear, Time = 8.0, Cost = 100.0, GradeCode = "G1", Name = "Staff1" }
            };
            var staffViews = new List<WorkGroupStaffView>
            {
                new() { PactId = "S1", Name = "John", WorkGroupGrade = "WGG1", FpsYear = DefaultFpsYear }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Program = "PGM1", ProjectTitle = "Test", Customer = "C1", Disease = "D1", Contract = "CON1", ProjectStatus = "A", IncomeAccountCode = "IAC1", FpsYear = DefaultFpsYear }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WGG1", WorkGroup = "SellingWG1", GradeCode = "GC1", FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(
                timeCostCalcs: timeCostCalcs,
                staffViews: staffViews,
                projects: projects,
                gradeViews: gradeViews);

            var result = await repo.GetTimePurchaseProjectAsync("PRJ1");

            Assert.Single(result);
            var item = result.First();
            Assert.Equal("PRJ1", item.Project);
            Assert.Equal("SellingWG1", item.SellingWg);
            Assert.Equal("GC1", item.GradeCode);
            Assert.Equal("John", item.Name);
            Assert.Equal(8.0, item.Time);
            Assert.Equal(100.0, item.Cost);
            Assert.Equal(1, item.Month);
            Assert.Equal("JC1", item.JobCode);
        }

        [Fact]
        public async Task GetTimePurchaseProjectAsync_NoMatchingProject_ReturnsEmpty()
        {
            var timeCostCalcs = new List<TimeCostCalcs>
            {
                new() { Project = "PRJ2", StaffId = "S1", WorkGroup = "WG1", JobCode = "JC1", Month = 1, FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(timeCostCalcs: timeCostCalcs);

            var result = await repo.GetTimePurchaseProjectAsync("PRJ1");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTimePurchaseProjectAsync_NoJoinMatch_ReturnsEmpty()
        {
            var timeCostCalcs = new List<TimeCostCalcs>
            {
                new() { Project = "PRJ1", StaffId = "S1", WorkGroup = "WG1", JobCode = "JC1", Month = 1, FpsYear = DefaultFpsYear }
            };
            var staffViews = new List<WorkGroupStaffView>
            {
                new() { PactId = "S2", Name = "Jane", WorkGroupGrade = "WGG1", FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(
                timeCostCalcs: timeCostCalcs,
                staffViews: staffViews);

            var result = await repo.GetTimePurchaseProjectAsync("PRJ1");

            Assert.Empty(result);
        }

        #endregion

        #region GetTimeSaleProfitCentreAsync

        [Fact]
        public async Task GetTimeSaleProfitCentreAsync_WithMatchingData_ReturnsGroupedResults()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1", FpsYear = DefaultFpsYear }
            };
            var timeCostCalcs = new List<TimeCostCalcs>
            {
                new() { WorkGroup = "WG1", Project = "PRJ1", StaffId = "S1", JobCode = "JC1", Month = 1, FpsYear = DefaultFpsYear, GradeCode = "G1", Name = "Staff1", Time = 5.0, Cost = 50.0 },
                new() { WorkGroup = "WG1", Project = "PRJ1", StaffId = "S1", JobCode = "JC1", Month = 2, FpsYear = DefaultFpsYear, GradeCode = "G1", Name = "Staff1", Time = 3.0, Cost = 30.0 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Program = "PGM1", ProjectTitle = "Test", Customer = "C1", Disease = "D1", Contract = "CON1", ProjectStatus = "A", IncomeAccountCode = "IAC1", FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(
                workGroups: workGroups,
                timeCostCalcs: timeCostCalcs,
                projects: projects);

            var result = await repo.GetTimeSaleProfitCentreAsync("PC1");

            Assert.NotEmpty(result);
            var item = result.First();
            Assert.Equal("PC1", item.ProfitCentre);
            Assert.Equal("WG1", item.WorkGroup);
            Assert.Equal("PRJ1", item.ParentProject);
        }

        [Fact]
        public async Task GetTimeSaleProfitCentreAsync_NoMatchingProfitCentre_ReturnsEmpty()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC2", FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(workGroups: workGroups);

            var result = await repo.GetTimeSaleProfitCentreAsync("PC1");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTimeSaleProfitCentreAsync_NoTimeCostCalcsForWorkGroup_ReturnsEmpty()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1", FpsYear = DefaultFpsYear }
            };
            var timeCostCalcs = new List<TimeCostCalcs>
            {
                new() { WorkGroup = "WG2", Project = "PRJ1", StaffId = "S1", JobCode = "JC1", Month = 1, FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(
                workGroups: workGroups,
                timeCostCalcs: timeCostCalcs);

            var result = await repo.GetTimeSaleProfitCentreAsync("PC1");

            Assert.Empty(result);
        }

        #endregion

        #region GetTimeSaleWorkGroupAsync

        [Fact]
        public async Task GetTimeSaleWorkGroupAsync_WithMatchingData_ReturnsJoinedResults()
        {
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WGG1", WorkGroup = "WG1", GradeCode = "GC1", FpsYear = DefaultFpsYear }
            };
            var staffViews = new List<WorkGroupStaffView>
            {
                new() { PactId = "S1", Name = "John", WorkGroupGrade = "WGG1", FpsYear = DefaultFpsYear }
            };
            var timeCostCalcs = new List<TimeCostCalcs>
            {
                new() { Project = "PRJ1", StaffId = "S1", WorkGroup = "WG1", JobCode = "JC1", Month = 1, FpsYear = DefaultFpsYear, Time = 8.0, Cost = 100.0 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Program = "PGM1", Manager = "MGR1", ProjectTitle = "Test", Customer = "C1", Disease = "D1", Contract = "CON1", ProjectStatus = "A", IncomeAccountCode = "IAC1", FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(
                gradeViews: gradeViews,
                staffViews: staffViews,
                timeCostCalcs: timeCostCalcs,
                projects: projects);

            var result = await repo.GetTimeSaleWorkGroupAsync("WG1");

            Assert.Single(result);
            var item = result.First();
            Assert.Equal("WG1", item.SellingWg);
            Assert.Equal("John", item.Name);
            Assert.Equal(8.0, item.Time);
            Assert.Equal(100.0, item.Cost);
            Assert.Equal(1, item.Month);
            Assert.Equal(string.Empty, item.PlanCategory);
            Assert.Equal("PGM1", item.Program);
            Assert.Equal("PRJ1", item.Project);
            Assert.Equal("JC1", item.JobCode);
            Assert.Equal("MGR1", item.Manager);
        }

        [Fact]
        public async Task GetTimeSaleWorkGroupAsync_NoMatchingWorkGroup_ReturnsEmpty()
        {
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WGG1", WorkGroup = "WG2", GradeCode = "GC1", FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(gradeViews: gradeViews);

            var result = await repo.GetTimeSaleWorkGroupAsync("WG1");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTimeSaleWorkGroupAsync_NoStaffJoinMatch_ReturnsEmpty()
        {
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WGG1", WorkGroup = "WG1", GradeCode = "GC1", FpsYear = DefaultFpsYear }
            };
            var staffViews = new List<WorkGroupStaffView>
            {
                new() { PactId = "S1", Name = "John", WorkGroupGrade = "WGG2", FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(
                gradeViews: gradeViews,
                staffViews: staffViews);

            var result = await repo.GetTimeSaleWorkGroupAsync("WG1");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTimeSaleWorkGroupAsync_NoProjectJoinMatch_ReturnsEmpty()
        {
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WGG1", WorkGroup = "WG1", GradeCode = "GC1", FpsYear = DefaultFpsYear }
            };
            var staffViews = new List<WorkGroupStaffView>
            {
                new() { PactId = "S1", Name = "John", WorkGroupGrade = "WGG1", FpsYear = DefaultFpsYear }
            };
            var timeCostCalcs = new List<TimeCostCalcs>
            {
                new() { Project = "PRJ2", StaffId = "S1", WorkGroup = "WG1", JobCode = "JC1", Month = 1, FpsYear = DefaultFpsYear, Time = 8.0, Cost = 100.0 }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Program = "PGM1", ProjectTitle = "Test", Customer = "C1", Disease = "D1", Contract = "CON1", ProjectStatus = "A", IncomeAccountCode = "IAC1", FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(
                gradeViews: gradeViews,
                staffViews: staffViews,
                timeCostCalcs: timeCostCalcs,
                projects: projects);

            var result = await repo.GetTimeSaleWorkGroupAsync("WG1");

            Assert.Empty(result);
        }

        #endregion

        #region GetTestSaleSellingWorkgroupAsync

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_WithMatchingData_ReturnsResultsWithFeeCalculation()
        {
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { WorkGroup = "WG1", Buyer = "PRJ1", TestCode = "TC1", Month = 1, Volume = 10.0, FpsYear = DefaultFpsYear }
            };
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1", FpsYear = DefaultFpsYear }
            };
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC1", ProfitCentreName = "ProfitCentre1", Division = "DIV1" }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Program = "PGM1", ProjectTitle = "Test", Customer = "C1", Disease = "D1", Contract = "CON1", ProjectStatus = "A", IncomeAccountCode = "IAC1", FpsYear = DefaultFpsYear }
            };
            var testCapabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "TC1", PlanPortfolio = "PORT1", FpsYear = DefaultFpsYear }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { Buyer = "PRJ1", TestCode = "TC1", UnitPrice = 25.50m, FpsYear = DefaultFpsYear }
            };
            var programs = new List<Program>
            {
                new() { ProgramNo = "PGM1", ProgramName = "Program1", FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(
                monthlyOutputs: monthlyOutputs,
                workGroups: workGroups,
                profitCentres: profitCentres,
                projects: projects,
                testCapabilities: testCapabilities,
                testRequirements: testRequirements,
                programs: programs);

            var result = await repo.GetTestSaleSellingWorkgroupAsync("WG1");

            Assert.Single(result);
            var item = result.First();
            Assert.Equal("PGM1", item.ProgramNo);
            Assert.Equal("PRJ1", item.Buyer);
            Assert.Equal("WG1", item.SellerWG);
            Assert.Equal("PORT1", item.Portfolio);
            Assert.Equal("TC1", item.TestCode);
            Assert.Equal(1, item.Month);
            Assert.Equal(10.0, item.Volume);
            Assert.Equal(255.00m, item.Fee); // 10.0 * 25.50
        }

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_BuyerStartsWithFT_BuyerTypeIsCommercial()
        {
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { WorkGroup = "WG1", Buyer = "FT001", TestCode = "TC1", Month = 1, Volume = 5.0, FpsYear = DefaultFpsYear }
            };
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1", FpsYear = DefaultFpsYear }
            };
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC1", ProfitCentreName = "ProfitCentre1", Division = "DIV1" }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "FT001", Program = "PGM1", ProjectTitle = "Test", Customer = "C1", Disease = "D1", Contract = "CON1", ProjectStatus = "A", IncomeAccountCode = "IAC1", FpsYear = DefaultFpsYear }
            };
            var testCapabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "TC1", PlanPortfolio = "PORT1", FpsYear = DefaultFpsYear }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { Buyer = "FT001", TestCode = "TC1", UnitPrice = 10.0m, FpsYear = DefaultFpsYear }
            };
            var programs = new List<Program>
            {
                new() { ProgramNo = "PGM1", ProgramName = "Program1", FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(
                monthlyOutputs: monthlyOutputs,
                workGroups: workGroups,
                profitCentres: profitCentres,
                projects: projects,
                testCapabilities: testCapabilities,
                testRequirements: testRequirements,
                programs: programs);

            var result = await repo.GetTestSaleSellingWorkgroupAsync("WG1");

            Assert.Single(result);
            Assert.Equal("Commercial", result.First().BuyerType);
        }

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_BuyerStartsWithUT_BuyerTypeIsCommercial()
        {
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { WorkGroup = "WG1", Buyer = "UT002", TestCode = "TC1", Month = 1, Volume = 5.0, FpsYear = DefaultFpsYear }
            };
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1", FpsYear = DefaultFpsYear }
            };
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC1", ProfitCentreName = "ProfitCentre1", Division = "DIV1" }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "UT002", Program = "PGM1", ProjectTitle = "Test", Customer = "C1", Disease = "D1", Contract = "CON1", ProjectStatus = "A", IncomeAccountCode = "IAC1", FpsYear = DefaultFpsYear }
            };
            var testCapabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "TC1", PlanPortfolio = "PORT1", FpsYear = DefaultFpsYear }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { Buyer = "UT002", TestCode = "TC1", UnitPrice = 10.0m, FpsYear = DefaultFpsYear }
            };
            var programs = new List<Program>
            {
                new() { ProgramNo = "PGM1", ProgramName = "Program1", FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(
                monthlyOutputs: monthlyOutputs,
                workGroups: workGroups,
                profitCentres: profitCentres,
                projects: projects,
                testCapabilities: testCapabilities,
                testRequirements: testRequirements,
                programs: programs);

            var result = await repo.GetTestSaleSellingWorkgroupAsync("WG1");

            Assert.Single(result);
            Assert.Equal("Commercial", result.First().BuyerType);
        }

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_BuyerDoesNotStartWithFTOrUT_BuyerTypeIsBuyer()
        {
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { WorkGroup = "WG1", Buyer = "OTHER1", TestCode = "TC1", Month = 1, Volume = 5.0, FpsYear = DefaultFpsYear }
            };
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1", FpsYear = DefaultFpsYear }
            };
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC1", ProfitCentreName = "ProfitCentre1", Division = "DIV1" }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "OTHER1", Program = "PGM1", ProjectTitle = "Test", Customer = "C1", Disease = "D1", Contract = "CON1", ProjectStatus = "A", IncomeAccountCode = "IAC1", FpsYear = DefaultFpsYear }
            };
            var testCapabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "TC1", PlanPortfolio = "PORT1", FpsYear = DefaultFpsYear }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { Buyer = "OTHER1", TestCode = "TC1", UnitPrice = 10.0m, FpsYear = DefaultFpsYear }
            };
            var programs = new List<Program>
            {
                new() { ProgramNo = "PGM1", ProgramName = "Program1", FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(
                monthlyOutputs: monthlyOutputs,
                workGroups: workGroups,
                profitCentres: profitCentres,
                projects: projects,
                testCapabilities: testCapabilities,
                testRequirements: testRequirements,
                programs: programs);

            var result = await repo.GetTestSaleSellingWorkgroupAsync("WG1");

            Assert.Single(result);
            Assert.Equal("OTHER1", result.First().BuyerType);
        }

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_NullVolume_FeeIsNull()
        {
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { WorkGroup = "WG1", Buyer = "PRJ1", TestCode = "TC1", Month = 1, Volume = null, FpsYear = DefaultFpsYear }
            };
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1", FpsYear = DefaultFpsYear }
            };
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC1", ProfitCentreName = "ProfitCentre1", Division = "DIV1" }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Program = "PGM1", ProjectTitle = "Test", Customer = "C1", Disease = "D1", Contract = "CON1", ProjectStatus = "A", IncomeAccountCode = "IAC1", FpsYear = DefaultFpsYear }
            };
            var testCapabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "TC1", PlanPortfolio = "PORT1", FpsYear = DefaultFpsYear }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { Buyer = "PRJ1", TestCode = "TC1", UnitPrice = 25.50m, FpsYear = DefaultFpsYear }
            };
            var programs = new List<Program>
            {
                new() { ProgramNo = "PGM1", ProgramName = "Program1", FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(
                monthlyOutputs: monthlyOutputs,
                workGroups: workGroups,
                profitCentres: profitCentres,
                projects: projects,
                testCapabilities: testCapabilities,
                testRequirements: testRequirements,
                programs: programs);

            var result = await repo.GetTestSaleSellingWorkgroupAsync("WG1");

            Assert.Single(result);
            Assert.Null(result.First().Fee);
        }

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_NullUnitPrice_FeeIsNull()
        {
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { WorkGroup = "WG1", Buyer = "PRJ1", TestCode = "TC1", Month = 1, Volume = 10.0, FpsYear = DefaultFpsYear }
            };
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1", FpsYear = DefaultFpsYear }
            };
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC1", ProfitCentreName = "ProfitCentre1", Division = "DIV1" }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Program = "PGM1", ProjectTitle = "Test", Customer = "C1", Disease = "D1", Contract = "CON1", ProjectStatus = "A", IncomeAccountCode = "IAC1", FpsYear = DefaultFpsYear }
            };
            var testCapabilities = new List<TestCapability>
            {
                new() { WorkGroup = "WG1", TestCode = "TC1", PlanPortfolio = "PORT1", FpsYear = DefaultFpsYear }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { Buyer = "PRJ1", TestCode = "TC1", UnitPrice = null, FpsYear = DefaultFpsYear }
            };
            var programs = new List<Program>
            {
                new() { ProgramNo = "PGM1", ProgramName = "Program1", FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(
                monthlyOutputs: monthlyOutputs,
                workGroups: workGroups,
                profitCentres: profitCentres,
                projects: projects,
                testCapabilities: testCapabilities,
                testRequirements: testRequirements,
                programs: programs);

            var result = await repo.GetTestSaleSellingWorkgroupAsync("WG1");

            Assert.Single(result);
            Assert.Null(result.First().Fee);
        }

        [Fact]
        public async Task GetTestSaleSellingWorkgroupAsync_NoMatchingWorkGroup_ReturnsEmpty()
        {
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { WorkGroup = "WG2", Buyer = "PRJ1", TestCode = "TC1", Month = 1, Volume = 10.0, FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(monthlyOutputs: monthlyOutputs);

            var result = await repo.GetTestSaleSellingWorkgroupAsync("WG1");

            Assert.Empty(result);
        }

        #endregion

        #region GetTestSaleBuyingProjectAsync

        [Fact]
        public async Task GetTestSaleBuyingProjectAsync_WithMatchingData_ReturnsResultsWithChargeCalculation()
        {
            var programs = new List<Program>
            {
                new() { ProgramNo = "PGM1", ProgramName = "Program1", FpsYear = DefaultFpsYear }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Program = "PGM1", ProjectTitle = "Test", Customer = "C1", Disease = "D1", Contract = "CON1", ProjectStatus = "A", IncomeAccountCode = "IAC1", FpsYear = DefaultFpsYear }
            };
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { WorkGroup = "WG1", Buyer = "PRJ1", TestCode = "TC1", Month = 1, Volume = 8.0, FpsYear = DefaultFpsYear }
            };
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1", FpsYear = DefaultFpsYear }
            };
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC1", ProfitCentreName = "ProfitCentre1", Division = "DIV1" }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { Buyer = "PRJ1", TestCode = "TC1", UnitPrice = 12.50m, FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(
                programs: programs,
                projects: projects,
                monthlyOutputs: monthlyOutputs,
                workGroups: workGroups,
                profitCentres: profitCentres,
                testRequirements: testRequirements);

            var result = await repo.GetTestSaleBuyingProjectAsync("PRJ1");

            Assert.Single(result);
            var item = result.First();
            Assert.Equal("PGM1", item.ProgramNo);
            Assert.Equal("PRJ1", item.Buyer);
            Assert.Equal("PC1", item.SellerPC);
            Assert.Equal("WG1", item.SellerWG);
            Assert.Equal("TC1", item.TestCode);
            Assert.Equal(1, item.Month);
            Assert.Equal(8.0, item.Volume);
            Assert.Equal(100.00m, item.Charge); // 12.50 * 8.0
        }

        [Fact]
        public async Task GetTestSaleBuyingProjectAsync_NullVolume_ChargeIsNull()
        {
            var programs = new List<Program>
            {
                new() { ProgramNo = "PGM1", ProgramName = "Program1", FpsYear = DefaultFpsYear }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Program = "PGM1", ProjectTitle = "Test", Customer = "C1", Disease = "D1", Contract = "CON1", ProjectStatus = "A", IncomeAccountCode = "IAC1", FpsYear = DefaultFpsYear }
            };
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { WorkGroup = "WG1", Buyer = "PRJ1", TestCode = "TC1", Month = 1, Volume = null, FpsYear = DefaultFpsYear }
            };
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1", FpsYear = DefaultFpsYear }
            };
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC1", ProfitCentreName = "ProfitCentre1", Division = "DIV1" }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { Buyer = "PRJ1", TestCode = "TC1", UnitPrice = 12.50m, FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(
                programs: programs,
                projects: projects,
                monthlyOutputs: monthlyOutputs,
                workGroups: workGroups,
                profitCentres: profitCentres,
                testRequirements: testRequirements);

            var result = await repo.GetTestSaleBuyingProjectAsync("PRJ1");

            Assert.Single(result);
            Assert.Null(result.First().Charge);
        }

        [Fact]
        public async Task GetTestSaleBuyingProjectAsync_NullUnitPrice_ChargeIsNull()
        {
            var programs = new List<Program>
            {
                new() { ProgramNo = "PGM1", ProgramName = "Program1", FpsYear = DefaultFpsYear }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ1", Program = "PGM1", ProjectTitle = "Test", Customer = "C1", Disease = "D1", Contract = "CON1", ProjectStatus = "A", IncomeAccountCode = "IAC1", FpsYear = DefaultFpsYear }
            };
            var monthlyOutputs = new List<MonthlyOutput>
            {
                new() { WorkGroup = "WG1", Buyer = "PRJ1", TestCode = "TC1", Month = 1, Volume = 8.0, FpsYear = DefaultFpsYear }
            };
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1", FpsYear = DefaultFpsYear }
            };
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC1", ProfitCentreName = "ProfitCentre1", Division = "DIV1" }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { Buyer = "PRJ1", TestCode = "TC1", UnitPrice = null, FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(
                programs: programs,
                projects: projects,
                monthlyOutputs: monthlyOutputs,
                workGroups: workGroups,
                profitCentres: profitCentres,
                testRequirements: testRequirements);

            var result = await repo.GetTestSaleBuyingProjectAsync("PRJ1");

            Assert.Single(result);
            Assert.Null(result.First().Charge);
        }

        [Fact]
        public async Task GetTestSaleBuyingProjectAsync_NoMatchingParentProject_ReturnsEmpty()
        {
            var programs = new List<Program>
            {
                new() { ProgramNo = "PGM1", ProgramName = "Program1", FpsYear = DefaultFpsYear }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PRJ2", Program = "PGM1", ProjectTitle = "Test", Customer = "C1", Disease = "D1", Contract = "CON1", ProjectStatus = "A", IncomeAccountCode = "IAC1", FpsYear = DefaultFpsYear }
            };

            var (repo, _) = CreateRepositoryWithData(
                programs: programs,
                projects: projects);

            var result = await repo.GetTestSaleBuyingProjectAsync("PRJ1");

            Assert.Empty(result);
        }

        #endregion
    }
}
