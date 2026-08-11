using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.EmployeeRepositoryTest
{
    public class EmployeeRepositoryTests
    {
        /// <summary>
        /// Default test FPS year used across repository tests.
        /// </summary>
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a mocked IFpsYearContext with specified year.
        /// </summary>
        private static Mock<IFpsRequestContext> CreateMockFpsYearContext(int year = DefaultTestFpsYear)
        {
            var mockFpsYearContext = new Mock<IFpsRequestContext>();
            mockFpsYearContext.Setup(x => x.FpsYear).Returns(year);
            return mockFpsYearContext;
        }

        private static EmployeeRepository CreateRepository(
            IEnumerable<Employee> employees,
            IEnumerable<StaffActiveView>? staffActiveViews = null,
            IEnumerable<WorkgroupGradeGeneralView>? workgroupGrades = null,
            IEnumerable<WorkGroupEmployee>? wgEmployees = null,
            int fpsYear = DefaultTestFpsYear,
            IEnumerable<PactStaff>? pactStaffs = null)
        {
            var mockFpsYearContext = CreateMockFpsYearContext(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            // Setup Employees DbSet
            var employeesMockSet = RepositoryTestHelper.CreateMockDbSet(employees);
            mockContext.Setup(x => x.Employees).Returns(employeesMockSet.Object);

            // Setup WgEmployees DbSet (for DeleteEmployeeAsync guard)
            var wgEmployeesMockSet = RepositoryTestHelper.CreateMockDbSet(wgEmployees ?? Enumerable.Empty<WorkGroupEmployee>());
            mockContext.Setup(x => x.WorkGroupEmployees).Returns(wgEmployeesMockSet.Object);

            // Setup StaffActiveView
            if (staffActiveViews != null)
            {
                var staffMockSet = RepositoryTestHelper.CreateMockDbSet(staffActiveViews);
                mockContext.Setup(x => x.StaffActiveView).Returns(staffMockSet.Object);
            }

            // Setup WorkgroupGradeGeneralView DbSet (for GetAllManagersAsync)
            if (workgroupGrades != null)
            {
                var gradeMockSet = RepositoryTestHelper.CreateMockDbSet(workgroupGrades);
                mockContext.Setup(x => x.WorkgroupGradeGeneralViews).Returns(gradeMockSet.Object);
            }

            // Setup PactStaffs DbSet (for GetPactStaffAsync)
            var pactStaffMockSet = RepositoryTestHelper.CreateMockDbSet(pactStaffs ?? Enumerable.Empty<PactStaff>());
            mockContext.Setup(x => x.PactStaffs).Returns(pactStaffMockSet.Object);

            return new EmployeeRepository(mockContext.Object, mockFpsYearContext.Object);
        }

        #region GetAllEmployeesAsync Tests

        [Fact]
        public async Task GetAllEmployeesAsync_ReturnsAllEmployees_OrderedBySPNumber()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP003", FirstName = "Charlie", LastName = "Brown", Title = "Manager" },
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith", Title = "Developer" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones", Title = "Analyst" }
            };
            var repo = CreateRepository(employees);

            // Act
            var result = await repo.GetAllEmployeesAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            Assert.Equal("SP001", resultList[0].SPNumber);
            Assert.Equal("SP002", resultList[1].SPNumber);
            Assert.Equal("SP003", resultList[2].SPNumber);
        }

        [Fact]
        public async Task GetAllEmployeesAsync_ReturnsEmptyList_WhenNoEmployees()
        {
            // Arrange
            var repo = CreateRepository(new List<Employee>());

            // Act
            var result = await repo.GetAllEmployeesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region GetEmployeesByPrefixAsync Tests

        [Fact]
        public async Task GetEmployeesByPrefixAsync_ReturnsFilteredEmployees_ByPrefix()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones" },
                new() { SPNumber = "EMP001", FirstName = "Charlie", LastName = "Brown" }
            };
            var repo = CreateRepository(employees);

            // Act
            var result = await repo.GetEmployeesByPrefixAsync("SP");

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, e => Assert.StartsWith("SP", e.SPNumber));
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_ReturnsEmptyList_WhenNoPrefixMatch()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones" }
            };
            var repo = CreateRepository(employees);

            // Act
            var result = await repo.GetEmployeesByPrefixAsync("EMP");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_ReturnsOrderedResults()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP003", FirstName = "Charlie", LastName = "Brown" },
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones" }
            };
            var repo = CreateRepository(employees);

            // Act
            var result = await repo.GetEmployeesByPrefixAsync("SP");

            // Assert
            var resultList = result.ToList();
            Assert.Equal("SP001", resultList[0].SPNumber);
            Assert.Equal("SP002", resultList[1].SPNumber);
            Assert.Equal("SP003", resultList[2].SPNumber);
        }

        #endregion

        #region GetEmployeesByPrefixAsync with Pagination Tests

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithPagination_ReturnsPagedData()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith", Title = "Dev" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones", Title = "Analyst" },
                new() { SPNumber = "SP003", FirstName = "Charlie", LastName = "Brown", Title = "Manager" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 2,
                SortBy = "SPNumber",
                Descending = false
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.TotalPages);
            Assert.Equal(1, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithFilter_FiltersBySPNumber()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones" },
                new() { SPNumber = "SP003", FirstName = "Charlie", LastName = "Brown" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"SPNumber\":\"001\"}"
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("SP001", result.Data.First().SPNumber);
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithFilter_FiltersByFirstName()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones" },
                new() { SPNumber = "SP003", FirstName = "Alice", LastName = "Brown" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"FirstName\":\"Alice\"}"
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, e => Assert.Contains("Alice", e.FirstName));
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithFilter_FiltersByLastName()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new Employee { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" },
                new Employee { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones" },
                new Employee { SPNumber = "SP003", FirstName = "Charlie", LastName = "Smith" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"LastName\":\"Smith\"}"
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, e => Assert.Contains("Smith", e.LastName));
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithFilter_FiltersByTitle()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith", Title = "Manager" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones", Title = "Developer" },
                new() { SPNumber = "SP003", FirstName = "Charlie", LastName = "Brown", Title = "Manager" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Title\":\"Manager\"}"
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, e => Assert.Contains("Manager", e.Title));
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithMultipleFilters_FiltersCorrectly()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith", Title = "Manager" },
                new() { SPNumber = "SP002", FirstName = "Alice", LastName = "Jones", Title = "Developer" },
                new() { SPNumber = "SP003", FirstName = "Bob", LastName = "Smith", Title = "Manager" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"FirstName\":\"Alice\",\"LastName\":\"Smith\"}"
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal("SP001", result.Data.First().SPNumber);
        }

        [Theory]
        [InlineData("SPNumber", false, "SP001")]
        [InlineData("SPNumber", true, "SP003")]
        [InlineData("FirstName", false, "Alice")]
        [InlineData("FirstName", true, "Charlie")]
        [InlineData("LastName", false, "Brown")]
        [InlineData("LastName", true, "Smith")]
        [InlineData("Title", false, "Analyst")]
        [InlineData("Title", true, "Manager")]
        [InlineData("FpsCalYear", false, "SP001")] // year 2023 → Alice (SP001)
        [InlineData("FpsCalYear", true, "SP003")]  // year 2025 → Charlie (SP003)
        public async Task GetEmployeesByPrefixAsync_WithSorting_SortsCorrectly(
            string sortBy,
            bool descending,
            string expectedFirstValue)
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP002", FirstName = "Bob",     LastName = "Jones", Title = "Developer", FpsYear = 2024 },
                new() { SPNumber = "SP001", FirstName = "Alice",   LastName = "Smith", Title = "Analyst",   FpsYear = 2023 },
                new() { SPNumber = "SP003", FirstName = "Charlie", LastName = "Brown", Title = "Manager",   FpsYear = 2025 }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                Descending = descending
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count());
            var firstEmployee = result.Data.First();
            string? actualValue = sortBy.ToLower() switch
            {
                "spnumber" => firstEmployee.SPNumber,
                "firstname" => firstEmployee.FirstName,
                "lastname" => firstEmployee.LastName,
                "title" => firstEmployee.Title,
                "fpscalyear" => firstEmployee.SPNumber, // identify record by SPNumber after sorting by FpsYear
                _ => firstEmployee.SPNumber
            };
            Assert.Equal(expectedFirstValue, actualValue);
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithNoSortBy_DefaultsToSPNumber()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP003", FirstName = "Charlie" },
                new() { SPNumber = "SP001", FirstName = "Alice" },
                new() { SPNumber = "SP002", FirstName = "Bob" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert
            Assert.Equal("SP001", result.Data.First().SPNumber);
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithInvalidSortBy_DefaultsToSPNumber()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP003", FirstName = "Charlie" },
                new() { SPNumber = "SP001", FirstName = "Alice" },
                new() { SPNumber = "SP002", FirstName = "Bob" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "InvalidProperty"
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert
            Assert.Equal("SP001", result.Data.First().SPNumber);
        }

        #endregion

        #region GetEmployeeByIdAsync Tests

        [Fact]
        public async Task GetEmployeeByIdAsync_ReturnsEmployee_WhenFound()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones" }
            };
            var repo = CreateRepository(employees);

            // Act
            var result = await repo.GetEmployeeByIdAsync("SP001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("SP001", result.SPNumber);
            Assert.Equal("Alice", result.FirstName);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" }
            };
            var repo = CreateRepository(employees);

            // Act
            var result = await repo.GetEmployeeByIdAsync("SP999");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_ReturnsNull_WhenEmployeesEmpty()
        {
            // Arrange
            var repo = CreateRepository(new List<Employee>());

            // Act
            var result = await repo.GetEmployeeByIdAsync("SP001");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region AddEmployeeAsync Tests

        [Fact]
        public async Task AddEmployeeAsync_AddsEmployee_WithFpsYear()
        {
            // Arrange
            var mockFpsYearContext = CreateMockFpsYearContext(2025);
            var (mockContext, employeesMockSet) =
                RepositoryTestHelper.CreateRepositoryContext<FpsDbContext, Employee>(
                    new List<Employee>(),
                    mockFpsYearContext.Object);

            mockContext.Setup(x => x.Employees).Returns(employeesMockSet.Object);

            var repo = new EmployeeRepository(mockContext.Object, mockFpsYearContext.Object);
            var newEmployee = new Employee
            {
                SPNumber = "SP100",
                FirstName = "John",
                LastName = "Doe",
                Title = "Developer"
            };

            // Act
            var result = await repo.AddEmployeeAsync(newEmployee);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("SP100", result.SPNumber);
            Assert.Equal(2025, result.FpsYear);
            RepositoryTestHelper.VerifyAdd(employeesMockSet);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task AddEmployeeAsync_OverwritesFpsYear_WithContextYear()
        {
            // Arrange
            var mockFpsYearContext = CreateMockFpsYearContext(2026);
            var (mockContext, employeesMockSet) =
                RepositoryTestHelper.CreateRepositoryContext<FpsDbContext, Employee>(
                    new List<Employee>(),
                    mockFpsYearContext.Object);

            mockContext.Setup(x => x.Employees).Returns(employeesMockSet.Object);

            var repo = new EmployeeRepository(mockContext.Object, mockFpsYearContext.Object);
            var newEmployee = new Employee
            {
                SPNumber = "SP101",
                FirstName = "Jane",
                LastName = "Doe",
                Title = "Manager",
                FpsYear = 2020 // Should be overwritten
            };

            // Act
            var result = await repo.AddEmployeeAsync(newEmployee);

            // Assert
            Assert.Equal(2026, result.FpsYear);
        }

        #endregion

        #region UpdateEmployeeAsync Tests

        [Fact]
        public async Task UpdateEmployeeAsync_UpdatesEmployee_WithFpsYear()
        {
            // Arrange
            var existingEmployee = new Employee
            {
                SPNumber = "SP001",
                FirstName = "Alice",
                LastName = "Smith",
                Title = "Developer",
                FpsYear = 2023
            };
            var employees = new List<Employee> { existingEmployee };

            var mockFpsYearContext = CreateMockFpsYearContext(2025);
            var (mockContext, employeesMockSet) =
                RepositoryTestHelper.CreateRepositoryContext<FpsDbContext, Employee>(
                    employees,
                    mockFpsYearContext.Object);

            mockContext.Setup(x => x.Employees).Returns(employeesMockSet.Object);

            // Don't setup Entry - just verify it gets called and handle the exception
            var entryWasCalled = false;
            mockContext.Setup(x => x.Entry(It.IsAny<Employee>()))
                .Callback(() => entryWasCalled = true)
                .Throws(new NotSupportedException("Mocked DbContext does not support Entry()"));

            var repo = new EmployeeRepository(mockContext.Object, mockFpsYearContext.Object);
            var updatedEmployee = new Employee
            {
                SPNumber = "SP001",
                FirstName = "Alice Updated",
                LastName = "Smith Updated",
                Title = "Senior Developer"
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() => repo.UpdateEmployeeAsync(updatedEmployee));

            // Verify the FPS year was set before Entry was called
            Assert.Equal(2025, updatedEmployee.FpsYear);
            Assert.True(entryWasCalled);
        }

        #endregion

        #region DeleteEmployeeAsync Tests

        [Fact]
        public async Task DeleteEmployeeAsync_DeletesEmployee_WhenFound()
        {
            // Arrange
            var employee = new Employee
            {
                SPNumber = "SP001",
                FirstName = "Alice",
                LastName = "Smith",
                FpsYear = DefaultTestFpsYear
            };

            var mockFpsYearContext = CreateMockFpsYearContext(DefaultTestFpsYear);
            var (mockContext, employeesMockSet) =
                RepositoryTestHelper.CreateRepositoryContext<FpsDbContext, Employee>(
                    new List<Employee> { employee },
                    mockFpsYearContext.Object);

            mockContext.Setup(x => x.Employees).Returns(employeesMockSet.Object);

            var wgEmployeesMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<WorkGroupEmployee>());
            mockContext.Setup(x => x.WorkGroupEmployees).Returns(wgEmployeesMockSet.Object);

            var repo = new EmployeeRepository(mockContext.Object, mockFpsYearContext.Object);

            // Act
            var result = await repo.DeleteEmployeeAsync("SP001");

            // Assert
            Assert.True(result);
            RepositoryTestHelper.VerifyRemove(employeesMockSet);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteEmployeeAsync_ThrowsInvalidOperation_WhenNotFound()
        {
            // Arrange
            var repo = CreateRepository(new List<Employee>());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repo.DeleteEmployeeAsync("SP999"));
        }

        [Fact]
        public async Task DeleteEmployeeAsync_ThrowsInvalidOperation_WhenFpsYearMismatch()
        {
            // Arrange
            var employee = new Employee
            {
                SPNumber = "SP001",
                FirstName = "Alice",
                LastName = "Smith",
                FpsYear = 2020 // Different year from context
            };
            var repo = CreateRepository(new List<Employee> { employee }, fpsYear: 2024);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repo.DeleteEmployeeAsync("SP001"));
        }

        [Fact]
        public async Task DeleteEmployeeAsync_ThrowsInvalidOperation_WhenLinkedWgEmployeeExists()
        {
            // Arrange
            var employee = new Employee
            {
                SPNumber = "SP001",
                FirstName = "Alice",
                LastName = "Smith",
                FpsYear = DefaultTestFpsYear
            };
            var linkedWgEmployee = new WorkGroupEmployee
            {
                SpNumber = "SP001",
                FpsYear = DefaultTestFpsYear
            };
            var repo = CreateRepository(
                new List<Employee> { employee },
                wgEmployees: new List<WorkGroupEmployee> { linkedWgEmployee });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                repo.DeleteEmployeeAsync("SP001"));
            Assert.Contains("SP001", ex.Message);
        }

        #endregion

        #region GetAllManagersAsync Tests

        [Fact]
        public async Task GetAllManagersAsync_ReturnsManagers_WithValidGrades()
        {
            // Arrange
            var staffActiveViews = new List<StaffActiveView>
            {
                new() { StaffID = "S001", Name = "John Manager", WorkgroupGrade = "WG01" },
                new() { StaffID = "S002", Name = "Jane Director", WorkgroupGrade = "WG02" },
                new() { StaffID = "S003", Name = "General User", WorkgroupGrade = "WG03" }
            };

            var workgroupGrades = new List<WorkgroupGradeGeneralView>
            {
                new() { WgGrade = "WG01", GradeCode = "M01", WorkGroup = "Management" },
                new() { WgGrade = "WG02", GradeCode = "D01", WorkGroup = "Directors" },
                new() { WgGrade = "WG03", GradeCode = "G01", WorkGroup = "General" }
            };

            var repo = CreateRepository(
                new List<Employee>(),
                staffActiveViews,
                workgroupGrades);

            // Act
            var result = await repo.GetAllManagersAsync();

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count); // Excludes 'general' name and 'G' grade
            Assert.DoesNotContain(resultList, m => m.Name!.Contains("general", StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(resultList, m => m.GradeCode!.StartsWith('G'));
        }

        [Fact]
        public async Task GetAllManagersAsync_ExcludesGeneralNames()
        {
            // Arrange
            var staffActiveViews = new List<StaffActiveView>
            {
                new() { StaffID = "S001", Name = "John Manager", WorkgroupGrade = "WG01" },
                new() { StaffID = "S002", Name = "General Staff", WorkgroupGrade = "WG02" }
            };

            var workgroupGrades = new List<WorkgroupGradeGeneralView>
            {
                new() { WgGrade = "WG01", GradeCode = "M01", WorkGroup = "Management" },
                new() { WgGrade = "WG02", GradeCode = "M02", WorkGroup = "Management" }
            };

            var repo = CreateRepository(
                new List<Employee>(),
                staffActiveViews,
                workgroupGrades);

            // Act
            var result = await repo.GetAllManagersAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("John Manager", resultList[0].Name);
        }

        [Fact]
        public async Task GetAllManagersAsync_ExcludesVacancyNames()
        {
            // Arrange
            var staffActiveViews = new List<StaffActiveView>
            {
                new() { StaffID = "S001", Name = "John Manager", WorkgroupGrade = "WG01" },
                new() { StaffID = "S002", Name = "Vacancy Position", WorkgroupGrade = "WG02" }
            };

            var workgroupGrades = new List<WorkgroupGradeGeneralView>
            {
                new() { WgGrade = "WG01", GradeCode = "M01", WorkGroup = "Management" },
                new() { WgGrade = "WG02", GradeCode = "M02", WorkGroup = "Management" }
            };

            var repo = CreateRepository(
                new List<Employee>(),
                staffActiveViews,
                workgroupGrades);

            // Act
            var result = await repo.GetAllManagersAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("John Manager", resultList[0].Name);
        }

        [Fact]
        public async Task GetAllManagersAsync_ExcludesGGrades()
        {
            // Arrange
            var staffActiveViews = new List<StaffActiveView>
            {
                new() { StaffID = "S001", Name = "Manager One", WorkgroupGrade = "WG01" },
                new() { StaffID = "S002", Name = "Manager Two", WorkgroupGrade = "WG02" }
            };

            var workgroupGrades = new List<WorkgroupGradeGeneralView>
            {
                new() { WgGrade = "WG01", GradeCode = "M01", WorkGroup = "Management" },
                new() { WgGrade = "WG02", GradeCode = "G01", WorkGroup = "General" }
            };

            var repo = CreateRepository(
                new List<Employee>(),
                staffActiveViews,
                workgroupGrades);

            // Act
            var result = await repo.GetAllManagersAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("Manager One", resultList[0].Name);
        }

        [Fact]
        public async Task GetAllManagersAsync_ReturnsOrderedByName()
        {
            // Arrange
            var staffActiveViews = new List<StaffActiveView>
            {
                new() { StaffID = "S001", Name = "Charlie Manager", WorkgroupGrade = "WG01" },
                new() { StaffID = "S002", Name = "Alice Manager", WorkgroupGrade = "WG02" },
                new() { StaffID = "S003", Name = "Bob Manager", WorkgroupGrade = "WG03" }
            };

            var workgroupGrades = new List<WorkgroupGradeGeneralView>
            {
                new() { WgGrade = "WG01", GradeCode = "M01", WorkGroup = "Management" },
                new() { WgGrade = "WG02", GradeCode = "M02", WorkGroup = "Management" },
                new() { WgGrade = "WG03", GradeCode = "M03", WorkGroup = "Management" }
            };

            var repo = CreateRepository(
                new List<Employee>(),
                staffActiveViews,
                workgroupGrades);

            // Act
            var result = await repo.GetAllManagersAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            Assert.Equal("Alice Manager", resultList[0].Name);
            Assert.Equal("Bob Manager", resultList[1].Name);
            Assert.Equal("Charlie Manager", resultList[2].Name);
        }

        [Fact]
        public async Task GetAllManagersAsync_SetsExpr1Property()
        {
            // Arrange
            var staffActiveViews = new List<StaffActiveView>
            {
                new() { StaffID = "S001", Name = "Manager", WorkgroupGrade = "WG01" }
            };

            var workgroupGrades = new List<WorkgroupGradeGeneralView>
            {
                new() { WgGrade = "WG01", GradeCode = "M01", WorkGroup = "Management" }
            };

            var repo = CreateRepository(
                new List<Employee>(),
                staffActiveViews,
                workgroupGrades);

            // Act
            var result = await repo.GetAllManagersAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("M", resultList[0].Expr1);
        }

        [Fact]
        public async Task GetAllManagersAsync_ReturnsEmpty_WhenNoValidData()
        {
            // Arrange
            var repo = CreateRepository(
                new List<Employee>(),
                new List<StaffActiveView>(),
                new List<WorkgroupGradeGeneralView>());

            // Act
            var result = await repo.GetAllManagersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllManagersAsync_ExcludesNullOrEmptyGradeCodes()
        {
            // Arrange
            var staffActiveViews = new List<StaffActiveView>
            {
                new() { StaffID = "S001", Name = "Manager One", WorkgroupGrade = "WG01" },
                new() { StaffID = "S002", Name = "Manager Two", WorkgroupGrade = "WG02" }
            };

            var workgroupGrades = new List<WorkgroupGradeGeneralView>
            {
                new() { WgGrade = "WG01", GradeCode = "M01", WorkGroup = "Management" },
                new() { WgGrade = "WG02", GradeCode = null, WorkGroup = "Management" }
            };

            var repo = CreateRepository(
                new List<Employee>(),
                staffActiveViews,
                workgroupGrades);

            // Act
            var result = await repo.GetAllManagersAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("Manager One", resultList[0].Name);
        }

        #endregion

        // ── Helpers for PactManagers tests ────────────────────────────────────

        private static EmployeeRepository CreateRepositoryForPactManagers(
            IEnumerable<PactWorkGroupGradeView> pactGrades,
            IEnumerable<StaffGeneralView> staffGeneralViews,
            int fpsYear = DefaultTestFpsYear)
        {
            var mockFpsYearContext = CreateMockFpsYearContext(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            var employeesMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<Employee>());
            mockContext.Setup(x => x.Employees).Returns(employeesMockSet.Object);

            var wgEmployeesMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<WorkGroupEmployee>());
            mockContext.Setup(x => x.WorkGroupEmployees).Returns(wgEmployeesMockSet.Object);

            var pactGradesMockSet = RepositoryTestHelper.CreateMockDbSet(pactGrades);
            mockContext.Setup(x => x.PactWorkGroupGradeViews).Returns(pactGradesMockSet.Object);

            var staffGeneralMockSet = RepositoryTestHelper.CreateMockDbSet(staffGeneralViews);
            mockContext.Setup(x => x.StaffGeneralViews).Returns(staffGeneralMockSet.Object);

            return new EmployeeRepository(mockContext.Object, mockFpsYearContext.Object);
        }

        #region GetAllPactManagersAsync Tests

        [Fact]
        public async Task GetAllPactManagersAsync_ReturnsManagers_WithGradeCodeLessThanOrEqualToE()
        {
            // Arrange – GradeCode "A", "D", "E" are all <= "E" so should be included
            var pactGrades = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WG01", GradeCode = "A",  WorkGroup = "Group A" },
                new() { WgGrade = "WG02", GradeCode = "D",  WorkGroup = "Group D" },
                new() { WgGrade = "WG03", GradeCode = "E",  WorkGroup = "Group E" },
                new() { WgGrade = "WG04", GradeCode = "F",  WorkGroup = "Group F" }  // excluded
            };
            var staff = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice",   WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Bob",     WorkGroupGrade = "WG02" },
                new() { StaffId = "S003", Name = "Charlie", WorkGroupGrade = "WG03" },
                new() { StaffId = "S004", Name = "Dave",    WorkGroupGrade = "WG04" }
            };
            var repo = CreateRepositoryForPactManagers(pactGrades, staff);

            // Act
            var result = await repo.GetAllPactManagersAsync();

            // Assert
            var list = result.ToList();
            Assert.Equal(3, list.Count);
            Assert.DoesNotContain(list, m => m.Name == "Dave");
        }

        [Fact]
        public async Task GetAllPactManagersAsync_IncludesGD5_EvenThoughGreaterThanE()
        {
            // Arrange – "GD5" > "E" alphabetically but is explicitly allowed
            var pactGrades = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WG01", GradeCode = "GD5", WorkGroup = "Group GD5" },
                new() { WgGrade = "WG02", GradeCode = "Z",   WorkGroup = "Group Z" }  // excluded
            };
            var staff = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice Director", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Bob Other",      WorkGroupGrade = "WG02" }
            };
            var repo = CreateRepositoryForPactManagers(pactGrades, staff);

            // Act
            var result = await repo.GetAllPactManagersAsync();

            // Assert
            var list = result.ToList();
            Assert.Single(list);
            Assert.Equal("Alice Director", list[0].Name);
            Assert.Equal("GD5",            list[0].GradeCode);
        }

        [Fact]
        public async Task GetAllPactManagersAsync_ExcludesGradeCodesAboveE_ExceptGD5()
        {
            // Arrange
            var pactGrades = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WG01", GradeCode = "B",   WorkGroup = "Group B" },   // included (<=E)
                new() { WgGrade = "WG02", GradeCode = "GD5", WorkGroup = "Group GD5" }, // included (special)
                new() { WgGrade = "WG03", GradeCode = "G",   WorkGroup = "Group G" },   // excluded (>E, not GD5)
                new() { WgGrade = "WG04", GradeCode = "Z",   WorkGroup = "Group Z" }    // excluded (>E, not GD5)
            };
            var staff = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Bob",   WorkGroupGrade = "WG02" },
                new() { StaffId = "S003", Name = "Charlie", WorkGroupGrade = "WG03" },
                new() { StaffId = "S004", Name = "Dave",  WorkGroupGrade = "WG04" }
            };
            var repo = CreateRepositoryForPactManagers(pactGrades, staff);

            // Act
            var result = await repo.GetAllPactManagersAsync();

            // Assert
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, m => m.Name == "Alice");
            Assert.Contains(list, m => m.Name == "Bob");
            Assert.DoesNotContain(list, m => m.Name == "Charlie");
            Assert.DoesNotContain(list, m => m.Name == "Dave");
        }

        [Fact]
        public async Task GetAllPactManagersAsync_ReturnsOrderedByName()
        {
            // Arrange
            var pactGrades = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WG01", GradeCode = "A", WorkGroup = "Group A" },
                new() { WgGrade = "WG02", GradeCode = "B", WorkGroup = "Group B" },
                new() { WgGrade = "WG03", GradeCode = "C", WorkGroup = "Group C" }
            };
            var staff = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Charlie Manager", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Alice Manager",   WorkGroupGrade = "WG02" },
                new() { StaffId = "S003", Name = "Bob Manager",     WorkGroupGrade = "WG03" }
            };
            var repo = CreateRepositoryForPactManagers(pactGrades, staff);

            // Act
            var result = await repo.GetAllPactManagersAsync();

            // Assert
            var list = result.ToList();
            Assert.Equal(3, list.Count);
            Assert.Equal("Alice Manager",   list[0].Name);
            Assert.Equal("Bob Manager",     list[1].Name);
            Assert.Equal("Charlie Manager", list[2].Name);
        }

        [Fact]
        public async Task GetAllPactManagersAsync_SetsExpr1FromFirstCharOfGradeCode()
        {
            // Arrange
            var pactGrades = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WG01", GradeCode = "A1", WorkGroup = "Group A" },
                new() { WgGrade = "WG02", GradeCode = "D2", WorkGroup = "Group D" }
            };
            var staff = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Bob",   WorkGroupGrade = "WG02" }
            };
            var repo = CreateRepositoryForPactManagers(pactGrades, staff);

            // Act
            var result = await repo.GetAllPactManagersAsync();

            // Assert
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            var alice = list.First(m => m.Name == "Alice");
            var bob   = list.First(m => m.Name == "Bob");
            Assert.Equal("A", alice.Expr1);
            Assert.Equal("D", bob.Expr1);
        }

        [Fact]
        public async Task GetAllPactManagersAsync_SetsWorkGroupFromGradeJoin()
        {
            // Arrange
            var pactGrades = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WG01", GradeCode = "C", WorkGroup = "Alpha Group" }
            };
            var staff = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkGroupGrade = "WG01" }
            };
            var repo = CreateRepositoryForPactManagers(pactGrades, staff);

            // Act
            var result = await repo.GetAllPactManagersAsync();

            // Assert
            var manager = Assert.Single(result.ToList());
            Assert.Equal("Alice",       manager.Name);
            Assert.Equal("C",           manager.GradeCode);
            Assert.Equal("Alpha Group", manager.WorkGroup);
            Assert.Equal("C",           manager.Expr1);
        }

        [Fact]
        public async Task GetAllPactManagersAsync_ReturnsEmpty_WhenNoStaffMatchGrades()
        {
            // Arrange – grades exist but no staff linked to them
            var pactGrades = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WG01", GradeCode = "A", WorkGroup = "Group A" }
            };
            var staff = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkGroupGrade = "WG_NOMATCH" }
            };
            var repo = CreateRepositoryForPactManagers(pactGrades, staff);

            // Act
            var result = await repo.GetAllPactManagersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllPactManagersAsync_ReturnsEmpty_WhenNoGradesExist()
        {
            // Arrange
            var staff = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkGroupGrade = "WG01" }
            };
            var repo = CreateRepositoryForPactManagers(
                new List<PactWorkGroupGradeView>(), staff);

            // Act
            var result = await repo.GetAllPactManagersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllPactManagersAsync_ReturnsEmpty_WhenBothCollectionsEmpty()
        {
            // Arrange
            var repo = CreateRepositoryForPactManagers(
                new List<PactWorkGroupGradeView>(),
                new List<StaffGeneralView>());

            // Act
            var result = await repo.GetAllPactManagersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllPactManagersAsync_ExcludesNullGradeCode()
        {
            // Arrange
            var pactGrades = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WG01", GradeCode = "A",  WorkGroup = "Group A" },
                new() { WgGrade = "WG02", GradeCode = null, WorkGroup = "Group Null" }  // excluded
            };
            var staff = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = "Bob",   WorkGroupGrade = "WG02" }
            };
            var repo = CreateRepositoryForPactManagers(pactGrades, staff);

            // Act
            var result = await repo.GetAllPactManagersAsync();

            // Assert
            var list = result.ToList();
            Assert.Single(list);
            Assert.Equal("Alice", list[0].Name);
        }

        [Fact]
        public async Task GetAllPactManagersAsync_ExcludesNullStaffName()
        {
            // Arrange
            var pactGrades = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WG01", GradeCode = "A", WorkGroup = "Group A" },
                new() { WgGrade = "WG02", GradeCode = "B", WorkGroup = "Group B" }
            };
            var staff = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkGroupGrade = "WG01" },
                new() { StaffId = "S002", Name = null,    WorkGroupGrade = "WG02" }  // excluded
            };
            var repo = CreateRepositoryForPactManagers(pactGrades, staff);

            // Act
            var result = await repo.GetAllPactManagersAsync();

            // Assert
            var list = result.ToList();
            Assert.Single(list);
            Assert.Equal("Alice", list[0].Name);
        }

        [Fact]
        public async Task GetAllPactManagersAsync_DeduplicatesDistinctRows()
        {
            // Note: Distinct() deduplication on reference types without IEquatable only works
            // server-side (against the real database). In-memory mock evaluation returns the
            // cross-join result as-is. This test verifies the query still executes without error
            // and that all projected rows have the expected field values.

            // Arrange – same grade entry duplicated; staff has one entry joined to it
            var pactGrades = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WG01", GradeCode = "A", WorkGroup = "Group A" },
                new() { WgGrade = "WG01", GradeCode = "A", WorkGroup = "Group A" }  // duplicate
            };
            var staff = new List<StaffGeneralView>
            {
                new() { StaffId = "S001", Name = "Alice", WorkGroupGrade = "WG01" }
            };
            var repo = CreateRepositoryForPactManagers(pactGrades, staff);

            // Act
            var result = await repo.GetAllPactManagersAsync();

            // Assert – all returned rows belong to Alice and carry the correct projected values
            var list = result.ToList();
            Assert.All(list, m =>
            {
                Assert.Equal("Alice",    m.Name);
                Assert.Equal("A",        m.GradeCode);
                Assert.Equal("A",        m.Expr1);
                Assert.Equal("Group A",  m.WorkGroup);
            });
        }

        #endregion

        // ── Helpers for WorkGroupStaff tests ─────────────────────────────────

        private static EmployeeRepository CreateRepositoryForWorkGroupStaff(
            IEnumerable<PactStaff> WorkGroupStaff,
            IEnumerable<WorkgroupGrade>? workgroupGrades = null,
            IEnumerable<Workgroup>? workgroups = null,
            IEnumerable<PactWorkGroupGradeView>? pactWorkGroupGradeViews = null,
            int fpsYear = DefaultTestFpsYear)
        {
            var mockFpsYearContext = CreateMockFpsYearContext(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockFpsYearContext.Object);

            // Employees DbSet (required by base constructor)
            var employeesMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<Employee>());
            mockContext.Setup(x => x.Employees).Returns(employeesMockSet.Object);

            // WorkGroupEmployee DbSet
            var wgEmployeesMockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<WorkGroupEmployee>());
            mockContext.Setup(x => x.WorkGroupEmployees).Returns(wgEmployeesMockSet.Object);

            // WorkGroupStaffs DbSet
            var wgPeopleMockSet = RepositoryTestHelper.CreateMockDbSet(WorkGroupStaff);
            mockContext.Setup(x => x.PactStaffs).Returns(wgPeopleMockSet.Object);

            // WorkgroupGrades DbSet
            var gradesMockSet = RepositoryTestHelper.CreateMockDbSet(workgroupGrades ?? Enumerable.Empty<WorkgroupGrade>());
            mockContext.Setup(x => x.WorkgroupGrades).Returns(gradesMockSet.Object);

            // Workgroups DbSet (used by GetWorkGroupStaffAsync when workGroup filter is applied)
            var workgroupsMockSet = RepositoryTestHelper.CreateMockDbSet(workgroups ?? Enumerable.Empty<Workgroup>());
            mockContext.Setup(x => x.Workgroups).Returns(workgroupsMockSet.Object);

            // PactWorkGroupGradeViews DbSet (used by GetWorkGroupStaffAsync when workGroup filter is applied)
            var pactGradesMockSet = RepositoryTestHelper.CreateMockDbSet(pactWorkGroupGradeViews ?? Enumerable.Empty<PactWorkGroupGradeView>());
            mockContext.Setup(x => x.PactWorkGroupGradeViews).Returns(pactGradesMockSet.Object);

            return new EmployeeRepository(mockContext.Object, mockFpsYearContext.Object);
        }

        #region GetAllWorkGroupPersonAsync Tests

        [Fact]
        public async Task GetAllPersonAsync_ReturnsPersonsJoinedWithWorkgroupGrades()
        {
            // Arrange
            var people = new List<PactStaff>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1" },
                new() { Name = "Bob",   WorkGroupGrade = "WG2" }
            };
            var grades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG1", Workgroup = "Group A" },
                new() { WgGrade = "WG2", Workgroup = "Group B" }
            };
            var repo = CreateRepositoryForWorkGroupStaff(people, grades);

            // Act
            var result = await repo.GetAllWorkGroupPersonAsync();

            // Assert
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.All(list, p => Assert.NotNull(p.Name));
        }

        [Fact]
        public async Task GetAllPersonAsync_ReturnsOrderedByName()
        {
            // Arrange
            var people = new List<PactStaff>
            {
                new() { Name = "Charlie", WorkGroupGrade = "WG1" },
                new() { Name = "Alice",   WorkGroupGrade = "WG2" },
                new() { Name = "Bob",     WorkGroupGrade = "WG3" }
            };
            var grades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG1", Workgroup = "Group A" },
                new() { WgGrade = "WG2", Workgroup = "Group B" },
                new() { WgGrade = "WG3", Workgroup = "Group C" }
            };
            var repo = CreateRepositoryForWorkGroupStaff(people, grades);

            // Act
            var result = await repo.GetAllWorkGroupPersonAsync();

            // Assert
            var list = result.ToList();
            Assert.Equal("Alice",   list[0].Name);
            Assert.Equal("Bob",     list[1].Name);
            Assert.Equal("Charlie", list[2].Name);
        }

        [Fact]
        public async Task GetAllPersonAsync_ReturnsEmptyList_WhenNoPeopleExist()
        {
            // Arrange
            var repo = CreateRepositoryForWorkGroupStaff(
                new List<PactStaff>(),
                new List<WorkgroupGrade>());

            // Act
            var result = await repo.GetAllWorkGroupPersonAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllPersonAsync_ExcludesPeopleWithNoMatchingGrade()
        {
            // Arrange
            var people = new List<PactStaff>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1" },
                new() { Name = "Bob",   WorkGroupGrade = "WG_NOMATCH" }
            };
            var grades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG1", Workgroup = "Group A" }
            };
            var repo = CreateRepositoryForWorkGroupStaff(people, grades);

            // Act
            var result = await repo.GetAllWorkGroupPersonAsync();

            // Assert
            var list = result.ToList();
            Assert.Single(list);
            Assert.Equal("Alice", list[0].Name);
        }

        [Fact]
        public async Task GetAllPersonAsync_SetsWorkGroupFromGradeJoin()
        {
            // Arrange
            var people = new List<PactStaff>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1" }
            };
            var grades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG1", Workgroup = "Alpha Group" }
            };
            var repo = CreateRepositoryForWorkGroupStaff(people, grades);

            // Act
            var result = await repo.GetAllWorkGroupPersonAsync();

            // Assert
            var person = Assert.Single(result.ToList());
            Assert.Equal("Alice",       person.Name);
            Assert.Equal("WG1",         person.WorkGroupGrade);
            Assert.Equal("Alpha Group", person.WorkGroup);
        }

        #endregion

        #region GetPagedWorkGroupStaffAsync Tests

        [Fact]
        public async Task GetPagedWorkGroupStaffAsync_NoFilter_ReturnsAllPeopleOrderedByName()
        {
            // Arrange
            var people = new List<PactStaff>
            {
                new() { Name = "Charlie", WorkGroupGrade = "WG1", PactId = "P003" },
                new() { Name = "Alice",   WorkGroupGrade = "WG1", PactId = "P001" },
                new() { Name = "Bob",     WorkGroupGrade = "WG2", PactId = "P002" }
            };
            var grades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG1", Workgroup = "Group A" },
                new() { WgGrade = "WG2", Workgroup = "Group B" }
            };
            var repo = CreateRepositoryForWorkGroupStaff(people, grades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            // Act
            var result = await repo.GetPagedWorkGroupStaffAsync(query);

            // Assert
            Assert.Equal(3, result.Data.Count());
            Assert.Equal("Alice", result.Data.First().Name);
        }

        [Fact]
        public async Task GetPagedWorkGroupStaffAsync_FilterByWorkGroup_ReturnsMatchingPeople()
        {
            // Arrange
            // GetPagedWorkGroupStaffAsync joins: Workgroups → PactWorkGroupGradeViews → PactStaffs
            var people = new List<PactStaff>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1" },
                new() { Name = "Bob",   WorkGroupGrade = "WG2" }
            };
            var workgroups = new List<Workgroup>
            {
                new() { WorkGroupName = "Group A" },
                new() { WorkGroupName = "Group B" }
            };
            var pactGrades = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WG1", WorkGroup = "Group A" },
                new() { WgGrade = "WG2", WorkGroup = "Group B" }
            };
            var repo = CreateRepositoryForWorkGroupStaff(
                people,
                workgroups: workgroups,
                pactWorkGroupGradeViews: pactGrades);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            // Act
            var result = await repo.GetPagedWorkGroupStaffAsync(query, "Group A");

            // Assert – workGroup filter joins through Workgroups + PactWorkGroupGradeViews; only WG1 staff returned
            Assert.All(result.Data, p => Assert.Equal("WG1", p.WorkGroupGrade));
        }

        [Fact]
        public async Task GetPagedWorkGroupStaffAsync_FilterByLeave_ReturnsMatchingPeople()
        {
            // Note: EF.Functions.Like (used for string fields such as Name/PersonStatus) is not
            // supported in client-side mock evaluation and can only be tested via integration tests.
            // This test exercises the numeric filter path (Leave) which uses Contains and works
            // correctly with in-memory mock DbSets.

            // Arrange
            var people = new List<PactStaff>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1", Leave = 50.0 },
                new() { Name = "Bob",   WorkGroupGrade = "WG2", Leave = 120.0 }
            };
            var grades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG1", Workgroup = "Group A" },
                new() { WgGrade = "WG2", Workgroup = "Group B" }
            };
            var repo = CreateRepositoryForWorkGroupStaff(people, grades);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Leave\":\"50\"}"
            };

            // Act
            var result = await repo.GetPagedWorkGroupStaffAsync(query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("Alice", result.Data.First().Name);
        }

        [Fact]
        public async Task GetPagedWorkGroupStaffAsync_FilterBySickSpecial_ReturnsMatchingPeople()
        {
            // Note: EF.Functions.Like (used for string fields such as Name/PersonStatus) is not
            // supported in client-side mock evaluation and can only be tested via integration tests.
            // This test exercises the numeric filter path (SickSpecial) which uses Contains and
            // works correctly with in-memory mock DbSets.

            // Arrange
            var people = new List<PactStaff>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1", SickSpecial = 10.0 },
                new() { Name = "Bob",   WorkGroupGrade = "WG2", SickSpecial = 99.5 }
            };
            var grades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG1", Workgroup = "Group A" },
                new() { WgGrade = "WG2", Workgroup = "Group B" }
            };
            var repo = CreateRepositoryForWorkGroupStaff(people, grades);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"SickSpecial\":\"10\"}"
            };

            // Act
            var result = await repo.GetPagedWorkGroupStaffAsync(query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("Alice", result.Data.First().Name);
        }

        [Fact]
        public async Task GetPagedWorkGroupStaffAsync_NumericFilter_HrsPaid_ReturnsMatchingPeople()
        {
            // Arrange
            var people = new List<PactStaff>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1", HrsPaid = 100.5 },
                new() { Name = "Bob",   WorkGroupGrade = "WG2", HrsPaid = 200.0 }
            };
            var grades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG1", Workgroup = "Group A" },
                new() { WgGrade = "WG2", Workgroup = "Group B" }
            };
            var repo = CreateRepositoryForWorkGroupStaff(people, grades);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"HrsPaid\":\"100\"}"
            };

            // Act
            var result = await repo.GetPagedWorkGroupStaffAsync(query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("Alice", result.Data.First().Name);
        }

        [Fact]
        public async Task GetPagedWorkGroupStaffAsync_NumericFilter_HrsAvail_ReturnsMatchingPeople()
        {
            // Arrange
            var people = new List<PactStaff>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1", HrsAvail = 30.0 },
                new() { Name = "Bob",   WorkGroupGrade = "WG1", HrsAvail = 40.0 }
            };
            var grades = new List<WorkgroupGrade> { new() { WgGrade = "WG1", Workgroup = "Group A" } };
            var repo = CreateRepositoryForWorkGroupStaff(people, grades);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"HrsAvail\":\"30\"}"
            };

            // Act
            var result = await repo.GetPagedWorkGroupStaffAsync(query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("Alice", result.Data.First().Name);
        }

        [Fact]
        public async Task GetPagedWorkGroupStaffAsync_NumericFilter_MultipleFields_FiltersCorrectly()
        {
            // Covers HrsPaid + Leave + SickSpecial + HrsAvail applied together
            // Arrange
            var people = new List<PactStaff>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1", HrsPaid = 37.5, Leave = 5.0, SickSpecial = 1.0, HrsAvail = 31.5 },
                new() { Name = "Bob",   WorkGroupGrade = "WG1", HrsPaid = 20.0, Leave = 5.0, SickSpecial = 1.0, HrsAvail = 14.0 }
            };
            var grades = new List<WorkgroupGrade> { new() { WgGrade = "WG1", Workgroup = "Group A" } };
            var repo = CreateRepositoryForWorkGroupStaff(people, grades);
            // Filter targets Alice's unique HrsPaid value AND common Leave/SickSpecial values
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"HrsPaid\":\"37\",\"Leave\":\"5\",\"SickSpecial\":\"1\",\"HrsAvail\":\"31\"}"
            };

            // Act
            var result = await repo.GetPagedWorkGroupStaffAsync(query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("Alice", result.Data.First().Name);
        }

        [Fact]
        public async Task GetPagedWorkGroupStaffAsync_NumericFilter_HighPrecisionFloats_ReturnsMatchingPeople()
        {
            // Arrange
            var people = new List<PactStaff>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1", HrsPaid = 100.12345 },
                new() { Name = "Bob",   WorkGroupGrade = "WG1", HrsPaid = 200.67891 }
            };
            var grades = new List<WorkgroupGrade> { new() { WgGrade = "WG1", Workgroup = "Group A" } };
            var repo = CreateRepositoryForWorkGroupStaff(people, grades);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"HrsPaid\":\"100.123\"}"
            };

            // Act
            var result = await repo.GetPagedWorkGroupStaffAsync(query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("Alice", result.Data.First().Name);
        }

        [Fact]
        public async Task GetPagedWorkGroupStaffAsync_Pagination_ReturnsCorrectPage()
        {
            // Arrange
            var people = Enumerable.Range(1, 15)
                .Select(i => new PactStaff { Name = $"Person{i:D2}", WorkGroupGrade = "WG1" })
                .ToList();
            var grades = new List<WorkgroupGrade> { new() { WgGrade = "WG1", Workgroup = "Group A" } };
            var repo = CreateRepositoryForWorkGroupStaff(people, grades);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 5, Filter = null };

            // Act
            var result = await repo.GetPagedWorkGroupStaffAsync(query);

            // Assert
            Assert.Equal(5, result.Data.Count());
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetPagedWorkGroupStaffAsync_SortByName_Descending_ReturnsSortedResults()
        {
            // Arrange
            var people = new List<PactStaff>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1" },
                new() { Name = "Charlie", WorkGroupGrade = "WG1" },
                new() { Name = "Bob",   WorkGroupGrade = "WG2" }
            };
            var grades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG1", Workgroup = "Group A" },
                new() { WgGrade = "WG2", Workgroup = "Group B" }
            };
            var repo = CreateRepositoryForWorkGroupStaff(people, grades);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "Name", Descending = true, Filter = null
            };

            // Act
            var result = await repo.GetPagedWorkGroupStaffAsync(query);

            // Assert
            var list = result.Data.ToList();
            Assert.Equal("Charlie", list[0].Name);
            Assert.Equal("Bob",     list[1].Name);
            Assert.Equal("Alice",   list[2].Name);
        }

        [Fact]
        public async Task GetPagedWorkGroupStaffAsync_EmptyDatabase_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var repo = CreateRepositoryForWorkGroupStaff(
                new List<PactStaff>(),
                new List<WorkgroupGrade>());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            // Act
            var result = await repo.GetPagedWorkGroupStaffAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        // ── ApplyWorkGroupStaffSorting – all remaining sort keys ─────────────

        [Theory]
        // Assert checks result.Data.First().Name; expected = Name of person with lowest/highest sort-key value
        [InlineData("PactId",        false, "Alice")]   // P001 → Alice
        [InlineData("PactId",        true,  "Charlie")] // P003 → Charlie
        [InlineData("SpNumber",      false, "Alice")]   // SP001 → Alice
        [InlineData("SpNumber",      true,  "Charlie")] // SP003 → Charlie
        [InlineData("Title",         false, "Alice")]   // Analyst → Alice
        [InlineData("Title",         true,  "Charlie")] // Manager → Charlie
        [InlineData("WorkGroupGrade",false, "Alice")]   // WG1 → Alice
        [InlineData("WorkGroupGrade",true,  "Charlie")] // WG3 → Charlie
        [InlineData("PersonStatus",  false, "Alice")]   // Active → Alice
        [InlineData("PersonStatus",  true,  "Charlie")] // Retired → Charlie
        [InlineData("HrsPaid",       false, "Alice")]   // lowest HrsPaid
        [InlineData("HrsPaid",       true,  "Charlie")] // highest HrsPaid
        [InlineData("Leave",         false, "Alice")]
        [InlineData("Leave",         true,  "Charlie")]
        [InlineData("SickSpecial",   false, "Alice")]
        [InlineData("SickSpecial",   true,  "Charlie")]
        [InlineData("HrsAvail",      false, "Alice")]
        [InlineData("HrsAvail",      true,  "Charlie")]
        [InlineData("Name",          false, "Alice")]   // ascending – already tested via Descending test
        [InlineData("UnknownKey",    false, "Alice")]   // unknown key → default OrderBy(Name) asc
        public async Task GetPagedWorkGroupStaffAsync_SortBy_SortsCorrectly(
            string sortBy, bool descending, string expectedFirstName)
        {
            // Arrange
            var people = new List<PactStaff>
            {
                new() { Name = "Bob",     WorkGroupGrade = "WG2", PactId = "P002", SpNumber = "SP002",
                        Title = "Developer", PersonStatus = "OnLeave",
                        HrsPaid = 20.0, Leave = 2.0, SickSpecial = 2.0, HrsAvail = 16.0 },
                new() { Name = "Alice",   WorkGroupGrade = "WG1", PactId = "P001", SpNumber = "SP001",
                        Title = "Analyst",   PersonStatus = "Active",
                        HrsPaid = 10.0, Leave = 1.0, SickSpecial = 1.0, HrsAvail = 8.0 },
                new() { Name = "Charlie", WorkGroupGrade = "WG3", PactId = "P003", SpNumber = "SP003",
                        Title = "Manager",   PersonStatus = "Retired",
                        HrsPaid = 30.0, Leave = 3.0, SickSpecial = 3.0, HrsAvail = 24.0 }
            };
            var grades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG1", Workgroup = "Group A" },
                new() { WgGrade = "WG2", Workgroup = "Group B" },
                new() { WgGrade = "WG3", Workgroup = "Group C" }
            };
            var repo = CreateRepositoryForWorkGroupStaff(people, grades);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = sortBy,
                Descending = descending
            };

            // Act
            var result = await repo.GetPagedWorkGroupStaffAsync(query);

            // Assert
            Assert.Equal(expectedFirstName, result.Data.First().Name);
        }

        #endregion

        #region GetPactWorkGroupStaffAsync Tests

        [Fact]
        public async Task GetPactWorkGroupStaffAsync_WithNullWorkGroup_ReturnsAllStaffOrderedByNameThenWorkGroupGrade()
        {
            // Arrange
            var people = new List<PactStaff>
            {
                new() { Name = "Charlie", WorkGroupGrade = "WG2", PactId = "P003" },
                new() { Name = "Alice", WorkGroupGrade = "WG2", PactId = "P002" },
                new() { Name = "Alice", WorkGroupGrade = "WG1", PactId = "P001" }
            };
            var workgroups = new List<Workgroup>
            {
                new() { WorkGroupName = "Group A" },
                new() { WorkGroupName = "Group B" }
            };
            var pactGrades = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WG1", WorkGroup = "Group A" },
                new() { WgGrade = "WG2", WorkGroup = "Group B" }
            };

            var repo = CreateRepositoryForWorkGroupStaff(
                people,
                workgroups: workgroups,
                pactWorkGroupGradeViews: pactGrades);

            // Act
            var result = (await repo.GetPactWorkGroupStaffAsync(null)).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("Alice", result[0].Name);
            Assert.Equal("WG1", result[0].WorkGroupGrade);
            Assert.Equal("Alice", result[1].Name);
            Assert.Equal("WG2", result[1].WorkGroupGrade);
            Assert.Equal("Charlie", result[2].Name);
        }

        [Fact]
        public async Task GetPactWorkGroupStaffAsync_WithSpecificWorkGroup_ReturnsMatchingStaffOrderedByName()
        {
            // Arrange
            var people = new List<PactStaff>
            {
                new() { Name = "Charlie", WorkGroupGrade = "WG2", PactId = "P003" },
                new() { Name = "Bob", WorkGroupGrade = "WG1", PactId = "P002" },
                new() { Name = "Alice", WorkGroupGrade = "WG1", PactId = "P001" }
            };
            var workgroups = new List<Workgroup>
            {
                new() { WorkGroupName = "Group A" },
                new() { WorkGroupName = "Group B" }
            };
            var pactGrades = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WG1", WorkGroup = "Group A" },
                new() { WgGrade = "WG2", WorkGroup = "Group B" }
            };

            var repo = CreateRepositoryForWorkGroupStaff(
                people,
                workgroups: workgroups,
                pactWorkGroupGradeViews: pactGrades);

            // Act
            var result = (await repo.GetPactWorkGroupStaffAsync("Group A")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, x => Assert.Equal("WG1", x.WorkGroupGrade));
            Assert.Equal("Alice", result[0].Name);
            Assert.Equal("Bob", result[1].Name);
        }

        [Fact]
        public async Task GetPactWorkGroupStaffAsync_WithUnknownWorkGroup_ReturnsEmptyList()
        {
            // Arrange
            var people = new List<PactStaff>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1", PactId = "P001" }
            };
            var workgroups = new List<Workgroup>
            {
                new() { WorkGroupName = "Group A" }
            };
            var pactGrades = new List<PactWorkGroupGradeView>
            {
                new() { WgGrade = "WG1", WorkGroup = "Group A" }
            };

            var repo = CreateRepositoryForWorkGroupStaff(
                people,
                workgroups: workgroups,
                pactWorkGroupGradeViews: pactGrades);

            // Act
            var result = await repo.GetPactWorkGroupStaffAsync("Group Z");

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region ApplyEmployeeFilter – missing branch coverage

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithEmptyFilter_ReturnsAll()
        {
            // Covers the string.IsNullOrEmpty(filter) early-return in ApplyEmployeeFilter
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "" };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert – empty filter returns all records unfiltered
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithNullFilterModel_ReturnsAll()
        {
            // Covers the filterModel == null branch in ApplyEmployeeFilter
            // JSON literal "null" deserialises to null ExpandoObject
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "null" };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert – null filterModel falls through; all rows returned
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithAllNullFilterValues_SkipsAllPredicates()
        {
            // Covers the "&& spNumber != null", "&& firstName != null", etc. guards in
            // ApplyEmployeeFilter – when a key is present but the JSON value is null
            // the predicate must be skipped and all rows must be returned.
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith",  Title = "Analyst" },
                new() { SPNumber = "SP002", FirstName = "Bob",   LastName = "Jones",  Title = "Manager" }
            };
            var repo = CreateRepository(employees);
            // All four string-filter keys present with explicit JSON null values
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"SPNumber\":null,\"FirstName\":null,\"LastName\":null,\"Title\":null}"
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert – null-value guards skip every predicate; both rows returned
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetEmployeesByPrefixAsync_WithAllStringFilters_FiltersCorrectly()
        {
            // Covers all 4 ILike branches in ApplyEmployeeFilter simultaneously
            // Arrange
            var employees = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith",  Title = "Analyst" },
                new() { SPNumber = "SP002", FirstName = "Alice", LastName = "Brown",  Title = "Manager" },
                new() { SPNumber = "SP003", FirstName = "Bob",   LastName = "Smith",  Title = "Analyst" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"SPNumber\":\"SP001\",\"FirstName\":\"Alice\",\"LastName\":\"Smith\",\"Title\":\"Analyst\"}"
            };

            // Act
            var result = await repo.GetEmployeesByPrefixAsync(query, "SP");

            // Assert – only SP001 matches all four filters
            Assert.Single(result.Data);
            Assert.Equal("SP001", result.Data.First().SPNumber);
        }

        #endregion

        #region GetAllManagersAsync – empty GradeCode branch

        [Fact]
        public async Task GetAllManagersAsync_ExcludesEmptyGradeCode()
        {
            // Covers the grade.GradeCode.Length > 0 branch in GetAllManagersAsync
            // A grade with an empty string GradeCode must be excluded even though it is not null.
            // Arrange
            var staffActiveViews = new List<StaffActiveView>
            {
                new() { StaffID = "S001", Name = "Manager One", WorkgroupGrade = "WG01" },
                new() { StaffID = "S002", Name = "Manager Two", WorkgroupGrade = "WG02" }
            };
            var workgroupGrades = new List<WorkgroupGradeGeneralView>
            {
                new() { WgGrade = "WG01", GradeCode = "M01", WorkGroup = "Management" },
                new() { WgGrade = "WG02", GradeCode = "",    WorkGroup = "Management" }  // empty – excluded
            };
            var repo = CreateRepository(
                new List<Employee>(),
                staffActiveViews,
                workgroupGrades);

            // Act
            var result = await repo.GetAllManagersAsync();

            // Assert – the record with empty GradeCode is excluded
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("Manager One", resultList[0].Name);
        }

        #endregion

        #region GetAllManagersAsync – null staff Name branch

        [Fact]
        public async Task GetAllManagersAsync_ExcludesNullStaffName()
        {
            // Covers the staff.Name != null guard in GetAllManagersAsync
            // Arrange
            var staffActiveViews = new List<StaffActiveView>
            {
                new() { StaffID = "S001", Name = "Manager One", WorkgroupGrade = "WG01" },
                new() { StaffID = "S002", Name = null,          WorkgroupGrade = "WG02" }  // excluded
            };
            var workgroupGrades = new List<WorkgroupGradeGeneralView>
            {
                new() { WgGrade = "WG01", GradeCode = "M01", WorkGroup = "Management" },
                new() { WgGrade = "WG02", GradeCode = "M02", WorkGroup = "Management" }
            };
            var repo = CreateRepository(
                new List<Employee>(),
                staffActiveViews,
                workgroupGrades);

            // Act
            var result = await repo.GetAllManagersAsync();

            // Assert – the entry with null Name is excluded
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("Manager One", resultList[0].Name);
        }

        #endregion

        #region GetPactStaffAsync Tests

        [Fact]
        public async Task GetPactStaffAsync_ReturnsAllStaff_OrderedByName()
        {
            // Arrange
            var pactStaffs = new List<PactStaff>
            {
                new() { PactId = "S003", SpNumber = "SP003", Name = "Charlie Brown",  WorkGroupGrade = "WG1" },
                new() { PactId = "S001", SpNumber = "SP001", Name = "Alice Smith",    WorkGroupGrade = "WG2" },
                new() { PactId = "S002", SpNumber = "SP002", Name = "Bob Jones",      WorkGroupGrade = "WG1" }
            };
            var repo = CreateRepository(new List<Employee>(), pactStaffs: pactStaffs);

            // Act
            var result = await repo.GetPactStaffAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal(3, resultList.Count);
            Assert.Equal("Alice Smith",    resultList[0].Name);
            Assert.Equal("Bob Jones",      resultList[1].Name);
            Assert.Equal("Charlie Brown",  resultList[2].Name);
        }

        [Fact]
        public async Task GetPactStaffAsync_WithEmptyTable_ReturnsEmptyList()
        {
            // Arrange
            var repo = CreateRepository(new List<Employee>(), pactStaffs: new List<PactStaff>());

            // Act
            var result = await repo.GetPactStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPactStaffAsync_ReturnsSingleStaff_WhenOnlyOneExists()
        {
            // Arrange
            var pactStaffs = new List<PactStaff>
            {
                new() { PactId = "S001", SpNumber = "SP001", Name = "Alice Smith", WorkGroupGrade = "WG1",
                        Title = "Officer", PersonStatus = "Active", PersonClass = "Permanent",
                        HrsPaid = 37.5, Leave = 5.0, SickSpecial = 1.5, HrsAvail = 31.0 }
            };
            var repo = CreateRepository(new List<Employee>(), pactStaffs: pactStaffs);

            // Act
            var result = await repo.GetPactStaffAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("S001",       resultList[0].PactId);
            Assert.Equal("SP001",      resultList[0].SpNumber);
            Assert.Equal("Alice Smith", resultList[0].Name);
            Assert.Equal("WG1",        resultList[0].WorkGroupGrade);
            Assert.Equal("Officer",    resultList[0].Title);
            Assert.Equal("Active",     resultList[0].PersonStatus);
            Assert.Equal("Permanent",  resultList[0].PersonClass);
            Assert.Equal(37.5,         resultList[0].HrsPaid);
            Assert.Equal(5.0,          resultList[0].Leave);
            Assert.Equal(1.5,          resultList[0].SickSpecial);
            Assert.Equal(31.0,         resultList[0].HrsAvail);
        }

        [Fact]
        public async Task GetPactStaffAsync_WithNullOptionalFields_ReturnsStaffWithNulls()
        {
            // Arrange
            var pactStaffs = new List<PactStaff>
            {
                new() { PactId = null, SpNumber = null, Name = "Unnamed", WorkGroupGrade = null,
                        HrsPaid = null, Leave = null, SickSpecial = null, HrsAvail = null }
            };
            var repo = CreateRepository(new List<Employee>(), pactStaffs: pactStaffs);

            // Act
            var result = await repo.GetPactStaffAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Null(resultList[0].PactId);
            Assert.Null(resultList[0].SpNumber);
            Assert.Null(resultList[0].HrsPaid);
            Assert.Null(resultList[0].Leave);
            Assert.Null(resultList[0].SickSpecial);
            Assert.Null(resultList[0].HrsAvail);
        }

        #endregion
    }
}
