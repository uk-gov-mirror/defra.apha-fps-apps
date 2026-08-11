using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Web;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsMilestoneApiClientTest
{
    public class PimsMilestoneApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsMilestoneApiClient _client;

        public PimsMilestoneApiClientTests()
        {
            _http   = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsMilestoneApiClient(_http, _mapper);
        }

        #region Constructor

        [Fact]
        public void Constructor_WithValidDependencies_InitializesClient()
        {
            // Act
            var client = new PimsMilestoneApiClient(_http, _mapper);

            // Assert
            Assert.NotNull(client);
        }

        #endregion

        #region GetAllMilestonesAsync

        [Fact]
        public async Task GetAllMilestonesAsync_WithSuccessResponseAndData_ReturnsMappedDtoList()
        {
            // Arrange
            const string project = "PP001";
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };

            var resList     = new List<MilestoneRes> { new() { Project = project, Number = "M1" } };
            var apiResponse = new ApiResponse<List<MilestoneRes>> { Success = true, Data = resList };
            var mappedDto   = ApiResponseDto<List<MilestoneDto>>.SuccessResponse(
                new List<MilestoneDto> { new() { Project = project, Number = "M1" } });

            _http.GetAsync<List<MilestoneRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MilestoneDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllMilestonesAsync(query, project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(project, result.Data[0].Project);
            await _http.Received(1).GetAsync<List<MilestoneRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<MilestoneDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllMilestonesAsync_AppendProjectAndQueryStringToUrl()
        {
            // Arrange
            const string project = "PP001";
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };

            var apiResponse = new ApiResponse<List<MilestoneRes>> { Success = true, Data = new List<MilestoneRes>() };
            var mappedDto   = ApiResponseDto<List<MilestoneDto>>.SuccessResponse(new List<MilestoneDto>());

            _http.GetAsync<List<MilestoneRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MilestoneDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetAllMilestonesAsync(query, project);

            // Assert
            await _http.Received(1).GetAsync<List<MilestoneRes>>(
                Arg.Is<string>(u => u.Contains(PimsApiEndpoints.GetAllMilestones) && u.Contains($"project={project}")));
        }

        [Fact]
        public async Task GetAllMilestonesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };

            var errors      = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<MilestoneRes>> { Success = false, Data = null, Errors = errors };
            var mappedDto   = new ApiResponseDto<List<MilestoneDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<MilestoneRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MilestoneDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllMilestonesAsync(query, project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Not found", result.Errors[0].Message);
        }

        [Fact]
        public async Task GetAllMilestonesAsync_WhenSuccessWithNullData_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            var query            = new QueryParameters<string> { Page = 1, PageSize = 10 };

            var apiResponse = new ApiResponse<List<MilestoneRes>> { Success = true, Data = null };
            var mappedDto   = new ApiResponseDto<List<MilestoneDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "No data", Code = "NO_DATA" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<MilestoneRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MilestoneDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllMilestonesAsync(query, project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllMilestonesAsync_WhenHttpExecutorThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _http.GetAsync<List<MilestoneRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetAllMilestonesAsync(query, "PP001"));
        }

        #endregion

        #region GetMilestoneAsync

        [Fact]
        public async Task GetMilestoneAsync_WithSuccessResponseAndData_ReturnsMappedDto()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";
            var url         = string.Format(PimsApiEndpoints.GetMilestone, Uri.EscapeDataString(project), HttpUtility.UrlEncode(number));
            var milestoneRes = new MilestoneRes { Project = project, Number = number };
            var apiResponse  = new ApiResponse<MilestoneRes> { Success = true, Data = milestoneRes };
            var mappedDto    = ApiResponseDto<MilestoneDto>.SuccessResponse(new MilestoneDto { Project = project, Number = number });

            _http.GetAsync<MilestoneRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MilestoneDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetMilestoneAsync(project, number);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(project, result.Data.Project);
            Assert.Equal(number,  result.Data.Number);
            await _http.Received(1).GetAsync<MilestoneRes>(url);
            _mapper.Received(1).Map<ApiResponseDto<MilestoneDto>>(apiResponse);
        }

        [Fact]
        public async Task GetMilestoneAsync_WithSuccessResponseAndNullData_ReturnsSuccessWithNull()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "UNKNOWN";
            var url         = string.Format(PimsApiEndpoints.GetMilestone, Uri.EscapeDataString(project), HttpUtility.UrlEncode(number));
            var apiResponse = new ApiResponse<MilestoneRes> { Success = true, Data = null };

            _http.GetAsync<MilestoneRes>(url).Returns(apiResponse);

            // Act
            var result = await _client.GetMilestoneAsync(project, number);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Null(result.Data);
            _mapper.DidNotReceive().Map<ApiResponseDto<MilestoneDto>>(Arg.Any<ApiResponse<MilestoneRes>>());
        }

        [Fact]
        public async Task GetMilestoneAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";
            var url      = string.Format(PimsApiEndpoints.GetMilestone, Uri.EscapeDataString(project), HttpUtility.UrlEncode(number));
            var errors   = new List<ApiError> { new() { Message = "Milestone not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<MilestoneRes> { Success = false, Data = null, Errors = errors };
            var mappedDto   = new ApiResponseDto<MilestoneDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Milestone not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<MilestoneRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MilestoneDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetMilestoneAsync(project, number);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetMilestoneAsync_UrlEncodesProjectAndNumber()
        {
            // Arrange — special characters require encoding
            const string project = "PP 001";
            const string number  = "M/1";
            var expectedUrl = string.Format(PimsApiEndpoints.GetMilestone, Uri.EscapeDataString(project), HttpUtility.UrlEncode(number));
            var apiResponse = new ApiResponse<MilestoneRes> { Success = true, Data = new MilestoneRes { Project = project, Number = number } };
            var mappedDto   = ApiResponseDto<MilestoneDto>.SuccessResponse(new MilestoneDto { Project = project, Number = number });

            _http.GetAsync<MilestoneRes>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MilestoneDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetMilestoneAsync(project, number);

            // Assert
            await _http.Received(1).GetAsync<MilestoneRes>(Arg.Is<string>(u => u == expectedUrl));
        }

        [Fact]
        public async Task GetMilestoneAsync_WhenHttpExecutorThrows_PropagatesException()
        {
            // Arrange
            var url = string.Format(PimsApiEndpoints.GetMilestone, Uri.EscapeDataString("PP001"), HttpUtility.UrlEncode("M1"));
            _http.GetAsync<MilestoneRes>(url).ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetMilestoneAsync("PP001", "M1"));
        }

        #endregion

        #region SaveMilestoneAsync

        [Fact]
        public async Task SaveMilestoneAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            const string project = "PP001";
            var dto       = new MilestoneDto { Project = project, Number = "M1", DateDue = DateTime.Today.AddDays(30) };
            var request   = new MilestoneReq  { Project = project, Number = "M1" };
            var res       = new MilestoneRes  { Project = project, Number = "M1" };
            var url        = string.Format(PimsApiEndpoints.SaveMilestone, Uri.EscapeDataString(project));
            var apiResponse = new ApiResponse<MilestoneRes> { Success = true, Data = res };
            var mappedDto   = ApiResponseDto<MilestoneDto>.SuccessResponse(new MilestoneDto { Project = project, Number = "M1" });

            _mapper.Map<MilestoneReq>(dto).Returns(request);
            _http.PostAsync<MilestoneReq, MilestoneRes>(url, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MilestoneDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.SaveMilestoneAsync(project, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("M1", result.Data.Number);
            _mapper.Received(1).Map<MilestoneReq>(dto);
            await _http.Received(1).PostAsync<MilestoneReq, MilestoneRes>(url, request);
            _mapper.Received(1).Map<ApiResponseDto<MilestoneDto>>(apiResponse);
        }

        [Fact]
        public async Task SaveMilestoneAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            var dto       = new MilestoneDto { Project = project, Number = "M1" };
            var request   = new MilestoneReq  { Project = project, Number = "M1" };
            var url        = string.Format(PimsApiEndpoints.SaveMilestone, Uri.EscapeDataString(project));
            var errors     = new List<ApiError> { new() { Message = "Validation error", Code = "VALIDATION_ERROR" } };
            var apiResponse = new ApiResponse<MilestoneRes> { Success = false, Data = null, Errors = errors };
            var mappedDto   = new ApiResponseDto<MilestoneDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Validation error", Code = "VALIDATION_ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<MilestoneReq>(dto).Returns(request);
            _http.PostAsync<MilestoneReq, MilestoneRes>(url, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MilestoneDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.SaveMilestoneAsync(project, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("VALIDATION_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task SaveMilestoneAsync_CallsCorrectUrlWithEscapedProject()
        {
            // Arrange
            const string project    = "PP001";
            var expectedUrl = string.Format(PimsApiEndpoints.SaveMilestone, Uri.EscapeDataString(project));
            var dto       = new MilestoneDto { Project = project, Number = "M1" };
            var request   = new MilestoneReq  { Project = project, Number = "M1" };
            var apiResponse = new ApiResponse<MilestoneRes> { Success = true, Data = new MilestoneRes { Project = project } };
            var mappedDto   = ApiResponseDto<MilestoneDto>.SuccessResponse(new MilestoneDto { Project = project });

            _mapper.Map<MilestoneReq>(dto).Returns(request);
            _http.PostAsync<MilestoneReq, MilestoneRes>(expectedUrl, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MilestoneDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.SaveMilestoneAsync(project, dto);

            // Assert
            await _http.Received(1).PostAsync<MilestoneReq, MilestoneRes>(
                Arg.Is<string>(u => u == expectedUrl), Arg.Any<MilestoneReq>());
        }

        [Fact]
        public async Task SaveMilestoneAsync_WhenMapperThrowsOnRequestMapping_PropagatesException()
        {
            // Arrange
            var dto = new MilestoneDto { Project = "PP001", Number = "M1" };
            _mapper.Map<MilestoneReq>(dto).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _client.SaveMilestoneAsync("PP001", dto));
        }

        #endregion

        #region UpdateMilestoneAsync

        [Fact]
        public async Task UpdateMilestoneAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";
            var dto       = new MilestoneDto { Project = project, Number = number, Description = "Updated" };
            var request   = new MilestoneReq  { Project = project, Number = number };
            var res       = new MilestoneRes  { Project = project, Number = number, Description = "Updated" };
            var url        = string.Format(PimsApiEndpoints.UpdateMilestone, Uri.EscapeDataString(project), HttpUtility.UrlEncode(number));
            var apiResponse = new ApiResponse<MilestoneRes> { Success = true, Data = res };
            var mappedDto   = ApiResponseDto<MilestoneDto>.SuccessResponse(new MilestoneDto { Project = project, Number = number, Description = "Updated" });

            _mapper.Map<MilestoneReq>(dto).Returns(request);
            _http.PutAsync<MilestoneReq, MilestoneRes>(url, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MilestoneDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateMilestoneAsync(project, number, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Updated", result.Data.Description);
            _mapper.Received(1).Map<MilestoneReq>(dto);
            await _http.Received(1).PutAsync<MilestoneReq, MilestoneRes>(url, request);
            _mapper.Received(1).Map<ApiResponseDto<MilestoneDto>>(apiResponse);
        }

        [Fact]
        public async Task UpdateMilestoneAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";
            var dto       = new MilestoneDto { Project = project, Number = number };
            var request   = new MilestoneReq  { Project = project, Number = number };
            var url        = string.Format(PimsApiEndpoints.UpdateMilestone, Uri.EscapeDataString(project), HttpUtility.UrlEncode(number));
            var errors     = new List<ApiError> { new() { Message = "Validation error", Code = "VALIDATION_ERROR" } };
            var apiResponse = new ApiResponse<MilestoneRes> { Success = false, Data = null, Errors = errors };
            var mappedDto   = new ApiResponseDto<MilestoneDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Validation error", Code = "VALIDATION_ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<MilestoneReq>(dto).Returns(request);
            _http.PutAsync<MilestoneReq, MilestoneRes>(url, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MilestoneDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateMilestoneAsync(project, number, dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("VALIDATION_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task UpdateMilestoneAsync_UrlEncodesProjectAndNumber()
        {
            // Arrange
            const string project = "PP 001";
            const string number  = "M/1";
            var expectedUrl = string.Format(PimsApiEndpoints.UpdateMilestone, Uri.EscapeDataString(project), HttpUtility.UrlEncode(number));
            var dto       = new MilestoneDto { Project = project, Number = number };
            var request   = new MilestoneReq();
            var apiResponse = new ApiResponse<MilestoneRes> { Success = true, Data = new MilestoneRes() };
            var mappedDto   = ApiResponseDto<MilestoneDto>.SuccessResponse(new MilestoneDto());

            _mapper.Map<MilestoneReq>(dto).Returns(request);
            _http.PutAsync<MilestoneReq, MilestoneRes>(expectedUrl, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MilestoneDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.UpdateMilestoneAsync(project, number, dto);

            // Assert
            await _http.Received(1).PutAsync<MilestoneReq, MilestoneRes>(
                Arg.Is<string>(u => u == expectedUrl), Arg.Any<MilestoneReq>());
        }

        #endregion

        #region UpdateMilestoneAsync_PMD

        [Fact]
        public async Task UpdateMilestoneAsync_PMD_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";
            var dto       = new MilestoneDto 
            { 
                Project = project, 
                Number = number, 
                UnderSdReview = 1,
                OnTarget = 0,
                DateCompleted = DateTime.Now.AddDays(-5),
                ProjectLeaderComment = "Completed on time"
            };
            var request   = new MilestoneReq  
            { 
                Project = project, 
                Number = number,
                UnderSdReview = 1,
                OnTarget = 0,
                DateCompleted = DateTime.Now.AddDays(-5),
                ProjectLeaderComment = "Completed on time"
            };
            var url = $"{PimsApiEndpoints.UpdateMilestoneAsync_PMD}?project={Uri.EscapeDataString(project)}&number={HttpUtility.UrlEncode(number)}";
            var res = new MilestoneRes  
            { 
                Project = project, 
                Number = number,
                UnderSdReview = 1,
                OnTarget = 0,
                DateCompleted = DateTime.Now.AddDays(-5),
                ProjectLeaderComment = "Completed on time"
            };
            var apiResponse = new ApiResponse<MilestoneRes> { Success = true, Data = res };
            var mappedDto   = ApiResponseDto<MilestoneDto>.SuccessResponse(new MilestoneDto 
            { 
                Project = project, 
                Number = number,
                UnderSdReview = 1,
                OnTarget = 0,
                DateCompleted = DateTime.Now.AddDays(-5),
                ProjectLeaderComment = "Completed on time"
            });

            _mapper.Map<MilestoneReq>(dto).Returns(request);
            _http.PutAsync<MilestoneReq, MilestoneRes>(url, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MilestoneDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateMilestoneAsync_PMD(project, number, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Completed on time", result.Data.ProjectLeaderComment);
            Assert.Equal((short)1, result.Data.UnderSdReview);
            _mapper.Received(1).Map<MilestoneReq>(dto);
            await _http.Received(1).PutAsync<MilestoneReq, MilestoneRes>(url, request);
            _mapper.Received(1).Map<ApiResponseDto<MilestoneDto>>(apiResponse);
        }

        [Fact]
        public async Task UpdateMilestoneAsync_PMD_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";
            var dto       = new MilestoneDto { Project = project, Number = number };
            var request   = new MilestoneReq  { Project = project, Number = number };
            var url = $"{PimsApiEndpoints.UpdateMilestoneAsync_PMD}?project={Uri.EscapeDataString(project)}&number={HttpUtility.UrlEncode(number)}";
            var errors     = new List<ApiError> { new() { Message = "Validation error", Code = "VALIDATION_ERROR" } };
            var apiResponse = new ApiResponse<MilestoneRes> { Success = false, Data = null, Errors = errors };
            var mappedDto   = new ApiResponseDto<MilestoneDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Validation error", Code = "VALIDATION_ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<MilestoneReq>(dto).Returns(request);
            _http.PutAsync<MilestoneReq, MilestoneRes>(url, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MilestoneDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateMilestoneAsync_PMD(project, number, dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("VALIDATION_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task UpdateMilestoneAsync_PMD_UrlEncodesProjectAndNumberAsQueryParams()
        {
            // Arrange
            const string project = "PP/001";
            const string number  = "M-1";
            var dto       = new MilestoneDto { Project = project, Number = number };
            var request   = new MilestoneReq  { Project = project, Number = number };
            var expectedUrl = $"{PimsApiEndpoints.UpdateMilestoneAsync_PMD}?project={Uri.EscapeDataString(project)}&number={HttpUtility.UrlEncode(number)}";
            var res = new MilestoneRes  { Project = project, Number = number };
            var apiResponse = new ApiResponse<MilestoneRes> { Success = true, Data = res };
            var mappedDto   = ApiResponseDto<MilestoneDto>.SuccessResponse(new MilestoneDto { Project = project, Number = number });

            _mapper.Map<MilestoneReq>(dto).Returns(request);
            _http.PutAsync<MilestoneReq, MilestoneRes>(expectedUrl, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MilestoneDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.UpdateMilestoneAsync_PMD(project, number, dto);

            // Assert
            await _http.Received(1).PutAsync<MilestoneReq, MilestoneRes>(
                Arg.Is<string>(u => u == expectedUrl), Arg.Any<MilestoneReq>());
        }

        #endregion

        #region DeleteMilestoneAsync

        [Fact]
        public async Task DeleteMilestoneAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";
            var url         = string.Format(PimsApiEndpoints.DeleteMilestone, Uri.EscapeDataString(project), HttpUtility.UrlEncode(number));
            var apiResponse = new ApiResponse<object> { Success = true, Data = new { success = true } };
            var mappedDto   = ApiResponseDto<object>.SuccessResponse(new { success = true });

            _http.DeleteAsync<object>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteMilestoneAsync(project, number);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<object>(url);
            _mapper.Received(1).Map<ApiResponseDto<object>>(apiResponse);
        }

        [Fact]
        public async Task DeleteMilestoneAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            const string number  = "M1";
            var url     = string.Format(PimsApiEndpoints.DeleteMilestone, Uri.EscapeDataString(project), HttpUtility.UrlEncode(number));
            var errors  = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<object> { Success = false, Errors = errors };
            var mappedDto   = new ApiResponseDto<object>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.DeleteAsync<object>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteMilestoneAsync(project, number);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        [Fact]
        public async Task DeleteMilestoneAsync_UrlEncodesProjectAndNumber()
        {
            // Arrange
            const string project = "PP 001";
            const string number  = "M/1";
            var expectedUrl = string.Format(PimsApiEndpoints.DeleteMilestone, Uri.EscapeDataString(project), HttpUtility.UrlEncode(number));
            var apiResponse = new ApiResponse<object> { Success = true, Data = new object() };
            var mappedDto   = ApiResponseDto<object>.SuccessResponse(new object());

            _http.DeleteAsync<object>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.DeleteMilestoneAsync(project, number);

            // Assert
            await _http.Received(1).DeleteAsync<object>(Arg.Is<string>(u => u == expectedUrl));
        }

        #endregion

        #region UpdateFormRequiredAsync

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task UpdateFormRequiredAsync_WithSuccessResponse_ReturnsMappedDto(bool formRequired)
        {
            // Arrange
            const string parent = "PP001";
            var url         = string.Format(PimsApiEndpoints.UpdateFormRequired, Uri.EscapeDataString(parent));
            var apiResponse = new ApiResponse<object> { Success = true, Data = new { success = true } };
            var mappedDto   = ApiResponseDto<object>.SuccessResponse(new { success = true });

            _http.PatchAsync<bool, object>(url, formRequired).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateFormRequiredAsync(parent, formRequired);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _http.Received(1).PatchAsync<bool, object>(url, formRequired);
            _mapper.Received(1).Map<ApiResponseDto<object>>(apiResponse);
        }

        [Fact]
        public async Task UpdateFormRequiredAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string parent = "PP001";
            var url     = string.Format(PimsApiEndpoints.UpdateFormRequired, Uri.EscapeDataString(parent));
            var errors  = new List<ApiError> { new() { Message = "Server error", Code = "SERVER_ERROR" } };
            var apiResponse = new ApiResponse<object> { Success = false, Errors = errors };
            var mappedDto   = new ApiResponseDto<object>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Server error", Code = "SERVER_ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.PatchAsync<bool, object>(url, true).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateFormRequiredAsync(parent, true);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("SERVER_ERROR", result.Errors![0].Code);
        }

        [Fact]
        public async Task UpdateFormRequiredAsync_CallsCorrectUrl()
        {
            // Arrange
            const string parent    = "PP001";
            var expectedUrl = string.Format(PimsApiEndpoints.UpdateFormRequired, Uri.EscapeDataString(parent));
            var apiResponse = new ApiResponse<object> { Success = true, Data = new object() };
            var mappedDto   = ApiResponseDto<object>.SuccessResponse(new object());

            _http.PatchAsync<bool, object>(expectedUrl, true).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.UpdateFormRequiredAsync(parent, true);

            // Assert
            await _http.Received(1).PatchAsync<bool, object>(
                Arg.Is<string>(u => u == expectedUrl), Arg.Any<bool>());
        }

        #endregion

        #region GetMilestoneTypesAsync

        [Fact]
        public async Task GetMilestoneTypesAsync_WithSuccessResponseAndNoFilter_ReturnsMappedDtoList()
        {
            // Arrange
            var typeResList = new List<MilestoneTypeRes> { new() { IdType = 'A', Type = "Alpha" } };
            var apiResponse = new ApiResponse<List<MilestoneTypeRes>> { Success = true, Data = typeResList };
            var mappedDto   = ApiResponseDto<List<MilestoneTypeDto>>.SuccessResponse(
                new List<MilestoneTypeDto> { new() { IdType = 'A', Type = "Alpha" } });

            _http.GetAsync<List<MilestoneTypeRes>>(PimsApiEndpoints.GetMilestoneTypes).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MilestoneTypeDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetMilestoneTypesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _http.Received(1).GetAsync<List<MilestoneTypeRes>>(PimsApiEndpoints.GetMilestoneTypes);
        }

        [Fact]
        public async Task GetMilestoneTypesAsync_WithFilter_AppendsFilterToUrl()
        {
            // Arrange
            const string filter      = "M";
            var expectedUrl   = $"{PimsApiEndpoints.GetMilestoneTypes}?milestoneDeliverable={Uri.EscapeDataString(filter)}";
            var typeResList   = new List<MilestoneTypeRes> { new() { IdType = 'B', Type = "Beta", MilestoneDeliverable = 'M' } };
            var apiResponse   = new ApiResponse<List<MilestoneTypeRes>> { Success = true, Data = typeResList };
            var mappedDto     = ApiResponseDto<List<MilestoneTypeDto>>.SuccessResponse(
                new List<MilestoneTypeDto> { new() { IdType = 'B', Type = "Beta", MilestoneDeliverable = 'M' } });

            _http.GetAsync<List<MilestoneTypeRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MilestoneTypeDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetMilestoneTypesAsync(filter);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<MilestoneTypeRes>>(Arg.Is<string>(u => u == expectedUrl));
        }

        [Fact]
        public async Task GetMilestoneTypesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors      = new List<ApiError> { new() { Message = "Server error", Code = "SERVER_ERROR" } };
            var apiResponse = new ApiResponse<List<MilestoneTypeRes>> { Success = false, Errors = errors };
            var mappedDto   = new ApiResponseDto<List<MilestoneTypeDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Server error", Code = "SERVER_ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<MilestoneTypeRes>>(PimsApiEndpoints.GetMilestoneTypes).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MilestoneTypeDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetMilestoneTypesAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("SERVER_ERROR", result.Errors![0].Code);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetMilestoneTypesAsync_WhenFilterIsNullOrWhitespace_UsesBaseUrl(string? filter)
        {
            // Arrange
            var apiResponse = new ApiResponse<List<MilestoneTypeRes>> { Success = true, Data = new List<MilestoneTypeRes>() };
            var mappedDto   = ApiResponseDto<List<MilestoneTypeDto>>.SuccessResponse(new List<MilestoneTypeDto>());

            _http.GetAsync<List<MilestoneTypeRes>>(PimsApiEndpoints.GetMilestoneTypes).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MilestoneTypeDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetMilestoneTypesAsync(filter);

            // Assert — no query string appended for null/whitespace filter
            await _http.Received(1).GetAsync<List<MilestoneTypeRes>>(
                Arg.Is<string>(u => u == PimsApiEndpoints.GetMilestoneTypes));
        }

        #endregion

        #region GetAllMilestoneFormDatesAsync

        [Fact]
        public async Task GetAllMilestoneFormDatesAsync_WithSuccessResponseAndData_ReturnsMappedDtoList()
        {
            // Arrange
            const string parent = "PP001";
            var parameters  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var formDateRes = new List<MilestoneFormDatesRes> { new() { Year = 2024, ParentProject = parent } };
            var apiResponse = new ApiResponse<List<MilestoneFormDatesRes>> { Success = true, Data = formDateRes };
            var mappedDto   = ApiResponseDto<List<MilestoneFormDatesDto>>.SuccessResponse(
                new List<MilestoneFormDatesDto> { new() { Year = 2024, ParentProject = parent } });

            _http.GetAsync<List<MilestoneFormDatesRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MilestoneFormDatesDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllMilestoneFormDatesAsync(parent, parameters);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal((short)2024, result.Data[0].Year);
            await _http.Received(1).GetAsync<List<MilestoneFormDatesRes>>(
                Arg.Is<string>(u => u.Contains(Uri.EscapeDataString(parent))));
        }

        [Fact]
        public async Task GetAllMilestoneFormDatesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string parent = "PP001";
            var parameters  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors      = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<MilestoneFormDatesRes>> { Success = false, Data = null, Errors = errors };
            var mappedDto   = new ApiResponseDto<List<MilestoneFormDatesDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<MilestoneFormDatesRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MilestoneFormDatesDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllMilestoneFormDatesAsync(parent, parameters);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        [Fact]
        public async Task GetAllMilestoneFormDatesAsync_WhenSuccessWithNullData_ReturnsFailureResponse()
        {
            // Arrange
            const string parent = "PP001";
            var parameters  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<MilestoneFormDatesRes>> { Success = true, Data = null };
            var mappedDto   = new ApiResponseDto<List<MilestoneFormDatesDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto>(),
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<MilestoneFormDatesRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MilestoneFormDatesDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllMilestoneFormDatesAsync(parent, parameters);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetMilestoneFormDatesAsync

        [Fact]
        public async Task GetMilestoneFormDatesAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            const string parent = "PP001";
            const short  year   = 2024;
            var url         = string.Format(PimsApiEndpoints.GetMilestoneFormDates, Uri.EscapeDataString(parent), year);
            var res         = new MilestoneFormDatesRes { Year = year, ParentProject = parent, Jan = new DateTime(2024, 1, 31) };
            var apiResponse = new ApiResponse<MilestoneFormDatesRes> { Success = true, Data = res };
            var mappedDto   = ApiResponseDto<MilestoneFormDatesDto>.SuccessResponse(
                new MilestoneFormDatesDto { Year = year, ParentProject = parent, Jan = new DateTime(2024, 1, 31) });

            _http.GetAsync<MilestoneFormDatesRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MilestoneFormDatesDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetMilestoneFormDatesAsync(parent, year);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(year, result.Data.Year);
            Assert.Equal(new DateTime(2024, 1, 31), result.Data.Jan);
            await _http.Received(1).GetAsync<MilestoneFormDatesRes>(url);
            _mapper.Received(1).Map<ApiResponseDto<MilestoneFormDatesDto>>(apiResponse);
        }

        [Fact]
        public async Task GetMilestoneFormDatesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string parent = "PP001";
            const short  year   = 2024;
            var url     = string.Format(PimsApiEndpoints.GetMilestoneFormDates, Uri.EscapeDataString(parent), year);
            var errors  = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<MilestoneFormDatesRes> { Success = false, Errors = errors };
            var mappedDto   = new ApiResponseDto<MilestoneFormDatesDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<MilestoneFormDatesRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MilestoneFormDatesDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetMilestoneFormDatesAsync(parent, year);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        [Fact]
        public async Task GetMilestoneFormDatesAsync_CallsCorrectUrl()
        {
            // Arrange
            const string parent     = "PP001";
            const short  year       = 2024;
            var expectedUrl = string.Format(PimsApiEndpoints.GetMilestoneFormDates, Uri.EscapeDataString(parent), year);
            var apiResponse = new ApiResponse<MilestoneFormDatesRes> { Success = true, Data = new MilestoneFormDatesRes { Year = year } };
            var mappedDto   = ApiResponseDto<MilestoneFormDatesDto>.SuccessResponse(new MilestoneFormDatesDto { Year = year });

            _http.GetAsync<MilestoneFormDatesRes>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MilestoneFormDatesDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetMilestoneFormDatesAsync(parent, year);

            // Assert
            await _http.Received(1).GetAsync<MilestoneFormDatesRes>(Arg.Is<string>(u => u == expectedUrl));
        }

        #endregion

        #region SaveMilestoneFormDatesAsync

        [Fact]
        public async Task SaveMilestoneFormDatesAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            const string parent = "PP001";
            var dto       = new MilestoneFormDatesDto { Year = 2024, ParentProject = parent, Jan = new DateTime(2024, 1, 31) };
            var request   = new MilestoneFormDatesReq { Year = 2024, ParentProject = parent };
            var res       = new MilestoneFormDatesRes { Year = 2024, ParentProject = parent };
            var url        = string.Format(PimsApiEndpoints.SaveMilestoneFormDates, Uri.EscapeDataString(parent));
            var apiResponse = new ApiResponse<MilestoneFormDatesRes> { Success = true, Data = res };
            var mappedDto   = ApiResponseDto<MilestoneFormDatesDto>.SuccessResponse(new MilestoneFormDatesDto { Year = 2024, ParentProject = parent });

            _mapper.Map<MilestoneFormDatesReq>(dto).Returns(request);
            _http.PostAsync<MilestoneFormDatesReq, MilestoneFormDatesRes>(url, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MilestoneFormDatesDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.SaveMilestoneFormDatesAsync(parent, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal((short)2024, result.Data.Year);
            _mapper.Received(1).Map<MilestoneFormDatesReq>(dto);
            await _http.Received(1).PostAsync<MilestoneFormDatesReq, MilestoneFormDatesRes>(url, request);
            _mapper.Received(1).Map<ApiResponseDto<MilestoneFormDatesDto>>(apiResponse);
        }

        [Fact]
        public async Task SaveMilestoneFormDatesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string parent = "PP001";
            var dto       = new MilestoneFormDatesDto { Year = 2024, ParentProject = parent };
            var request   = new MilestoneFormDatesReq { Year = 2024, ParentProject = parent };
            var url        = string.Format(PimsApiEndpoints.SaveMilestoneFormDates, Uri.EscapeDataString(parent));
            var errors     = new List<ApiError> { new() { Message = "Validation error", Code = "VALIDATION_ERROR" } };
            var apiResponse = new ApiResponse<MilestoneFormDatesRes> { Success = false, Errors = errors };
            var mappedDto   = new ApiResponseDto<MilestoneFormDatesDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Validation error", Code = "VALIDATION_ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<MilestoneFormDatesReq>(dto).Returns(request);
            _http.PostAsync<MilestoneFormDatesReq, MilestoneFormDatesRes>(url, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MilestoneFormDatesDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.SaveMilestoneFormDatesAsync(parent, dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("VALIDATION_ERROR", result.Errors![0].Code);
        }

        [Fact]
        public async Task SaveMilestoneFormDatesAsync_CallsCorrectUrl()
        {
            // Arrange
            const string parent     = "PP001";
            var expectedUrl = string.Format(PimsApiEndpoints.SaveMilestoneFormDates, Uri.EscapeDataString(parent));
            var dto       = new MilestoneFormDatesDto { Year = 2024, ParentProject = parent };
            var request   = new MilestoneFormDatesReq { Year = 2024 };
            var apiResponse = new ApiResponse<MilestoneFormDatesRes> { Success = true, Data = new MilestoneFormDatesRes() };
            var mappedDto   = ApiResponseDto<MilestoneFormDatesDto>.SuccessResponse(new MilestoneFormDatesDto());

            _mapper.Map<MilestoneFormDatesReq>(dto).Returns(request);
            _http.PostAsync<MilestoneFormDatesReq, MilestoneFormDatesRes>(expectedUrl, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<MilestoneFormDatesDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.SaveMilestoneFormDatesAsync(parent, dto);

            // Assert
            await _http.Received(1).PostAsync<MilestoneFormDatesReq, MilestoneFormDatesRes>(
                Arg.Is<string>(u => u == expectedUrl), Arg.Any<MilestoneFormDatesReq>());
        }

        #endregion

        #region DeleteMilestoneFormDatesAsync — has try/catch

        [Fact]
        public async Task DeleteMilestoneFormDatesAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            const string parent = "PP001";
            const short  year   = 2024;
            var url         = string.Format(PimsApiEndpoints.DeleteMilestoneFormDates, Uri.EscapeDataString(parent), year);
            var apiResponse = new ApiResponse<object> { Success = true, Data = new object() };
            var mappedDto   = ApiResponseDto<object>.SuccessResponse(new object());

            _http.DeleteAsync<object>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteMilestoneFormDatesAsync(parent, year);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<object>(url);
            _mapper.Received(1).Map<ApiResponseDto<object>>(apiResponse);
        }

        [Fact]
        public async Task DeleteMilestoneFormDatesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string parent = "PP001";
            const short  year   = 2024;
            var url     = string.Format(PimsApiEndpoints.DeleteMilestoneFormDates, Uri.EscapeDataString(parent), year);
            var errors  = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<object> { Success = false, Errors = errors };
            var mappedDto   = new ApiResponseDto<object>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.DeleteAsync<object>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteMilestoneFormDatesAsync(parent, year);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        [Fact]
        public async Task DeleteMilestoneFormDatesAsync_WhenHttpExecutorThrows_ReturnsInternalError()
        {
            // Arrange — DeleteMilestoneFormDatesAsync has a try/catch; exception should be swallowed
            const string parent = "PP001";
            const short  year   = 2024;
            var url = string.Format(PimsApiEndpoints.DeleteMilestoneFormDates, Uri.EscapeDataString(parent), year);

            _http.DeleteAsync<object>(url).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.DeleteMilestoneFormDatesAsync(parent, year);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to delete milestone form dates", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task DeleteMilestoneFormDatesAsync_CallsCorrectUrl()
        {
            // Arrange
            const string parent     = "PP001";
            const short  year       = 2024;
            var expectedUrl = string.Format(PimsApiEndpoints.DeleteMilestoneFormDates, Uri.EscapeDataString(parent), year);
            var apiResponse = new ApiResponse<object> { Success = true, Data = new object() };
            var mappedDto   = ApiResponseDto<object>.SuccessResponse(new object());

            _http.DeleteAsync<object>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.DeleteMilestoneFormDatesAsync(parent, year);

            // Assert
            await _http.Received(1).DeleteAsync<object>(Arg.Is<string>(u => u == expectedUrl));
        }

        #endregion

        #region GetLogMilestonesAsync

        [Fact]
        public async Task GetLogMilestonesAsync_WithSuccessResponseAndData_ReturnsMappedDtoList()
        {
            // Arrange
            const string project     = "PP001";
            const string numberPart1 = "M";
            const string numberPart2 = "1";
            var parameters  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList     = new List<LogMilestoneRes> { new() { Project = project, Number = "M1", Description = "Log Entry 1" } };
            var apiResponse = new ApiResponse<List<LogMilestoneRes>> { Success = true, Data = resList };
            var mappedDto   = ApiResponseDto<List<LogMilestoneDto>>.SuccessResponse(
                new List<LogMilestoneDto> { new() { Project = project, Number = "M1", Description = "Log Entry 1" } });

            _http.GetAsync<List<LogMilestoneRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<LogMilestoneDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetLogMilestonesAsync(parameters, project, numberPart1, numberPart2);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            Assert.Equal(project, result.Data[0].Project);
            Assert.Equal("M1",    result.Data[0].Number);
            await _http.Received(1).GetAsync<List<LogMilestoneRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<LogMilestoneDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetLogMilestonesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var parameters  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors      = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<LogMilestoneRes>> { Success = false, Data = null, Errors = errors };
            var mappedDto   = new ApiResponseDto<List<LogMilestoneDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<LogMilestoneRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<LogMilestoneDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetLogMilestonesAsync(parameters, null, null, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("NOT_FOUND", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetLogMilestonesAsync_WhenSuccessWithNullData_ReturnsFailureResponse()
        {
            // Arrange
            var parameters  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<LogMilestoneRes>> { Success = true, Data = null };
            var mappedDto   = new ApiResponseDto<List<LogMilestoneDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto>(),
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<LogMilestoneRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<LogMilestoneDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetLogMilestonesAsync(parameters, null, null, null);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetLogMilestonesAsync_WithAllOptionalParams_AppendsAllParamsToUrl()
        {
            // Arrange
            const string project     = "PP001";
            const string numberPart1 = "M";
            const string numberPart2 = "5";
            var parameters  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<LogMilestoneRes>> { Success = true, Data = new List<LogMilestoneRes>() };
            var mappedDto   = ApiResponseDto<List<LogMilestoneDto>>.SuccessResponse(new List<LogMilestoneDto>());

            _http.GetAsync<List<LogMilestoneRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<LogMilestoneDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetLogMilestonesAsync(parameters, project, numberPart1, numberPart2);

            // Assert
            await _http.Received(1).GetAsync<List<LogMilestoneRes>>(
                Arg.Is<string>(u =>
                    u.Contains(PimsApiEndpoints.GetLogMilestones) &&
                    u.Contains($"project={Uri.EscapeDataString(project)}") &&
                    u.Contains($"numberPart1={Uri.EscapeDataString(numberPart1)}") &&
                    u.Contains($"numberPart2={Uri.EscapeDataString(numberPart2)}")));
        }

        [Theory]
        [InlineData(null,  null,  null)]
        [InlineData("",    "",    "")]
        [InlineData("   ", "   ", "   ")]
        public async Task GetLogMilestonesAsync_WhenOptionalParamsAreNullOrWhitespace_DoesNotAppendThem(
            string? project, string? numberPart1, string? numberPart2)
        {
            // Arrange
            var parameters  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<LogMilestoneRes>> { Success = true, Data = new List<LogMilestoneRes>() };
            var mappedDto   = ApiResponseDto<List<LogMilestoneDto>>.SuccessResponse(new List<LogMilestoneDto>());

            _http.GetAsync<List<LogMilestoneRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<LogMilestoneDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetLogMilestonesAsync(parameters, project, numberPart1, numberPart2);

            // Assert — no optional params appended; URL must not contain the param keys
            await _http.Received(1).GetAsync<List<LogMilestoneRes>>(
                Arg.Is<string>(u =>
                    !u.Contains("&project=") &&
                    !u.Contains("&numberPart1=") &&
                    !u.Contains("&numberPart2=")));
        }

        [Fact]
        public async Task GetLogMilestonesAsync_WhenHttpExecutorThrows_PropagatesException()
        {
            // Arrange
            var parameters = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _http.GetAsync<List<LogMilestoneRes>>(Arg.Any<string>()).ThrowsAsync(new Exception("Network error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _client.GetLogMilestonesAsync(parameters, "PP001", null, null));
        }

        #endregion

        #region GetAllStagingRowsAsync

        [Fact]
        public async Task GetAllStagingRowsAsync_WithSuccessResponseAndData_ReturnsMappedDtoList()
        {
            // Arrange
            var parameters  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList     = new List<StagingMilestoneRes> { new() { Id = 1, Project = "PP001", Number = "M1" } };
            var apiResponse = new ApiResponse<List<StagingMilestoneRes>> { Success = true, Data = resList };
            var mappedDto   = ApiResponseDto<List<StagingMilestoneDto>>.SuccessResponse(
                new List<StagingMilestoneDto> { new() { Id = 1, Project = "PP001", Number = "M1" } });

            _http.GetAsync<List<StagingMilestoneRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StagingMilestoneDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllStagingRowsAsync(parameters);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<StagingMilestoneRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<StagingMilestoneDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetAllStagingRowsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var parameters  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors      = new List<ApiError> { new() { Message = "Server error", Code = "SERVER_ERROR" } };
            var apiResponse = new ApiResponse<List<StagingMilestoneRes>> { Success = false, Errors = errors };
            var mappedDto   = new ApiResponseDto<List<StagingMilestoneDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Server error", Code = "SERVER_ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<StagingMilestoneRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StagingMilestoneDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllStagingRowsAsync(parameters);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("SERVER_ERROR", result.Errors![0].Code);
        }

        [Fact]
        public async Task GetAllStagingRowsAsync_WithSuccessAndNullData_ReturnsFailureResponse()
        {
            // Arrange
            var parameters  = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<StagingMilestoneRes>> { Success = true, Data = null };
            var mappedDto   = new ApiResponseDto<List<StagingMilestoneDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto>(),
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<StagingMilestoneRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StagingMilestoneDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetAllStagingRowsAsync(parameters);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetStagingRowsAsync

        [Fact]
        public async Task GetStagingRowsAsync_WithId_ReturnsRows()
        {
            // Arrange
            const int id = 1;
            var expectedUrl = $"{PimsApiEndpoints.GetStagingMilestones}?id={id}";
            var apiResponse = new ApiResponse<List<StagingMilestoneRes>> { Success = true, Data = new List<StagingMilestoneRes>() };
            var mappedDto   = ApiResponseDto<List<StagingMilestoneDto>>.SuccessResponse(new List<StagingMilestoneDto>());

            _http.GetAsync<List<StagingMilestoneRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StagingMilestoneDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetStagingRowsAsync(id);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).GetAsync<List<StagingMilestoneRes>>(Arg.Is<string>(u => u == expectedUrl));
        }

        [Fact]
        public async Task GetStagingRowsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const int id = 1;
            var expectedUrl = $"{PimsApiEndpoints.GetStagingMilestones}?id={id}";
            var errors      = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<StagingMilestoneRes>> { Success = false, Errors = errors };
            var mappedDto   = new ApiResponseDto<List<StagingMilestoneDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<StagingMilestoneRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<StagingMilestoneDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetStagingRowsAsync(id);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        #endregion

        #region AddStagingRowAsync

        [Fact]
        public async Task AddStagingRowAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            const int year = 2025;
            var dto = new StagingMilestoneDto { Project = "PP001", Number = "M1" };
            var request = new StagingMilestoneReq { Project = "PP001", Number = "M1" };
            var url = string.Format(PimsApiEndpoints.AddStagingMilestone, year);
            var apiResponse = new ApiResponse<StagingMilestoneRes> { Success = true, Data = new StagingMilestoneRes { Project = "PP001", Number = "M1" } };
            var mappedDto = ApiResponseDto<StagingMilestoneDto>.SuccessResponse(new StagingMilestoneDto { Project = "PP001", Number = "M1" });

            _mapper.Map<StagingMilestoneReq>(dto).Returns(request);
            _http.PostAsync<StagingMilestoneReq, StagingMilestoneRes>(url, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StagingMilestoneDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.AddStagingRowAsync(dto, year);

            // Assert
            Assert.True(result.Success);
            _mapper.Received(1).Map<StagingMilestoneReq>(dto);
            await _http.Received(1).PostAsync<StagingMilestoneReq, StagingMilestoneRes>(url, request);
            _mapper.Received(1).Map<ApiResponseDto<StagingMilestoneDto>>(apiResponse);
        }

        [Fact]
        public async Task AddStagingRowAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const int year = 2025;
            var dto = new StagingMilestoneDto { Project = "PP001", Number = "M1" };
            var request = new StagingMilestoneReq { Project = "PP001", Number = "M1" };
            var url = string.Format(PimsApiEndpoints.AddStagingMilestone, year);
            var errors = new List<ApiError> { new() { Message = "Validation error", Code = "VALIDATION_ERROR" } };
            var apiResponse = new ApiResponse<StagingMilestoneRes> { Success = false, Errors = errors };
            var mappedDto = new ApiResponseDto<StagingMilestoneDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Validation error", Code = "VALIDATION_ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<StagingMilestoneReq>(dto).Returns(request);
            _http.PostAsync<StagingMilestoneReq, StagingMilestoneRes>(url, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StagingMilestoneDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.AddStagingRowAsync(dto, year);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("VALIDATION_ERROR", result.Errors![0].Code);
        }

        [Fact]
        public async Task AddStagingRowAsync_WhenMapperThrows_PropagatesException()
        {
            // Arrange
            var dto = new StagingMilestoneDto { Project = "PP001", Number = "M1" };
            _mapper.Map<StagingMilestoneReq>(dto).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act & Assert
            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _client.AddStagingRowAsync(dto, 2025));
        }

        #endregion

        #region UpdateStagingRowAsync

        [Fact]
        public async Task UpdateStagingRowAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            const int id = 12;
            var dto = new StagingMilestoneDto { Id = id, Project = "PP001", Number = "M1" };
            var request = new StagingMilestoneReq { Project = "PP001", Number = "M1" };
            var url = string.Format(PimsApiEndpoints.UpdateStagingMilestone, id);
            var apiResponse = new ApiResponse<StagingMilestoneRes> { Success = true, Data = new StagingMilestoneRes { Project = "PP001", Number = "M1" } };
            var mappedDto = ApiResponseDto<StagingMilestoneDto>.SuccessResponse(new StagingMilestoneDto { Project = "PP001", Number = "M1" });

            _mapper.Map<StagingMilestoneReq>(dto).Returns(request);
            _http.PutAsync<StagingMilestoneReq, StagingMilestoneRes>(url, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StagingMilestoneDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateStagingRowAsync(id, dto);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).PutAsync<StagingMilestoneReq, StagingMilestoneRes>(url, request);
        }

        [Fact]
        public async Task UpdateStagingRowAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const int id = 12;
            var dto = new StagingMilestoneDto { Id = id, Project = "PP001" };
            var request = new StagingMilestoneReq { Project = "PP001" };
            var url = string.Format(PimsApiEndpoints.UpdateStagingMilestone, id);
            var errors = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<StagingMilestoneRes> { Success = false, Errors = errors };
            var mappedDto = new ApiResponseDto<StagingMilestoneDto>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta    = new ApiMetaDto()
            };

            _mapper.Map<StagingMilestoneReq>(dto).Returns(request);
            _http.PutAsync<StagingMilestoneReq, StagingMilestoneRes>(url, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<StagingMilestoneDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateStagingRowAsync(id, dto);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        #endregion

        #region DeleteStagingRowAsync

        [Fact]
        public async Task DeleteStagingRowAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            const int id = 10;
            var url = string.Format(PimsApiEndpoints.DeleteStagingMilestone, id);
            var apiResponse = new ApiResponse<object> { Success = true, Data = new object() };
            var mappedDto   = ApiResponseDto<object>.SuccessResponse(new object());

            _http.DeleteAsync<object>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteStagingRowAsync(id);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<object>(Arg.Is<string>(u => u == url));
        }

        [Fact]
        public async Task DeleteStagingRowAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const int id = 10;
            var url = string.Format(PimsApiEndpoints.DeleteStagingMilestone, id);
            var errors = new List<ApiError> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<object> { Success = false, Errors = errors };
            var mappedDto = new ApiResponseDto<object>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<object>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteStagingRowAsync(id);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("NOT_FOUND", result.Errors![0].Code);
        }

        #endregion

        #region ClearStagingAsync

        [Fact]
        public async Task ClearStagingAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            const string project = "PP001";
            var url = string.Format(PimsApiEndpoints.ClearStagingMilestones, Uri.EscapeDataString(project));
            var apiResponse = new ApiResponse<object> { Success = true, Data = new object() };
            var mappedDto   = ApiResponseDto<object>.SuccessResponse(new object());

            _http.DeleteAsync<object>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.ClearStagingAsync(project);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).DeleteAsync<object>(Arg.Is<string>(u => u == url));
        }

        [Fact]
        public async Task ClearStagingAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            var url = string.Format(PimsApiEndpoints.ClearStagingMilestones, Uri.EscapeDataString(project));
            var errors = new List<ApiError> { new() { Message = "Server error", Code = "SERVER_ERROR" } };
            var apiResponse = new ApiResponse<object> { Success = false, Errors = errors };
            var mappedDto = new ApiResponseDto<object>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Server error", Code = "SERVER_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<object>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.ClearStagingAsync(project);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("SERVER_ERROR", result.Errors![0].Code);
        }

        #endregion

        #region ValidateStagingAsync

        [Fact]
        public async Task ValidateStagingAsync_WithTypeId_AppendsQueryParamsAndReturnsMappedDto()
        {
            // Arrange
            const string project = "PP001";
            const string typeId = "M";
            const bool isDeliverableMode = true;
            var expectedUrl = string.Format(PimsApiEndpoints.ValidateStagingMilestones, Uri.EscapeDataString(project)) +
                              $"?typeId={Uri.EscapeDataString(typeId)}&isDeliverableMode={isDeliverableMode}";

            var apiResponse = new ApiResponse<object> { Success = true, Data = new object() };
            var mappedDto   = ApiResponseDto<object>.SuccessResponse(new object());

            _http.PostAsync<object, object>(expectedUrl, Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.ValidateStagingAsync(project, typeId, isDeliverableMode);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).PostAsync<object, object>(Arg.Is<string>(u => u == expectedUrl), Arg.Any<object>());
        }

        [Fact]
        public async Task ValidateStagingAsync_WithoutTypeId_AppendsOnlyDeliverableMode()
        {
            // Arrange
            const string project = "PP001";
            const bool isDeliverableMode = false;
            var expectedUrl = string.Format(PimsApiEndpoints.ValidateStagingMilestones, Uri.EscapeDataString(project)) +
                              $"?isDeliverableMode={isDeliverableMode}";
            var apiResponse = new ApiResponse<object> { Success = true, Data = new object() };
            var mappedDto   = ApiResponseDto<object>.SuccessResponse(new object());

            _http.PostAsync<object, object>(expectedUrl, Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.ValidateStagingAsync(project, null, isDeliverableMode);

            // Assert
            await _http.Received(1).PostAsync<object, object>(Arg.Is<string>(u => u == expectedUrl), Arg.Any<object>());
        }

        [Fact]
        public async Task ValidateStagingAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            var expectedUrl = string.Format(PimsApiEndpoints.ValidateStagingMilestones, Uri.EscapeDataString(project)) +
                              "?isDeliverableMode=True";
            var errors = new List<ApiError> { new() { Message = "Validation failed", Code = "VALIDATION_ERROR" } };
            var apiResponse = new ApiResponse<object> { Success = false, Errors = errors };
            var mappedDto = new ApiResponseDto<object>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Validation failed", Code = "VALIDATION_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<object, object>(expectedUrl, Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.ValidateStagingAsync(project, null, true);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("VALIDATION_ERROR", result.Errors![0].Code);
        }

        #endregion

        #region ImportStagingAsync

        [Fact]
        public async Task ImportStagingAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            const string project = "PP001";
            var url = string.Format(PimsApiEndpoints.ImportStagingMilestones, Uri.EscapeDataString(project));
            var apiResponse = new ApiResponse<object> { Success = true, Data = new object() };
            var mappedDto   = ApiResponseDto<object>.SuccessResponse(new object());

            _http.PostAsync<object, object>(url, Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.ImportStagingAsync(project);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).PostAsync<object, object>(Arg.Is<string>(u => u == url), Arg.Any<object>());
        }

        [Fact]
        public async Task ImportStagingAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            var url = string.Format(PimsApiEndpoints.ImportStagingMilestones, Uri.EscapeDataString(project));
            var errors = new List<ApiError> { new() { Message = "Server error", Code = "SERVER_ERROR" } };
            var apiResponse = new ApiResponse<object> { Success = false, Errors = errors };
            var mappedDto = new ApiResponseDto<object>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Server error", Code = "SERVER_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<object, object>(url, Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.ImportStagingAsync(project);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("SERVER_ERROR", result.Errors![0].Code);
        }

        #endregion

        #region ImportWithOverwriteAsync

        [Fact]
        public async Task ImportWithOverwriteAsync_WithSuccessResponse_ReturnsMappedDto()
        {
            // Arrange
            const string project = "PP001";
            var url = string.Format(PimsApiEndpoints.ImportOverwriteStagingMilestones, Uri.EscapeDataString(project));
            var apiResponse = new ApiResponse<object> { Success = true, Data = new object() };
            var mappedDto   = ApiResponseDto<object>.SuccessResponse(new object());

            _http.PostAsync<object, object>(url, Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.ImportWithOverwriteAsync(project);

            // Assert
            Assert.True(result.Success);
            await _http.Received(1).PostAsync<object, object>(Arg.Is<string>(u => u == url), Arg.Any<object>());
        }

        [Fact]
        public async Task ImportWithOverwriteAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const string project = "PP001";
            var url = string.Format(PimsApiEndpoints.ImportOverwriteStagingMilestones, Uri.EscapeDataString(project));
            var errors = new List<ApiError> { new() { Message = "Server error", Code = "SERVER_ERROR" } };
            var apiResponse = new ApiResponse<object> { Success = false, Errors = errors };
            var mappedDto = new ApiResponseDto<object>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Server error", Code = "SERVER_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<object, object>(url, Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<object>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.ImportWithOverwriteAsync(project);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("SERVER_ERROR", result.Errors![0].Code);
        }

        #endregion
    }
}
