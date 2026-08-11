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

namespace Apha.FPS.Api.UnitTests.Controller.ProfitCentreControllerTest
{
    public class ProfitCentreControllerTests
    {
        private readonly IProfitCentreService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ProfitCentreController _controller;

        public ProfitCentreControllerTests()
        {
            _serviceMock = Substitute.For<IProfitCentreService>();
            _mapperMock  = Substitute.For<IMapper>();
            _controller  = new ProfitCentreController(_serviceMock, _mapperMock);
        }

        private static ProfitCentreDto BuildDto(string id = "PC01") =>
            new() { ProfitCentreId = id, ProfitCentreName = "Centre One", Division = "DIV1" };

        private static ProfitCentreReq BuildReq(string id = "PC01") =>
            new() { ProfitCentreId = id, ProfitCentreName = "Centre One", Division = "DIV1" };

        private static ProfitCentreRes BuildRes(string id = "PC01") =>
            new() { ProfitCentreId = id, ProfitCentreName = "Centre One", Division = "DIV1" };

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ProfitCentreController(null!, _mapperMock));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ProfitCentreController(_serviceMock, null!));
        }

        #endregion

        #region GetProfitCentresAsync Tests

        [Fact]
        public async Task GetProfitCentresAsync_WithValidData_ReturnsOk()
        {
            // Arrange
            var dtos = new List<ProfitCentreDto>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Profit Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Profit Centre Two" }
            };
            var expectedRes = new List<ProfitCentreRes>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Profit Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Profit Centre Two" }
            };

            _serviceMock.GetProfitCentresAsync().Returns(dtos);
            _mapperMock.Map<List<ProfitCentreRes>>(dtos).Returns(expectedRes);

            // Act
            var result = await _controller.GetProfitCentresAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(expectedRes);
            await _serviceMock.Received(1).GetProfitCentresAsync();
        }

        [Fact]
        public async Task GetProfitCentresAsync_WithEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos        = new List<ProfitCentreDto>();
            var expectedRes = new List<ProfitCentreRes>();

            _serviceMock.GetProfitCentresAsync().Returns(dtos);
            _mapperMock.Map<List<ProfitCentreRes>>(dtos).Returns(expectedRes);

            // Act
            var result = await _controller.GetProfitCentresAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().BeEquivalentTo(expectedRes);
        }

        [Fact]
        public async Task GetProfitCentresAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetProfitCentresAsync()
                .ThrowsAsync(new InvalidOperationException("Service failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.GetProfitCentresAsync());
        }

        #endregion

        #region GetAllProfitCentres Tests

        [Fact]
        public async Task GetAllProfitCentres_WithValidData_ReturnsOkWithMappedList()
        {
            // Arrange
            var dtos = new List<ProfitCentreDto>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Centre Two" }
            };
            var mapped = new List<ProfitCentreRes>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Centre Two" }
            };

            _serviceMock.GetAllProfitCentresAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<ProfitCentreRes>>(dtos).Returns(mapped);

            // Act
            var result = await _controller.GetAllProfitCentres();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().Be(mapped);
            await _serviceMock.Received(1).GetAllProfitCentresAsync();
        }

        [Fact]
        public async Task GetAllProfitCentres_WithEmptyList_ReturnsOkWithEmptyList()
        {
            // Arrange
            var dtos = new List<ProfitCentreDto>();
            var mapped = new List<ProfitCentreRes>();

            _serviceMock.GetAllProfitCentresAsync().Returns(dtos);
            _mapperMock.Map<IEnumerable<ProfitCentreRes>>(dtos).Returns(mapped);

            // Act
            var result = await _controller.GetAllProfitCentres();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().BeEquivalentTo(mapped);
        }

        [Fact]
        public async Task GetAllProfitCentres_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.GetAllProfitCentresAsync()
                .ThrowsAsync(new InvalidOperationException("Service failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetAllProfitCentres());
        }

        #endregion

        #region GetAllProfitCentresPagedAsync Tests

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos  = new List<ProfitCentreDto> { BuildDto() };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var serviceResult = new PaginatedResult<ProfitCentreDto>(dtos, pagination);
            var expectedResponse = new PaginationRes<ProfitCentreRes>
            {
                Data = new List<ProfitCentreRes> { BuildRes() },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 1 }
            };

            _serviceMock.GetAllProfitCentresPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProfitCentreRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetAllProfitCentresPagedAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResponse, okResult.Value);
            await _serviceMock.Received(1).GetAllProfitCentresPagedAsync(query);
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_NullResult_ThrowsArgumentException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _serviceMock.GetAllProfitCentresPagedAsync(query).Returns((PaginatedResult<ProfitCentreDto>)null!);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.GetAllProfitCentresPagedAsync(query));
        }

        [Fact]
        public async Task GetAllProfitCentresPagedAsync_WithSortingAndFilter_ReturnsOk()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page = 2, PageSize = 5, SortBy = "ProfitCentreId", Descending = true
            };
            var dtos       = new List<ProfitCentreDto> { BuildDto() };
            var pagination = new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 10 };
            var serviceResult = new PaginatedResult<ProfitCentreDto>(dtos, pagination);
            var expectedResponse = new PaginationRes<ProfitCentreRes>
            {
                Data = new List<ProfitCentreRes> { BuildRes() },
                PaginationData = new Pagination { PageNumber = 2, PageSize = 5, TotalRecords = 10 }
            };

            _serviceMock.GetAllProfitCentresPagedAsync(query).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProfitCentreRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetAllProfitCentresPagedAsync(query);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<ProfitCentreRes>>(okResult.Value);
            Assert.Equal(2, response.PaginationData.PageNumber);
        }

        #endregion

        #region GetProfitCentreByIdAsync Tests

        [Fact]
        public async Task GetProfitCentreByIdAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var dto = BuildDto("PC01");
            var res = BuildRes("PC01");

            _serviceMock.GetProfitCentreByIdAsync("PC01").Returns(dto);
            _mapperMock.Map<ProfitCentreRes>(dto).Returns(res);

            // Act
            var result = await _controller.GetProfitCentreByIdAsync("PC01");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);
            await _serviceMock.Received(1).GetProfitCentreByIdAsync("PC01");
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_NullResult_ThrowsArgumentException()
        {
            // Arrange
            _serviceMock.GetProfitCentreByIdAsync("NOTEXIST").Returns((ProfitCentreDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.GetProfitCentreByIdAsync("NOTEXIST"));
            Assert.Contains("NOTEXIST", exception.Message);
        }

        #endregion

        #region CreateProfitCentreAsync Tests

        [Fact]
        public async Task CreateProfitCentreAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var req     = BuildReq("PC01");
            var dto     = BuildDto("PC01");
            var created = BuildDto("PC01");
            var res     = BuildRes("PC01");

            _mapperMock.Map<ProfitCentreDto>(req).Returns(dto);
            _serviceMock.CreateProfitCentreAsync(dto).Returns(created);
            _mapperMock.Map<ProfitCentreRes>(created).Returns(res);

            // Act
            var result = await _controller.CreateProfitCentreAsync(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);
            await _serviceMock.Received(1).CreateProfitCentreAsync(dto);
        }

        [Fact]
        public async Task CreateProfitCentreAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var req = BuildReq();
            var dto = BuildDto();

            _mapperMock.Map<ProfitCentreDto>(req).Returns(dto);
            _serviceMock.CreateProfitCentreAsync(dto)
                .ThrowsAsync(new InvalidOperationException("already exists"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.CreateProfitCentreAsync(req));
        }

        #endregion

        #region UpdateProfitCentreAsync Tests

        [Fact]
        public async Task UpdateProfitCentreAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            var req     = BuildReq("PC01");
            var dto     = BuildDto("PC01");
            var updated = BuildDto("PC01");
            var res     = BuildRes("PC01");

            _mapperMock.Map<ProfitCentreDto>(req).Returns(dto);
            _serviceMock.UpdateProfitCentreAsync("PC01", dto).Returns(updated);
            _mapperMock.Map<ProfitCentreRes>(updated).Returns(res);

            // Act
            var result = await _controller.UpdateProfitCentreAsync("PC01", req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, okResult.Value);
            await _serviceMock.Received(1).UpdateProfitCentreAsync("PC01", dto);
        }

        [Fact]
        public async Task UpdateProfitCentreAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var req = BuildReq();
            var dto = BuildDto();

            _mapperMock.Map<ProfitCentreDto>(req).Returns(dto);
            _serviceMock.UpdateProfitCentreAsync("PC01", dto)
                .ThrowsAsync(new InvalidOperationException("not found"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.UpdateProfitCentreAsync("PC01", req));
        }

        #endregion

        #region DeleteProfitCentreAsync Tests

        [Fact]
        public async Task DeleteProfitCentreAsync_HappyPath_ReturnsOk()
        {
            // Arrange
            _serviceMock.DeleteProfitCentreAsync("PC01").Returns(true);

            // Act
            var result = await _controller.DeleteProfitCentreAsync("PC01");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).DeleteProfitCentreAsync("PC01");
        }

        [Fact]
        public async Task DeleteProfitCentreAsync_WithNullOrWhitespace_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteProfitCentreAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => _controller.DeleteProfitCentreAsync("   "));
        }

        [Fact]
        public async Task DeleteProfitCentreAsync_WhenNotFound_ThrowsArgumentException()
        {
            // Arrange
            _serviceMock.DeleteProfitCentreAsync("NOTEXIST").Returns(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _controller.DeleteProfitCentreAsync("NOTEXIST"));
            Assert.Contains("NOTEXIST", exception.Message);
        }

        [Fact]
        public async Task DeleteProfitCentreAsync_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            _serviceMock.DeleteProfitCentreAsync("PC01")
                .ThrowsAsync(new InvalidOperationException("in use"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.DeleteProfitCentreAsync("PC01"));
        }

        #endregion

        #region PatchSettings Tests

        [Fact]
        public async Task PatchSettings_WithValidRequest_ReturnsOkWithTrue()
        {
            // Arrange
            var request = new UpdateProfitCentreSettingsReq
            {
                ProfitCentre = "PC01",
                Timesheet = -1,
                Outputsheet = -1,
                TimesheetLayout = 1
            };

            _serviceMock.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1).Returns(true);

            // Act
            var result = await _controller.PatchSettings(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
            await _serviceMock.Received(1).UpdateProfitCentreSettingsAsync("PC01", -1, -1, 1);
        }

        [Fact]
        public async Task PatchSettings_WithNullOrEmptyProfitCentre_ReturnsBadRequest()
        {
            // Arrange
            var request = new UpdateProfitCentreSettingsReq { ProfitCentre = "" };

            // Act
            var result = await _controller.PatchSettings(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            await _serviceMock.DidNotReceive()
                .UpdateProfitCentreSettingsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<short>());
        }

        [Fact]
        public async Task PatchSettings_WithWhitespaceProfitCentre_ReturnsBadRequest()
        {
            // Arrange
            var request = new UpdateProfitCentreSettingsReq { ProfitCentre = "   " };

            // Act
            var result = await _controller.PatchSettings(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task PatchSettings_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var request = new UpdateProfitCentreSettingsReq { ProfitCentre = "PC01" };
            _serviceMock.UpdateProfitCentreSettingsAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<short>())
                .ThrowsAsync(new InvalidOperationException("Service failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.PatchSettings(request));
        }

        #endregion

        #region GetPagedProfitCenterCostSummary Tests

        [Fact]
        public async Task GetPagedProfitCenterCostSummary_WithMonthNumber_ReturnsOkWithPagedData()
        {
            // Arrange
            const double monthNumber = 1.0;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<ProfitCentreCostDto>
            {
                new() { ProfitCentre = "PC01", Cost = 1000.00m },
                new() { ProfitCentre = "PC02", Cost = 2000.00m }
            };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 };
            var serviceResult = new PaginatedResult<ProfitCentreCostDto>(dtos, pagination);
            var expectedResponse = new PaginationRes<ProfitCentreCostRes>
            {
                Data = new List<ProfitCentreCostRes>
                {
                    new() { ProfitCentre = "PC01", Cost = 1000.00m },
                    new() { ProfitCentre = "PC02", Cost = 2000.00m }
                },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };

            _serviceMock.GetPagedProfitCenterCostSummaryAsync(query, monthNumber).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProfitCentreCostRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPagedProfitCenterCostSummary(query, monthNumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<ProfitCentreCostRes>>(okResult.Value);
            response.Data.Should().HaveCount(2);
            response.PaginationData.TotalRecords.Should().Be(2);
            await _serviceMock.Received(1).GetPagedProfitCenterCostSummaryAsync(query, monthNumber);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummary_WithMonthNumber_ReturnsOkWithFilteredPagedData()
        {
            // Arrange
            const double monthNumber = 3.0;
            var query = new QueryParameters<string> { Page = 1, PageSize = 5 };
            var dtos = new List<ProfitCentreCostDto>
            {
                new() { ProfitCentre = "PC01", Cost = 1500.00m }
            };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 5, TotalRecords = 1 };
            var serviceResult = new PaginatedResult<ProfitCentreCostDto>(dtos, pagination);
            var expectedResponse = new PaginationRes<ProfitCentreCostRes>
            {
                Data = new List<ProfitCentreCostRes>
                {
                    new() { ProfitCentre = "PC01", Cost = 1500.00m }
                },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 5, TotalRecords = 1 }
            };

            _serviceMock.GetPagedProfitCenterCostSummaryAsync(query, monthNumber).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProfitCentreCostRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPagedProfitCenterCostSummary(query, monthNumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<ProfitCentreCostRes>>(okResult.Value);
            response.Data.Should().HaveCount(1);
            response.PaginationData.TotalRecords.Should().Be(1);
            await _serviceMock.Received(1).GetPagedProfitCenterCostSummaryAsync(query, monthNumber);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummary_WithSortingAndPaging_ReturnsOkWithSortedData()
        {
            // Arrange
            const double monthNumber = 6.0;
            var query = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 5,
                SortBy = "ProfitCentre",
                Descending = true
            };
            var dtos = new List<ProfitCentreCostDto>
            {
                new() { ProfitCentre = "PC05", Cost = 500.00m },
                new() { ProfitCentre = "PC04", Cost = 400.00m }
            };
            var pagination = new PaginationDto { PageNumber = 2, PageSize = 5, TotalRecords = 10 };
            var serviceResult = new PaginatedResult<ProfitCentreCostDto>(dtos, pagination);
            var expectedResponse = new PaginationRes<ProfitCentreCostRes>
            {
                Data = new List<ProfitCentreCostRes>
                {
                    new() { ProfitCentre = "PC05", Cost = 500.00m },
                    new() { ProfitCentre = "PC04", Cost = 400.00m }
                },
                PaginationData = new Pagination { PageNumber = 2, PageSize = 5, TotalRecords = 10 }
            };

            _serviceMock.GetPagedProfitCenterCostSummaryAsync(query, monthNumber).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProfitCentreCostRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPagedProfitCenterCostSummary(query, monthNumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<ProfitCentreCostRes>>(okResult.Value);
            response.PaginationData.PageNumber.Should().Be(2);
            response.PaginationData.PageSize.Should().Be(5);
            response.PaginationData.TotalRecords.Should().Be(10);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummary_WithEmptyResult_ReturnsOkWithEmptyPagedData()
        {
            // Arrange
            const double monthNumber = 1.0;
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<ProfitCentreCostDto>();
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 };
            var serviceResult = new PaginatedResult<ProfitCentreCostDto>(dtos, pagination);
            var expectedResponse = new PaginationRes<ProfitCentreCostRes>
            {
                Data = new List<ProfitCentreCostRes>(),
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };

            _serviceMock.GetPagedProfitCenterCostSummaryAsync(query, monthNumber).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProfitCentreCostRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPagedProfitCenterCostSummary(query, monthNumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<ProfitCentreCostRes>>(okResult.Value);
            response.Data.Should().BeEmpty();
            response.PaginationData.TotalRecords.Should().Be(0);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummary_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _serviceMock.GetPagedProfitCenterCostSummaryAsync(query, Arg.Any<double>())
                .ThrowsAsync(new InvalidOperationException("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.GetPagedProfitCenterCostSummary(query, 1.0));
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummary_WithLargePageNumber_ReturnsOkWithEmptyPage()
        {
            // Arrange
            const double monthNumber = 1.0;
            var query = new QueryParameters<string> { Page = 999, PageSize = 10 };
            var dtos = new List<ProfitCentreCostDto>();
            var pagination = new PaginationDto { PageNumber = 999, PageSize = 10, TotalRecords = 50 };
            var serviceResult = new PaginatedResult<ProfitCentreCostDto>(dtos, pagination);
            var expectedResponse = new PaginationRes<ProfitCentreCostRes>
            {
                Data = new List<ProfitCentreCostRes>(),
                PaginationData = new Pagination { PageNumber = 999, PageSize = 10, TotalRecords = 50 }
            };

            _serviceMock.GetPagedProfitCenterCostSummaryAsync(query, monthNumber).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProfitCentreCostRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPagedProfitCenterCostSummary(query, monthNumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<ProfitCentreCostRes>>(okResult.Value);
            response.Data.Should().BeEmpty();
            response.PaginationData.PageNumber.Should().Be(999);
        }

        [Fact]
        public async Task GetPagedProfitCenterCostSummary_WithMinimumPageSize_ReturnsOkWithSingleItem()
        {
            // Arrange
            const double monthNumber = 1.0;
            var query = new QueryParameters<string> { Page = 1, PageSize = 1 };
            var dtos = new List<ProfitCentreCostDto>
            {
                new() { ProfitCentre = "PC01", Cost = 1000.00m }
            };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 1, TotalRecords = 10 };
            var serviceResult = new PaginatedResult<ProfitCentreCostDto>(dtos, pagination);
            var expectedResponse = new PaginationRes<ProfitCentreCostRes>
            {
                Data = new List<ProfitCentreCostRes>
                {
                    new() { ProfitCentre = "PC01", Cost = 1000.00m }
                },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 1, TotalRecords = 10 }
            };

            _serviceMock.GetPagedProfitCenterCostSummaryAsync(query, monthNumber).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<ProfitCentreCostRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPagedProfitCenterCostSummary(query, monthNumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<ProfitCentreCostRes>>(okResult.Value);
            response.Data.Should().HaveCount(1);
            response.PaginationData.PageSize.Should().Be(1);
        }

        #endregion

        #region GetPagedWgStaffPlan Tests

        private static WgStaffPlanViewDto BuildWgStaffPlanDto(string workGroup = "WG001", string name = "Staff One") =>
            new()
            {
                WorkGroup = workGroup,
                GradeCode = "G1",
                Name = name,
                Manager = "Manager01",
                Program = "PROG01",
                JobCode = "JOB001",
                ProjectStatus = "Active",
                PlannedHours = 40.0,
                Fee = 1000m
            };

        private static WgStaffPlanViewRes BuildWgStaffPlanRes(string workGroup = "WG001", string name = "Staff One") =>
            new()
            {
                WorkGroup = workGroup,
                GradeCode = "G1",
                Name = name,
                Manager = "Manager01",
                Program = "PROG01",
                JobCode = "JOB001",
                ProjectStatus = "Active",
                PlannedHours = 40.0,
                Fee = 1000m
            };

        [Fact]
        public async Task GetPagedWgStaffPlan_WithValidData_ReturnsOkWithPaginatedResponse()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<WgStaffPlanViewDto>
            {
                BuildWgStaffPlanDto(workGroup, "Staff One"),
                BuildWgStaffPlanDto(workGroup, "Staff Two")
            };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 };
            var serviceResult = new PaginatedResult<WgStaffPlanViewDto>(dtos, pagination);
            var expectedResponse = new PaginationRes<WgStaffPlanViewRes>
            {
                Data = new List<WgStaffPlanViewRes>
                {
                    BuildWgStaffPlanRes(workGroup, "Staff One"),
                    BuildWgStaffPlanRes(workGroup, "Staff Two")
                },
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };

            _serviceMock.GetPagedWgStaffPlanAsync(query, workGroup).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WgStaffPlanViewRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPagedWgStaffPlan(query, workGroup);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<WgStaffPlanViewRes>>(okResult.Value);
            response.Data.Should().HaveCount(2);
            response.PaginationData.TotalRecords.Should().Be(2);
            await _serviceMock.Received(1).GetPagedWgStaffPlanAsync(query, workGroup);
        }

        [Fact]
        public async Task GetPagedWgStaffPlan_WithEmptyResult_ReturnsOkWithEmptyList()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WgStaffPlanViewDto>(
                new List<WgStaffPlanViewDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });
            var expectedResponse = new PaginationRes<WgStaffPlanViewRes>
            {
                Data = new List<WgStaffPlanViewRes>(),
                PaginationData = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };

            _serviceMock.GetPagedWgStaffPlanAsync(query, workGroup).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WgStaffPlanViewRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPagedWgStaffPlan(query, workGroup);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<WgStaffPlanViewRes>>(okResult.Value);
            response.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPagedWgStaffPlan_WhenServiceThrows_PropagatesException()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _serviceMock.GetPagedWgStaffPlanAsync(query, workGroup)
                .ThrowsAsync(new InvalidOperationException("Service failure"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _controller.GetPagedWgStaffPlan(query, workGroup));
        }

        [Fact]
        public async Task GetPagedWgStaffPlan_PassesCorrectWorkGroupToService()
        {
            // Arrange
            const string workGroup = "WG-SPECIAL-001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResult = new PaginatedResult<WgStaffPlanViewDto>(
                new List<WgStaffPlanViewDto>(),
                new PaginationDto());
            var expectedResponse = new PaginationRes<WgStaffPlanViewRes>
            {
                Data = new List<WgStaffPlanViewRes>(),
                PaginationData = new Pagination()
            };

            _serviceMock.GetPagedWgStaffPlanAsync(query, workGroup).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WgStaffPlanViewRes>>(serviceResult).Returns(expectedResponse);

            // Act
            await _controller.GetPagedWgStaffPlan(query, workGroup);

            // Assert
            await _serviceMock.Received(1).GetPagedWgStaffPlanAsync(
                Arg.Any<QueryParameters<string>>(),
                Arg.Is<string>(wg => wg == workGroup));
        }

        [Fact]
        public async Task GetPagedWgStaffPlan_PassesCorrectQueryToService()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string>
            {
                Page = 2,
                PageSize = 20,
                SortBy = "Name",
                Descending = true
            };
            var serviceResult = new PaginatedResult<WgStaffPlanViewDto>(
                new List<WgStaffPlanViewDto>(),
                new PaginationDto { PageNumber = 2, PageSize = 20 });
            var expectedResponse = new PaginationRes<WgStaffPlanViewRes>
            {
                Data = new List<WgStaffPlanViewRes>(),
                PaginationData = new Pagination { PageNumber = 2, PageSize = 20 }
            };

            _serviceMock.GetPagedWgStaffPlanAsync(query, workGroup).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WgStaffPlanViewRes>>(serviceResult).Returns(expectedResponse);

            // Act
            await _controller.GetPagedWgStaffPlan(query, workGroup);

            // Assert
            await _serviceMock.Received(1).GetPagedWgStaffPlanAsync(
                Arg.Is<QueryParameters<string>>(q =>
                    q.Page == 2 && q.PageSize == 20 && q.SortBy == "Name" && q.Descending == true),
                Arg.Any<string>());
        }

        [Fact]
        public async Task GetPagedWgStaffPlan_MapsPaginationDataCorrectly()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string> { Page = 3, PageSize = 15 };
            var dtos = new List<WgStaffPlanViewDto> { BuildWgStaffPlanDto(workGroup) };
            var pagination = new PaginationDto
            {
                PageNumber = 3,
                PageSize = 15,
                TotalRecords = 42,
                TotalPages = 3
            };
            var serviceResult = new PaginatedResult<WgStaffPlanViewDto>(dtos, pagination);
            var expectedResponse = new PaginationRes<WgStaffPlanViewRes>
            {
                Data = new List<WgStaffPlanViewRes> { BuildWgStaffPlanRes(workGroup) },
                PaginationData = new Pagination
                {
                    PageNumber = 3,
                    PageSize = 15,
                    TotalRecords = 42,
                    TotalPages = 3
                }
            };

            _serviceMock.GetPagedWgStaffPlanAsync(query, workGroup).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WgStaffPlanViewRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPagedWgStaffPlan(query, workGroup);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<WgStaffPlanViewRes>>(okResult.Value);
            response.PaginationData.PageNumber.Should().Be(3);
            response.PaginationData.PageSize.Should().Be(15);
            response.PaginationData.TotalRecords.Should().Be(42);
            response.PaginationData.TotalPages.Should().Be(3);
        }

        [Fact]
        public async Task GetPagedWgStaffPlan_MapsDataCorrectly()
        {
            // Arrange
            const string workGroup = "WG001";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var dtos = new List<WgStaffPlanViewDto>
            {
                new()
                {
                    WorkGroup = workGroup,
                    GradeCode = "G1",
                    Name = "John Doe",
                    Manager = "Manager A",
                    PlannedHours = 40.0,
                    Fee = 1500m
                }
            };
            var serviceResult = new PaginatedResult<WgStaffPlanViewDto>(dtos, new PaginationDto());
            var expectedResponse = new PaginationRes<WgStaffPlanViewRes>
            {
                Data = new List<WgStaffPlanViewRes>
                {
                    new()
                    {
                        WorkGroup = workGroup,
                        GradeCode = "G1",
                        Name = "John Doe",
                        Manager = "Manager A",
                        PlannedHours = 40.0,
                        Fee = 1500m
                    }
                },
                PaginationData = new Pagination()
            };

            _serviceMock.GetPagedWgStaffPlanAsync(query, workGroup).Returns(serviceResult);
            _mapperMock.Map<PaginationRes<WgStaffPlanViewRes>>(serviceResult).Returns(expectedResponse);

            // Act
            var result = await _controller.GetPagedWgStaffPlan(query, workGroup);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginationRes<WgStaffPlanViewRes>>(okResult.Value);
            var firstItem = response.Data.First();
            firstItem.WorkGroup.Should().Be(workGroup);
            firstItem.Name.Should().Be("John Doe");
            firstItem.PlannedHours.Should().Be(40.0);
            firstItem.Fee.Should().Be(1500m);
        }

        #endregion

    }
}
