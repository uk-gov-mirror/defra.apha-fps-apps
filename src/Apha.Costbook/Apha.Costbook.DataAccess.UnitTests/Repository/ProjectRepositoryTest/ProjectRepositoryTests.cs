using Apha.Common.Helpers.Repository;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess.Data;
using Apha.Costbook.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Web;

namespace Apha.Costbook.DataAccess.UnitTests.Repository.ProjectRepositoryTest
{
    public class ProjectRepositoryTests
    {
        /// <summary>
        /// Creates a ProjectRepository with in-memory Projects data.
        /// CostbookDbContext is mocked using Moq and RepositoryTestHelper.
        /// </summary>
        private static ProjectRepository CreateRepository(IEnumerable<Project> projects)
        {
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockSettingsRepository = new Mock<ISettingsRepository>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);

            var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects);
            mockContext.Setup(x => x.Set<Project>()).Returns(projectsMockSet.Object);
            mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);

            // Setup additional DbSets for Delete operations (they need to exist even if empty)
            var emptyAnimalReqs = RepositoryTestHelper.CreateMockDbSet(new List<AnimalRequirement>());
            var emptyAdditionalCosts = RepositoryTestHelper.CreateMockDbSet(new List<AdditionalCost>());
            var emptyTestReqs = RepositoryTestHelper.CreateMockDbSet(new List<TestRequirement>());
            var emptyStaffReqs = RepositoryTestHelper.CreateMockDbSet(new List<StaffRequirement>());
            var emptyProjectYears = RepositoryTestHelper.CreateMockDbSet(new List<ProjectYear>());

            mockContext.Setup(x => x.Set<AnimalRequirement>()).Returns(emptyAnimalReqs.Object);
            mockContext.Setup(x => x.Set<AdditionalCost>()).Returns(emptyAdditionalCosts.Object);
            mockContext.Setup(x => x.Set<TestRequirement>()).Returns(emptyTestReqs.Object);
            mockContext.Setup(x => x.Set<StaffRequirement>()).Returns(emptyStaffReqs.Object);
            mockContext.Setup(x => x.Set<ProjectYear>()).Returns(emptyProjectYears.Object);

            // Setup SaveChangesAsync
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new ProjectRepository(mockContext.Object, mockSettingsRepository.Object);
        }

        /// <summary>
        /// Creates a ProjectRepository with full pivot-related data sets.
        /// </summary>
        private static ProjectRepository CreateRepositoryWithPivotData(
            IEnumerable<StaffRequirement> staffRequirements,
            IEnumerable<WorkGroupGrade>? workGroupGrades = null,
            IEnumerable<AnimalRequirement>? animalRequirements = null,
            IEnumerable<AdditionalCost>? additionalCosts = null,
            IEnumerable<TestRequirement>? testRequirements = null,
            IEnumerable<FpsAccountCategory>? accountCategories = null,
            IEnumerable<Project>? projects = null)
        {
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockSettingsRepository = new Mock<ISettingsRepository>();
            mockSettingsRepository
                .Setup(s => s.GetSettingValueByIdAsync("DaysInYear"))
                .ReturnsAsync("220");

            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);

            var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects ?? new List<Project>());
            mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);
            mockContext.Setup(x => x.Set<Project>()).Returns(projectsMockSet.Object);

            var staffMockSet = RepositoryTestHelper.CreateMockDbSet(staffRequirements);
            mockContext.Setup(x => x.StaffRequirements).Returns(staffMockSet.Object);
            mockContext.Setup(x => x.Set<StaffRequirement>()).Returns(staffMockSet.Object);

            var wgMockSet = RepositoryTestHelper.CreateMockDbSet(workGroupGrades ?? new List<WorkGroupGrade>());
            mockContext.Setup(x => x.WorkGroupGrades).Returns(wgMockSet.Object);

            var animalMockSet = RepositoryTestHelper.CreateMockDbSet(animalRequirements ?? new List<AnimalRequirement>());
            mockContext.Setup(x => x.AnimalRequirements).Returns(animalMockSet.Object);
            mockContext.Setup(x => x.Set<AnimalRequirement>()).Returns(animalMockSet.Object);

            var additionalCostMockSet = RepositoryTestHelper.CreateMockDbSet(additionalCosts ?? new List<AdditionalCost>());
            mockContext.Setup(x => x.AdditionalCosts).Returns(additionalCostMockSet.Object);
            mockContext.Setup(x => x.Set<AdditionalCost>()).Returns(additionalCostMockSet.Object);

            var testReqMockSet = RepositoryTestHelper.CreateMockDbSet(testRequirements ?? new List<TestRequirement>());
            mockContext.Setup(x => x.TestRequirements).Returns(testReqMockSet.Object);
            mockContext.Setup(x => x.Set<TestRequirement>()).Returns(testReqMockSet.Object);

            var accountCatMockSet = RepositoryTestHelper.CreateMockDbSet(accountCategories ?? new List<FpsAccountCategory>());
            mockContext.Setup(x => x.FpsAccountCategories).Returns(accountCatMockSet.Object);

            var emptyProjectYears = RepositoryTestHelper.CreateMockDbSet(new List<ProjectYear>());
            mockContext.Setup(x => x.Set<ProjectYear>()).Returns(emptyProjectYears.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new ProjectRepository(mockContext.Object, mockSettingsRepository.Object);
        }

        #region GetPaginatedProjectsAsync

        [Fact]
        public async Task GetPaginatedProjectsAsync_WithNoFilter_ReturnsAllProjects()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ProjectTitle = "Project 1", ContractNumber = "CON001" },
                new() { ProjectId = "2024/002", ProjectTitle = "Project 2", ContractNumber = "CON002" },
                new() { ProjectId = "2024/003", ProjectTitle = "Project 3", ContractNumber = "CON003" }
            };
            var repo = CreateRepository(projects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await repo.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count());
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.PageNumber);
            Assert.Equal(10, result.PaginationData.PageSize);
        }

        [Fact]
        public async Task GetPaginatedProjectsAsync_WithProjectIdFilter_ReturnsFilteredProjects()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ProjectTitle = "Project 1" },
                new() { ProjectId = "2024/002", ProjectTitle = "Project 2" },
                new() { ProjectId = "2025/001", ProjectTitle = "Project 3" }
            };
            var repo = CreateRepository(projects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"ProjectId\":\"2024\"}"
            };

            // Act
            var result = await repo.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, p => Assert.Contains("2024", p.ProjectId));
        }

        [Fact]
        public async Task GetPaginatedProjectsAsync_WithPaging_ReturnsCorrectPage()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ProjectTitle = "Project 1" },
                new() { ProjectId = "2024/002", ProjectTitle = "Project 2" },
                new() { ProjectId = "2024/003", ProjectTitle = "Project 3" },
                new() { ProjectId = "2024/004", ProjectTitle = "Project 4" },
                new() { ProjectId = "2024/005", ProjectTitle = "Project 5" }
            };
            var repo = CreateRepository(projects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 2,
                PageSize = 2
            };

            // Act
            var result = await repo.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        [Theory]
        [InlineData("projectid", false, "2024/001")]
        [InlineData("projectid", true, "2024/003")]
        [InlineData("projecttitle", false, "Project A")]
        [InlineData("projecttitle", true, "Project C")]
        [InlineData("programme", false, "Programme A")]
        [InlineData("programme", true, "Programme C")]
        [InlineData("contractnumber", false, "CON001")]
        [InlineData("contractnumber", true, "CON003")]
        public async Task GetPaginatedProjectsAsync_WithSorting_ReturnsSortedProjects(
            string sortBy, bool descending, string expectedFirstValue)
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/002", ProjectTitle = "Project B", Programme = "Programme B", ContractNumber = "CON002" },
                new() { ProjectId = "2024/001", ProjectTitle = "Project A", Programme = "Programme A", ContractNumber = "CON001" },
                new() { ProjectId = "2024/003", ProjectTitle = "Project C", Programme = "Programme C", ContractNumber = "CON003" }
            };
            var repo = CreateRepository(projects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                Descending = descending
            };

            // Act
            var result = await repo.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            var firstProject = result.Data.First();
            var actualValue = sortBy.ToLower() switch
            {
                "projectid" => firstProject.ProjectId,
                "projecttitle" => firstProject.ProjectTitle,
                "programme" => firstProject.Programme,
                "contractnumber" => firstProject.ContractNumber,
                _ => null
            };
            Assert.Equal(expectedFirstValue, actualValue);
        }

        [Fact]
        public async Task GetPaginatedProjectsAsync_WithInvalidSortBy_UsesDefaultSorting()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ProjectTitle = "Project 1" },
                new() { ProjectId = "2024/002", ProjectTitle = "Project 2" },
                new() { ProjectId = "2024/003", ProjectTitle = "Project 3" }
            };
            var repo = CreateRepository(projects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "invalid_field"
            };

            // Act
            var result = await repo.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            // Default sorting is descending by ProjectId
            Assert.Equal("2024/003", result.Data.First().ProjectId);
        }

        [Fact]
        public async Task GetPaginatedProjectsAsync_WithEmptyResult_ReturnsEmptyPagedData()
        {
            // Arrange
            var projects = new List<Project>();
            var repo = CreateRepository(projects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await repo.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
            Assert.Equal(0, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetPaginatedProjectsAsync_WithFilterAndSorting_ReturnsFilteredAndSortedProjects()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ProjectTitle = "Test Project A", Programme = "Programme A" },
                new() { ProjectId = "2024/002", ProjectTitle = "Test Project B", Programme = "Programme B" },
                new() { ProjectId = "2025/001", ProjectTitle = "Other Project", Programme = "Programme C" }
            };
            var repo = CreateRepository(projects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"ProjectId\":\"2024\"}",
                SortBy = "ProjectTitle",
                Descending = false
            };

            // Act
            var result = await repo.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal("Test Project A", result.Data.First().ProjectTitle);
            Assert.All(result.Data, p => Assert.Contains("2024", p.ProjectId));
        }

        [Theory]
        [InlineData("customername", false)]
        [InlineData("customername", true)]
        [InlineData("disease", false)]
        [InlineData("disease", true)]
        [InlineData("StartDate", false)]
        [InlineData("StartDate", true)]
        [InlineData("ContractPrice", false)]
        [InlineData("ContractPrice", true)]
        [InlineData("preparedby", false)]
        [InlineData("preparedby", true)]
        [InlineData("dateofsubmission", false)]
        [InlineData("dateofsubmission", true)]
        public async Task GetPaginatedProjectsAsync_WithDifferentSortFields_SortsCorrectly(string sortBy, bool descending)
        {
            // Arrange
            var projects = new List<Project>
            {
                new()
                {
                    ProjectId = "2024/001",
                    CustomerName = "Customer A",
                    Disease = "Disease A",
                    StartDate = new DateTime(2024, 1, 1),
                    ContractPrice = 1000,
                    PreparedBy = "Person A",
                    DateOfSubmission = new DateTime(2024, 1, 1)
                },
                new()
                {
                    ProjectId = "2024/002",
                    CustomerName = "Customer B",
                    Disease = "Disease B",
                    StartDate = new DateTime(2024, 2, 1),
                    ContractPrice = 2000,
                    PreparedBy = "Person B",
                    DateOfSubmission = new DateTime(2024, 2, 1)
                }
            };
            var repo = CreateRepository(projects);
            var queryFilter = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                Descending = descending
            };

            // Act
            var result = await repo.GetPaginatedProjectsAsync(queryFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
        }

        #endregion        


        #region GetProjectByIdAsync

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsProject_WhenProjectExists()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ContractNumber = "CON001", SubmittedByLName = "Smith", SubmittedByFName = "John" },
                new() { ProjectId = "2024/002", ContractNumber = "CON002", SubmittedByLName = "Jones", SubmittedByFName = "Jane" }
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetProjectByIdAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("2024/001", result.ProjectId);
            Assert.Equal("CON001", result.ContractNumber);
        }

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsNull_WhenProjectDoesNotExist()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ContractNumber = "CON001", SubmittedByLName = "Smith", SubmittedByFName = "John" }
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetProjectByIdAsync("NONEXISTENT");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProjectByIdAsync_HandlesUrlDecodedId()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001a", ContractNumber = "CON001", SubmittedByLName = "Smith", SubmittedByFName = "John" }
            };
            var repo = CreateRepository(projects);
            var encodedId = HttpUtility.UrlEncode("2024/001a");

            // Act
            var result = await repo.GetProjectByIdAsync(encodedId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("2024/001a", result.ProjectId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetProjectByIdAsync_ReturnsNull_WhenIdIsNullOrEmpty(string? id)
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ContractNumber = "CON001", SubmittedByLName = "Smith", SubmittedByFName = "John" }
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetProjectByIdAsync(id!);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddProjectAsync

        [Fact]
        public async Task AddProjectAsync_AddsProject_AndReturnsProject()
        {
            // Arrange
            var projects = new List<Project>();
            var repo = CreateRepository(projects);
            var newProject = new Project
            {
                ProjectId = "2024/001",
                ContractNumber = "CON001",
                SubmittedByLName = "Smith",
                SubmittedByFName = "John"
            };

            // Act
            var result = await repo.AddProjectAsync(newProject);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newProject.ProjectId, result.ProjectId);
            Assert.Equal(newProject.ContractNumber, result.ContractNumber);
        }

        [Fact]
        public async Task AddProjectAsync_ReturnsCorrectProject_WhenProjectHasAllProperties()
        {
            // Arrange
            var projects = new List<Project>();
            var repo = CreateRepository(projects);
            var newProject = new Project
            {
                ProjectId = "2024/001",
                ContractNumber = "CON001",
                SubmittedByLName = "Smith",
                SubmittedByFName = "John"
            };

            // Act
            var result = await repo.AddProjectAsync(newProject);

            // Assert
            Assert.Same(newProject, result);
        }

        #endregion

        #region UpdateProjectAsync

        [Fact]
        public async Task UpdateProjectAsync_UpdatesProject_AndReturnsProject()
        {
            // Arrange
            var existingProject = new Project
            {
                ProjectId = "2024/001",
                ContractNumber = "CON001",
                SubmittedByLName = "Smith",
                SubmittedByFName = "John"
            };
            var projects = new List<Project> { existingProject };
            var repo = CreateRepository(projects);

            var updatedProject = new Project
            {
                ProjectId = "2024/001",
                ContractNumber = "CON002",
                SubmittedByLName = "Jones",
                SubmittedByFName = "Jane"
            };

            // Act
            var result = await repo.UpdateProjectAsync(updatedProject);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updatedProject.ProjectId, result.ProjectId);
            Assert.Equal(updatedProject.ContractNumber, result.ContractNumber);
        }

        [Fact]
        public async Task UpdateProjectAsync_ReturnsUpdatedProject()
        {
            // Arrange
            var projects = new List<Project>();
            var repo = CreateRepository(projects);
            var projectToUpdate = new Project
            {
                ProjectId = "2024/001",
                ContractNumber = "CON001",
                SubmittedByLName = "Smith",
                SubmittedByFName = "John"
            };

            // Act
            var result = await repo.UpdateProjectAsync(projectToUpdate);

            // Assert
            Assert.Same(projectToUpdate, result);
        }

        #endregion



        #region GetNextProjectNumberAsync

        [Fact]
        public async Task GetNextProjectNumberAsync_ReturnsFirstProjectNumber_WhenNoProjectsExist()
        {
            // Arrange
            var projects = new List<Project>();
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetNextProjectNumberAsync(null);

            // Assert
            Assert.NotNull(result);
            var currentYear = DateTime.Now.Month <= 3 ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            Assert.Equal($"{currentYear}/001", result);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_ReturnsNextSequentialNumber_WhenProjectsExistForCurrentYear()
        {
            // Arrange
            var currentYear = DateTime.Now.Month <= 3 ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var projects = new List<Project>
            {
                new() { ProjectId = $"{currentYear}/001" },
                new() { ProjectId = $"{currentYear}/002" },
                new() { ProjectId = $"{currentYear}/003" }
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetNextProjectNumberAsync(null);

            // Assert
            Assert.Equal($"{currentYear}/004", result);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_ReturnsFirstNumberForYear_WhenNoProjectsForCurrentYear()
        {
            // Arrange
            var currentYear = DateTime.Now.Month <= 3 ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            var projects = new List<Project>
            {
                new() { ProjectId = $"{currentYear - 1}/001" },
                new() { ProjectId = $"{currentYear - 1}/002" }
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetNextProjectNumberAsync(null);

            // Assert
            Assert.Equal($"{currentYear}/001", result);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_ReturnsBaseNumber_WhenBaseNumberProvidedAndNoSimilarProjects()
        {
            // Arrange
            var projects = new List<Project>();
            var repo = CreateRepository(projects);
            var baseNumber = "2024/001";

            // Act
            var result = await repo.GetNextProjectNumberAsync(baseNumber);

            // Assert
            Assert.Equal(baseNumber, result);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_ReturnsNextLetterSuffix_WhenBaseNumberHasLetterSuffix()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001a" },
                new() { ProjectId = "2024/001b" }
            };
            var repo = CreateRepository(projects);
            var baseNumber = "2024/001a";

            // Act
            var result = await repo.GetNextProjectNumberAsync(baseNumber);

            // Assert
            Assert.Equal("2024/001c", result);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_ReturnsWithLetterSuffix_WhenBaseNumberExistsWithLetterVariation()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001" },
                new() { ProjectId = "2024/001a" }
            };
            var repo = CreateRepository(projects);
            var baseNumber = "2024/001";

            // Act
            var result = await repo.GetNextProjectNumberAsync(baseNumber);

            // Assert
            Assert.Equal("2024/001b", result);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_ReturnsFirstLetterSuffix_WhenOnlyBaseNumberExists()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001" }
            };
            var repo = CreateRepository(projects);
            var baseNumber = "2024/001";

            // Act
            var result = await repo.GetNextProjectNumberAsync(baseNumber);

            // Assert
            Assert.Equal("2024/001a", result);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_HandlesUrlDecodedBaseNumber()
        {
            // Arrange
            var projects = new List<Project>();
            var repo = CreateRepository(projects);
            var encodedBaseNumber = HttpUtility.UrlEncode("2024/001");

            // Act
            var result = await repo.GetNextProjectNumberAsync(encodedBaseNumber);

            // Assert
            Assert.Equal("2024/001", result);
        }

        [Fact]
        public async Task GetNextProjectNumberAsync_HandlesMalformedProjectIds()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "MALFORMED" },
                new() { ProjectId = "2024/ABC" }
            };
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetNextProjectNumberAsync(null);

            // Assert
            var currentYear = DateTime.Now.Month <= 3 ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            Assert.Equal($"{currentYear}/001", result);
        }

        #endregion

        #region GetCurrentFinancialYear

        [Fact]
        public async Task GetCurrentFinancialYear_ReturnsCurrentYear_WhenAfterMarch()
        {
            // This test validates the financial year logic
            // Note: Since GetCurrentFinancialYear is private, we test it indirectly through GetNextProjectNumberAsync

            // Arrange
            var projects = new List<Project>();
            var repo = CreateRepository(projects);

            // Act
            var result = await repo.GetNextProjectNumberAsync(null);

            // Assert
            var expectedYear = DateTime.Now.Month <= 3 ? DateTime.Now.Year - 1 : DateTime.Now.Year;
            Assert.StartsWith($"{expectedYear}/", result);
        }

        #endregion

        #region Edge Cases and Error Handling

        [Fact]
        public async Task GetProjectByIdAsync_ReturnsNullableProject()
        {
            // Arrange — verifies the return type contract allows null
            var repo = CreateRepository(new List<Project>());

            // Act
            var result = await repo.GetProjectByIdAsync("NONEXISTENT");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddProjectAsync_ReturnsNotNull()
        {
            // Arrange — verifies the return type contract is never null
            var projects = new List<Project>();
            var repo = CreateRepository(projects);
            var newProject = new Project { ProjectId = "2024/001" };

            // Act
            var result = await repo.AddProjectAsync(newProject);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateProjectAsync_ReturnsNotNull()
        {
            // Arrange — verifies the return type contract is never null
            var projects = new List<Project>();
            var repo = CreateRepository(projects);
            var projectToUpdate = new Project { ProjectId = "2024/001" };

            // Act
            var result = await repo.UpdateProjectAsync(projectToUpdate);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region GetStaffYearsPivotAsync

        [Fact]
        public async Task GetStaffYearsPivotAsync_WithNoData_ReturnsEmptyPivot()
        {
            // Arrange
            var repo = CreateRepositoryWithPivotData(new List<StaffRequirement>());

            // Act
            var result = await repo.GetStaffYearsPivotAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Rows);
            Assert.Empty(result.Years);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task GetStaffYearsPivotAsync_ReturnsCorrectYears()
        {
            // Arrange
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "A1", Year = 2024, Nodays = 110.0 },
                new() { Project = "2024/001", WgGrade = "A1", Year = 2025, Nodays = 220.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements);

            // Act
            var result = await repo.GetStaffYearsPivotAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Years.Count);
            Assert.Contains(2024, result.Years);
            Assert.Contains(2025, result.Years);
        }

        [Fact]
        public async Task GetStaffYearsPivotAsync_GroupsByFirstLetterOfGrade()
        {
            // Arrange — different grades with same first letter are grouped together
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "A1", Year = 2024, Nodays = 110.0 },
                new() { Project = "2024/001", WgGrade = "A2", Year = 2024, Nodays = 220.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements);

            // Act
            var result = await repo.GetStaffYearsPivotAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.Equal("A", result.Rows[0].Grade);
        }

        [Fact]
        public async Task GetStaffYearsPivotAsync_GroupsGD5GradesSeparately()
        {
            // Arrange — GD5 grades should be grouped as "GD5" not "G"
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "GD5A", Year = 2024, Nodays = 110.0 },
                new() { Project = "2024/001", WgGrade = "GD5B", Year = 2024, Nodays = 110.0 },
                new() { Project = "2024/001", WgGrade = "G1",   Year = 2024, Nodays = 110.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements);

            // Act
            var result = await repo.GetStaffYearsPivotAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Rows.Count);
            Assert.Contains(result.Rows, r => r.Grade == "GD5");
            Assert.Contains(result.Rows, r => r.Grade == "G");
        }

        [Fact]
        public async Task GetStaffYearsPivotAsync_CalculatesTotalAsFractionOfDaysInYear()
        {
            // Arrange — DaysInYear = 220, Nodays = 110 → Total = 110/220 = 0.5
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "B1", Year = 2024, Nodays = 110.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements);

            // Act
            var result = await repo.GetStaffYearsPivotAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.Equal(0.5, result.Rows[0].Total, precision: 5);
        }

        [Fact]
        public async Task GetStaffYearsPivotAsync_PopulatesYearlyAmounts_PerGrade()
        {
            // Arrange
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "C1", Year = 2024, Nodays = 220.0 },
                new() { Project = "2024/001", WgGrade = "C2", Year = 2025, Nodays = 110.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements);

            // Act
            var result = await repo.GetStaffYearsPivotAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            var row = Assert.Single(result.Rows);
            Assert.Equal("C", row.Grade);
            Assert.Equal(1.0, row.YearlyAmounts[2024], precision: 5);
            Assert.Equal(0.5, row.YearlyAmounts[2025], precision: 5);
        }

        [Fact]
        public async Task GetStaffYearsPivotAsync_OnlyIncludesRecordsForRequestedProject()
        {
            // Arrange
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "D1", Year = 2024, Nodays = 220.0 },
                new() { Project = "2024/002", WgGrade = "D1", Year = 2024, Nodays = 220.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements);

            // Act
            var result = await repo.GetStaffYearsPivotAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.All(result.Rows, r => Assert.Equal("2024/001", r.Project));
        }

        [Fact]
        public async Task GetStaffYearsPivotAsync_WithGradeFilter_ReturnsFilteredRows()
        {
            // Arrange
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "A1", Year = 2024, Nodays = 220.0 },
                new() { Project = "2024/001", WgGrade = "B1", Year = 2024, Nodays = 220.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements);
            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Grade\":\"A\"}"
            };

            // Act
            var result = await repo.GetStaffYearsPivotAsync("2024/001", parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.Equal("A", result.Rows[0].Grade);
        }

        [Fact]
        public async Task GetStaffYearsPivotAsync_WithSortByGradeDescending_ReturnsSortedRows()
        {
            // Arrange
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "A1", Year = 2024, Nodays = 110.0 },
                new() { Project = "2024/001", WgGrade = "C1", Year = 2024, Nodays = 220.0 },
                new() { Project = "2024/001", WgGrade = "B1", Year = 2024, Nodays = 330.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements);
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "grade", Descending = true };

            // Act
            var result = await repo.GetStaffYearsPivotAsync("2024/001", parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(new[] { "C", "B", "A" }, result.Rows.Select(r => r.Grade).ToArray());
        }

        [Fact]
        public async Task GetStaffYearsPivotAsync_WithPaging_ReturnsCorrectPage()
        {
            // Arrange — 3 distinct grade groups; request page 2 with page size 1
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "A1", Year = 2024, Nodays = 110.0 },
                new() { Project = "2024/001", WgGrade = "B2", Year = 2024, Nodays = 220.0 },
                new() { Project = "2024/001", WgGrade = "C1", Year = 2024, Nodays = 330.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements);
            var parameters = new PaginationParameters<string> { Page = 2, PageSize = 1 };

            // Act
            var result = await repo.GetStaffYearsPivotAsync("2024/001", parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.Equal(3, result.TotalCount);
        }

        [Fact]
        public async Task GetStaffYearsPivotAsync_HandlesUrlEncodedProjectId()
        {
            // Arrange
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "E1", Year = 2024, Nodays = 220.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements);
            var encodedId = HttpUtility.UrlEncode("2024/001");

            // Act
            var result = await repo.GetStaffYearsPivotAsync(encodedId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
        }

        [Fact]
        public async Task GetStaffYearsPivotAsync_WithNullParameters_ReturnsTotalCount()
        {
            // Arrange
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "F1", Year = 2024, Nodays = 220.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements);

            // Act
            var result = await repo.GetStaffYearsPivotAsync("2024/001", null);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Rows);
        }

        [Fact]
        public async Task GetStaffYearsPivotAsync_UsesDaysInYearSettingForCalculation()
        {
            // Arrange — DaysInYear = 220, Nodays = 220 → fraction = 1.0
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "G1", Year = 2024, Nodays = 220.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements);

            // Act
            var result = await repo.GetStaffYearsPivotAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.Equal(1.0, result.Rows[0].Total, precision: 5);
        }

        #endregion

        #region GetStaffEffortAsync

        [Fact]
        public async Task GetStaffEffortAsync_WithNoData_ReturnsEmptyPivot()
        {
            // Arrange
            var repo = CreateRepositoryWithPivotData(new List<StaffRequirement>());

            // Act
            var result = await repo.GetStaffEffortAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Rows);
            Assert.Empty(result.Years);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task GetStaffEffortAsync_ReturnsCorrectYears()
        {
            // Arrange
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "A1", Name = "Alice", Year = 2024, Nodays = 10.0 },
                new() { Project = "2024/001", WgGrade = "A1", Name = "Alice", Year = 2025, Nodays = 20.0 }
            };
            var workGroupGrades = new List<WorkGroupGrade>
            {
                new() { WgGrade = "A1", WorkGroup = "Science", ProfitCentreGrade = "PCG1" }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements, workGroupGrades);

            // Act
            var result = await repo.GetStaffEffortAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Years.Count);
            Assert.Contains(2024, result.Years);
            Assert.Contains(2025, result.Years);
        }

        [Fact]
        public async Task GetStaffEffortAsync_GroupsByProjectWorkGroupGradeCodeAndName()
        {
            // Arrange — two rows with same Project/WorkGroup/GradeCode/Name should merge into one
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "A1", Name = "Alice", Year = 2024, Nodays = 10.0 },
                new() { Project = "2024/001", WgGrade = "A1", Name = "Alice", Year = 2025, Nodays = 15.0 }
            };
            var workGroupGrades = new List<WorkGroupGrade>
            {
                new() { WgGrade = "A1", WorkGroup = "Science", ProfitCentreGrade = "PCG1" }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements, workGroupGrades);

            // Act
            var result = await repo.GetStaffEffortAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.Equal(25.0, result.Rows[0].Total, precision: 5);
        }

        [Fact]
        public async Task GetStaffEffortAsync_PopulatesWorkGroupFromLookup()
        {
            // Arrange
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "B2", Name = "Bob", Year = 2024, Nodays = 5.0 }
            };
            var workGroupGrades = new List<WorkGroupGrade>
            {
                new() { WgGrade = "B2", WorkGroup = "Virology", ProfitCentreGrade = "PCG2" }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements, workGroupGrades);

            // Act
            var result = await repo.GetStaffEffortAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.Equal("Virology", result.Rows[0].WorkGroup);
        }

        [Fact]
        public async Task GetStaffEffortAsync_UsesEmptyWorkGroup_WhenGradeNotInLookup()
        {
            // Arrange — WgGrade not in WorkGroupGrades lookup
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "Z9", Name = "Zara", Year = 2024, Nodays = 5.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements, new List<WorkGroupGrade>());

            // Act
            var result = await repo.GetStaffEffortAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.Equal(string.Empty, result.Rows[0].WorkGroup);
        }

        [Fact]
        public async Task GetStaffEffortAsync_GD5GradeCode_IsGroupedAsGD5()
        {
            // Arrange
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "GD5X", Name = "Carol", Year = 2024, Nodays = 10.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements, new List<WorkGroupGrade>());

            // Act
            var result = await repo.GetStaffEffortAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.Equal("GD5", result.Rows[0].GradeCode);
        }

        [Fact]
        public async Task GetStaffEffortAsync_OnlyIncludesRecordsForRequestedProject()
        {
            // Arrange
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "A1", Name = "Alice", Year = 2024, Nodays = 10.0 },
                new() { Project = "2024/002", WgGrade = "A1", Name = "Bob",   Year = 2024, Nodays = 10.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements, new List<WorkGroupGrade>());

            // Act
            var result = await repo.GetStaffEffortAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.All(result.Rows, r => Assert.Equal("2024/001", r.Project));
        }

        [Fact]
        public async Task GetStaffEffortAsync_WithGradeCodeFilter_ReturnsFilteredRows()
        {
            // Arrange
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "A1", Name = "Alice", Year = 2024, Nodays = 10.0 },
                new() { Project = "2024/001", WgGrade = "B1", Name = "Bob",   Year = 2024, Nodays = 10.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements, new List<WorkGroupGrade>());
            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"GradeCode\":\"A\"}"
            };

            // Act
            var result = await repo.GetStaffEffortAsync("2024/001", parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.Equal("A", result.Rows[0].GradeCode);
        }

        [Fact]
        public async Task GetStaffEffortAsync_WithNameFilter_ReturnsFilteredRows()
        {
            // Arrange
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "A1", Name = "Alice",  Year = 2024, Nodays = 10.0 },
                new() { Project = "2024/001", WgGrade = "A1", Name = "Bob",    Year = 2024, Nodays = 10.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements, new List<WorkGroupGrade>());
            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Name\":\"Alice\"}"
            };

            // Act
            var result = await repo.GetStaffEffortAsync("2024/001", parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.Equal("Alice", result.Rows[0].Name);
        }

        [Fact]
        public async Task GetStaffEffortAsync_WithWorkGroupFilter_ReturnsFilteredRows()
        {
            // Arrange
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "A1", Name = "Alice", Year = 2024, Nodays = 10.0 },
                new() { Project = "2024/001", WgGrade = "B1", Name = "Bob",   Year = 2024, Nodays = 10.0 }
            };
            var workGroupGrades = new List<WorkGroupGrade>
            {
                new() { WgGrade = "A1", WorkGroup = "Science",  ProfitCentreGrade = "PCG1" },
                new() { WgGrade = "B1", WorkGroup = "Virology", ProfitCentreGrade = "PCG2" }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements, workGroupGrades);
            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"WorkGroup\":\"Science\"}"
            };

            // Act
            var result = await repo.GetStaffEffortAsync("2024/001", parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.Equal("Science", result.Rows[0].WorkGroup);
        }

        [Fact]
        public async Task GetStaffEffortAsync_WithSortByNameAscending_ReturnsSortedRows()
        {
            // Arrange
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "A1", Name = "Charlie", Year = 2024, Nodays = 10.0 },
                new() { Project = "2024/001", WgGrade = "A1", Name = "Alice",   Year = 2024, Nodays = 10.0 },
                new() { Project = "2024/001", WgGrade = "A1", Name = "Bob",     Year = 2024, Nodays = 10.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements, new List<WorkGroupGrade>());
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "name", Descending = false };

            // Act
            var result = await repo.GetStaffEffortAsync("2024/001", parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(new[] { "Alice", "Bob", "Charlie" }, result.Rows.Select(r => r.Name).ToArray());
        }

        [Fact]
        public async Task GetStaffEffortAsync_WithSortByNameDescending_ReturnsSortedRows()
        {
            // Arrange
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "A1", Name = "Charlie", Year = 2024, Nodays = 10.0 },
                new() { Project = "2024/001", WgGrade = "A1", Name = "Alice",   Year = 2024, Nodays = 10.0 },
                new() { Project = "2024/001", WgGrade = "A1", Name = "Bob",     Year = 2024, Nodays = 10.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements, new List<WorkGroupGrade>());
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "name", Descending = true };

            // Act
            var result = await repo.GetStaffEffortAsync("2024/001", parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(new[] { "Charlie", "Bob", "Alice" }, result.Rows.Select(r => r.Name).ToArray());
        }

        [Fact]
        public async Task GetStaffEffortAsync_WithPaging_ReturnsCorrectPage()
        {
            // Arrange — 3 distinct name rows; request page 2 with page size 1
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "A1", Name = "Alice",   Year = 2024, Nodays = 10.0 },
                new() { Project = "2024/001", WgGrade = "A1", Name = "Bob",     Year = 2024, Nodays = 10.0 },
                new() { Project = "2024/001", WgGrade = "A1", Name = "Charlie", Year = 2024, Nodays = 10.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements, new List<WorkGroupGrade>());
            var parameters = new PaginationParameters<string> { Page = 2, PageSize = 1 };

            // Act
            var result = await repo.GetStaffEffortAsync("2024/001", parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.Equal(3, result.TotalCount);
        }

        [Fact]
        public async Task GetStaffEffortAsync_HandlesUrlEncodedProjectId()
        {
            // Arrange
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "A1", Name = "Alice", Year = 2024, Nodays = 10.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements, new List<WorkGroupGrade>());
            var encodedId = HttpUtility.UrlEncode("2024/001");

            // Act
            var result = await repo.GetStaffEffortAsync(encodedId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
        }

        [Fact]
        public async Task GetStaffEffortAsync_YearlyAmounts_SummedCorrectlyPerRow()
        {
            // Arrange — same person, two records in same year
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "A1", Name = "Alice", Year = 2024, Nodays = 30.0 },
                new() { Project = "2024/001", WgGrade = "A1", Name = "Alice", Year = 2024, Nodays = 20.0 }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements, new List<WorkGroupGrade>());

            // Act
            var result = await repo.GetStaffEffortAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.Equal(50.0, result.Rows[0].YearlyAmounts[2024], precision: 5);
            Assert.Equal(50.0, result.Rows[0].Total, precision: 5);
        }

        #endregion

        #region GetProjectCostsPivotAsync

        [Fact]
        public async Task GetProjectCostsPivotAsync_WithNoData_ReturnsEmptyPivot()
        {
            // Arrange
            var repo = CreateRepositoryWithPivotData(new List<StaffRequirement>());

            // Act
            var result = await repo.GetProjectCostsPivotAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Rows);
            Assert.Empty(result.Years);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_AnimalCosts_AreGroupedAsOtherCosts()
        {
            // Arrange — NumberOfDays=2, NumberOfAnimals=3, DailyRate=10 → Cost = 60
            var animalRequirements = new List<AnimalRequirement>
            {
                new() { Project = "2024/001", Year = 2024, NumberOfDays = 2, NumberOfAnimals = 3, DailyRate = 10.0 }
            };
            var repo = CreateRepositoryWithPivotData(
                new List<StaffRequirement>(),
                animalRequirements: animalRequirements);

            // Act
            var result = await repo.GetProjectCostsPivotAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            var otherRow = result.Rows.FirstOrDefault(r => r.Category == "Other Costs");
            Assert.NotNull(otherRow);
            Assert.Equal(60.0, otherRow.YearlyAmounts[2024], precision: 5);
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_TestCosts_AreAggregatedIntoOtherCosts()
        {
            // Arrange — NumberOfTests=5, UnitPrice=20 → Cost = 100
            var testRequirements = new List<TestRequirement>
            {
                new() { Project = "2024/001", Year = 2024, NumberOfTests = 5, UnitPrice = 20.0 }
            };
            var repo = CreateRepositoryWithPivotData(
                new List<StaffRequirement>(),
                testRequirements: testRequirements);

            // Act
            var result = await repo.GetProjectCostsPivotAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            var otherRow = result.Rows.FirstOrDefault(r => r.Category == "Other Costs");
            Assert.NotNull(otherRow);
            Assert.Equal(100.0, otherRow.YearlyAmounts[2024], precision: 5);
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_StaffOHRRows_AreGroupedAsOverheads()
        {
            // Arrange — Nohours=10, Chargerate=50, Ohr=0.2, Npr=0.1
            // Cost = 10*50*(0.2+0.1)/50 = 10*0.3 = 3
            var staffRequirements = new List<StaffRequirement>
            {
                new()
                {
                    Project = "2024/001", WgGrade = "A1", Year = 2024,
                    Nohours = 10.0, Chargerate = 50.0, Ohr = 0.2, Npr = 0.1, Payrate = 0.0, Nodays = 0.0
                }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements);

            // Act
            var result = await repo.GetProjectCostsPivotAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            var overheadsRow = result.Rows.FirstOrDefault(r => r.Category == "Overheads");
            Assert.NotNull(overheadsRow);
            Assert.Equal(3.0, overheadsRow.YearlyAmounts[2024], precision: 5);
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_StaffPayRows_AreGroupedAsPay()
        {
            // Arrange — Nohours=10, Chargerate=50, Payrate=30
            // Cost = 10*50*30/50 = 10*30 = 300
            var staffRequirements = new List<StaffRequirement>
            {
                new()
                {
                    Project = "2024/001", WgGrade = "A1", Year = 2024,
                    Nohours = 10.0, Chargerate = 50.0, Payrate = 30.0, Ohr = 0.0, Npr = 0.0, Nodays = 0.0
                }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements);

            // Act
            var result = await repo.GetProjectCostsPivotAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            var payRow = result.Rows.FirstOrDefault(r => r.Category == "Pay");
            Assert.NotNull(payRow);
            Assert.Equal(300.0, payRow.YearlyAmounts[2024], precision: 5);
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_AdditionalCosts_UseCsg7GroupAsCategory()
        {
            // Arrange
            var additionalCosts = new List<AdditionalCost>
            {
                new() { Project = "2024/001", Year = 2024, AccountCat = "TRAVEL", ItemCost = 500.0 }
            };
            var accountCategories = new List<FpsAccountCategory>
            {
                new() { AccShortName = "TRAVEL", Csg7Group = "Travel & Subsistence", FpsYear = 2024 }
            };
            var repo = CreateRepositoryWithPivotData(
                new List<StaffRequirement>(),
                additionalCosts: additionalCosts,
                accountCategories: accountCategories);

            // Act
            var result = await repo.GetProjectCostsPivotAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            var travelRow = result.Rows.FirstOrDefault(r => r.Category == "Travel & Subsistence");
            Assert.NotNull(travelRow);
            Assert.Equal(500.0, travelRow.YearlyAmounts[2024], precision: 5);
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_AdditionalCosts_WithNullCsg7Group_FallsBackToOther()
        {
            // Arrange
            var additionalCosts = new List<AdditionalCost>
            {
                new() { Project = "2024/001", Year = 2024, AccountCat = "MISC", ItemCost = 250.0 }
            };
            var accountCategories = new List<FpsAccountCategory>
            {
                new() { AccShortName = "MISC", Csg7Group = null, FpsYear = 2024 }
            };
            var repo = CreateRepositoryWithPivotData(
                new List<StaffRequirement>(),
                additionalCosts: additionalCosts,
                accountCategories: accountCategories);

            // Act
            var result = await repo.GetProjectCostsPivotAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            var otherRow = result.Rows.FirstOrDefault(r => r.Category == "Other");
            Assert.NotNull(otherRow);
            Assert.Equal(250.0, otherRow.YearlyAmounts[2024], precision: 5);
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_OnlyIncludesRecordsForRequestedProject()
        {
            // Arrange
            var animalRequirements = new List<AnimalRequirement>
            {
                new() { Project = "2024/001", Year = 2024, NumberOfDays = 1, NumberOfAnimals = 1, DailyRate = 100.0 },
                new() { Project = "2024/002", Year = 2024, NumberOfDays = 1, NumberOfAnimals = 1, DailyRate = 100.0 }
            };
            var repo = CreateRepositoryWithPivotData(
                new List<StaffRequirement>(),
                animalRequirements: animalRequirements);

            // Act
            var result = await repo.GetProjectCostsPivotAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.All(result.Rows, r => Assert.Equal("2024/001", r.Project));
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_WithCategoryFilter_ReturnsFilteredRows()
        {
            // Arrange
            var animalRequirements = new List<AnimalRequirement>
            {
                new() { Project = "2024/001", Year = 2024, NumberOfDays = 1, NumberOfAnimals = 1, DailyRate = 100.0 }
            };
            var staffRequirements = new List<StaffRequirement>
            {
                new()
                {
                    Project = "2024/001", WgGrade = "A1", Year = 2024,
                    Nohours = 5.0, Chargerate = 20.0, Payrate = 10.0, Ohr = 0.0, Npr = 0.0, Nodays = 0.0
                }
            };
            var repo = CreateRepositoryWithPivotData(
                staffRequirements,
                animalRequirements: animalRequirements);
            var parameters = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Category\":\"Pay\"}"
            };

            // Act
            var result = await repo.GetProjectCostsPivotAsync("2024/001", parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.Equal("Pay", result.Rows[0].Category);
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_WithSortByCategoryDescending_ReturnsSortedRows()
        {
            // Arrange — generate Pay + Overheads + Other Costs categories
            var staffRequirements = new List<StaffRequirement>
            {
                new()
                {
                    Project = "2024/001", WgGrade = "A1", Year = 2024,
                    Nohours = 10.0, Chargerate = 50.0, Payrate = 30.0, Ohr = 0.2, Npr = 0.1, Nodays = 0.0
                }
            };
            var animalRequirements = new List<AnimalRequirement>
            {
                new() { Project = "2024/001", Year = 2024, NumberOfDays = 1, NumberOfAnimals = 1, DailyRate = 100.0 }
            };
            var repo = CreateRepositoryWithPivotData(
                staffRequirements,
                animalRequirements: animalRequirements);
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "category", Descending = true };

            // Act
            var result = await repo.GetProjectCostsPivotAsync("2024/001", parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(new[] { "Pay", "Overheads", "Other Costs" }, result.Rows.Select(r => r.Category).ToArray());
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_WithPaging_ReturnsCorrectPage()
        {
            // Arrange — generate Pay + Overheads + Other Costs categories
            var staffRequirements = new List<StaffRequirement>
            {
                new()
                {
                    Project = "2024/001", WgGrade = "A1", Year = 2024,
                    Nohours = 10.0, Chargerate = 50.0, Payrate = 30.0, Ohr = 0.2, Npr = 0.1, Nodays = 0.0
                }
            };
            var animalRequirements = new List<AnimalRequirement>
            {
                new() { Project = "2024/001", Year = 2024, NumberOfDays = 1, NumberOfAnimals = 1, DailyRate = 100.0 }
            };
            var repo = CreateRepositoryWithPivotData(
                staffRequirements,
                animalRequirements: animalRequirements);
            var parameters = new PaginationParameters<string> { Page = 1, PageSize = 1 };

            // Act
            var result = await repo.GetProjectCostsPivotAsync("2024/001", parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Rows);
            Assert.True(result.TotalCount >= 2); // At least Pay, Overheads, Other Costs
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_HandlesUrlEncodedProjectId()
        {
            // Arrange
            var animalRequirements = new List<AnimalRequirement>
            {
                new() { Project = "2024/001", Year = 2024, NumberOfDays = 1, NumberOfAnimals = 1, DailyRate = 50.0 }
            };
            var repo = CreateRepositoryWithPivotData(
                new List<StaffRequirement>(),
                animalRequirements: animalRequirements);
            var encodedId = HttpUtility.UrlEncode("2024/001");

            // Act
            var result = await repo.GetProjectCostsPivotAsync(encodedId);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result.Rows);
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_TotalIsSum_AcrossAllYears()
        {
            // Arrange — animal cost in two different years
            var animalRequirements = new List<AnimalRequirement>
            {
                new() { Project = "2024/001", Year = 2024, NumberOfDays = 1, NumberOfAnimals = 1, DailyRate = 100.0 },
                new() { Project = "2024/001", Year = 2025, NumberOfDays = 1, NumberOfAnimals = 1, DailyRate = 200.0 }
            };
            var repo = CreateRepositoryWithPivotData(
                new List<StaffRequirement>(),
                animalRequirements: animalRequirements);

            // Act
            var result = await repo.GetProjectCostsPivotAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            var otherRow = result.Rows.FirstOrDefault(r => r.Category == "Other Costs");
            Assert.NotNull(otherRow);
            Assert.Equal(300.0, otherRow.Total, precision: 5);
        }

        [Fact]
        public async Task GetProjectCostsPivotAsync_StaffWithZeroChargeRate_IsExcluded()
        {
            // Arrange — Chargerate = 0 should be excluded from Pay/Overheads
            var staffRequirements = new List<StaffRequirement>
            {
                new()
                {
                    Project = "2024/001", WgGrade = "A1", Year = 2024,
                    Nohours = 10.0, Chargerate = 0.0, Payrate = 30.0, Ohr = 0.2, Npr = 0.1, Nodays = 0.0
                }
            };
            var repo = CreateRepositoryWithPivotData(staffRequirements);

            // Act
            var result = await repo.GetProjectCostsPivotAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.DoesNotContain(result.Rows, r => r.Category == "Pay");
            Assert.DoesNotContain(result.Rows, r => r.Category == "Overheads");
        }

        #endregion

        #region GetProjectSummaryExportDataAsync

        /// <summary>
        /// Creates a ProjectRepository with all data sets required by GetProjectSummaryExportDataAsync.
        /// </summary>
        private static ProjectRepository CreateRepositoryForExport(
            IEnumerable<Project>? projects = null,
            IEnumerable<ProjectYear>? projectYears = null,
            IEnumerable<StaffRequirement>? staffRequirements = null,
            IEnumerable<TestRequirement>? testRequirements = null,
            IEnumerable<AnimalRequirement>? animalRequirements = null,
            IEnumerable<AdditionalCost>? additionalCosts = null)
        {
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockSettingsRepository = new Mock<ISettingsRepository>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);

            var projectsMockSet = RepositoryTestHelper.CreateMockDbSet(projects ?? new List<Project>());
            mockContext.Setup(x => x.Projects).Returns(projectsMockSet.Object);
            mockContext.Setup(x => x.Set<Project>()).Returns(projectsMockSet.Object);

            var projectYearsMockSet = RepositoryTestHelper.CreateMockDbSet(projectYears ?? new List<ProjectYear>());
            mockContext.Setup(x => x.ProjectYears).Returns(projectYearsMockSet.Object);
            mockContext.Setup(x => x.Set<ProjectYear>()).Returns(projectYearsMockSet.Object);

            var staffMockSet = RepositoryTestHelper.CreateMockDbSet(staffRequirements ?? new List<StaffRequirement>());
            mockContext.Setup(x => x.StaffRequirements).Returns(staffMockSet.Object);
            mockContext.Setup(x => x.Set<StaffRequirement>()).Returns(staffMockSet.Object);

            var testReqMockSet = RepositoryTestHelper.CreateMockDbSet(testRequirements ?? new List<TestRequirement>());
            mockContext.Setup(x => x.TestRequirements).Returns(testReqMockSet.Object);
            mockContext.Setup(x => x.Set<TestRequirement>()).Returns(testReqMockSet.Object);

            var animalMockSet = RepositoryTestHelper.CreateMockDbSet(animalRequirements ?? new List<AnimalRequirement>());
            mockContext.Setup(x => x.AnimalRequirements).Returns(animalMockSet.Object);
            mockContext.Setup(x => x.Set<AnimalRequirement>()).Returns(animalMockSet.Object);

            var additionalCostMockSet = RepositoryTestHelper.CreateMockDbSet(additionalCosts ?? new List<AdditionalCost>());
            mockContext.Setup(x => x.AdditionalCosts).Returns(additionalCostMockSet.Object);
            mockContext.Setup(x => x.Set<AdditionalCost>()).Returns(additionalCostMockSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new ProjectRepository(mockContext.Object, mockSettingsRepository.Object);
        }
        #endregion

        #region GetProjectSummaryExportDataAsync

        [Fact]
        public async Task GetProjectSummaryExportDataAsync_WithAllData_ReturnsPopulatedExportData()
        {
            // Arrange
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001", ProjectTitle = "Test Project", Inflation = 3 }
            };
            var projectYears = new List<ProjectYear>
            {
                new() { Project = "2024/001", YearValue = 2024 },
                new() { Project = "2024/001", YearValue = 2025 }
            };
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "A1", Year = 2024, Nohours = 100.0, Chargerate = 50.0 }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { Project = "2024/001", TestCode = "BLOOD", Year = 2024, UnitPrice = 15.0, NumberOfTests = 10.0 }
            };
            var animalRequirements = new List<AnimalRequirement>
            {
                new() { Project = "2024/001", AnimalType = "Mouse", Year = 2024, DailyRate = 5.0, NumberOfDays = 30.0, NumberOfAnimals = 3.0 }
            };
            var additionalCosts = new List<AdditionalCost>
            {
                new() { Project = "2024/001", Description = "Consumables", AccountCat = "CAT1", Year = 2024, ItemCost = 200.0 }
            };
            var repo = CreateRepositoryForExport(projects, projectYears, staffRequirements, testRequirements, animalRequirements, additionalCosts);

            // Act
            var result = await repo.GetProjectSummaryExportDataAsync("2024/001");

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Project);
            Assert.Equal("2024/001", result.Project.ProjectId);
            Assert.Equal(2, result.Years.Count);
            Assert.Single(result.StaffRequirements);
            Assert.Single(result.TestRequirements);
            Assert.Single(result.AnimalRequirements);
            Assert.Single(result.AdditionalCosts);
        }

        [Fact]
        public async Task GetProjectSummaryExportDataAsync_WithNoMatchingProject_ReturnsNullProject()
        {
            // Arrange — project list is empty; all child collections also empty
            var repo = CreateRepositoryForExport();

            // Act
            var result = await repo.GetProjectSummaryExportDataAsync("NONEXISTENT");

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Project);
        }

        [Fact]
        public async Task GetProjectSummaryExportDataAsync_OnlyReturnsDataForRequestedProject()
        {
            // Arrange — two projects; only data for "2024/001" should appear in results
            var projects = new List<Project>
            {
                new() { ProjectId = "2024/001" },
                new() { ProjectId = "2024/002" }
            };
            var projectYears = new List<ProjectYear>
            {
                new() { Project = "2024/001", YearValue = 2024 },
                new() { Project = "2024/002", YearValue = 2024 }
            };
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "A1", Year = 2024, Nodays = 10.0 },
                new() { Project = "2024/002", WgGrade = "B1", Year = 2024, Nodays = 10.0 }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { Project = "2024/001", TestCode = "BLOOD", Year = 2024 },
                new() { Project = "2024/002", TestCode = "URINE", Year = 2024 }
            };
            var animalRequirements = new List<AnimalRequirement>
            {
                new() { Project = "2024/001", AnimalType = "Mouse", Year = 2024 },
                new() { Project = "2024/002", AnimalType = "Rat",   Year = 2024 }
            };
            var additionalCosts = new List<AdditionalCost>
            {
                new() { Project = "2024/001", Description = "Cost A", AccountCat = "CAT1", Year = 2024 },
                new() { Project = "2024/002", Description = "Cost B", AccountCat = "CAT2", Year = 2024 }
            };
            var repo = CreateRepositoryForExport(projects, projectYears, staffRequirements, testRequirements, animalRequirements, additionalCosts);

            // Act
            var result = await repo.GetProjectSummaryExportDataAsync("2024/001");

            // Assert
            Assert.All(result.Years, y => Assert.Equal("2024/001", y.Project));
            Assert.All(result.StaffRequirements, sr => Assert.Equal("2024/001", sr.Project));
            Assert.All(result.TestRequirements, tr => Assert.Equal("2024/001", tr.Project));
            Assert.All(result.AnimalRequirements, ar => Assert.Equal("2024/001", ar.Project));
            Assert.All(result.AdditionalCosts, ac => Assert.Equal("2024/001", ac.Project));
        }

        [Fact]
        public async Task GetProjectSummaryExportDataAsync_YearsOrderedAscendingByYearValue()
        {
            // Arrange — years inserted out of order
            var projects = new List<Project> { new() { ProjectId = "2024/001" } };
            var projectYears = new List<ProjectYear>
            {
                new() { Project = "2024/001", YearValue = 2026 },
                new() { Project = "2024/001", YearValue = 2024 },
                new() { Project = "2024/001", YearValue = 2025 }
            };
            var repo = CreateRepositoryForExport(projects, projectYears);

            // Act
            var result = await repo.GetProjectSummaryExportDataAsync("2024/001");

            // Assert
            Assert.Equal(3, result.Years.Count);
            Assert.Equal(2024, result.Years[0].YearValue);
            Assert.Equal(2025, result.Years[1].YearValue);
            Assert.Equal(2026, result.Years[2].YearValue);
        }

        [Fact]
        public async Task GetProjectSummaryExportDataAsync_StaffOrderedByYearThenWgGrade()
        {
            // Arrange — staff inserted out of order
            var projects = new List<Project> { new() { ProjectId = "2024/001" } };
            var staffRequirements = new List<StaffRequirement>
            {
                new() { Project = "2024/001", WgGrade = "C1", Year = 2024, Nodays = 10.0 },
                new() { Project = "2024/001", WgGrade = "A1", Year = 2025, Nodays = 10.0 },
                new() { Project = "2024/001", WgGrade = "B1", Year = 2024, Nodays = 10.0 }
            };
            var repo = CreateRepositoryForExport(projects, staffRequirements: staffRequirements);

            // Act
            var result = await repo.GetProjectSummaryExportDataAsync("2024/001");

            // Assert
            Assert.Equal(3, result.StaffRequirements.Count);
            Assert.Equal(2024, result.StaffRequirements[0].Year);
            Assert.Equal("B1", result.StaffRequirements[0].WgGrade);
            Assert.Equal(2024, result.StaffRequirements[1].Year);
            Assert.Equal("C1", result.StaffRequirements[1].WgGrade);
            Assert.Equal(2025, result.StaffRequirements[2].Year);
            Assert.Equal("A1", result.StaffRequirements[2].WgGrade);
        }

        [Fact]
        public async Task GetProjectSummaryExportDataAsync_TestsOrderedByYearThenTestCode()
        {
            // Arrange
            var projects = new List<Project> { new() { ProjectId = "2024/001" } };
            var testRequirements = new List<TestRequirement>
            {
                new() { Project = "2024/001", TestCode = "URINE", Year = 2024 },
                new() { Project = "2024/001", TestCode = "BLOOD", Year = 2024 },
                new() { Project = "2024/001", TestCode = "BLOOD", Year = 2025 }
            };
            var repo = CreateRepositoryForExport(projects, testRequirements: testRequirements);

            // Act
            var result = await repo.GetProjectSummaryExportDataAsync("2024/001");

            // Assert
            Assert.Equal(3, result.TestRequirements.Count);
            Assert.Equal(2024, result.TestRequirements[0].Year);
            Assert.Equal("BLOOD", result.TestRequirements[0].TestCode);
            Assert.Equal(2024, result.TestRequirements[1].Year);
            Assert.Equal("URINE", result.TestRequirements[1].TestCode);
            Assert.Equal(2025, result.TestRequirements[2].Year);
        }

        [Fact]
        public async Task GetProjectSummaryExportDataAsync_AnimalsOrderedByYearThenAnimalType()
        {
            // Arrange
            var projects = new List<Project> { new() { ProjectId = "2024/001" } };
            var animalRequirements = new List<AnimalRequirement>
            {
                new() { Project = "2024/001", AnimalType = "Rat",   Year = 2024 },
                new() { Project = "2024/001", AnimalType = "Mouse", Year = 2024 },
                new() { Project = "2024/001", AnimalType = "Mouse", Year = 2025 }
            };
            var repo = CreateRepositoryForExport(projects, animalRequirements: animalRequirements);

            // Act
            var result = await repo.GetProjectSummaryExportDataAsync("2024/001");

            // Assert
            Assert.Equal(3, result.AnimalRequirements.Count);
            Assert.Equal(2024, result.AnimalRequirements[0].Year);
            Assert.Equal("Mouse", result.AnimalRequirements[0].AnimalType);
            Assert.Equal(2024, result.AnimalRequirements[1].Year);
            Assert.Equal("Rat", result.AnimalRequirements[1].AnimalType);
            Assert.Equal(2025, result.AnimalRequirements[2].Year);
        }

        [Fact]
        public async Task GetProjectSummaryExportDataAsync_AdditionalCostsOrderedByYearThenDescription()
        {
            // Arrange
            var projects = new List<Project> { new() { ProjectId = "2024/001" } };
            var additionalCosts = new List<AdditionalCost>
            {
                new() { Project = "2024/001", Description = "Travel",      AccountCat = "T", Year = 2024 },
                new() { Project = "2024/001", Description = "Consumables", AccountCat = "C", Year = 2024 },
                new() { Project = "2024/001", Description = "Travel",      AccountCat = "T", Year = 2025 }
            };
            var repo = CreateRepositoryForExport(projects, additionalCosts: additionalCosts);

            // Act
            var result = await repo.GetProjectSummaryExportDataAsync("2024/001");

            // Assert
            Assert.Equal(3, result.AdditionalCosts.Count);
            Assert.Equal(2024, result.AdditionalCosts[0].Year);
            Assert.Equal("Consumables", result.AdditionalCosts[0].Description);
            Assert.Equal(2024, result.AdditionalCosts[1].Year);
            Assert.Equal("Travel", result.AdditionalCosts[1].Description);
            Assert.Equal(2025, result.AdditionalCosts[2].Year);
        }

        [Fact]
        public async Task GetProjectSummaryExportDataAsync_WithNoChildData_ReturnsEmptyCollections()
        {
            // Arrange — project exists but no related records in any child table
            var projects = new List<Project> { new() { ProjectId = "2024/001" } };
            var repo = CreateRepositoryForExport(projects);

            // Act
            var result = await repo.GetProjectSummaryExportDataAsync("2024/001");

            // Assert
            Assert.NotNull(result.Project);
            Assert.Equal("2024/001", result.Project.ProjectId);
            Assert.Empty(result.Years);
            Assert.Empty(result.StaffRequirements);
            Assert.Empty(result.TestRequirements);
            Assert.Empty(result.AnimalRequirements);
            Assert.Empty(result.AdditionalCosts);
        }

        [Fact]
        public async Task GetProjectSummaryExportDataAsync_HandlesUrlEncodedProjectId()
        {
            // Arrange
            var projects = new List<Project> { new() { ProjectId = "2024/001" } };
            var repo = CreateRepositoryForExport(projects);
            var encodedId = HttpUtility.UrlEncode("2024/001");

            // Act
            var result = await repo.GetProjectSummaryExportDataAsync(encodedId);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Project);
            Assert.Equal("2024/001", result.Project.ProjectId);
        }

        #endregion

        #region GetProjectYearCostSummaryAsync

        private static ProjectRepository CreateRepositoryForCostSummary(
            IEnumerable<StaffRequirement>? staffRequirements = null,
            IEnumerable<TestRequirement>? testRequirements = null,
            IEnumerable<AnimalRequirement>? animalRequirements = null,
            IEnumerable<AdditionalCost>? additionalCosts = null)
        {
            var mockFPSYearContext = new Mock<IFPSYearContext>();
            var mockSettingsRepository = new Mock<ISettingsRepository>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<CostbookDbContext>(mockFPSYearContext.Object);

            var staffMockSet = RepositoryTestHelper.CreateMockDbSet(staffRequirements ?? new List<StaffRequirement>());
            mockContext.Setup(x => x.StaffRequirements).Returns(staffMockSet.Object);
            mockContext.Setup(x => x.Set<StaffRequirement>()).Returns(staffMockSet.Object);

            var testReqMockSet = RepositoryTestHelper.CreateMockDbSet(testRequirements ?? new List<TestRequirement>());
            mockContext.Setup(x => x.TestRequirements).Returns(testReqMockSet.Object);
            mockContext.Setup(x => x.Set<TestRequirement>()).Returns(testReqMockSet.Object);

            var animalMockSet = RepositoryTestHelper.CreateMockDbSet(animalRequirements ?? new List<AnimalRequirement>());
            mockContext.Setup(x => x.AnimalRequirements).Returns(animalMockSet.Object);
            mockContext.Setup(x => x.Set<AnimalRequirement>()).Returns(animalMockSet.Object);

            var additionalCostMockSet = RepositoryTestHelper.CreateMockDbSet(additionalCosts ?? new List<AdditionalCost>());
            mockContext.Setup(x => x.AdditionalCosts).Returns(additionalCostMockSet.Object);
            mockContext.Setup(x => x.Set<AdditionalCost>()).Returns(additionalCostMockSet.Object);

            var emptyProjects = RepositoryTestHelper.CreateMockDbSet(new List<Project>());
            mockContext.Setup(x => x.Projects).Returns(emptyProjects.Object);
            mockContext.Setup(x => x.Set<Project>()).Returns(emptyProjects.Object);

            var emptyProjectYears = RepositoryTestHelper.CreateMockDbSet(new List<ProjectYear>());
            mockContext.Setup(x => x.Set<ProjectYear>()).Returns(emptyProjectYears.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new ProjectRepository(mockContext.Object, mockSettingsRepository.Object);
        }

        [Fact]
        public async Task GetProjectYearCostSummaryAsync_WithAllCosts_ReturnsSummedTotals()
        {
            // Arrange — Chargerate*Nohours=500, UnitPrice*NumberOfTests=100,
            //            DailyRate*NumberOfDays*NumberOfAnimals=60, ItemCost=200
            var repo = CreateRepositoryForCostSummary(
                staffRequirements: new List<StaffRequirement>
                {
                    new() { Project = "2024/001", Year = 2024, Chargerate = 50.0, Nohours = 10.0 }
                },
                testRequirements: new List<TestRequirement>
                {
                    new() { Project = "2024/001", Year = 2024, UnitPrice = 20.0, NumberOfTests = 5.0 }
                },
                animalRequirements: new List<AnimalRequirement>
                {
                    new() { Project = "2024/001", Year = 2024, DailyRate = 4.0, NumberOfDays = 5.0, NumberOfAnimals = 3.0 }
                },
                additionalCosts: new List<AdditionalCost>
                {
                    new() { Project = "2024/001", Year = 2024, ItemCost = 200.0 }
                });

            // Act
            var result = await repo.GetProjectYearCostSummaryAsync("2024/001", 2024);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("2024/001", result.Project);
            Assert.Equal(2024,  result.Year);
            Assert.Equal(500.0, result.StaffCostTotal,      precision: 5);
            Assert.Equal(100.0, result.TestCostTotal,       precision: 5);
            Assert.Equal(60.0,  result.AnimalCostTotal,     precision: 5);
            Assert.Equal(200.0, result.AdditionalCostTotal, precision: 5);
            Assert.Equal(860.0, result.GrandTotal,          precision: 5);
        }

        [Fact]
        public async Task GetProjectYearCostSummaryAsync_WithNoData_ReturnsZeroTotals()
        {
            // Arrange — all child tables empty
            var repo = CreateRepositoryForCostSummary();

            // Act
            var result = await repo.GetProjectYearCostSummaryAsync("2024/001", 2024);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0.0, result.StaffCostTotal);
            Assert.Equal(0.0, result.TestCostTotal);
            Assert.Equal(0.0, result.AnimalCostTotal);
            Assert.Equal(0.0, result.AdditionalCostTotal);
            Assert.Equal(0.0, result.GrandTotal);
        }

        [Fact]
        public async Task GetProjectYearCostSummaryAsync_OnlyIncludesRecordsForRequestedYear()
        {
            // Arrange — two years; only year 2024 should contribute to the totals
            var repo = CreateRepositoryForCostSummary(
                staffRequirements: new List<StaffRequirement>
                {
                    new() { Project = "2024/001", Year = 2024, Chargerate = 100.0, Nohours = 10.0 },
                    new() { Project = "2024/001", Year = 2025, Chargerate = 999.0, Nohours = 99.0 }
                });

            // Act
            var result = await repo.GetProjectYearCostSummaryAsync("2024/001", 2024);

            // Assert
            Assert.Equal(1000.0, result.StaffCostTotal, precision: 5);
        }

        [Fact]
        public async Task GetProjectYearCostSummaryAsync_OnlyIncludesRecordsForRequestedProject()
        {
            // Arrange — two projects; only "2024/001" data should be summed
            var repo = CreateRepositoryForCostSummary(
                staffRequirements: new List<StaffRequirement>
                {
                    new() { Project = "2024/001", Year = 2024, Chargerate = 50.0, Nohours = 4.0 },
                    new() { Project = "2024/002", Year = 2024, Chargerate = 999.0, Nohours = 99.0 }
                });

            // Act
            var result = await repo.GetProjectYearCostSummaryAsync("2024/001", 2024);

            // Assert
            Assert.Equal(200.0, result.StaffCostTotal, precision: 5);
        }

        [Fact]
        public async Task GetProjectYearCostSummaryAsync_HandlesUrlEncodedProjectId()
        {
            // Arrange
            var repo = CreateRepositoryForCostSummary(
                staffRequirements: new List<StaffRequirement>
                {
                    new() { Project = "2024/001", Year = 2024, Chargerate = 10.0, Nohours = 10.0 }
                });
            var encodedId = HttpUtility.UrlEncode("2024/001");

            // Act
            var result = await repo.GetProjectYearCostSummaryAsync(encodedId, 2024);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("2024/001", result.Project);
            Assert.Equal(100.0, result.StaffCostTotal, precision: 5);
        }

        [Fact]
        public async Task GetProjectYearCostSummaryAsync_StaffWithNullChargerateOrNohours_ContributesZero()
        {
            // Arrange — null fields should be treated as 0 per the SumAsync expression
            var repo = CreateRepositoryForCostSummary(
                staffRequirements: new List<StaffRequirement>
                {
                    new() { Project = "2024/001", Year = 2024, Chargerate = null, Nohours = 10.0 },
                    new() { Project = "2024/001", Year = 2024, Chargerate = 50.0, Nohours = null }
                });

            // Act
            var result = await repo.GetProjectYearCostSummaryAsync("2024/001", 2024);

            // Assert
            Assert.Equal(0.0, result.StaffCostTotal, precision: 5);
        }

        [Fact]
        public async Task GetProjectYearCostSummaryAsync_MultipleRowsPerCategory_SumsCorrectly()
        {
            // Arrange — two staff rows for the same project/year should be summed
            var repo = CreateRepositoryForCostSummary(
                staffRequirements: new List<StaffRequirement>
                {
                    new() { Project = "2024/001", Year = 2024, Chargerate = 50.0, Nohours = 8.0 },
                    new() { Project = "2024/001", Year = 2024, Chargerate = 25.0, Nohours = 4.0 }
                },
                additionalCosts: new List<AdditionalCost>
                {
                    new() { Project = "2024/001", Year = 2024, ItemCost = 100.0 },
                    new() { Project = "2024/001", Year = 2024, ItemCost = 150.0 }
                });

            // Act
            var result = await repo.GetProjectYearCostSummaryAsync("2024/001", 2024);

            // Assert — 50*8 + 25*4 = 500
            Assert.Equal(500.0, result.StaffCostTotal,      precision: 5);
            Assert.Equal(250.0, result.AdditionalCostTotal, precision: 5);
        }

        #endregion
    }
}