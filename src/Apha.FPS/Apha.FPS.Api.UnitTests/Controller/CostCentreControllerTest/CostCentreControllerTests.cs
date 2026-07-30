using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Api.Controllers;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPS.Api.UnitTests.Controller.CostCentreControllerTest
{
    public class CostCentreControllerTests
    {
        private readonly ICostCentreService _costCentreServiceMock;
        private readonly IStoredProcRepository _repositoryMock;
        private readonly IFpsRequestContext _fpsRequestContextMock;
        private readonly IMapper _mapperMock;
        private readonly CostCentreController _controller;

        public CostCentreControllerTests()
        {
            _costCentreServiceMock = Substitute.For<ICostCentreService>();
            _repositoryMock = Substitute.For<IStoredProcRepository>();
            _fpsRequestContextMock = Substitute.For<IFpsRequestContext>();
            _mapperMock = Substitute.For<IMapper>();
            _fpsRequestContextMock.FpsYear.Returns(2024);
            _controller = new CostCentreController(
                _costCentreServiceMock,
                _repositoryMock,
                _fpsRequestContextMock,
                _mapperMock);
        }

        // ─── Constructor Null-Guard Tests ──────────────────────────────────────────

        [Fact]
        public void Constructor_WithNullCostCentreService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new CostCentreController(null!, _repositoryMock, _fpsRequestContextMock, _mapperMock));
        }

        [Fact]
        public void Constructor_WithNullStoredProcRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new CostCentreController(_costCentreServiceMock, null!, _fpsRequestContextMock, _mapperMock));
        }

        [Fact]
        public void Constructor_WithNullFpsRequestContext_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new CostCentreController(_costCentreServiceMock, _repositoryMock, null!, _mapperMock));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new CostCentreController(_costCentreServiceMock, _repositoryMock, _fpsRequestContextMock, null!));
        }

        // ─── Workgroup Lookup (existing tests — preserved) ─────────────────────────

        [Fact]
        public async Task GetAllCostCentresAsync_HappyPath_ReturnsOk()
        {
            var serviceResult = new List<CostCentreWorkgroup> { new() { CostCentre = 100, ProfitCentre = "PC1" } };
            var mappedResult = new List<CostCentreWorkgroupRes> { new() { CostCentre = 100, ProfitCentre = "PC1" } };

            _repositoryMock.GetAllCostCentreWorkgroupAsync().Returns(serviceResult);
            _mapperMock.Map<IEnumerable<CostCentreWorkgroupRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetAllCostCentresAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(mappedResult, okResult.Value);
        }

        [Fact]
        public async Task GetAllCostCentresAsync_EmptyList_ReturnsOk()
        {
            var serviceResult = new List<CostCentreWorkgroup>();
            var mappedResult = new List<CostCentreWorkgroupRes>();

            _repositoryMock.GetAllCostCentreWorkgroupAsync().Returns(serviceResult);
            _mapperMock.Map<IEnumerable<CostCentreWorkgroupRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetAllCostCentresAsync();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<CostCentreWorkgroupRes>>(okResult.Value);
            Assert.Empty(value);
        }

        [Fact]
        public async Task GetAllCostCentresAsync_RepositoryThrows_PropagatesException()
        {
            _repositoryMock.GetAllCostCentreWorkgroupAsync().Throws(new Exception("Repository error"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetAllCostCentresAsync());
        }

        [Fact]
        public async Task GetAllCostCentresAsync_MapperThrows_PropagatesException()
        {
            var serviceResult = new List<CostCentreWorkgroup> { new() { CostCentre = 100 } };
            _repositoryMock.GetAllCostCentreWorkgroupAsync().Returns(serviceResult);
            _mapperMock.Map<IEnumerable<CostCentreWorkgroupRes>>(serviceResult).Throws(new AutoMapperMappingException("Mapping error"));

            await Assert.ThrowsAsync<AutoMapperMappingException>(() => _controller.GetAllCostCentresAsync());
        }

        // ─── CRUD Endpoint Tests ────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllCostCentresPagedAsync_HappyPath_ReturnsOk()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<CostCentreDto>(
                new List<CostCentreDto> { new() { CostCentreNo = 100, ProfitCentre = "PC1", FpsYear = 2024 } },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 });
            var mappedResult = new PaginationRes<CostCentreRes>
            {
                Data = new List<CostCentreRes> { new() { CostCentreNo = 100, ProfitCentre = "PC1", FpsYear = 2024 } }
            };

            _costCentreServiceMock.GetAllCostCentresPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<CostCentreRes>>(serviceResult).Returns(mappedResult);

            var result = await _controller.GetAllCostCentresPagedAsync(query) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(mappedResult, result.Value);
        }

        [Fact]
        public async Task GetAllCostCentresPagedAsync_NullResult_ThrowsArgumentException()
        {
            var query = new QueryParameters<string>();
            _costCentreServiceMock.GetAllCostCentresPagedAsync(query).Returns(Task.FromResult<PaginatedResult<CostCentreDto>>(null!));

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetAllCostCentresPagedAsync(query));
        }

        [Fact]
        public async Task GetCostCentreByIdAsync_HappyPath_ReturnsOk()
        {
            const double costCentreNo = 100.0;
            const int fpsYear = 2024;
            var dto = new CostCentreDto { CostCentreNo = costCentreNo, ProfitCentre = "PC1", FpsYear = fpsYear };
            var res = new CostCentreRes { CostCentreNo = costCentreNo, ProfitCentre = "PC1", FpsYear = fpsYear };

            _costCentreServiceMock.GetCostCentreByIdAsync(costCentreNo, fpsYear).Returns(dto);
            _mapperMock.Map<CostCentreRes>(dto).Returns(res);

            var result = await _controller.GetCostCentreByIdAsync(costCentreNo);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, okResult.Value);
        }

        [Fact]
        public async Task GetCostCentreByIdAsync_NotFound_ThrowsArgumentException()
        {
            const double costCentreNo = 999.0;
            _costCentreServiceMock.GetCostCentreByIdAsync(costCentreNo, 2024).Returns((CostCentreDto?)null);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetCostCentreByIdAsync(costCentreNo));
        }

        [Fact]
        public async Task CreateCostCentreAsync_HappyPath_ReturnsOk()
        {
            var request = new CostCentreReq { CostCentreNo = 200, ProfitCentre = "PC2" };
            var dto = new CostCentreDto { CostCentreNo = 200, ProfitCentre = "PC2", FpsYear = 2024 };
            var created = new CostCentreDto { CostCentreNo = 200, ProfitCentre = "PC2", FpsYear = 2024 };
            var res = new CostCentreRes { CostCentreNo = 200, ProfitCentre = "PC2", FpsYear = 2024 };

            _mapperMock.Map<CostCentreDto>(request).Returns(dto);
            _costCentreServiceMock.CreateCostCentreAsync(Arg.Is<CostCentreDto>(d => d.FpsYear == 2024 && d.CostCentreNo == 200)).Returns(created);
            _mapperMock.Map<CostCentreRes>(created).Returns(res);

            var result = await _controller.CreateCostCentreAsync(request);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, okResult.Value);
        }

        [Fact]
        public async Task UpdateCostCentreAsync_HappyPath_ReturnsOk()
        {
            const double costCentreNo = 100.0;
            var request = new CostCentreReq { CostCentreNo = 100, ProfitCentre = "PC3" };
            var dto = new CostCentreDto { CostCentreNo = 100, ProfitCentre = "PC3", FpsYear = 2024 };
            var updated = new CostCentreDto { CostCentreNo = 100, ProfitCentre = "PC3", FpsYear = 2024 };
            var res = new CostCentreRes { CostCentreNo = 100, ProfitCentre = "PC3", FpsYear = 2024 };

            _mapperMock.Map<CostCentreDto>(request).Returns(dto);
            _costCentreServiceMock.UpdateCostCentreAsync(costCentreNo, 2024, Arg.Any<CostCentreDto>()).Returns(updated);
            _mapperMock.Map<CostCentreRes>(updated).Returns(res);

            var result = await _controller.UpdateCostCentreAsync(costCentreNo, request);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(res, okResult.Value);
        }

        [Fact]
        public async Task DeleteCostCentreAsync_HappyPath_ReturnsOkTrue()
        {
            const double costCentreNo = 100.0;
            _costCentreServiceMock.DeleteCostCentreAsync(costCentreNo, 2024).Returns(true);

            var result = await _controller.DeleteCostCentreAsync(costCentreNo) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.True(Assert.IsType<bool>(result.Value));
        }

        [Fact]
        public async Task DeleteCostCentreAsync_NotFound_ThrowsArgumentException()
        {
            const double costCentreNo = 999.0;
            _costCentreServiceMock.DeleteCostCentreAsync(costCentreNo, 2024).Returns(false);

            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteCostCentreAsync(costCentreNo));
        }
    }
}
