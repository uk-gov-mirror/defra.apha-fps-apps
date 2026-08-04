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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsProjectApiClientTest
{
    public class FpsProjectApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsProjectApiClient _client;

        public FpsProjectApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsProjectApiClient(_http, _mapper);
        }

        #region GetProjectsByProgramAsync Tests

        [Fact]
        public async Task GetProjectsByProgramAsync_WithSuccessResponse_ReturnsMappedProjectList()
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
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _http.GetAsync<List<ProjectRes>>(Arg.Is<string>(url =>
                    url.Contains("api/v1/project/paged") && url.Contains("programNo=P001")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectsByProgramAsync(query, programNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProjectRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/project/paged") && url.Contains("programNo=P001")));
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<ProjectRes>>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<List<ProjectDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProjectsByProgramAsync(query, programNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_ConstructsUrlWithEscapedProgramNo()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = true, Data = new List<ProjectRes>() };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectsByProgramAsync(query, programNo);

            // Assert
            await _http.Received(1).GetAsync<List<ProjectRes>>(
                Arg.Is<string>(url => url.Contains($"programNo={Uri.EscapeDataString(programNo)}")));
        }

        #endregion

        #region GetAllProjectsAsync Tests

        [Fact]
        public async Task GetAllProjectsAsync_WithSuccessResponse_ReturnsMappedProjectList()
        {
            // Arrange
            var projectList = new List<ProjectRes>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
            };
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = true, Data = projectList };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>
                {
                    new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                    new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
                }
            );

            _http.GetAsync<List<ProjectRes>>("api/v1/project").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProjectRes>>("api/v1/project");
        }

        [Fact]
        public async Task GetAllProjectsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ProjectDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetAllPactProjectsAsync Tests

        [Fact]
        public async Task GetAllPactProjectsAsync_WithSuccessResponse_ReturnsMappedPactProjectList()
        {
            // Arrange
            var projectList = new List<ProjectRes>
            {
                new() { ParentProject = "PP001", ProjectTitle = "PACT Project One" },
                new() { ParentProject = "PP002", ProjectTitle = "PACT Project Two" }
            };
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = true, Data = projectList };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>
                {
                    new() { ParentProject = "PP001", ProjectTitle = "PACT Project One" },
                    new() { ParentProject = "PP002", ProjectTitle = "PACT Project Two" }
                }
            );

            _http.GetAsync<List<ProjectRes>>("api/v1/project/pactview/all").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllPactProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProjectRes>>("api/v1/project/pactview/all");
        }

        [Fact]
        public async Task GetAllPactProjectsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ProjectDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllPactProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetPagedProjectsAsync Tests

        [Fact]
        public async Task GetPagedProjectsAsync_WithSuccessResponse_ReturnsMappedPagedProjects()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectList = new List<ProjectRes> { new() { ParentProject = "PP001", ProjectTitle = "Alpha" } };
            var apiResponse = new ApiResponse<List<ProjectRes>>
            {
                Success = true,
                Data = projectList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto> { new() { ParentProject = "PP001" } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            );

            _http.GetAsync<List<ProjectRes>>(Arg.Is<string>(url => url.Contains("api/v1/project/paged"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<ProjectRes>>(Arg.Is<string>(url => url.Contains("api/v1/project/paged")));
        }

        [Fact]
        public async Task GetPagedProjectsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ProjectDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPagedProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetPagedProjectsByUserAsync Tests

        [Fact]
        public async Task GetPagedProjectsByUserAsync_WithSuccessResponse_ReturnsMappedProjects()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectList = new List<ProjectRes> { new() { ParentProject = "PP001", ProjectTitle = "Alpha" } };
            var apiResponse = new ApiResponse<List<ProjectRes>>
            {
                Success = true,
                Data = projectList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto> { new() { ParentProject = "PP001" } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            );

            _http.GetAsync<List<ProjectRes>>(Arg.Is<string>(url => url.Contains("api/v1/project/paged/by-user"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedProjectsByUserAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<ProjectRes>>(Arg.Is<string>(url => url.Contains("api/v1/project/paged/by-user")));
        }

        [Fact]
        public async Task GetPagedProjectsByUserAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ProjectDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPagedProjectsByUserAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetPagedPactProjectsAsync Tests

        [Fact]
        public async Task GetPagedPactProjectsAsync_WithSuccessResponse_ReturnsMappedPactProjects()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectList = new List<ProjectRes> { new() { ParentProject = "PP001", ProjectTitle = "PACT Project" } };
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = true, Data = projectList };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto> { new() { ParentProject = "PP001" } }
            );

            _http.GetAsync<List<ProjectRes>>(Arg.Is<string>(url => url.Contains("api/v1/project/pactview"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedPactProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<ProjectRes>>(Arg.Is<string>(url => url.Contains("api/v1/project/pactview")));
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ProjectDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPagedPactProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetPagedPactProjectsByProgramAsync Tests

        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_WithSuccessResponse_ReturnsMappedProjectList()
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
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _http.GetAsync<List<ProjectRes>>(Arg.Is<string>(url =>
                    url.Contains("api/v1/project/pactview/by-program") && url.Contains("programNo=P001")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedPactProjectsByProgramAsync(query, programNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProjectRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/project/pactview/by-program") && url.Contains("programNo=P001")));
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<List<ProjectRes>>
            {
                Success = false,
                Errors = errors
            };
            var mappedResponse = new ApiResponseDto<List<ProjectDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetPagedPactProjectsByProgramAsync(query, programNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_ConstructsUrlWithEscapedProgramNo()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = true, Data = new List<ProjectRes>() };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetPagedPactProjectsByProgramAsync(query, programNo);

            // Assert
            await _http.Received(1).GetAsync<List<ProjectRes>>(
                Arg.Is<string>(url => url.Contains($"programNo={Uri.EscapeDataString(programNo)}")));
        }

        #endregion

        #region GetProjectByIdAsync Tests

        [Fact]
        public async Task GetProjectByIdAsync_WithValidId_ReturnsMappedProject()
        {
            // Arrange
            var parentProject = "PP001";
            var projectRes = new ProjectRes { ParentProject = parentProject, ProjectTitle = "Alpha Project" };
            var apiResponse = new ApiResponse<ProjectRes> { Success = true, Data = projectRes };
            var expectedDto = ApiResponseDto<ProjectDto>.SuccessResponse(
                new ProjectDto { ParentProject = parentProject, ProjectTitle = "Alpha Project" }
            );

            _http.GetAsync<ProjectRes>(Arg.Is<string>(url => url.Contains(Uri.EscapeDataString(parentProject)))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectByIdAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(parentProject, result.Data?.ParentProject);
            await _http.Received(1).GetAsync<ProjectRes>(Arg.Is<string>(url => url.Contains($"api/v1/project/{Uri.EscapeDataString(parentProject)}")));
        }

        [Fact]
        public async Task GetProjectByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var parentProject = "NONEXISTENT";
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ProjectRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<ProjectRes>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProjectByIdAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateProjectAsync Tests

        [Fact]
        public async Task CreateProjectAsync_WithValidProject_ReturnsMappedCreatedProject()
        {
            // Arrange
            var projectDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "New Project", Program = "P001" };
            var projectReq = new ProjectReq { ParentProject = "PP001", ProjectTitle = "New Project" };
            var projectRes = new ProjectRes { ParentProject = "PP001", ProjectTitle = "New Project" };
            var apiResponse = new ApiResponse<ProjectRes> { Success = true, Data = projectRes };
            var expectedDto = ApiResponseDto<ProjectDto>.SuccessResponse(projectDto);

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PostAsync<ProjectReq, ProjectRes>("api/v1/project", projectReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.CreateProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("PP001", result.Data?.ParentProject);
            await _http.Received(1).PostAsync<ProjectReq, ProjectRes>("api/v1/project", projectReq);
        }

        [Fact]
        public async Task CreateProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var projectDto = new ProjectDto { ParentProject = "PP001" };
            var projectReq = new ProjectReq { ParentProject = "PP001" };
            var errors = new List<ApiError> { new() { Message = "Duplicate", Code = "DUPLICATE" } };
            var apiResponse = new ApiResponse<ProjectRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Duplicate", Code = "DUPLICATE" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PostAsync<ProjectReq, ProjectRes>(Arg.Any<string>(), Arg.Any<ProjectReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.CreateProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateProjectAsync Tests

        [Fact]
        public async Task UpdateProjectAsync_WithValidProject_ReturnsMappedUpdatedProject()
        {
            // Arrange
            var projectDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Updated Project" };
            var projectReq = new ProjectReq { ParentProject = "PP001", ProjectTitle = "Updated Project" };
            var projectRes = new ProjectRes { ParentProject = "PP001", ProjectTitle = "Updated Project" };
            var apiResponse = new ApiResponse<ProjectRes> { Success = true, Data = projectRes };
            var expectedDto = ApiResponseDto<ProjectDto>.SuccessResponse(projectDto);

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PutAsync<ProjectReq, ProjectRes>("api/v1/project", projectReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdateProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Updated Project", result.Data?.ProjectTitle);
            await _http.Received(1).PutAsync<ProjectReq, ProjectRes>("api/v1/project", projectReq);
        }

        [Fact]
        public async Task UpdateProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var projectDto = new ProjectDto { ParentProject = "NONEXISTENT" };
            var projectReq = new ProjectReq { ParentProject = "NONEXISTENT" };
            var errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var apiResponse = new ApiResponse<ProjectRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PutAsync<ProjectReq, ProjectRes>(Arg.Any<string>(), Arg.Any<ProjectReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdateProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdatePactProjectAsync Tests

        [Fact]
        public async Task UpdatePactProjectAsync_WithValidProject_ReturnsMappedUpdatedProject()
        {
            // Arrange
            var projectDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "PACT Updated" };
            var projectReq = new ProjectReq { ParentProject = "PP001", ProjectTitle = "PACT Updated" };
            var projectRes = new ProjectRes { ParentProject = "PP001", ProjectTitle = "PACT Updated" };
            var apiResponse = new ApiResponse<ProjectRes> { Success = true, Data = projectRes };
            var expectedDto = ApiResponseDto<ProjectDto>.SuccessResponse(projectDto);

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PatchAsync<ProjectReq, ProjectRes>("api/v1/project/external/pact", projectReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.UpdatePactProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("PP001", result.Data?.ParentProject);
            await _http.Received(1).PatchAsync<ProjectReq, ProjectRes>("api/v1/project/external/pact", projectReq);
        }

        [Fact]
        public async Task UpdatePactProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var projectDto = new ProjectDto { ParentProject = "PP001" };
            var projectReq = new ProjectReq { ParentProject = "PP001" };
            var errors = new List<ApiError> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var apiResponse = new ApiResponse<ProjectRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PatchAsync<ProjectReq, ProjectRes>(Arg.Any<string>(), Arg.Any<ProjectReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.UpdatePactProjectAsync(projectDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteProjectAsync Tests

        [Fact]
        public async Task DeleteProjectAsync_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var parentProject = "PP001";
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Is<string>(url => url.Contains(Uri.EscapeDataString(parentProject)))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.DeleteProjectAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool?>(Arg.Is<string>(url => url.Contains($"api/v1/project/{Uri.EscapeDataString(parentProject)}")));
        }

        [Fact]
        public async Task DeleteProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var parentProject = "NONEXISTENT";
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
            var result = await _client.DeleteProjectAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateProjectAsync (with parentProject) Tests

        [Fact]
        public async Task UpdateProjectAsync_WithParentProject_ReturnsSuccess()
        {
            var projectDto = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Updated" };
            var projectReq = new ProjectReq { ParentProject = "PP001", ProjectTitle = "Updated" };
            var projectRes = new ProjectRes { ParentProject = "PP001", ProjectTitle = "Updated" };
            var apiResponse = new ApiResponse<ProjectRes> { Success = true, Data = projectRes };
            var expectedDto = ApiResponseDto<ProjectDto>.SuccessResponse(projectDto);

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PutAsync<ProjectReq, ProjectRes>(Arg.Is<string>(url => url.Contains("PP001")), projectReq).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(expectedDto);

            var result = await _client.UpdateProjectAsync("PP001", projectDto);

            Assert.True(result.Success);
            Assert.Equal("PP001", result.Data?.ParentProject);
        }

        [Fact]
        public async Task UpdateProjectAsync_WithParentProject_WhenFails_ReturnsFailure()
        {
            var projectDto = new ProjectDto { ParentProject = "PP001" };
            var projectReq = new ProjectReq { ParentProject = "PP001" };
            var errors = new List<ApiError> { new() { Message = "Failed", Code = "ERR" } };
            var apiResponse = new ApiResponse<ProjectRes> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<ProjectDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Failed", Code = "ERR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<ProjectReq>(projectDto).Returns(projectReq);
            _http.PutAsync<ProjectReq, ProjectRes>(Arg.Any<string>(), Arg.Any<ProjectReq>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectDto>>(apiResponse).Returns(mappedResponse);

            var result = await _client.UpdateProjectAsync("PP001", projectDto);

            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteProjectAndChildrenAsync Tests

        [Fact]
        public async Task DeleteProjectAndChildrenAsync_WithValidId_ReturnsSuccess()
        {
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool?>(Arg.Is<string>(url => url.Contains("PP001") && url.Contains("delete-with-children"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            var result = await _client.DeleteProjectAndChildrenAsync("PP001");

            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task DeleteProjectAndChildrenAsync_WhenFails_ReturnsFailure()
        {
            var errors = new List<ApiError> { new() { Message = "Failed", Code = "ERR" } };
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Failed", Code = "ERR" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool?>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            var result = await _client.DeleteProjectAndChildrenAsync("PP001");

            Assert.False(result.Success);
        }

        #endregion

        #region ChangeProjectCodeAsync Tests

        [Fact]
        public async Task ChangeProjectCodeAsync_WithValidCodes_ReturnsSuccess()
        {
            var apiResponse = new ApiResponse<bool?> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.PostAsync<object, bool?>(Arg.Is<string>(url => url.Contains("change-code")), Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            var result = await _client.ChangeProjectCodeAsync("OLD1", "NEW1");

            Assert.True(result.Success);
            await _http.Received(1).PostAsync<object, bool?>(Arg.Is<string>(url => url.Contains("change-code")), Arg.Any<object>());
        }

        [Fact]
        public async Task ChangeProjectCodeAsync_WhenFails_ReturnsFailure()
        {
            var errors = new List<ApiError> { new() { Message = "Code exists", Code = "DUPLICATE" } };
            var apiResponse = new ApiResponse<bool?> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Code exists", Code = "DUPLICATE" } },
                Meta = new ApiMetaDto()
            };

            _http.PostAsync<object, bool?>(Arg.Any<string>(), Arg.Any<object>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            var result = await _client.ChangeProjectCodeAsync("OLD1", "NEW1");

            Assert.False(result.Success);
        }

        #endregion

        #region CheckProjectExistsAsync Tests

        [Fact]
        public async Task CheckProjectExistsAsync_WhenExists_ReturnsTrue()
        {
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var expectedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.GetAsync<bool>(Arg.Is<string>(url => url.Contains("check-exists/PP001"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(expectedDto);

            var result = await _client.CheckProjectExistsAsync("PP001");

            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task CheckProjectExistsAsync_WhenFails_ReturnsFailure()
        {
            var errors = new List<ApiError> { new() { Message = "Error", Code = "ERR" } };
            var apiResponse = new ApiResponse<bool> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<bool>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedResponse);

            var result = await _client.CheckProjectExistsAsync("PP001");

            Assert.False(result.Success);
        }

        #endregion

        #region GetManagersAsync Tests

        [Fact]
        public async Task GetManagersAsync_WithSuccess_ReturnsMappedManagers()
        {
            var managerList = new List<ManagerRes> { new() { Name = "Alice" } };
            var apiResponse = new ApiResponse<List<ManagerRes>> { Success = true, Data = managerList };
            var expectedDto = ApiResponseDto<List<ManagerDto>>.SuccessResponse(new List<ManagerDto> { new() { Name = "Alice" } });

            _http.GetAsync<List<ManagerRes>>(Arg.Is<string>(url => url.Contains("employee/managers"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ManagerDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetManagersAsync();

            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetManagersAsync_WhenFails_ReturnsFailure()
        {
            var apiResponse = new ApiResponse<List<ManagerRes>> { Success = false, Errors = new List<ApiError> { new() { Message = "Error" } } };
            var mappedResponse = new ApiResponseDto<List<ManagerDto>> { Success = false, Errors = new List<ApiErrorDto> { new() { Message = "Error" } }, Meta = new ApiMetaDto() };

            _http.GetAsync<List<ManagerRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ManagerDto>>>(apiResponse).Returns(mappedResponse);

            var result = await _client.GetManagersAsync();

            Assert.False(result.Success);
        }

        #endregion

        #region GetCostCentresAsync Tests

        [Fact]
        public async Task GetCostCentresAsync_WithSuccess_ReturnsMappedCostCentres()
        {
            var data = new List<CostCentreWorkgroupRes> { new() { CostCentre = 100 } };
            var apiResponse = new ApiResponse<List<CostCentreWorkgroupRes>> { Success = true, Data = data };
            var expectedDto = ApiResponseDto<List<CostCentreWorkgroupDto>>.SuccessResponse(new List<CostCentreWorkgroupDto> { new() { CostCentre = 100 } });

            _http.GetAsync<List<CostCentreWorkgroupRes>>(Arg.Is<string>(url => url.Contains("costcentre"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CostCentreWorkgroupDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetCostCentresAsync();

            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetCostCentresAsync_WhenFails_ReturnsFailure()
        {
            var apiResponse = new ApiResponse<List<CostCentreWorkgroupRes>> { Success = false, Errors = new List<ApiError> { new() { Message = "Error" } } };
            var mappedResponse = new ApiResponseDto<List<CostCentreWorkgroupDto>> { Success = false, Errors = new List<ApiErrorDto> { new() { Message = "Error" } }, Meta = new ApiMetaDto() };

            _http.GetAsync<List<CostCentreWorkgroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CostCentreWorkgroupDto>>>(apiResponse).Returns(mappedResponse);

            var result = await _client.GetCostCentresAsync();

            Assert.False(result.Success);
        }

        #endregion

        #region GetAccountCodesAsync Tests

        [Fact]
        public async Task GetAccountCodesAsync_WithSuccess_ReturnsMappedAccountCodes()
        {
            var data = new List<AccountCodeRes> { new() { Code = "AC1" } };
            var apiResponse = new ApiResponse<List<AccountCodeRes>> { Success = true, Data = data };
            var expectedDto = ApiResponseDto<List<AccountCodeDto>>.SuccessResponse(new List<AccountCodeDto> { new() { Code = "AC1" } });

            _http.GetAsync<List<AccountCodeRes>>(Arg.Is<string>(url => url.Contains("accountcode"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AccountCodeDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetAccountCodesAsync();

            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetAccountCodesAsync_WhenFails_ReturnsFailure()
        {
            var apiResponse = new ApiResponse<List<AccountCodeRes>> { Success = false, Errors = new List<ApiError> { new() { Message = "Error" } } };
            var mappedResponse = new ApiResponseDto<List<AccountCodeDto>> { Success = false, Errors = new List<ApiErrorDto> { new() { Message = "Error" } }, Meta = new ApiMetaDto() };

            _http.GetAsync<List<AccountCodeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<AccountCodeDto>>>(apiResponse).Returns(mappedResponse);

            var result = await _client.GetAccountCodesAsync();

            Assert.False(result.Success);
        }

        #endregion

        #region GetSubAccountsAsync Tests

        [Fact]
        public async Task GetSubAccountsAsync_WithSuccess_ReturnsMappedSubAccounts()
        {
            var data = new List<SubAccountRes> { new() { SubAccountCode = "SA1" } };
            var apiResponse = new ApiResponse<List<SubAccountRes>> { Success = true, Data = data };
            var expectedDto = ApiResponseDto<List<SubAccountDto>>.SuccessResponse(new List<SubAccountDto> { new() { SubAccountCode = "SA1" } });

            _http.GetAsync<List<SubAccountRes>>(Arg.Is<string>(url => url.Contains("subaccount"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<SubAccountDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetSubAccountsAsync();

            Assert.True(result.Success);
            Assert.Single(result.Data!);
        }

        [Fact]
        public async Task GetSubAccountsAsync_WhenFails_ReturnsFailure()
        {
            var apiResponse = new ApiResponse<List<SubAccountRes>> { Success = false, Errors = new List<ApiError> { new() { Message = "Error" } } };
            var mappedResponse = new ApiResponseDto<List<SubAccountDto>> { Success = false, Errors = new List<ApiErrorDto> { new() { Message = "Error" } }, Meta = new ApiMetaDto() };

            _http.GetAsync<List<SubAccountRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<SubAccountDto>>>(apiResponse).Returns(mappedResponse);

            var result = await _client.GetSubAccountsAsync();

            Assert.False(result.Success);
        }

        #endregion

        #region GetPagedProjectSpecificQueryAsync Tests

        [Fact]
        public async Task GetPagedProjectSpecificQueryAsync_WithSuccessResponse_ReturnsMappedData()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var list = new List<ProjectSpecificQueryRes> { new() { ParentProject = "PP001", Account = "ACC1" } };
            var apiResponse = new ApiResponse<List<ProjectSpecificQueryRes>>
            {
                Success = true,
                Data = list,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };
            var expectedDto = ApiResponseDto<List<ProjectSpecificQueryDto>>.SuccessResponse(
                new List<ProjectSpecificQueryDto> { new() { ParentProject = "PP001", Account = "ACC1" } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });

            _http.GetAsync<List<ProjectSpecificQueryRes>>(Arg.Is<string>(url => url.Contains("api/v1/project/specific-query/paged"))).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectSpecificQueryDto>>>(apiResponse).Returns(expectedDto);

            var result = await _client.GetPagedProjectSpecificQueryAsync(query);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<ProjectSpecificQueryRes>>(Arg.Is<string>(url => url.Contains("api/v1/project/specific-query/paged")));
        }

        [Fact]
        public async Task GetPagedProjectSpecificQueryAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectSpecificQueryRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<List<ProjectSpecificQueryDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectSpecificQueryRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectSpecificQueryDto>>>(apiResponse).Returns(mappedResponse);

            var result = await _client.GetPagedProjectSpecificQueryAsync(query);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetProjectExceptionalCostsPagedAsync Tests

        [Fact]
        public async Task GetProjectExceptionalCostsPagedAsync_WithSuccessResponse_ReturnsMappedDtos()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<ProjectExceptionalCostViewRes>
            {
                new() { Directorate = "DIR1", Programme = "P001", Project = "PP001", AccountCat = "ACC1", ItemCost = 100m },
                new() { Directorate = "DIR2", Programme = "P002", Project = "PP002", AccountCat = "ACC2", ItemCost = 200m }
            };
            var apiResponse = new ApiResponse<IEnumerable<ProjectExceptionalCostViewRes>>
            {
                Success = true,
                Data = data,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<ProjectExceptionalCostViewDto>>.SuccessResponse(
                new List<ProjectExceptionalCostViewDto>
                {
                    new() { Directorate = "DIR1", Programme = "P001", Project = "PP001", AccountCat = "ACC1", ItemCost = 100m },
                    new() { Directorate = "DIR2", Programme = "P002", Project = "PP002", AccountCat = "ACC2", ItemCost = 200m }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _http.GetAsync<IEnumerable<ProjectExceptionalCostViewRes>>(Arg.Is<string>(url =>
                    url.Contains("api/v1/project/exceptionalcosts/paged")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectExceptionalCostViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectExceptionalCostsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<IEnumerable<ProjectExceptionalCostViewRes>>(
                Arg.Is<string>(url => url.Contains("api/v1/project/exceptionalcosts/paged")));
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectExceptionalCostViewDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetProjectExceptionalCostsPagedAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<IEnumerable<ProjectExceptionalCostViewRes>>
            {
                Success = true,
                Data = new List<ProjectExceptionalCostViewRes>()
            };
            var expectedDto = ApiResponseDto<List<ProjectExceptionalCostViewDto>>.SuccessResponse(
                new List<ProjectExceptionalCostViewDto>());

            _http.GetAsync<IEnumerable<ProjectExceptionalCostViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectExceptionalCostViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectExceptionalCostsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProjectExceptionalCostsPagedAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<IEnumerable<ProjectExceptionalCostViewRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Not Found", Code = "NOT_FOUND" } }
            };
            var mappedResponse = new ApiResponseDto<List<ProjectExceptionalCostViewDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<IEnumerable<ProjectExceptionalCostViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectExceptionalCostViewDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProjectExceptionalCostsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetProjectExceptionalCostsPagedAsync_PassesQueryParametersToUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 25, SortBy = "Directorate", Descending = true };
            var apiResponse = new ApiResponse<IEnumerable<ProjectExceptionalCostViewRes>>
            {
                Success = true,
                Data = new List<ProjectExceptionalCostViewRes>()
            };
            var expectedDto = ApiResponseDto<List<ProjectExceptionalCostViewDto>>.SuccessResponse(
                new List<ProjectExceptionalCostViewDto>());

            _http.GetAsync<IEnumerable<ProjectExceptionalCostViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectExceptionalCostViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectExceptionalCostsPagedAsync(query);

            // Assert
            await _http.Received(1).GetAsync<IEnumerable<ProjectExceptionalCostViewRes>>(
                Arg.Is<string>(url => url.Contains("Page=2") && url.Contains("PageSize=25")));
        }

        #endregion
    }
}
