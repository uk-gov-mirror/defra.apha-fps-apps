using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Application.Services;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Application.UnitTests.Services.EmployeeServiceTest
{
    public class EmployeeServiceTests
    {
        private readonly IEmployeeRepository _mockRepository;
        private readonly IMapper _mockMapper;
        private readonly EmployeeService _sut;

        public EmployeeServiceTests()
        {
            _mockRepository = Substitute.For<IEmployeeRepository>();
            _mockMapper = Substitute.For<IMapper>();
            _sut = new EmployeeService(_mockRepository, _mockMapper);
        }

        #region GetFilteredEmployeesAsync (Paginated)

        [Fact]
        public async Task GetFilteredEmployeesAsync_Paginated_WithFilterOption1_ReturnsAllEmployees()
        {
            // Arrange
            var queryFilter = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            var mappedPaginationParams = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10
            };

            var repositoryResult = new PagedData<Employee>
            {
                Data = new List<Employee>
                {
                    new Employee { SPNumber = "SP001", FirstName = "John" },
                    new Employee { SPNumber = "SP002", FirstName = "Jane" }
                },
                PaginationData = new PaginationData
                {
                    TotalPages = 1,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 2
                }
            };

            var expectedResult = new PaginatedResult<EmployeeDto>
            {
                Data = new List<EmployeeDto>
                {
                    new EmployeeDto { SPNumber = "SP001", FirstName = "John" },
                    new EmployeeDto { SPNumber = "SP002", FirstName = "Jane" }
                },
                PaginationData = new PaginationDto
                {
                    TotalPages = 1,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 2
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter).Returns(mappedPaginationParams);
            _mockRepository.GetEmployeesByPrefixAsync(mappedPaginationParams, "").Returns(repositoryResult);
            _mockMapper.Map<PaginatedResult<EmployeeDto>>(repositoryResult).Returns(expectedResult);

            // Act
            var result = await _sut.GetFilteredEmployeesAsync(queryFilter, 1);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.PaginationData.TotalRecords.Should().Be(2);
            result.Data.First().SPNumber.Should().Be("SP001");

            _mockMapper.Received(1).Map<PaginationParameters<string>>(queryFilter);
            await _mockRepository.Received(1).GetEmployeesByPrefixAsync(mappedPaginationParams, "");
            _mockMapper.Received(1).Map<PaginatedResult<EmployeeDto>>(repositoryResult);
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_Paginated_WithFilterOption2_ReturnsEmployeesWithPrefixT()
        {
            // Arrange
            var queryFilter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedPaginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var repositoryResult = new PagedData<Employee>
            {
                Data = new List<Employee>
                {
                    new Employee { SPNumber = "T001", FirstName = "Tom" }
                },
                PaginationData = new PaginationData
                {
                    TotalPages = 1,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 1
                }
            };

            var expectedResult = new PaginatedResult<EmployeeDto>
            {
                Data = new List<EmployeeDto>
                {
                    new EmployeeDto { SPNumber = "T001", FirstName = "Tom" }
                },
                PaginationData = new PaginationDto
                {
                    TotalPages = 1,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 1
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter).Returns(mappedPaginationParams);
            _mockRepository.GetEmployeesByPrefixAsync(mappedPaginationParams, "T").Returns(repositoryResult);
            _mockMapper.Map<PaginatedResult<EmployeeDto>>(repositoryResult).Returns(expectedResult);

            // Act
            var result = await _sut.GetFilteredEmployeesAsync(queryFilter, 2);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.Data.First().SPNumber.Should().Be("T001");

            await _mockRepository.Received(1).GetEmployeesByPrefixAsync(mappedPaginationParams, "T");
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_Paginated_WithFilterOption3_ReturnsEmployeesWithPrefixG()
        {
            // Arrange
            var queryFilter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedPaginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var repositoryResult = new PagedData<Employee>
            {
                Data = new List<Employee>
                {
                    new Employee { SPNumber = "G001", FirstName = "George" }
                },
                PaginationData = new PaginationData
                {
                    TotalPages = 1,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 1
                }
            };

            var expectedResult = new PaginatedResult<EmployeeDto>
            {
                Data = new List<EmployeeDto>
                {
                    new EmployeeDto { SPNumber = "G001", FirstName = "George" }
                },
                PaginationData = new PaginationDto
                {
                    TotalPages = 1,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 1
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter).Returns(mappedPaginationParams);
            _mockRepository.GetEmployeesByPrefixAsync(mappedPaginationParams, "G").Returns(repositoryResult);
            _mockMapper.Map<PaginatedResult<EmployeeDto>>(repositoryResult).Returns(expectedResult);

            // Act
            var result = await _sut.GetFilteredEmployeesAsync(queryFilter, 3);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.Data.First().SPNumber.Should().Be("G001");

            await _mockRepository.Received(1).GetEmployeesByPrefixAsync(mappedPaginationParams, "G");
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_Paginated_WithEmptyResult_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var queryFilter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedPaginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var emptyRepositoryResult = new PagedData<Employee>
            {
                Data = new List<Employee>(),
                PaginationData = new PaginationData
                {
                    TotalPages = 0,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 0
                }
            };

            var emptyExpectedResult = new PaginatedResult<EmployeeDto>
            {
                Data = new List<EmployeeDto>(),
                PaginationData = new PaginationDto
                {
                    TotalPages = 0,
                    PageNumber = 1,
                    PageSize = 10,
                    TotalRecords = 0
                }
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter).Returns(mappedPaginationParams);
            _mockRepository.GetEmployeesByPrefixAsync(mappedPaginationParams, "").Returns(emptyRepositoryResult);
            _mockMapper.Map<PaginatedResult<EmployeeDto>>(emptyRepositoryResult).Returns(emptyExpectedResult);

            // Act
            var result = await _sut.GetFilteredEmployeesAsync(queryFilter, 1);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_Paginated_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var queryFilter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedPaginationParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter).Returns(mappedPaginationParams);
            _mockRepository.GetEmployeesByPrefixAsync(mappedPaginationParams, "")
                .Throws(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sut.GetFilteredEmployeesAsync(queryFilter, 1)
            );

            exception.Message.Should().Be("Database connection failed");
            _mockMapper.DidNotReceive().Map<PaginatedResult<EmployeeDto>>(Arg.Any<PagedData<Employee>>());
        }

        #endregion

        #region GetFilteredEmployeesAsync (Non-Paginated)

        [Fact]
        public async Task GetFilteredEmployeesAsync_NonPaginated_WithFilterOption1_ReturnsAllEmployees()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new Employee { SPNumber = "SP001", FirstName = "John" },
                new Employee { SPNumber = "SP002", FirstName = "Jane" }
            };

            var expectedDtos = new List<EmployeeDto>
            {
                new EmployeeDto { SPNumber = "SP001", FirstName = "John" },
                new EmployeeDto { SPNumber = "SP002", FirstName = "Jane" }
            };

            _mockRepository.GetAllEmployeesAsync().Returns(employees);
            _mockMapper.Map<IEnumerable<EmployeeDto>>(employees).Returns(expectedDtos);

            // Act
            var result = await _sut.GetFilteredEmployeesAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().SPNumber.Should().Be("SP001");

            await _mockRepository.Received(1).GetAllEmployeesAsync();
            _mockMapper.Received(1).Map<IEnumerable<EmployeeDto>>(employees);
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_NonPaginated_WithFilterOption2_ReturnsEmployeesWithPrefixT()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new Employee { SPNumber = "T001", FirstName = "Tom" }
            };

            var expectedDtos = new List<EmployeeDto>
            {
                new EmployeeDto { SPNumber = "T001", FirstName = "Tom" }
            };

            _mockRepository.GetEmployeesByPrefixAsync("T").Returns(employees);
            _mockMapper.Map<IEnumerable<EmployeeDto>>(employees).Returns(expectedDtos);

            // Act
            var result = await _sut.GetFilteredEmployeesAsync(2);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().SPNumber.Should().Be("T001");

            await _mockRepository.Received(1).GetEmployeesByPrefixAsync("T");
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_NonPaginated_WithFilterOption3_ReturnsEmployeesWithPrefixG()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new Employee { SPNumber = "G001", FirstName = "George" }
            };

            var expectedDtos = new List<EmployeeDto>
            {
                new EmployeeDto { SPNumber = "G001", FirstName = "George" }
            };

            _mockRepository.GetEmployeesByPrefixAsync("G").Returns(employees);
            _mockMapper.Map<IEnumerable<EmployeeDto>>(employees).Returns(expectedDtos);

            // Act
            var result = await _sut.GetFilteredEmployeesAsync(3);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().SPNumber.Should().Be("G001");

            await _mockRepository.Received(1).GetEmployeesByPrefixAsync("G");
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_NonPaginated_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var emptyEmployees = new List<Employee>();
            var emptyDtos = new List<EmployeeDto>();

            _mockRepository.GetAllEmployeesAsync().Returns(emptyEmployees);
            _mockMapper.Map<IEnumerable<EmployeeDto>>(emptyEmployees).Returns(emptyDtos);

            // Act
            var result = await _sut.GetFilteredEmployeesAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetAllEmployeesAsync();
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_NonPaginated_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockRepository.GetAllEmployeesAsync()
                .Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetFilteredEmployeesAsync(1)
            );

            exception.Message.Should().Be("Database connection failed");
            _mockMapper.DidNotReceive().Map<IEnumerable<EmployeeDto>>(Arg.Any<IEnumerable<Employee>>());
        }

        #endregion

        #region GetEmployeeByIdAsync

        [Fact]
        public async Task GetEmployeeByIdAsync_WithValidSpNumber_ReturnsEmployeeDto()
        {
            // Arrange
            var spNumber = "SP001";
            var employee = new Employee { SPNumber = spNumber, FirstName = "John" };
            var expectedDto = new EmployeeDto { SPNumber = spNumber, FirstName = "John" };

            _mockRepository.GetEmployeeByIdAsync(spNumber).Returns(employee);
            _mockMapper.Map<EmployeeDto>(employee).Returns(expectedDto);

            // Act
            var result = await _sut.GetEmployeeByIdAsync(spNumber);

            // Assert
            result.Should().NotBeNull();
            result.SPNumber.Should().Be(spNumber);
            result.FirstName.Should().Be("John");

            await _mockRepository.Received(1).GetEmployeeByIdAsync(spNumber);
            _mockMapper.Received(1).Map<EmployeeDto>(employee);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_WhenEmployeeNotFound_ReturnsNull()
        {
            // Arrange
            var spNumber = "SP999";

            _mockRepository.GetEmployeeByIdAsync(spNumber).Returns((Employee?)null);
            _mockMapper.Map<EmployeeDto>((Employee?)null).Returns((EmployeeDto?)null);

            // Act
            var result = await _sut.GetEmployeeByIdAsync(spNumber);

            // Assert
            result.Should().BeNull();

            await _mockRepository.Received(1).GetEmployeeByIdAsync(spNumber);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetEmployeeByIdAsync_WithInvalidSpNumber_ThrowsArgumentException(string? spNumber)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _sut.GetEmployeeByIdAsync(spNumber!)
            );

            exception.ParamName.Should().Be("spNumber");
            exception.Message.Should().Contain("SPNumber cannot be null or empty");

            await _mockRepository.DidNotReceive().GetEmployeeByIdAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var spNumber = "SP001";
            _mockRepository.GetEmployeeByIdAsync(spNumber)
                .Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetEmployeeByIdAsync(spNumber)
            );

            exception.Message.Should().Be("Database connection failed");
            _mockMapper.DidNotReceive().Map<EmployeeDto>(Arg.Any<Employee>());
        }

        #endregion

        #region AddEmployeeAsync

        [Fact]
        public async Task AddEmployeeAsync_WithValidEmployee_ReturnsAddedEmployeeDto()
        {
            // Arrange
            var inputDto = new EmployeeDto
            {
                SPNumber = "SP001",
                FirstName = "John",
                LastName = "Doe"
            };

            var mappedEntity = new Employee
            {
                SPNumber = "SP001",
                FirstName = "John",
                LastName = "Doe"
            };

            var repositoryResult = new Employee
            {
                SPNumber = "SP001",
                FirstName = "John",
                LastName = "Doe"
            };

            var expectedDto = new EmployeeDto
            {
                SPNumber = "SP001",
                FirstName = "John",
                LastName = "Doe"
            };

            _mockMapper.Map<Employee>(inputDto).Returns(mappedEntity);
            _mockRepository.AddEmployeeAsync(mappedEntity).Returns(repositoryResult);
            _mockMapper.Map<EmployeeDto>(repositoryResult).Returns(expectedDto);

            // Act
            var result = await _sut.AddEmployeeAsync(inputDto);

            // Assert
            result.Should().NotBeNull();
            result.SPNumber.Should().Be("SP001");
            result.FirstName.Should().Be("John");
            result.LastName.Should().Be("Doe");

            _mockMapper.Received(1).Map<Employee>(inputDto);
            await _mockRepository.Received(1).AddEmployeeAsync(mappedEntity);
            _mockMapper.Received(1).Map<EmployeeDto>(repositoryResult);
        }

        [Fact]
        public async Task AddEmployeeAsync_WithNullEmployee_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _sut.AddEmployeeAsync(null!)
            );

            exception.ParamName.Should().Be("employeeDto");
            exception.Message.Should().Contain("EmployeeDto cannot be null or empty.");

            await _mockRepository.DidNotReceive().AddEmployeeAsync(Arg.Any<Employee>());
        }

        [Fact]
        public async Task AddEmployeeAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var inputDto = new EmployeeDto { SPNumber = "SP001" };
            var mappedEntity = new Employee { SPNumber = "SP001" };

            _mockMapper.Map<Employee>(inputDto).Returns(mappedEntity);
            _mockRepository.AddEmployeeAsync(mappedEntity)
                .Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.AddEmployeeAsync(inputDto)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).AddEmployeeAsync(mappedEntity);
        }

        #endregion

        #region UpdateEmployeeAsync

        [Fact]
        public async Task UpdateEmployeeAsync_WithValidEmployee_ReturnsUpdatedEmployeeDto()
        {
            // Arrange
            var inputDto = new EmployeeDto
            {
                SPNumber = "SP001",
                FirstName = "John",
                LastName = "Doe"
            };

            var mappedEntity = new Employee
            {
                SPNumber = "SP001",
                FirstName = "John",
                LastName = "Doe"
            };

            var updatedEntity = new Employee
            {
                SPNumber = "SP001",
                FirstName = "John",
                LastName = "Smith"
            };

            var expectedDto = new EmployeeDto
            {
                SPNumber = "SP001",
                FirstName = "John",
                LastName = "Smith"
            };

            _mockMapper.Map<Employee>(inputDto).Returns(mappedEntity);
            _mockRepository.UpdateEmployeeAsync(mappedEntity).Returns(updatedEntity);
            _mockMapper.Map<EmployeeDto>(updatedEntity).Returns(expectedDto);

            // Act
            var result = await _sut.UpdateEmployeeAsync(inputDto);

            // Assert
            result.Should().NotBeNull();
            result.SPNumber.Should().Be("SP001");
            result.LastName.Should().Be("Smith");

            _mockMapper.Received(1).Map<Employee>(inputDto);
            await _mockRepository.Received(1).UpdateEmployeeAsync(mappedEntity);
            _mockMapper.Received(1).Map<EmployeeDto>(updatedEntity);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_WithNullEmployee_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _sut.UpdateEmployeeAsync(null!)
            );

            exception.ParamName.Should().Be("employeeDto");
            exception.Message.Should().Contain("EmployeeDto cannot be null or empty.");

            await _mockRepository.DidNotReceive().UpdateEmployeeAsync(Arg.Any<Employee>());
        }

        [Fact]
        public async Task UpdateEmployeeAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var inputDto = new EmployeeDto { SPNumber = "SP001" };
            var mappedEntity = new Employee { SPNumber = "SP001" };
            
            _mockMapper.Map<Employee>(inputDto).Returns(mappedEntity);
            _mockRepository.UpdateEmployeeAsync(mappedEntity)
                .Throws(new InvalidOperationException("Employee not found"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _sut.UpdateEmployeeAsync(inputDto)
            );

            exception.Message.Should().Be("Employee not found");
            await _mockRepository.Received(1).UpdateEmployeeAsync(mappedEntity);
        }

        #endregion

        #region DeleteEmployeeAsync

        [Fact]
        public async Task DeleteEmployeeAsync_WithValidSpNumber_ReturnsTrue()
        {
            // Arrange
            var spNumber = "SP001";
            _mockRepository.DeleteEmployeeAsync(spNumber).Returns(true);

            // Act
            var result = await _sut.DeleteEmployeeAsync(spNumber);

            // Assert
            result.Should().BeTrue();
            await _mockRepository.Received(1).DeleteEmployeeAsync(spNumber);
        }

        [Fact]
        public async Task DeleteEmployeeAsync_WithNonExistentEmployee_ReturnsFalse()
        {
            // Arrange
            var spNumber = "SP999";
            _mockRepository.DeleteEmployeeAsync(spNumber).Returns(false);

            // Act
            var result = await _sut.DeleteEmployeeAsync(spNumber);

            // Assert
            result.Should().BeFalse();
            await _mockRepository.Received(1).DeleteEmployeeAsync(spNumber);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteEmployeeAsync_WithInvalidSpNumber_ThrowsArgumentException(string? spNumber)
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                async () => await _sut.DeleteEmployeeAsync(spNumber!)
            );

            exception.ParamName.Should().Be("spNumber");
            exception.Message.Should().Contain("SPNumber cannot be null or empty");

            await _mockRepository.DidNotReceive().DeleteEmployeeAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteEmployeeAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var spNumber = "SP001";
            _mockRepository.DeleteEmployeeAsync(spNumber)
                .Throws(new Exception("Database connection failed"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.DeleteEmployeeAsync(spNumber)
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).DeleteEmployeeAsync(spNumber);
        }

        #endregion

        #region GetAllManagersAsync

        [Fact]
        public async Task GetAllManagersAsync_WithValidData_ReturnsManagerDtoList()
        {
            // Arrange
            var managers = new List<Manager>
            {
                new Manager { Name = "Alice Manager", WorkGroup = "WG001", GradeCode = "G1" },
                new Manager { Name = "Bob Supervisor", WorkGroup = "WG002", GradeCode = "G2" }
            };

            var expectedDtos = new List<ManagerDto>
            {
                new ManagerDto { Name = "Alice Manager", WorkGroup = "WG001", GradeCode = "G1" },
                new ManagerDto { Name = "Bob Supervisor", WorkGroup = "WG002", GradeCode = "G2" }
            };

            _mockRepository.GetAllManagersAsync().Returns(managers);
            _mockMapper.Map<IEnumerable<ManagerDto>>(managers).Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllManagersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().Name.Should().Be("Alice Manager");
            result.First().WorkGroup.Should().Be("WG001");

            await _mockRepository.Received(1).GetAllManagersAsync();
            _mockMapper.Received(1).Map<IEnumerable<ManagerDto>>(managers);
        }

        [Fact]
        public async Task GetAllManagersAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var emptyManagers = new List<Manager>();
            var emptyDtos = new List<ManagerDto>();

            _mockRepository.GetAllManagersAsync().Returns(emptyManagers);
            _mockMapper.Map<IEnumerable<ManagerDto>>(emptyManagers).Returns(emptyDtos);

            // Act
            var result = await _sut.GetAllManagersAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetAllManagersAsync();
            _mockMapper.Received(1).Map<IEnumerable<ManagerDto>>(emptyManagers);
        }

        [Fact]
        public async Task GetAllManagersAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetAllManagersAsync()
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllManagersAsync()
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetAllManagersAsync();
            _mockMapper.DidNotReceive().Map<IEnumerable<ManagerDto>>(Arg.Any<IEnumerable<Manager>>());
        }

        #endregion

        #region GetAllWorkGroupPersonAsync

        [Fact]
        public async Task GetAllPersonAsync_WithData_ReturnsMappedPersonDtos()
        {
            // Arrange
            var persons = new List<WorkGroupPerson>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1", WorkGroup = "Group A" },
                new() { Name = "Bob",   WorkGroupGrade = "WG2", WorkGroup = "Group B" }
            };
            var expectedDtos = new List<WorkGroupPersonDto>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1", WorkGroup = "Group A" },
                new() { Name = "Bob",   WorkGroupGrade = "WG2", WorkGroup = "Group B" }
            };

            _mockRepository.GetAllWorkGroupPersonAsync().Returns(persons);
            _mockMapper.Map<IEnumerable<WorkGroupPersonDto>>(persons).Returns(expectedDtos);

            // Act
            var result = await _sut.GetAllWorkGroupPersonAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().Name.Should().Be("Alice");

            await _mockRepository.Received(1).GetAllWorkGroupPersonAsync();
            _mockMapper.Received(1).Map<IEnumerable<WorkGroupPersonDto>>(persons);
        }

        [Fact]
        public async Task GetAllPersonAsync_EmptyRepository_ReturnsEmptyList()
        {
            // Arrange
            _mockRepository.GetAllWorkGroupPersonAsync().Returns(new List<WorkGroupPerson>());
            _mockMapper.Map<IEnumerable<WorkGroupPersonDto>>(Arg.Any<IEnumerable<WorkGroupPerson>>())
                .Returns(new List<WorkGroupPersonDto>());

            // Act
            var result = await _sut.GetAllWorkGroupPersonAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetAllWorkGroupPersonAsync();
        }

        [Fact]
        public async Task GetAllPersonAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetAllWorkGroupPersonAsync()
                .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetAllWorkGroupPersonAsync());
            ex.Message.Should().Be("Database error");
            await _mockRepository.Received(1).GetAllWorkGroupPersonAsync();
        }

        #endregion

        #region GetPagedWorkGroupStaffAsync

        [Fact]
        public async Task GetPagedWorkGroupStaffAsync_WithNoWorkGroup_ReturnsAllStaff()
        {
            // Arrange
            var queryFilter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var repoResult = new PagedData<PactStaff>
            {
                Data = new List<PactStaff>
                {
                    new() { Name = "Alice", WorkGroupGrade = "WG1" }
                },
                PaginationData = new PaginationData { TotalRecords = 1, PageNumber = 1, PageSize = 10 }
            };
            var expectedDto = new PaginatedResult<PactStaffDto>
            {
                Data = [new() { Name = "Alice", WorkGroupGrade = "WG1" }],
                PaginationData = new PaginationDto { TotalRecords = 1 }
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter).Returns(mappedParams);
            _mockRepository.GetPagedWorkGroupStaffAsync(mappedParams, null).Returns(repoResult);
            _mockMapper.Map<PaginatedResult<PactStaffDto>>(repoResult).Returns(expectedDto);

            // Act
            var result = await _sut.GetPagedWorkGroupStaffAsync(queryFilter);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().HaveCount(1);
            result.Data.First().Name.Should().Be("Alice");

            await _mockRepository.Received(1).GetPagedWorkGroupStaffAsync(mappedParams, null);
            _mockMapper.Received(1).Map<PaginatedResult<PactStaffDto>>(repoResult);
        }

        [Fact]
        public async Task GetPagedWorkGroupStaffAsync_WithWorkGroup_PassesWorkGroupToRepository()
        {
            // Arrange
            var queryFilter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var repoResult = new PagedData<PactStaff>
            {
                Data = new List<PactStaff> { new() { Name = "Alice" } },
                PaginationData = new PaginationData { TotalRecords = 1 }
            };
            var expectedDto = new PaginatedResult<PactStaffDto>
            {
                Data = [new() { Name = "Alice" }],
                PaginationData = new PaginationDto { TotalRecords = 1 }
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter).Returns(mappedParams);
            _mockRepository.GetPagedWorkGroupStaffAsync(mappedParams, "WG1").Returns(repoResult);
            _mockMapper.Map<PaginatedResult<PactStaffDto>>(repoResult).Returns(expectedDto);

            // Act
            var result = await _sut.GetPagedWorkGroupStaffAsync(queryFilter, "WG1");

            // Assert
            result.Should().NotBeNull();
            await _mockRepository.Received(1).GetPagedWorkGroupStaffAsync(mappedParams, "WG1");
        }

        [Fact]
        public async Task GetPagedWorkGroupStaffAsync_EmptyResult_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var queryFilter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var mappedParams = new PaginationParameters<string> { Page = 1, PageSize = 10 };
            var repoResult = new PagedData<PactStaff>
            {
                Data = new List<PactStaff>(),
                PaginationData = new PaginationData { TotalRecords = 0, PageNumber = 1, PageSize = 10 }
            };
            var expectedDto = new PaginatedResult<PactStaffDto>
            {
                Data = [],
                PaginationData = new PaginationDto { TotalRecords = 0 }
            };

            _mockMapper.Map<PaginationParameters<string>>(queryFilter).Returns(mappedParams);
            _mockRepository.GetPagedWorkGroupStaffAsync(mappedParams, null).Returns(repoResult);
            _mockMapper.Map<PaginatedResult<PactStaffDto>>(repoResult).Returns(expectedDto);

            // Act
            var result = await _sut.GetPagedWorkGroupStaffAsync(queryFilter);

            // Assert
            result.Should().NotBeNull();
            result.Data.Should().BeEmpty();
            result.PaginationData.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetPagedWorkGroupStaffAsync_RepositoryThrows_PropagatesException()
        {
            // Arrange
            var queryFilter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mockMapper.Map<PaginationParameters<string>>(queryFilter)
                .Returns(new PaginationParameters<string>());
            _mockRepository.GetPagedWorkGroupStaffAsync(Arg.Any<PaginationParameters<string>>(), Arg.Any<string?>())
                .Throws(new Exception("DB failure"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetPagedWorkGroupStaffAsync(queryFilter));
            ex.Message.Should().Be("DB failure");
        }

        #endregion

        #region GetPactStaffAsync

        [Fact]
        public async Task GetPactStaffAsync_WithValidData_ReturnsPactStaffDtoList()
        {
            // Arrange
            var pactStaffs = new List<PactStaff>
            {
                new PactStaff { PactId = "1", SpNumber = "SP001", Name = "Alice Smith", WorkGroupGrade = "WG1" },
                new PactStaff { PactId = "2", SpNumber = "SP002", Name = "Bob Jones", WorkGroupGrade = "WG2" }
            };

            var expectedDtos = new List<PactStaffDto>
            {
                new PactStaffDto { PactId = "1", SpNumber = "SP001", Name = "Alice Smith", WorkGroupGrade = "WG1" },
                new PactStaffDto { PactId = "2", SpNumber = "SP002", Name = "Bob Jones", WorkGroupGrade = "WG2" }
            };

            _mockRepository.GetPactStaffAsync().Returns(pactStaffs);
            _mockMapper.Map<IEnumerable<PactStaffDto>>(pactStaffs).Returns(expectedDtos);

            // Act
            var result = await _sut.GetPactStaffAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.First().SpNumber.Should().Be("SP001");
            result.First().Name.Should().Be("Alice Smith");

            await _mockRepository.Received(1).GetPactStaffAsync();
            _mockMapper.Received(1).Map<IEnumerable<PactStaffDto>>(pactStaffs);
        }

        [Fact]
        public async Task GetPactStaffAsync_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var emptyPactStaffs = new List<PactStaff>();
            var emptyDtos = new List<PactStaffDto>();

            _mockRepository.GetPactStaffAsync().Returns(emptyPactStaffs);
            _mockMapper.Map<IEnumerable<PactStaffDto>>(emptyPactStaffs).Returns(emptyDtos);

            // Act
            var result = await _sut.GetPactStaffAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            await _mockRepository.Received(1).GetPactStaffAsync();
            _mockMapper.Received(1).Map<IEnumerable<PactStaffDto>>(emptyPactStaffs);
        }

        [Fact]
        public async Task GetPactStaffAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new Exception("Database connection failed");

            _mockRepository.GetPactStaffAsync()
                .Throws(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetPactStaffAsync()
            );

            exception.Message.Should().Be("Database connection failed");
            await _mockRepository.Received(1).GetPactStaffAsync();
            _mockMapper.DidNotReceive().Map<IEnumerable<PactStaffDto>>(Arg.Any<IEnumerable<PactStaff>>());
        }

        #endregion

        #region GetPactWorkGroupStaffAsync

        [Fact]
        public async Task GetPactWorkGroupStaffAsync_WithWorkGroup_ReturnsMappedPactStaff()
        {
            // Arrange
            const string workGroup = "WG1";
            var repositoryResult = new List<PactStaff>
            {
                new() { PactId = "P001", Name = "Alice", WorkGroupGrade = "WG1" }
            };
            var expectedDto = new List<PactStaffDto>
            {
                new() { PactId = "P001", Name = "Alice", WorkGroupGrade = "WG1" }
            };

            _mockRepository.GetPactWorkGroupStaffAsync(workGroup).Returns(repositoryResult);
            _mockMapper.Map<IEnumerable<PactStaffDto>>(repositoryResult).Returns(expectedDto);

            // Act
            var result = await _sut.GetPactWorkGroupStaffAsync(workGroup);

            // Assert
            result.Should().BeEquivalentTo(expectedDto);
            await _mockRepository.Received(1).GetPactWorkGroupStaffAsync(workGroup);
            _mockMapper.Received(1).Map<IEnumerable<PactStaffDto>>(repositoryResult);
        }

        [Fact]
        public async Task GetPactWorkGroupStaffAsync_WithNullWorkGroup_ReturnsMappedPactStaff()
        {
            // Arrange
            var repositoryResult = new List<PactStaff>();
            var expectedDto = new List<PactStaffDto>();

            _mockRepository.GetPactWorkGroupStaffAsync(null).Returns(repositoryResult);
            _mockMapper.Map<IEnumerable<PactStaffDto>>(repositoryResult).Returns(expectedDto);

            // Act
            var result = await _sut.GetPactWorkGroupStaffAsync(null);

            // Assert
            result.Should().BeEmpty();
            await _mockRepository.Received(1).GetPactWorkGroupStaffAsync(null);
            _mockMapper.Received(1).Map<IEnumerable<PactStaffDto>>(repositoryResult);
        }

        [Fact]
        public async Task GetPactWorkGroupStaffAsync_WhenRepositoryThrows_PropagatesException()
        {
            // Arrange
            _mockRepository.GetPactWorkGroupStaffAsync(Arg.Any<string?>())
                .Throws(new Exception("DB failure"));

            // Act
            var exception = await Assert.ThrowsAsync<Exception>(
                async () => await _sut.GetPactWorkGroupStaffAsync("WG1"));

            // Assert
            exception.Message.Should().Be("DB failure");
            _mockMapper.DidNotReceive().Map<IEnumerable<PactStaffDto>>(Arg.Any<IEnumerable<PactStaff>>());
        }

        #endregion
    }
}
