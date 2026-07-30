using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.StaffJobServiceTest
{
    public class StaffJobServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsStaffJobApiClient _fpsStaffJobApiClient;
        private readonly StaffJobService _staffJobService;

        public StaffJobServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsStaffJobApiClient = Substitute.For<IFpsStaffJobApiClient>();
            _fpsClient.FpsStaffJob.Returns(_fpsStaffJobApiClient);
            _staffJobService = new StaffJobService(_fpsClient);
        }

        #region GetStaffWorkgroupLookupAsync Tests

        [Fact]
        public async Task GetStaffWorkgroupLookupAsync_ReturnsListOfWorkgroups()
        {
            // Arrange
            var workgroups = new List<StaffWorkgroupLookupDto>
            {
                new StaffWorkgroupLookupDto { StaffID = "S001", Name = "John Doe", WorkGroupGrade = "WG1", HrsAvail = 37.5 },
                new StaffWorkgroupLookupDto { StaffID = "S002", Name = "Jane Smith", WorkGroupGrade = "WG2", HrsAvail = 30.0 }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>.SuccessResponse(workgroups);

            _fpsStaffJobApiClient.GetStaffWorkgroupLookupAsync().Returns(expectedResponse);

            // Act
            var result = await _staffJobService.GetStaffWorkgroupLookupAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count());
            await _fpsStaffJobApiClient.Received(1).GetStaffWorkgroupLookupAsync();
        }

        [Fact]
        public async Task GetStaffWorkgroupLookupAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>.SuccessResponse(
                new List<StaffWorkgroupLookupDto>());

            _fpsStaffJobApiClient.GetStaffWorkgroupLookupAsync().Returns(expectedResponse);

            // Act
            var result = await _staffJobService.GetStaffWorkgroupLookupAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetStaffWorkgroupLookupAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<IEnumerable<StaffWorkgroupLookupDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.GetStaffWorkgroupLookupAsync().Returns(expectedResponse);

            // Act
            var result = await _staffJobService.GetStaffWorkgroupLookupAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetAllStaffJobsAsync Tests

        [Fact]
        public async Task GetAllStaffJobsAsync_WithValidParameters_ReturnsSuccessResponse()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var jobCode = "JOB001";
            var staffJobs = new List<StaffJobViewDto>
            {
                new StaffJobViewDto { StaffID = "S001", JobCode = jobCode, PlannedHours = 40.0, Name = "John Doe" },
                new StaffJobViewDto { StaffID = "S002", JobCode = jobCode, PlannedHours = 20.0, Name = "Jane Smith" }
            };
            var expectedResponse = ApiResponseDto<List<StaffJobViewDto>>.SuccessResponse(
                staffJobs,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _fpsStaffJobApiClient.GetAllStaffJobAsync(queryParameters, jobCode).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.GetAllStaffJobsAsync(queryParameters, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsStaffJobApiClient.Received(1).GetAllStaffJobAsync(queryParameters, jobCode);
        }

        [Fact]
        public async Task GetAllStaffJobsAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var jobCode = "JOB001";
            var expectedResponse = ApiResponseDto<List<StaffJobViewDto>>.SuccessResponse(
                new List<StaffJobViewDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            );

            _fpsStaffJobApiClient.GetAllStaffJobAsync(queryParameters, jobCode).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.GetAllStaffJobsAsync(queryParameters, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Theory]
        [InlineData("JOB001")]
        [InlineData("JOB002")]
        [InlineData("PROJ123")]
        public async Task GetAllStaffJobsAsync_WithDifferentJobCodes_PassesCorrectValue(string jobCode)
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<StaffJobViewDto>>.SuccessResponse(
                new List<StaffJobViewDto>(),
                new PaginationDto()
            );

            _fpsStaffJobApiClient.GetAllStaffJobAsync(queryParameters, jobCode).Returns(expectedResponse);

            // Act
            await _staffJobService.GetAllStaffJobsAsync(queryParameters, jobCode);

            // Assert
            await _fpsStaffJobApiClient.Received(1).GetAllStaffJobAsync(queryParameters, jobCode);
        }

        [Fact]
        public async Task GetAllStaffJobsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<StaffJobViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.GetAllStaffJobAsync(queryParameters, Arg.Any<string>()).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.GetAllStaffJobsAsync(queryParameters, "JOB001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetStaffJobByIdAsync Tests

        [Fact]
        public async Task GetStaffJobByIdAsync_WithValidIds_ReturnsStaffJob()
        {
            // Arrange
            var staffId = "S001";
            var jobCode = "JOB001";
            var staffJob = new StaffJobDto { StaffId = staffId, JobCode = jobCode, PlannedHours = 40.0 };
            var expectedResponse = ApiResponseDto<StaffJobDto>.SuccessResponse(staffJob);

            _fpsStaffJobApiClient.GetStaffJobByIdAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.GetStaffJobByIdAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(staffId, result.Data.StaffId);
            Assert.Equal(jobCode, result.Data.JobCode);
            await _fpsStaffJobApiClient.Received(1).GetStaffJobByIdAsync(staffId, jobCode);
        }

        [Fact]
        public async Task GetStaffJobByIdAsync_WithNonExistentIds_ReturnsFailureResponse()
        {
            // Arrange
            var staffId = "NONEXISTENT";
            var jobCode = "INVALID";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Staff job not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<StaffJobDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.GetStaffJobByIdAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.GetStaffJobByIdAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("S001", "JOB001")]
        [InlineData("S002", "JOB002")]
        [InlineData("S003", "PROJ123")]
        public async Task GetStaffJobByIdAsync_WithVariousIds_CallsApiClient(string staffId, string jobCode)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<StaffJobDto>.SuccessResponse(
                new StaffJobDto { StaffId = staffId, JobCode = jobCode });

            _fpsStaffJobApiClient.GetStaffJobByIdAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            await _staffJobService.GetStaffJobByIdAsync(staffId, jobCode);

            // Assert
            await _fpsStaffJobApiClient.Received(1).GetStaffJobByIdAsync(staffId, jobCode);
        }

        #endregion

        #region GetViewByStaffIdAsync Tests

        [Fact]
        public async Task GetViewByStaffIdAsync_WithValidIds_ReturnsStaffJobView()
        {
            // Arrange
            var staffId = "S001";
            var jobCode = "JOB001";
            var staffJobView = new StaffJobViewDto
            {
                StaffID = staffId,
                JobCode = jobCode,
                PlannedHours = 40.0,
                Name = "John Doe",
                ChargeRate = 50.00m,
                StaffCost = 2000.00m
            };
            var expectedResponse = ApiResponseDto<StaffJobViewDto?>.SuccessResponse(staffJobView);

            _fpsStaffJobApiClient.GetViewByStaffIdAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(staffId, result.Data.StaffID);
            Assert.Equal(jobCode, result.Data.JobCode);
            await _fpsStaffJobApiClient.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
        }

        [Fact]
        public async Task GetViewByStaffIdAsync_WithNonExistentIds_ReturnsFailureResponse()
        {
            // Arrange
            var staffId = "NONEXISTENT";
            var jobCode = "INVALID";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Staff job view not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<StaffJobViewDto?>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.GetViewByStaffIdAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("S001", "JOB001")]
        [InlineData("S002", "JOB002")]
        [InlineData("S003", "PROJ123")]
        public async Task GetViewByStaffIdAsync_WithVariousIds_CallsApiClient(string staffId, string jobCode)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<StaffJobViewDto?>.SuccessResponse(
                new StaffJobViewDto { StaffID = staffId, JobCode = jobCode });

            _fpsStaffJobApiClient.GetViewByStaffIdAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            await _staffJobService.GetViewByStaffIdAsync(staffId, jobCode);

            // Assert
            await _fpsStaffJobApiClient.Received(1).GetViewByStaffIdAsync(staffId, jobCode);
        }

        #endregion

        #region GetStaffChargeRate Tests

        [Fact]
        public async Task GetStaffChargeRate_WithValidIds_ReturnsChargeRate()
        {
            // Arrange
            var staffId = "S001";
            var jobCode = "JOB001";
            var expectedRate = 75.50m;
            var expectedResponse = ApiResponseDto<decimal?>.SuccessResponse(expectedRate);

            _fpsStaffJobApiClient.GetStaffChargeRate(staffId, jobCode).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.GetStaffChargeRate(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedRate, result.Data);
            await _fpsStaffJobApiClient.Received(1).GetStaffChargeRate(staffId, jobCode);
        }

        [Fact]
        public async Task GetStaffChargeRate_WithNonExistentIds_ReturnsFailureResponse()
        {
            // Arrange
            var staffId = "NONEXISTENT";
            var jobCode = "INVALID";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Charge rate not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<decimal?>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.GetStaffChargeRate(staffId, jobCode).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.GetStaffChargeRate(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("S001", "JOB001", 50.00)]
        [InlineData("S002", "JOB002", 75.25)]
        [InlineData("S003", "PROJ123", 100.00)]
        public async Task GetStaffChargeRate_WithVariousIds_ReturnsCorrectRate(string staffId, string jobCode, double rate)
        {
            // Arrange
            var expectedRate = (decimal)rate;
            var expectedResponse = ApiResponseDto<decimal?>.SuccessResponse(expectedRate);

            _fpsStaffJobApiClient.GetStaffChargeRate(staffId, jobCode).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.GetStaffChargeRate(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedRate, result.Data);
        }

        #endregion

        #region CreateStaffJobAsync Tests

        [Fact]
        public async Task CreateStaffJobAsync_WithValidStaffJob_ReturnsSuccessResponse()
        {
            // Arrange
            var newStaffJob = new StaffJobDto
            {
                StaffId = "S001",
                JobCode = "JOB001",
                PlannedHours = 40.0,
                FpsCalYear = 2024
            };
            var expectedResponse = ApiResponseDto<StaffJobDto>.SuccessResponse(newStaffJob);

            _fpsStaffJobApiClient.CreateStaffJobAsync(newStaffJob).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.CreateStaffJobAsync(newStaffJob);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(newStaffJob.StaffId, result.Data.StaffId);
            Assert.Equal(newStaffJob.JobCode, result.Data.JobCode);
            await _fpsStaffJobApiClient.Received(1).CreateStaffJobAsync(newStaffJob);
        }

        [Fact]
        public async Task CreateStaffJobAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var newStaffJob = new StaffJobDto { StaffId = "S001", JobCode = "JOB001", PlannedHours = 40.0 };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Duplicate staff job", Code = "DUPLICATE" }
            };
            var expectedResponse = ApiResponseDto<StaffJobDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.CreateStaffJobAsync(newStaffJob).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.CreateStaffJobAsync(newStaffJob);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task CreateStaffJobAsync_PassesExactStaffJobObject()
        {
            // Arrange
            var newStaffJob = new StaffJobDto
            {
                StaffId = "S001",
                JobCode = "JOB001",
                PlannedHours = 37.5,
                FpsCalYear = 2024
            };
            var expectedResponse = ApiResponseDto<StaffJobDto>.SuccessResponse(newStaffJob);

            _fpsStaffJobApiClient.CreateStaffJobAsync(newStaffJob).Returns(expectedResponse);

            // Act
            await _staffJobService.CreateStaffJobAsync(newStaffJob);

            // Assert
            await _fpsStaffJobApiClient.Received(1).CreateStaffJobAsync(Arg.Is<StaffJobDto>(s =>
                s.StaffId == newStaffJob.StaffId &&
                s.JobCode == newStaffJob.JobCode &&
                s.PlannedHours == newStaffJob.PlannedHours &&
                s.FpsCalYear == newStaffJob.FpsCalYear
            ));
        }

        #endregion

        #region UpdateStaffJobAsync Tests

        [Fact]
        public async Task UpdateStaffJobAsync_WithValidStaffJob_ReturnsSuccessResponse()
        {
            // Arrange
            var staffId = "S001";
            var updatedStaffJob = new StaffJobDto
            {
                StaffId = staffId,
                JobCode = "JOB001",
                PlannedHours = 35.0,
                FpsCalYear = 2024
            };
            var expectedResponse = ApiResponseDto<StaffJobDto>.SuccessResponse(updatedStaffJob);

            _fpsStaffJobApiClient.UpdateStaffJobAsync(updatedStaffJob).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.UpdateStaffJobAsync(staffId, updatedStaffJob);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(staffId, result.Data.StaffId);
            Assert.Equal(35.0, result.Data.PlannedHours);
            await _fpsStaffJobApiClient.Received(1).UpdateStaffJobAsync(updatedStaffJob);
        }

        [Fact]
        public async Task UpdateStaffJobAsync_WithNonExistentStaffJob_ReturnsFailureResponse()
        {
            // Arrange
            var staffJob = new StaffJobDto { StaffId = "NONEXISTENT", JobCode = "INVALID" };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Staff job not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<StaffJobDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.UpdateStaffJobAsync(staffJob).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.UpdateStaffJobAsync(staffJob.StaffId, staffJob);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task UpdateStaffJobAsync_PassesExactStaffJobObject()
        {
            // Arrange
            var staffId = "S001";
            var staffJob = new StaffJobDto
            {
                StaffId = staffId,
                JobCode = "JOB001",
                PlannedHours = 20.0,
                FpsCalYear = 2024
            };
            var expectedResponse = ApiResponseDto<StaffJobDto>.SuccessResponse(staffJob);

            _fpsStaffJobApiClient.UpdateStaffJobAsync(staffJob).Returns(expectedResponse);

            // Act
            await _staffJobService.UpdateStaffJobAsync(staffId, staffJob);

            // Assert
            await _fpsStaffJobApiClient.Received(1).UpdateStaffJobAsync(Arg.Is<StaffJobDto>(s =>
                s.StaffId == staffJob.StaffId &&
                s.JobCode == staffJob.JobCode &&
                s.PlannedHours == staffJob.PlannedHours
            ));
        }

        [Fact]
        public async Task UpdateStaffJobAsync_WhenApiReturnsError_ReturnsFailureResponse()
        {
            // Arrange
            var staffJob = new StaffJobDto { StaffId = "S001", JobCode = "JOB001" };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Update failed", Code = "UPDATE_ERROR" }
            };
            var expectedResponse = ApiResponseDto<StaffJobDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.UpdateStaffJobAsync(staffJob).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.UpdateStaffJobAsync(staffJob.StaffId, staffJob);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteStaffJobAsync Tests

        [Fact]
        public async Task DeleteStaffJobAsync_WithValidIds_ReturnsSuccessResponse()
        {
            // Arrange
            var staffId = "S001";
            var jobCode = "JOB001";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _fpsStaffJobApiClient.DeleteStaffJobAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.DeleteStaffJobAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsStaffJobApiClient.Received(1).DeleteStaffJobAsync(staffId, jobCode);
        }

        [Fact]
        public async Task DeleteStaffJobAsync_WithNonExistentIds_ReturnsFailureResponse()
        {
            // Arrange
            var staffId = "NONEXISTENT";
            var jobCode = "INVALID";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Staff job not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.DeleteStaffJobAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.DeleteStaffJobAsync(staffId, jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Theory]
        [InlineData("S001", "JOB001")]
        [InlineData("S002", "JOB002")]
        [InlineData("S003", "PROJ123")]
        public async Task DeleteStaffJobAsync_WithVariousIds_CallsApiClient(string staffId, string jobCode)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsStaffJobApiClient.DeleteStaffJobAsync(staffId, jobCode).Returns(expectedResponse);

            // Act
            await _staffJobService.DeleteStaffJobAsync(staffId, jobCode);

            // Assert
            await _fpsStaffJobApiClient.Received(1).DeleteStaffJobAsync(staffId, jobCode);
        }

        #endregion

        #region Edge Cases and Integration Tests

        [Fact]
        public async Task GetAllStaffJobsAsync_CallsApiClientOnce()
        {
            // Arrange
            var queryParameters = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<StaffJobViewDto>>.SuccessResponse(
                new List<StaffJobViewDto>(),
                new PaginationDto()
            );

            _fpsStaffJobApiClient.GetAllStaffJobAsync(queryParameters, Arg.Any<string>())
                .Returns(expectedResponse);

            // Act
            await _staffJobService.GetAllStaffJobsAsync(queryParameters, "JOB001");

            // Assert
            await _fpsStaffJobApiClient.Received(1).GetAllStaffJobAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetStaffChargeRate_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<decimal?>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.GetStaffChargeRate(Arg.Any<string>(), Arg.Any<string>())
                .Returns(expectedResponse);

            // Act
            var result = await _staffJobService.GetStaffChargeRate("S001", "JOB001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task UpdateStaffJobAsync_IgnoresStaffIdParameter_DelegatesToApiClient()
        {
            // Arrange — service passes the DTO directly; the staffId route param is not forwarded
            var staffJob = new StaffJobDto { StaffId = "S001", JobCode = "JOB001", PlannedHours = 10.0 };
            var expectedResponse = ApiResponseDto<StaffJobDto>.SuccessResponse(staffJob);

            _fpsStaffJobApiClient.UpdateStaffJobAsync(staffJob).Returns(expectedResponse);

            // Act
            await _staffJobService.UpdateStaffJobAsync(staffJob.StaffId, staffJob);

            // Assert
            await _fpsStaffJobApiClient.Received(1).UpdateStaffJobAsync(staffJob);
        }

        #endregion

        #region GetTotalStaffCostAsync Tests

        [Fact]
        public async Task GetTotalStaffCostAsync_WithValidJobCode_ReturnsSuccessResponse()
        {
            // Arrange
            var jobCode = "JOB001";
            var expectedTotal = 4500m;
            var expectedResponse = ApiResponseDto<decimal>.SuccessResponse(expectedTotal);

            _fpsStaffJobApiClient.GetTotalStaffCostAsync(jobCode).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.GetTotalStaffCostAsync(jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(expectedTotal, result.Data);
            await _fpsStaffJobApiClient.Received(1).GetTotalStaffCostAsync(jobCode);
        }

        [Fact]
        public async Task GetTotalStaffCostAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var jobCode = "JOB001";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Failed to retrieve total staff cost", Code = "INTERNAL_ERROR" }
            };
            var expectedResponse = ApiResponseDto<decimal>.FailureResponse(errors, new ApiMetaDto());

            _fpsStaffJobApiClient.GetTotalStaffCostAsync(jobCode).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.GetTotalStaffCostAsync(jobCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            await _fpsStaffJobApiClient.Received(1).GetTotalStaffCostAsync(jobCode);
        }

        [Theory]
        [InlineData("JOB001")]
        [InlineData("FZ2000")]
        [InlineData("PROJ123")]
        public async Task GetTotalStaffCostAsync_WithVariousJobCodes_CallsApiClient(string jobCode)
        {
            // Arrange
            var expectedResponse = ApiResponseDto<decimal>.SuccessResponse(100m);
            _fpsStaffJobApiClient.GetTotalStaffCostAsync(jobCode).Returns(expectedResponse);

            // Act
            await _staffJobService.GetTotalStaffCostAsync(jobCode);

            // Assert
            await _fpsStaffJobApiClient.Received(1).GetTotalStaffCostAsync(jobCode);
        }

        #endregion

        #region GetStaffResourceUtilisationAsync

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_WithValidWorkgroup_ReturnsSuccessResponse()
        {
            // Arrange
            const string workgroup = "WG01";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedData = new List<StaffResourceUtilisationDto>
            {
                new() { WorkGroup = workgroup, Name = "John Doe", WgGrade = "GR1", HrsAvail = 37.5, ApprovedSoct = 20.0 },
                new() { WorkGroup = workgroup, Name = "Jane Smith", WgGrade = "GR2", HrsAvail = 30.0, ApprovedSoct = 15.0 }
            };
            var expectedResponse = ApiResponseDto<List<StaffResourceUtilisationDto>>.SuccessResponse(expectedData);

            _fpsStaffJobApiClient.GetStaffResourceUtilisationAsync(query, workgroup).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.GetStaffResourceUtilisationAsync(query, workgroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("John Doe", result.Data[0].Name);
            Assert.Equal(37.5, result.Data[0].HrsAvail);
            await _fpsStaffJobApiClient.Received(1).GetStaffResourceUtilisationAsync(query, workgroup);
        }

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            const string workgroup = "WG_EMPTY";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<StaffResourceUtilisationDto>>.SuccessResponse(
                new List<StaffResourceUtilisationDto>());

            _fpsStaffJobApiClient.GetStaffResourceUtilisationAsync(query, workgroup).Returns(expectedResponse);

            // Act
            var result = await _staffJobService.GetStaffResourceUtilisationAsync(query, workgroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _fpsStaffJobApiClient.Received(1).GetStaffResourceUtilisationAsync(query, workgroup);
        }

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            const string workgroup = "WG01";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var failureResponse = ApiResponseDto<List<StaffResourceUtilisationDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "API error" } }, new ApiMetaDto());

            _fpsStaffJobApiClient.GetStaffResourceUtilisationAsync(query, workgroup).Returns(failureResponse);

            // Act
            var result = await _staffJobService.GetStaffResourceUtilisationAsync(query, workgroup);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _fpsStaffJobApiClient.Received(1).GetStaffResourceUtilisationAsync(query, workgroup);
        }

        [Fact]
        public async Task GetStaffResourceUtilisationAsync_DelegatesDirectlyToApiClient()
        {
            // Arrange
            const string workgroup = "WG02";
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var response = ApiResponseDto<List<StaffResourceUtilisationDto>>.SuccessResponse(new List<StaffResourceUtilisationDto>());
            _fpsStaffJobApiClient.GetStaffResourceUtilisationAsync(query, workgroup).Returns(response);

            // Act
            await _staffJobService.GetStaffResourceUtilisationAsync(query, workgroup);

            // Assert — service is a thin pass-through; client called exactly once with same args
            await _fpsStaffJobApiClient.Received(1).GetStaffResourceUtilisationAsync(query, workgroup);
        }

        #endregion
    }
}
