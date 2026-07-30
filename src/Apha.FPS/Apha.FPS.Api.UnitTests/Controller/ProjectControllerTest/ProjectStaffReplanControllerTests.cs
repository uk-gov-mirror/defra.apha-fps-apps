using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPS.Api.UnitTests.Controller.ProjectControllerTest
{
    public class ProjectStaffReplanControllerTests
    {
        private readonly IProjectService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ProjectController _controller;

        public ProjectStaffReplanControllerTests()
        {
            _serviceMock = Substitute.For<IProjectService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ProjectController(_serviceMock, _mapperMock);
        }

        // ── GetProjectStaffReplanAsync Tests ──────────────────────────────────

        #region GetProjectStaffReplanAsync

        [Fact]
        public async Task GetProjectStaffReplanAsync_HappyPath_ReturnsOkWithMappedData()
        {
            // Arrange
            var workgroup = "WorkGroupA";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            var dtos = new List<ProjectStaffReplanDto>
            {
                new() { WorkGroup = workgroup, WgGrade = "WG01", GradeCode = "GC01", Name = "Smith, John",   PlannedHours = 10.0, ParentProject = "PP001", Program = "P001" },
                new() { WorkGroup = workgroup, WgGrade = "WG01", GradeCode = "GC01", Name = "Jones, Alice",  PlannedHours = 8.0,  ParentProject = "PP002", Program = "P001" }
            };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 };
            var serviceResult = new PaginatedResult<ProjectStaffReplanDto>(dtos, pagination);

            var mappedResult = new PaginationRes<ProjectStaffReplanRes>
            {
                Data = new List<ProjectStaffReplanRes>
                {
                    new() { WorkGroup = workgroup, WgGrade = "WG01", Name = "Smith, John",  PlannedHours = 10.0 },
                    new() { WorkGroup = workgroup, WgGrade = "WG01", Name = "Jones, Alice", PlannedHours = 8.0 }
                }
            };

            _serviceMock.GetProjectStaffReplanAsync(query, workgroup).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectStaffReplanRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetProjectStaffReplanAsync(workgroup, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            await _serviceMock.Received(1).GetProjectStaffReplanAsync(query, workgroup);
            _mapperMock.Received(1).Map<PaginationRes<ProjectStaffReplanRes>>(serviceResult);
        }

        [Fact]
        public async Task GetProjectStaffReplanAsync_WithEmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var workgroup = "WorkGroupA";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            var emptyResult = new PaginatedResult<ProjectStaffReplanDto>(
                [],
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });
            var mappedResult = new PaginationRes<ProjectStaffReplanRes> { Data = [] };

            _serviceMock.GetProjectStaffReplanAsync(query, workgroup).Returns(emptyResult);
            _mapperMock.Map<PaginationRes<ProjectStaffReplanRes>>(emptyResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetProjectStaffReplanAsync(workgroup, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<PaginationRes<ProjectStaffReplanRes>>(okResult.Value);
            Assert.Empty(value.Data!);
        }

        [Theory]
        [InlineData("WorkGroupA")]
        [InlineData("WG-Budget-001")]
        [InlineData("Test Group")]
        public async Task GetProjectStaffReplanAsync_WithVariousWorkgroups_CallsServiceWithCorrectWorkgroup(string workgroup)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<ProjectStaffReplanDto>(
                [], new PaginationDto());
            var mappedResult = new PaginationRes<ProjectStaffReplanRes>();

            _serviceMock.GetProjectStaffReplanAsync(query, workgroup).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectStaffReplanRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetProjectStaffReplanAsync(workgroup, query);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).GetProjectStaffReplanAsync(query, workgroup);
        }

        [Fact]
        public async Task GetProjectStaffReplanAsync_WithPaginationParameters_ReturnsCorrectPage()
        {
            // Arrange
            var workgroup = "WorkGroupA";
            var query = new QueryParameters<string> { Page = 2, PageSize = 5 };

            var dtos = new List<ProjectStaffReplanDto>
            {
                new() { WorkGroup = workgroup, WgGrade = "WG01", Name = "Brown, Bob", PlannedHours = 5.0, ParentProject = "PP003", Program = "P002" }
            };
            var pagination = new PaginationDto { PageNumber = 2, PageSize = 5, TotalPages = 3, TotalRecords = 11 };
            var serviceResult = new PaginatedResult<ProjectStaffReplanDto>(dtos, pagination);
            var mappedResult = new PaginationRes<ProjectStaffReplanRes>();

            _serviceMock.GetProjectStaffReplanAsync(query, workgroup).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProjectStaffReplanRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetProjectStaffReplanAsync(workgroup, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            await _serviceMock.Received(1).GetProjectStaffReplanAsync(query, workgroup);
        }

        [Fact]
        public async Task GetProjectStaffReplanAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var workgroup = "WorkGroupA";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _serviceMock.GetProjectStaffReplanAsync(query, workgroup)
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(
                () => _controller.GetProjectStaffReplanAsync(workgroup, query));
            await _serviceMock.Received(1).GetProjectStaffReplanAsync(query, workgroup);
        }

        #endregion
    }
}
