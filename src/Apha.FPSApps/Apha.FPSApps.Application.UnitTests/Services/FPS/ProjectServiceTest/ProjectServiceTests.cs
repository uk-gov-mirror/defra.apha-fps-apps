using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.ProjectServiceTest
{
    public class ProjectServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsProjectApiClient _fpsProjectApiClient;
        private readonly IFpsLookupApiClient _fpsLookupApiClient;
        private readonly IFpsProjectGroupApiClient _fpsProjectGroupApiClient;
        private readonly ProjectService _projectService;

        public ProjectServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _fpsProjectApiClient = Substitute.For<IFpsProjectApiClient>();
            _fpsLookupApiClient = Substitute.For<IFpsLookupApiClient>();
            _fpsProjectGroupApiClient = Substitute.For<IFpsProjectGroupApiClient>();
            _fpsClient.FpsProject.Returns(_fpsProjectApiClient);
            _fpsClient.FpsLookup.Returns(_fpsLookupApiClient);
            _fpsClient.FpsProjectGroup.Returns(_fpsProjectGroupApiClient);
            _projectService = new ProjectService(_fpsClient);
        }

        #region GetProjectsByProgramAsync Tests

        [Fact]
        public async Task GetProjectsByProgramAsync_WithSuccessResponse_ReturnsProjectList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project",  Program = "P001" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                projects,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _fpsProjectApiClient.GetProjectsByProgramAsync(query, programNo).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetProjectsByProgramAsync(query, programNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsProjectApiClient.Received(1).GetProjectsByProgramAsync(query, programNo);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            );

            _fpsProjectApiClient.GetProjectsByProgramAsync(query, programNo).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetProjectsByProgramAsync(query, programNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsProjectApiClient.GetProjectsByProgramAsync(query, programNo).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetProjectsByProgramAsync(query, programNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetProjectsByProgramAsync_CallsFpsProjectApiClient_WithCorrectArguments()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5, SortBy = "parentproject" };
            var programNo = "P002";
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());

            _fpsProjectApiClient.GetProjectsByProgramAsync(query, programNo).Returns(expectedResponse);

            // Act
            await _projectService.GetProjectsByProgramAsync(query, programNo);

            // Assert
            await _fpsProjectApiClient.Received(1).GetProjectsByProgramAsync(query, programNo);
        }

        #endregion

        #region GetAllProjectsAsync Tests

        [Fact]
        public async Task GetAllProjectsAsync_WithSuccessResponse_ReturnsProjectList()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project",  Program = "P001" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects);
            _fpsProjectApiClient.GetAllProjectsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsProjectApiClient.Received(1).GetAllProjectsAsync();
        }

        [Fact]
        public async Task GetAllProjectsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());
            _fpsProjectApiClient.GetAllProjectsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _fpsProjectApiClient.Received(1).GetAllProjectsAsync();
        }

        [Fact]
        public async Task GetAllProjectsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.GetAllProjectsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            await _fpsProjectApiClient.Received(1).GetAllProjectsAsync();
        }

        #endregion

        #region GetAllPactProjectsAsync Tests

        [Fact]
        public async Task GetAllPactProjectsAsync_WithSuccessResponse_ReturnsProjectList()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "PACT Project One" },
                new() { ParentProject = "PP002", ProjectTitle = "PACT Project Two" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects);
            _fpsProjectApiClient.GetAllPactProjectsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllPactProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsProjectApiClient.Received(1).GetAllPactProjectsAsync();
        }

        [Fact]
        public async Task GetAllPactProjectsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());
            _fpsProjectApiClient.GetAllPactProjectsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllPactProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllPactProjectsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.GetAllPactProjectsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllPactProjectsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetPagedProjectsAsync Tests

        [Fact]
        public async Task GetPagedProjectsAsync_WithValidQuery_ReturnsPaginatedProjects()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Search = "Test" };
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                projects,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );
            _fpsProjectApiClient.GetPagedProjectsAsync(query).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetPagedProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsProjectApiClient.Received(1).GetPagedProjectsAsync(query);
        }

        [Fact]
        public async Task GetPagedProjectsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.GetPagedProjectsAsync(query).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetPagedProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetPagedProjectSpecificQueryAsync Tests

        [Fact]
        public async Task GetPagedProjectSpecificQueryAsync_WithValidQuery_ReturnsData()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<ProjectSpecificQueryDto> { new() { ParentProject = "PP001", Account = "ACC1" } };
            var expectedResponse = ApiResponseDto<List<ProjectSpecificQueryDto>>.SuccessResponse(
                data, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });
            _fpsProjectApiClient.GetPagedProjectSpecificQueryAsync(query).Returns(expectedResponse);

            var result = await _projectService.GetPagedProjectSpecificQueryAsync(query);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsProjectApiClient.Received(1).GetPagedProjectSpecificQueryAsync(query);
        }

        [Fact]
        public async Task GetPagedProjectSpecificQueryAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectSpecificQueryDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.GetPagedProjectSpecificQueryAsync(query).Returns(expectedResponse);

            var result = await _projectService.GetPagedProjectSpecificQueryAsync(query);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetPagedProjectsByUserAsync Tests

        [Fact]
        public async Task GetPagedProjectsByUserAsync_WithValidQuery_ReturnsPaginatedProjects()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Search = "Test" };
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                projects,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );
            _fpsProjectApiClient.GetPagedProjectsByUserAsync(query).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetPagedProjectsByUserAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsProjectApiClient.Received(1).GetPagedProjectsByUserAsync(query);
        }

        [Fact]
        public async Task GetPagedProjectsByUserAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.GetPagedProjectsByUserAsync(query).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetPagedProjectsByUserAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetPagedPactProjectsAsync Tests

        [Fact]
        public async Task GetPagedPactProjectsAsync_WithValidQuery_ReturnsPaginatedPactProjects()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "PACT Project" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                projects,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            );
            _fpsProjectApiClient.GetPagedPactProjectsAsync(query).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetPagedPactProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsProjectApiClient.Received(1).GetPagedPactProjectsAsync(query);
        }

        [Fact]
        public async Task GetPagedPactProjectsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.GetPagedPactProjectsAsync(query).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetPagedPactProjectsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetPagedPactProjectsByProgramAsync Tests

        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_WithSuccessResponse_ReturnsProjectList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", Program = "P001" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project",  Program = "P001" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                projects,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _fpsProjectApiClient.GetPagedPactProjectsByProgramAsync(query, programNo).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetPagedPactProjectsByProgramAsync(query, programNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsProjectApiClient.Received(1).GetPagedPactProjectsByProgramAsync(query, programNo);
        }

        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            );

            _fpsProjectApiClient.GetPagedPactProjectsByProgramAsync(query, programNo).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetPagedPactProjectsByProgramAsync(query, programNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsProjectApiClient.GetPagedPactProjectsByProgramAsync(query, programNo).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetPagedPactProjectsByProgramAsync(query, programNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetPagedPactProjectsByProgramAsync_CallsFpsProjectApiClient_WithCorrectArguments()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 2, PageSize = 5, SortBy = "parentproject" };
            var programNo = "P002";
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());

            _fpsProjectApiClient.GetPagedPactProjectsByProgramAsync(query, programNo).Returns(expectedResponse);

            // Act
            await _projectService.GetPagedPactProjectsByProgramAsync(query, programNo);

            // Assert
            await _fpsProjectApiClient.Received(1).GetPagedPactProjectsByProgramAsync(query, programNo);
        }

        #endregion

        #region GetProjectByIdAsync Tests

        [Fact]
        public async Task GetProjectByIdAsync_WithValidId_ReturnsProject()
        {
            // Arrange
            var parentProject = "PP001";
            var project = new ProjectDto { ParentProject = parentProject, ProjectTitle = "Test Project", Program = "P001" };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(project);
            _fpsProjectApiClient.GetProjectByIdAsync(parentProject).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetProjectByIdAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(parentProject, result.Data?.ParentProject);
            await _fpsProjectApiClient.Received(1).GetProjectByIdAsync(parentProject);
        }

        [Fact]
        public async Task GetProjectByIdAsync_WithNonExistentId_ReturnsFailureResponse()
        {
            // Arrange
            var parentProject = "NONEXISTENT";
            var errors = new List<ApiErrorDto> { new() { Message = "Project not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.GetProjectByIdAsync(parentProject).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetProjectByIdAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateProjectAsync Tests

        [Fact]
        public async Task CreateProjectAsync_WithValidProject_ReturnsSuccessResponse()
        {
            // Arrange
            var newProject = new ProjectDto { ParentProject = "PP001", ProjectTitle = "New Project", Program = "P001" };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(newProject);
            _fpsProjectApiClient.CreateProjectAsync(newProject).Returns(expectedResponse);

            // Act
            var result = await _projectService.CreateProjectAsync(newProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(newProject.ParentProject, result.Data?.ParentProject);
            await _fpsProjectApiClient.Received(1).CreateProjectAsync(newProject);
        }

        [Fact]
        public async Task CreateProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var newProject = new ProjectDto { ParentProject = "PP001" };
            var errors = new List<ApiErrorDto> { new() { Message = "Duplicate project", Code = "DUPLICATE" } };
            var expectedResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.CreateProjectAsync(newProject).Returns(expectedResponse);

            // Act
            var result = await _projectService.CreateProjectAsync(newProject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateProjectAsync Tests

        [Fact]
        public async Task UpdateProjectAsync_WithValidProject_ReturnsSuccessResponse()
        {
            // Arrange
            var updatedProject = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Updated Project", Program = "P002" };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(updatedProject);
            _fpsProjectApiClient.UpdateProjectAsync(updatedProject).Returns(expectedResponse);

            // Act
            var result = await _projectService.UpdateProjectAsync(updatedProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Updated Project", result.Data?.ProjectTitle);
            await _fpsProjectApiClient.Received(1).UpdateProjectAsync(updatedProject);
        }

        [Fact]
        public async Task UpdateProjectAsync_WithNonExistentProject_ReturnsFailureResponse()
        {
            // Arrange
            var project = new ProjectDto { ParentProject = "NONEXISTENT" };
            var errors = new List<ApiErrorDto> { new() { Message = "Project not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.UpdateProjectAsync(project).Returns(expectedResponse);

            // Act
            var result = await _projectService.UpdateProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdatePactProjectAsync Tests

        [Fact]
        public async Task UpdatePactProjectAsync_WithValidProject_ReturnsSuccessResponse()
        {
            // Arrange
            var project = new ProjectDto { ParentProject = "PP001", ProjectTitle = "PACT Updated" };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(project);
            _fpsProjectApiClient.UpdatePactProjectAsync(project).Returns(expectedResponse);

            // Act
            var result = await _projectService.UpdatePactProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(project.ParentProject, result.Data?.ParentProject);
            await _fpsProjectApiClient.Received(1).UpdatePactProjectAsync(project);
        }

        [Fact]
        public async Task UpdatePactProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var project = new ProjectDto { ParentProject = "PP001" };
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var expectedResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.UpdatePactProjectAsync(project).Returns(expectedResponse);

            // Act
            var result = await _projectService.UpdatePactProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteProjectAsync Tests

        [Fact]
        public async Task DeleteProjectAsync_WithValidId_ReturnsSuccessResponse()
        {
            // Arrange
            var parentProject = "PP001";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsProjectApiClient.DeleteProjectAsync(parentProject).Returns(expectedResponse);

            // Act
            var result = await _projectService.DeleteProjectAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsProjectApiClient.Received(1).DeleteProjectAsync(parentProject);
        }

        [Fact]
        public async Task DeleteProjectAsync_WithNonExistentId_ReturnsFailureResponse()
        {
            // Arrange
            var parentProject = "NONEXISTENT";
            var errors = new List<ApiErrorDto> { new() { Message = "Project not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.DeleteProjectAsync(parentProject).Returns(expectedResponse);

            // Act
            var result = await _projectService.DeleteProjectAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetAllStatusesAsync Tests

        [Fact]
        public async Task GetAllStatusesAsync_WithSuccessResponse_ReturnsStatusList()
        {
            // Arrange
            var statuses = new List<StatusDto> { new() { Status = "Active" }, new() { Status = "Inactive" } };
            var expectedResponse = ApiResponseDto<List<StatusDto>>.SuccessResponse(statuses);
            _fpsLookupApiClient.GetAllStatusesAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllStatusesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsLookupApiClient.Received(1).GetAllStatusesAsync();
        }

        [Fact]
        public async Task GetAllStatusesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<StatusDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsLookupApiClient.GetAllStatusesAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllStatusesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetAllDiseasesAsync Tests

        [Fact]
        public async Task GetAllDiseasesAsync_WithSuccessResponse_ReturnsDiseaseList()
        {
            // Arrange
            var diseases = new List<DiseaseDto> { new() { Disease = "Foot and Mouth" }, new() { Disease = "Avian Flu" } };
            var expectedResponse = ApiResponseDto<List<DiseaseDto>>.SuccessResponse(diseases);
            _fpsLookupApiClient.GetAllDiseasesAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsLookupApiClient.Received(1).GetAllDiseasesAsync();
        }

        [Fact]
        public async Task GetAllDiseasesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<DiseaseDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsLookupApiClient.GetAllDiseasesAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllDiseasesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetAllCustomersAsync Tests

        [Fact]
        public async Task GetAllCustomersAsync_WithSuccessResponse_ReturnsCustomerList()
        {
            // Arrange
            var customers = new List<CustomerDto> { new() { Customer = "DEFRA" }, new() { Customer = "APHA" } };
            var expectedResponse = ApiResponseDto<List<CustomerDto>>.SuccessResponse(customers);
            _fpsLookupApiClient.GetAllCustomersAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsLookupApiClient.Received(1).GetAllCustomersAsync();
        }

        [Fact]
        public async Task GetAllCustomersAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<CustomerDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsLookupApiClient.GetAllCustomersAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllCustomersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetAllProjectGroupsAsync Tests

        [Fact]
        public async Task GetAllProjectGroupsAsync_WithSuccessResponse_ReturnsProjectGroupList()
        {
            // Arrange
            var projectGroups = new List<ProjectGroupDto>
            {
                new() { ProjectGroupName = "Surveillance", ProjectGroup = "SRV" },
                new() { ProjectGroupName = "Research", ProjectGroup = "RSH" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(projectGroups);
            _fpsLookupApiClient.GetAllProjectGroupsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllProjectGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsLookupApiClient.Received(1).GetAllProjectGroupsAsync();
        }

        [Fact]
        public async Task GetAllProjectGroupsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectGroupDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsLookupApiClient.GetAllProjectGroupsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllProjectGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetAllContractsAsync Tests

        [Fact]
        public async Task GetAllContractsAsync_WithSuccessResponse_ReturnsContractList()
        {
            // Arrange
            var contracts = new List<ContractDto> { new() { ContractNo = "C001" }, new() { ContractNo = "C002" } };
            var expectedResponse = ApiResponseDto<List<ContractDto>>.SuccessResponse(contracts);
            _fpsLookupApiClient.GetAllContractsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllContractsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsLookupApiClient.Received(1).GetAllContractsAsync();
        }

        [Fact]
        public async Task GetAllContractsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ContractDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsLookupApiClient.GetAllContractsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllContractsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetAllPactContractsAsync Tests

        [Fact]
        public async Task GetAllPactContractsAsync_WithSuccessResponse_ReturnsContractList()
        {
            // Arrange
            var contracts = new List<ContractDto> { new() { ContractNo = "C001" }, new() { ContractNo = "C002" } };
            var expectedResponse = ApiResponseDto<List<ContractDto>>.SuccessResponse(contracts);
            _fpsLookupApiClient.GetAllPactContractsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllPactContractsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsLookupApiClient.Received(1).GetAllPactContractsAsync();
        }

        [Fact]
        public async Task GetAllPactContractsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ContractDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsLookupApiClient.GetAllPactContractsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllPactContractsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetProgrammeNewProjectByIdAsync Tests

        [Fact]
        public async Task GetProgrammeNewProjectByIdAsync_WithValidId_ReturnsProject()
        {
            var project = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Test" };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(project);
            _fpsProjectApiClient.GetProjectByIdAsync("PP001").Returns(expectedResponse);

            var result = await _projectService.GetProgrammeNewProjectByIdAsync("PP001");

            Assert.True(result.Success);
            Assert.Equal("PP001", result.Data?.ParentProject);
            await _fpsProjectApiClient.Received(1).GetProjectByIdAsync("PP001");
        }

        [Fact]
        public async Task GetProgrammeNewProjectByIdAsync_WhenApiFails_ReturnsFailure()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.GetProjectByIdAsync("NOPE").Returns(expectedResponse);

            var result = await _projectService.GetProgrammeNewProjectByIdAsync("NOPE");

            Assert.False(result.Success);
        }

        #endregion

        #region UpdateProjectAsync (with parentProject) Tests

        [Fact]
        public async Task UpdateProjectAsync_WithParentProject_ReturnsSuccess()
        {
            var project = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Updated" };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(project);
            _fpsProjectApiClient.UpdateProjectAsync("PP001", project).Returns(expectedResponse);

            var result = await _projectService.UpdateProjectAsync("PP001", project);

            Assert.True(result.Success);
            Assert.Equal("Updated", result.Data?.ProjectTitle);
            await _fpsProjectApiClient.Received(1).UpdateProjectAsync("PP001", project);
        }

        [Fact]
        public async Task UpdateProjectAsync_WithParentProject_WhenApiFails_ReturnsFailure()
        {
            var project = new ProjectDto { ParentProject = "PP001" };
            var errors = new List<ApiErrorDto> { new() { Message = "Failed", Code = "ERR" } };
            var expectedResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.UpdateProjectAsync("PP001", project).Returns(expectedResponse);

            var result = await _projectService.UpdateProjectAsync("PP001", project);

            Assert.False(result.Success);
        }

        #endregion

        #region DeleteProjectAndChildrenAsync Tests

        [Fact]
        public async Task DeleteProjectAndChildrenAsync_WithValidId_ReturnsSuccess()
        {
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsProjectApiClient.DeleteProjectAndChildrenAsync("PP001").Returns(expectedResponse);

            var result = await _projectService.DeleteProjectAndChildrenAsync("PP001");

            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsProjectApiClient.Received(1).DeleteProjectAndChildrenAsync("PP001");
        }

        [Fact]
        public async Task DeleteProjectAndChildrenAsync_WhenApiFails_ReturnsFailure()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Has children", Code = "HAS_CHILDREN" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.DeleteProjectAndChildrenAsync("PP001").Returns(expectedResponse);

            var result = await _projectService.DeleteProjectAndChildrenAsync("PP001");

            Assert.False(result.Success);
        }

        #endregion

        #region ChangeProjectCodeAsync Tests

        [Fact]
        public async Task ChangeProjectCodeAsync_WithValidCodes_ReturnsSuccess()
        {
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsProjectApiClient.ChangeProjectCodeAsync("OLD1", "NEW1").Returns(expectedResponse);

            var result = await _projectService.ChangeProjectCodeAsync("OLD1", "NEW1");

            Assert.True(result.Success);
            await _fpsProjectApiClient.Received(1).ChangeProjectCodeAsync("OLD1", "NEW1");
        }

        [Fact]
        public async Task ChangeProjectCodeAsync_WhenApiFails_ReturnsFailure()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Code exists", Code = "DUPLICATE" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.ChangeProjectCodeAsync("OLD1", "NEW1").Returns(expectedResponse);

            var result = await _projectService.ChangeProjectCodeAsync("OLD1", "NEW1");

            Assert.False(result.Success);
        }

        #endregion

        #region CheckProjectExistsAsync Tests

        [Fact]
        public async Task CheckProjectExistsAsync_WhenExists_ReturnsTrue()
        {
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsProjectApiClient.CheckProjectExistsAsync("PP001").Returns(expectedResponse);

            var result = await _projectService.CheckProjectExistsAsync("PP001");

            Assert.True(result.Success);
            Assert.True(result.Data);
            await _fpsProjectApiClient.Received(1).CheckProjectExistsAsync("PP001");
        }

        [Fact]
        public async Task CheckProjectExistsAsync_WhenApiFails_ReturnsFailure()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.CheckProjectExistsAsync("PP001").Returns(expectedResponse);

            var result = await _projectService.CheckProjectExistsAsync("PP001");

            Assert.False(result.Success);
        }

        #endregion

        #region GetManagersAsync Tests

        [Fact]
        public async Task GetManagersAsync_WithSuccess_ReturnsManagerList()
        {
            var managers = new List<ManagerDto> { new() { Name = "Alice" } };
            var expectedResponse = ApiResponseDto<List<ManagerDto>>.SuccessResponse(managers);
            _fpsProjectApiClient.GetManagersAsync().Returns(expectedResponse);

            var result = await _projectService.GetManagersAsync();

            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsProjectApiClient.Received(1).GetManagersAsync();
        }

        [Fact]
        public async Task GetManagersAsync_WhenApiFails_ReturnsFailure()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            var expectedResponse = ApiResponseDto<List<ManagerDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.GetManagersAsync().Returns(expectedResponse);

            var result = await _projectService.GetManagersAsync();

            Assert.False(result.Success);
        }

        #endregion

        #region GetCostCentresAsync Tests

        [Fact]
        public async Task GetCostCentresAsync_WithSuccess_ReturnsCostCentreList()
        {
            var data = new List<CostCentreWorkgroupDto> { new() { CostCentre = 100, ProfitCentre = "PC01", WGs = "WG1" } };
            var expectedResponse = ApiResponseDto<List<CostCentreWorkgroupDto>>.SuccessResponse(data);
            _fpsProjectApiClient.GetCostCentresAsync().Returns(expectedResponse);

            var result = await _projectService.GetCostCentresAsync();

            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsProjectApiClient.Received(1).GetCostCentresAsync();
        }

        [Fact]
        public async Task GetCostCentresAsync_WhenApiFails_ReturnsFailure()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            var expectedResponse = ApiResponseDto<List<CostCentreWorkgroupDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.GetCostCentresAsync().Returns(expectedResponse);

            var result = await _projectService.GetCostCentresAsync();

            Assert.False(result.Success);
        }

        #endregion

        #region GetProjectGroupsAsync (via FpsProjectGroup) Tests

        [Fact]
        public async Task GetProjectGroupsAsync_WithSuccess_ReturnsProjectGroupList()
        {
            var data = new List<ProjectGroupDto> { new() { ProjectGroupName = "GRP1", ProjectGroup = "G1" } };
            var expectedResponse = ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(data);
            _fpsProjectGroupApiClient.GetAllProjectGroupsAsync().Returns(expectedResponse);

            var result = await _projectService.GetProjectGroupsAsync();

            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsProjectGroupApiClient.Received(1).GetAllProjectGroupsAsync();
        }

        [Fact]
        public async Task GetProjectGroupsAsync_WhenApiFails_ReturnsFailure()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            var expectedResponse = ApiResponseDto<List<ProjectGroupDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectGroupApiClient.GetAllProjectGroupsAsync().Returns(expectedResponse);

            var result = await _projectService.GetProjectGroupsAsync();

            Assert.False(result.Success);
        }

        #endregion

        #region GetProjectGroupsByUserAsync (via FpsProjectGroup) Tests

        [Fact]
        public async Task GetProjectGroupsByUserAsync_WithSuccess_ReturnsProjectGroupList()
        {
            var data = new List<ProjectGroupDto> { new() { ProjectGroupName = "GRP1", ProjectGroup = "G1" } };
            var expectedResponse = ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(data);
            _fpsProjectGroupApiClient.GetProjectGroupsByUserAsync().Returns(expectedResponse);

            var result = await _projectService.GetProjectGroupsByUserAsync();

            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsProjectGroupApiClient.Received(1).GetProjectGroupsByUserAsync();
        }

        [Fact]
        public async Task GetProjectGroupsByUserAsync_WhenApiFails_ReturnsFailure()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            var expectedResponse = ApiResponseDto<List<ProjectGroupDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectGroupApiClient.GetProjectGroupsByUserAsync().Returns(expectedResponse);

            var result = await _projectService.GetProjectGroupsByUserAsync();

            Assert.False(result.Success);
        }

        #endregion

        #region GetProjectsByProjectGroupAsync (via FpsProjectGroup) Tests

        [Fact]
        public async Task GetProjectsByProjectGroupAsync_WithSuccessResponse_ReturnsProjectList()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "GRP1";
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", ProjectGroup = "GRP1" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project",  ProjectGroup = "GRP1" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                projects,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );
            _fpsProjectGroupApiClient.GetProjectsByProjectGroupAsync(query, projectGroup).Returns(expectedResponse);

            var result = await _projectService.GetProjectsByProjectGroupAsync(query, projectGroup);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsProjectGroupApiClient.Received(1).GetProjectsByProjectGroupAsync(query, projectGroup);
        }

        [Fact]
        public async Task GetProjectsByProjectGroupAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "GRP1";
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            );
            _fpsProjectGroupApiClient.GetProjectsByProjectGroupAsync(query, projectGroup).Returns(expectedResponse);

            var result = await _projectService.GetProjectsByProjectGroupAsync(query, projectGroup);

            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProjectsByProjectGroupAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "GRP1";
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectGroupApiClient.GetProjectsByProjectGroupAsync(query, projectGroup).Returns(expectedResponse);

            var result = await _projectService.GetProjectsByProjectGroupAsync(query, projectGroup);

            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task GetProjectsByProjectGroupAsync_CallsFpsProjectGroupApiClient_WithCorrectArguments()
        {
            var query = new QueryParameters<string> { Page = 2, PageSize = 5, SortBy = "parentproject" };
            var projectGroup = "GRP2";
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());
            _fpsProjectGroupApiClient.GetProjectsByProjectGroupAsync(query, projectGroup).Returns(expectedResponse);

            await _projectService.GetProjectsByProjectGroupAsync(query, projectGroup);

            await _fpsProjectGroupApiClient.Received(1).GetProjectsByProjectGroupAsync(query, projectGroup);
        }

        #endregion

        #region GetAccountCodesAsync Tests

        [Fact]
        public async Task GetAccountCodesAsync_WithSuccess_ReturnsAccountCodeList()
        {
            var data = new List<AccountCodeDto> { new() { Code = "AC1", Description = "Account Code 1" } };
            var expectedResponse = ApiResponseDto<List<AccountCodeDto>>.SuccessResponse(data);
            _fpsProjectApiClient.GetAccountCodesAsync().Returns(expectedResponse);

            var result = await _projectService.GetAccountCodesAsync();

            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsProjectApiClient.Received(1).GetAccountCodesAsync();
        }

        [Fact]
        public async Task GetAccountCodesAsync_WhenApiFails_ReturnsFailure()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            var expectedResponse = ApiResponseDto<List<AccountCodeDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.GetAccountCodesAsync().Returns(expectedResponse);

            var result = await _projectService.GetAccountCodesAsync();

            Assert.False(result.Success);
        }

        #endregion

        #region GetSubAccountsAsync Tests

        [Fact]
        public async Task GetSubAccountsAsync_WithSuccess_ReturnsSubAccountList()
        {
            var data = new List<SubAccountDto> { new() { SubAccountCode = "SA1", SubAccount = "Sub Account 1" } };
            var expectedResponse = ApiResponseDto<List<SubAccountDto>>.SuccessResponse(data);
            _fpsProjectApiClient.GetSubAccountsAsync().Returns(expectedResponse);

            var result = await _projectService.GetSubAccountsAsync();

            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsProjectApiClient.Received(1).GetSubAccountsAsync();
        }

        [Fact]
        public async Task GetSubAccountsAsync_WhenApiFails_ReturnsFailure()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            var expectedResponse = ApiResponseDto<List<SubAccountDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.GetSubAccountsAsync().Returns(expectedResponse);

            var result = await _projectService.GetSubAccountsAsync();

            Assert.False(result.Success);
        }

        #endregion

        #region UpdateFpsPortfolioAsync Tests

        [Fact]
        public async Task UpdateFpsPortfolioAsync_WithValidProject_ReturnsSuccessResponse()
        {
            // Arrange
            var project = new ProjectDto
            {
                ParentProject = "PP001",
                ProjectTitle  = "FPS Portfolio Updated",
                Program       = "P002",
                Manager       = "Manager A",
                Disease       = "FMD",
                ProjectStatus = "Active",
                TransferIncome = 500m,
                CustIncome     = 600m,
                Profit         = 150m,
                Contract       = "C001",
                Customer       = "DEFRA"
            };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(project);
            _fpsProjectApiClient.UpdateFpsPortfolioAsync(project).Returns(expectedResponse);

            // Act
            var result = await _projectService.UpdateFpsPortfolioAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("PP001", result.Data?.ParentProject);
            Assert.Equal("FPS Portfolio Updated", result.Data?.ProjectTitle);
            await _fpsProjectApiClient.Received(1).UpdateFpsPortfolioAsync(project);
        }

        [Fact]
        public async Task UpdateFpsPortfolioAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var project = new ProjectDto { ParentProject = "PP001" };
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var expectedResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.UpdateFpsPortfolioAsync(project).Returns(expectedResponse);

            // Act
            var result = await _projectService.UpdateFpsPortfolioAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            await _fpsProjectApiClient.Received(1).UpdateFpsPortfolioAsync(project);
        }

        [Fact]
        public async Task UpdateFpsPortfolioAsync_WhenProjectNotFound_ReturnsNotFoundFailure()
        {
            // Arrange
            var project = new ProjectDto { ParentProject = "PP_NONEXISTENT" };
            var errors = new List<ApiErrorDto> { new() { Message = "Project record not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsProjectApiClient.UpdateFpsPortfolioAsync(project).Returns(expectedResponse);

            // Act
            var result = await _projectService.UpdateFpsPortfolioAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "NOT_FOUND");
            await _fpsProjectApiClient.Received(1).UpdateFpsPortfolioAsync(project);
        }

        [Fact]
        public async Task UpdateFpsPortfolioAsync_DelegatesExactlyToFpsProjectApiClient()
        {
            // Arrange — verifies the service delegates to the correct client, no extra calls
            var project = new ProjectDto { ParentProject = "PP001", ProjectTitle = "Test" };
            var expectedResponse = ApiResponseDto<ProjectDto>.SuccessResponse(project);
            _fpsProjectApiClient.UpdateFpsPortfolioAsync(project).Returns(expectedResponse);

            // Act
            await _projectService.UpdateFpsPortfolioAsync(project);

            // Assert — called exactly once on the FpsProject client, no other clients touched
            await _fpsProjectApiClient.Received(1).UpdateFpsPortfolioAsync(project);
            await _fpsLookupApiClient.DidNotReceive().GetAllStatusesAsync();
        }

        #endregion

        #region GetAllProjectsForAllUsersAsync Tests

        [Fact]
        public async Task GetAllProjectsForAllUsersAsync_WithSuccessResponse_ReturnsProjectList()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects);

            _fpsProjectApiClient.GetAllProjectsForAllUsersAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllProjectsForAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsProjectApiClient.Received(1).GetAllProjectsForAllUsersAsync();
        }

        [Fact]
        public async Task GetAllProjectsForAllUsersAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());

            _fpsProjectApiClient.GetAllProjectsForAllUsersAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllProjectsForAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllProjectsForAllUsersAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsProjectApiClient.GetAllProjectsForAllUsersAsync().Returns(expectedResponse);

            // Act
            var result = await _projectService.GetAllProjectsForAllUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetProjectExceptionalCostsPagedAsync Tests

        [Fact]
        public async Task GetProjectExceptionalCostsPagedAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var data = new List<ProjectExceptionalCostViewDto>
            {
                new() { Directorate = "DIR1", Programme = "P001", Project = "PP001", AccountCat = "ACC1", ItemCost = 100m },
                new() { Directorate = "DIR2", Programme = "P002", Project = "PP002", AccountCat = "ACC2", ItemCost = 200m }
            };
            var expectedResponse = ApiResponseDto<List<ProjectExceptionalCostViewDto>>.SuccessResponse(
                data, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _fpsProjectApiClient.GetProjectExceptionalCostsPagedAsync(query).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetProjectExceptionalCostsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsProjectApiClient.Received(1).GetProjectExceptionalCostsPagedAsync(query);
        }

        [Fact]
        public async Task GetProjectExceptionalCostsPagedAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProjectExceptionalCostViewDto>>.SuccessResponse(
                new List<ProjectExceptionalCostViewDto>());

            _fpsProjectApiClient.GetProjectExceptionalCostsPagedAsync(query).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetProjectExceptionalCostsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _fpsProjectApiClient.Received(1).GetProjectExceptionalCostsPagedAsync(query);
        }

        [Fact]
        public async Task GetProjectExceptionalCostsPagedAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectExceptionalCostViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _fpsProjectApiClient.GetProjectExceptionalCostsPagedAsync(query).Returns(expectedResponse);

            // Act
            var result = await _projectService.GetProjectExceptionalCostsPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion
    }
}
