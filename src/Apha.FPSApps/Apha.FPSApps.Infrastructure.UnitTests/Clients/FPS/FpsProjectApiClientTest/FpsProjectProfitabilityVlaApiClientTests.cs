using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsProjectApiClientTest
{
    public class FpsProjectProfitabilityVlaApiClientTests
    {
        private readonly IFpsHttpExecutor _httpExecutor;
        private readonly IMapper _mapper;
        private readonly FpsProjectApiClient _client;

        private const string BaseEndpoint = "api/v1/project/profitability-vla";

        public FpsProjectProfitabilityVlaApiClientTests()
        {
            _httpExecutor = Substitute.For<IFpsHttpExecutor>();
            _mapper       = Substitute.For<IMapper>();
            _client       = new FpsProjectApiClient(_httpExecutor, _mapper);
        }

        #region GetProjectProfitabilityVlaAsync

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithSuccessResponse_ReturnsMappedSuccessDto()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            // the frontend DTO uses 'JobCode' as the natural key — Phase 15 build fix.
            var responseItems = new List<ProjectProfitabilityVlaRes>
            {
                new() { Project = "PP001", StaffCosts = 1000m, Budget = 5000m, Profit = 4000m, TargetProfit = 3500m, OffTarget = 500m },
                new() { Project = "PP002", StaffCosts = 2000m, Budget = 6000m, Profit = 4000m, TargetProfit = 3000m, OffTarget = 1000m }
            };
            var apiResponse = new ApiResponse<List<ProjectProfitabilityVlaRes>>
            {
                Success = true,
                Data    = responseItems
            };
            var expectedDto = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(
                responseItems.Select(r => new ProjectProfitabilityVlaDto { JobCode = r.Project }).ToList(),
                new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 2 });

            _httpExecutor.GetAsync<List<ProjectProfitabilityVlaRes>>(Arg.Any<string>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfitabilityVlaDto>>>(apiResponse)
                .Returns(expectedDto);

            // Act
            var result = await _client.GetProjectProfitabilityVlaAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _httpExecutor.Received(1).GetAsync<List<ProjectProfitabilityVlaRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectProfitabilityVlaDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithFailureResponse_ReturnsFailureDto()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<ProjectProfitabilityVlaRes>>
            {
                Success = false,
                Errors  = errors
            };
            var mappedDto = new ApiResponseDto<List<ProjectProfitabilityVlaDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } }
            };

            _httpExecutor.GetAsync<List<ProjectProfitabilityVlaRes>>(Arg.Any<string>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfitabilityVlaDto>>>(apiResponse)
                .Returns(mappedDto);

            // Act
            var result = await _client.GetProjectProfitabilityVlaAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WhenHttpExecutorThrows_ReturnsFailureWithInternalErrorCode()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };

            _httpExecutor.GetAsync<List<ProjectProfitabilityVlaRes>>(Arg.Any<string>())
                .ThrowsAsync(new Exception("Network failure"));

            // Act
            var result = await _client.GetProjectProfitabilityVlaAsync(query);

            // Assert — catch block returns FailureResponse with INTERNAL_ERROR code
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Code == "INTERNAL_ERROR");
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WhenHttpExecutorThrows_MapperIsNotCalled()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };

            _httpExecutor.GetAsync<List<ProjectProfitabilityVlaRes>>(Arg.Any<string>())
                .ThrowsAsync(new HttpRequestException("Timeout"));

            // Act
            await _client.GetProjectProfitabilityVlaAsync(query);

            // Assert — mapper must not be called when exception path is taken
            _mapper.DidNotReceive().Map<ApiResponseDto<List<ProjectProfitabilityVlaDto>>>(Arg.Any<object>());
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithNoFilters_CallsBaseEndpointUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            var apiResponse = new ApiResponse<List<ProjectProfitabilityVlaRes>> { Success = true, Data = new() };
            var expectedDto = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(new List<ProjectProfitabilityVlaDto>());

            _httpExecutor.GetAsync<List<ProjectProfitabilityVlaRes>>(Arg.Any<string>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfitabilityVlaDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectProfitabilityVlaAsync(query);

            // Assert — URL must contain the base endpoint
            await _httpExecutor.Received(1).GetAsync<List<ProjectProfitabilityVlaRes>>(
                Arg.Is<string>(url => url.Contains(BaseEndpoint)));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithProjectStatusFilter_AppendsProjectStatusQueryParam()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            var apiResponse = new ApiResponse<List<ProjectProfitabilityVlaRes>> { Success = true, Data = new() };
            var expectedDto = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(new List<ProjectProfitabilityVlaDto>());

            _httpExecutor.GetAsync<List<ProjectProfitabilityVlaRes>>(Arg.Any<string>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfitabilityVlaDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectProfitabilityVlaAsync(query, projectStatus: "Approved");

            // Assert — URL must contain the projectStatus param
            await _httpExecutor.Received(1).GetAsync<List<ProjectProfitabilityVlaRes>>(
                Arg.Is<string>(url => url.Contains("projectStatus=Approved")));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithProgramNoFilter_AppendsProgramNoQueryParam()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            var apiResponse = new ApiResponse<List<ProjectProfitabilityVlaRes>> { Success = true, Data = new() };
            var expectedDto = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(new List<ProjectProfitabilityVlaDto>());

            _httpExecutor.GetAsync<List<ProjectProfitabilityVlaRes>>(Arg.Any<string>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfitabilityVlaDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectProfitabilityVlaAsync(query, programNo: "P001");

            // Assert
            await _httpExecutor.Received(1).GetAsync<List<ProjectProfitabilityVlaRes>>(
                Arg.Is<string>(url => url.Contains("programNo=P001")));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithManagerFilter_AppendsManagerQueryParam()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            var apiResponse = new ApiResponse<List<ProjectProfitabilityVlaRes>> { Success = true, Data = new() };
            var expectedDto = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(new List<ProjectProfitabilityVlaDto>());

            _httpExecutor.GetAsync<List<ProjectProfitabilityVlaRes>>(Arg.Any<string>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfitabilityVlaDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectProfitabilityVlaAsync(query, manager: "JohnSmith");

            // Assert
            await _httpExecutor.Received(1).GetAsync<List<ProjectProfitabilityVlaRes>>(
                Arg.Is<string>(url => url.Contains("manager=JohnSmith")));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithCustomerFilter_AppendsCustomerQueryParam()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            var apiResponse = new ApiResponse<List<ProjectProfitabilityVlaRes>> { Success = true, Data = new() };
            var expectedDto = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(new List<ProjectProfitabilityVlaDto>());

            _httpExecutor.GetAsync<List<ProjectProfitabilityVlaRes>>(Arg.Any<string>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfitabilityVlaDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectProfitabilityVlaAsync(query, customer: "ACMELtd");

            // Assert
            await _httpExecutor.Received(1).GetAsync<List<ProjectProfitabilityVlaRes>>(
                Arg.Is<string>(url => url.Contains("customer=ACMELtd")));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithAllFilters_AppendsAllQueryParams()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            var apiResponse = new ApiResponse<List<ProjectProfitabilityVlaRes>> { Success = true, Data = new() };
            var expectedDto = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(new List<ProjectProfitabilityVlaDto>());

            _httpExecutor.GetAsync<List<ProjectProfitabilityVlaRes>>(Arg.Any<string>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfitabilityVlaDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectProfitabilityVlaAsync(
                query,
                projectStatus: "Approved",
                programNo: "P001",
                manager: "JohnSmith",
                customer: "ACMELtd");

            // Assert — all 4 optional params appended
            await _httpExecutor.Received(1).GetAsync<List<ProjectProfitabilityVlaRes>>(
                Arg.Is<string>(url =>
                    url.Contains("projectStatus=Approved") &&
                    url.Contains("programNo=P001") &&
                    url.Contains("manager=JohnSmith") &&
                    url.Contains("customer=ACMELtd")));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithSuccessResponse_MapperCalledOnce()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            var apiResponse = new ApiResponse<List<ProjectProfitabilityVlaRes>> { Success = true, Data = new() };
            var expectedDto = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(new List<ProjectProfitabilityVlaDto>());

            _httpExecutor.GetAsync<List<ProjectProfitabilityVlaRes>>(Arg.Any<string>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfitabilityVlaDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectProfitabilityVlaAsync(query);

            // Assert — mapper invoked exactly once with the HTTP response object
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectProfitabilityVlaDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithNullFilters_DoesNotAppendNullParams()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 15 };
            var apiResponse = new ApiResponse<List<ProjectProfitabilityVlaRes>> { Success = true, Data = new() };
            var expectedDto = ApiResponseDto<List<ProjectProfitabilityVlaDto>>.SuccessResponse(new List<ProjectProfitabilityVlaDto>());

            _httpExecutor.GetAsync<List<ProjectProfitabilityVlaRes>>(Arg.Any<string>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfitabilityVlaDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectProfitabilityVlaAsync(
                query, projectStatus: null, programNo: null, manager: null, customer: null);

            // Assert — URL must NOT contain any filter param keys when all are null
            await _httpExecutor.Received(1).GetAsync<List<ProjectProfitabilityVlaRes>>(
                Arg.Is<string>(url =>
                    !url.Contains("Filter.ProjectStatus=") &&
                    !url.Contains("Filter.ProgramNo=") &&
                    !url.Contains("Filter.Manager=") &&
                    !url.Contains("Filter.Customer=")));
        }

        #endregion

        #region GetProjectsByProgramProjectProfitabilityVLAAsync

        [Fact]
        public async Task GetProjectsByProgramProjectProfitabilityVLAAsync_WithSuccessResponse_ReturnsMappedSuccessDto()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var projectList = new List<ProjectRes>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
            };
            var apiResponse = new ApiResponse<List<ProjectRes>>
            {
                Success = true,
                Data = projectList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>
                {
                    new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                    new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _httpExecutor.GetAsync<List<ProjectRes>>(
                    Arg.Is<string>(url => url.Contains("paged-vla") && url.Contains("programNo=P001")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectsByProgramProjectProfitabilityVLAAsync(query, programNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _httpExecutor.Received(1).GetAsync<List<ProjectRes>>(
                Arg.Is<string>(url => url.Contains("paged-vla") && url.Contains("programNo=P001")));
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetProjectsByProgramProjectProfitabilityVLAAsync_WithFailureResponse_ReturnsFailureDto()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var apiResponse = new ApiResponse<List<ProjectRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } }
            };
            var mappedDto = new ApiResponseDto<List<ProjectDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } }
            };

            _httpExecutor.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetProjectsByProgramProjectProfitabilityVLAAsync(query, programNo);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetProjectsByProgramProjectProfitabilityVLAAsync_UsesVlaEndpointUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = true, Data = new List<ProjectRes>() };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());

            _httpExecutor.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectsByProgramProjectProfitabilityVLAAsync(query, programNo);

            // Assert — URL must use the VLA endpoint, not the standard paged endpoint
            await _httpExecutor.Received(1).GetAsync<List<ProjectRes>>(
                Arg.Is<string>(url => url.Contains("paged-vla") && url.Contains("programNo=P001")));
        }

        [Fact]
        public async Task GetProjectsByProgramProjectProfitabilityVLAAsync_EncodesSpecialCharactersInProgramNo()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001 & Test";
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = true, Data = new List<ProjectRes>() };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());

            _httpExecutor.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectsByProgramProjectProfitabilityVLAAsync(query, programNo);

            // Assert — raw ampersand must not appear unencoded in the programNo path segment
            await _httpExecutor.Received(1).GetAsync<List<ProjectRes>>(
                Arg.Is<string>(url => !url.Contains("P001 & Test")));
        }

        [Fact]
        public async Task GetProjectsByProgramProjectProfitabilityVLAAsync_PassesPaginationQueryParameters()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 3, PageSize = 5, SortBy = "parentproject", Descending = true };
            var programNo = "P001";
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = true, Data = new List<ProjectRes>() };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());

            _httpExecutor.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectsByProgramProjectProfitabilityVLAAsync(query, programNo);

            // Assert
            await _httpExecutor.Received(1).GetAsync<List<ProjectRes>>(
                Arg.Is<string>(url => url.Contains("Page=3") || url.Contains("page=3")));
        }

        #endregion
    }
}
