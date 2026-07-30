using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.WorkGroupEmployeeRepositoryTest
{
    public class WorkGroupEmployeeRepositoryTests
    {
        private const string DefaultWgGrade = "WG01";
        private const string DefaultPactId  = "PACT001";

        private static WorkGroupEmployeeRepository CreateRepository(
            IEnumerable<WorkGroupEmployee> employees,
            IEnumerable<Employee>? staffMembers = null)
        {
            const string testEmail = "test@example.com";

            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(2024);
            requestContext.UserEmailId.Returns(testEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var employeesMockSet = RepositoryTestHelper.CreateMockDbSet(employees);
            mockContext.Setup(x => x.WorkGroupEmployees).Returns(employeesMockSet.Object);

            var viewWgEmployees = employees.Select(e => new WorkGroupEmployeeView
            {
                PactId         = e.PactId,
                SpNumber       = e.SpNumber,
                WorkGroupGrade = e.WorkGroupGrade,
                PersonStatus   = e.PersonStatus,
                UserEmail      = testEmail,
                FpsYear        = requestContext.FpsYear
            });
            var viewWgEmployeesMockSet = RepositoryTestHelper.CreateMockDbSet(viewWgEmployees);
            mockContext.Setup(x => x.WorkGroupEmployeeViews).Returns(viewWgEmployeesMockSet.Object);

            var staff = (staffMembers ?? employees.Select(e => new Employee
            {
                SPNumber  = e.SpNumber,
                FirstName = "First",
                LastName  = "Last"
            }))
            .Select(s => new Employee
            {
                SPNumber = s.SPNumber,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Title = s.Title,
                FpsYear = s.FpsYear ?? requestContext.FpsYear
            });
            var staffMockSet = RepositoryTestHelper.CreateMockDbSet(staff);
            mockContext.Setup(x => x.Employees).Returns(staffMockSet.Object);

            return new WorkGroupEmployeeRepository(mockContext.Object, requestContext);
        }

        #region GetWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WithMatchingWgGrade_ReturnsActiveEmployees()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P003", SpNumber = "SP003", WorkGroupGrade = "OTHER",        PersonStatus = "A" },
                new() { PactId = "P004", SpNumber = "SP004", WorkGroupGrade = DefaultWgGrade, PersonStatus = "I" }
            };
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, e => Assert.Equal(DefaultWgGrade, e.WorkGroupGrade));
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WithNoMatchingWgGrade_ReturnsEmpty()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = "OTHER", PersonStatus = "A" }
            };
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WithPagination_ReturnsCorrectPage()
        {
            var employees = Enumerable.Range(1, 5).Select(i => new WorkGroupEmployee
            {
                PactId         = $"P00{i}",
                SpNumber       = $"SP00{i}",
                WorkGroupGrade = DefaultWgGrade,
                PersonStatus   = "A"
            }).ToList();
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            var result = await repo.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WithEmptyJsonFilter_ReturnsAllActiveEmployees()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };

            var result = await repo.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WithNameSort_ProjectsEmployeeNames()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var staffMembers = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones" }
            };
            var repo = CreateRepository(employees, staffMembers);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "name", Descending = false };

            var result = await repo.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            Assert.All(result.Data, item => Assert.False(string.IsNullOrWhiteSpace(item.Name)));
            Assert.Contains(result.Data, item => item.Name!.Contains("Smith"));
            Assert.Contains(result.Data, item => item.Name!.Contains("Jones"));
        }

        [Fact]
        public async Task GetWorkGroupEmployeeForStaffAsync_WithDefaultSort_ReturnsAllEmployeesForGrade()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var staffMembers = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" },
                new() { SPNumber = "SP002", FirstName = "Bob", LastName = "Jones" }
            };
            var repo = CreateRepository(employees, staffMembers);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupEmployeeForStaffAsync(query, DefaultWgGrade);

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, item => Assert.Equal(DefaultWgGrade, item.WorkGroupGrade));
            Assert.All(result.Data, item => Assert.False(string.IsNullOrWhiteSpace(item.Name)));
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WithNullFilter_ReturnsAllActiveEmployees()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            var result = await repo.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_SortBySpNumberAscending_ReturnsOrderedResults()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P003", SpNumber = "SP003", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "spnumber", Descending = false };

            var result = await repo.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            var list = result.Data.ToList();
            Assert.Equal("SP001", list[0].SpNumber);
            Assert.Equal("SP002", list[1].SpNumber);
            Assert.Equal("SP003", list[2].SpNumber);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_SortBySpNumberDescending_ReturnsOrderedResults()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P003", SpNumber = "SP003", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "spnumber", Descending = true };

            var result = await repo.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            var list = result.Data.ToList();
            Assert.Equal("SP003", list[0].SpNumber);
            Assert.Equal("SP002", list[1].SpNumber);
            Assert.Equal("SP001", list[2].SpNumber);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_SortByNameDescending_ReturnsOrderedResults()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var staffMembers = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Anderson" },
                new() { SPNumber = "SP002", FirstName = "Zara",  LastName = "Zebra" }
            };
            var repo  = CreateRepository(employees, staffMembers);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "name", Descending = true };

            var result = await repo.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            var list = result.Data.ToList();
            Assert.Contains("Zebra", list[0].Name);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_SortByNameAscending_ReturnsOrderedResults()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var staffMembers = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Anderson" },
                new() { SPNumber = "SP002", FirstName = "Zara",  LastName = "Zebra" }
            };
            var repo  = CreateRepository(employees, staffMembers);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "name", Descending = false };

            var result = await repo.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            var list = result.Data.ToList();
            Assert.Contains("Anderson", list[0].Name);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_SortByStaffNameAscending_ReturnsOrderedResults()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var staffMembers = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Anderson" },
                new() { SPNumber = "SP002", FirstName = "Zara",  LastName = "Zebra" }
            };
            var repo  = CreateRepository(employees, staffMembers);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "staffname", Descending = false };

            var result = await repo.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            var list = result.Data.ToList();
            Assert.Contains("Anderson", list[0].Name);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeForStaffAsync_WithPactIdFilter_ReturnsMatchingEmployees()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"PactId\":\"P001\"}" };

            var result = await repo.GetWorkGroupEmployeeForStaffAsync(query, DefaultWgGrade);

            Assert.Single(result.Data);
            Assert.Equal("P001", result.Data.First().PactId);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeForStaffAsync_WithWorkGroupGradeFilter_ReturnsMatchingEmployees()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = "WG-A", PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = "WG-B", PersonStatus = "A" }
            };
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{\"WorkGroupGrade\":\"WG-B\"}" };

            var result = await repo.GetWorkGroupEmployeeForStaffAsync(query, string.Empty);

            Assert.Single(result.Data);
            Assert.Equal("WG-B", result.Data.First().WorkGroupGrade);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeForStaffAsync_WithWgGradeSortAlias_ReturnsOrderedResults()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = "WG-A", PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = "WG-B", PersonStatus = "A" }
            };
            var repo = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "wggrade", Descending = true };

            var result = await repo.GetWorkGroupEmployeeForStaffAsync(query, string.Empty);

            var list = result.Data.ToList();
            Assert.Equal("WG-B", list[0].WorkGroupGrade);
            Assert.Equal("WG-A", list[1].WorkGroupGrade);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WithLiteralNullFilter_ReturnsAllActiveEmployees()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "null" };

            var result = await repo.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetWorkGroupEmployeeForStaffAsync_SortByPactIdAscending_ReturnsOrderedResults()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P003", SpNumber = "SP003", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "pactid", Descending = false };

            var result = await repo.GetWorkGroupEmployeeForStaffAsync(query, DefaultWgGrade);

            var list = result.Data.ToList();
            Assert.Equal("P001", list[0].PactId);
            Assert.Equal("P002", list[1].PactId);
            Assert.Equal("P003", list[2].PactId);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeForStaffAsync_SortByWorkGroupGradeDescending_ReturnsOrderedResults()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = "WG-A", PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = "WG-C", PersonStatus = "A" },
                new() { PactId = "P003", SpNumber = "SP003", WorkGroupGrade = "WG-B", PersonStatus = "A" }
            };
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "workgroupgrade", Descending = true };

            var result = await repo.GetWorkGroupEmployeeForStaffAsync(query, string.Empty);

            var list = result.Data.ToList();
            Assert.Equal("WG-C", list[0].WorkGroupGrade);
            Assert.Equal("WG-B", list[1].WorkGroupGrade);
            Assert.Equal("WG-A", list[2].WorkGroupGrade);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeForStaffAsync_SortByWgGradeDescending_ReturnsOrderedResults()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = "WG-A", PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = "WG-C", PersonStatus = "A" },
                new() { PactId = "P003", SpNumber = "SP003", WorkGroupGrade = "WG-B", PersonStatus = "A" }
            };
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "wggrade", Descending = true };

            var result = await repo.GetWorkGroupEmployeeForStaffAsync(query, string.Empty);

            var list = result.Data.ToList();
            Assert.Equal("WG-C", list[0].WorkGroupGrade);
            Assert.Equal("WG-B", list[1].WorkGroupGrade);
            Assert.Equal("WG-A", list[2].WorkGroupGrade);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeForStaffAsync_SortByPersonStatusAscending_ReturnsOrderedResults()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "B" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "personstatus", Descending = false };

            var result = await repo.GetWorkGroupEmployeeForStaffAsync(query, DefaultWgGrade);

            var list = result.Data.ToList();
            Assert.Equal("A", list[0].PersonStatus);
            Assert.Equal("B", list[1].PersonStatus);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_SortByUnknownProperty_KeepsOriginalOrder()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P010", SpNumber = "SP010", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P011", SpNumber = "SP011", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "unknown", Descending = false };

            var result = await repo.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            var list = result.Data.ToList();
            Assert.Equal("P010", list[0].PactId);
            Assert.Equal("P011", list[1].PactId);
        }

        #endregion

        #region GetWorkGroupEmployeeByIdAsync Tests

        [Fact]
        public async Task GetWorkGroupEmployeeByIdAsync_WithMatchingPactId_ReturnsEmployee()
        {
            var startDate = new DateTime(2025, 4, 1);
            var endDate = new DateTime(2025, 12, 31);
            var employees = new List<WorkGroupEmployee>
            {
                new()
                {
                    PactId = DefaultPactId,
                    SpNumber = "SP001",
                    WorkGroupGrade = DefaultWgGrade,
                    PersonStatus = "A",
                    TimeRecorder = 1,
                    StartDate = startDate,
                    EndDate = endDate,
                    HoursPerWeek = 37.5
                }
            };
            var repo = CreateRepository(employees);

            var result = await repo.GetWorkGroupEmployeeByIdAsync(DefaultPactId);

            Assert.NotNull(result);
            Assert.Equal(DefaultPactId, result!.PactId);
            Assert.Equal(1, result.TimeRecorder);
            Assert.Equal(startDate, result.StartDate);
            Assert.Equal(endDate, result.EndDate);
            Assert.Equal(37.5, result.HoursPerWeek);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeByIdAsync_WithNoMatchingPactId_ReturnsNull()
        {
            var repo = CreateRepository(new List<WorkGroupEmployee>());

            var result = await repo.GetWorkGroupEmployeeByIdAsync("NONEXISTENT");

            Assert.Null(result);
        }

        #endregion

        #region UpdateWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task UpdateWorkGroupEmployeeAsync_WithValidEntity_UpdatesAndReturnsEntity()
        {
            var existing = new WorkGroupEmployee
            {
                PactId         = DefaultPactId,
                SpNumber       = "SP001",
                WorkGroupGrade = DefaultWgGrade,
                PersonStatus   = "A",
                HrsPaid        = 37.0,
                Leave          = 0.0,
                SickSpecial    = 0.0,
                HrsAvail       = 37.0,
                MakeAvailable  = -1
            };
            var repo = CreateRepository(new List<WorkGroupEmployee> { existing });

            var update = new WorkGroupEmployee
            {
                PactId        = DefaultPactId,
                HrsPaid       = 40.0,
                Leave         = 2.0,
                SickSpecial   = 1.0,
                HrsAvail      = 1200.0,
                PersonStatus  = "A",
                MakeAvailable = 0
            };

            var result = await repo.UpdateWorkGroupEmployeeAsync(update);

            Assert.NotNull(result);
            Assert.Equal(40.0, result.HrsPaid);
            Assert.Equal(2.0, result.Leave);
            Assert.Equal(1.0, result.SickSpecial);
            Assert.Equal(37.0, result.HrsAvail);
        }

        [Fact]
        public async Task UpdateWorkGroupEmployeeForStaffAsync_WithValidEntity_UpdatesAndReturnsEntity()
        {
            var startDate = new DateTime(2025, 4, 1);
            var endDate = new DateTime(2025, 12, 31);
            var existing = new WorkGroupEmployee
            {
                PactId         = DefaultPactId,
                SpNumber       = "SP001",
                WorkGroupGrade = DefaultWgGrade,
                PersonStatus   = "A",
                HrsPaid        = 37.0,
                Leave          = 0.0,
                SickSpecial    = 0.0,
                HrsAvail       = 37.0,
                MakeAvailable  = -1,
                TimeRecorder   = 0,
                StartDate      = null,
                EndDate        = null,
                HoursPerWeek   = null
            };
            var repo = CreateRepository(new List<WorkGroupEmployee> { existing });

            var update = new WorkGroupEmployee
            {
                PactId         = DefaultPactId,
                SpNumber       = "SP999",
                WorkGroupGrade = "WG-UPDATED",
                HrsPaid        = 40.0,
                Leave          = 2.0,
                SickSpecial    = 1.0,
                HrsAvail       = 1200.0,
                PersonStatus   = "A",
                MakeAvailable  = 0,
                TimeRecorder   = 1,
                StartDate      = startDate,
                EndDate        = endDate,
                HoursPerWeek   = 37.5
            };

            var result = await repo.UpdateWorkGroupEmployeeForStaffAsync(update);

            Assert.NotNull(result);
            Assert.Equal("SP999", result.SpNumber);
            Assert.Equal("WG-UPDATED", result.WorkGroupGrade);
            Assert.Equal(40.0, result.HrsPaid);
            Assert.Equal(1200.0, result.HrsAvail);
            Assert.Equal(1, result.TimeRecorder);
            Assert.Equal(startDate, result.StartDate);
            Assert.Equal(endDate, result.EndDate);
            Assert.Equal(37.5, result.HoursPerWeek);
        }

        [Fact]
        public async Task UpdateWorkGroupEmployeeAsync_WithNonExistentPactId_ThrowsKeyNotFoundException()
        {
            var repo   = CreateRepository(new List<WorkGroupEmployee>());
            var entity = new WorkGroupEmployee { PactId = "NONEXISTENT" };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                repo.UpdateWorkGroupEmployeeAsync(entity));
        }

        #endregion

        #region HasAssociatedStaffAsync Tests

        [Fact]
        public async Task HasAssociatedStaffAsync_WithMatchingWgGrade_ReturnsTrue()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var repo = CreateRepository(employees);

            var result = await repo.HasAssociatedStaffAsync(DefaultWgGrade);

            Assert.True(result);
        }

        [Fact]
        public async Task HasAssociatedStaffAsync_WithNoMatchingWgGrade_ReturnsFalse()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = "OTHER", PersonStatus = "A" }
            };
            var repo = CreateRepository(employees);

            var result = await repo.HasAssociatedStaffAsync(DefaultWgGrade);

            Assert.False(result);
        }

        [Fact]
        public async Task HasAssociatedStaffAsync_WithEmptyRepository_ReturnsFalse()
        {
            var repo = CreateRepository(new List<WorkGroupEmployee>());

            var result = await repo.HasAssociatedStaffAsync(DefaultWgGrade);

            Assert.False(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task HasAssociatedStaffAsync_WithNullOrWhitespaceWgGrade_ReturnsFalse(string? wgGrade)
        {
            var repo = CreateRepository(new List<WorkGroupEmployee>());

            var result = await repo.HasAssociatedStaffAsync(wgGrade!);

            Assert.False(result);
        }

        #endregion

        #region DeleteWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task DeleteWorkGroupEmployeeAsync_WithExistingPactId_ReturnsTrueAndRemoves()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = DefaultPactId, SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var repo = CreateRepository(employees);

            var result = await repo.DeleteWorkGroupEmployeeAsync(DefaultPactId);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteWorkGroupEmployeeAsync_WithNonExistentPactId_ReturnsFalse()
        {
            var repo = CreateRepository(new List<WorkGroupEmployee>());

            var result = await repo.DeleteWorkGroupEmployeeAsync("NONEXISTENT");

            Assert.False(result);
        }

        #endregion

        #region GetAllActiveWorkGroupEmployeesAsync Tests

        [Fact]
        public async Task GetAllActiveWorkGroupEmployeesAsync_ExcludesInactiveEmployees()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = DefaultWgGrade, PersonStatus = "I" },
                new() { PactId = "P003", SpNumber = "SP003", WorkGroupGrade = "WG02",         PersonStatus = "A" }
            };
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAllActiveWorkGroupEmployeesAsync(query, DefaultWgGrade);

            Assert.Single(result.Data);
            Assert.All(result.Data, e => Assert.NotEqual("I", e.PersonStatus));
        }

        [Fact]
        public async Task GetAllActiveWorkGroupEmployeesAsync_FiltersToSpecificWorkGroup()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = "WG01", PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = "WG02", PersonStatus = "A" },
                new() { PactId = "P003", SpNumber = "SP003", WorkGroupGrade = "WG03", PersonStatus = "A" }
            };
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAllActiveWorkGroupEmployeesAsync(query, "WG02");

            Assert.Single(result.Data);
            Assert.Equal("WG02", result.Data.First().WorkGroupGrade);
        }

        [Fact]
        public async Task GetAllActiveWorkGroupEmployeesAsync_ProjectsEmployeeNameFromJoin()
        {
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var staffMembers = new List<Employee>
            {
                new() { SPNumber = "SP001", FirstName = "Alice", LastName = "Smith" }
            };
            var repo  = CreateRepository(employees, staffMembers);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAllActiveWorkGroupEmployeesAsync(query, DefaultWgGrade);

            Assert.Single(result.Data);
            Assert.Equal("Smith Alice", result.Data.First().Name);
        }

        [Fact]
        public async Task GetAllActiveWorkGroupEmployeesAsync_WithEmptyDataSet_ReturnsEmpty()
        {
            var repo  = CreateRepository(new List<WorkGroupEmployee>());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAllActiveWorkGroupEmployeesAsync(query, DefaultWgGrade);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetAllActiveWorkGroupEmployeesAsync_WithPagination_ReturnsCorrectPage()
        {
            var employees = Enumerable.Range(1, 5).Select(i => new WorkGroupEmployee
            {
                PactId         = $"P00{i}",
                SpNumber       = $"SP00{i}",
                WorkGroupGrade = DefaultWgGrade,
                PersonStatus   = "A"
            }).ToList();
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            var result = await repo.GetAllActiveWorkGroupEmployeesAsync(query, DefaultWgGrade);

            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAllActiveWorkGroupEmployeesAsync_DoesNotFilterByUserEmail()
        {
            // All employees are active; the method must return them regardless of user scope
            var employees = new List<WorkGroupEmployee>
            {
                new() { PactId = "P001", SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" },
                new() { PactId = "P002", SpNumber = "SP002", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var repo  = CreateRepository(employees);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetAllActiveWorkGroupEmployeesAsync(query, DefaultWgGrade);

            Assert.Equal(2, result.Data.Count());
        }

        #endregion
    }
}
