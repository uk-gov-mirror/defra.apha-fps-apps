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

namespace Apha.FPS.Api.UnitTests.Controller.ProgramControllerTest
{
    public class ProgramControllerTest
    {
        private readonly IProgramService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ProgramController _controller;

        public ProgramControllerTest()
        {
            _serviceMock = Substitute.For<IProgramService>();
            _mapperMock = Substitute.For<IMapper>();
            _controller = new ProgramController(_serviceMock, _mapperMock);
        }

        #region GetAllProgramsAsync

        [Fact]
        public async Task GetAllProgramsAsync_HappyPath_ReturnsOk()
        {
            var serviceResult = new List<ProgramDto> { new ProgramDto { ProgramNo = "P1" } };
            var mappedResult = new List<ProgramRes> { new ProgramRes { ProgramNo = "P1" } };

            _serviceMock.GetAllProgramsAsync().Returns(serviceResult);
            _mapperMock.Map<List<ProgramRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetAllProgramsAsync();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]      
        public async Task GetAllProgramsAsync_NullResult_ThrowsArgumentException()
        {
            _serviceMock.GetAllProgramsAsync().Returns((List<ProgramDto>)null!);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetAllProgramsAsync());
        }

        #endregion

        #region GetAllProgramsForAllUsersAsync

        [Fact]
        public async Task GetAllProgramsForAllUsersAsync_HappyPath_ReturnsOk()
        {
            var serviceResult = new List<ProgramDto> { new ProgramDto { ProgramNo = "P1" } };
            var mappedResult = new List<ProgramRes> { new ProgramRes { ProgramNo = "P1" } };

            _serviceMock.GetAllProgramsForAllUsersAsync().Returns(serviceResult);
            _mapperMock.Map<List<ProgramRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetAllProgramsForAllUsersAsync();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetAllProgramsForAllUsersAsync_NullResult_ThrowsArgumentException()
        {
            _serviceMock.GetAllProgramsForAllUsersAsync().Returns((List<ProgramDto>)null!);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetAllProgramsForAllUsersAsync());
        }

        #endregion

        #region GetAllProgramsPagedAsync

        [Fact]
        public async Task GetAllProgramsPagedAsync_HappyPath_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            var programDtos = new List<ProgramDto>
            {
                new ProgramDto { ProgramNo = "P1", ProgramName = "Test Program" }
            };
            var paginationData = new PaginationDto
            {
                PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1
            };
            var serviceResult = new PaginatedResult<ProgramDto>(programDtos, paginationData);

            var expectedApiResponse = new PaginationRes<ProgramRes>
            {
                Data = new List<ProgramRes> { new ProgramRes { ProgramNo = "P1", ProgramName = "Test Program" } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };

            _serviceMock.GetAllProgramsAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProgramRes>>(serviceResult).Returns(expectedApiResponse);

            var result = await _controller.GetAllProgramsPagedAsync(query);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedApiResponse, okResult.Value);
        }

        [Fact]
        public async Task GetAllProgramsPagedAsync_NullResult_ThrowsArgumentException()
        {
            var query = new QueryParameters<string>();
            _serviceMock.GetAllProgramsAsync(query).Returns((PaginatedResult<ProgramDto>)null!);            

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetAllProgramsPagedAsync(query));
        }

        #endregion

        #region GetProgramTimeSnapshotAsync

        [Fact]
        public async Task GetProgramTimeSnapshotAsync_HappyPath_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            var planCostDtos = new List<ProgramPlanCostDto>
            {
                new ProgramPlanCostDto { Program = "P1", HoursCost = 1000m }
            };
            var paginationData = new PaginationDto
            {
                PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1
            };
            var serviceResult = new PaginatedResult<ProgramPlanCostDto>(planCostDtos, paginationData);

            var expectedApiResponse = new PaginationRes<ProgramPlanCostRes>
            {
                Data = new List<ProgramPlanCostRes> { new ProgramPlanCostRes { Program = "P1", HoursCost = 1000m } },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1, TotalPages = 1 }
            };

            _serviceMock.GetProgramTimeSnapshotAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProgramPlanCostRes>>(serviceResult).Returns(expectedApiResponse);

            var result = await _controller.GetProgramTimeSnapshotAsync(query);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedApiResponse, okResult.Value);
        }

        [Fact]
        public async Task GetProgramTimeSnapshotAsync_NullResult_ThrowsArgumentException()
        {
            var query = new QueryParameters<string>();
            _serviceMock.GetProgramTimeSnapshotAsync(query).Returns((PaginatedResult<ProgramPlanCostDto>)null!);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetProgramTimeSnapshotAsync(query));
        }

        #endregion

        #region GetProgramById

        [Fact]
        public async Task GetProgramById_HappyPath_ReturnsOk()
        {
            var dto = new ProgramDto { ProgramNo = "P1" };
            var mapped = new ProgramRes { ProgramNo = "P1" };

            _serviceMock.GetProgramByIdAsync("P1").Returns(dto);
            _mapperMock.Map<ProgramRes>(dto).Returns(mapped);

            var result = await _controller.GetProgramById("P1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task GetProgramById_NullResult_ThrowsArgumentException()
        {
            _serviceMock.GetProgramByIdAsync("P2").Returns((ProgramDto?)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetProgramById("P2"));
        }

        #endregion

        #region CreateProgram

        [Fact]
        public async Task CreateProgram_HappyPath_ReturnsOk()
        {
            var req = new ProgramReq { ProgramNo = "P1" };
            var dto = new ProgramDto { ProgramNo = "P1" };
            var resultDto = new ProgramDto { ProgramNo = "P1" };
            var mapped = new ProgramRes { ProgramNo = "P1" };

            _mapperMock.Map<ProgramDto>(req).Returns(dto);
            _serviceMock.AddProgramAsync(dto).Returns(resultDto);
            _mapperMock.Map<ProgramRes>(resultDto).Returns(mapped);

            var result = await _controller.CreateProgram(req);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task CreateProgram_Error_ServiceThrows()
        {
            var req = new ProgramReq { ProgramNo = "P1" };
            var dto = new ProgramDto { ProgramNo = "P1" };

            _mapperMock.Map<ProgramDto>(req).Returns(dto);
            _serviceMock.AddProgramAsync(dto).Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.CreateProgram(req));
        }

        #endregion

        #region UpdateProgram

        [Fact]
        public async Task UpdateProgram_HappyPath_ReturnsOk()
        {
            var req = new ProgramReq { ProgramNo = "P1" };
            var dto = new ProgramDto { ProgramNo = "P1" };
            var resultDto = new ProgramDto { ProgramNo = "P1" };
            var mapped = new ProgramRes { ProgramNo = "P1" };

            _mapperMock.Map<ProgramDto>(req).Returns(dto);
            _serviceMock.UpdateProgramAsync(dto).Returns(resultDto);
            _mapperMock.Map<ProgramRes>(resultDto).Returns(mapped);

            var result = await _controller.UpdateProgram(req);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mapped, okResult.Value);
        }

        [Fact]
        public async Task UpdateProgram_Error_ServiceThrows()
        {
            var req = new ProgramReq { ProgramNo = "P1" };
            var dto = new ProgramDto { ProgramNo = "P1" };

            _mapperMock.Map<ProgramDto>(req).Returns(dto);
            _serviceMock.UpdateProgramAsync(dto).Throws(new Exception("Service error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.UpdateProgram(req));
        }

        #endregion

        #region DeleteProgram

        [Fact]
        public async Task DeleteProgram_HappyPath_ReturnsOk()
        {
            _serviceMock.DeleteProgramAsync("P1").Returns(true);

            var result = await _controller.DeleteProgram("P1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task DeleteProgram_NullOrEmpty_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteProgram(""));
        }

        [Fact]
        public async Task DeleteProgram_NotFound_ThrowsArgumentException()
        {
            _serviceMock.DeleteProgramAsync("P2").Returns(false);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteProgram("P2"));
        }

        #endregion
    }
}