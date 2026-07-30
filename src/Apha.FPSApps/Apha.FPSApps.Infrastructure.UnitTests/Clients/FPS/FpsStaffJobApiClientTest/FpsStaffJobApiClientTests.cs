using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsStaffJobApiClientTest
{
    public class FpsStaffJobApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsStaffJobApiClient _client;

        public FpsStaffJobApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsStaffJobApiClient(_http, _mapper);
        }

        #region GetAllStaffJobAsync Tests

        [Fact]
        public async Task GetAllStaffJobAsync_WithSuccessResponse_ReturnsMappedStaffJobList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Search = "Test" };
            var jobCode = "JOB001";
            var staffJobViewResList = new List<StaffJobViewRes>
            {
                new StaffJobViewRes { StaffID = "S001", JobCode = jobCode, PlannedHours = 8 },
                new StaffJobViewRes { StaffID = "S002", JobCode = jobCode, PlannedHours = 6 }
            };
            var apiResponse = new ApiResponse<List<StaffJobViewRes>>
            {
                Success = true,
                Data = staffJobViewResList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<StaffJobViewDto>>.SuccessResponse(
                new List<StaffJobViewDto>
                {
                    new StaffJobViewDto { StaffID = "S001", JobCode = jobCode, PlannedHours = 8 },
                    new StaffJobViewDto { StaffID = "S002", JobCode = jobCode, PlannedHours = 6 }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _http.GetAsync<List<StaffJobViewRes>>(Arg.Is<string>(url => url.Contains($"jobCode={jobCode}"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffJobViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllStaffJobAsync(query, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<StaffJobViewRes>>(Arg.Is<string>(url => url.Contains($"jobCode={jobCode}")));
            _mapper.Received(1).Map<ApiResponseDto<List<StaffJobViewDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllStaffJobAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var jobCode = "JOB001";
            var errors = new List<ApiError> { new ApiError { Message = "API Error", Code = "ERROR" } };
            var apiResponse = new ApiResponse<List<StaffJobViewRes>>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<List<StaffJobViewDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<StaffJobViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffJobViewDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllStaffJobAsync(query, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("JOB001")]
        [InlineData("FZ2000")]
        [InlineData("TEST_JOB")]
        public async Task GetAllStaffJobAsync_WithVariousJobCodes_ConstructsUrlWithJobCode(string jobCode)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<StaffJobViewRes>> { Success = true, Data = new List<StaffJobViewRes>() };
            var expectedDto = ApiResponseDto<List<StaffJobViewDto>>.SuccessResponse(new List<StaffJobViewDto>(), new PaginationDto());

            _http.GetAsync<List<StaffJobViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffJobViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetAllStaffJobAsync(query, jobCode);

            // Assert
            await _http.Received(1).GetAsync<List<StaffJobViewRes>>(Arg.Is<string>(url => url.Contains($"jobCode={jobCode}")));
        }

        #endregion

        #region GetStaffWorkgroupLookupAsync Tests

        [Fact]
        public async Task GetStaffWorkgroupLookupAsync_WithSuccessResponse_ReturnsMappedLookupData()
        {
            // Arrange
            var workgroupResList = new List<StaffWorkgroupLookupRes>
            {
                new StaffWorkgroupLookupRes { StaffID = "S001", Name = "John Doe", WorkGroupGrade = "Grade A" },
                new StaffWorkgroupLookupRes { StaffID = "S002", Name = "Jane Smith", WorkGroupGrade = "Grade B" }
            };
            var apiResponse = new ApiResponse<IEnumerable<StaffWorkgroupLookupRes>>
            {
                Success = true,
                Data = workgroupResList
            };
            var expectedDto = ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>.SuccessResponse(
                new List<StaffWorkgroupLookupDto>
                {
                    new StaffWorkgroupLookupDto { StaffID = "S001", Name = "John Doe", WorkGroupGrade = "Grade A" },
                    new StaffWorkgroupLookupDto { StaffID = "S002", Name = "Jane Smith", WorkGroupGrade = "Grade B" }
                }
            );

            _http.GetAsync<IEnumerable<StaffWorkgroupLookupRes>>("api/v1/staffjob/workgrouplookup").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetStaffWorkgroupLookupAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count());
            await _http.Received(1).GetAsync<IEnumerable<StaffWorkgroupLookupRes>>("api/v1/staffjob/workgrouplookup");
            _mapper.Received(1).Map<ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetStaffWorkgroupLookupAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<IEnumerable<StaffWorkgroupLookupRes>>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<IEnumerable<StaffWorkgroupLookupRes>>("api/v1/staffjob/workgrouplookup").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetStaffWorkgroupLookupAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetStaffChargeRate Tests

        [Fact]
        public async Task GetStaffChargeRate_WithValidIds_ReturnsChargeRate()
        {
            // Arrange
            var staffId = "S001";
            var jobCode = "JOB001";
            var chargeRate = 125.50m;
            var apiResponse = new ApiResponse<decimal?>
            {
                Success = true,
                Data = chargeRate
            };
            var expectedDto = ApiResponseDto<decimal?>.SuccessResponse(chargeRate);

            _http.GetAsync<decimal?>($"api/v1/staffjob/chargerate?staffId={staffId}&jobcode={jobCode}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal?>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetStaffChargeRate(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(chargeRate, result.Data);
            await _http.Received(1).GetAsync<decimal?>($"api/v1/staffjob/chargerate?staffId={staffId}&jobcode={jobCode}");
        }

        [Fact]
        public async Task GetStaffChargeRate_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var staffId = "S001";
            var jobCode = "JOB001";
            var errors = new List<ApiError> { new ApiError { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<decimal?>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<decimal?>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<decimal?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal?>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetStaffChargeRate(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("S001", "JOB001")]
        [InlineData("S999", "FZ2000")]
        [InlineData("EMP123", "TEST_JOB")]
        public async Task GetStaffChargeRate_WithVariousIds_CallsCorrectUrl(string staffId, string jobCode)
        {
            // Arrange
            var apiResponse = new ApiResponse<decimal?> { Success = true, Data = 100m };
            var expectedDto = ApiResponseDto<decimal?>.SuccessResponse(100m);

            _http.GetAsync<decimal?>($"api/v1/staffjob/chargerate?staffId={staffId}&jobcode={jobCode}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal?>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetStaffChargeRate(staffId, jobCode);

            // Assert
            await _http.Received(1).GetAsync<decimal?>($"api/v1/staffjob/chargerate?staffId={staffId}&jobcode={jobCode}");
        }

        #endregion

        #region GetStaffJobByIdAsync Tests

        [Fact]
        public async Task GetStaffJobByIdAsync_WithValidIds_ReturnsStaffJob()
        {
            // Arrange
            var staffId = "S001";
            var jobCode = "JOB001";
            var staffJobRes = new StaffJobRes { StaffId = staffId, JobCode = jobCode, PlannedHours = 8 };
            var apiResponse = new ApiResponse<StaffJobRes>
            {
                Success = true,
                Data = staffJobRes
            };
            var expectedDto = ApiResponseDto<StaffJobDto>.SuccessResponse(
                new StaffJobDto { StaffId = staffId, JobCode = jobCode, PlannedHours = 8 }
            );

            _http.GetAsync<StaffJobRes>($"api/v1/staffjob/{staffId}/{jobCode}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StaffJobDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetStaffJobByIdAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(staffId, result.Data.StaffId);
            Assert.Equal(jobCode, result.Data.JobCode);
            await _http.Received(1).GetAsync<StaffJobRes>($"api/v1/staffjob/{staffId}/{jobCode}");
        }

        [Fact]
        public async Task GetStaffJobByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new ApiError { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<StaffJobRes>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<StaffJobDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<StaffJobRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StaffJobDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetStaffJobByIdAsync("S001", "JOB001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("S001", "JOB001")]
        [InlineData("S999", "FZ2000")]
        [InlineData("EMP123", "TEST_JOB")]
        public async Task GetStaffJobByIdAsync_WithVariousIds_CallsCorrectUrl(string staffId, string jobCode)
        {
            // Arrange
            var apiResponse = new ApiResponse<StaffJobRes>
            {
                Success = true,
                Data = new StaffJobRes { StaffId = staffId, JobCode = jobCode }
            };
            var expectedDto = ApiResponseDto<StaffJobDto>.SuccessResponse(new StaffJobDto { StaffId = staffId, JobCode = jobCode });

            _http.GetAsync<StaffJobRes>($"api/v1/staffjob/{staffId}/{jobCode}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StaffJobDto>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetStaffJobByIdAsync(staffId, jobCode);

            // Assert
            await _http.Received(1).GetAsync<StaffJobRes>($"api/v1/staffjob/{staffId}/{jobCode}");
        }

        #endregion

        #region CreateStaffJobAsync Tests

        [Fact]
        public async Task CreateStaffJobAsync_WithValidStaffJob_ReturnsCreatedStaffJob()
        {
            // Arrange
            var staffJobDto = new StaffJobDto { StaffId = "S001", JobCode = "JOB001", PlannedHours = 8 };
            var staffJobReq = new StaffJobReq { StaffId = "S001", JobCode = "JOB001", PlannedHours = 8 };
            var apiResponse = new ApiResponse<StaffJobRes>
            {
                Success = true,
                Data = new StaffJobRes { StaffId = "S001", JobCode = "JOB001", PlannedHours = 8 }
            };
            var expectedDto = ApiResponseDto<StaffJobDto>.SuccessResponse(staffJobDto);

            _mapper.Map<StaffJobReq>(staffJobDto).Returns(staffJobReq);
            _http.PostAsync<StaffJobReq, StaffJobRes>("api/v1/staffjob", staffJobReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StaffJobDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateStaffJobAsync(staffJobDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("S001", result.Data.StaffId);
            Assert.Equal("JOB001", result.Data.JobCode);
            await _http.Received(1).PostAsync<StaffJobReq, StaffJobRes>("api/v1/staffjob", staffJobReq);
        }

        [Fact]
        public async Task CreateStaffJobAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var staffJobDto = new StaffJobDto { StaffId = "S001", JobCode = "JOB001" };
            var staffJobReq = new StaffJobReq { StaffId = "S001", JobCode = "JOB001" };
            var errors = new List<ApiError> { new ApiError { Message = "Duplicate", Code = "DUPLICATE" } };
            var apiResponse = new ApiResponse<StaffJobRes>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<StaffJobDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Duplicate", Code = "DUPLICATE" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<StaffJobReq>(staffJobDto).Returns(staffJobReq);
            _http.PostAsync<StaffJobReq, StaffJobRes>("api/v1/staffjob", staffJobReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StaffJobDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateStaffJobAsync(staffJobDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateStaffJobAsync Tests

        [Fact]
        public async Task UpdateStaffJobAsync_WithValidStaffJob_ReturnsUpdatedStaffJob()
        {
            // Arrange
            var staffJobDto = new StaffJobDto { StaffId = "S001", JobCode = "JOB001", PlannedHours = 10 };
            var staffJobReq = new StaffJobReq { StaffId = "S001", JobCode = "JOB001", PlannedHours = 10 };
            var apiResponse = new ApiResponse<StaffJobRes>
            {
                Success = true,
                Data = new StaffJobRes { StaffId = "S001", JobCode = "JOB001", PlannedHours = 10 }
            };
            var expectedDto = ApiResponseDto<StaffJobDto>.SuccessResponse(staffJobDto);

            _mapper.Map<StaffJobReq>(staffJobDto).Returns(staffJobReq);
            _http.PutAsync<StaffJobReq, StaffJobRes>("api/v1/staffjob", staffJobReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StaffJobDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateStaffJobAsync(staffJobDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(10, result.Data.PlannedHours);
            await _http.Received(1).PutAsync<StaffJobReq, StaffJobRes>("api/v1/staffjob", staffJobReq);
        }

        [Fact]
        public async Task UpdateStaffJobAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var staffJobDto = new StaffJobDto { StaffId = "S001", JobCode = "JOB001" };
            var staffJobReq = new StaffJobReq { StaffId = "S001", JobCode = "JOB001" };
            var errors = new List<ApiError> { new ApiError { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<StaffJobRes>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<StaffJobDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<StaffJobReq>(staffJobDto).Returns(staffJobReq);
            _http.PutAsync<StaffJobReq, StaffJobRes>("api/v1/staffjob", staffJobReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StaffJobDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateStaffJobAsync(staffJobDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteStaffJobAsync Tests

        [Fact]
        public async Task DeleteStaffJobAsync_WithValidIds_ReturnsSuccess()
        {
            // Arrange
            var staffId = "S001";
            var jobCode = "JOB001";
            var apiResponse = new ApiResponse<bool?>
            {
                Success = true,
                Data = true
            };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>($"api/v1/staffjob?staffId={staffId}&jobcode={jobCode}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteStaffJobAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool?>($"api/v1/staffjob?staffId={staffId}&jobcode={jobCode}");
        }

        [Fact]
        public async Task DeleteStaffJobAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var staffId = "S001";
            var jobCode = "JOB001";
            var errors = new List<ApiError> { new ApiError { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<bool?>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteStaffJobAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("S001", "JOB001")]
        [InlineData("S999", "FZ2000")]
        [InlineData("EMP123", "TEST_JOB")]
        public async Task DeleteStaffJobAsync_WithVariousIds_CallsCorrectUrl(string staffId, string jobCode)
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>($"api/v1/staffjob?staffId={staffId}&jobcode={jobCode}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.DeleteStaffJobAsync(staffId, jobCode);

            // Assert
            await _http.Received(1).DeleteAsync<bool?>($"api/v1/staffjob?staffId={staffId}&jobcode={jobCode}");
        }

        #endregion

        #region GetViewByStaffIdAsync Tests

        [Fact]
        public async Task GetViewByStaffIdAsync_WithValidIds_ReturnsStaffJobView()
        {
            // Arrange
            var staffId = "S001";
            var jobCode = "JOB001";
            var staffJobViewRes = new StaffJobViewRes
            {
                StaffID = staffId,
                JobCode = jobCode,
                PlannedHours = 8,
                Name = "John Doe",
                WorkGroupGrade = "Grade A",
                ChargeRate = 125.50m
            };
            var apiResponse = new ApiResponse<StaffJobViewRes>
            {
                Success = true,
                Data = staffJobViewRes
            };
            var mappedDto = new StaffJobViewDto
            {
                StaffID = staffId,
                JobCode = jobCode,
                PlannedHours = 8,
                Name = "John Doe",
                WorkGroupGrade = "Grade A",
                ChargeRate = 125.50m
            };

            _http.GetAsync<StaffJobViewRes>($"api/v1/staffjob/view?staffId={staffId}&jobcode={jobCode}").Returns(apiResponse);
            _mapper.Map<StaffJobViewDto>(staffJobViewRes).Returns(mappedDto);

            // Act
            var result = await _client.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(staffId, result.Data.StaffID);
            Assert.Equal(jobCode, result.Data.JobCode);
            Assert.Equal(8, result.Data.PlannedHours);
            Assert.Equal("John Doe", result.Data.Name);
            Assert.Equal("Grade A", result.Data.WorkGroupGrade);
            Assert.Equal(125.50m, result.Data.ChargeRate);
            await _http.Received(1).GetAsync<StaffJobViewRes>($"api/v1/staffjob/view?staffId={staffId}&jobcode={jobCode}");
            _mapper.Received(1).Map<StaffJobViewDto>(staffJobViewRes);
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var staffId = "S001";
            var jobCode = "JOB001";
            var errors = new List<ApiError> { new ApiError { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<StaffJobViewRes>
            {
                Success = false,
                Errors = errors,
                Data = null
            };
            var mappedResponse = new ApiResponseDto<StaffJobViewDto?>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<StaffJobViewRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StaffJobViewDto?>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_WhenApiReturnsNullData_ReturnsSuccessWithNull()
        {
            // Arrange
            var staffId = "S001";
            var jobCode = "JOB001";
            var apiResponse = new ApiResponse<StaffJobViewRes>
            {
                Success = true,
                Data = null
            };

            _http.GetAsync<StaffJobViewRes>($"api/v1/staffjob/view?staffId={staffId}&jobcode={jobCode}").Returns(apiResponse);

            // Act
            var result = await _client.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Null(result.Data);
        }

        [Theory]
        [InlineData("S001", "JOB001")]
        [InlineData("S999", "FZ2000")]
        [InlineData("EMP123", "TEST_JOB")]
        public async Task GetViewByStaffIdAsync_WithVariousIds_CallsCorrectUrl(string staffId, string jobCode)
        {
            // Arrange
            var staffJobViewRes = new StaffJobViewRes { StaffID = staffId, JobCode = jobCode };
            var apiResponse = new ApiResponse<StaffJobViewRes>
            {
                Success = true,
                Data = staffJobViewRes
            };
            var expectedDto = new StaffJobViewDto { StaffID = staffId, JobCode = jobCode };

            _http.GetAsync<StaffJobViewRes>($"api/v1/staffjob/view?staffId={staffId}&jobcode={jobCode}").Returns(apiResponse);
            _mapper.Map<StaffJobViewDto>(staffJobViewRes).Returns(expectedDto);

            // Act
            await _client.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            await _http.Received(1).GetAsync<StaffJobViewRes>($"api/v1/staffjob/view?staffId={staffId}&jobcode={jobCode}");
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_WithAllProperties_MapsCorrectly()
        {
            // Arrange
            var staffId = "S001";
            var jobCode = "JOB001";
            var staffJobViewRes = new StaffJobViewRes
            {
                StaffID = staffId,
                JobCode = jobCode,
                PlannedHours = 40,
                Name = "Jane Smith",
                WorkGroupGrade = "Senior Grade",
                ChargeRate = 150.75m,
                StaffCost = 6030.00m,
                GradeCode = "SG01",
                WorkGroup = "Development",
                SectorName = "Technology",
                Days = 5
            };
            var apiResponse = new ApiResponse<StaffJobViewRes>
            {
                Success = true,
                Data = staffJobViewRes
            };
            var expectedDto = new StaffJobViewDto
            {
                StaffID = staffId,
                JobCode = jobCode,
                PlannedHours = 40,
                Name = "Jane Smith",
                WorkGroupGrade = "Senior Grade",
                ChargeRate = 150.75m,
                StaffCost = 6030.00m,
                GradeCode = "SG01",
                WorkGroup = "Development",
                SectorName = "Technology",
                Days = 5
            };

            _http.GetAsync<StaffJobViewRes>($"api/v1/staffjob/view?staffId={staffId}&jobcode={jobCode}").Returns(apiResponse);
            _mapper.Map<StaffJobViewDto>(staffJobViewRes).Returns(expectedDto);

            // Act
            var result = await _client.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(staffId, result.Data.StaffID);
            Assert.Equal(jobCode, result.Data.JobCode);
            Assert.Equal(40, result.Data.PlannedHours);
            Assert.Equal("Jane Smith", result.Data.Name);
            Assert.Equal("Senior Grade", result.Data.WorkGroupGrade);
            Assert.Equal(150.75m, result.Data.ChargeRate);
            Assert.Equal(6030.00m, result.Data.StaffCost);
            Assert.Equal("SG01", result.Data.GradeCode);
            Assert.Equal("Development", result.Data.WorkGroup);
            Assert.Equal("Technology", result.Data.SectorName);
            Assert.Equal(5, result.Data.Days);
        }

        #endregion

        #region GetTotalStaffCostAsync Tests

        [Fact]
        public async Task GetTotalStaffCostAsync_WithSuccessResponse_ReturnsTotalStaffCost()
        {
            // Arrange
            var jobCode = "JOB001";
            var total = 4500m;
            var apiResponse = new ApiResponse<decimal>
            {
                Success = true,
                Data = total
            };
            var expectedDto = ApiResponseDto<decimal>.SuccessResponse(total);

            _http.GetAsync<decimal>($"api/v1/staffjob/totalstaffcost?jobCode={jobCode}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTotalStaffCostAsync(jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(total, result.Data);
            await _http.Received(1).GetAsync<decimal>($"api/v1/staffjob/totalstaffcost?jobCode={jobCode}");
        }

        [Fact]
        public async Task GetTotalStaffCostAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var jobCode = "JOB001";
            var errors = new List<ApiError> { new ApiError { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<decimal>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<decimal>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<decimal>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetTotalStaffCostAsync(jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("JOB001")]
        [InlineData("FZ2000")]
        [InlineData("TEST_JOB")]
        public async Task GetTotalStaffCostAsync_WithVariousJobCodes_ConstructsUrlWithJobCode(string jobCode)
        {
            // Arrange
            var apiResponse = new ApiResponse<decimal> { Success = true, Data = 100m };
            var expectedDto = ApiResponseDto<decimal>.SuccessResponse(100m);

            _http.GetAsync<decimal>($"api/v1/staffjob/totalstaffcost?jobCode={jobCode}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<decimal>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetTotalStaffCostAsync(jobCode);

            // Assert
            await _http.Received(1).GetAsync<decimal>($"api/v1/staffjob/totalstaffcost?jobCode={jobCode}");
        }

        #endregion

        #region GetStaffResourceUtilisationAsync

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            const string workgroup = "WG01";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<StaffResourceUtilisationRes>
            {
                new() { WorkGroup = workgroup, Name = "John Doe", WgGrade = "GR1", HrsAvail = 37.5 },
                new() { WorkGroup = workgroup, Name = "Jane Smith", WgGrade = "GR2", HrsAvail = 30.0 }
            };
            var apiResponse = new ApiResponse<List<StaffResourceUtilisationRes>> { Success = true, Data = resList };
            var mappedDtos = new List<StaffResourceUtilisationDto>
            {
                new() { WorkGroup = workgroup, Name = "John Doe", WgGrade = "GR1", HrsAvail = 37.5 },
                new() { WorkGroup = workgroup, Name = "Jane Smith", WgGrade = "GR2", HrsAvail = 30.0 }
            };
            var mappedResponse = ApiResponseDto<List<StaffResourceUtilisationDto>>.SuccessResponse(mappedDtos);
            mappedResponse.Pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 };

            _http.GetAsync<List<StaffResourceUtilisationRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffResourceUtilisationDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetStaffResourceUtilisationAsync(query, workgroup);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("John Doe", result.Data[0].Name);
            Assert.NotNull(result.Pagination);
            Assert.Equal(2, result.Pagination!.TotalRecords);
        }

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_WithFailureResponse_ReturnsFailureDto()
        {
            // Arrange
            const string workgroup = "WG01";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<StaffResourceUtilisationRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not found" } }
            };
            var failureDto = ApiResponseDto<List<StaffResourceUtilisationDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Not found" } }, new ApiMetaDto());

            _http.GetAsync<List<StaffResourceUtilisationRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffResourceUtilisationDto>>>(apiResponse).Returns(failureDto);

            // Act
            var result = await _client.GetStaffResourceUtilisationAsync(query, workgroup);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_WithSuccessAndNoData_ReturnsEmptyList()
        {
            // Arrange
            const string workgroup = "WG_EMPTY";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var emptyList = new List<StaffResourceUtilisationRes>();
            var apiResponse = new ApiResponse<List<StaffResourceUtilisationRes>> { Success = true, Data = emptyList };
            var mappedResponse = ApiResponseDto<List<StaffResourceUtilisationDto>>.SuccessResponse(new List<StaffResourceUtilisationDto>());

            _http.GetAsync<List<StaffResourceUtilisationRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffResourceUtilisationDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetStaffResourceUtilisationAsync(query, workgroup);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_UrlContainsResourceUtilisationAndWorkgroup()
        {
            // Arrange
            const string workgroup = "WG_URL_CHECK";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<StaffResourceUtilisationRes>> { Success = true, Data = new() };

            _http.GetAsync<List<StaffResourceUtilisationRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StaffResourceUtilisationDto>>>(Arg.Any<object>())
                .Returns(ApiResponseDto<List<StaffResourceUtilisationDto>>.SuccessResponse(new()));

            // Act
            await _client.GetStaffResourceUtilisationAsync(query, workgroup);

            // Assert — URL must contain both path and workgroup query param
            await _http.Received(1).GetAsync<List<StaffResourceUtilisationRes>>(
                Arg.Is<string>(u => u.Contains("resourceutilisation") && u.Contains($"workgroup={workgroup}")));
        }

        #endregion
    }
}
