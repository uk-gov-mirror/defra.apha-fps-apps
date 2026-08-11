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
namespace Apha.FPS.Api.UnitTests.Controller.EmployeeControllerTest
{
    public class EmployeeControllerTests
    {
        private readonly IEmployeeService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly EmployeeController _controller;

        public EmployeeControllerTests()
        {
            _serviceMock = Substitute.For<IEmployeeService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new EmployeeController(_serviceMock, _mapperMock);
        }

        #region GetFilteredEmployeesAsync (Paginated)

        [Fact]
        public async Task GetFilteredEmployeesAsync_Paginated_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new PaginationReq<string>();
            var serviceResult = new PaginatedResult<EmployeeDto>();
            var mappedResult = new PaginationRes<EmployeeRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetFilteredEmployeesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>()).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<EmployeeRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetFilteredEmployeesAsync(query, 1);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_Paginated_EdgeCase_EmptyResult()
        {
            // Arrange
            var query = new PaginationReq<string>();
            var serviceResult = new PaginatedResult<EmployeeDto>();
            var mappedResult = new PaginationRes<EmployeeRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetFilteredEmployeesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>()).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<EmployeeRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetFilteredEmployeesAsync(query, 2);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_Paginated_Error_ServiceThrows()
        {
            // Arrange
            var query = new PaginationReq<string>();
            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetFilteredEmployeesAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<int>()).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetFilteredEmployeesAsync(query, 1));
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_Paginated_Error_MapperThrows()
        {
            // Arrange
            var query = new PaginationReq<string>();
            _mapperMock.Map<QueryParameters<string>>(query).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetFilteredEmployeesAsync(query, 1));
        }

        #endregion

        #region GetFilteredEmployeesAsync (Non-Paginated)

        [Fact]
        public async Task GetFilteredEmployeesAsync_NonPaginated_HappyPath_ReturnsOk()
        {
            // Arrange
            var serviceResult = new List<EmployeeDto>();
            var mappedResult = new List<EmployeeRes>();

            _serviceMock.GetFilteredEmployeesAsync(Arg.Any<int>()).Returns(serviceResult);
            _mapperMock.Map<List<EmployeeRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetFilteredEmployeesAsync(1);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_NonPaginated_EdgeCase_EmptyList()
        {
            // Arrange
            var serviceResult = new List<EmployeeDto>();
            var mappedResult = new List<EmployeeRes>();

            _serviceMock.GetFilteredEmployeesAsync(Arg.Any<int>()).Returns(serviceResult);
            _mapperMock.Map<List<EmployeeRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetFilteredEmployeesAsync(3);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_NonPaginated_Error_ServiceThrows()
        {
            // Arrange
            _serviceMock.GetFilteredEmployeesAsync(Arg.Any<int>()).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetFilteredEmployeesAsync(1));
        }

        [Fact]
        public async Task GetFilteredEmployeesAsync_NonPaginated_Error_MapperThrows()
        {
            // Arrange
            var serviceResult = new List<EmployeeDto>();
            _serviceMock.GetFilteredEmployeesAsync(Arg.Any<int>()).Returns(serviceResult);
            _mapperMock.Map<List<EmployeeRes>>(serviceResult).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetFilteredEmployeesAsync(1));
        }

        #endregion

        #region GetEmployeeByIdAsync

        [Fact]
        public async Task GetEmployeeByIdAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var dto = new EmployeeDto { SPNumber = "SP001" };
            var mapped = new EmployeeRes();

            _serviceMock.GetEmployeeByIdAsync("SP001").Returns(dto);
            _mapperMock.Map<EmployeeRes>(dto).Returns(mapped);

            // Act
            var result = await _controller.GetEmployeeByIdAsync("SP001");

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_EdgeCase_NullResult_ReturnsNotFound()
        {
            // Arrange
            _serviceMock.GetEmployeeByIdAsync("SP999").Returns((EmployeeDto)null!);

            // Act
            var result = await _controller.GetEmployeeByIdAsync("SP999");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_Error_ServiceThrows()
        {
            // Arrange
            _serviceMock.GetEmployeeByIdAsync("SP001").Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetEmployeeByIdAsync("SP001"));
        }

        [Fact]
        public async Task GetEmployeeByIdAsync_Error_MapperThrows()
        {
            // Arrange
            var dto = new EmployeeDto { SPNumber = "SP001" };
            _serviceMock.GetEmployeeByIdAsync("SP001").Returns(dto);
            _mapperMock.Map<EmployeeRes>(dto).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetEmployeeByIdAsync("SP001"));
        }

        #endregion

        #region AddEmployeeAsync

        [Fact]
        public async Task AddEmployeeAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var req = new EmployeeReq { SPNumber = "SP001" };
            var dto = new EmployeeDto { SPNumber = "SP001" };
            var resultDto = new EmployeeDto { SPNumber = "SP001" };
            var mapped = new EmployeeRes();

            _mapperMock.Map<EmployeeDto>(req).Returns(dto);
            _serviceMock.AddEmployeeAsync(dto).Returns(resultDto);
            _mapperMock.Map<EmployeeRes>(resultDto).Returns(mapped);

            // Act
            var result = await _controller.AddEmployeeAsync(req);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task AddEmployeeAsync_EdgeCase_MinimalInput()
        {
            // Arrange
            var req = new EmployeeReq { SPNumber = "" };
            var dto = new EmployeeDto { SPNumber = "" };
            var resultDto = new EmployeeDto { SPNumber = "" };
            var mapped = new EmployeeRes();

            _mapperMock.Map<EmployeeDto>(req).Returns(dto);
            _serviceMock.AddEmployeeAsync(dto).Returns(resultDto);
            _mapperMock.Map<EmployeeRes>(resultDto).Returns(mapped);

            // Act
            var result = await _controller.AddEmployeeAsync(req);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task AddEmployeeAsync_Error_ServiceThrows()
        {
            // Arrange
            var req = new EmployeeReq { SPNumber = "SP001" };
            var dto = new EmployeeDto { SPNumber = "SP001" };

            _mapperMock.Map<EmployeeDto>(req).Returns(dto);
            _serviceMock.AddEmployeeAsync(dto).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.AddEmployeeAsync(req));
        }

        [Fact]
        public async Task AddEmployeeAsync_Error_MapperThrows()
        {
            // Arrange
            var req = new EmployeeReq { SPNumber = "SP001" };
            _mapperMock.Map<EmployeeDto>(req).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.AddEmployeeAsync(req));
        }

        #endregion

        #region UpdateEmployeeAsync

        [Fact]
        public async Task UpdateEmployeeAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var req = new EmployeeReq { SPNumber = "SP001" };
            var dto = new EmployeeDto { SPNumber = "SP001" };
            var resultDto = new EmployeeDto { SPNumber = "SP001" };
            var mapped = new EmployeeRes();

            _mapperMock.Map<EmployeeDto>(req).Returns(dto);
            _serviceMock.UpdateEmployeeAsync(dto).Returns(resultDto);
            _mapperMock.Map<EmployeeRes>(resultDto).Returns(mapped);

            // Act
            var result = await _controller.UpdateEmployeeAsync(req);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mapped, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_EdgeCase_MinimalInput()
        {
            // Arrange
            var req = new EmployeeReq { SPNumber = "" };
            var dto = new EmployeeDto { SPNumber = "" };
            var resultDto = new EmployeeDto { SPNumber = "" };
            var mapped = new EmployeeRes();

            _mapperMock.Map<EmployeeDto>(req).Returns(dto);
            _serviceMock.UpdateEmployeeAsync(dto).Returns(resultDto);
            _mapperMock.Map<EmployeeRes>(resultDto).Returns(mapped);

            // Act
            var result = await _controller.UpdateEmployeeAsync(req);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateEmployeeAsync_Error_ServiceThrows()
        {
            // Arrange
            var req = new EmployeeReq { SPNumber = "SP001" };
            var dto = new EmployeeDto { SPNumber = "SP001" };

            _mapperMock.Map<EmployeeDto>(req).Returns(dto);
            _serviceMock.UpdateEmployeeAsync(dto).Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateEmployeeAsync(req));
        }

        [Fact]
        public async Task UpdateEmployeeAsync_Error_MapperThrows()
        {
            // Arrange
            var req = new EmployeeReq { SPNumber = "SP001" };
            _mapperMock.Map<EmployeeDto>(req).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateEmployeeAsync(req));
        }

        #endregion

        #region DeleteEmployeeAsync

        [Fact]
        public async Task DeleteEmployeeAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            _serviceMock.DeleteEmployeeAsync("SP001").Returns(true);

            // Act
            var result = await _controller.DeleteEmployeeAsync("SP001");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteEmployeeAsync_EdgeCase_NotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            _serviceMock.DeleteEmployeeAsync("SP999").Returns(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteEmployeeAsync("SP999"));
        }

        [Fact]
        public async Task DeleteEmployeeAsync_Error_ServiceThrows()
        {
            // Arrange
            _serviceMock.DeleteEmployeeAsync("SP001").Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.DeleteEmployeeAsync("SP001"));
        }

        #endregion

        #region GetAllManagersAsync

        [Fact]
        public async Task GetAllManagersAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var serviceResult = new List<ManagerDto>();
            var mappedResult = new List<ManagerRes>();

            _serviceMock.GetAllManagersAsync().Returns(serviceResult);
            _mapperMock.Map<List<ManagerRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllManagersAsync();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetAllManagersAsync_EdgeCase_EmptyList()
        {
            // Arrange
            var serviceResult = new List<ManagerDto>();
            var mappedResult = new List<ManagerRes>();

            _serviceMock.GetAllManagersAsync().Returns(serviceResult);
            _mapperMock.Map<List<ManagerRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllManagersAsync();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAllManagersAsync_Error_ServiceThrows()
        {
            // Arrange
            _serviceMock.GetAllManagersAsync().Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllManagersAsync());
        }

        [Fact]
        public async Task GetAllManagersAsync_Error_MapperThrows()
        {
            // Arrange
            var serviceResult = new List<ManagerDto>();
            _serviceMock.GetAllManagersAsync().Returns(serviceResult);
            _mapperMock.Map<List<ManagerRes>>(serviceResult).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllManagersAsync());
        }

        #endregion

        #region GetAllWorkGroupPersonAsync

        [Fact]
        public async Task GetAllPersonAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var serviceResult = new List<WorkGroupPersonDto>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1", WorkGroup = "GroupA" },
                new() { Name = "Bob",   WorkGroupGrade = "WG2", WorkGroup = "GroupB" }
            };
            var mappedResult = new List<WorkGroupPersonRes>
            {
                new() { Name = "Alice" },
                new() { Name = "Bob" }
            };

            _serviceMock.GetAllWorkGroupPersonAsync().Returns(serviceResult);
            _mapperMock.Map<List<WorkGroupPersonRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllWorkGroupPersonAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetAllPersonAsync_EdgeCase_EmptyList_ReturnsOk()
        {
            // Arrange
            var serviceResult = new List<WorkGroupPersonDto>();
            var mappedResult = new List<WorkGroupPersonRes>();

            _serviceMock.GetAllWorkGroupPersonAsync().Returns(serviceResult);
            _mapperMock.Map<List<WorkGroupPersonRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllWorkGroupPersonAsync();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAllPersonAsync_Error_ServiceThrows()
        {
            // Arrange
            _serviceMock.GetAllWorkGroupPersonAsync().Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllWorkGroupPersonAsync());
        }

        [Fact]
        public async Task GetAllPersonAsync_Error_MapperThrows()
        {
            // Arrange
            var serviceResult = new List<WorkGroupPersonDto>();
            _serviceMock.GetAllWorkGroupPersonAsync().Returns(serviceResult);
            _mapperMock.Map<List<WorkGroupPersonRes>>(serviceResult).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllWorkGroupPersonAsync());
        }

        #endregion

        #region GetWorkGroupStaffPaginatedAsync

        [Fact]
        public async Task GetWorkGroupStaffPaginatedAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mappedQuery = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var people = new List<PactStaffDto>
            {
                new() { Name = "Alice", WorkGroupGrade = "WG1" }
            };
            var pagedResult = new PaginatedResult<PactStaffDto>(people, new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 });
            var mappedRes = new PaginationRes<PactStaffRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetPagedWorkGroupStaffAsync(mappedQuery, null).Returns(pagedResult);
            _mapperMock.Map<PaginationRes<PactStaffRes>>(pagedResult).Returns(mappedRes);

            // Act
            var result = await _controller.GetWorkGroupStaffPaginatedAsync(query, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedRes, okResult.Value);
        }

        [Fact]
        public async Task GetWorkGroupStaffPaginatedAsync_WithWorkGroup_PassesWorkGroupToService()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mappedQuery = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pagedResult = new PaginatedResult<PactStaffDto>([], new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 });
            var mappedRes = new PaginationRes<PactStaffRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetPagedWorkGroupStaffAsync(mappedQuery, "WG1").Returns(pagedResult);
            _mapperMock.Map<PaginationRes<PactStaffRes>>(pagedResult).Returns(mappedRes);

            // Act
            var result = await _controller.GetWorkGroupStaffPaginatedAsync(query, "WG1");

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).GetPagedWorkGroupStaffAsync(Arg.Any<QueryParameters<string>>(), "WG1");
        }

        [Fact]
        public async Task GetWorkGroupStaffPaginatedAsync_EdgeCase_EmptyResult_ReturnsOk()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            var mappedQuery = new QueryParameters<string>();
            var pagedResult = new PaginatedResult<PactStaffDto>([], new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0, TotalPages = 0 });
            var mappedRes = new PaginationRes<PactStaffRes>();

            _mapperMock.Map<QueryParameters<string>>(query).Returns(mappedQuery);
            _serviceMock.GetPagedWorkGroupStaffAsync(mappedQuery, null).Returns(pagedResult);
            _mapperMock.Map<PaginationRes<PactStaffRes>>(pagedResult).Returns(mappedRes);

            // Act
            var result = await _controller.GetWorkGroupStaffPaginatedAsync(query, null);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetWorkGroupStaffPaginatedAsync_Error_ServiceThrows()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            _mapperMock.Map<QueryParameters<string>>(query).Returns(new QueryParameters<string>());
            _serviceMock.GetPagedWorkGroupStaffAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string?>())
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetWorkGroupStaffPaginatedAsync(query, null));
        }

        [Fact]
        public async Task GetWorkGroupStaffPaginatedAsync_Error_MapperThrows()
        {
            // Arrange
            var query = new PaginationReq<string> { Page = 1, PageSize = 10 };
            _mapperMock.Map<QueryParameters<string>>(query).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetWorkGroupStaffPaginatedAsync(query, null));
        }

        #endregion

        #region GetAllPactStaffAsync

        [Fact]
        public async Task GetAllPactStaffAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var serviceResult = new List<PactStaffDto>();
            var mappedResult = new List<PactStaffRes>();

            _serviceMock.GetPactStaffAsync().Returns(serviceResult);
            _mapperMock.Map<List<PactStaffRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllPactStaffAsync();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, ((OkObjectResult)result).Value);
        }

        [Fact]
        public async Task GetAllPactStaffAsync_EdgeCase_EmptyList()
        {
            // Arrange
            var serviceResult = new List<PactStaffDto>();
            var mappedResult = new List<PactStaffRes>();

            _serviceMock.GetPactStaffAsync().Returns(serviceResult);
            _mapperMock.Map<List<PactStaffRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetAllPactStaffAsync();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetAllPactStaffAsync_Error_ServiceThrows()
        {
            // Arrange
            _serviceMock.GetPactStaffAsync().Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllPactStaffAsync());
        }

        [Fact]
        public async Task GetAllPactStaffAsync_Error_MapperThrows()
        {
            // Arrange
            var serviceResult = new List<PactStaffDto>();
            _serviceMock.GetPactStaffAsync().Returns(serviceResult);
            _mapperMock.Map<List<PactStaffRes>>(serviceResult).Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllPactStaffAsync());
        }

        #endregion

        #region GetPactWorkGroupStaffAsync

        [Fact]
        public async Task GetPactWorkGroupStaffAsync_WithWorkGroup_ReturnsOkResult()
        {
            // Arrange
            const string workGroup = "WG1";
            var serviceResult = new List<PactStaffDto>
            {
                new() { PactId = "P001", Name = "Alice", WorkGroupGrade = "WG1" }
            };
            var mappedResult = new List<PactStaffRes>
            {
                new() { PactId = "P001", Name = "Alice", WorkGroupGrade = "WG1" }
            };

            _serviceMock.GetPactWorkGroupStaffAsync(workGroup).Returns(serviceResult);
            _mapperMock.Map<List<PactStaffRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetPactWorkGroupStaffAsync(workGroup);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            await _serviceMock.Received(1).GetPactWorkGroupStaffAsync(workGroup);
        }

        [Fact]
        public async Task GetPactWorkGroupStaffAsync_WithNullWorkGroup_ReturnsOkResult()
        {
            // Arrange
            var serviceResult = new List<PactStaffDto>();
            var mappedResult = new List<PactStaffRes>();

            _serviceMock.GetPactWorkGroupStaffAsync(null).Returns(serviceResult);
            _mapperMock.Map<List<PactStaffRes>>(serviceResult).Returns(mappedResult);

            // Act
            var result = await _controller.GetPactWorkGroupStaffAsync(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            await _serviceMock.Received(1).GetPactWorkGroupStaffAsync(null);
        }

        [Fact]
        public async Task GetPactWorkGroupStaffAsync_WhenServiceThrows_ThrowsException()
        {
            // Arrange
            _serviceMock.GetPactWorkGroupStaffAsync(Arg.Any<string?>())
                .Throws(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetPactWorkGroupStaffAsync("WG1"));
        }

        #endregion
    }
}
