using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.EmployeeServiceTest
{
    public class EmployeeServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsEmployeeApiClient _fpsEmployeeApiClient;
        private readonly EmployeeService _employeeService;

        public EmployeeServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsEmployeeApiClient = Substitute.For<IFpsEmployeeApiClient>();
            _fpsClient.FpsEmployee.Returns(_fpsEmployeeApiClient);
            _employeeService = new EmployeeService(_fpsClient);
        }

        #region GetFilteredEmployeesAsync Tests

        [Fact]
        public async Task GetFilteredEmployeesAsync_WithValidCriteria_ReturnsSuccessResponse()
        {
            // Arrange
            var queryParameters = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };
            var employees = new List<EmployeeDto>
            {
                new EmployeeDto { SPNumber = "000001", FirstName = "John", LastName = "Doe" },
                new EmployeeDto { SPNumber = "000002", FirstName = "Jane", LastName = "Smith" }
            };
            var expectedResponse = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(
                employees,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _fpsEmployeeApiClient.GetFilteredEmployeesAsync(queryParameters, 1)
                .Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetFilteredEmployeesAsync(queryParameters, 1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsEmployeeApiClient.Received(1).GetFilteredEmployeesAsync(queryParameters, 1);
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var queryParameters = new QueryParameters<string>
            {
                Page = 1,
                PageSize = 10
            };
            var expectedResponse = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(
                new List<EmployeeDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            );

            _fpsEmployeeApiClient.GetFilteredEmployeesAsync(queryParameters, 1)
                .Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetFilteredEmployeesAsync(queryParameters, 1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetFilteredEmployeesAsync_WithDifferentFilterOptions_PassesCorrectValue(int filterOption)
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(
                new List<EmployeeDto>(),
                new PaginationDto()
            );

            _fpsEmployeeApiClient.GetFilteredEmployeesAsync(queryParameters, filterOption)
                .Returns(expectedResponse);

            // Act
            await _employeeService.GetFilteredEmployeesAsync(queryParameters, filterOption);

            // Assert
            await _fpsEmployeeApiClient.Received(1).GetFilteredEmployeesAsync(queryParameters, filterOption);
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<EmployeeDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsEmployeeApiClient.GetFilteredEmployeesAsync(queryParameters, 1)
                .Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetFilteredEmployeesAsync(queryParameters, 1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetEmployeeByIdAsync Tests

        [Fact]
        public async Task GetEmployeeByIdAsync_WithValidSPNumber_ReturnsEmployee()
        {
            // Arrange
            var spNumber = "000001";
            var employee = new EmployeeDto
            {
                SPNumber = spNumber,
                FirstName = "John",
                LastName = "Doe",
                Title = "Manager"
            };
            var expectedResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(employee);

            _fpsEmployeeApiClient.GetEmployeeIdAsync(spNumber).Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetEmployeeByIdAsync(spNumber);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(spNumber, result.Data.SPNumber);
            await _fpsEmployeeApiClient.Received(1).GetEmployeeIdAsync(spNumber);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_WithNonExistentSPNumber_ReturnsFailureResponse()
        {
            // Arrange
            var spNumber = "999999";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Employee not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<EmployeeDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsEmployeeApiClient.GetEmployeeIdAsync(spNumber).Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetEmployeeByIdAsync(spNumber);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("000001")]
        [InlineData("123456")]
        [InlineData("SP9999")]
        public async Task GetEmployeeByIdAsync_WithVariousSPNumbers_CallsApiClient(string spNumber)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(new EmployeeDto { SPNumber = spNumber });
            _fpsEmployeeApiClient.GetEmployeeIdAsync(spNumber).Returns(expectedResponse);

            // Act
            await _employeeService.GetEmployeeByIdAsync(spNumber);

            // Assert
            await _fpsEmployeeApiClient.Received(1).GetEmployeeIdAsync(spNumber);
        }

        #endregion

        #region CreateEmployeeAsync Tests

        [Fact]
        public async Task CreateEmployeeAsync_WithValidEmployee_ReturnsSuccessResponse()
        {
            // Arrange
            var newEmployee = new EmployeeDto
            {
                SPNumber = "000001",
                FirstName = "John",
                LastName = "Doe",
                Title = "Manager"
            };
            var expectedResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(newEmployee);

            _fpsEmployeeApiClient.CreateEmployeeAsync(newEmployee).Returns(expectedResponse);

            // Act
            var result = await _employeeService.CreateEmployeeAsync(newEmployee);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(newEmployee.SPNumber, result.Data.SPNumber);
            await _fpsEmployeeApiClient.Received(1).CreateEmployeeAsync(newEmployee);
        }

        [Fact]
        public async Task CreateEmployeeAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var newEmployee = new EmployeeDto
            {
                SPNumber = "000001",
                FirstName = "John",
                LastName = "Doe"
            };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Duplicate employee", Code = "DUPLICATE" }
            };
            var expectedResponse = ApiResponseDto<EmployeeDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsEmployeeApiClient.CreateEmployeeAsync(newEmployee).Returns(expectedResponse);

            // Act
            var result = await _employeeService.CreateEmployeeAsync(newEmployee);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task CreateEmployeeAsync_WithMinimalData_CallsApiClient()
        {
            // Arrange
            var newEmployee = new EmployeeDto { SPNumber = "000001" };
            var expectedResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(newEmployee);

            _fpsEmployeeApiClient.CreateEmployeeAsync(newEmployee).Returns(expectedResponse);

            // Act
            await _employeeService.CreateEmployeeAsync(newEmployee);

            // Assert
            await _fpsEmployeeApiClient.Received(1).CreateEmployeeAsync(newEmployee);
        }

        #endregion

        #region UpdateEmployeeAsync Tests

        [Fact]
        public async Task UpdateEmployeeAsync_WithValidEmployee_ReturnsSuccessResponse()
        {
            // Arrange
            var updatedEmployee = new EmployeeDto
            {
                SPNumber = "000001",
                FirstName = "Jane",
                LastName = "Smith",
                Title = "Senior Manager"
            };
            var expectedResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(updatedEmployee);

            _fpsEmployeeApiClient.UpdateEmployeeAsync(updatedEmployee).Returns(expectedResponse);

            // Act
            var result = await _employeeService.UpdateEmployeeAsync(updatedEmployee);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Jane", result.Data.FirstName);
            await _fpsEmployeeApiClient.Received(1).UpdateEmployeeAsync(updatedEmployee);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_WithNonExistentEmployee_ReturnsFailureResponse()
        {
            // Arrange
            var employee = new EmployeeDto { SPNumber = "999999" };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Employee not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<EmployeeDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsEmployeeApiClient.UpdateEmployeeAsync(employee).Returns(expectedResponse);

            // Act
            var result = await _employeeService.UpdateEmployeeAsync(employee);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_WhenApiReturnsError_ReturnsFailureResponse()
        {
            // Arrange
            var employee = new EmployeeDto { SPNumber = "000001" };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Update failed", Code = "UPDATE_ERROR" }
            };
            var expectedResponse = ApiResponseDto<EmployeeDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsEmployeeApiClient.UpdateEmployeeAsync(employee).Returns(expectedResponse);

            // Act
            var result = await _employeeService.UpdateEmployeeAsync(employee);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteEmployeeAsync Tests

        [Fact]
        public async Task DeleteEmployeeAsync_WithValidSPNumber_ReturnsSuccessResponse()
        {
            // Arrange
            var spNumber = "000001";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _fpsEmployeeApiClient.DeleteEmployeeAsync(spNumber).Returns(expectedResponse);

            // Act
            var result = await _employeeService.DeleteEmployeeAsync(spNumber);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsEmployeeApiClient.Received(1).DeleteEmployeeAsync(spNumber);
        }

        [Fact]
        public async Task DeleteEmployeeAsync_WithNonExistentSPNumber_ReturnsFailureResponse()
        {
            // Arrange
            var spNumber = "999999";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Employee not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _fpsEmployeeApiClient.DeleteEmployeeAsync(spNumber).Returns(expectedResponse);

            // Act
            var result = await _employeeService.DeleteEmployeeAsync(spNumber);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("000001")]
        [InlineData("123456")]
        [InlineData("SP9999")]
        public async Task DeleteEmployeeAsync_WithVariousSPNumbers_CallsApiClient(string spNumber)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsEmployeeApiClient.DeleteEmployeeAsync(spNumber).Returns(expectedResponse);

            // Act
            await _employeeService.DeleteEmployeeAsync(spNumber);

            // Assert
            await _fpsEmployeeApiClient.Received(1).DeleteEmployeeAsync(spNumber);
        }

        #endregion

        #region GetAllManagersAsync Tests

        [Fact]
        public async Task GetAllManagersAsync_ReturnsListOfManagers()
        {
            // Arrange
            var managers = new List<ManagerDto>
            {
                new ManagerDto { Name = "John Manager", WorkGroup = "Operations", GradeCode = "M1" },
                new ManagerDto { Name = "Jane Director", WorkGroup = "Finance", GradeCode = "D1" }
            };
            var expectedResponse = ApiResponseDto<List<ManagerDto>>.SuccessResponse(managers);

            _fpsEmployeeApiClient.GetAllManagerAsync().Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetAllManagersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _fpsEmployeeApiClient.Received(1).GetAllManagerAsync();
        }

        [Fact]
        public async Task GetAllManagersAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<ManagerDto>>.SuccessResponse(new List<ManagerDto>());

            _fpsEmployeeApiClient.GetAllManagerAsync().Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetAllManagersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllManagersAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ManagerDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsEmployeeApiClient.GetAllManagerAsync().Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetAllManagersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetAllPactManagersAsync Tests

        [Fact]
        public async Task GetAllPactManagersAsync_ReturnsListOfManagers()
        {
            // Arrange
            var managers = new List<ManagerDto>
            {
                new ManagerDto { Name = "John Pact Manager", WorkGroup = "Operations", GradeCode = "M1" },
                new ManagerDto { Name = "Jane Pact Director", WorkGroup = "Finance", GradeCode = "D1" }
            };
            var expectedResponse = ApiResponseDto<List<ManagerDto>>.SuccessResponse(managers);

            _fpsEmployeeApiClient.GetAllPactManagerAsync().Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetAllPactManagersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _fpsEmployeeApiClient.Received(1).GetAllPactManagerAsync();
        }

        [Fact]
        public async Task GetAllPactManagersAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<ManagerDto>>.SuccessResponse(new List<ManagerDto>());

            _fpsEmployeeApiClient.GetAllPactManagerAsync().Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetAllPactManagersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllPactManagersAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ManagerDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsEmployeeApiClient.GetAllPactManagerAsync().Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetAllPactManagersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public async Task GetFilteredEmployeesAsync_CallsApiClientOnce()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(
                new List<EmployeeDto>(),
                new PaginationDto()
            );

            _fpsEmployeeApiClient.GetFilteredEmployeesAsync(queryParameters, 1)
                .Returns(expectedResponse);

            // Act
            await _employeeService.GetFilteredEmployeesAsync(queryParameters, 1);

            // Assert
            await _fpsEmployeeApiClient.Received(1).GetFilteredEmployeesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>());
        }

        [Fact]
        public async Task CreateEmployeeAsync_PassesExactEmployeeObject()
        {
            // Arrange
            var employee = new EmployeeDto
            {
                SPNumber = "000001",
                FirstName = "Test",
                LastName = "User",
                Title = "Tester"
            };
            var expectedResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(employee);

            _fpsEmployeeApiClient.CreateEmployeeAsync(employee).Returns(expectedResponse);

            // Act
            await _employeeService.CreateEmployeeAsync(employee);

            // Assert
            await _fpsEmployeeApiClient.Received(1).CreateEmployeeAsync(Arg.Is<EmployeeDto>(e =>
                e.SPNumber == employee.SPNumber &&
                e.FirstName == employee.FirstName &&
                e.LastName == employee.LastName &&
                e.Title == employee.Title
            ));
        }

        [Fact]
        public async Task UpdateEmployeeAsync_PassesExactEmployeeObject()
        {
            // Arrange
            var employee = new EmployeeDto
            {
                SPNumber = "000001",
                FirstName = "Updated",
                LastName = "User"
            };
            var expectedResponse = ApiResponseDto<EmployeeDto>.SuccessResponse(employee);

            _fpsEmployeeApiClient.UpdateEmployeeAsync(employee).Returns(expectedResponse);

            // Act
            await _employeeService.UpdateEmployeeAsync(employee);

            // Assert
            await _fpsEmployeeApiClient.Received(1).UpdateEmployeeAsync(Arg.Is<EmployeeDto>(e =>
                e.SPNumber == employee.SPNumber &&
                e.FirstName == employee.FirstName &&
                e.LastName == employee.LastName
            ));
        }

        #endregion

        #region GetAllWorkGroupPersonAsync Tests

        [Fact]
        public async Task GetAllPersonAsync_WithSuccessResponse_ReturnsPersonList()
        {
            // Arrange
            var persons = new List<WorkGroupPersonDto>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1", WorkGroup = "Group A" },
                new() { Name = "Bob",   WorkGroupGrade = "WG2", WorkGroup = "Group B" }
            };
            var expectedResponse = ApiResponseDto<List<WorkGroupPersonDto>>.SuccessResponse(persons);

            _fpsEmployeeApiClient.GetAllWorkGroupPersonAsync().Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetAllWorkGroupPersonAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.Equal("Alice", result.Data![0].Name);
            await _fpsEmployeeApiClient.Received(1).GetAllWorkGroupPersonAsync();
        }

        [Fact]
        public async Task GetAllPersonAsync_WithEmptyList_ReturnsEmptySuccessResponse()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<WorkGroupPersonDto>>.SuccessResponse([]);
            _fpsEmployeeApiClient.GetAllWorkGroupPersonAsync().Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetAllWorkGroupPersonAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _fpsEmployeeApiClient.Received(1).GetAllWorkGroupPersonAsync();
        }

        [Fact]
        public async Task GetAllPersonAsync_WithFailureResponse_ReturnsFailure()
        {
            // Arrange
            var failureResponse = ApiResponseDto<List<WorkGroupPersonDto>>.FailureResponse([], new ApiMetaDto());
            _fpsEmployeeApiClient.GetAllWorkGroupPersonAsync().Returns(failureResponse);

            // Act
            var result = await _employeeService.GetAllWorkGroupPersonAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _fpsEmployeeApiClient.Received(1).GetAllWorkGroupPersonAsync();
        }

        [Fact]
        public async Task GetAllPersonAsync_ClientThrows_PropagatesException()
        {
            // Arrange
            _fpsEmployeeApiClient.GetAllWorkGroupPersonAsync()
                .ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await _employeeService.GetAllWorkGroupPersonAsync());
            Assert.Equal("API unavailable", ex.Message);
            await _fpsEmployeeApiClient.Received(1).GetAllWorkGroupPersonAsync();
        }

        #endregion

        #region GetWorkGroupStaffAsync Tests

        [Fact]
        public async Task GetWorkGroupStaffAsync_WithNoWorkGroup_ReturnsSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var people = new List<PactStaffDto>
            {
                new()
                {
                    PactId       = "P001",
                    SpNumber     = "SP123",
                    Name         = "Alice",
                    WorkGroupGrade = "WG1",
                    Title        = "Senior Officer",
                    PersonStatus = "Active",
                    PersonClass  = "Permanent",
                    HrsPaid      = 37.5,
                    Leave        = 5.0,
                    SickSpecial  = 2.5,
                    HrsAvail     = 30.0
                },
                new()
                {
                    PactId       = "P002",
                    SpNumber     = "SP456",
                    Name         = "Bob",
                    WorkGroupGrade = "WG2",
                    Title        = "Officer",
                    PersonStatus = "Inactive",
                    PersonClass  = "Temporary",
                    HrsPaid      = null,
                    Leave        = null,
                    SickSpecial  = null,
                    HrsAvail     = null
                }
            };
            var paginatedResult = new PaginatedResult<PactStaffDto>(people, 2, 1, 10);
            var expectedResponse = ApiResponseDto<PaginatedResult<PactStaffDto>>
                .SuccessResponse(paginatedResult);

            _fpsEmployeeApiClient.GetWorkGroupStaffAsync(query, null).Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetWorkGroupStaffAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.data.Count());

            var first = result.Data!.data.First();
            Assert.Equal("P001",           first.PactId);
            Assert.Equal("SP123",          first.SpNumber);
            Assert.Equal("Alice",          first.Name);
            Assert.Equal("WG1",            first.WorkGroupGrade);
            Assert.Equal("Senior Officer", first.Title);
            Assert.Equal("Active",         first.PersonStatus);
            Assert.Equal("Permanent",      first.PersonClass);
            Assert.Equal(37.5,             first.HrsPaid);
            Assert.Equal(5.0,              first.Leave);
            Assert.Equal(2.5,              first.SickSpecial);
            Assert.Equal(30.0,             first.HrsAvail);

            var second = result.Data!.data.Last();
            Assert.Equal("P002",      second.PactId);
            Assert.Equal("SP456",     second.SpNumber);
            Assert.Equal("Bob",       second.Name);
            Assert.Equal("WG2",       second.WorkGroupGrade);
            Assert.Equal("Officer",   second.Title);
            Assert.Equal("Inactive",  second.PersonStatus);
            Assert.Equal("Temporary", second.PersonClass);
            Assert.Null(second.HrsPaid);
            Assert.Null(second.Leave);
            Assert.Null(second.SickSpecial);
            Assert.Null(second.HrsAvail);

            await _fpsEmployeeApiClient.Received(1).GetWorkGroupStaffAsync(query, null);
        }

        [Fact]
        public async Task GetWorkGroupStaffAsync_WithWorkGroup_PassesWorkGroupToClient()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginatedResult = new PaginatedResult<PactStaffDto>([], 0, 1, 10);
            var expectedResponse = ApiResponseDto<PaginatedResult<PactStaffDto>>
                .SuccessResponse(paginatedResult);

            _fpsEmployeeApiClient.GetWorkGroupStaffAsync(query, "WG1").Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetWorkGroupStaffAsync(query, "WG1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsEmployeeApiClient.Received(1).GetWorkGroupStaffAsync(query, "WG1");
        }

        [Fact]
        public async Task GetWorkGroupStaffAsync_WithEmptyResult_ReturnsEmptyPaginatedResult()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginatedResult = new PaginatedResult<PactStaffDto>([], 0, 1, 10);
            var expectedResponse = ApiResponseDto<PaginatedResult<PactStaffDto>>
                .SuccessResponse(paginatedResult);

            _fpsEmployeeApiClient.GetWorkGroupStaffAsync(query, null).Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetWorkGroupStaffAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!.data);
            await _fpsEmployeeApiClient.Received(1).GetWorkGroupStaffAsync(query, null);
        }

        [Fact]
        public async Task GetWorkGroupStaffAsync_WithFailureResponse_ReturnsFailure()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var failureResponse = ApiResponseDto<PaginatedResult<PactStaffDto>>.FailureResponse([], new ApiMetaDto());

            _fpsEmployeeApiClient.GetWorkGroupStaffAsync(query, null).Returns(failureResponse);

            // Act
            var result = await _employeeService.GetWorkGroupStaffAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetWorkGroupStaffAsync_ClientThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _fpsEmployeeApiClient
                .GetWorkGroupStaffAsync(query, null)
                .ThrowsAsync(new Exception("API error"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await _employeeService.GetWorkGroupStaffAsync(query));
            Assert.Equal("API error", ex.Message);
        }

        #endregion

        #region GetPactWorkGroupStaffAsync Tests

        [Fact]
        public async Task GetPactWorkGroupStaffAsync_WithWorkGroup_ReturnsSuccessResponse()
        {
            // Arrange
            const string workGroup = "WG1";
            var expectedResponse = ApiResponseDto<List<PactStaffDto>>.SuccessResponse(
                new List<PactStaffDto>
                {
                    new PactStaffDto { PactId = "P001", Name = "Alice", WorkGroupGrade = "WG1" }
                });

            _fpsEmployeeApiClient.GetPactWorkGroupStaffAsync(workGroup).Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetPactWorkGroupStaffAsync(workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsEmployeeApiClient.Received(1).GetPactWorkGroupStaffAsync(workGroup);
        }

        [Fact]
        public async Task GetPactWorkGroupStaffAsync_WithNullWorkGroup_ReturnsSuccessResponse()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<PactStaffDto>>.SuccessResponse([]);

            _fpsEmployeeApiClient.GetPactWorkGroupStaffAsync(null).Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetPactWorkGroupStaffAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _fpsEmployeeApiClient.Received(1).GetPactWorkGroupStaffAsync(null);
        }

        [Fact]
        public async Task GetPactWorkGroupStaffAsync_WhenClientThrows_PropagatesException()
        {
            // Arrange
            _fpsEmployeeApiClient.GetPactWorkGroupStaffAsync(Arg.Any<string?>())
                .ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await _employeeService.GetPactWorkGroupStaffAsync("WG1"));
            Assert.Equal("API unavailable", ex.Message);
        }

        #endregion

        #region GetPactStaffAsync Tests

        [Fact]
        public async Task GetPactStaffAsync_WithSuccessResponse_ReturnsPactStaffList()
        {
            // Arrange
            var staff = new List<PactStaffDto>
            {
                new PactStaffDto { PactId = "S001", SpNumber = "SP001", Name = "John Smith" },
                new PactStaffDto { PactId = "S002", SpNumber = "SP002", Name = "Jane Doe" }
            };
            var expectedResponse = ApiResponseDto<List<PactStaffDto>>.SuccessResponse(staff);

            _fpsEmployeeApiClient.GetPactStaffAsync().Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetPactStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.Equal("S001", result.Data![0].PactId);
            Assert.Equal("John Smith", result.Data![0].Name);
            await _fpsEmployeeApiClient.Received(1).GetPactStaffAsync();
        }

        [Fact]
        public async Task GetPactStaffAsync_WithEmptyList_ReturnsEmptySuccessResponse()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<PactStaffDto>>.SuccessResponse([]);

            _fpsEmployeeApiClient.GetPactStaffAsync().Returns(expectedResponse);

            // Act
            var result = await _employeeService.GetPactStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _fpsEmployeeApiClient.Received(1).GetPactStaffAsync();
        }

        [Fact]
        public async Task GetPactStaffAsync_WithFailureResponse_ReturnsFailure()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var failureResponse = ApiResponseDto<List<PactStaffDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsEmployeeApiClient.GetPactStaffAsync().Returns(failureResponse);

            // Act
            var result = await _employeeService.GetPactStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            await _fpsEmployeeApiClient.Received(1).GetPactStaffAsync();
        }

        [Fact]
        public async Task GetPactStaffAsync_ClientThrows_PropagatesException()
        {
            // Arrange
            _fpsEmployeeApiClient.GetPactStaffAsync()
                .ThrowsAsync(new Exception("API unavailable"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(
                async () => await _employeeService.GetPactStaffAsync());
            Assert.Equal("API unavailable", ex.Message);
            await _fpsEmployeeApiClient.Received(1).GetPactStaffAsync();
        }

        #endregion
    }
}