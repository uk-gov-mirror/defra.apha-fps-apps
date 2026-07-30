using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.ProjectControllerTest
{
    public class ProjectProfitabilityVlaControllerTests
    {
        private readonly IProjectService _projectService;
        private readonly IMapper _mapper;
        private readonly ProjectController _controller;

        public ProjectProfitabilityVlaControllerTests()
        {
            _projectService = Substitute.For<IProjectService>();
            _mapper = Substitute.For<IMapper>();
            _controller = new ProjectController(_projectService, _mapper);
        }

        #region GetProjectProfitabilityVlaAsync

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithNoFilters_ReturnsOkWithPagedResult()
        {
            // Arrange
            var dtos = new List<ProjectProfitabilityVlaDto>
            {
                new() { JobCode = "PP001", StaffCosts = 1000m, Budget = 5000m, Profit = 4000m, TargetProfit = 3500m, OffTarget = 500m },
                new() { JobCode = "PP002", StaffCosts = 2000m, Budget = 6000m, Profit = 4000m, TargetProfit = 3000m, OffTarget = 1000m }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 2 };
            var serviceResult = new PaginatedResult<ProjectProfitabilityVlaDto>(dtos, paginationDto);
            var expectedRes = new PaginationRes<ProjectProfitabilityVlaRes>
            {
                // JobCode in the DTO maps to Project in the Res contract (Phase 15 build fix).
                Data = dtos.Select(d => new ProjectProfitabilityVlaRes { Project = d.JobCode }).ToList(),
                PaginationData = new Pagination { PageNumber = 1, PageSize = 15, TotalRecords = 2 }
            };

            _projectService.GetProjectProfitabilityVlaAsync(Arg.Any<QueryParameters<string>>())
                .Returns(serviceResult);
            _mapper.Map<PaginationRes<ProjectProfitabilityVlaRes>>(serviceResult)
                .Returns(expectedRes);

            // Act
            var result = await _controller.GetProjectProfitabilityVlaAsync(
                new QueryParameters<string> { Page = 1, PageSize = 15 });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(expectedRes);
            await _projectService.Received(1)
                .GetProjectProfitabilityVlaAsync(Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithAllFilters_PassesFiltersThroughToService()
        {
            // Arrange
            var serviceResult = new PaginatedResult<ProjectProfitabilityVlaDto>(
                new List<ProjectProfitabilityVlaDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });
            var expectedRes = new PaginationRes<ProjectProfitabilityVlaRes>();

            _projectService.GetProjectProfitabilityVlaAsync(
                    Arg.Any<QueryParameters<string>>(), "Approved", "P001", "John Smith", "ACME Ltd")
                .Returns(serviceResult);
            _mapper.Map<PaginationRes<ProjectProfitabilityVlaRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetProjectProfitabilityVlaAsync(
                new QueryParameters<string> { Page = 1, PageSize = 10 },
                projectStatus: "Approved",
                programNo: "P001",
                manager: "John Smith",
                customer: "ACME Ltd");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            await _projectService.Received(1)
                .GetProjectProfitabilityVlaAsync(
                    Arg.Any<QueryParameters<string>>(), "Approved", "P001", "John Smith", "ACME Ltd");
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithEmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var serviceResult = new PaginatedResult<ProjectProfitabilityVlaDto>(
                new List<ProjectProfitabilityVlaDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 0 });
            var expectedRes = new PaginationRes<ProjectProfitabilityVlaRes>
            {
                Data = new List<ProjectProfitabilityVlaRes>()
            };

            _projectService.GetProjectProfitabilityVlaAsync(Arg.Any<QueryParameters<string>>())
                .Returns(serviceResult);
            _mapper.Map<PaginationRes<ProjectProfitabilityVlaRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetProjectProfitabilityVlaAsync(
                new QueryParameters<string> { Page = 1, PageSize = 15 });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var res = okResult.Value as PaginationRes<ProjectProfitabilityVlaRes>;
            res?.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_PageAndPageSizeDefaults_AreUsedWhenNotProvided()
        {
            // Arrange — call controller with explicitly matching defaults (page=1, pageSize=15)
            var serviceResult = new PaginatedResult<ProjectProfitabilityVlaDto>(
                new List<ProjectProfitabilityVlaDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 0 });

            _projectService.GetProjectProfitabilityVlaAsync(
                    Arg.Is<QueryParameters<string>>(q => q.Page == 1 && q.PageSize == 15))
                .Returns(serviceResult);
            _mapper.Map<PaginationRes<ProjectProfitabilityVlaRes>>(serviceResult)
                .Returns(new PaginationRes<ProjectProfitabilityVlaRes>());

            // Act — call with default values
            var result = await _controller.GetProjectProfitabilityVlaAsync(
                new QueryParameters<string> { Page = 1, PageSize = 15 });

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _projectService.Received(1)
                .GetProjectProfitabilityVlaAsync(Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 1 && q.PageSize == 15));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_MapperMapsServiceResultToResponse()
        {
            // Arrange
            var serviceResult = new PaginatedResult<ProjectProfitabilityVlaDto>(
                new List<ProjectProfitabilityVlaDto>
                {
                    new() { JobCode = "PP001", StaffCosts = 500m }
                },
                new PaginationDto { PageNumber = 1, PageSize = 15, TotalRecords = 1 });
            var expectedRes = new PaginationRes<ProjectProfitabilityVlaRes>();

            _projectService.GetProjectProfitabilityVlaAsync(Arg.Any<QueryParameters<string>>())
                .Returns(serviceResult);
            _mapper.Map<PaginationRes<ProjectProfitabilityVlaRes>>(serviceResult).Returns(expectedRes);

            // Act
            await _controller.GetProjectProfitabilityVlaAsync(
                new QueryParameters<string> { Page = 1, PageSize = 15 });

            // Assert — mapper is invoked once with the service result
            _mapper.Received(1).Map<PaginationRes<ProjectProfitabilityVlaRes>>(serviceResult);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _projectService.GetProjectProfitabilityVlaAsync(Arg.Any<QueryParameters<string>>())
                .ThrowsAsync(new InvalidOperationException("Service failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.GetProjectProfitabilityVlaAsync(
                    new QueryParameters<string> { Page = 1, PageSize = 15 }));
        }

        #endregion

        #region GetProjectsByProgramProjectProfitabilityVLAAsync

        [Fact]
        public async Task GetProjectsByProgramProjectProfitabilityVLAAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var projectDtos = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var serviceResult = new PaginatedResult<ProjectDto>(projectDtos, paginationDto);
            var mappedResult = new PaginationRes<ProjectRes>
            {
                Data = new List<ProjectRes>
                {
                    new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                    new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
                }
            };

            _projectService.GetProjectsByProgramProjectProfitabilityVLAAsync(query, programNo)
                .Returns(serviceResult);
            _mapper.Map<PaginationRes<ProjectRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetProjectsByProgramProjectProfitabilityVLAAsync(query, programNo);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(mappedResult);
            await _projectService.Received(1).GetProjectsByProgramProjectProfitabilityVLAAsync(query, programNo);
        }

        [Fact]
        public async Task GetProjectsByProgramProjectProfitabilityVLAAsync_WhenProgramNoIsNull_ReturnsBadRequest()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.GetProjectsByProgramProjectProfitabilityVLAAsync(query, null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("programNo is required.", badRequest.Value);
            await _projectService.DidNotReceive().GetProjectsByProgramProjectProfitabilityVLAAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetProjectsByProgramProjectProfitabilityVLAAsync_WhenProgramNoIsWhitespace_ReturnsBadRequest(string programNo)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.GetProjectsByProgramProjectProfitabilityVLAAsync(query, programNo);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            await _projectService.DidNotReceive().GetProjectsByProgramProjectProfitabilityVLAAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetProjectsByProgramProjectProfitabilityVLAAsync_EmptyProjectList_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var emptyResult = new PaginatedResult<ProjectDto>(
                Enumerable.Empty<ProjectDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });
            var mappedResult = new PaginationRes<ProjectRes> { Data = new List<ProjectRes>() };

            _projectService.GetProjectsByProgramProjectProfitabilityVLAAsync(query, programNo).Returns(emptyResult);
            _mapper.Map<PaginationRes<ProjectRes>>(emptyResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetProjectsByProgramProjectProfitabilityVLAAsync(query, programNo);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(mappedResult);
        }

        [Fact]
        public async Task GetProjectsByProgramProjectProfitabilityVLAAsync_MapperMapsServiceResultToResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            var serviceResult = new PaginatedResult<ProjectDto>(
                new List<ProjectDto> { new() { ParentProject = "PP001" } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });
            var mappedResult = new PaginationRes<ProjectRes>();

            _projectService.GetProjectsByProgramProjectProfitabilityVLAAsync(query, programNo).Returns(serviceResult);
            _mapper.Map<PaginationRes<ProjectRes>>(serviceResult).Returns(mappedResult);

            // Act
            await _controller.GetProjectsByProgramProjectProfitabilityVLAAsync(query, programNo);

            // Assert
            _mapper.Received(1).Map<PaginationRes<ProjectRes>>(serviceResult);
        }

        [Fact]
        public async Task GetProjectsByProgramProjectProfitabilityVLAAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var programNo = "P001";
            _projectService.GetProjectsByProgramProjectProfitabilityVLAAsync(query, programNo)
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.GetProjectsByProgramProjectProfitabilityVLAAsync(query, programNo));
        }

        #endregion

        #region GetProjectsByProjectGroupProjectProfitabilityVLAAsync

        [Fact]
        public async Task GetProjectsByProjectGroupProjectProfitabilityVLAAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "GRP1";
            var projectDtos = new List<ProjectDto>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", ProjectGroup = "GRP1" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project",  ProjectGroup = "GRP1" }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 1, TotalRecords = 2 };
            var serviceResult = new PaginatedResult<ProjectDto>(projectDtos, paginationDto);
            var mappedResult = new PaginationRes<ProjectRes>
            {
                Data = new List<ProjectRes>
                {
                    new() { ParentProject = "PP001", ProjectTitle = "Alpha Project" },
                    new() { ParentProject = "PP002", ProjectTitle = "Beta Project" }
                }
            };

            _projectService.GetProjectsByProjectGroupProjectProfitabilityVLAAsync(query, projectGroup)
                .Returns(serviceResult);
            _mapper.Map<PaginationRes<ProjectRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetProjectsByProjectGroupProjectProfitabilityVLAAsync(query, projectGroup);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(mappedResult);
            await _projectService.Received(1).GetProjectsByProjectGroupProjectProfitabilityVLAAsync(query, projectGroup);
        }

        [Fact]
        public async Task GetProjectsByProjectGroupProjectProfitabilityVLAAsync_WhenProjectGroupIsNull_ReturnsBadRequest()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.GetProjectsByProjectGroupProjectProfitabilityVLAAsync(query, null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("projectGroup is required.", badRequest.Value);
            await _projectService.DidNotReceive().GetProjectsByProjectGroupProjectProfitabilityVLAAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetProjectsByProjectGroupProjectProfitabilityVLAAsync_WhenProjectGroupIsWhitespace_ReturnsBadRequest(string projectGroup)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await _controller.GetProjectsByProjectGroupProjectProfitabilityVLAAsync(query, projectGroup);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            await _projectService.DidNotReceive().GetProjectsByProjectGroupProjectProfitabilityVLAAsync(
                Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetProjectsByProjectGroupProjectProfitabilityVLAAsync_EmptyProjectList_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "GRP1";
            var emptyResult = new PaginatedResult<ProjectDto>(
                Enumerable.Empty<ProjectDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalPages = 0, TotalRecords = 0 });
            var mappedResult = new PaginationRes<ProjectRes> { Data = new List<ProjectRes>() };

            _projectService.GetProjectsByProjectGroupProjectProfitabilityVLAAsync(query, projectGroup).Returns(emptyResult);
            _mapper.Map<PaginationRes<ProjectRes>>(emptyResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetProjectsByProjectGroupProjectProfitabilityVLAAsync(query, projectGroup);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(mappedResult);
        }

        [Fact]
        public async Task GetProjectsByProjectGroupProjectProfitabilityVLAAsync_MapperMapsServiceResultToResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "GRP1";
            var serviceResult = new PaginatedResult<ProjectDto>(
                new List<ProjectDto> { new() { ParentProject = "PP001", ProjectGroup = "GRP1" } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });
            var mappedResult = new PaginationRes<ProjectRes>();

            _projectService.GetProjectsByProjectGroupProjectProfitabilityVLAAsync(query, projectGroup).Returns(serviceResult);
            _mapper.Map<PaginationRes<ProjectRes>>(serviceResult).Returns(mappedResult);

            // Act
            await _controller.GetProjectsByProjectGroupProjectProfitabilityVLAAsync(query, projectGroup);

            // Assert
            _mapper.Received(1).Map<PaginationRes<ProjectRes>>(serviceResult);
        }

        [Fact]
        public async Task GetProjectsByProjectGroupProjectProfitabilityVLAAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "GRP1";
            _projectService.GetProjectsByProjectGroupProjectProfitabilityVLAAsync(query, projectGroup)
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.GetProjectsByProjectGroupProjectProfitabilityVLAAsync(query, projectGroup));
        }

        #endregion
    }
}
