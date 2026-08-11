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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactTimeCodeValidApiClientTest
{
    public class PactTimeCodeValidApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactTimeCodeValidApiClient _client;

        public PactTimeCodeValidApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactTimeCodeValidApiClient(_http, _mapper);
        }

        #region GetTimeCodeValidsByWorkGroupAsync Tests

        [Fact]
        public async Task GetTimeCodeValidsByWorkGroupAsync_WithSuccessResponse_ReturnsMappedList()
        {
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>>
            {
                Success = true,
                Data = [new TimeCodeValidRes { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001" }]
            };
            var expectedDto = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
                [new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001" }]);

            _http.GetAsync<List<TimeCodeValidRes>>(Arg.Is<string>(url =>
                url.Contains("timecodevalid/workgroup") &&
                url.Contains($"workGroup={Uri.EscapeDataString("WG001")}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetTimeCodeValidsByWorkGroupAsync("WG001");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetTimeCodeValidsByWorkGroupAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>>
            {
                Success = false,
                Errors = [new ApiError { Message = "error", Code = "API_ERROR" }]
            };
            var mapped = new ApiResponseDto<List<TimeCodeValidDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "error", Code = "API_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<TimeCodeValidRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(mapped);

            var result = await _client.GetTimeCodeValidsByWorkGroupAsync("WG001");

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync Tests

        [Fact]
        public async Task GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync_WithSuccessResponse_ReturnsMappedProjects()
        {
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = ["PP001", "PP002"] };
            var expectedDto = ApiResponseDto<List<string>>.SuccessResponse(["PP001", "PP002"]);

            _http.GetAsync<List<string>>(Arg.Is<string>(url =>
                url.Contains("timecodevalid/projects") &&
                url.Contains($"workgroup={Uri.EscapeDataString("WG001")}") &&
                url.Contains($"timecode={Uri.EscapeDataString("TC001")}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("WG001", "TC001");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
        }

        [Fact]
        public async Task GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<List<string>>
            {
                Success = false,
                Errors = [new ApiError { Message = "error", Code = "API_ERROR" }]
            };
            var mapped = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "error", Code = "API_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<string>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mapped);

            var result = await _client.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("WG001", "TC001");

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetAllDistinctTimeCodesAsync Tests

        [Fact]
        public async Task GetAllDistinctTimeCodesAsync_WithSuccessResponse_ReturnsMappedList()
        {
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = ["TC001", "TC002"] };
            var expectedDto = ApiResponseDto<List<string>>.SuccessResponse(["TC001", "TC002"]);

            _http.GetAsync<List<string>>(Arg.Is<string>(url => url.Contains("timecodes/all"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetAllDistinctTimeCodesAsync();

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
        }

        [Fact]
        public async Task GetAllDistinctTimeCodesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<List<string>>
            {
                Success = false,
                Errors = [new ApiError { Message = "error", Code = "API_ERROR" }]
            };
            var mapped = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "error", Code = "API_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<string>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mapped);

            var result = await _client.GetAllDistinctTimeCodesAsync();

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetAllDistinctProjectsAsync Tests

        [Fact]
        public async Task GetAllDistinctProjectsAsync_WithSuccessResponse_ReturnsMappedList()
        {
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = ["PP001", "PP002"] };
            var expectedDto = ApiResponseDto<List<string>>.SuccessResponse(["PP001", "PP002"]);

            _http.GetAsync<List<string>>(Arg.Is<string>(url => url.Contains("projects/all"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetAllDistinctProjectsAsync();

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
        }

        [Fact]
        public async Task GetAllDistinctProjectsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var apiResponse = new ApiResponse<List<string>>
            {
                Success = false,
                Errors = [new ApiError { Message = "error", Code = "API_ERROR" }]
            };
            var mapped = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "error", Code = "API_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<string>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mapped);

            var result = await _client.GetAllDistinctProjectsAsync();

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetTimeCodeValidAsync Tests

        [Fact]
        public async Task GetTimeCodeValidAsync_WithValidKey_ReturnsMappedSingleItem()
        {
            var itemRes = new TimeCodeValidRes { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001" };
            var apiResponse = new ApiResponse<TimeCodeValidRes> { Success = true, Data = itemRes };
            var expectedDto = ApiResponseDto<TimeCodeValidDto>.SuccessResponse(
                new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001" });

            _http.GetAsync<TimeCodeValidRes>(Arg.Is<string>(url =>
                url.Contains("api/v1/timecodevalid/wgtimecodeprojectcode") && 
                url.Contains($"workGroup={Uri.EscapeDataString("WG001")}") && 
                url.Contains($"timeCode={Uri.EscapeDataString("TC001")}") && 
                url.Contains($"parentProject={Uri.EscapeDataString("PP001")}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetTimeCodeValidAsync("WG001", "TC001", "PP001");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("TC001", result.Data?.TimeCode);
        }

        [Fact]
        public async Task GetTimeCodeValidAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<TimeCodeValidRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<TimeCodeValidDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };
            _http.GetAsync<TimeCodeValidRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(apiResponse).Returns(mappedResponse);

            var result = await _client.GetTimeCodeValidAsync("WG_NONE", "TC_NONE", "PP001");

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetPagedByProjectAndTestCodeAsync Tests

        [Fact]
        public async Task GetPagedByProjectAndTestCodeAsync_WithValidParams_IncludesProjectAndTestCodeInUrl()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var timeCodeList = new List<TimeCodeValidRes>
            {
                new() { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001" }
            };
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>>
            {
                Success = true,
                Data = timeCodeList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expectedDto = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
                new List<TimeCodeValidDto> { new() { TimeCode = "TC001" } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

            _http.GetAsync<List<TimeCodeValidRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/timecodevalid/paged/byprojectandtest") &&
                url.Contains($"parentProject={Uri.EscapeDataString("PP001")}") &&
                url.Contains($"testCode={Uri.EscapeDataString("TST001")}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetPagedByProjectAndTestCodeAsync(query, "PP001", "TST001");

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetPagedByProjectAndTestCodeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<TimeCodeValidDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } },
                Meta = new ApiMetaDto()
            };
            _http.GetAsync<List<TimeCodeValidRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(mappedResponse);

            var result = await _client.GetPagedByProjectAndTestCodeAsync(query, "PP001", "TST001");

            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetByJobCodeAsync Tests

        [Fact]
        public async Task GetByJobCodeAsync_WithValidParams_ReturnsMappedTimeCodeList()
        {
            // Arrange
            var jobCode = "JC001";
            var parentProject = "PP001";
            var timeCodeList = new List<TimeCodeValidRes>
            {
                new() { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = parentProject, JobCode = jobCode },
                new() { TimeCode = "TC002", WorkGroup = "WG001", ParentProject = parentProject, JobCode = jobCode }
            };
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>> { Success = true, Data = timeCodeList };
            var expectedDto = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
                new List<TimeCodeValidDto>
                {
                    new() { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = parentProject },
                    new() { TimeCode = "TC002", WorkGroup = "WG001", ParentProject = parentProject }
                }
            );

            _http.GetAsync<List<TimeCodeValidRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/timecodevalid/jobcode") &&
                url.Contains($"jobCode={Uri.EscapeDataString(jobCode)}") &&
                url.Contains($"parentProject={Uri.EscapeDataString(parentProject)}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetByJobCodeAsync(jobCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<TimeCodeValidRes>>(
                Arg.Is<string>(url => 
                    url.Contains("api/v1/timecodevalid/jobcode") &&
                    url.Contains($"jobCode={Uri.EscapeDataString(jobCode)}") &&
                    url.Contains($"parentProject={Uri.EscapeDataString(parentProject)}")));
        }

        [Fact]
        public async Task GetByJobCodeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<TimeCodeValidDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<TimeCodeValidRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetByJobCodeAsync("JC001", "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetByJobCodeAsync_WithSpecialCharacters_UrlEncodesParams()
        {
            // Arrange
            var jobCode = "JC&001";  // Contains special character &
            var parentProject = "PP 001";  // Contains space
            var timeCodeList = new List<TimeCodeValidRes>
            {
                new() { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = parentProject, JobCode = jobCode }
            };
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>> { Success = true, Data = timeCodeList };
            var expectedDto = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
                new List<TimeCodeValidDto>
                {
                    new() { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = parentProject }
                }
            );

            _http.GetAsync<List<TimeCodeValidRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/timecodevalid/jobcode") &&
                url.Contains($"jobCode={Uri.EscapeDataString(jobCode)}") &&
                url.Contains($"parentProject={Uri.EscapeDataString(parentProject)}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetByJobCodeAsync(jobCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<TimeCodeValidRes>>(
                Arg.Is<string>(url =>
                    url.Contains("jobCode=JC%26001") &&  // & should be encoded as %26
                    url.Contains("parentProject=PP%20001")));  // space should be encoded as %20
        }

        #endregion

        #region GetPagedTimeCodesAsync Tests

        [Fact]
        public async Task GetPagedTimeCodesAsync_WithJobCodeAndProject_IncludesBothInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var jobCode = "JC001";
            var parentProject = "PP001";
            var timeCodeList = new List<TimeCodeValidRes>
            {
                new() { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = parentProject }
            };
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>>
            {
                Success = true,
                Data = timeCodeList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expectedDto = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
                new List<TimeCodeValidDto> { new() { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = parentProject } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            );

            _http.GetAsync<List<TimeCodeValidRes>>(Arg.Is<string>(url =>
                url.Contains("api/v1/timecodevalid/paged") &&
                url.Contains($"jobCode={Uri.EscapeDataString(jobCode)}") &&
                url.Contains($"parentProject={Uri.EscapeDataString(parentProject)}")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedTimeCodesAsync(query, jobCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetPagedTimeCodesAsync_WithNullParams_OmitsOptionalParamsFromUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>> { Success = true, Data = new List<TimeCodeValidRes>() };
            var expectedDto = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(new List<TimeCodeValidDto>(), new PaginationDto());

            _http.GetAsync<List<TimeCodeValidRes>>(Arg.Is<string>(url => url.Contains("api/v1/timecodevalid/paged"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedTimeCodesAsync(query, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
        }

        #endregion

        #region CreateTimeCodeValidAsync Tests

        [Fact]
        public async Task CreateTimeCodeValidAsync_WithValidItem_ReturnsMappedCreatedTimeCode()
        {
            // Arrange
            var itemDto = new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001", JobCode = "JC001", Active = true };
            var itemReq = new TimeCodeValidReq { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001", JobCode = "JC001" };
            var itemRes = new TimeCodeValidRes { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001", JobCode = "JC001" };
            var apiResponse = new ApiResponse<TimeCodeValidRes> { Success = true, Data = itemRes };
            var expectedDto = ApiResponseDto<TimeCodeValidDto>.SuccessResponse(itemDto);

            _mapper.Map<TimeCodeValidReq>(itemDto).Returns(itemReq);
            _http.PostAsync<TimeCodeValidReq, TimeCodeValidRes>("api/v1/timecodevalid", itemReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateTimeCodeValidAsync(itemDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("TC001", result.Data?.TimeCode);
            await _http.Received(1).PostAsync<TimeCodeValidReq, TimeCodeValidRes>("api/v1/timecodevalid", itemReq);
        }

        [Fact]
        public async Task CreateTimeCodeValidAsync_WithTestCodeAndPortfolio_ReturnsMappedCreatedTimeCode()
        {
            // Arrange
            var itemDto = new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001", TestCode = "TST001", Portfolio = "PF001" };
            var itemReq = new TimeCodeValidReq { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001", TestCode = "TST001", Portfolio = "PF001" };
            var itemRes = new TimeCodeValidRes { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001", TestCode = "TST001", Portfolio = "PF001" };
            var apiResponse = new ApiResponse<TimeCodeValidRes> { Success = true, Data = itemRes };
            var expectedDto = ApiResponseDto<TimeCodeValidDto>.SuccessResponse(itemDto);

            _mapper.Map<TimeCodeValidReq>(itemDto).Returns(itemReq);
            _http.PostAsync<TimeCodeValidReq, TimeCodeValidRes>("api/v1/timecodevalid", itemReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateTimeCodeValidAsync(itemDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("TST001", result.Data?.TestCode);
            Assert.Equal("PF001", result.Data?.Portfolio);
            await _http.Received(1).PostAsync<TimeCodeValidReq, TimeCodeValidRes>("api/v1/timecodevalid", itemReq);
        }

        [Fact]
        public async Task CreateTimeCodeValidAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var itemDto = new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001" };
            var errors = new List<ApiError> { new() { Message = "Duplicate", Code = "DUPLICATE" } };
            var apiResponse = new ApiResponse<TimeCodeValidRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<TimeCodeValidDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Duplicate", Code = "DUPLICATE" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<TimeCodeValidReq>(itemDto).Returns(new TimeCodeValidReq());
            _http.PostAsync<TimeCodeValidReq, TimeCodeValidRes>(Arg.Any<string>(), Arg.Any<TimeCodeValidReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateTimeCodeValidAsync(itemDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateTimeCodeValidAsync Tests

        [Fact]
        public async Task UpdateTimeCodeValidAsync_WithValidItem_ReturnsMappedUpdatedTimeCode()
        {
            // Arrange
            var itemDto = new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001", JobCode = "JC001", Active = false };
            var itemReq = new TimeCodeValidReq { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001", JobCode = "JC001" };
            var itemRes = new TimeCodeValidRes { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001", JobCode = "JC001" };
            var apiResponse = new ApiResponse<TimeCodeValidRes> { Success = true, Data = itemRes };
            var expectedDto = ApiResponseDto<TimeCodeValidDto>.SuccessResponse(itemDto);

            _mapper.Map<TimeCodeValidReq>(itemDto).Returns(itemReq);
            _http.PutAsync<TimeCodeValidReq, TimeCodeValidRes>("api/v1/timecodevalid", itemReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateTimeCodeValidAsync(itemDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("TC001", result.Data?.TimeCode);
            await _http.Received(1).PutAsync<TimeCodeValidReq, TimeCodeValidRes>("api/v1/timecodevalid", itemReq);
        }

        [Fact]
        public async Task UpdateTimeCodeValidAsync_WithTestCodeAndPortfolio_ReturnsMappedUpdatedTimeCode()
        {
            // Arrange
            var itemDto = new TimeCodeValidDto { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001", TestCode = "TST001", Portfolio = "PF001" };
            var itemReq = new TimeCodeValidReq { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001", TestCode = "TST001", Portfolio = "PF001" };
            var itemRes = new TimeCodeValidRes { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = "PP001", TestCode = "TST001", Portfolio = "PF001" };
            var apiResponse = new ApiResponse<TimeCodeValidRes> { Success = true, Data = itemRes };
            var expectedDto = ApiResponseDto<TimeCodeValidDto>.SuccessResponse(itemDto);

            _mapper.Map<TimeCodeValidReq>(itemDto).Returns(itemReq);
            _http.PutAsync<TimeCodeValidReq, TimeCodeValidRes>("api/v1/timecodevalid", itemReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateTimeCodeValidAsync(itemDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("TST001", result.Data?.TestCode);
            Assert.Equal("PF001", result.Data?.Portfolio);
            await _http.Received(1).PutAsync<TimeCodeValidReq, TimeCodeValidRes>("api/v1/timecodevalid", itemReq);
        }

        [Fact]
        public async Task UpdateTimeCodeValidAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var itemDto = new TimeCodeValidDto { TimeCode = "NONEXISTENT", WorkGroup = "WG001", ParentProject = "PP001" };
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<TimeCodeValidRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<TimeCodeValidDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<TimeCodeValidReq>(itemDto).Returns(new TimeCodeValidReq());
            _http.PutAsync<TimeCodeValidReq, TimeCodeValidRes>(Arg.Any<string>(), Arg.Any<TimeCodeValidReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<TimeCodeValidDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateTimeCodeValidAsync(itemDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteTimeCodeValidAsync Tests

        [Fact]
        public async Task DeleteTimeCodeValidAsync_WithValidParams_ReturnsSuccess()
        {
            // Arrange
            var workGroup = "WG001";
            var timeCode = "TC001";
            var parentProject = "PP001";
            var expectedUrl = $"api/v1/timecodevalid/delete?workGroup={Uri.EscapeDataString(workGroup)}&timeCode={Uri.EscapeDataString(timeCode)}&parentProject={Uri.EscapeDataString(parentProject)}";
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteTimeCodeValidAsync(workGroup, timeCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool?>(expectedUrl);
        }

        [Fact]
        public async Task DeleteTimeCodeValidAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
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
            var result = await _client.DeleteTimeCodeValidAsync("WG001", "NONE", "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteAllByJobCodeAsync Tests

        [Fact]
        public async Task DeleteAllByJobCodeAsync_WithValidParams_ReturnsSuccess()
        {
            // Arrange
            var jobCode = "JC001";
            var parentProject = "PP001";
            var expectedUrl = $"api/v1/timecodevalid/deletebyjobcode?jobCode={Uri.EscapeDataString(jobCode)}&parentProject={Uri.EscapeDataString(parentProject)}";
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteAllByJobCodeAsync(jobCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool?>(expectedUrl);
        }

        [Fact]
        public async Task DeleteAllByJobCodeAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteAllByJobCodeAsync("JC001", "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CopyWorkGroupAsync Tests

        [Fact]
        public async Task CopyWorkGroupAsync_WithValidParams_ReturnsCopiedTimeCodes()
        {
            // Arrange
            var sourceJobCode = "JC001";
            var targetJobCode = "JC002";
            var parentProject = "PP001";
            var expectedUrl = $"api/v1/timecodevalid/copy?sourceJobCode={Uri.EscapeDataString(sourceJobCode)}&targetJobCode={Uri.EscapeDataString(targetJobCode)}&parentProject={Uri.EscapeDataString(parentProject)}";
            var timeCodeList = new List<TimeCodeValidRes>
            {
                new() { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = parentProject, JobCode = targetJobCode },
                new() { TimeCode = "TC002", WorkGroup = "WG001", ParentProject = parentProject, JobCode = targetJobCode }
            };
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>> { Success = true, Data = timeCodeList };
            var expectedDto = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
                new List<TimeCodeValidDto>
                {
                    new() { TimeCode = "TC001", WorkGroup = "WG001", ParentProject = parentProject },
                    new() { TimeCode = "TC002", WorkGroup = "WG001", ParentProject = parentProject }
                }
            );

            _http.PostAsync<object, List<TimeCodeValidRes>>(expectedUrl, Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CopyWorkGroupAsync(sourceJobCode, targetJobCode, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).PostAsync<object, List<TimeCodeValidRes>>(expectedUrl, Arg.Any<object>());
        }

        [Fact]
        public async Task CopyWorkGroupAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "Copy failed", Code = "COPY_ERROR" } };
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<TimeCodeValidDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Copy failed", Code = "COPY_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<object, List<TimeCodeValidRes>>(Arg.Any<string>(), Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CopyWorkGroupAsync("JC001", "JC002", "PP001");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteBulkAsync Tests

        [Fact]
        public async Task DeleteBulkAsync_WithValidRequest_PostsToBulkDeleteEndpointAndReturnsSuccess()
        {
            // Arrange
            var requestDto = new BulkDeleteTimeCodeRequestDto
            {
                ParentProject = "PP001",
                Items = [new TimeCodeKeyItemDto { WorkGroup = "WG001", TimeCode = "TC001" }]
            };
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.PostAsync<BulkDeleteTimeCodeReq, bool?>("api/v1/timecodevalid/deletebulk", Arg.Any<BulkDeleteTimeCodeReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteBulkAsync(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).PostAsync<BulkDeleteTimeCodeReq, bool?>(
                "api/v1/timecodevalid/deletebulk", Arg.Any<BulkDeleteTimeCodeReq>());
        }

        [Fact]
        public async Task DeleteBulkAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var requestDto = new BulkDeleteTimeCodeRequestDto
            {
                ParentProject = "PP001",
                Items = [new TimeCodeKeyItemDto { WorkGroup = "WG001", TimeCode = "TC001" }]
            };
            var errors = new List<ApiError> { new() { Message = "Bulk delete failed", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Bulk delete failed", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<BulkDeleteTimeCodeReq, bool?>(Arg.Any<string>(), Arg.Any<BulkDeleteTimeCodeReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.DeleteBulkAsync(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CopySelectedWorkGroupsAsync Tests

        [Fact]
        public async Task CopySelectedWorkGroupsAsync_WithValidRequest_PostsToCopyBulkEndpointAndReturnsCopiedItems()
        {
            // Arrange
            var requestDto = new BulkCopyWorkGroupRequestDto
            {
                ParentProject = "PP001",
                SourceJobCode = "JC001",
                TargetJobCode = "JC002",
                WorkGroups = ["WG001", "WG002"]
            };
            var timeCodeList = new List<TimeCodeValidRes>
            {
                new() { TimeCode = "JC002", WorkGroup = "WG001", ParentProject = "PP001", JobCode = "JC002" },
                new() { TimeCode = "JC002", WorkGroup = "WG002", ParentProject = "PP001", JobCode = "JC002" }
            };
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>> { Success = true, Data = timeCodeList };
            var expectedDto = ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
                new List<TimeCodeValidDto>
                {
                    new() { TimeCode = "JC002", WorkGroup = "WG001", ParentProject = "PP001" },
                    new() { TimeCode = "JC002", WorkGroup = "WG002", ParentProject = "PP001" }
                }
            );

            _http.PostAsync<BulkCopyWorkGroupReq, List<TimeCodeValidRes>>(
                "api/v1/timecodevalid/copybulkworkgroups", Arg.Any<BulkCopyWorkGroupReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CopySelectedWorkGroupsAsync(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).PostAsync<BulkCopyWorkGroupReq, List<TimeCodeValidRes>>(
                "api/v1/timecodevalid/copybulkworkgroups", Arg.Any<BulkCopyWorkGroupReq>());
        }

        [Fact]
        public async Task CopySelectedWorkGroupsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var requestDto = new BulkCopyWorkGroupRequestDto
            {
                ParentProject = "PP001",
                SourceJobCode = "JC001",
                TargetJobCode = "JC002",
                WorkGroups = ["WG001"]
            };
            var errors = new List<ApiError> { new() { Message = "Copy failed", Code = "COPY_ERROR" } };
            var apiResponse = new ApiResponse<List<TimeCodeValidRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<TimeCodeValidDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Copy failed", Code = "COPY_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<BulkCopyWorkGroupReq, List<TimeCodeValidRes>>(Arg.Any<string>(), Arg.Any<BulkCopyWorkGroupReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<TimeCodeValidDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CopySelectedWorkGroupsAsync(requestDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion
    }
}