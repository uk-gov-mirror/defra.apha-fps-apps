using Apha.Common.Contracts;
using Apha.Common.Contracts.PIMS;
using Apha.PIMS.Api.Controllers;
using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PIMS.Api.UnitTests.Controllers.ProjectCommentControllerTest
{
    public class ProjectCommentControllerTests
    {
        private readonly ICommentService _service;
        private readonly IMapper _mapper;
        private readonly ProjectCommentController _controller;

        public ProjectCommentControllerTests()
        {
            _service = Substitute.For<ICommentService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new ProjectCommentController(_service, _mapper);
        }

        #region GetCommentsByProject

        [Fact]
        public async Task GetCommentsByProject_ReturnsOkResult_WithMappedPaginatedComments()
        {
            // Arrange
            var project = "PP001";
            var year = 2024;
            string? topic = null;
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var filter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginatedResult = new PaginatedResult<CommentDto>();
            var mappedResult = new PaginationRes<CommentRes>();

            _mapper.Map<QueryParameters<string>>(query).Returns(filter);
           
            _service.GetCommentsByProjectAsync(project, year, filter, topic).Returns(paginatedResult);
            _mapper.Map<PaginationRes<CommentRes>>(paginatedResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetCommentsByProject(project, year, topic, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            _mapper.Received(1).Map<QueryParameters<string>>(query);
            await _service.Received(1).GetCommentsByProjectAsync(project, year, filter, topic);
            _mapper.Received(1).Map<PaginationRes<CommentRes>>(paginatedResult);
        }

        [Fact]
        public async Task GetCommentsByProject_WithNullYear_ReturnsOkResult_WithMappedPaginatedComments()
        {
            // Arrange
            var project = "PP001";
            int? year = null;
            string? topic = null;
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var filter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginatedResult = new PaginatedResult<CommentDto>();
            var mappedResult = new PaginationRes<CommentRes>();

            _mapper.Map<QueryParameters<string>>(query).Returns(filter);
            _service.GetCommentsByProjectAsync(project, year, filter, topic).Returns(paginatedResult);
            _mapper.Map<PaginationRes<CommentRes>>(paginatedResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetCommentsByProject(project, year, topic, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            _mapper.Received(1).Map<QueryParameters<string>>(query);
            await _service.Received(1).GetCommentsByProjectAsync(project, year, filter, topic);
            _mapper.Received(1).Map<PaginationRes<CommentRes>>(paginatedResult);
        }

        [Fact]
        public async Task GetCommentsByProject_WithEmptyServiceResult_ReturnsOkWithEmptyPagination()
        {
            // Arrange
            var project = "PP001";
            var year = 2024;
            string? topic = null;
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var filter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var emptyResult = new PaginatedResult<CommentDto>();
            var emptyMapped = new PaginationRes<CommentRes>();

            _mapper.Map<QueryParameters<string>>(query).Returns(filter);
            _service.GetCommentsByProjectAsync(project, year, filter, topic).Returns(emptyResult);
            _mapper.Map<PaginationRes<CommentRes>>(emptyResult).Returns(emptyMapped);

            // Act
            var result = await _controller.GetCommentsByProject(project, year, topic, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(emptyMapped, okResult.Value);

            await _service.Received(1).GetCommentsByProjectAsync(project, year, filter, topic);
        }

        [Fact]
        public async Task GetCommentsByProject_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var project = "PP001";
            var year = 2024;
            string? topic = null;
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var filter = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _mapper.Map<QueryParameters<string>>(query).Returns(filter);
            _service.GetCommentsByProjectAsync(project, year, filter, topic).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetCommentsByProject(project, year, topic, query));

            _mapper.Received(1).Map<QueryParameters<string>>(query);
            await _service.Received(1).GetCommentsByProjectAsync(project, year, filter, topic);
        }

        [Fact]
        public async Task GetCommentsByProject_WithTopicFilter_ReturnsOkResult_WithFilteredComments()
        {
            // Arrange
            var project = "PP001";
            var year = 2024;
            var topic = "Budget";
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var filter = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginatedResult = new PaginatedResult<CommentDto>();
            var mappedResult = new PaginationRes<CommentRes>();

            _mapper.Map<QueryParameters<string>>(query).Returns(filter);
            _service.GetCommentsByProjectAsync(project, year, filter, topic).Returns(paginatedResult);
            _mapper.Map<PaginationRes<CommentRes>>(paginatedResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetCommentsByProject(project, year, topic, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);

            await _service.Received(1).GetCommentsByProjectAsync(project, year, filter, topic);
        }

        #endregion

        #region GetById

        [Fact]
        public async Task GetById_WithValidComment_ReturnsOkResult_WithMappedComment()
        {
            // Arrange
            var CommentNo = 1;
            var commentDto = new CommentDto
            {
                CommentNo = CommentNo,
                Project = "PP001",
                Year = 2024,
                Topic = "Budget",
                CommentText = "Review required",
                MadeBy = "user1",
                DateEntered = new DateTime(2024, 1, 15)
            };
            var commentRes = new CommentRes
            {
                CommentNo = CommentNo,
                Project = "PP001",
                Year = 2024,
                Topic = "Budget",
                Comment = "Review required",
                MadeBy = "user1",
                DateEntered = new DateTime(2024, 1, 15)
            };

            _service.GetByIdAsync(CommentNo).Returns(commentDto);
            _mapper.Map<CommentRes>(commentDto).Returns(commentRes);

            // Act
            var result = await _controller.GetById(CommentNo);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(commentRes, okResult.Value);

            await _service.Received(1).GetByIdAsync(CommentNo);
            _mapper.Received(1).Map<CommentRes>(commentDto);
        }

        [Fact]
        public async Task GetById_WhenCommentNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var CommentNo = 999;
            _service.GetByIdAsync(CommentNo).Returns((CommentDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _controller.GetById(CommentNo));

            Assert.Equal($"Comment {CommentNo} not found.", exception.Message);

            await _service.Received(1).GetByIdAsync(CommentNo);
            _mapper.DidNotReceive().Map<CommentRes>(Arg.Any<CommentDto>());
        }

        [Fact]
        public async Task GetById_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var CommentNo = 1;
            _service.GetByIdAsync(CommentNo).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetById(CommentNo));

            await _service.Received(1).GetByIdAsync(CommentNo);
            _mapper.DidNotReceive().Map<CommentRes>(Arg.Any<CommentDto>());
        }

        #endregion

        #region Create

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WithMappedComment()
        {
            // Arrange
            var request = new CommentReq
            {
                Project = "PP001",
                Year = 2024,
                Topic = "Budget",
                Comment = "Initial review required",
                MadeBy = "user1"
            };
            var dto = new CommentDto
            {
                Project = "PP001",
                Year = 2024,
                Topic = "Budget",
                CommentText = "Initial review required",
                MadeBy = "user1"
            };
            var createdDto = new CommentDto
            {
                CommentNo = 42,
                Project = "PP001",
                Year = 2024,
                Topic = "Budget",
                CommentText = "Initial review required",
                MadeBy = "user1",
                DateEntered = new DateTime(2024, 6, 1)
            };
            var createdRes = new CommentRes
            {
                CommentNo = 42,
                Project = "PP001",
                Year = 2024,
                Topic = "Budget",
                Comment = "Initial review required",
                MadeBy = "user1",
                DateEntered = new DateTime(2024, 6, 1)
            };

            _mapper.Map<CommentDto>(request).Returns(dto);
            _service.AddAsync(dto).Returns(createdDto);
            _mapper.Map<CommentRes>(createdDto).Returns(createdRes);

            // Act
            var result = await _controller.Create(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
            Assert.NotNull(createdResult.RouteValues);
            Assert.Equal(42, createdResult.RouteValues["CommentNo"]);
            Assert.Equal(createdRes, createdResult.Value);

            _mapper.Received(1).Map<CommentDto>(request);
            await _service.Received(1).AddAsync(dto);
            _mapper.Received(1).Map<CommentRes>(createdDto);
        }

        [Fact]
        public async Task Create_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var request = new CommentReq { Project = "PP001", Topic = "Budget", Comment = "Review required" };
            var dto = new CommentDto { Project = "PP001", Topic = "Budget", CommentText = "Review required" };

            _mapper.Map<CommentDto>(request).Returns(dto);
            _service.AddAsync(dto).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Create(request));

            _mapper.Received(1).Map<CommentDto>(request);
            await _service.Received(1).AddAsync(dto);
            _mapper.DidNotReceive().Map<CommentRes>(Arg.Any<CommentDto>());
        }

        #endregion

        #region Update

        [Fact]
        public async Task Update_ReturnsOkResult_WithMappedComment_AndSetsCommentNo()
        {
            // Arrange
            var CommentNo = 10;
            var request = new CommentReq
            {
                Project = "PP001",
                Year = 2024,
                Topic = "Risk",
                Comment = "Updated risk review",
                MadeBy = "user2"
            };
            var dto = new CommentDto
            {
                Project = "PP001",
                Year = 2024,
                Topic = "Risk",
                CommentText = "Updated risk review",
                MadeBy = "user2"
            };
            var updatedDto = new CommentDto
            {
                CommentNo = CommentNo,
                Project = "PP001",
                Year = 2024,
                Topic = "Risk",
                CommentText = "Updated risk review",
                MadeBy = "user2"
            };
            var updatedRes = new CommentRes
            {
                CommentNo = CommentNo,
                Project = "PP001",
                Year = 2024,
                Topic = "Risk",
                Comment = "Updated risk review",
                MadeBy = "user2"
            };

            _mapper.Map<CommentDto>(request).Returns(dto);
            _service.UpdateAsync(dto).Returns(updatedDto);
            _mapper.Map<CommentRes>(updatedDto).Returns(updatedRes);

            // Act
            var result = await _controller.Update(CommentNo, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updatedRes, okResult.Value);

            // Verify CommentNo was set on the dto before service call
            Assert.Equal(CommentNo, dto.CommentNo);

            _mapper.Received(1).Map<CommentDto>(request);
            await _service.Received(1).UpdateAsync(dto);
            _mapper.Received(1).Map<CommentRes>(updatedDto);
        }

        [Fact]
        public async Task Update_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var CommentNo = 10;
            var request = new CommentReq { Project = "PP001", Topic = "Risk", Comment = "Updated risk review" };
            var dto = new CommentDto { Project = "PP001", Topic = "Risk", CommentText = "Updated risk review" };

            _mapper.Map<CommentDto>(request).Returns(dto);
            _service.UpdateAsync(dto).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Update(CommentNo, request));

            _mapper.Received(1).Map<CommentDto>(request);
            await _service.Received(1).UpdateAsync(dto);
            _mapper.DidNotReceive().Map<CommentRes>(Arg.Any<CommentDto>());
        }

        #endregion

        #region Delete

        [Fact]
        public async Task Delete_WhenCommentExists_ReturnsOkWithTrue()
        {
            // Arrange
            var CommentNo = 1;
            _service.DeleteAsync(CommentNo).Returns(true);

            // Act
            var result = await _controller.Delete(CommentNo);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<bool>(okResult.Value);
            Assert.True(value);

            await _service.Received(1).DeleteAsync(CommentNo);
        }

        [Fact]
        public async Task Delete_WhenCommentDoesNotExist_ReturnsOkWithFalse()
        {
            // Arrange
            var CommentNo = 999;
            _service.DeleteAsync(CommentNo).Returns(false);

            // Act
            var result = await _controller.Delete(CommentNo);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<bool>(okResult.Value);
            Assert.False(value);

            await _service.Received(1).DeleteAsync(CommentNo);
        }

        [Fact]
        public async Task Delete_WhenServiceThrowsException_PropagatesException()
        {
            // Arrange
            var CommentNo = 1;
            _service.DeleteAsync(CommentNo).Throws(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Delete(CommentNo));

            await _service.Received(1).DeleteAsync(CommentNo);
        }

        #endregion

        #region GetCommentTopics

        [Fact]
        public async Task GetCommentTopics_ReturnsOkResult_WithMappedTopics()
        {
            // Arrange
            var topicDtos = new List<CommentTopicDto>
            {
                new CommentTopicDto { Topic = "Budget" },
                new CommentTopicDto { Topic = "Risk" }
            };
            var topicRes = new List<CommentTopicRes>
            {
                new CommentTopicRes { Topic = "Budget" },
                new CommentTopicRes { Topic = "Risk" }
            };

            _service.GetCommentTopicsAsync().Returns(topicDtos);
            _mapper.Map<IEnumerable<CommentTopicRes>>(topicDtos).Returns(topicRes);

            // Act
            var result = await _controller.GetCommentTopics();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(topicRes, okResult.Value);
            await _service.Received(1).GetCommentTopicsAsync();
            _mapper.Received(1).Map<IEnumerable<CommentTopicRes>>(topicDtos);
        }

        #endregion

        #region GetForecastSpendByProject

        [Fact]
        public async Task GetForecastSpendByProject_ReturnsOkResult_WithForecastSpendResponse()
        {
            // Arrange
            const string project = "PP001";
            const double expectedForecastSpend = 12345.67;
            _service.GetForecastSpendByProjectAsync(project).Returns(expectedForecastSpend);

            // Act
            var result = await _controller.GetForecastSpendByProject(project);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<ProjectCommentForecastSpendRes>(okResult.Value);
            Assert.Equal(expectedForecastSpend, value.ForecastSpend);
            await _service.Received(1).GetForecastSpendByProjectAsync(project);
        }

        #endregion

        #region UpdateForecastSpendByProject

        [Fact]
        public async Task UpdateForecastSpendByProject_WithEmptyProject_ReturnsBadRequest()
        {
            // Arrange
            var request = new ProjectCommentForecastSpendRes { ForecastSpend = 12.34 };

            // Act
            var result = await _controller.UpdateForecastSpendByProject(string.Empty, request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Project is required.", badRequest.Value);
            await _service.DidNotReceive().UpdateForecastSpendByProjectAsync(Arg.Any<string>(), Arg.Any<double?>());
        }

        [Fact]
        public async Task UpdateForecastSpendByProject_WithNullRequest_ReturnsBadRequest()
        {
            // Arrange
            const string project = "PP001";

            // Act
            var result = await _controller.UpdateForecastSpendByProject(project, null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Forecast spend payload is required.", badRequest.Value);
            await _service.DidNotReceive().UpdateForecastSpendByProjectAsync(Arg.Any<string>(), Arg.Any<double?>());
        }

        [Fact]
        public async Task UpdateForecastSpendByProject_WithValidInput_ReturnsOkResult_WithUpdatedForecastSpend()
        {
            // Arrange
            const string project = "PP001";
            var request = new ProjectCommentForecastSpendRes { ForecastSpend = 9876.54 };
            _service.UpdateForecastSpendByProjectAsync(project, request.ForecastSpend).Returns(request.ForecastSpend);

            // Act
            var result = await _controller.UpdateForecastSpendByProject(project, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<ProjectCommentForecastSpendRes>(okResult.Value);
            Assert.Equal(request.ForecastSpend, value.ForecastSpend);
            await _service.Received(1).UpdateForecastSpendByProjectAsync(project, request.ForecastSpend);
        }

        #endregion
    }
}
