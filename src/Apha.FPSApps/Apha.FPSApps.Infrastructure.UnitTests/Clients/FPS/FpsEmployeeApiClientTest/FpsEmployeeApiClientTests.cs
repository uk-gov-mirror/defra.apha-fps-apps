using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsEmployeeApiClientTest
{
    public class FpsEmployeeApiClientTests
    {
        private readonly IFpsHttpExecutor _httpExecutor;
        private readonly IMapper _mapper;
        private readonly FpsEmployeeApiClient _client;

        public FpsEmployeeApiClientTests()
        {
            _httpExecutor = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsEmployeeApiClient(_httpExecutor, _mapper);
        }

        #region GetFilteredEmployeesAsync Tests

        [Fact]
        public async Task GetFilteredEmployeesAsync_WithSuccessResponse_ReturnsMappedEmployeeList()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var filterOption = 1;
            var employeeResList = new List<EmployeeRes>
            {
                new EmployeeRes { SPNumber = "000001", FirstName = "John", LastName = "Doe" },
                new EmployeeRes { SPNumber = "000002", FirstName = "Jane", LastName = "Smith" }
            };
            var apiResponse = new ApiResponse<List<EmployeeRes>>
            {
                Success = true,
                Data = employeeResList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(
                new List<EmployeeDto>
                {
                    new EmployeeDto { SPNumber = "000001", FirstName = "John", LastName = "Doe" },
                    new EmployeeDto { SPNumber = "000002", FirstName = "Jane", LastName = "Smith" }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _httpExecutor.GetAsync<List<EmployeeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<EmployeeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetFilteredEmployeesAsync(queryParameters, filterOption);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _httpExecutor.Received(1).GetAsync<List<EmployeeRes>>(Arg.Is<string>(url => url.Contains("filterOption=1")));
            _mapper.Received(1).Map<ApiResponseDto<List<EmployeeDto>>>(apiResponse);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetFilteredEmployeesAsync_WithDifferentFilterOptions_PassesCorrectUrl(int filterOption)
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<EmployeeRes>>
            {
                Success = true,
                Data = new List<EmployeeRes>()
            };
            var expectedDto = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(
                new List<EmployeeDto>(),
                new PaginationDto()
            );

            _httpExecutor.GetAsync<List<EmployeeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<EmployeeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetFilteredEmployeesAsync(queryParameters, filterOption);

            // Assert
            await _httpExecutor.Received(1).GetAsync<List<EmployeeRes>>(Arg.Is<string>(url => url.Contains($"filterOption={filterOption}")));
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR" } };
            var apiResponse = new ApiResponse<List<EmployeeRes>>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<List<EmployeeDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _httpExecutor.GetAsync<List<EmployeeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<EmployeeDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetFilteredEmployeesAsync(queryParameters, 1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetEmployeeIdAsync Tests

        [Fact]
        public async Task GetEmployeeIdAsync_WithValidSPNumber_ReturnsEmployee()
        {
            // Arrange
            var spNumber = "000001";
            var employeeRes = new EmployeeRes
            {
                SPNumber = spNumber,
                FirstName = "John",
                LastName = "Doe",
                Title = "Manager"
            };
            var apiResponse = new ApiResponse<EmployeeRes>
            {
                Success = true,
                Data = employeeRes
            };
            var expectedDto = ApiResponseDto<EmployeeDto>.SuccessResponse(
                new EmployeeDto { SPNumber = spNumber, FirstName = "John", LastName = "Doe", Title = "Manager" }
            );

            _httpExecutor.GetAsync<EmployeeRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<EmployeeDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetEmployeeIdAsync(spNumber);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(spNumber, result.Data.SPNumber);
            await _httpExecutor.Received(1).GetAsync<EmployeeRes>($"api/v1/employee/{spNumber}");
        }

        [Fact]
        public async Task GetEmployeeIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var spNumber = "999999";
            var errors = new List<ApiError> { new ApiError { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<EmployeeRes>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<EmployeeDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _httpExecutor.GetAsync<EmployeeRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<EmployeeDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetEmployeeIdAsync(spNumber);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("000001")]
        [InlineData("123456")]
        [InlineData("SP9999")]
        public async Task GetEmployeeIdAsync_WithVariousSPNumbers_CallsCorrectUrl(string spNumber)
        {
            // Arrange
            var apiResponse = new ApiResponse<EmployeeRes>
            {
                Success = true,
                Data = new EmployeeRes { SPNumber = spNumber }
            };
            var expectedDto = ApiResponseDto<EmployeeDto>.SuccessResponse(new EmployeeDto { SPNumber = spNumber });

            _httpExecutor.GetAsync<EmployeeRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<EmployeeDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetEmployeeIdAsync(spNumber);

            // Assert
            await _httpExecutor.Received(1).GetAsync<EmployeeRes>($"api/v1/employee/{spNumber}");
        }

        #endregion

        #region CreateEmployeeAsync Tests

        [Fact]
        public async Task CreateEmployeeAsync_WithValidEmployee_ReturnsCreatedEmployee()
        {
            // Arrange
            var employeeDto = new EmployeeDto
            {
                SPNumber = "000001",
                FirstName = "John",
                LastName = "Doe",
                Title = "Manager"
            };
            var employeeReq = new EmployeeReq
            {
                SPNumber = "000001",
                FirstName = "John",
                LastName = "Doe",
                Title = "Manager"
            };
            var employeeRes = new EmployeeRes
            {
                SPNumber = "000001",
                FirstName = "John",
                LastName = "Doe",
                Title = "Manager"
            };
            var apiResponse = new ApiResponse<EmployeeRes>
            {
                Success = true,
                Data = employeeRes
            };
            var expectedDto = ApiResponseDto<EmployeeDto>.SuccessResponse(employeeDto);

            _mapper.Map<EmployeeReq>(employeeDto).Returns(employeeReq);
            _httpExecutor.PostAsync<EmployeeReq, EmployeeRes>(Arg.Any<string>(), Arg.Any<EmployeeReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<EmployeeDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateEmployeeAsync(employeeDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("000001", result.Data.SPNumber);
            await _httpExecutor.Received(1).PostAsync<EmployeeReq, EmployeeRes>("api/v1/employee", employeeReq);
        }

        [Fact]
        public async Task CreateEmployeeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var employeeDto = new EmployeeDto { SPNumber = "000001" };
            var employeeReq = new EmployeeReq { SPNumber = "000001" };
            var errors = new List<ApiError> { new ApiError { Message = "Duplicate", Code = "DUPLICATE" } };
            var apiResponse = new ApiResponse<EmployeeRes>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<EmployeeDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Duplicate", Code = "DUPLICATE" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<EmployeeReq>(employeeDto).Returns(employeeReq);
            _httpExecutor.PostAsync<EmployeeReq, EmployeeRes>(Arg.Any<string>(), Arg.Any<EmployeeReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<EmployeeDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateEmployeeAsync(employeeDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task CreateEmployeeAsync_MapsEmployeeDtoToEmployeeReq()
        {
            // Arrange
            var employeeDto = new EmployeeDto
            {
                SPNumber = "000001",
                FirstName = "Test",
                LastName = "User"
            };
            var employeeReq = new EmployeeReq
            {
                SPNumber = "000001",
                FirstName = "Test",
                LastName = "User"
            };
            var apiResponse = new ApiResponse<EmployeeRes> { Success = true, Data = new EmployeeRes() };
            var expectedDto = ApiResponseDto<EmployeeDto>.SuccessResponse(employeeDto);

            _mapper.Map<EmployeeReq>(employeeDto).Returns(employeeReq);
            _httpExecutor.PostAsync<EmployeeReq, EmployeeRes>(Arg.Any<string>(), Arg.Any<EmployeeReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<EmployeeDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.CreateEmployeeAsync(employeeDto);

            // Assert
            _mapper.Received(1).Map<EmployeeReq>(employeeDto);
        }

        #endregion

        #region UpdateEmployeeAsync Tests

        [Fact]
        public async Task UpdateEmployeeAsync_WithValidEmployee_ReturnsUpdatedEmployee()
        {
            // Arrange
            var employeeDto = new EmployeeDto
            {
                SPNumber = "000001",
                FirstName = "Jane",
                LastName = "Smith",
                Title = "Director"
            };
            var employeeReq = new EmployeeReq
            {
                SPNumber = "000001",
                FirstName = "Jane",
                LastName = "Smith",
                Title = "Director"
            };
            var employeeRes = new EmployeeRes
            {
                SPNumber = "000001",
                FirstName = "Jane",
                LastName = "Smith",
                Title = "Director"
            };
            var apiResponse = new ApiResponse<EmployeeRes>
            {
                Success = true,
                Data = employeeRes
            };
            var expectedDto = ApiResponseDto<EmployeeDto>.SuccessResponse(employeeDto);

            _mapper.Map<EmployeeReq>(employeeDto).Returns(employeeReq);
            _httpExecutor.PutAsync<EmployeeReq, EmployeeRes>(Arg.Any<string>(), Arg.Any<EmployeeReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<EmployeeDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateEmployeeAsync(employeeDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Jane", result.Data.FirstName);
            await _httpExecutor.Received(1).PutAsync<EmployeeReq, EmployeeRes>("api/v1/employee", employeeReq);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var employeeDto = new EmployeeDto { SPNumber = "999999" };
            var employeeReq = new EmployeeReq { SPNumber = "999999" };
            var errors = new List<ApiError> { new ApiError { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<EmployeeRes>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<EmployeeDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<EmployeeReq>(employeeDto).Returns(employeeReq);
            _httpExecutor.PutAsync<EmployeeReq, EmployeeRes>(Arg.Any<string>(), Arg.Any<EmployeeReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<EmployeeDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateEmployeeAsync(employeeDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteEmployeeAsync Tests

        [Fact]
        public async Task DeleteEmployeeAsync_WithValidSPNumber_ReturnsSuccess()
        {
            // Arrange
            var spNumber = "000001";
            var apiResponse = new ApiResponse<bool?>
            {
                Success = true,
                Data = true
            };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _httpExecutor.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteEmployeeAsync(spNumber);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _httpExecutor.Received(1).DeleteAsync<bool?>($"api/v1/employee/{spNumber}");
        }

        [Fact]
        public async Task DeleteEmployeeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var spNumber = "999999";
            var errors = new List<ApiError> { new ApiError { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<bool?>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _httpExecutor.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteEmployeeAsync(spNumber);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("000001")]
        [InlineData("123456")]
        [InlineData("SP9999")]
        public async Task DeleteEmployeeAsync_WithVariousSPNumbers_CallsCorrectUrl(string spNumber)
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _httpExecutor.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.DeleteEmployeeAsync(spNumber);

            // Assert
            await _httpExecutor.Received(1).DeleteAsync<bool?>($"api/v1/employee/{spNumber}");
        }

        #endregion

        #region GetAllManagerAsync Tests

        [Fact]
        public async Task GetAllManagerAsync_WithSuccessResponse_ReturnsManagerList()
        {
            // Arrange
            var managerResList = new List<ManagerRes>
            {
                new ManagerRes { Name = "John Manager", WorkGroup = "Operations", GradeCode = "M1" },
                new ManagerRes { Name = "Jane Director", WorkGroup = "Finance", GradeCode = "D1" }
            };
            var apiResponse = new ApiResponse<List<ManagerRes>>
            {
                Success = true,
                Data = managerResList
            };
            var expectedDto = ApiResponseDto<List<ManagerDto>>.SuccessResponse(
                new List<ManagerDto>
                {
                    new ManagerDto { Name = "John Manager", WorkGroup = "Operations", GradeCode = "M1" },
                    new ManagerDto { Name = "Jane Director", WorkGroup = "Finance", GradeCode = "D1" }
                }
            );

            _httpExecutor.GetAsync<List<ManagerRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ManagerDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllManagerAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _httpExecutor.Received(1).GetAsync<List<ManagerRes>>("api/v1/employee/managers");
        }

        [Fact]
        public async Task GetAllManagerAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ManagerRes>>
            {
                Success = true,
                Data = new List<ManagerRes>()
            };
            var expectedDto = ApiResponseDto<List<ManagerDto>>.SuccessResponse(new List<ManagerDto>());

            _httpExecutor.GetAsync<List<ManagerRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ManagerDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllManagerAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllManagerAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR" } };
            var apiResponse = new ApiResponse<List<ManagerRes>>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<List<ManagerDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _httpExecutor.GetAsync<List<ManagerRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ManagerDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllManagerAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetAllPactManagerAsync Tests

        [Fact]
        public async Task GetAllPactManagerAsync_WithSuccessResponse_ReturnsPactManagerList()
        {
            // Arrange
            var managerResList = new List<ManagerRes>
            {
                new ManagerRes { Name = "PACT Manager One", WorkGroup = "Operations", GradeCode = "M1" },
                new ManagerRes { Name = "PACT Manager Two", WorkGroup = "Finance", GradeCode = "M2" }
            };
            var apiResponse = new ApiResponse<List<ManagerRes>> { Success = true, Data = managerResList };
            var expectedDto = ApiResponseDto<List<ManagerDto>>.SuccessResponse(
                new List<ManagerDto>
                {
                    new ManagerDto { Name = "PACT Manager One", WorkGroup = "Operations", GradeCode = "M1" },
                    new ManagerDto { Name = "PACT Manager Two", WorkGroup = "Finance", GradeCode = "M2" }
                }
            );

            _httpExecutor.GetAsync<List<ManagerRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ManagerDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllPactManagerAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _httpExecutor.Received(1).GetAsync<List<ManagerRes>>("api/v1/employee/pactmanagers");
        }

        [Fact]
        public async Task GetAllPactManagerAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ManagerRes>> { Success = true, Data = new List<ManagerRes>() };
            var expectedDto = ApiResponseDto<List<ManagerDto>>.SuccessResponse(new List<ManagerDto>());

            _httpExecutor.GetAsync<List<ManagerRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ManagerDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllPactManagerAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllPactManagerAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR" } };
            var apiResponse = new ApiResponse<List<ManagerRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ManagerDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _httpExecutor.GetAsync<List<ManagerRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ManagerDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllPactManagerAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetPactWorkGroupStaffAsync Tests

        [Fact]
        public async Task GetPactWorkGroupStaffAsync_WithSuccessResponse_ReturnsMappedPactStaffList()
        {
            // Arrange
            const string workGroup = "WG1";
            var apiResponse = new ApiResponse<List<PactStaffRes>>
            {
                Success = true,
                Data = new List<PactStaffRes>
                {
                    new PactStaffRes { PactId = "P001", Name = "Alice", WorkGroupGrade = "WG1" }
                }
            };
            var expectedDto = ApiResponseDto<List<PactStaffDto>>.SuccessResponse(
                new List<PactStaffDto>
                {
                    new PactStaffDto { PactId = "P001", Name = "Alice", WorkGroupGrade = "WG1" }
                });

            _httpExecutor.GetAsync<List<PactStaffRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<PactStaffDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPactWorkGroupStaffAsync(workGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _httpExecutor.Received(1).GetAsync<List<PactStaffRes>>(Arg.Is<string>(x => x.Contains("PactWorkGroupStaff") && x.Contains(workGroup)));
            _mapper.Received(1).Map<ApiResponseDto<List<PactStaffDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetPactWorkGroupStaffAsync_WithNullWorkGroup_ReturnsMappedResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<PactStaffRes>>
            {
                Success = true,
                Data = new List<PactStaffRes>()
            };
            var expectedDto = ApiResponseDto<List<PactStaffDto>>.SuccessResponse([]);

            _httpExecutor.GetAsync<List<PactStaffRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<PactStaffDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPactWorkGroupStaffAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPactWorkGroupStaffAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<PactStaffRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<List<PactStaffDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _httpExecutor.GetAsync<List<PactStaffRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<PactStaffDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPactWorkGroupStaffAsync("WG1");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetPactStaffAsync Tests

        [Fact]
        public async Task GetPactStaffAsync_WithSuccessResponse_ReturnsMappedPactStaffList()
        {
            // Arrange
            var pactStaffResList = new List<PactStaffRes>
            {
                new PactStaffRes { PactId = "S001", SpNumber = "SP001", Name = "John Smith" },
                new PactStaffRes { PactId = "S002", SpNumber = "SP002", Name = "Jane Doe" }
            };
            var apiResponse = new ApiResponse<List<PactStaffRes>> { Success = true, Data = pactStaffResList };
            var expectedDto = ApiResponseDto<List<PactStaffDto>>.SuccessResponse(
                new List<PactStaffDto>
                {
                    new PactStaffDto { PactId = "S001", SpNumber = "SP001", Name = "John Smith" },
                    new PactStaffDto { PactId = "S002", SpNumber = "SP002", Name = "Jane Doe" }
                }
            );

            _httpExecutor.GetAsync<List<PactStaffRes>>("api/v1/employee/pactstaff").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<PactStaffDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPactStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _httpExecutor.Received(1).GetAsync<List<PactStaffRes>>("api/v1/employee/pactstaff");
        }

        [Fact]
        public async Task GetPactStaffAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<PactStaffRes>> { Success = true, Data = new List<PactStaffRes>() };
            var expectedDto = ApiResponseDto<List<PactStaffDto>>.SuccessResponse(new List<PactStaffDto>());

            _httpExecutor.GetAsync<List<PactStaffRes>>("api/v1/employee/pactstaff").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<PactStaffDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPactStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _httpExecutor.Received(1).GetAsync<List<PactStaffRes>>("api/v1/employee/pactstaff");
        }

        [Fact]
        public async Task GetPactStaffAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR" } };
            var apiResponse = new ApiResponse<List<PactStaffRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<PactStaffDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _httpExecutor.GetAsync<List<PactStaffRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<PactStaffDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPactStaffAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetPactStaffAsync_MapsApiResponseToPactStaffDtoList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<PactStaffRes>>
            {
                Success = true,
                Data = new List<PactStaffRes> { new PactStaffRes { PactId = "S001", Name = "John Smith" } }
            };
            var expectedDto = ApiResponseDto<List<PactStaffDto>>.SuccessResponse(
                new List<PactStaffDto> { new PactStaffDto { PactId = "S001", Name = "John Smith" } }
            );

            _httpExecutor.GetAsync<List<PactStaffRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<PactStaffDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetPactStaffAsync();

            // Assert
            _mapper.Received(1).Map<ApiResponseDto<List<PactStaffDto>>>(apiResponse);
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public async Task GetFilteredEmployeesAsync_ConstructsUrlWithQueryParameters()
        {
            // Arrange
            var queryParameters = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 20,
                Search = "John",
                SortBy = "LastName",
                Descending = true
            };
            var apiResponse = new ApiResponse<List<EmployeeRes>> { Success = true, Data = new List<EmployeeRes>() };
            var expectedDto = ApiResponseDto<List<EmployeeDto>>.SuccessResponse(new List<EmployeeDto>(), new PaginationDto());

            _httpExecutor.GetAsync<List<EmployeeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<EmployeeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetFilteredEmployeesAsync(queryParameters, 1);

            // Assert
            await _httpExecutor.Received(1).GetAsync<List<EmployeeRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/employee/paginated") &&
                url.Contains("filterOption=1")
            ));
        }

        [Fact]
        public async Task CreateEmployeeAsync_CallsMapperBeforeHttpExecutor()
        {
            // Arrange
            var employeeDto = new EmployeeDto { SPNumber = "000001" };
            var employeeReq = new EmployeeReq { SPNumber = "000001" };
            var apiResponse = new ApiResponse<EmployeeRes> { Success = true, Data = new EmployeeRes() };
            var expectedDto = ApiResponseDto<EmployeeDto>.SuccessResponse(employeeDto);

            _mapper.Map<EmployeeReq>(employeeDto).Returns(employeeReq);
            _httpExecutor.PostAsync<EmployeeReq, EmployeeRes>(Arg.Any<string>(), Arg.Any<EmployeeReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<EmployeeDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.CreateEmployeeAsync(employeeDto);

            // Assert
            Received.InOrder(() =>
            {
                _mapper.Map<EmployeeReq>(employeeDto);
                _httpExecutor.PostAsync<EmployeeReq, EmployeeRes>("api/v1/employee", employeeReq);
            });
        }

        [Fact]
        public async Task UpdateEmployeeAsync_CallsMapperBeforeHttpExecutor()
        {
            // Arrange
            var employeeDto = new EmployeeDto { SPNumber = "000001" };
            var employeeReq = new EmployeeReq { SPNumber = "000001" };
            var apiResponse = new ApiResponse<EmployeeRes> { Success = true, Data = new EmployeeRes() };
            var expectedDto = ApiResponseDto<EmployeeDto>.SuccessResponse(employeeDto);

            _mapper.Map<EmployeeReq>(employeeDto).Returns(employeeReq);
            _httpExecutor.PutAsync<EmployeeReq, EmployeeRes>(Arg.Any<string>(), Arg.Any<EmployeeReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<EmployeeDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.UpdateEmployeeAsync(employeeDto);

            // Assert
            Received.InOrder(() =>
            {
                _mapper.Map<EmployeeReq>(employeeDto);
                _httpExecutor.PutAsync<EmployeeReq, EmployeeRes>("api/v1/employee", employeeReq);
            });
        }

        #endregion
    }
}