using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PIMS;
using NSubstitute;

namespace Apha.FPSApps.Application.UnitTests.Services.PIMS.ProjectCommentServiceTest
{
    public class ProjectCommentServiceTests
    {
        private readonly IPimsApiClient _pimsApiClient;
        private readonly IPimsProjectCommentApiClient _pimsProjectCommentApiClient;
        private readonly ProjectCommentService _projectCommentService;

        public ProjectCommentServiceTests()
        {
            _pimsApiClient = Substitute.For<IPimsApiClient>();
            _pimsProjectCommentApiClient = Substitute.For<IPimsProjectCommentApiClient>();
            _pimsApiClient.PimsProjectComment.Returns(_pimsProjectCommentApiClient);
            _projectCommentService = new ProjectCommentService(_pimsApiClient);
        }

        #region GetCommentsByProjectAsync Tests

        [Fact]
        public async Task GetCommentsByProjectAsync_WithSuccessResponse_ReturnsCommentList()
        {
            // Arrange
            var project = "PP001";
            var year = 2024;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var comments = new List<CommentDto>
            {
                new CommentDto { CommentNo = 1, Project = project, Year = year, Topic = "Topic1", Comment = "Comment1" },
                new CommentDto { CommentNo = 2, Project = project, Year = year, Topic = "Topic2", Comment = "Comment2" }
            };
            var expectedResponse = ApiResponseDto<List<CommentDto>>.SuccessResponse(comments);

            _pimsProjectCommentApiClient.GetCommentsByProjectAsync(project, year, null, query).Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.GetCommentsByProjectAsync(project, year, null, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _pimsProjectCommentApiClient.Received(1).GetCommentsByProjectAsync(project, year, null, query);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var project = "PP001";
            var query = new QueryParameters<string>();
            var expectedResponse = ApiResponseDto<List<CommentDto>>.SuccessResponse(new List<CommentDto>());

            _pimsProjectCommentApiClient.GetCommentsByProjectAsync(project, null, null, query).Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.GetCommentsByProjectAsync(project, null, null, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var project = "INVALID";
            var query = new QueryParameters<string>();
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Comments not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<List<CommentDto>>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectCommentApiClient.GetCommentsByProjectAsync(project, null, null, query).Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.GetCommentsByProjectAsync(project, null, null, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_PassesCorrectParameters()
        {
            // Arrange
            var project = "PP123";
            var year = 2023;
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };
            var expectedResponse = ApiResponseDto<List<CommentDto>>.SuccessResponse(new List<CommentDto>());

            _pimsProjectCommentApiClient.GetCommentsByProjectAsync(project, year, null, query).Returns(expectedResponse);

            // Act
            await _projectCommentService.GetCommentsByProjectAsync(project, year, null, query);

            // Assert
            await _pimsProjectCommentApiClient.Received(1).GetCommentsByProjectAsync(
                Arg.Is<string>(p => p == project),
                Arg.Is<int?>(y => y == year),
                Arg.Is<string?>(t => t == null),
                Arg.Is<QueryParameters<string>>(q => q.Page == 2 && q.PageSize == 5)
            );
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithSuccessResponse_ReturnsComment()
        {
            // Arrange
            var CommentNo = 1;
            var comment = new CommentDto
            {
                CommentNo = CommentNo,
                Project = "PP001",
                Topic = "Test Topic",
                Comment = "Test Comment"
            };
            var expectedResponse = ApiResponseDto<CommentDto>.SuccessResponse(comment);

            _pimsProjectCommentApiClient.GetByIdAsync(CommentNo).Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.GetByIdAsync(CommentNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(CommentNo, result.Data.CommentNo);
            await _pimsProjectCommentApiClient.Received(1).GetByIdAsync(CommentNo);
        }

        [Fact]
        public async Task GetByIdAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var CommentNo = 999;
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Comment not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<CommentDto>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectCommentApiClient.GetByIdAsync(CommentNo).Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.GetByIdAsync(CommentNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetByIdAsync_PassesCorrectCommentNo()
        {
            // Arrange
            var CommentNo = 42;
            var expectedResponse = ApiResponseDto<CommentDto>.SuccessResponse(new CommentDto { CommentNo = CommentNo });

            _pimsProjectCommentApiClient.GetByIdAsync(CommentNo).Returns(expectedResponse);

            // Act
            await _projectCommentService.GetByIdAsync(CommentNo);

            // Assert
            await _pimsProjectCommentApiClient.Received(1).GetByIdAsync(CommentNo);
        }

        #endregion

        #region CreateCommentAsync Tests

        [Fact]
        public async Task CreateCommentAsync_WithValidData_ReturnsCreatedComment()
        {
            // Arrange
            var dto = new CommentDto
            {
                Project = "PP001",
                Year = 2024,
                Topic = "New Topic",
                Comment = "New Comment",
                MadeBy = "user1"
            };
            var expectedResponse = ApiResponseDto<CommentDto>.SuccessResponse(dto);

            _pimsProjectCommentApiClient.CreateCommentAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.CreateCommentAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("PP001", result.Data.Project);
            await _pimsProjectCommentApiClient.Received(1).CreateCommentAsync(dto);
        }

        [Fact]
        public async Task CreateCommentAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new CommentDto { Project = "INVALID" };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Create failed", Code = "CREATE_ERROR" }
            };
            var expectedResponse = ApiResponseDto<CommentDto>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectCommentApiClient.CreateCommentAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.CreateCommentAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task CreateCommentAsync_PassesCorrectDto()
        {
            // Arrange
            var dto = new CommentDto
            {
                Project = "PP123",
                Year = 2024,
                Topic = "Topic",
                MadeBy = "user2"
            };
            var expectedResponse = ApiResponseDto<CommentDto>.SuccessResponse(dto);

            _pimsProjectCommentApiClient.CreateCommentAsync(dto).Returns(expectedResponse);

            // Act
            await _projectCommentService.CreateCommentAsync(dto);

            // Assert
            await _pimsProjectCommentApiClient.Received(1).CreateCommentAsync(
                Arg.Is<CommentDto>(d => d.Project == "PP123" && d.Year == 2024 && d.MadeBy == "user2")
            );
        }

        #endregion

        #region UpdateCommentAsync Tests

        [Fact]
        public async Task UpdateCommentAsync_WithValidData_ReturnsUpdatedComment()
        {
            // Arrange
            var CommentNo = 1;
            var dto = new CommentDto
            {
                CommentNo = CommentNo,
                Project = "PP001",
                Topic = "Updated Topic",
                Comment = "Updated Comment"
            };
            var expectedResponse = ApiResponseDto<CommentDto>.SuccessResponse(dto);

            _pimsProjectCommentApiClient.UpdateCommentAsync(CommentNo, dto).Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.UpdateCommentAsync(CommentNo, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(CommentNo, result.Data.CommentNo);
            await _pimsProjectCommentApiClient.Received(1).UpdateCommentAsync(CommentNo, dto);
        }

        [Fact]
        public async Task UpdateCommentAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var CommentNo = 999;
            var dto = new CommentDto { CommentNo = CommentNo };
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Update failed", Code = "UPDATE_ERROR" }
            };
            var expectedResponse = ApiResponseDto<CommentDto>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectCommentApiClient.UpdateCommentAsync(CommentNo, dto).Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.UpdateCommentAsync(CommentNo, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task UpdateCommentAsync_PassesCorrectParameters()
        {
            // Arrange
            var CommentNo = 7;
            var dto = new CommentDto
            {
                CommentNo = CommentNo,
                Project = "PP123",
                Topic = "Topic",
                MadeBy = "editor1"
            };
            var expectedResponse = ApiResponseDto<CommentDto>.SuccessResponse(dto);

            _pimsProjectCommentApiClient.UpdateCommentAsync(CommentNo, dto).Returns(expectedResponse);

            // Act
            await _projectCommentService.UpdateCommentAsync(CommentNo, dto);

            // Assert
            await _pimsProjectCommentApiClient.Received(1).UpdateCommentAsync(
                Arg.Is<int>(c => c == CommentNo),
                Arg.Is<CommentDto>(d => d.CommentNo == CommentNo && d.Project == "PP123" && d.MadeBy == "editor1")
            );
        }

        #endregion

        #region DeleteCommentAsync Tests

        [Fact]
        public async Task DeleteCommentAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var CommentNo = 1;
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _pimsProjectCommentApiClient.DeleteCommentAsync(CommentNo).Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.DeleteCommentAsync(CommentNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pimsProjectCommentApiClient.Received(1).DeleteCommentAsync(CommentNo);
        }

        [Fact]
        public async Task DeleteCommentAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var CommentNo = 999;
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Delete failed", Code = "DELETE_ERROR" }
            };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectCommentApiClient.DeleteCommentAsync(CommentNo).Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.DeleteCommentAsync(CommentNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task DeleteCommentAsync_PassesCorrectCommentNo()
        {
            // Arrange
            var CommentNo = 15;
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _pimsProjectCommentApiClient.DeleteCommentAsync(CommentNo).Returns(expectedResponse);

            // Act
            await _projectCommentService.DeleteCommentAsync(CommentNo);

            // Assert
            await _pimsProjectCommentApiClient.Received(1).DeleteCommentAsync(CommentNo);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidClient_InitializesService()
        {
            // Arrange & Act
            var service = new ProjectCommentService(_pimsApiClient);

            // Assert
            Assert.NotNull(service);
        }

        #endregion

       
        #region GetCommentsByProjectAsync Topic Filter Tests

        [Fact]
        public async Task GetCommentsByProjectAsync_WithTopicFilter_PassesTopicToApiClient()
        {
            // Arrange
            var project = "PP001";
            var year = 2024;
            var topic = "Budget";
            var query = new QueryParameters<string>();
            var expectedResponse = ApiResponseDto<List<CommentDto>>.SuccessResponse(new List<CommentDto>());

            _pimsProjectCommentApiClient.GetCommentsByProjectAsync(project, year, topic, query).Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.GetCommentsByProjectAsync(project, year, topic, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _pimsProjectCommentApiClient.Received(1).GetCommentsByProjectAsync(
                Arg.Is<string>(p => p == project),
                Arg.Is<int?>(y => y == year),
                Arg.Is<string?>(t => t == topic),
                Arg.Is<QueryParameters<string>>(q => q == query));
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_WithNullTopicFilter_PassesNullToApiClient()
        {
            // Arrange
            var project = "PP001";
            var query = new QueryParameters<string>();
            var expectedResponse = ApiResponseDto<List<CommentDto>>.SuccessResponse(new List<CommentDto>());

            _pimsProjectCommentApiClient.GetCommentsByProjectAsync(project, null, null, query).Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.GetCommentsByProjectAsync(project, null, null, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _pimsProjectCommentApiClient.Received(1).GetCommentsByProjectAsync(
                project, null, null, query);
        }

        #endregion

       
        #region GetCommentTopicsAsync Tests

        [Fact]
        public async Task GetCommentTopicsAsync_ApiClientReturnsTopics_ReturnsSuccessWithTopicList()
        {
            // Arrange
            var topics = new List<CommentTopicDto>
            {
                new CommentTopicDto { Topic = "Budget" },
                new CommentTopicDto { Topic = "Risk" },
                new CommentTopicDto { Topic = "Schedule" }
            };
            var expectedResponse = ApiResponseDto<List<CommentTopicDto>>.SuccessResponse(topics);
            _pimsProjectCommentApiClient.GetCommentTopicsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.GetCommentTopicsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count);
            await _pimsProjectCommentApiClient.Received(1).GetCommentTopicsAsync();
        }

        [Fact]
        public async Task GetCommentTopicsAsync_ApiClientReturnsEmptyList_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<CommentTopicDto>>.SuccessResponse(new List<CommentTopicDto>());
            _pimsProjectCommentApiClient.GetCommentTopicsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.GetCommentTopicsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetCommentTopicsAsync_ApiClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Code = "TOPICS_ERROR", Message = "Failed to retrieve comment topics" }
            };
            var expectedResponse = ApiResponseDto<List<CommentTopicDto>>.FailureResponse(errors, new ApiMetaDto());
            _pimsProjectCommentApiClient.GetCommentTopicsAsync().Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.GetCommentTopicsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("TOPICS_ERROR", result.Errors[0].Code);
            await _pimsProjectCommentApiClient.Received(1).GetCommentTopicsAsync();
        }

        [Fact]
        public async Task GetCommentTopicsAsync_DelegatesToPimsProjectCommentApiClient()
        {
            // Arrange
            _pimsProjectCommentApiClient.GetCommentTopicsAsync()
                .Returns(ApiResponseDto<List<CommentTopicDto>>.SuccessResponse([]));

            // Act
            await _projectCommentService.GetCommentTopicsAsync();

            // Assert — verify thin delegation: exactly one call to the correct API client method
            await _pimsProjectCommentApiClient.Received(1).GetCommentTopicsAsync();
        }

        #endregion

        #region GetForecastSpendByProjectAsync Tests

        [Fact]
        public async Task GetForecastSpendByProjectAsync_WithSuccessResponse_ReturnsForecastSpend()
        {
            // Arrange
            var project = "PP001";
            var forecastSpend = new ProjectCommentForecastSpendDto { ForecastSpend = 12345.67 };
            var expectedResponse = ApiResponseDto<ProjectCommentForecastSpendDto>.SuccessResponse(forecastSpend);

            _pimsProjectCommentApiClient.GetForecastSpendByProjectAsync(project).Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.GetForecastSpendByProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(12345.67, result.Data.ForecastSpend);
            await _pimsProjectCommentApiClient.Received(1).GetForecastSpendByProjectAsync(project);
        }

        [Fact]
        public async Task GetForecastSpendByProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var project = "INVALID";
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Forecast spend not found", Code = "NOT_FOUND" }
            };
            var expectedResponse = ApiResponseDto<ProjectCommentForecastSpendDto>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectCommentApiClient.GetForecastSpendByProjectAsync(project).Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.GetForecastSpendByProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            await _pimsProjectCommentApiClient.Received(1).GetForecastSpendByProjectAsync(project);
        }

        #endregion

        #region UpdateForecastSpendByProjectAsync Tests

        [Fact]
        public async Task UpdateForecastSpendByProjectAsync_WithSuccessResponse_ReturnsUpdatedForecastSpend()
        {
            // Arrange
            var project = "PP001";
            double? forecastSpend = 20000.50;
            var updatedForecastSpend = new ProjectCommentForecastSpendDto { ForecastSpend = forecastSpend };
            var expectedResponse = ApiResponseDto<ProjectCommentForecastSpendDto>.SuccessResponse(updatedForecastSpend);

            _pimsProjectCommentApiClient.UpdateForecastSpendByProjectAsync(project, forecastSpend).Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.UpdateForecastSpendByProjectAsync(project, forecastSpend);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(forecastSpend, result.Data.ForecastSpend);
            await _pimsProjectCommentApiClient.Received(1).UpdateForecastSpendByProjectAsync(project, forecastSpend);
        }

        [Fact]
        public async Task UpdateForecastSpendByProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var project = "PP001";
            double? forecastSpend = 0;
            var errors = new List<ApiErrorDto>
            {
                new ApiErrorDto { Message = "Unable to update forecast spend", Code = "UPDATE_FAILED" }
            };
            var expectedResponse = ApiResponseDto<ProjectCommentForecastSpendDto>.FailureResponse(errors, new ApiMetaDto());

            _pimsProjectCommentApiClient.UpdateForecastSpendByProjectAsync(project, forecastSpend).Returns(expectedResponse);

            // Act
            var result = await _projectCommentService.UpdateForecastSpendByProjectAsync(project, forecastSpend);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            await _pimsProjectCommentApiClient.Received(1).UpdateForecastSpendByProjectAsync(project, forecastSpend);
        }

        #endregion
    }
}
