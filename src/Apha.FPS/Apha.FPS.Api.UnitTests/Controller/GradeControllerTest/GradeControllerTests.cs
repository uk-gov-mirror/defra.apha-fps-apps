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
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.GradeControllerTest
{
    public class GradeControllerTests
    {
        private readonly IGradeService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly GradeController _controller;

        public GradeControllerTests()
        {
            _serviceMock = Substitute.For<IGradeService>();
            _mapperMock  = Substitute.For<IMapper>();
            _controller  = new GradeController(_serviceMock, _mapperMock);
        }

        private static GradeDto BuildDto(string code = "A") =>
            new() { GradeCode = code, Description = "Grade A", AvSalary = 50000m, FpsYear = 2025 };

        private static GradeReq BuildReq(string code = "A") =>
            new() { GradeCode = code, Description = "Grade A", AvSalary = 50000m };

        private static GradeRes BuildRes(string code = "A") =>
            new() { GradeCode = code, Description = "Grade A", AvSalary = 50000m };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenServiceIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GradeController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new GradeController(_serviceMock, null!));
        }

        #endregion

        #region GetAllPagedAsync Tests

        [Fact]
        public async Task GetAllPagedAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<GradeDto> { BuildDto() };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var serviceResult = new PaginatedResult<GradeDto>(dtos, pagination);
            var expectedResponse = new PaginationRes<GradeRes>
            {
                Data = new List<GradeRes> { BuildRes() },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _serviceMock.GetAllPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<GradeRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetAllPagedAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
            await _serviceMock.Received(1).GetAllPagedAsync(query);
        }

        [Fact]
        public async Task GetAllPagedAsync_NullResult_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _serviceMock.GetAllPagedAsync(query).Returns((PaginatedResult<GradeDto>)null!);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetAllPagedAsync(query));
        }

        [Fact]
        public async Task GetAllPagedAsync_WithFilterAndSorting_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 2, PageSize = 5, SortBy = "GradeCode", Descending = true
            };
            var dtos = new List<GradeDto> { BuildDto() };
            var pagination = new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 10 };
            var serviceResult = new PaginatedResult<GradeDto>(dtos, pagination);
            var expectedResponse = new PaginationRes<GradeRes>
            {
                Data = new List<GradeRes> { BuildRes() },
                PaginationData = new Pagination { PageNumber = 2, PageSize = 5, TotalRecords = 10 }
            };

            _serviceMock.GetAllPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<GradeRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetAllPagedAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<GradeRes>>(okResult.Value);
            Assert.Equal(2, response.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetAllPagedAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _serviceMock.GetAllPagedAsync(query).ThrowsAsync(new InvalidOperationException("service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetAllPagedAsync(query));
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var dto = BuildDto("A");
            var res = BuildRes("A");

            _serviceMock.GetByIdAsync("A").Returns(dto);
            _mapperMock.Map<GradeRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetByIdAsync("A");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, okResult.Value);
            await _serviceMock.Received(1).GetByIdAsync("A");
        }

        [Fact]
        public async Task GetByIdAsync_NullResult_ThrowsArgumentException()
        {
            // Arrange
            _serviceMock.GetByIdAsync("NOTEXIST").Returns((GradeDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.GetByIdAsync("NOTEXIST"));
            Assert.Contains("NOTEXIST", exception.Message);
        }

        [Fact]
        public async Task GetByIdAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetByIdAsync("A").ThrowsAsync(new InvalidOperationException("service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetByIdAsync("A"));
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var req     = BuildReq("A");
            var dto     = BuildDto("A");
            var created = BuildDto("A");
            var res     = BuildRes("A");

            _mapperMock.Map<GradeDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).Returns(created);
            _mapperMock.Map<GradeRes>(created).Returns(res);

            // Act
            var result = await _controller.CreateAsync(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, okResult.Value);
            await _serviceMock.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var req = BuildReq();
            var dto = BuildDto();

            _mapperMock.Map<GradeDto>(req).Returns(dto);
            _serviceMock.CreateAsync(dto).ThrowsAsync(new InvalidOperationException("already exists"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.CreateAsync(req));
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var req     = BuildReq("A");
            var dto     = BuildDto("A");
            var updated = BuildDto("A");
            var res     = BuildRes("A");

            _mapperMock.Map<GradeDto>(req).Returns(dto);
            _serviceMock.UpdateAsync("A", dto).Returns(updated);
            _mapperMock.Map<GradeRes>(updated).Returns(res);

            // Act
            var result = await _controller.UpdateAsync("A", req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, okResult.Value);
            await _serviceMock.Received(1).UpdateAsync("A", dto);
        }

        [Fact]
        public async Task UpdateAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var req = BuildReq();
            var dto = BuildDto();

            _mapperMock.Map<GradeDto>(req).Returns(dto);
            _serviceMock.UpdateAsync("A", dto).ThrowsAsync(new InvalidOperationException("not found"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.UpdateAsync("A", req));
        }

        [Fact]
        public async Task UpdateAsync_WhenServiceThrowsOnCodeConflict_PropagatesException()
        {
            // Arrange
            var req = BuildReq("B");
            var dto = BuildDto("B");

            _mapperMock.Map<GradeDto>(req).Returns(dto);
            _serviceMock.UpdateAsync("A", dto)
                .ThrowsAsync(new InvalidOperationException("Cannot rename — code already exists"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.UpdateAsync("A", req));
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            _serviceMock.DeleteAsync("A").Returns(true);

            // Act
            var result = await _controller.DeleteAsync("A");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).DeleteAsync("A");
        }

        [Fact]
        public async Task DeleteAsync_WithNullOrWhitespace_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteAsync("   "));
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ThrowsArgumentException()
        {
            // Arrange
            _serviceMock.DeleteAsync("NOTEXIST").Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.DeleteAsync("NOTEXIST"));
            Assert.Contains("NOTEXIST", exception.Message);
        }

        [Fact]
        public async Task DeleteAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.DeleteAsync("A").ThrowsAsync(new InvalidOperationException("service error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.DeleteAsync("A"));
        }

        #endregion
    }
}
