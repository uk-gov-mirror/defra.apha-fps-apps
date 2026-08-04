using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactJobCodeApiClientTest
{
    public class PactJobCodeApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactJobCodeApiClient _client;

        public PactJobCodeApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactJobCodeApiClient(_http, _mapper);
        }

        #region GetJobCodesAsync Tests

        [Fact]
        public async Task GetJobCodesAsync_WithSuccessResponse_ReturnsMappedJobCodeList()
        {
            // Arrange
            var jobCodeList = new List<JobCodeRes>
            {
                new() { JobCodeId = "JC001", ParentProject = "PP001", JobCodeName = "Job Code One" },
                new() { JobCodeId = "JC002", ParentProject = "PP002", JobCodeName = "Job Code Two" }
            };
            var apiResponse = new ApiResponse<List<JobCodeRes>> { Success = true, Data = jobCodeList };
            var expectedDto = ApiResponseDto<List<JobCodeDto>>.SuccessResponse(
                new List<JobCodeDto>
                {
                    new() { JobCodeId = "JC001", ParentProject = "PP001" },
                    new() { JobCodeId = "JC002", ParentProject = "PP002" }
                }
            );

            _http.GetAsync<List<JobCodeRes>>("api/v1/jobcode").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<JobCodeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetJobCodesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<JobCodeRes>>("api/v1/jobcode");
        }

        [Fact]
        public async Task GetJobCodesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<JobCodeRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<JobCodeDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<JobCodeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<JobCodeDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetJobCodesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetJobCodesAsync_WithEmptyList_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<JobCodeRes>> { Success = true, Data = new List<JobCodeRes>() };
            var expectedDto = ApiResponseDto<List<JobCodeDto>>.SuccessResponse(new List<JobCodeDto>());

            _http.GetAsync<List<JobCodeRes>>("api/v1/jobcode").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<JobCodeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetJobCodesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _http.Received(1).GetAsync<List<JobCodeRes>>("api/v1/jobcode");
        }

        #endregion

        #region GetJobCodesByProjectAsync Tests

        [Fact]
        public async Task GetJobCodesByProjectAsync_WithValidProject_ReturnsMappedJobCodeList()
        {
            // Arrange
            var parentProject = "PP001";
            var jobCodeList = new List<JobCodeRes>
            {
                new() { JobCodeId = "JC001", ParentProject = parentProject, JobCodeName = "Job Code One" },
                new() { JobCodeId = "JC002", ParentProject = parentProject, JobCodeName = "Job Code Two" }
            };
            var apiResponse = new ApiResponse<List<JobCodeRes>> { Success = true, Data = jobCodeList };
            var expectedDto = ApiResponseDto<List<JobCodeDto>>.SuccessResponse(
                new List<JobCodeDto>
                {
                    new() { JobCodeId = "JC001", ParentProject = parentProject },
                    new() { JobCodeId = "JC002", ParentProject = parentProject }
                }
            );

            _http.GetAsync<List<JobCodeRes>>(Arg.Is<string>(url => url.Contains($"api/v1/jobcode/project?parentProject={Uri.EscapeDataString(parentProject)}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<JobCodeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetJobCodesByProjectAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<JobCodeRes>>(
                Arg.Is<string>(url => url.Contains($"api/v1/jobcode/project?parentProject={Uri.EscapeDataString(parentProject)}")));
        }

        [Fact]
        public async Task GetJobCodesByProjectAsync_WithSpecialCharacters_EncodesUrlCorrectly()
        {
            // Arrange
            var parentProject = "PP/001 & Test";
            var expectedEncodedProject = Uri.EscapeDataString(parentProject); // Should be "PP%2F001%20%26%20Test"
            var jobCodeList = new List<JobCodeRes>
            {
                new() { JobCodeId = "JC001", ParentProject = parentProject, JobCodeName = "Job Code One" }
            };
            var apiResponse = new ApiResponse<List<JobCodeRes>> { Success = true, Data = jobCodeList };
            var expectedDto = ApiResponseDto<List<JobCodeDto>>.SuccessResponse(
                new List<JobCodeDto>
                {
                    new() { JobCodeId = "JC001", ParentProject = parentProject }
                }
            );

            _http.GetAsync<List<JobCodeRes>>(Arg.Is<string>(url => url.Contains($"api/v1/jobcode/project?parentProject={expectedEncodedProject}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<JobCodeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetJobCodesByProjectAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<JobCodeRes>>(
                Arg.Is<string>(url => url.Contains($"api/v1/jobcode/project?parentProject={expectedEncodedProject}")));
        }

        [Theory]
        [InlineData("PP/001", "PP%2F001")]
        [InlineData("PP 001", "PP%20001")]
        [InlineData("PP&001", "PP%26001")]
        [InlineData("PP+001", "PP%2B001")]
        [InlineData("PP#001", "PP%23001")]
        public async Task GetJobCodesByProjectAsync_WithVariousSpecialCharacters_EncodesCorrectly(string parentProject, string expectedEncoded)
        {
            // Arrange
            var jobCodeList = new List<JobCodeRes>
            {
                new() { JobCodeId = "JC001", ParentProject = parentProject }
            };
            var apiResponse = new ApiResponse<List<JobCodeRes>> { Success = true, Data = jobCodeList };
            var expectedDto = ApiResponseDto<List<JobCodeDto>>.SuccessResponse(
                new List<JobCodeDto>
                {
                    new() { JobCodeId = "JC001", ParentProject = parentProject }
                }
            );

            _http.GetAsync<List<JobCodeRes>>(Arg.Is<string>(url => url.Contains($"api/v1/jobcode/project?parentProject={expectedEncoded}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<JobCodeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetJobCodesByProjectAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<JobCodeRes>>(
                Arg.Is<string>(url => url.Contains($"api/v1/jobcode/project?parentProject={expectedEncoded}")));
        }

        [Fact]
        public async Task GetJobCodesByProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<JobCodeRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<JobCodeDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<JobCodeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<JobCodeDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetJobCodesByProjectAsync("PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetPagedJobCodesAsync Tests

        [Fact]
        public async Task GetPagedJobCodesAsync_WithParentProject_IncludesProjectInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parentProject = "PP001";
            var jobCodeList = new List<JobCodeRes> { new() { JobCodeId = "JC001" } };
            var apiResponse = new ApiResponse<List<JobCodeRes>>
            {
                Success = true,
                Data = jobCodeList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expectedDto = ApiResponseDto<List<JobCodeDto>>.SuccessResponse(
                new List<JobCodeDto> { new() { JobCodeId = "JC001" } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            );

            _http.GetAsync<List<JobCodeRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/jobcode/paged") && url.Contains($"parentProject={Uri.EscapeDataString(parentProject)}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<JobCodeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedJobCodesAsync(query, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<JobCodeRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/jobcode/paged") && url.Contains($"parentProject={Uri.EscapeDataString(parentProject)}")));
        }

        [Fact]
        public async Task GetPagedJobCodesAsync_WithNullParentProject_OmitsProjectFromUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<JobCodeRes>> { Success = true, Data = new List<JobCodeRes>() };
            var expectedDto = ApiResponseDto<List<JobCodeDto>>.SuccessResponse(new List<JobCodeDto>(), new PaginationDto());

            _http.GetAsync<List<JobCodeRes>>(Arg.Is<string>(url => url.Contains("api/v1/jobcode/paged"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<JobCodeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedJobCodesAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<JobCodeRes>>(Arg.Is<string>(url => url.Contains("api/v1/jobcode/paged")));
        }

        [Fact]
        public async Task GetPagedJobCodesAsync_WithSpecialCharactersInProject_EncodesUrlCorrectly()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parentProject = "PP/001 & Test";
            var expectedEncoded = Uri.EscapeDataString(parentProject);
            var jobCodeList = new List<JobCodeRes> { new() { JobCodeId = "JC001" } };
            var apiResponse = new ApiResponse<List<JobCodeRes>>
            {
                Success = true,
                Data = jobCodeList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expectedDto = ApiResponseDto<List<JobCodeDto>>.SuccessResponse(
                new List<JobCodeDto> { new() { JobCodeId = "JC001" } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            );

            _http.GetAsync<List<JobCodeRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/jobcode/paged") && url.Contains($"parentProject={expectedEncoded}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<JobCodeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedJobCodesAsync(query, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<JobCodeRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/jobcode/paged") && url.Contains($"parentProject={expectedEncoded}")));
        }

        #endregion

        #region GetJobCodeByIdAsync Tests

        [Fact]
        public async Task GetJobCodeByIdAsync_WithValidId_ReturnsMappedJobCode()
        {
            // Arrange
            var jobCodeId = "JC001";
            var jobCodeRes = new JobCodeRes { JobCodeId = jobCodeId, JobCodeName = "Test Job Code" };
            var apiResponse = new ApiResponse<JobCodeRes> { Success = true, Data = jobCodeRes };
            var expectedDto = ApiResponseDto<JobCodeDto>.SuccessResponse(
                new JobCodeDto { JobCodeId = jobCodeId, JobCodeName = "Test Job Code" }
            );

            _http.GetAsync<JobCodeRes>($"api/v1/jobcode/jobCodeId?jobCodeId={Uri.EscapeDataString(jobCodeId)}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<JobCodeDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetJobCodeByIdAsync(jobCodeId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(jobCodeId, result.Data?.JobCodeId);
            await _http.Received(1).GetAsync<JobCodeRes>($"api/v1/jobcode/jobCodeId?jobCodeId={Uri.EscapeDataString(jobCodeId)}");
        }

        [Fact]
        public async Task GetJobCodeByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<JobCodeRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<JobCodeDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<JobCodeRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<JobCodeDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetJobCodeByIdAsync("NONEXISTENT");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetJobCodeByIdAsync_WithSpecialCharactersInId_EncodesUrlCorrectly()
        {
            // Arrange
            var jobCodeId = "JC/001 & Test";
            var expectedEncoded = Uri.EscapeDataString(jobCodeId);
            var jobCodeRes = new JobCodeRes { JobCodeId = jobCodeId, JobCodeName = "Test Job Code" };
            var apiResponse = new ApiResponse<JobCodeRes> { Success = true, Data = jobCodeRes };
            var expectedDto = ApiResponseDto<JobCodeDto>.SuccessResponse(
                new JobCodeDto { JobCodeId = jobCodeId, JobCodeName = "Test Job Code" }
            );

            _http.GetAsync<JobCodeRes>($"api/v1/jobcode/jobCodeId?jobCodeId={expectedEncoded}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<JobCodeDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetJobCodeByIdAsync(jobCodeId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(jobCodeId, result.Data?.JobCodeId);
            await _http.Received(1).GetAsync<JobCodeRes>($"api/v1/jobcode/jobCodeId?jobCodeId={expectedEncoded}");
        }

        [Theory]
        [InlineData("JC/001", "JC%2F001")]
        [InlineData("JC 001", "JC%20001")]
        [InlineData("JC&001", "JC%26001")]
        [InlineData("JC+001", "JC%2B001")]
        [InlineData("JC#001", "JC%23001")]
        public async Task GetJobCodeByIdAsync_WithVariousSpecialCharacters_EncodesCorrectly(string jobCodeId, string expectedEncoded)
        {
            // Arrange
            var jobCodeRes = new JobCodeRes { JobCodeId = jobCodeId };
            var apiResponse = new ApiResponse<JobCodeRes> { Success = true, Data = jobCodeRes };
            var expectedDto = ApiResponseDto<JobCodeDto>.SuccessResponse(
                new JobCodeDto { JobCodeId = jobCodeId }
            );

            _http.GetAsync<JobCodeRes>($"api/v1/jobcode/jobCodeId?jobCodeId={expectedEncoded}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<JobCodeDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetJobCodeByIdAsync(jobCodeId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<JobCodeRes>($"api/v1/jobcode/jobCodeId?jobCodeId={expectedEncoded}");
        }

        #endregion

        #region GetTypesAsync Tests

        [Fact]
        public async Task GetTypesAsync_WithSuccessResponse_ReturnsMappedTypeList()
        {
            // Arrange
            var types = new List<string> { "TypeA", "TypeB", "TypeC" };
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = types };
            var expectedDto = ApiResponseDto<List<string>>.SuccessResponse(types);

            _http.GetAsync<List<string>>("api/v1/jobcode/types").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data?.Count);
            await _http.Received(1).GetAsync<List<string>>("api/v1/jobcode/types");
        }

        [Fact]
        public async Task GetTypesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<string>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<string>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateJobCodeAsync Tests

        [Fact]
        public async Task CreateJobCodeAsync_WithValidJobCode_ReturnsMappedCreatedJobCode()
        {
            // Arrange
            var jobCodeDto = new JobCodeDto { JobCodeId = "JC001", ParentProject = "PP001", JobCodeName = "New Job Code" };
            var jobCodeReq = new JobCodeReq { JobCodeId = "JC001", ParentProject = "PP001" };
            var jobCodeRes = new JobCodeRes { JobCodeId = "JC001", ParentProject = "PP001" };
            var apiResponse = new ApiResponse<JobCodeRes> { Success = true, Data = jobCodeRes };
            var expectedDto = ApiResponseDto<JobCodeDto>.SuccessResponse(jobCodeDto);

            _mapper.Map<JobCodeReq>(jobCodeDto).Returns(jobCodeReq);
            _http.PostAsync<JobCodeReq, JobCodeRes>("api/v1/jobcode", jobCodeReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<JobCodeDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateJobCodeAsync(jobCodeDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("JC001", result.Data?.JobCodeId);
            await _http.Received(1).PostAsync<JobCodeReq, JobCodeRes>("api/v1/jobcode", jobCodeReq);
        }

        [Fact]
        public async Task CreateJobCodeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var jobCodeDto = new JobCodeDto { JobCodeId = "JC001" };
            var jobCodeReq = new JobCodeReq { JobCodeId = "JC001" };
            var errors = new List<ApiError> { new() { Message = "Duplicate", Code = "DUPLICATE" } };
            var apiResponse = new ApiResponse<JobCodeRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<JobCodeDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Duplicate", Code = "DUPLICATE" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<JobCodeReq>(jobCodeDto).Returns(jobCodeReq);
            _http.PostAsync<JobCodeReq, JobCodeRes>(Arg.Any<string>(), Arg.Any<JobCodeReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<JobCodeDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateJobCodeAsync(jobCodeDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateJobCodeAsync Tests

        [Fact]
        public async Task UpdateJobCodeAsync_WithValidJobCode_ReturnsMappedUpdatedJobCode()
        {
            // Arrange
            var jobCodeDto = new JobCodeDto { JobCodeId = "JC001", JobCodeName = "Updated Job Code" };
            var jobCodeReq = new JobCodeReq { JobCodeId = "JC001" };
            var jobCodeRes = new JobCodeRes { JobCodeId = "JC001", JobCodeName = "Updated Job Code" };
            var apiResponse = new ApiResponse<JobCodeRes> { Success = true, Data = jobCodeRes };
            var expectedDto = ApiResponseDto<JobCodeDto>.SuccessResponse(jobCodeDto);

            _mapper.Map<JobCodeReq>(jobCodeDto).Returns(jobCodeReq);
            _http.PutAsync<JobCodeReq, JobCodeRes>("api/v1/jobcode", jobCodeReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<JobCodeDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateJobCodeAsync(jobCodeDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Updated Job Code", result.Data?.JobCodeName);
            await _http.Received(1).PutAsync<JobCodeReq, JobCodeRes>("api/v1/jobcode", jobCodeReq);
        }

        [Fact]
        public async Task UpdateJobCodeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var jobCodeDto = new JobCodeDto { JobCodeId = "NONEXISTENT" };
            var jobCodeReq = new JobCodeReq { JobCodeId = "NONEXISTENT" };
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<JobCodeRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<JobCodeDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<JobCodeReq>(jobCodeDto).Returns(jobCodeReq);
            _http.PutAsync<JobCodeReq, JobCodeRes>(Arg.Any<string>(), Arg.Any<JobCodeReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<JobCodeDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateJobCodeAsync(jobCodeDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteJobCodeAsync Tests

        [Fact]
        public async Task DeleteJobCodeAsync_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var jobCodeId = "JC001";
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>($"api/v1/jobcode/jobCodeId?jobCodeId={Uri.EscapeDataString(jobCodeId)}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteJobCodeAsync(jobCodeId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool?>($"api/v1/jobcode/jobCodeId?jobCodeId={Uri.EscapeDataString(jobCodeId)}");
        }

        [Fact]
        public async Task DeleteJobCodeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteJobCodeAsync("NONEXISTENT");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task DeleteJobCodeAsync_WhenApiReturnsBusinessRuleViolation_ReturnsFailureResponse()
        {
            // Arrange — API returns 409 when related TimeCodeValid records exist (trigger validation)
            var errors = new List<ApiError> { new() { Message = "This JobCode has related records in TimeCodeValid and cannot be deleted.", Code = "BUSINESS_RULE_VIOLATION" } };
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "This JobCode has related records in TimeCodeValid and cannot be deleted.", Code = "BUSINESS_RULE_VIOLATION" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteJobCodeAsync("JC001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "BUSINESS_RULE_VIOLATION");
        }

        [Fact]
        public async Task DeleteJobCodeAsync_WithSpecialCharactersInId_EncodesUrlCorrectly()
        {
            // Arrange
            var jobCodeId = "JC/001 & Test";
            var expectedEncoded = Uri.EscapeDataString(jobCodeId);
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>($"api/v1/jobcode/jobCodeId?jobCodeId={expectedEncoded}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteJobCodeAsync(jobCodeId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<bool?>($"api/v1/jobcode/jobCodeId?jobCodeId={expectedEncoded}");
        }

        [Theory]
        [InlineData("JC/001", "JC%2F001")]
        [InlineData("JC 001", "JC%20001")]
        [InlineData("JC&001", "JC%26001")]
        [InlineData("JC+001", "JC%2B001")]
        [InlineData("JC#001", "JC%23001")]
        public async Task DeleteJobCodeAsync_WithVariousSpecialCharacters_EncodesCorrectly(string jobCodeId, string expectedEncoded)
        {
            // Arrange
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>($"api/v1/jobcode/jobCodeId?jobCodeId={expectedEncoded}").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteJobCodeAsync(jobCodeId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<bool?>($"api/v1/jobcode/jobCodeId?jobCodeId={expectedEncoded}");
        }

        #endregion
    }
}
