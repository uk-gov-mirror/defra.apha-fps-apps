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

namespace Apha.FPS.Api.UnitTests.Controller.WorkGroupEmployeeControllerTest
{
    public class WorkGroupEmployeeControllerTests
    {
        private const string DefaultWgGrade = "WG01";
        private const string DefaultPactId  = "PACT001";

        private readonly IWorkGroupEmployeeService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly WorkGroupEmployeeController _controller;

        public WorkGroupEmployeeControllerTests()
        {
            _serviceMock = Substitute.For<IWorkGroupEmployeeService>();
            _mapperMock  = Substitute.For<IMapper>();
            _controller  = new WorkGroupEmployeeController(_serviceMock, _mapperMock);
        }

        #region GetWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var query  = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var employees = new List<WorkGroupEmployeeDto>
            {
                new() { PactId = DefaultPactId, SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResult = new PaginatedResult<WorkGroupEmployeeDto>(employees, paginationDto);
            var expectedRes   = new PaginationRes<WorkGroupEmployeeRes>
            {
                Data           = new List<WorkGroupEmployeeRes> { new() { PactId = DefaultPactId } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetWorkGroupEmployeeAsync(mapped, DefaultWgGrade).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupEmployeeRes>>(serviceResult).Returns(expectedRes);

            // Act
            var result = await _controller.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).GetWorkGroupEmployeeAsync(mapped, DefaultWgGrade);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query  = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetWorkGroupEmployeeAsync(mapped, DefaultWgGrade)
                .ThrowsAsync(new ArgumentException("Invalid wg grade"));

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _controller.GetWorkGroupEmployeeAsync(query, DefaultWgGrade));
        }

        [Fact]
        public async Task GetWorkGroupEmployeeForStaffAsync_WithNullWgGrade_UsesEmptyStringAndReturnsOk()
        {
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WorkGroupEmployeeDto>([], new PaginationDto());
            var expectedRes = new PaginationRes<WorkGroupEmployeeRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetWorkGroupEmployeeForStaffAsync(mapped, string.Empty).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupEmployeeRes>>(serviceResult).Returns(expectedRes);

            var result = await _controller.GetWorkGroupEmployeeForStaffAsync(query, null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).GetWorkGroupEmployeeForStaffAsync(mapped, string.Empty);
        }

        #endregion

        #region GetAllActiveWorkGroupEmployeesAsync Tests

        [Fact]
        public async Task GetAllActiveWorkGroupEmployeesAsync_WithValidRequest_ReturnsOk()
        {
            var query  = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var employees = new List<WorkGroupEmployeeDto>
            {
                new() { PactId = DefaultPactId, SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade, PersonStatus = "A" }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 };
            var serviceResult = new PaginatedResult<WorkGroupEmployeeDto>(employees, paginationDto);
            var expectedRes   = new PaginationRes<WorkGroupEmployeeRes>
            {
                Data           = new List<WorkGroupEmployeeRes> { new() { PactId = DefaultPactId } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetAllActiveWorkGroupEmployeesAsync(mapped, DefaultWgGrade).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupEmployeeRes>>(serviceResult).Returns(expectedRes);

            var result = await _controller.GetAllActiveWorkGroupEmployeesAsync(query, DefaultWgGrade);

            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).GetAllActiveWorkGroupEmployeesAsync(mapped, DefaultWgGrade);
        }

        [Fact]
        public async Task GetAllActiveWorkGroupEmployeesAsync_WithNullWgGrade_UsesEmptyStringAndCallsService()
        {
            var query  = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WorkGroupEmployeeDto>([], new PaginationDto());
            var expectedRes   = new PaginationRes<WorkGroupEmployeeRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetAllActiveWorkGroupEmployeesAsync(mapped, string.Empty).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WorkGroupEmployeeRes>>(serviceResult).Returns(expectedRes);

            var result = await _controller.GetAllActiveWorkGroupEmployeesAsync(query, null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).GetAllActiveWorkGroupEmployeesAsync(mapped, string.Empty);
        }

        [Fact]
        public async Task GetAllActiveWorkGroupEmployeesAsync_WhenServiceThrows_PropagatesException()
        {
            var query  = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mapped = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mapped);
            _serviceMock.GetAllActiveWorkGroupEmployeesAsync(mapped, DefaultWgGrade)
                .ThrowsAsync(new ArgumentNullException("query"));

            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _controller.GetAllActiveWorkGroupEmployeesAsync(query, DefaultWgGrade));
        }

        #endregion

        #region GetWorkGroupEmployeeByIdAsync Tests

        [Fact]
        public async Task GetWorkGroupEmployeeByIdAsync_WithValidPactId_ReturnsOk()
        {
            // Arrange
            var dto = new WorkGroupEmployeeDto
            {
                PactId         = DefaultPactId,
                SpNumber       = "SP001",
                WorkGroupGrade = DefaultWgGrade
            };
            var expectedRes = new WorkGroupEmployeeRes { PactId = DefaultPactId };

            _serviceMock.GetWorkGroupEmployeeByIdAsync(DefaultPactId).Returns(dto);
            _mapperMock.Map<WorkGroupEmployeeRes>(dto).Returns(expectedRes);

            // Act
            var result = await _controller.GetWorkGroupEmployeeByIdAsync(DefaultPactId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).GetWorkGroupEmployeeByIdAsync(DefaultPactId);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeByIdAsync_WhenNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.GetWorkGroupEmployeeByIdAsync(DefaultPactId).Returns((WorkGroupEmployeeDto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.GetWorkGroupEmployeeByIdAsync(DefaultPactId));
        }

        #endregion

        #region CreateWorkGroupEmployeeForStaffAsync Tests

        [Fact]
        public async Task CreateWorkGroupEmployeeForStaffAsync_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var req         = new WorkGroupEmployeeReq { PactId = DefaultPactId, HrsPaid = 40.0 };
            var dto         = new WorkGroupEmployeeDto { PactId = DefaultPactId, WorkGroupGrade = DefaultWgGrade };
            var createdDto  = new WorkGroupEmployeeDto { PactId = DefaultPactId, WorkGroupGrade = DefaultWgGrade };
            var expectedRes = new WorkGroupEmployeeRes { PactId = DefaultPactId };

            _mapperMock.Map<WorkGroupEmployeeDto>(req).Returns(dto);
            _serviceMock.CreateWorkGroupEmployeeForStaffAsync(dto).Returns(createdDto);
            _mapperMock.Map<WorkGroupEmployeeRes>(createdDto).Returns(expectedRes);

            // Act
            var result = await _controller.CreateWorkGroupEmployeeForStaffAsync(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).CreateWorkGroupEmployeeForStaffAsync(dto);
        }

        [Fact]
        public async Task CreateWorkGroupEmployeeForStaffAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var req = new WorkGroupEmployeeReq { PactId = DefaultPactId };
            var dto = new WorkGroupEmployeeDto { PactId = DefaultPactId, WorkGroupGrade = DefaultWgGrade };

            _mapperMock.Map<WorkGroupEmployeeDto>(req).Returns(dto);
            _serviceMock.CreateWorkGroupEmployeeForStaffAsync(dto)
                .ThrowsAsync(new InvalidOperationException("Duplicate PactId."));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.CreateWorkGroupEmployeeForStaffAsync(req));
        }

        #endregion

        #region UpdateWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task UpdateWorkGroupEmployeeAsync_WithValidRequest_ReturnsOk()
        {
            // Arrange
            var req = new WorkGroupEmployeeReq { PactId = DefaultPactId, HrsPaid = 40.0 };
            var dto = new WorkGroupEmployeeDto { PactId = DefaultPactId };
            var updatedDto  = new WorkGroupEmployeeDto { PactId = DefaultPactId };
            var expectedRes = new WorkGroupEmployeeRes { PactId = DefaultPactId };

            _mapperMock.Map<WorkGroupEmployeeDto>(req).Returns(dto);
            _serviceMock.UpdateWorkGroupEmployeeAsync(dto).Returns(updatedDto);
            _mapperMock.Map<WorkGroupEmployeeRes>(updatedDto).Returns(expectedRes);

            // Act
            var result = await _controller.UpdateWorkGroupEmployeeAsync(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).UpdateWorkGroupEmployeeAsync(dto);
        }

        [Fact]
        public async Task UpdateWorkGroupEmployeeAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var req = new WorkGroupEmployeeReq { PactId = DefaultPactId };
            var dto = new WorkGroupEmployeeDto { PactId = DefaultPactId };

            _mapperMock.Map<WorkGroupEmployeeDto>(req).Returns(dto);
            _serviceMock.UpdateWorkGroupEmployeeAsync(dto)
                .ThrowsAsync(new KeyNotFoundException("Employee not found."));

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.UpdateWorkGroupEmployeeAsync(req));
        }

        [Fact]
        public async Task UpdateWorkGroupEmployeeForStaffAsync_WithValidRequest_ReturnsOk()
        {
            var req = new WorkGroupEmployeeReq { PactId = DefaultPactId, HrsPaid = 40.0 };
            var dto = new WorkGroupEmployeeDto { PactId = DefaultPactId };
            var updatedDto = new WorkGroupEmployeeDto { PactId = DefaultPactId };
            var expectedRes = new WorkGroupEmployeeRes { PactId = DefaultPactId };

            _mapperMock.Map<WorkGroupEmployeeDto>(req).Returns(dto);
            _serviceMock.UpdateWorkGroupEmployeeForStaffAsync(dto).Returns(updatedDto);
            _mapperMock.Map<WorkGroupEmployeeRes>(updatedDto).Returns(expectedRes);

            var result = await _controller.UpdateWorkGroupEmployeeForStaffAsync(req);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).UpdateWorkGroupEmployeeForStaffAsync(dto);
        }

        [Fact]
        public async Task UpdateWorkGroupEmployeeForStaffAsync_WhenServiceThrows_PropagatesException()
        {
            var req = new WorkGroupEmployeeReq { PactId = DefaultPactId };
            var dto = new WorkGroupEmployeeDto { PactId = DefaultPactId };

            _mapperMock.Map<WorkGroupEmployeeDto>(req).Returns(dto);
            _serviceMock.UpdateWorkGroupEmployeeForStaffAsync(dto)
                .ThrowsAsync(new KeyNotFoundException("Employee not found."));

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.UpdateWorkGroupEmployeeForStaffAsync(req));
        }

        #endregion

        #region DeleteWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task DeleteWorkGroupEmployeeAsync_WithValidPactId_ReturnsOk()
        {
            // Arrange
            _serviceMock.DeleteWorkGroupEmployeeAsync(DefaultPactId).Returns(true);

            // Act
            var result = await _controller.DeleteWorkGroupEmployeeAsync(DefaultPactId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, okResult.Value);
            await _serviceMock.Received(1).DeleteWorkGroupEmployeeAsync(DefaultPactId);
        }

        [Fact]
        public async Task DeleteWorkGroupEmployeeAsync_WhenNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.DeleteWorkGroupEmployeeAsync(DefaultPactId).Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _controller.DeleteWorkGroupEmployeeAsync(DefaultPactId));
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new WorkGroupEmployeeController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new WorkGroupEmployeeController(_serviceMock, null!));
        }

        #endregion
    }
}
