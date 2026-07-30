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

namespace Apha.FPS.Api.UnitTests.Controller.ResourceAllocationControllerTest
{
    public class ResourceAllocationControllerTests
    {
        private const string DefaultWorkGroupGrade = "WG01";
        private const string DefaultStaffId = "PACT001";

        private readonly IResourceAllocationService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ResourceAllocationController _controller;

        public ResourceAllocationControllerTests()
        {
            _serviceMock = Substitute.For<IResourceAllocationService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ResourceAllocationController(_serviceMock, _mapperMock);
        }

        // ── Constructor Tests ─────────────────────────────────────────────────

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ResourceAllocationController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ResourceAllocationController(_serviceMock, null!));
        }

        #endregion

        // ── GetPagedStaffAllocationsByWorkGroupGradeAsync Tests ───────────────

        #region GetPagedStaffAllocationsByWorkGroupGradeAsync Tests

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2, TotalPages = 1 };
            var dtos = new List<ResourceStaffAllocationDto>
            {
                new() { StaffId = "PACT001", Name = "Alpha, Staff", PlannedHours = 20.0 },
                new() { StaffId = "PACT002", Name = "Beta, Staff",  PlannedHours = 15.0 }
            };
            var serviceResult = new PaginatedResult<ResourceStaffAllocationDto>(dtos, paginationDto);
            var expectedRes = new PaginationRes<ResourceStaffAllocationRes>
            {
                Data = new List<ResourceStaffAllocationRes>
                {
                    new() { StaffId = "PACT001", PlannedHours = 20.0 },
                    new() { StaffId = "PACT002", PlannedHours = 15.0 }
                }
            };

            _serviceMock.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query)
                .Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ResourceStaffAllocationRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1)
                .GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query);
        }

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_WithNullQuery_UsesDefaultQuery()
        {
            // Arrange
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 };
            var serviceResult = new PaginatedResult<ResourceStaffAllocationDto>([], paginationDto);
            var expectedRes = new PaginationRes<ResourceStaffAllocationRes>();

            _serviceMock.GetPagedStaffAllocationsByWorkGroupGradeAsync(
                    DefaultWorkGroupGrade, Arg.Any<QueryParameters<string>>())
                .Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ResourceStaffAllocationRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, null!);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1)
                .GetPagedStaffAllocationsByWorkGroupGradeAsync(
                    DefaultWorkGroupGrade, Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _serviceMock.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query)
                .ThrowsAsync(new ArgumentException("Invalid grade"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query));
        }

        [Fact]
        public async Task GetPagedStaffAllocationsByWorkGroupGradeAsync_WithEmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 };
            var serviceResult = new PaginatedResult<ResourceStaffAllocationDto>([], paginationDto);
            var expectedRes = new PaginationRes<ResourceStaffAllocationRes> { Data = [] };

            _serviceMock.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query)
                .Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ResourceStaffAllocationRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetPagedStaffAllocationsByWorkGroupGradeAsync(DefaultWorkGroupGrade, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<PaginationRes<ResourceStaffAllocationRes>>(okResult.Value);
            Assert.Empty(value.Data!);
        }

        #endregion

        // ── GetPagedStaffJobDetailsByStaffIdAsync Tests ───────────────────────

        #region GetPagedStaffJobDetailsByStaffIdAsync Tests

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var dtos = new List<ResourceStaffJobDetailDto>
            {
                new() { StaffId = DefaultStaffId, JobCode = "J001", PlannedHours = 10.0 }
            };
            var serviceResult = new PaginatedResult<ResourceStaffJobDetailDto>(dtos, paginationDto);
            var expectedRes = new PaginationRes<ResourceStaffJobDetailRes>
            {
                Data = new List<ResourceStaffJobDetailRes>
                {
                    new() { StaffId = DefaultStaffId, JobCode = "J001", PlannedHours = 10.0 }
                }
            };

            _serviceMock.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query)
                .Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ResourceStaffJobDetailRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1)
                .GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query);
        }

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_WithNullQuery_UsesDefaultQuery()
        {
            // Arrange
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 };
            var serviceResult = new PaginatedResult<ResourceStaffJobDetailDto>([], paginationDto);
            var expectedRes = new PaginationRes<ResourceStaffJobDetailRes>();

            _serviceMock.GetPagedStaffJobDetailsByStaffIdAsync(
                    DefaultStaffId, Arg.Any<QueryParameters<string>>())
                .Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ResourceStaffJobDetailRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, null!);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1)
                .GetPagedStaffJobDetailsByStaffIdAsync(
                    DefaultStaffId, Arg.Any<QueryParameters<string>>());
        }

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _serviceMock.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query)
                .ThrowsAsync(new ArgumentException("Invalid staffId"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query));
        }

        [Fact]
        public async Task GetPagedStaffJobDetailsByStaffIdAsync_WithEmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 };
            var serviceResult = new PaginatedResult<ResourceStaffJobDetailDto>([], paginationDto);
            var expectedRes = new PaginationRes<ResourceStaffJobDetailRes> { Data = [] };

            _serviceMock.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query)
                .Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ResourceStaffJobDetailRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetPagedStaffJobDetailsByStaffIdAsync(DefaultStaffId, query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = Assert.IsType<PaginationRes<ResourceStaffJobDetailRes>>(okResult.Value);
            Assert.Empty(value.Data!);
        }

        #endregion
    }
}
