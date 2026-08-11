using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Api.Controllers;
using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Application.Pagination;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.PACT.Api.UnitTests.Controller.MonthlyOutputControllerTest
{
    public class MonthlyOutputControllerTests
    {
        private readonly IMonthlyOutputService _serviceMock;
        private readonly IMapper _mapperMock;
        private readonly ICurrentUserContext _currentUserContextMock;
        private readonly MonthlyOutputController _controller;

        public MonthlyOutputControllerTests()
        {
            _serviceMock = Substitute.For<IMonthlyOutputService>();
            _mapperMock = Substitute.For<IMapper>();
            _currentUserContextMock = Substitute.For<ICurrentUserContext>();
            _controller = new MonthlyOutputController(_serviceMock, _mapperMock, _currentUserContextMock);
        }

        #region SearchAsync

        [Fact]
        public async Task SearchAsync_HappyPath_ReturnsOkWithMappedResult()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var paginatedResult = new PaginatedResult<MonthlyOutputLogDto>
            {
                Data = new List<MonthlyOutputLogDto> { new() { TestCode = "TC1", Buyer = "BuyerA" } },
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10 }
            };
            var mappedResult = new PaginationRes<MonthlyOutputLogRes>
            {
                Data = new List<MonthlyOutputLogRes> { new() { TestCode = "TC1", Buyer = "BuyerA" } }
            };

            _serviceMock.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null)
                .Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<MonthlyOutputLogRes>>(paginatedResult)
                .Returns(mappedResult);

            // Act
            var result = await _controller.SearchAsync(query, null, null, null, null, null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(mappedResult, okResult.Value);
            await _serviceMock.Received(1).GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task SearchAsync_WithAllFilters_PassesFiltersToService()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var dateImported = new DateTime(2024, 1, 15);
            var paginatedResult = new PaginatedResult<MonthlyOutputLogDto> { Data = new List<MonthlyOutputLogDto>() };
            var mappedResult = new PaginationRes<MonthlyOutputLogRes>();

            _serviceMock.GetMonthlyOutputLogAsync(query, "WG1", "TC1", "BuyerA", dateImported, 1.0, "user1", "I")
                .Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<MonthlyOutputLogRes>>(paginatedResult)
                .Returns(mappedResult);

            // Act
            var result = await _controller.SearchAsync(query, "WG1", "TC1", "BuyerA", dateImported, 1.0, "user1", "I");

            // Assert
            Assert.IsType<OkObjectResult>(result);
            await _serviceMock.Received(1).GetMonthlyOutputLogAsync(query, "WG1", "TC1", "BuyerA", dateImported, 1.0, "user1", "I");
        }

        [Fact]
        public async Task SearchAsync_EmptyResult_ReturnsOkWithEmptyData()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var paginatedResult = new PaginatedResult<MonthlyOutputLogDto>
            {
                Data = new List<MonthlyOutputLogDto>(),
                PaginationData = new PaginationDto { PageNumber = 1, PageSize = 10 }
            };
            var mappedResult = new PaginationRes<MonthlyOutputLogRes>
            {
                Data = new List<MonthlyOutputLogRes>()
            };

            _serviceMock.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null)
                .Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<MonthlyOutputLogRes>>(paginatedResult)
                .Returns(mappedResult);

            // Act
            var result = await _controller.SearchAsync(query, null, null, null, null, null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PaginationRes<MonthlyOutputLogRes>>(okResult.Value);
            Assert.Empty(returnValue.Data);
        }

        [Fact]
        public async Task SearchAsync_MapsServiceResultToResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var paginatedResult = new PaginatedResult<MonthlyOutputLogDto>();
            var mappedResult = new PaginationRes<MonthlyOutputLogRes>();

            _serviceMock.GetMonthlyOutputLogAsync(query, null, null, null, null, null, null, null)
                .Returns(paginatedResult);
            _mapperMock.Map<PaginationRes<MonthlyOutputLogRes>>(paginatedResult)
                .Returns(mappedResult);

            // Act
            await _controller.SearchAsync(query, null, null, null, null, null, null, null);

            // Assert
            _mapperMock.Received(1).Map<PaginationRes<MonthlyOutputLogRes>>(paginatedResult);
        }

        [Fact]
        public async Task SearchAsync_ServiceThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();

            _serviceMock.GetMonthlyOutputLogAsync(
                    Arg.Any<QueryParameters<string>>(),
                    Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                    Arg.Any<DateTime?>(), Arg.Any<double?>(), Arg.Any<string?>(), Arg.Any<string?>())
                .ThrowsAsync(new Exception("Service error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _controller.SearchAsync(query, null, null, null, null, null, null, null));
        }

        #endregion

        #region Live and Staging Endpoints

        [Fact]
        public async Task GetLiveByKey_WhenItemNotFound_ThrowsKeyNotFoundException()
        {
            _serviceMock.GetLiveByKeyAsync("TC1", "B1", 6, "WG1").Returns((MonthlyOutputDto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetLiveByKey("TC1", "B1", 6, "WG1"));
        }

        [Fact]
        public async Task UpdateLive_WithValidRequest_ReturnsOk()
        {
            var request = new MonthlyOutputReq { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 10 };
            var dto = new MonthlyOutputDto { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 10 };
            var res = new MonthlyOutputRes { TestCode = "TC1", Buyer = "B1", Month = 6, WorkGroup = "WG1", Volume = 10 };

            _mapperMock.Map<MonthlyOutputDto>(request).Returns(dto);
            _serviceMock.UpdateLiveAsync(dto).Returns(dto);
            _mapperMock.Map<MonthlyOutputRes>(dto).Returns(res);

            var result = await _controller.UpdateLive(request);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(res, ok.Value);
        }

        [Fact]
        public async Task DeleteLive_WithValidKey_ReturnsOkWithBoolean()
        {
            _serviceMock.DeleteLiveAsync("TC1", "B1", 6, "WG1").Returns(true);

            var result = await _controller.DeleteLive("TC1", "B1", 6, "WG1");

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(true, ok.Value);
        }

        [Fact]
        public async Task GetStagingById_WhenItemNotFound_ThrowsKeyNotFoundException()
        {
            _currentUserContextMock.UserId.Returns("user1");
            _serviceMock.GetStagingByIdAsync(10, "user1").Returns((StagingMonthlyOutputDto?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetStagingById(10));
        }

        [Fact]
        public async Task CreateStaging_WithValidRequest_ReturnsCreatedAtAction()
        {
            _currentUserContextMock.UserId.Returns("user1");
            var request = new StagingMonthlyOutputReq { TestCode = "TC1", Buyer = "B1", WorkGroup = "WG1", Month = 6, Volume = 1 };
            var dto = new StagingMonthlyOutputDto { Id = 25, TestCode = "TC1", Buyer = "B1", WorkGroup = "WG1", Month = 6, Volume = 1 };
            var res = new StagingMonthlyOutputRes { Id = 25, TestCode = "TC1", Buyer = "B1", WorkGroup = "WG1", Month = 6, Volume = 1 };

            _mapperMock.Map<StagingMonthlyOutputDto>(request).Returns(dto);
            _serviceMock.CreateStagingAsync(dto, "user1").Returns(dto);
            _mapperMock.Map<StagingMonthlyOutputRes>(dto).Returns(res);

            var result = await _controller.CreateStaging(request);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(MonthlyOutputController.GetStagingById), created.ActionName);
        }

        #endregion
    }
}
