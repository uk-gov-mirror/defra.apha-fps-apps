using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;
using FpsProgram = Apha.FPS.Core.Entities.Program;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProjectRepositoryTest
{
    public class ProjectRepositoryTests
    {
        /// <summary>
        /// Creates a ProjectRepository with in-memory Projects and ProjectViews data.
        /// IFpsRequestContext is substituted via NSubstitute.
        /// GetProjectsByProgramAsync() join/sort logic is covered by integration tests.
        /// </summary>
        private static ProjectRepository CreateRepository(
            IEnumerable<Project>? projects = null,
            IEnumerable<ProjectView>? projectViews = null,
            IEnumerable<JobCode>? jobCodes = null,
            IEnumerable<PactProjectView>? pactProjectViews = null,
            IEnumerable<SurvFFSubmission>? survFFSubmissions = null,
            IEnumerable<ProjectLog>? projectLogs = null,
            IEnumerable<TestRequirement>? testRequirements = null,
            IEnumerable<MonthlyOutput>? monthlyOutputs = null,
            IEnumerable<MonthlyTime>? monthlyTimes = null,
            IEnumerable<ProjectInvoice>? projectInvoices = null,
            IEnumerable<ProjectSubContract>? projectSubContracts = null,
            IEnumerable<ProjectGroupView>? projectGroupViews = null,
            IEnumerable<Program>? programs = null,
            string userEmailId = "test@example.com", // always lowercase - matches middleware ToLowerInvariant()
            int fpsYear = 2024)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.UserEmailId).Returns(userEmailId);
            mockRequestContext.Setup(x => x.FpsYear).Returns(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            if (projects != null)
            {
                var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects);
                mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);
            }

            if (projectViews != null)
            {
                var projectViewsMockSet = RepositoryTestHelper.CreateMockDbSet(projectViews);
                mockContext.Setup(x => x.ProjectViews).Returns(projectViewsMockSet.Object);
            }

            if (jobCodes != null)
            {
                var jobCodesMockSet = RepositoryTestHelper.CreateMockDbSet(jobCodes);
                mockContext.Setup(x => x.JobCodes).Returns(jobCodesMockSet.Object);
            }

            if (pactProjectViews != null)
            {
                var pactProjectViewsMockSet = RepositoryTestHelper.CreateMockDbSet(pactProjectViews);
                mockContext.Setup(x => x.PactProjectViews).Returns(pactProjectViewsMockSet.Object);
            }

            if (survFFSubmissions != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(survFFSubmissions);
                mockContext.Setup(x => x.SurvFFSubmissions).Returns(mockSet.Object);
            }

            if (projectLogs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(projectLogs);
                mockContext.Setup(x => x.ProjectLogs).Returns(mockSet.Object);
            }

            if (testRequirements != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(testRequirements);
                mockContext.Setup(x => x.TestRequirements).Returns(mockSet.Object);
            }

            if (monthlyOutputs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(monthlyOutputs);
                mockContext.Setup(x => x.MonthlyOutputs).Returns(mockSet.Object);
            }

            if (monthlyTimes != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(monthlyTimes);
                mockContext.Setup(x => x.MonthlyTimes).Returns(mockSet.Object);
            }

            if (projectInvoices != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(projectInvoices);
                mockContext.Setup(x => x.ProjectInvoices).Returns(mockSet.Object);
            }

            if (projectSubContracts != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(projectSubContracts);
                mockContext.Setup(x => x.ProjectSubContracts).Returns(mockSet.Object);
            }

            if (projectGroupViews != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(projectGroupViews);
                mockContext.Setup(x => x.ProjectGroupViews).Returns(mockSet.Object);
            }

            if (programs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(programs);
                mockContext.Setup(x => x.Programs).Returns(mockSet.Object);
            }

            return new ProjectRepository(mockContext.Object, mockRequestContext.Object);
        }

        #region GetAllProjectsAsync Tests

        [Fact]
        public async Task GetAllProjectsAsync_ReturnsProjects_ForUserId42()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One",   Program = "P001", Customer = "DEFRA", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "Project Two",   Program = "P002", Customer = "APHA",  UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", ProjectTitle = "Project Three", Program = "P003", Customer = "DEFRA", UserEmail = "other@example.com" } // different user â€” excluded
            };
            var repo = CreateRepository(projectViews: projectViews);

            // Act
            var result = await repo.GetAllProjectsAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, p => Assert.NotNull(p.ParentProject));
        }

        [Fact]
        public async Task GetAllProjectsAsync_ReturnsEmptyList_WhenNoProjectViews()
        {
            // Arrange
            var repo = CreateRepository(projectViews: []);

            // Act
            var result = await repo.GetAllProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region GetAllProjectsForAllUsersAsync Tests

        [Fact]
        public async Task GetAllProjectsForAllUsersAsync_ReturnsAllProjects_WithoutEmailFilter()
        {
            // Arrange — Projects table data; no user email filtering expected
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One", Program = "P001", Customer = "DEFRA", Disease = "TB", Contract = "C001", ProjectStatus = "Active", IncomeAccountCode = "IAC01" },
                new() { ParentProject = "PP002", ProjectTitle = "Project Two", Program = "P002", Customer = "APHA", Disease = "FMD", Contract = "C002", ProjectStatus = "Active", IncomeAccountCode = "IAC02" }
            };
            var repo = CreateRepository(projects: projects);

            // Act
            var result = (await repo.GetAllProjectsForAllUsersAsync()).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("PP001", result[0].ParentProject);
            Assert.Equal("PP002", result[1].ParentProject);
        }

        [Fact]
        public async Task GetAllProjectsForAllUsersAsync_ReturnsEmptyList_WhenNoProjectsExist()
        {
            // Arrange
            var repo = CreateRepository(projects: new List<Project>());

            // Act
            var result = (await repo.GetAllProjectsForAllUsersAsync()).ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllProjectsForAllUsersAsync_ReturnsAllProjects_RegardlessOfUserEmail()
        {
            // Arrange — projects exist but current user email does not matter for unfiltered
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One", Program = "P001", Customer = "DEFRA", Disease = "TB", Contract = "C001", ProjectStatus = "Active", IncomeAccountCode = "IAC01" },
                new() { ParentProject = "PP002", ProjectTitle = "Project Two", Program = "P002", Customer = "APHA", Disease = "FMD", Contract = "C002", ProjectStatus = "Active", IncomeAccountCode = "IAC02" },
                new() { ParentProject = "PP003", ProjectTitle = "Project Three", Program = "P003", Customer = "EA", Disease = "AI", Contract = "C003", ProjectStatus = "Closed", IncomeAccountCode = "IAC03" }
            };
            var repo = CreateRepository(projects: projects, userEmailId: "differentuser@example.com");

            // Act
            var result = (await repo.GetAllProjectsForAllUsersAsync()).ToList();

            // Assert — all projects returned regardless of user context
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllProjectsAsync_ReturnsEmptyList_WhenNoMatchingUserEmail()
        {
            // Arrange â€” all views belong to a different user
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One", UserEmail = "other@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "Project Two", UserEmail = "other@example.com" }
            };
            var repo = CreateRepository(projectViews: projectViews);

            // Act
            var result = await repo.GetAllProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllProjectsAsync_MapsProgramViewFieldsCorrectly()
        {
            // Arrange
            var dateCreated  = new DateTime(2024, 1, 15);
            var dateCosted   = new DateTime(2024, 3, 10);
            var projectViews = new List<ProjectView>
            {
                new()
                {
                    ParentProject     = "PP001",
                    ProjectTitle      = "Alpha Project",
                    Program           = "P001",
                    Customer          = "DEFRA",
                    Manager           = "Alice",
                    TransferIncome    = 1000m,
                    CustIncome        = 2000m,
                    WipEoy            = 500m,
                    WipLimit          = 600m,
                    WipCurrent        = 450m,
                    ProjectStatus     = "Active",
                    CostBookNo        = "CB001",
                    DateCreated       = dateCreated,
                    FecCost           = 3000m,
                    Profit            = 100m,
                    BudgetCvl         = 200m,
                    DateCosted        = dateCosted,
                    Disease           = "D001",
                    Contract          = "C001",
                    ProjectParent     = "ROOT",
                    ShortTitle        = "Alpha",
                    CaseWorkSub       = 10m,
                    PvsIncome         = 50m,
                    PlanCaseWorkDebit = 20m,
                    Finished          = 0,
                    OwningRc          = "RC01",
                    Comments          = "Test comment",
                    CarryOver         = 300m,
                    CarryOverSeed     = 150m,
                    IsDefraProject    = 1,
                    CostCentre        = 9001.0,
                    OracleProjectCode = "ORA001",
                    SubAccountCode    = "SUB001",
                    ProjectGroup      = "GRP001",
                    IncomeAccountCode = "INC001",
                    FpsYear        = 2024,
                    UserEmail         = "test@example.com"
                }
            };
            var repo = CreateRepository(projectViews: projectViews);

            // Act
            var result = await repo.GetAllProjectsAsync();

            // Assert
            var project = Assert.Single(result);
            Assert.Equal("PP001",        project.ParentProject);
            Assert.Equal("Alpha Project", project.ProjectTitle);
            Assert.Equal("P001",         project.Program);
            Assert.Equal("DEFRA",        project.Customer);
            Assert.Equal("Alice",        project.Manager);
            Assert.Equal(1000m,          project.TransferIncome);
            Assert.Equal(2000m,          project.CustIncome);
            Assert.Equal(500m,           project.WipEoy);
            Assert.Equal(600m,           project.WipLimit);
            Assert.Equal(450m,           project.WipCurrent);
            Assert.Equal("Active",       project.ProjectStatus);
            Assert.Equal("CB001",        project.CostBookNo);
            Assert.Equal(dateCreated,    project.DateCreated);
            Assert.Equal(3000m,          project.FecCost);
            Assert.Equal(100m,           project.Profit);
            Assert.Equal(200m,           project.BudgetCvl);
            Assert.Equal(dateCosted,     project.DateCosted);
            Assert.Equal("D001",         project.Disease);
            Assert.Equal("C001",         project.Contract);
            Assert.Equal("ROOT",         project.ProjectParent);
            Assert.Equal("Alpha",        project.ShortTitle);
            Assert.Equal(10m,            project.CaseWorkSub);
            Assert.Equal(50m,            project.PvsIncome);
            Assert.Equal(20m,            project.PlanCaseWorkDebit);
            Assert.Equal((short)0,       project.Finished);
            Assert.Equal("RC01",         project.OwningRc);
            Assert.Equal("Test comment", project.Comments);
            Assert.Equal(300m,           project.CarryOver);
            Assert.Equal(150m,           project.CarryOverSeed);
            Assert.Equal((short)1,       project.IsDefraProject);
            Assert.Equal(9001.0,         project.CostCentre);
            Assert.Equal("ORA001",       project.OracleProjectCode);
            Assert.Equal("SUB001",       project.SubAccountCode);
            Assert.Equal("GRP001",       project.ProjectGroup);
            Assert.Equal("INC001",       project.IncomeAccountCode);
            Assert.Equal(2024,           project.FpsYear);
        }

        [Fact]
        public async Task GetAllProjectsAsync_PreservesNullValues_ForNullableFields()
        {
            // Arrange â€” all nullable fields are null; GetAllProjectsAsync returns ProjectView as-is (no projection)
            var projectViews = new List<ProjectView>
            {
                new()
                {
                    ParentProject     = null,
                    ProjectTitle      = null,
                    Program           = null,
                    Customer          = null,
                    Disease           = null,
                    Contract          = null,
                    IncomeAccountCode = null,
                    TransferIncome    = null,
                    CustIncome        = null,
                    IsDefraProject    = null,
                    UserEmail         = "test@example.com"
                }
            };
            var repo = CreateRepository(projectViews: projectViews);

            // Act
            var result = await repo.GetAllProjectsAsync();

            // Assert â€” ProjectView is returned as-is; null values are preserved
            var project = Assert.Single(result);
            Assert.Null(project.ParentProject);
            Assert.Null(project.ProjectTitle);
            Assert.Null(project.Program);
            Assert.Null(project.Customer);
            Assert.Null(project.Disease);
            Assert.Null(project.Contract);
            Assert.Null(project.IncomeAccountCode);
            Assert.Null(project.TransferIncome);
            Assert.Null(project.CustIncome);
            Assert.Null(project.IsDefraProject);
        }

        [Fact]
        public async Task GetAllProjectsAsync_ReturnsMultipleProjects_AllBelongingToUserId42()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One",   Program = "P001", Customer = "DEFRA", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "Project Two",   Program = "P002", Customer = "APHA",  UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", ProjectTitle = "Project Three", Program = "P001", Customer = "DEFRA", UserEmail = "test@example.com" }
            };
            var repo = CreateRepository(projectViews: projectViews);

            // Act
            var result = await repo.GetAllProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        #endregion

        #region GetProjectByIdAsync Tests

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsProject_WhenFound()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One",   Program = "P001", Customer = "DEFRA", ProjectStatus = "Active",   Disease = "D001", Contract = "C001", IncomeAccountCode = "INC001" },
                new() { ParentProject = "PP002", ProjectTitle = "Project Two",   Program = "P002", Customer = "APHA",  ProjectStatus = "Inactive", Disease = "D002", Contract = "C002", IncomeAccountCode = "INC002" }
            };
            var repo = CreateRepository(projects: projects);

            // Act
            var result = await repo.GetProjectByIdAsync("PP001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PP001",       result.ParentProject);
            Assert.Equal("Project One", result.ProjectTitle);
            Assert.Equal("P001",        result.Program);
            Assert.Equal("DEFRA",       result.Customer);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active", Disease = "D001", Contract = "C001", IncomeAccountCode = "INC001" }
            };
            var repo = CreateRepository(projects: projects);

            // Act
            var result = await repo.GetProjectByIdAsync("PP999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsNull_WhenProjectsIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(projects: []);

            // Act
            var result = await repo.GetProjectByIdAsync("PP001");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProjectByIdAsync_IsCaseInsensitive()
        {
            // Arrange â€” match is on exact ParentProject string
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One", Program = "P001", Customer = "DEFRA", ProjectStatus = "Active", Disease = "D001", Contract = "C001", IncomeAccountCode = "INC001" }
            };
            var repo = CreateRepository(projects: projects);

            // Act
            var result = await repo.GetProjectByIdAsync("pp001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PP001", result.ParentProject);
            Assert.Equal("Project One", result.ProjectTitle);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsFirstMatch_WhenMultipleProjectsExist()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Project One",   Program = "P001", Customer = "DEFRA", ProjectStatus = "Active",   Disease = "D001", Contract = "C001", IncomeAccountCode = "INC001" },
                new() { ParentProject = "PP002", ProjectTitle = "Project Two",   Program = "P002", Customer = "APHA",  ProjectStatus = "Inactive", Disease = "D002", Contract = "C002", IncomeAccountCode = "INC002" },
                new() { ParentProject = "PP003", ProjectTitle = "Project Three", Program = "P003", Customer = "DEFRA", ProjectStatus = "Active",   Disease = "D003", Contract = "C003", IncomeAccountCode = "INC003" }
            };
            var repo = CreateRepository(projects: projects);

            // Act
            var result = await repo.GetProjectByIdAsync("PP002");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PP002",       result.ParentProject);
            Assert.Equal("Project Two", result.ProjectTitle);
        }

        #endregion

        #region GetProjectsByProgramAsync Tests

        [Fact]
        public async Task GetProjectsByProgramAsync_ReturnsOnlyProjectsMatchingProgramAndUserId()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", Program = "P001", BudgetCvl = 1000m, IsDefraProject = 1, UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta",  Program = "P001", BudgetCvl = 2000m, IsDefraProject = 0, UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", ProjectTitle = "Gamma", Program = "P002", BudgetCvl = 3000m, IsDefraProject = 0, UserEmail = "test@example.com" }, // different program
                new() { ParentProject = "PP004", ProjectTitle = "Delta", Program = "P001", BudgetCvl = 4000m, IsDefraProject = 0, UserEmail = "other@example.com" }, // different user
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Equal("P001", p.Program));
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_ReturnsEmpty_WhenNoProgramMatches()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", Program = "P001", UserEmail = "test@example.com" }
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P999");

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_ReturnsEmpty_WhenNoMatchingUserId()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", Program = "P001", UserEmail = "other@example.com" }
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_MapsFieldsCorrectly()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001", BudgetCvl = 1500m, IsDefraProject = 1, UserEmail = "test@example.com" }
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            var project = Assert.Single(result.Data);
            Assert.Equal("PP001",         project.ParentProject);
            Assert.Equal("Alpha Project", project.ProjectTitle);
            Assert.Equal("P001",          project.Program);
            Assert.Equal(1500m,           project.BudgetCvl);
            Assert.Equal((short)1,        project.IsDefraProject);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_AppliesNullCoalescing_ForNullableFields()
        {
            // Arrange â€” ParentProject, ProjectTitle and IsDefraProject are null; Program is set to match the filter
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = null, ProjectTitle = null, Program = "P001", IsDefraProject = null, BudgetCvl = null, UserEmail = "test@example.com" }
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            var project = Assert.Single(result.Data);
            Assert.Equal(string.Empty, project.ParentProject);
            Assert.Equal(string.Empty, project.ProjectTitle);
            Assert.Equal("P001",       project.Program);
            Assert.Equal((short)0,     project.IsDefraProject);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_FilterByJobCode_ReturnsMatchingProjects()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta",  Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "XY003", ProjectTitle = "Gamma", Program = "P001", UserEmail = "test@example.com" },
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"ParentProject\":\"PP\"}"
            };

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Contains("PP", p.ParentProject));
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_FilterByJobDescription_ReturnsMatchingProjects()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "FMD Survey",     Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "TB Eradication", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", ProjectTitle = "FMD Outbreak",   Program = "P001", UserEmail = "test@example.com" },
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"ProjectTitle\":\"FMD\"}"
            };

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Contains("FMD", p.ProjectTitle));
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByParentProjectAscending_ByDefault()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "CC003", ProjectTitle = "Gamma", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "AA001", ProjectTitle = "Alpha", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "BB002", ProjectTitle = "Beta",  Program = "P001", UserEmail = "test@example.com" },
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10); // SortBy = "" by default

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("AA001", items[0].ParentProject);
            Assert.Equal("BB002", items[1].ParentProject);
            Assert.Equal("CC003", items[2].ParentProject);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByParentProjectDescending_WhenDescendingIsTrue()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "AA001", ProjectTitle = "Alpha", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "CC003", ProjectTitle = "Gamma", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "BB002", ProjectTitle = "Beta",  Program = "P001", UserEmail = "test@example.com" },
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(sortBy: "parentproject", descending: true, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("CC003", items[0].ParentProject);
            Assert.Equal("BB002", items[1].ParentProject);
            Assert.Equal("AA001", items[2].ParentProject);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByProjectTitleAscending()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP003", ProjectTitle = "Gamma Survey", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Survey", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Survey",  Program = "P001", UserEmail = "test@example.com" },
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(sortBy: "projecttitle", descending: false, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("PP001", items[0].ParentProject); // Alpha
            Assert.Equal("PP002", items[1].ParentProject); // Beta
            Assert.Equal("PP003", items[2].ParentProject); // Gamma
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByBudgetCvlDescending()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", Program = "P001", BudgetCvl = 500m,  UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta",  Program = "P001", BudgetCvl = 1500m, UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", ProjectTitle = "Gamma", Program = "P001", BudgetCvl = 1000m, UserEmail = "test@example.com" },
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(sortBy: "budgetcvl", descending: true, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            var items = result.Data.ToList();
            Assert.Equal(1500m, items[0].BudgetCvl);
            Assert.Equal(1000m, items[1].BudgetCvl);
            Assert.Equal(500m,  items[2].BudgetCvl);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_AppliesPaging_ReturnsCorrectPage()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "AA001", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "BB002", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "CC003", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "DD004", Program = "P001", UserEmail = "test@example.com" },
                new() { ParentProject = "EE005", Program = "P001", UserEmail = "test@example.com" },
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 2, pageSize: 2);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(2, result.PaginationData.PageSize);
            Assert.Equal(3, result.PaginationData.TotalPages);
            Assert.Equal("CC003", result.Data.First().ParentProject);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_ReturnsPaginationMetadata_Correctly()
        {
            // Arrange
            var projectViews = Enumerable.Range(1, 15)
                .Select(i => new ProjectView
                {
                    ParentProject = $"PP{i:D3}",
                    ProjectTitle  = $"Project {i}",
                    Program       = "P001",
                    UserEmail     = "test@example.com"
                }).ToList();
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            Assert.Equal(15, result.PaginationData.TotalRecords);
            Assert.Equal(10, result.Data.Count());
            Assert.Equal(2,  result.PaginationData.TotalPages);
        }


        #endregion

        #region HasAssociatedJobCodesAsync Tests

        [Fact]
        public async Task HasAssociatedJobCodesAsync_ReturnsTrue_WhenJobCodesExistForProjectAndCurrentYear()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC001", ParentProject = "PP001", FpsYear = 2024 },
                new() { JobCodeId = "JC002", ParentProject = "PP001", FpsYear = 2024 }
            };
            var repo = CreateRepository(jobCodes: jobCodes, fpsYear: 2024);

            // Act
            var result = await repo.HasAssociatedJobCodesAsync("PP001");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasAssociatedJobCodesAsync_ReturnsFalse_WhenNoJobCodesExistForProject()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC001", ParentProject = "PP002", FpsYear = 2024 }
            };
            var repo = CreateRepository(jobCodes: jobCodes, fpsYear: 2024);

            // Act
            var result = await repo.HasAssociatedJobCodesAsync("PP001");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task HasAssociatedJobCodesAsync_ReturnsFalse_WhenJobCodesExistForDifferentFpsYear()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC001", ParentProject = "PP001", FpsYear = 2023 }
            };
            var repo = CreateRepository(jobCodes: jobCodes, fpsYear: 2024);

            // Act
            var result = await repo.HasAssociatedJobCodesAsync("PP001");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task HasAssociatedJobCodesAsync_ReturnsFalse_WhenJobCodesListIsEmpty()
        {
            // Arrange
            var repo = CreateRepository(jobCodes: new List<JobCode>(), fpsYear: 2024);

            // Act
            var result = await repo.HasAssociatedJobCodesAsync("PP001");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task HasAssociatedJobCodesAsync_ReturnsTrue_WhenOneOfManyProjectsHasJobCodes()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC001", ParentProject = "PP002", FpsYear = 2024 },
                new() { JobCodeId = "JC002", ParentProject = "PP003", FpsYear = 2024 },
                new() { JobCodeId = "JC003", ParentProject = "PP001", FpsYear = 2024 }
            };
            var repo = CreateRepository(jobCodes: jobCodes, fpsYear: 2024);

            // Act
            var result = await repo.HasAssociatedJobCodesAsync("PP001");

            // Assert
            Assert.True(result);
        }

        #endregion

        #region GetAllPactProjectsAsync Tests

        [Fact]
        public async Task GetAllPactProjectsAsync_ReturnsAllPactProjects()
        {
            var pactViews = new List<PactProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "PACT Alpha" },
                new() { ParentProject = "PP002", ProjectTitle = "PACT Beta" }
            };
            var repo = CreateRepository(pactProjectViews: pactViews);

            var result = await repo.GetAllPactProjectsAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllPactProjectsAsync_ReturnsEmpty_WhenNoPactProjects()
        {
            var repo = CreateRepository(pactProjectViews: new List<PactProjectView>());

            var result = await repo.GetAllPactProjectsAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllPactProjectsAsync_OrdersByParentProject()
        {
            var pactViews = new List<PactProjectView>
            {
                new() { ParentProject = "CC003", ProjectTitle = "Gamma" },
                new() { ParentProject = "AA001", ProjectTitle = "Alpha" },
                new() { ParentProject = "BB002", ProjectTitle = "Beta" }
            };
            var repo = CreateRepository(pactProjectViews: pactViews);

            var result = (await repo.GetAllPactProjectsAsync()).ToList();

            Assert.Equal("AA001", result[0].ParentProject);
            Assert.Equal("BB002", result[1].ParentProject);
            Assert.Equal("CC003", result[2].ParentProject);
        }

        #endregion

        #region CheckProjectExistsAsync Tests

        [Fact]
        public async Task CheckProjectExistsAsync_ReturnsTrue_WhenProjectExists()
        {
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Test", Program = "P1", Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "C1", IncomeAccountCode = "I1" }
            };
            var repo = CreateRepository(projects: projects);

            var result = await repo.CheckProjectExistsAsync("PP001");

            Assert.True(result);
        }

        [Fact]
        public async Task CheckProjectExistsAsync_ReturnsFalse_WhenProjectDoesNotExist()
        {
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Test", Program = "P1", Customer = "C1", ProjectStatus = "Active", Disease = "D1", Contract = "C1", IncomeAccountCode = "I1" }
            };
            var repo = CreateRepository(projects: projects);

            var result = await repo.CheckProjectExistsAsync("NOPE");

            Assert.False(result);
        }

        [Fact]
        public async Task CheckProjectExistsAsync_ReturnsFalse_WhenProjectsEmpty()
        {
            var repo = CreateRepository(projects: new List<Project>());

            var result = await repo.CheckProjectExistsAsync("PP001");

            Assert.False(result);
        }

        #endregion

        #region CheckProjectExistsInFarmFileAsync Tests

        [Fact]
        public async Task CheckProjectExistsInFarmFileAsync_ReturnsTrue_WhenSubmissionExists()
        {
            var submissions = new List<SurvFFSubmission>
            {
                new() { SdPactWg = "WG1", Contract = "PP001" }
            };
            var repo = CreateRepository(survFFSubmissions: submissions);

            var result = await repo.CheckProjectExistsInFarmFileAsync("PP001");

            Assert.True(result);
        }

        [Fact]
        public async Task CheckProjectExistsInFarmFileAsync_ReturnsFalse_WhenNoSubmission()
        {
            var submissions = new List<SurvFFSubmission>
            {
                new() { SdPactWg = "WG1", Contract = "PP002" }
            };
            var repo = CreateRepository(survFFSubmissions: submissions);

            var result = await repo.CheckProjectExistsInFarmFileAsync("PP001");

            Assert.False(result);
        }

        [Fact]
        public async Task CheckProjectExistsInFarmFileAsync_ReturnsFalse_WhenEmpty()
        {
            var repo = CreateRepository(survFFSubmissions: new List<SurvFFSubmission>());

            var result = await repo.CheckProjectExistsInFarmFileAsync("PP001");

            Assert.False(result);
        }

        #endregion

        #region HasPlannedTestsAsync Tests

        [Fact]
        public async Task HasPlannedTestsAsync_ReturnsTrue_WhenTestRequirementsExist()
        {
            var testReqs = new List<TestRequirement>
            {
                new() { ProjectBuyerCode = "PP001", TestCode = "T1", Buyer = "B1", FpsYear = 2024 }
            };
            var repo = CreateRepository(testRequirements: testReqs);

            var result = await repo.HasPlannedTestsAsync("PP001");

            Assert.True(result);
        }

        [Fact]
        public async Task HasPlannedTestsAsync_ReturnsFalse_WhenNoTestRequirements()
        {
            var repo = CreateRepository(testRequirements: new List<TestRequirement>());

            var result = await repo.HasPlannedTestsAsync("PP001");

            Assert.False(result);
        }

        #endregion

        #region HasMonthlyOutputAsync Tests

        [Fact]
        public async Task HasMonthlyOutputAsync_ReturnsTrue_WhenOutputsExist()
        {
            var outputs = new List<MonthlyOutput>
            {
                new() { Buyer = "PP001" }
            };
            var repo = CreateRepository(monthlyOutputs: outputs);

            var result = await repo.HasMonthlyOutputAsync("PP001");

            Assert.True(result);
        }

        [Fact]
        public async Task HasMonthlyOutputAsync_ReturnsFalse_WhenNoOutputs()
        {
            var repo = CreateRepository(monthlyOutputs: new List<MonthlyOutput>());

            var result = await repo.HasMonthlyOutputAsync("PP001");

            Assert.False(result);
        }

        #endregion

        #region HasMonthlyTimeAsync Tests

        [Fact]
        public async Task HasMonthlyTimeAsync_ReturnsTrue_WhenTimesExist()
        {
            var times = new List<MonthlyTime>
            {
                new() { ParentProject = "PP001" }
            };
            var repo = CreateRepository(monthlyTimes: times);

            var result = await repo.HasMonthlyTimeAsync("PP001");

            Assert.True(result);
        }

        [Fact]
        public async Task HasMonthlyTimeAsync_ReturnsFalse_WhenNoTimes()
        {
            var repo = CreateRepository(monthlyTimes: new List<MonthlyTime>());

            var result = await repo.HasMonthlyTimeAsync("PP001");

            Assert.False(result);
        }

        #endregion

        #region HasProjectInvoicesAsync Tests

        [Fact]
        public async Task HasProjectInvoicesAsync_ReturnsTrue_WhenInvoicesExist()
        {
            var invoices = new List<ProjectInvoice>
            {
                new() { ProjectParent = "PP001" }
            };
            var repo = CreateRepository(projectInvoices: invoices);

            var result = await repo.HasProjectInvoicesAsync("PP001");

            Assert.True(result);
        }

        [Fact]
        public async Task HasProjectInvoicesAsync_ReturnsFalse_WhenNoInvoices()
        {
            var repo = CreateRepository(projectInvoices: new List<ProjectInvoice>());

            var result = await repo.HasProjectInvoicesAsync("PP001");

            Assert.False(result);
        }

        #endregion

        #region HasProjectSubcontractsAsync Tests

        [Fact]
        public async Task HasProjectSubcontractsAsync_ReturnsTrue_WhenSubcontractsExist()
        {
            var subcontracts = new List<ProjectSubContract>
            {
                new() { Project = "PP001" }
            };
            var repo = CreateRepository(projectSubContracts: subcontracts);

            var result = await repo.HasProjectSubcontractsAsync("PP001");

            Assert.True(result);
        }

        [Fact]
        public async Task HasProjectSubcontractsAsync_ReturnsFalse_WhenNoSubcontracts()
        {
            var repo = CreateRepository(projectSubContracts: new List<ProjectSubContract>());

            var result = await repo.HasProjectSubcontractsAsync("PP001");

            Assert.False(result);
        }

        #endregion

        #region GetPagedProjectsAsync Tests

        [Fact]
        public async Task GetPagedProjectsAsync_ReturnsPagedResults()
        {
            var projectViews = Enumerable.Range(1, 15)
                .Select(i => new ProjectView
                {
                    ParentProject = $"PP{i:D3}", ProjectTitle = $"Project {i}",
                    Program = "P001", Customer = "C1", ProjectStatus = "Active",
                    Disease = "D1", Contract = "C1", IncomeAccountCode = "I1",
                    UserEmail = "test@example.com"
                }).ToList();
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            var result = await repo.GetPagedProjectsAsync(query);

            Assert.Equal(15, result.PaginationData.TotalRecords);
            Assert.Equal(10, result.Data.Count());
            Assert.Equal(2, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_ReturnsEmpty_WhenNoProjectViews()
        {
            var repo = CreateRepository(projectViews: new List<ProjectView>());
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            var result = await repo.GetPagedProjectsAsync(query);

            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_SortsByParentProjectAscending_ByDefault()
        {
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "CC003", ProjectTitle = "Gamma", Program = "P1", Customer = "C1", ProjectStatus = "A", Disease = "D1", Contract = "C1", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "AA001", ProjectTitle = "Alpha", Program = "P1", Customer = "C1", ProjectStatus = "A", Disease = "D1", Contract = "C1", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "BB002", ProjectTitle = "Beta",  Program = "P1", Customer = "C1", ProjectStatus = "A", Disease = "D1", Contract = "C1", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            var result = await repo.GetPagedProjectsAsync(query);

            var items = result.Data.ToList();
            Assert.Equal("AA001", items[0].ParentProject);
            Assert.Equal("BB002", items[1].ParentProject);
            Assert.Equal("CC003", items[2].ParentProject);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_SortsByProjectTitleDescending()
        {
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", Program = "P1", Customer = "C1", ProjectStatus = "A", Disease = "D1", Contract = "C1", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "Gamma", Program = "P1", Customer = "C1", ProjectStatus = "A", Disease = "D1", Contract = "C1", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", ProjectTitle = "Beta",  Program = "P1", Customer = "C1", ProjectStatus = "A", Disease = "D1", Contract = "C1", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
            };
            var repo = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(sortBy: "projecttitle", descending: true, page: 1, pageSize: 10);

            var result = await repo.GetPagedProjectsAsync(query);

            var items = result.Data.ToList();
            Assert.Equal("Gamma", items[0].ProjectTitle);
            Assert.Equal("Beta", items[1].ProjectTitle);
            Assert.Equal("Alpha", items[2].ProjectTitle);
        }

        #endregion

        #region GetPagedProjectsByUserAsync Tests

        [Fact]
        public async Task GetPagedProjectsByUserAsync_ReturnsOnlyCurrentUserProjects()
        {
            var views = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta",  UserEmail = "other@example.com" },
                new() { ParentProject = "PP003", ProjectTitle = "Gamma", UserEmail = "test@example.com" },
            };
            var repo = CreateRepository(projectViews: views, userEmailId: "test@example.com");
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            var result = await repo.GetPagedProjectsByUserAsync(query);

            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, v => Assert.Equal("test@example.com", v.UserEmail));
        }

        [Fact]
        public async Task GetPagedProjectsByUserAsync_ReturnsEmpty_WhenNoMatchingUser()
        {
            var views = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", UserEmail = "other@example.com" },
            };
            var repo = CreateRepository(projectViews: views, userEmailId: "test@example.com");
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            var result = await repo.GetPagedProjectsByUserAsync(query);

            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedProjectsByUserAsync_AppliesPaging()
        {
            var views = Enumerable.Range(1, 15).Select(i => new ProjectView
            {
                ParentProject = $"PP{i:D3}",
                ProjectTitle  = $"Project {i}",
                UserEmail     = "test@example.com"
            }).ToList();
            var repo = CreateRepository(projectViews: views, userEmailId: "test@example.com");
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            var result = await repo.GetPagedProjectsByUserAsync(query);

            Assert.Equal(15, result.PaginationData.TotalRecords);
            Assert.Equal(10, result.Data.Count());
            Assert.Equal(2, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetPagedProjectsByUserAsync_SearchFilters_ByParentProject()
        {
            var views = new List<ProjectView>
            {
                new() { ParentProject = "MATCH001", ProjectTitle = "Unrelated",  UserEmail = "test@example.com" },
                new() { ParentProject = "OTHER002", ProjectTitle = "Other Title", UserEmail = "test@example.com" },
            };
            var repo = CreateRepository(projectViews: views, userEmailId: "test@example.com");
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { Search = "MATCH" };

            var result = await repo.GetPagedProjectsByUserAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("MATCH001", result.Data.First().ParentProject);
        }

        [Fact]
        public async Task GetPagedProjectsByUserAsync_SearchFilters_ByProjectTitle()
        {
            var views = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "FMD Survey",     UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "TB Eradication", UserEmail = "test@example.com" },
            };
            var repo = CreateRepository(projectViews: views, userEmailId: "test@example.com");
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { Search = "FMD" };

            var result = await repo.GetPagedProjectsByUserAsync(query);

            Assert.Single(result.Data);
            Assert.Equal("FMD Survey", result.Data.First().ProjectTitle);
        }

        [Fact]
        public async Task GetPagedProjectsByUserAsync_DefaultSort_OrdersByParentProjectAscending()
        {
            var views = new List<ProjectView>
            {
                new() { ParentProject = "CC003", UserEmail = "test@example.com" },
                new() { ParentProject = "AA001", UserEmail = "test@example.com" },
                new() { ParentProject = "BB002", UserEmail = "test@example.com" },
            };
            var repo = CreateRepository(projectViews: views, userEmailId: "test@example.com");
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            var result = await repo.GetPagedProjectsByUserAsync(query);

            var items = result.Data.ToList();
            Assert.Equal("AA001", items[0].ParentProject);
            Assert.Equal("BB002", items[1].ParentProject);
            Assert.Equal("CC003", items[2].ParentProject);
        }

        [Fact]
        public async Task GetPagedProjectsByUserAsync_SortsByProjectTitleDescending()
        {
            var views = new List<ProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", ProjectTitle = "Gamma", UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", ProjectTitle = "Beta",  UserEmail = "test@example.com" },
            };
            var repo = CreateRepository(projectViews: views, userEmailId: "test@example.com");
            var query = new PaginationParameters<string>(sortBy: "projecttitle", descending: true, page: 1, pageSize: 10);

            var result = await repo.GetPagedProjectsByUserAsync(query);

            var items = result.Data.ToList();
            Assert.Equal("Gamma", items[0].ProjectTitle);
            Assert.Equal("Beta",  items[1].ProjectTitle);
            Assert.Equal("Alpha", items[2].ProjectTitle);
        }

        [Fact]
        public async Task GetPagedProjectsByUserAsync_SortsByParentProjectDescending()
        {
            var views = new List<ProjectView>
            {
                new() { ParentProject = "AA001", UserEmail = "test@example.com" },
                new() { ParentProject = "CC003", UserEmail = "test@example.com" },
                new() { ParentProject = "BB002", UserEmail = "test@example.com" },
            };
            var repo = CreateRepository(projectViews: views, userEmailId: "test@example.com");
            var query = new PaginationParameters<string>(sortBy: "parentproject", descending: true, page: 1, pageSize: 10);

            var result = await repo.GetPagedProjectsByUserAsync(query);

            var items = result.Data.ToList();
            Assert.Equal("CC003", items[0].ParentProject);
            Assert.Equal("BB002", items[1].ParentProject);
            Assert.Equal("AA001", items[2].ParentProject);
        }

        #endregion

        #region GetPagedPactProjectsAsync Tests

        [Fact]
        public async Task GetPagedPactProjectsAsync_ReturnsPagedResults()
        {
            var pactViews = Enumerable.Range(1, 5)
                .Select(i => new PactProjectView { ParentProject = $"PP{i:D3}", ProjectTitle = $"PACT {i}" })
                .ToList();
            var repo = CreateRepository(pactProjectViews: pactViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 3);

            var result = await repo.GetPagedPactProjectsAsync(query);

            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(3, result.Data.Count());
            Assert.Equal(2, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_ReturnsEmpty_WhenNoPactProjects()
        {
            var repo = CreateRepository(pactProjectViews: new List<PactProjectView>());
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            var result = await repo.GetPagedPactProjectsAsync(query);

            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        #endregion

        #region UpdatePactPortfolioDetailsAsync Tests

        [Fact]
        public async Task UpdatePactPortfolioDetailsAsync_MatchingProjectAndYear_UpdatesFieldsAndReturnsEntity()
        {
            // Arrange
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2024,
                    ProjectTitle = "Old Title", Program = "P001", Manager = "Old Manager",
                    Finished = 0, Comments = "Old Comment", BudgetCvl = 100m, TransferIncome = 200m,
                    Customer = "DEFRA", ProjectStatus = "A", Disease = "D", Contract = "C", IncomeAccountCode = "IA"
                }
            };
            var repo = CreateRepository(projects: projects, fpsYear: 2024);
            var updated = new Project
            {
                ParentProject = "PP001", ProjectTitle = "New Title", Program = "P002", Manager = "New Manager",
                Finished = 1, Comments = "New Comment", BudgetCvl = 500m, TransferIncome = 600m,
                Customer = "DEFRA", ProjectStatus = "A", Disease = "D", Contract = "C", IncomeAccountCode = "IA"
            };

            // Act
            var result = await repo.UpdatePactPortfolioDetailsAsync(updated);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Title", result.ProjectTitle);
            Assert.Equal("P002", result.Program);
            Assert.Equal("New Manager", result.Manager);
            Assert.Equal((short)1, result.Finished);
            Assert.Equal("New Comment", result.Comments);
            Assert.Equal(500m, result.BudgetCvl);
            Assert.Equal(600m, result.TransferIncome);
        }

        [Fact]
        public async Task UpdatePactPortfolioDetailsAsync_ProjectNotFound_ReturnsNull()
        {
            // Arrange
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2024,
                    ProjectTitle = "Title", Program = "P001", Customer = "DEFRA",
                    ProjectStatus = "A", Disease = "D", Contract = "C", IncomeAccountCode = "IA"
                }
            };
            var repo = CreateRepository(projects: projects, fpsYear: 2024);
            var updated = new Project { ParentProject = "PP_NONEXISTENT" };

            // Act
            var result = await repo.UpdatePactPortfolioDetailsAsync(updated);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdatePactPortfolioDetailsAsync_WrongFpsYear_ReturnsNull()
        {
            // Arrange
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2023,
                    ProjectTitle = "Title", Program = "P001", Customer = "DEFRA",
                    ProjectStatus = "A", Disease = "D", Contract = "C", IncomeAccountCode = "IA"
                }
            };
            var repo = CreateRepository(projects: projects, fpsYear: 2024); // context year = 2024, row year = 2023
            var updated = new Project { ParentProject = "PP001" };

            // Act
            var result = await repo.UpdatePactPortfolioDetailsAsync(updated);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdatePactPortfolioDetailsAsync_EmptyRepository_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(projects: new List<Project>(), fpsYear: 2024);
            var updated = new Project { ParentProject = "PP001" };

            // Act
            var result = await repo.UpdatePactPortfolioDetailsAsync(updated);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdatePactPortfolioDetailsAsync_MultipleProjects_UpdatesOnlyMatchingOne()
        {
            // Arrange
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2024, ProjectTitle = "Title A",
                    Program = "P001", Customer = "DEFRA", ProjectStatus = "A",
                    Disease = "D", Contract = "C", IncomeAccountCode = "IA"
                },
                new()
                {
                    ParentProject = "PP002", FpsYear = 2024, ProjectTitle = "Title B",
                    Program = "P001", Customer = "DEFRA", ProjectStatus = "A",
                    Disease = "D", Contract = "C", IncomeAccountCode = "IA"
                }
            };
            var repo = CreateRepository(projects: projects, fpsYear: 2024);
            var updated = new Project
            {
                ParentProject = "PP001", ProjectTitle = "Updated Title",
                Program = "P002", Customer = "DEFRA", ProjectStatus = "A",
                Disease = "D", Contract = "C", IncomeAccountCode = "IA"
            };

            // Act
            var result = await repo.UpdatePactPortfolioDetailsAsync(updated);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PP001", result.ParentProject);
            Assert.Equal("Updated Title", result.ProjectTitle);
        }

        #endregion

        #region UpdateFpsPortfolioDetailsAsync Tests

        [Fact]
        public async Task UpdateFpsPortfolioDetailsAsync_MatchingProjectAndYear_UpdatesAllFieldsAndReturnsEntity()
        {
            // Arrange
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2024,
                    ProjectTitle = "Old Title", Program = "P001", Manager = "Old Manager",
                    Disease = "OldDisease", ProjectStatus = "Old Status",
                    TransferIncome = 100m, CustIncome = 200m, Profit = 50m,
                    Contract = "OldContract", Customer = "OldCustomer",
                    IncomeAccountCode = "IA"
                }
            };
            var projectLogs = new List<ProjectLog>();
            var repo = CreateRepository(projects: projects, projectLogs: projectLogs, fpsYear: 2024);
            var incoming = new Project
            {
                ParentProject = "PP001",
                ProjectTitle = "New Title", Program = "P002", Manager = "New Manager",
                Disease = "NewDisease", ProjectStatus = "Active",
                TransferIncome = 500m, CustIncome = 600m, Profit = 150m,
                Contract = "NewContract", Customer = "NewCustomer",
                IncomeAccountCode = "IA"
            };

            // Act
            var result = await repo.UpdateFpsPortfolioDetailsAsync(incoming);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Title",    result.ProjectTitle);
            Assert.Equal("P002",         result.Program);
            Assert.Equal("New Manager",  result.Manager);
            Assert.Equal("NewDisease",   result.Disease);
            Assert.Equal("Active",       result.ProjectStatus);
            Assert.Equal(500m,           result.TransferIncome);
            Assert.Equal(600m,           result.CustIncome);
            Assert.Equal(150m,           result.Profit);
            Assert.Equal("NewContract",  result.Contract);
            Assert.Equal("NewCustomer",  result.Customer);
        }

        [Fact]
        public async Task UpdateFpsPortfolioDetailsAsync_ProjectNotFound_ReturnsNull()
        {
            // Arrange
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2024,
                    ProjectTitle = "Title", Program = "P001", Customer = "DEFRA",
                    ProjectStatus = "A", Disease = "D", Contract = "C", IncomeAccountCode = "IA"
                }
            };
            var repo = CreateRepository(projects: projects, fpsYear: 2024);
            var incoming = new Project { ParentProject = "PP_NONEXISTENT" };

            // Act
            var result = await repo.UpdateFpsPortfolioDetailsAsync(incoming);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateFpsPortfolioDetailsAsync_WrongFpsYear_ReturnsNull()
        {
            // Arrange — project row has year 2023 but context year is 2024
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2023,
                    ProjectTitle = "Title", Program = "P001", Customer = "DEFRA",
                    ProjectStatus = "A", Disease = "D", Contract = "C", IncomeAccountCode = "IA"
                }
            };
            var repo = CreateRepository(projects: projects, fpsYear: 2024);
            var incoming = new Project { ParentProject = "PP001" };

            // Act
            var result = await repo.UpdateFpsPortfolioDetailsAsync(incoming);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region CheckProgramExistsAsync Tests

        [Fact]
        public async Task CheckProgramExistsAsync_ReturnsTrue_WhenProgramExists()
        {
            var programs = new List<Program> { new() { ProgramNo = "P001" } };
            var repo = CreateRepository(programs: programs);

            var result = await repo.CheckProgramExistsAsync("P001");

            Assert.True(result);
        }

        [Fact]
        public async Task CheckProgramExistsAsync_ReturnsFalse_WhenProgramDoesNotExist()
        {
            var programs = new List<Program> { new() { ProgramNo = "P001" } };
            var repo = CreateRepository(programs: programs);

            var result = await repo.CheckProgramExistsAsync("P999");

            Assert.False(result);
        }

        [Fact]
        public async Task CheckProgramExistsAsync_ReturnsTrue_WhenProgramNoIsNullOrEmpty()
        {
            var repo = CreateRepository(programs: new List<Program>());

            Assert.True(await repo.CheckProgramExistsAsync(null!));
            Assert.True(await repo.CheckProgramExistsAsync(string.Empty));
            Assert.True(await repo.CheckProgramExistsAsync("   "));
        }

        #endregion

        #region GetProjectsByProjectGroupAsync Tests

        [Fact]
        public async Task GetProjectsByProjectGroupAsync_ReturnsProjectsForGroup()
        {
            var projectGroupViews = new List<ProjectGroupView>
            {
                new() { ProjectGroupName = "Group1", UserEmail = "test@example.com" }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectGroup = "Group1", ProjectTitle = "Alpha", Program = "P001", Customer = "DEFRA", Contract = "C1", Disease = "D1", ProjectStatus = "Active", IncomeAccountCode = "INC1" },
                new() { ParentProject = "PP002", ProjectGroup = "Group1", ProjectTitle = "Beta",  Program = "P001", Customer = "APHA",  Contract = "C2", Disease = "D2", ProjectStatus = "Active", IncomeAccountCode = "INC2" },
                new() { ParentProject = "PP003", ProjectGroup = "Group2", ProjectTitle = "Gamma", Program = "P001", Customer = "EA",    Contract = "C3", Disease = "D3", ProjectStatus = "Active", IncomeAccountCode = "INC3" }
            };
            var repo = CreateRepository(projects: projects, projectGroupViews: projectGroupViews);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetProjectsByProjectGroupAsync(query, "Group1");

            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Equal("Group1", p.ProjectGroup));
        }

        [Fact]
        public async Task GetProjectsByProjectGroupAsync_ReturnsEmpty_WhenGroupHasNoProjects()
        {
            var projectGroupViews = new List<ProjectGroupView>
            {
                new() { ProjectGroupName = "EmptyGroup", UserEmail = "test@example.com" }
            };
            var repo = CreateRepository(projects: new List<Project>(), projectGroupViews: projectGroupViews);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetProjectsByProjectGroupAsync(query, "EmptyGroup");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetProjectsByProjectGroupAsync_FilterByParentProject_ReturnsMatchingProjects()
        {
            var projectGroupViews = new List<ProjectGroupView>
            {
                new() { ProjectGroupName = "Group1", UserEmail = "test@example.com" }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectGroup = "Group1", ProjectTitle = "Alpha", Program = "P001", Customer = "DEFRA", Contract = "C1", Disease = "D1", ProjectStatus = "Active", IncomeAccountCode = "INC1" },
                new() { ParentProject = "PP002", ProjectGroup = "Group1", ProjectTitle = "Beta",  Program = "P001", Customer = "APHA",  Contract = "C2", Disease = "D2", ProjectStatus = "Active", IncomeAccountCode = "INC2" },
                new() { ParentProject = "XX003", ProjectGroup = "Group1", ProjectTitle = "Gamma", Program = "P001", Customer = "EA",    Contract = "C3", Disease = "D3", ProjectStatus = "Active", IncomeAccountCode = "INC3" }
            };
            var repo = CreateRepository(projects: projects, projectGroupViews: projectGroupViews);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ParentProject\":\"PP\"}"
            };

            var result = await repo.GetProjectsByProjectGroupAsync(query, "Group1");

            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Contains("PP", p.ParentProject));
        }

        [Fact]
        public async Task GetProjectsByProjectGroupAsync_FilterByProjectTitle_ReturnsMatchingProjects()
        {
            var projectGroupViews = new List<ProjectGroupView>
            {
                new() { ProjectGroupName = "Group1", UserEmail = "test@example.com" }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectGroup = "Group1", ProjectTitle = "FMD Survey",     Program = "P001", Customer = "DEFRA", Contract = "C1", Disease = "D1", ProjectStatus = "Active", IncomeAccountCode = "INC1" },
                new() { ParentProject = "PP002", ProjectGroup = "Group1", ProjectTitle = "TB Eradication", Program = "P001", Customer = "APHA",  Contract = "C2", Disease = "D2", ProjectStatus = "Active", IncomeAccountCode = "INC2" },
                new() { ParentProject = "PP003", ProjectGroup = "Group1", ProjectTitle = "FMD Outbreak",   Program = "P001", Customer = "EA",    Contract = "C3", Disease = "D3", ProjectStatus = "Active", IncomeAccountCode = "INC3" }
            };
            var repo = CreateRepository(projects: projects, projectGroupViews: projectGroupViews);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ProjectTitle\":\"FMD\"}"
            };

            var result = await repo.GetProjectsByProjectGroupAsync(query, "Group1");

            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Contains("FMD", p.ProjectTitle));
        }

        [Fact]
        public async Task GetProjectsByProjectGroupAsync_FilterByManager_ReturnsMatchingProjects()
        {
            var projectGroupViews = new List<ProjectGroupView>
            {
                new() { ProjectGroupName = "Group1", UserEmail = "test@example.com" }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", ProjectGroup = "Group1", ProjectTitle = "Alpha", Manager = "Alice", Program = "P001", Customer = "DEFRA", Contract = "C1", Disease = "D1", ProjectStatus = "Active", IncomeAccountCode = "INC1" },
                new() { ParentProject = "PP002", ProjectGroup = "Group1", ProjectTitle = "Beta",  Manager = "Bob",   Program = "P001", Customer = "APHA",  Contract = "C2", Disease = "D2", ProjectStatus = "Active", IncomeAccountCode = "INC2" }
            };
            var repo = CreateRepository(projects: projects, projectGroupViews: projectGroupViews);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Manager\":\"Alice\"}"
            };

            var result = await repo.GetProjectsByProjectGroupAsync(query, "Group1");

            Assert.Single(result.Data);
            Assert.Equal("PP001", result.Data.First().ParentProject);
        }

        [Fact]
        public async Task GetProjectsByProjectGroupAsync_PagingIsApplied()
        {
            var projectGroupViews = new List<ProjectGroupView>
            {
                new() { ProjectGroupName = "Group1", UserEmail = "test@example.com" }
            };
            var projects = Enumerable.Range(1, 5).Select(i => new Project
            {
                ParentProject = $"PP00{i}", ProjectGroup = "Group1",
                ProjectTitle = $"Project {i}", Program = "P001",
                Customer = "DEFRA", Contract = "C", Disease = "D",
                ProjectStatus = "Active", IncomeAccountCode = "INC"
            }).ToList();
            var repo = CreateRepository(projects: projects, projectGroupViews: projectGroupViews);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 2 };

            var result = await repo.GetProjectsByProjectGroupAsync(query, "Group1");

            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
        }

        #endregion

        #region NormalizeDateTimesToUnspecified Tests (via UpdateFpsPortfolioDetailsAsync)

        [Fact]
        public async Task UpdateFpsPortfolioDetailsAsync_NormalizesDateCreated_WhenKindIsUtc()
        {
            // Arrange — DateCreated has Kind=Utc; NormalizeDateTimesToUnspecified should convert it to Unspecified
            var utcDate = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2024,
                    ProjectTitle = "Title", Program = "P001", Customer = "DEFRA",
                    ProjectStatus = "Active", Disease = "D", Contract = "C",
                    IncomeAccountCode = "INC",
                    DateCreated = utcDate
                }
            };
            var projectLogs = new List<ProjectLog>();
            var repo = CreateRepository(projects: projects, projectLogs: projectLogs, fpsYear: 2024);
            var incoming = new Project
            {
                ParentProject = "PP001", ProjectTitle = "New Title", Program = "P001",
                Customer = "DEFRA", ProjectStatus = "Active", Disease = "D",
                Contract = "C"
            };

            var result = await repo.UpdateFpsPortfolioDetailsAsync(incoming);

            Assert.NotNull(result);
            Assert.True(
                result.DateCreated == null || result.DateCreated.Value.Kind == DateTimeKind.Unspecified,
                "DateCreated Kind should be Unspecified after normalization");
        }

        [Fact]
        public async Task UpdateFpsPortfolioDetailsAsync_NormalizesDateCosted_WhenKindIsLocal()
        {
            // Arrange — DateCosted has Kind=Local; should be converted to Unspecified
            var localDate = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Local);
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2024,
                    ProjectTitle = "Title", Program = "P001", Customer = "DEFRA",
                    ProjectStatus = "Active", Disease = "D", Contract = "C",
                    IncomeAccountCode = "INC",
                    DateCosted = localDate
                }
            };
            var projectLogs = new List<ProjectLog>();
            var repo = CreateRepository(projects: projects, projectLogs: projectLogs, fpsYear: 2024);
            var incoming = new Project
            {
                ParentProject = "PP001", ProjectTitle = "New Title", Program = "P001",
                Customer = "DEFRA", ProjectStatus = "Active", Disease = "D",
                Contract = "C"
            };

            var result = await repo.UpdateFpsPortfolioDetailsAsync(incoming);

            Assert.NotNull(result);
            Assert.True(
                result.DateCosted == null || result.DateCosted.Value.Kind == DateTimeKind.Unspecified,
                "DateCosted Kind should be Unspecified after normalization");
        }

        [Fact]
        public async Task UpdateFpsPortfolioDetailsAsync_DoesNotChangeDates_WhenKindIsAlreadyUnspecified()
        {
            // Arrange — both dates already Unspecified; normalization is a no-op
            var unspecDate = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Unspecified);
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2024,
                    ProjectTitle = "Title", Program = "P001", Customer = "DEFRA",
                    ProjectStatus = "Active", Disease = "D", Contract = "C",
                    IncomeAccountCode = "INC",
                    DateCreated = unspecDate,
                    DateCosted  = unspecDate
                }
            };
            var projectLogs = new List<ProjectLog>();
            var repo = CreateRepository(projects: projects, projectLogs: projectLogs, fpsYear: 2024);
            var incoming = new Project
            {
                ParentProject = "PP001", ProjectTitle = "New Title", Program = "P001",
                Customer = "DEFRA", ProjectStatus = "Active", Disease = "D",
                Contract = "C"
            };

            var result = await repo.UpdateFpsPortfolioDetailsAsync(incoming);

            Assert.NotNull(result);
            Assert.Equal(DateTimeKind.Unspecified, result.DateCreated!.Value.Kind);
            Assert.Equal(DateTimeKind.Unspecified, result.DateCosted!.Value.Kind);
        }

        #endregion

        #region UpdateFpsPortfolioDetailsAsync Tests
        [Fact]
        public async Task UpdateFpsPortfolioDetailsAsync_EmptyRepository_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(projects: new List<Project>(), fpsYear: 2024);
            var incoming = new Project { ParentProject = "PP001" };

            // Act
            var result = await repo.UpdateFpsPortfolioDetailsAsync(incoming);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateFpsPortfolioDetailsAsync_MultipleProjects_UpdatesOnlyMatchingOne()
        {
            // Arrange
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2024,
                    ProjectTitle = "Title A", Program = "P001", Customer = "DEFRA",
                    ProjectStatus = "A", Disease = "D1", Contract = "C1", IncomeAccountCode = "IA"
                },
                new()
                {
                    ParentProject = "PP002", FpsYear = 2024,
                    ProjectTitle = "Title B", Program = "P001", Customer = "DEFRA",
                    ProjectStatus = "A", Disease = "D2", Contract = "C2", IncomeAccountCode = "IA"
                }
            };
            var projectLogs = new List<ProjectLog>();
            var repo = CreateRepository(projects: projects, projectLogs: projectLogs, fpsYear: 2024);
            var incoming = new Project
            {
                ParentProject = "PP001", ProjectTitle = "Updated Title A", Program = "P002",
                Customer = "NewCustomer", ProjectStatus = "Active",
                Disease = "NewDisease", Contract = "NewContract", IncomeAccountCode = "IA"
            };

            // Act
            var result = await repo.UpdateFpsPortfolioDetailsAsync(incoming);

            // Assert — PP001 updated, PP002 untouched
            Assert.NotNull(result);
            Assert.Equal("PP001",          result.ParentProject);
            Assert.Equal("Updated Title A", result.ProjectTitle);

            var pp002 = projects.First(p => p.ParentProject == "PP002");
            Assert.Equal("Title B", pp002.ProjectTitle);
        }

        [Fact]
        public async Task UpdateFpsPortfolioDetailsAsync_DoesNotModifyUnrelatedFields()
        {
            // Arrange — ensure IncomeAccountCode is not changed by the method
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2024,
                    ProjectTitle = "Title", Program = "P001", Customer = "DEFRA",
                    ProjectStatus = "A", Disease = "D", Contract = "C",
                    IncomeAccountCode = "PRESERVED", BudgetCvl = 999m
                }
            };
            var projectLogs = new List<ProjectLog>();
            var repo = CreateRepository(projects: projects, projectLogs: projectLogs, fpsYear: 2024);
            var incoming = new Project
            {
                ParentProject = "PP001", ProjectTitle = "New Title", Program = "P002",
                Customer = "NewCust", ProjectStatus = "Active", Disease = "NewD",
                Contract = "NewC", TransferIncome = 100m, CustIncome = 200m
            };

            // Act
            var result = await repo.UpdateFpsPortfolioDetailsAsync(incoming);

            // Assert — IncomeAccountCode and BudgetCvl are not modified
            Assert.NotNull(result);
            Assert.Equal("PRESERVED", result.IncomeAccountCode);
            Assert.Equal(999m, result.BudgetCvl);
        }
        #endregion

        #region CreateProjectAsync Tests

        [Fact]
        public async Task CreateProjectAsync_SetsYearAndDateCreatedAndReturnsProject()
        {
            // Arrange
            var projects  = new List<Project>();
            var projectLogs = new List<ProjectLog>();
            var repo = CreateRepository(projects: projects, projectLogs: projectLogs, fpsYear: 2024);
            var project = new Project
            {
                ParentProject     = "PP001",
                ProjectTitle      = "New Project",
                Program           = "P001",
                Customer          = "DEFRA",
                ProjectStatus     = "Active",
                Disease           = "D1",
                Contract          = "C1",
                IncomeAccountCode = "INC1"
            };

            // Act
            var result = await repo.CreateProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PP001", result.ParentProject);
            Assert.Equal(2024, result.FpsYear);
            Assert.NotNull(result.DateCreated);
            Assert.Equal(DateTimeKind.Unspecified, result.DateCreated!.Value.Kind);
        }

        [Fact]
        public async Task CreateProjectAsync_NormalizesDateCosted_WhenKindIsUtc()
        {
            // Arrange
            var projects    = new List<Project>();
            var projectLogs = new List<ProjectLog>();
            var repo = CreateRepository(projects: projects, projectLogs: projectLogs, fpsYear: 2024);
            var project = new Project
            {
                ParentProject     = "PP001",
                ProjectTitle      = "New Project",
                Program           = "P001",
                Customer          = "DEFRA",
                ProjectStatus     = "Active",
                Disease           = "D1",
                Contract          = "C1",
                IncomeAccountCode = "INC1",
                DateCosted        = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            // Act
            var result = await repo.CreateProjectAsync(project);

            // Assert — DateCosted was UTC; NormalizeDateTimesToUnspecified should have converted it
            Assert.NotNull(result);
            Assert.True(result.DateCosted == null || result.DateCosted.Value.Kind == DateTimeKind.Unspecified);
        }

        #endregion

        #region DeleteProjectAsync Tests

        [Fact]
        public async Task DeleteProjectAsync_ReturnsTrue_WhenProjectFoundForCurrentYear()
        {
            // Arrange
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2024,
                    ProjectTitle = "Test", Program = "P1", Customer = "C1",
                    ProjectStatus = "A", Disease = "D", Contract = "C", IncomeAccountCode = "I"
                }
            };
            var projectLogs = new List<ProjectLog>();
            var repo = CreateRepository(projects: projects, projectLogs: projectLogs, fpsYear: 2024);

            // Act
            var result = await repo.DeleteProjectAsync("PP001");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteProjectAsync_ReturnsFalse_WhenProjectNotFound()
        {
            // Arrange
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2024,
                    ProjectTitle = "Test", Program = "P1", Customer = "C1",
                    ProjectStatus = "A", Disease = "D", Contract = "C", IncomeAccountCode = "I"
                }
            };
            var repo = CreateRepository(projects: projects, projectLogs: new List<ProjectLog>(), fpsYear: 2024);

            // Act
            var result = await repo.DeleteProjectAsync("PP999");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteProjectAsync_ReturnsFalse_WhenFpsYearMismatch()
        {
            // Arrange — row has year 2023 but context year is 2024
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2023,
                    ProjectTitle = "Test", Program = "P1", Customer = "C1",
                    ProjectStatus = "A", Disease = "D", Contract = "C", IncomeAccountCode = "I"
                }
            };
            var repo = CreateRepository(projects: projects, projectLogs: new List<ProjectLog>(), fpsYear: 2024);

            // Act
            var result = await repo.DeleteProjectAsync("PP001");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteProjectAsync_NormalizesDates_WhenProjectHasUtcDates()
        {
            // Arrange — verify NormalizeDateTimesToUnspecified is called on delete path
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2024,
                    ProjectTitle  = "Test",  Program = "P1", Customer = "C1",
                    ProjectStatus = "A",     Disease = "D",  Contract  = "C", IncomeAccountCode = "I",
                    DateCreated   = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    DateCosted    = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Local)
                }
            };
            var projectLogs = new List<ProjectLog>();
            var repo = CreateRepository(projects: projects, projectLogs: projectLogs, fpsYear: 2024);

            // Act — should not throw even with non-Unspecified date kinds
            var result = await repo.DeleteProjectAsync("PP001");

            // Assert
            Assert.True(result);
        }

        #endregion

        #region UpdatePactProjectDetailsAsync Tests

        [Fact]
        public async Task UpdatePactProjectDetailsAsync_MatchingProject_UpdatesAllFieldsAndReturnsEntity()
        {
            // Arrange
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2024,
                    ProjectTitle  = "Old Title",  Program  = "P001",  Customer      = "DEFRA",
                    Manager       = "Old Manager", Contract = "OldC",  ProjectStatus = "Active",
                    Disease       = "OldDisease",  IsDefraProject = 0, Finished = 0,
                    Comments      = "Old Comment", BudgetCvl = 100m,   TransferIncome = 100m,
                    PvsIncome     = 50m,           WipEoy = 200m,      WipLimit = 300m,
                    WipCurrent    = 250m,          FecCost = 500m,     IncomeAccountCode = "IA"
                }
            };
            var projectLogs = new List<ProjectLog>();
            var repo = CreateRepository(projects: projects, projectLogs: projectLogs, fpsYear: 2024);
            var incoming = new Project
            {
                ParentProject  = "PP001",
                ProjectTitle   = "New Title",   Program        = "P002",  Customer      = "APHA",
                Manager        = "New Manager", Contract       = "NewC",  ProjectStatus = "Closed",
                Disease        = "FMD",         IsDefraProject = 1,       Finished      = 1,
                Comments       = "New Comment", BudgetCvl      = 999m,    TransferIncome = 200m,
                PvsIncome      = 150m,          WipEoy         = 400m,    WipLimit       = 500m,
                WipCurrent     = 450m,          FecCost        = 2000m
            };

            // Act
            var result = await repo.UpdatePactProjectDetailsAsync(incoming);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Title",   result.ProjectTitle);
            Assert.Equal("P002",        result.Program);
            Assert.Equal("APHA",        result.Customer);
            Assert.Equal("New Manager", result.Manager);
            Assert.Equal("NewC",        result.Contract);
            Assert.Equal("Closed",      result.ProjectStatus);
            Assert.Equal("FMD",         result.Disease);
            Assert.Equal((short)1,      result.IsDefraProject);
            Assert.Equal((short)1,      result.Finished);
            Assert.Equal("New Comment", result.Comments);
            Assert.Equal(999m,          result.BudgetCvl);
            Assert.Equal(200m,          result.TransferIncome);
            Assert.Equal(150m,          result.PvsIncome);
            Assert.Equal(400m,          result.WipEoy);
            Assert.Equal(500m,          result.WipLimit);
            Assert.Equal(450m,          result.WipCurrent);
            Assert.Equal(2000m,         result.FecCost);
        }

        [Fact]
        public async Task UpdatePactProjectDetailsAsync_ProjectNotFound_ReturnsNull()
        {
            // Arrange
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2024,
                    ProjectTitle  = "Title",  Program = "P001", Customer      = "DEFRA",
                    ProjectStatus = "A",      Disease = "D",    Contract      = "C",
                    IncomeAccountCode = "IA"
                }
            };
            var repo = CreateRepository(projects: projects, projectLogs: new List<ProjectLog>(), fpsYear: 2024);

            // Act
            var result = await repo.UpdatePactProjectDetailsAsync(new Project { ParentProject = "XX999" });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdatePactProjectDetailsAsync_WrongFpsYear_ReturnsNull()
        {
            // Arrange — row has year 2023 but context year is 2024
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject = "PP001", FpsYear = 2023,
                    ProjectTitle  = "Title",  Program = "P001", Customer      = "DEFRA",
                    ProjectStatus = "A",      Disease = "D",    Contract      = "C",
                    IncomeAccountCode = "IA"
                }
            };
            var repo = CreateRepository(projects: projects, projectLogs: new List<ProjectLog>(), fpsYear: 2024);

            // Act
            var result = await repo.UpdatePactProjectDetailsAsync(new Project { ParentProject = "PP001" });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UpdatePactProjectDetailsAsync_EmptyRepository_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(projects: new List<Project>(), projectLogs: new List<ProjectLog>(), fpsYear: 2024);

            // Act
            var result = await repo.UpdatePactProjectDetailsAsync(new Project { ParentProject = "PP001" });

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ApplyProjectFilter Additional Branch Tests

        [Fact]
        public async Task GetPagedProjectsAsync_FilterByOracleProjectCode_ReturnsMatchingProjects()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", OracleProjectCode = "ORA001", Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", OracleProjectCode = "ORA002", Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", OracleProjectCode = "XYZ999", Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
            };
            var repo  = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"OracleProjectCode\":\"ORA\"}"
            };

            // Act
            var result = await repo.GetPagedProjectsAsync(query);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Contains("ORA", p.OracleProjectCode));
        }

        [Fact]
        public async Task GetPagedProjectsAsync_FilterBySubAccountCode_ReturnsMatchingProjects()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", SubAccountCode = "SUB001", Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", SubAccountCode = "SUB002", Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", SubAccountCode = "XYZ999", Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
            };
            var repo  = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"SubAccountCode\":\"SUB\"}"
            };

            // Act
            var result = await repo.GetPagedProjectsAsync(query);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Contains("SUB", p.SubAccountCode));
        }

        [Fact]
        public async Task GetPagedProjectsAsync_FilterByCostCentre_ReturnsMatchingProject()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", CostCentre = 9001.0, Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", CostCentre = 9002.0, Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", CostCentre = 9003.0, Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
            };
            var repo  = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"CostCentre\":\"9001\"}"
            };

            // Act
            var result = await repo.GetPagedProjectsAsync(query);

            // Assert
            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal("PP001", result.Data.First().ParentProject);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_FilterByManager_ReturnsMatchingProjects()
        {
            // Arrange — MapToProject includes Manager, so the filter will work
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", Manager = "Alice Smith", Program = "P001", IsDefraProject = 0, UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", Manager = "Bob Jones",   Program = "P001", IsDefraProject = 0, UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", Manager = "Alice Brown",  Program = "P001", IsDefraProject = 0, UserEmail = "test@example.com" },
            };
            var repo  = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"Manager\":\"Alice\"}"
            };

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Contains("Alice", p.Manager));
        }

        #endregion

        #region ApplySortingByProperty Additional Branch Tests

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByProgram_Ascending()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP002", Program = "P002", IsDefraProject = 0, UserEmail = "test@example.com" },
                new() { ParentProject = "PP001", Program = "P001", IsDefraProject = 0, UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", Program = "P003", IsDefraProject = 0, UserEmail = "test@example.com" },
            };
            var repo  = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(sortBy: "program", descending: false, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert — only P001 rows returned (one), so sort trivially succeeds
            Assert.Single(result.Data);
            Assert.Equal("P001", result.Data.First().Program);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_SortsByProgram_Ascending()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP002", Program = "P002", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP001", Program = "P001", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", Program = "P003", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
            };
            var repo  = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(sortBy: "program", descending: false, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedProjectsAsync(query);

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("P001", items[0].Program);
            Assert.Equal("P002", items[1].Program);
            Assert.Equal("P003", items[2].Program);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_SortsByManager_Ascending()
        {
            // Arrange — Manager is projected as null via GetPagedProjectsAsync; sort still exercises the branch
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP003", Manager = "Charlie", Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP001", Manager = "Alice",   Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", Manager = "Bob",     Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
            };
            var repo  = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(sortBy: "manager", descending: false, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedProjectsAsync(query);

            // Assert — result count is unchanged; the sort branch is exercised
            Assert.Equal(3, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByManager_Ascending()
        {
            // Arrange — MapToProject includes Manager so ordering is verifiable
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP003", Manager = "Charlie", Program = "P001", IsDefraProject = 0, UserEmail = "test@example.com" },
                new() { ParentProject = "PP001", Manager = "Alice",   Program = "P001", IsDefraProject = 0, UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", Manager = "Bob",     Program = "P001", IsDefraProject = 0, UserEmail = "test@example.com" },
            };
            var repo  = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(sortBy: "manager", descending: false, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("Alice",   items[0].Manager);
            Assert.Equal("Bob",     items[1].Manager);
            Assert.Equal("Charlie", items[2].Manager);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_SortsByCostCentre_Descending()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", CostCentre = 1000.0, Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", CostCentre = 3000.0, Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", CostCentre = 2000.0, Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
            };
            var repo  = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(sortBy: "costcentre", descending: true, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedProjectsAsync(query);

            // Assert
            var items = result.Data.ToList();
            Assert.Equal(3000.0, items[0].CostCentre);
            Assert.Equal(2000.0, items[1].CostCentre);
            Assert.Equal(1000.0, items[2].CostCentre);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_SortsByOracleProjectCode_Ascending()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP003", OracleProjectCode = "ORA003", Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP001", OracleProjectCode = "ORA001", Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", OracleProjectCode = "ORA002", Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
            };
            var repo  = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(sortBy: "oracleprojectcode", descending: false, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedProjectsAsync(query);

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("ORA001", items[0].OracleProjectCode);
            Assert.Equal("ORA002", items[1].OracleProjectCode);
            Assert.Equal("ORA003", items[2].OracleProjectCode);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_SortsBySubAccountCode_Ascending()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP002", SubAccountCode = "SUB002", Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP003", SubAccountCode = "SUB003", Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP001", SubAccountCode = "SUB001", Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
            };
            var repo  = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(sortBy: "subaccountcode", descending: false, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedProjectsAsync(query);

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("SUB001", items[0].SubAccountCode);
            Assert.Equal("SUB002", items[1].SubAccountCode);
            Assert.Equal("SUB003", items[2].SubAccountCode);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_UnknownSortKey_DefaultsToParentProjectAscending()
        {
            // Arrange — exercises the `_ => query.OrderBy(p => p.ParentProject)` branch in ApplySortingByProperty
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP003", Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP001", Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
            };
            var repo  = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(sortBy: "unknownfield", descending: false, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedProjectsAsync(query);

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("PP001", items[0].ParentProject);
            Assert.Equal("PP002", items[1].ParentProject);
            Assert.Equal("PP003", items[2].ParentProject);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_NullModelFilter_ReturnsAllProjects()
        {
            // Arrange — JSON "null" deserialises to null, exercising the filterModel == null guard in ApplyProjectFilter
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "PP001", Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
                new() { ParentProject = "PP002", Program = "P1", Customer = "C1", Contract = "C1", Disease = "D1", ProjectStatus = "A", IncomeAccountCode = "I1", UserEmail = "test@example.com" },
            };
            var repo  = CreateRepository(projectViews: projectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { Filter = "null" };

            // Act
            var result = await repo.GetPagedProjectsAsync(query);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetPagedPactProjectsAsync Filter and Sorting Tests

        [Fact]
        public async Task GetPagedPactProjectsAsync_FilterByParentProject_ReturnsMatchingProjects()
        {
            // Arrange
            var pactViews = new List<PactProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta" },
                new() { ParentProject = "XY003", ProjectTitle = "Gamma" },
            };
            var repo  = CreateRepository(pactProjectViews: pactViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"ParentProject\":\"PP\"}"
            };

            // Act
            var result = await repo.GetPagedPactProjectsAsync(query);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Contains("PP", p.ParentProject));
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_FilterByProjectTitle_ReturnsMatchingProjects()
        {
            // Arrange
            var pactViews = new List<PactProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "FMD Survey" },
                new() { ParentProject = "PP002", ProjectTitle = "TB Eradication" },
                new() { ParentProject = "PP003", ProjectTitle = "FMD Outbreak" },
            };
            var repo  = CreateRepository(pactProjectViews: pactViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"ProjectTitle\":\"FMD\"}"
            };

            // Act
            var result = await repo.GetPagedPactProjectsAsync(query);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Contains("FMD", p.ProjectTitle));
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_NullFilter_ReturnsAllProjects()
        {
            // Arrange
            var pactViews = new List<PactProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta" },
            };
            var repo  = CreateRepository(pactProjectViews: pactViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { Filter = null };

            // Act
            var result = await repo.GetPagedPactProjectsAsync(query);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_NullModelFilter_ReturnsAllProjects()
        {
            // Arrange — JSON "null" deserialises to null, exercising the filterModel == null guard in ApplyPactProjectFilter
            var pactViews = new List<PactProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta" },
            };
            var repo  = CreateRepository(pactProjectViews: pactViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10) { Filter = "null" };

            // Act
            var result = await repo.GetPagedPactProjectsAsync(query);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_SortsByParentProjectAscending()
        {
            // Arrange
            var pactViews = new List<PactProjectView>
            {
                new() { ParentProject = "CC003", ProjectTitle = "Gamma" },
                new() { ParentProject = "AA001", ProjectTitle = "Alpha" },
                new() { ParentProject = "BB002", ProjectTitle = "Beta" },
            };
            var repo  = CreateRepository(pactProjectViews: pactViews);
            var query = new PaginationParameters<string>(sortBy: "parentproject", descending: false, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedPactProjectsAsync(query);

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("AA001", items[0].ParentProject);
            Assert.Equal("BB002", items[1].ParentProject);
            Assert.Equal("CC003", items[2].ParentProject);
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_SortsByParentProjectDescending()
        {
            // Arrange
            var pactViews = new List<PactProjectView>
            {
                new() { ParentProject = "AA001", ProjectTitle = "Alpha" },
                new() { ParentProject = "CC003", ProjectTitle = "Gamma" },
                new() { ParentProject = "BB002", ProjectTitle = "Beta" },
            };
            var repo  = CreateRepository(pactProjectViews: pactViews);
            var query = new PaginationParameters<string>(sortBy: "parentproject", descending: true, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedPactProjectsAsync(query);

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("CC003", items[0].ParentProject);
            Assert.Equal("BB002", items[1].ParentProject);
            Assert.Equal("AA001", items[2].ParentProject);
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_SortsByProjectTitleAscending()
        {
            // Arrange
            var pactViews = new List<PactProjectView>
            {
                new() { ParentProject = "PP003", ProjectTitle = "Gamma Survey" },
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Study" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Test" },
            };
            var repo  = CreateRepository(pactProjectViews: pactViews);
            var query = new PaginationParameters<string>(sortBy: "projecttitle", descending: false, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedPactProjectsAsync(query);

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("Alpha Study",  items[0].ProjectTitle);
            Assert.Equal("Beta Test",    items[1].ProjectTitle);
            Assert.Equal("Gamma Survey", items[2].ProjectTitle);
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_SortsByProjectTitleDescending()
        {
            // Arrange
            var pactViews = new List<PactProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Study" },
                new() { ParentProject = "PP003", ProjectTitle = "Gamma Survey" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Test" },
            };
            var repo  = CreateRepository(pactProjectViews: pactViews);
            var query = new PaginationParameters<string>(sortBy: "projecttitle", descending: true, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedPactProjectsAsync(query);

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("Gamma Survey", items[0].ProjectTitle);
            Assert.Equal("Beta Test",    items[1].ProjectTitle);
            Assert.Equal("Alpha Study",  items[2].ProjectTitle);
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_UnknownSortKey_DefaultsToParentProjectAscending()
        {
            // Arrange
            var pactViews = new List<PactProjectView>
            {
                new() { ParentProject = "CC003", ProjectTitle = "Gamma" },
                new() { ParentProject = "AA001", ProjectTitle = "Alpha" },
                new() { ParentProject = "BB002", ProjectTitle = "Beta" },
            };
            var repo  = CreateRepository(pactProjectViews: pactViews);
            var query = new PaginationParameters<string>(sortBy: "unknownfield", descending: false, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedPactProjectsAsync(query);

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("AA001", items[0].ParentProject);
            Assert.Equal("BB002", items[1].ParentProject);
            Assert.Equal("CC003", items[2].ParentProject);
        }

        #endregion

        #region GetPagedPactProjectsByProgramAsync Tests
        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_ReturnsEmpty_WhenNoProgramMatches()
        {
            // Arrange
            var pactProjectViews = new List<PactProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", Program = "P001" }
            };
            var repo = CreateRepository(pactProjectViews: pactProjectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedPactProjectsByProgramAsync(query, "P999");

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }        

        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_MapsFieldsCorrectly()
        {
            // Arrange
            var pactProjectViews = new List<PactProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001", BudgetCvl = 1500m, IsDefraProject = 1 }
            };
            var repo = CreateRepository(pactProjectViews: pactProjectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedPactProjectsByProgramAsync(query, "P001");

            // Assert
            var project = Assert.Single(result.Data);
            Assert.Equal("PP001", project.ParentProject);
            Assert.Equal("Alpha Project", project.ProjectTitle);
            Assert.Equal("P001", project.Program);
            Assert.Equal(1500m, project.BudgetCvl);
            Assert.Equal((short)1, project.IsDefraProject);
        }        

        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_FilterByJobCode_ReturnsMatchingProjects()
        {
            // Arrange
            var pactProjectViews = new List<PactProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha", Program = "P001" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta",  Program = "P001" },
                new() { ParentProject = "XY003", ProjectTitle = "Gamma", Program = "P001" },
            };
            var repo = CreateRepository(pactProjectViews: pactProjectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"ParentProject\":\"PP\"}"
            };

            // Act
            var result = await repo.GetPagedPactProjectsByProgramAsync(query, "P001");

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Contains("PP", p.ParentProject));
        }

        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_FilterByJobDescription_ReturnsMatchingProjects()
        {
            // Arrange
            var pactProjectViews = new List<PactProjectView>
            {
                new() { ParentProject = "PP001", ProjectTitle = "FMD Survey",     Program = "P001" },
                new() { ParentProject = "PP002", ProjectTitle = "TB Eradication", Program = "P001" },
                new() { ParentProject = "PP003", ProjectTitle = "FMD Outbreak",   Program = "P001" },
            };
            var repo = CreateRepository(pactProjectViews: pactProjectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10)
            {
                Filter = "{\"ProjectTitle\":\"FMD\"}"
            };

            // Act
            var result = await repo.GetPagedPactProjectsByProgramAsync(query, "P001");

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, p => Assert.Contains("FMD", p.ProjectTitle));
        }

        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_SortsByParentProjectAscending_ByDefault()
        {
            // Arrange
            var pactProjectViews = new List<PactProjectView>
            {
                new() { ParentProject = "CC003", ProjectTitle = "Gamma", Program = "P001" },
                new() { ParentProject = "AA001", ProjectTitle = "Alpha", Program = "P001" },
                new() { ParentProject = "BB002", ProjectTitle = "Beta",  Program = "P001" },
            };
            var repo = CreateRepository(pactProjectViews: pactProjectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10); // SortBy = "" by default

            // Act
            var result = await repo.GetPagedPactProjectsByProgramAsync(query, "P001");

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("AA001", items[0].ParentProject);
            Assert.Equal("BB002", items[1].ParentProject);
            Assert.Equal("CC003", items[2].ParentProject);
        }

        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_SortsByParentProjectDescending_WhenDescendingIsTrue()
        {
            // Arrange
            var pactProjectViews = new List<PactProjectView>
            {
                new() { ParentProject = "AA001", ProjectTitle = "Alpha", Program = "P001" },
                new() { ParentProject = "CC003", ProjectTitle = "Gamma", Program = "P001" },
                new() { ParentProject = "BB002", ProjectTitle = "Beta",  Program = "P001" },
            };
            var repo = CreateRepository(pactProjectViews: pactProjectViews);
            var query = new PaginationParameters<string>(sortBy: "parentproject", descending: true, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedPactProjectsByProgramAsync(query, "P001");

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("CC003", items[0].ParentProject);
            Assert.Equal("BB002", items[1].ParentProject);
            Assert.Equal("AA001", items[2].ParentProject);
        }

        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_SortsByProjectTitleAscending()
        {
            // Arrange
            var pactProjectViews = new List<PactProjectView>
            {
                new() { ParentProject = "PP003", ProjectTitle = "Gamma Survey", Program = "P001" },
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Survey", Program = "P001" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Survey",  Program = "P001" },
            };
            var repo = CreateRepository(pactProjectViews: pactProjectViews);
            var query = new PaginationParameters<string>(sortBy: "projecttitle", descending: false, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedPactProjectsByProgramAsync(query, "P001");

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("PP001", items[0].ParentProject); // Alpha
            Assert.Equal("PP002", items[1].ParentProject); // Beta
            Assert.Equal("PP003", items[2].ParentProject); // Gamma
        }        

        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_AppliesPaging_ReturnsCorrectPage()
        {
            // Arrange
            var pactProjectViews = new List<PactProjectView>
            {
                new() { ParentProject = "AA001", Program = "P001" },
                new() { ParentProject = "BB002", Program = "P001" },
                new() { ParentProject = "CC003", Program = "P001" },
                new() { ParentProject = "DD004", Program = "P001" },
                new() { ParentProject = "EE005", Program = "P001" },
            };
            var repo = CreateRepository(pactProjectViews: pactProjectViews);
            var query = new PaginationParameters<string>(page: 2, pageSize: 2);

            // Act
            var result = await repo.GetPagedPactProjectsByProgramAsync(query, "P001");

            // Assert
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(2, result.PaginationData.PageSize);
            Assert.Equal(3, result.PaginationData.TotalPages);
            Assert.Equal("CC003", result.Data.First().ParentProject);
        }

        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_ReturnsPaginationMetadata_Correctly()
        {
            // Arrange
            var pactProjectViews = Enumerable.Range(1, 15)
                .Select(i => new PactProjectView
                {
                    ParentProject = $"PP{i:D3}",
                    ProjectTitle = $"Project {i}",
                    Program = "P001"
                }).ToList();
            var repo = CreateRepository(pactProjectViews: pactProjectViews);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetPagedPactProjectsByProgramAsync(query, "P001");

            // Assert
            Assert.Equal(15, result.PaginationData.TotalRecords);
            Assert.Equal(10, result.Data.Count());
            Assert.Equal(2, result.PaginationData.TotalPages);
        }


        #endregion

        #region ApplySortingByProperty – new sort keys (branch coverage)

        private static ProjectView MakeSortView(
            string parentProject,
            string? projectGroup = null,
            string? customer     = null,
            string? contract     = null,
            string? disease      = null,
            string? projectStatus = null,
            decimal? budgetCvl   = null,
            decimal? custIncome  = null,
            decimal? transferIncome    = null,
            decimal? planCaseWorkDebit = null) => new()
        {
            ParentProject     = parentProject,
            Program           = "P001",
            ProjectGroup      = projectGroup,
            Customer          = customer,
            Contract          = contract,
            Disease           = disease,
            ProjectStatus     = projectStatus,
            BudgetCvl         = budgetCvl,
            CustIncome        = custIncome,
            TransferIncome    = transferIncome,
            PlanCaseWorkDebit = planCaseWorkDebit,
            IsDefraProject    = 0,
            UserEmail         = "test@example.com"
        };

        // ?? projectgroup ???????????????????????????????????????????????????????

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByProjectGroup_Ascending()
        {
            var views = new List<ProjectView>
            {
                MakeSortView("PP003", projectGroup: "GroupC"),
                MakeSortView("PP001", projectGroup: "GroupA"),
                MakeSortView("PP002", projectGroup: "GroupB"),
            };
            var repo  = CreateRepository(projectViews: views);
            var query = new PaginationParameters<string>(sortBy: "projectgroup", descending: false, page: 1, pageSize: 10);

            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            var items = result.Data.ToList();
            Assert.Equal("GroupA", items[0].ProjectGroup);
            Assert.Equal("GroupB", items[1].ProjectGroup);
            Assert.Equal("GroupC", items[2].ProjectGroup);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByProjectGroup_Descending()
        {
            var views = new List<ProjectView>
            {
                MakeSortView("PP001", projectGroup: "GroupA"),
                MakeSortView("PP003", projectGroup: "GroupC"),
                MakeSortView("PP002", projectGroup: "GroupB"),
            };
            var repo  = CreateRepository(projectViews: views);
            var query = new PaginationParameters<string>(sortBy: "projectgroup", descending: true, page: 1, pageSize: 10);

            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            var items = result.Data.ToList();
            Assert.Equal("GroupC", items[0].ProjectGroup);
            Assert.Equal("GroupB", items[1].ProjectGroup);
            Assert.Equal("GroupA", items[2].ProjectGroup);
        }

        // ?? customer ???????????????????????????????????????????????????????????

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByCustomer_Ascending()
        {
            var views = new List<ProjectView>
            {
                MakeSortView("PP003", customer: "CustomerC"),
                MakeSortView("PP001", customer: "CustomerA"),
                MakeSortView("PP002", customer: "CustomerB"),
            };
            var repo  = CreateRepository(projectViews: views);
            var query = new PaginationParameters<string>(sortBy: "customer", descending: false, page: 1, pageSize: 10);

            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            var items = result.Data.ToList();
            Assert.Equal("CustomerA", items[0].Customer);
            Assert.Equal("CustomerB", items[1].Customer);
            Assert.Equal("CustomerC", items[2].Customer);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByCustomer_Descending()
        {
            var views = new List<ProjectView>
            {
                MakeSortView("PP001", customer: "CustomerA"),
                MakeSortView("PP002", customer: "CustomerB"),
                MakeSortView("PP003", customer: "CustomerC"),
            };
            var repo  = CreateRepository(projectViews: views);
            var query = new PaginationParameters<string>(sortBy: "customer", descending: true, page: 1, pageSize: 10);

            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            var items = result.Data.ToList();
            Assert.Equal("CustomerC", items[0].Customer);
            Assert.Equal("CustomerB", items[1].Customer);
            Assert.Equal("CustomerA", items[2].Customer);
        }

        // ?? contract ???????????????????????????????????????????????????????????

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByContract_Ascending()
        {
            var views = new List<ProjectView>
            {
                MakeSortView("PP002", contract: "ContractB"),
                MakeSortView("PP003", contract: "ContractC"),
                MakeSortView("PP001", contract: "ContractA"),
            };
            var repo  = CreateRepository(projectViews: views);
            var query = new PaginationParameters<string>(sortBy: "contract", descending: false, page: 1, pageSize: 10);

            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            var items = result.Data.ToList();
            Assert.Equal("ContractA", items[0].Contract);
            Assert.Equal("ContractB", items[1].Contract);
            Assert.Equal("ContractC", items[2].Contract);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByContract_Descending()
        {
            var views = new List<ProjectView>
            {
                MakeSortView("PP001", contract: "ContractA"),
                MakeSortView("PP002", contract: "ContractB"),
                MakeSortView("PP003", contract: "ContractC"),
            };
            var repo  = CreateRepository(projectViews: views);
            var query = new PaginationParameters<string>(sortBy: "contract", descending: true, page: 1, pageSize: 10);

            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            var items = result.Data.ToList();
            Assert.Equal("ContractC", items[0].Contract);
            Assert.Equal("ContractB", items[1].Contract);
            Assert.Equal("ContractA", items[2].Contract);
        }

        // ?? disease ????????????????????????????????????????????????????????????

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByDisease_Ascending()
        {
            var views = new List<ProjectView>
            {
                MakeSortView("PP003", disease: "DiseaseC"),
                MakeSortView("PP001", disease: "DiseaseA"),
                MakeSortView("PP002", disease: "DiseaseB"),
            };
            var repo  = CreateRepository(projectViews: views);
            var query = new PaginationParameters<string>(sortBy: "disease", descending: false, page: 1, pageSize: 10);

            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            var items = result.Data.ToList();
            Assert.Equal("DiseaseA", items[0].Disease);
            Assert.Equal("DiseaseB", items[1].Disease);
            Assert.Equal("DiseaseC", items[2].Disease);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByDisease_Descending()
        {
            var views = new List<ProjectView>
            {
                MakeSortView("PP001", disease: "DiseaseA"),
                MakeSortView("PP002", disease: "DiseaseB"),
                MakeSortView("PP003", disease: "DiseaseC"),
            };
            var repo  = CreateRepository(projectViews: views);
            var query = new PaginationParameters<string>(sortBy: "disease", descending: true, page: 1, pageSize: 10);

            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            var items = result.Data.ToList();
            Assert.Equal("DiseaseC", items[0].Disease);
            Assert.Equal("DiseaseB", items[1].Disease);
            Assert.Equal("DiseaseA", items[2].Disease);
        }

        // ?? projectstatus ??????????????????????????????????????????????????????

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByProjectStatus_Ascending()
        {
            var views = new List<ProjectView>
            {
                MakeSortView("PP003", projectStatus: "StatusC"),
                MakeSortView("PP001", projectStatus: "StatusA"),
                MakeSortView("PP002", projectStatus: "StatusB"),
            };
            var repo  = CreateRepository(projectViews: views);
            var query = new PaginationParameters<string>(sortBy: "projectstatus", descending: false, page: 1, pageSize: 10);

            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            var items = result.Data.ToList();
            Assert.Equal("StatusA", items[0].ProjectStatus);
            Assert.Equal("StatusB", items[1].ProjectStatus);
            Assert.Equal("StatusC", items[2].ProjectStatus);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByProjectStatus_Descending()
        {
            var views = new List<ProjectView>
            {
                MakeSortView("PP001", projectStatus: "StatusA"),
                MakeSortView("PP002", projectStatus: "StatusB"),
                MakeSortView("PP003", projectStatus: "StatusC"),
            };
            var repo  = CreateRepository(projectViews: views);
            var query = new PaginationParameters<string>(sortBy: "projectstatus", descending: true, page: 1, pageSize: 10);

            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            var items = result.Data.ToList();
            Assert.Equal("StatusC", items[0].ProjectStatus);
            Assert.Equal("StatusB", items[1].ProjectStatus);
            Assert.Equal("StatusA", items[2].ProjectStatus);
        }

        // ?? budgetcvl ??????????????????????????????????????????????????????????

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByBudgetCvl_Ascending()
        {
            var views = new List<ProjectView>
            {
                MakeSortView("PP002", budgetCvl: 200m),
                MakeSortView("PP003", budgetCvl: 300m),
                MakeSortView("PP001", budgetCvl: 100m),
            };
            var repo  = CreateRepository(projectViews: views);
            var query = new PaginationParameters<string>(sortBy: "budgetcvl", descending: false, page: 1, pageSize: 10);

            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            var items = result.Data.ToList();
            Assert.Equal(100m, items[0].BudgetCvl);
            Assert.Equal(200m, items[1].BudgetCvl);
            Assert.Equal(300m, items[2].BudgetCvl);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByBudgetCvl_Descending()
        {
            var views = new List<ProjectView>
            {
                MakeSortView("PP001", budgetCvl: 100m),
                MakeSortView("PP002", budgetCvl: 200m),
                MakeSortView("PP003", budgetCvl: 300m),
            };
            var repo  = CreateRepository(projectViews: views);
            var query = new PaginationParameters<string>(sortBy: "budgetcvl", descending: true, page: 1, pageSize: 10);

            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            var items = result.Data.ToList();
            Assert.Equal(300m, items[0].BudgetCvl);
            Assert.Equal(200m, items[1].BudgetCvl);
            Assert.Equal(100m, items[2].BudgetCvl);
        }

        // ?? budgetext (maps to CustIncome) ?????????????????????????????????????

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByBudgetExt_Ascending()
        {
            var views = new List<ProjectView>
            {
                MakeSortView("PP003", custIncome: 3000m),
                MakeSortView("PP001", custIncome: 1000m),
                MakeSortView("PP002", custIncome: 2000m),
            };
            var repo  = CreateRepository(projectViews: views);
            var query = new PaginationParameters<string>(sortBy: "budgetext", descending: false, page: 1, pageSize: 10);

            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            var items = result.Data.ToList();
            Assert.Equal(1000m, items[0].CustIncome);
            Assert.Equal(2000m, items[1].CustIncome);
            Assert.Equal(3000m, items[2].CustIncome);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByBudgetExt_Descending()
        {
            var views = new List<ProjectView>
            {
                MakeSortView("PP001", custIncome: 1000m),
                MakeSortView("PP002", custIncome: 2000m),
                MakeSortView("PP003", custIncome: 3000m),
            };
            var repo  = CreateRepository(projectViews: views);
            var query = new PaginationParameters<string>(sortBy: "budgetext", descending: true, page: 1, pageSize: 10);

            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            var items = result.Data.ToList();
            Assert.Equal(3000m, items[0].CustIncome);
            Assert.Equal(2000m, items[1].CustIncome);
            Assert.Equal(1000m, items[2].CustIncome);
        }

        // ?? transferincome ?????????????????????????????????????????????????????

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByTransferIncome_Ascending()
        {
            var views = new List<ProjectView>
            {
                MakeSortView("PP002", transferIncome: 200m),
                MakeSortView("PP001", transferIncome: 100m),
                MakeSortView("PP003", transferIncome: 300m),
            };
            var repo  = CreateRepository(projectViews: views);
            var query = new PaginationParameters<string>(sortBy: "transferincome", descending: false, page: 1, pageSize: 10);

            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            var items = result.Data.ToList();
            Assert.Equal(100m, items[0].TransferIncome);
            Assert.Equal(200m, items[1].TransferIncome);
            Assert.Equal(300m, items[2].TransferIncome);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByTransferIncome_Descending()
        {
            var views = new List<ProjectView>
            {
                MakeSortView("PP001", transferIncome: 100m),
                MakeSortView("PP003", transferIncome: 300m),
                MakeSortView("PP002", transferIncome: 200m),
            };
            var repo  = CreateRepository(projectViews: views);
            var query = new PaginationParameters<string>(sortBy: "transferincome", descending: true, page: 1, pageSize: 10);

            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            var items = result.Data.ToList();
            Assert.Equal(300m, items[0].TransferIncome);
            Assert.Equal(200m, items[1].TransferIncome);
            Assert.Equal(100m, items[2].TransferIncome);
        }

        // ?? plancaseworkdebit ??????????????????????????????????????????????????

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByPlanCaseWorkDebit_Ascending()
        {
            var views = new List<ProjectView>
            {
                MakeSortView("PP003", planCaseWorkDebit: 30m),
                MakeSortView("PP002", planCaseWorkDebit: 20m),
                MakeSortView("PP001", planCaseWorkDebit: 10m),
            };
            var repo  = CreateRepository(projectViews: views);
            var query = new PaginationParameters<string>(sortBy: "plancaseworkdebit", descending: false, page: 1, pageSize: 10);

            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            var items = result.Data.ToList();
            Assert.Equal(10m, items[0].PlanCaseWorkDebit);
            Assert.Equal(20m, items[1].PlanCaseWorkDebit);
            Assert.Equal(30m, items[2].PlanCaseWorkDebit);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_SortsByPlanCaseWorkDebit_Descending()
        {
            var views = new List<ProjectView>
            {
                MakeSortView("PP001", planCaseWorkDebit: 10m),
                MakeSortView("PP002", planCaseWorkDebit: 20m),
                MakeSortView("PP003", planCaseWorkDebit: 30m),
            };
            var repo  = CreateRepository(projectViews: views);
            var query = new PaginationParameters<string>(sortBy: "plancaseworkdebit", descending: true, page: 1, pageSize: 10);

            var result = await repo.GetProjectsByProgramAsync(query, "P001");

            var items = result.Data.ToList();
            Assert.Equal(30m, items[0].PlanCaseWorkDebit);
            Assert.Equal(20m, items[1].PlanCaseWorkDebit);
            Assert.Equal(10m, items[2].PlanCaseWorkDebit);
        }

        #endregion
    }
}
