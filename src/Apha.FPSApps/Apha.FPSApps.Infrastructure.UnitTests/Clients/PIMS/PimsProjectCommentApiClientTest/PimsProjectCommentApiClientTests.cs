using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients;
using AutoMapper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PIMS.PimsProjectCommentApiClientTest
{
    public class PimsProjectCommentApiClientTests
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PimsProjectCommentApiClient _client;

        public PimsProjectCommentApiClientTests()
        {
            _http = Substitute.For<IPimsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PimsProjectCommentApiClient(_http, _mapper);
        }

        #region GetCommentsByProjectAsync Tests

        [Fact]
        public async Task GetCommentsByProjectAsync_WithSuccessResponse_ReturnsMappedCommentList()
        {
            // Arrange
            var project = "PP001";
            int? year = 2024;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetCommentsByProject, query);
            url = QueryStringHelper.AddQueryString(url, new { project, year });
            var commentResList = new List<CommentRes>
            {
                new CommentRes { CommentNo = 1, Project = project, Year = year, Topic = "Topic1", Comment = "Comment1" },
                new CommentRes { CommentNo = 2, Project = project, Year = year, Topic = "Topic2", Comment = "Comment2" }
            };
            var apiResponse = new ApiResponse<List<CommentRes>> { Success = true, Data = commentResList };
            var mappedDto = ApiResponseDto<List<CommentDto>>.SuccessResponse(new List<CommentDto>
            {
                new CommentDto { CommentNo = 1, Project = project, Year = year, Topic = "Topic1", Comment = "Comment1" },
                new CommentDto { CommentNo = 2, Project = project, Year = year, Topic = "Topic2", Comment = "Comment2" }
            });

            _http.GetAsync<List<CommentRes>>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CommentDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetCommentsByProjectAsync(project, year, null, query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _http.Received(1).GetAsync<List<CommentRes>>(url);
            _mapper.Received(1).Map<ApiResponseDto<List<CommentDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var project = "INVALID";
            int? year = 2024;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetCommentsByProject, query);
            url = QueryStringHelper.AddQueryString(url, new { project, year });
            var errors = new List<ApiError>
            {
                new ApiError { Message = "Comments not found", Code = "NOT_FOUND" }
            };
            var apiResponse = new ApiResponse<List<CommentRes>> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<List<CommentDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Comments not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<CommentRes>>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CommentDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetCommentsByProjectAsync(project, year, null, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Comments not found", result.Errors[0].Message);
            await _http.Received(1).GetAsync<List<CommentRes>>(url);
            _mapper.Received(1).Map<ApiResponseDto<List<CommentDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var project = "PP001";
            int? year = 2024;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetCommentsByProject, query);
            url = QueryStringHelper.AddQueryString(url, new { project, year });
            _http.GetAsync<List<CommentRes>>(url).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetCommentsByProjectAsync(project, year, null, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve comments", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var project = "PP001";
            int? year = 2024;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetCommentsByProject, query);
            url = QueryStringHelper.AddQueryString(url, new { project, year });
            var apiResponse = new ApiResponse<List<CommentRes>>
            {
                Success = true,
                Data = new List<CommentRes> { new CommentRes { CommentNo = 1, Project = project } }
            };

            _http.GetAsync<List<CommentRes>>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CommentDto>>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetCommentsByProjectAsync(project, year, null, query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve comments", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var project = "PP123";
            int? year = 2024;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedUrl = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetCommentsByProject, query);
            expectedUrl = QueryStringHelper.AddQueryString(expectedUrl, new { project, year });
            var apiResponse = new ApiResponse<List<CommentRes>> { Success = true, Data = new List<CommentRes>() };
            var mappedDto = ApiResponseDto<List<CommentDto>>.SuccessResponse(new List<CommentDto>());

            _http.GetAsync<List<CommentRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CommentDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetCommentsByProjectAsync(project, year, null, query);

            // Assert
            await _http.Received(1).GetAsync<List<CommentRes>>(Arg.Is<string>(s => s == expectedUrl));
        }

        [Fact]
        public async Task GetCommentsByProjectAsync_WithTopicFilter_CallsWithTopicInUrl()
        {
            // Arrange
            var project = "PP123";
            int? year = 2024;
            var topic = "Financial";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedUrl = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetCommentsByProject, query);
            expectedUrl = QueryStringHelper.AddQueryString(expectedUrl, new { project, year, topic });
            var apiResponse = new ApiResponse<List<CommentRes>> { Success = true, Data = [] };
            var mappedDto = ApiResponseDto<List<CommentDto>>.SuccessResponse([]);

            _http.GetAsync<List<CommentRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CommentDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetCommentsByProjectAsync(project, year, topic, query);

            // Assert
            await _http.Received(1).GetAsync<List<CommentRes>>(Arg.Is<string>(s => s == expectedUrl));
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithSuccessResponse_ReturnsMappedComment()
        {
            // Arrange
            var CommentNo = 1;
            var url = string.Format(PimsApiEndpoints.GetCommentById, CommentNo);
            var commentRes = new CommentRes { CommentNo = CommentNo, Project = "PP001", Topic = "Topic1", Comment = "Comment1" };
            var apiResponse = new ApiResponse<CommentRes> { Success = true, Data = commentRes };
            var mappedDto = ApiResponseDto<CommentDto>.SuccessResponse(
                new CommentDto { CommentNo = CommentNo, Project = "PP001", Topic = "Topic1", Comment = "Comment1" }
            );

            _http.GetAsync<CommentRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CommentDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetByIdAsync(CommentNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(CommentNo, result.Data.CommentNo);
            await _http.Received(1).GetAsync<CommentRes>(url);
            _mapper.Received(1).Map<ApiResponseDto<CommentDto>>(apiResponse);
        }

        [Fact]
        public async Task GetByIdAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var CommentNo = 999;
            var url = string.Format(PimsApiEndpoints.GetCommentById, CommentNo);
            var errors = new List<ApiError>
            {
                new ApiError { Message = "Comment not found", Code = "NOT_FOUND" }
            };
            var apiResponse = new ApiResponse<CommentRes> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<CommentDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Comment not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<CommentRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CommentDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetByIdAsync(CommentNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Comment not found", result.Errors[0].Message);
            await _http.Received(1).GetAsync<CommentRes>(url);
            _mapper.Received(1).Map<ApiResponseDto<CommentDto>>(apiResponse);
        }

        [Fact]
        public async Task GetByIdAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var CommentNo = 1;
            var url = string.Format(PimsApiEndpoints.GetCommentById, CommentNo);
            _http.GetAsync<CommentRes>(url).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetByIdAsync(CommentNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve comment", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetByIdAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var CommentNo = 1;
            var url = string.Format(PimsApiEndpoints.GetCommentById, CommentNo);
            var apiResponse = new ApiResponse<CommentRes> { Success = true, Data = new CommentRes { CommentNo = CommentNo } };

            _http.GetAsync<CommentRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CommentDto>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.GetByIdAsync(CommentNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve comment", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetByIdAsync_EnsuresCorrectApiEndpoint_CallsWithCorrectUrl()
        {
            // Arrange
            var CommentNo = 42;
            var expectedUrl = string.Format(PimsApiEndpoints.GetCommentById, CommentNo);
            var apiResponse = new ApiResponse<CommentRes> { Success = true, Data = new CommentRes { CommentNo = CommentNo } };
            var mappedDto = ApiResponseDto<CommentDto>.SuccessResponse(new CommentDto { CommentNo = CommentNo });

            _http.GetAsync<CommentRes>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CommentDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetByIdAsync(CommentNo);

            // Assert
            await _http.Received(1).GetAsync<CommentRes>(Arg.Is<string>(s => s == expectedUrl));
        }

        #endregion

        #region CreateCommentAsync Tests

        [Fact]
        public async Task CreateCommentAsync_WithSuccessResponse_ReturnsMappedComment()
        {
            // Arrange
            var dto = new CommentDto { Project = "PP001", Year = 2024, Topic = "Topic1", Comment = "Comment1", MadeBy = "User1" };
            var request = new CommentReq { Project = "PP001", Year = 2024, Topic = "Topic1", Comment = "Comment1", MadeBy = "User1" };
            var commentRes = new CommentRes { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic1", Comment = "Comment1" };
            var apiResponse = new ApiResponse<CommentRes> { Success = true, Data = commentRes };
            var mappedDto = ApiResponseDto<CommentDto>.SuccessResponse(
                new CommentDto { CommentNo = 1, Project = "PP001", Year = 2024, Topic = "Topic1", Comment = "Comment1" }
            );

            _mapper.Map<CommentReq>(dto).Returns(request);
            _http.PostAsync<CommentReq, CommentRes>(PimsApiEndpoints.CreateComment, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CommentDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.CreateCommentAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1, result.Data.CommentNo);
            _mapper.Received(1).Map<CommentReq>(dto);
            await _http.Received(1).PostAsync<CommentReq, CommentRes>(PimsApiEndpoints.CreateComment, request);
            _mapper.Received(1).Map<ApiResponseDto<CommentDto>>(apiResponse);
        }

        [Fact]
        public async Task CreateCommentAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new CommentDto { Project = "PP001", Topic = "Topic1" };
            var request = new CommentReq { Project = "PP001", Topic = "Topic1" };
            var errors = new List<ApiError>
            {
                new ApiError { Message = "Validation error", Code = "VALIDATION_ERROR" }
            };
            var apiResponse = new ApiResponse<CommentRes> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<CommentDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Validation error", Code = "VALIDATION_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<CommentReq>(dto).Returns(request);
            _http.PostAsync<CommentReq, CommentRes>(PimsApiEndpoints.CreateComment, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CommentDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.CreateCommentAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Validation error", result.Errors[0].Message);
            await _http.Received(1).PostAsync<CommentReq, CommentRes>(PimsApiEndpoints.CreateComment, request);
        }

        [Fact]
        public async Task CreateCommentAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var dto = new CommentDto { Project = "PP001" };
            var request = new CommentReq { Project = "PP001" };

            _mapper.Map<CommentReq>(dto).Returns(request);
            _http.PostAsync<CommentReq, CommentRes>(PimsApiEndpoints.CreateComment, request)
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.CreateCommentAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to create comment", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task CreateCommentAsync_WhenMapperThrowsExceptionOnRequestMapping_ReturnsInternalError()
        {
            // Arrange
            var dto = new CommentDto { Project = "PP001" };
            _mapper.Map<CommentReq>(dto).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.CreateCommentAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to create comment", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task CreateCommentAsync_EnsuresCorrectApiEndpoint_CallsPostWithCorrectUrl()
        {
            // Arrange
            var dto = new CommentDto { Project = "PP001" };
            var request = new CommentReq { Project = "PP001" };
            var apiResponse = new ApiResponse<CommentRes> { Success = true, Data = new CommentRes { Project = "PP001" } };
            var mappedDto = ApiResponseDto<CommentDto>.SuccessResponse(new CommentDto { Project = "PP001" });

            _mapper.Map<CommentReq>(dto).Returns(request);
            _http.PostAsync<CommentReq, CommentRes>(PimsApiEndpoints.CreateComment, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CommentDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.CreateCommentAsync(dto);

            // Assert
            await _http.Received(1).PostAsync<CommentReq, CommentRes>(
                Arg.Is<string>(s => s == PimsApiEndpoints.CreateComment),
                Arg.Any<CommentReq>()
            );
        }

        #endregion

        #region UpdateCommentAsync Tests

        [Fact]
        public async Task UpdateCommentAsync_WithSuccessResponse_ReturnsMappedComment()
        {
            // Arrange
            var CommentNo = 1;
            var dto = new CommentDto { CommentNo = CommentNo, Project = "PP001", Topic = "UpdatedTopic", Comment = "UpdatedComment" };
            var request = new CommentReq { CommentNo = CommentNo, Project = "PP001", Topic = "UpdatedTopic", Comment = "UpdatedComment" };
            var commentRes = new CommentRes { CommentNo = CommentNo, Project = "PP001", Topic = "UpdatedTopic", Comment = "UpdatedComment" };
            var apiResponse = new ApiResponse<CommentRes> { Success = true, Data = commentRes };
            var mappedDto = ApiResponseDto<CommentDto>.SuccessResponse(
                new CommentDto { CommentNo = CommentNo, Project = "PP001", Topic = "UpdatedTopic", Comment = "UpdatedComment" }
            );

            _mapper.Map<CommentReq>(dto).Returns(request);
            _http.PutAsync<CommentReq, CommentRes>(string.Format(PimsApiEndpoints.UpdateComment, CommentNo), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CommentDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateCommentAsync(CommentNo, dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(CommentNo, result.Data.CommentNo);
            _mapper.Received(1).Map<CommentReq>(dto);
            await _http.Received(1).PutAsync<CommentReq, CommentRes>(string.Format(PimsApiEndpoints.UpdateComment, CommentNo), request);
            _mapper.Received(1).Map<ApiResponseDto<CommentDto>>(apiResponse);
        }

        [Fact]
        public async Task UpdateCommentAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var CommentNo = 1;
            var dto = new CommentDto { CommentNo = CommentNo, Project = "PP001" };
            var request = new CommentReq { CommentNo = CommentNo, Project = "PP001" };
            var errors = new List<ApiError>
            {
                new ApiError { Message = "Validation error", Code = "VALIDATION_ERROR" }
            };
            var apiResponse = new ApiResponse<CommentRes> { Success = false, Data = null, Errors = errors };
            var mappedDto = new ApiResponseDto<CommentDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Validation error", Code = "VALIDATION_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _mapper.Map<CommentReq>(dto).Returns(request);
            _http.PutAsync<CommentReq, CommentRes>(string.Format(PimsApiEndpoints.UpdateComment, CommentNo), request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CommentDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateCommentAsync(CommentNo, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Validation error", result.Errors[0].Message);
            await _http.Received(1).PutAsync<CommentReq, CommentRes>(string.Format(PimsApiEndpoints.UpdateComment, CommentNo), request);
        }

        [Fact]
        public async Task UpdateCommentAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var CommentNo = 1;
            var dto = new CommentDto { CommentNo = CommentNo, Project = "PP001" };
            var request = new CommentReq { CommentNo = CommentNo, Project = "PP001" };

            _mapper.Map<CommentReq>(dto).Returns(request);
            _http.PutAsync<CommentReq, CommentRes>(string.Format(PimsApiEndpoints.UpdateComment, CommentNo), request)
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.UpdateCommentAsync(CommentNo, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to update comment", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task UpdateCommentAsync_WhenMapperThrowsExceptionOnRequestMapping_ReturnsInternalError()
        {
            // Arrange
            var CommentNo = 1;
            var dto = new CommentDto { CommentNo = CommentNo, Project = "PP001" };
            _mapper.Map<CommentReq>(dto).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.UpdateCommentAsync(CommentNo, dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to update comment", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task UpdateCommentAsync_EnsuresCorrectApiEndpoint_CallsPutWithCorrectUrl()
        {
            // Arrange
            var CommentNo = 42;
            var expectedUrl = string.Format(PimsApiEndpoints.UpdateComment, CommentNo);
            var dto = new CommentDto { CommentNo = CommentNo };
            var request = new CommentReq { CommentNo = CommentNo };
            var apiResponse = new ApiResponse<CommentRes> { Success = true, Data = new CommentRes { CommentNo = CommentNo } };
            var mappedDto = ApiResponseDto<CommentDto>.SuccessResponse(new CommentDto { CommentNo = CommentNo });

            _mapper.Map<CommentReq>(dto).Returns(request);
            _http.PutAsync<CommentReq, CommentRes>(expectedUrl, request).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<CommentDto>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.UpdateCommentAsync(CommentNo, dto);

            // Assert
            await _http.Received(1).PutAsync<CommentReq, CommentRes>(
                Arg.Is<string>(s => s == expectedUrl),
                Arg.Any<CommentReq>()
            );
        }

        #endregion

        #region DeleteCommentAsync Tests

        [Fact]
        public async Task DeleteCommentAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var CommentNo = 1;
            var url = string.Format(PimsApiEndpoints.DeleteComment, CommentNo);
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var mappedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteCommentAsync(CommentNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _http.Received(1).DeleteAsync<bool>(url);
            _mapper.Received(1).Map<ApiResponseDto<bool>>(apiResponse);
        }

        [Fact]
        public async Task DeleteCommentAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var CommentNo = 999;
            var url = string.Format(PimsApiEndpoints.DeleteComment, CommentNo);
            var errors = new List<ApiError>
            {
                new ApiError { Message = "Comment not found", Code = "NOT_FOUND" }
            };
            var apiResponse = new ApiResponse<bool> { Success = false, Data = false, Errors = errors };
            var mappedDto = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Comment not found", Code = "NOT_FOUND" } },
                Meta = new ApiMetaDto()
            };

            _http.DeleteAsync<bool>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.DeleteCommentAsync(CommentNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);  // bool defaults to false, not null
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Comment not found", result.Errors[0].Message);
            await _http.Received(1).DeleteAsync<bool>(url);
            _mapper.Received(1).Map<ApiResponseDto<bool>>(apiResponse);
        }

        [Fact]
        public async Task DeleteCommentAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var CommentNo = 1;
            var url = string.Format(PimsApiEndpoints.DeleteComment, CommentNo);
            _http.DeleteAsync<bool>(url).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.DeleteCommentAsync(CommentNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);  // bool defaults to false, not null
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to delete comment", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task DeleteCommentAsync_WhenMapperThrowsException_ReturnsInternalError()
        {
            // Arrange
            var CommentNo = 1;
            var url = string.Format(PimsApiEndpoints.DeleteComment, CommentNo);
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };

            _http.DeleteAsync<bool>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Throws(new AutoMapperMappingException("Mapping failed"));

            // Act
            var result = await _client.DeleteCommentAsync(CommentNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.False(result.Data);  // bool defaults to false, not null
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to delete comment", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task DeleteCommentAsync_EnsuresCorrectApiEndpoint_CallsDeleteWithCorrectUrl()
        {
            // Arrange
            var CommentNo = 42;
            var expectedUrl = string.Format(PimsApiEndpoints.DeleteComment, CommentNo);
            var apiResponse = new ApiResponse<bool> { Success = true, Data = true };
            var mappedDto = ApiResponseDto<bool>.SuccessResponse(true);

            _http.DeleteAsync<bool>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<bool>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.DeleteCommentAsync(CommentNo);

            // Assert
            await _http.Received(1).DeleteAsync<bool>(Arg.Is<string>(s => s == expectedUrl));
        }

        #endregion

        #region GetCommentTopicsAsync Tests

        [Fact]
        public async Task GetCommentTopicsAsync_WithSuccessResponse_ReturnsMappedTopics()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<CommentTopicRes>>
            {
                Success = true,
                Data =
                [
                    new CommentTopicRes { Topic = "Finance" },
                    new CommentTopicRes { Topic = "Delivery" }
                ]
            };
            var mappedDto = ApiResponseDto<List<CommentTopicDto>>.SuccessResponse(
            [
                new CommentTopicDto { Topic = "Finance" },
                new CommentTopicDto { Topic = "Delivery" }
            ]);

            _http.GetAsync<List<CommentTopicRes>>(PimsApiEndpoints.GetCommentTopics).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CommentTopicDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetCommentTopicsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            await _http.Received(1).GetAsync<List<CommentTopicRes>>(PimsApiEndpoints.GetCommentTopics);
            _mapper.Received(1).Map<ApiResponseDto<List<CommentTopicDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetCommentTopicsAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            _http.GetAsync<List<CommentTopicRes>>(PimsApiEndpoints.GetCommentTopics).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetCommentTopicsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve comment topics", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        #endregion

        #region GetForecastSpendByProjectAsync Tests

        [Fact]
        public async Task GetForecastSpendByProjectAsync_WithSuccessResponse_ReturnsMappedForecastSpend()
        {
            // Arrange
            var project = "PP001";
            var url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetCommentForecastSpend, new { project });
            var apiResponse = new ApiResponse<ProjectCommentForecastSpendRes>
            {
                Success = true,
                Data = new ProjectCommentForecastSpendRes { ForecastSpend = 15000.75 }
            };
            var mappedDto = ApiResponseDto<ProjectCommentForecastSpendDto>.SuccessResponse(
                new ProjectCommentForecastSpendDto { ForecastSpend = 15000.75 });

            _http.GetAsync<ProjectCommentForecastSpendRes>(url).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectCommentForecastSpendDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetForecastSpendByProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(15000.75, result.Data.ForecastSpend);
            await _http.Received(1).GetAsync<ProjectCommentForecastSpendRes>(url);
            _mapper.Received(1).Map<ApiResponseDto<ProjectCommentForecastSpendDto>>(apiResponse);
        }

        [Fact]
        public async Task GetForecastSpendByProjectAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var project = "PP001";
            var url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetCommentForecastSpend, new { project });
            _http.GetAsync<ProjectCommentForecastSpendRes>(url).ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.GetForecastSpendByProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to retrieve forecast spend", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        #endregion

        #region UpdateForecastSpendByProjectAsync Tests

        [Fact]
        public async Task UpdateForecastSpendByProjectAsync_WithSuccessResponse_ReturnsMappedForecastSpend()
        {
            // Arrange
            var project = "PP001";
            double? forecastSpend = 22000.40;
            var url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetCommentForecastSpend, new { project });
            var apiResponse = new ApiResponse<ProjectCommentForecastSpendRes>
            {
                Success = true,
                Data = new ProjectCommentForecastSpendRes { ForecastSpend = forecastSpend }
            };
            var mappedDto = ApiResponseDto<ProjectCommentForecastSpendDto>.SuccessResponse(
                new ProjectCommentForecastSpendDto { ForecastSpend = forecastSpend });

            _http.PutAsync<ProjectCommentForecastSpendRes, ProjectCommentForecastSpendRes>(
                Arg.Is<string>(s => s == url),
                Arg.Any<ProjectCommentForecastSpendRes>())
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<ProjectCommentForecastSpendDto>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.UpdateForecastSpendByProjectAsync(project, forecastSpend);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(forecastSpend, result.Data.ForecastSpend);
            await _http.Received(1).PutAsync<ProjectCommentForecastSpendRes, ProjectCommentForecastSpendRes>(
                Arg.Is<string>(s => s == url),
                Arg.Is<ProjectCommentForecastSpendRes>(r => r.ForecastSpend == forecastSpend));
            _mapper.Received(1).Map<ApiResponseDto<ProjectCommentForecastSpendDto>>(apiResponse);
        }

        [Fact]
        public async Task UpdateForecastSpendByProjectAsync_WhenHttpExecutorThrowsException_ReturnsInternalError()
        {
            // Arrange
            var project = "PP001";
            double? forecastSpend = 22000.40;
            var url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetCommentForecastSpend, new { project });
            _http.PutAsync<ProjectCommentForecastSpendRes, ProjectCommentForecastSpendRes>(
                Arg.Is<string>(s => s == url),
                Arg.Any<ProjectCommentForecastSpendRes>())
                .ThrowsAsync(new Exception("Network error"));

            // Act
            var result = await _client.UpdateForecastSpendByProjectAsync(project, forecastSpend);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Failed to update forecast spend", result.Errors[0].Message);
            Assert.Equal("INTERNAL_ERROR", result.Errors[0].Code);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependencies_InitializesClient()
        {
            // Arrange & Act
            var client = new PimsProjectCommentApiClient(_http, _mapper);

            // Assert
            Assert.NotNull(client);
        }

        #endregion
    }
}
